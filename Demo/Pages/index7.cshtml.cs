using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Models;
using Demo.Services;
using Demo.Utilities;
using System.Text.Json;
using ClosedXML.Excel;

namespace Demo.Pages;

/// <summary>
/// 記帳系統列表頁面
/// </summary>
public class index7 : PageModel
{
    private readonly ILogger<index7> _logger;
    private readonly IAccountingService _accountingService;
    private readonly IStatisticsService _statisticsService;
    private readonly IStatisticsExportService _statisticsExportService;

    public index7(ILogger<index7> logger, IAccountingService accountingService, IStatisticsService statisticsService, IStatisticsExportService statisticsExportService)
    {
        _logger = logger;
        _accountingService = accountingService;
        _statisticsService = statisticsService;
        _statisticsExportService = statisticsExportService;
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
    /// 取得統計分析資料
    /// </summary>
    /// <param name="startDate">開始日期</param>
    /// <param name="endDate">結束日期</param>
    /// <returns>統計分析資料</returns>
    public async Task<IActionResult> OnGetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // 預設為最近 6 個月
            var end = endDate ?? DateTime.Today;
            var start = startDate ?? DateTime.Today.AddMonths(-6);
            
            // 驗證日期範圍
            if (start > end)
            {
                return new JsonResult(new { success = false, message = "開始日期不能晚於結束日期" });
            }
            
            // 限制最大查詢範圍為 2 年
            var maxDays = 730;
            var daysDiff = (end - start).TotalDays;
            if (daysDiff > maxDays)
            {
                return new JsonResult(new { success = false, message = "日期範圍不能超過 2 年" });
            }
            
            var viewModel = new StatisticsViewModel
            {
                StartDate = start,
                EndDate = end,
                MonthlyTrend = await _statisticsService.GetMonthlyTrendAsync(6),
                ExpenseCategories = await _statisticsService.GetExpenseCategoryAnalysisAsync(start, end),
                Summary = await _statisticsService.GetStatisticsSummaryAsync(start, end)
            };
            
            _logger.LogInformation("成功載入統計分析資料，期間：{StartDate} ~ {EndDate}", start, end);
            return new JsonResult(viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入統計分析資料時發生錯誤");
            return new JsonResult(new { success = false, message = "載入統計資料時發生錯誤，請稍後再試" });
        }
    }

