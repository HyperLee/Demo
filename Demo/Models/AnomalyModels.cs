namespace Demo.Models;

/// <summary>
/// 異常警報資料模型
/// </summary>
public class AnomalyAlert
{
    public int Id { get; set; }
    public DateTime DetectedDate { get; set; }
    public string AlertType { get; set; } = string.Empty; // "amount", "frequency", "category", "pattern"
    public string Severity { get; set; } = string.Empty; // "low", "medium", "high", "critical"
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal BaselineAmount { get; set; }
    public decimal DeviationPercentage { get; set; }
    public string Recommendation { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public bool IsDismissed { get; set; }
}

/// <summary>
/// 分類異常資料模型
/// </summary>
public class CategoryAnomaly
{
    public string Category { get; set; } = string.Empty;
    public decimal CurrentAmount { get; set; }
    public decimal HistoricalAverage { get; set; }
    public decimal StandardDeviation { get; set; }
    public double ZScore { get; set; }
    public string AnomalyType { get; set; } = string.Empty; // "spike", "drop", "trend_change"
    public int DaysFromNormal { get; set; }
}

/// <summary>
/// 頻率異常資料模型
/// </summary>
public class FrequencyAnomaly
{
    public string Category { get; set; } = string.Empty;
    public int CurrentFrequency { get; set; }
    public double HistoricalAverageFrequency { get; set; }
    public string AnomalyType { get; set; } = string.Empty; // "increased", "decreased", "irregular"
    public DateTime FirstDetectedDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// 風險評估資料模型
/// </summary>
public class RiskAssessment
{
    public int OverallScore { get; set; } // 0-100
    public string RiskLevel { get; set; } = string.Empty; // "low", "medium", "high"
    public List<RiskFactor> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public decimal DebtToIncomeRatio { get; set; }
    public decimal SavingsRate { get; set; }
    public decimal ExpenseVolatility { get; set; }
}

/// <summary>
/// 風險因子資料模型
/// </summary>
public class RiskFactor
{
    public string Factor { get; set; } = string.Empty;
    public int Score { get; set; } // 0-100
    public string Impact { get; set; } = string.Empty; // "positive", "negative", "neutral"
    public string Description { get; set; } = string.Empty;
}
