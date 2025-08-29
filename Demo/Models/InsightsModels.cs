namespace Demo.Models;

/// <summary>
/// 智能洞察資料模型
/// </summary>
public class SmartInsight
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty; // "spending_pattern", "savings_opportunity", "trend_analysis"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty; // "success", "warning", "info", "danger"
    public decimal Impact { get; set; } // 預估影響金額
    public int Priority { get; set; } // 1-5 優先級
    public List<string> ActionItems { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
    public bool IsActionable { get; set; }
}

/// <summary>
/// 個人化建議資料模型
/// </summary>
public class PersonalizedRecommendation
{
    public string Category { get; set; } = string.Empty;
    public string Recommendation { get; set; } = string.Empty;
    public string Reasoning { get; set; } = string.Empty;
    public decimal PotentialSavings { get; set; }
    public string DifficultyLevel { get; set; } = string.Empty; // "easy", "medium", "hard"
    public int TimeFrame { get; set; } // 建議實施的天數
    public List<string> Steps { get; set; } = new();
}

/// <summary>
/// 財務健康分數資料模型
/// </summary>
public class FinancialHealthScore
{
    public int OverallScore { get; set; } // 0-100
    public string HealthLevel { get; set; } = string.Empty; // "excellent", "good", "fair", "poor"
    public List<HealthMetric> Metrics { get; set; } = new();
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public DateTime CalculatedDate { get; set; }
    
    // 前端 UI 顯示用的個別分數
    public int SavingsScore { get; set; } // 儲蓄能力分數
    public int BalanceScore { get; set; } // 收支平衡分數
    public int GrowthScore { get; set; } // 成長趨勢分數
}

/// <summary>
/// 健康指標資料模型
/// </summary>
public class HealthMetric
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Benchmark { get; set; } = string.Empty;
    public bool IsGood { get; set; }
}

/// <summary>
/// 節省機會資料模型
/// </summary>
public class SavingsOpportunity
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal PotentialMonthlySavings { get; set; }
    public decimal PotentialYearlySavings { get; set; }
    public string Method { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = string.Empty;
    public List<string> RequiredActions { get; set; } = new();
}

/// <summary>
/// 消費效率分析資料模型
/// </summary>
public class SpendingEfficiencyAnalysis
{
    public decimal EfficiencyScore { get; set; } // 0-100
    public List<CategoryEfficiency> CategoryEfficiencies { get; set; } = new();
    public List<string> OptimizationSuggestions { get; set; } = new();
    public decimal WastedAmount { get; set; }
    public decimal OptimalAmount { get; set; }
}

/// <summary>
/// 分類效率資料模型
/// </summary>
public class CategoryEfficiency
{
    public string Category { get; set; } = string.Empty;
    public decimal CurrentSpending { get; set; }
    public decimal OptimalSpending { get; set; }
    public decimal EfficiencyScore { get; set; }
    public string Status { get; set; } = string.Empty; // "efficient", "over_spending", "under_spending"
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// 支出預測資料模型
/// </summary>
public class ExpenseForecast
{
    public string Category { get; set; } = string.Empty;
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal ForecastedAmount { get; set; }
    public decimal ConfidenceInterval { get; set; }
    public string TrendDirection { get; set; } = string.Empty; // "increasing", "decreasing", "stable"
    public List<ForecastDataPoint> DataPoints { get; set; } = new();
}

/// <summary>
/// 預測資料點模型
/// </summary>
public class ForecastDataPoint
{
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public decimal UpperBound { get; set; }
    public decimal LowerBound { get; set; }
}

/// <summary>
/// 收入預測資料模型
/// </summary>
public class IncomeForecast
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public decimal ForecastedAmount { get; set; }
    public decimal ConfidenceInterval { get; set; }
    public string TrendDirection { get; set; } = string.Empty;
    public List<ForecastDataPoint> DataPoints { get; set; } = new();
}

/// <summary>
/// 現金流預測資料模型
/// </summary>
public class CashFlowProjection
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public List<CashFlowDataPoint> DataPoints { get; set; } = new();
    public decimal MinBalance { get; set; }
    public decimal MaxBalance { get; set; }
    public DateTime MinBalanceDate { get; set; }
    public DateTime MaxBalanceDate { get; set; }
    public List<string> Warnings { get; set; } = new();
}

/// <summary>
/// 現金流資料點模型
/// </summary>
public class CashFlowDataPoint
{
    public DateTime Date { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
    public decimal NetFlow { get; set; }
    public decimal CumulativeBalance { get; set; }
}

/// <summary>
/// 季節性趨勢資料模型
/// </summary>
public class SeasonalTrend
{
    public string Category { get; set; } = string.Empty;
    public int Month { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal SeasonalityIndex { get; set; } // 相對於年平均的比例
    public string TrendDescription { get; set; } = string.Empty;
    public List<int> HighActivityMonths { get; set; } = new();
    public List<int> LowActivityMonths { get; set; } = new();
}

/// <summary>
/// 目標達成預測資料模型
/// </summary>
public class GoalAchievementPrediction
{
    public decimal TargetAmount { get; set; }
    public DateTime TargetDate { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal RequiredAmount { get; set; }
    public decimal PredictedAmount { get; set; }
    public double AchievementProbability { get; set; } // 0-1
    public DateTime PredictedAchievementDate { get; set; }
    public int DaysAhead { get; set; } // 正數為提前，負數為延遲
    public List<string> Recommendations { get; set; } = new();
}
