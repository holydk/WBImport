using System.Text;

namespace WBReportImport
{
    internal class WBReportAnalyser
    {
        internal const string SALE_DOC_TYPE_NAME = "Продажа";

        #region Methods

        public static IEnumerable<WBItemSummary> GetItemTotal(IEnumerable<WBReportLine> report, string barcode)
        {
            ArgumentNullException.ThrowIfNull(report, nameof(report));
            ArgumentNullException.ThrowIfNullOrEmpty(barcode, nameof(barcode));

            var reportByShkId = report
                .OrderBy(doc => doc.OrderDt)
                .Where(doc => doc.ShkId > 0 && doc.Barcode.Equals(barcode, StringComparison.OrdinalIgnoreCase))
                .GroupBy(doc => doc.ShkId);

            var items = new List<WBItemSummary>();

            foreach (var docsByShkId in reportByShkId)
            {
                var saleDocument = docsByShkId.FirstOrDefault(doc => doc.DocTypeName == SALE_DOC_TYPE_NAME)
                    ?? docsByShkId.FirstOrDefault();

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
                Cost = document.PpvzForPay,
                OrderedAt = document.OrderDt,
                Price = document.RetailPriceWithdiscRub,
                ShkId = document.ShkId,
            };

            var shkIdAsString = document.ShkId.ToString();
            var expenseDocs = report.Where(doc => doc.NumberId > 0 && (doc.ShkId == document.ShkId || doc.StickerId == shkIdAsString));
            if (expenseDocs?.Any() == true)
            {
                var deliveryCost = decimal.Zero;
                var expenseItems = new List<(string Name, decimal Cost)>();

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
                        doc.RebillLogisticCost +
                        doc.Penalty;

                    deliveryCost += cost;

                    if (cost == decimal.Zero)
                        cost = doc.PpvzForPay;

                    expenseItems.Add(new(nameBuider.ToString(), cost));
                }

                itemSummary.DeliveryCost = deliveryCost;
                itemSummary.ExpenseItems = expenseItems;
            }

            return itemSummary;
        }

        #endregion Utilities
    }
}