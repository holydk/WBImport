using System.Text.Json.Serialization;

namespace WBImport
{
    public class WBSupply
    {
        #region Properties

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        #endregion Properties
    }
}