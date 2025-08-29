# Phase 1 開發規格書 - 統計分析基礎功能 (MVP)

## 📋 開發概述
實現統計分析功能的最小可行產品 (MVP)，提供基本的收支統計和分類分析功能。

## 🎯 開發目標
- 建立統計分析對話框的基本架構
- 實現月度收支趨勢分析
- 實現支出分類圓餅圖
- 提供基本的時間範圍選擇功能

## 📂 檔案結構規劃

### 新增檔案
```
Services/
├── StatisticsService.cs          # 統計分析服務類別
Models/
├── StatisticsModels.cs           # 統計相關資料模型
wwwroot/js/
├── statistics.js                 # 統計分析前端邏輯
```

### 修改檔案
```
Pages/
├── index7.cshtml                 # 新增統計 Modal HTML
├── index7.cshtml.cs             # 新增統計資料處理方法
```

## 🔧 技術規格

### 1. 後端開發

#### StatisticsService.cs
```csharp
namespace Demo.Services
{
    public class StatisticsService
    {
        private readonly AccountingService _accountingService;
        
        public StatisticsService(AccountingService accountingService)
        {
            _accountingService = accountingService;
        }
        
        // Phase 1 必要方法
        public async Task<List<MonthlyTrendData>> GetMonthlyTrendAsync(int months = 6)
        public async Task<List<CategoryAnalysisData>> GetExpenseCategoryAnalysisAsync(DateTime startDate, DateTime endDate)
        public async Task<StatisticsSummaryData> GetStatisticsSummaryAsync(DateTime startDate, DateTime endDate)
    }
}
```

#### StatisticsModels.cs
```csharp
namespace Demo.Models
{
    public class StatisticsViewModel
    {
        public List<MonthlyTrendData> MonthlyTrend { get; set; } = new();
        public List<CategoryAnalysisData> ExpenseCategories { get; set; } = new();
        public StatisticsSummaryData Summary { get; set; } = new();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class MonthlyTrendData
    {
        public string Month { get; set; } // "2024-08"
        public string MonthName { get; set; } // "2024年8月"
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal NetIncome { get; set; }
        public int TotalRecords { get; set; }
    }

    public class CategoryAnalysisData
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public decimal Percentage { get; set; }
        public int RecordCount { get; set; }
        public string Color { get; set; } // 圖表顏色
    }

    public class StatisticsSummaryData
    {
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal NetIncome { get; set; }
        public int TotalRecords { get; set; }
        public decimal AverageMonthlyIncome { get; set; }
        public decimal AverageMonthlyExpense { get; set; }
        public string TopExpenseCategory { get; set; }
        public decimal TopExpenseAmount { get; set; }
    }
}
```

#### index7.cshtml.cs 新增方法
```csharp
public async Task<IActionResult> OnGetStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null)
{
    // 預設為最近 6 個月
    var end = endDate ?? DateTime.Today;
    var start = startDate ?? DateTime.Today.AddMonths(-6);
    
    var viewModel = new StatisticsViewModel
    {
        StartDate = start,
        EndDate = end,
        MonthlyTrend = await _statisticsService.GetMonthlyTrendAsync(6),
        ExpenseCategories = await _statisticsService.GetExpenseCategoryAnalysisAsync(start, end),
        Summary = await _statisticsService.GetStatisticsSummaryAsync(start, end)
    };
    
    return new JsonResult(viewModel);
}
```

### 2. 前端開發

#### index7.cshtml Modal HTML
```html
<!-- 統計分析對話框 -->
<div class="modal fade" id="statisticsModal" tabindex="-1" aria-labelledby="statisticsModalLabel" aria-hidden="true">
    <div class="modal-dialog modal-xl">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title" id="statisticsModalLabel">
                    <i class="fas fa-chart-pie text-primary"></i> 統計分析
                </h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <!-- Loading 動畫 -->
                <div id="statisticsLoading" class="text-center py-5">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">載入中...</span>
                    </div>
                    <div class="mt-3">正在分析資料...</div>
                </div>
                
                <!-- 統計內容 -->
                <div id="statisticsContent" style="display: none;">
                    <!-- 時間範圍選擇 -->
                    <div class="row mb-4">
                        <div class="col-md-6">
                            <label for="statsStartDate" class="form-label">開始日期</label>
                            <input type="date" class="form-control" id="statsStartDate">
                        </div>
                        <div class="col-md-6">
                            <label for="statsEndDate" class="form-label">結束日期</label>
                            <input type="date" class="form-control" id="statsEndDate">
                        </div>
                    </div>
                    
                    <!-- 統計摘要 -->
                    <div class="row mb-4" id="statisticsSummary">
                        <!-- 動態生成摘要卡片 -->
                    </div>
                    
                    <!-- 圖表區域 -->
                    <div class="row">
                        <div class="col-md-8">
                            <div class="card">
                                <div class="card-header">
                                    <h6 class="mb-0">
                                        <i class="fas fa-chart-line"></i> 月度收支趨勢
                                    </h6>
                                </div>
                                <div class="card-body">
                                    <canvas id="monthlyTrendChart"></canvas>
                                </div>
                            </div>
                        </div>
                        <div class="col-md-4">
                            <div class="card">
                                <div class="card-header">
                                    <h6 class="mb-0">
                                        <i class="fas fa-chart-pie"></i> 支出分類分析
                                    </h6>
                                </div>
                                <div class="card-body">
                                    <canvas id="expenseCategoryChart"></canvas>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
                <button type="button" class="btn btn-primary" onclick="refreshStatistics()">
                    <i class="fas fa-refresh"></i> 重新整理
                </button>
            </div>
        </div>
    </div>
</div>
```

