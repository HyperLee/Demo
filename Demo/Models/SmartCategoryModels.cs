using System;
using System.Collections.Generic;
using System.Linq;

namespace Demo.Models
{
    /// <summary>
    /// 智能分類建議模型
    /// </summary>
    public class CategorySuggestion
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SuggestionSourceType SourceType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 分類建議來源類型
    /// </summary>
    public enum SuggestionSourceType
    {
        RuleBased,      // 規則引擎
        KeywordBased,   // 關鍵字匹配
        HistoryBased,   // 歷史相似度
        MerchantBased,  // 商家匹配
        AmountBased,    // 金額範圍
        MachineLearning // 機器學習
    }

    /// <summary>
    /// 分類規則模型
    /// </summary>
    public class CategoryRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new List<string>();
        public List<string> MerchantPatterns { get; set; } = new List<string>();
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public double MinConfidence { get; set; } = 0.7;
        public int Priority { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; } = 0;
    }

    /// <summary>
    /// 分類訓練資料模型
    /// </summary>
    public class CategoryTrainingData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = true;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public CategoryFeatures Features { get; set; } = new CategoryFeatures();
    }

    /// <summary>
    /// 分類特徵模型
    /// </summary>
    public class CategoryFeatures
    {
        public List<string> Keywords { get; set; } = new List<string>();
        public string MerchantType { get; set; } = string.Empty;
        public string AmountRange { get; set; } = string.Empty;
        public string TimePattern { get; set; } = string.Empty;
        public int TextLength { get; set; }
        public bool HasNumbers { get; set; }
        public string Language { get; set; } = "zh-TW";
        public List<string> ExtractedEntities { get; set; } = new List<string>();
    }

    /// <summary>
    /// 商家對應模型
    /// </summary>
    public class MerchantMapping
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MerchantName { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string MerchantType { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new List<string>();
        public double Confidence { get; set; } = 1.0;
        public bool IsVerified { get; set; } = false;
    }

    /// <summary>
    /// 智能分類請求模型
    /// </summary>
    public class SmartCategoryRequest
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public int MaxSuggestions { get; set; } = 5;
    }

    /// <summary>
    /// 分類回饋模型
    /// </summary>
    public class CategoryFeedback
    {
        public string CategoryId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 關鍵字字典模型
    /// </summary>
    public class KeywordDictionary
    {
        public Dictionary<string, List<string>> CategoryKeywords { get; set; } = new Dictionary<string, List<string>>();
        public Dictionary<string, string> MerchantAliases { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, List<string>> AmountRangeKeywords { get; set; } = new Dictionary<string, List<string>>();
        public List<string> StopWords { get; set; } = new List<string>();
    }

    /// <summary>
    /// 模型準確度報告
    /// </summary>
    public class ModelAccuracyReport
    {
        public double OverallAccuracy { get; set; }
        public Dictionary<string, double> CategoryAccuracy { get; set; } = new Dictionary<string, double>();
        public int TotalTestCases { get; set; }
        public int CorrectPredictions { get; set; }
        public DateTime EvaluationDate { get; set; }
        public List<CategoryPerformance> DetailedPerformance { get; set; } = new List<CategoryPerformance>();
    }

    /// <summary>
    /// 分類效能詳情
    /// </summary>
    public class CategoryPerformance
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int TotalCases { get; set; }
        public int CorrectPredictions { get; set; }
        public double Accuracy { get; set; }
        public double AverageConfidence { get; set; }
        public List<string> CommonMistakes { get; set; } = new List<string>();
    }
}
