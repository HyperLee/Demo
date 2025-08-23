using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    /// <summary>
    /// 多時區電子時鐘頁面 Model
    /// </summary>
    public class index2Model : PageModel
    {
        private readonly ILogger<index2Model> _logger;

        /// <summary>
        /// 支援的時區資料
        /// </summary>
        private readonly List<TimezoneInfo> _supportedTimezones = new()
        {
            new TimezoneInfo { City = "紐約", Timezone = "America/New_York", DisplayName = "美國/紐約" },
            new TimezoneInfo { City = "倫敦", Timezone = "Europe/London", DisplayName = "歐洲/倫敦" },
            new TimezoneInfo { City = "東京", Timezone = "Asia/Tokyo", DisplayName = "亞洲/東京" },
            new TimezoneInfo { City = "沙烏地阿拉伯", Timezone = "Asia/Riyadh", DisplayName = "亞洲/利雅德" }
        };

        public index2Model(ILogger<index2Model> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 頁面載入時呼叫，僅供前端初始化。
        /// </summary>
        public void OnGet()
        {
            _logger.LogInformation("多時區時鐘頁面載入");
        }

        /// <summary>
        /// 取得多時區時間資訊 API
        /// </summary>
        /// <returns>世界各地時區時間資訊</returns>
        /// <example>
        /// <code>
        /// var result = OnGetWorldTimes();
        /// </code>
        /// </example>
        public JsonResult OnGetWorldTimes()
        {
            try
            {
                var worldTimes = _supportedTimezones.Select(tz => 
                {
                    try
                    {
                        // 使用 JavaScript 相容的時區格式轉換
                        var utcNow = DateTime.UtcNow;
                        var localTime = DateTime.Now;
                        
                        // 計算該時區的當前時間
                        var tzTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcNow, tz.Timezone);
                        
                        // 計算時差
                        var offset = (tzTime - localTime).TotalHours;
                        var offsetString = offset >= 0 ? $"+{Math.Round(offset)}" : $"{Math.Round(offset)}";

                        return new
                        {
                            city = tz.City,
                            timezone = tz.Timezone,
                            displayName = tz.DisplayName,
                            currentTime = tzTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            offset = offsetString,
                            success = true
                        };
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "時區 {Timezone} 轉換失敗", tz.Timezone);
                        return new
                        {
                            city = tz.City,
                            timezone = tz.Timezone,
                            displayName = tz.DisplayName,
                            currentTime = "時間取得失敗",
                            offset = "N/A",
                            success = false
                        };
                    }
                }).ToList();

                return new JsonResult(new { success = true, data = worldTimes });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得世界時區時間失敗");
                return new JsonResult(new { success = false, error = "取得時區資訊失敗" });
            }
        }
    }

    /// <summary>
    /// 時區資訊類別
    /// </summary>
    public class TimezoneInfo
    {
        /// <summary>
        /// 城市名稱
        /// </summary>
        public string City { get; set; } = string.Empty;

        /// <summary>
        /// 時區識別碼
        /// </summary>
        public string Timezone { get; set; } = string.Empty;

        /// <summary>
        /// 顯示名稱
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;
    }
}
