using iText.Html2pdf;
using iText.Html2pdf.Resolver.Font;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Layout.Borders;
using Demo.Models;
using Newtonsoft.Json;
using System.Text;

namespace Demo.Services;

/// <summary>
/// PDF 匯出服務
/// 使用 iText 7 函式庫處理 PDF 匯出，特別處理中文字型以避免亂碼
/// </summary>
public class PdfExportService
{
    private readonly ILogger<PdfExportService> _logger;
    private readonly string _templatesPath;

    public PdfExportService(ILogger<PdfExportService> logger)
    {
        _logger = logger;
        _templatesPath = Path.Combine("App_Data", "export-templates", "pdf-templates");
    }

    /// <summary>
    /// 匯出資料為 PDF 格式
    /// </summary>
    /// <param name="data">要匯出的資料</param>
    /// <param name="request">匯出請求</param>
    /// <returns>匯出結果</returns>
    public async Task<ExportResult> ExportToPdfAsync(
        Dictionary<string, object> data, 
        ExportRequest request)
    {
        var result = new ExportResult
        {
            CreatedAt = DateTime.Now,
            Status = ExportStatus.Processing
        };

        try
        {
            var fileName = GenerateFileName("report", "pdf");
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            
            // 確保匯出目錄存在
            var exportDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            using var writer = new PdfWriter(filePath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            // 設定中文字型以避免亂碼
            await SetupChineseFontAsync(document);
            
            // 添加標題頁
            await AddTitlePageAsync(document, request);
            
            // 根據資料類型添加內容
            foreach (var dataType in request.DataTypes)
            {
                if (data.ContainsKey(dataType))
                {
                    await AddDataSectionAsync(document, dataType, data[dataType]);
                }
            }
            
            // 添加摘要頁
            await AddSummaryPageAsync(document, data, request);

            var fileInfo = new FileInfo(filePath);
            result.FileName = fileName;
            result.FilePath = filePath;
            result.FileSize = fileInfo.Length;
            result.ContentType = "application/pdf";
            result.Status = ExportStatus.Completed;
            result.CompletedAt = DateTime.Now;

            // 設定元資料
            result.Metadata = new ExportMetadata
            {
                DataTypes = request.DataTypes,
                Format = "pdf",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TemplateName = request.TemplateName
            };

            _logger.LogInformation("PDF 匯出完成: {FileName}, 大小: {FileSize} bytes", 
                fileName, result.FileSize);

            return result;
        }
        catch (Exception ex)
        {
            result.Status = ExportStatus.Failed;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "PDF 匯出失敗: {Error}", ex.Message);
            return result;
        }
    }

