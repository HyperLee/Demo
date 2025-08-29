# 待辦清單開發規格書

## 專案概述
開發一個簡潔實用的待辦清單管理系統，讓使用者能夠有效管理日常任務。此功能將整合到現有的個人管理系統中，提供任務建立、分類、優先級設定、完成追蹤等核心功能。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, jQuery, HTML5, CSS3
- **特效函式庫**: SortableJS (拖拽排序)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 核心功能模組

### 1. 待辦清單主頁面
- **前端**: `#file:todo.cshtml` (將建立)
- **後端**: `#file:todo.cshtml.cs` (將建立)
- **路由**: `/todo`

### 1.1 功能描述
- **主要顯示**: 以清單形式顯示所有待辦事項
- **分類管理**: 支援自訂分類和標籤
- **優先級設定**: 高、中、低三個優先級
- **狀態追蹤**: 待處理、進行中、已完成
- **快速操作**: 一鍵完成、編輯、刪除

### 1.2 頁面佈局設計

#### A. 頂部工具列
```html
<div class="todo-header mb-4">
    <div class="d-flex justify-content-between align-items-center">
        <h1 class="h2 mb-0">
            <i class="fas fa-tasks text-primary"></i>
            我的待辦清單
        </h1>
        <div class="todo-actions">
            <button class="btn btn-primary" data-bs-toggle="modal" data-bs-target="#addTodoModal">
                <i class="fas fa-plus"></i> 新增任務
            </button>
            <div class="btn-group ms-2">
                <button class="btn btn-outline-secondary dropdown-toggle" data-bs-toggle="dropdown">
                    <i class="fas fa-filter"></i> 篩選
                </button>
                <ul class="dropdown-menu">
                    <li><a class="dropdown-item" href="#" data-filter="all">全部任務</a></li>
                    <li><a class="dropdown-item" href="#" data-filter="pending">待處理</a></li>
                    <li><a class="dropdown-item" href="#" data-filter="in-progress">進行中</a></li>
                    <li><a class="dropdown-item" href="#" data-filter="completed">已完成</a></li>
                    <li><hr class="dropdown-divider"></li>
                    <li><a class="dropdown-item" href="#" data-filter="high">高優先級</a></li>
                    <li><a class="dropdown-item" href="#" data-filter="today">今日到期</a></li>
                </ul>
            </div>
        </div>
    </div>
    
    <!-- 統計摘要 -->
    <div class="todo-stats mt-3">
        <div class="row">
            <div class="col-md-3">
                <div class="stat-card text-center">
                    <div class="stat-number text-warning">@Model.PendingCount</div>
                    <div class="stat-label">待處理</div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-card text-center">
                    <div class="stat-number text-info">@Model.InProgressCount</div>
                    <div class="stat-label">進行中</div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-card text-center">
                    <div class="stat-number text-success">@Model.CompletedCount</div>
                    <div class="stat-label">已完成</div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="stat-card text-center">
                    <div class="stat-number text-danger">@Model.OverdueCount</div>
                    <div class="stat-label">已逾期</div>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### B. 任務清單區域
```html
<div class="todo-list">
    <!-- 分組顯示 -->
    <div class="todo-section" data-section="today">
        <h5 class="section-title">
            <i class="fas fa-calendar-day"></i> 今日任務
            <span class="badge bg-warning ms-2">@Model.TodayTasks.Count</span>
        </h5>
        <div class="todo-items" id="today-tasks">
            @foreach (var task in Model.TodayTasks)
            {
                <div class="todo-item" data-id="@task.Id" data-priority="@task.Priority.ToLower()">
                    <div class="todo-content">
                        <div class="todo-check">
                            <input type="checkbox" class="form-check-input" @(task.IsCompleted ? "checked" : "") 
                                   onchange="toggleTask(@task.Id)">
                        </div>
                        <div class="todo-details">
                            <div class="todo-title @(task.IsCompleted ? "completed" : "")">@task.Title</div>
                            <div class="todo-meta">
                                @if (task.DueDate.HasValue)
                                {
                                    <span class="due-date @(task.IsOverdue ? "overdue" : "")">
                                        <i class="fas fa-clock"></i> @task.DueDate.Value.ToString("MM/dd HH:mm")
                                    </span>
                                }
                                @if (!string.IsNullOrEmpty(task.Category))
                                {
                                    <span class="category-tag">@task.Category</span>
                                }
                                <span class="priority-badge priority-@task.Priority.ToLower()">@task.Priority</span>
                            </div>
                        </div>
                        <div class="todo-actions">
                            <button class="btn btn-sm btn-outline-secondary" onclick="editTask(@task.Id)">
                                <i class="fas fa-edit"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="deleteTask(@task.Id)">
                                <i class="fas fa-trash"></i>
                            </button>
                        </div>
                    </div>
                </div>
            }
        </div>
    </div>
    
    <!-- 其他分組：明日、本週、未來、已完成 -->
    <!-- ... 類似結構 ... -->
