using Confiti.MoySklad.Remap.Api;
using Confiti.MoySklad.Remap.Client;
using Confiti.MoySklad.Remap.Entities;
using Confiti.MoySklad.Remap.Queries;

namespace WBImport
{
    internal class ConsoleWBReportRelatedToMSDemandsImporter : IWBReportImporter
    {
        #region Methods

        public async Task ImportAsync(IEnumerable<WBReportLine> report)
        {
            ArgumentNullException.ThrowIfNull(report);

            if (!report.Any())
                return;

            var itemsTotal = WBReportAnalyser
                .GetTotal(report)
                .Where(item => !string.IsNullOrEmpty(item.Barcode))
                .OrderBy(item => item.OrderedAt)
                .GroupBy(item => item.Barcode)
                .ToDictionary(item => item.Key);

            var processedWbBarcodes = new Dictionary<long, WBItemSummary>();

            // todo:
            // 1. Загузить сборочные задания WB от минимальной даты из report
            // 2. Собрать все уникальные supplyId из сборочных заданий
            // 3. Получить сборочные задания по IDs
            // 4. Загрузить отгрузки MS от минимальной даты сборочного задания WB
            // 5. Сопоставить отгрузки MS и поставки WB по названию

            var moment = DateTime.Now.AddMonths(-1);

            await foreach (var demand in GetDemandsAsync(moment))
            {
                var positions = demand.Positions?.Rows;
                if (positions == null || positions.Length == 0)
                    continue;

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

                        if (itemTotal?.Any() == true)
                        {
                            foreach (var itemSummary in itemTotal)
                            {
                                if (processedWbBarcodes.TryGetValue(itemSummary.ShkId, out _))
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
                                Console.WriteLine($"\tКомиссия: {(itemSummary.Price > decimal.Zero && itemSummary.Cost > decimal.Zero ? $"{itemSummary.Price - itemSummary.Cost} {Defaults.RUB}" : "-")}");
                                Console.WriteLine($"\tК выплате: {(itemSummary.Cost > decimal.Zero ? $"{itemSummary.Cost} {Defaults.RUB}" : "-")}");
                                Console.WriteLine($"\tИтого за логистику: {itemSummary.DeliveryCost} {Defaults.RUB}");

                                if (itemSummary.OrderedAt.HasValue)
                                    Console.WriteLine($"\tДата заказа: {itemSummary.OrderedAt.Value}");

                                if (itemSummary.Documents?.Any() == true)
                                {
                                    var status = 0;

                                    foreach (var (name, cost) in itemSummary.Documents.OrderBy(doc => doc.Name))
                                    {
                                        if (name == "Возврат" || name.Contains("при возврате"))
                                            status = 1;

                                        if (name.Contains("при отмене"))
                                            status = 2;

                                        if (name.Contains("Штраф"))
                                            Console.ForegroundColor = ConsoleColor.Yellow;

                                        Console.WriteLine($"\t{name}: {cost} {Defaults.RUB}");
                                        Console.ResetColor();
                                    }

                                    if (status > 0)
                                        Console.ForegroundColor = ConsoleColor.Red;

                                    if (status == 1)
                                        Console.WriteLine("\tВозврат");
                                    else if (status == 2)
                                        Console.WriteLine("\tОтмена");

                                    Console.ResetColor();
                                }

                                Console.WriteLine();

                                processedWbBarcodes[itemSummary.ShkId] = itemSummary;

                                break;
                            }
                        }
                    }
                }

                Console.WriteLine($"Итого за логистику: {deliveryCostTotal}");

                var overheadSum = demand.Overhead?.Sum;
                if (overheadSum.HasValue)
                {
                    if (overheadSum != deliveryCostTotal * 100)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"Расхождения по накл. расходам");
                        Console.ResetColor();
                    }
                }

                Console.WriteLine();
            }
        }

        #endregion Methods

        #region Utilities

        private static async IAsyncEnumerable<Demand> GetDemandsAsync(DateTime moment)
        {
            var settings = Settings.Default?.MoySklad;

            if (string.IsNullOrEmpty(settings?.AccessToken))
                throw new InvalidOperationException("MoySklad access token was empty.");

            var moySkladApi = new MoySkladApi(new MoySkladCredentials
            {
                AccessToken = settings.AccessToken
            });

            var query = new ApiParameterBuilder<DemandQuery>();

            query.Limit(100);
            query.Parameter("moment").Should().BeGreaterOrEqualTo(moment.ToString(Defaults.DATE_TIME_FORMAT));
            query.Expand()
                .With(x => x.Positions).And
                .With("positions.assortment").And
                .With("positions.assortment.product");
            query.Order().By("moment");

            if (!string.IsNullOrEmpty(settings.SalesChannelId))
                query.Parameter("salesChannel").Should().Be($"https://api.moysklad.ru/api/remap/1.2/entity/store/{settings.SalesChannelId}");

            var offset = 0;

            ApiResponse<EntitiesResponse<Demand>> response = null;
            do
            {
                query.Offset(offset);

                response = await moySkladApi.Demand.GetAllAsync(query);

                var payload = response?.Payload;

                if (payload?.Meta == null)
                    break;

                if (payload.Rows == null || payload.Rows.Length == 0)
                    break;

                offset += payload.Meta.Limit;

                foreach (var row in payload.Rows)
                    yield return row;
            }
            while (!string.IsNullOrEmpty(response.Payload.Meta.NextHref));
        }

        #endregion Utilities
    }
}