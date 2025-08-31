using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;
using Demo.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Demo.Controllers;

/// <summary>
/// 共享記帳 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FamilyAccountingController : ControllerBase
{
    private readonly ISharedAccountingService _sharedAccountingService;
    private readonly IFamilyService _familyService;
    private readonly IHubContext<FamilyHub> _hubContext;
    private readonly ILogger<FamilyAccountingController> _logger;

    public FamilyAccountingController(
        ISharedAccountingService sharedAccountingService,
        IFamilyService familyService,
        IHubContext<FamilyHub> hubContext,
        ILogger<FamilyAccountingController> logger)
    {
        _sharedAccountingService = sharedAccountingService;
        _familyService = familyService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 取得家庭共享記帳記錄
    /// </summary>
    [HttpGet("records")]
    public async Task<IActionResult> GetRecords(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            var records = await _sharedAccountingService.GetRecordsAsync(family.Id, startDate, endDate);

            return Ok(new FamilySharingResponse<List<SharedAccountingRecord>>
            {
                Success = true,
                Message = "取得成功",
                Data = records
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得共享記帳記錄時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 新增快速支出記錄
    /// </summary>
    [HttpPost("quick-expense")]
    public async Task<IActionResult> CreateQuickExpense([FromBody] QuickExpenseRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = errors,
                    ErrorCode = "VALIDATION_ERROR"
                });
            }

            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            // 建立共享記帳記錄
            var record = new SharedAccountingRecord
            {
                FamilyId = family.Id,
                UserId = userId,
                Type = request.Type,
                Amount = request.Amount,
                Category = request.Category,
                Description = request.Description,
                Date = request.Date,
                SplitType = request.SplitType,
                SplitDetails = request.SplitDetails ?? new Dictionary<string, decimal>()
            };

            // 處理分攤邏輯
            if (request.SplitType == "我支付")
            {
                record.SplitDetails[userId] = request.Amount;
            }
            else if (request.SplitType == "平均分攤")
            {
                var members = await _familyService.GetFamilyMembersAsync(family.Id);
                var activeMembers = members.Where(m => m.IsActive).ToList();
                var equalAmount = request.Amount / activeMembers.Count;
                
                foreach (var member in activeMembers)
                {
                    record.SplitDetails[member.UserId] = equalAmount;
                }
            }

            var recordId = await _sharedAccountingService.CreateRecordAsync(record);
            var createdRecord = await _sharedAccountingService.GetRecordByIdAsync(recordId);

            // 發送即時通知
            await _hubContext.Clients.Group($"Family_{family.Id}")
                .SendAsync("ExpenseAdded", new FamilyNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = NotificationType.ExpenseAdded,
                    Title = "新增支出記錄",
                    Message = $"{createdRecord!.UserNickname} 新增了一筆{createdRecord.Type}: {createdRecord.Description} ${createdRecord.Amount:N0}",
                    Data = createdRecord,
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("用戶 {UserId} 新增快速支出記錄: {Amount}", userId, request.Amount);

            return Ok(new FamilySharingResponse<SharedAccountingRecord>
            {
                Success = true,
                Message = "記錄新增成功",
                Data = createdRecord
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new FamilySharingResponse<object>
            {
                Success = false,
                Message = ex.Message,
                ErrorCode = "INVALID_OPERATION"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增快速支出記錄時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "新增失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 更新支出記錄
    /// </summary>
    [HttpPut("records/{recordId}")]
    public async Task<IActionResult> UpdateRecord(string recordId, [FromBody] SharedAccountingRecord record)
    {
        try
        {
            if (recordId != record.Id)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "記錄 ID 不一致",
                    ErrorCode = "ID_MISMATCH"
                });
            }

            var userId = GetCurrentUserId();
            record.LastModifiedBy = userId;

            var success = await _sharedAccountingService.UpdateRecordAsync(record);
            
            if (!success)
            {
                return NotFound(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "找不到指定的記錄",
                    ErrorCode = "RECORD_NOT_FOUND"
                });
            }

            // 發送即時通知
            await _hubContext.Clients.Group($"Family_{record.FamilyId}")
                .SendAsync("ExpenseUpdated", new FamilyNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = NotificationType.ExpenseUpdated,
                    Title = "支出記錄更新",
                    Message = $"{record.UserNickname} 更新了記錄: {record.Description}",
                    Data = record,
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("用戶 {UserId} 更新支出記錄: {RecordId}", userId, recordId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "記錄更新成功"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新支出記錄時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "更新失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 刪除支出記錄
    /// </summary>
    [HttpDelete("records/{recordId}")]
    public async Task<IActionResult> DeleteRecord(string recordId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var record = await _sharedAccountingService.GetRecordByIdAsync(recordId);
            
            if (record == null)
            {
                return NotFound(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "找不到指定的記錄",
                    ErrorCode = "RECORD_NOT_FOUND"
                });
            }

            var success = await _sharedAccountingService.DeleteRecordAsync(recordId, userId);
            
            if (!success)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "刪除記錄失敗",
                    ErrorCode = "DELETE_FAILED"
                });
            }

            // 取得用戶暱稱用於通知
            var member = await _familyService.GetMemberAsync(record.FamilyId, userId);
            var memberNickname = member?.Nickname ?? "未知用戶";

            // 發送即時通知
            await _hubContext.Clients.Group($"Family_{record.FamilyId}")
                .SendAsync("ExpenseDeleted", new FamilyNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = NotificationType.ExpenseDeleted,
                    Title = "支出記錄刪除",
                    Message = $"{memberNickname} 刪除了支出記錄: {record.Description}",
                    Data = new { RecordId = recordId, DeletedBy = memberNickname },
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("用戶 {UserId} 刪除支出記錄: {RecordId}", userId, recordId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "記錄刪除成功"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除支出記錄時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "刪除失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 取得待審核記錄列表
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals()
    {
        try
        {
            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            var records = await _sharedAccountingService.GetPendingApprovalsAsync(family.Id);

            return Ok(new FamilySharingResponse<List<SharedAccountingRecord>>
            {
                Success = true,
                Message = "取得成功",
                Data = records
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得待審核記錄時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 審核支出記錄
    /// </summary>
    [HttpPost("records/{recordId}/approve")]
    public async Task<IActionResult> ApproveRecord(string recordId, [FromBody] ApprovalRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            if (request.Approved)
            {
                var success = await _sharedAccountingService.ApproveRecordAsync(recordId, userId);
                if (!success)
                {
                    return BadRequest(new FamilySharingResponse<object>
                    {
                        Success = false,
                        Message = "審核失敗",
                        ErrorCode = "APPROVAL_FAILED"
                    });
                }
            }
            else
            {
                var success = await _sharedAccountingService.RejectRecordAsync(recordId, userId, request.Reason ?? "未提供拒絕原因");
                if (!success)
                {
                    return BadRequest(new FamilySharingResponse<object>
                    {
                        Success = false,
                        Message = "拒絕失敗",
                        ErrorCode = "REJECTION_FAILED"
                    });
                }
            }

            // 取得記錄和審核者資訊
            var record = await _sharedAccountingService.GetRecordByIdAsync(recordId);
            var member = await _familyService.GetMemberAsync(record!.FamilyId, userId);

            // 發送即時通知
            await _hubContext.Clients.Group($"Family_{record.FamilyId}")
                .SendAsync("RecordApproval", new FamilyNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = request.Approved ? NotificationType.ExpenseApproved : NotificationType.ExpenseRejected,
                    Title = request.Approved ? "支出記錄審核通過" : "支出記錄審核拒絕",
                    Message = $"{member!.Nickname} {(request.Approved ? "通過" : "拒絕")}了支出記錄的審核: {record.Description}",
                    Data = new { RecordId = recordId, Approved = request.Approved, ReviewerNickname = member.Nickname },
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("用戶 {UserId} {Action} 支出記錄審核: {RecordId}", 
                userId, request.Approved ? "通過" : "拒絕", recordId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = request.Approved ? "審核通過" : "審核拒絕"
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "審核支出記錄時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "審核失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 取得家庭統計資料
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(int? year = null, int? month = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            // 預設使用當前年月
            var currentDate = DateTime.Now;
            var targetYear = year ?? currentDate.Year;
            var targetMonth = month ?? currentDate.Month;

            var statistics = await _sharedAccountingService.GetFamilyStatisticsAsync(family.Id, targetYear, targetMonth);

            return Ok(new FamilySharingResponse<FamilyStatistics>
            {
                Success = true,
                Message = "取得成功",
                Data = statistics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭統計資料時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 取得當前預算資訊
    /// </summary>
    [HttpGet("current-budget")]
    public async Task<IActionResult> GetCurrentBudget()
    {
        try
        {
            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            var budget = await _sharedAccountingService.GetCurrentBudgetAsync(family.Id);

            return Ok(new FamilySharingResponse<SharedBudget?>
            {
                Success = true,
                Message = "取得成功",
                Data = budget
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得當前預算時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 建立或更新預算
    /// </summary>
    [HttpPost("budget")]
    public async Task<IActionResult> CreateBudget([FromBody] SharedBudget budget)
    {
        try
        {
            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            // 檢查權限
            var member = await _familyService.GetMemberAsync(family.Id, userId);
            if (member == null || member.Role != "admin")
            {
                return Forbid();
            }

            budget.FamilyId = family.Id;
            budget.CreatedBy = userId;
            
            var budgetId = await _sharedAccountingService.CreateBudgetAsync(budget);

            _logger.LogInformation("用戶 {UserId} 建立家庭預算: {BudgetId}", userId, budgetId);

            return Ok(new FamilySharingResponse<string>
            {
                Success = true,
                Message = "預算建立成功",
                Data = budgetId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立預算時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "建立失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 計算自訂分攤金額
    /// </summary>
    [HttpPost("calculate-split")]
    public async Task<IActionResult> CalculateSplit([FromBody] CalculateSplitRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var family = await _familyService.GetUserFamilyAsync(userId);
            
            if (family == null)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組",
                    ErrorCode = "NO_FAMILY"
                });
            }

            var splitDetails = await _sharedAccountingService.CalculateSplitAmountsAsync(
                request.TotalAmount, 
                request.SplitType, 
                family.Id, 
                request.CustomSplit);

            return Ok(new FamilySharingResponse<Dictionary<string, decimal>>
            {
                Success = true,
                Message = "計算成功",
                Data = splitDetails
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "計算分攤金額時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "計算失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 取得當前用戶 ID (暫時模擬，實際應從認證系統取得)
    /// </summary>
    private string GetCurrentUserId()
    {
        // 暫時模擬用戶 ID，實際應該從 JWT Token 或 Session 中取得
        return "user_" + (HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown");
    }
}

/// <summary>
/// 審核請求資料模型
/// </summary>
public class ApprovalRequest
{
    /// <summary>
    /// 是否通過審核
    /// </summary>
    public bool Approved { get; set; }

    /// <summary>
    /// 拒絕原因 (當 Approved = false 時)
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// 計算分攤請求資料模型
/// </summary>
public class CalculateSplitRequest
{
    /// <summary>
    /// 總金額
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 分攤類型
    /// </summary>
    public string SplitType { get; set; } = string.Empty;

    /// <summary>
    /// 自訂分攤詳細資訊
    /// </summary>
    public Dictionary<string, decimal>? CustomSplit { get; set; }
}
