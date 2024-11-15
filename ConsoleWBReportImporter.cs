﻿namespace WBImport
{
    public class ConsoleWBReportImporter : IWBReportImporter
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
                Console.WriteLine();
                Console.ResetColor();

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
            }

            return Task.CompletedTask;
        }

        #endregion Methods
    }
}