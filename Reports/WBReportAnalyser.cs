using System.Text;
using WBImport.Models;

namespace WBImport.Reports
{
    internal sealed class WBReportAnalyser
    {
        #region Methods

        public static IEnumerable<WBItemSummary> GetTotal(IEnumerable<WBReportLine> report)
        {
            ArgumentNullException.ThrowIfNull(report, nameof(report));

            var reportByShkId = report
                .Where(doc => doc.ShkId > 0)
                .GroupBy(doc => doc.ShkId);

            var items = new List<WBItemSummary>();

            foreach (var docsByShkId in reportByShkId)
            {
                var saleDocument = docsByShkId
                    .Where(doc => doc.DocTypeName.Equals(Defaults.SALE_DOC_TYPE_NAME, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(doc => doc.OrderDt)
                    .LastOrDefault()
                    ?? docsByShkId.FirstOrDefault(doc => doc.AssemblyId > 0);

                if (saleDocument != null)
                    items.Add(CreateItemSummary(report, saleDocument));
            }

            return items;
        }

        #endregion Methods

        #region Utilities

        private static WBItemSummary CreateItemSummary(IEnumerable<WBReportLine> report, WBReportLine document)
        {
            var itemSummary = new WBItemSummary
            {
                Article = document.SaName,
                Barcode = document.Barcode,
                Cost = document.PpvzForPay,
                Price = document.RetailPriceWithdiscRub,
                ShkId = document.ShkId,
                Size = document.TsName,
                WBArticle = document.NumberId,
            };

            var shkIdAsString = document.ShkId.ToString();
            var expenseDocs = report.Where(doc => doc.NumberId > 0 && (doc.ShkId == document.ShkId || doc.StickerId == shkIdAsString));
            if (expenseDocs?.Any() == true)
            {
                if (string.IsNullOrEmpty(itemSummary.Article))
                    itemSummary.Article = expenseDocs.FirstOrDefault(doc => !string.IsNullOrEmpty(doc.SaName))?.SaName;

                if (string.IsNullOrEmpty(itemSummary.Barcode))
                    itemSummary.Barcode = expenseDocs.FirstOrDefault(doc => !string.IsNullOrEmpty(doc.Barcode))?.Barcode;

                if (string.IsNullOrEmpty(itemSummary.Size))
                    itemSummary.Size = expenseDocs.FirstOrDefault(doc => !string.IsNullOrEmpty(doc.TsName))?.TsName;

                if (itemSummary.WBArticle == 0)
                    itemSummary.WBArticle = expenseDocs.FirstOrDefault(doc => doc.NumberId > 0)?.NumberId ?? 0;

                itemSummary.OrderedAt =
                    !document.DocTypeName.Equals(Defaults.SALE_DOC_TYPE_NAME, StringComparison.OrdinalIgnoreCase)
                        ? expenseDocs.Where(doc => doc.OrderDt.HasValue).Min(doc => doc.OrderDt)
                        : document.OrderDt;

                itemSummary.OrderId = document.AssemblyId == 0
                    // the order ID can be 0 if item was sold by WB warehouse
                    // steps to reproduce
                    // 1. Seller uses FBO with auto return to pick-up point
                    // 2. Item was sold
                    // 3. Return was requested
                    // 4. Item go to WB warehouse
                    // 5. Item was sold twice
                    // 6. Then new sale document has order ID = 0
                    // and we should find previous document with order ID
                    ? expenseDocs.FirstOrDefault(doc => doc.AssemblyId > 0)?.AssemblyId ?? 0
                    : document.AssemblyId;

                var deliveryCost = decimal.Zero;
                var expenseItems = new List<WBItemSummaryDocument>();

                foreach (var doc in expenseDocs)
                {
                    var nameBuider = new StringBuilder();

                    nameBuider.Append(
                        !string.IsNullOrEmpty(doc.SupplierOperName)
                            ? doc.SupplierOperName
                            : doc.DocTypeName
                    );

                    if (!string.IsNullOrEmpty(doc.BonusTypeName))
                        nameBuider.AppendFormat(". {0}", doc.BonusTypeName);

                    var cost =
                        doc.Acceptance +
                        doc.DeliveryRub +
                        doc.Penalty +
                        doc.StorageFee +
                        doc.Deduction;

                    deliveryCost += cost;

                    if (cost == decimal.Zero)
                        cost = doc.PpvzForPay;

                    expenseItems.Add(new()
                    {
                        Cost = cost,
                        Name = nameBuider.ToString(),
                        OrderedAt = doc.OrderDt,
                        OrderId = doc.AssemblyId
                    });
                }

                itemSummary.DeliveryCost = deliveryCost;
                itemSummary.Documents = expenseItems;
            }

            return itemSummary;
        }

        #endregion Utilities
    }
}