using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Services;

namespace Demo.Pages;

/// <summary>
/// Razor Pages PageModel for the index3 calendar view.
/// 處理 QueryString 參數驗證、日期計算與 42 格月曆模型輸出。
/// </summary>
public sealed class Index3Model : PageModel
{
    private readonly ILogger<Index3Model> logger;
    private readonly INoteService noteService;

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
    /// 備註內容，用於 POST 操作。
    /// </summary>
    [BindProperty]
    public string? NoteText { get; set; }

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
    /// 選取日期的備註內容。
    /// </summary>
    public string? SelectedDateNote { get; private set; }

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
    public bool HasCorrection => !string.IsNullOrEmpty(CorrectionMessage);

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
    public IReadOnlyList<string> WeekdayNames { get; } = ["日", "一", "二", "三", "四", "五", "六"];

    /// <summary>
    /// 前一個月份的年份。
    /// </summary>
    public int PrevYear => DisplayMonth == 1 ? DisplayYear - 1 : DisplayYear;

    /// <summary>
    /// 前一個月份。
    /// </summary>
    public int PrevMonth => DisplayMonth == 1 ? 12 : DisplayMonth - 1;

    /// <summary>
    /// 下一個月份的年份。
    /// </summary>
    public int NextYear => DisplayMonth == 12 ? DisplayYear + 1 : DisplayYear;

    /// <summary>
    /// 下一個月份。
    /// </summary>
    public int NextMonth => DisplayMonth == 12 ? 1 : DisplayMonth + 1;

    /// <summary>
    /// 建構函式。
    /// </summary>
    public Index3Model(ILogger<Index3Model> logger, INoteService noteService)
    {
        this.logger = logger;
        this.noteService = noteService;
    }

    /// <summary>
    /// GET 進入點：讀取查詢參數、進行驗證與自動更正，並生成 42 格月曆模型。
    /// </summary>
    public async Task<IActionResult> OnGetAsync()
    {
        // 初始化輸入（缺省以今天為準）
        var inputYear = Year ?? Today.Year;
        var inputMonth = Month ?? Today.Month;
        var inputDay = Day;

        // 驗證與裁切範圍
        var correctedYear = Math.Clamp(inputYear, 1900, 2100);
        var correctedMonth = Math.Clamp(inputMonth, 1, 12);
        var hasCorrection = correctedYear != inputYear || correctedMonth != inputMonth;

        var daysInMonth = DateTime.DaysInMonth(correctedYear, correctedMonth);
        int? correctedDay = null;

        if (inputDay is not null)
        {
            correctedDay = Math.Clamp(inputDay.Value, 1, daysInMonth);
            hasCorrection = hasCorrection || correctedDay != inputDay;
        }

        DisplayYear = correctedYear;
        DisplayMonth = correctedMonth;

        if (correctedDay is not null)
        {
            SelectedDate = new DateOnly(DisplayYear, DisplayMonth, correctedDay.Value);
            // 載入選取日期的註記
            SelectedDateNote = await noteService.GetNoteAsync(SelectedDate.Value);
            NoteText = SelectedDateNote; // 預填入表單
        }

        if (hasCorrection)
        {
            CorrectionMessage = correctedDay is not null
                ? $"參數已自動更正為 {DisplayYear}/{DisplayMonth:00}/{correctedDay:00}。"
                : $"參數已自動更正為 {DisplayYear}/{DisplayMonth:00}。";
            
            logger.LogInformation("Parameters auto-corrected: {Message}", CorrectionMessage);
        }

        CalendarCells = GenerateCalendarGrid(DisplayYear, DisplayMonth, SelectedDate);
        return Page();
    }

    /// <summary>
    /// POST 儲存註記：儲存或更新選取日期的註記。
    /// </summary>
    public async Task<IActionResult> OnPostSaveNoteAsync()
    {
        if (!Year.HasValue || !Month.HasValue || !Day.HasValue)
        {
            return BadRequest("缺少必要的日期參數");
        }

        try
        {
            var date = new DateOnly(Year.Value, Month.Value, Day.Value);
            
            if (string.IsNullOrWhiteSpace(NoteText))
            {
                await noteService.DeleteNoteAsync(date);
            }
            else
            {
                await noteService.SaveNoteAsync(date, NoteText);
            }

            // 重新導向到相同頁面，保持選取狀態
            return RedirectToPage("/index3", new { Year, Month, Day });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "儲存註記時發生錯誤");
            ModelState.AddModelError(string.Empty, "儲存註記時發生錯誤，請稍後再試。");
            
            // 重新載入頁面資料
            await OnGetAsync();
            return Page();
        }
    }

    /// <summary>
    /// POST 刪除註記：移除選取日期的註記。
    /// </summary>
    public async Task<IActionResult> OnPostDeleteNoteAsync()
    {
        if (!Year.HasValue || !Month.HasValue || !Day.HasValue)
        {
            return BadRequest("缺少必要的日期參數");
        }

        try
        {
            var date = new DateOnly(Year.Value, Month.Value, Day.Value);
            await noteService.DeleteNoteAsync(date);

            // 重新導向到相同頁面，保持選取狀態
            return RedirectToPage("/index3", new { Year, Month, Day });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除註記時發生錯誤");
            ModelState.AddModelError(string.Empty, "刪除註記時發生錯誤，請稍後再試。");
            
            // 重新載入頁面資料
            await OnGetAsync();
            return Page();
        }
    }

    /// <summary>
    /// 產生 42 格（6 週 × 7 天）的月曆資料。
    /// </summary>
    private IReadOnlyList<CalendarCellView> GenerateCalendarGrid(int year, int month, DateOnly? selectedDate)
    {
        var firstOfMonth = new DateOnly(year, month, 1);
        // 以星期日為一週起始：DayOfWeek.Sunday = 0
        var startOffset = (int)firstOfMonth.DayOfWeek;
        var gridStartDate = firstOfMonth.AddDays(-startOffset);

        var cells = new List<CalendarCellView>(42);
        
        for (var i = 0; i < 42; i++)
        {
            var currentDate = gridStartDate.AddDays(i);
            var isInCurrentMonth = currentDate.Month == month && currentDate.Year == year;
            var isToday = currentDate == Today;
            var isSelected = selectedDate.HasValue && currentDate == selectedDate.Value;
            var dayOfWeekIndex = (int)currentDate.DayOfWeek;

            cells.Add(new CalendarCellView(
                Date: currentDate,
                InCurrentMonth: isInCurrentMonth,
                IsToday: isToday,
                IsSelected: isSelected,
                DayOfWeekIndex: dayOfWeekIndex
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
