using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Models;
using Demo.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

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

    #endregion

    #region 語音記帳處理方法

    /// <summary>
    /// 解析語音輸入
    /// </summary>
    public async Task<IActionResult> OnPostParseVoiceInputAsync([FromBody] VoiceParseRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.VoiceText))
            {
                return new JsonResult(new VoiceParseResult
                {
                    IsSuccess = false,
                    ErrorMessage = "語音文字不可為空"
                });
            }

            var parseResult = await ParseVoiceTextAsync(request.VoiceText);
            return new JsonResult(parseResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析語音輸入時發生錯誤");
            return new JsonResult(new VoiceParseResult
            {
                IsSuccess = false,
                ErrorMessage = "語音解析失敗，請重試"
            });
        }
    }

    /// <summary>
    /// 語音記帳記錄提交
    /// </summary>
    public async Task<IActionResult> OnPostVoiceRecordAsync([FromBody] VoiceRecordRequest voiceRequest)
    {
        try
        {
            if (string.IsNullOrEmpty(voiceRequest.Type) || voiceRequest.Amount <= 0 || string.IsNullOrEmpty(voiceRequest.Category))
            {
                return new JsonResult(new { success = false, message = "語音記錄資料不完整" });
            }

            // 建立記帳記錄
            var record = new AccountingRecord
            {
                Date = DateTime.Today,
                Type = voiceRequest.Type,
                Amount = voiceRequest.Amount,
                Category = voiceRequest.Category,
                SubCategory = "",
                PaymentMethod = "現金",
                Note = $"語音記帳：{voiceRequest.Description}"
            };

            var recordId = await _accountingService.CreateRecordAsync(record);

            if (recordId > 0)
            {
                return new JsonResult(new { success = true, message = "語音記帳成功", recordId = recordId });
            }
            else
            {
                return new JsonResult(new { success = false, message = "儲存記錄失敗" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "語音記帳時發生錯誤");
            return new JsonResult(new { success = false, message = "語音記帳失敗，請重試" });
        }
    }

    /// <summary>
    /// 解析語音文字為記帳資料
    /// </summary>
    private async Task<VoiceParseResult> ParseVoiceTextAsync(string voiceText)
    {
        var result = new VoiceParseResult
        {
            OriginalText = voiceText,
            IsSuccess = false
        };

        try
        {
            // 解析金額
            var amountPattern = @"(\d+(?:\.\d+)?)\s*(?:元|塊|塊錢)?";
            var amountMatch = Regex.Match(voiceText, amountPattern);
            
            if (amountMatch.Success && decimal.TryParse(amountMatch.Groups[1].Value, out decimal amount))
            {
                result.Amount = amount;
            }
            else
            {
                result.ErrorMessage = "未能識別金額";
                return result;
            }

            // 判斷收支類型
            if (voiceText.Contains("收入") || voiceText.Contains("賺") || voiceText.Contains("薪水") || voiceText.Contains("獎金"))
            {
                result.Type = "Income";
            }
            else
            {
                result.Type = "Expense";
            }

            // 解析類別
            result.Category = await ParseCategoryFromTextAsync(voiceText, result.Type);
            if (string.IsNullOrEmpty(result.Category))
            {
                result.Category = result.Type == "Income" ? "其他收入" : "其他支出";
            }

            // 解析描述
            result.Description = ParseDescriptionFromText(voiceText);

            // 計算解析信心度
            result.ParseConfidence = CalculateParseConfidence(result);
            result.IsSuccess = result.ParseConfidence >= 0.6; // 信心度閾值

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "語音文字解析時發生錯誤");
            result.ErrorMessage = "語音解析過程中發生錯誤";
            return result;
        }
    }

    /// <summary>
    /// 從文字中解析類別
    /// </summary>
    private async Task<string> ParseCategoryFromTextAsync(string text, string type)
    {
        try
        {
            var categories = await _accountingService.GetCategoriesAsync(type);
            
            // 直接匹配類別名稱
            foreach (var category in categories)
            {
                if (text.Contains(category.Name))
                {
                    return category.Name;
                }
                
                // 檢查子類別
                if (category.SubCategories != null)
                {
                    foreach (var subCategory in category.SubCategories)
                    {
                        if (text.Contains(subCategory.Name))
                        {
                            return category.Name;
                        }
                    }
                }
            }

            // 關鍵字匹配
            var categoryKeywords = new Dictionary<string, string>
            {
                {"吃", "餐飲美食"}, {"喝", "餐飲美食"}, {"早餐", "餐飲美食"}, {"午餐", "餐飲美食"}, {"晚餐", "餐飲美食"},
                {"加油", "交通運輸"}, {"停車", "交通運輸"}, {"計程車", "交通運輸"}, {"公車", "交通運輸"},
                {"電影", "娛樂休閒"}, {"唱歌", "娛樂休閒"}, {"遊戲", "娛樂休閒"},
                {"衣服", "服飾美容"}, {"鞋子", "服飾美容"}, {"化妝品", "服飾美容"},
                {"藥", "醫療保健"}, {"看醫生", "醫療保健"}, {"健檢", "醫療保健"},
                {"水電", "居家生活"}, {"房租", "居家生活"}, {"瓦斯", "居家生活"}
            };

            foreach (var keyword in categoryKeywords)
            {
                if (text.Contains(keyword.Key))
                {
                    return keyword.Value;
                }
            }

            return "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析類別時發生錯誤");
            return "";
        }
    }

    /// <summary>
    /// 從文字中解析描述
    /// </summary>
    private string ParseDescriptionFromText(string text)
    {
        // 移除金額部分
        var cleanText = Regex.Replace(text, @"\d+(?:\.\d+)?\s*(?:元|塊|塊錢)?", "").Trim();
        
        // 移除常見的動詞
        var commonWords = new[] { "買", "花了", "支出", "付", "給", "用了", "消費" };
        foreach (var word in commonWords)
        {
            cleanText = cleanText.Replace(word, "").Trim();
        }

        return string.IsNullOrWhiteSpace(cleanText) ? "語音記帳" : cleanText;
    }

    /// <summary>
    /// 計算解析信心度
    /// </summary>
    private double CalculateParseConfidence(VoiceParseResult result)
    {
        double confidence = 0.0;

        // 基礎分數：有金額就給 0.4
        if (result.Amount > 0)
        {
            confidence += 0.4;
        }

        // 有類別增加信心度
        if (!string.IsNullOrEmpty(result.Category) && result.Category != "其他收入" && result.Category != "其他支出")
        {
            confidence += 0.3;
        }

        // 類型判斷合理性
        if (!string.IsNullOrEmpty(result.Type))
        {
            confidence += 0.2;
        }

        // 有有意義的描述增加信心度
        if (!string.IsNullOrEmpty(result.Description) && 
            result.Description != "語音記帳" && 
            result.Description.Length > 2)
        {
            confidence += 0.1;
        }

        return Math.Min(confidence, 1.0);
    }

    #endregion

    #region AJAX 處理方法

    /// <summary>
    /// 取得分類 AJAX 處理
    /// </summary>
    public async Task<IActionResult> OnGetCategoriesAsync(string type)
    {
        try
        {
            var categories = await _accountingService.GetCategoriesAsync(type);
            var result = categories.Select(c => new { name = c.Name, id = c.Id }).ToList();
            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分類時發生錯誤");
            return new JsonResult(new { error = "取得分類時發生錯誤" });
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

    /// <summary>
    /// 新增大分類 AJAX 處理
    /// </summary>
    public async Task<IActionResult> OnPostCreateCategoryAsync([FromBody] CreateCategoryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return new JsonResult(new { success = false, message = "分類名稱不可為空" });
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return new JsonResult(new { success = false, message = "分類類型不可為空" });
            }

            var success = await _accountingService.CreateCategoryAsync(request.Name, request.Type, request.Icon ?? "fas fa-folder");
            
            if (success)
            {
                _logger.LogInformation("成功建立新分類 {Name} ({Type})", request.Name, request.Type);
                return new JsonResult(new { success = true, message = "分類建立成功" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "分類建立失敗，可能已存在相同名稱的分類" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立分類時發生錯誤");
            return new JsonResult(new { success = false, message = "建立分類時發生錯誤" });
        }
    }

    /// <summary>
    /// 新增子分類 AJAX 處理
    /// </summary>
    public async Task<IActionResult> OnPostCreateSubCategoryAsync([FromBody] CreateSubCategoryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.CategoryName))
            {
                return new JsonResult(new { success = false, message = "請選擇大分類" });
            }

            if (string.IsNullOrWhiteSpace(request.SubCategoryName))
            {
                return new JsonResult(new { success = false, message = "子分類名稱不可為空" });
            }

            if (string.IsNullOrWhiteSpace(request.Type))
            {
                return new JsonResult(new { success = false, message = "分類類型不可為空" });
            }

            var success = await _accountingService.CreateSubCategoryAsync(request.CategoryName, request.SubCategoryName, request.Type);
            
            if (success)
            {
                _logger.LogInformation("成功建立新子分類 {SubCategoryName} 於分類 {CategoryName} ({Type})", request.SubCategoryName, request.CategoryName, request.Type);
                return new JsonResult(new { success = true, message = "子分類建立成功" });
            }
            else
            {
                return new JsonResult(new { success = false, message = "子分類建立失敗，可能已存在相同名稱的子分類" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立子分類時發生錯誤");
            return new JsonResult(new { success = false, message = "建立子分類時發生錯誤" });
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

#region 檢視模型和請求模型

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
/// 語音記錄請求模型
/// </summary>
public class VoiceRecordRequest
{
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VoiceText { get; set; } = string.Empty;
}

/// <summary>
/// 驗證金額請求模型
/// </summary>
public class ValidateAmountRequest
{
    public decimal Amount { get; set; }
}

/// <summary>
/// 建立分類請求模型
/// </summary>
public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Icon { get; set; }
}

/// <summary>
/// 建立子分類請求模型
/// </summary>
public class CreateSubCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string SubCategoryName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
}

#endregion
