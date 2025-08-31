# 習慣追蹤器開發規格書

## 📋 專案概述
開發一個個人成長工具，幫助使用者建立和維持良好習慣。此功能將整合到現有的個人管理系統中，提供習慣建立、每日打卡、進度追蹤、視覺化統計等核心功能，激勵使用者持續成長。

## 🎯 開發目標
- 建立簡潔直觀的習慣管理介面
- 實現每日打卡和連續天數統計
- 提供視覺化進度追蹤
- 建立成就系統和激勵機制
- 支援自訂習慣和目標設定

## 🔧 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, jQuery, Chart.js, HTML5, CSS3
- **視覺化**: Chart.js (進度圖表), FullCalendar (習慣日曆)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 📂 檔案結構規劃

### 新增檔案
```
Pages/
├── habits.cshtml                 # 習慣追蹤主頁面
├── habits.cshtml.cs             # 習慣追蹤後端邏輯
├── Shared/
│   ├── _HabitCard.cshtml        # 習慣卡片部分檢視
│   └── _HabitProgress.cshtml    # 進度視覺化部分檢視

Services/
├── HabitService.cs              # 習慣管理服務類別

Models/
├── HabitModels.cs               # 習慣相關資料模型

App_Data/
├── habits.json                  # 習慣定義資料
├── habit-records.json           # 習慣記錄資料
├── habit-categories.json        # 習慣分類資料

wwwroot/js/
├── habits.js                    # 習慣追蹤前端邏輯

wwwroot/css/
├── habits.css                   # 習慣追蹤樣式
```

## 🎨 核心功能模組

### 1. 習慣追蹤主頁面
- **前端**: `habits.cshtml`
- **後端**: `habits.cshtml.cs`
- **路由**: `/habits`

#### 1.1 頁面佈局設計

##### A. 頂部統計儀表板
```html
<div class="habit-dashboard mb-4">
    <div class="row g-4">
        <div class="col-md-3">
            <div class="card text-center">
                <div class="card-body">
                    <h5 class="card-title">今日完成</h5>
                    <h2 class="text-success">@Model.TodayCompleted/@Model.TotalHabits</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-center">
                <div class="card-body">
                    <h5 class="card-title">本週成功率</h5>
                    <h2 class="text-primary">@Model.WeeklySuccessRate%</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-center">
                <div class="card-body">
                    <h5 class="card-title">最長連續</h5>
                    <h2 class="text-warning">@Model.LongestStreak 天</h2>
                </div>
            </div>
        </div>
        <div class="col-md-3">
            <div class="card text-center">
                <div class="card-body">
                    <h5 class="card-title">總習慣數</h5>
                    <h2 class="text-info">@Model.TotalHabits</h2>
                </div>
            </div>
        </div>
    </div>
</div>
```

##### B. 今日習慣清單
```html
<div class="today-habits mb-4">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h3><i class="fas fa-calendar-day text-primary"></i> 今日習慣</h3>
        <button class="btn btn-success" data-bs-toggle="modal" data-bs-target="#addHabitModal">
            <i class="fas fa-plus"></i> 新增習慣
        </button>
    </div>
    
    <div class="habit-grid">
        @foreach (var habit in Model.TodayHabits)
        {
            <partial name="_HabitCard" model="habit" />
        }
    </div>
</div>
```

##### C. 習慣分類標籤
```html
<div class="habit-categories mb-3">
    <div class="btn-group" role="group">
        <button type="button" class="btn btn-outline-primary active" data-category="all">
            全部 (@Model.TotalHabits)
        </button>
        @foreach (var category in Model.Categories)
        {
            <button type="button" class="btn btn-outline-primary" data-category="@category.Id">
                @category.Name (@category.HabitCount)
            </button>
        }
    </div>
</div>
```

### 2. 習慣卡片元件 (_HabitCard.cshtml)

```html
<div class="habit-card @(Model.IsTodayCompleted ? "completed" : "")" data-habit-id="@Model.Id">
    <div class="habit-header">
        <div class="habit-icon">
            <i class="@Model.IconClass"></i>
        </div>
        <div class="habit-info">
            <h5 class="habit-name">@Model.Name</h5>
            <small class="text-muted">@Model.Category.Name</small>
        </div>
        <div class="habit-actions">
            <button class="btn btn-sm btn-outline-secondary" onclick="editHabit(@Model.Id)">
                <i class="fas fa-edit"></i>
            </button>
            <button class="btn btn-sm btn-outline-danger" onclick="deleteHabit(@Model.Id)">
                <i class="fas fa-trash"></i>
            </button>
        </div>
    </div>
    
    <div class="habit-progress">
        <div class="progress mb-2">
            <div class="progress-bar bg-success" role="progressbar" 
                 style="width: @Model.CompletionRate%" 
                 aria-valuenow="@Model.CompletionRate" 
                 aria-valuemin="0" aria-valuemax="100">
                @Model.CompletionRate%
            </div>
        </div>
        <div class="habit-stats">
            <span class="badge bg-warning">連續 @Model.CurrentStreak 天</span>
            <span class="badge bg-info">共 @Model.TotalCompletions 次</span>
        </div>
    </div>
    
    <div class="habit-actions-bottom">
        @if (Model.IsTodayCompleted)
        {
            <button class="btn btn-success btn-sm w-100" disabled>
                <i class="fas fa-check"></i> 今日已完成
            </button>
        }
        else
        {
            <button class="btn btn-outline-success btn-sm w-100" onclick="markComplete(@Model.Id)">
                <i class="fas fa-check"></i> 標記完成
            </button>
        }
    </div>
</div>
```

