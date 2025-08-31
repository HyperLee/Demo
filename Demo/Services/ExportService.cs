using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 統一匯出服務
/// 負責協調各種格式的匯出服務，並提供資料收集和匯出歷史管理功能
/// </summary>
public class ExportService
{
    private readonly IAccountingService _accountingService;
    private readonly INoteService _noteService;
    private readonly HabitService _habitService;
    private readonly TodoService _todoService;
    private readonly PdfExportService _pdfService;
    private readonly ExcelExportService _excelService;
    private readonly CsvExportService _csvService;
    private readonly ILogger<ExportService> _logger;
    
    private readonly string _exportPath;
    private readonly string _historyPath;

    public ExportService(
        IAccountingService accountingService,
        INoteService noteService,
        HabitService habitService,
        TodoService todoService,
        PdfExportService pdfService,
        ExcelExportService excelService,
        CsvExportService csvService,
        ILogger<ExportService> logger)
    {
        _accountingService = accountingService;
        _noteService = noteService;
        _habitService = habitService;
        _todoService = todoService;
        _pdfService = pdfService;
        _excelService = excelService;
        _csvService = csvService;
        _logger = logger;
        
        _exportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
        _historyPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "export-history.json");
        
        EnsureDirectoriesExist();
    }

    /// <summary>
    /// 執行資料匯出
    /// </summary>
    public async Task<ExportResult> ExportDataAsync(ExportRequest request)
    {
        try
        {
            var exportData = await CollectExportDataAsync(request);
            ExportResult result;

            switch (request.Format.ToLower())
            {
                case "pdf":
                    result = await ExportToPdfAsync(exportData, request);
                    break;
                case "excel":
                    result = await ExportToExcelAsync(exportData, request);
                    break;
                case "csv":
                    result = await ExportToCsvAsync(exportData, request);
                    break;
                case "json":
                    result = await ExportToJsonAsync(exportData, request);
                    break;
                default:
                    throw new ArgumentException($"不支援的匯出格式: {request.Format}");
            }

            await SaveExportHistoryAsync(request, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出資料失敗，格式: {Format}, 錯誤: {Error}", request.Format, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 取得匯出歷史記錄
    /// </summary>
    public async Task<List<ExportHistory>> GetExportHistoryAsync()
    {
        return await LoadExportHistoryAsync();
    }

    /// <summary>
    /// 刪除匯出歷史記錄
    /// </summary>
    public async Task<bool> DeleteExportHistoryAsync(string id)
    {
        try
        {
            var history = await LoadExportHistoryAsync();
            var removed = history.RemoveAll(h => h.Id == id);
            
            if (removed > 0)
            {
                await SaveExportHistoryAsync(history);
                return true;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除匯出歷史失敗: {Id}", id);
            return false;
        }
    }

    /// <summary>
    /// 清理過期匯出檔案
    /// </summary>
    public async Task CleanupExpiredExportsAsync()
    {
        try
        {
            var expiredDate = DateTime.Now.AddDays(-7); // 7天後清理
            var exportDir = new DirectoryInfo(_exportPath);
            
            if (exportDir.Exists)
            {
                var expiredFiles = exportDir.GetFiles()
                    .Where(f => f.CreationTime < expiredDate)
                    .ToArray();
                
                foreach (var file in expiredFiles)
                {
                    try
                    {
                        file.Delete();
                        _logger.LogInformation("已清理過期匯出檔案: {FileName}", file.Name);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "清理檔案失敗: {FileName}", file.Name);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理過期匯出檔案失敗");
        }
    }

    /// <summary>
    /// 收集匯出資料
    /// </summary>
    private async Task<Dictionary<string, object>> CollectExportDataAsync(ExportRequest request)
    {
        var data = new Dictionary<string, object>();
        
        foreach (var dataType in request.DataTypes)
        {
            try
            {
                switch (dataType.ToLower())
                {
                    case "accounting":
                        data["accounting"] = await CollectAccountingDataAsync(request.StartDate, request.EndDate);
                        break;
                    case "notes":
                        data["notes"] = await CollectNotesDataAsync(request.StartDate, request.EndDate);
                        break;
                    case "habits":
                        data["habits"] = await CollectHabitsDataAsync(request.StartDate, request.EndDate);
                        break;
                    case "todo":
                        data["todo"] = await CollectTodoDataAsync(request.StartDate, request.EndDate);
                        break;
                    default:
                        _logger.LogWarning("未知的資料類型: {DataType}", dataType);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "收集資料失敗，類型: {DataType}, 錯誤: {Error}", 
                    dataType, ex.Message);
                throw;
            }
        }
        
        return data;
    }

    /// <summary>
    /// 收集記帳資料
    /// </summary>
    private async Task<object> CollectAccountingDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var records = await _accountingService.GetRecordsAsync(startDate, endDate);
        var incomeCategories = await _accountingService.GetCategoriesAsync("income");
        var expenseCategories = await _accountingService.GetCategoriesAsync("expense");
        var categories = incomeCategories.Concat(expenseCategories).ToList();
        
        return new
        {
            Records = records,
            Categories = categories,
            Summary = new
            {
                TotalRecords = records.Count,
                TotalIncome = records.Where(r => r.Type == "income").Sum(r => r.Amount),
                TotalExpense = records.Where(r => r.Type == "expense").Sum(r => r.Amount),
                Period = new { StartDate = startDate, EndDate = endDate }
            },
            CategorySummary = records
                .GroupBy(r => r.Category)
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(r => r.Amount)
                })
                .ToList()
        };
    }

    /// <summary>
    /// 收集備忘錄資料
    /// </summary>
    private async Task<object> CollectNotesDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var notesList = new List<object>();
        
        if (startDate.HasValue && endDate.HasValue)
        {
            var startDateOnly = DateOnly.FromDateTime(startDate.Value);
            var endDateOnly = DateOnly.FromDateTime(endDate.Value);
            
            for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
            {
                var noteContent = await _noteService.GetNoteAsync(date);
                if (!string.IsNullOrWhiteSpace(noteContent))
                {
                    notesList.Add(new 
                    { 
                        Date = date,
                        Content = noteContent,
                        CreatedAt = date.ToDateTime(TimeOnly.MinValue)
                    });
                }
            }
        }
        
        return new
        {
            Notes = notesList,
            Summary = new
            {
                TotalNotes = notesList.Count,
                Period = new { StartDate = startDate, EndDate = endDate }
            }
        };
    }

    /// <summary>
    /// 收集習慣追蹤資料
    /// </summary>
    private async Task<object> CollectHabitsDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var habits = await _habitService.GetAllHabitsAsync();
        var allRecords = new List<HabitRecord>();
        
        // 為每個習慣收集記錄
        foreach (var habit in habits)
        {
            var records = await _habitService.GetHabitRecordsAsync(habit.Id, startDate, endDate);
            allRecords.AddRange(records);
        }
        
        return new
        {
            Habits = habits,
            Records = allRecords,
            Summary = new
            {
                TotalHabits = habits.Count,
                TotalRecords = allRecords.Count,
                Period = new { StartDate = startDate, EndDate = endDate }
            }
        };
    }

    /// <summary>
    /// 收集待辦事項資料
    /// </summary>
    private async Task<object> CollectTodoDataAsync(DateTime? startDate, DateTime? endDate)
    {
        var tasks = _todoService.GetAllTasks();
        var categories = _todoService.GetCategories();
        
        // 按日期範圍篩選
        if (startDate.HasValue || endDate.HasValue)
        {
            tasks = tasks.Where(t =>
            {
                if (startDate.HasValue && t.CreatedDate < startDate.Value)
                    return false;
                if (endDate.HasValue && t.CreatedDate > endDate.Value)
                    return false;
                return true;
            }).ToList();
        }
        
        return new
        {
            Tasks = tasks,
            Categories = categories,
            Summary = new
            {
                TotalTasks = tasks.Count,
                CompletedTasks = tasks.Count(t => t.Status == TodoStatus.Completed),
                PendingTasks = tasks.Count(t => t.Status == TodoStatus.Pending),
                InProgressTasks = tasks.Count(t => t.Status == TodoStatus.InProgress),
                Period = new { StartDate = startDate, EndDate = endDate }
            }
        };
    }

    /// <summary>
    /// 匯出為 PDF
    /// </summary>
    private async Task<ExportResult> ExportToPdfAsync(Dictionary<string, object> data, ExportRequest request)
    {
        return await _pdfService.ExportToPdfAsync(data, request);
    }

    /// <summary>
    /// 匯出為 Excel
    /// </summary>
    private async Task<ExportResult> ExportToExcelAsync(Dictionary<string, object> data, ExportRequest request)
    {
        return await _excelService.ExportToExcelAsync(data, request);
    }

    /// <summary>
    /// 匯出為 CSV
    /// </summary>
    private async Task<ExportResult> ExportToCsvAsync(Dictionary<string, object> data, ExportRequest request)
    {
        return await _csvService.ExportToCsvAsync(data, request);
    }

    /// <summary>
    /// 匯出為 JSON
    /// </summary>
    private async Task<ExportResult> ExportToJsonAsync(Dictionary<string, object> data, ExportRequest request)
    {
        var fileName = GenerateFileName("json", request);
        var filePath = Path.Combine(_exportPath, fileName);
        
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        
        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(filePath, json, System.Text.Encoding.UTF8);
        
        return new ExportResult
        {
            Status = ExportStatus.Completed,
            FilePath = filePath,
            FileName = fileName,
            FileSize = new FileInfo(filePath).Length,
            CreatedAt = DateTime.UtcNow,
            CompletedAt = DateTime.UtcNow,
            ContentType = "application/json"
        };
    }

    /// <summary>
    /// 生成檔案名稱
    /// </summary>
    private string GenerateFileName(string extension, ExportRequest request)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        var dataTypes = string.Join("-", request.DataTypes);
        return $"export_{dataTypes}_{timestamp}.{extension}";
    }

    /// <summary>
    /// 儲存匯出歷史
    /// </summary>
    private async Task SaveExportHistoryAsync(ExportRequest request, ExportResult result)
    {
        try
        {
            var history = await LoadExportHistoryAsync();
            
            var newEntry = new ExportHistory
            {
                Id = Guid.NewGuid().ToString(),
                UserId = request.UserId,
                Request = request,
                Result = result,
                CreatedAt = DateTime.UtcNow
            };
            
            history.Add(newEntry);
            
            // 只保留最近 100 筆記錄
            if (history.Count > 100)
            {
                history = history.OrderByDescending(h => h.CreatedAt).Take(100).ToList();
            }
            
            await SaveExportHistoryAsync(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存匯出歷史失敗: {Error}", ex.Message);
        }
    }

    /// <summary>
    /// 載入匯出歷史
    /// </summary>
    private async Task<List<ExportHistory>> LoadExportHistoryAsync()
    {
        try
        {
            if (File.Exists(_historyPath))
            {
                var json = await File.ReadAllTextAsync(_historyPath);
                return JsonSerializer.Deserialize<List<ExportHistory>>(json) ?? new List<ExportHistory>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入匯出歷史失敗: {Error}", ex.Message);
        }
        
        return new List<ExportHistory>();
    }

    /// <summary>
    /// 儲存匯出歷史到檔案
    /// </summary>
    private async Task SaveExportHistoryAsync(List<ExportHistory> history)
    {
        var json = JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_historyPath, json, System.Text.Encoding.UTF8);
    }

    /// <summary>
    /// 確保必要的目錄存在
    /// </summary>
    private void EnsureDirectoriesExist()
    {
        Directory.CreateDirectory(_exportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(_historyPath)!);
    }
}
