using WBImport.Models;

namespace WBImport.Parsers
{
    public interface IWBReportParser
    {
        Task<IEnumerable<WBReportLine>> ParseAsync(Stream stream);
    }
}