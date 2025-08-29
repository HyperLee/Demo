using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Models;
using Demo.Services;
using System.Text.Json;

namespace Demo.Pages;

/// <summary>
/// 記帳系統列表頁面
/// </summary>
public class index7 : PageModel
{
    private readonly ILogger<index7> _logger;
    private readonly IAccountingService _accountingService;

    public index7(ILogger<index7> logger, IAccountingService accountingService)
    {
        _logger = logger;
        _accountingService = accountingService;
    }

    #region 屬性

    /// <summary>
    /// 當前檢視年份
    /// </summary>
    public int ViewYear { get; set; }

    /// <summary>
    /// 當前檢視月份
    /// </summary>
    public int ViewMonth { get; set; }

    /// <summary>
    /// 月曆資料
    /// </summary>
    public List<CalendarDay> CalendarDays { get; set; } = new();

    /// <summary>
    /// 月度統計摘要
    /// </summary>
    public MonthlySummary MonthlySummary { get; set; } = new();

    /// <summary>
    /// 是否有搜尋條件
    /// </summary>
    public bool HasFilter => ViewYear != DateTime.Now.Year || ViewMonth != DateTime.Now.Month;

    #endregion

    #region 頁面處理方法

    /// <summary>
    /// 頁面載入處理
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int year = 0, int month = 0)
    {
        try
        {
            var currentDate = DateTime.Now;
            ViewYear = year == 0 ? currentDate.Year : year;
            ViewMonth = month == 0 ? currentDate.Month : month;

            // 驗證年月份範圍
            if (ViewMonth < 1 || ViewMonth > 12)
            {
                ViewMonth = currentDate.Month;
            }

            if (ViewYear < 1900 || ViewYear > 2100)
            {
                ViewYear = currentDate.Year;
            }

            // 載入月曆資料
            CalendarDays = await _accountingService.GetCalendarDataAsync(ViewYear, ViewMonth);
            
            // 載入月度統計
            MonthlySummary = await _accountingService.GetMonthlySummaryAsync(ViewYear, ViewMonth);

            ViewData["Title"] = $"記帳系統 - {ViewYear}年{ViewMonth}月";

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入記帳列表頁面時發生錯誤");
            TempData["ErrorMessage"] = "載入頁面時發生錯誤，請稍後再試。";
            return Page();
        }
    }

    /// <summary>
    /// 刪除記錄處理
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync(int id, int year, int month)
    {
        try
        {
            var success = await _accountingService.DeleteRecordAsync(id);
            
            if (success)
            {
                TempData["SuccessMessage"] = "記錄已成功刪除。";
                _logger.LogInformation("成功刪除記帳記錄 {Id}", id);
            }
            else
            {
                TempData["ErrorMessage"] = "找不到要刪除的記錄。";
                _logger.LogWarning("嘗試刪除不存在的記帳記錄 {Id}", id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除記帳記錄 {Id} 時發生錯誤", id);
            TempData["ErrorMessage"] = "刪除記錄時發生錯誤，請稍後再試。";
        }

        return RedirectToPage("index7", new { year, month });
    }

    /// <summary>
    /// AJAX 刪除記錄處理
    /// </summary>
    public async Task<IActionResult> OnPostDeleteRecordAsync([FromBody] DeleteRecordRequest request)
    {
        try
        {
            var success = await _accountingService.DeleteRecordAsync(request.Id);
            
            if (success)
            {
                _logger.LogInformation("成功刪除記帳記錄 {Id}", request.Id);
                return new JsonResult(new { success = true, message = "記錄已成功刪除" });
            }
            else
            {
                _logger.LogWarning("嘗試刪除不存在的記帳記錄 {Id}", request.Id);
                return new JsonResult(new { success = false, message = "找不到要刪除的記錄" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除記帳記錄 {Id} 時發生錯誤", request.Id);
            return new JsonResult(new { success = false, message = "刪除記錄時發生錯誤" });
        }
    }

    /// <summary>
    /// 匯出報表處理
    /// </summary>
    public async Task<IActionResult> OnPostExportAsync(string format, string period, bool includeIncome, bool includeExpense, string? startDate, string? endDate)
    {
        try
        {
            var (exportStartDate, exportEndDate) = GetDateRangeFromPeriod(period, startDate, endDate);
            var records = await _accountingService.GetRecordsAsync(exportStartDate, exportEndDate);

            // 根據選項篩選記錄
            if (!includeIncome)
            {
                records = records.Where(r => r.Type != "Income").ToList();
            }
            if (!includeExpense)
            {
                records = records.Where(r => r.Type != "Expense").ToList();
            }

            if (!records.Any())
            {
                TempData["ErrorMessage"] = "選擇的期間內沒有記錄可匯出。";
                return RedirectToPage("index7", new { year = ViewYear, month = ViewMonth });
            }

            var exportOptions = new ExportOptions
            {
                StartDate = exportStartDate,
                EndDate = exportEndDate,
                IncludeIncome = includeIncome,
                IncludeExpense = includeExpense,
                Title = $"記帳報表 ({exportStartDate:yyyy-MM-dd} ~ {exportEndDate:yyyy-MM-dd})"
            };

            byte[] fileData;
            string fileName;
            string contentType;

            switch (format.ToLower())
            {
                case "pdf":
                    // TODO: 實作 PDF 匯出
                    TempData["InfoMessage"] = "PDF 匯出功能開發中，請選擇其他格式。";
                    return RedirectToPage("index7", new { year = ViewYear, month = ViewMonth });

                case "excel":
                    // TODO: 實作 Excel 匯出  
                    TempData["InfoMessage"] = "Excel 匯出功能開發中，請選擇其他格式。";
                    return RedirectToPage("index7", new { year = ViewYear, month = ViewMonth });

                case "csv":
                    fileData = GenerateCsvExport(records);
                    fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                    contentType = "text/csv";
                    break;

                default:
                    TempData["ErrorMessage"] = "不支援的匯出格式。";
                    return RedirectToPage("index7", new { year = ViewYear, month = ViewMonth });
            }

            _logger.LogInformation("成功匯出 {Count} 筆記錄為 {Format} 格式", records.Count, format.ToUpper());
            return File(fileData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出記帳報表時發生錯誤，格式: {Format}", format);
            TempData["ErrorMessage"] = "匯出時發生錯誤，請稍後再試。";
            return RedirectToPage("index7", new { year = ViewYear, month = ViewMonth });
        }
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 根據期間參數取得日期範圍
    /// </summary>
    private (DateTime startDate, DateTime endDate) GetDateRangeFromPeriod(string period, string? startDate, string? endDate)
    {
        var now = DateTime.Now;
        
        return period switch
        {
            "current_month" => (new DateTime(now.Year, now.Month, 1), new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))),
            "last_month" => (new DateTime(now.Year, now.Month - 1, 1), new DateTime(now.Year, now.Month - 1, DateTime.DaysInMonth(now.Year, now.Month - 1))),
            "current_year" => (new DateTime(now.Year, 1, 1), new DateTime(now.Year, 12, 31)),
            "custom" => (
                DateTime.TryParse(startDate, out var start) ? start : now.AddMonths(-1),
                DateTime.TryParse(endDate, out var end) ? end : now
            ),
            _ => (new DateTime(ViewYear, ViewMonth, 1), new DateTime(ViewYear, ViewMonth, DateTime.DaysInMonth(ViewYear, ViewMonth)))
        };
    }

    /// <summary>
    /// 產生 CSV 匯出檔案
    /// </summary>
    private byte[] GenerateCsvExport(List<AccountingRecord> records)
    {
        var csv = new System.Text.StringBuilder();
        
        // CSV 標題列
        csv.AppendLine("日期,類型,大分類,細分類,金額,付款方式,備註");
        
        // 資料列
        foreach (var record in records.OrderBy(r => r.Date))
        {
            var type = record.Type == "Income" ? "收入" : "支出";
            csv.AppendLine($"{record.Date:yyyy-MM-dd}," +
                          $"\"{type}\"," +
                          $"\"{record.Category}\"," +
                          $"\"{record.SubCategory}\"," +
                          $"{record.Amount}," +
                          $"\"{record.PaymentMethod}\"," +
                          $"\"{record.Note.Replace("\"", "\"\"")}\"");
        }
        
        return System.Text.Encoding.UTF8.GetBytes(csv.ToString());
    }

    #endregion
}

/// <summary>
/// 刪除記錄請求模型
/// </summary>
public class DeleteRecordRequest
{
    public int Id { get; set; }
}
