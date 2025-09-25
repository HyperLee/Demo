using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Demo.Pages;

/// <summary>
/// Provides the server-side backing logic for the Pomodoro Technique experience, exposing default durations, localization hints, and onboarding metadata.
/// </summary>
public sealed class PomodoroTechniqueModel : PageModel
{
    private const string PomodoroConfigurationSectionName = "PomodoroSettings";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly ILogger<PomodoroTechniqueModel> logger;
    private readonly IConfiguration configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="PomodoroTechniqueModel"/> class.
    /// </summary>
    /// <param name="logger">The logger used to emit diagnostic messages.</param>
    /// <param name="configuration">The application configuration used for optional overrides.</param>
    public PomodoroTechniqueModel(ILogger<PomodoroTechniqueModel> logger, IConfiguration configuration)
    {
        this.logger = logger;
        this.configuration = configuration;
    }

    /// <summary>
    /// Gets the default timer configuration that the client uses when no customized preference is found in local storage.
    /// </summary>
    public PomodoroDefaultSettings DefaultSettings { get; private set; } = null!;

    /// <summary>
    /// Gets the serialized localization manifest that the client can parse to enable runtime culture switching.
    /// </summary>
    public string LocalizationJson { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the serialized onboarding steps that describe the quick tutorial sequence.
    /// </summary>
    public string OnboardingJson { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the semantic version of the onboarding flow so the client can decide whether to resurface updated guidance.
    /// </summary>
    public string OnboardingVersion => "1.0.0";

    /// <summary>
    /// Handles the initial GET request by assembling configuration and metadata payloads for the Razor page.
    /// </summary>
    public void OnGet()
    {
        DefaultSettings = BuildDefaultSettings();
        var localizationManifest = BuildLocalizationManifest();
        var onboardingSteps = BuildOnboardingSteps();

        LocalizationJson = JsonSerializer.Serialize(localizationManifest, SerializerOptions);
        OnboardingJson = JsonSerializer.Serialize(onboardingSteps, SerializerOptions);

        logger.LogInformation("Pomodoro Technique page model initialized with focus duration {FocusMinutes} minutes and long break every {LongBreakInterval} sessions.", DefaultSettings.FocusMinutes, DefaultSettings.LongBreakInterval);
    }

    /// <summary>
    /// Computes the effective default settings, merging optional values found in configuration with the documented defaults.
    /// </summary>
    /// <returns>The composed <see cref="PomodoroDefaultSettings"/> instance.</returns>
    private PomodoroDefaultSettings BuildDefaultSettings()
    {
        var section = configuration.GetSection(PomodoroConfigurationSectionName);

        var focusMinutes = section.GetValue("FocusMinutes", 25);
        var shortBreakMinutes = section.GetValue("ShortBreakMinutes", 5);
        var longBreakMinutes = section.GetValue("LongBreakMinutes", 15);
        var longBreakInterval = section.GetValue("LongBreakInterval", 4);
        var audioEnabled = section.GetValue("AudioEnabledByDefault", true);

        var result = new PomodoroDefaultSettings(focusMinutes, shortBreakMinutes, longBreakMinutes, longBreakInterval, audioEnabled);

        logger.LogDebug("Calculated Pomodoro defaults {@Defaults} from configuration section {SectionName}.", result, PomodoroConfigurationSectionName);

        return result;
    }

    /// <summary>
    /// Builds a minimal localization manifest to bootstrap the client-side translator.
    /// </summary>
    /// <returns>A manifest keyed by culture code.</returns>
    private static PomodoroLocalizationManifest BuildLocalizationManifest()
    {
        var resources = new Dictionary<string, IReadOnlyDictionary<string, string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["zh-TW"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["focus"] = "專注中",
                ["shortBreak"] = "短休息",
                ["longBreak"] = "長休息",
                ["idle"] = "準備開始",
                ["paused"] = "已暫停",
                ["start"] = "開始",
                ["pause"] = "暫停",
                ["resume"] = "繼續",
                ["reset"] = "重置",
                ["skip"] = "跳過休息",
                ["toggleAudioOn"] = "開啟提示音",
                ["toggleAudioOff"] = "關閉提示音",
                ["dailySummary"] = "今日統計",
                ["completedSessions"] = "已完成番茄鐘",
                ["totalFocusMinutes"] = "累積專注分鐘",
                ["currentLoop"] = "迴圈進度",
                ["noSessionsMessage"] = "尚未有完成的番茄鐘，現在就開始你的第一輪吧！",
                ["shortBreakToast"] = "工作完成，休息 5 分鐘。",
                ["longBreakToast"] = "四個番茄鐘完成，建議長休息 15–30 分鐘。",
                ["restCompleteToast"] = "休息結束，準備下一輪專注。",
                ["skipBreakToast"] = "已跳過休息，進入下一個番茄鐘。",
                ["resetAnnouncement"] = "已回到初始狀態。",
                ["pageHiddenAnnouncement"] = "頁面已隱藏，請記得返回掌握狀態。",
                ["tutorialTitle"] = "快速導覽",
                ["tutorialStep1"] = "輸入你的任務名稱並啟動 25 分鐘倒數。",
                ["tutorialStep2"] = "當需要暫停或重置時，使用控制按鈕。",
                ["tutorialStep3"] = "倒數結束會提醒你休息，別忘了放鬆。",
                ["tutorialStep4"] = "查看今日統計，追蹤投入的時間。",
                ["tutorialDismiss"] = "我知道了",
                ["statsPlaceholderTask"] = "未命名任務"
            }.ToImmutableDictionary(),
            ["en-US"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["focus"] = "Focusing",
                ["shortBreak"] = "Short Break",
                ["longBreak"] = "Long Break",
                ["idle"] = "Ready",
                ["paused"] = "Paused",
                ["start"] = "Start",
                ["pause"] = "Pause",
                ["resume"] = "Resume",
                ["reset"] = "Reset",
                ["skip"] = "Skip Break",
                ["toggleAudioOn"] = "Enable chime",
                ["toggleAudioOff"] = "Mute chime",
                ["dailySummary"] = "Today",
                ["completedSessions"] = "Completed Pomodoros",
                ["totalFocusMinutes"] = "Total focus minutes",
                ["currentLoop"] = "Loop progress",
                ["noSessionsMessage"] = "No pomodoros yet—start your first focus sprint now!",
                ["shortBreakToast"] = "Great work! Take a 5-minute break.",
                ["longBreakToast"] = "Four pomodoros done. Enjoy a long 15–30 minute break.",
                ["restCompleteToast"] = "Break finished. Let’s get back to focus.",
                ["skipBreakToast"] = "Break skipped. Jumping into the next pomodoro.",
                ["resetAnnouncement"] = "Timer reset to defaults.",
                ["pageHiddenAnnouncement"] = "The page is hidden—come back soon to stay in rhythm.",
                ["tutorialTitle"] = "Quick tour",
                ["tutorialStep1"] = "Enter a task and launch a 25-minute timer.",
                ["tutorialStep2"] = "Pause or reset when life interrupts you.",
                ["tutorialStep3"] = "Break reminders fire when the countdown ends.",
                ["tutorialStep4"] = "Check today’s stats to stay on track.",
                ["tutorialDismiss"] = "Got it",
                ["statsPlaceholderTask"] = "Untitled task"
            }.ToImmutableDictionary()
        };

        return new PomodoroLocalizationManifest("zh-TW", resources.ToImmutableDictionary());
    }

    /// <summary>
    /// Produces the onboarding steps shown in the first-run teaching overlay.
    /// </summary>
    /// <returns>A read-only list of onboarding steps.</returns>
    private static IReadOnlyList<PomodoroOnboardingStep> BuildOnboardingSteps()
    {
        return new List<PomodoroOnboardingStep>
        {
            new("task", "輸入任務", "填寫最多 120 字元的任務名稱，專注前先鎖定目標。", "ri-pencil-line"),
            new("start", "啟動計時", "按下開始後倒數即刻啟動，保持專注直到提醒響起。", "ri-timer-line"),
            new("rest", "按節奏休息", "提醒響起後切換至短休息，連續四次工作後享受長休息。", "ri-cup-line"),
            new("review", "追蹤成果", "利用右側統計區掌握今日累積專注時間與番茄鐘數量。", "ri-bar-chart-box-line")
        }.ToImmutableArray();
    }
}

/// <summary>
/// Represents the baseline configuration for the Pomodoro timer.
/// </summary>
/// <param name="FocusMinutes">The default focus duration in minutes.</param>
/// <param name="ShortBreakMinutes">The default short break duration in minutes.</param>
/// <param name="LongBreakMinutes">The default long break duration in minutes.</param>
/// <param name="LongBreakInterval">The number of completed focus sessions before triggering a long break.</param>
/// <param name="AudioEnabledByDefault">Indicates whether audio cues start enabled.</param>
public sealed record PomodoroDefaultSettings(int FocusMinutes, int ShortBreakMinutes, int LongBreakMinutes, int LongBreakInterval, bool AudioEnabledByDefault);

/// <summary>
/// Represents the localized resource manifest supplied to the client.
/// </summary>
/// <param name="DefaultCulture">The culture code that should be applied when none is specified.</param>
/// <param name="Cultures">A mapping of culture codes to localized strings.</param>
public sealed record PomodoroLocalizationManifest(string DefaultCulture, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Cultures);

/// <summary>
/// Describes a single step in the onboarding tour.
/// </summary>
/// <param name="Id">A stable identifier for the step.</param>
/// <param name="Title">The localized title of the step.</param>
/// <param name="Description">The localized body text for the step.</param>
/// <param name="Icon">An icon key used by the client-side renderer.</param>
public sealed record PomodoroOnboardingStep(string Id, string Title, string Description, string Icon);
