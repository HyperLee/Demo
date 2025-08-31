using Microsoft.AspNetCore.Mvc;
using Demo.Models;
using Demo.Services;
using Demo.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Demo.Controllers;

/// <summary>
/// 家庭管理 API 控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FamilyManagementController : ControllerBase
{
    private readonly IFamilyService _familyService;
    private readonly IHubContext<FamilyHub> _hubContext;
    private readonly ILogger<FamilyManagementController> _logger;

    public FamilyManagementController(
        IFamilyService familyService,
        IHubContext<FamilyHub> hubContext,
        ILogger<FamilyManagementController> logger)
    {
        _familyService = familyService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// 取得當前用戶的家庭群組資訊
    /// </summary>
    [HttpGet("current-family")]
    public async Task<IActionResult> GetCurrentFamily()
    {
        try
        {
            // 模擬用戶 ID，實際應該從認證系統取得
            var userId = GetCurrentUserId();
            
            var family = await _familyService.GetUserFamilyAsync(userId);
            if (family == null)
            {
                return Ok(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "尚未加入任何家庭群組"
                });
            }

            var members = await _familyService.GetFamilyMembersAsync(family.Id);
            
            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "取得成功",
                Data = new { Family = family, Members = members }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得當前家庭群組時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 建立新的家庭群組
    /// </summary>
    [HttpPost("create")]
    public async Task<IActionResult> CreateFamily([FromBody] CreateFamilyRequest request)
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
            var familyId = await _familyService.CreateFamilyAsync(request, userId);

            var family = await _familyService.GetFamilyByIdAsync(familyId);
            var members = await _familyService.GetFamilyMembersAsync(familyId);

            _logger.LogInformation("用戶 {UserId} 建立家庭群組 {FamilyId}", userId, familyId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "家庭群組建立成功",
                Data = new { Family = family, Members = members }
            });
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
            _logger.LogError(ex, "建立家庭群組時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "建立失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 加入家庭群組
    /// </summary>
    [HttpPost("join")]
    public async Task<IActionResult> JoinFamily([FromBody] JoinFamilyRequest request)
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
            
            var success = await _familyService.UseInviteAsync(request.InviteCode, userId, request.Nickname);
            if (!success)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "邀請碼無效或已過期",
                    ErrorCode = "INVALID_INVITE_CODE"
                });
            }

            // 取得加入的家庭群組資訊
            var family = await _familyService.GetUserFamilyAsync(userId);
            var member = await _familyService.GetMemberAsync(family!.Id, userId);

            // 通知其他成員有新成員加入
            await _hubContext.Clients.Group($"Family_{family.Id}")
                .SendAsync("MemberJoined", new FamilyNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = NotificationType.MemberJoined,
                    Title = "新成員加入",
                    Message = $"{request.Nickname} 加入了家庭群組",
                    Data = member,
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("用戶 {UserId} 加入家庭群組 {FamilyId}", userId, family.Id);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "成功加入家庭群組",
                Data = new { Family = family, Member = member }
            });
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
            _logger.LogError(ex, "加入家庭群組時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "加入失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 產生家庭邀請連結
    /// </summary>
    [HttpPost("generate-invite")]
    public async Task<IActionResult> GenerateInvite([FromBody] GenerateInviteRequest request)
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

            // 檢查用戶權限
            var member = await _familyService.GetMemberAsync(family.Id, userId);
            if (member == null || !member.Permissions.CanManageUsers)
            {
                return Forbid();
            }

            var invite = await _familyService.CreateInviteAsync(
                family.Id, 
                userId, 
                request.MaxUses, 
                request.ExpiryDays);

            _logger.LogInformation("用戶 {UserId} 為家庭 {FamilyId} 產生邀請連結", userId, family.Id);

            return Ok(new FamilySharingResponse<FamilyInvite>
            {
                Success = true,
                Message = "邀請連結產生成功",
                Data = invite
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "產生邀請連結時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "產生失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 取得家庭成員列表
    /// </summary>
    [HttpGet("members")]
    public async Task<IActionResult> GetMembers()
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

            var members = await _familyService.GetFamilyMembersAsync(family.Id);

            return Ok(new FamilySharingResponse<List<FamilyMember>>
            {
                Success = true,
                Message = "取得成功",
                Data = members
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭成員列表時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 更新成員權限
    /// </summary>
    [HttpPut("members/{memberId}/permissions")]
    public async Task<IActionResult> UpdateMemberPermissions(string memberId, [FromBody] MemberPermissions permissions)
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

            // 檢查管理者權限
            var currentMember = await _familyService.GetMemberAsync(family.Id, userId);
            if (currentMember == null || !currentMember.Permissions.CanManageUsers)
            {
                return Forbid();
            }

            // 找到目標成員
            var members = await _familyService.GetFamilyMembersAsync(family.Id);
            var targetMember = members.FirstOrDefault(m => m.Id == memberId);
            
            if (targetMember == null)
            {
                return NotFound(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "找不到指定的成員",
                    ErrorCode = "MEMBER_NOT_FOUND"
                });
            }

            var success = await _familyService.UpdateMemberPermissionsAsync(family.Id, targetMember.UserId, permissions);
            
            if (!success)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "更新權限失敗",
                    ErrorCode = "UPDATE_FAILED"
                });
            }

            _logger.LogInformation("用戶 {UserId} 更新成員 {MemberId} 權限", userId, memberId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "權限更新成功"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新成員權限時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "更新失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 移除家庭成員
    /// </summary>
    [HttpDelete("members/{memberId}")]
    public async Task<IActionResult> RemoveMember(string memberId)
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

            // 檢查管理者權限
            var currentMember = await _familyService.GetMemberAsync(family.Id, userId);
            if (currentMember == null || !currentMember.Permissions.CanManageUsers)
            {
                return Forbid();
            }

            // 找到目標成員
            var members = await _familyService.GetFamilyMembersAsync(family.Id);
            var targetMember = members.FirstOrDefault(m => m.Id == memberId);
            
            if (targetMember == null)
            {
                return NotFound(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "找不到指定的成員",
                    ErrorCode = "MEMBER_NOT_FOUND"
                });
            }

            var success = await _familyService.RemoveMemberAsync(family.Id, targetMember.UserId);
            
            if (!success)
            {
                return BadRequest(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "移除成員失敗",
                    ErrorCode = "REMOVE_FAILED"
                });
            }

            // 通知其他成員有成員離開
            await _hubContext.Clients.Group($"Family_{family.Id}")
                .SendAsync("MemberLeft", new FamilyNotification
                {
                    Id = Guid.NewGuid().ToString(),
                    Type = NotificationType.MemberLeft,
                    Title = "成員離開",
                    Message = $"{targetMember.Nickname} 已離開家庭群組",
                    Data = new { Nickname = targetMember.Nickname },
                    Timestamp = DateTime.UtcNow
                });

            _logger.LogInformation("用戶 {UserId} 移除成員 {MemberId}", userId, memberId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "成員移除成功"
            });
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
            _logger.LogError(ex, "移除成員時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "移除失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 取得家庭邀請列表
    /// </summary>
    [HttpGet("invites")]
    public async Task<IActionResult> GetInvites()
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

            var invites = await _familyService.GetFamilyInvitesAsync(family.Id);

            return Ok(new FamilySharingResponse<List<FamilyInvite>>
            {
                Success = true,
                Message = "取得成功",
                Data = invites
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得邀請列表時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "取得失敗",
                ErrorCode = "INTERNAL_ERROR"
            });
        }
    }

    /// <summary>
    /// 撤銷邀請
    /// </summary>
    [HttpDelete("invites/{inviteId}")]
    public async Task<IActionResult> RevokeInvite(string inviteId)
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

            // 檢查管理者權限
            var member = await _familyService.GetMemberAsync(family.Id, userId);
            if (member == null || !member.Permissions.CanManageUsers)
            {
                return Forbid();
            }

            var success = await _familyService.RevokeInviteAsync(inviteId);
            
            if (!success)
            {
                return NotFound(new FamilySharingResponse<object>
                {
                    Success = false,
                    Message = "找不到指定的邀請",
                    ErrorCode = "INVITE_NOT_FOUND"
                });
            }

            _logger.LogInformation("用戶 {UserId} 撤銷邀請 {InviteId}", userId, inviteId);

            return Ok(new FamilySharingResponse<object>
            {
                Success = true,
                Message = "邀請撤銷成功"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤銷邀請時發生錯誤");
            return StatusCode(500, new FamilySharingResponse<object>
            {
                Success = false,
                Message = "撤銷失敗",
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
/// 產生邀請請求資料模型
/// </summary>
public class GenerateInviteRequest
{
    /// <summary>
    /// 最大使用次數 (0表示無限制)
    /// </summary>
    public int MaxUses { get; set; } = 1;

    /// <summary>
    /// 過期天數
    /// </summary>
    public int ExpiryDays { get; set; } = 7;
}
