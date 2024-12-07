using WBImport.Models;

namespace WBImport.Importers
{
    public interface IWBReportImporter
    {
        Task ImportAsync(IEnumerable<WBReportLine> report);
    }
}