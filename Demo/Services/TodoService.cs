using System.Text.Json;
using Demo.Models;

namespace Demo.Services;

/// <summary>
/// 待辦清單服務類別
/// 提供任務管理的核心業務邏輯，包含CRUD操作、統計分析等功能
/// </summary>
public class TodoService
{
    private readonly string _dataPath;
    private readonly string _categoriesPath;
    private readonly ILogger<TodoService> _logger;

    /// <summary>
    /// TodoService 建構函式
    /// </summary>
    /// <param name="logger">日誌記錄器</param>
    public TodoService(ILogger<TodoService> logger)
    {
        _dataPath = Path.Combine("App_Data", "todo-tasks.json");
        _categoriesPath = Path.Combine("App_Data", "todo-categories.json");
        _logger = logger;

        // 確保資料目錄存在
        EnsureDataDirectoryExists();
    }

    #region 任務查詢方法

    /// <summary>
    /// 取得所有任務
    /// </summary>
    /// <returns>所有任務的清單，按建立時間降序排列</returns>
    public List<TodoTask> GetAllTasks()
    {
        try
        {
            var data = LoadTasksData();
            return data.Tasks.OrderByDescending(t => t.CreatedDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得所有任務時發生錯誤");
            return [];
        }
    }

    /// <summary>
    /// 根據狀態取得任務
    /// </summary>
    /// <param name="status">任務狀態</param>
    /// <returns>符合狀態的任務清單</returns>
    public List<TodoTask> GetTasksByStatus(TodoStatus status)
    {
        try
        {
            var tasks = GetAllTasks();
            return tasks.Where(t => t.Status == status).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據狀態 {Status} 取得任務時發生錯誤", status);
            return [];
        }
    }

    /// <summary>
    /// 根據優先級取得任務
    /// </summary>
    /// <param name="priority">任務優先級</param>
    /// <returns>符合優先級的任務清單</returns>
    public List<TodoTask> GetTasksByPriority(TodoPriority priority)
    {
        try
        {
            var tasks = GetAllTasks();
            return tasks.Where(t => t.Priority == priority).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據優先級 {Priority} 取得任務時發生錯誤", priority);
            return [];
        }
    }

    /// <summary>
    /// 根據分類取得任務
    /// </summary>
    /// <param name="category">任務分類</param>
    /// <returns>符合分類的任務清單</returns>
    public List<TodoTask> GetTasksByCategory(string category)
    {
        try
        {
            var tasks = GetAllTasks();
            return tasks.Where(t => string.Equals(t.Category, category, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "根據分類 {Category} 取得任務時發生錯誤", category);
            return [];
        }
    }

    /// <summary>
    /// 取得今日任務（今天到期或已逾期）
    /// </summary>
    /// <returns>今日相關的任務清單</returns>
    public List<TodoTask> GetTodayTasks()
    {
        try
        {
            var tasks = GetAllTasks();
            var today = DateTime.Today;
            return tasks.Where(t => 
                t.DueDate.HasValue && 
                (t.DueDate.Value.Date == today || t.IsOverdue) &&
                !t.IsCompleted
            ).OrderBy(t => t.DueDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得今日任務時發生錯誤");
            return [];
        }
    }

    /// <summary>
    /// 取得即將到來的任務
    /// </summary>
    /// <param name="days">未來天數，預設為7天</param>
    /// <returns>即將到來的任務清單</returns>
    public List<TodoTask> GetUpcomingTasks(int days = 7)
    {
        try
        {
            var tasks = GetAllTasks();
            var startDate = DateTime.Today.AddDays(1);
            var endDate = DateTime.Today.AddDays(days);

            return tasks.Where(t => 
                t.DueDate.HasValue && 
                t.DueDate.Value.Date >= startDate && 
                t.DueDate.Value.Date <= endDate &&
                !t.IsCompleted
            ).OrderBy(t => t.DueDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得未來 {Days} 天任務時發生錯誤", days);
            return [];
        }
    }

    /// <summary>
    /// 取得已逾期的任務
    /// </summary>
    /// <returns>已逾期的任務清單</returns>
    public List<TodoTask> GetOverdueTasks()
    {
        try
        {
            var tasks = GetAllTasks();
            return tasks.Where(t => t.IsOverdue).OrderBy(t => t.DueDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得已逾期任務時發生錯誤");
            return [];
        }
    }

    /// <summary>
    /// 根據ID取得特定任務
    /// </summary>
    /// <param name="id">任務ID</param>
    /// <returns>任務物件，如果找不到則返回null</returns>
    public TodoTask? GetTaskById(int id)
    {
        try
        {
            var tasks = GetAllTasks();
            return tasks.FirstOrDefault(t => t.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得任務 {Id} 時發生錯誤", id);
            return null;
        }
    }

    #endregion

    #region 任務操作方法

    /// <summary>
    /// 建立新任務
    /// </summary>
    /// <param name="task">要建立的任務物件</param>
    /// <returns>建立成功返回true，失敗返回false</returns>
    public bool CreateTask(TodoTask task)
    {
        try
        {
            var data = LoadTasksData();
            
            // 設定任務ID和建立時間
            task.Id = data.NextId++;
            task.CreatedDate = DateTime.Now;
            task.SortOrder = data.Tasks.Count;

            // 新增任務到清單
            data.Tasks.Add(task);
            
            // 儲存資料
            SaveTasksData(data);
            
            _logger.LogInformation("成功建立任務：{Title} (ID: {Id})", task.Title, task.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立任務 {Title} 時發生錯誤", task.Title);
            return false;
        }
    }

    /// <summary>
    /// 更新現有任務
    /// </summary>
    /// <param name="task">要更新的任務物件</param>
    /// <returns>更新成功返回true，失敗返回false</returns>
    public bool UpdateTask(TodoTask task)
    {
        try
        {
            var data = LoadTasksData();
            var existingTaskIndex = data.Tasks.FindIndex(t => t.Id == task.Id);
            
            if (existingTaskIndex == -1)
            {
                _logger.LogWarning("嘗試更新不存在的任務 ID: {Id}", task.Id);
                return false;
            }

            // 保留原始建立時間
            var originalCreatedDate = data.Tasks[existingTaskIndex].CreatedDate;
            task.CreatedDate = originalCreatedDate;

            // 如果任務狀態變為已完成，設定完成時間
            if (task.Status == TodoStatus.Completed && data.Tasks[existingTaskIndex].Status != TodoStatus.Completed)
            {
                task.CompletedDate = DateTime.Now;
            }
            else if (task.Status != TodoStatus.Completed)
            {
                task.CompletedDate = null;
            }

            // 更新任務
            data.Tasks[existingTaskIndex] = task;
            
            // 儲存資料
            SaveTasksData(data);
            
            _logger.LogInformation("成功更新任務：{Title} (ID: {Id})", task.Title, task.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任務 {Id} 時發生錯誤", task.Id);
            return false;
        }
    }

    /// <summary>
    /// 刪除任務
    /// </summary>
    /// <param name="id">要刪除的任務ID</param>
    /// <returns>刪除成功返回true，失敗返回false</returns>
    public bool DeleteTask(int id)
    {
        try
        {
            var data = LoadTasksData();
            var taskToDelete = data.Tasks.FirstOrDefault(t => t.Id == id);
            
            if (taskToDelete is null)
            {
                _logger.LogWarning("嘗試刪除不存在的任務 ID: {Id}", id);
                return false;
            }

            // 移除任務
            data.Tasks.Remove(taskToDelete);
            
            // 儲存資料
            SaveTasksData(data);
            
            _logger.LogInformation("成功刪除任務：{Title} (ID: {Id})", taskToDelete.Title, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除任務 {Id} 時發生錯誤", id);
            return false;
        }
    }

    /// <summary>
    /// 切換任務完成狀態
    /// </summary>
    /// <param name="id">任務ID</param>
    /// <returns>切換後的完成狀態，如果失敗返回null</returns>
    public bool? ToggleTaskComplete(int id)
    {
        try
        {
            var task = GetTaskById(id);
            if (task is null)
            {
                _logger.LogWarning("嘗試切換不存在任務的完成狀態 ID: {Id}", id);
                return null;
            }

            // 切換完成狀態
            task.Status = task.Status == TodoStatus.Completed ? TodoStatus.Pending : TodoStatus.Completed;
            
            // 更新任務
            var success = UpdateTask(task);
            return success ? task.IsCompleted : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切換任務 {Id} 完成狀態時發生錯誤", id);
            return null;
        }
    }

    /// <summary>
    /// 更新任務排序順序
    /// </summary>
    /// <param name="id">任務ID</param>
    /// <param name="newOrder">新的排序順序</param>
    /// <returns>更新成功返回true，失敗返回false</returns>
    public bool UpdateTaskOrder(int id, int newOrder)
    {
        try
        {
            var data = LoadTasksData();
            var task = data.Tasks.FirstOrDefault(t => t.Id == id);
            
            if (task is null)
            {
                _logger.LogWarning("嘗試更新不存在任務的排序順序 ID: {Id}", id);
                return false;
            }

            task.SortOrder = newOrder;
            SaveTasksData(data);
            
            _logger.LogInformation("成功更新任務 {Id} 的排序順序為 {Order}", id, newOrder);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新任務 {Id} 排序順序時發生錯誤", id);
            return false;
        }
    }

    #endregion

    #region 統計方法

    /// <summary>
    /// 取得任務統計資訊
    /// </summary>
    /// <returns>統計資訊物件</returns>
    public TodoStatistics GetStatistics()
    {
        try
        {
            var tasks = GetAllTasks();
            var completedTasks = tasks.Where(t => t.IsCompleted).ToList();
            
            var statistics = new TodoStatistics
            {
                TotalTasks = tasks.Count,
                PendingCount = tasks.Count(t => t.Status == TodoStatus.Pending),
                InProgressCount = tasks.Count(t => t.Status == TodoStatus.InProgress),
                CompletedCount = completedTasks.Count,
                OverdueCount = tasks.Count(t => t.IsOverdue),
                TodayCount = GetTodayTasks().Count,
                ThisWeekCount = GetUpcomingTasks(7).Count
            };

            // 計算完成率
            statistics.CompletionRate = tasks.Count > 0 
                ? Math.Round((double)statistics.CompletedCount / statistics.TotalTasks * 100, 1)
                : 0;

            // 計算平均完成時間
            var tasksWithActualTime = completedTasks.Where(t => t.ActualMinutes > 0).ToList();
            statistics.AverageCompletionTime = tasksWithActualTime.Any()
                ? Math.Round(tasksWithActualTime.Average(t => t.ActualMinutes), 1)
                : 0;

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得統計資訊時發生錯誤");
            return new TodoStatistics();
        }
    }

    #endregion

    #region 分類管理方法

    /// <summary>
    /// 取得所有分類
    /// </summary>
    /// <returns>分類清單</returns>
    public List<TodoCategory> GetCategories()
    {
        try
        {
            var data = LoadCategoriesData();
            return data.Categories.OrderBy(c => c.SortOrder).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分類清單時發生錯誤");
            return GetDefaultCategories();
        }
    }

    /// <summary>
    /// 儲存分類清單
    /// </summary>
    /// <param name="categories">要儲存的分類清單</param>
    /// <returns>儲存成功返回true，失敗返回false</returns>
    public bool SaveCategories(List<TodoCategory> categories)
    {
        try
        {
            var data = new TodoCategoryData
            {
                Categories = categories,
                NextId = categories.Any() ? categories.Max(c => c.Id) + 1 : 1
            };

            SaveCategoriesData(data);
            _logger.LogInformation("成功儲存 {Count} 個分類", categories.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存分類時發生錯誤");
            return false;
        }
    }

    #endregion

    #region 私有輔助方法

    /// <summary>
    /// 確保資料目錄存在
    /// </summary>
    private void EnsureDataDirectoryExists()
    {
        var dataDir = Path.GetDirectoryName(_dataPath);
        if (!string.IsNullOrEmpty(dataDir) && !Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
            _logger.LogInformation("建立資料目錄: {Directory}", dataDir);
        }
    }

    /// <summary>
    /// 載入任務資料
    /// </summary>
    /// <returns>任務資料物件</returns>
    private TodoData LoadTasksData()
    {
        if (!File.Exists(_dataPath))
        {
            var defaultData = new TodoData();
            SaveTasksData(defaultData);
            return defaultData;
        }

        try
        {
            var json = File.ReadAllText(_dataPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            return JsonSerializer.Deserialize<TodoData>(json, options) ?? new TodoData();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入任務資料時發生錯誤");
            return new TodoData();
        }
    }

    /// <summary>
    /// 儲存任務資料
    /// </summary>
    /// <param name="data">要儲存的任務資料</param>
    private void SaveTasksData(TodoData data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(_dataPath, json);
    }

    /// <summary>
    /// 載入分類資料
    /// </summary>
    /// <returns>分類資料物件</returns>
    private TodoCategoryData LoadCategoriesData()
    {
        if (!File.Exists(_categoriesPath))
        {
            _logger.LogInformation("分類檔案不存在，建立預設分類: {Path}", _categoriesPath);
            var defaultData = new TodoCategoryData
            {
                Categories = GetDefaultCategories()
            };
            defaultData.NextId = defaultData.Categories.Max(c => c.Id) + 1;
            SaveCategoriesData(defaultData);
            return defaultData;
        }

        try
        {
            var json = File.ReadAllText(_categoriesPath);
            _logger.LogInformation("載入分類檔案: {Path}, 內容長度: {Length}", _categoriesPath, json.Length);
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            
            var result = JsonSerializer.Deserialize<TodoCategoryData>(json, options) ?? new TodoCategoryData();
            _logger.LogInformation("成功載入 {Count} 個分類", result.Categories.Count);
            
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入分類資料時發生錯誤，使用預設分類");
            return new TodoCategoryData { Categories = GetDefaultCategories() };
        }
    }

    /// <summary>
    /// 儲存分類資料
    /// </summary>
    /// <param name="data">要儲存的分類資料</param>
    private void SaveCategoriesData(TodoCategoryData data)
    {
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
        File.WriteAllText(_categoriesPath, json);
    }

    /// <summary>
    /// 取得預設分類清單
    /// </summary>
    /// <returns>預設分類清單</returns>
    private static List<TodoCategory> GetDefaultCategories() =>
    [
        new() { Id = 1, Name = "工作", Color = "#007bff", Icon = "fas fa-briefcase", SortOrder = 1 },
        new() { Id = 2, Name = "個人", Color = "#28a745", Icon = "fas fa-user", SortOrder = 2 },
        new() { Id = 3, Name = "學習", Color = "#17a2b8", Icon = "fas fa-graduation-cap", SortOrder = 3 },
        new() { Id = 4, Name = "健康", Color = "#dc3545", Icon = "fas fa-heart", SortOrder = 4 },
        new() { Id = 5, Name = "購物", Color = "#ffc107", Icon = "fas fa-shopping-cart", SortOrder = 5 }
    ];

    #endregion
}
