namespace WBReportImport
{
    public class WBItemSummary
    {
        #region Properties

        public decimal Cost { get; set; }
        public decimal DeliveryCost { get; set; }
        public IEnumerable<(string Name, decimal Cost)> ExpenseItems { get; set; }
        public DateTime? OrderedAt { get; set; }
        public decimal Price { get; set; }
        public long ShkId { get; set; }

        #endregion Properties
    }
}