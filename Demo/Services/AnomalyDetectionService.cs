using Demo.Models;

namespace Demo.Services;

/// <summary>
/// 異常偵測服務，用於偵測支出異常和財務風險評估
/// </summary>
public class AnomalyDetectionService
{
    private readonly IAccountingService _accountingService;
    private readonly ILogger<AnomalyDetectionService> _logger;

    public AnomalyDetectionService(
        IAccountingService accountingService,
        ILogger<AnomalyDetectionService> logger)
    {
        _accountingService = accountingService;
        _logger = logger;
    }

    /// <summary>
    /// 偵測支出異常，使用統計學方法分析異常支出模式
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>異常警報列表</returns>
    public async Task<List<AnomalyAlert>> DetectSpendingAnomaliesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("開始偵測支出異常: {StartDate} - {EndDate}", startDate, endDate);
            
            var alerts = new List<AnomalyAlert>();
            var currentRecords = await _accountingService.GetRecordsAsync(startDate, endDate);
            var expenseRecords = currentRecords.Where(r => r.Type == "Expense").ToList();

            // 1. Z-Score 分析 - 偵測異常高額支出
            var zScoreAlerts = await DetectZScoreAnomaliesAsync(expenseRecords, startDate, endDate);
            alerts.AddRange(zScoreAlerts);

            // 2. 移動平均偏差分析 - 偵測支出模式改變
            var movingAverageAlerts = await DetectMovingAverageAnomaliesAsync(expenseRecords, startDate, endDate);
            alerts.AddRange(movingAverageAlerts);

            // 3. 頻率異常分析 - 偵測消費頻率變化
            var frequencyAlerts = await DetectFrequencyAnomaliesAsync(expenseRecords, startDate, endDate);
            alerts.AddRange(frequencyAlerts);

