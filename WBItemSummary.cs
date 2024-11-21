namespace WBImport
{
    public class WBItemSummary
    {
        #region Properties

        public string Article { get; set; }
        public string Barcode { get; set; }
        public decimal Cost { get; set; }
        public decimal DeliveryCost { get; set; }
        public IEnumerable<WBItemSummaryDocument> Documents { get; set; }
        public DateTime? OrderedAt { get; set; }
        public long OrderId { get; set; }
        public decimal Price { get; set; }
        public long ShkId { get; set; }
        public string Size { get; set; }
        public long WBArticle { get; set; }

        #endregion Properties
    }

    public class WBItemSummaryDocument
    {
        #region Properties

        public decimal Cost { get; set; }
        public string Name { get; set; }
        public DateTime? OrderedAt { get; set; }
        public long OrderId { get; set; }

        #endregion Properties
    }
}