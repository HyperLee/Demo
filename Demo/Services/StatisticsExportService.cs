using ClosedXML.Excel;
using Demo.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace Demo.Services;

/// <summary>
/// 統計匯出服務介面
/// </summary>
public interface IStatisticsExportService
{
    /// <summary>
    /// 匯出統計資料為 Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync(StatisticsExportRequest request);

    /// <summary>
    /// 匯出統計資料為 PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync(StatisticsExportRequest request);
}

/// <summary>
/// 統計匯出服務實作
/// </summary>
public class StatisticsExportService : IStatisticsExportService
{
    private readonly IStatisticsService _statisticsService;
    private readonly IAccountingService _accountingService;
    private readonly ILogger<StatisticsExportService> _logger;

    public StatisticsExportService(
        IStatisticsService statisticsService,
        IAccountingService accountingService,
        ILogger<StatisticsExportService> logger)
    {
        _statisticsService = statisticsService;
        _accountingService = accountingService;
        _logger = logger;
    }

    /// <summary>
    /// 匯出統計資料為 Excel
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync(StatisticsExportRequest request)
    {
        try
        {
            using var workbook = new XLWorkbook();
            
            // 建立摘要工作表
            if (request.IncludeSummary)
            {
                await CreateSummaryWorksheetAsync(workbook, request);
            }
            
            // 建立分類分析工作表
            if (request.IncludeAnalysis.Contains("category"))
            {
                await CreateCategoryAnalysisWorksheetAsync(workbook, request);
            }
            
            // 建立時間模式工作表
            if (request.IncludeAnalysis.Contains("pattern"))
            {
                await CreateTimePatternWorksheetAsync(workbook, request);
            }
            
            // 建立排行榜工作表
            if (request.IncludeAnalysis.Contains("ranking"))
            {
                await CreateRankingWorksheetAsync(workbook, request);
            }
            
            // 建立詳細記錄工作表
            if (request.IncludeDetailedRecords)
            {
                await CreateDetailedRecordsWorksheetAsync(workbook, request);
            }
            
            // 轉為位元組陣列
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出 Excel 時發生錯誤");
            throw;
        }
    }