#### statistics.js
```javascript
// 統計分析相關的 JavaScript 函式
let monthlyTrendChart = null;
let expenseCategoryChart = null;

// 顯示統計分析對話框
async function showStatisticsModal() {
    const modal = new bootstrap.Modal(document.getElementById('statisticsModal'));
    modal.show();
    
    // 載入統計資料
    await loadStatisticsData();
}

// 載入統計資料
async function loadStatisticsData() {
    try {
        showStatisticsLoading();
        
        const startDate = document.getElementById('statsStartDate').value;
        const endDate = document.getElementById('statsEndDate').value;
        
        const params = new URLSearchParams();
        if (startDate) params.append('startDate', startDate);
        if (endDate) params.append('endDate', endDate);
        
        const response = await fetch(`/index7?handler=Statistics&${params.toString()}`);
        const data = await response.json();
        
        // 渲染統計資料
        renderStatisticsSummary(data.summary);
        renderMonthlyTrendChart(data.monthlyTrend);
        renderExpenseCategoryChart(data.expenseCategories);
        
        hideStatisticsLoading();
        
    } catch (error) {
        console.error('載入統計資料失敗:', error);
        hideStatisticsLoading();
        showStatisticsError();
    }
}

// 渲染月度趨勢圖表
function renderMonthlyTrendChart(data) {
    const ctx = document.getElementById('monthlyTrendChart').getContext('2d');
    
    if (monthlyTrendChart) {
        monthlyTrendChart.destroy();
    }
    
    monthlyTrendChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(d => d.monthName),
            datasets: [{
                label: '收入',
                data: data.map(d => d.income),
                borderColor: '#28a745',
                backgroundColor: 'rgba(40, 167, 69, 0.1)',
                tension: 0.3
            }, {
                label: '支出',
                data: data.map(d => d.expense),
                borderColor: '#dc3545',
                backgroundColor: 'rgba(220, 53, 69, 0.1)',
                tension: 0.3
            }]
        },
        options: {
            responsive: true,
            plugins: {
                title: {
                    display: false
                },
                legend: {
                    position: 'top'
                }
            },
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

// 渲染支出分類圓餅圖
function renderExpenseCategoryChart(data) {
    const ctx = document.getElementById('expenseCategoryChart').getContext('2d');
    
    if (expenseCategoryChart) {
        expenseCategoryChart.destroy();
    }
    
    expenseCategoryChart = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: data.map(d => d.category),
            datasets: [{
                data: data.map(d => d.amount),
                backgroundColor: data.map(d => d.color || getRandomColor()),
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom'
                },
                tooltip: {
                    callbacks: {
                        label: function(context) {
                            const data = context.raw;
                            const percentage = ((data / context.dataset.data.reduce((a, b) => a + b, 0)) * 100).toFixed(1);
                            return `${context.label}: NT$ ${data.toLocaleString()} (${percentage}%)`;
                        }
                    }
                }
            }
        }
    });
}
```

### 3. 依賴套件

#### NuGet 套件
- 無需新增額外套件 (使用現有的 Newtonsoft.Json)

#### JavaScript 函式庫
```html
<!-- 新增到 _Layout.cshtml -->
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
```

## 📝 開發檢核清單

### 後端開發
- [ ] 建立 `StatisticsService.cs` 服務類別
- [ ] 建立 `StatisticsModels.cs` 資料模型
- [ ] 在 `Program.cs` 註冊 `StatisticsService`
- [ ] 實現 `GetMonthlyTrendAsync` 方法
- [ ] 實現 `GetExpenseCategoryAnalysisAsync` 方法
- [ ] 實現 `GetStatisticsSummaryAsync` 方法
- [ ] 在 `index7.cshtml.cs` 新增 `OnGetStatisticsAsync` 方法

### 前端開發
- [ ] 在 `index7.cshtml` 新增統計 Modal HTML
- [ ] 建立 `statistics.js` 檔案
- [ ] 實現 `showStatisticsModal` 函式
- [ ] 實現 `loadStatisticsData` 函式
- [ ] 實現月度趨勢圖表渲染
- [ ] 實現支出分類圓餅圖渲染
- [ ] 新增時間範圍選擇功能
- [ ] 新增 Loading 動畫和錯誤處理

### 測試
- [ ] 測試統計資料計算準確性
- [ ] 測試圖表顯示效果
- [ ] 測試時間範圍篩選功能
- [ ] 測試行動裝置相容性
- [ ] 測試錯誤處理機制

## ⚠️ 注意事項

1. **效能考量**：確保統計查詢效能，考慮新增資料庫索引
2. **資料驗證**：驗證日期範圍的合理性
3. **錯誤處理**：提供友善的錯誤訊息
4. **響應式設計**：確保在小螢幕裝置上的顯示效果
5. **圖表顏色**：使用一致的配色方案

## 🚀 完成標準

- 統計對話框能正常開啟和關閉
- 月度收支趨勢圖表能正確顯示資料
- 支出分類圓餅圖能正確顯示分類佔比
- 時間範圍選擇功能正常運作
- 統計摘要資料準確顯示
- 載入動畫和錯誤處理機制完善
- 行動裝置相容性良好

## 📅 預估開發時間

- 後端開發：3-4 工作天
- 前端開發：4-5 工作天
- 測試除錯：1-2 工作天
- **總計：8-11 工作天**
