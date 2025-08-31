using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Demo.Controllers;

/// <summary>
/// 資料匯出 API 控制器
/// 提供資料匯出相關的 RESTful API 端點
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly ExportService _exportService;
    private readonly ILogger<ExportController> _logger;

    public ExportController(ExportService exportService, ILogger<ExportController> logger)
    {
        _exportService = exportService;
        _logger = logger;
    }

    /// <summary>
    /// 開始資料匯出
    /// </summary>
    /// <param name="request">匯出請求</param>
    /// <returns>匯出結果</returns>
    [HttpPost("start")]
    public async Task<IActionResult> StartExport([FromBody] ExportRequest request)
    {
        try
        {
            // 驗證請求
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.DataTypes == null || !request.DataTypes.Any())
            {
                return BadRequest("請至少選擇一種資料類型");
            }

            if (string.IsNullOrEmpty(request.Format))
            {
                return BadRequest("請選擇匯出格式");
            }

            // 驗證格式
            var supportedFormats = new[] { "pdf", "excel", "csv", "json" };
            if (!supportedFormats.Contains(request.Format.ToLower()))
            {
                return BadRequest($"不支援的匯出格式: {request.Format}");
            }

            // 驗證資料類型
            var supportedDataTypes = new[] { "accounting", "notes", "habits", "todo" };
            var invalidDataTypes = request.DataTypes.Where(dt => !supportedDataTypes.Contains(dt.ToLower())).ToList();
            if (invalidDataTypes.Any())
            {
                return BadRequest($"不支援的資料類型: {string.Join(", ", invalidDataTypes)}");
            }

            // 驗證日期範圍
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.StartDate > request.EndDate)
            {
                return BadRequest("開始日期不能晚於結束日期");
            }

            // 設定用戶ID（暫時使用固定值，實際應用中應從認證中獲取）
            request.UserId = "demo-user";
            request.CreatedAt = DateTime.UtcNow;

            _logger.LogInformation("開始匯出資料，格式: {Format}, 類型: {DataTypes}", 
                request.Format, string.Join(",", request.DataTypes));

            // 執行匯出
            var result = await _exportService.ExportDataAsync(request);

            return Ok(new
            {
                success = true,
                message = "匯出完成",
                data = new
                {
                    exportId = result.Id,
                    fileName = result.FileName,
                    fileSize = result.FileSize,
                    status = result.Status.ToString(),
                    createdAt = result.CreatedAt,
                    completedAt = result.CompletedAt,
                    downloadUrl = Url.Action("DownloadFile", new { id = result.Id })
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "匯出資料時發生錯誤");
            return StatusCode(500, new
            {
                success = false,
                message = "匯出失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 取得匯出進度
    /// </summary>
    /// <param name="id">匯出ID</param>
    /// <returns>匯出進度資訊</returns>
    [HttpGet("progress/{id}")]
    public async Task<IActionResult> GetExportProgress(string id)
    {
        try
        {
            // 注意：這是一個簡化實作，實際應用中可能需要更複雜的進度追蹤
            var history = await _exportService.GetExportHistoryAsync();
            var export = history.FirstOrDefault(h => h.Id == id);

            if (export == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "找不到指定的匯出記錄"
                });
            }

            return Ok(new
            {
                success = true,
                data = new
                {
                    id = export.Id,
                    status = export.Result.Status.ToString(),
                    progress = export.Result.Status == ExportStatus.Completed ? 100 : 
                              export.Result.Status == ExportStatus.Processing ? 50 : 0,
                    message = GetStatusMessage(export.Result.Status),
                    fileName = export.Result.FileName,
                    fileSize = export.Result.FileSize,
                    createdAt = export.Result.CreatedAt,
                    completedAt = export.Result.CompletedAt,
                    errorMessage = export.Result.ErrorMessage
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得匯出進度時發生錯誤: {ExportId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "取得進度失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 下載匯出檔案
    /// </summary>
    /// <param name="id">匯出ID</param>
    /// <returns>檔案下載</returns>
    [HttpGet("download/{id}")]
    public async Task<IActionResult> DownloadFile(string id)
    {
        try
        {
            var history = await _exportService.GetExportHistoryAsync();
            var export = history.FirstOrDefault(h => h.Id == id);

            if (export == null)
            {
                return NotFound("找不到指定的匯出記錄");
            }

            if (export.Result.Status != ExportStatus.Completed)
            {
                return BadRequest("匯出尚未完成或失敗");
            }

            if (!System.IO.File.Exists(export.Result.FilePath))
            {
                return NotFound("匯出檔案不存在或已被清理");
            }

            var fileBytes = await System.IO.File.ReadAllBytesAsync(export.Result.FilePath);
            var contentType = export.Result.ContentType ?? "application/octet-stream";

            _logger.LogInformation("下載匯出檔案: {FileName}", export.Result.FileName);

            return File(fileBytes, contentType, export.Result.FileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下載檔案時發生錯誤: {ExportId}", id);
            return StatusCode(500, "下載失敗: " + ex.Message);
        }
    }

    /// <summary>
    /// 取得匯出歷史記錄
    /// </summary>
    /// <param name="limit">限制筆數</param>
    /// <returns>匯出歷史列表</returns>
    [HttpGet("history")]
    public async Task<IActionResult> GetExportHistory([FromQuery] int limit = 50)
    {
        try
        {
            var history = await _exportService.GetExportHistoryAsync();
            var limitedHistory = history.Take(limit).ToList();

            var response = limitedHistory.Select(h => new
            {
                id = h.Id,
                userId = h.UserId,
                createdAt = h.CreatedAt,
                request = new
                {
                    dataTypes = h.Request.DataTypes,
                    format = h.Request.Format,
                    startDate = h.Request.StartDate,
                    endDate = h.Request.EndDate,
                    templateName = h.Request.TemplateName
                },
                result = new
                {
                    fileName = h.Result.FileName,
                    fileSize = h.Result.FileSize,
                    status = h.Result.Status.ToString(),
                    createdAt = h.Result.CreatedAt,
                    completedAt = h.Result.CompletedAt,
                    errorMessage = h.Result.ErrorMessage,
                    downloadUrl = h.Result.Status == ExportStatus.Completed 
                        ? Url.Action("DownloadFile", new { id = h.Id }) 
                        : null
                }
            }).ToList();

            return Ok(new
            {
                success = true,
                data = response,
                total = response.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得匯出歷史時發生錯誤");
            return StatusCode(500, new
            {
                success = false,
                message = "取得歷史記錄失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 刪除匯出歷史記錄
    /// </summary>
    /// <param name="id">匯出ID</param>
    /// <returns>刪除結果</returns>
    [HttpDelete("history/{id}")]
    public async Task<IActionResult> DeleteExportHistory(string id)
    {
        try
        {
            var success = await _exportService.DeleteExportHistoryAsync(id);

            if (success)
            {
                _logger.LogInformation("已刪除匯出歷史: {ExportId}", id);
                return Ok(new
                {
                    success = true,
                    message = "刪除成功"
                });
            }
            else
            {
                return NotFound(new
                {
                    success = false,
                    message = "找不到指定的匯出記錄"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除匯出歷史時發生錯誤: {ExportId}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "刪除失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 清理過期檔案
    /// </summary>
    /// <returns>清理結果</returns>
    [HttpPost("cleanup")]
    public async Task<IActionResult> CleanupExpiredFiles()
    {
        try
        {
            await _exportService.CleanupExpiredExportsAsync();
            _logger.LogInformation("已執行過期檔案清理");
            
            return Ok(new
            {
                success = true,
                message = "清理完成"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "清理過期檔案時發生錯誤");
            return StatusCode(500, new
            {
                success = false,
                message = "清理失敗: " + ex.Message
            });
        }
    }

    /// <summary>
    /// 取得支援的匯出選項
    /// </summary>
    /// <returns>匯出選項資訊</returns>
    [HttpGet("options")]
    public IActionResult GetExportOptions()
    {
        return Ok(new
        {
            success = true,
            data = new
            {
                formats = new[]
                {
                    new { value = "pdf", label = "PDF 報表", description = "適合列印和分享的完整報表" },
                    new { value = "excel", label = "Excel 試算表", description = "適合進一步分析的結構化資料" },
                    new { value = "csv", label = "CSV 檔案", description = "純文字格式，相容性最佳" },
                    new { value = "json", label = "JSON 資料", description = "程式開發用的結構化資料格式" }
                },
                dataTypes = new[]
                {
                    new { value = "accounting", label = "記帳資料", description = "收支記錄、分類統計" },
                    new { value = "notes", label = "備忘錄", description = "日常備忘錄和筆記" },
                    new { value = "habits", label = "習慣追蹤", description = "習慣記錄和統計" },
                    new { value = "todo", label = "待辦事項", description = "任務列表和完成狀況" }
                },
                templates = new[]
                {
                    new { value = "default", label = "預設樣板", description = "標準格式匯出" },
                    new { value = "summary", label = "摘要報表", description = "僅包含摘要統計" },
                    new { value = "detailed", label = "詳細報表", description = "包含完整詳細資料" }
                }
            }
        });
    }

    /// <summary>
    /// 取得狀態訊息
    /// </summary>
    private string GetStatusMessage(ExportStatus status)
    {
        return status switch
        {
            ExportStatus.Pending => "等待處理",
            ExportStatus.Processing => "處理中...",
            ExportStatus.Completed => "匯出完成",
            ExportStatus.Failed => "匯出失敗",
            ExportStatus.Expired => "檔案已過期",
            _ => "未知狀態"
        };
    }
}
