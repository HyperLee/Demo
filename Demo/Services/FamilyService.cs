using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 家庭服務介面
/// </summary>
public interface IFamilyService
{
    Task<List<Family>> GetFamiliesAsync();
    Task<Family?> GetFamilyByIdAsync(string familyId);
    Task<Family?> GetUserFamilyAsync(string userId);
    Task<string> CreateFamilyAsync(CreateFamilyRequest request, string userId);
    Task<bool> UpdateFamilyAsync(Family family);
    Task<bool> DeleteFamilyAsync(string familyId);
    
    // 成員管理
    Task<List<FamilyMember>> GetFamilyMembersAsync(string familyId);
    Task<FamilyMember?> GetMemberAsync(string familyId, string userId);
    Task<bool> AddMemberAsync(string familyId, string userId, string nickname, string role = "member");
    Task<bool> UpdateMemberAsync(FamilyMember member);
    Task<bool> RemoveMemberAsync(string familyId, string userId);
    Task<bool> UpdateMemberPermissionsAsync(string familyId, string userId, MemberPermissions permissions);
    
    // 邀請管理
    Task<FamilyInvite> CreateInviteAsync(string familyId, string createdBy, int maxUses = 0, int expiryDays = 7);
    Task<FamilyInvite?> GetInviteByCodeAsync(string inviteCode);
    Task<bool> UseInviteAsync(string inviteCode, string userId, string nickname);
    Task<List<FamilyInvite>> GetFamilyInvitesAsync(string familyId);
    Task<bool> RevokeInviteAsync(string inviteId);
}

/// <summary>
/// 家庭服務實作
/// </summary>
public class FamilyService : IFamilyService
{
    private readonly ILogger<FamilyService> _logger;
    private readonly string _familiesFilePath;
    private readonly string _membersFilePath;
    private readonly string _invitesFilePath;

    public FamilyService(ILogger<FamilyService> logger)
    {
        _logger = logger;
        
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _familiesFilePath = Path.Combine(appDataPath, "families.json");
        _membersFilePath = Path.Combine(appDataPath, "family-members.json");
        _invitesFilePath = Path.Combine(appDataPath, "family-invites.json");
        
        // 初始化預設資料
        _ = Task.Run(InitializeDefaultDataAsync);
    }

    #region 家庭群組管理

    public async Task<List<Family>> GetFamiliesAsync()
    {
        try
        {
            return await LoadFamiliesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭群組清單時發生錯誤");
            return new List<Family>();
        }
    }