</div>
```

### 1.3 資料模型設計

#### A. 任務模型
```csharp
public class TodoTask
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public TodoStatus Status { get; set; } = TodoStatus.Pending;
    public TodoPriority Priority { get; set; } = TodoPriority.Medium;
    public string Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public bool IsCompleted => Status == TodoStatus.Completed;
    public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && !IsCompleted;
    public int EstimatedMinutes { get; set; } // 預估完成時間
    public int ActualMinutes { get; set; } // 實際花費時間
}

public enum TodoStatus
{
    Pending,     // 待處理
    InProgress,  // 進行中
    Completed,   // 已完成
    Cancelled    // 已取消
}

public enum TodoPriority
{
    Low,    // 低優先級
    Medium, // 中優先級
    High    // 高優先級
}
```

#### B. 分類模型
```csharp
public class TodoCategory
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Color { get; set; }
    public string Icon { get; set; }
    public int SortOrder { get; set; }
}
```

### 1.4 新增/編輯任務模態框
```html
<!-- 任務編輯模態框 -->
<div class="modal fade" id="addTodoModal" tabindex="-1">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">
                    <i class="fas fa-plus-circle"></i> 
                    <span id="modal-title">新增任務</span>
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <form id="todoForm">
                    <input type="hidden" id="taskId" name="Id" value="0">
                    
                    <!-- 任務標題 -->
                    <div class="mb-3">
                        <label for="taskTitle" class="form-label">任務標題 <span class="text-danger">*</span></label>
                        <input type="text" class="form-control" id="taskTitle" name="Title" 
                               placeholder="輸入任務標題..." maxlength="100" required>
                        <div class="form-text">
                            <span id="titleCount">0</span>/100 字元
                        </div>
                    </div>
                    
                    <!-- 任務描述 -->
                    <div class="mb-3">
                        <label for="taskDescription" class="form-label">詳細描述</label>
                        <textarea class="form-control" id="taskDescription" name="Description" 
                                  rows="3" placeholder="輸入任務詳細描述..."></textarea>
                    </div>
                    
                    <div class="row">
                        <!-- 優先級 -->
                        <div class="col-md-6 mb-3">
                            <label for="taskPriority" class="form-label">優先級</label>
                            <select class="form-select" id="taskPriority" name="Priority">
                                <option value="Low">低優先級</option>
                                <option value="Medium" selected>中優先級</option>
                                <option value="High">高優先級</option>
                            </select>
                        </div>
                        
                        <!-- 分類 -->
                        <div class="col-md-6 mb-3">
                            <label for="taskCategory" class="form-label">分類</label>
                            <select class="form-select" id="taskCategory" name="Category">
                                <option value="">選擇分類...</option>
                                <option value="工作">工作</option>
                                <option value="個人">個人</option>
                                <option value="學習">學習</option>
                                <option value="健康">健康</option>
                                <option value="購物">購物</option>
                            </select>
                        </div>
                    </div>
                    
                    <div class="row">
                        <!-- 到期日期 -->
                        <div class="col-md-6 mb-3">
                            <label for="taskDueDate" class="form-label">到期日期</label>
                            <input type="datetime-local" class="form-control" id="taskDueDate" name="DueDate">
                        </div>
                        
                        <!-- 預估時間 -->
                        <div class="col-md-6 mb-3">
                            <label for="taskEstimated" class="form-label">預估時間（分鐘）</label>
                            <input type="number" class="form-control" id="taskEstimated" name="EstimatedMinutes" 
                                   min="5" max="480" step="5" placeholder="30">
                        </div>
                    </div>
                    
                    <!-- 標籤 -->
                    <div class="mb-3">
                        <label for="taskTags" class="form-label">標籤</label>
                        <input type="text" class="form-control" id="taskTags" name="Tags" 
                               placeholder="輸入標籤，用逗號分隔...">
                        <div class="form-text">例如：重要, 緊急, 專案A</div>
                    </div>
                </form>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">取消</button>
                <button type="button" class="btn btn-primary" onclick="saveTodoTask()">
                    <i class="fas fa-save"></i> 儲存
                </button>
            </div>
        </div>
    </div>
