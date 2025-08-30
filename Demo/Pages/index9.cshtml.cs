using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Services;
using Demo.Models;

namespace Demo.Pages;

public class Index9Model : PageModel
{
    private readonly IAccountingService _accountingService;
    private static DashboardStats? _cachedStats;
    private static DateTime _lastCacheUpdate = DateTime.MinValue;
    
    [BindProperty]
    public string TimeRange { get; set; } = "thisMonth";

    public List<AccountingRecord> RecentTransactions { get; set; } = [];
    public DashboardStats? Stats { get; set; }
    public List<DashboardCard> Cards { get; set; } = [];

    public Index9Model(IAccountingService accountingService)
    {
        _accountingService = accountingService;
    }

    public async Task OnGetAsync()
    {
        // 初始載入使用當月資料
        await GetDashboardStatsAsync();
        await LoadDisplayData();
    }

    public async Task<IActionResult> OnPostUpdateDataAsync()
    {
        await GetDashboardStatsAsync();
        await LoadDisplayData();
        
        return new JsonResult(new
        {
            success = true,
            data = Stats,
            cards = Cards,
            recentTransactions = RecentTransactions
        });
    }

    private async Task LoadDisplayData()
    {
        if (_cachedStats == null) return;
        
        Stats = _cachedStats;
        
        // 準備統計卡片
        Cards =
        [
            new DashboardCard
            {
                Title = "本期收入",
                Value = _cachedStats.CurrentMonthIncome,
                FormattedValue = _cachedStats.CurrentMonthIncome.ToString("C0"),
                IconClass = "fas fa-arrow-up",
                BackgroundColor = "success",
                ChangePercent = _cachedStats.ComparisonData?.IncomeChangePercent ?? 0,
                Trend = GetTrend(_cachedStats.ComparisonData?.IncomeChangePercent ?? 0)
            },
            new DashboardCard
            {
                Title = "本期支出",
                Value = _cachedStats.CurrentMonthExpense,
                FormattedValue = _cachedStats.CurrentMonthExpense.ToString("C0"),
                IconClass = "fas fa-arrow-down",
                BackgroundColor = "danger",
                ChangePercent = _cachedStats.ComparisonData?.ExpenseChangePercent ?? 0,
                Trend = GetTrend(_cachedStats.ComparisonData?.ExpenseChangePercent ?? 0)
            },
            new DashboardCard
            {
                Title = "淨收支",
                Value = _cachedStats.NetIncome,
                FormattedValue = _cachedStats.NetIncome.ToString("C0"),
                IconClass = "fas fa-balance-scale",
                BackgroundColor = _cachedStats.NetIncome >= 0 ? "primary" : "warning",
                ChangePercent = _cachedStats.ComparisonData?.NetIncomeChangePercent ?? 0,
                Trend = GetTrend(_cachedStats.ComparisonData?.NetIncomeChangePercent ?? 0)
            },
            new DashboardCard
            {
                Title = "日均支出",
                Value = _cachedStats.DailyAverageExpense,
                FormattedValue = _cachedStats.DailyAverageExpense.ToString("C0"),
                IconClass = "fas fa-calendar-day",
                BackgroundColor = "info",
                Subtitle = "每日平均"
            }
        ];

        // 載入最近交易記錄
        var allRecords = await _accountingService.GetRecordsAsync();
        var (startDate, endDate) = GetDateRange();
        var filteredRecords = allRecords.Where(r => 
            r.Date >= startDate && r.Date <= endDate).ToList();
        
        RecentTransactions = filteredRecords
            .OrderByDescending(r => r.Date)
            .Take(10)
            .ToList();
    }

    private async Task GetDashboardStatsAsync()
    {
        // 快取機制：避免重複計算
        if (_cachedStats != null && DateTime.Now.Subtract(_lastCacheUpdate).TotalMinutes < 5)
        {
            return;
        }

        var allRecords = await _accountingService.GetRecordsAsync();
        var (startDate, endDate) = GetDateRange();
        
        var filteredRecords = allRecords.Where(r => 
            r.Date >= startDate && r.Date <= endDate).ToList();

        _cachedStats = CalculateStats(filteredRecords, allRecords);
        _lastCacheUpdate = DateTime.Now;
    }

    private (DateTime startDate, DateTime endDate) GetDateRange()
    {
        var now = DateTime.Now;
        
        if (TimeRange == "thisWeek")
        {
            var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
            return (startOfWeek.Date, now.Date);
        }
        else if (TimeRange == "thisMonth")
        {
            return (new DateTime(now.Year, now.Month, 1), now.Date);
        }
        else if (TimeRange == "thisYear")
        {
            return (new DateTime(now.Year, 1, 1), now.Date);
        }
        else if (TimeRange == "lastMonth")
        {
            var lastMonth = now.AddMonths(-1);
            var startOfLastMonth = new DateTime(lastMonth.Year, lastMonth.Month, 1);
            var endOfLastMonth = startOfLastMonth.AddMonths(1).AddDays(-1);
            return (startOfLastMonth, endOfLastMonth);
        }
        else
        {
            // 預設本月
            return (new DateTime(now.Year, now.Month, 1), now.Date);
        }
    }

