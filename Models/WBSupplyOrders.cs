using System.Text.Json.Serialization;
using WBImport.Models;

namespace WBImport
{
    public class WBSupplyOrders
    {
        #region Properties

        [JsonPropertyName("orders")]
        public WBOrder[] Orders { get; set; }

        #endregion Properties
    }
}