    public async Task<Family?> GetFamilyByIdAsync(string familyId)
    {
        try
        {
            var families = await LoadFamiliesAsync();
            return families.FirstOrDefault(f => f.Id == familyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭群組 {FamilyId} 時發生錯誤", familyId);
            return null;
        }
    }

    public async Task<Family?> GetUserFamilyAsync(string userId)
    {
        try
        {
            var members = await LoadMembersAsync();
            var userMember = members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
            
            if (userMember == null)
                return null;

            return await GetFamilyByIdAsync(userMember.FamilyId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得用戶 {UserId} 的家庭群組時發生錯誤", userId);
            return null;
        }
    }

    public async Task<string> CreateFamilyAsync(CreateFamilyRequest request, string userId)
    {
        try
        {
            // 檢查用戶是否已經在其他家庭群組中
            var existingFamily = await GetUserFamilyAsync(userId);
            if (existingFamily != null)
            {
                throw new InvalidOperationException("用戶已加入其他家庭群組");
            }

            var families = await LoadFamiliesAsync();
            
            var newFamily = new Family
            {
                Id = $"family_{Guid.NewGuid():N}",
                Name = request.Name,
                Description = request.Description,
                CreatedBy = userId,
                CreatedAt = DateTime.UtcNow,
                Settings = new FamilySettings
                {
                    Currency = request.Currency,
                    RequireApprovalForLargeExpense = request.RequireApproval,
                    LargeExpenseThreshold = request.ApprovalThreshold
                }
            };

            families.Add(newFamily);
            await SaveFamiliesAsync(families);

            // 將建立者加入為管理員
            await AddMemberAsync(newFamily.Id, userId, "管理員", "admin");

            _logger.LogInformation("建立家庭群組: {FamilyId}, {FamilyName}", newFamily.Id, newFamily.Name);
            return newFamily.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立家庭群組時發生錯誤");
            throw;
        }
    }

    public async Task<bool> UpdateFamilyAsync(Family family)
    {
        try
        {
            var families = await LoadFamiliesAsync();
            var index = families.FindIndex(f => f.Id == family.Id);
            
            if (index == -1)
                return false;

            families[index] = family;
            await SaveFamiliesAsync(families);

            _logger.LogInformation("更新家庭群組: {FamilyId}", family.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新家庭群組 {FamilyId} 時發生錯誤", family.Id);
            return false;
        }
    }

    public async Task<bool> DeleteFamilyAsync(string familyId)
    {
        try
        {
            var families = await LoadFamiliesAsync();
            var family = families.FirstOrDefault(f => f.Id == familyId);
            
            if (family == null)
                return false;

            families.Remove(family);
            await SaveFamiliesAsync(families);

            // 同時移除所有成員
            var members = await LoadMembersAsync();
            members.RemoveAll(m => m.FamilyId == familyId);
            await SaveMembersAsync(members);

            // 同時移除所有邀請
            var invites = await LoadInvitesAsync();
            invites.RemoveAll(i => i.FamilyId == familyId);
            await SaveInvitesAsync(invites);

            _logger.LogInformation("刪除家庭群組: {FamilyId}", familyId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除家庭群組 {FamilyId} 時發生錯誤", familyId);
            return false;
        }
    }

    #endregion

    #region 成員管理

    public async Task<List<FamilyMember>> GetFamilyMembersAsync(string familyId)
    {
        try
        {
            var members = await LoadMembersAsync();
            return members.Where(m => m.FamilyId == familyId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭 {FamilyId} 成員清單時發生錯誤", familyId);
            return new List<FamilyMember>();
        }
    }

    public async Task<FamilyMember?> GetMemberAsync(string familyId, string userId)
    {
        try
        {
            var members = await LoadMembersAsync();
            return members.FirstOrDefault(m => m.FamilyId == familyId && m.UserId == userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭 {FamilyId} 成員 {UserId} 時發生錯誤", familyId, userId);
            return null;
        }
    }

    public async Task<bool> AddMemberAsync(string familyId, string userId, string nickname, string role = "member")
    {
        try
        {
            // 檢查用戶是否已經在其他家庭群組中
            var members = await LoadMembersAsync();
            var existingMember = members.FirstOrDefault(m => m.UserId == userId && m.IsActive);
            
            if (existingMember != null && existingMember.FamilyId != familyId)
            {
                throw new InvalidOperationException("用戶已加入其他家庭群組");
            }

            // 檢查是否已經是此家庭的成員
            if (existingMember?.FamilyId == familyId)
            {
                return false;
            }

            var permissions = role == "admin" 
                ? new MemberPermissions 
                { 
                    CanAddExpense = true,
                    CanEditExpense = true,
                    CanDeleteExpense = true,
                    CanManageUsers = true,
                    CanViewReports = true,
                    CanExportData = true
                }
                : new MemberPermissions();

            var newMember = new FamilyMember
            {
                Id = $"member_{Guid.NewGuid():N}",
                FamilyId = familyId,
                UserId = userId,
                Nickname = nickname,
                Role = role,
                Permissions = permissions,
                JoinedAt = DateTime.UtcNow,
                IsActive = true
            };

            members.Add(newMember);
            await SaveMembersAsync(members);

            _logger.LogInformation("新增家庭成員: {FamilyId}, {UserId}, {Nickname}", familyId, userId, nickname);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增家庭成員時發生錯誤");
            throw;
        }
    }

    public async Task<bool> UpdateMemberAsync(FamilyMember member)
    {
        try
        {
            var members = await LoadMembersAsync();
            var index = members.FindIndex(m => m.Id == member.Id);
            
            if (index == -1)
                return false;

            members[index] = member;
            await SaveMembersAsync(members);

            _logger.LogInformation("更新家庭成員: {MemberId}", member.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新家庭成員 {MemberId} 時發生錯誤", member.Id);
            return false;
        }
    }

    public async Task<bool> RemoveMemberAsync(string familyId, string userId)
    {
        try
        {
            var members = await LoadMembersAsync();
            var member = members.FirstOrDefault(m => m.FamilyId == familyId && m.UserId == userId);
            
            if (member == null)
                return false;

            // 檢查是否為最後一個管理員
            var adminCount = members.Count(m => m.FamilyId == familyId && m.Role == "admin" && m.IsActive);
            if (member.Role == "admin" && adminCount <= 1)
            {
                throw new InvalidOperationException("不能移除最後一個管理員");
            }

            member.IsActive = false;
            await SaveMembersAsync(members);

            _logger.LogInformation("移除家庭成員: {FamilyId}, {UserId}", familyId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移除家庭成員時發生錯誤");
            throw;
        }
    }

    public async Task<bool> UpdateMemberPermissionsAsync(string familyId, string userId, MemberPermissions permissions)
    {
        try
        {
            var members = await LoadMembersAsync();
            var member = members.FirstOrDefault(m => m.FamilyId == familyId && m.UserId == userId);
            
            if (member == null)
                return false;

            member.Permissions = permissions;
            await SaveMembersAsync(members);

            _logger.LogInformation("更新成員權限: {FamilyId}, {UserId}", familyId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新成員權限時發生錯誤");
            return false;
        }
    }

    #endregion

    #region 邀請管理

    public async Task<FamilyInvite> CreateInviteAsync(string familyId, string createdBy, int maxUses = 0, int expiryDays = 7)
    {
        try
        {
            var invites = await LoadInvitesAsync();
            
            // 產生邀請碼 (8位數字母)
            var inviteCode = GenerateInviteCode();
            
            var invite = new FamilyInvite
            {
                Id = $"invite_{Guid.NewGuid():N}",
                FamilyId = familyId,
                InviteCode = inviteCode,
                InviteLink = $"/family-management/join?code={inviteCode}",
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
                MaxUses = maxUses,
                UsedCount = 0
            };

            invites.Add(invite);
            await SaveInvitesAsync(invites);

            _logger.LogInformation("建立家庭邀請: {InviteId}, {InviteCode}", invite.Id, invite.InviteCode);
            return invite;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立家庭邀請時發生錯誤");
            throw;
        }
    }

    public async Task<FamilyInvite?> GetInviteByCodeAsync(string inviteCode)
    {
        try
        {
            var invites = await LoadInvitesAsync();
            return invites.FirstOrDefault(i => i.InviteCode == inviteCode && i.IsValid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得邀請碼 {InviteCode} 時發生錯誤", inviteCode);
            return null;
        }
    }

    public async Task<bool> UseInviteAsync(string inviteCode, string userId, string nickname)
    {
        try
        {
            var invites = await LoadInvitesAsync();
            var invite = invites.FirstOrDefault(i => i.InviteCode == inviteCode && i.IsValid);
            
            if (invite == null)
                return false;

            // 檢查使用次數限制
            if (invite.MaxUses > 0 && invite.UsedCount >= invite.MaxUses)
                return false;

            // 檢查家庭群組是否存在
            var family = await GetFamilyByIdAsync(invite.FamilyId);
            if (family == null)
                return false;

            // 加入家庭
            await AddMemberAsync(invite.FamilyId, userId, nickname);

            // 更新邀請使用次數
            invite.UsedCount++;
            await SaveInvitesAsync(invites);

            _logger.LogInformation("使用邀請碼加入家庭: {InviteCode}, {UserId}", inviteCode, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "使用邀請碼時發生錯誤");
            return false;
        }
    }

    public async Task<List<FamilyInvite>> GetFamilyInvitesAsync(string familyId)
    {
        try
        {
            var invites = await LoadInvitesAsync();
            return invites.Where(i => i.FamilyId == familyId).OrderByDescending(i => i.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭 {FamilyId} 邀請清單時發生錯誤", familyId);
            return new List<FamilyInvite>();
        }
    }

    public async Task<bool> RevokeInviteAsync(string inviteId)
    {
        try
        {
            var invites = await LoadInvitesAsync();
            var invite = invites.FirstOrDefault(i => i.Id == inviteId);
            
            if (invite == null)
                return false;

            invites.Remove(invite);
            await SaveInvitesAsync(invites);

            _logger.LogInformation("撤銷邀請: {InviteId}", inviteId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "撤銷邀請 {InviteId} 時發生錯誤", inviteId);
            return false;
        }
    }

    #endregion

    #region 私有方法

    private async Task<List<Family>> LoadFamiliesAsync()
    {
        if (!File.Exists(_familiesFilePath))
            return new List<Family>();

        var json = await File.ReadAllTextAsync(_familiesFilePath);
        var data = JsonSerializer.Deserialize<FamilyData>(json);
        return data?.Families ?? new List<Family>();
    }

    private async Task SaveFamiliesAsync(List<Family> families)
    {
        var data = new FamilyData { Families = families };
        var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(_familiesFilePath, json);
    }

    private async Task<List<FamilyMember>> LoadMembersAsync()
    {
        if (!File.Exists(_membersFilePath))
            return new List<FamilyMember>();

        var json = await File.ReadAllTextAsync(_membersFilePath);
        var data = JsonSerializer.Deserialize<FamilyMemberData>(json);
        return data?.FamilyMembers ?? new List<FamilyMember>();
    }

    private async Task SaveMembersAsync(List<FamilyMember> members)
    {
        var data = new FamilyMemberData { FamilyMembers = members };
        var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(_membersFilePath, json);
    }

    private async Task<List<FamilyInvite>> LoadInvitesAsync()
    {
        if (!File.Exists(_invitesFilePath))
            return new List<FamilyInvite>();

        var json = await File.ReadAllTextAsync(_invitesFilePath);
        var data = JsonSerializer.Deserialize<FamilyInviteData>(json);
        return data?.FamilyInvites ?? new List<FamilyInvite>();
    }

    private async Task SaveInvitesAsync(List<FamilyInvite> invites)
    {
        var data = new FamilyInviteData { FamilyInvites = invites };
        var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(_invitesFilePath, json);
    }

    private async Task InitializeDefaultDataAsync()
    {
        try
        {
            await LoadFamiliesAsync();
            await LoadMembersAsync();
            await LoadInvitesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化家庭服務預設資料時發生錯誤");
        }
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    #endregion
}

/// <summary>
/// 家庭資料容器
/// </summary>
internal class FamilyData
{
    public List<Family> Families { get; set; } = new();
}

/// <summary>
/// 家庭成員資料容器
/// </summary>
internal class FamilyMemberData
{
    public List<FamilyMember> FamilyMembers { get; set; } = new();
}

/// <summary>
/// 家庭邀請資料容器
/// </summary>
internal class FamilyInviteData
{
    public List<FamilyInvite> FamilyInvites { get; set; } = new();
}
