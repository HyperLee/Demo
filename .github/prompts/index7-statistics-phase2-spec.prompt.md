# Phase 2 開發規格書 - 統計分析進階功能

## 📋 開發概述
在 Phase 1 基礎功能上擴展，新增收入分析、時間模式分析、分類排行榜和資料匯出功能。

## 🎯 開發目標
- 新增收入分類分析功能
- 實現時間模式分析 (週日模式、月內模式)
- 新增分類排行榜功能
- 實現統計資料匯出功能 (Excel/PDF)
- 優化使用者介面和互動體驗

## 📂 檔案結構規劃

### 新增檔案
```
Services/
├── StatisticsExportService.cs    # 統計報表匯出服務
Utilities/
├── StatisticsReportUtility.cs    # 統計報表產生工具
wwwroot/js/
├── statistics-advanced.js        # 進階統計分析邏輯
```

### 修改檔案
```
Services/
├── StatisticsService.cs          # 擴展統計分析方法
Models/
├── StatisticsModels.cs           # 新增進階統計模型
Pages/
├── index7.cshtml                 # 擴展統計 Modal 內容
├── index7.cshtml.cs             # 新增進階統計處理方法
```

## 🔧 技術規格

### 1. 後端開發擴展

#### StatisticsService.cs 新增方法
```csharp
public class StatisticsService
{
    // Phase 2 新增方法
    public async Task<List<CategoryAnalysisData>> GetIncomeCategoryAnalysisAsync(DateTime startDate, DateTime endDate)
    public async Task<List<CategoryRankingData>> GetCategoryRankingAsync(DateTime startDate, DateTime endDate, bool isIncome = false)
    public async Task<TimePatternAnalysisData> GetTimePatternAnalysisAsync(DateTime startDate, DateTime endDate)
    public async Task<List<WeekdayPatternData>> GetWeekdayPatternsAsync(DateTime startDate, DateTime endDate)
    public async Task<List<MonthlyPatternData>> GetMonthlyPatternsAsync(DateTime startDate, DateTime endDate)
    public async Task<ComparisonAnalysisData> GetComparisonAnalysisAsync(DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd)
}
```

#### StatisticsModels.cs 新增模型
```csharp
public class CategoryRankingData
{
    public int Rank { get; set; }
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public decimal Amount { get; set; }
    public int RecordCount { get; set; }
    public decimal AverageAmount { get; set; }
    public decimal PercentageChange { get; set; } // 與上期比較
    public string Trend { get; set; } // "up", "down", "stable"
}

public class TimePatternAnalysisData
{
    public List<WeekdayPatternData> WeekdayPatterns { get; set; } = new();
    public List<MonthlyPatternData> MonthlyPatterns { get; set; } = new();
    public DailyPatternSummary DailySummary { get; set; } = new();
}

public class WeekdayPatternData
{
    public string Weekday { get; set; } // "週一", "週二"...
    public int WeekdayIndex { get; set; } // 0=週日, 1=週一...
    public decimal AverageIncome { get; set; }
    public decimal AverageExpense { get; set; }
    public int RecordCount { get; set; }
    public List<string> PopularCategories { get; set; } = new();
}

public class MonthlyPatternData
{
    public string Period { get; set; } // "月初", "月中", "月底"
    public int StartDay { get; set; }
    public int EndDay { get; set; }
    public decimal AverageIncome { get; set; }
    public decimal AverageExpense { get; set; }
    public int RecordCount { get; set; }
    public decimal Percentage { get; set; } // 佔全月比例
}

public class DailyPatternSummary
{
    public string MostActiveWeekday { get; set; }
    public string HighestExpenseWeekday { get; set; }
    public string MostActivePeriod { get; set; }
    public decimal WeekdayAverageExpense { get; set; }
    public decimal WeekendAverageExpense { get; set; }
}

public class ComparisonAnalysisData
{
    public string PeriodName { get; set; } // "本月 vs 上月"
    public StatisticsSummaryData CurrentPeriod { get; set; }
    public StatisticsSummaryData PreviousPeriod { get; set; }
    public decimal IncomeGrowthRate { get; set; }
    public decimal ExpenseGrowthRate { get; set; }
    public decimal NetIncomeGrowthRate { get; set; }
    public List<CategoryComparisonData> CategoryChanges { get; set; } = new();
}

public class CategoryComparisonData
{
    public string Category { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal PreviousAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal ChangePercentage { get; set; }
    public string ChangeDirection { get; set; } // "增加", "減少", "持平"
}

public class StatisticsExportRequest
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Format { get; set; } // "excel", "pdf"
    public bool IncludeCharts { get; set; }
    public bool IncludeSummary { get; set; }
    public bool IncludeDetailedRecords { get; set; }
    public List<string> IncludeAnalysis { get; set; } = new(); // "trend", "category", "pattern"
}
```