### 3. 資料模型定義 (HabitModels.cs)

```csharp
namespace Demo.Models
{
    public class Habit
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fas fa-star";
        public string CategoryId { get; set; } = string.Empty;
        public HabitFrequency Frequency { get; set; } = HabitFrequency.Daily;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? TargetEndDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string Color { get; set; } = "#007bff";
        public int TargetCount { get; set; } = 1; // 每日目標次數
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class HabitRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string HabitId { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; } = 0;
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public class HabitCategory
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconClass { get; set; } = "fas fa-folder";
        public string Color { get; set; } = "#6c757d";
        public int SortOrder { get; set; } = 0;
    }

    public enum HabitFrequency
    {
        Daily,      // 每日
        Weekly,     // 每週
        Monthly,    // 每月
        Custom      // 自訂
    }

    // 檢視模型
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
    }

    public class HabitProgressData
    {
        public DateTime Date { get; set; }
        public int CompletedCount { get; set; }
        public int TotalCount { get; set; }
        public double SuccessRate { get; set; }
    }
}
```

### 4. 服務類別實作 (HabitService.cs)

```csharp
namespace Demo.Services
{
    public class HabitService
    {
        private readonly string _habitsPath;
        private readonly string _recordsPath;
        private readonly string _categoriesPath;

        public HabitService()
        {
            _habitsPath = Path.Combine("App_Data", "habits.json");
            _recordsPath = Path.Combine("App_Data", "habit-records.json");
            _categoriesPath = Path.Combine("App_Data", "habit-categories.json");
        }

        // 習慣管理方法
        public async Task<List<Habit>> GetAllHabitsAsync()
        public async Task<Habit?> GetHabitByIdAsync(string id)
        public async Task<bool> CreateHabitAsync(Habit habit)
        public async Task<bool> UpdateHabitAsync(Habit habit)
        public async Task<bool> DeleteHabitAsync(string id)

        // 習慣記錄方法
        public async Task<bool> MarkHabitCompleteAsync(string habitId, DateTime date, string notes = "")
        public async Task<List<HabitRecord>> GetHabitRecordsAsync(string habitId, DateTime? startDate = null, DateTime? endDate = null)
        public async Task<bool> IsHabitCompletedTodayAsync(string habitId)

        // 統計分析方法
        public async Task<HabitsPageModel> GetHabitsPageModelAsync()
        public async Task<int> GetCurrentStreakAsync(string habitId)
        public async Task<double> GetCompletionRateAsync(string habitId, int days = 30)
        public async Task<List<HabitProgressData>> GetWeeklyProgressAsync()

        // 分類管理方法
        public async Task<List<HabitCategory>> GetAllCategoriesAsync()
        public async Task<bool> CreateCategoryAsync(HabitCategory category)
        public async Task<bool> UpdateCategoryAsync(HabitCategory category)
        public async Task<bool> DeleteCategoryAsync(string id)
    }
}
```

### 5. 前端 JavaScript 功能 (habits.js)