</div>
```

## 2. 服務層設計

### 2.1 待辦清單服務
```csharp
public class TodoService
{
    private readonly string _dataPath = Path.Combine("App_Data", "todo-tasks.json");
    private readonly string _categoriesPath = Path.Combine("App_Data", "todo-categories.json");
    
    public List<TodoTask> GetAllTasks()
    public List<TodoTask> GetTasksByStatus(TodoStatus status)
    public List<TodoTask> GetTasksByPriority(TodoPriority priority)
    public List<TodoTask> GetTasksByCategory(string category)
    public List<TodoTask> GetTodayTasks()
    public List<TodoTask> GetUpcomingTasks(int days = 7)
    public List<TodoTask> GetOverdueTasks()
    
    public TodoTask GetTaskById(int id)
    public void CreateTask(TodoTask task)
    public void UpdateTask(TodoTask task)
    public void DeleteTask(int id)
    public void ToggleTaskComplete(int id)
    
    public TodoStatistics GetStatistics()
    public List<TodoCategory> GetCategories()
    public void SaveCategories(List<TodoCategory> categories)
}
```

### 2.2 統計資料類別
```csharp
public class TodoStatistics
{
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int CompletedCount { get; set; }
    public int OverdueCount { get; set; }
    public int TodayCount { get; set; }
    public int ThisWeekCount { get; set; }
    public double CompletionRate { get; set; }
    public double AverageCompletionTime { get; set; } // 平均完成時間（分鐘）
}
```

## 3. 前端 JavaScript 功能

### 3.1 任務操作
```javascript
// 切換任務完成狀態
function toggleTask(taskId) {
    $.post('/todo?handler=ToggleComplete', { id: taskId })
        .done(function(result) {
            if (result.success) {
                updateTaskStatus(taskId, result.isCompleted);
                updateStatistics();
            }
        });
}

// 編輯任務
function editTask(taskId) {
    $.get('/todo?handler=GetTask', { id: taskId })
        .done(function(task) {
            populateForm(task);
            $('#addTodoModal').modal('show');
        });
}

// 刪除任務
function deleteTask(taskId) {
    if (confirm('確定要刪除這個任務嗎？')) {
        $.post('/todo?handler=Delete', { id: taskId })
            .done(function(result) {
                if (result.success) {
                    $(`[data-id="${taskId}"]`).fadeOut(300, function() {
                        $(this).remove();
                        updateStatistics();
                    });
                }
            });
    }
}

// 儲存任務
function saveTodoTask() {
    const form = document.getElementById('todoForm');
    const formData = new FormData(form);
    
    $.post('/todo?handler=Save', formData)
        .done(function(result) {
            if (result.success) {
                $('#addTodoModal').modal('hide');
                location.reload(); // 重新載入頁面
            } else {
                showError(result.message);
            }
        });
}
```

### 3.2 篩選和搜尋
```javascript
// 篩選任務
function filterTasks(filterType) {
    $('.todo-item').each(function() {
        const $item = $(this);
        let show = true;
        
        switch(filterType) {
            case 'pending':
                show = !$item.find('.form-check-input').is(':checked');
                break;
            case 'completed':
                show = $item.find('.form-check-input').is(':checked');
                break;
            case 'high':
                show = $item.attr('data-priority') === 'high';
                break;
            case 'today':
                show = $item.closest('[data-section="today"]').length > 0;
                break;
            default:
                show = true;
        }
        
        $item.toggle(show);
    });
}

// 即時搜尋
let searchTimeout;
function setupSearch() {
    $('#searchInput').on('input', function() {
        clearTimeout(searchTimeout);
        const query = $(this).val().toLowerCase();
        
        searchTimeout = setTimeout(function() {
            $('.todo-item').each(function() {
                const $item = $(this);
                const title = $item.find('.todo-title').text().toLowerCase();
                const description = $item.find('.todo-description').text().toLowerCase();
                const show = title.includes(query) || description.includes(query);
                $item.toggle(show);
            });
        }, 300);
    });
}
```

### 3.3 拖拽排序
```javascript
// 使用 SortableJS 實現拖拽排序
function initSortable() {
    $('.todo-items').each(function() {
        new Sortable(this, {
            animation: 150,
            ghostClass: 'sortable-ghost',
            onEnd: function(evt) {
                const taskId = evt.item.getAttribute('data-id');
                const newIndex = evt.newIndex;
                updateTaskOrder(taskId, newIndex);
            }
        });
    });
}