    /// <summary>
    /// 取得收入分類分析
    /// </summary>
    public async Task<IActionResult> OnGetIncomeCategoryAnalysisAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return new JsonResult(new { success = false, message = "開始日期不能晚於結束日期" });
            }

            var incomeCategories = await _statisticsService.GetIncomeCategoryAnalysisAsync(startDate, endDate);
            
            return new JsonResult(new
            {
                success = true,
                data = incomeCategories
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得收入分類分析時發生錯誤");
            return new JsonResult(new { success = false, message = "取得收入分類分析時發生錯誤" });
        }
    }

    /// <summary>
    /// 取得分類排行榜
    /// </summary>
    public async Task<IActionResult> OnGetCategoryRankingAsync(DateTime startDate, DateTime endDate, string type, int topCount = 10)
    {
        try
        {
            if (startDate > endDate)
            {
                return new JsonResult(new { success = false, message = "開始日期不能晚於結束日期" });
            }

            if (!new[] { "income", "expense" }.Contains(type.ToLower()))
            {
                return new JsonResult(new { success = false, message = "無效的排行榜類型" });
            }

            var ranking = await _statisticsService.GetCategoryRankingAsync(startDate, endDate, 
                type.Substring(0, 1).ToUpper() + type.Substring(1).ToLower(), topCount);
            
            return new JsonResult(new
            {
                success = true,
                data = ranking
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分類排行榜時發生錯誤");
            return new JsonResult(new { success = false, message = "取得分類排行榜時發生錯誤" });
        }
    }

    /// <summary>
    /// 取得時間模式分析
    /// </summary>
    public async Task<IActionResult> OnGetTimePatternAnalysisAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            if (startDate > endDate)
            {
                return new JsonResult(new { success = false, message = "開始日期不能晚於結束日期" });
            }

            var timePattern = await _statisticsService.GetTimePatternAnalysisAsync(startDate, endDate);
            
            return new JsonResult(new
            {
                success = true,
                data = timePattern
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得時間模式分析時發生錯誤");
            return new JsonResult(new { success = false, message = "取得時間模式分析時發生錯誤" });
        }
    }

    /// <summary>
    /// 取得比較分析
    /// </summary>
    public async Task<IActionResult> OnGetComparisonAnalysisAsync(DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd)
    {
        try
        {
            if (currentStart > currentEnd || previousStart > previousEnd)
            {
                return new JsonResult(new { success = false, message = "日期範圍無效" });
            }

            var comparison = await _statisticsService.GetComparisonAnalysisAsync(currentStart, currentEnd, previousStart, previousEnd);
            
            return new JsonResult(new
            {
                success = true,
                data = comparison
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得比較分析時發生錯誤");
            return new JsonResult(new { success = false, message = "取得比較分析時發生錯誤" });
        }
    }

    /// <summary>
    /// 匯出統計資料
    /// </summary>
    public async Task<IActionResult> OnPostExportStatisticsAsync([FromForm] StatisticsExportFormModel model)
    {
        try
        {
            if (!DateTime.TryParse(model.StartDate, out var startDate) || 
                !DateTime.TryParse(model.EndDate, out var endDate))
            {
                return BadRequest("日期格式無效");
            }

            if (startDate > endDate)
            {
                return BadRequest("開始日期不能晚於結束日期");
            }

            var request = new StatisticsExportRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                Format = model.Format,
                IncludeCharts = model.IncludeCharts,
                IncludeSummary = model.IncludeSummary,
                IncludeDetailedRecords = model.IncludeDetailedRecords,
                IncludeAnalysis = model.IncludeAnalysis ?? new List<string>()
            };

            byte[] fileData;
            string fileName;
            string contentType;

            if (request.Format.ToLower() == "excel")
            {
                fileData = await _statisticsExportService.ExportToExcelAsync(request);
                fileName = $"統計分析報告_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.xlsx";
                contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            }
            else if (request.Format.ToLower() == "pdf")
            {
                fileData = await _statisticsExportService.ExportToPdfAsync(request);
                fileName = $"統計分析報告_{startDate:yyyyMMdd}-{endDate:yyyyMMdd}.pdf";
                contentType = "application/pdf";
            }
            else
            {
                return BadRequest("不支援的匯出格式");
            }

            _logger.LogInformation("成功匯出統計報告，格式：{Format}，期間：{StartDate} ~ {EndDate}", 
                request.Format, startDate, endDate);

            return File(fileData, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出統計報告時發生錯誤");
            return BadRequest("匯出統計報告時發生錯誤，請稍後再試");
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
                    try
                    {
                        fileData = GeneratePdfExport(records, exportOptions);
                        fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "PDF 匯出失敗，回退到 CSV 格式");
                        fileData = GenerateCsvExport(records);
                        fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        contentType = "text/csv";
                    }
                    break;

                case "excel":
                    try
                    {
                        fileData = await GenerateExcelExport(records, exportOptions);
                        fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Excel 匯出失敗，回退到 CSV 格式");
                        fileData = GenerateCsvExport(records);
                        fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                        contentType = "text/csv";
                    }
                    break;

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
    /// 產生 PDF 匯出檔案
    /// </summary>
    private byte[] GeneratePdfExport(List<AccountingRecord> records, ExportOptions options)
    {
        try
        {
            var htmlReport = GenerateAccountingHtmlReport(records, options);
            var pdfBytes = PdfExportUtility.ConvertHtmlToPdfWithChineseSupport(htmlReport, _logger);
            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "產生 PDF 匯出時發生錯誤");
            throw;
        }
    }

    /// <summary>
    /// 產生記帳報表的 HTML 內容
    /// </summary>
    private string GenerateAccountingHtmlReport(List<AccountingRecord> records, ExportOptions options)
    {
        var totalIncome = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
        var totalExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
        var netIncome = totalIncome - totalExpense;
        var incomeCount = records.Count(r => r.Type == "Income");
        var expenseCount = records.Count(r => r.Type == "Expense");

        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{options.Title}</title>
    <style>{PdfExportUtility.GetChineseSupportedCss()}</style>
    <style>
        .summary-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        .summary-table th, .summary-table td {{
            border: 1px solid #ddd;
            padding: 8px;
            text-align: left;
        }}
        .summary-table th {{
            background-color: #f2f2f2;
            font-weight: bold;
        }}
        .detail-table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            font-size: 12px;
        }}
        .detail-table th, .detail-table td {{
            border: 1px solid #ddd;
            padding: 6px;
            text-align: left;
        }}
        .detail-table th {{
            background-color: #f8f9fa;
            font-weight: bold;
        }}
        .income {{ color: #28a745; }}
        .expense {{ color: #dc3545; }}
        .amount {{ text-align: right; }}
        .page-break {{ page-break-before: always; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{options.Title}</h1>
        <p>報表期間：{options.StartDate:yyyy-MM-dd} ~ {options.EndDate:yyyy-MM-dd}</p>
        <p>匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
    </div>
    
    <h2>財務摘要</h2>
    <table class='summary-table'>
        <tr>
            <th>項目</th>
            <th>金額 (NT$)</th>
            <th>筆數</th>
            <th>平均 (NT$)</th>
        </tr>
        <tr>
            <td class='income'>總收入</td>
            <td class='amount income'>{totalIncome:N0}</td>
            <td>{incomeCount}</td>
            <td class='amount income'>{(incomeCount > 0 ? totalIncome / incomeCount : 0):N0}</td>
        </tr>
        <tr>
            <td class='expense'>總支出</td>
            <td class='amount expense'>{totalExpense:N0}</td>
            <td>{expenseCount}</td>
            <td class='amount expense'>{(expenseCount > 0 ? totalExpense / expenseCount : 0):N0}</td>
        </tr>
        <tr style='font-weight: bold; background-color: #f8f9fa;'>
            <td>淨收支</td>
            <td class='amount {(netIncome >= 0 ? "income" : "expense")}'>{netIncome:N0}</td>
            <td>{records.Count}</td>
            <td class='amount'>{(records.Count > 0 ? Math.Abs(netIncome) / records.Count : 0):N0}</td>
        </tr>
    </table>";

        // 分類分析
        var incomeByCategory = records
            .Where(r => r.Type == "Income")
            .GroupBy(r => r.Category ?? "未分類")
            .Select(g => new { Category = g.Key, Amount = g.Sum(r => r.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var expenseByCategory = records
            .Where(r => r.Type == "Expense")
            .GroupBy(r => r.Category ?? "未分類")
            .Select(g => new { Category = g.Key, Amount = g.Sum(r => r.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Amount)
            .ToList();

        if (incomeByCategory.Any())
        {
            html += @"
    <h2>收入分類分析</h2>
    <table class='summary-table'>
        <tr>
            <th>分類</th>
            <th>金額 (NT$)</th>
            <th>筆數</th>
            <th>占比</th>
        </tr>";

            foreach (var item in incomeByCategory)
            {
                var percentage = totalIncome > 0 ? (item.Amount / totalIncome * 100) : 0;
                html += $@"
        <tr>
            <td>{item.Category}</td>
            <td class='amount income'>{item.Amount:N0}</td>
            <td>{item.Count}</td>
            <td>{percentage:F1}%</td>
        </tr>";
            }
            html += "</table>";
        }

        if (expenseByCategory.Any())
        {
            html += @"
    <h2>支出分類分析</h2>
    <table class='summary-table'>
        <tr>
            <th>分類</th>
            <th>金額 (NT$)</th>
            <th>筆數</th>
            <th>占比</th>
        </tr>";

            foreach (var item in expenseByCategory)
            {
                var percentage = totalExpense > 0 ? (item.Amount / totalExpense * 100) : 0;
                html += $@"
        <tr>
            <td>{item.Category}</td>
            <td class='amount expense'>{item.Amount:N0}</td>
            <td>{item.Count}</td>
            <td>{percentage:F1}%</td>
        </tr>";
            }
            html += "</table>";
        }

        // 詳細記錄
        html += @"
    <div class='page-break'>
        <h2>詳細記錄</h2>
        <table class='detail-table'>
            <tr>
                <th>日期</th>
                <th>類型</th>
                <th>大分類</th>
                <th>細分類</th>
                <th>金額 (NT$)</th>
                <th>付款方式</th>
                <th>備註</th>
            </tr>";

        foreach (var record in records.OrderBy(r => r.Date))
        {
            var typeClass = record.Type == "Income" ? "income" : "expense";
            var typeText = record.Type == "Income" ? "收入" : "支出";
            
            html += $@"
            <tr>
                <td>{record.Date:yyyy-MM-dd}</td>
                <td class='{typeClass}'>{typeText}</td>
                <td>{record.Category ?? ""}</td>
                <td>{record.SubCategory ?? ""}</td>
                <td class='amount {typeClass}'>{record.Amount:N0}</td>
                <td>{record.PaymentMethod ?? ""}</td>
                <td>{record.Note ?? ""}</td>
            </tr>";
        }

        html += @"
        </table>
    </div>
</body>
</html>";

        return html;
    }

    /// <summary>
    /// 產生 Excel 匯出檔案
    /// </summary>
    private Task<byte[]> GenerateExcelExport(List<AccountingRecord> records, ExportOptions options)
    {
        try
        {
            using (var workbook = new ClosedXML.Excel.XLWorkbook())
            {
                // 建立統計摘要工作表
                var summarySheet = workbook.Worksheets.Add("統計摘要");
                CreateSummarySheet(summarySheet, records, options);
                
                // 建立詳細記錄工作表
                var detailSheet = workbook.Worksheets.Add("詳細記錄");
                CreateDetailSheet(detailSheet, records);
                
                // 建立分類分析工作表
                var analysisSheet = workbook.Worksheets.Add("分類分析");
                CreateAnalysisSheet(analysisSheet, records);
                
                // 匯出為 byte array
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return Task.FromResult(stream.ToArray());
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "產生 Excel 匯出時發生錯誤");
            throw;
        }
    }

    /// <summary>
    /// 建立統計摘要工作表
    /// </summary>
    private void CreateSummarySheet(ClosedXML.Excel.IXLWorksheet sheet, List<AccountingRecord> records, ExportOptions options)
    {
        // 設定工作表標題
        sheet.Cell("A1").Value = "記帳統計摘要";
        sheet.Cell("A1").Style.Font.FontSize = 16;
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        sheet.Range("A1:D1").Merge();

        // 基本資訊
        sheet.Cell("A3").Value = "報表期間：";
        sheet.Cell("B3").Value = $"{options.StartDate:yyyy-MM-dd} ~ {options.EndDate:yyyy-MM-dd}";
        
        sheet.Cell("A4").Value = "匯出時間：";
        sheet.Cell("B4").Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        sheet.Cell("A5").Value = "記錄總數：";
        sheet.Cell("B5").Value = records.Count;

        // 統計數據
        var totalIncome = records.Where(r => r.Type == "Income").Sum(r => r.Amount);
        var totalExpense = records.Where(r => r.Type == "Expense").Sum(r => r.Amount);
        var netIncome = totalIncome - totalExpense;
        var incomeCount = records.Count(r => r.Type == "Income");
        var expenseCount = records.Count(r => r.Type == "Expense");

        sheet.Cell("A7").Value = "財務統計";
        sheet.Cell("A7").Style.Font.Bold = true;
        sheet.Cell("A7").Style.Font.FontSize = 14;
        
        sheet.Cell("A9").Value = "總收入：";
        sheet.Cell("B9").Value = totalIncome;
        sheet.Cell("B9").Style.NumberFormat.Format = "#,##0";
        sheet.Cell("C9").Value = $"({incomeCount} 筆記錄)";
        
        sheet.Cell("A10").Value = "總支出：";
        sheet.Cell("B10").Value = totalExpense;
        sheet.Cell("B10").Style.NumberFormat.Format = "#,##0";
        sheet.Cell("C10").Value = $"({expenseCount} 筆記錄)";
        
        sheet.Cell("A11").Value = "淨收支：";
        sheet.Cell("B11").Value = netIncome;
        sheet.Cell("B11").Style.NumberFormat.Format = "#,##0";
        
        // 設定樣式
        sheet.Columns().AdjustToContents();
        sheet.Column("B").Width = 15;
    }

    /// <summary>
    /// 建立詳細記錄工作表
    /// </summary>
    private void CreateDetailSheet(ClosedXML.Excel.IXLWorksheet sheet, List<AccountingRecord> records)
    {
        var headers = new[] { "日期", "類型", "大分類", "細分類", "金額", "付款方式", "備註" };
        
        // 設定標題列
        for (int i = 0; i < headers.Length; i++)
        {
            sheet.Cell(1, i + 1).Value = headers[i];
            sheet.Cell(1, i + 1).Style.Font.Bold = true;
            sheet.Cell(1, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightBlue;
            sheet.Cell(1, i + 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        }
        
        // 填入資料
        for (int i = 0; i < records.Count; i++)
        {
            var record = records.OrderBy(r => r.Date).ElementAt(i);
            var row = i + 2; // 從第二列開始（第一列是標題）
            
            sheet.Cell(row, 1).Value = record.Date.ToString("yyyy-MM-dd");
            sheet.Cell(row, 2).Value = record.Type == "Income" ? "收入" : "支出";
            sheet.Cell(row, 3).Value = record.Category ?? "";
            sheet.Cell(row, 4).Value = record.SubCategory ?? "";
            sheet.Cell(row, 5).Value = record.Amount;
            sheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0";
            sheet.Cell(row, 6).Value = record.PaymentMethod ?? "";
            sheet.Cell(row, 7).Value = record.Note ?? "";
            
            // 收入和支出用不同顏色標示
            if (record.Type == "Income")
            {
                sheet.Cell(row, 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.Green;
            }
            else
            {
                sheet.Cell(row, 2).Style.Font.FontColor = ClosedXML.Excel.XLColor.Red;
            }
        }
        
        // 調整欄寬
        sheet.Columns().AdjustToContents();
        
        // 新增篩選功能
        var dataRange = sheet.Range(1, 1, records.Count + 1, headers.Length);
        dataRange.SetAutoFilter();
    }

    /// <summary>
    /// 建立分類分析工作表
    /// </summary>
    private void CreateAnalysisSheet(ClosedXML.Excel.IXLWorksheet sheet, List<AccountingRecord> records)
    {
        // 收入分析
        var incomeByCategory = records
            .Where(r => r.Type == "Income")
            .GroupBy(r => r.Category ?? "未分類")
            .Select(g => new { Category = g.Key, Amount = g.Sum(r => r.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Amount)
            .ToList();

        sheet.Cell("A1").Value = "收入分析";
        sheet.Cell("A1").Style.Font.FontSize = 14;
        sheet.Cell("A1").Style.Font.Bold = true;
        
        sheet.Cell("A3").Value = "分類";
        sheet.Cell("B3").Value = "金額";
        sheet.Cell("C3").Value = "筆數";
        sheet.Cell("D3").Value = "平均";
        
        // 設定標題樣式
        for (int col = 1; col <= 4; col++)
        {
            sheet.Cell(3, col).Style.Font.Bold = true;
            sheet.Cell(3, col).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGreen;
        }
        
        // 填入收入分析資料
        for (int i = 0; i < incomeByCategory.Count; i++)
        {
            var item = incomeByCategory[i];
            var row = i + 4;
            
            sheet.Cell(row, 1).Value = item.Category;
            sheet.Cell(row, 2).Value = item.Amount;
            sheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            sheet.Cell(row, 3).Value = item.Count;
            sheet.Cell(row, 4).Value = item.Count > 0 ? item.Amount / item.Count : 0;
            sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
        }

        // 支出分析
        var expenseByCategory = records
            .Where(r => r.Type == "Expense")
            .GroupBy(r => r.Category ?? "未分類")
            .Select(g => new { Category = g.Key, Amount = g.Sum(r => r.Amount), Count = g.Count() })
            .OrderByDescending(x => x.Amount)
            .ToList();

        var startRow = incomeByCategory.Count + 6;
        
        sheet.Cell(startRow, 1).Value = "支出分析";
        sheet.Cell(startRow, 1).Style.Font.FontSize = 14;
        sheet.Cell(startRow, 1).Style.Font.Bold = true;
        
        sheet.Cell(startRow + 2, 1).Value = "分類";
        sheet.Cell(startRow + 2, 2).Value = "金額";
        sheet.Cell(startRow + 2, 3).Value = "筆數";
        sheet.Cell(startRow + 2, 4).Value = "平均";
        
        // 設定標題樣式
        for (int col = 1; col <= 4; col++)
        {
            sheet.Cell(startRow + 2, col).Style.Font.Bold = true;
            sheet.Cell(startRow + 2, col).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightCoral;
        }
        
        // 填入支出分析資料
        for (int i = 0; i < expenseByCategory.Count; i++)
        {
            var item = expenseByCategory[i];
            var row = startRow + 3 + i;
            
            sheet.Cell(row, 1).Value = item.Category;
            sheet.Cell(row, 2).Value = item.Amount;
            sheet.Cell(row, 2).Style.NumberFormat.Format = "#,##0";
            sheet.Cell(row, 3).Value = item.Count;
            sheet.Cell(row, 4).Value = item.Count > 0 ? item.Amount / item.Count : 0;
            sheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0";
        }
        
        // 調整欄寬
        sheet.Columns().AdjustToContents();
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
            var escapedCategory = EscapeCsvField(record.Category ?? "");
            var escapedSubCategory = EscapeCsvField(record.SubCategory ?? "");
            var escapedPaymentMethod = EscapeCsvField(record.PaymentMethod ?? "");
            var escapedNote = EscapeCsvField(record.Note ?? "");
            
            csv.AppendLine($"{record.Date:yyyy-MM-dd}," +
                          $"\"{type}\"," +
                          $"\"{escapedCategory}\"," +
                          $"\"{escapedSubCategory}\"," +
                          $"{record.Amount}," +  // 移除千分位符號，避免Excel誤判
                          $"\"{escapedPaymentMethod}\"," +
                          $"\"{escapedNote}\"");
        }
        
        // 產生 UTF-8 with BOM 編碼的位元組陣列，確保Excel正確識別中文編碼
        var encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return encoding.GetBytes(csv.ToString());
    }

    /// <summary>
    /// 處理 CSV 欄位中的特殊字元
    /// </summary>
    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;
            
        // 處理引號：將 " 替換為 ""
        return field.Replace("\"", "\"\"");
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

/// <summary>
/// 統計匯出表單模型
/// </summary>
public class StatisticsExportFormModel
{
    /// <summary>
    /// 開始日期
    /// </summary>
    public string StartDate { get; set; } = string.Empty;
    
    /// <summary>
    /// 結束日期
    /// </summary>
    public string EndDate { get; set; } = string.Empty;
    
    /// <summary>
    /// 匯出格式
    /// </summary>
    public string Format { get; set; } = string.Empty;
    
    /// <summary>
    /// 是否包含圖表
    /// </summary>
    public bool IncludeCharts { get; set; }
    
    /// <summary>
    /// 是否包含摘要
    /// </summary>
    public bool IncludeSummary { get; set; }
    
    /// <summary>
    /// 是否包含詳細記錄
    /// </summary>
    public bool IncludeDetailedRecords { get; set; }
    
    /// <summary>
    /// 包含的分析類型
    /// </summary>
    public List<string>? IncludeAnalysis { get; set; }
}