#### StatisticsExportService.cs
```csharp
public class StatisticsExportService
{
    private readonly StatisticsService _statisticsService;
    
    public async Task<byte[]> ExportToExcelAsync(StatisticsExportRequest request)
    {
        // 使用 ClosedXML 產生 Excel 報表
    }
    
    public async Task<byte[]> ExportToPdfAsync(StatisticsExportRequest request)
    {
        // 使用 iText 產生 PDF 報表
    }
    
    private async Task<StatisticsReportData> PrepareReportDataAsync(StatisticsExportRequest request)
    {
        // 準備報表資料
    }
}
```

### 2. 前端開發擴展

#### index7.cshtml Modal 擴展
```html
<!-- 統計分析對話框擴展 -->
<div class="modal-body">
    <!-- 頁籤導航 -->
    <ul class="nav nav-tabs" id="statisticsTab" role="tablist">
        <li class="nav-item" role="presentation">
            <button class="nav-link active" id="overview-tab" data-bs-toggle="tab" data-bs-target="#overview" type="button" role="tab">
                <i class="fas fa-chart-line"></i> 收支概況
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="category-tab" data-bs-toggle="tab" data-bs-target="#category" type="button" role="tab">
                <i class="fas fa-chart-pie"></i> 分類分析
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="pattern-tab" data-bs-toggle="tab" data-bs-target="#pattern" type="button" role="tab">
                <i class="fas fa-calendar-week"></i> 時間模式
            </button>
        </li>
        <li class="nav-item" role="presentation">
            <button class="nav-link" id="ranking-tab" data-bs-toggle="tab" data-bs-target="#ranking" type="button" role="tab">
                <i class="fas fa-trophy"></i> 分類排行
            </button>
        </li>
    </ul>
    
    <!-- 頁籤內容 -->
    <div class="tab-content" id="statisticsTabContent">
        <!-- 收支概況頁籤 -->
        <div class="tab-pane fade show active" id="overview" role="tabpanel">
            <!-- Phase 1 的內容 -->
        </div>
        
        <!-- 分類分析頁籤 -->
        <div class="tab-pane fade" id="category" role="tabpanel">
            <div class="row mt-3">
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="mb-0">
                                <i class="fas fa-arrow-up text-success"></i> 收入分類分析
                            </h6>
                        </div>
                        <div class="card-body">
                            <canvas id="incomeCategoryChart"></canvas>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="mb-0">
                                <i class="fas fa-arrow-down text-danger"></i> 支出分類分析
                            </h6>
                        </div>
                        <div class="card-body">
                            <canvas id="expenseCategoryChart"></canvas>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- 時間模式頁籤 -->
        <div class="tab-pane fade" id="pattern" role="tabpanel">
            <div class="row mt-3">
                <div class="col-md-7">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="mb-0">
                                <i class="fas fa-calendar-week"></i> 週間消費模式
                            </h6>
                        </div>
                        <div class="card-body">
                            <canvas id="weekdayPatternChart"></canvas>
                        </div>
                    </div>
                </div>
                <div class="col-md-5">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="mb-0">
                                <i class="fas fa-calendar-day"></i> 月內消費模式
                            </h6>
                        </div>
                        <div class="card-body">
                            <canvas id="monthlyPatternChart"></canvas>
                        </div>
                    </div>
                </div>
            </div>
            
            <!-- 模式分析摘要 -->
            <div class="row mt-3">
                <div class="col-12">
                    <div class="card">
                        <div class="card-header">
                            <h6 class="mb-0">
                                <i class="fas fa-lightbulb"></i> 消費習慣分析
                            </h6>
                        </div>
                        <div class="card-body" id="patternInsights">
                            <!-- 動態生成分析結果 -->
                        </div>
                    </div>
                </div>
            </div>
        </div>
        
        <!-- 分類排行頁籤 -->
        <div class="tab-pane fade" id="ranking" role="tabpanel">
            <div class="row mt-3">
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-header d-flex justify-content-between">
                            <h6 class="mb-0">
                                <i class="fas fa-trophy text-warning"></i> 支出排行榜
                            </h6>
                            <small class="text-muted">TOP 10</small>
                        </div>
                        <div class="card-body p-0">
                            <div class="table-responsive">
                                <table class="table table-striped mb-0" id="expenseRankingTable">
                                    <!-- 動態生成排行榜 -->
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-md-6">
                    <div class="card">
                        <div class="card-header d-flex justify-content-between">
                            <h6 class="mb-0">
                                <i class="fas fa-medal text-success"></i> 收入排行榜
                            </h6>
                            <small class="text-muted">TOP 10</small>
                        </div>
                        <div class="card-body p-0">
                            <div class="table-responsive">
                                <table class="table table-striped mb-0" id="incomeRankingTable">
                                    <!-- 動態生成排行榜 -->
                                </table>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- Modal Footer 擴展 -->
<div class="modal-footer">
    <div class="btn-group dropup">
        <button type="button" class="btn btn-success dropdown-toggle" data-bs-toggle="dropdown">
            <i class="fas fa-download"></i> 匯出報表
        </button>
        <ul class="dropdown-menu">
            <li><a class="dropdown-item" href="#" onclick="exportStatistics('excel')">
                <i class="fas fa-file-excel text-success"></i> Excel 格式
            </a></li>
            <li><a class="dropdown-item" href="#" onclick="exportStatistics('pdf')">
                <i class="fas fa-file-pdf text-danger"></i> PDF 格式
            </a></li>
        </ul>
    </div>
    <button type="button" class="btn btn-info" onclick="showComparisonModal()">
        <i class="fas fa-balance-scale"></i> 期間比較
    </button>
    <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">關閉</button>
    <button type="button" class="btn btn-primary" onclick="refreshStatistics()">
        <i class="fas fa-refresh"></i> 重新整理
    </button>
</div>
```