```javascript
// 習慣追蹤器主要功能
class HabitTracker {
    constructor() {
        this.initializeEventListeners();
        this.loadHabitData();
    }

    initializeEventListeners() {
        // 標記完成按鈕
        $(document).on('click', '.mark-complete-btn', this.markComplete.bind(this));
        
        // 新增習慣表單提交
        $('#addHabitForm').on('submit', this.createHabit.bind(this));
        
        // 編輯習慣
        $(document).on('click', '.edit-habit-btn', this.editHabit.bind(this));
        
        // 刪除習慣
        $(document).on('click', '.delete-habit-btn', this.deleteHabit.bind(this));
        
        // 分類篩選
        $('.habit-categories button').on('click', this.filterByCategory.bind(this));
    }

    async markComplete(event) {
        const habitId = $(event.target).closest('.habit-card').data('habit-id');
        try {
            const response = await fetch('/habits/mark-complete', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ habitId: habitId, date: new Date().toISOString() })
            });
            
            if (response.ok) {
                this.showSuccess('習慣已標記為完成！');
                this.refreshHabitCard(habitId);
            }
        } catch (error) {
            this.showError('標記失敗，請重試');
        }
    }

    async createHabit(event) {
        event.preventDefault();
        const formData = new FormData(event.target);
        const habitData = Object.fromEntries(formData);
        
        try {
            const response = await fetch('/habits/create', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(habitData)
            });
            
            if (response.ok) {
                this.showSuccess('習慣已新增成功！');
                $('#addHabitModal').modal('hide');
                this.loadHabitData();
            }
        } catch (error) {
            this.showError('新增失敗，請重試');
        }
    }

    // 進度視覺化
    renderProgressChart() {
        const ctx = document.getElementById('habitProgressChart').getContext('2d');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: this.progressData.map(d => d.date),
                datasets: [{
                    label: '完成率',
                    data: this.progressData.map(d => d.successRate),
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                    tension: 0.1
                }]
            },
            options: {
                responsive: true,
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 100
                    }
                }
            }
        });
    }
}

// 初始化
$(document).ready(function() {
    window.habitTracker = new HabitTracker();
});
```

### 6. 樣式設計 (habits.css)

```css
/* 習慣追蹤器樣式 */
.habit-dashboard .card {
    border: none;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
    transition: transform 0.2s ease;
}

.habit-dashboard .card:hover {
    transform: translateY(-2px);
}

.habit-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
    gap: 1.5rem;
}

.habit-card {
    border: 1px solid #dee2e6;
    border-radius: 8px;
    padding: 1rem;
    background: white;
    transition: all 0.3s ease;
}

.habit-card:hover {
    box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    transform: translateY(-2px);
}

.habit-card.completed {
    background: linear-gradient(45deg, #d4edda, #ffffff);
    border-color: #28a745;
}

.habit-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    margin-bottom: 1rem;
}

.habit-icon {
    width: 40px;
    height: 40px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    background: #007bff;
    color: white;
    font-size: 1.2rem;
}

.habit-progress .progress {
    height: 8px;
    border-radius: 4px;
}

.habit-stats .badge {
    margin-right: 0.5rem;
}

.habit-categories {
    overflow-x: auto;
    white-space: nowrap;
}

.habit-categories .btn-group {
    display: flex;
    flex-wrap: nowrap;
}

/* 響應式設計 */
@media (max-width: 768px) {
    .habit-grid {
        grid-template-columns: 1fr;
    }
    
    .habit-dashboard .row {
        margin: 0;
    }
    
    .habit-dashboard .col-md-3 {
        margin-bottom: 1rem;
    }
}

/* 動畫效果 */
@keyframes completeAnimation {
    0% { transform: scale(1); }
    50% { transform: scale(1.05); }
    100% { transform: scale(1); }
}

.habit-card.completing {
    animation: completeAnimation 0.3s ease;
}

/* 深色模式支援 */
[data-bs-theme="dark"] .habit-card {
    background: #2d3748;
    border-color: #4a5568;
}

[data-bs-theme="dark"] .habit-card.completed {
    background: linear-gradient(45deg, #2d5a3d, #2d3748);
}
```

## 🎯 開發階段規劃

### Phase 1: 基礎功能 (1-2 週)
- [x] 建立資料模型和服務類別
- [x] 實作基本的習慣 CRUD 功能
- [x] 建立主頁面和基本 UI
- [x] 實作每日打卡功能

### Phase 2: 進階功能 (2-3 週)
- [ ] 實作統計分析和視覺化
- [ ] 加入成就系統
- [ ] 實作習慣分類管理
- [ ] 加入進度圖表顯示

### Phase 3: 優化增強 (1-2 週)
- [ ] 響應式設計優化
- [ ] 深色模式支援
- [ ] 動畫效果增強
- [ ] 效能優化和錯誤處理

## 📝 測試規格

### 功能測試
1. 習慣建立、編輯、刪除
2. 每日打卡功能
3. 統計數據準確性
4. 分類篩選功能
5. 響應式佈局

### 使用者體驗測試
1. 介面直觀性
2. 操作流暢度
3. 視覺回饋效果
4. 行動裝置適配性

## 🚀 部署注意事項

1. 確保 App_Data 目錄有寫入權限
2. 初始化預設習慣分類資料
3. 檢查前端資源檔案路徑
4. 驗證 JSON 檔案格式正確性

## 📚 相關文件
- [Chart.js 官方文件](https://www.chartjs.org/docs/latest/)
- [Bootstrap 5 元件文件](https://getbootstrap.com/docs/5.0/components/)
- [ASP.NET Core Razor Pages 指南](https://docs.microsoft.com/en-us/aspnet/core/razor-pages/)
