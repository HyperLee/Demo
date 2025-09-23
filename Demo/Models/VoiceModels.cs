using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    /// <summary>
    /// 語音解析請求模型
    /// </summary>
    public class VoiceParseRequest
    {
        /// <summary>
        /// 語音識別的文字內容
        /// </summary>
        [Required]
        public string VoiceText { get; set; } = string.Empty;
        
        /// <summary>
        /// 使用情境：personal(個人記帳) 或 family(家庭記帳)
        /// </summary>
        public string Context { get; set; } = "personal";
        
        /// <summary>
        /// 語音識別的信心度
        /// </summary>
        public double Confidence { get; set; } = 1.0;
    }

    /// <summary>
    /// 語音解析結果模型 (Phase 1 增強版)
    /// </summary>
    public class VoiceParseResult
    {
        // === 現有欄位 (保持不變) ===
        /// <summary>
        /// 原始語音文字
        /// </summary>
        public string OriginalText { get; set; } = string.Empty;
        
        /// <summary>
        /// 收支類型：Income(收入) 或 Expense(支出)
        /// </summary>
        public string Type { get; set; } = "Expense";
        
        /// <summary>
        /// 金額
        /// </summary>
        public decimal? Amount { get; set; }
        
        /// <summary>
        /// 類別
        /// </summary>
        public string? Category { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 分攤方式（家庭模式專用）
        /// </summary>
        public string? SplitType { get; set; }
        
        /// <summary>
        /// 解析時間
        /// </summary>
        public DateTime ParsedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 解析信心度（0-1）
        /// </summary>
        public double ParseConfidence { get; set; } = 1.0;
        
        /// <summary>
        /// 是否解析成功
        /// </summary>
        public bool IsSuccess { get; set; } = true;
        
        /// <summary>
        /// 錯誤訊息
        /// </summary>
        public string? ErrorMessage { get; set; }
        
        // === Phase 1 新增欄位 ===
        /// <summary>
        /// 解析的日期
        /// </summary>
        public DateTime? Date { get; set; }
        
        /// <summary>
        /// 付款方式
        /// </summary>
        public string? PaymentMethod { get; set; }
        
        /// <summary>
        /// 細分類
        /// </summary>
        public string? SubCategory { get; set; }
        
        /// <summary>
        /// 商家名稱
        /// </summary>
        public string? MerchantName { get; set; }
        
        /// <summary>
        /// 備註資訊 (從描述中分離出來的額外資訊)
        /// </summary>
        public string? Note { get; set; }
        
        /// <summary>
        /// 各欄位解析信心度 (0-1)
        /// </summary>
        public Dictionary<string, double> FieldConfidence { get; set; } = new();
        
        /// <summary>
        /// 無法解析的剩餘內容
        /// </summary>
        public string? UnparsedContent { get; set; }
        
        /// <summary>
        /// 解析過程中的中間結果 (供除錯使用)
        /// </summary>
        public Dictionary<string, object> ParsedSteps { get; set; } = new();
    }

    /// <summary>
    /// 家庭語音記帳請求（繼承快速記帳請求）
    /// </summary>
    public class VoiceFamilyExpenseRequest
    {
        /// <summary>
        /// 收支類型
        /// </summary>
        [Required]
        public string Type { get; set; } = "支出";
        
        /// <summary>
        /// 金額
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於 0")]
        public decimal Amount { get; set; }
        
        /// <summary>
        /// 類別
        /// </summary>
        [Required]
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// 描述
        /// </summary>
        [Required]
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; } = DateTime.Today.ToString("yyyy-MM-dd");
        
        /// <summary>
        /// 分攤方式
        /// </summary>
        public string SplitType { get; set; } = "我支付";
        
        /// <summary>
        /// 原始語音文字
        /// </summary>
        public string VoiceText { get; set; } = string.Empty;
        
        /// <summary>
        /// 解析信心度
        /// </summary>
        public double ParseConfidence { get; set; } = 1.0;
    }

    /// <summary>
    /// 語音命令類型枚舉
    /// </summary>
    public enum VoiceCommandType
    {
        /// <summary>
        /// 新增記錄
        /// </summary>
        AddRecord,
        
        /// <summary>
        /// 查詢記錄
        /// </summary>
        QueryRecord,
        
        /// <summary>
        /// 刪除記錄
        /// </summary>
        DeleteRecord,
        
        /// <summary>
        /// 修改記錄
        /// </summary>
        UpdateRecord,
        
        /// <summary>
        /// 未知命令
        /// </summary>
        Unknown
    }

    /// <summary>
    /// 語音識別狀態
    /// </summary>
    public enum VoiceRecognitionStatus
    {
        /// <summary>
        /// 閒置
        /// </summary>
        Idle,
        
        /// <summary>
        /// 聆聽中
        /// </summary>
        Listening,
        
        /// <summary>
        /// 處理中
        /// </summary>
        Processing,
        
        /// <summary>
        /// 完成
        /// </summary>
        Completed,
        
        /// <summary>
        /// 錯誤
        /// </summary>
        Error
    }

    /// <summary>
    /// 解析配置模型
    /// </summary>
    public class VoiceParseConfig
    {
        /// <summary>
        /// 最低信心度閾值
        /// </summary>
        public double MinConfidenceThreshold { get; set; } = 0.3;
        
        /// <summary>
        /// 是否啟用模糊匹配
        /// </summary>
        public bool EnableFuzzyMatching { get; set; } = true;
        
        /// <summary>
        /// 日期解析模式
        /// </summary>
        public DateParseMode DateParseMode { get; set; } = DateParseMode.Flexible;
    }

    /// <summary>
    /// 日期解析模式
    /// </summary>
    public enum DateParseMode
    {
        /// <summary>
        /// 嚴格模式：只解析明確的日期格式
        /// </summary>
        Strict,
        
        /// <summary>
        /// 彈性模式：支援相對日期 (今天、昨天等)
        /// </summary>
        Flexible,
        
        /// <summary>
        /// 智能模式：根據上下文推測日期
        /// </summary>
        Smart
    }
}
