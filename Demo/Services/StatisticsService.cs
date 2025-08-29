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

    /// <summary>
    /// 取得收入分類分析資料
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>分類分析資料</returns>
    Task<List<CategoryAnalysisData>> GetIncomeCategoryAnalysisAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 取得分類排行榜
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <param name="type">排行榜類型 ("income", "expense")</param>
    /// <param name="topCount">取得筆數</param>
    /// <returns>分類排行榜資料</returns>
    Task<List<CategoryRankingData>> GetCategoryRankingAsync(DateTime startDate, DateTime endDate, string type, int topCount = 10);

    /// <summary>
    /// 取得時間模式分析
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>時間模式分析資料</returns>
    Task<TimePatternAnalysisData> GetTimePatternAnalysisAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 取得比較分析資料
    /// </summary>
    /// <param name="currentStart">目前期間開始日期</param>
    /// <param name="currentEnd">目前期間結束日期</param>
    /// <param name="previousStart">前期期間開始日期</param>
    /// <param name="previousEnd">前期期間結束日期</param>
    /// <returns>比較分析資料</returns>
    Task<ComparisonAnalysisData> GetComparisonAnalysisAsync(DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd);

    // Phase 3: AI 智能分析功能

    /// <summary>
    /// 取得財務健康分數
    /// </summary>
    /// <returns>財務健康分數</returns>
    Task<FinancialHealthScore> GetFinancialHealthScoreAsync();

    /// <summary>
    /// 取得智能洞察
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>智能洞察列表</returns>
    Task<List<SmartInsight>> GetSmartInsightsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 取得個人化建議
    /// </summary>
    /// <returns>個人化建議列表</returns>
    Task<List<PersonalizedRecommendation>> GetPersonalizedRecommendationsAsync();

    /// <summary>
    /// 取得異常警報
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>異常警報列表</returns>
    Task<List<AnomalyAlert>> GetAnomalyAlertsAsync(DateTime startDate, DateTime endDate);

    /// <summary>
    /// 取得支出預測
    /// </summary>
    /// <param name="monthsAhead">預測月數</param>
    /// <returns>支出預測列表</returns>
    Task<List<ExpenseForecast>> GetExpenseForecastAsync(int monthsAhead = 6);

    /// <summary>
    /// 取得現金流預測
    /// </summary>
    /// <param name="monthsAhead">預測月數</param>
    /// <returns>現金流預測</returns>
    Task<CashFlowProjection> GetCashFlowProjectionAsync(int monthsAhead = 12);

    /// <summary>
    /// 取得預算建議
    /// </summary>
    /// <param name="totalBudget">總預算</param>
    /// <param name="priorityCategories">優先分類</param>
    /// <returns>預算建議</returns>
    Task<List<BudgetSuggestion>> GetBudgetSuggestionsAsync(decimal totalBudget, List<string> priorityCategories);

    /// <summary>
    /// 取得節省機會
    /// </summary>
    /// <returns>節省機會列表</returns>
    Task<List<SavingsOpportunity>> GetSavingsOpportunitiesAsync();
}

