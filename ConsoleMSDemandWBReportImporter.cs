using Confiti.MoySklad.Remap.Entities;

namespace WBReportImport
{
    internal class ConsoleMSDemandWBReportImporter(Func<IAsyncEnumerable<Demand>> demandFactory) : IWBReportImporter
    {
        internal const string RUB = "руб.";

        #region Methods

        public async Task ImportAsync(IEnumerable<WBReportLine> report)
        {
            ArgumentNullException.ThrowIfNull(report);

            if (demandFactory == null)
                return;

            var processedWbBarcodes = new Dictionary<long, WBItemSummary>();
            var processedBarcodes = new Dictionary<string, IEnumerable<WBItemSummary>>();

            await foreach (var demand in demandFactory())
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
                        if (!processedBarcodes.TryGetValue(barcode.Value, out var itemTotal))
                        {
                            itemTotal = WBReportAnalyser.GetItemTotal(report, barcode.Value);
                            if (itemTotal?.Any() == true)
                                processedBarcodes[barcode.Value] = itemTotal;
                        }

                        if (itemTotal?.Any() == true)
                        {
                            foreach (var itemSummary in itemTotal)
                            {
                                if (processedWbBarcodes.TryGetValue(itemSummary.ShkId, out _))
                                    continue;

                                deliveryCostTotal += itemSummary.DeliveryCost;

                                Console.WriteLine($"{position.Assortment.Name}");
                                Console.WriteLine($"\tЦена: {(itemSummary.Price > decimal.Zero ? $"{itemSummary.Price} {RUB}" : "-")}");
                                Console.WriteLine($"\tКомиссия: {(itemSummary.Price > decimal.Zero && itemSummary.Cost > decimal.Zero ? $"{itemSummary.Price - itemSummary.Cost} {RUB}" : "-")}");
                                Console.WriteLine($"\tК выплате: {(itemSummary.Cost > decimal.Zero ? $"{itemSummary.Cost} {RUB}" : "-")}");
                                Console.WriteLine($"\tИтого за логистику: {itemSummary.DeliveryCost} {RUB}");

                                if (itemSummary.OrderedAt.HasValue)
                                    Console.WriteLine($"\tДата заказа: {itemSummary.OrderedAt.Value}");

                                if (itemSummary.ExpenseItems?.Any() == true)
                                {
                                    var status = 0;

                                    foreach (var (name, cost) in itemSummary.ExpenseItems)
                                    {
                                        if (name == "Возврат" || name.Contains("при возврате"))
                                            status = 1;

                                        if (name.Contains("при отмене"))
                                            status = 2;

                                        if (name.Contains("Штраф"))
                                            Console.ForegroundColor = ConsoleColor.Yellow;

                                        Console.WriteLine($"\t{name}: {cost} {RUB}");
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
    }
}