# 財務儀表板開發規格書

## 專案概述
開發一個綜合性財務儀表板，整合現有的記帳數據，提供視覺化的財務分析和洞察。此功能將作為財務管理的控制中心，讓使用者能快速了解個人財務狀況。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, Chart.js, jQuery, HTML5, CSS3
- **圖表函式庫**: Chart.js 4.0+ (用於數據視覺化)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 核心功能模組

### 1. 儀表板主頁面
- **前端**: `#file:index9.cshtml`
- **後端**: `#file:index9.cshtml.cs`
- **路由**: `/index9`

### 1.1 功能描述
- **主要顯示**: 財務概覽儀表板，包含多個數據視覺化區塊
- **資料整合**: 從現有的記帳系統 (index7) 讀取數據
- **實時更新**: 顯示最新的財務狀態和趨勢

### 1.2 儀表板區塊設計

#### A. 頂部統計卡片
```html
<!-- 四張統計卡片 -->
<div class="row mb-4">
    <div class="col-lg-3 col-md-6 mb-3">
        <!-- 本月總收入 -->
    </div>
    <div class="col-lg-3 col-md-6 mb-3">
        <!-- 本月總支出 -->
    </div>
    <div class="col-lg-3 col-md-6 mb-3">
        <!-- 淨收支 -->
    </div>
    <div class="col-lg-3 col-md-6 mb-3">
        <!-- 平均每日支出 -->
    </div>
</div>
```

#### B. 圖表區域
```html
<div class="row">
    <div class="col-lg-8 mb-4">
        <!-- 收支趨勢線圖 (最近6個月) -->
        <div class="card">
            <div class="card-header">
                <h5>收支趨勢分析</h5>
            </div>
            <div class="card-body">
                <canvas id="trendChart"></canvas>
            </div>
        </div>
    </div>
    <div class="col-lg-4 mb-4">
        <!-- 支出分類圓餅圖 -->
        <div class="card">
            <div class="card-header">
                <h5>支出分類分布</h5>
            </div>
            <div class="card-body">
                <canvas id="categoryChart"></canvas>
            </div>
        </div>
    </div>
</div>
```

#### C. 最近交易記錄
```html
<div class="row">
    <div class="col-12">
        <div class="card">
            <div class="card-header d-flex justify-content-between">
                <h5>最近交易記錄</h5>
                <a href="/index7" class="btn btn-sm btn-outline-primary">查看全部</a>
            </div>
            <div class="card-body">
                <!-- 最近10筆交易記錄表格 -->
            </div>
        </div>
    </div>
</div>
```

### 1.3 資料分析功能

#### A. 統計計算
```csharp
public class DashboardStats
{
    public decimal CurrentMonthIncome { get; set; }
    public decimal CurrentMonthExpense { get; set; }
    public decimal NetIncome { get; set; }
    public decimal DailyAverageExpense { get; set; }
    public decimal YearToDateIncome { get; set; }
    public decimal YearToDateExpense { get; set; }
    public List<MonthlyTrend> TrendData { get; set; }
    public List<CategorySummary> CategoryData { get; set; }
}

public class MonthlyTrend
{
    public string Month { get; set; }
    public decimal Income { get; set; }
    public decimal Expense { get; set; }
}

public class CategorySummary
{
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public string Color { get; set; }
}
```

#### B. 比較分析
- **同期比較**: 與上月、去年同期的收支比較
- **趨勢分析**: 收支增長率、消費習慣變化
- **預算控制**: 顯示預算使用情況（如果有設定預算）

### 1.4 互動功能

#### A. 時間範圍選擇
```html
<div class="dashboard-controls mb-4">
    <div class="btn-group" role="group">
        <input type="radio" class="btn-check" name="timeRange" id="thisMonth" value="thisMonth" checked>
        <label class="btn btn-outline-primary" for="thisMonth">本月</label>
        
        <input type="radio" class="btn-check" name="timeRange" id="last3Months" value="last3Months">
        <label class="btn btn-outline-primary" for="last3Months">近3個月</label>
        
        <input type="radio" class="btn-check" name="timeRange" id="last6Months" value="last6Months">
        <label class="btn btn-outline-primary" for="last6Months">近6個月</label>
        
        <input type="radio" class="btn-check" name="timeRange" id="thisYear" value="thisYear">
        <label class="btn btn-outline-primary" for="thisYear">今年</label>
    </div>
</div>
```

#### B. 圖表互動
- **點擊圖表**: 可以鑽取到詳細數據
- **懸停提示**: 顯示具體金額和百分比
- **圖表切換**: 可切換不同的圖表類型

