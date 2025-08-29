using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 預算管理服務，提供預算建立、追蹤和分析功能
/// </summary>
public class BudgetManagementService
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<BudgetManagementService> _logger;
    private readonly string _budgetFilePath;

    public BudgetManagementService(
        IAccountingService accountingService,
        ILogger<BudgetManagementService> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
        _budgetFilePath = Path.Combine("App_Data", "budget-settings.json");
    }

    /// <summary>
    /// 取得指定月份的預算列表
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <returns>預算項目列表</returns>
    public async Task<List<BudgetItem>> GetBudgetsAsync(int year, int month)
    {
        try
        {
            var budgets = await LoadBudgetsFromFileAsync();
            return budgets
                .Where(b => b.Year == year && b.Month == month && b.IsActive)
                .OrderBy(b => b.Category)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得預算列表時發生錯誤");
            return new List<BudgetItem>();
        }
    }

    /// <summary>
    /// 建立新預算
    /// </summary>
    /// <param name="request">建立預算請求</param>
    /// <returns>建立的預算項目</returns>
    public async Task<BudgetItem> CreateBudgetAsync(CreateBudgetRequest request)
    {
        try
        {
            var budgets = await LoadBudgetsFromFileAsync();
            var newBudget = new BudgetItem
            {
                Id = budgets.Any() ? budgets.Max(b => b.Id) + 1 : 1,
                Category = request.Category,
                SubCategory = request.SubCategory,
                BudgetAmount = request.BudgetAmount,
                Year = request.Year,
                Month = request.Month,
                CreatedDate = DateTime.Now,
                IsActive = true,
                Notes = request.Notes
            };

            budgets.Add(newBudget);
            await SaveBudgetsToFileAsync(budgets);

            _logger.LogInformation("建立新預算: {Category} - NT${Amount:N0}", 
                newBudget.Category, newBudget.BudgetAmount);
            
            return newBudget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立預算時發生錯誤");
            throw;
        }
    }

    /// <summary>
    /// 更新預算
    /// </summary>
    /// <param name="budgetId">預算ID</param>
    /// <param name="request">更新預算請求</param>
    /// <returns>更新的預算項目</returns>
    public async Task<BudgetItem> UpdateBudgetAsync(int budgetId, UpdateBudgetRequest request)
    {
        try
        {
            var budgets = await LoadBudgetsFromFileAsync();
            var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
            
            if (budget is null)
                throw new ArgumentException($"找不到ID為 {budgetId} 的預算");

            budget.Category = request.Category;
            budget.SubCategory = request.SubCategory;
            budget.BudgetAmount = request.BudgetAmount;
            budget.Notes = request.Notes;
            budget.ModifiedDate = DateTime.Now;

            await SaveBudgetsToFileAsync(budgets);

            _logger.LogInformation("更新預算: {Category} - NT${Amount:N0}", 
                budget.Category, budget.BudgetAmount);

            return budget;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新預算時發生錯誤");
            throw;
        }
    }

    /// <summary>
    /// 刪除預算
    /// </summary>
    /// <param name="budgetId">預算ID</param>
    /// <returns>刪除是否成功</returns>
    public async Task<bool> DeleteBudgetAsync(int budgetId)
    {
        try
        {
            var budgets = await LoadBudgetsFromFileAsync();
            var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
            
            if (budget is null)
                return false;

            budget.IsActive = false;
            budget.ModifiedDate = DateTime.Now;

            await SaveBudgetsToFileAsync(budgets);

            _logger.LogInformation("刪除預算: {Category}", budget.Category);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除預算時發生錯誤");
            return false;
        }
    }

    /// <summary>
    /// 取得預算執行表現
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <returns>預算表現列表</returns>
    public async Task<List<BudgetPerformance>> GetBudgetPerformanceAsync(int year, int month)
    {
        try
        {
            var budgets = await GetBudgetsAsync(year, month);
            var performances = new List<BudgetPerformance>();

            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);

            var daysInMonth = DateTime.DaysInMonth(year, month);
            var currentDay = DateTime.Now.Day;
            var daysRemaining = Math.Max(0, daysInMonth - currentDay);

            foreach (var budget in budgets)
            {
                var actualAmount = Math.Abs(records
                    .Where(r => r.Category == budget.Category && r.Amount < 0)
                    .Sum(r => r.Amount));

                var remainingAmount = Math.Max(0, budget.BudgetAmount - actualAmount);
                var usedPercentage = budget.BudgetAmount > 0 
                    ? (actualAmount / budget.BudgetAmount) * 100 
                    : 0;

                var status = usedPercentage switch
                {
                    <= 75 => "on_track",
                    <= 100 => "warning",
                    _ => "exceeded"
                };

                var dailyAverageSpending = currentDay > 0 ? actualAmount / currentDay : 0;
                var recommendedDailySpending = daysRemaining > 0 
                    ? remainingAmount / daysRemaining 
                    : 0;

                performances.Add(new BudgetPerformance
                {
                    Budget = budget,
                    ActualAmount = actualAmount,
                    RemainingAmount = remainingAmount,
                    UsedPercentage = usedPercentage,
                    DaysRemaining = daysRemaining,
                    Status = status,
                    DailyAverageSpending = dailyAverageSpending,
                    RecommendedDailySpending = recommendedDailySpending
                });
            }

            return performances;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得預算表現時發生錯誤");
            return new List<BudgetPerformance>();
        }
    }

    /// <summary>
    /// 取得預算摘要
    /// </summary>
    /// <param name="year">年份</param>
    /// <param name="month">月份</param>
    /// <returns>預算摘要</returns>
    public async Task<BudgetSummary> GetBudgetSummaryAsync(int year, int month)
    {
        try
        {
            var performances = await GetBudgetPerformanceAsync(year, month);
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var daysRemaining = Math.Max(0, daysInMonth - DateTime.Now.Day);

            return new BudgetSummary
            {
                TotalBudgetAmount = performances.Sum(p => p.Budget.BudgetAmount),
                TotalActualAmount = performances.Sum(p => p.ActualAmount),
                TotalRemainingAmount = performances.Sum(p => p.RemainingAmount),
                OverallUsedPercentage = performances.Any() 
                    ? performances.Average(p => p.UsedPercentage) 
                    : 0,
                CategoriesOnTrack = performances.Count(p => p.Status == "on_track"),
                CategoriesWarning = performances.Count(p => p.Status == "warning"),
                CategoriesExceeded = performances.Count(p => p.Status == "exceeded"),
                DaysRemainingInMonth = daysRemaining
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得預算摘要時發生錯誤");
            return new BudgetSummary();
        }
    }

    /// <summary>
    /// 取得預算警報
    /// </summary>
    /// <returns>預算警報列表</returns>
    public async Task<List<BudgetAlert>> GetBudgetAlertsAsync()
    {
        try
        {
            var alerts = new List<BudgetAlert>();
            var now = DateTime.Now;
            var performances = await GetBudgetPerformanceAsync(now.Year, now.Month);

            foreach (var performance in performances)
            {
                var alertType = performance.UsedPercentage switch
                {
                    >= 100 => "exceeded",
                    >= 80 => "approaching_limit",
                    _ when performance.ActualAmount == 0 => "no_activity",
                    _ => null
                };

                if (alertType is not null)
                {
                    var message = alertType switch
                    {
                        "exceeded" => $"{performance.Budget.Category} 預算已超支 {performance.UsedPercentage - 100:F0}%",
                        "approaching_limit" => $"{performance.Budget.Category} 預算已使用 {performance.UsedPercentage:F0}%",
                        "no_activity" => $"{performance.Budget.Category} 本月尚無支出記錄",
                        _ => string.Empty
                    };

                    alerts.Add(new BudgetAlert
                    {
                        Category = performance.Budget.Category,
                        AlertType = alertType,
                        BudgetAmount = performance.Budget.BudgetAmount,
                        CurrentAmount = performance.ActualAmount,
                        Percentage = performance.UsedPercentage,
                        Message = message,
                        CreatedDate = DateTime.Now
                    });
                }
            }

            return alerts;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得預算警報時發生錯誤");
            return new List<BudgetAlert>();
        }
    }

    /// <summary>
    /// 產生預算建議
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>預算建議列表</returns>
    public async Task<List<BudgetSuggestion>> GenerateBudgetSuggestionsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var suggestions = new List<BudgetSuggestion>();
            var categories = await _accountingService.GetCategoriesAsync("expense");

            // 分析過去6個月的支出模式
            var historicalStartDate = startDate.AddMonths(-6);
            var historicalRecords = await _accountingService.GetRecordsAsync(historicalStartDate, startDate);

            foreach (var category in categories)
            {
                var categoryRecords = historicalRecords
                    .Where(r => r.Category == category.Name && r.Amount < 0)
                    .ToList();

                if (!categoryRecords.Any()) continue;

                // 計算歷史平均和趨勢
                var monthlyAmounts = new List<decimal>();
                for (int i = 0; i < 6; i++)
                {
                    var monthStart = startDate.AddMonths(-(i + 1));
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var monthAmount = Math.Abs(categoryRecords
                        .Where(r => r.Date >= monthStart && r.Date <= monthEnd)
                        .Sum(r => r.Amount));
                    monthlyAmounts.Add(monthAmount);
                }

                var historicalAverage = monthlyAmounts.Average();
                var recentAverage = monthlyAmounts.Take(3).Average(); // 最近3個月
                var trend = recentAverage - historicalAverage;

                // 考慮趨勢調整建議金額
                var baseAmount = historicalAverage;
                var adjustmentFactor = trend > 0 ? 1.1m : 0.95m; // 上升趨勢增加10%，下降趨勢減少5%
                var suggestedAmount = Math.Round(baseAmount * adjustmentFactor, 0);

                // 計算信心分數
                var variance = monthlyAmounts.Select(a => Math.Pow((double)(a - historicalAverage), 2)).Average();
                var stdDev = (decimal)Math.Sqrt(variance);
                var coefficientOfVariation = historicalAverage > 0 ? stdDev / historicalAverage : 0;
                var confidenceScore = Math.Max(0, 1 - (decimal)coefficientOfVariation);

                var reasoning = trend switch
                {
                    > 0 => $"基於過去6個月平均 NT${historicalAverage:N0}，考慮到上升趨勢建議增加預算",
                    < 0 => $"基於過去6個月平均 NT${historicalAverage:N0}，考慮到下降趨勢可適度減少預算",
                    _ => $"基於過去6個月穩定的支出模式，建議預算為 NT${historicalAverage:N0}"
                };

                var considerationFactors = new List<string>();
                if (coefficientOfVariation > 0.3m)
                    considerationFactors.Add("支出變動較大，建議預留彈性");
                if (monthlyAmounts.Any(a => a == 0))
                    considerationFactors.Add("部分月份無支出，可能為非固定開銷");
                if (trend > historicalAverage * 0.2m)
                    considerationFactors.Add("支出呈現明顯上升趨勢");

                suggestions.Add(new BudgetSuggestion
                {
                    Category = category.Name,
                    SuggestedAmount = suggestedAmount,
                    HistoricalAverage = historicalAverage,
                    Reasoning = reasoning,
                    ConfidenceScore = confidenceScore,
                    ConsiderationFactors = considerationFactors
                });
            }

            return suggestions
                .Where(s => s.SuggestedAmount > 0)
                .OrderByDescending(s => s.SuggestedAmount)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "產生預算建議時發生錯誤");
            return new List<BudgetSuggestion>();
        }
    }

    /// <summary>
    /// 最佳化預算計劃
    /// </summary>
    /// <param name="totalBudget">總預算</param>
    /// <param name="priorityCategories">優先分類</param>
    /// <returns>最佳預算計劃</returns>
    public async Task<OptimalBudgetPlan> OptimizeBudgetPlanAsync(decimal totalBudget, List<string> priorityCategories)
    {
        try
        {
            var suggestions = await GenerateBudgetSuggestionsAsync(DateTime.Now.AddMonths(-1), DateTime.Now);
            var allocations = new List<BudgetAllocation>();
            var strategies = new List<string>();

            // 基本分配邏輯：50% 必需品，30% 想要的，20% 儲蓄
            var essentialCategories = new[] { "餐飲", "交通", "生活用品", "醫療" };
            var wantCategories = new[] { "娛樂", "購物", "旅遊" };

            var essentialBudget = totalBudget * 0.5m;
            var wantBudget = totalBudget * 0.3m;
            var savingsBudget = totalBudget * 0.2m;

            // 分配必需品預算
            var essentialSuggestions = suggestions
                .Where(s => essentialCategories.Contains(s.Category))
                .ToList();

            var essentialTotal = essentialSuggestions.Sum(s => s.SuggestedAmount);
            foreach (var suggestion in essentialSuggestions)
            {
                var percentage = essentialTotal > 0 
                    ? (suggestion.SuggestedAmount / essentialTotal) * 50 
                    : 0;
                allocations.Add(new BudgetAllocation
                {
                    Category = suggestion.Category,
                    Amount = essentialTotal > 0 
                        ? Math.Round(essentialBudget * (suggestion.SuggestedAmount / essentialTotal), 0)
                        : 0,
                    Percentage = percentage,
                    Priority = "high",
                    Justification = "生活必需品，優先分配"
                });
            }

            // 分配想要的預算
            var wantSuggestions = suggestions
                .Where(s => wantCategories.Contains(s.Category))
                .ToList();

            var wantTotal = wantSuggestions.Sum(s => s.SuggestedAmount);
            foreach (var suggestion in wantSuggestions)
            {
                var percentage = wantTotal > 0 
                    ? (suggestion.SuggestedAmount / wantTotal) * 30 
                    : 0;
                allocations.Add(new BudgetAllocation
                {
                    Category = suggestion.Category,
                    Amount = wantTotal > 0 
                        ? Math.Round(wantBudget * (suggestion.SuggestedAmount / wantTotal), 0)
                        : 0,
                    Percentage = percentage,
                    Priority = "medium",
                    Justification = "提升生活品質的支出"
                });
            }

            // 儲蓄分配
            allocations.Add(new BudgetAllocation
            {
                Category = "儲蓄",
                Amount = savingsBudget,
                Percentage = 20,
                Priority = "high",
                Justification = "財務安全的重要基礎"
            });

            strategies.Add("採用50/30/20法則：50%必需品、30%想要的、20%儲蓄");
            strategies.Add("優先確保生活基本需求，再分配其他支出");
            strategies.Add("維持至少20%的儲蓄率以確保財務健康");

            return new OptimalBudgetPlan
            {
                TotalBudget = totalBudget,
                Allocations = allocations,
                OptimizationStrategies = strategies,
                ProjectedSavings = savingsBudget,
                ConfidenceLevel = "medium"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "最佳化預算計劃時發生錯誤");
            return new OptimalBudgetPlan { TotalBudget = totalBudget };
        }
    }

    #region 私有方法

    /// <summary>
    /// 從檔案載入預算資料
    /// </summary>
    private async Task<List<BudgetItem>> LoadBudgetsFromFileAsync()
    {
        try
        {
            if (!File.Exists(_budgetFilePath))
            {
                return new List<BudgetItem>();
            }

            var json = await File.ReadAllTextAsync(_budgetFilePath);
            return JsonSerializer.Deserialize<List<BudgetItem>>(json) ?? new List<BudgetItem>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入預算檔案時發生錯誤");
            return new List<BudgetItem>();
        }
    }

    /// <summary>
    /// 將預算資料儲存到檔案
    /// </summary>
    private async Task SaveBudgetsToFileAsync(List<BudgetItem> budgets)
    {
        try
        {
            var directory = Path.GetDirectoryName(_budgetFilePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            var json = JsonSerializer.Serialize(budgets, options);
            await File.WriteAllTextAsync(_budgetFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存預算檔案時發生錯誤");
            throw;
        }
    }

    #endregion
}
