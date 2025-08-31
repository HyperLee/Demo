using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Demo.Models
{
    /// <summary>
    /// 習慣實體模型
    /// </summary>
    public class Habit
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "習慣名稱為必填項")]
        [StringLength(100, ErrorMessage = "習慣名稱不能超過100個字元")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(500, ErrorMessage = "描述不能超過500個字元")]
        public string Description { get; set; } = string.Empty;
        
        public string IconClass { get; set; } = "fas fa-star";
        
        [Required(ErrorMessage = "請選擇習慣分類")]
        public string CategoryId { get; set; } = string.Empty;
        
        public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public DateTime? TargetEndDate { get; set; }
        
        public bool IsActive { get; set; } = true;
        
        public string Color { get; set; } = "#007bff";
        
        [Range(1, 10, ErrorMessage = "每日目標次數必須在1-10之間")]
        public int TargetCount { get; set; } = 1;
        
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// 習慣記錄實體模型
    /// </summary>
    public class HabitRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        public string HabitId { get; set; } = string.Empty;
        
        public DateTime Date { get; set; }
        
        [Range(0, 10, ErrorMessage = "完成次數必須在0-10之間")]
        public int CompletedCount { get; set; } = 0;
        
        [StringLength(200, ErrorMessage = "備註不能超過200個字元")]
        public string Notes { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// 習慣分類實體模型
    /// </summary>
    public class HabitCategory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required(ErrorMessage = "分類名稱為必填項")]
        [StringLength(50, ErrorMessage = "分類名稱不能超過50個字元")]
        public string Name { get; set; } = string.Empty;
        
        [StringLength(200, ErrorMessage = "描述不能超過200個字元")]
        public string Description { get; set; } = string.Empty;
        
        public string IconClass { get; set; } = "fas fa-folder";
        
        public string Color { get; set; } = "#6c757d";
        
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// 習慣頻率列舉
    /// </summary>
    public enum HabitFrequency
    {
        Daily = 1,      // 每日
        Weekly = 2,     // 每週
        Monthly = 3,    // 每月
        Custom = 4      // 自訂
    }

    /// <summary>
    /// 習慣頁面檢視模型
    /// </summary>
    public class HabitsPageModel
    {
        public List<HabitViewModel> TodayHabits { get; set; } = new List<HabitViewModel>();
        public List<HabitCategory> Categories { get; set; } = new List<HabitCategory>();
        public int TodayCompleted { get; set; }
        public int TotalHabits { get; set; }
        public double WeeklySuccessRate { get; set; }
        public int LongestStreak { get; set; }
        public List<HabitProgressData> WeeklyProgress { get; set; } = new List<HabitProgressData>();
    }

    /// <summary>
    /// 習慣檢視模型
    /// </summary>
    public class HabitViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public HabitCategory Category { get; set; } = new HabitCategory();
        public int CurrentStreak { get; set; }
        public int TotalCompletions { get; set; }
        public double CompletionRate { get; set; }
        public bool IsTodayCompleted { get; set; }
        public string Color { get; set; } = string.Empty;
        public int TargetCount { get; set; } = 1;
        public int TodayCompleted { get; set; } = 0;
        public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// 習慣進度資料
    /// </summary>
    public class HabitProgressData
    {
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
        public double SuccessRate { get; set; }
    }

    /// <summary>
    /// 新增/編輯習慣的請求模型
    /// </summary>
    public class CreateHabitRequest
    {
        [Required(ErrorMessage = "習慣名稱為必填項")]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fas fa-star";
        
        [Required(ErrorMessage = "請選擇分類")]
        public string CategoryId { get; set; } = string.Empty;
        
        public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
        public DateTime? TargetEndDate { get; set; }
        public string Color { get; set; } = "#007bff";
        public int TargetCount { get; set; } = 1;
        public List<string> Tags { get; set; } = new List<string>();
    }

    /// <summary>
    /// 標記完成請求模型
    /// </summary>
    public class MarkCompleteRequest
    {
        [Required]
        public string HabitId { get; set; } = string.Empty;
        
        public DateTime Date { get; set; } = DateTime.Today;
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// 習慣統計模型
    /// </summary>
    public class HabitStatistics
    {
        public string HabitId { get; set; } = string.Empty;
        public string HabitName { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalCompletions { get; set; }
        public double CompletionRate { get; set; }
        public int DaysActive { get; set; }
        public DateTime LastCompleted { get; set; }
        public List<HabitProgressData> RecentProgress { get; set; } = new List<HabitProgressData>();
    }
}
