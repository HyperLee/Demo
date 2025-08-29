using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

/// <summary>
/// 記帳記錄資料模型
/// </summary>
public class AccountingRecord
{
    /// <summary>
    /// 記錄 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 記錄日期
    /// </summary>
    [Required(ErrorMessage = "日期不可為空")]
    public DateTime Date { get; set; }

    /// <summary>
    /// 收支類型 (Income 或 Expense)
    /// </summary>
    [Required(ErrorMessage = "請選擇收支類型")]
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 金額 (使用 decimal 避免浮點精度問題)
    /// </summary>
    [Required(ErrorMessage = "金額不可為空")]
    [Range(0, double.MaxValue, ErrorMessage = "金額必須為正數")]
    public decimal Amount { get; set; }

    /// <summary>
    /// 大分類
    /// </summary>
    [Required(ErrorMessage = "請選擇大分類")]
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 細分類
    /// </summary>
    public string SubCategory { get; set; } = string.Empty;

    /// <summary>
    /// 付款方式
    /// </summary>
    public string PaymentMethod { get; set; } = "現金";

    /// <summary>
    /// 備註
    /// </summary>
    [MaxLength(500, ErrorMessage = "備註不可超過 500 字元")]
    public string Note { get; set; } = string.Empty;

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime ModifiedDate { get; set; }
}

/// <summary>
/// 分類資料模型
/// </summary>
public class AccountingCategory
{
    /// <summary>
    /// 分類 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分類類型 (Income 或 Expense)
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 分類圖示
    /// </summary>
    public string Icon { get; set; } = "fas fa-folder";

    /// <summary>
    /// 子分類清單
    /// </summary>
    public List<AccountingSubCategory> SubCategories { get; set; } = new();
}

/// <summary>
/// 子分類資料模型
/// </summary>
public class AccountingSubCategory
{
    /// <summary>
    /// 子分類 ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 子分類名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// 月度統計摘要
/// </summary>
public class MonthlySummary
{
    /// <summary>
    /// 年份
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// 月份
    /// </summary>
    public int Month { get; set; }

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
    public decimal NetIncome => TotalIncome - TotalExpense;

    /// <summary>
    /// 記錄總數
    /// </summary>
    public int TotalRecords { get; set; }

    /// <summary>
    /// 收入記錄數
    /// </summary>
    public int IncomeRecords { get; set; }

    /// <summary>
    /// 支出記錄數
    /// </summary>
    public int ExpenseRecords { get; set; }
}

/// <summary>
/// 匯出選項
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime StartDate { get; set; }

    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 是否包含收入
    /// </summary>
    public bool IncludeIncome { get; set; } = true;

    /// <summary>
    /// 是否包含支出
    /// </summary>
    public bool IncludeExpense { get; set; } = true;

    /// <summary>
    /// 是否包含統計摘要
    /// </summary>
    public bool IncludeSummary { get; set; } = true;

    /// <summary>
    /// 是否包含分類分析
    /// </summary>
    public bool IncludeCategoryAnalysis { get; set; } = true;

    /// <summary>
    /// 報表標題
    /// </summary>
    public string Title { get; set; } = "記帳報表";
}

/// <summary>
/// 日曆檢視資料模型
/// </summary>
public class CalendarDay
{
    /// <summary>
    /// 日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 是否為當月日期
    /// </summary>
    public bool IsCurrentMonth { get; set; }

    /// <summary>
    /// 是否為今日
    /// </summary>
    public bool IsToday { get; set; }

    /// <summary>
    /// 當日記錄清單
    /// </summary>
    public List<AccountingRecord> Records { get; set; } = new();

    /// <summary>
    /// 當日收入總額
    /// </summary>
    public decimal DailyIncome => Records.Where(r => r.Type == "Income").Sum(r => r.Amount);

    /// <summary>
    /// 當日支出總額
    /// </summary>
    public decimal DailyExpense => Records.Where(r => r.Type == "Expense").Sum(r => r.Amount);

    /// <summary>
    /// 當日淨收支
    /// </summary>
    public decimal DailyNet => DailyIncome - DailyExpense;
}
