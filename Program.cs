using Confiti.MoySklad.Remap.Api;
using Confiti.MoySklad.Remap.Client;
using Confiti.MoySklad.Remap.Entities;
using Confiti.MoySklad.Remap.Queries;
using WBReportImport;

internal class Program
{
    internal const string REPORTS_FOLDER_NAME = "Reports";
    internal const string SETTINGS_FILE_NAME = "settings.json";

    #region Methods

    public static async Task Main(string[] args)
    {
        var dateTimeFrom = DateTime.Now.AddMonths(-1);

        var reportsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, REPORTS_FOLDER_NAME);
        var files = Directory.EnumerateFiles(reportsFolderPath);
        if (files?.Any() == true)
        {
            var settings = await Settings.FromFileAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETTINGS_FILE_NAME));
            var importer = new ConsoleMSDemandWBReportImporter(() => GetDemandsAsync(settings, dateTimeFrom));

            var allReports = new List<WBReportLine>();

            foreach (var file in files)
            {
                var report = await WBReportReader.FromFile(file).GetReportAsync();
                if (report == null || !report.Any())
                    continue;

                allReports.AddRange(report);
            }

            await importer.ImportAsync(allReports);
        }

        Console.ReadKey();
    }

    #endregion Methods

    #region Utils

    private static async IAsyncEnumerable<Demand> GetDemandsAsync(Settings settings, DateTime moment)
    {
        ArgumentNullException.ThrowIfNull(nameof(settings));

        if (string.IsNullOrEmpty(settings.MoySkladAccessToken))
            throw new InvalidOperationException("MoySklad access token was empty.");

        var moySkladApi = new MoySkladApi(new MoySkladCredentials
        {
            AccessToken = settings.MoySkladAccessToken
        });

        var query = new ApiParameterBuilder<DemandQuery>();

        query.Limit(100);
        query.Parameter("moment").Should().BeGreaterOrEqualTo(moment.ToString("yyyy-MM-dd"));
        query.Expand().With(x => x.Positions).And.With("positions.assortment");
        query.Order().By("moment");

        if (!string.IsNullOrEmpty(settings.SalesChannelId))
            query.Parameter("salesChannel").Should().Be($"https://api.moysklad.ru/api/remap/1.2/entity/store/{settings.SalesChannelId}");

        var offset = 0;

        ApiResponse<EntitiesResponse<Demand>> response = null;
        do
        {
            query.Offset(offset);

            response = await moySkladApi.Demand.GetAllAsync(query);

            var payload = response?.Payload;

            if (payload?.Meta == null)
                break;

            if (payload.Rows == null || payload.Rows.Length == 0)
                break;

            offset += payload.Meta.Limit;

            foreach (var row in payload.Rows)
                yield return row;
        }
        while (!string.IsNullOrEmpty(response.Payload.Meta.NextHref));
    }

    #endregion Utils
}