using WBImport.Models;
using WBImport.Parsers;

namespace WBImport.Infrastructure
{
    internal sealed class WBReportReader
    {
        #region Fields

        private readonly IWBReportParser _parser;
        private readonly Func<Stream> _reportFactory;

        #endregion Fields

        #region Ctor

        private WBReportReader(Func<Stream> streamFactory, IWBReportParser parser)
        {
            _reportFactory = streamFactory;
            _parser = parser;
        }

        #endregion Ctor

        #region Methods

        public static WBReportReader FromFile(string fileName)
        {
            ArgumentException.ThrowIfNullOrEmpty(fileName);

            IWBReportParser parser = null;

            try
            {
                parser = WBReportParserFactory.Create(fileName);
            }
            catch (InvalidOperationException)
            {
            }

            if (parser == null)
                return new WBReportReader(() => Stream.Null, null);

            return new WBReportReader(() => File.OpenRead(fileName), parser);
        }

        public async Task<IEnumerable<WBReportLine>> GetReportAsync(DateTime? from = null, DateTime? to = null)
        {
            using var stream = _reportFactory();

            if (!stream.CanRead || stream.Length == 0)
                return null;

            var report = await _parser.ParseAsync(stream);
            if (report == null || !report.Any())
                return null;

            if (from.HasValue)
                report = report.Where(doc => doc.OrderDt >= from);

            if (to.HasValue)
                report = report.Where(doc => doc.OrderDt <= to);

            return report;
        }

        #endregion Methods
    }
}