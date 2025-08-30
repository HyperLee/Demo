using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Models;
using Demo.Services;
using System.Text.Json;

namespace Demo.Pages;

/// <summary>
/// 待辦清單頁面模型
/// </summary>
public class TodoModel : PageModel
{
    private readonly TodoService _todoService;
    private readonly ILogger<TodoModel> _logger;

    public TodoModel(TodoService todoService, ILogger<TodoModel> logger)
    {
        _todoService = todoService;
        _logger = logger;
    }

    #region 屬性

    /// <summary>
    /// 今日任務清單
    /// </summary>
    public List<TodoTask> TodayTasks { get; set; } = [];

    /// <summary>
    /// 明日任務清單
    /// </summary>
    public List<TodoTask> TomorrowTasks { get; set; } = [];

    /// <summary>
    /// 本週任務清單
    /// </summary>
    public List<TodoTask> ThisWeekTasks { get; set; } = [];

    /// <summary>
    /// 未來任務清單
    /// </summary>
    public List<TodoTask> FutureTasks { get; set; } = [];

    /// <summary>
    /// 無到期日任務清單
    /// </summary>
    public List<TodoTask> NoDueDateTasks { get; set; } = [];

    /// <summary>
    /// 已完成任務清單
    /// </summary>
    public List<TodoTask> CompletedTasks { get; set; } = [];

    /// <summary>
    /// 統計資訊
    /// </summary>
    public TodoStatistics Statistics { get; set; } = new();

    /// <summary>
    /// 分類清單
    /// </summary>
    public List<TodoCategory> Categories { get; set; } = [];

    /// <summary>
    /// 用於綁定表單的任務物件
    /// </summary>
    [BindProperty]
    public TodoTask Task { get; set; } = new();

    /// <summary>
    /// 待處理任務數量
    /// </summary>
    public int PendingCount => Statistics.PendingCount;

    /// <summary>
    /// 進行中任務數量
    /// </summary>
    public int InProgressCount => Statistics.InProgressCount;

    /// <summary>
    /// 已完成任務數量
    /// </summary>
    public int CompletedCount => Statistics.CompletedCount;

    /// <summary>
    /// 已逾期任務數量
    /// </summary>
    public int OverdueCount => Statistics.OverdueCount;

    #endregion

    #region 頁面處理方法

    /// <summary>
    /// GET 請求處理 - 載入頁面資料
    /// </summary>
    public void OnGet()
    {
        try
        {
            LoadPageData();
            _logger.LogInformation("待辦清單頁面載入成功，分類數量：{CategoryCount}", Categories.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入待辦清單頁面時發生錯誤");
            TempData["Error"] = "載入頁面資料時發生錯誤，請重新整理頁面。";
        }
    }

    /// <summary>
    /// 儲存任務（新增或更新）
    /// </summary>
    public IActionResult OnPostSave()
    {
        try
        {
            // 添加調試資訊
            _logger.LogInformation("接收到儲存請求，任務標題：{Title}", Task.Title);
            
            if (!ModelState.IsValid)
            {
                // 記錄驗證錯誤詳情
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) })
                    .ToList();
                
                _logger.LogWarning("模型驗證失敗：{Errors}", string.Join("; ", errors.SelectMany(e => e.Errors ?? [])));
                
                return new JsonResult(new { 
                    success = false, 
                    message = "資料驗證失敗，請檢查輸入內容。",
                    errors = errors 
                });
            }

            // 處理標籤字串轉換
            if (!string.IsNullOrWhiteSpace(Request.Form["TagsString"]))
            {
                var tagsString = Request.Form["TagsString"].ToString();
                Task.Tags = tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                    .Select(tag => tag.Trim())
                                    .Where(tag => !string.IsNullOrWhiteSpace(tag))
                                    .ToList();
            }

            bool success;
            if (Task.Id == 0)
            {
                // 新增任務
                success = _todoService.CreateTask(Task);
                var action = success ? "新增成功" : "新增失敗";
                _logger.LogInformation("嘗試新增任務 {Title}: {Action}", Task.Title, action);
            }
            else
            {
                // 更新任務
                success = _todoService.UpdateTask(Task);
                var action = success ? "更新成功" : "更新失敗";
                _logger.LogInformation("嘗試更新任務 {Id}: {Action}", Task.Id, action);
            }

            var message = success 
                ? (Task.Id == 0 ? "任務新增成功！" : "任務更新成功！")
                : (Task.Id == 0 ? "任務新增失敗，請稍後再試。" : "任務更新失敗，請稍後再試。");

            return new JsonResult(new { success, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存任務時發生錯誤");
            return new JsonResult(new { success = false, message = "儲存任務時發生錯誤，請稍後再試。" });
        }
    }

