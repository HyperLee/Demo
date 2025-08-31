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
    /// 語音解析結果模型
    /// </summary>
    public class VoiceParseResult
    {
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
}
