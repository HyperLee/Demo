using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Demo.Models;
using Newtonsoft.Json;

namespace Demo.Services;

/// <summary>
/// CSV 匯出服務
/// 負責將資料匯出為 CSV 格式，並特別處理中文編碼問題
/// </summary>
public class CsvExportService
{
    private readonly ILogger<CsvExportService> _logger;

    public CsvExportService(ILogger<CsvExportService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 匯出資料為 CSV 格式
    /// </summary>
    /// <param name="data">要匯出的資料</param>
    /// <param name="request">匯出請求</param>
    /// <returns>匯出結果</returns>
    public async Task<ExportResult> ExportToCsvAsync(
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
            var fileName = GenerateFileName("export", "csv");
            var filePath = Path.Combine("wwwroot", "exports", fileName);
            
            // 確保匯出目錄存在
            var exportDir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(exportDir) && !Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            // 為每個資料類型建立單獨的 CSV 檔案或合併到一個檔案
            if (request.DataTypes.Count == 1)
            {
                // 單一資料類型，建立單一 CSV 檔案
                await ExportSingleDataTypeToCsvAsync(data, request.DataTypes[0], filePath);
            }
            else
            {
                // 多個資料類型，建立壓縮檔包含多個 CSV
                await ExportMultipleDataTypesToCsvAsync(data, request.DataTypes, filePath);
            }

            var fileInfo = new FileInfo(filePath);
            result.FileName = fileName;
            result.FilePath = filePath;
            result.FileSize = fileInfo.Length;
            result.ContentType = request.DataTypes.Count == 1 ? "text/csv" : "application/zip";
            result.Status = ExportStatus.Completed;
            result.CompletedAt = DateTime.Now;

            // 設定元資料
            result.Metadata = new ExportMetadata
            {
                DataTypes = request.DataTypes,
                Format = "csv",
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                TemplateName = request.TemplateName
            };

            _logger.LogInformation("CSV 匯出完成: {FileName}, 大小: {FileSize} bytes", 
                fileName, result.FileSize);

            return result;
        }
        catch (Exception ex)
        {
            result.Status = ExportStatus.Failed;
            result.ErrorMessage = ex.Message;
            _logger.LogError(ex, "CSV 匯出失敗: {Error}", ex.Message);
            return result;
        }
    }

    /// <summary>
    /// 匯出單一資料類型到 CSV 檔案
    /// </summary>
    private async Task ExportSingleDataTypeToCsvAsync(
        Dictionary<string, object> data, 
        string dataType, 
        string filePath)
    {
        if (!data.ContainsKey(dataType))
            throw new ArgumentException($"資料中不包含指定的類型: {dataType}");

        // 使用 UTF-8 with BOM 編碼以確保中文正確顯示
        using var writer = new StreamWriter(filePath, false, new UTF8Encoding(true));
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // 確保中文字元正確處理
            Encoding = Encoding.UTF8,
            HasHeaderRecord = true,
            // 處理包含逗號的欄位
            Quote = '"',
            Escape = '"'
        };

        using var csv = new CsvWriter(writer, config);

