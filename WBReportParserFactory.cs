namespace WBImport
{
    internal sealed class WBReportParserFactory
    {
        private static readonly Dictionary<string, Func<IWBReportParser>> _map = new()
        {
            [".json"] = () => new JsonWBReportParser(),
            [".xlsx"] = () => new ExcelWBReportParser(),
        };

        #region Methods

        public static IWBReportParser Create(string fileName)
        {
            var extension = Path.GetExtension(fileName);

            if (!_map.TryGetValue(extension, out var factory))
                throw new InvalidOperationException($"Cannot create parser for file extension \"{extension}\".");

            return factory();
        }

        #endregion Methods
    }
}