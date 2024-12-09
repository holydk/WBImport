using Confiti.MoySklad.Remap.Entities;
using WBImport.Infrastructure;
using WBImport.Models;

namespace WBImport.Reports.Importers
{
    internal class ConsoleWBReportRelatedToMSDemandsImporter : IWBReportImporter
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

            await foreach (var demand in MSClient.GetDemandsAsync(dateTimeFrom))
            {
                var positions = demand.Positions?.Rows;
                if (positions == null || positions.Length == 0)
                    continue;

                if (string.IsNullOrEmpty(demand.Name) || !demand.Name.StartsWith("WB"))
                    continue;

                if (!ordersBySupplyId.TryGetValue(demand.Name, out var supplyOrders))
                    continue;

                var supplyOrdersById = supplyOrders.ToDictionary(order => order.Id);
                var deliveryCostTotal = decimal.Zero;

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Отгрузка № {demand.Name}");
                Console.WriteLine();
                Console.ResetColor();

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

                            var article = position.Assortment switch
                            {
                                Product product => product.Article,
                                Variant variant => variant.Product.Article,
                                Bundle bundle => bundle.Article,
                                _ => string.Empty
                            };

                            Console.WriteLine($"{(!string.IsNullOrEmpty(article) ? $"{article} " : string.Empty)}{position.Assortment.Name}");
                            Console.WriteLine($"\tЦена: {(itemSummary.Price > decimal.Zero ? $"{itemSummary.Price} {Defaults.RUB}" : "-")}");

                            Console.Write($"\tКомиссия: ");

                            if (itemSummary.Price > decimal.Zero && itemSummary.Cost > decimal.Zero)
                                Console.WriteLine($"{itemSummary.Price - itemSummary.Cost} {Defaults.RUB} ({Math.Round((1 - itemSummary.Cost / itemSummary.Price) * 100, 2)} %)");
                            else
                                Console.WriteLine("-");

                            //Console.WriteLine($"\tКомиссия: {(itemSummary.Price > decimal.Zero && itemSummary.Cost > decimal.Zero ? $"{itemSummary.Price - itemSummary.Cost} {Defaults.RUB}" : "-")}");
                            Console.WriteLine($"\tК выплате: {(itemSummary.Cost > decimal.Zero ? $"{itemSummary.Cost} {Defaults.RUB}" : "-")}");
                            Console.WriteLine($"\tИтого за логистику: {itemSummary.DeliveryCost} {Defaults.RUB}");

                            if (itemSummary.OrderedAt.HasValue)
                                Console.WriteLine($"\tДата заказа: {itemSummary.OrderedAt.Value}");

                            Console.WriteLine($"\t№ заказа: {itemSummary.OrderId}");
                            Console.WriteLine($"\t№ стикера: {itemSummary.ShkId}");

                            foreach (var barcodeToPrint in barcodes)
                                Console.WriteLine($"\tБаркод: {barcodeToPrint.Value}");

                            if (itemSummary.Documents?.Any() == true)
                            {
                                var status = 0;

                                foreach (var doc in itemSummary.Documents.OrderBy(doc => doc.Name))
                                {
                                    if (!itemSummary.OrderedAt.HasValue
                                            || doc.OrderedAt.HasValue
                                                || doc.OrderedAt.Value.Date >= itemSummary.OrderedAt.Value.Date)
                                    {
                                        if (doc.Name == "Возврат" || doc.Name.Contains("при возврате"))
                                            status = 1;

                                        if (doc.Name.Contains("при отмене"))
                                            status = 2;
                                    }

                                    if (doc.Name.Contains("Штраф"))
                                        Console.ForegroundColor = ConsoleColor.Yellow;

                                    Console.WriteLine($"\t{doc.Name}: {doc.Cost} {Defaults.RUB}");
                                    Console.ResetColor();
                                }

                                if (status > 0)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;

                                    if (status == 1)
                                        Console.WriteLine("\tВозврат");
                                    else if (status == 2)
                                        Console.WriteLine("\tОтмена");
                                }
                                else if (itemSummary.Cost > decimal.Zero)
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine("\tПродажа");
                                }

                                Console.ResetColor();
                            }

                            Console.WriteLine();

                            processedStickerIds[itemSummary.ShkId] = itemSummary;

                            break;
                        }
                    }
                }

                Console.WriteLine($"Итого за логистику: {deliveryCostTotal}");

                var overheadSum = demand.Overhead?.Sum ?? 0;
                if (overheadSum != deliveryCostTotal * 100)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Расхождения по накл. расходам");
                    Console.ResetColor();
                }

                Console.WriteLine();
            }
        }

        #endregion Methods
    }
}