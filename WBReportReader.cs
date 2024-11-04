namespace WBReportImport
{
    public class WBReportReader
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
            ArgumentNullException.ThrowIfNullOrEmpty(fileName, nameof(fileName));

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

            return new WBReportReader(() => File.OpenRead(fileName), WBReportParserFactory.Create(fileName));
        }

        public async Task<IEnumerable<WBReportLine>> GetReportAsync()
        {
            using var stream = _reportFactory();

            if (!stream.CanRead || stream.Length == 0)
                return null;

            return await _parser.ParseAsync(stream);
        }

        #endregion Methods
    }
}