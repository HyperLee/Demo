using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Models;
using Demo.Services;

namespace Demo.Pages
{
    public class FamilyManagementModel : PageModel
    {
        private readonly IFamilyService _familyService;
        private readonly ILogger<FamilyManagementModel> _logger;

        public FamilyManagementModel(IFamilyService familyService, ILogger<FamilyManagementModel> logger)
        {
            _familyService = familyService;
            _logger = logger;
        }

        [BindProperty]
        public Family? CurrentFamily { get; set; }

        [BindProperty]
        public List<FamilyMember> Members { get; set; } = new();

        [BindProperty]
        public List<FamilyInvite> Invites { get; set; } = new();

        [BindProperty]
        public string? ErrorMessage { get; set; }

        [BindProperty]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                CurrentFamily = await _familyService.GetUserFamilyAsync(userId);

                if (CurrentFamily != null)
                {
                    Members = await _familyService.GetFamilyMembersAsync(CurrentFamily.Id);
                    Invites = await _familyService.GetFamilyInvitesAsync(CurrentFamily.Id);
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入家庭管理頁面時發生錯誤");
                ErrorMessage = "載入頁面時發生錯誤，請重新載入頁面";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostCreateFamilyAsync(CreateFamilyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "請檢查輸入的資料";
                    return await OnGetAsync();
                }

                var userId = GetCurrentUserId();
                var familyId = await _familyService.CreateFamilyAsync(request, userId);

                SuccessMessage = $"成功建立家庭群組: {request.Name}";
                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立家庭群組時發生錯誤");
                ErrorMessage = "建立家庭群組時發生錯誤，請重試";
                return await OnGetAsync();
            }
        }

        public async Task<IActionResult> OnPostJoinFamilyAsync(JoinFamilyRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "請檢查輸入的資料";
                    return await OnGetAsync();
                }

                var userId = GetCurrentUserId();
                var success = await _familyService.UseInviteAsync(request.InviteCode, userId, request.Nickname);

                if (success)
                {
                    SuccessMessage = "成功加入家庭群組";
                    return RedirectToPage();
                }
                else
                {
                    ErrorMessage = "邀請碼無效或已過期";
                    return await OnGetAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加入家庭群組時發生錯誤");
                ErrorMessage = "加入家庭群組時發生錯誤，請重試";
                return await OnGetAsync();
            }
        }

        public async Task<IActionResult> OnPostGenerateInviteAsync(int expirationDays = 7)
        {
            try
            {
                var userId = GetCurrentUserId();
                CurrentFamily = await _familyService.GetUserFamilyAsync(userId);

                if (CurrentFamily == null)
                {
                    ErrorMessage = "找不到您的家庭群組";
                    return Page();
                }

                // 檢查權限 - 獲取成員列表
                var members = await _familyService.GetFamilyMembersAsync(CurrentFamily.Id);
                var currentMember = members.FirstOrDefault(m => m.UserId == userId);
                if (currentMember?.Role != "admin")
                {
                    ErrorMessage = "只有管理員可以產生邀請連結";
                    await LoadPageDataAsync();
                    return Page();
                }

                var invite = await _familyService.CreateInviteAsync(CurrentFamily.Id, userId, 0, expirationDays);
                if (invite != null)
                {
                    SuccessMessage = $"邀請連結已產生：{invite.InviteCode}";
                }
                else
                {
                    ErrorMessage = "產生邀請連結失敗";
                }

                await LoadPageDataAsync();
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "產生邀請連結時發生錯誤");
                ErrorMessage = "產生邀請連結時發生錯誤";
                return Page();
            }
        }

        private async Task LoadPageDataAsync()
        {
            if (CurrentFamily != null)
            {
                Members = await _familyService.GetFamilyMembersAsync(CurrentFamily.Id);
                Invites = await _familyService.GetFamilyInvitesAsync(CurrentFamily.Id);
            }
        }

        public async Task<IActionResult> OnPostRemoveMemberAsync(string memberId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var family = await _familyService.GetUserFamilyAsync(userId);

                if (family == null)
                {
                    ErrorMessage = "尚未加入任何家庭群組";
                    return await OnGetAsync();
                }

                // 檢查權限
                var currentMember = await _familyService.GetMemberAsync(family.Id, userId);
                if (currentMember == null || !currentMember.Permissions.CanManageUsers)
                {
                    ErrorMessage = "您沒有移除成員的權限";
                    return await OnGetAsync();
                }

                // 找到目標成員
                var members = await _familyService.GetFamilyMembersAsync(family.Id);
                var targetMember = members.FirstOrDefault(m => m.Id == memberId);

                if (targetMember == null)
                {
                    ErrorMessage = "找不到指定的成員";
                    return await OnGetAsync();
                }

                var success = await _familyService.RemoveMemberAsync(family.Id, targetMember.UserId);

                if (success)
                {
                    SuccessMessage = $"成功移除成員: {targetMember.Nickname}";
                }
                else
                {
                    ErrorMessage = "移除成員失敗";
                }

                return RedirectToPage();
            }
            catch (InvalidOperationException ex)
            {
                ErrorMessage = ex.Message;
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "移除成員時發生錯誤");
                ErrorMessage = "移除成員時發生錯誤，請重試";
                return await OnGetAsync();
            }
        }

        public async Task<IActionResult> OnPostRevokeInviteAsync(string inviteId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var family = await _familyService.GetUserFamilyAsync(userId);

                if (family == null)
                {
                    ErrorMessage = "尚未加入任何家庭群組";
                    return await OnGetAsync();
                }

                // 檢查權限
                var member = await _familyService.GetMemberAsync(family.Id, userId);
                if (member == null || !member.Permissions.CanManageUsers)
                {
                    ErrorMessage = "您沒有撤銷邀請的權限";
                    return await OnGetAsync();
                }

                var success = await _familyService.RevokeInviteAsync(inviteId);

                if (success)
                {
                    SuccessMessage = "邀請撤銷成功";
                }
                else
                {
                    ErrorMessage = "撤銷邀請失敗";
                }

                return RedirectToPage();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "撤銷邀請時發生錯誤");
                ErrorMessage = "撤銷邀請時發生錯誤，請重試";
                return await OnGetAsync();
            }
        }

        /// <summary>
        /// 檢查當前用戶是否為管理員
        /// </summary>
        public bool IsCurrentUserAdmin()
        {
            if (CurrentFamily == null) return false;

            var userId = GetCurrentUserId();
            var member = Members.FirstOrDefault(m => m.UserId == userId);
            return member?.Role == "admin";
        }

        /// <summary>
        /// 取得當前用戶 ID (暫時模擬，實際應從認證系統取得)
        /// </summary>
        private string GetCurrentUserId()
        {
            // 暫時模擬用戶 ID，實際應該從 JWT Token 或 Session 中取得
            // 為測試目的使用固定的用戶 ID
            return "test-user-001";
        }
    }
}
