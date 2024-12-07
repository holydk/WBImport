using System.Text.Json;
using WBImport.Models;
using WBImport.Parsers;

namespace WBImport
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