function updateTaskOrder(taskId, newOrder) {
    $.post('/todo?handler=UpdateOrder', { 
        id: taskId, 
        order: newOrder 
    });
}
```

## 4. CSS 樣式設計

### 4.1 任務項目樣式
```css
.todo-item {
    background: white;
    border: 1px solid #e9ecef;
    border-radius: 8px;
    margin-bottom: 12px;
    padding: 16px;
    transition: all 0.3s ease;
    box-shadow: 0 2px 4px rgba(0,0,0,0.05);
}

.todo-item:hover {
    box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    transform: translateY(-2px);
}

.todo-content {
    display: flex;
    align-items: flex-start;
    gap: 12px;
}

.todo-title {
    font-weight: 500;
    color: #333;
    margin-bottom: 4px;
    line-height: 1.4;
}

.todo-title.completed {
    text-decoration: line-through;
    color: #6c757d;
    opacity: 0.7;
}

.todo-meta {
    display: flex;
    gap: 8px;
    align-items: center;
    font-size: 0.875rem;
    color: #666;
}

.priority-badge {
    padding: 2px 8px;
    border-radius: 12px;
    font-size: 0.75rem;
    font-weight: 500;
}

.priority-high {
    background-color: #ffe6e6;
    color: #dc3545;
}

.priority-medium {
    background-color: #fff3cd;
    color: #856404;
}

.priority-low {
    background-color: #e6f3ff;
    color: #0066cc;
}

.due-date.overdue {
    color: #dc3545;
    font-weight: 500;
}

.category-tag {
    background-color: #f8f9fa;
    padding: 2px 6px;
    border-radius: 4px;
    font-size: 0.75rem;
    border: 1px solid #dee2e6;
}
```

### 4.2 統計卡片樣式
```css
.todo-stats {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    border-radius: 12px;
    padding: 20px;
    color: white;
}

.stat-card {
    background: rgba(255,255,255,0.1);
    border-radius: 8px;
    padding: 15px;
    backdrop-filter: blur(10px);
}

.stat-number {
    font-size: 2rem;
    font-weight: bold;
    line-height: 1;
}

.stat-label {
    font-size: 0.875rem;
    opacity: 0.9;
    margin-top: 4px;
}
```

## 5. 響應式設計

### 5.1 手機版適配
```css
@media (max-width: 768px) {
    .todo-content {
        flex-direction: column;
        gap: 8px;
    }
    
    .todo-actions {
        align-self: flex-end;
    }
    
    .todo-stats .row > div {
        margin-bottom: 12px;
    }
    
    .stat-number {
        font-size: 1.5rem;
    }
}
```

## 6. 資料持久化

### 6.1 JSON 檔案結構
```json
// todo-tasks.json
{
  "tasks": [
    {
      "id": 1,
      "title": "完成專案報告",
      "description": "撰寫Q3季度專案總結報告",
      "status": "InProgress",
      "priority": "High",
      "category": "工作",
      "tags": ["重要", "專案"],
      "createdDate": "2024-01-15T10:00:00",
      "dueDate": "2024-01-20T18:00:00",
      "estimatedMinutes": 120,
      "actualMinutes": 0
    }
  ],
  "nextId": 2
}
```

## 7. 後續擴展功能

### 7.1 進階功能
- **子任務支援**: 將大任務分解為小任務
- **循環任務**: 每日、每週、每月重複任務
- **任務模板**: 常用任務模板快速建立
- **時間追蹤**: 番茄鐘整合
- **協作功能**: 任務分享和指派
- **提醒通知**: 到期提醒、瀏覽器通知

### 7.2 整合功能
- **行事曆整合**: 與現有的月曆系統整合
- **記帳整合**: 與花費相關的任務記錄支出
- **備忘錄整合**: 任務與備忘錄互相轉換
- **統計報告**: 生產力分析報告

這個待辦清單系統將提供簡潔高效的任務管理體驗，幫助用戶更好地組織和完成日常工作。
