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
    /// 子分類名稱
    /// </summary>
    public string SubCategory { get; set; } = string.Empty;
    
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

/// <summary>
/// 分類排行榜資料模型
/// </summary>
public class CategoryRankingData
{
    /// <summary>
    /// 排名
    /// </summary>
    public int Rank { get; set; }
    
    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 子分類名稱
    /// </summary>
    public string SubCategory { get; set; } = string.Empty;
    
    /// <summary>
    /// 金額
    /// </summary>
    public decimal Amount { get; set; }
    
    /// <summary>
    /// 記錄筆數
    /// </summary>
    public int RecordCount { get; set; }
    
    /// <summary>
    /// 平均金額
    /// </summary>
    public decimal AverageAmount { get; set; }
    
    /// <summary>
    /// 與上期變化百分比
    /// </summary>
    public decimal PercentageChange { get; set; }
    
    /// <summary>
    /// 趨勢方向 ("up", "down", "stable")
    /// </summary>
    public string Trend { get; set; } = string.Empty;
}

/// <summary>
/// 時間模式分析資料模型
/// </summary>
public class TimePatternAnalysisData
{
    /// <summary>
    /// 週日模式資料
    /// </summary>
    public List<WeekdayPatternData> WeekdayPatterns { get; set; } = new();
    
    /// <summary>
    /// 月內模式資料
    /// </summary>
    public List<MonthlyPatternData> MonthlyPatterns { get; set; } = new();
    
    /// <summary>
    /// 日常模式摘要
    /// </summary>
    public DailyPatternSummary DailySummary { get; set; } = new();
}

/// <summary>
/// 週日模式資料模型
/// </summary>
public class WeekdayPatternData
{
    /// <summary>
    /// 星期名稱
    /// </summary>
    public string Weekday { get; set; } = string.Empty;
    
    /// <summary>
    /// 星期索引 (0=週日, 1=週一...)
    /// </summary>
    public int WeekdayIndex { get; set; }
    
    /// <summary>
    /// 平均收入
    /// </summary>
    public decimal AverageIncome { get; set; }
    
    /// <summary>
    /// 平均支出
    /// </summary>
    public decimal AverageExpense { get; set; }
    
    /// <summary>
    /// 記錄筆數
    /// </summary>
    public int RecordCount { get; set; }
    
    /// <summary>
    /// 熱門分類
    /// </summary>
    public List<string> PopularCategories { get; set; } = new();
}

/// <summary>
/// 月內模式資料模型
/// </summary>
public class MonthlyPatternData
{
    /// <summary>
    /// 期間名稱
    /// </summary>
    public string Period { get; set; } = string.Empty;
    
    /// <summary>
    /// 開始日期
    /// </summary>
    public int StartDay { get; set; }
    
    /// <summary>
    /// 結束日期
    /// </summary>
    public int EndDay { get; set; }
    
    /// <summary>
    /// 平均收入
    /// </summary>
    public decimal AverageIncome { get; set; }
    
    /// <summary>
    /// 平均支出
    /// </summary>
    public decimal AverageExpense { get; set; }
    
    /// <summary>
    /// 記錄筆數
    /// </summary>
    public int RecordCount { get; set; }
    
    /// <summary>
    /// 佔全月比例
    /// </summary>
    public decimal Percentage { get; set; }
}

/// <summary>
/// 日常模式摘要資料模型
/// </summary>
public class DailyPatternSummary
{
    /// <summary>
    /// 最活躍的星期
    /// </summary>
    public string MostActiveWeekday { get; set; } = string.Empty;
    
    /// <summary>
    /// 支出最高的星期
    /// </summary>
    public string HighestExpenseWeekday { get; set; } = string.Empty;
    
    /// <summary>
    /// 最活躍的期間
    /// </summary>
    public string MostActivePeriod { get; set; } = string.Empty;
    
    /// <summary>
    /// 平日平均支出
    /// </summary>
    public decimal WeekdayAverageExpense { get; set; }
    
    /// <summary>
    /// 週末平均支出
    /// </summary>
    public decimal WeekendAverageExpense { get; set; }
}

/// <summary>
/// 比較分析資料模型
/// </summary>
public class ComparisonAnalysisData
{
    /// <summary>
    /// 期間名稱
    /// </summary>
    public string PeriodName { get; set; } = string.Empty;
    
    /// <summary>
    /// 當前期間資料
    /// </summary>
    public StatisticsSummaryData CurrentPeriod { get; set; } = new();
    
    /// <summary>
    /// 前一期間資料
    /// </summary>
    public StatisticsSummaryData PreviousPeriod { get; set; } = new();
    
    /// <summary>
    /// 收入成長率
    /// </summary>
    public decimal IncomeGrowthRate { get; set; }
    
    /// <summary>
    /// 支出成長率
    /// </summary>
    public decimal ExpenseGrowthRate { get; set; }
    
    /// <summary>
    /// 淨收支成長率
    /// </summary>
    public decimal NetIncomeGrowthRate { get; set; }
    
    /// <summary>
    /// 分類變化資料
    /// </summary>
    public List<CategoryComparisonData> CategoryChanges { get; set; } = new();
}

/// <summary>
/// 分類比較資料模型
/// </summary>
public class CategoryComparisonData
{
    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// 當前金額
    /// </summary>
    public decimal CurrentAmount { get; set; }
    
    /// <summary>
    /// 前期金額
    /// </summary>
    public decimal PreviousAmount { get; set; }
    
    /// <summary>
    /// 變化金額
    /// </summary>
    public decimal ChangeAmount { get; set; }
    
    /// <summary>
    /// 變化百分比
    /// </summary>
    public decimal ChangePercentage { get; set; }
    
    /// <summary>
    /// 變化方向
    /// </summary>
    public string ChangeDirection { get; set; } = string.Empty;
}

/// <summary>
/// 統計匯出請求模型
/// </summary>
public class StatisticsExportRequest
{
    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime StartDate { get; set; }
    
    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime EndDate { get; set; }
    
    /// <summary>
    /// 匯出格式
    /// </summary>
    public string Format { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否包含圖表
    /// </summary>
    public bool IncludeCharts { get; set; }
    
    /// <summary>
    /// 是否包含摘要
    /// </summary>
    public bool IncludeSummary { get; set; }
    
    /// <summary>
    /// 是否包含詳細記錄
    /// </summary>
    public bool IncludeDetailedRecords { get; set; }
    
    /// <summary>
    /// 包含的分析類型
    /// </summary>
    public List<string> IncludeAnalysis { get; set; } = new();
}
