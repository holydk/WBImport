using System.Text.Json;
using WBImport.Models;

namespace WBImport.Reports.Parsers
{
    internal sealed class JsonWBReportParser : IWBReportParser
    {
        #region Methods

        public async Task<IEnumerable<WBReportLine>> ParseAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanRead)
                return null;

            return await JsonSerializer.DeserializeAsync<WBReportLine[]>(stream);
        }

        #endregion Methods
    }
}