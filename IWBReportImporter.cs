namespace WBImport
{
    public interface IWBReportImporter
    {
        Task ImportAsync(IEnumerable<WBReportLine> report);
    }
}