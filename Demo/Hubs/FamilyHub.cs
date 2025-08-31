using Microsoft.AspNetCore.SignalR;
using Demo.Models;

namespace Demo.Hubs;

/// <summary>
/// 家庭共享即時通訊 Hub
/// </summary>
public class FamilyHub : Hub
{
    private readonly ILogger<FamilyHub> _logger;

    public FamilyHub(ILogger<FamilyHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 加入家庭群組
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    public async Task JoinFamily(string familyId)
    {
        try
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, GetFamilyGroupName(familyId));
            _logger.LogInformation("用戶 {ConnectionId} 加入家庭群組 {FamilyId}", Context.ConnectionId, familyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加入家庭群組時發生錯誤");
        }
    }

    /// <summary>
    /// 離開家庭群組
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    public async Task LeaveFamily(string familyId)
    {
        try
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, GetFamilyGroupName(familyId));
            _logger.LogInformation("用戶 {ConnectionId} 離開家庭群組 {FamilyId}", Context.ConnectionId, familyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "離開家庭群組時發生錯誤");
        }
    }

    /// <summary>
    /// 通知新增支出記錄
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="record">支出記錄</param>
    public async Task NotifyExpenseAdded(string familyId, SharedAccountingRecord record)
    {
        try
        {
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = NotificationType.ExpenseAdded,
                Title = "新增支出記錄",
                Message = $"{record.UserNickname} 新增了一筆{record.Type}: {record.Description} ${record.Amount:N0}",
                Data = record,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("ExpenseAdded", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 新增支出記錄: {RecordId}", familyId, record.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知新增支出記錄時發生錯誤");
        }
    }

    /// <summary>
    /// 通知支出記錄更新
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="record">支出記錄</param>
    public async Task NotifyExpenseUpdated(string familyId, SharedAccountingRecord record)
    {
        try
        {
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = NotificationType.ExpenseUpdated,
                Title = "支出記錄更新",
                Message = $"{record.UserNickname} 更新了記錄: {record.Description}",
                Data = record,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("ExpenseUpdated", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 支出記錄更新: {RecordId}", familyId, record.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知支出記錄更新時發生錯誤");
        }
    }

    /// <summary>
    /// 通知支出記錄刪除
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="recordId">記錄 ID</param>
    /// <param name="deletedBy">刪除者暱稱</param>
    public async Task NotifyExpenseDeleted(string familyId, string recordId, string deletedBy)
    {
        try
        {
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = NotificationType.ExpenseDeleted,
                Title = "支出記錄刪除",
                Message = $"{deletedBy} 刪除了一筆支出記錄",
                Data = new { RecordId = recordId, DeletedBy = deletedBy },
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("ExpenseDeleted", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 支出記錄刪除: {RecordId}", familyId, recordId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知支出記錄刪除時發生錯誤");
        }
    }

    /// <summary>
    /// 通知成員加入
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="member">新成員資訊</param>
    public async Task NotifyMemberJoined(string familyId, FamilyMember member)
    {
        try
        {
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = NotificationType.MemberJoined,
                Title = "新成員加入",
                Message = $"{member.Nickname} 加入了家庭群組",
                Data = member,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("MemberJoined", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 成員加入: {MemberId}", familyId, member.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知成員加入時發生錯誤");
        }
    }

    /// <summary>
    /// 通知成員離開
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="memberNickname">成員暱稱</param>
    public async Task NotifyMemberLeft(string familyId, string memberNickname)
    {
        try
        {
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = NotificationType.MemberLeft,
                Title = "成員離開",
                Message = $"{memberNickname} 離開了家庭群組",
                Data = new { Nickname = memberNickname },
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("MemberLeft", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 成員離開: {MemberNickname}", familyId, memberNickname);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知成員離開時發生錯誤");
        }
    }

    /// <summary>
    /// 通知記錄審核結果
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="recordId">記錄 ID</param>
    /// <param name="approved">是否通過審核</param>
    /// <param name="reviewerNickname">審核者暱稱</param>
    public async Task NotifyRecordApproval(string familyId, string recordId, bool approved, string reviewerNickname)
    {
        try
        {
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = approved ? NotificationType.ExpenseApproved : NotificationType.ExpenseRejected,
                Title = approved ? "支出記錄審核通過" : "支出記錄審核拒絕",
                Message = $"{reviewerNickname} {(approved ? "通過" : "拒絕")}了一筆支出記錄的審核",
                Data = new { RecordId = recordId, Approved = approved, ReviewerNickname = reviewerNickname },
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("RecordApproval", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 記錄審核結果: {RecordId}, {Approved}", familyId, recordId, approved);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知記錄審核結果時發生錯誤");
        }
    }

    /// <summary>
    /// 通知預算警告
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="category">類別</param>
    /// <param name="usagePercentage">使用率</param>
    public async Task NotifyBudgetWarning(string familyId, string category, decimal usagePercentage)
    {
        try
        {
            var warningType = usagePercentage >= 100 ? "超支" : "即將超支";
            var notification = new FamilyNotification
            {
                Id = Guid.NewGuid().ToString(),
                Type = NotificationType.BudgetWarning,
                Title = $"預算{warningType}警告",
                Message = $"{category} 類別預算已使用 {usagePercentage:F1}%",
                Data = new { Category = category, UsagePercentage = usagePercentage },
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("BudgetWarning", notification);

            _logger.LogInformation("通知家庭 {FamilyId} 預算警告: {Category}, {UsagePercentage}%", familyId, category, usagePercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "通知預算警告時發生錯誤");
        }
    }

    /// <summary>
    /// 發送聊天訊息
    /// </summary>
    /// <param name="familyId">家庭群組 ID</param>
    /// <param name="message">訊息內容</param>
    /// <param name="senderNickname">發送者暱稱</param>
    public async Task SendChatMessage(string familyId, string message, string senderNickname)
    {
        try
        {
            var chatMessage = new FamilyChatMessage
            {
                Id = Guid.NewGuid().ToString(),
                FamilyId = familyId,
                SenderNickname = senderNickname,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await Clients.Group(GetFamilyGroupName(familyId))
                .SendAsync("ChatMessage", chatMessage);

            _logger.LogInformation("發送聊天訊息到家庭 {FamilyId}: {Message}", familyId, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "發送聊天訊息時發生錯誤");
        }
    }

    /// <summary>
    /// 用戶連接時的處理
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("用戶連接到 FamilyHub: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 用戶斷線時的處理
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("用戶從 FamilyHub 斷線: {ConnectionId}", Context.ConnectionId);
        
        if (exception != null)
        {
            _logger.LogError(exception, "用戶斷線時發生錯誤");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 取得家庭群組名稱
    /// </summary>
    private static string GetFamilyGroupName(string familyId)
    {
        return $"Family_{familyId}";
    }
}

/// <summary>
/// 家庭通知資料模型
/// </summary>
public class FamilyNotification
{
    /// <summary>
    /// 通知 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 通知類型
    /// </summary>
    public NotificationType Type { get; set; }

    /// <summary>
    /// 通知標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 通知訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 通知資料
    /// </summary>
    public object? Data { get; set; }

    /// <summary>
    /// 時間戳記
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 家庭聊天訊息資料模型
/// </summary>
public class FamilyChatMessage
{
    /// <summary>
    /// 訊息 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 發送者暱稱
    /// </summary>
    public string SenderNickname { get; set; } = string.Empty;

    /// <summary>
    /// 訊息內容
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 時間戳記
    /// </summary>
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// 通知類型列舉
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// 新增支出記錄
    /// </summary>
    ExpenseAdded,

    /// <summary>
    /// 支出記錄更新
    /// </summary>
    ExpenseUpdated,

    /// <summary>
    /// 支出記錄刪除
    /// </summary>
    ExpenseDeleted,

    /// <summary>
    /// 支出記錄審核通過
    /// </summary>
    ExpenseApproved,

    /// <summary>
    /// 支出記錄審核拒絕
    /// </summary>
    ExpenseRejected,

    /// <summary>
    /// 成員加入
    /// </summary>
    MemberJoined,

    /// <summary>
    /// 成員離開
    /// </summary>
    MemberLeft,

    /// <summary>
    /// 預算警告
    /// </summary>
    BudgetWarning,

    /// <summary>
    /// 聊天訊息
    /// </summary>
    ChatMessage
}
