using WBImport.Models;

namespace WBImport.Reports.Parsers
{
    public interface IWBReportParser
    {
        Task<IEnumerable<WBReportLine>> ParseAsync(Stream stream);
    }
}