using System.Text.Json.Serialization;

namespace WBImport
{
    public class WBOrders : WBPagedResponse
    {
        #region Properties

        [JsonPropertyName("orders")]
        public WBOrder[] Orders { get; set; }

        #endregion Properties
    }
}