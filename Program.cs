using WBReportImport;

internal sealed class Program
{
    private static Dictionary<WBReportImporterType, Func<IWBReportImporter>> _importers = new()
    {
        [WBReportImporterType.Console] = () => new ConsoleWBReportImporter(),
        [WBReportImporterType.ConsoleRelatedToMoySkladDemands] = () => new ConsoleWBReportRelatedToMSDemandsImporter()
    };

    #region Methods

    public static async Task Main(string[] args)
    {
        await InitSettingsAsync();

        var importerType = GetReportImporterType();
        if (!importerType.HasValue)
            return;

        // todo: read dateTime from console or settings ?
        var dateTimeFrom = DateTime.Now.AddMonths(-1);
        var dateTimeTo = DateTime.Now;

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var reportsFolderPath = Path.Combine(baseDirectory, Defaults.REPORTS_FOLDER_NAME);
        var fileNames = Directory.EnumerateFiles(reportsFolderPath);
        if (fileNames?.Any() == true)
        {
            var allReports = new List<WBReportLine>();

            foreach (var fileName in fileNames)
            {
                var report = await WBReportReader
                    .FromFile(fileName).GetReportAsync(dateTimeFrom, dateTimeTo);
                if (report == null || !report.Any())
                    continue;

                allReports.AddRange(report);
            }

            await _importers[importerType.Value]().ImportAsync(allReports);
        }

        Console.ReadKey();
    }

    #endregion Methods

    #region Utils

    private static WBReportImporterType? GetReportImporterType()
    {
        Console.WriteLine("Куда импортировать?");
        Console.WriteLine("1 - в консоль");
        Console.WriteLine("2 - в консоль и соотнести с отгрузками МойСклад");

        var key = Console.ReadKey().Key;

        Console.WriteLine();

        return key switch
        {
            ConsoleKey.D1 => WBReportImporterType.Console,
            ConsoleKey.D2 => WBReportImporterType.ConsoleRelatedToMoySkladDemands,
            _ => null
        };
    }

    private static async Task InitSettingsAsync()
    {
        Settings.Default = await Settings.FromFileAsync(
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                Defaults.SETTINGS_FILE_NAME
            )
        );
    }

    #endregion Utils
}