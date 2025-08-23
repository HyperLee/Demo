using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    /// <summary>
    /// 多時區電子時鐘頁面 Model - 中期改進版本（P2）
    /// 支援自訂時區選擇、格式設定、深色模式及多語言
    /// </summary>
    public class index2Model : PageModel
    {
        private readonly ILogger<index2Model> _logger;

        /// <summary>
        /// 支援的時區資料 - 擴展版本
        /// </summary>
        private readonly List<EnhancedTimezoneInfo> _supportedTimezones = new()
        {
            // 亞洲地區
            new EnhancedTimezoneInfo 
            { 
                City = "台北", 
                Timezone = "Asia/Taipei", 
                DisplayName = "台北時間", 
                Country = "台灣", 
                Region = "亞洲",
                UtcOffsetMinutes = 480,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "東京", 
                Timezone = "Asia/Tokyo", 
                DisplayName = "日本標準時間", 
                Country = "日本", 
                Region = "亞洲",
                UtcOffsetMinutes = 540,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "首爾", 
                Timezone = "Asia/Seoul", 
                DisplayName = "韓國標準時間", 
                Country = "韓國", 
                Region = "亞洲",
                UtcOffsetMinutes = 540,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "香港", 
                Timezone = "Asia/Hong_Kong", 
                DisplayName = "香港時間", 
                Country = "香港", 
                Region = "亞洲",
                UtcOffsetMinutes = 480,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "新加坡", 
                Timezone = "Asia/Singapore", 
                DisplayName = "新加坡標準時間", 
                Country = "新加坡", 
                Region = "亞洲",
                UtcOffsetMinutes = 480,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "曼谷", 
                Timezone = "Asia/Bangkok", 
                DisplayName = "印度支那時間", 
                Country = "泰國", 
                Region = "亞洲",
                UtcOffsetMinutes = 420,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "雅加達", 
                Timezone = "Asia/Jakarta", 
                DisplayName = "西印尼時間", 
                Country = "印尼", 
                Region = "亞洲",
                UtcOffsetMinutes = 420,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "馬尼拉", 
                Timezone = "Asia/Manila", 
                DisplayName = "菲律賓標準時間", 
                Country = "菲律賓", 
                Region = "亞洲",
                UtcOffsetMinutes = 480,
                SupportsDST = false
            },
            
            // 歐美地區
            new EnhancedTimezoneInfo 
            { 
                City = "倫敦", 
                Timezone = "Europe/London", 
                DisplayName = "格林威治標準時間", 
                Country = "英國", 
                Region = "歐洲",
                UtcOffsetMinutes = 0,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "巴黎", 
                Timezone = "Europe/Paris", 
                DisplayName = "中歐時間", 
                Country = "法國", 
                Region = "歐洲",
                UtcOffsetMinutes = 60,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "紐約", 
                Timezone = "America/New_York", 
                DisplayName = "美國東部時間", 
                Country = "美國", 
                Region = "北美洲",
                UtcOffsetMinutes = -300,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "洛杉磯", 
                Timezone = "America/Los_Angeles", 
                DisplayName = "美國太平洋時間", 
                Country = "美國", 
                Region = "北美洲",
                UtcOffsetMinutes = -480,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "芝加哥", 
                Timezone = "America/Chicago", 
                DisplayName = "美國中部時間", 
                Country = "美國", 
                Region = "北美洲",
                UtcOffsetMinutes = -360,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "多倫多", 
                Timezone = "America/Toronto", 
                DisplayName = "加拿大東部時間", 
                Country = "加拿大", 
                Region = "北美洲",
                UtcOffsetMinutes = -300,
                SupportsDST = true
            },

            // 其他地區
            new EnhancedTimezoneInfo 
            { 
                City = "雪梨", 
                Timezone = "Australia/Sydney", 
                DisplayName = "澳洲東部標準時間", 
                Country = "澳洲", 
                Region = "大洋洲",
                UtcOffsetMinutes = 600,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "墨爾本", 
                Timezone = "Australia/Melbourne", 
                DisplayName = "澳洲東部標準時間", 
                Country = "澳洲", 
                Region = "大洋洲",
                UtcOffsetMinutes = 600,
                SupportsDST = true
            },
            new EnhancedTimezoneInfo 
            { 
                City = "杜拜", 
                Timezone = "Asia/Dubai", 
                DisplayName = "海灣標準時間", 
                Country = "阿聯", 
                Region = "中東",
                UtcOffsetMinutes = 240,
                SupportsDST = false
            },
            new EnhancedTimezoneInfo 
            { 
                City = "開羅", 
                Timezone = "Africa/Cairo", 
                DisplayName = "東歐時間", 
                Country = "埃及", 
                Region = "非洲",
                UtcOffsetMinutes = 120,
                SupportsDST = false
            }
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
            _logger.LogInformation("多時區時鐘頁面載入（P2增強版）");
        }

        /// <summary>
        /// 取得多時區時間資訊 API
        /// </summary>
        /// <returns>世界各地時區時間資訊</returns>
        public JsonResult OnGetWorldTimes()
        {
            try
            {
                var worldTimes = _supportedTimezones.Select(tz => 
                {
                    try
                    {
                        var utcNow = DateTime.UtcNow;
                        var localTime = DateTime.Now;
                        
                        // 使用系統時區轉換
                        var tzTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utcNow, tz.Timezone);
                        
                        // 計算時差
                        var offset = (tzTime - localTime).TotalHours;
                        var offsetString = offset >= 0 ? $"+{Math.Round(offset)}" : $"{Math.Round(offset)}";

                        return new
                        {
                            city = tz.City,
                            timezone = tz.Timezone,
                            displayName = tz.DisplayName,
                            country = tz.Country,
                            region = tz.Region,
                            currentTime = tzTime.ToString("yyyy/MM/dd HH:mm:ss"),
                            offset = offsetString,
                            utcOffsetMinutes = tz.UtcOffsetMinutes,
                            supportsDST = tz.SupportsDST,
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
                            country = tz.Country,
                            region = tz.Region,
                            currentTime = "時間取得失敗",
                            offset = "N/A",
                            utcOffsetMinutes = tz.UtcOffsetMinutes,
                            supportsDST = tz.SupportsDST,
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

        /// <summary>
        /// 取得支援的時間格式清單 API
        /// </summary>
        /// <returns>時間格式選項</returns>
        public JsonResult OnGetTimeFormats()
        {
            try
            {
                var timeFormats = new
                {
                    time24Hour = new[]
                    {
                        new { value = "HH:MM:SS", label = "HH:MM:SS", example = DateTime.Now.ToString("HH:mm:ss") },
                        new { value = "HH:MM", label = "HH:MM", example = DateTime.Now.ToString("HH:mm") },
                        new { value = "HH時MM分SS秒", label = "HH時MM分SS秒", example = DateTime.Now.ToString("HH時mm分ss秒") },
                        new { value = "HH時MM分", label = "HH時MM分", example = DateTime.Now.ToString("HH時mm分") }
                    },
                    time12Hour = new[]
                    {
                        new { value = "hh:MM:SS tt", label = "hh:MM:SS AM/PM", example = DateTime.Now.ToString("hh:mm:ss tt") },
                        new { value = "hh:MM tt", label = "hh:MM AM/PM", example = DateTime.Now.ToString("hh:mm tt") },
                        new { value = "hh時MM分SS秒 tt", label = "hh時MM分SS秒 上午/下午", example = DateTime.Now.ToString("hh時mm分ss秒 tt") },
                        new { value = "hh時MM分 tt", label = "hh時MM分 上午/下午", example = DateTime.Now.ToString("hh時mm分 tt") }
                    },
                    dateFormats = new[]
                    {
                        new { value = "YYYY/MM/DD", label = "YYYY/MM/DD", example = DateTime.Now.ToString("yyyy/MM/dd") },
                        new { value = "YYYY-MM-DD", label = "YYYY-MM-DD", example = DateTime.Now.ToString("yyyy-MM-dd") },
                        new { value = "MM/DD/YYYY", label = "MM/DD/YYYY", example = DateTime.Now.ToString("MM/dd/yyyy") },
                        new { value = "DD/MM/YYYY", label = "DD/MM/YYYY", example = DateTime.Now.ToString("dd/MM/yyyy") },
                        new { value = "YYYY年MM月DD日", label = "YYYY年MM月DD日", example = DateTime.Now.ToString("yyyy年MM月dd日") },
                        new { value = "MM月DD日", label = "MM月DD日", example = DateTime.Now.ToString("MM月dd日") },
                        new { value = "dddd, YYYY年MM月DD日", label = "星期X, YYYY年MM月DD日", example = DateTime.Now.ToString("dddd, yyyy年MM月dd日") }
                    }
                };

                return new JsonResult(new { success = true, data = timeFormats });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得時間格式失敗");
                return new JsonResult(new { success = false, error = "取得時間格式失敗" });
            }
        }

        /// <summary>
        /// 取得支援的語言清單 API
        /// </summary>
        /// <returns>語言選項</returns>
        public JsonResult OnGetLanguages()
        {
            try
            {
                var languages = new[]
                {
                    new { code = "zh-TW", name = "繁體中文", nativeName = "繁體中文", flag = "🇹🇼" },
                    new { code = "zh-CN", name = "简体中文", nativeName = "简体中文", flag = "🇨🇳" },
                    new { code = "en-US", name = "English", nativeName = "English", flag = "🇺🇸" },
                    new { code = "ja-JP", name = "日本語", nativeName = "日本語", flag = "🇯🇵" }
                };

                return new JsonResult(new { success = true, data = languages });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得語言清單失敗");
                return new JsonResult(new { success = false, error = "取得語言清單失敗" });
            }
        }
    }

    /// <summary>
    /// 增強型時區資訊類別（P2版本）
    /// </summary>
    public class EnhancedTimezoneInfo
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

        /// <summary>
        /// 國家名稱
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// 地區分類
        /// </summary>
        public string Region { get; set; } = string.Empty;

        /// <summary>
        /// UTC偏移量（分鐘）
        /// </summary>
        public int UtcOffsetMinutes { get; set; }

        /// <summary>
        /// 是否支援日光節約時間
        /// </summary>
        public bool SupportsDST { get; set; }
    }

    /// <summary>
    /// 時間格式設定類別
    /// </summary>
    public class TimeFormatSettings
    {
        /// <summary>
        /// 使用24小時制
        /// </summary>
        public bool Use24HourFormat { get; set; } = true;

        /// <summary>
        /// 顯示秒數
        /// </summary>
        public bool ShowSeconds { get; set; } = true;

        /// <summary>
        /// 顯示日期
        /// </summary>
        public bool ShowDate { get; set; } = true;

        /// <summary>
        /// 時間格式字串
        /// </summary>
        public string TimeFormat { get; set; } = "HH:MM:SS";

        /// <summary>
        /// 日期格式字串
        /// </summary>
        public string DateFormat { get; set; } = "YYYY/MM/DD";

        /// <summary>
        /// 日期分隔符
        /// </summary>
        public string DateSeparator { get; set; } = "/";

        /// <summary>
        /// 顯示星期
        /// </summary>
        public bool ShowWeekday { get; set; } = false;
    }

    /// <summary>
    /// 使用者偏好設定類別
    /// </summary>
    public class UserPreferences
    {
        /// <summary>
        /// 語言設定
        /// </summary>
        public string Language { get; set; } = "zh-TW";

        /// <summary>
        /// 時區設定
        /// </summary>
        public string TimeZone { get; set; } = "Asia/Taipei";

        /// <summary>
        /// 主題設定
        /// </summary>
        public string Theme { get; set; } = "auto"; // "light", "dark", "auto"

        /// <summary>
        /// 時間格式設定
        /// </summary>
        public TimeFormatSettings TimeFormat { get; set; } = new();

        /// <summary>
        /// 最後更新時間
        /// </summary>
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 時區資訊類別（向後相容）
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
