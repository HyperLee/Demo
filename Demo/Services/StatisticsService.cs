using Demo.Models;

namespace Demo.Services;

/// <summary>
/// 統計分析服務介面
/// </summary>
public interface IStatisticsService
{
    /// <summary>
    /// 取得月度收支趨勢資料
    /// </summary>
    /// <param name="months">月份數量</param>
    /// <returns>月度趨勢資料</returns>
    Task<List<MonthlyTrendData>> GetMonthlyTrendAsync(int months = 6);
    
    /// <summary>
    /// 取得支出分類分析資料
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>分類分析資料</returns>
    Task<List<CategoryAnalysisData>> GetExpenseCategoryAnalysisAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// 取得統計摘要資料
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>統計摘要資料</returns>
    Task<StatisticsSummaryData> GetStatisticsSummaryAsync(DateTime startDate, DateTime endDate);
}

/// <summary>
/// 統計分析服務實作
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<StatisticsService> _logger;
    
    /// <summary>
    /// 預設圖表顏色清單
    /// </summary>
    private static readonly string[] DefaultColors = {
        "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
        "#FF9F40", "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4",
        "#FFEAA7", "#DDA0DD", "#98D8C8", "#F7DC6F", "#BB8FCE"
    };
    
    public StatisticsService(IAccountingService accountingService, ILogger<StatisticsService> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }
    
    /// <summary>
    /// 取得月度收支趨勢資料
    /// </summary>
    public async Task<List<MonthlyTrendData>> GetMonthlyTrendAsync(int months = 6)
    {
        try
        {
            var result = new List<MonthlyTrendData>();
            var currentDate = DateTime.Now;
            
            for (int i = months - 1; i >= 0; i--)
            {
                var targetDate = currentDate.AddMonths(-i);
                var year = targetDate.Year;
                var month = targetDate.Month;
                
                // 取得該月份的記帳記錄
                var startDate = new DateTime(year, month, 1);
                var endDate = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                var records = await _accountingService.GetRecordsAsync(startDate, endDate);
                
                // 計算收支統計
                var income = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
                var expense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
                
                result.Add(new MonthlyTrendData
                {
                    Month = $"{year:0000}-{month:00}",
                    MonthName = $"{year}年{month}月",
                    Income = income,
                    Expense = expense,
                    NetIncome = income - expense,
                    TotalRecords = records.Count
                });
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得月度收支趨勢時發生錯誤");
            return new List<MonthlyTrendData>();
        }
    }
    
    /// <summary>
    /// 取得支出分類分析資料
    /// </summary>
    public async Task<List<CategoryAnalysisData>> GetExpenseCategoryAnalysisAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var expenseRecords = records.Where(r => r.Type == "Expense").ToList();
            
            if (!expenseRecords.Any())
            {
                return new List<CategoryAnalysisData>();
            }
            
            var totalExpense = expenseRecords.Sum(r => r.Amount);
            
            // 按分類分組並計算統計
            var categoryGroups = expenseRecords
                .GroupBy(r => r.Category ?? "未分類")
                .Select((g, index) => new CategoryAnalysisData
                {
                    Category = g.Key,
                    Amount = g.Sum(r => r.Amount),
                    RecordCount = g.Count(),
                    Percentage = totalExpense > 0 ? (g.Sum(r => r.Amount) / totalExpense) * 100 : 0,
                    Color = DefaultColors[index % DefaultColors.Length]
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
                
            return categoryGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得支出分類分析時發生錯誤");
            return new List<CategoryAnalysisData>();
        }
    }
    
    /// <summary>
    /// 取得統計摘要資料
    /// </summary>
    public async Task<StatisticsSummaryData> GetStatisticsSummaryAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            
            var totalIncome = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
            var totalExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
            var monthsDiff = Math.Max(1, (int)Math.Ceiling((endDate - startDate).TotalDays / 30.0));
            
            // 找出最大支出分類
            var topExpenseCategory = records
                .Where(r => r.Type == "Expense")
                .GroupBy(r => r.Category ?? "未分類")
                .OrderByDescending(g => g.Sum(r => r.Amount))
                .FirstOrDefault();
            
            return new StatisticsSummaryData
            {
                TotalIncome = totalIncome,
                TotalExpense = totalExpense,
                NetIncome = totalIncome - totalExpense,
                TotalRecords = records.Count,
                AverageMonthlyIncome = totalIncome / monthsDiff,
                AverageMonthlyExpense = totalExpense / monthsDiff,
                TopExpenseCategory = topExpenseCategory?.Key ?? "無",
                TopExpenseAmount = topExpenseCategory?.Sum(r => r.Amount) ?? 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得統計摘要時發生錯誤");
            return new StatisticsSummaryData();
        }
    }
}
