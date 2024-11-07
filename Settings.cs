using System.Text.Json;

namespace WBReportImport
{
    public sealed class MoySkladSettings
    {
        #region Properties

        public string AccessToken { get; set; }
        public string SalesChannelId { get; set; }

        #endregion Properties
    }

    public sealed class Settings
    {
        public static Settings Default { internal set; get; }

        #region Properties

        public MoySkladSettings MoySklad { get; set; }

        #endregion Properties

        #region Methods

        public static async Task<Settings> FromFileAsync(string fileName)
        {
            using var fileStream = File.OpenRead(fileName);

            return await JsonSerializer.DeserializeAsync<Settings>(fileStream);
        }

        #endregion Methods
    }
}