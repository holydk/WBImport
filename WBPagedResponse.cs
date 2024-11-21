using System.Text.Json.Serialization;

namespace WBImport
{
    public class WBPagedResponse
    {
        #region Properties

        [JsonPropertyName("next")]
        public long Next { get; set; }

        #endregion Properties
    }
}