using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Demo.Models;

/// <summary>
/// 待辦任務模型
/// </summary>
public class TodoTask
{
    /// <summary>
    /// 任務唯一識別碼
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 任務標題
    /// </summary>
    [Required(ErrorMessage = "任務標題為必填欄位")]
    [StringLength(100, ErrorMessage = "任務標題不能超過100個字元")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 任務詳細描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 任務狀態
    /// </summary>
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    /// <summary>
    /// 任務優先級
    /// </summary>
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;

    /// <summary>
    /// 任務分類
    /// </summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>
    /// 任務標籤清單
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedDate { get; set; } = DateTime.Now;

    /// <summary>
    /// 到期日期（可選）
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// 完成日期（可選）
    /// </summary>
    public DateTime? CompletedDate { get; set; }

    /// <summary>
    /// 預估完成時間（分鐘）
    /// </summary>
    [Range(5, 480, ErrorMessage = "預估時間應在5-480分鐘之間")]
    public int EstimatedMinutes { get; set; }

    /// <summary>
    /// 實際花費時間（分鐘）
    /// </summary>
    public int ActualMinutes { get; set; }

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }

    /// <summary>
    /// 任務是否已完成
    /// </summary>
    [JsonIgnore]
    public bool IsCompleted => Status == TodoStatus.Completed;

    /// <summary>
    /// 任務是否已逾期
    /// </summary>
    [JsonIgnore]
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && !IsCompleted;

    /// <summary>
    /// 取得任務狀態的中文顯示名稱
    /// </summary>
    [JsonIgnore]
    public string StatusDisplayName => Status switch
    {
        TodoStatus.Pending => "待處理",
        TodoStatus.InProgress => "進行中",
        TodoStatus.Completed => "已完成",
        TodoStatus.Cancelled => "已取消",
        _ => "未知"
    };

    /// <summary>
    /// 取得優先級的中文顯示名稱
    /// </summary>
    [JsonIgnore]
    public string PriorityDisplayName => Priority switch
    {
        TodoPriority.Low => "低優先級",
        TodoPriority.Medium => "中優先級",
        TodoPriority.High => "高優先級",
        _ => "未設定"
    };
}

/// <summary>
/// 任務狀態列舉
/// </summary>
public enum TodoStatus
{
    /// <summary>待處理</summary>
    Pending,
    /// <summary>進行中</summary>
    InProgress,
    /// <summary>已完成</summary>
    Completed,
    /// <summary>已取消</summary>
    Cancelled
}

/// <summary>
/// 任務優先級列舉
/// </summary>
public enum TodoPriority
{
    /// <summary>低優先級</summary>
    Low,
    /// <summary>中優先級</summary>
    Medium,
    /// <summary>高優先級</summary>
    High
}

/// <summary>
/// 任務分類模型
/// </summary>
public class TodoCategory
{
    /// <summary>
    /// 分類識別碼
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分類名稱
    /// </summary>
    [Required(ErrorMessage = "分類名稱為必填欄位")]
    [StringLength(50, ErrorMessage = "分類名稱不能超過50個字元")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 分類顏色（十六進制色碼）
    /// </summary>
    public string Color { get; set; } = "#007bff";

    /// <summary>
    /// 分類圖標（Font Awesome 類別名）
    /// </summary>
    public string Icon { get; set; } = "fas fa-folder";

    /// <summary>
    /// 排序順序
    /// </summary>
    public int SortOrder { get; set; }
}

/// <summary>
/// 任務統計資料模型
/// </summary>
public class TodoStatistics
{
    /// <summary>
    /// 待處理任務數量
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// 進行中任務數量
    /// </summary>
    public int InProgressCount { get; set; }

    /// <summary>
    /// 已完成任務數量
    /// </summary>
    public int CompletedCount { get; set; }

    /// <summary>
    /// 已逾期任務數量
    /// </summary>
    public int OverdueCount { get; set; }

    /// <summary>
    /// 今日任務數量
    /// </summary>
    public int TodayCount { get; set; }

    /// <summary>
    /// 本週任務數量
    /// </summary>
    public int ThisWeekCount { get; set; }

    /// <summary>
    /// 完成率（百分比）
    /// </summary>
    public double CompletionRate { get; set; }

    /// <summary>
    /// 平均完成時間（分鐘）
    /// </summary>
    public double AverageCompletionTime { get; set; }

    /// <summary>
    /// 總任務數量
    /// </summary>
    public int TotalTasks { get; set; }
}

/// <summary>
/// 任務資料容器模型（用於JSON序列化）
/// </summary>
public class TodoData
{
    /// <summary>
    /// 任務清單
    /// </summary>
    public List<TodoTask> Tasks { get; set; } = [];

    /// <summary>
    /// 下一個任務ID
    /// </summary>
    public int NextId { get; set; } = 1;
}

/// <summary>
/// 分類資料容器模型（用於JSON序列化）
/// </summary>
public class TodoCategoryData
{
    /// <summary>
    /// 分類清單
    /// </summary>
    public List<TodoCategory> Categories { get; set; } = [];

    /// <summary>
    /// 下一個分類ID
    /// </summary>
    public int NextId { get; set; } = 1;
}
