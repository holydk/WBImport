using Confiti.MoySklad.Remap.Api;
using Confiti.MoySklad.Remap.Client;
using Confiti.MoySklad.Remap.Entities;
using WBImport.Infrastructure;
using WBImport.Models;

namespace WBImport.Reports.Importers
{
    internal class UpdateMSDemandsWBReportImporter : IWBReportImporter
    {
        #region Methods

        public async Task ImportAsync(IEnumerable<WBReportLine> report)
        {
            ArgumentNullException.ThrowIfNull(report);

            if (!report.Any())
                return;

            var orderedAtReport = WBReportAnalyser
                .GetTotal(report)
                .Where(item => !string.IsNullOrEmpty(item.Barcode))
                .OrderBy(item => item.OrderedAt);

            var dateTimeFrom = orderedAtReport
                .FirstOrDefault(summary => summary.OrderedAt.HasValue)
                ?.OrderedAt
                ?? DateTime.Now.AddDays(-30);

            var dateTimeTo = orderedAtReport
                .LastOrDefault(summary => summary.OrderedAt.HasValue)
                ?.OrderedAt
                ?? DateTime.Now;

            var itemsTotal = orderedAtReport
                .GroupBy(item => item.Barcode)
                .ToDictionary(group => group.Key);

            var ordersMaxDateTimeOffsetTo = (DateTimeOffset)dateTimeTo;
            var ordersDateTimeOffsetFrom = (DateTimeOffset)dateTimeFrom;
            var ordersDateTimeOffsetTo = (DateTimeOffset)dateTimeFrom.AddDays(30);
            var next = 0L;
            var limit = 100;

            var allOrders = new List<WBOrder>();

            do
            {
                do
                {
                    var response = await WBClient.GetAsync<WBOrders>(
                        $"{Defaults.WB_MARKETPLACE_BASE_URL}/orders",
                        new()
                        {
                            ["limit"] = limit.ToString(),
                            ["next"] = next.ToString(),
                            ["dateFrom"] = ordersDateTimeOffsetFrom.ToUnixTimeSeconds().ToString(),
                            ["dateTo"] = ordersDateTimeOffsetTo.ToUnixTimeSeconds().ToString()
                        });

                    allOrders.AddRange(response.Orders);

                    next = response.Next;
                }
                while (next > 0);

                ordersDateTimeOffsetFrom = ordersDateTimeOffsetTo;
                ordersDateTimeOffsetTo = ordersDateTimeOffsetFrom.AddDays(30);

                if (ordersDateTimeOffsetTo > ordersMaxDateTimeOffsetTo)
                    ordersDateTimeOffsetTo = ordersMaxDateTimeOffsetTo;
            }
            while (ordersDateTimeOffsetFrom < ordersDateTimeOffsetTo);

            if (allOrders.Count == 0)
                return;

            var processedStickerIds = new Dictionary<long, WBItemSummary>();
            var ordersBySupplyId = allOrders
                .Where(order => !string.IsNullOrEmpty(order.SupplyId))
                .GroupBy(order => order.SupplyId)
                .ToDictionary(group => group.Key);

            var demandsToUpdate = new List<Demand>();

            var shouldBeUpdate = false;

            await foreach (var demand in MSClient.GetDemandsAsync(dateTimeFrom))
            {
                shouldBeUpdate = false;

                var positions = demand.Positions?.Rows;
                if (positions == null || positions.Length == 0)
                    continue;

                if (string.IsNullOrEmpty(demand.Name) || !demand.Name.StartsWith("WB"))
                    continue;

                if (!ordersBySupplyId.TryGetValue(demand.Name, out var supplyOrders))
                    continue;

                var supplyOrdersById = supplyOrders.ToDictionary(order => order.Id);
                var deliveryCostTotal = decimal.Zero;

                foreach (var position in positions)
                {
                    var barcodes = position.Assortment.Barcodes;
                    if (barcodes == null || barcodes.Length == 0)
                        continue;

                    foreach (var barcode in barcodes)
                    {
                        if (!itemsTotal.TryGetValue(barcode.Value, out var itemTotal))
                            continue;

                        foreach (var itemSummary in itemTotal)
                        {
                            if (processedStickerIds.TryGetValue(itemSummary.ShkId, out _))
                                continue;

                            if (itemSummary.OrderId == 0)
                                continue;

                            if (!supplyOrdersById.TryGetValue(itemSummary.OrderId, out var _))
                                continue;

                            deliveryCostTotal += itemSummary.DeliveryCost;

                            if (itemSummary.Cost > decimal.Zero && position.Price != itemSummary.Cost * 100)
                            {
                                Console.WriteLine($"{demand.Name}. Обновить цену \"{position.Assortment.Name}\": {position.Price / 100} -> {itemSummary.Cost}");
                                position.Price = (long)(itemSummary.Cost * 100);

                                shouldBeUpdate = true;
                            }

                            processedStickerIds[itemSummary.ShkId] = itemSummary;

                            break;
                        }
                    }
                }

                var overheadSum = demand.Overhead?.Sum ?? 0;
                if (overheadSum != deliveryCostTotal * 100)
                {
                    Console.WriteLine($"{demand.Name}. Обновить накл. расходы: {overheadSum / 100} -> {deliveryCostTotal}");

                    if (demand.Overhead != null)
                        demand.Overhead.Sum = (long)(deliveryCostTotal * 100);
                    else
                        demand.Overhead = new DocumentOverhead
                        {
                            Distribution = OverheadDistributionType.Price,
                            Sum = (long)(deliveryCostTotal * 100),
                        };

                    shouldBeUpdate = true;
                }

                if (shouldBeUpdate)
                    demandsToUpdate.Add(demand);
            }

            Console.WriteLine();

            if (demandsToUpdate.Count == 0)
            {
                Console.WriteLine("Нет отгрузок для обновления.");
                Console.WriteLine();
                return;
            }

            Console.WriteLine("Обновить отгрузки? (y, n)");

            switch (Console.ReadKey().Key)
            {
                case ConsoleKey.Y:

                    var settings = Settings.Default?.MoySklad;

                    if (string.IsNullOrEmpty(settings?.AccessToken))
                        throw new InvalidOperationException("MoySklad access token was empty.");

                    var moySkladApi = new MoySkladApi(new MoySkladCredentials
                    {
                        AccessToken = settings.AccessToken
                    }, Defaults.HttpClient);

                    foreach (var demand in demandsToUpdate)
                        await moySkladApi.Demand.UpdateAsync(demand);

                    break;
            }

            Console.WriteLine();
        }

        #endregion Methods
    }
}