/// <summary>
/// 統計分析服務實作
/// </summary>
public class StatisticsService : IStatisticsService
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<StatisticsService> _logger;
    private readonly AnomalyDetectionService _anomalyDetectionService;
    private readonly BudgetManagementService _budgetManagementService;
    private readonly FinancialInsightsService _financialInsightsService;
    private readonly PredictiveAnalysisService _predictiveAnalysisService;
    
    /// <summary>
    /// 預設圖表顏色清單
    /// </summary>
    private static readonly string[] DefaultColors = {
        "#FF6384", "#36A2EB", "#FFCE56", "#4BC0C0", "#9966FF",
        "#FF9F40", "#FF6B6B", "#4ECDC4", "#45B7D1", "#96CEB4",
        "#FFEAA7", "#DDA0DD", "#98D8C8", "#F7DC6F", "#BB8FCE"
    };
    
    public StatisticsService(
        IAccountingService accountingService, 
        ILogger<StatisticsService> logger,
        AnomalyDetectionService anomalyDetectionService,
        BudgetManagementService budgetManagementService,
        FinancialInsightsService financialInsightsService,
        PredictiveAnalysisService predictiveAnalysisService)
    {
        _accountingService = accountingService;
        _logger = logger;
        _anomalyDetectionService = anomalyDetectionService;
        _budgetManagementService = budgetManagementService;
        _financialInsightsService = financialInsightsService;
        _predictiveAnalysisService = predictiveAnalysisService;
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
                .GroupBy(r => new { Category = r.Category ?? "未分類", SubCategory = r.SubCategory ?? "其他" })
                .Select((g, index) => new CategoryAnalysisData
                {
                    Category = g.Key.Category,
                    SubCategory = g.Key.SubCategory,
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

    /// <summary>
    /// 取得收入分類分析資料
    /// </summary>
    public async Task<List<CategoryAnalysisData>> GetIncomeCategoryAnalysisAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var incomeRecords = records.Where(r => r.Type == "Income").ToList();
            
            if (incomeRecords.Count == 0)
            {
                return new List<CategoryAnalysisData>();
            }

            var totalIncome = incomeRecords.Sum(r => r.Amount);
            
            var categoryGroups = incomeRecords
                .GroupBy(r => new { Category = r.Category ?? "未分類", SubCategory = r.SubCategory ?? "其他" })
                .Select(g => new CategoryAnalysisData
                {
                    Category = g.Key.Category,
                    SubCategory = g.Key.SubCategory,
                    Amount = g.Sum(r => r.Amount),
                    Percentage = totalIncome > 0 ? (g.Sum(r => r.Amount) / totalIncome) * 100 : 0,
                    RecordCount = g.Count()
                })
                .OrderByDescending(c => c.Amount)
                .ToList();
            
            return categoryGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得收入分類分析時發生錯誤");
            return new List<CategoryAnalysisData>();
        }
    }

    /// <summary>
    /// 取得分類排行榜
    /// </summary>
    public async Task<List<CategoryRankingData>> GetCategoryRankingAsync(DateTime startDate, DateTime endDate, string type, int topCount = 10)
    {
        try
        {
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var filteredRecords = records.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (filteredRecords.Count == 0)
            {
                return new List<CategoryRankingData>();
            }

            // 計算前期比較資料（前一個相同時間段）
            var periodDays = (endDate - startDate).Days;
            var previousStart = startDate.AddDays(-periodDays);
            var previousEnd = startDate;
            var previousRecords = await _accountingService.GetRecordsAsync(previousStart, previousEnd);
            var previousFiltered = previousRecords.Where(r => r.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

            var categoryGroups = filteredRecords
                .GroupBy(r => new { Category = r.Category ?? "未分類", SubCategory = r.SubCategory ?? "其他" })
                .Select((g, index) => 
                {
                    var categoryKey = g.Key.Category;
                    var subCategoryKey = g.Key.SubCategory;
                    var currentAmount = g.Sum(r => r.Amount);
                    var recordCount = g.Count();
                    var averageAmount = recordCount > 0 ? currentAmount / recordCount : 0;
                    
                    // 查找前期相同分類的資料
                    var previousCategoryData = previousFiltered
                        .Where(r => (r.Category ?? "未分類") == categoryKey && (r.SubCategory ?? "其他") == subCategoryKey)
                        .ToList();
                    var previousAmount = previousCategoryData.Sum(r => r.Amount);
                    
                    var percentageChange = previousAmount > 0 ? 
                        ((currentAmount - previousAmount) / previousAmount) * 100 : 
                        (currentAmount > 0 ? 100 : 0);
                    
                    var trend = percentageChange > 5 ? "up" : 
                               percentageChange < -5 ? "down" : "stable";
                    
                    return new CategoryRankingData
                    {
                        Rank = index + 1,
                        Category = categoryKey,
                        SubCategory = subCategoryKey,
                        Amount = currentAmount,
                        RecordCount = recordCount,
                        AverageAmount = averageAmount,
                        PercentageChange = percentageChange,
                        Trend = trend
                    };
                })
                .OrderByDescending(c => c.Amount)
                .Take(topCount)
                .Select((c, index) => new CategoryRankingData
                {
                    Rank = index + 1,
                    Category = c.Category,
                    SubCategory = c.SubCategory,
                    Amount = c.Amount,
                    RecordCount = c.RecordCount,
                    AverageAmount = c.AverageAmount,
                    PercentageChange = c.PercentageChange,
                    Trend = c.Trend
                })
                .ToList();
            
            return categoryGroups;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分類排行榜時發生錯誤");
            return new List<CategoryRankingData>();
        }
    }

    /// <summary>
    /// 取得時間模式分析
    /// </summary>
    public async Task<TimePatternAnalysisData> GetTimePatternAnalysisAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            
            if (records.Count == 0)
            {
                return new TimePatternAnalysisData();
            }

            var result = new TimePatternAnalysisData();
            
            // 週日模式分析
            var weekdayGroups = records
                .GroupBy(r => r.Date.DayOfWeek)
                .ToList();
            
            var weekdayNames = new[] { "週日", "週一", "週二", "週三", "週四", "週五", "週六" };
            
            result.WeekdayPatterns = weekdayGroups
                .Select(g => 
                {
                    var weekdayRecords = g.ToList();
                    var incomeRecords = weekdayRecords.Where(r => r.Type == "Income").ToList();
                    var expenseRecords = weekdayRecords.Where(r => r.Type == "Expense").ToList();
                    
                    var popularCategories = weekdayRecords
                        .GroupBy(r => r.Category ?? "未分類")
                        .OrderByDescending(cg => cg.Count())
                        .Take(3)
                        .Select(cg => cg.Key)
                        .ToList();
                    
                    return new WeekdayPatternData
                    {
                        Weekday = weekdayNames[(int)g.Key],
                        WeekdayIndex = (int)g.Key,
                        AverageIncome = incomeRecords.Count > 0 ? incomeRecords.Average(r => r.Amount) : 0,
                        AverageExpense = expenseRecords.Count > 0 ? expenseRecords.Average(r => r.Amount) : 0,
                        RecordCount = weekdayRecords.Count,
                        PopularCategories = popularCategories
                    };
                })
                .OrderBy(w => w.WeekdayIndex)
                .ToList();
            
            // 月內模式分析
            result.MonthlyPatterns = new List<MonthlyPatternData>
            {
                CreateMonthlyPattern(records, "月初 (1-10日)", 1, 10),
                CreateMonthlyPattern(records, "月中 (11-20日)", 11, 20),
                CreateMonthlyPattern(records, "月末 (21-31日)", 21, 31)
            };
            
            // 日常模式摘要
            var mostActiveWeekday = result.WeekdayPatterns
                .OrderByDescending(w => w.RecordCount)
                .FirstOrDefault();
            
            var highestExpenseWeekday = result.WeekdayPatterns
                .OrderByDescending(w => w.AverageExpense)
                .FirstOrDefault();
            
            var mostActivePeriod = result.MonthlyPatterns
                .OrderByDescending(p => p.RecordCount)
                .FirstOrDefault();
            
            var weekdayExpense = result.WeekdayPatterns
                .Where(w => w.WeekdayIndex >= 1 && w.WeekdayIndex <= 5)
                .Average(w => w.AverageExpense);
            
            var weekendExpense = result.WeekdayPatterns
                .Where(w => w.WeekdayIndex == 0 || w.WeekdayIndex == 6)
                .Average(w => w.AverageExpense);
            
            result.DailySummary = new DailyPatternSummary
            {
                MostActiveWeekday = mostActiveWeekday?.Weekday ?? "無資料",
                HighestExpenseWeekday = highestExpenseWeekday?.Weekday ?? "無資料",
                MostActivePeriod = mostActivePeriod?.Period ?? "無資料",
                WeekdayAverageExpense = weekdayExpense,
                WeekendAverageExpense = weekendExpense
            };
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得時間模式分析時發生錯誤");
            return new TimePatternAnalysisData();
        }
    }

    /// <summary>
    /// 取得比較分析資料
    /// </summary>
    public async Task<ComparisonAnalysisData> GetComparisonAnalysisAsync(DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd)
    {
        try
        {
            var currentSummary = await GetStatisticsSummaryAsync(currentStart, currentEnd);
            var previousSummary = await GetStatisticsSummaryAsync(previousStart, previousEnd);
            
            var incomeGrowthRate = previousSummary.TotalIncome > 0 ? 
                ((currentSummary.TotalIncome - previousSummary.TotalIncome) / previousSummary.TotalIncome) * 100 : 
                (currentSummary.TotalIncome > 0 ? 100 : 0);
            
            var expenseGrowthRate = previousSummary.TotalExpense > 0 ? 
                ((currentSummary.TotalExpense - previousSummary.TotalExpense) / previousSummary.TotalExpense) * 100 : 
                (currentSummary.TotalExpense > 0 ? 100 : 0);
            
            var netIncomeGrowthRate = previousSummary.NetIncome != 0 ? 
                ((currentSummary.NetIncome - previousSummary.NetIncome) / Math.Abs(previousSummary.NetIncome)) * 100 : 
                (currentSummary.NetIncome != 0 ? (currentSummary.NetIncome > 0 ? 100 : -100) : 0);
            
            // 分類變化分析
            var currentRecords = await _accountingService.GetRecordsAsync(currentStart, currentEnd);
            var previousRecords = await _accountingService.GetRecordsAsync(previousStart, previousEnd);
            
            var currentCategories = currentRecords
                .GroupBy(r => r.Category ?? "未分類")
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
            
            var previousCategories = previousRecords
                .GroupBy(r => r.Category ?? "未分類")
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
            
            var allCategories = currentCategories.Keys.Union(previousCategories.Keys).ToList();
            
            var categoryChanges = allCategories
                .Select(category =>
                {
                    var currentAmount = currentCategories.GetValueOrDefault(category, 0);
                    var previousAmount = previousCategories.GetValueOrDefault(category, 0);
                    var changeAmount = currentAmount - previousAmount;
                    var changePercentage = previousAmount > 0 ? 
                        (changeAmount / previousAmount) * 100 : 
                        (currentAmount > 0 ? 100 : 0);
                    
                    var changeDirection = Math.Abs(changePercentage) < 5 ? "stable" :
                                        changePercentage > 0 ? "increase" : "decrease";
                    
                    return new CategoryComparisonData
                    {
                        Category = category,
                        CurrentAmount = currentAmount,
                        PreviousAmount = previousAmount,
                        ChangeAmount = changeAmount,
                        ChangePercentage = changePercentage,
                        ChangeDirection = changeDirection
                    };
                })
                .Where(c => c.CurrentAmount > 0 || c.PreviousAmount > 0)
                .OrderByDescending(c => Math.Abs(c.ChangeAmount))
                .Take(10)
                .ToList();
            
            var periodName = $"{currentStart:yyyy/MM/dd} - {currentEnd:yyyy/MM/dd} vs {previousStart:yyyy/MM/dd} - {previousEnd:yyyy/MM/dd}";
            
            return new ComparisonAnalysisData
            {
                PeriodName = periodName,
                CurrentPeriod = currentSummary,
                PreviousPeriod = previousSummary,
                IncomeGrowthRate = incomeGrowthRate,
                ExpenseGrowthRate = expenseGrowthRate,
                NetIncomeGrowthRate = netIncomeGrowthRate,
                CategoryChanges = categoryChanges
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得比較分析時發生錯誤");
            return new ComparisonAnalysisData();
        }
    }

    /// <summary>
    /// 建立月內模式資料的輔助方法
    /// </summary>
    private MonthlyPatternData CreateMonthlyPattern(List<AccountingRecord> records, string period, int startDay, int endDay)
    {
        var periodRecords = records
            .Where(r => r.Date.Day >= startDay && r.Date.Day <= endDay)
            .ToList();
        
        if (periodRecords.Count == 0)
        {
            return new MonthlyPatternData
            {
                Period = period,
                StartDay = startDay,
                EndDay = endDay,
                AverageIncome = 0,
                AverageExpense = 0,
                RecordCount = 0,
                Percentage = 0
            };
        }
        
        var incomeRecords = periodRecords.Where(r => r.Type == "Income").ToList();
        var expenseRecords = periodRecords.Where(r => r.Type == "Expense").ToList();
        var totalRecords = records.Count;
        
        return new MonthlyPatternData
        {
            Period = period,
            StartDay = startDay,
            EndDay = endDay,
            AverageIncome = incomeRecords.Count > 0 ? incomeRecords.Average(r => r.Amount) : 0,
            AverageExpense = expenseRecords.Count > 0 ? expenseRecords.Average(r => r.Amount) : 0,
            RecordCount = periodRecords.Count,
            Percentage = totalRecords > 0 ? (decimal)periodRecords.Count / totalRecords * 100 : 0
        };
    }

    #region Phase 3: AI 智能分析功能

    /// <summary>
    /// 取得財務健康分數
    /// </summary>
    public async Task<FinancialHealthScore> GetFinancialHealthScoreAsync()
    {
        try
        {
            return await _financialInsightsService.CalculateFinancialHealthAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得財務健康分數時發生錯誤");
            return new FinancialHealthScore { OverallScore = 50, HealthLevel = "fair" };
        }
    }

    /// <summary>
    /// 取得智能洞察
    /// </summary>
    public async Task<List<SmartInsight>> GetSmartInsightsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _financialInsightsService.GenerateSmartInsightsAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得智能洞察時發生錯誤");
            return new List<SmartInsight>();
        }
    }

    /// <summary>
    /// 取得個人化建議
    /// </summary>
    public async Task<List<PersonalizedRecommendation>> GetPersonalizedRecommendationsAsync()
    {
        try
        {
            return await _financialInsightsService.GetPersonalizedRecommendationsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得個人化建議時發生錯誤");
            return new List<PersonalizedRecommendation>();
        }
    }

    /// <summary>
    /// 取得異常警報
    /// </summary>
    public async Task<List<AnomalyAlert>> GetAnomalyAlertsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _anomalyDetectionService.DetectSpendingAnomaliesAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得異常警報時發生錯誤");
            return new List<AnomalyAlert>();
        }
    }

    /// <summary>
    /// 取得支出預測
    /// </summary>
    public async Task<List<ExpenseForecast>> GetExpenseForecastAsync(int monthsAhead = 6)
    {
        try
        {
            return await _predictiveAnalysisService.ForecastExpensesAsync(monthsAhead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得支出預測時發生錯誤");
            return new List<ExpenseForecast>();
        }
    }

    /// <summary>
    /// 取得現金流預測
    /// </summary>
    public async Task<CashFlowProjection> GetCashFlowProjectionAsync(int monthsAhead = 12)
    {
        try
        {
            return await _predictiveAnalysisService.ProjectCashFlowAsync(monthsAhead);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得現金流預測時發生錯誤");
            return new CashFlowProjection
            {
                PeriodStart = DateTime.Now,
                PeriodEnd = DateTime.Now.AddMonths(monthsAhead),
                DataPoints = new List<CashFlowDataPoint>(),
                Warnings = new List<string> { "無法取得現金流預測" }
            };
        }
    }

    /// <summary>
    /// 取得預算建議
    /// </summary>
    public async Task<List<BudgetSuggestion>> GetBudgetSuggestionsAsync(decimal totalBudget, List<string> priorityCategories)
    {
        try
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-1);
            return await _budgetManagementService.GenerateBudgetSuggestionsAsync(startDate, endDate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得預算建議時發生錯誤");
            return new List<BudgetSuggestion>();
        }
    }

    /// <summary>
    /// 取得節省機會
    /// </summary>
    public async Task<List<SavingsOpportunity>> GetSavingsOpportunitiesAsync()
    {
        try
        {
            return await _financialInsightsService.IdentifySavingsOpportunitiesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得節省機會時發生錯誤");
            return new List<SavingsOpportunity>();
        }
    }

    #endregion
}
