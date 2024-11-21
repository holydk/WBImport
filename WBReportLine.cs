using System.Text.Json.Serialization;

namespace WBImport
{
    public class WBReportLine
    {
        #region Properties

        /// <summary>
        /// Стоимость платной приёмки
        /// </summary>
        [JsonPropertyName("acceptance")]
        public decimal Acceptance { get; set; }

        /// <summary>
        /// Номер сборочного задания
        /// </summary>
        [JsonPropertyName("assembly_id")]
        public long AssemblyId { get; set; }

        /// <summary>
        /// Штрих-код товара продавца
        /// </summary>
        [JsonPropertyName("barcode")]
        public string Barcode { get; set; }

        /// <summary>
        /// Обоснование для оплаты
        /// </summary>
        [JsonPropertyName("bonus_type_name")]
        public string BonusTypeName { get; set; }

        /// <summary>
        /// Стоимость логистики
        /// </summary>
        [JsonPropertyName("delivery_rub")]
        public decimal DeliveryRub { get; set; }

        /// <summary>
        /// Тип документа
        /// </summary>
        [JsonPropertyName("doc_type_name")]
        public string DocTypeName { get; set; }

        /// <summary>
        /// Артикул WB
        /// </summary>
        [JsonPropertyName("nm_id")]
        public long NumberId { get; set; }

        /// <summary>
        /// Дата заказа. Присылается с явным указанием часового пояса
        /// </summary>
        [JsonPropertyName("order_dt")]
        public DateTime? OrderDt { get; set; }

        /// <summary>
        /// Штрафы
        /// </summary>
        [JsonPropertyName("penalty")]
        public decimal Penalty { get; set; }

        /// <summary>
        /// К перечислению продавцу за реализованный товар
        /// </summary>
        [JsonPropertyName("ppvz_for_pay")]
        public decimal PpvzForPay { get; set; }

        /// <summary>
        /// Возмещение издержек по перевозке. Поле будет в ответе при наличии значения
        /// </summary>
        [JsonPropertyName("rebill_logistic_cost")]
        public decimal RebillLogisticCost { get; set; }

        /// <summary>
        /// Цена розничная с учетом согласованной скидки
        /// </summary>
        [JsonPropertyName("retail_price_withdisc_rub")]
        public decimal RetailPriceWithdiscRub { get; set; }

        /// <summary>
        /// Артикул продавца
        /// </summary>
        [JsonPropertyName("sa_name")]
        public string SaName { get; set; }

        /// <summary>
        /// Штрих-код / стикер WB
        /// </summary>
        [JsonPropertyName("shk_id")]
        public long ShkId { get; set; }

        /// <summary>
        /// Цифровое значение стикера, который клеится на товар в процессе сборки заказа по схеме "Маркетплейс"
        /// </summary>
        [JsonPropertyName("sticker_id")]
        public string StickerId { get; set; }

        /// <summary>
        /// Обоснование для оплаты
        /// </summary>
        [JsonPropertyName("supplier_oper_name")]
        public string SupplierOperName { get; set; }

        /// <summary>
        /// Размер
        /// </summary>
        [JsonPropertyName("ts_name")]
        public string TsName { get; set; }

        #endregion Properties
    }
}