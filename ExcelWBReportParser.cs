using System.Text;
using ExcelDataReader;

namespace WBImport
{
    internal sealed class ExcelWBReportParser : IWBReportParser
    {
        static ExcelWBReportParser()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        #region Methods

        public Task<IEnumerable<WBReportLine>> ParseAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);

            if (!stream.CanRead)
                return Task.FromResult<IEnumerable<WBReportLine>>(null);

            var result = new List<WBReportLine>();

            using var reader = ExcelReaderFactory.CreateReader(stream);

            var dataSet = reader.AsDataSet();
            if (dataSet.Tables == null || dataSet.Tables.Count == 0)
                return Task.FromResult<IEnumerable<WBReportLine>>(result);

            var rows = dataSet.Tables[0].Rows;
            if (rows == null || rows.Count <= 1)
                return Task.FromResult<IEnumerable<WBReportLine>>(result);

            for (var i = 1; i < rows.Count; i++)
            {
                var reportLine = new WBReportLine
                {
                    Acceptance = rows[i].ItemArray[36] != DBNull.Value ? Convert.ToDecimal(rows[i].ItemArray[61]) : decimal.Zero,
                    AssemblyId = rows[i].ItemArray[53] != DBNull.Value ? Convert.ToInt64(rows[i].ItemArray[53]) : 0,
                    Barcode = rows[i].ItemArray[8] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[8]) : string.Empty,
                    BonusTypeName = rows[i].ItemArray[42] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[42]) : string.Empty,
                    DeliveryRub = rows[i].ItemArray[36] != DBNull.Value ? Convert.ToDecimal(rows[i].ItemArray[36]) : decimal.Zero,
                    DocTypeName = rows[i].ItemArray[9] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[9]) : string.Empty,
                    NumberId = rows[i].ItemArray[3] != DBNull.Value ? Convert.ToInt64(rows[i].ItemArray[3]) : 0,
                    OrderDt = rows[i].ItemArray[11] != DBNull.Value ? Convert.ToDateTime(rows[i].ItemArray[11]) : null,
                    Penalty = rows[i].ItemArray[40] != DBNull.Value ? Convert.ToDecimal(rows[i].ItemArray[40]) : decimal.Zero,
                    PpvzForPay = rows[i].ItemArray[33] != DBNull.Value ? Convert.ToDecimal(rows[i].ItemArray[33]) : decimal.Zero,
                    RebillLogisticCost = rows[i].ItemArray[57] != DBNull.Value ? Convert.ToDecimal(rows[i].ItemArray[57]) : decimal.Zero,
                    RetailPriceWithdiscRub = rows[i].ItemArray[19] != DBNull.Value ? Convert.ToDecimal(rows[i].ItemArray[19]) : decimal.Zero,
                    SaName = rows[i].ItemArray[5] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[5]) : string.Empty,
                    ShkId = rows[i].ItemArray[55] != DBNull.Value ? Convert.ToInt64(rows[i].ItemArray[55]) : 0,
                    StickerId = rows[i].ItemArray[43] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[43]) : string.Empty,
                    SupplierOperName = rows[i].ItemArray[10] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[10]) : string.Empty,
                    TsName = rows[i].ItemArray[7] != DBNull.Value ? Convert.ToString(rows[i].ItemArray[7]) : string.Empty,
                };

                result.Add(reportLine);
            }

            return Task.FromResult<IEnumerable<WBReportLine>>(result);
        }

        #endregion Methods
    }
}