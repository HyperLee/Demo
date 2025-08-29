using Demo.Models;

namespace Demo.Services;

/// <summary>
/// 財務洞察服務，提供智能分析和個人化建議
/// </summary>
public class FinancialInsightsService
{
    private readonly IAccountingService _accountingService;
    private readonly BudgetManagementService _budgetService;
    private readonly ILogger<FinancialInsightsService> _logger;

    public FinancialInsightsService(
        IAccountingService accountingService,
        BudgetManagementService budgetService,
        ILogger<FinancialInsightsService> logger)
    {
        _accountingService = accountingService;
        _budgetService = budgetService;
        _logger = logger;
    }

    /// <summary>
    /// 產生智能洞察
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>智能洞察列表</returns>
    public async Task<List<SmartInsight>> GenerateSmartInsightsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("產生智能洞察: {StartDate} - {EndDate}", startDate, endDate);
            
            var insights = new List<SmartInsight>();
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);

            // 1. 支出模式洞察
            var spendingPatternInsights = GenerateSpendingPatternInsights(records, startDate, endDate);
            insights.AddRange(spendingPatternInsights);

            // 2. 節省機會洞察
            var savingsInsights = await GenerateSavingsInsightsAsync(records, startDate, endDate);
            insights.AddRange(savingsInsights);

            // 3. 趨勢分析洞察
            var trendInsights = await GenerateTrendInsightsAsync(records, startDate, endDate);
            insights.AddRange(trendInsights);

            // 4. 比較分析洞察
            var comparisonInsights = await GenerateComparisonInsightsAsync(records, startDate, endDate);
            insights.AddRange(comparisonInsights);

            return insights
                .OrderByDescending(i => i.Priority)
                .ThenByDescending(i => i.Impact)
                .Take(10) // 只顯示前10個最重要的洞察
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "產生智能洞察時發生錯誤");
            return new List<SmartInsight>();
        }
    }

    /// <summary>
    /// 取得個人化建議
    /// </summary>
    /// <returns>個人化建議列表</returns>
    public async Task<List<PersonalizedRecommendation>> GetPersonalizedRecommendationsAsync()
    {
        try
        {
            var recommendations = new List<PersonalizedRecommendation>();
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // 分析目前支出狀況
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            foreach (var category in categories)
            {
                var recommendation = await GenerateCategoryRecommendationAsync(category.Name, records, startDate, endDate);
                if (recommendation is not null)
                {
                    recommendations.Add(recommendation);
                }
            }

            // 整體財務建議
            var overallRecommendation = GenerateOverallRecommendation(records, startDate, endDate);
            if (overallRecommendation is not null)
            {
                recommendations.Add(overallRecommendation);
            }

            return recommendations
                .OrderByDescending(r => r.PotentialSavings)
                .ThenBy(r => r.DifficultyLevel switch
                {
                    "easy" => 1,
                    "medium" => 2,
                    "hard" => 3,
                    _ => 4
                })
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得個人化建議時發生錯誤");
            return new List<PersonalizedRecommendation>();
        }
    }

    /// <summary>
    /// 計算財務健康分數
    /// </summary>
    /// <returns>財務健康分數</returns>
    public async Task<FinancialHealthScore> CalculateFinancialHealthAsync()
    {
        try
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);

            var metrics = new List<HealthMetric>();
            var totalScore = 0;

            // 1. 收支平衡指標
            var incomeExpenseMetric = CalculateIncomeExpenseMetric(records);
            metrics.Add(incomeExpenseMetric);
            totalScore += incomeExpenseMetric.Score;

            // 2. 儲蓄率指標
            var savingsMetric = CalculateSavingsMetric(records);
            metrics.Add(savingsMetric);
            totalScore += savingsMetric.Score;

            // 3. 支出多樣性指標
            var diversityMetric = CalculateDiversityMetric(records);
            metrics.Add(diversityMetric);
            totalScore += diversityMetric.Score;

            // 4. 預算遵循指標
            var budgetComplianceMetric = await CalculateBudgetComplianceMetricAsync(startDate, endDate);
            metrics.Add(budgetComplianceMetric);
            totalScore += budgetComplianceMetric.Score;

            // 5. 支出穩定性指標
            var stabilityMetric = CalculateStabilityMetric(records);
            metrics.Add(stabilityMetric);
            totalScore += stabilityMetric.Score;

            var overallScore = totalScore / metrics.Count;
            var healthLevel = overallScore switch
            {
                >= 85 => "excellent",
                >= 70 => "good",
                >= 55 => "fair",
                _ => "poor"
            };

            var strengthAreas = metrics
                .Where(m => m.IsGood)
                .Select(m => m.Name)
                .ToList();

            var improvementAreas = metrics
                .Where(m => !m.IsGood)
                .Select(m => m.Name)
                .ToList();

            return new FinancialHealthScore
            {
                OverallScore = overallScore,
                HealthLevel = healthLevel,
                Metrics = metrics,
                StrengthAreas = strengthAreas,
                ImprovementAreas = improvementAreas,
                CalculatedDate = DateTime.Now,
                
                // 設置前端 UI 顯示用的個別分數
                SavingsScore = savingsMetric.Score,
                BalanceScore = incomeExpenseMetric.Score,
                GrowthScore = stabilityMetric.Score
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "計算財務健康分數時發生錯誤");
            return new FinancialHealthScore { OverallScore = 50, HealthLevel = "fair" };
        }
    }

    /// <summary>
    /// 識別節省機會
    /// </summary>
    /// <returns>節省機會列表</returns>
    public async Task<List<SavingsOpportunity>> IdentifySavingsOpportunitiesAsync()
    {
        try
        {
            var opportunities = new List<SavingsOpportunity>();
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            foreach (var category in categories)
            {
                var opportunity = AnalyzeCategorySavingsOpportunity(category.Name, records, startDate, endDate);
                if (opportunity is not null)
                {
                    opportunities.Add(opportunity);
                }
            }

            return opportunities
                .Where(o => o.PotentialMonthlySavings > 0)
                .OrderByDescending(o => o.PotentialMonthlySavings)
                .Take(5)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "識別節省機會時發生錯誤");
            return new List<SavingsOpportunity>();
        }
    }

    /// <summary>
    /// 分析支出效率
    /// </summary>
    /// <returns>支出效率分析</returns>
    public async Task<SpendingEfficiencyAnalysis> AnalyzeSpendingEfficiencyAsync()
    {
        try
        {
            var now = DateTime.Now;
            var startDate = new DateTime(now.Year, now.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            var categoryEfficiencies = new List<CategoryEfficiency>();
            var totalCurrent = 0m;
            var totalOptimal = 0m;

            foreach (var category in categories)
            {
                var efficiency = CalculateCategoryEfficiency(category.Name, records);
                if (efficiency is not null)
                {
                    categoryEfficiencies.Add(efficiency);
                    totalCurrent += efficiency.CurrentSpending;
                    totalOptimal += efficiency.OptimalSpending;
                }
            }

            var overallEfficiency = totalCurrent > 0 
                ? (totalOptimal / totalCurrent) * 100 
                : 100;

            var wastedAmount = Math.Max(0, totalCurrent - totalOptimal);

            var suggestions = new List<string>();
            var lowEfficiencyCategories = categoryEfficiencies
                .Where(c => c.EfficiencyScore < 70)
                .OrderBy(c => c.EfficiencyScore)
                .Take(3);

            foreach (var category in lowEfficiencyCategories)
            {
                suggestions.Add($"優化 {category.Category} 支出，可節省約 NT${category.CurrentSpending - category.OptimalSpending:N0}");
            }

            if (!suggestions.Any())
            {
                suggestions.Add("您的支出效率良好，請保持現有的消費習慣");
            }

            return new SpendingEfficiencyAnalysis
            {
                EfficiencyScore = overallEfficiency,
                CategoryEfficiencies = categoryEfficiencies,
                OptimizationSuggestions = suggestions,
                WastedAmount = wastedAmount,
                OptimalAmount = totalOptimal
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析支出效率時發生錯誤");
            return new SpendingEfficiencyAnalysis { EfficiencyScore = 100 };
        }
    }

    #region 私有方法

    /// <summary>
    /// 產生支出模式洞察
    /// </summary>
    private static List<SmartInsight> GenerateSpendingPatternInsights(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var insights = new List<SmartInsight>();

        // 分析支出集中度
        var expenseRecords = records.Where(r => r.Type == "Expense").ToList();
        var categoryTotals = expenseRecords
            .GroupBy(r => r.Category)
            .Select(g => new { Category = g.Key, Total = g.Sum(r => r.Amount) })
            .OrderByDescending(c => c.Total)
            .ToList();

        if (categoryTotals.Any())
        {
            var topCategory = categoryTotals.First();
            var totalExpense = categoryTotals.Sum(c => c.Total);
            var concentration = (topCategory.Total / totalExpense) * 100;

            if (concentration > 40)
            {
                insights.Add(new SmartInsight
                {
                    Type = "spending_pattern",
                    Title = "支出過度集中",
                    Description = $"{topCategory.Category} 佔總支出的 {concentration:F0}%，建議分散支出風險",
                    Icon = "fa-chart-pie",
                    Color = "warning",
                    Impact = topCategory.Total * 0.1m, // 假設可節省10%
                    Priority = concentration > 60 ? 5 : 4,
                    ActionItems = new List<string>
                    {
                        $"檢視 {topCategory.Category} 支出的合理性",
                        "考慮分散到其他必要類別",
                        "設定此類別的支出上限"
                    },
                    IsActionable = true,
                    GeneratedDate = DateTime.Now
                });
            }
        }

        // 分析支出時間模式
        var weekdayExpense = expenseRecords
            .Where(r => r.Date.DayOfWeek >= DayOfWeek.Monday && r.Date.DayOfWeek <= DayOfWeek.Friday)
            .Sum(r => r.Amount);
        var weekendExpense = expenseRecords
            .Where(r => r.Date.DayOfWeek == DayOfWeek.Saturday || r.Date.DayOfWeek == DayOfWeek.Sunday)
            .Sum(r => r.Amount);

        var weekendRatio = (weekdayExpense + weekendExpense) > 0 
            ? weekendExpense / (weekdayExpense + weekendExpense) 
            : 0;

        if (weekendRatio > 0.4m)
        {
            insights.Add(new SmartInsight
            {
                Type = "spending_pattern",
                Title = "週末支出偏高",
                Description = $"週末支出佔總支出的 {weekendRatio:P0}，可考慮平衡平日與假日消費",
                Icon = "fa-calendar-week",
                Color = "info",
                Impact = weekendExpense * 0.15m,
                Priority = 3,
                ActionItems = new List<string>
                {
                    "規劃週末活動預算",
                    "尋找成本較低的休閒選擇",
                    "將部分週末消費分散到平日"
                },
                IsActionable = true,
                GeneratedDate = DateTime.Now
            });
        }

        return insights;
    }

    /// <summary>
    /// 產生節省洞察
    /// </summary>
    private async Task<List<SmartInsight>> GenerateSavingsInsightsAsync(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var insights = new List<SmartInsight>();
        var opportunities = await IdentifySavingsOpportunitiesAsync();

        foreach (var opportunity in opportunities.Take(2)) // 只取前2個最大的機會
        {
            insights.Add(new SmartInsight
            {
                Type = "savings_opportunity",
                Title = $"{opportunity.Category} 節省機會",
                Description = opportunity.Description,
                Icon = "fa-piggy-bank",
                Color = "success",
                Impact = opportunity.PotentialMonthlySavings,
                Priority = opportunity.PotentialMonthlySavings > 1000 ? 5 : 4,
                ActionItems = opportunity.RequiredActions,
                IsActionable = opportunity.DifficultyLevel != "hard",
                GeneratedDate = DateTime.Now
            });
        }

        return insights;
    }

    /// <summary>
    /// 產生趨勢分析洞察
    /// </summary>
    private async Task<List<SmartInsight>> GenerateTrendInsightsAsync(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var insights = new List<SmartInsight>();

        // 比較與上個月的差異
        var lastMonthStart = startDate.AddMonths(-1);
        var lastMonthEnd = lastMonthStart.AddMonths(1).AddDays(-1);
        var lastMonthRecords = await _accountingService.GetRecordsAsync(lastMonthStart, lastMonthEnd);

        var currentExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
        var lastMonthExpense = lastMonthRecords.Where(r => r.Type == "Expense").Sum(r => r.Amount);

        if (lastMonthExpense > 0)
        {
            var changePercent = ((currentExpense - lastMonthExpense) / lastMonthExpense) * 100;

            if (Math.Abs(changePercent) > 20)
            {
                var isIncrease = changePercent > 0;
                insights.Add(new SmartInsight
                {
                    Type = "trend_analysis",
                    Title = isIncrease ? "支出大幅增加" : "支出大幅減少",
                    Description = $"較上月支出{(isIncrease ? "增加" : "減少")} {Math.Abs(changePercent):F0}%",
                    Icon = isIncrease ? "fa-trending-up" : "fa-trending-down",
                    Color = isIncrease ? "danger" : "success",
                    Impact = Math.Abs(currentExpense - lastMonthExpense),
                    Priority = Math.Abs(changePercent) > 50 ? 5 : 4,
                    ActionItems = isIncrease 
                        ? new List<string>
                        {
                            "分析增加支出的原因",
                            "檢視是否為一次性支出",
                            "調整下月預算規劃"
                        }
                        : new List<string>
                        {
                            "分析節省成功的因素",
                            "鞏固良好的消費習慣",
                            "考慮將節省金額轉為儲蓄"
                        },
                    IsActionable = true,
                    GeneratedDate = DateTime.Now
                });
            }
        }

        return insights;
    }

    /// <summary>
    /// 產生比較分析洞察
    /// </summary>
    private async Task<List<SmartInsight>> GenerateComparisonInsightsAsync(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var insights = new List<SmartInsight>();

        // 與預算比較
        var budgetPerformances = await _budgetService.GetBudgetPerformanceAsync(startDate.Year, startDate.Month);
        var exceededBudgets = budgetPerformances.Where(bp => bp.Status == "exceeded").ToList();

        if (exceededBudgets.Any())
        {
            var totalExceeded = exceededBudgets.Sum(bp => bp.ActualAmount - bp.Budget.BudgetAmount);
            var worstCategory = exceededBudgets.OrderByDescending(bp => bp.UsedPercentage).First();

            insights.Add(new SmartInsight
            {
                Type = "budget_comparison",
                Title = "預算超支警告",
                Description = $"{exceededBudgets.Count} 個類別超出預算，總計超支 NT${totalExceeded:N0}",
                Icon = "fa-exclamation-triangle",
                Color = "danger",
                Impact = totalExceeded,
                Priority = 5,
                ActionItems = new List<string>
                {
                    $"重點關注 {worstCategory.Budget.Category}（超支 {worstCategory.UsedPercentage - 100:F0}%）",
                    "檢討預算設定是否合理",
                    "調整剩餘月份的消費計劃"
                },
                IsActionable = true,
                GeneratedDate = DateTime.Now
            });
        }

        return insights;
    }

    /// <summary>
    /// 為特定分類產生建議
    /// </summary>
    private async Task<PersonalizedRecommendation?> GenerateCategoryRecommendationAsync(
        string category, List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var categoryRecords = records.Where(r => r.Category == category && r.Type == "Expense").ToList();
        if (!categoryRecords.Any()) return null;

        var currentAmount = categoryRecords.Sum(r => r.Amount);

        // 取得歷史資料比較
        var historicalStart = startDate.AddMonths(-3);
        var historicalRecords = await _accountingService.GetRecordsAsync(historicalStart, startDate);
        var historicalAverage = historicalRecords
            .Where(r => r.Category == category && r.Type == "Expense")
            .GroupBy(r => new { r.Date.Year, r.Date.Month })
            .Select(g => g.Sum(r => r.Amount))
            .DefaultIfEmpty(0)
            .Average();

        if (currentAmount > historicalAverage * 1.2m) // 超過歷史平均20%
        {
            var potentialSavings = currentAmount - historicalAverage;
            var difficultyLevel = category switch
            {
                "餐飲" or "娛樂" => "easy",
                "交通" or "購物" => "medium",
                _ => "medium"
            };

            var steps = category switch
            {
                "餐飲" => new List<string>
                {
                    "增加在家用餐頻率",
                    "尋找性價比高的餐廳",
                    "減少外食和飲料購買"
                },
                "娛樂" => new List<string>
                {
                    "選擇免費或低成本的娛樂活動",
                    "善用優惠券和折扣",
                    "與朋友分享活動成本"
                },
                _ => new List<string>
                {
                    $"檢視 {category} 的必要性",
                    "比較不同選項的價格",
                    "設定每月支出上限"
                }
            };

            return new PersonalizedRecommendation
            {
                Category = category,
                Recommendation = $"您的 {category} 支出較平時高 {((currentAmount - historicalAverage) / historicalAverage) * 100:F0}%",
                Reasoning = $"基於過去3個月的平均支出分析，建議控制在 NT${historicalAverage:N0} 左右",
                PotentialSavings = potentialSavings,
                DifficultyLevel = difficultyLevel,
                TimeFrame = 30,
                Steps = steps
            };
        }

        return null;
    }

    /// <summary>
    /// 產生整體財務建議
    /// </summary>
    private static PersonalizedRecommendation? GenerateOverallRecommendation(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var totalIncome = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
        var totalExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
        
        if (totalIncome == 0) return null;

        var savingsRate = (totalIncome - totalExpense) / totalIncome;

        if (savingsRate < 0.1m) // 儲蓄率低於10%
        {
            var targetSavings = totalIncome * 0.2m; // 建議儲蓄率20%
            var requiredReduction = totalExpense - (totalIncome - targetSavings);

            return new PersonalizedRecommendation
            {
                Category = "整體財務",
                Recommendation = $"您的儲蓄率為 {savingsRate:P0}，建議提高到至少20%",
                Reasoning = "良好的儲蓄率是財務健康的重要指標，建議至少達到收入的20%",
                PotentialSavings = requiredReduction,
                DifficultyLevel = "medium",
                TimeFrame = 90,
                Steps = new List<string>
                {
                    "分析並減少非必要支出",
                    "設定自動儲蓄計劃",
                    "每月檢視預算執行狀況",
                    "尋找增加收入的機會"
                }
            };
        }

        return null;
    }

    /// <summary>
    /// 計算收支平衡指標
    /// </summary>
    private static HealthMetric CalculateIncomeExpenseMetric(List<AccountingRecord> records)
    {
        var totalIncome = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
        var totalExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
        
        var ratio = totalIncome > 0 ? totalExpense / totalIncome : 1;
        var score = ratio switch
        {
            <= 0.7m => 90,
            <= 0.85m => 75,
            <= 1.0m => 50,
            _ => 25
        };

        return new HealthMetric
        {
            Name = "收支平衡",
            Score = score,
            Description = $"支出佔收入 {ratio:P0}",
            Benchmark = "建議控制在85%以內",
            IsGood = score >= 70
        };
    }

    /// <summary>
    /// 計算儲蓄率指標
    /// </summary>
    private static HealthMetric CalculateSavingsMetric(List<AccountingRecord> records)
    {
        var totalIncome = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
        var totalExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
        
        var savingsRate = totalIncome > 0 ? (totalIncome - totalExpense) / totalIncome : 0;
        var score = savingsRate switch
        {
            >= 0.2m => 95,
            >= 0.15m => 85,
            >= 0.1m => 70,
            >= 0.05m => 50,
            _ => 30
        };

        return new HealthMetric
        {
            Name = "儲蓄能力",
            Score = score,
            Description = $"儲蓄率 {savingsRate:P0}",
            Benchmark = "建議達到收入的20%",
            IsGood = score >= 70
        };
    }

    /// <summary>
    /// 計算支出多樣性指標
    /// </summary>
    private static HealthMetric CalculateDiversityMetric(List<AccountingRecord> records)
    {
        var expenseRecords = records.Where(r => r.Type == "Expense").ToList();
        if (!expenseRecords.Any())
        {
            return new HealthMetric
            {
                Name = "支出多樣性",
                Score = 50,
                Description = "本月無支出記錄",
                Benchmark = "支出應分散在多個類別",
                IsGood = false
            };
        }

        var categories = expenseRecords.Select(r => r.Category).Distinct().Count();
        var score = categories switch
        {
            >= 8 => 90,
            >= 6 => 80,
            >= 4 => 70,
            >= 2 => 60,
            _ => 40
        };

        return new HealthMetric
        {
            Name = "支出多樣性",
            Score = score,
            Description = $"涵蓋 {categories} 個支出類別",
            Benchmark = "建議分散到6個以上類別",
            IsGood = score >= 70
        };
    }

    /// <summary>
    /// 計算預算遵循指標
    /// </summary>
    private async Task<HealthMetric> CalculateBudgetComplianceMetricAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var budgetPerformances = await _budgetService.GetBudgetPerformanceAsync(startDate.Year, startDate.Month);
            if (!budgetPerformances.Any())
            {
                return new HealthMetric
                {
                    Name = "預算遵循",
                    Score = 60,
                    Description = "尚未設定預算",
                    Benchmark = "建議設定各類別預算",
                    IsGood = false
                };
            }

            var onTrackCount = budgetPerformances.Count(bp => bp.Status == "on_track");
            var totalCount = budgetPerformances.Count;
            var complianceRate = (double)onTrackCount / totalCount;

            var score = (int)(complianceRate * 100);

            return new HealthMetric
            {
                Name = "預算遵循",
                Score = score,
                Description = $"{onTrackCount}/{totalCount} 個類別在預算內",
                Benchmark = "80%以上類別應在預算內",
                IsGood = score >= 80
            };
        }
        catch
        {
            return new HealthMetric
            {
                Name = "預算遵循",
                Score = 60,
                Description = "無法計算預算遵循度",
                Benchmark = "建議設定並追蹤預算",
                IsGood = false
            };
        }
    }

    /// <summary>
    /// 計算支出穩定性指標
    /// </summary>
    private static HealthMetric CalculateStabilityMetric(List<AccountingRecord> records)
    {
        var dailyExpenses = records
            .Where(r => r.Type == "Expense")
            .GroupBy(r => r.Date.Date)
            .Select(g => g.Sum(r => r.Amount))
            .ToList();

        if (dailyExpenses.Count < 5)
        {
            return new HealthMetric
            {
                Name = "支出穩定性",
                Score = 50,
                Description = "資料不足以評估穩定性",
                Benchmark = "支出應保持相對穩定",
                IsGood = false
            };
        }

        var average = dailyExpenses.Average();
        var variance = dailyExpenses.Select(e => Math.Pow((double)(e - average), 2)).Average();
        var stdDev = Math.Sqrt(variance);
        var coefficient = average > 0 ? stdDev / (double)average : 0;

        var score = coefficient switch
        {
            <= 0.3 => 90,
            <= 0.5 => 80,
            <= 0.7 => 70,
            <= 1.0 => 60,
            _ => 40
        };

        return new HealthMetric
        {
            Name = "支出穩定性",
            Score = score,
            Description = $"變動係數 {coefficient:P0}",
            Benchmark = "變動係數應低於50%",
            IsGood = score >= 70
        };
    }

    /// <summary>
    /// 分析分類節省機會
    /// </summary>
    private static SavingsOpportunity? AnalyzeCategorySavingsOpportunity(
        string category, List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var categoryRecords = records.Where(r => r.Category == category && r.Type == "Expense").ToList();
        if (!categoryRecords.Any()) return null;

        var currentAmount = categoryRecords.Sum(r => r.Amount);

        // 分析節省潛力
        var savingsPotential = category switch
        {
            "餐飲" => currentAmount * 0.2m, // 可節省20%
            "娛樂" => currentAmount * 0.3m, // 可節省30%
            "購物" => currentAmount * 0.25m, // 可節省25%
            "交通" => currentAmount * 0.15m, // 可節省15%
            _ => currentAmount * 0.1m // 其他類別10%
        };

        if (savingsPotential < 100) return null; // 節省金額太少不值得建議

        var method = category switch
        {
            "餐飲" => "增加在家用餐，選擇性價比高的選項",
            "娛樂" => "尋找免費或低成本的娛樂活動",
            "購物" => "制定購物清單，避免衝動消費",
            "交通" => "使用大眾運輸，合併行程",
            _ => "比較價格，選擇更經濟的選項"
        };

        var difficulty = category switch
        {
            "餐飲" or "娛樂" => "easy",
            "購物" => "medium",
            _ => "medium"
        };

        var actions = category switch
        {
            "餐飲" => new List<string> { "規劃每週菜單", "減少外食頻率", "使用優惠券" },
            "娛樂" => new List<string> { "尋找免費活動", "與朋友分攤費用", "選擇優惠時段" },
            "購物" => new List<string> { "列購物清單", "比較價格", "避免衝動購買" },
            _ => new List<string> { "比較選項", "尋找替代方案", "規劃使用時機" }
        };

        return new SavingsOpportunity
        {
            Category = category,
            Description = $"透過{method}，預估可節省 {(savingsPotential / currentAmount) * 100:F0}% 的支出",
            PotentialMonthlySavings = savingsPotential,
            PotentialYearlySavings = savingsPotential * 12,
            Method = method,
            DifficultyLevel = difficulty,
            RequiredActions = actions
        };
    }

    /// <summary>
    /// 計算分類效率
    /// </summary>
    private static CategoryEfficiency? CalculateCategoryEfficiency(
        string category, List<AccountingRecord> records)
    {
        var categoryRecords = records.Where(r => r.Category == category && r.Type == "Expense").ToList();
        if (!categoryRecords.Any()) return null;

        var currentSpending = categoryRecords.Sum(r => r.Amount);

        // 基於一般標準估算最佳支出
        var optimalSpending = category switch
        {
            "餐飲" => currentSpending * 0.8m, // 建議減少20%
            "娛樂" => currentSpending * 0.7m, // 建議減少30%
            "購物" => currentSpending * 0.75m, // 建議減少25%
            "交通" => currentSpending * 0.85m, // 建議減少15%
            _ => currentSpending * 0.9m // 其他類別減少10%
        };

        var efficiency = currentSpending > 0 ? (optimalSpending / currentSpending) * 100 : 100;
        var status = efficiency switch
        {
            >= 90 => "efficient",
            >= 70 => "over_spending",
            _ => "under_spending"
        };

        var suggestions = status switch
        {
            "over_spending" => new List<string> { $"建議減少 {category} 支出", "尋找更經濟的替代方案" },
            "under_spending" => new List<string> { $"您的 {category} 支出控制良好", "可考慮適度增加品質" },
            _ => new List<string> { $"保持目前的 {category} 消費水準" }
        };

        return new CategoryEfficiency
        {
            Category = category,
            CurrentSpending = currentSpending,
            OptimalSpending = optimalSpending,
            EfficiencyScore = efficiency,
            Status = status,
            Suggestions = suggestions
        };
    }

    #endregion
}
