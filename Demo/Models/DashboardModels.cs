namespace Demo.Models;

/// <summary>
/// 財務儀表板統計資料模型
/// 包含收支統計、趨勢分析等核心數據
/// </summary>
public class DashboardStats
{
    /// <summary>本月總收入</summary>
    public decimal CurrentMonthIncome { get; set; }
    
    /// <summary>本月總支出</summary>
    public decimal CurrentMonthExpense { get; set; }
    
    /// <summary>淨收支 (收入-支出)</summary>
    public decimal NetIncome { get; set; }
    
    /// <summary>平均每日支出</summary>
    public decimal DailyAverageExpense { get; set; }
    
    /// <summary>年度至今總收入</summary>
    public decimal YearToDateIncome { get; set; }
    
    /// <summary>年度至今總支出</summary>
    public decimal YearToDateExpense { get; set; }
    
    /// <summary>趨勢分析資料</summary>
    public List<MonthlyTrend> TrendData { get; set; } = [];
    
    /// <summary>分類統計資料</summary>
    public List<CategorySummary> CategoryData { get; set; } = [];
    
    /// <summary>與上期比較數據</summary>
    public ComparisonStats? ComparisonData { get; set; }
}

/// <summary>
/// 月度趨勢分析模型
/// 用於展示收支變化趨勢
/// </summary>
public class MonthlyTrend
{
    /// <summary>月份 (格式: YYYY-MM)</summary>
    public string Month { get; set; } = string.Empty;
    
    /// <summary>月份顯示名稱 (如: 2024年1月)</summary>
    public string MonthName { get; set; } = string.Empty;
    
    /// <summary>該月收入</summary>
    public decimal Income { get; set; }
    
    /// <summary>該月支出</summary>
    public decimal Expense { get; set; }
    
    /// <summary>淨收支</summary>
    public decimal NetIncome => Income - Expense;
    
    /// <summary>交易筆數</summary>
    public int TransactionCount { get; set; }
}

/// <summary>
/// 分類統計摘要模型
/// 用於圓餅圖和分類分析
/// </summary>
public class CategorySummary
{
    /// <summary>分類名稱</summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>該分類金額</summary>
    public decimal Amount { get; set; }
    
    /// <summary>佔總支出百分比</summary>
    public decimal Percentage { get; set; }
    
    /// <summary>圖表顏色</summary>
    public string Color { get; set; } = "#3498db";
    
    /// <summary>交易筆數</summary>
    public int TransactionCount { get; set; }
    
    /// <summary>平均每筆金額</summary>
    public decimal AverageAmount => TransactionCount > 0 ? Amount / TransactionCount : 0;
}

/// <summary>
/// 比較分析模型
/// 用於顯示與上期的變化情況
/// </summary>
public class ComparisonStats
{
    /// <summary>收入變化率 (%)</summary>
    public decimal IncomeChangePercent { get; set; }
    
    /// <summary>支出變化率 (%)</summary>
    public decimal ExpenseChangePercent { get; set; }
    
    /// <summary>淨收支變化率 (%)</summary>
    public decimal NetIncomeChangePercent { get; set; }
    
    /// <summary>收入變化金額</summary>
    public decimal IncomeChangeAmount { get; set; }
    
    /// <summary>支出變化金額</summary>
    public decimal ExpenseChangeAmount { get; set; }
    
    /// <summary>比較期間描述</summary>
    public string ComparisonPeriod { get; set; } = "上月";
}

/// <summary>
/// 時間範圍枚舉
/// 用於篩選和分析不同時間區間的數據
/// </summary>
public enum DashboardTimeRange
{
    /// <summary>本月</summary>
    ThisMonth,
    
    /// <summary>近3個月</summary>
    Last3Months,
    
    /// <summary>近6個月</summary>
    Last6Months,
    
    /// <summary>今年</summary>
    ThisYear,
    
    /// <summary>全部</summary>
    All
}

/// <summary>
/// 儀表板快速統計卡片模型
/// 用於頂部統計區域顯示
/// </summary>
public class DashboardCard
{
    /// <summary>標題</summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>數值</summary>
    public decimal Value { get; set; }
    
    /// <summary>格式化後的顯示值</summary>
    public string FormattedValue { get; set; } = string.Empty;
    
    /// <summary>圖示 CSS 類別</summary>
    public string IconClass { get; set; } = "fas fa-dollar-sign";
    
    /// <summary>卡片背景色</summary>
    public string BackgroundColor { get; set; } = "primary";
    
    /// <summary>變化百分比</summary>
    public decimal? ChangePercent { get; set; }
    
    /// <summary>變化趨勢 (up/down/stable)</summary>
    public string? Trend { get; set; }
    
    /// <summary>副標題或說明文字</summary>
    public string? Subtitle { get; set; }
}

/// <summary>
/// AJAX 回應模型
/// 用於動態更新儀表板數據
/// </summary>
public class DashboardUpdateResponse
{
    /// <summary>統計數據</summary>
    public DashboardStats Stats { get; set; } = new();
    
    /// <summary>統計卡片數據</summary>
    public List<DashboardCard> Cards { get; set; } = [];
    
    /// <summary>最近交易記錄</summary>
    public List<AccountingRecord> RecentTransactions { get; set; } = [];
    
    /// <summary>更新時間</summary>
    public DateTime UpdateTime { get; set; } = DateTime.Now;
    
    /// <summary>是否成功</summary>
    public bool Success { get; set; } = true;
    
    /// <summary>錯誤訊息</summary>
    public string? Message { get; set; }
}
