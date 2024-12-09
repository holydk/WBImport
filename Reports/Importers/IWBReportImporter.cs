using WBImport.Models;

namespace WBImport.Reports.Importers
{
    public interface IWBReportImporter
    {
        Task ImportAsync(IEnumerable<WBReportLine> report);
    }
}