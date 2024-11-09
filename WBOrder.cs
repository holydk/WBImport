using System.Text.Json.Serialization;

namespace WBImport
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

        #endregion Properties
    }
}