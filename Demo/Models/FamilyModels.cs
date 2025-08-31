using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

/// <summary>
/// 家庭群組資料模型
/// </summary>
public class Family
{
    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組名稱
    /// </summary>
    [Required(ErrorMessage = "家庭群組名稱不可為空")]
    [StringLength(50, ErrorMessage = "群組名稱不可超過50個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組描述
    /// </summary>
    [StringLength(200, ErrorMessage = "群組描述不可超過200個字元")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 建立者用戶 ID
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 家庭設定
    /// </summary>
    public FamilySettings Settings { get; set; } = new();
}

/// <summary>
/// 家庭設定資料模型
/// </summary>
public class FamilySettings
{
    /// <summary>
    /// 預設幣別
    /// </summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 時區
    /// </summary>
    public string Timezone { get; set; } = "Asia/Taipei";

    /// <summary>
    /// 是否允許訪客檢視
    /// </summary>
    public bool AllowGuestView { get; set; } = false;

    /// <summary>
    /// 大額支出是否需要審核
    /// </summary>
    public bool RequireApprovalForLargeExpense { get; set; } = true;

    /// <summary>
    /// 大額支出門檻
    /// </summary>
    public decimal LargeExpenseThreshold { get; set; } = 5000;
}

/// <summary>
/// 家庭成員資料模型
/// </summary>
public class FamilyMember
{
    /// <summary>
    /// 成員 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 用戶 ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 在群組中的暱稱
    /// </summary>
    [Required(ErrorMessage = "暱稱不可為空")]
    [StringLength(20, ErrorMessage = "暱稱不可超過20個字元")]
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// 成員角色 (admin, member, guest)
    /// </summary>
    public string Role { get; set; } = "member";

    /// <summary>
    /// 成員權限設定
    /// </summary>
    public MemberPermissions Permissions { get; set; } = new();

    /// <summary>
    /// 加入時間
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 是否為活躍成員
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// 成員權限設定
/// </summary>
public class MemberPermissions
{
    /// <summary>
    /// 可以新增支出
    /// </summary>
    public bool CanAddExpense { get; set; } = true;

    /// <summary>
    /// 可以編輯支出
    /// </summary>
    public bool CanEditExpense { get; set; } = true;

    /// <summary>
    /// 可以刪除支出
    /// </summary>
    public bool CanDeleteExpense { get; set; } = false;

    /// <summary>
    /// 可以管理用戶
    /// </summary>
    public bool CanManageUsers { get; set; } = false;

    /// <summary>
    /// 可以檢視報表
    /// </summary>
    public bool CanViewReports { get; set; } = true;

    /// <summary>
    /// 可以匯出資料
    /// </summary>
    public bool CanExportData { get; set; } = true;
}

/// <summary>
/// 共享記帳記錄資料模型
/// </summary>
public class SharedAccountingRecord
{
    /// <summary>
    /// 記錄 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 記錄者用戶 ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 記錄者暱稱
    /// </summary>
    public string UserNickname { get; set; } = string.Empty;

    /// <summary>
    /// 收支類型 (收入/支出)
    /// </summary>
    [Required(ErrorMessage = "請選擇收支類型")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 金額
    /// </summary>
    [Required(ErrorMessage = "金額不可為空")]
    [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 支出類別
    /// </summary>
    [Required(ErrorMessage = "請選擇類別")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 支出描述
    /// </summary>
    [Required(ErrorMessage = "描述不可為空")]
    [StringLength(100, ErrorMessage = "描述不可超過100個字元")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 記帳日期
    /// </summary>
    [Required(ErrorMessage = "日期不可為空")]
    public DateTime Date { get; set; }

    /// <summary>
    /// 分攤類型 (我支付/平均分攤/自訂分攤)
    /// </summary>
    public string SplitType { get; set; } = "我支付";

    /// <summary>
    /// 分攤詳細資訊 (成員ID對應金額)
    /// </summary>
    public Dictionary<string, decimal> SplitDetails { get; set; } = new();

    /// <summary>
    /// 附件清單
    /// </summary>
    public List<string> Attachments { get; set; } = new();

    /// <summary>
    /// 記錄狀態 (已確認/待審核/已拒絕)
    /// </summary>
    public string Status { get; set; } = "已確認";

    /// <summary>
    /// 審核通過者清單
    /// </summary>
    public List<string> ApprovedBy { get; set; } = new();

    /// <summary>
    /// 是否需要審核
    /// </summary>
    public bool NeedsApproval { get; set; } = false;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最後修改時間
    /// </summary>
    public DateTime LastModifiedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 最後修改者
    /// </summary>
    public string LastModifiedBy { get; set; } = string.Empty;
}

/// <summary>
/// 共享預算資料模型
/// </summary>
public class SharedBudget
{
    /// <summary>
    /// 預算 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 預算名稱
    /// </summary>
    [Required(ErrorMessage = "預算名稱不可為空")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 預算週期 (monthly, yearly)
    /// </summary>
    public string Period { get; set; } = "monthly";

    /// <summary>
    /// 年份
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月份 (如果是月預算)
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// 各類別預算設定
    /// </summary>
    public Dictionary<string, BudgetCategory> Categories { get; set; } = new();

    /// <summary>
    /// 總預算
    /// </summary>
    public decimal TotalBudget { get; set; }

    /// <summary>
    /// 已使用金額
    /// </summary>
    public decimal TotalSpent { get; set; }

    /// <summary>
    /// 建立者
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 預算類別資料模型
/// </summary>
public class BudgetCategory
{
    /// <summary>
    /// 預算金額
    /// </summary>
    public decimal Budget { get; set; }

    /// <summary>
    /// 已支出金額
    /// </summary>
    public decimal Spent { get; set; }

    /// <summary>
    /// 剩餘金額
    /// </summary>
    public decimal Remaining => Budget - Spent;

    /// <summary>
    /// 使用率百分比
    /// </summary>
    public decimal UsagePercentage => Budget > 0 ? (Spent / Budget) * 100 : 0;
}

/// <summary>
/// 家庭邀請連結資料模型
/// </summary>
public class FamilyInvite
{
    /// <summary>
    /// 邀請 ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 邀請碼
    /// </summary>
    public string InviteCode { get; set; } = string.Empty;

    /// <summary>
    /// 邀請連結
    /// </summary>
    public string InviteLink { get; set; } = string.Empty;

    /// <summary>
    /// 建立者
    /// </summary>
    public string CreatedBy { get; set; } = string.Empty;

    /// <summary>
    /// 建立時間
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 過期時間
    /// </summary>
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    /// <summary>
    /// 是否有效
    /// </summary>
    public bool IsValid => DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// 最大使用次數 (0表示無限制)
    /// </summary>
    public int MaxUses { get; set; } = 0;

    /// <summary>
    /// 已使用次數
    /// </summary>
    public int UsedCount { get; set; } = 0;
}

/// <summary>
/// 家庭統計資料模型
/// </summary>
public class FamilyStatistics
{
    /// <summary>
    /// 家庭群組 ID
    /// </summary>
    public string FamilyId { get; set; } = string.Empty;

    /// <summary>
    /// 統計期間 (年月)
    /// </summary>
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// 總收入
    /// </summary>
    public decimal TotalIncome { get; set; }

    /// <summary>
    /// 總支出
    /// </summary>
    public decimal TotalExpense { get; set; }

    /// <summary>
    /// 淨收支
    /// </summary>
    public decimal NetAmount => TotalIncome - TotalExpense;

    /// <summary>
    /// 各類別支出統計
    /// </summary>
    public Dictionary<string, decimal> CategoryExpenses { get; set; } = new();

    /// <summary>
    /// 各成員支出統計
    /// </summary>
    public Dictionary<string, decimal> MemberExpenses { get; set; } = new();

    /// <summary>
    /// 待審核項目數量
    /// </summary>
    public int PendingApprovals { get; set; }

    /// <summary>
    /// 預算使用率
    /// </summary>
    public decimal BudgetUsage { get; set; }
}

/// <summary>
/// 家庭共享系統的通用回應資料模型
/// </summary>
public class FamilySharingResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 訊息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 回應資料
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 錯誤代碼
    /// </summary>
    public string? ErrorCode { get; set; }
}

/// <summary>
/// 加入家庭群組請求資料模型
/// </summary>
public class JoinFamilyRequest
{
    /// <summary>
    /// 邀請碼
    /// </summary>
    [Required(ErrorMessage = "邀請碼不可為空")]
    public string InviteCode { get; set; } = string.Empty;

    /// <summary>
    /// 成員暱稱
    /// </summary>
    [Required(ErrorMessage = "暱稱不可為空")]
    [StringLength(20, ErrorMessage = "暱稱不可超過20個字元")]
    public string Nickname { get; set; } = string.Empty;
}

/// <summary>
/// 建立家庭群組請求資料模型
/// </summary>
public class CreateFamilyRequest
{
    /// <summary>
    /// 群組名稱
    /// </summary>
    [Required(ErrorMessage = "群組名稱不可為空")]
    [StringLength(50, ErrorMessage = "群組名稱不可超過50個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 群組描述
    /// </summary>
    [StringLength(200, ErrorMessage = "群組描述不可超過200個字元")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 預設幣別
    /// </summary>
    public string Currency { get; set; } = "TWD";

    /// <summary>
    /// 是否需要大額支出審核
    /// </summary>
    public bool RequireApproval { get; set; } = true;

    /// <summary>
    /// 大額支出門檻
    /// </summary>
    [Range(0, double.MaxValue, ErrorMessage = "門檻金額必須為正數")]
    public decimal ApprovalThreshold { get; set; } = 5000;
}

/// <summary>
/// 快速新增支出請求資料模型
/// </summary>
public class QuickExpenseRequest
{
    /// <summary>
    /// 收支類型
    /// </summary>
    [Required(ErrorMessage = "請選擇收支類型")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 金額
    /// </summary>
    [Required(ErrorMessage = "金額不可為空")]
    [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 類別
    /// </summary>
    [Required(ErrorMessage = "請選擇類別")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    [Required(ErrorMessage = "描述不可為空")]
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 分攤類型
    /// </summary>
    public string SplitType { get; set; } = "我支付";

    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Today;

    /// <summary>
    /// 自訂分攤詳細資訊
    /// </summary>
    public Dictionary<string, decimal>? SplitDetails { get; set; }
}
