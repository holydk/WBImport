namespace WBImport
{
    internal static class Defaults
    {
        internal const string DATE_TIME_FORMAT = "yyyy-MM-dd";
        internal const string REPORTS_FOLDER_NAME = "Reports";
        internal const string RUB = "руб.";
        internal const string SALE_DOC_TYPE_NAME = "Продажа";
        internal const string SETTINGS_FILE_NAME = "settings.json";

        public static Dictionary<WBReportImporterType, Func<IWBReportImporter>> Importers = new()
        {
            [WBReportImporterType.Console] = () => new ConsoleWBReportImporter(),
            [WBReportImporterType.ConsoleRelatedToMoySkladDemands] = () => new ConsoleWBReportRelatedToMSDemandsImporter()
        };
    }
}