            _logger.LogInformation("偵測到 {Count} 個異常警報", alerts.Count);
            return alerts.OrderByDescending(a => a.Severity switch
            {
                "critical" => 4,
                "high" => 3,
                "medium" => 2,
                "low" => 1,
                _ => 0
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "偵測支出異常時發生錯誤");
            return new List<AnomalyAlert>();
        }
    }

    /// <summary>
    /// 偵測各分類的異常支出
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>分類異常列表</returns>
    public async Task<List<CategoryAnomaly>> DetectCategoryAnomaliesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var anomalies = new List<CategoryAnomaly>();
            var currentRecords = await _accountingService.GetRecordsAsync(startDate, endDate);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            // 獲取歷史資料進行比較 (過去6個月)
            var historicalStartDate = startDate.AddMonths(-6);
            var historicalRecords = await _accountingService.GetRecordsAsync(historicalStartDate, startDate);

            foreach (var category in categories)
            {
                var currentAmount = currentRecords
                    .Where(r => r.Category == category.Name && r.Type == "Expense")
                    .Sum(r => r.Amount);

                var historicalAmounts = new List<decimal>();
                for (int i = 0; i < 6; i++)
                {
                    var monthStart = startDate.AddMonths(-(i + 1));
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var monthAmount = historicalRecords
                        .Where(r => r.Category == category.Name && r.Type == "Expense" &&
                                   r.Date >= monthStart && r.Date <= monthEnd)
                        .Sum(r => r.Amount);
                    historicalAmounts.Add(monthAmount);
                }

                if (historicalAmounts.Any() && historicalAmounts.Average() > 0)
                {
                    var average = historicalAmounts.Average();
                    var stdDev = CalculateStandardDeviation(historicalAmounts);
                    var zScore = stdDev > 0 ? (double)((currentAmount - average) / stdDev) : 0;

                    // 如果 Z-Score 超過 2（約95%信賴區間），視為異常
                    if (Math.Abs(zScore) > 2.0)
                    {
                        anomalies.Add(new CategoryAnomaly
                        {
                            Category = category.Name,
                            CurrentAmount = currentAmount,
                            HistoricalAverage = average,
                            StandardDeviation = stdDev,
                            ZScore = zScore,
                            AnomalyType = zScore > 0 ? "spike" : "drop",
                            DaysFromNormal = (int)Math.Abs(zScore * 30) // 估算偏離正常的天數
                        });
                    }
                }
            }

            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "偵測分類異常時發生錯誤");
            return new List<CategoryAnomaly>();
        }
    }

    /// <summary>
    /// 偵測消費頻率異常
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>頻率異常列表</returns>
    public async Task<List<FrequencyAnomaly>> DetectFrequencyAnomaliesAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var anomalies = new List<FrequencyAnomaly>();
            var currentRecords = await _accountingService.GetRecordsAsync(startDate, endDate);
            var categories = await _accountingService.GetCategoriesAsync("expense");

            foreach (var category in categories)
            {
                var currentFrequency = currentRecords
                    .Count(r => r.Category == category.Name && r.Amount < 0);

                // 獲取歷史頻率數據 (過去3個月)
                var historicalFrequencies = new List<int>();
                for (int i = 1; i <= 3; i++)
                {
                    var monthStart = startDate.AddMonths(-i);
                    var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                    var historicalRecords = await _accountingService.GetRecordsAsync(monthStart, monthEnd);
                    var frequency = historicalRecords
                        .Count(r => r.Category == category.Name && r.Amount < 0);
                    historicalFrequencies.Add(frequency);
                }

                if (historicalFrequencies.Any())
                {
                    var averageFrequency = historicalFrequencies.Average();
                    var deviation = Math.Abs(currentFrequency - averageFrequency) / (averageFrequency + 1); // 避免除零

                    // 如果頻率變化超過50%，視為異常
                    if (deviation > 0.5 && averageFrequency > 0)
                    {
                        anomalies.Add(new FrequencyAnomaly
                        {
                            Category = category.Name,
                            CurrentFrequency = currentFrequency,
                            HistoricalAverageFrequency = averageFrequency,
                            AnomalyType = currentFrequency > averageFrequency ? "increased" : "decreased",
                            FirstDetectedDate = DateTime.Now,
                            Description = currentFrequency > averageFrequency 
                                ? $"{category.Name} 消費頻率較平常增加 {deviation:P0}"
                                : $"{category.Name} 消費頻率較平常減少 {deviation:P0}"
                        });
                    }
                }
            }

            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "偵測頻率異常時發生錯誤");
            return new List<FrequencyAnomaly>();
        }
    }

    /// <summary>
    /// 評估財務風險等級
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>風險評估結果</returns>
    public async Task<RiskAssessment> AssessFinancialRiskAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await _accountingService.GetRecordsAsync(startDate, endDate);
            var totalIncome = records.Where(r => r.Amount > 0).Sum(r => r.Amount);
            var totalExpense = Math.Abs(records.Where(r => r.Amount < 0).Sum(r => r.Amount));
            
            var riskFactors = new List<RiskFactor>();
            int totalScore = 0;

            // 1. 收支比分析
            var incomeExpenseRatio = totalIncome > 0 ? totalExpense / totalIncome : 1;
            var incomeExpenseScore = incomeExpenseRatio switch
            {
                <= 0.5m => 90, // 支出僅佔收入50%以下，風險很低
                <= 0.7m => 75, // 支出佔收入70%以下，風險低
                <= 0.9m => 50, // 支出佔收入90%以下，風險中等
                <= 1.0m => 25, // 支出接近收入，風險高
                _ => 10        // 支出超過收入，風險很高
            };

            riskFactors.Add(new RiskFactor
            {
                Factor = "收支平衡",
                Score = incomeExpenseScore,
                Impact = incomeExpenseScore >= 70 ? "positive" : incomeExpenseScore >= 40 ? "neutral" : "negative",
                Description = $"支出佔收入 {incomeExpenseRatio:P0}"
            });

            // 2. 支出波動性分析
            var dailyExpenses = records.Where(r => r.Amount < 0)
                .GroupBy(r => r.Date.Date)
                .Select(g => Math.Abs(g.Sum(r => r.Amount)))
                .ToList();

            var expenseVolatility = CalculateStandardDeviation(dailyExpenses) / (dailyExpenses.Average() + 1);
            var volatilityScore = expenseVolatility switch
            {
                <= 0.3M => 85, // 低波動
                <= 0.6M => 60, // 中波動
                _ => 30       // 高波動
            };

            riskFactors.Add(new RiskFactor
            {
                Factor = "支出穩定性",
                Score = volatilityScore,
                Impact = volatilityScore >= 70 ? "positive" : volatilityScore >= 50 ? "neutral" : "negative",
                Description = $"支出波動度 {expenseVolatility:P0}"
            });

            // 3. 儲蓄率分析
            var savingsRate = totalIncome > 0 ? (totalIncome - totalExpense) / totalIncome : 0;
            var savingsScore = savingsRate switch
            {
                >= 0.2m => 90, // 儲蓄率20%以上
                >= 0.1m => 70, // 儲蓄率10-20%
                >= 0.0m => 40, // 收支平衡
                _ => 10        // 負儲蓄
            };

            riskFactors.Add(new RiskFactor
            {
                Factor = "儲蓄能力",
                Score = savingsScore,
                Impact = savingsScore >= 60 ? "positive" : savingsScore >= 40 ? "neutral" : "negative",
                Description = $"儲蓄率 {savingsRate:P0}"
            });

            totalScore = (int)riskFactors.Average(rf => rf.Score);

            var riskLevel = totalScore switch
            {
                >= 75 => "low",
                >= 50 => "medium",
                _ => "high"
            };

            var recommendations = GenerateRiskRecommendations(riskFactors, riskLevel);

            return new RiskAssessment
            {
                OverallScore = totalScore,
                RiskLevel = riskLevel,
                RiskFactors = riskFactors,
                Recommendations = recommendations,
                DebtToIncomeRatio = incomeExpenseRatio,
                SavingsRate = savingsRate,
                ExpenseVolatility = expenseVolatility
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "評估財務風險時發生錯誤");
            return new RiskAssessment { OverallScore = 50, RiskLevel = "medium" };
        }
    }

    #region 私有方法

    /// <summary>
    /// 使用 Z-Score 方法偵測異常
    /// </summary>
    private async Task<List<AnomalyAlert>> DetectZScoreAnomaliesAsync(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var alerts = new List<AnomalyAlert>();
        var categories = await _accountingService.GetCategoriesAsync("expense");

        foreach (var category in categories)
        {
            var categoryRecords = records.Where(r => r.Category == category.Name).ToList();
            if (!categoryRecords.Any()) continue;

            var amounts = categoryRecords.Select(r => r.Amount).ToList();
            var mean = amounts.Average();
            var stdDev = CalculateStandardDeviation(amounts);

            foreach (var record in categoryRecords)
            {
                var amount = record.Amount;
                if (stdDev > 0)
                {
                    var zScore = (double)((amount - mean) / stdDev);
                    if (zScore > 2.5) // 超過2.5個標準差
                    {
                        alerts.Add(new AnomalyAlert
                        {
                            DetectedDate = DateTime.Now,
                            AlertType = "amount",
                            Severity = zScore > 3.0 ? "critical" : "high",
                            Title = $"{category.Name} 異常高額支出",
                            Description = $"在 {record.Date:yyyy/MM/dd} 的 {category.Name} 支出 NT${amount:N0} 超過正常範圍",
                            Category = category.Name,
                            Amount = amount,
                            BaselineAmount = mean,
                            DeviationPercentage = (amount - mean) / mean * 100,
                            Recommendation = "建議檢視此筆支出是否為必要消費，並考慮未來預算調整"
                        });
                    }
                }
            }
        }

        return alerts;
    }

    /// <summary>
    /// 使用移動平均偏差分析偵測異常
    /// </summary>
    private async Task<List<AnomalyAlert>> DetectMovingAverageAnomaliesAsync(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var alerts = new List<AnomalyAlert>();
        var categories = await _accountingService.GetCategoriesAsync("expense");

        foreach (var category in categories)
        {
            var categoryRecords = records
                .Where(r => r.Category == category.Name)
                .OrderBy(r => r.Date)
                .ToList();

            if (categoryRecords.Count < 5) continue; // 需要足夠的資料點

            var amounts = categoryRecords.Select(r => r.Amount).ToList();
            var movingAverages = CalculateMovingAverage(amounts, 5);

            for (int i = 5; i < amounts.Count; i++)
            {
                var deviation = Math.Abs(amounts[i] - movingAverages[i - 5]) / (movingAverages[i - 5] + 1);
                
                if (deviation > 1.0M) // 偏差超過100%
                {
                    alerts.Add(new AnomalyAlert
                    {
                        DetectedDate = DateTime.Now,
                        AlertType = "pattern",
                        Severity = deviation > 2.0M ? "high" : "medium",
                        Title = $"{category.Name} 支出模式改變",
                        Description = $"近期 {category.Name} 支出模式與過往不符，建議關注",
                        Category = category.Name,
                        Amount = amounts[i],
                        BaselineAmount = movingAverages[i - 5],
                        DeviationPercentage = deviation * 100,
                        Recommendation = "檢查是否有新的消費習慣或特殊情況導致支出模式改變"
                    });
                }
            }
        }

        return alerts;
    }

    /// <summary>
    /// 偵測頻率異常並產生警報
    /// </summary>
    private async Task<List<AnomalyAlert>> DetectFrequencyAnomaliesAsync(
        List<AccountingRecord> records, DateTime startDate, DateTime endDate)
    {
        var alerts = new List<AnomalyAlert>();
        var frequencyAnomalies = await DetectFrequencyAnomaliesAsync(startDate, endDate);

        foreach (var anomaly in frequencyAnomalies)
        {
            var severity = Math.Abs(anomaly.CurrentFrequency - anomaly.HistoricalAverageFrequency) switch
            {
                >= 10 => "high",
                >= 5 => "medium",
                _ => "low"
            };

            alerts.Add(new AnomalyAlert
            {
                DetectedDate = DateTime.Now,
                AlertType = "frequency",
                Severity = severity,
                Title = $"{anomaly.Category} 消費頻率異常",
                Description = anomaly.Description,
                Category = anomaly.Category,
                Amount = 0,
                BaselineAmount = 0,
                DeviationPercentage = 0,
                Recommendation = anomaly.AnomalyType == "increased" 
                    ? "消費頻率增加，建議檢視是否符合預算規劃"
                    : "消費頻率減少，可能是好的節約表現"
            });
        }

        return alerts;
    }

    /// <summary>
    /// 計算標準差
    /// </summary>
    private static decimal CalculateStandardDeviation(IEnumerable<decimal> values)
    {
        var valueList = values.ToList();
        if (valueList.Count < 2) return 0;

        var mean = valueList.Average();
        var sumOfSquaredDifferences = valueList.Sum(val => (val - mean) * (val - mean));
        return (decimal)Math.Sqrt((double)(sumOfSquaredDifferences / (valueList.Count - 1)));
    }

    /// <summary>
    /// 計算移動平均
    /// </summary>
    private static List<decimal> CalculateMovingAverage(List<decimal> values, int window)
    {
        var movingAverages = new List<decimal>();
        for (int i = window; i <= values.Count; i++)
        {
            var average = values.Skip(i - window).Take(window).Average();
            movingAverages.Add(average);
        }
        return movingAverages;
    }

    /// <summary>
    /// 根據風險因子產生建議
    /// </summary>
    private static List<string> GenerateRiskRecommendations(List<RiskFactor> riskFactors, string riskLevel)
    {
        var recommendations = new List<string>();

        var lowScoreFactors = riskFactors.Where(rf => rf.Score < 50).ToList();

        foreach (var factor in lowScoreFactors)
        {
            switch (factor.Factor)
            {
                case "收支平衡":
                    recommendations.Add("建議檢視並減少非必要支出，提高收支平衡");
                    break;
                case "支出穩定性":
                    recommendations.Add("建議建立固定預算，減少支出波動");
                    break;
                case "儲蓄能力":
                    recommendations.Add("建議提高儲蓄比例，為未來財務安全做準備");
                    break;
            }
        }

        if (riskLevel == "high")
        {
            recommendations.Add("財務風險較高，建議尋求專業理財顧問協助");
        }

        return recommendations;
    }

    #endregion
}
