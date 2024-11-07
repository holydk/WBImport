namespace WBReportImport
{
    public class WBItemSummary
    {
        #region Properties

        public string Article { get; set; }
        public string Barcode { get; set; }
        public decimal Cost { get; set; }
        public decimal DeliveryCost { get; set; }
        public IEnumerable<(string Name, decimal Cost)> Documents { get; set; }
        public DateTime? OrderedAt { get; set; }
        public decimal Price { get; set; }
        public long ShkId { get; set; }
        public string Size { get; set; }
        public long WBArticle { get; set; }

        #endregion Properties
    }
}