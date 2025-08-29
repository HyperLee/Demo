namespace Demo.Models;

/// <summary>
/// 統計分析檢視模型
/// </summary>
public class StatisticsViewModel
{
    /// <summary>
    /// 月度收支趨勢資料
    /// </summary>
    public List<MonthlyTrendData> MonthlyTrend { get; set; } = new();
    
    /// <summary>
    /// 支出分類分析資料
    /// </summary>
    public List<CategoryAnalysisData> ExpenseCategories { get; set; } = new();
    
    /// <summary>
    /// 統計摘要資料
    /// </summary>
    public StatisticsSummaryData Summary { get; set; } = new();
    
    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime EndDate { get; set; }
}

/// <summary>
/// 月度收支趨勢資料模型
/// </summary>
public class MonthlyTrendData
{
    /// <summary>
    /// 月份 (格式: "2024-08")
    /// </summary>
    public string Month { get; set; } = string.Empty;
    
    /// <summary>
    /// 月份名稱 (格式: "2024年8月")
    /// </summary>
    public string MonthName { get; set; } = string.Empty;
    
    /// <summary>
    /// 收入金額
    /// </summary>
    public decimal Income { get; set; }
    
    /// <summary>
    /// 支出金額
    /// </summary>
    public decimal Expense { get; set; }
    
    /// <summary>
    /// 淨收支
    /// </summary>
    public decimal NetIncome { get; set; }
    
    /// <summary>
    /// 記錄總數
    /// </summary>
    public int TotalRecords { get; set; }
}

/// <summary>
/// 分類分析資料模型
/// </summary>
public class CategoryAnalysisData
{
    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 金額
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// 百分比
    /// </summary>
    public decimal Percentage { get; set; }
    
    /// <summary>
    /// 記錄筆數
    /// </summary>
    public int RecordCount { get; set; }
    
    /// <summary>
    /// 圖表顏色
    /// </summary>
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// 統計摘要資料模型
/// </summary>
public class StatisticsSummaryData
{
    /// <summary>
    /// 總收入
    /// </summary>
    public decimal TotalIncome { get; set; }
    
    /// <summary>
    /// 總支出
    /// </summary>
    public decimal TotalExpense { get; set; }
    
    /// <summary>
    /// 淨收支
    /// </summary>
    public decimal NetIncome { get; set; }
    
    /// <summary>
    /// 記錄總數
    /// </summary>
    public int TotalRecords { get; set; }
    
    /// <summary>
    /// 平均月收入
    /// </summary>
    public decimal AverageMonthlyIncome { get; set; }
    
    /// <summary>
    /// 平均月支出
    /// </summary>
    public decimal AverageMonthlyExpense { get; set; }
    
    /// <summary>
    /// 最大支出分類
    /// </summary>
    public string TopExpenseCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// 最大支出金額
    /// </summary>
    public decimal TopExpenseAmount { get; set; }
}
