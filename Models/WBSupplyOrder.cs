using System.Text.Json.Serialization;
using WBImport.Models;

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