    /// <summary>
    /// 匯出統計資料為 PDF
    /// </summary>
    public async Task<byte[]> ExportToPdfAsync(StatisticsExportRequest request)
    {
        try
        {
            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // 設定字型 - 使用系統預設字型支援中文
            PdfFont font;
            try 
            {
                // 嘗試使用系統字型
                font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }
            catch
            {
                // 如果失敗則使用預設字型
                font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            }

            // 標題
            var title = new Paragraph("Statistics Analysis Report")
                .SetFont(font)
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(title);

            var period = new Paragraph($"Period: {request.StartDate:yyyy/MM/dd} - {request.EndDate:yyyy/MM/dd}")
                .SetFont(font)
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(period);

            document.Add(new Paragraph("\n"));

            // 建立摘要
            if (request.IncludeSummary)
            {
                await CreateSummaryPdfSectionAsync(document, request, font);
            }

            // 分類分析
            if (request.IncludeAnalysis.Contains("category"))
            {
                await CreateCategoryAnalysisPdfSectionAsync(document, request, font);
            }

            // 時間模式分析
            if (request.IncludeAnalysis.Contains("pattern"))
            {
                await CreateTimePatternPdfSectionAsync(document, request, font);
            }

            // 排行榜分析
            if (request.IncludeAnalysis.Contains("ranking"))
            {
                await CreateRankingPdfSectionAsync(document, request, font);
            }

            // 確保文件被正確關閉
            document.Close();
            
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出 PDF 時發生錯誤: {Message}", ex.Message);
            throw new InvalidOperationException($"PDF 匯出失敗: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 建立摘要工作表
    /// </summary>
    private async Task CreateSummaryWorksheetAsync(IXLWorkbook workbook, StatisticsExportRequest request)
    {
        var worksheet = workbook.Worksheets.Add("摘要");
        var summary = await _statisticsService.GetStatisticsSummaryAsync(request.StartDate, request.EndDate);

        // 標題
        worksheet.Cell("A1").Value = "統計摘要";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 16;

        // 資料
        var row = 3;
        worksheet.Cell($"A{row}").Value = "總收入";
        worksheet.Cell($"B{row}").Value = summary.TotalIncome;
        row++;

        worksheet.Cell($"A{row}").Value = "總支出";
        worksheet.Cell($"B{row}").Value = summary.TotalExpense;
        row++;

        worksheet.Cell($"A{row}").Value = "淨收支";
        worksheet.Cell($"B{row}").Value = summary.NetIncome;
        row++;

        worksheet.Cell($"A{row}").Value = "總記錄筆數";
        worksheet.Cell($"B{row}").Value = summary.TotalRecords;
        row++;

        worksheet.Cell($"A{row}").Value = "平均月收入";
        worksheet.Cell($"B{row}").Value = summary.AverageMonthlyIncome;
        row++;

        worksheet.Cell($"A{row}").Value = "平均月支出";
        worksheet.Cell($"B{row}").Value = summary.AverageMonthlyExpense;
        row++;

        worksheet.Cell($"A{row}").Value = "最大支出分類";
        worksheet.Cell($"B{row}").Value = summary.TopExpenseCategory;
        row++;

        worksheet.Cell($"A{row}").Value = "最大支出金額";
        worksheet.Cell($"B{row}").Value = summary.TopExpenseAmount;

        // 設定欄寬
        worksheet.Columns().AdjustToContents();
    }

    /// <summary>
    /// 建立分類分析工作表
    /// </summary>
    private async Task CreateCategoryAnalysisWorksheetAsync(IXLWorkbook workbook, StatisticsExportRequest request)
    {
        var worksheet = workbook.Worksheets.Add("分類分析");

        // 支出分類
        var expenseCategories = await _statisticsService.GetExpenseCategoryAnalysisAsync(request.StartDate, request.EndDate);
        
        worksheet.Cell("A1").Value = "支出分類分析";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 14;

        worksheet.Cell("A3").Value = "分類";
        worksheet.Cell("B3").Value = "子分類";
        worksheet.Cell("C3").Value = "金額";
        worksheet.Cell("D3").Value = "百分比";
        worksheet.Cell("E3").Value = "記錄筆數";

        var row = 4;
        foreach (var category in expenseCategories)
        {
            worksheet.Cell($"A{row}").Value = category.Category;
            worksheet.Cell($"B{row}").Value = category.SubCategory;
            worksheet.Cell($"C{row}").Value = category.Amount;
            worksheet.Cell($"D{row}").Value = $"{category.Percentage:F2}%";
            worksheet.Cell($"E{row}").Value = category.RecordCount;
            row++;
        }

        // 收入分類
        var incomeCategories = await _statisticsService.GetIncomeCategoryAnalysisAsync(request.StartDate, request.EndDate);
        
        row += 2;
        worksheet.Cell($"A{row}").Value = "收入分類分析";
        worksheet.Cell($"A{row}").Style.Font.Bold = true;
        worksheet.Cell($"A{row}").Style.Font.FontSize = 14;

        row += 2;
        worksheet.Cell($"A{row}").Value = "分類";
        worksheet.Cell($"B{row}").Value = "子分類";
        worksheet.Cell($"C{row}").Value = "金額";
        worksheet.Cell($"D{row}").Value = "百分比";
        worksheet.Cell($"E{row}").Value = "記錄筆數";

        row++;
        foreach (var category in incomeCategories)
        {
            worksheet.Cell($"A{row}").Value = category.Category;
            worksheet.Cell($"B{row}").Value = category.SubCategory;
            worksheet.Cell($"C{row}").Value = category.Amount;
            worksheet.Cell($"D{row}").Value = $"{category.Percentage:F2}%";
            worksheet.Cell($"E{row}").Value = category.RecordCount;
            row++;
        }

        worksheet.Columns().AdjustToContents();
    }

    /// <summary>
    /// 建立時間模式工作表
    /// </summary>
    private async Task CreateTimePatternWorksheetAsync(IXLWorkbook workbook, StatisticsExportRequest request)
    {
        var worksheet = workbook.Worksheets.Add("時間模式");
        var timePattern = await _statisticsService.GetTimePatternAnalysisAsync(request.StartDate, request.EndDate);

        worksheet.Cell("A1").Value = "時間模式分析";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 14;

        // 週日模式
        worksheet.Cell("A3").Value = "週日模式";
        worksheet.Cell("A3").Style.Font.Bold = true;

        worksheet.Cell("A5").Value = "星期";
        worksheet.Cell("B5").Value = "平均收入";
        worksheet.Cell("C5").Value = "平均支出";
        worksheet.Cell("D5").Value = "記錄筆數";
        worksheet.Cell("E5").Value = "熱門分類";

        var row = 6;
        foreach (var pattern in timePattern.WeekdayPatterns)
        {
            worksheet.Cell($"A{row}").Value = pattern.Weekday;
            worksheet.Cell($"B{row}").Value = pattern.AverageIncome;
            worksheet.Cell($"C{row}").Value = pattern.AverageExpense;
            worksheet.Cell($"D{row}").Value = pattern.RecordCount;
            worksheet.Cell($"E{row}").Value = string.Join(", ", pattern.PopularCategories);
            row++;
        }

        // 月內模式
        row += 2;
        worksheet.Cell($"A{row}").Value = "月內模式";
        worksheet.Cell($"A{row}").Style.Font.Bold = true;

        row += 2;
        worksheet.Cell($"A{row}").Value = "期間";
        worksheet.Cell($"B{row}").Value = "平均收入";
        worksheet.Cell($"C{row}").Value = "平均支出";
        worksheet.Cell($"D{row}").Value = "記錄筆數";
        worksheet.Cell($"E{row}").Value = "佔比";

        row++;
        foreach (var pattern in timePattern.MonthlyPatterns)
        {
            worksheet.Cell($"A{row}").Value = pattern.Period;
            worksheet.Cell($"B{row}").Value = pattern.AverageIncome;
            worksheet.Cell($"C{row}").Value = pattern.AverageExpense;
            worksheet.Cell($"D{row}").Value = pattern.RecordCount;
            worksheet.Cell($"E{row}").Value = $"{pattern.Percentage:F2}%";
            row++;
        }

        worksheet.Columns().AdjustToContents();
    }

    /// <summary>
    /// 建立排行榜工作表
    /// </summary>
    private async Task CreateRankingWorksheetAsync(IXLWorkbook workbook, StatisticsExportRequest request)
    {
        var worksheet = workbook.Worksheets.Add("排行榜");

        // 支出排行榜
        var expenseRanking = await _statisticsService.GetCategoryRankingAsync(request.StartDate, request.EndDate, "Expense", 10);
        
        worksheet.Cell("A1").Value = "支出分類排行榜";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 14;

        worksheet.Cell("A3").Value = "排名";
        worksheet.Cell("B3").Value = "分類";
        worksheet.Cell("C3").Value = "子分類";
        worksheet.Cell("D3").Value = "金額";
        worksheet.Cell("E3").Value = "筆數";
        worksheet.Cell("F3").Value = "平均金額";
        worksheet.Cell("G3").Value = "變化率";
        worksheet.Cell("H3").Value = "趨勢";

        var row = 4;
        foreach (var ranking in expenseRanking)
        {
            worksheet.Cell($"A{row}").Value = ranking.Rank;
            worksheet.Cell($"B{row}").Value = ranking.Category;
            worksheet.Cell($"C{row}").Value = ranking.SubCategory;
            worksheet.Cell($"D{row}").Value = ranking.Amount;
            worksheet.Cell($"E{row}").Value = ranking.RecordCount;
            worksheet.Cell($"F{row}").Value = ranking.AverageAmount;
            worksheet.Cell($"G{row}").Value = $"{ranking.PercentageChange:F2}%";
            worksheet.Cell($"H{row}").Value = ranking.Trend;
            row++;
        }

        // 收入排行榜
        var incomeRanking = await _statisticsService.GetCategoryRankingAsync(request.StartDate, request.EndDate, "Income", 10);
        
        row += 2;
        worksheet.Cell($"A{row}").Value = "收入分類排行榜";
        worksheet.Cell($"A{row}").Style.Font.Bold = true;
        worksheet.Cell($"A{row}").Style.Font.FontSize = 14;

        row += 2;
        worksheet.Cell($"A{row}").Value = "排名";
        worksheet.Cell($"B{row}").Value = "分類";
        worksheet.Cell($"C{row}").Value = "子分類";
        worksheet.Cell($"D{row}").Value = "金額";
        worksheet.Cell($"E{row}").Value = "筆數";
        worksheet.Cell($"F{row}").Value = "平均金額";
        worksheet.Cell($"G{row}").Value = "變化率";
        worksheet.Cell($"H{row}").Value = "趨勢";

        row++;
        foreach (var ranking in incomeRanking)
        {
            worksheet.Cell($"A{row}").Value = ranking.Rank;
            worksheet.Cell($"B{row}").Value = ranking.Category;
            worksheet.Cell($"C{row}").Value = ranking.SubCategory;
            worksheet.Cell($"D{row}").Value = ranking.Amount;
            worksheet.Cell($"E{row}").Value = ranking.RecordCount;
            worksheet.Cell($"F{row}").Value = ranking.AverageAmount;
            worksheet.Cell($"G{row}").Value = $"{ranking.PercentageChange:F2}%";
            worksheet.Cell($"H{row}").Value = ranking.Trend;
            row++;
        }

        worksheet.Columns().AdjustToContents();
    }

    /// <summary>
    /// 建立詳細記錄工作表
    /// </summary>
    private async Task CreateDetailedRecordsWorksheetAsync(IXLWorkbook workbook, StatisticsExportRequest request)
    {
        var worksheet = workbook.Worksheets.Add("詳細記錄");
        var records = await _accountingService.GetRecordsAsync(request.StartDate, request.EndDate);

        worksheet.Cell("A1").Value = "詳細記錄";
        worksheet.Cell("A1").Style.Font.Bold = true;
        worksheet.Cell("A1").Style.Font.FontSize = 14;

        worksheet.Cell("A3").Value = "日期";
        worksheet.Cell("B3").Value = "類型";
        worksheet.Cell("C3").Value = "分類";
        worksheet.Cell("D3").Value = "子分類";
        worksheet.Cell("E3").Value = "金額";
        worksheet.Cell("F3").Value = "備註";

        var row = 4;
        foreach (var record in records.OrderByDescending(r => r.Date))
        {
            worksheet.Cell($"A{row}").Value = record.Date.ToString("yyyy/MM/dd");
            worksheet.Cell($"B{row}").Value = record.Type;
            worksheet.Cell($"C{row}").Value = record.Category ?? "";
            worksheet.Cell($"D{row}").Value = record.SubCategory ?? "";
            worksheet.Cell($"E{row}").Value = record.Amount;
            worksheet.Cell($"F{row}").Value = record.Note ?? "";
            row++;
        }

        worksheet.Columns().AdjustToContents();
    }

    /// <summary>
    /// 建立摘要 PDF 區段
    /// </summary>
    private async Task CreateSummaryPdfSectionAsync(Document document, StatisticsExportRequest request, PdfFont font)
    {
        var summary = await _statisticsService.GetStatisticsSummaryAsync(request.StartDate, request.EndDate);

        var sectionTitle = new Paragraph("Summary")
            .SetFont(font)
            .SetFontSize(16)
            .SetBold();
        document.Add(sectionTitle);

        var summaryTable = new Table(2)
            .SetWidth(UnitValue.CreatePercentValue(100))
            .SetFont(font);

        summaryTable.AddHeaderCell(new Cell().Add(new Paragraph("Item").SetFont(font).SetBold()));
        summaryTable.AddHeaderCell(new Cell().Add(new Paragraph("Value").SetFont(font).SetBold()));

        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Income").SetFont(font)));
        summaryTable.AddCell(new Cell().Add(new Paragraph($"${summary.TotalIncome:N0}").SetFont(font)));

        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Expense").SetFont(font)));
        summaryTable.AddCell(new Cell().Add(new Paragraph($"${summary.TotalExpense:N0}").SetFont(font)));

        summaryTable.AddCell(new Cell().Add(new Paragraph("Net Income").SetFont(font)));
        summaryTable.AddCell(new Cell().Add(new Paragraph($"${summary.NetIncome:N0}").SetFont(font)));

        summaryTable.AddCell(new Cell().Add(new Paragraph("Total Records").SetFont(font)));
        summaryTable.AddCell(new Cell().Add(new Paragraph(summary.TotalRecords.ToString()).SetFont(font)));

        summaryTable.AddCell(new Cell().Add(new Paragraph("Avg Monthly Income").SetFont(font)));
        summaryTable.AddCell(new Cell().Add(new Paragraph($"${summary.AverageMonthlyIncome:N0}").SetFont(font)));

        summaryTable.AddCell(new Cell().Add(new Paragraph("Avg Monthly Expense").SetFont(font)));
        summaryTable.AddCell(new Cell().Add(new Paragraph($"${summary.AverageMonthlyExpense:N0}").SetFont(font)));

        document.Add(summaryTable);
        document.Add(new Paragraph("\n"));
    }

    /// <summary>
    /// 建立分類分析 PDF 區段
    /// </summary>
    private async Task CreateCategoryAnalysisPdfSectionAsync(Document document, StatisticsExportRequest request, PdfFont font)
    {
        var sectionTitle = new Paragraph("Category Analysis")
            .SetFont(font)
            .SetFontSize(16)
            .SetBold();
        document.Add(sectionTitle);

        // 支出分類
        var expenseCategories = await _statisticsService.GetExpenseCategoryAnalysisAsync(request.StartDate, request.EndDate);
        
        var expenseTitle = new Paragraph("Expense Categories")
            .SetFont(font)
            .SetFontSize(14)
            .SetBold();
        document.Add(expenseTitle);

        if (expenseCategories.Any())
        {
            var expenseTable = new Table(4)
                .SetWidth(UnitValue.CreatePercentValue(100))
                .SetFont(font);

            expenseTable.AddHeaderCell(new Cell().Add(new Paragraph("Category").SetFont(font).SetBold()));
            expenseTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount").SetFont(font).SetBold()));
            expenseTable.AddHeaderCell(new Cell().Add(new Paragraph("Percentage").SetFont(font).SetBold()));
            expenseTable.AddHeaderCell(new Cell().Add(new Paragraph("Records").SetFont(font).SetBold()));

            foreach (var category in expenseCategories.Take(10))
            {
                expenseTable.AddCell(new Cell().Add(new Paragraph(category.Category ?? "Other").SetFont(font)));
                expenseTable.AddCell(new Cell().Add(new Paragraph($"${category.Amount:N0}").SetFont(font)));
                expenseTable.AddCell(new Cell().Add(new Paragraph($"{category.Percentage:F1}%").SetFont(font)));
                expenseTable.AddCell(new Cell().Add(new Paragraph(category.RecordCount.ToString()).SetFont(font)));
            }

            document.Add(expenseTable);
        }
        else
        {
            document.Add(new Paragraph("No expense data available.").SetFont(font));
        }

        document.Add(new Paragraph("\n"));
    }

    /// <summary>
    /// 建立時間模式 PDF 區段
    /// </summary>
    private async Task CreateTimePatternPdfSectionAsync(Document document, StatisticsExportRequest request, PdfFont font)
    {
        try
        {
            var timePattern = await _statisticsService.GetTimePatternAnalysisAsync(request.StartDate, request.EndDate);

            var sectionTitle = new Paragraph("Time Pattern Analysis")
                .SetFont(font)
                .SetFontSize(16)
                .SetBold();
            document.Add(sectionTitle);

            if (timePattern?.DailySummary != null)
            {
                var summaryText = new Paragraph()
                    .SetFont(font)
                    .Add($"Most Active Weekday: {timePattern.DailySummary.MostActiveWeekday}")
                    .Add("\n")
                    .Add($"Highest Expense Weekday: {timePattern.DailySummary.HighestExpenseWeekday}")
                    .Add("\n")
                    .Add($"Most Active Period: {timePattern.DailySummary.MostActivePeriod}")
                    .Add("\n")
                    .Add($"Weekday Average Expense: ${timePattern.DailySummary.WeekdayAverageExpense:N0}")
                    .Add("\n")
                    .Add($"Weekend Average Expense: ${timePattern.DailySummary.WeekendAverageExpense:N0}");

                document.Add(summaryText);
            }
            else
            {
                document.Add(new Paragraph("No time pattern data available.").SetFont(font));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "無法載入時間模式資料");
            document.Add(new Paragraph("Time pattern data unavailable.").SetFont(font));
        }

        document.Add(new Paragraph("\n"));
    }

    /// <summary>
    /// 建立排行榜 PDF 區段
    /// </summary>
    private async Task CreateRankingPdfSectionAsync(Document document, StatisticsExportRequest request, PdfFont font)
    {
        try
        {
            var sectionTitle = new Paragraph("Category Ranking")
                .SetFont(font)
                .SetFontSize(16)
                .SetBold();
            document.Add(sectionTitle);

            var expenseRanking = await _statisticsService.GetCategoryRankingAsync(request.StartDate, request.EndDate, "Expense", 5);

            if (expenseRanking.Any())
            {
                var rankingTable = new Table(4)
                    .SetWidth(UnitValue.CreatePercentValue(100))
                    .SetFont(font);

                rankingTable.AddHeaderCell(new Cell().Add(new Paragraph("Rank").SetFont(font).SetBold()));
                rankingTable.AddHeaderCell(new Cell().Add(new Paragraph("Category").SetFont(font).SetBold()));
                rankingTable.AddHeaderCell(new Cell().Add(new Paragraph("Amount").SetFont(font).SetBold()));
                rankingTable.AddHeaderCell(new Cell().Add(new Paragraph("Trend").SetFont(font).SetBold()));

                foreach (var ranking in expenseRanking)
                {
                    rankingTable.AddCell(new Cell().Add(new Paragraph(ranking.Rank.ToString()).SetFont(font)));
                    rankingTable.AddCell(new Cell().Add(new Paragraph(ranking.Category ?? "Other").SetFont(font)));
                    rankingTable.AddCell(new Cell().Add(new Paragraph($"${ranking.Amount:N0}").SetFont(font)));
                    rankingTable.AddCell(new Cell().Add(new Paragraph(ranking.Trend ?? "stable").SetFont(font)));
                }

                document.Add(rankingTable);
            }
            else
            {
                document.Add(new Paragraph("No ranking data available.").SetFont(font));
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "無法載入排行榜資料");
            document.Add(new Paragraph("Ranking data unavailable.").SetFont(font));
        }

        document.Add(new Paragraph("\n"));
    }
}
