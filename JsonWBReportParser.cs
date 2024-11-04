using System.Text.Json;

namespace WBReportImport
{
    internal class JsonWBReportParser : IWBReportParser
    {
        #region Methods

        public async Task<IEnumerable<WBReportLine>> ParseAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            return await JsonSerializer.DeserializeAsync<WBReportLine[]>(stream);
        }

        #endregion Methods
    }
}