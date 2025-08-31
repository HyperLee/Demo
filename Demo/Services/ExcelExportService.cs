using ClosedXML.Excel;
using Demo.Models;
using Newtonsoft.Json;

namespace Demo.Services;

/// <summary>
/// Excel 匯出服務
/// 使用 ClosedXML 函式庫處理 Excel 檔案匯出，支援中文字元
/// </summary>
public class ExcelExportService
{
    private readonly ILogger<ExcelExportService> _logger;

    public ExcelExportService(ILogger<ExcelExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 匯出資料為 Excel 格式
    /// </summary>
    /// <param name="data">要匯出的資料</param>
    /// <param name="request">匯出請求</param>
    /// <returns>匯出結果</returns>
    public async Task<ExportResult> ExportToExcelAsync(
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
            var fileName = GenerateFileName("export", "xlsx");
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            
            // 確保匯出目錄存在
            var exportDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            using var workbook = new XLWorkbook();
            
            // 為每種資料類型建立工作表
            foreach (var dataType in request.DataTypes)
            {
                if (data.ContainsKey(dataType))
                {
                    await CreateWorksheetAsync(workbook, dataType, data[dataType]);
                }
            }
            
            // 添加摘要工作表
            await CreateSummaryWorksheetAsync(workbook, data, request);
            
            workbook.SaveAs(filePath);

            var fileInfo = new FileInfo(filePath);
            result.FileName = fileName;
            result.FilePath = filePath;
            result.FileSize = fileInfo.Length;
            result.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            result.Status = ExportStatus.Completed;
            result.CompletedAt = DateTime.Now;

            // 設定元資料
            result.Metadata = new ExportMetadata
            {
                DataTypes = request.DataTypes,
                Format = "excel",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TemplateName = request.TemplateName
            };

            _logger.LogInformation("Excel 匯出完成: {FileName}, 大小: {FileSize} bytes", 
                fileName, result.FileSize);

            return result;
        }
        catch (Exception ex)
        {
            result.Status = ExportStatus.Failed;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "Excel 匯出失敗: {Error}", ex.Message);
            return result;
        }
    }

    /// <summary>
    /// 建立工作表
    /// </summary>
    private async Task CreateWorksheetAsync(XLWorkbook workbook, string dataType, object data)
    {
        switch (dataType.ToLower())
        {
            case "accounting":
                await CreateAccountingWorksheetAsync(workbook, data);
                break;
            case "habits":
                await CreateHabitsWorksheetAsync(workbook, data);
                break;
            case "notes":
                await CreateNotesWorksheetAsync(workbook, data);
                break;
            case "todo":
                await CreateTodoWorksheetAsync(workbook, data);
                break;
            default:
                await CreateGenericWorksheetAsync(workbook, dataType, data);
                break;
        }
    }

    /// <summary>
    /// 建立記帳資料工作表
    /// </summary>
    private async Task CreateAccountingWorksheetAsync(XLWorkbook workbook, object data)
    {
        var accountingData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var worksheet = workbook.Worksheets.Add("記帳記錄");
        
        // 設定標題
        worksheet.Cell(1, 1).Value = "日期";
        worksheet.Cell(1, 2).Value = "描述";
        worksheet.Cell(1, 3).Value = "分類";
        worksheet.Cell(1, 4).Value = "類型";
        worksheet.Cell(1, 5).Value = "金額";
        worksheet.Cell(1, 6).Value = "備註";
        
        // 設定標題樣式
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        // 填入資料
        int row = 2;
        var records = accountingData?.Records;
        if (records != null)
        {
            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = DateTime.Parse(record?.Date?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = record?.Description?.ToString() ?? "";
                worksheet.Cell(row, 3).Value = record?.CategoryName?.ToString() ?? "";
                worksheet.Cell(row, 4).Value = record?.Type?.ToString() == "income" ? "收入" : "支出";
                
                // 處理金額，確保為數字格式
                if (decimal.TryParse(record?.Amount?.ToString(), out decimal amount))
                {
                    worksheet.Cell(row, 5).Value = amount;
                    worksheet.Cell(row, 5).Style.NumberFormat.Format = "#,##0.00";
                }
                else
                {
                    worksheet.Cell(row, 5).Value = 0;
                }
                
                worksheet.Cell(row, 6).Value = record?.Note?.ToString() ?? "";
                
                // 根據收支類型設定不同顏色
                if (record?.Type?.ToString() == "income")
                {
                    worksheet.Row(row).Style.Font.FontColor = XLColor.Green;
                }
                else
                {
                    worksheet.Row(row).Style.Font.FontColor = XLColor.Red;
                }
                
                row++;
            }
        }
        
        // 自動調整列寬
        worksheet.Columns().AdjustToContents();
        
        // 添加統計資訊
        var summary = accountingData?.Summary;
        if (summary != null)
        {
            var summaryStartRow = row + 2;
            
            // 設定統計標題
            worksheet.Cell(summaryStartRow, 4).Value = "摘要統計";
            worksheet.Cell(summaryStartRow, 4).Style.Font.Bold = true;
            worksheet.Cell(summaryStartRow, 4).Style.Font.FontSize = 12;
            
            worksheet.Cell(summaryStartRow + 1, 4).Value = "總收入：";
            worksheet.Cell(summaryStartRow + 1, 5).Value = (decimal)(summary?.TotalIncome ?? 0);
            worksheet.Cell(summaryStartRow + 1, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(summaryStartRow + 1, 5).Style.Font.FontColor = XLColor.Green;
            
            worksheet.Cell(summaryStartRow + 2, 4).Value = "總支出：";
            worksheet.Cell(summaryStartRow + 2, 5).Value = (decimal)(summary?.TotalExpense ?? 0);
            worksheet.Cell(summaryStartRow + 2, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(summaryStartRow + 2, 5).Style.Font.FontColor = XLColor.Red;
            
            worksheet.Cell(summaryStartRow + 3, 4).Value = "淨收入：";
            var netIncome = (decimal)(summary?.TotalIncome ?? 0) - (decimal)(summary?.TotalExpense ?? 0);
            worksheet.Cell(summaryStartRow + 3, 5).Value = netIncome;
            worksheet.Cell(summaryStartRow + 3, 5).Style.NumberFormat.Format = "#,##0.00";
            worksheet.Cell(summaryStartRow + 3, 5).Style.Font.FontColor = netIncome >= 0 ? XLColor.Green : XLColor.Red;
            
            // 統計區域樣式
            var summaryRange = worksheet.Range(summaryStartRow, 4, summaryStartRow + 3, 5);
            summaryRange.Style.Font.Bold = true;
            summaryRange.Style.Fill.BackgroundColor = XLColor.LightGreen;
            summaryRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// 建立習慣追蹤工作表
    /// </summary>
    private async Task CreateHabitsWorksheetAsync(XLWorkbook workbook, object data)
    {
        var habitsData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var worksheet = workbook.Worksheets.Add("習慣追蹤");
        
        // 設定標題
        worksheet.Cell(1, 1).Value = "日期";
        worksheet.Cell(1, 2).Value = "習慣名稱";
        worksheet.Cell(1, 3).Value = "完成狀態";
        worksheet.Cell(1, 4).Value = "備註";
        
        // 設定標題樣式
        var headerRange = worksheet.Range(1, 1, 1, 4);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        // 填入資料
        int row = 2;
        var records = habitsData?.Records;
        if (records != null)
        {
            foreach (var record in records)
            {
                worksheet.Cell(row, 1).Value = DateTime.Parse(record?.Date?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd");
                worksheet.Cell(row, 2).Value = record?.HabitName?.ToString() ?? "";
                
                var isCompleted = record?.IsCompleted?.ToString() == "True";
                worksheet.Cell(row, 3).Value = isCompleted ? "已完成" : "未完成";
                worksheet.Cell(row, 3).Style.Font.FontColor = isCompleted ? XLColor.Green : XLColor.Red;
                
                worksheet.Cell(row, 4).Value = record?.Note?.ToString() ?? "";
                row++;
            }
        }
        
        // 自動調整列寬
        worksheet.Columns().AdjustToContents();

        await Task.CompletedTask;
    }

    /// <summary>
    /// 建立備忘錄工作表
    /// </summary>
    private async Task CreateNotesWorksheetAsync(XLWorkbook workbook, object data)
    {
        var notesData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var worksheet = workbook.Worksheets.Add("備忘錄");
        
        // 設定標題
        worksheet.Cell(1, 1).Value = "建立時間";
        worksheet.Cell(1, 2).Value = "標題";
        worksheet.Cell(1, 3).Value = "內容";
        worksheet.Cell(1, 4).Value = "標籤";
        worksheet.Cell(1, 5).Value = "分類";
        
        // 設定標題樣式
        var headerRange = worksheet.Range(1, 1, 1, 5);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        // 填入資料
        int row = 2;
        var notes = notesData?.Notes ?? notesData;
        if (notes is IEnumerable<object> noteList)
        {
            foreach (var note in noteList)
            {
                var noteObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(note));
                worksheet.Cell(row, 1).Value = DateTime.Parse(noteObj?.CreatedAt?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 2).Value = noteObj?.Title?.ToString() ?? "";
                
                // 處理內容，如果太長則截斷
                var content = noteObj?.Content?.ToString() ?? "";
                if (content.Length > 500)
                {
                    content = content.Substring(0, 500) + "...";
                }
                worksheet.Cell(row, 3).Value = content;
                
                worksheet.Cell(row, 4).Value = string.Join(", ", noteObj?.Tags ?? new string[0]);
                worksheet.Cell(row, 5).Value = noteObj?.Category?.ToString() ?? "";
                row++;
            }
        }
        
        // 自動調整列寬，但限制最大寬度
        worksheet.Columns().AdjustToContents();
        worksheet.Column(3).Width = Math.Min(worksheet.Column(3).Width, 50); // 限制內容欄寬度

        await Task.CompletedTask;
    }

    /// <summary>
    /// 建立待辦事項工作表
    /// </summary>
    private async Task CreateTodoWorksheetAsync(XLWorkbook workbook, object data)
    {
        var todoData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var worksheet = workbook.Worksheets.Add("待辦事項");
        
        // 設定標題
        worksheet.Cell(1, 1).Value = "建立時間";
        worksheet.Cell(1, 2).Value = "標題";
        worksheet.Cell(1, 3).Value = "描述";
        worksheet.Cell(1, 4).Value = "優先級";
        worksheet.Cell(1, 5).Value = "狀態";
        worksheet.Cell(1, 6).Value = "到期日";
        
        // 設定標題樣式
        var headerRange = worksheet.Range(1, 1, 1, 6);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        
        // 填入資料
        int row = 2;
        var tasks = todoData?.Tasks ?? todoData;
        if (tasks is IEnumerable<object> taskList)
        {
            foreach (var task in taskList)
            {
                var taskObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(task));
                worksheet.Cell(row, 1).Value = DateTime.Parse(taskObj?.CreatedAt?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cell(row, 2).Value = taskObj?.Title?.ToString() ?? "";
                worksheet.Cell(row, 3).Value = taskObj?.Description?.ToString() ?? "";
                
                var priority = GetPriorityDisplayName(taskObj?.Priority?.ToString() ?? "");
                worksheet.Cell(row, 4).Value = priority;
                // 根據優先級設定顏色
                worksheet.Cell(row, 4).Style.Font.FontColor = priority switch
                {
                    "高" => XLColor.Red,
                    "中" => XLColor.Orange,
                    "低" => XLColor.Green,
                    _ => XLColor.Black
                };
                
                var status = GetStatusDisplayName(taskObj?.Status?.ToString() ?? "");
                worksheet.Cell(row, 5).Value = status;
                worksheet.Cell(row, 5).Style.Font.FontColor = status switch
                {
                    "已完成" => XLColor.Green,
                    "進行中" => XLColor.Blue,
                    "已取消" => XLColor.Gray,
                    _ => XLColor.Black
                };
                
                // 處理到期日
                var dueDateStr = taskObj?.DueDate?.ToString();
                if (!string.IsNullOrEmpty(dueDateStr))
                {
                    try
                    {
                        var dueDate = DateTime.Parse(dueDateStr);
                        worksheet.Cell(row, 6).Value = dueDate.ToString("yyyy-MM-dd");
                        
                        // 如果已過期，標記為紅色
                        if (dueDate < DateTime.Now && status != "已完成")
                        {
                            worksheet.Cell(row, 6).Style.Font.FontColor = XLColor.Red;
                            worksheet.Cell(row, 6).Style.Font.Bold = true;
                        }
                    }
                    catch
                    {
                        worksheet.Cell(row, 6).Value = "";
                    }
                }
                else
                {
                    worksheet.Cell(row, 6).Value = "";
                }
                
                row++;
            }
        }
        
        // 自動調整列寬
        worksheet.Columns().AdjustToContents();

        await Task.CompletedTask;
    }

    /// <summary>
    /// 建立通用工作表
    /// </summary>
    private async Task CreateGenericWorksheetAsync(XLWorkbook workbook, string dataType, object data)
    {
        var worksheet = workbook.Worksheets.Add(GetDataTypeDisplayName(dataType));
        
        // 將資料轉換為 JSON 字串並顯示
        var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        var lines = jsonString.Split('\n');
        
        worksheet.Cell(1, 1).Value = "資料內容";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
        
        int row = 2;
        foreach (var line in lines)
        {
            worksheet.Cell(row, 1).Value = line.Trim();
            row++;
        }
        
        worksheet.Columns().AdjustToContents();

        await Task.CompletedTask;
    }

    /// <summary>
    /// 建立摘要工作表
    /// </summary>
    private async Task CreateSummaryWorksheetAsync(XLWorkbook workbook, Dictionary<string, object> data, ExportRequest request)
    {
        var worksheet = workbook.Worksheets.Add("匯出摘要");
        
        // 匯出資訊
        worksheet.Cell(1, 1).Value = "個人管理系統 - 資料匯出摘要";
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 16;
        worksheet.Range(1, 1, 1, 3).Merge();
        
        int row = 3;
        worksheet.Cell(row, 1).Value = "匯出時間：";
        worksheet.Cell(row, 2).Value = DateTime.Now.ToString("yyyy年MM月dd日 HH:mm:ss");
        row++;
        
        worksheet.Cell(row, 1).Value = "匯出格式：";
        worksheet.Cell(row, 2).Value = "Excel (.xlsx)";
        row++;
        
        worksheet.Cell(row, 1).Value = "資料範圍：";
        var dateRange = request.StartDate.HasValue && request.EndDate.HasValue 
            ? $"{request.StartDate:yyyy-MM-dd} 至 {request.EndDate:yyyy-MM-dd}"
            : "全部資料";
        worksheet.Cell(row, 2).Value = dateRange;
        row++;
        
        worksheet.Cell(row, 1).Value = "資料類型：";
        worksheet.Cell(row, 2).Value = string.Join("、", request.DataTypes.Select(GetDataTypeDisplayName));
        row += 2;
        
        // 資料統計
        worksheet.Cell(row, 1).Value = "資料統計：";
        worksheet.Cell(row, 1).Style.Font.Bold = true;
        row++;
        
        foreach (var kvp in data)
        {
            worksheet.Cell(row, 2).Value = GetDataTypeDisplayName(kvp.Key);
            
            // 嘗試計算記錄數
            try
            {
                var jsonData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(kvp.Value));
                int recordCount = 0;
                
                if (jsonData?.Records != null)
                {
                    recordCount = ((IEnumerable<object>)jsonData.Records).Count();
                }
                else if (jsonData is IEnumerable<object> items)
                {
                    recordCount = items.Count();
                }
                
                worksheet.Cell(row, 3).Value = $"{recordCount} 筆記錄";
            }
            catch
            {
                worksheet.Cell(row, 3).Value = "無法統計";
            }
            
            row++;
        }
        
        // 設定樣式
        var infoRange = worksheet.Range(3, 1, row - 1, 1);
        infoRange.Style.Font.Bold = true;
        
        // 自動調整列寬
        worksheet.Columns().AdjustToContents();

        await Task.CompletedTask;
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