#### statistics-advanced.js 新增功能
```javascript
// Phase 2 新增的進階統計功能

// 載入分類分析資料
async function loadCategoryAnalysis() {
    // 載入收入和支出分類分析
}

// 載入時間模式分析
async function loadTimePatternAnalysis() {
    // 載入週間和月內消費模式
}

// 載入分類排行榜
async function loadCategoryRanking() {
    // 載入收入和支出排行榜
}

// 渲染收入分類圓餅圖
function renderIncomeCategoryChart(data) {
    // 實現收入分類圓餅圖
}

// 渲染週間消費模式圖表
function renderWeekdayPatternChart(data) {
    // 使用柱狀圖顯示一週七天的消費模式
}

// 渲染月內消費模式圖表
function renderMonthlyPatternChart(data) {
    // 使用圓餅圖顯示月初中底的消費分佈
}

// 渲染排行榜表格
function renderRankingTable(data, tableId, type) {
    // 動態生成排行榜表格
}

// 生成消費習慣分析摘要
function generatePatternInsights(patternData) {
    // 分析消費模式並生成建議
}

// 匯出統計報表
async function exportStatistics(format) {
    // 匯出 Excel 或 PDF 報表
}

// 顯示期間比較對話框
function showComparisonModal() {
    // 顯示期間比較功能
}
```

### 3. 匯出功能實現

#### Excel 匯出範例
```csharp
public async Task<byte[]> ExportToExcelAsync(StatisticsExportRequest request)
{
    using var workbook = new XLWorkbook();
    
    // 摘要工作表
    var summarySheet = workbook.Worksheets.Add("統計摘要");
    
    // 收支趨勢工作表
    var trendSheet = workbook.Worksheets.Add("收支趨勢");
    
    // 分類分析工作表
    var categorySheet = workbook.Worksheets.Add("分類分析");
    
    // 時間模式工作表
    var patternSheet = workbook.Worksheets.Add("時間模式");
    
    // 填入資料...
    
    using var stream = new MemoryStream();
    workbook.SaveAs(stream);
    return stream.ToArray();
}
```

## 📝 開發檢核清單

### 後端開發
- [ ] 擴展 `StatisticsService.cs` 新增進階分析方法
- [ ] 建立 `StatisticsExportService.cs`
- [ ] 新增進階統計資料模型
- [ ] 實現收入分類分析功能
- [ ] 實現時間模式分析功能
- [ ] 實現分類排行榜功能
- [ ] 實現資料匯出功能 (Excel/PDF)
- [ ] 新增期間比較分析功能

### 前端開發
- [ ] 新增統計頁籤導航
- [ ] 實現收入分類圓餅圖
- [ ] 實現週間消費模式圖表
- [ ] 實現月內消費模式圖表
- [ ] 實現分類排行榜表格
- [ ] 新增匯出功能按鈕和邏輯
- [ ] 實現消費習慣分析摘要
- [ ] 新增期間比較對話框

### 測試
- [ ] 測試進階統計計算準確性
- [ ] 測試頁籤切換功能
- [ ] 測試圖表互動效果
- [ ] 測試匯出功能
- [ ] 測試期間比較功能
- [ ] 測試行動裝置相容性

## ⚠️ 注意事項

1. **效能最佳化**：大量資料的統計計算需要考慮快取機制
2. **圖表效能**：避免在同一頁面渲染過多圖表
3. **匯出檔案大小**：控制匯出檔案的大小和內容
4. **記憶體管理**：注意匯出大量資料時的記憶體使用
5. **使用者體驗**：提供適當的載入提示和進度指示

## 🚀 完成標準

- 四個統計頁籤功能完全正常
- 收入分類分析圖表正確顯示
- 時間模式分析準確呈現消費習慣
- 分類排行榜資料正確排序
- Excel 和 PDF 匯出功能正常
- 期間比較分析邏輯正確
- 消費習慣分析提供有用見解
- 所有圖表互動功能正常
- 行動裝置相容性良好

## 📅 預估開發時間

- 後端擴展開發：4-5 工作天
- 前端進階功能：5-6 工作天
- 匯出功能開發：2-3 工作天
- 測試除錯：2-3 工作天
- **總計：13-17 工作天**
