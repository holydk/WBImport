using System.Net;
using WBImport.Importers;

namespace WBImport
{
    internal static class Defaults
    {
        public static Dictionary<WBReportImporterType, Func<IWBReportImporter>> ReportImporters = new()
        {
            [WBReportImporterType.Console] = () => new ConsoleWBReportImporter(),
            [WBReportImporterType.ConsoleRelatedToMoySkladDemands] = () => new ConsoleWBReportRelatedToMSDemandsImporter(),
            [WBReportImporterType.UpdateMSDemands] = () => new UpdateMSDemandsWBReportImporter()
        };

        internal const string DATE_TIME_FORMAT = "yyyy-MM-dd";

        internal const string REPORTS_FOLDER_NAME = "Reports";

        internal const string RUB = "руб.";

        internal const string SALE_DOC_TYPE_NAME = "Продажа";

        internal const string SETTINGS_FILE_NAME = "settings.json";

        internal const string USER_AGENT = "WBImport/1.0";

        internal const string WB_MARKETPLACE_BASE_URL = "https://marketplace-api.wildberries.ru/api/v3";

        private static Lazy<HttpClient> _defaultHttpClient = new(() =>
        {
            var client = new HttpClient(
                new SocketsHttpHandler
                {
                    // Recreate every 15 minutes
                    PooledConnectionLifetime = TimeSpan.FromMinutes(15),
                    AutomaticDecompression = DecompressionMethods.GZip
                },
                true
            )
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            client.DefaultRequestHeaders.Add("UserAgent", USER_AGENT);

            return client;
        });

        public static HttpClient HttpClient => _defaultHttpClient.Value;
    }
}