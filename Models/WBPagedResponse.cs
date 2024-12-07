using System.Text.Json.Serialization;

namespace WBImport.Models
{
    public class WBPagedResponse
    {
        #region Properties

        [JsonPropertyName("next")]
        public long Next { get; set; }

        #endregion Properties
    }
}