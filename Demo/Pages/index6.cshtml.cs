using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Demo.Services;
using Demo.Models;

namespace Demo.Pages
{
    public class index6 : PageModel
    {
        private readonly ILogger<index6> _logger;
        private readonly ExchangeRateService _exchangeRateService;

        public index6(ILogger<index6> logger, ExchangeRateService exchangeRateService)
        {
            _logger = logger;
            _exchangeRateService = exchangeRateService;
        }

        #region 屬性

        [BindProperty]
        [Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
        public decimal Amount { get; set; }

        [BindProperty]
        public string FromCurrency { get; set; } = "TWD";

        [BindProperty]
        public string ToCurrency { get; set; } = "USD";

        [BindProperty]
        public bool IsTwdToForeign { get; set; } = true;

        public decimal Result { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;

        // 日期相關屬性
        public DateTime CurrentDate { get; set; }
        public DateTime? LastUpdated { get; set; }
        public string DayOfWeekChinese { get; set; } = string.Empty;
        public bool IsRateDataStale { get; set; }

        // 下拉選單資料
        public List<SelectListItem> CurrencyOptions { get; set; } = new List<SelectListItem>();

        // 匯率相關資訊
        public decimal UsedExchangeRate { get; set; }
        public string CalculationDetails { get; set; } = string.Empty;
        
        // 當前匯率顯示
        public decimal CurrentBuyRate { get; set; }
        public decimal CurrentSellRate { get; set; }
        public decimal CurrentCashBuyRate { get; set; }
        public decimal CurrentCashSellRate { get; set; }
        public bool HasValidRateData { get; set; } = true;

        #endregion

        #region 頁面處理方法

        public async Task OnGetAsync()
        {
            // 如果有查詢參數，則更新對應的屬性值
            if (Request.Query.ContainsKey("IsTwdToForeign"))
            {
                bool.TryParse(Request.Query["IsTwdToForeign"], out bool isTwdToForeign);
                IsTwdToForeign = isTwdToForeign;
            }
            
            if (Request.Query.ContainsKey("ToCurrency"))
            {
                ToCurrency = Request.Query["ToCurrency"].ToString() ?? "USD";
            }
            
            if (Request.Query.ContainsKey("Amount"))
            {
                decimal.TryParse(Request.Query["Amount"], out decimal amount);
                Amount = amount;
            }

            await InitializePageDataAsync();
        }

        public async Task<IActionResult> OnPostCalculateAsync()
        {
            await InitializePageDataAsync();

            // 檢查匯率資料是否有效
            if (!HasValidRateData)
            {
                IsError = true;
                ErrorMessage = "目前選擇的貨幣缺少有效的匯率資料，請先點擊「更新匯率」更新資料後再進行計算";
                return Page();
            }

            if (!ModelState.IsValid)
            {
                IsError = true;
                // 聚合欄位錯誤訊息
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(m => !string.IsNullOrEmpty(m)).ToList();
                if (errors.Any())
                {
                    ErrorMessage = string.Join("; ", errors);
                }
                else
                {
                    ErrorMessage = "請檢查輸入的資料是否正確";
                }

                return Page();
            }

            try
            {
                // 根據計算方式設定來源和目標貨幣
                var fromCurrency = IsTwdToForeign ? "TWD" : ToCurrency;
                var toCurrency = IsTwdToForeign ? ToCurrency : "TWD";

                var result = await _exchangeRateService.CalculateExchangeAsync(Amount, fromCurrency, toCurrency);

                if (result.IsSuccess)
                {
                    Result = result.Amount;
                    UsedExchangeRate = result.ExchangeRate;
                    IsError = false;
                    
                    // 設定計算詳細資訊
                    if (IsTwdToForeign)
                    {
                        CalculationDetails = $"台幣 {Amount:N2} ÷ {UsedExchangeRate:N6} = {Result:N6} {ToCurrency}";
                    }
                    else
                    {
                        CalculationDetails = $"{ToCurrency} {Amount:N6} × {UsedExchangeRate:N6} = {Result:N2} 台幣";
                    }

                    _logger.LogInformation("匯率計算成功: {Details}", CalculationDetails);
                }
                else
                {
                    IsError = true;
                    ErrorMessage = result.ErrorMessage;
                    Result = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "計算匯率時發生未預期的錯誤");
                IsError = true;
                ErrorMessage = "計算過程發生錯誤，請稍後再試";
                Result = 0;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostUpdateRatesAsync()
        {
            try
            {
                var data = await _exchangeRateService.FetchExchangeRatesAsync();
                
                if (data != null && data.Rates.Any())
                {
                    TempData["SuccessMessage"] = $"匯率資料更新成功！共更新 {data.Rates.Count} 種貨幣，更新時間：{data.LastUpdated:yyyy-MM-dd HH:mm:ss}";
                    _logger.LogInformation("使用者手動更新匯率資料成功");
                }
                else
                {
                    TempData["ErrorMessage"] = "更新匯率資料失敗，請檢查網路連線後再試";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新匯率資料時發生錯誤");
                TempData["ErrorMessage"] = "更新匯率資料時發生錯誤，請稍後再試";
            }

            await InitializePageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostClearAsync()
        {
            // 清除表單資料
            Amount = 0;
            Result = 0;
            IsError = false;
            ErrorMessage = string.Empty;
            CalculationDetails = string.Empty;
            UsedExchangeRate = 0;
            
            // 重設為預設值
            FromCurrency = "TWD";
            ToCurrency = "USD";
            IsTwdToForeign = true;

            await InitializePageDataAsync();
            
            _logger.LogInformation("使用者清除表單資料");
            return Page();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 初始化頁面資料
        /// </summary>
        private async Task InitializePageDataAsync()
        {
            // 設定當前日期
            CurrentDate = DateTime.Now;
            DayOfWeekChinese = GetChineseDayOfWeek(CurrentDate.DayOfWeek);

            // 載入匯率資料並檢查時效性
            var exchangeData = await _exchangeRateService.LoadExchangeRatesAsync();
            if (exchangeData != null)
            {
                LastUpdated = exchangeData.LastUpdated;
                IsRateDataStale = await _exchangeRateService.IsRateDataStaleAsync();
            }
            else
            {
                IsRateDataStale = true;
            }

            // 設定貨幣選項
            SetupCurrencyOptions();
            
            // 更新當前匯率顯示
            await UpdateCurrentRateDisplayAsync();
        }

        /// <summary>
        /// 設定貨幣下拉選單選項
        /// </summary>
        private void SetupCurrencyOptions()
        {
            CurrencyOptions = new List<SelectListItem>
            {
                new SelectListItem { Value = "USD", Text = "美金 (USD)" },
                new SelectListItem { Value = "JPY", Text = "日圓 (JPY)" },
                new SelectListItem { Value = "CNY", Text = "人民幣 (CNY)" },
                new SelectListItem { Value = "EUR", Text = "歐元 (EUR)" },
                new SelectListItem { Value = "GBP", Text = "英鎊 (GBP)" },
                new SelectListItem { Value = "HKD", Text = "港幣 (HKD)" },
                new SelectListItem { Value = "AUD", Text = "澳幣 (AUD)" }
            };
        }

        /// <summary>
        /// 取得中文星期顯示
        /// </summary>
        private string GetChineseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "一",
                DayOfWeek.Tuesday => "二",
                DayOfWeek.Wednesday => "三",
                DayOfWeek.Thursday => "四",
                DayOfWeek.Friday => "五",
                DayOfWeek.Saturday => "六",
                DayOfWeek.Sunday => "日",
                _ => ""
            };
        }

        /// <summary>
        /// 更新當前匯率顯示
        /// </summary>
        private async Task UpdateCurrentRateDisplayAsync()
        {
            var exchangeData = await _exchangeRateService.LoadExchangeRatesAsync();
            if (exchangeData == null || !exchangeData.Rates.Any())
            {
                HasValidRateData = false;
                return;
            }

            var targetCurrency = ToCurrency?.ToUpper();
            var rate = exchangeData.Rates.FirstOrDefault(r => r.CurrencyCode.ToUpper() == targetCurrency);
            
            if (rate == null)
            {
                HasValidRateData = false;
                return;
            }

            CurrentBuyRate = rate.BuyRate;
            CurrentSellRate = rate.SellRate;
            CurrentCashBuyRate = rate.CashBuyRate;
            CurrentCashSellRate = rate.CashSellRate;

            // 檢查是否有有效的匯率資料
            if (IsTwdToForeign)
            {
                // 台幣轉外幣需要賣出匯率
                HasValidRateData = (CurrentCashSellRate > 0 || CurrentSellRate > 0);
            }
            else
            {
                // 外幣轉台幣需要買入匯率
                HasValidRateData = (CurrentCashBuyRate > 0 || CurrentBuyRate > 0);
            }
        }

        #endregion
    }
}
