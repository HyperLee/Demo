using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Models;
using Demo.Services;
using System.ComponentModel.DataAnnotations;

namespace Demo.Pages;

/// <summary>
/// 記帳系統編輯頁面
/// </summary>
public class index8 : PageModel
{
    private readonly ILogger<index8> _logger;
    private readonly IAccountingService _accountingService;

    public index8(ILogger<index8> logger, IAccountingService accountingService)
    {
        _logger = logger;
        _accountingService = accountingService;
    }

    #region 屬性

    /// <summary>
    /// 記帳記錄資料
    /// </summary>
    [BindProperty]
    public AccountingRecordViewModel Record { get; set; } = new();

    /// <summary>
    /// 是否為編輯模式
    /// </summary>
    public bool IsEditMode => Record.Id > 0;

    /// <summary>
    /// 頁面標題
    /// </summary>
    public string PageTitle => IsEditMode ? "編輯記帳記錄" : "新增記帳記錄";

    /// <summary>
    /// 收入分類選項
    /// </summary>
    public List<SelectListItem> IncomeCategoryOptions { get; set; } = new();

    /// <summary>
    /// 支出分類選項
    /// </summary>
    public List<SelectListItem> ExpenseCategoryOptions { get; set; } = new();

    /// <summary>
    /// 付款方式選項
    /// </summary>
    public List<SelectListItem> PaymentMethodOptions { get; set; } = new();

    #endregion

    #region 頁面處理方法

    /// <summary>
    /// 頁面載入處理
    /// </summary>
    public async Task<IActionResult> OnGetAsync(int? id, string? date)
    {
        try
        {
            await LoadSelectListsAsync();

            if (id.HasValue && id.Value > 0)
            {
                // 編輯模式：載入現有記錄
                var existingRecord = await _accountingService.GetRecordByIdAsync(id.Value);
                if (existingRecord == null)
                {
                    TempData["ErrorMessage"] = "找不到要編輯的記錄。";
                    return RedirectToPage("index7");
                }

                Record = new AccountingRecordViewModel
                {
                    Id = existingRecord.Id,
                    Date = existingRecord.Date,
                    Type = existingRecord.Type,
                    Amount = existingRecord.Amount,
                    Category = existingRecord.Category,
                    SubCategory = existingRecord.SubCategory,
                    PaymentMethod = existingRecord.PaymentMethod,
                    Note = existingRecord.Note
                };

                ViewData["Title"] = "編輯記帳記錄";
            }
            else
            {
                // 新增模式：設定預設值
                Record = new AccountingRecordViewModel
                {
                    Date = DateTime.TryParse(date, out var parsedDate) ? parsedDate : DateTime.Today,
                    Type = "Expense", // 預設為支出
                    PaymentMethod = "現金"
                };

                ViewData["Title"] = "新增記帳記錄";
            }

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入記帳編輯頁面時發生錯誤，ID: {Id}", id);
            TempData["ErrorMessage"] = "載入頁面時發生錯誤，請稍後再試。";
            return RedirectToPage("index7");
        }
    }

    /// <summary>
    /// 儲存記錄處理
    /// </summary>
    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await LoadSelectListsAsync();

            if (!ModelState.IsValid)
            {
                ViewData["Title"] = PageTitle;
                return Page();
            }

            // 驗證日期不可為未來
            if (Record.Date.Date > DateTime.Today)
            {
                ModelState.AddModelError("Record.Date", "記錄日期不可為未來日期");
                ViewData["Title"] = PageTitle;
                return Page();
            }

            // 驗證金額
            if (Record.Amount <= 0)
            {
                ModelState.AddModelError("Record.Amount", "金額必須大於 0");
                ViewData["Title"] = PageTitle;
                return Page();
            }

            // 建立記帳記錄物件
            var accountingRecord = new AccountingRecord
            {
                Id = Record.Id,
                Date = Record.Date,
                Type = Record.Type,
                Amount = Record.Amount,
                Category = Record.Category,
                SubCategory = Record.SubCategory ?? string.Empty,
                PaymentMethod = Record.PaymentMethod,
                Note = Record.Note ?? string.Empty
            };

