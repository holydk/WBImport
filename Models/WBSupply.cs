using System.Text.Json.Serialization;

namespace WBImport.Models
{
    public class WBSupply
    {
        #region Properties

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; }

        #endregion Properties
    }
}