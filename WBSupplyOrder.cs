using System.Text.Json.Serialization;

namespace WBImport
{
    public class WBSupplyOrder
    {
        #region Properties

        [JsonPropertyName("orders")]
        public WBOrder[] Orders { get; set; }

        #endregion Properties
    }
}