            bool success;
            if (IsEditMode)
            {
                // 更新現有記錄
                success = await _accountingService.UpdateRecordAsync(accountingRecord);
                if (success)
                {
                    TempData["SuccessMessage"] = "記錄已成功更新。";
                    _logger.LogInformation("成功更新記帳記錄 {Id}", Record.Id);
                }
                else
                {
                    TempData["ErrorMessage"] = "更新記錄失敗，可能記錄已被刪除。";
                    _logger.LogWarning("更新記帳記錄失敗 {Id}", Record.Id);
                }
            }
            else
            {
                // 建立新記錄
                var newId = await _accountingService.CreateRecordAsync(accountingRecord);
                success = newId > 0;
                
                if (success)
                {
                    TempData["SuccessMessage"] = "記錄已成功新增。";
                    _logger.LogInformation("成功建立記帳記錄 {Id}", newId);
                }
                else
                {
                    TempData["ErrorMessage"] = "新增記錄失敗，請稍後再試。";
                    _logger.LogError("建立記帳記錄失敗");
                }
            }

            if (success)
            {
                return RedirectToPage("index7", new { 
                    year = Record.Date.Year, 
                    month = Record.Date.Month 
                });
            }
            else
            {
                ViewData["Title"] = PageTitle;
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存記帳記錄時發生錯誤");
            ModelState.AddModelError("", "儲存時發生錯誤，請稍後再試。");
            ViewData["Title"] = PageTitle;
            return Page();
        }
    }

    /// <summary>
    /// 取得子分類 AJAX 處理
    /// </summary>
    public async Task<IActionResult> OnGetSubCategoriesAsync(string category, string type)
    {
        try
        {
            var categories = await _accountingService.GetCategoriesAsync(type);
            var selectedCategory = categories.FirstOrDefault(c => c.Name == category);
            
            if (selectedCategory?.SubCategories?.Any() == true)
            {
                var subCategories = selectedCategory.SubCategories.Select(sc => new { 
                    value = sc.Name, 
                    text = sc.Name 
                }).ToList();
                
                return new JsonResult(subCategories);
            }
            
            return new JsonResult(new List<object>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得子分類時發生錯誤");
            return new JsonResult(new { error = "取得子分類時發生錯誤" });
        }
    }

    /// <summary>
    /// 驗證金額 AJAX 處理
    /// </summary>
    public IActionResult OnPostValidateAmountAsync([FromBody] ValidateAmountRequest request)
    {
        try
        {
            if (request.Amount <= 0)
            {
                return new JsonResult(new { valid = false, message = "金額必須大於 0" });
            }

            if (request.Amount > 999999999)
            {
                return new JsonResult(new { valid = false, message = "金額不可超過 999,999,999" });
            }

            return new JsonResult(new { valid = true });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "驗證金額時發生錯誤");
            return new JsonResult(new { valid = false, message = "驗證時發生錯誤" });
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 載入下拉選單選項
    /// </summary>
    private async Task LoadSelectListsAsync()
    {
        try
        {
            // 載入收入分類
            var incomeCategories = await _accountingService.GetCategoriesAsync("Income");
            IncomeCategoryOptions = incomeCategories.Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            }).ToList();

            // 載入支出分類
            var expenseCategories = await _accountingService.GetCategoriesAsync("Expense");
            ExpenseCategoryOptions = expenseCategories.Select(c => new SelectListItem
            {
                Value = c.Name,
                Text = c.Name
            }).ToList();

            // 載入付款方式
            var paymentMethods = await _accountingService.GetPaymentMethodsAsync();
            PaymentMethodOptions = paymentMethods.Select(pm => new SelectListItem
            {
                Value = pm,
                Text = pm
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入下拉選單選項時發生錯誤");
        }
    }

    #endregion
}

/// <summary>
/// 記帳記錄檢視模型
/// </summary>
public class AccountingRecordViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請選擇日期")]
    [Display(Name = "日期")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "請選擇收支類型")]
    [Display(Name = "收支類型")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, 999999999, ErrorMessage = "金額必須介於 0.01 到 999,999,999 之間")]
    [Display(Name = "金額")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "請選擇大分類")]
    [Display(Name = "大分類")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "細分類")]
    public string? SubCategory { get; set; }

    [Required(ErrorMessage = "請選擇付款方式")]
    [Display(Name = "付款方式")]
    public string PaymentMethod { get; set; } = "現金";

    [MaxLength(500, ErrorMessage = "備註不可超過 500 字元")]
    [Display(Name = "備註")]
    public string? Note { get; set; }
}

/// <summary>
/// 驗證金額請求模型
/// </summary>
public class ValidateAmountRequest
{
    public decimal Amount { get; set; }
}