### 1.5 響應式設計
```css
/* 手機版適配 */
@media (max-width: 768px) {
    .dashboard-stats .card {
        margin-bottom: 1rem;
    }
    
    .chart-container {
        height: 300px;
    }
    
    .dashboard-controls {
        overflow-x: auto;
    }
}

/* 平板版適配 */
@media (max-width: 992px) {
    .chart-container {
        height: 400px;
    }
}
```

## 2. 服務層設計

### 2.1 財務分析服務
```csharp
public class FinancialDashboardService
{
    public DashboardStats GetDashboardStats(string timeRange = "thisMonth")
    public List<MonthlyTrend> GetTrendData(int months = 6)
    public List<CategorySummary> GetCategoryBreakdown(string timeRange = "thisMonth")
    public List<AccountingRecord> GetRecentTransactions(int count = 10)
    public ComparisonStats GetComparisonStats()
}
```

### 2.2 數據緩存
```csharp
// 簡單的記憶體緩存，避免重複計算
private static readonly Dictionary<string, (DateTime timestamp, object data)> _cache = new();
private static readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);
```

## 3. 前端 JavaScript 功能

### 3.1 圖表初始化
```javascript
// 趨勢圖表
function initTrendChart(data) {
    const ctx = document.getElementById('trendChart').getContext('2d');
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.months,
            datasets: [
                {
                    label: '收入',
                    data: data.income,
                    borderColor: 'rgb(75, 192, 192)',
                    backgroundColor: 'rgba(75, 192, 192, 0.2)',
                },
                {
                    label: '支出',
                    data: data.expense,
                    borderColor: 'rgb(255, 99, 132)',
                    backgroundColor: 'rgba(255, 99, 132, 0.2)',
                }
            ]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        callback: function(value) {
                            return 'NT$ ' + value.toLocaleString();
                        }
                    }
                }
            }
        }
    });
}
```

### 3.2 動態更新
```javascript
// AJAX 更新數據
function updateDashboard(timeRange) {
    $.post('/index9?handler=UpdateData', { timeRange: timeRange })
        .done(function(data) {
            updateStatCards(data.stats);
            updateCharts(data);
            updateRecentTransactions(data.recentTransactions);
        });
}
```

## 4. 整合現有系統

### 4.1 數據源整合
- **記帳數據**: 從 `accounting-records.json` 讀取
- **分類數據**: 從 `accounting-categories.json` 讀取
- **預算數據**: 從 `budget-settings.json` 讀取（如果存在）

### 4.2 導航整合
```html
<!-- 在主選單中新增儀表板連結 -->
<li class="nav-item">
    <a class="nav-link" asp-page="/index9">
        <i class="fas fa-tachometer-alt"></i> 財務儀表板
    </a>
</li>
```

## 5. 效能考量

### 5.1 數據載入優化
- 使用分頁載入大量交易記錄
- 圖表數據按需載入
- 實施簡單的客戶端緩存

### 5.2 響應速度優化
```csharp
// 異步處理大量數據計算
public async Task<DashboardStats> GetDashboardStatsAsync(string timeRange)
{
    return await Task.Run(() => CalculateStats(timeRange));
}
```

## 6. 用戶體驗設計

### 6.1 載入狀態
```html
<!-- 載入動畫 -->
<div class="d-flex justify-content-center" id="loadingIndicator">
    <div class="spinner-border" role="status">
        <span class="visually-hidden">載入中...</span>
    </div>
</div>
```

### 6.2 空狀態處理
```html
<!-- 無數據時的顯示 -->
<div class="empty-state text-center py-5" id="emptyState" style="display: none;">
    <i class="fas fa-chart-line fa-3x text-muted mb-3"></i>
    <h4>還沒有記帳數據</h4>
    <p class="text-muted">開始記帳，建立您的財務分析！</p>
    <a href="/index8" class="btn btn-primary">新增第一筆記錄</a>
</div>
```

## 7. 測試規劃

### 7.1 功能測試
- 統計數據計算正確性
- 圖表顯示準確性
- 時間範圍篩選功能
- 響應式設計測試

### 7.2 效能測試
- 大量數據載入測試
- 圖表渲染效能測試
- 記憶體使用量監控

## 8. 後續擴展規劃

### 8.1 進階功能
- 預算追蹤和警示
- 財務目標設定
- 投資組合整合
- 自動分類建議

### 8.2 數據匯出
- PDF 報告生成
- Excel 數據匯出
- 數據備份功能

這個財務儀表板將作為整個財務管理系統的核心，提供直觀的數據視覺化和深入的財務洞察，幫助用戶更好地管理個人財務。
