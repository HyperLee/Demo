namespace Demo.Models
{
    /// <summary>
    /// 匯率資料模型
    /// </summary>
    public class ExchangeRate
    {
        /// <summary>
        /// 貨幣代碼 (如 USD, JPY)
        /// </summary>
        public string CurrencyCode { get; set; } = string.Empty;

        /// <summary>
        /// 貨幣名稱 (如 美金, 日圓)
        /// </summary>
        public string CurrencyName { get; set; } = string.Empty;

        /// <summary>
        /// 即期買入匯率
        /// </summary>
        public decimal BuyRate { get; set; }

        /// <summary>
        /// 即期賣出匯率
        /// </summary>
        public decimal SellRate { get; set; }

        /// <summary>
        /// 現金買入匯率
        /// </summary>
        public decimal CashBuyRate { get; set; }

        /// <summary>
        /// 現金賣出匯率
        /// </summary>
        public decimal CashSellRate { get; set; }
    }

    /// <summary>
    /// 匯率資料集合
    /// </summary>
    public class ExchangeRateData
    {
        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 資料來源
        /// </summary>
        public string Source { get; set; } = "台灣銀行CSV API";

        /// <summary>
        /// 匯率清單
        /// </summary>
        public List<ExchangeRate> Rates { get; set; } = new List<ExchangeRate>();
    }

    /// <summary>
    /// 匯率計算結果
    /// </summary>
    public class ExchangeCalculationResult
    {
        /// <summary>
        /// 計算結果金額
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// 使用的匯率
        /// </summary>
        public decimal ExchangeRate { get; set; }

        /// <summary>
        /// 來源貨幣
        /// </summary>
        public string FromCurrency { get; set; } = string.Empty;

        /// <summary>
        /// 目標貨幣
        /// </summary>
        public string ToCurrency { get; set; } = string.Empty;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
