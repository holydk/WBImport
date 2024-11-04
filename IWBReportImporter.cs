namespace WBReportImport
{
    public interface IWBReportImporter
    {
        Task ImportAsync(IEnumerable<WBReportLine> report);
    }
}