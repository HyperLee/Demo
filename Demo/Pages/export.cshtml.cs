using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Demo.Pages;

/// <summary>
/// 資料匯出頁面模型
/// </summary>
public class ExportModel : PageModel
{
    private readonly ExportService _exportService;
    private readonly ILogger<ExportModel> _logger;

    public ExportModel(ExportService exportService, ILogger<ExportModel> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// 匯出歷史記錄
    /// </summary>
    public List<ExportHistory> ExportHistory { get; set; } = new();

    /// <summary>
    /// 支援的資料類型
    /// </summary>
    public List<DataTypeOption> DataTypes { get; set; } = new();

    /// <summary>
    /// 支援的匯出格式
    /// </summary>
    public List<FormatOption> Formats { get; set; } = new();

    /// <summary>
    /// 頁面載入
    /// </summary>
    public async Task OnGetAsync()
    {
        try
        {
            // 載入匯出歷史
            ExportHistory = await _exportService.GetExportHistoryAsync();

            // 初始化選項
            InitializeOptions();

            _logger.LogInformation("匯出頁面已載入，歷史記錄數量: {Count}", ExportHistory.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入匯出頁面時發生錯誤");
            ExportHistory = new List<ExportHistory>();
        }
    }

    /// <summary>
    /// 處理匯出請求
    /// </summary>
    public async Task<IActionResult> OnPostExportAsync([FromBody] ExportRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "請求參數無效",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });
            }

            // 設定用戶資訊
            request.UserId = "demo-user"; // 實際應用中應從認證中獲取
            request.CreatedAt = DateTime.UtcNow;

            // 執行匯出
            var result = await _exportService.ExportDataAsync(request);

            _logger.LogInformation("匯出請求已處理: {ExportId}", result.Id);

            return new JsonResult(new
            {
                success = true,
                message = "匯出成功",
                data = new
                {
                    exportId = result.Id,
                    fileName = result.FileName,
                    fileSize = result.FileSize,
                    status = result.Status.ToString(),
                    downloadUrl = Url.Page("/Export", "Download", new { id = result.Id })
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理匯出請求時發生錯誤");
            return new JsonResult(new
            {
                success = false,
                message = "匯出失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 下載檔案
    /// </summary>
    public async Task<IActionResult> OnGetDownloadAsync(string id)
    {
        try
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var history = await _exportService.GetExportHistoryAsync();
            var export = history.FirstOrDefault(h => h.Id == id);

            if (export == null)
            {
                TempData["ErrorMessage"] = "找不到指定的匯出記錄";
                return RedirectToPage();
            }

            if (export.Result.Status != ExportStatus.Completed)
            {
                TempData["ErrorMessage"] = "匯出尚未完成或失敗";
                return RedirectToPage();
            }

            if (!System.IO.File.Exists(export.Result.FilePath))
            {
                TempData["ErrorMessage"] = "匯出檔案不存在或已被清理";
                return RedirectToPage();
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(export.Result.FilePath);
            var contentType = export.Result.ContentType ?? "application/octet-stream";

            _logger.LogInformation("下載匯出檔案: {FileName}", export.Result.FileName);

            return File(fileBytes, contentType, export.Result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下載檔案時發生錯誤: {ExportId}", id);
            TempData["ErrorMessage"] = "下載失敗: " + ex.Message;
            return RedirectToPage();
        }
    }

    /// <summary>
    /// 刪除匯出記錄
    /// </summary>
    public async Task<IActionResult> OnPostDeleteAsync([FromBody] DeleteRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Id))
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "無效的匯出ID"
                });
            }

            var success = await _exportService.DeleteExportHistoryAsync(request.Id);

            if (success)
            {
                _logger.LogInformation("已刪除匯出記錄: {ExportId}", request.Id);
                return new JsonResult(new
                {
                    success = true,
                    message = "刪除成功"
                });
            }
            else
            {
                return new JsonResult(new
                {
                    success = false,
                    message = "找不到指定的匯出記錄"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除匯出記錄時發生錯誤: {ExportId}", request.Id);
            return new JsonResult(new
            {
                success = false,
                message = "刪除失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 初始化選項
    /// </summary>
    private void InitializeOptions()
    {
        // 資料類型選項
        DataTypes = new List<DataTypeOption>
        {
            new DataTypeOption
            {
                Value = "accounting",
                Label = "記帳資料",
                Description = "收支記錄、分類統計",
                Icon = "fas fa-wallet",
                Color = "primary"
            },
            new DataTypeOption
            {
                Value = "notes",
                Label = "備忘錄",
                Description = "日常備忘錄和筆記",
                Icon = "fas fa-sticky-note",
                Color = "warning"
            },
            new DataTypeOption
            {
                Value = "habits",
                Label = "習慣追蹤",
                Description = "習慣記錄和統計",
                Icon = "fas fa-chart-line",
                Color = "success"
            },
            new DataTypeOption
            {
                Value = "todo",
                Label = "待辦事項",
                Description = "任務列表和完成狀況",
                Icon = "fas fa-tasks",
                Color = "info"
            }
        };

        // 匯出格式選項
        Formats = new List<FormatOption>
        {
            new FormatOption
            {
                Value = "pdf",
                Label = "PDF 報表",
                Description = "適合列印和分享的完整報表",
                Icon = "fas fa-file-pdf",
                Color = "danger"
            },
            new FormatOption
            {
                Value = "excel",
                Label = "Excel 試算表",
                Description = "適合進一步分析的結構化資料",
                Icon = "fas fa-file-excel",
                Color = "success"
            },
            new FormatOption
            {
                Value = "csv",
                Label = "CSV 檔案",
                Description = "純文字格式，相容性最佳",
                Icon = "fas fa-file-csv",
                Color = "secondary"
            },
            new FormatOption
            {
                Value = "json",
                Label = "JSON 資料",
                Description = "程式開發用的結構化資料格式",
                Icon = "fas fa-code",
                Color = "dark"
            }
        };
    }

    /// <summary>
    /// 格式化檔案大小
    /// </summary>
    public static string FormatFileSize(long bytes)
    {
        if (bytes == 0) return "0 B";
        
        string[] sizes = { "B", "KB", "MB", "GB" };
        int order = 0;
        double size = bytes;
        
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        
        return $"{size:0.##} {sizes[order]}";
    }

    /// <summary>
    /// 取得狀態顯示名稱
    /// </summary>
    public static string GetStatusDisplayName(ExportStatus status)
    {
        return status switch
        {
            ExportStatus.Pending => "等待處理",
            ExportStatus.Processing => "處理中",
            ExportStatus.Completed => "已完成",
            ExportStatus.Failed => "失敗",
            ExportStatus.Expired => "已過期",
            _ => "未知"
        };
    }

    /// <summary>
    /// 取得狀態 CSS 類別
    /// </summary>
    public static string GetStatusBadgeClass(ExportStatus status)
    {
        return status switch
        {
            ExportStatus.Pending => "badge bg-secondary",
            ExportStatus.Processing => "badge bg-primary",
            ExportStatus.Completed => "badge bg-success",
            ExportStatus.Failed => "badge bg-danger",
            ExportStatus.Expired => "badge bg-warning",
            _ => "badge bg-light text-dark"
        };
    }
}

/// <summary>
/// 資料類型選項
/// </summary>
public class DataTypeOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// 格式選項
/// </summary>
public class FormatOption
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

/// <summary>
/// 刪除請求模型
/// </summary>
public class DeleteRequest
{
    public string Id { get; set; } = string.Empty;
}
