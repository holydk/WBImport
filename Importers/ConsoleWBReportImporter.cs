using WBImport.Infrastructure;
using WBImport.Models;

namespace WBImport.Importers
{
    internal class ConsoleWBReportImporter : IWBReportImporter
    {
        #region Methods

        public Task ImportAsync(IEnumerable<WBReportLine> report)
        {
            ArgumentNullException.ThrowIfNull(report);

            if (!report.Any())
                return Task.CompletedTask;

            var itemsTotal = WBReportAnalyser
                .GetTotal(report)
                .OrderBy(item => item.WBArticle)
                .ThenBy(item => item.OrderedAt);
            if (itemsTotal == null)
                return Task.CompletedTask;

            foreach (var itemSummary in itemsTotal)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{(!string.IsNullOrEmpty(itemSummary.Article) ? itemSummary.Article : '-')} / {itemSummary.WBArticle} " +
                    $"{(!string.IsNullOrEmpty(itemSummary.Size) ? $"(р-р {itemSummary.Size})" : string.Empty)}");
                Console.ResetColor();

                Console.WriteLine($"\tЦена: {(itemSummary.Price > decimal.Zero ? $"{itemSummary.Price} {Defaults.RUB}" : "-")}");

                Console.Write($"\tКомиссия: ");

                if (itemSummary.Price > decimal.Zero && itemSummary.Cost > decimal.Zero)
                    Console.WriteLine($"{itemSummary.Price - itemSummary.Cost} {Defaults.RUB} ({Math.Round((1 - itemSummary.Cost / itemSummary.Price) * 100, 2)} %)");
                else
                    Console.WriteLine("-");

                Console.WriteLine($"\tК выплате: {(itemSummary.Cost > decimal.Zero ? $"{itemSummary.Cost} {Defaults.RUB}" : "-")}");
                Console.WriteLine($"\tИтого за логистику: {itemSummary.DeliveryCost} {Defaults.RUB}");

                if (itemSummary.OrderedAt.HasValue)
                    Console.WriteLine($"\tДата заказа: {itemSummary.OrderedAt.Value}");

                if (itemSummary.OrderId > 0)
                    Console.WriteLine($"\t№ заказа: {itemSummary.OrderId}");

                Console.WriteLine($"\t№ стикера: {itemSummary.ShkId}");

                if (!string.IsNullOrEmpty(itemSummary.Barcode))
                    Console.WriteLine($"\tБаркод: {itemSummary.Barcode}");

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
            }

            return Task.CompletedTask;
        }

        #endregion Methods
    }
}