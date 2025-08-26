using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Pages;

/// <summary>
/// Razor Pages PageModel for the index3 calendar view.
/// 處理 QueryString 參數驗證、日期計算與 42 格月曆模型輸出。
/// </summary>
public sealed class Index3Model : PageModel
{
    private readonly ILogger<Index3Model> logger;

    /// <summary>
    /// 以 QueryString 綁定的年份。允許為 null；為 null 時會使用今日年分。
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int? Year { get; set; }

    /// <summary>
    /// 以 QueryString 綁定的月份。允許為 null；為 null 時會使用今日月份。
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int? Month { get; set; }

    /// <summary>
    /// 以 QueryString 綁定的日期。允許為 null；為 null 時代表不特別選取某日。
    /// </summary>
    [BindProperty(SupportsGet = true)]
    public int? Day { get; set; }

    /// <summary>
    /// 實際顯示的年份。
    /// </summary>
    public int DisplayYear { get; private set; }

    /// <summary>
    /// 實際顯示的月份。
    /// </summary>
    public int DisplayMonth { get; private set; }

    /// <summary>
    /// 伺服器時區的今天日期。
    /// </summary>
    public DateOnly Today { get; } = DateOnly.FromDateTime(DateTime.Today);

    /// <summary>
    /// 使用者選取的日期（若提供有效 Day 參數）。
    /// </summary>
    public DateOnly? SelectedDate { get; private set; }

    /// <summary>
    /// 42 格（6 週 × 7 天）的月曆視圖模型。
    /// </summary>
    public IReadOnlyList<CalendarCellView> CalendarCells { get; private set; } = Array.Empty<CalendarCellView>();

    /// <summary>
    /// 若輸入參數被自動更正，顯示的提示訊息。
    /// </summary>
    public string? CorrectionMessage { get; private set; }

    /// <summary>
    /// 是否需要顯示自動更正提示。
    /// </summary>
    public bool HasCorrection => CorrectionMessage is not null && CorrectionMessage.Length > 0;

    /// <summary>
    /// 年份下拉選項（1900–2100）。
    /// </summary>
    public IReadOnlyList<int> YearOptions { get; } = Enumerable.Range(1900, 201).ToArray();

    /// <summary>
    /// 月份下拉選項（1–12）。
    /// </summary>
    public IReadOnlyList<int> MonthOptions { get; } = Enumerable.Range(1, 12).ToArray();

    /// <summary>
    /// 週標題（週日為起始）。
    /// </summary>
    public IReadOnlyList<string> WeekdayNames { get; } = new[] { "日", "一", "二", "三", "四", "五", "六" };

    /// <summary>
    /// 前一個月份的年份。
    /// </summary>
    public int PrevYear => DisplayMonth == 1 ? DisplayYear - 1 : DisplayYear;

    /// <summary>
    /// 前一個月份。
    /// </summary>
    public int PrevMonthValue => DisplayMonth == 1 ? 12 : DisplayMonth - 1;

    /// <summary>
    /// 下一個月份的年份。
    /// </summary>
    public int NextYear => DisplayMonth == 12 ? DisplayYear - 0 + 1 : DisplayYear;

    /// <summary>
    /// 下一個月份。
    /// </summary>
    public int NextMonthValue => DisplayMonth == 12 ? 1 : DisplayMonth + 1;

    /// <summary>
    /// 建構函式。
    /// </summary>
    public Index3Model(ILogger<Index3Model> logger)
    {
        this.logger = logger;
    }

    /// <summary>
    /// GET 進入點：讀取查詢參數、進行驗證與自動更正，並生成 42 格月曆模型。
    /// </summary>
    public IActionResult OnGet()
    {
        // 初始化輸入（缺省以今天為準）。
        var inputYear = Year ?? Today.Year;
        var inputMonth = Month ?? Today.Month;
        int? inputDay = Day;

        // 驗證與裁切範圍。
        var normYear = Math.Clamp(inputYear, 1900, 2100);
        var normMonth = Math.Clamp(inputMonth, 1, 12);

        var corrected = normYear != inputYear || normMonth != inputMonth;

        var daysInMonth = DateTime.DaysInMonth(normYear, normMonth);

        int? normDay = null;
        if (inputDay is not null)
        {
            normDay = Math.Clamp(inputDay.Value, 1, daysInMonth);
            corrected = corrected || normDay != inputDay;
        }

        DisplayYear = normYear;
        DisplayMonth = normMonth;

        if (normDay is not null)
        {
            SelectedDate = new DateOnly(DisplayYear, DisplayMonth, normDay.Value);
        }

        if (corrected)
        {
            CorrectionMessage = normDay is not null
                ? $"參數已自動更正為 {DisplayYear}/{DisplayMonth:00}/{normDay:00}。"
                : $"參數已自動更正為 {DisplayYear}/{DisplayMonth:00}。";
            logger.LogDebug("{Message}", CorrectionMessage);
        }

        CalendarCells = BuildCalendar(DisplayYear, DisplayMonth, SelectedDate);
        return Page();
    }

    /// <summary>
    /// 產生 42 格（6 週 × 7 天）的月曆資料。
    /// </summary>
    private IReadOnlyList<CalendarCellView> BuildCalendar(int year, int month, DateOnly? selected)
    {
        var firstOfMonth = new DateOnly(year, month, 1);
        // 以星期日為一週起始：DayOfWeek Sunday=0。
        var startOffset = (int)firstOfMonth.DayOfWeek; // 0..6
        var gridStart = firstOfMonth.AddDays(-startOffset);

        var cells = new List<CalendarCellView>(capacity: 42);
        for (var i = 0; i < 42; i++)
        {
            var d = gridStart.AddDays(i);
            var inCurrent = d.Month == month && d.Year == year;
            var isToday = d == Today;
            var isSelected = selected is not null && d == selected.Value;
            var dowIndex = (int)d.DayOfWeek; // 0..6

            cells.Add(new CalendarCellView(
                Date: d,
                InCurrentMonth: inCurrent,
                IsToday: isToday,
                IsSelected: isSelected,
                DayOfWeekIndex: dowIndex
            ));
        }

        return cells;
    }

    /// <summary>
    /// 月曆單格視圖模型。
    /// </summary>
    public sealed record CalendarCellView(
        DateOnly Date,
        bool InCurrentMonth,
        bool IsToday,
        bool IsSelected,
        int DayOfWeekIndex
    );
}
