using System.Text.Json.Serialization;

namespace WBImport.Models
{
    public class WBOrder
    {
        #region Properties

        [JsonPropertyName("article")]
        public string Article { get; set; }

        [JsonPropertyName("skus")]
        public string[] Barcodes { get; set; }

        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("supplyId")]
        public string SupplyId { get; set; }

        #endregion Properties
    }
}