    /// <summary>
    /// 取得特定任務資料（用於編輯）
    /// </summary>
    public IActionResult OnGetTask(int id)
    {
        try
        {
            var task = _todoService.GetTaskById(id);
            if (task is null)
            {
                return new JsonResult(new { success = false, message = "找不到指定的任務。" });
            }

            // 準備標籤字串
            var tagsString = string.Join(", ", task.Tags);

            var result = new
            {
                success = true,
                task = new
                {
                    task.Id,
                    task.Title,
                    task.Description,
                    Status = task.Status.ToString(),
                    Priority = task.Priority.ToString(),
                    task.Category,
                    TagsString = tagsString,
                    DueDate = task.DueDate?.ToString("yyyy-MM-ddTHH:mm"),
                    task.EstimatedMinutes
                }
            };

            return new JsonResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得任務 {Id} 資料時發生錯誤", id);
            return new JsonResult(new { success = false, message = "取得任務資料時發生錯誤。" });
        }
    }

    /// <summary>
    /// 切換任務完成狀態
    /// </summary>
    public IActionResult OnPostToggleComplete(int id)
    {
        try
        {
            var result = _todoService.ToggleTaskComplete(id);
            if (result is null)
            {
                return new JsonResult(new { success = false, message = "找不到指定的任務。" });
            }

            _logger.LogInformation("任務 {Id} 狀態已切換為 {Status}", id, result.Value ? "已完成" : "未完成");
            return new JsonResult(new { success = true, isCompleted = result.Value });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換任務 {Id} 完成狀態時發生錯誤", id);
            return new JsonResult(new { success = false, message = "切換任務狀態時發生錯誤。" });
        }
    }

    /// <summary>
    /// 刪除任務
    /// </summary>
    public IActionResult OnPostDelete(int id)
    {
        try
        {
            var success = _todoService.DeleteTask(id);
            var message = success ? "任務刪除成功！" : "刪除任務失敗，找不到指定的任務。";
            
            _logger.LogInformation("嘗試刪除任務 {Id}: {Result}", id, success ? "成功" : "失敗");
            return new JsonResult(new { success, message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除任務 {Id} 時發生錯誤", id);
            return new JsonResult(new { success = false, message = "刪除任務時發生錯誤，請稍後再試。" });
        }
    }

    /// <summary>
    /// 更新任務排序順序
    /// </summary>
    public IActionResult OnPostUpdateOrder(int id, int order)
    {
        try
        {
            var success = _todoService.UpdateTaskOrder(id, order);
            return new JsonResult(new { success });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任務 {Id} 排序順序時發生錯誤", id);
            return new JsonResult(new { success = false });
        }
    }

    /// <summary>
    /// 取得任務統計資料
    /// </summary>
    public IActionResult OnGetStatistics()
    {
        try
        {
            var statistics = _todoService.GetStatistics();
            return new JsonResult(new { success = true, statistics });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得統計資料時發生錯誤");
            return new JsonResult(new { success = false, message = "取得統計資料時發生錯誤。" });
        }
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 載入頁面所需的所有資料
    /// </summary>
    private void LoadPageData()
    {
        // 載入統計資料
        Statistics = _todoService.GetStatistics();
        
        // 載入分類資料
        Categories = _todoService.GetCategories();

        // 載入今日任務
        TodayTasks = _todoService.GetTodayTasks();

        // 載入明日任務
        var tomorrow = DateTime.Today.AddDays(1);
        var allTasks = _todoService.GetAllTasks();
        TomorrowTasks = allTasks.Where(t => 
            t.DueDate.HasValue && 
            t.DueDate.Value.Date == tomorrow && 
            !t.IsCompleted
        ).OrderBy(t => t.DueDate).ToList();

        // 載入本週任務（排除今日和明日）
        var dayAfterTomorrow = DateTime.Today.AddDays(2);
        var endOfWeek = DateTime.Today.AddDays(7);
        ThisWeekTasks = allTasks.Where(t => 
            t.DueDate.HasValue && 
            t.DueDate.Value.Date >= dayAfterTomorrow && 
            t.DueDate.Value.Date <= endOfWeek &&
            !t.IsCompleted
        ).OrderBy(t => t.DueDate).ToList();

        // 載入未來任務（一週後）
        var nextWeek = DateTime.Today.AddDays(8);
        FutureTasks = allTasks.Where(t => 
            t.DueDate.HasValue && 
            t.DueDate.Value.Date >= nextWeek &&
            !t.IsCompleted
        ).OrderBy(t => t.DueDate).ToList();

        // 載入無到期日任務
        NoDueDateTasks = allTasks.Where(t => 
            !t.DueDate.HasValue && 
            !t.IsCompleted
        ).OrderByDescending(t => t.CreatedDate).ToList();

        // 載入已完成任務（最近30個）
        CompletedTasks = allTasks
            .Where(t => t.IsCompleted)
            .OrderByDescending(t => t.CompletedDate)
            .Take(30)
            .ToList();
    }

    #endregion
}
