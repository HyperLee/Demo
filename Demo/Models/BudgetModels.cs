namespace Demo.Models;

/// <summary>
/// 預算項目資料模型
/// </summary>
public class BudgetItem
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// 預算表現資料模型
/// </summary>
public class BudgetPerformance
{
    public BudgetItem Budget { get; set; } = new();
    public decimal ActualAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal UsedPercentage { get; set; }
    public int DaysRemaining { get; set; }
    public string Status { get; set; } = string.Empty; // "on_track", "warning", "exceeded"
    public decimal DailyAverageSpending { get; set; }
    public decimal RecommendedDailySpending { get; set; }
}

/// <summary>
/// 預算摘要資料模型
/// </summary>
public class BudgetSummary
{
    public decimal TotalBudgetAmount { get; set; }
    public decimal TotalActualAmount { get; set; }
    public decimal TotalRemainingAmount { get; set; }
    public decimal OverallUsedPercentage { get; set; }
    public int CategoriesOnTrack { get; set; }
    public int CategoriesWarning { get; set; }
    public int CategoriesExceeded { get; set; }
    public int DaysRemainingInMonth { get; set; }
}

/// <summary>
/// 預算警報資料模型
/// </summary>
public class BudgetAlert
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string AlertType { get; set; } = string.Empty; // "approaching_limit", "exceeded", "no_activity"
    public decimal BudgetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal Percentage { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }
}

/// <summary>
/// 預算建議資料模型
/// </summary>
public class BudgetSuggestion
{
    public string Category { get; set; } = string.Empty;
    public decimal SuggestedAmount { get; set; }
    public decimal HistoricalAverage { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public decimal ConfidenceScore { get; set; } // 0-1
    public List<string> ConsiderationFactors { get; set; } = new();
}

/// <summary>
/// 建立預算請求資料模型
/// </summary>
public class CreateBudgetRequest
{
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// 更新預算請求資料模型
/// </summary>
public class UpdateBudgetRequest
{
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public decimal BudgetAmount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

/// <summary>
/// 最佳預算計劃資料模型
/// </summary>
public class OptimalBudgetPlan
{
    public decimal TotalBudget { get; set; }
    public List<BudgetAllocation> Allocations { get; set; } = new();
    public List<string> OptimizationStrategies { get; set; } = new();
    public decimal ProjectedSavings { get; set; }
    public string ConfidenceLevel { get; set; } = string.Empty;
}

/// <summary>
/// 預算分配資料模型
/// </summary>
public class BudgetAllocation
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Percentage { get; set; }
    public string Priority { get; set; } = string.Empty; // "high", "medium", "low"
    public string Justification { get; set; } = string.Empty;
}