        await WriteDataTypeToCsvAsync(csv, dataType, data[dataType]);
    }

    /// <summary>
    /// 匯出多個資料類型到壓縮檔
    /// </summary>
    private async Task ExportMultipleDataTypesToCsvAsync(
        Dictionary<string, object> data, 
        List<string> dataTypes, 
        string zipFilePath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            // 為每個資料類型建立 CSV 檔案
            foreach (var dataType in dataTypes)
            {
                if (data.ContainsKey(dataType))
                {
                    var csvFileName = $"{GetDataTypeDisplayName(dataType)}.csv";
                    var csvFilePath = Path.Combine(tempDir, csvFileName);
                    
                    using var writer = new StreamWriter(csvFilePath, false, new UTF8Encoding(true));
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Encoding = Encoding.UTF8,
                        HasHeaderRecord = true,
                        Quote = '"',
                        Escape = '"'
                    };

                    using var csv = new CsvWriter(writer, config);
                    await WriteDataTypeToCsvAsync(csv, dataType, data[dataType]);
                }
            }

            // 建立 ZIP 壓縮檔
            System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipFilePath);
        }
        finally
        {
            // 清理暫存目錄
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    /// <summary>
    /// 將特定資料類型寫入 CSV
    /// </summary>
    private async Task WriteDataTypeToCsvAsync(CsvWriter csv, string dataType, object data)
    {
        switch (dataType.ToLower())
        {
            case "accounting":
                await WriteAccountingDataToCsvAsync(csv, data);
                break;
            case "habits":
                await WriteHabitsDataToCsvAsync(csv, data);
                break;
            case "notes":
                await WriteNotesDataToCsvAsync(csv, data);
                break;
            case "todo":
                await WriteTodoDataToCsvAsync(csv, data);
                break;
            default:
                await WriteGenericDataToCsvAsync(csv, data);
                break;
        }
    }

    /// <summary>
    /// 寫入記帳資料到 CSV
    /// </summary>
    private async Task WriteAccountingDataToCsvAsync(CsvWriter csv, object data)
    {
        var accountingData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var records = accountingData?.Records;

        if (records == null) return;

        // 寫入標頭
        csv.WriteField("日期");
        csv.WriteField("描述");
        csv.WriteField("分類");
        csv.WriteField("類型");
        csv.WriteField("金額");
        csv.WriteField("備註");
        await csv.NextRecordAsync();

        // 寫入資料
        foreach (var record in records)
        {
            csv.WriteField(DateTime.Parse(record.Date.ToString()).ToString("yyyy-MM-dd"));
            csv.WriteField(record.Description?.ToString() ?? "");
            csv.WriteField(record.CategoryName?.ToString() ?? "未知分類");
            csv.WriteField(record.Type?.ToString() == "income" ? "收入" : "支出");
            csv.WriteField(record.Amount?.ToString() ?? "0");
            csv.WriteField(record.Note?.ToString() ?? "");
            await csv.NextRecordAsync();
        }

        // 寫入摘要資訊
        var summary = accountingData?.Summary;
        if (summary != null)
        {
            await csv.NextRecordAsync(); // 空行
            csv.WriteField("摘要統計");
            await csv.NextRecordAsync();
            
            csv.WriteField("總收入");
            csv.WriteField(summary.TotalIncome?.ToString() ?? "0");
            await csv.NextRecordAsync();
            
            csv.WriteField("總支出");
            csv.WriteField(summary.TotalExpense?.ToString() ?? "0");
            await csv.NextRecordAsync();
            
            csv.WriteField("淨收入");
            csv.WriteField(((decimal)(summary.TotalIncome ?? 0) - (decimal)(summary.TotalExpense ?? 0)).ToString());
            await csv.NextRecordAsync();
        }
    }

    /// <summary>
    /// 寫入習慣資料到 CSV
    /// </summary>
    private async Task WriteHabitsDataToCsvAsync(CsvWriter csv, object data)
    {
        var habitsData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var records = habitsData?.Records;

        if (records == null) return;

        // 寫入標頭
        csv.WriteField("日期");
        csv.WriteField("習慣名稱");
        csv.WriteField("完成狀態");
        csv.WriteField("備註");
        await csv.NextRecordAsync();

        // 寫入資料
        foreach (var record in records)
        {
            csv.WriteField(DateTime.Parse(record.Date.ToString()).ToString("yyyy-MM-dd"));
            csv.WriteField(record.HabitName?.ToString() ?? "");
            csv.WriteField(record.IsCompleted?.ToString() == "True" ? "已完成" : "未完成");
            csv.WriteField(record.Note?.ToString() ?? "");
            await csv.NextRecordAsync();
        }
    }

    /// <summary>
    /// 寫入備忘錄資料到 CSV
    /// </summary>
    private async Task WriteNotesDataToCsvAsync(CsvWriter csv, object data)
    {
        var notesData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var notes = notesData?.Notes ?? notesData;

        if (notes == null) return;

        // 寫入標頭
        csv.WriteField("建立時間");
        csv.WriteField("標題");
        csv.WriteField("內容");
        csv.WriteField("標籤");
        csv.WriteField("分類");
        await csv.NextRecordAsync();

        // 寫入資料
        if (notes is IEnumerable<object> noteList)
        {
            foreach (var note in noteList)
            {
                var noteObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(note));
                csv.WriteField(DateTime.Parse(noteObj?.CreatedAt?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(noteObj?.Title?.ToString() ?? "");
                csv.WriteField(noteObj?.Content?.ToString() ?? "");
                csv.WriteField(string.Join(", ", noteObj?.Tags ?? new string[0]));
                csv.WriteField(noteObj?.Category?.ToString() ?? "");
                await csv.NextRecordAsync();
            }
        }
    }

    /// <summary>
    /// 寫入待辦事項資料到 CSV
    /// </summary>
    private async Task WriteTodoDataToCsvAsync(CsvWriter csv, object data)
    {
        var todoData = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(data));
        var tasks = todoData?.Tasks ?? todoData;

        if (tasks == null) return;

        // 寫入標頭
        csv.WriteField("建立時間");
        csv.WriteField("標題");
        csv.WriteField("描述");
        csv.WriteField("優先級");
        csv.WriteField("狀態");
        csv.WriteField("到期日");
        await csv.NextRecordAsync();

        // 寫入資料
        if (tasks is IEnumerable<object> taskList)
        {
            foreach (var task in taskList)
            {
                var taskObj = JsonConvert.DeserializeObject<dynamic>(JsonConvert.SerializeObject(task));
                csv.WriteField(DateTime.Parse(taskObj?.CreatedAt?.ToString() ?? DateTime.Now.ToString()).ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(taskObj?.Title?.ToString() ?? "");
                csv.WriteField(taskObj?.Description?.ToString() ?? "");
                csv.WriteField(GetPriorityDisplayName(taskObj?.Priority?.ToString() ?? ""));
                csv.WriteField(GetStatusDisplayName(taskObj?.Status?.ToString() ?? ""));
                
                // 處理到期日
                var dueDateStr = taskObj?.DueDate?.ToString();
                if (!string.IsNullOrEmpty(dueDateStr))
                {
                    try
                    {
                        var parsedDate = DateTime.Parse(dueDateStr);
                        csv.WriteField(parsedDate.ToString("yyyy-MM-dd"));
                    }
                    catch
                    {
                        csv.WriteField("");
                    }
                }
                else
                {
                    csv.WriteField("");
                }
                
                await csv.NextRecordAsync();
            }
        }
    }

    /// <summary>
    /// 寫入通用資料到 CSV
    /// </summary>
    private async Task WriteGenericDataToCsvAsync(CsvWriter csv, object data)
    {
        var jsonString = JsonConvert.SerializeObject(data, Formatting.Indented);
        var lines = jsonString.Split('\n');
        
        csv.WriteField("資料內容");
        await csv.NextRecordAsync();
        
        foreach (var line in lines)
        {
            csv.WriteField(line.Trim());
            await csv.NextRecordAsync();
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
