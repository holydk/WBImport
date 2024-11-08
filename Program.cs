using WBImport;

internal sealed class Program
{
    #region Methods

    public static async Task Main(string[] args)
    {
        await InitSettingsAsync();

        var importerType = GetReportImporterType();
        if (!importerType.HasValue)
            return;

        var reportFileNames = GetReportFiles();
        if (reportFileNames == null || !reportFileNames.Any())
        {
            Console.WriteLine("Папка с отчетами пуста.");
            return;
        }

        // todo: read dateTime from console or settings ?
        var dateTimeFrom = DateTime.Now.AddMonths(-1);
        var dateTimeTo = DateTime.Now;
        var allReports = new List<WBReportLine>();

        foreach (var fileName in reportFileNames)
        {
            var report = await WBReportReader
                .FromFile(fileName).GetReportAsync(dateTimeFrom, dateTimeTo);
            if (report == null || !report.Any())
                continue;

            allReports.AddRange(report);
        }

        await Defaults.Importers[importerType.Value]().ImportAsync(allReports);

        Console.ReadKey();
    }

    #endregion Methods

    #region Utils

    private static IEnumerable<string> GetReportFiles()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var reportsFolderPath = Path.Combine(baseDirectory, Defaults.REPORTS_FOLDER_NAME);
        if (!Directory.Exists(reportsFolderPath))
            throw new InvalidOperationException($"Папка с отчетами для пути \"{reportsFolderPath}\" не найдена.");

        return Directory.EnumerateFiles(reportsFolderPath);
    }

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