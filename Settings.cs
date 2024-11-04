using System.Text.Json;

namespace WBReportImport
{
    internal class Settings
    {
        #region Properties

        public string MoySkladAccessToken { get; set; }
        public string SalesChannelId { get; set; }

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