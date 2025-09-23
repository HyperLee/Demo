using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Models;
using Demo.Services;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Globalization;

namespace Demo.Pages;

/// <summary>
/// 記帳系統編輯頁面
/// </summary>
public class index8 : PageModel
{
    private readonly ILogger<index8> _logger;
    private readonly IAccountingService _accountingService;
    private readonly VoiceParseConfig _parseConfig;

    public index8(ILogger<index8> logger, IAccountingService accountingService)
    {
        _logger = logger;
        _accountingService = accountingService;
        _parseConfig = new VoiceParseConfig(); // 使用預設配置
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

    #region Phase 1 語音解析內部類別

    /// <summary>
    /// 日期解析結果
    /// </summary>
    private class DateParseResult
    {
        public DateTime Date { get; set; }
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 付款方式解析結果
    /// </summary>
    private class PaymentMethodParseResult
    {
        public string Method { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 商家名稱解析結果
    /// </summary>
    private class MerchantNameParseResult
    {
        public string Name { get; set; } = string.Empty;
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 收支類型解析結果
    /// </summary>
    private class TypeParseResult
    {
        public string Type { get; set; } = "Expense";
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 分類解析結果 (包含細分類)
    /// </summary>
    private class CategoryParseResult
    {
        public string Category { get; set; } = string.Empty;
        public string? SubCategory { get; set; }
        public double Confidence { get; set; }
    }

    /// <summary>
    /// 描述和備註解析結果
    /// </summary>
    private class DescriptionNoteParseResult
    {
        public string Description { get; set; } = string.Empty;
        public string? Note { get; set; }
        public double DescriptionConfidence { get; set; }
        public double NoteConfidence { get; set; }
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
    /// 語音文字解析主方法 (Phase 1 重構版)
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
            // 1. 預處理：清理和標準化文字
            var normalizedText = PreprocessVoiceText(voiceText);
            result.ParsedSteps["PreprocessedText"] = normalizedText;

            // 2. 金額解析 (優先級最高)
            var amountResult = ParseAmountFromText(normalizedText);
            if (amountResult.HasValue)
            {
                result.Amount = amountResult.Value;
                result.FieldConfidence["Amount"] = 0.9; // 數字解析通常很準確
            }

            // 3. 日期解析
            var dateResult = ParseDateFromText(normalizedText);
            if (dateResult != null)
            {
                result.Date = dateResult.Date;
                result.FieldConfidence["Date"] = dateResult.Confidence;
            }

            // 4. 收支類型判斷
            var typeResult = ParseTypeFromText(normalizedText);
            result.Type = typeResult.Type;
            result.FieldConfidence["Type"] = typeResult.Confidence;

            // 5. 付款方式識別
            var paymentResult = ParsePaymentMethodFromText(normalizedText);
            if (!string.IsNullOrEmpty(paymentResult.Method))
            {
                result.PaymentMethod = paymentResult.Method;
                result.FieldConfidence["PaymentMethod"] = paymentResult.Confidence;
            }

            // 6. 商家名稱提取
            var merchantResult = ParseMerchantNameFromText(normalizedText);
            if (!string.IsNullOrEmpty(merchantResult.Name))
            {
                result.MerchantName = merchantResult.Name;
                result.FieldConfidence["MerchantName"] = merchantResult.Confidence;
            }

            // 7. 分類解析 (需要考慮商家名稱)
            var categoryResult = await ParseCategoryFromTextAsync(normalizedText, result.Type, result.MerchantName);
            if (!string.IsNullOrEmpty(categoryResult.Category))
            {
                result.Category = categoryResult.Category;
                result.SubCategory = categoryResult.SubCategory;
                result.FieldConfidence["Category"] = categoryResult.Confidence;
                if (!string.IsNullOrEmpty(categoryResult.SubCategory))
                {
                    result.FieldConfidence["SubCategory"] = categoryResult.Confidence;
                }
            }

            // 8. 描述和備註分離
            var descriptionResult = ParseDescriptionAndNoteFromText(normalizedText, result);
            result.Description = descriptionResult.Description;
            result.Note = descriptionResult.Note;
            result.FieldConfidence["Description"] = descriptionResult.DescriptionConfidence;
            if (!string.IsNullOrEmpty(result.Note))
            {
                result.FieldConfidence["Note"] = descriptionResult.NoteConfidence;
            }

            // 9. 計算整體信心度和成功狀態
            result.ParseConfidence = CalculateOverallConfidence(result);
            result.IsSuccess = result.ParseConfidence >= 0.4; // 降低成功閾值

            // 10. 識別未解析內容
            result.UnparsedContent = IdentifyUnparsedContent(normalizedText, result);

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

    #region Phase 1 新增解析方法

    /// <summary>
    /// 預處理語音文字
    /// </summary>
    private string PreprocessVoiceText(string voiceText)
    {
        if (string.IsNullOrWhiteSpace(voiceText))
            return string.Empty;

        // 轉換為小寫並移除多餘空白
        var text = voiceText.Trim().ToLowerInvariant();
        
        // 標準化數字表達
        text = Regex.Replace(text, @"(\d+)\s*塊", "$1元");
        text = Regex.Replace(text, @"(\d+)\s*塊錢", "$1元");
        
        // 標準化時間表達
        text = text.Replace("號", "日");
        
        return text;
    }

    /// <summary>
    /// 從文字中解析金額
    /// </summary>
    private decimal? ParseAmountFromText(string text)
    {
        try
        {
            // 支援多種金額格式
            var patterns = new[]
            {
                @"(\d+(?:\.\d+)?)\s*(?:元|塊|塊錢|dollars?|ntd?)",
                @"(?:花了?|支出|付了?|給了?)\s*(\d+(?:\.\d+)?)",
                @"(\d+(?:\.\d+)?)\s*(?:dollar|台幣|新台幣)"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success && decimal.TryParse(match.Groups[1].Value, out var amount))
                {
                    return amount;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "金額解析時發生錯誤");
            return null;
        }
    }

    /// <summary>
    /// 從文字中解析日期
    /// </summary>
    private DateParseResult? ParseDateFromText(string text)
    {
        try
        {
            // 1. 相對日期匹配
            var relativeDatePatterns = new Dictionary<string, int>
            {
                { "今天", 0 }, { "今日", 0 },
                { "昨天", -1 }, { "昨日", -1 },
                { "前天", -2 }, { "前日", -2 },
                { "大前天", -3 },
                { "明天", 1 }, { "明日", 1 },
                { "後天", 2 }
            };

            foreach (var pattern in relativeDatePatterns)
            {
                if (text.Contains(pattern.Key))
                {
                    return new DateParseResult
                    {
                        Date = DateTime.Today.AddDays(pattern.Value),
                        Confidence = 0.9
                    };
                }
            }

            // 2. 絕對日期匹配 - 完整格式
            var fullDatePattern = @"(\d{4})\s*年\s*(\d{1,2})\s*月\s*(\d{1,2})\s*[日號]";
            var fullMatch = Regex.Match(text, fullDatePattern);
            if (fullMatch.Success)
            {
                if (DateTime.TryParse($"{fullMatch.Groups[1].Value}-{fullMatch.Groups[2].Value}-{fullMatch.Groups[3].Value}", out var fullDate))
                {
                    return new DateParseResult
                    {
                        Date = fullDate,
                        Confidence = 0.95
                    };
                }
            }

            // 3. 月日格式
            var monthDayPattern = @"(\d{1,2})\s*月\s*(\d{1,2})\s*[日號]";
            var monthDayMatch = Regex.Match(text, monthDayPattern);
            if (monthDayMatch.Success)
            {
                var month = int.Parse(monthDayMatch.Groups[1].Value);
                var day = int.Parse(monthDayMatch.Groups[2].Value);
                var year = DateTime.Today.Year;
                
                // 如果日期已過，假設是明年
                var candidateDate = new DateTime(year, month, day);
                if (candidateDate < DateTime.Today)
                {
                    candidateDate = candidateDate.AddYears(1);
                }

                return new DateParseResult
                {
                    Date = candidateDate,
                    Confidence = 0.8
                };
            }

            // 4. 中文數字日期解析
            var chineseDatePattern = @"(十二|十一|十|一|二|三|四|五|六|七|八|九)\s*月\s*(三十一|三十|二十九|二十八|二十七|二十六|二十五|二十四|二十三|二十二|二十一|二十|十九|十八|十七|十六|十五|十四|十三|十二|十一|十|九|八|七|六|五|四|三|二|一)\s*[日號]";
            var chineseMatch = Regex.Match(text, chineseDatePattern);
            if (chineseMatch.Success)
            {
                var month = ConvertChineseNumberToInt(chineseMatch.Groups[1].Value);
                var day = ConvertChineseNumberToInt(chineseMatch.Groups[2].Value);
                
                if (month >= 1 && month <= 12 && day >= 1 && day <= 31)
                {
                    var year = DateTime.Today.Year;
                    var candidateDate = new DateTime(year, month, day);
                    if (candidateDate < DateTime.Today)
                    {
                        candidateDate = candidateDate.AddYears(1);
                    }

                    return new DateParseResult
                    {
                        Date = candidateDate,
                        Confidence = 0.7
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "日期解析時發生錯誤");
            return null;
        }
    }

    /// <summary>
    /// 中文數字轉換為阿拉伯數字
    /// </summary>
    private int ConvertChineseNumberToInt(string chineseNumber)
    {
        var chineseToNumber = new Dictionary<string, int>
        {
            {"一", 1}, {"二", 2}, {"三", 3}, {"四", 4}, {"五", 5},
            {"六", 6}, {"七", 7}, {"八", 8}, {"九", 9}, {"十", 10},
            {"十一", 11}, {"十二", 12}, {"十三", 13}, {"十四", 14}, {"十五", 15},
            {"十六", 16}, {"十七", 17}, {"十八", 18}, {"十九", 19}, {"二十", 20},
            {"二十一", 21}, {"二十二", 22}, {"二十三", 23}, {"二十四", 24}, {"二十五", 25},
            {"二十六", 26}, {"二十七", 27}, {"二十八", 28}, {"二十九", 29}, {"三十", 30},
            {"三十一", 31}
        };

        return chineseToNumber.ContainsKey(chineseNumber) ? chineseToNumber[chineseNumber] : 0;
    }

    /// <summary>
    /// 從文字中解析收支類型
    /// </summary>
    private TypeParseResult ParseTypeFromText(string text)
    {
        try
        {
            var incomeKeywords = new[] { "收入", "賺", "薪水", "獎金", "紅包", "利息", "分紅", "投資收益" };
            var expenseKeywords = new[] { "支出", "花", "買", "付", "給", "消費", "開銷" };

            foreach (var keyword in incomeKeywords)
            {
                if (text.Contains(keyword))
                {
                    return new TypeParseResult { Type = "Income", Confidence = 0.8 };
                }
            }

            foreach (var keyword in expenseKeywords)
            {
                if (text.Contains(keyword))
                {
                    return new TypeParseResult { Type = "Expense", Confidence = 0.7 };
                }
            }

            // 預設為支出
            return new TypeParseResult { Type = "Expense", Confidence = 0.5 };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "收支類型解析時發生錯誤");
            return new TypeParseResult { Type = "Expense", Confidence = 0.3 };
        }
    }

    /// <summary>
    /// 從文字中解析付款方式
    /// </summary>
    private PaymentMethodParseResult ParsePaymentMethodFromText(string text)
    {
        try
        {
            // 付款方式關鍵字字典 (關鍵字 -> 標準名稱)
            var paymentKeywords = new Dictionary<string, string>
            {
                // 現金類
                {"現金", "現金"}, {"cash", "現金"}, {"付現", "現金"},
                
                // 信用卡類
                {"信用卡", "信用卡"}, {"刷卡", "信用卡"}, {"credit", "信用卡"},
                {"visa", "信用卡"}, {"master", "信用卡"}, {"jcb", "信用卡"},
                
                // 悠遊卡類
                {"悠遊卡", "悠遊卡"}, {"easycard", "悠遊卡"},
                
                // 一卡通
                {"一卡通", "一卡通"}, {"ipass", "一卡通"},
                
                // 電子支付
                {"line pay", "LINE Pay"}, {"linepay", "LINE Pay"},
                {"apple pay", "Apple Pay"}, {"applepay", "Apple Pay"},
                {"google pay", "Google Pay"}, {"googlepay", "Google Pay"},
                {"街口", "街口支付"}, {"jkopay", "街口支付"},
                {"悠遊付", "悠遊付"}, {"easy wallet", "悠遊付"},
                {"icash", "愛金卡"}, {"愛金卡", "愛金卡"},
                {"pi錢包", "Pi錢包"}, {"pi wallet", "Pi錢包"},
                
                // 轉帳
                {"轉帳", "轉帳"}, {"匯款", "轉帳"}, {"atm", "轉帳"},
                
                // 支票
                {"支票", "支票"}, {"check", "支票"}
            };

            // 先進行精確匹配
            foreach (var keyword in paymentKeywords)
            {
                if (text.Contains(keyword.Key))
                {
                    return new PaymentMethodParseResult
                    {
                        Method = keyword.Value,
                        Confidence = 0.9
                    };
                }
            }

            // 模糊匹配 (編輯距離)
            if (_parseConfig.EnableFuzzyMatching)
            {
                // 簡化的模糊匹配邏輯
                // 這裡可以實作更複雜的模糊匹配演算法
                // 暫時返回預設值
            }

            return new PaymentMethodParseResult
            {
                Method = string.Empty,
                Confidence = 0.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "付款方式解析時發生錯誤");
            return new PaymentMethodParseResult { Method = string.Empty, Confidence = 0.0 };
        }
    }

    /// <summary>
    /// 從文字中解析商家名稱
    /// </summary>
    private MerchantNameParseResult ParseMerchantNameFromText(string text)
    {
        try
        {
            // 常見商家名稱字典
            var merchantKeywords = new Dictionary<string, string>
            {
                // 便利店
                {"7-11", "7-Eleven"}, {"seven", "7-Eleven"}, {"統一超商", "7-Eleven"},
                {"全家", "全家便利商店"}, {"family", "全家便利商店"},
                {"萊爾富", "萊爾富"}, {"ok", "OK便利店"},
                
                // 咖啡店
                {"星巴克", "星巴克"}, {"starbucks", "星巴克"},
                {"85度c", "85度C"}, {"路易莎", "路易莎咖啡"},
                {"cama", "cama咖啡"}, {"麥當勞", "麥當勞"},
                
                // 餐廳
                {"肯德基", "肯德基"}, {"kfc", "肯德基"},
                {"摩斯", "摩斯漢堡"}, {"mos", "摩斯漢堡"},
                {"漢堡王", "漢堡王"}, {"burger king", "漢堡王"},
                {"subway", "SUBWAY"}, {"頂呱呱", "頂呱呱"},
                
                // 購物
                {"家樂福", "家樂福"}, {"carrefour", "家樂福"},
                {"好市多", "好市多"}, {"costco", "好市多"},
                {"全聯", "全聯福利中心"}, {"大潤發", "大潤發"},
                
                // 百貨
                {"新光三越", "新光三越"}, {"遠百", "遠東百貨"},
                {"微風", "微風廣場"}, {"101", "台北101"}
            };

            // 位置介詞處理
            var locationPrepositions = new[] { "在", "去", "到", "從", "於" };
            var cleanText = text;
            
            foreach (var prep in locationPrepositions)
            {
                cleanText = cleanText.Replace(prep, " ");
            }

            // 精確匹配
            foreach (var merchant in merchantKeywords)
            {
                if (cleanText.Contains(merchant.Key))
                {
                    return new MerchantNameParseResult
                    {
                        Name = merchant.Value,
                        Confidence = 0.8
                    };
                }
            }

            // 模糊匹配
            if (_parseConfig.EnableFuzzyMatching)
            {
                // 簡化版模糊匹配
                // 實際應用中可使用 Levenshtein distance 等算法
                // 暫時返回預設值
            }

            return new MerchantNameParseResult
            {
                Name = string.Empty,
                Confidence = 0.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "商家名稱解析時發生錯誤");
            return new MerchantNameParseResult { Name = string.Empty, Confidence = 0.0 };
        }
    }

    /// <summary>
    /// 從文字中解析分類 (考慮商家名稱)
    /// </summary>
    private async Task<CategoryParseResult> ParseCategoryFromTextAsync(string text, string type, string? merchantName)
    {
        try
        {
            var categories = await _accountingService.GetCategoriesAsync(type);
            
            // 1. 先嘗試根據商家名稱推斷分類
            if (!string.IsNullOrEmpty(merchantName))
            {
                var merchantCategoryMap = new Dictionary<string, (string Category, string? SubCategory)>
                {
                    {"7-Eleven", ("餐飲美食", "便利商店")},
                    {"全家便利商店", ("餐飲美食", "便利商店")},
                    {"星巴克", ("餐飲美食", "咖啡茶飲")},
                    {"麥當勞", ("餐飲美食", "速食")},
                    {"肯德基", ("餐飲美食", "速食")},
                    {"家樂福", ("居家生活", "生活用品")},
                    {"好市多", ("居家生活", "生活用品")},
                    {"全聯福利中心", ("餐飲美食", "超市")},
                    {"台北101", ("娛樂休閒", "購物")}
                };

                if (merchantCategoryMap.ContainsKey(merchantName))
                {
                    var mapping = merchantCategoryMap[merchantName];
                    return new CategoryParseResult
                    {
                        Category = mapping.Category,
                        SubCategory = mapping.SubCategory,
                        Confidence = 0.8
                    };
                }
            }

            // 2. 直接匹配類別名稱
            foreach (var category in categories)
            {
                if (text.Contains(category.Name))
                {
                    return new CategoryParseResult
                    {
                        Category = category.Name,
                        Confidence = 0.9
                    };
                }
                
                // 檢查子類別
                if (category.SubCategories != null)
                {
                    foreach (var subCategory in category.SubCategories)
                    {
                        if (text.Contains(subCategory.Name))
                        {
                            return new CategoryParseResult
                            {
                                Category = category.Name,
                                SubCategory = subCategory.Name,
                                Confidence = 0.85
                            };
                        }
                    }
                }
            }

            // 3. 關鍵字匹配
            var categoryKeywords = new Dictionary<string, (string Category, string? SubCategory)>
            {
                {"吃", ("餐飲美食", null)}, {"喝", ("餐飲美食", "飲料")}, 
                {"早餐", ("餐飲美食", "早餐")}, {"午餐", ("餐飲美食", "午餐")}, {"晚餐", ("餐飲美食", "晚餐")},
                {"咖啡", ("餐飲美食", "咖啡茶飲")}, {"茶", ("餐飲美食", "咖啡茶飲")},
                
                {"加油", ("交通運輸", "汽車")}, {"停車", ("交通運輸", "停車費")}, 
                {"計程車", ("交通運輸", "計程車")}, {"公車", ("交通運輸", "大眾運輸")},
                {"機車", ("交通運輸", "機車")}, {"捷運", ("交通運輸", "大眾運輸")},
                
                {"電影", ("娛樂休閒", "電影")}, {"唱歌", ("娛樂休閒", "KTV")}, 
                {"遊戲", ("娛樂休閒", "遊戲")}, {"運動", ("娛樂休閒", "運動")},
                
                {"衣服", ("服飾美容", "服飾")}, {"鞋子", ("服飾美容", "鞋類")}, 
                {"化妝品", ("服飾美容", "美容")}, {"剪髮", ("服飾美容", "美容")},
                
                {"藥", ("醫療保健", "藥品")}, {"看醫生", ("醫療保健", "門診")}, 
                {"健檢", ("醫療保健", "健檢")}, {"牙醫", ("醫療保健", "牙科")},
                
                {"水電", ("居家生活", "水電費")}, {"房租", ("居家生活", "房租")}, 
                {"瓦斯", ("居家生活", "瓦斯費")}, {"網路", ("居家生活", "網路費")},
                {"手機", ("居家生活", "電話費")}
            };

            foreach (var keyword in categoryKeywords)
            {
                if (text.Contains(keyword.Key))
                {
                    return new CategoryParseResult
                    {
                        Category = keyword.Value.Category,
                        SubCategory = keyword.Value.SubCategory,
                        Confidence = 0.7
                    };
                }
            }

            return new CategoryParseResult
            {
                Category = string.Empty,
                Confidence = 0.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "解析類別時發生錯誤");
            return new CategoryParseResult
            {
                Category = string.Empty,
                Confidence = 0.0
            };
        }
    }

    /// <summary>
    /// 解析描述和備註 (從原始文字中分離)
    /// </summary>
    private DescriptionNoteParseResult ParseDescriptionAndNoteFromText(string text, VoiceParseResult result)
    {
        try
        {
            var cleanText = text;

            // 移除已解析的部分
            if (result.Amount.HasValue)
            {
                cleanText = Regex.Replace(cleanText, @"\d+(?:\.\d+)?\s*(?:元|塊|塊錢)?", "");
            }

            if (result.Date.HasValue)
            {
                // 移除日期相關文字
                var datePatterns = new[]
                {
                    @"\d{4}\s*年\s*\d{1,2}\s*月\s*\d{1,2}\s*[日號]",
                    @"\d{1,2}\s*月\s*\d{1,2}\s*[日號]",
                    @"今天|昨天|前天|明天|後天"
                };

                foreach (var pattern in datePatterns)
                {
                    cleanText = Regex.Replace(cleanText, pattern, "");
                }
            }

            if (!string.IsNullOrEmpty(result.PaymentMethod))
            {
                cleanText = cleanText.Replace(result.PaymentMethod.ToLower(), "");
            }

            if (!string.IsNullOrEmpty(result.MerchantName))
            {
                cleanText = cleanText.Replace(result.MerchantName.ToLower(), "");
            }

            // 移除常見動詞和介詞
            var commonWords = new[] { "買", "花了", "支出", "付", "給", "用了", "消費", "在", "去", "到", "從" };
            foreach (var word in commonWords)
            {
                cleanText = cleanText.Replace(word, "").Trim();
            }

            // 清理多餘空白
            cleanText = Regex.Replace(cleanText, @"\s+", " ").Trim();

            // 如果剩餘文字太短，設為預設描述
            if (string.IsNullOrWhiteSpace(cleanText) || cleanText.Length < 2)
            {
                return new DescriptionNoteParseResult
                {
                    Description = "語音記帳",
                    DescriptionConfidence = 0.3
                };
            }

            // 分離描述和備註 (簡化版：目前將全部視為描述)
            return new DescriptionNoteParseResult
            {
                Description = cleanText,
                DescriptionConfidence = 0.6,
                Note = null,
                NoteConfidence = 0.0
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "描述和備註解析時發生錯誤");
            return new DescriptionNoteParseResult
            {
                Description = "語音記帳",
                DescriptionConfidence = 0.3
            };
        }
    }

    /// <summary>
    /// 計算整體信心度
    /// </summary>
    private double CalculateOverallConfidence(VoiceParseResult result)
    {
        try
        {
            var confidenceValues = result.FieldConfidence.Values.ToList();
            
            // 如果沒有任何欄位解析成功，返回低信心度
            if (!confidenceValues.Any())
                return 0.2;

            // 加權平均：重要欄位權重較高
            var weightedSum = 0.0;
            var totalWeight = 0.0;

            var fieldWeights = new Dictionary<string, double>
            {
                {"Amount", 0.3},      // 金額最重要
                {"Type", 0.2},        // 收支類型次重要
                {"Category", 0.2},    // 分類重要
                {"Date", 0.1},        // 日期中等重要
                {"PaymentMethod", 0.1}, // 付款方式
                {"MerchantName", 0.05}, // 商家名稱
                {"Description", 0.03},  // 描述
                {"SubCategory", 0.02}   // 細分類
            };

            foreach (var fieldConf in result.FieldConfidence)
            {
                if (fieldWeights.ContainsKey(fieldConf.Key))
                {
                    var weight = fieldWeights[fieldConf.Key];
                    weightedSum += fieldConf.Value * weight;
                    totalWeight += weight;
                }
            }

            return totalWeight > 0 ? Math.Min(weightedSum / totalWeight, 1.0) : 0.2;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "計算整體信心度時發生錯誤");
            return 0.2;
        }
    }

    /// <summary>
    /// 識別未解析內容
    /// </summary>
    private string? IdentifyUnparsedContent(string originalText, VoiceParseResult result)
    {
        try
        {
            var remainingText = originalText.ToLower();

            // 移除已解析的內容 (簡化版)
            var parsedElements = new List<string>();

            if (result.Amount.HasValue)
                parsedElements.Add(result.Amount.Value.ToString());

            if (!string.IsNullOrEmpty(result.MerchantName))
                parsedElements.Add(result.MerchantName.ToLower());

            if (!string.IsNullOrEmpty(result.PaymentMethod))
                parsedElements.Add(result.PaymentMethod.ToLower());

            if (!string.IsNullOrEmpty(result.Description) && result.Description != "語音記帳")
                parsedElements.Add(result.Description.ToLower());

            foreach (var element in parsedElements)
            {
                remainingText = remainingText.Replace(element, "");
            }

            remainingText = Regex.Replace(remainingText, @"\s+", " ").Trim();

            return string.IsNullOrWhiteSpace(remainingText) ? null : remainingText;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "識別未解析內容時發生錯誤");
            return null;
        }
    }

    #endregion

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
