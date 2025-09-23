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
        
        /// <summary>
        /// 用戶ID (用於個人化學習) - Phase 3 新增
        /// </summary>
        public int? UserId { get; set; }
        
        /// <summary>
        /// 上下文資訊 (Phase 3 新增)
        /// </summary>
        public VoiceContext? VoiceContext { get; set; }
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
        
        /// <summary>
        /// 解析建議 (Phase 3 新增)
        /// </summary>
        public List<ParseSuggestion> Suggestions { get; set; } = new();
        
        /// <summary>
        /// 學習資訊 (Phase 3 新增)
        /// </summary>
        public LearningInfo? LearningInfo { get; set; }
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

    // === Phase 2 新增模型 ===

    /// <summary>
    /// 解析狀態枚舉
    /// </summary>
    public enum ParseState
    {
        NotStarted,     // 尚未開始
        Parsing,        // 解析中
        Completed,      // 完全解析完成
        PartialSuccess, // 部分成功
        Failed,         // 解析失敗
        RequiresInput   // 需要用戶輸入
    }

    /// <summary>
    /// 提示優先級
    /// </summary>
    public enum HintPriority
    {
        Low = 1,
        Medium = 2,
        High = 3
    }

    /// <summary>
    /// 缺失欄位提示模型
    /// </summary>
    public class MissingFieldHint
    {
        public string FieldName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;
        public HintPriority Priority { get; set; }
        public string Icon { get; set; } = string.Empty;
    }

    /// <summary>
    /// 語音解析回應模型 (Phase 2)
    /// </summary>
    public class VoiceParseResponse
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public VoiceParseResult? ParseResult { get; set; }
        public ParseState ParseState { get; set; }
        public List<MissingFieldHint> MissingFieldHints { get; set; } = new();
        public string? NextStepSuggestion { get; set; }
        
        /// <summary>
        /// Phase 3 新增：對話式回應
        /// </summary>
        public ConversationalResponse? ConversationalResponse { get; set; }
    }

    #region Phase 3 智能化增強模型

    /// <summary>
    /// 用戶語音偏好模型
    /// </summary>
    public class UserVoicePreferences
    {
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 個人化關鍵字字典
        /// </summary>
        public Dictionary<string, PersonalKeyword> PersonalKeywords { get; set; } = new();

        /// <summary>
        /// 常用分類映射
        /// </summary>
        public Dictionary<string, CategoryMapping> FrequentCategories { get; set; } = new();

        /// <summary>
        /// 常用商家識別
        /// </summary>
        public Dictionary<string, VoiceMerchantMapping> FrequentMerchants { get; set; } = new();

        /// <summary>
        /// 個人化語音習慣
        /// </summary>
        public VoiceBehaviorPattern VoiceBehaviorPattern { get; set; } = new();

        /// <summary>
        /// 學習統計
        /// </summary>
        public LearningStatistics Statistics { get; set; } = new();
    }

    /// <summary>
    /// 個人化關鍵字
    /// </summary>
    public class PersonalKeyword
    {
        public string Keyword { get; set; } = string.Empty;
        public string MappedValue { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty; // Amount, Category, Merchant, PaymentMethod
        public double Confidence { get; set; }
        public int UsageCount { get; set; }
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public List<string> Synonyms { get; set; } = new();
    }

    /// <summary>
    /// 分類映射
    /// </summary>
    public class CategoryMapping
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<string> TriggerWords { get; set; } = new();
        public double Confidence { get; set; }
        public int UsageCount { get; set; }
        public DateTime LastUsed { get; set; } = DateTime.Now;
        public string? PreferredSubCategory { get; set; }
    }

    /// <summary>
    /// 語音商家映射
    /// </summary>
    public class VoiceMerchantMapping
    {
        public string MerchantName { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new();
        public string PreferredCategory { get; set; } = string.Empty;
        public string? PreferredPaymentMethod { get; set; }
        public double AverageAmount { get; set; }
        public int VisitCount { get; set; }
        public DateTime LastVisit { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 語音行為模式
    /// </summary>
    public class VoiceBehaviorPattern
    {
        /// <summary>
        /// 常用的語音表達方式
        /// </summary>
        public List<string> CommonPhrases { get; set; } = new();

        /// <summary>
        /// 偏好的語音順序 (Amount-First, Category-First, etc.)
        /// </summary>
        public string PreferredOrder { get; set; } = "Amount-First";

        /// <summary>
        /// 常用的時間表達
        /// </summary>
        public Dictionary<string, string> TimeExpressions { get; set; } = new();

        /// <summary>
        /// 語音輸入習慣統計
        /// </summary>
        public VoiceInputStats InputStats { get; set; } = new();
    }

    /// <summary>
    /// 語音輸入統計
    /// </summary>
    public class VoiceInputStats
    {
        public int TotalInputs { get; set; }
        public int SuccessfulParses { get; set; }
        public double AverageConfidence { get; set; }
        public TimeSpan AverageInputLength { get; set; }
        public Dictionary<string, int> HourlyUsage { get; set; } = new();
        public Dictionary<string, int> CategoryUsage { get; set; } = new();
    }

    /// <summary>
    /// 學習統計
    /// </summary>
    public class LearningStatistics
    {
        public int TotalLearningEvents { get; set; }
        public DateTime LastLearningEvent { get; set; } = DateTime.Now;
        public double AccuracyImprovement { get; set; }
        public Dictionary<string, int> FieldImprovements { get; set; } = new();
        public List<LearningEvent> RecentLearningEvents { get; set; } = new();
    }

    /// <summary>
    /// 學習事件
    /// </summary>
    public class LearningEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string EventType { get; set; } = string.Empty; // UserCorrection, PatternLearning, KeywordLearning
        public string FieldName { get; set; } = string.Empty;
        public string OriginalValue { get; set; } = string.Empty;
        public string CorrectedValue { get; set; } = string.Empty;
        public double ConfidenceImpact { get; set; }
    }

    /// <summary>
    /// 語音上下文
    /// </summary>
    public class VoiceContext
    {
        /// <summary>
        /// 對話會話ID
        /// </summary>
        public string? SessionId { get; set; }
        
        /// <summary>
        /// 先前的解析結果
        /// </summary>
        public VoiceParseResult? PreviousResult { get; set; }
        
        /// <summary>
        /// 輸入意圖 (NewRecord, Correction, Clarification)
        /// </summary>
        public string Intent { get; set; } = "NewRecord";
        
        /// <summary>
        /// 需要修正的欄位
        /// </summary>
        public List<string> FieldsToCorrect { get; set; } = new();
    }

    /// <summary>
    /// 解析建議
    /// </summary>
    public class ParseSuggestion
    {
        public string FieldName { get; set; } = string.Empty;
        public string SuggestedValue { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SuggestionType Type { get; set; }
    }

    /// <summary>
    /// 建議類型
    /// </summary>
    public enum SuggestionType
    {
        PersonalizedLearning,    // 基於個人化學習
        ContextualAnalysis,      // 基於上下文分析
        PatternMatching,         // 基於模式匹配
        SmartCorrection         // 智能修正建議
    }

    /// <summary>
    /// 學習資訊
    /// </summary>
    public class LearningInfo
    {
        public bool HasLearningOpportunity { get; set; }
        public List<string> LearnablePatterns { get; set; } = new();
        public Dictionary<string, string> SuggestedKeywords { get; set; } = new();
        public double LearningImpact { get; set; }
    }

    /// <summary>
    /// 對話式回應
    /// </summary>
    public class ConversationalResponse
    {
        public string? Question { get; set; }
        public List<string> SuggestedAnswers { get; set; } = new();
        public string ResponseType { get; set; } = string.Empty; // Question, Confirmation, Suggestion
        public string? SessionId { get; set; }
    }

    #endregion
}
