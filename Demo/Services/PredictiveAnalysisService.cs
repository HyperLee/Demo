using Demo.Models;

namespace Demo.Services;

/// <summary>
/// 預測性分析服務，提供支出預測、現金流分析和目標達成預測
/// </summary>
public class PredictiveAnalysisService
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<PredictiveAnalysisService> _logger;

    public PredictiveAnalysisService(
        IAccountingService accountingService,
        ILogger<PredictiveAnalysisService> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    /// <summary>
    /// 預測未來支出
    /// </summary>
    /// <param name="monthsAhead">預測的月數</param>
    /// <returns>支出預測列表</returns>
    public async Task<List<ExpenseForecast>> ForecastExpensesAsync(int monthsAhead = 6)
    {
        try
        {
            _logger.LogInformation("開始預測未來 {MonthsAhead} 個月的支出", monthsAhead);
            
            var forecasts = new List<ExpenseForecast>();
            var now = DateTime.Now;
            
            // 取得過去12個月的資料作為基準
            var historicalStart = now.AddMonths(-12);
            var historicalRecords = await _accountingService.GetRecordsAsync(historicalStart, now);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            foreach (var category in categories)
            {
                var forecast = ForecastCategoryExpense(category.Name, historicalRecords, monthsAhead);
                if (forecast is not null)
                {
                    forecasts.Add(forecast);
                }
            }

            return forecasts.OrderByDescending(f => f.ForecastedAmount).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "預測支出時發生錯誤");
            return new List<ExpenseForecast>();
        }
    }

    /// <summary>
    /// 預測收入
    /// </summary>
    /// <param name="monthsAhead">預測的月數</param>
    /// <returns>收入預測</returns>
    public async Task<IncomeForecast> ForecastIncomeAsync(int monthsAhead = 6)
    {
        try
        {
            var now = DateTime.Now;
            var historicalStart = now.AddMonths(-12);
            var historicalRecords = await _accountingService.GetRecordsAsync(historicalStart, now);
            
            var incomeRecords = historicalRecords.Where(r => r.Amount > 0).ToList();
            if (!incomeRecords.Any())
            {
                return new IncomeForecast
                {
                    PeriodStart = now,
                    PeriodEnd = now.AddMonths(monthsAhead),
                    ForecastedAmount = 0,
                    TrendDirection = "stable",
                    DataPoints = new List<ForecastDataPoint>()
                };
            }

            // 計算每月收入
            var monthlyIncomes = new List<(DateTime Month, decimal Amount)>();
            for (int i = 0; i < 12; i++)
            {
                var monthStart = historicalStart.AddMonths(i);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                var monthIncome = incomeRecords
                    .Where(r => r.Date >= monthStart && r.Date <= monthEnd)
                    .Sum(r => r.Amount);
                monthlyIncomes.Add((monthStart, monthIncome));
            }

            // 使用線性回歸進行預測
            var (slope, intercept) = CalculateLinearRegression(monthlyIncomes);
            var dataPoints = new List<ForecastDataPoint>();
            var totalForecast = 0m;

            for (int i = 0; i < monthsAhead; i++)
            {
                var forecastMonth = now.AddMonths(i);
                var monthIndex = 12 + i; // 從第13個月開始預測
                var forecastAmount = Math.Max(0, (decimal)(slope * monthIndex + intercept));
                
                // 加入信賴區間
                var historicalStdDev = CalculateStandardDeviation(monthlyIncomes.Select(m => m.Amount));
                var upperBound = forecastAmount + historicalStdDev * 1.96m; // 95%信賴區間
                var lowerBound = Math.Max(0, forecastAmount - historicalStdDev * 1.96m);

                dataPoints.Add(new ForecastDataPoint
                {
                    Date = forecastMonth,
                    Amount = forecastAmount,
                    UpperBound = upperBound,
                    LowerBound = lowerBound
                });

                totalForecast += forecastAmount;
            }

            var trendDirection = slope switch
            {
                > 100 => "increasing",
                < -100 => "decreasing",
                _ => "stable"
            };

            return new IncomeForecast
            {
                PeriodStart = now,
                PeriodEnd = now.AddMonths(monthsAhead),
                ForecastedAmount = totalForecast,
                ConfidenceInterval = CalculateStandardDeviation(monthlyIncomes.Select(m => m.Amount)) * 1.96m,
                TrendDirection = trendDirection,
                DataPoints = dataPoints
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "預測收入時發生錯誤");
            return new IncomeForecast
            {
                PeriodStart = DateTime.Now,
                PeriodEnd = DateTime.Now.AddMonths(monthsAhead),
                ForecastedAmount = 0,
                TrendDirection = "stable",
                DataPoints = new List<ForecastDataPoint>()
            };
        }
    }

    /// <summary>
    /// 預測現金流
    /// </summary>
    /// <param name="monthsAhead">預測的月數</param>
    /// <returns>現金流預測</returns>
    public async Task<CashFlowProjection> ProjectCashFlowAsync(int monthsAhead = 12)
    {
        try
        {
            var incomeForecast = await ForecastIncomeAsync(monthsAhead);
            var expenseForecasts = await ForecastExpensesAsync(monthsAhead);

            var dataPoints = new List<CashFlowDataPoint>();
            var warnings = new List<string>();
            var cumulativeBalance = 0m; // 假設起始餘額為0
            var minBalance = decimal.MaxValue;
            var maxBalance = decimal.MinValue;
            var minBalanceDate = DateTime.Now;
            var maxBalanceDate = DateTime.Now;

            for (int i = 0; i < monthsAhead; i++)
            {
                var month = DateTime.Now.AddMonths(i);
                var monthlyIncome = incomeForecast.DataPoints.ElementAtOrDefault(i)?.Amount ?? 0;
                var monthlyExpense = expenseForecasts.Sum(ef => ef.DataPoints.ElementAtOrDefault(i)?.Amount ?? 0);
                var netFlow = monthlyIncome - monthlyExpense;
                cumulativeBalance += netFlow;

                // 追蹤最大和最小餘額
                if (cumulativeBalance < minBalance)
                {
                    minBalance = cumulativeBalance;
                    minBalanceDate = month;
                }
                if (cumulativeBalance > maxBalance)
                {
                    maxBalance = cumulativeBalance;
                    maxBalanceDate = month;
                }

                dataPoints.Add(new CashFlowDataPoint
                {
                    Date = month,
                    Income = monthlyIncome,
                    Expense = monthlyExpense,
                    NetFlow = netFlow,
                    CumulativeBalance = cumulativeBalance
                });

                // 產生警告
                if (cumulativeBalance < 0)
                {
                    warnings.Add($"{month:yyyy/MM} 預估現金流為負：NT${cumulativeBalance:N0}");
                }
                else if (netFlow < 0 && Math.Abs(netFlow) > monthlyIncome * 0.8m)
                {
                    warnings.Add($"{month:yyyy/MM} 支出過高，佔收入 {Math.Abs(netFlow) / monthlyIncome:P0}");
                }
            }

            return new CashFlowProjection
            {
                PeriodStart = DateTime.Now,
                PeriodEnd = DateTime.Now.AddMonths(monthsAhead),
                DataPoints = dataPoints,
                MinBalance = minBalance,
                MaxBalance = maxBalance,
                MinBalanceDate = minBalanceDate,
                MaxBalanceDate = maxBalanceDate,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "預測現金流時發生錯誤");
            return new CashFlowProjection
            {
                PeriodStart = DateTime.Now,
                PeriodEnd = DateTime.Now.AddMonths(monthsAhead),
                DataPoints = new List<CashFlowDataPoint>(),
                Warnings = new List<string> { "無法計算現金流預測" }
            };
        }
    }

    /// <summary>
    /// 分析季節性趨勢
    /// </summary>
    /// <returns>季節性趨勢列表</returns>
    public async Task<List<SeasonalTrend>> AnalyzeSeasonalTrendsAsync()
    {
        try
        {
            var trends = new List<SeasonalTrend>();
            var now = DateTime.Now;
            
            // 取得過去24個月的資料
            var historicalStart = now.AddMonths(-24);
            var historicalRecords = await _accountingService.GetRecordsAsync(historicalStart, now);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            foreach (var category in categories)
            {
                var trend = AnalyzeCategorySeasonalTrend(category.Name, historicalRecords);
                if (trend is not null)
                {
                    trends.Add(trend);
                }
            }

            return trends.Where(t => t.AverageAmount > 0).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "分析季節性趨勢時發生錯誤");
            return new List<SeasonalTrend>();
        }
    }

    /// <summary>
    /// 預測目標達成可能性
    /// </summary>
    /// <param name="targetAmount">目標金額</param>
    /// <param name="targetDate">目標日期</param>
    /// <returns>目標達成預測</returns>
    public async Task<GoalAchievementPrediction> PredictGoalAchievementAsync(decimal targetAmount, DateTime targetDate)
    {
        try
        {
            var now = DateTime.Now;
            var monthsToTarget = (int)Math.Ceiling((targetDate - now).TotalDays / 30.0);
            
            if (monthsToTarget <= 0)
            {
                return new GoalAchievementPrediction
                {
                    TargetAmount = targetAmount,
                    TargetDate = targetDate,
                    CurrentAmount = 0,
                    RequiredAmount = targetAmount,
                    PredictedAmount = 0,
                    AchievementProbability = 0,
                    PredictedAchievementDate = targetDate,
                    DaysAhead = 0,
                    Recommendations = new List<string> { "目標日期已過或太接近，請調整目標" }
                };
            }

            // 預測期間內的淨儲蓄
            var cashFlowProjection = await ProjectCashFlowAsync(monthsToTarget);
            var predictedNetSavings = cashFlowProjection.DataPoints.Sum(dp => dp.NetFlow);

            // 假設目前已有一定儲蓄（可從實際資料計算）
            var currentAmount = Math.Max(0, predictedNetSavings * 0.1m); // 假設目前有10%
            var requiredAmount = targetAmount - currentAmount;

            // 計算達成機率
            var monthlyAverageSavings = predictedNetSavings / monthsToTarget;
            var requiredMonthlySavings = requiredAmount / monthsToTarget;
            
            var achievementProbability = monthlyAverageSavings > 0 
                ? Math.Min(1.0, Math.Max(0.0, (double)(monthlyAverageSavings / requiredMonthlySavings)))
                : 0.0;

            // 預測實際達成日期
            var predictedAchievementMonths = monthlyAverageSavings > 0 
                ? (int)Math.Ceiling((double)(requiredAmount / monthlyAverageSavings))
                : int.MaxValue;

            var predictedAchievementDate = predictedAchievementMonths < 120 // 10年內
                ? now.AddMonths(predictedAchievementMonths)
                : targetDate.AddYears(10);

            var daysAhead = (int)(targetDate - predictedAchievementDate).TotalDays;

            // 產生建議
            var recommendations = new List<string>();
            
            if (achievementProbability < 0.7)
            {
                recommendations.Add($"建議每月至少儲蓄 NT${requiredMonthlySavings:N0} 以達成目標");
                recommendations.Add("考慮減少非必要支出以增加儲蓄");
            }
            
            if (achievementProbability < 0.5)
            {
                recommendations.Add("目標可能過於樂觀，建議調整目標金額或時間");
                recommendations.Add("尋找增加收入的機會");
            }
            else if (achievementProbability > 0.9)
            {
                recommendations.Add("目標達成機會很高，可考慮設定更積極的目標");
            }

            return new GoalAchievementPrediction
            {
                TargetAmount = targetAmount,
                TargetDate = targetDate,
                CurrentAmount = currentAmount,
                RequiredAmount = requiredAmount,
                PredictedAmount = currentAmount + predictedNetSavings,
                AchievementProbability = achievementProbability,
                PredictedAchievementDate = predictedAchievementDate,
                DaysAhead = daysAhead,
                Recommendations = recommendations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "預測目標達成時發生錯誤");
            return new GoalAchievementPrediction
            {
                TargetAmount = targetAmount,
                TargetDate = targetDate,
                CurrentAmount = 0,
                RequiredAmount = targetAmount,
                PredictedAmount = 0,
                AchievementProbability = 0,
                PredictedAchievementDate = targetDate,
                DaysAhead = 0,
                Recommendations = new List<string> { "無法計算目標達成預測" }
            };
        }
    }

    #region 私有方法

    /// <summary>
    /// 預測單一分類的支出
    /// </summary>
    private ExpenseForecast? ForecastCategoryExpense(
        string category, List<AccountingRecord> historicalRecords, int monthsAhead)
    {
        var categoryRecords = historicalRecords
            .Where(r => r.Category == category && r.Amount < 0)
            .ToList();

        if (!categoryRecords.Any())
            return null;

        // 計算每月支出
        var now = DateTime.Now;
        var monthlyExpenses = new List<(DateTime Month, decimal Amount)>();
        
        for (int i = -12; i < 0; i++) // 過去12個月
        {
            var monthStart = now.AddMonths(i);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var monthExpense = Math.Abs(categoryRecords
                .Where(r => r.Date >= monthStart && r.Date <= monthEnd)
                .Sum(r => r.Amount));
            monthlyExpenses.Add((monthStart, monthExpense));
        }

        // 使用線性回歸進行預測
        var (slope, intercept) = CalculateLinearRegression(monthlyExpenses);
        var dataPoints = new List<ForecastDataPoint>();
        var totalForecast = 0m;

        for (int i = 0; i < monthsAhead; i++)
        {
            var forecastMonth = now.AddMonths(i);
            var monthIndex = i;
            var baseAmount = Math.Max(0, (decimal)(slope * monthIndex + intercept));
            
            // 加入季節性調整
            var seasonalFactor = GetSeasonalFactor(category, forecastMonth.Month);
            var forecastAmount = baseAmount * seasonalFactor;

            // 計算信賴區間
            var historicalStdDev = CalculateStandardDeviation(monthlyExpenses.Select(m => m.Amount));
            var upperBound = forecastAmount + historicalStdDev * 1.96m;
            var lowerBound = Math.Max(0, forecastAmount - historicalStdDev * 1.96m);

            dataPoints.Add(new ForecastDataPoint
            {
                Date = forecastMonth,
                Amount = forecastAmount,
                UpperBound = upperBound,
                LowerBound = lowerBound
            });

            totalForecast += forecastAmount;
        }

        var trendDirection = slope switch
        {
            > 50 => "increasing",
            < -50 => "decreasing",
            _ => "stable"
        };

        return new ExpenseForecast
        {
            Category = category,
            PeriodStart = now,
            PeriodEnd = now.AddMonths(monthsAhead),
            ForecastedAmount = totalForecast,
            ConfidenceInterval = CalculateStandardDeviation(monthlyExpenses.Select(m => m.Amount)) * 1.96m,
            TrendDirection = trendDirection,
            DataPoints = dataPoints
        };
    }

    /// <summary>
    /// 分析分類的季節性趨勢
    /// </summary>
    private static SeasonalTrend? AnalyzeCategorySeasonalTrend(string category, List<AccountingRecord> records)
    {
        var categoryRecords = records
            .Where(r => r.Category == category && r.Amount < 0)
            .ToList();

        if (!categoryRecords.Any())
            return null;

        // 按月份分組計算平均
        var monthlyAverages = categoryRecords
            .GroupBy(r => r.Date.Month)
            .Select(g => new
            {
                Month = g.Key,
                Average = Math.Abs(g.Average(r => r.Amount))
            })
            .OrderBy(m => m.Month)
            .ToList();

        if (!monthlyAverages.Any())
            return null;

        var yearlyAverage = monthlyAverages.Average(m => m.Average);
        var highActivityMonths = monthlyAverages
            .Where(m => m.Average > yearlyAverage * 1.2m)
            .Select(m => m.Month)
            .ToList();

        var lowActivityMonths = monthlyAverages
            .Where(m => m.Average < yearlyAverage * 0.8m)
            .Select(m => m.Month)
            .ToList();

        // 找出最具季節性的月份
        var peakMonth = monthlyAverages.OrderByDescending(m => m.Average).First();
        var seasonalityIndex = yearlyAverage > 0 ? peakMonth.Average / yearlyAverage : 1;

        var trendDescription = seasonalityIndex switch
        {
            > 1.5m => $"{peakMonth.Month}月為高峰期，支出較平均高 {(seasonalityIndex - 1) * 100:F0}%",
            < 0.7m => $"{category} 支出相對穩定，無明顯季節性",
            _ => $"{peakMonth.Month}月支出略高於平均"
        };

        return new SeasonalTrend
        {
            Category = category,
            Month = peakMonth.Month,
            AverageAmount = peakMonth.Average,
            SeasonalityIndex = seasonalityIndex,
            TrendDescription = trendDescription,
            HighActivityMonths = highActivityMonths,
            LowActivityMonths = lowActivityMonths
        };
    }

    /// <summary>
    /// 計算線性回歸
    /// </summary>
    private static (double Slope, double Intercept) CalculateLinearRegression(List<(DateTime Month, decimal Amount)> data)
    {
        if (data.Count < 2)
            return (0, (double)(data.FirstOrDefault().Amount));

        var n = data.Count;
        var sumX = 0.0;
        var sumY = 0.0;
        var sumXY = 0.0;
        var sumXX = 0.0;

        for (int i = 0; i < n; i++)
        {
            var x = i;
            var y = (double)data[i].Amount;
            
            sumX += x;
            sumY += y;
            sumXY += x * y;
            sumXX += x * x;
        }

        var slope = (n * sumXY - sumX * sumY) / (n * sumXX - sumX * sumX);
        var intercept = (sumY - slope * sumX) / n;

        return (slope, intercept);
    }

    /// <summary>
    /// 計算標準差
    /// </summary>
    private static decimal CalculateStandardDeviation(IEnumerable<decimal> values)
    {
        var valueList = values.ToList();
        if (valueList.Count < 2)
            return 0;

        var mean = valueList.Average();
        var sumOfSquaredDifferences = valueList.Sum(val => (val - mean) * (val - mean));
        return (decimal)Math.Sqrt((double)(sumOfSquaredDifferences / (valueList.Count - 1)));
    }

    /// <summary>
    /// 取得季節性調整因子
    /// </summary>
    private static decimal GetSeasonalFactor(string category, int month)
    {
        // 根據一般經驗設定季節性因子
        return category switch
        {
            "餐飲" => month switch
            {
                12 or 1 or 2 => 1.2m, // 年末年初聚餐較多
                _ => 1.0m
            },
            "娛樂" => month switch
            {
                7 or 8 or 12 => 1.3m, // 暑假和年末娛樂支出較高
                _ => 1.0m
            },
            "購物" => month switch
            {
                11 or 12 => 1.4m, // 年末購物季
                _ => 1.0m
            },
            "交通" => month switch
            {
                7 or 8 => 1.2m, // 暑假出遊
                1 or 2 => 1.1m, // 過年返鄉
                _ => 1.0m
            },
            _ => 1.0m // 其他類別無季節性調整
        };
    }

    #endregion
}
