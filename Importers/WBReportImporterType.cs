namespace WBImport.Importers
{
    public enum WBReportImporterType
    {
        /// <summary>
        /// Import to console.
        /// </summary>
        Console = 1,

        /// <summary>
        /// Import to console. The products in the report are related to the demands of ERP MoySklad.
        /// </summary>
        ConsoleRelatedToMoySkladDemands,

        /// <summary>
        /// Update the related MoySklad demands.
        /// </summary>
        UpdateMSDemands
    }
}