    /// <summary>
    /// 設定中文字型
    /// 嘗試載入系統中可用的中文字型，以避免中文亂碼問題
    /// </summary>
    private async Task SetupChineseFontAsync(Document document)
    {
        try
        {
            PdfFont? chineseFont = null;
            
            // 嘗試載入系統中文字型
            var fontPaths = new[]
            {
                // macOS 中文字型
                "/System/Library/Fonts/PingFang.ttc",
                "/System/Library/Fonts/Helvetica.ttc",
                "/Library/Fonts/Arial Unicode.ttf",
                
                // Windows 中文字型
                "C:\\Windows\\Fonts\\msyh.ttc",      // 微軟雅黑
                "C:\\Windows\\Fonts\\simsun.ttc",    // 宋體
                "C:\\Windows\\Fonts\\simhei.ttf",    // 黑體
                
                // Linux 中文字型
                "/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf",
                "/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf"
            };

            foreach (var fontPath in fontPaths)
            {
                try
                {
                    if (File.Exists(fontPath))
                    {
                        chineseFont = PdfFontFactory.CreateFont(fontPath, "Identity-H");
                        _logger.LogInformation("成功載入中文字型: {FontPath}", fontPath);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "無法載入字型 {FontPath}: {Error}", fontPath, ex.Message);
                }
            }

            // 如果無法載入系統字型，使用內建字型
            if (chineseFont == null)
            {
                _logger.LogWarning("無法載入中文字型，使用預設字型");
                chineseFont = PdfFontFactory.CreateFont();
            }

            // 設定預設字型
            document.SetFont(chineseFont);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "設定中文字型時發生錯誤: {Error}", ex.Message);
            // 繼續使用預設字型
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加標題頁
    /// </summary>
    private async Task AddTitlePageAsync(Document document, ExportRequest request)
    {
        // 添加標題
        var title = new Paragraph("個人管理系統 - 資料報表")
            .SetTextAlignment(TextAlignment.CENTER)
            .SetFontSize(24)
            .SetBold()
            .SetFontColor(ColorConstants.DARK_GRAY)
            .SetMarginBottom(20);
        document.Add(title);
        
        // 添加生成資訊
        var infoTable = new Table(2).UseAllAvailableWidth();
        infoTable.SetBorder(Border.NO_BORDER);
        
        AddInfoRow(infoTable, "生成時間：", DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss"));
        
        var dateRange = request.StartDate.HasValue && request.EndDate.HasValue 
            ? $"{request.StartDate:yyyy-MM-dd} 至 {request.EndDate:yyyy-MM-dd}"
            : "全部資料";
        AddInfoRow(infoTable, "資料範圍：", dateRange);
        
        AddInfoRow(infoTable, "資料類型：", string.Join("、", request.DataTypes.Select(GetDataTypeDisplayName)));
        AddInfoRow(infoTable, "報表格式：", "PDF");
        
        document.Add(infoTable);
        
        // 換頁
        document.Add(new AreaBreak());

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加資訊行到表格
    /// </summary>
    private static void AddInfoRow(Table table, string label, string value)
    {
        table.AddCell(new Cell().Add(new Paragraph(label))
            .SetBorder(Border.NO_BORDER)
            .SetBold()
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetPaddingRight(10));
            
        table.AddCell(new Cell().Add(new Paragraph(value))
            .SetBorder(Border.NO_BORDER)
            .SetPaddingLeft(10));
    }

    /// <summary>
    /// 添加資料章節
    /// </summary>
    private async Task AddDataSectionAsync(Document document, string dataType, object data)
    {
        switch (dataType.ToLower())
        {
            case "accounting":
                await AddAccountingSectionAsync(document, data);
                break;
            case "habits":
                await AddHabitsSectionAsync(document, data);
                break;
            case "notes":
                await AddNotesSectionAsync(document, data);
                break;
            case "todo":
                await AddTodoSectionAsync(document, data);
                break;
            default:
                await AddGenericSectionAsync(document, dataType, data);
                break;
        }
    }

    /// <summary>
    /// 添加記帳資料章節
    /// </summary>
    private async Task AddAccountingSectionAsync(Document document, object data)
    {
        var accountingData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        
        // 添加章節標題
        var sectionTitle = new Paragraph("記帳資料分析")
            .SetFontSize(18)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetMarginBottom(15);
        document.Add(sectionTitle);
        
        // 添加摘要統計
        var summary = accountingData?.Summary;
        if (summary != null)
        {
            var summaryTable = new Table(2).UseAllAvailableWidth();
            summaryTable.SetBackgroundColor(ColorConstants.LIGHT_GRAY);
            
            var totalIncome = (decimal)(summary?.TotalIncome ?? 0);
            var totalExpense = (decimal)(summary?.TotalExpense ?? 0);
            var netIncome = totalIncome - totalExpense;
            var totalRecords = (int)(summary?.TotalRecords ?? 0);
            
            AddSummaryRow(summaryTable, "總收入：", $"${totalIncome:N2}");
            AddSummaryRow(summaryTable, "總支出：", $"${totalExpense:N2}");
            AddSummaryRow(summaryTable, "淨收入：", $"${netIncome:N2}");
            AddSummaryRow(summaryTable, "記錄筆數：", totalRecords.ToString());
            
            document.Add(summaryTable);
            document.Add(new Paragraph("\n"));
        }
        
        // 添加詳細記錄
        var records = accountingData?.Records;
        if (records != null)
        {
            var detailTitle = new Paragraph("詳細記錄")
                .SetFontSize(14)
                .SetBold()
                .SetMarginTop(20)
                .SetMarginBottom(10);
            document.Add(detailTitle);
            
            var recordsTable = new Table(5).UseAllAvailableWidth();
            
            // 添加表頭
            AddTableHeader(recordsTable, "日期", "描述", "分類", "類型", "金額");
            
            // 添加記錄
            foreach (var record in records)
            {
                var date = DateTime.Parse(record?.Date?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd");
                var description = record?.Description?.ToString() ?? "";
                var categoryName = record?.CategoryName?.ToString() ?? "";
                var type = record?.Type?.ToString() == "income" ? "收入" : "支出";
                var amount = $"${(decimal)(record?.Amount ?? 0):N2}";
                
                AddTableRow(recordsTable, date, description, categoryName, type, amount);
            }
            
            document.Add(recordsTable);
        }
        
        // 換頁
        document.Add(new AreaBreak());

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加習慣追蹤章節
    /// </summary>
    private async Task AddHabitsSectionAsync(Document document, object data)
    {
        var habitsData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        
        var sectionTitle = new Paragraph("習慣追蹤記錄")
            .SetFontSize(18)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetMarginBottom(15);
        document.Add(sectionTitle);
        
        var records = habitsData?.Records;
        if (records != null)
        {
            var habitsTable = new Table(4).UseAllAvailableWidth();
            AddTableHeader(habitsTable, "日期", "習慣名稱", "完成狀態", "備註");
            
            foreach (var record in records)
            {
                var date = DateTime.Parse(record?.Date?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd");
                var habitName = record?.HabitName?.ToString() ?? "";
                var isCompleted = record?.IsCompleted?.ToString() == "True" ? "已完成" : "未完成";
                var note = record?.Note?.ToString() ?? "";
                
                AddTableRow(habitsTable, date, habitName, isCompleted, note);
            }
            
            document.Add(habitsTable);
        }
        
        document.Add(new AreaBreak());

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加備忘錄章節
    /// </summary>
    private async Task AddNotesSectionAsync(Document document, object data)
    {
        var notesData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        
        var sectionTitle = new Paragraph("備忘錄記錄")
            .SetFontSize(18)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetMarginBottom(15);
        document.Add(sectionTitle);
        
        var notes = notesData?.Notes ?? notesData;
        if (notes is IEnumerable<object> noteList)
        {
            foreach (var note in noteList)
            {
                var noteObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(note));
                
                // 備忘錄標題
                var noteTitle = new Paragraph(noteObj?.Title?.ToString() ?? "無標題")
                    .SetFontSize(12)
                    .SetBold()
                    .SetFontColor(ColorConstants.BLACK);
                document.Add(noteTitle);
                
                // 建立時間
                var createdAt = DateTime.Parse(noteObj?.CreatedAt?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                var timeInfo = new Paragraph($"建立時間：{createdAt}")
                    .SetFontSize(10)
                    .SetFontColor(ColorConstants.GRAY)
                    .SetMarginBottom(5);
                document.Add(timeInfo);
                
                // 內容
                var content = noteObj?.Content?.ToString() ?? "";
                if (!string.IsNullOrEmpty(content))
                {
                    var contentPara = new Paragraph(content)
                        .SetMarginBottom(10);
                    document.Add(contentPara);
                }
                
                // 標籤和分類
                var tags = string.Join(", ", noteObj?.Tags ?? new string[0]);
                var category = noteObj?.Category?.ToString() ?? "";
                
                if (!string.IsNullOrEmpty(tags) || !string.IsNullOrEmpty(category))
                {
                    var metaInfo = new Paragraph($"標籤：{tags}　分類：{category}")
                        .SetFontSize(10)
                        .SetFontColor(ColorConstants.BLUE)
                        .SetMarginBottom(15);
                    document.Add(metaInfo);
                }
                
                // 添加分隔線
                document.Add(new Paragraph("─────────────────────────────────────")
                    .SetFontColor(ColorConstants.LIGHT_GRAY)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetMarginBottom(10));
            }
        }
        
        document.Add(new AreaBreak());

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加待辦事項章節
    /// </summary>
    private async Task AddTodoSectionAsync(Document document, object data)
    {
        var todoData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        
        var sectionTitle = new Paragraph("待辦事項記錄")
            .SetFontSize(18)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetMarginBottom(15);
        document.Add(sectionTitle);
        
        var tasks = todoData?.Tasks ?? todoData;
        if (tasks is IEnumerable<object> taskList)
        {
            var todoTable = new Table(6).UseAllAvailableWidth();
            AddTableHeader(todoTable, "建立時間", "標題", "描述", "優先級", "狀態", "到期日");
            
            foreach (var task in taskList)
            {
                var taskObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(task));
                
                var createdAt = DateTime.Parse(taskObj?.CreatedAt?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd");
                var title = taskObj?.Title?.ToString() ?? "";
                var description = taskObj?.Description?.ToString() ?? "";
                var priority = GetPriorityDisplayName(taskObj?.Priority?.ToString() ?? "");
                var status = GetStatusDisplayName(taskObj?.Status?.ToString() ?? "");
                
                // 處理到期日
                var dueDateStr = taskObj?.DueDate?.ToString();
                var dueDate = "";
                if (!string.IsNullOrEmpty(dueDateStr))
                {
                    try
                    {
                        var parsedDate = DateTime.Parse(dueDateStr);
                        dueDate = parsedDate.ToString("yyyy-MM-dd");
                    }
                    catch
                    {
                        dueDate = "";
                    }
                }
                
                AddTableRow(todoTable, createdAt, title, description, priority, status, dueDate);
            }
            
            document.Add(todoTable);
        }
        
        document.Add(new AreaBreak());

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加通用資料章節
    /// </summary>
    private async Task AddGenericSectionAsync(Document document, string dataType, object data)
    {
        var sectionTitle = new Paragraph(GetDataTypeDisplayName(dataType))
            .SetFontSize(18)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetMarginBottom(15);
        document.Add(sectionTitle);
        
        var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        var contentPara = new Paragraph(jsonString)
            .SetFontSize(10)
            .SetFontFamily("monospace");
        document.Add(contentPara);
        
        document.Add(new AreaBreak());

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加摘要頁
    /// </summary>
    private async Task AddSummaryPageAsync(Document document, Dictionary<string, object> data, ExportRequest request)
    {
        var summaryTitle = new Paragraph("匯出摘要")
            .SetFontSize(20)
            .SetBold()
            .SetFontColor(ColorConstants.BLUE)
            .SetTextAlignment(TextAlignment.CENTER)
            .SetMarginBottom(20);
        document.Add(summaryTitle);
        
        // 匯出資訊摘要
        var summaryTable = new Table(2).UseAllAvailableWidth();
        
        AddInfoRow(summaryTable, "匯出完成時間：", DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss"));
        AddInfoRow(summaryTable, "資料類型數量：", data.Count.ToString());
        AddInfoRow(summaryTable, "匯出格式：", "PDF");
        
        // 計算總記錄數
        int totalRecords = 0;
        foreach (var kvp in data)
        {
            try
            {
                var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(kvp.Value));
                if (jsonData?.Records != null)
                {
                    totalRecords += ((IEnumerable<object>)jsonData.Records).Count();
                }
                else if (jsonData is IEnumerable<object> items)
                {
                    totalRecords += items.Count();
                }
            }
            catch
            {
                // 忽略統計錯誤
            }
        }
        
        AddInfoRow(summaryTable, "總記錄數：", totalRecords.ToString());
        
        document.Add(summaryTable);

        await Task.CompletedTask;
    }

    /// <summary>
    /// 添加摘要行到表格
    /// </summary>
    private static void AddSummaryRow(Table table, string label, string value)
    {
        table.AddCell(new Cell().Add(new Paragraph(label))
            .SetBold()
            .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
            .SetTextAlignment(TextAlignment.RIGHT)
            .SetPadding(8));
            
        table.AddCell(new Cell().Add(new Paragraph(value))
            .SetBackgroundColor(ColorConstants.WHITE)
            .SetPadding(8));
    }

    /// <summary>
    /// 添加表格標頭
    /// </summary>
    private static void AddTableHeader(Table table, params string[] headers)
    {
        foreach (var header in headers)
        {
            table.AddHeaderCell(new Cell().Add(new Paragraph(header))
                .SetBold()
                .SetBackgroundColor(ColorConstants.BLUE)
                .SetFontColor(ColorConstants.WHITE)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(8));
        }
    }

    /// <summary>
    /// 添加表格行
    /// </summary>
    private static void AddTableRow(Table table, params string[] values)
    {
        foreach (var value in values)
        {
            table.AddCell(new Cell().Add(new Paragraph(value ?? ""))
                .SetPadding(5)
                .SetBorderBottom(new SolidBorder(ColorConstants.LIGHT_GRAY, 1)));
        }
    }

    /// <summary>
    /// 產生檔案名稱
    /// </summary>
    private static string GenerateFileName(string prefix, string extension)
    {
        return $"{prefix}_{DateTime.Now:yyyyMMdd_HHmmss}.{extension}";
    }

    /// <summary>
    /// 取得資料類型顯示名稱
    /// </summary>
    private static string GetDataTypeDisplayName(string dataType)
    {
        return dataType.ToLower() switch
        {
            "accounting" => "記帳資料",
            "habits" => "習慣追蹤",
            "notes" => "備忘錄",
            "todo" => "待辦事項",
            "calendar" => "日曆事件",
            _ => dataType
        };
    }

    /// <summary>
    /// 取得優先級顯示名稱
    /// </summary>
    private static string GetPriorityDisplayName(string priority)
    {
        return priority.ToLower() switch
        {
            "high" => "高",
            "medium" => "中",
            "low" => "低",
            _ => priority
        };
    }

    /// <summary>
    /// 取得狀態顯示名稱
    /// </summary>
    private static string GetStatusDisplayName(string status)
    {
        return status.ToLower() switch
        {
            "completed" => "已完成",
            "inprogress" => "進行中",
            "pending" => "待處理",
            "cancelled" => "已取消",
            _ => status
        };
    }
}
