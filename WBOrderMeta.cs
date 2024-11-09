using System.Text.Json.Serialization;

namespace WBImport
{
    public class SGtinMeta
    {
        #region Properties

        [JsonPropertyName("value")]
        public string[] Value { get; set; }

        #endregion Properties
    }

    public class WBMeta
    {
        #region Properties

        [JsonPropertyName("sgtin")]
        public SGtinMeta SGtin { get; set; }

        #endregion Properties
    }

    public class WBOrderMeta
    {
        #region Properties

        [JsonPropertyName("meta")]
        public WBMeta Meta { get; set; }

        #endregion Properties
    }
}