    private DashboardStats CalculateStats(List<AccountingRecord> currentRecords, List<AccountingRecord> allRecords)
    {
        var totalIncome = currentRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);
        var totalExpense = Math.Abs(currentRecords.Where(r => r.Amount < 0).Sum(r => r.Amount));
        var balance = totalIncome - totalExpense;
        
        var trendData = CalculateTrendData(currentRecords);
        var categoryData = CalculateCategoryData(currentRecords);
        var comparison = CalculateComparison(currentRecords, allRecords);

        return new DashboardStats
        {
            CurrentMonthIncome = totalIncome,
            CurrentMonthExpense = totalExpense,
            NetIncome = balance,
            DailyAverageExpense = totalExpense / (DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)),
            YearToDateIncome = totalIncome,
            YearToDateExpense = totalExpense,
            TrendData = trendData,
            CategoryData = categoryData,
            ComparisonData = comparison
        };
    }

    private List<MonthlyTrend> CalculateTrendData(List<AccountingRecord> records)
    {
        var trendData = new List<MonthlyTrend>();
        
        if (TimeRange == "thisYear")
        {
            // 年度趨勢：月份統計
            for (int month = 1; month <= 12; month++)
            {
                var monthlyRecords = records.Where(r => r.Date.Month == month).ToList();
                var income = monthlyRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);
                var expense = Math.Abs(monthlyRecords.Where(r => r.Amount < 0).Sum(r => r.Amount));
                
                trendData.Add(new MonthlyTrend
                {
                    Month = $"2024-{month:D2}",
                    MonthName = $"{month}月",
                    Income = income,
                    Expense = expense,
                    TransactionCount = monthlyRecords.Count
                });
            }
        }
        else
        {
            // 月度或週度趨勢：日統計
            var groupedByDay = records.GroupBy(r => r.Date.Date).ToList();
            
            foreach (var group in groupedByDay.OrderBy(g => g.Key))
            {
                var dayRecords = group.ToList();
                var income = dayRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);
                var expense = Math.Abs(dayRecords.Where(r => r.Amount < 0).Sum(r => r.Amount));
                
                trendData.Add(new MonthlyTrend
                {
                    Month = group.Key.ToString("yyyy-MM-dd"),
                    MonthName = group.Key.ToString("MM/dd"),
                    Income = income,
                    Expense = expense,
                    TransactionCount = dayRecords.Count
                });
            }
        }
        
        return trendData;
    }

    private List<CategorySummary> CalculateCategoryData(List<AccountingRecord> records)
    {
        var expenseRecords = records.Where(r => r.Amount < 0).ToList();
        
        var categoryTotals = expenseRecords
            .GroupBy(r => r.Category)
            .Select(g => new CategorySummary
            {
                Category = g.Key,
                Amount = Math.Abs(g.Sum(r => r.Amount)),
                TransactionCount = g.Count(),
                Percentage = 0, // 稍後計算
                Color = "#3498db"
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        var totalExpense = categoryTotals.Sum(c => c.Amount);
        
        foreach (var category in categoryTotals)
        {
            category.Percentage = totalExpense > 0 ? (category.Amount / totalExpense) * 100 : 0;
        }

        return categoryTotals;
    }

    private ComparisonStats CalculateComparison(List<AccountingRecord> currentRecords, List<AccountingRecord> allRecords)
    {
        // 取得上期資料進行比較
        var (currentStart, currentEnd) = GetDateRange();
        var period = currentEnd.Subtract(currentStart);
        
        var previousStart = currentStart.Subtract(period).AddDays(-1);
        var previousEnd = currentStart.AddDays(-1);
        
        var previousRecords = allRecords.Where(r => 
            r.Date >= previousStart && r.Date <= previousEnd).ToList();

        var currentIncome = currentRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);
        var currentExpense = Math.Abs(currentRecords.Where(r => r.Amount < 0).Sum(r => r.Amount));
        
        var previousIncome = previousRecords.Where(r => r.Amount > 0).Sum(r => r.Amount);
        var previousExpense = Math.Abs(previousRecords.Where(r => r.Amount < 0).Sum(r => r.Amount));

        var incomeChange = previousIncome > 0 ? ((currentIncome - previousIncome) / previousIncome) * 100 : 0;
        var expenseChange = previousExpense > 0 ? ((currentExpense - previousExpense) / previousExpense) * 100 : 0;

        return new ComparisonStats
        {
            IncomeChangePercent = incomeChange,
            ExpenseChangePercent = expenseChange,
            NetIncomeChangePercent = (currentIncome - currentExpense) - (previousIncome - previousExpense),
            IncomeChangeAmount = currentIncome - previousIncome,
            ExpenseChangeAmount = currentExpense - previousExpense,
            ComparisonPeriod = GetPeriodDescription()
        };
    }

    private string GetPeriodDescription()
    {
        if (TimeRange == "thisWeek")
            return "與上週比較";
        else if (TimeRange == "thisMonth")
            return "與上月比較";
        else if (TimeRange == "thisYear")
            return "與去年比較";
        else if (TimeRange == "lastMonth")
            return "與前月比較";
        else
            return "期間比較";
    }

    private string GetTrend(decimal changePercent)
    {
        if (changePercent > 0) return "up";
        else if (changePercent < 0) return "down";
        else return "stable";
    }

    public DashboardStats? GetCachedStats()
    {
        return _cachedStats;
    }
}
