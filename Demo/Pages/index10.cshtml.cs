using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace Demo.Pages
{
    /// <summary>
    /// 世界時鐘頁面 - 顯示多個時區的當前時間並提供城市特效功能
    /// </summary>
    public class index10 : PageModel
    {
        private readonly ILogger<index10> _logger;

        public index10(ILogger<index10> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 世界時鐘檢視模型
        /// </summary>
        public WorldClockViewModel WorldClock { get; set; } = new();

        /// <summary>
        /// 當前選中的時區 ID
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? SelectedTimeZoneId { get; set; }

        /// <summary>
        /// 當前選中的語言
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public string? SelectedLanguage { get; set; }

        /// <summary>
        /// 頁面載入處理
        /// </summary>
        public void OnGet()
        {
            InitializeWorldClock();
            _logger.LogInformation("世界時鐘頁面已載入，當前時區: {TimeZone}", SelectedTimeZoneId ?? "Asia/Taipei");
        }

        /// <summary>
        /// 初始化世界時鐘資料
        /// </summary>
        private void InitializeWorldClock()
        {
            // 設定預設值
            SelectedTimeZoneId ??= "Asia/Taipei";
            SelectedLanguage ??= "zh-tw";

            // 建立檢視模型
            WorldClock = new WorldClockViewModel
            {
                CurrentTimeZone = SelectedTimeZoneId,
                SelectedLanguage = SelectedLanguage,
                SupportedTimeZones = GetSupportedTimeZones(),
                Localizations = GetLocalizations(SelectedLanguage),
                CityEffects = GetCityEffectConfigs(),
                AudioSettings = new AudioSettings()
            };

            // 計算當前時間
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(ConvertToSystemTimeZoneId(SelectedTimeZoneId));
            WorldClock.CurrentTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneInfo);
        }

        /// <summary>
        /// 取得支援的時區清單
        /// </summary>
        private List<TimeZoneData> GetSupportedTimeZones()
        {
            var supportedZones = new List<(string Id, string DisplayName, string LocalizedNameCn, string LocalizedNameEn)>
            {
                ("Asia/Taipei", "台北", "台北", "Taipei"),
                ("Asia/Tokyo", "東京", "東京", "Tokyo"),
                ("America/New_York", "紐約", "紐約", "New York"),
                ("Europe/London", "倫敦", "倫敦", "London"),
                ("Europe/Paris", "巴黎", "巴黎", "Paris"),
                ("Europe/Berlin", "柏林", "柏林", "Berlin"),
                ("Europe/Moscow", "莫斯科", "莫斯科", "Moscow"),
                ("Australia/Sydney", "雪梨", "雪梨", "Sydney")
            };

            return supportedZones.Select(zone =>
            {
                var systemId = ConvertToSystemTimeZoneId(zone.Id);
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(systemId);
                
                return new TimeZoneData
                {
                    Id = zone.Id,
                    DisplayName = zone.DisplayName,
                    LocalizedName = SelectedLanguage == "zh-tw" ? zone.LocalizedNameCn : zone.LocalizedNameEn,
                    Offset = timeZoneInfo.GetUtcOffset(DateTime.UtcNow),
                    SupportsDaylightSavingTime = timeZoneInfo.SupportsDaylightSavingTime
                };
            }).ToList();
        }

        /// <summary>
        /// 取得本地化字串
        /// </summary>
        private Dictionary<string, string> GetLocalizations(string language)
        {
            var localizations = new Dictionary<string, string>();

            if (language == "zh-tw")
            {
                var zhTwLocalizations = new Dictionary<string, string>
                {
                    { "pageTitle", "世界時鐘" },
                    { "currentLocation", "當前位置" },
                    { "syncTime", "校時" },
                    { "monday", "星期一" },
                    { "tuesday", "星期二" },
                    { "wednesday", "星期三" },
                    { "thursday", "星期四" },
                    { "friday", "星期五" },
                    { "saturday", "星期六" },
                    { "sunday", "星期日" },
                    { "audioEnabled", "音效開啟" },
                    { "audioDisabled", "音效關閉" },
                    { "volumeControl", "音量控制" }
                };
                foreach (var item in zhTwLocalizations)
                {
                    localizations[item.Key] = item.Value;
                }
            }
            else // en-us
            {
                var enUsLocalizations = new Dictionary<string, string>
                {
                    { "pageTitle", "World Clock" },
                    { "currentLocation", "Current Location" },
                    { "syncTime", "Sync Time" },
                    { "monday", "Monday" },
                    { "tuesday", "Tuesday" },
                    { "wednesday", "Wednesday" },
                    { "thursday", "Thursday" },
                    { "friday", "Friday" },
                    { "saturday", "Saturday" },
                    { "sunday", "Sunday" },
                    { "audioEnabled", "Audio Enabled" },
                    { "audioDisabled", "Audio Disabled" },
                    { "volumeControl", "Volume Control" }
                };
                foreach (var item in enUsLocalizations)
                {
                    localizations[item.Key] = item.Value;
                }
            }

            return localizations;
        }

        /// <summary>
        /// 取得城市特效設定
        /// </summary>
        private List<CityEffectConfig> GetCityEffectConfigs()
        {
            return new List<CityEffectConfig>
            {
                new() 
                {
                    CityId = "Asia/Taipei",
                    CityName = "台北",
                    PrimaryColor = "#003D79",
                    SecondaryColor = "#FF69B4",
                    EffectType = "sakura",
                    DurationMs = 2500,
                    AnimationElements = new List<string> { "sakura-petals", "taipei-101", "night-market-glow" },
                    SoundFile = "taipei-bell-chime.mp3",
                    SoundDuration = 1.5m,
                    HasParticleSystem = true,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "Asia/Tokyo",
                    CityName = "東京",
                    PrimaryColor = "#DC143C",
                    SecondaryColor = "#F8F8FF",
                    EffectType = "japanese",
                    DurationMs = 3000,
                    AnimationElements = new List<string> { "paper-fan", "fuji-mountain", "cherry-blossoms", "washi-texture" },
                    SoundFile = "tokyo-wind-chime.mp3",
                    SoundDuration = 2.0m,
                    HasParticleSystem = true,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "America/New_York",
                    CityName = "紐約",
                    PrimaryColor = "#478778",
                    SecondaryColor = "#FFFF00",
                    EffectType = "urban",
                    DurationMs = 2000,
                    AnimationElements = new List<string> { "skyscrapers", "yellow-taxi", "neon-lights", "grid-scan" },
                    SoundFile = "newyork-city-buzz.mp3",
                    SoundDuration = 1.8m,
                    HasParticleSystem = false,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "Europe/London",
                    CityName = "倫敦",
                    PrimaryColor = "#012169",
                    SecondaryColor = "#C8102E",
                    EffectType = "british",
                    DurationMs = 3500,
                    AnimationElements = new List<string> { "big-ben-pendulum", "rain-drops", "double-decker-bus", "fog-mist" },
                    SoundFile = "london-big-ben.mp3",
                    SoundDuration = 3.0m,
                    HasParticleSystem = true,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "Europe/Paris",
                    CityName = "巴黎",
                    PrimaryColor = "#0055A4",
                    SecondaryColor = "#EF4135",
                    EffectType = "french",
                    DurationMs = 3000,
                    AnimationElements = new List<string> { "eiffel-tower", "champagne-bubbles", "rose-petals", "golden-rays" },
                    SoundFile = "paris-accordion.mp3",
                    SoundDuration = 2.5m,
                    HasParticleSystem = true,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "Europe/Berlin",
                    CityName = "柏林",
                    PrimaryColor = "#000000",
                    SecondaryColor = "#DD0000",
                    EffectType = "german",
                    DurationMs = 2500,
                    AnimationElements = new List<string> { "brandenburg-gate", "flag-stripes", "industrial-gears", "geometric-lines" },
                    SoundFile = "berlin-classical.mp3",
                    SoundDuration = 2.2m,
                    HasParticleSystem = false,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "Europe/Moscow",
                    CityName = "莫斯科",
                    PrimaryColor = "#D52B1E",
                    SecondaryColor = "#FFD700",
                    EffectType = "russian",
                    DurationMs = 3000,
                    AnimationElements = new List<string> { "onion-domes", "snowflakes", "red-square-bricks", "golden-stars" },
                    SoundFile = "moscow-bells.mp3",
                    SoundDuration = 2.8m,
                    HasParticleSystem = true,
                    SoundEnabled = true
                },
                new() 
                {
                    CityId = "Australia/Sydney",
                    CityName = "雪梨",
                    PrimaryColor = "#0057B8",
                    SecondaryColor = "#FF8C00",
                    EffectType = "australian",
                    DurationMs = 3500,
                    AnimationElements = new List<string> { "opera-house", "ocean-waves", "kangaroo-silhouette", "sunrise" },
                    SoundFile = "sydney-ocean-waves.mp3",
                    SoundDuration = 3.0m,
                    HasParticleSystem = true,
                    SoundEnabled = true
                }
            };
        }

        /// <summary>
        /// 轉換 IANA 時區 ID 為 Windows 系統時區 ID
        /// </summary>
        private static string ConvertToSystemTimeZoneId(string ianaId)
        {
            return ianaId switch
            {
                "Asia/Taipei" => "Taipei Standard Time",
                "Asia/Tokyo" => "Tokyo Standard Time",
                "America/New_York" => "Eastern Standard Time",
                "Europe/London" => "GMT Standard Time",
                "Europe/Paris" => "W. Europe Standard Time",
                "Europe/Berlin" => "W. Europe Standard Time",
                "Europe/Moscow" => "Russian Standard Time",
                "Australia/Sydney" => "AUS Eastern Standard Time",
                _ => "UTC"
            };
        }
    }

    /// <summary>
    /// 世界時鐘檢視模型
    /// </summary>
    public class WorldClockViewModel
    {
        public string CurrentTimeZone { get; set; } = string.Empty;
        public DateTime CurrentTime { get; set; }
        public List<TimeZoneData> SupportedTimeZones { get; set; } = new();
        public string SelectedLanguage { get; set; } = string.Empty;
        public Dictionary<string, string> Localizations { get; set; } = new();
        public List<CityEffectConfig> CityEffects { get; set; } = new();
        public AudioSettings AudioSettings { get; set; } = new();
    }

    /// <summary>
    /// 時區資料
    /// </summary>
    public class TimeZoneData
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string LocalizedName { get; set; } = string.Empty;
        public TimeSpan Offset { get; set; }
        public bool SupportsDaylightSavingTime { get; set; }
    }

    /// <summary>
    /// 城市特效設定
    /// </summary>
    public class CityEffectConfig
    {
        public string CityId { get; set; } = string.Empty;
        public string CityName { get; set; } = string.Empty;
        public string PrimaryColor { get; set; } = string.Empty;
        public string SecondaryColor { get; set; } = string.Empty;
        public string EffectType { get; set; } = string.Empty;
        public int DurationMs { get; set; }
        public List<string> AnimationElements { get; set; } = new();
        public string SoundFile { get; set; } = string.Empty;
        public decimal SoundDuration { get; set; }
        public bool HasParticleSystem { get; set; }
        public bool SoundEnabled { get; set; }
    }

    /// <summary>
    /// 音效設定
    /// </summary>
    public class AudioSettings
    {
        public bool Enabled { get; set; } = true;
        public decimal Volume { get; set; } = 0.7m;
        public bool PreloadSounds { get; set; } = true;
        public string FallbackSound { get; set; } = "default-chime.mp3";
    }
}
