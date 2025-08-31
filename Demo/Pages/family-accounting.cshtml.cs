using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.SignalR;
using Demo.Hubs;

namespace Demo.Pages
{
    public class FamilyAccountingModel : PageModel
    {
        private readonly ISharedAccountingService _sharedAccountingService;
        private readonly IFamilyService _familyService;
        private readonly IAccountingService _accountingService;
        private readonly IHubContext<FamilyHub> _hubContext;
        private readonly ILogger<FamilyAccountingModel> _logger;

        public FamilyAccountingModel(
            ISharedAccountingService sharedAccountingService,
            IFamilyService familyService,
            IAccountingService accountingService,
            IHubContext<FamilyHub> hubContext,
            ILogger<FamilyAccountingModel> logger)
        {
            _sharedAccountingService = sharedAccountingService;
            _familyService = familyService;
            _accountingService = accountingService;
            _hubContext = hubContext;
            _logger = logger;
        }

        [BindProperty]
        public Family? CurrentFamily { get; set; }

        [BindProperty]
        public List<FamilyMember> Members { get; set; } = new();

        [BindProperty]
        public List<SharedAccountingRecord> Records { get; set; } = new();

        [BindProperty]
        public List<SharedAccountingRecord> PendingApprovals { get; set; } = new();

        [BindProperty]
        public FamilyStatistics Statistics { get; set; } = new();

        [BindProperty]
        public SharedBudget? CurrentBudget { get; set; }

        [BindProperty]
        public List<AccountingCategory> Categories { get; set; } = new();

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

                if (CurrentFamily == null)
                {
                    return RedirectToPage("/family-management");
                }

                // 載入基本資料
                Members = await _familyService.GetFamilyMembersAsync(CurrentFamily.Id);
                Records = await _sharedAccountingService.GetRecordsAsync(CurrentFamily.Id);
                PendingApprovals = await _sharedAccountingService.GetPendingApprovalsAsync(CurrentFamily.Id);

                // 載入統計資料
                var currentDate = DateTime.Now;
                Statistics = await _sharedAccountingService.GetFamilyStatisticsAsync(
                    CurrentFamily.Id, currentDate.Year, currentDate.Month);

                // 載入當前預算
                CurrentBudget = await _sharedAccountingService.GetCurrentBudgetAsync(CurrentFamily.Id);

                // 載入支出類別
                Categories = await _accountingService.GetCategoriesAsync("支出");

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入共享記帳頁面時發生錯誤");
                ErrorMessage = "載入頁面時發生錯誤，請重新載入頁面";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostQuickExpenseAsync(QuickExpenseRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ErrorMessage = "請檢查輸入的資料";
                    return await OnGetAsync();
                }

                var userId = GetCurrentUserId();
                var family = await _familyService.GetUserFamilyAsync(userId);

                if (family == null)
                {
                    ErrorMessage = "尚未加入任何家庭群組";
                    return await OnGetAsync();
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
                    SplitDetails = new Dictionary<string, decimal>()
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
                else if (request.SplitType == "自訂分攤" && request.SplitDetails != null)
                {
                    record.SplitDetails = request.SplitDetails;
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

                SuccessMessage = $"成功新增{request.Type}: {request.Description} ${request.Amount:N0}";
                return RedirectToPage();
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "您沒有新增支出的權限";
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "新增快速支出記錄時發生錯誤");
                ErrorMessage = "新增支出記錄時發生錯誤，請重試";
                return await OnGetAsync();
            }
        }

        public async Task<IActionResult> OnPostDeleteRecordAsync(string recordId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var record = await _sharedAccountingService.GetRecordByIdAsync(recordId);
                
                if (record == null)
                {
                    ErrorMessage = "找不到指定的記錄";
                    return await OnGetAsync();
                }

                var success = await _sharedAccountingService.DeleteRecordAsync(recordId, userId);
                
                if (success)
                {
                    // 取得用戶暱稱
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

                    SuccessMessage = "記錄刪除成功";
                }
                else
                {
                    ErrorMessage = "刪除記錄失敗";
                }

                return RedirectToPage();
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "您沒有刪除此記錄的權限";
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除支出記錄時發生錯誤");
                ErrorMessage = "刪除支出記錄時發生錯誤，請重試";
                return await OnGetAsync();
            }
        }

        public async Task<IActionResult> OnPostApproveRecordAsync(string recordId, bool approved, string? reason = null)
        {
            try
            {
                var userId = GetCurrentUserId();
                
                bool success;
                if (approved)
                {
                    success = await _sharedAccountingService.ApproveRecordAsync(recordId, userId);
                }
                else
                {
                    success = await _sharedAccountingService.RejectRecordAsync(recordId, userId, reason ?? "未提供拒絕原因");
                }

                if (success)
                {
                    var record = await _sharedAccountingService.GetRecordByIdAsync(recordId);
                    var member = await _familyService.GetMemberAsync(record!.FamilyId, userId);

                    // 發送即時通知
                    await _hubContext.Clients.Group($"Family_{record.FamilyId}")
                        .SendAsync("RecordApproval", new FamilyNotification
                        {
                            Id = Guid.NewGuid().ToString(),
                            Type = approved ? NotificationType.ExpenseApproved : NotificationType.ExpenseRejected,
                            Title = approved ? "支出記錄審核通過" : "支出記錄審核拒絕",
                            Message = $"{member!.Nickname} {(approved ? "通過" : "拒絕")}了支出記錄的審核: {record.Description}",
                            Data = new { RecordId = recordId, Approved = approved, ReviewerNickname = member.Nickname },
                            Timestamp = DateTime.UtcNow
                        });

                    SuccessMessage = approved ? "審核通過" : "審核拒絕";
                }
                else
                {
                    ErrorMessage = "審核操作失敗";
                }

                return RedirectToPage();
            }
            catch (UnauthorizedAccessException)
            {
                ErrorMessage = "您沒有審核權限";
                return await OnGetAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "審核支出記錄時發生錯誤");
                ErrorMessage = "審核操作時發生錯誤，請重試";
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
        /// 檢查當前用戶是否可以編輯指定記錄
        /// </summary>
        public bool CanEditRecord(SharedAccountingRecord record)
        {
            var userId = GetCurrentUserId();
            var member = Members.FirstOrDefault(m => m.UserId == userId);
            
            if (member == null) return false;
            
            // 記錄的建立者或有編輯權限的成員可以編輯
            return record.UserId == userId || member.Permissions.CanEditExpense;
        }

        /// <summary>
        /// 檢查當前用戶是否可以刪除指定記錄
        /// </summary>
        public bool CanDeleteRecord(SharedAccountingRecord record)
        {
            var userId = GetCurrentUserId();
            var member = Members.FirstOrDefault(m => m.UserId == userId);
            
            if (member == null) return false;
            
            // 記錄的建立者或有刪除權限的成員可以刪除
            return record.UserId == userId || member.Permissions.CanDeleteExpense;
        }

        /// <summary>
        /// 取得記錄狀態對應的 CSS 類別
        /// </summary>
        public string GetStatusBadgeClass(string status)
        {
            return status switch
            {
                "已確認" => "bg-success",
                "待審核" => "bg-warning",
                "已拒絕" => "bg-danger",
                _ => "bg-secondary"
            };
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
