# Phase 3 開發規格書 - 統計分析智能功能

## 📋 開發概述
實現統計分析的最高級功能，包含異常偵測、預算追蹤、AI 智能分析和個人化建議，將記帳系統升級為智能財務管理助手。

## 🎯 開發目標
- 實現異常支出自動偵測和警報
- 建立預算管理和追蹤系統
- 實現消費習慣智能分析
- 提供個人化財務建議
- 建立財務健康評分機制
- 實現預測性分析功能

## 📂 檔案結構規劃

### 新增檔案
```
Services/
├── AnomalyDetectionService.cs    # 異常偵測服務
├── BudgetManagementService.cs    # 預算管理服務
├── FinancialInsightsService.cs   # 財務洞察服務
├── PredictiveAnalysisService.cs  # 預測分析服務
Models/
├── BudgetModels.cs              # 預算相關模型
├── AnomalyModels.cs             # 異常偵測模型
├── InsightsModels.cs            # 洞察分析模型
AI/
├── FinancialAdvisorAI.cs        # AI 財務顧問
├── SpendingPatternAnalyzer.cs   # 消費模式分析器
Pages/
├── Budget.cshtml                # 預算管理頁面
├── Budget.cshtml.cs            # 預算管理邏輯
wwwroot/js/
├── financial-ai.js             # AI 功能前端邏輯
├── budget-management.js         # 預算管理前端邏輯
├── anomaly-alerts.js           # 異常警報前端邏輯
```

### 修改檔案
```
Services/
├── StatisticsService.cs          # 整合智能分析功能
Models/
├── StatisticsModels.cs           # 新增智能分析模型
Pages/
├── index7.cshtml                 # 新增智能分析頁籤
├── index7.cshtml.cs             # 新增智能分析處理邏輯
App_Data/
├── budget-settings.json         # 預算設定檔案
├── spending-patterns.json       # 消費模式數據
```

## 🔧 技術規格

### 1. 異常偵測系統

#### AnomalyDetectionService.cs
```csharp
public class AnomalyDetectionService
{
    private readonly AccountingService _accountingService;
    private readonly StatisticsService _statisticsService;
    
    // 異常偵測演算法
    public async Task<List<AnomalyAlert>> DetectSpendingAnomaliesAsync(DateTime startDate, DateTime endDate)
    {
        // 使用統計學方法偵測異常支出
        // 1. Z-Score 分析
        // 2. 移動平均偏差分析
        // 3. 季節性調整分析
    }
    
    public async Task<List<CategoryAnomaly>> DetectCategoryAnomaliesAsync(DateTime startDate, DateTime endDate)
    {
        // 偵測各分類的異常支出
    }
    
    public async Task<List<FrequencyAnomaly>> DetectFrequencyAnomaliesAsync(DateTime startDate, DateTime endDate)
    {
        // 偵測消費頻率異常
    }
    
    public async Task<RiskAssessment> AssessFinancialRiskAsync(DateTime startDate, DateTime endDate)
    {
        // 評估財務風險等級
    }
}
```

#### AnomalyModels.cs
```csharp
public class AnomalyAlert
{
    public int Id { get; set; }
    public DateTime DetectedDate { get; set; }
    public string AlertType { get; set; } // "amount", "frequency", "category", "pattern"
    public string Severity { get; set; } // "low", "medium", "high", "critical"
    public string Title { get; set; }
    public string Description { get; set; }
    public string Category { get; set; }
    public decimal Amount { get; set; }
    public decimal BaselineAmount { get; set; }
    public decimal DeviationPercentage { get; set; }
    public string Recommendation { get; set; }
    public bool IsRead { get; set; }
    public bool IsDismissed { get; set; }
}

public class CategoryAnomaly
{
    public string Category { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal HistoricalAverage { get; set; }
    public decimal StandardDeviation { get; set; }
    public double ZScore { get; set; }
    public string AnomalyType { get; set; } // "spike", "drop", "trend_change"
    public int DaysFromNormal { get; set; }
}

public class RiskAssessment
{
    public int OverallScore { get; set; } // 0-100
    public string RiskLevel { get; set; } // "low", "medium", "high"
    public List<RiskFactor> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public decimal DebtToIncomeRatio { get; set; }
    public decimal SavingsRate { get; set; }
    public decimal ExpenseVolatility { get; set; }
}

public class RiskFactor
{
    public string Factor { get; set; }
    public int Score { get; set; } // 0-100
    public string Impact { get; set; } // "positive", "negative", "neutral"
    public string Description { get; set; }
}
```

### 2. 預算管理系統

#### BudgetManagementService.cs
```csharp
public class BudgetManagementService
{
    public async Task<List<BudgetItem>> GetBudgetsAsync(int year, int month)
    public async Task<BudgetItem> CreateBudgetAsync(CreateBudgetRequest request)
    public async Task<BudgetItem> UpdateBudgetAsync(int budgetId, UpdateBudgetRequest request)
    public async Task<bool> DeleteBudgetAsync(int budgetId)
    
    public async Task<List<BudgetPerformance>> GetBudgetPerformanceAsync(int year, int month)
    public async Task<BudgetSummary> GetBudgetSummaryAsync(int year, int month)
    public async Task<List<BudgetAlert>> GetBudgetAlertsAsync()
    
    // 預算建議功能
    public async Task<List<BudgetSuggestion>> GenerateBudgetSuggestionsAsync(DateTime startDate, DateTime endDate)
    public async Task<OptimalBudgetPlan> OptimizeBudgetPlanAsync(decimal totalBudget, List<string> priorityCategories)
}
```

#### BudgetModels.cs
```csharp
public class BudgetItem
{
    public int Id { get; set; }
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public decimal BudgetAmount { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public bool IsActive { get; set; }
    public string Notes { get; set; }
}

public class BudgetPerformance
{
    public BudgetItem Budget { get; set; }
    public decimal ActualAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public decimal UsedPercentage { get; set; }
    public int DaysRemaining { get; set; }
    public string Status { get; set; } // "on_track", "warning", "exceeded"
    public decimal DailyAverageSpending { get; set; }
    public decimal RecommendedDailySpending { get; set; }
}

public class BudgetAlert
{
    public int Id { get; set; }
    public string Category { get; set; }
    public string AlertType { get; set; } // "approaching_limit", "exceeded", "no_activity"
    public decimal BudgetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal Percentage { get; set; }
    public string Message { get; set; }
    public DateTime CreatedDate { get; set; }
    public bool IsRead { get; set; }
}

public class BudgetSuggestion
{
    public string Category { get; set; }
    public decimal SuggestedAmount { get; set; }
    public decimal HistoricalAverage { get; set; }
    public string Reasoning { get; set; }
    public decimal ConfidenceScore { get; set; } // 0-1
    public List<string> ConsiderationFactors { get; set; } = new();
}
```

### 3. AI 財務洞察系統

#### FinancialInsightsService.cs
```csharp
public class FinancialInsightsService
{
    public async Task<List<SmartInsight>> GenerateSmartInsightsAsync(DateTime startDate, DateTime endDate)
    public async Task<List<PersonalizedRecommendation>> GetPersonalizedRecommendationsAsync()
    public async Task<FinancialHealthScore> CalculateFinancialHealthAsync()
    public async Task<List<SavingsOpportunity>> IdentifySavingsOpportunitiesAsync()
    public async Task<SpendingEfficiencyAnalysis> AnalyzeSpendingEfficiencyAsync()
}
```

#### InsightsModels.cs
```csharp
public class SmartInsight
{
    public int Id { get; set; }
    public string Type { get; set; } // "spending_pattern", "savings_opportunity", "trend_analysis"
    public string Title { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public string Color { get; set; } // "success", "warning", "info", "danger"
    public decimal Impact { get; set; } // 預估影響金額
    public int Priority { get; set; } // 1-5 優先級
    public List<string> ActionItems { get; set; } = new();
    public DateTime GeneratedDate { get; set; }
    public bool IsActionable { get; set; }
}

public class PersonalizedRecommendation
{
    public string Category { get; set; }
    public string Recommendation { get; set; }
    public string Reasoning { get; set; }
    public decimal PotentialSavings { get; set; }
    public string DifficultyLevel { get; set; } // "easy", "medium", "hard"
    public int TimeFrame { get; set; } // 建議實施的天數
    public List<string> Steps { get; set; } = new();
}

public class FinancialHealthScore
{
    public int OverallScore { get; set; } // 0-100
    public string HealthLevel { get; set; } // "excellent", "good", "fair", "poor"
    public List<HealthMetric> Metrics { get; set; } = new();
    public List<string> StrengthAreas { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public DateTime CalculatedDate { get; set; }
}

public class HealthMetric
{
    public string Name { get; set; }
    public int Score { get; set; }
    public string Description { get; set; }
    public string Benchmark { get; set; }
    public bool IsGood { get; set; }
}

public class SavingsOpportunity
{
    public string Category { get; set; }
    public string Description { get; set; }
    public decimal PotentialMonthlySavings { get; set; }
    public decimal PotentialYearlySavings { get; set; }
    public string Method { get; set; }
    public string DifficultyLevel { get; set; }
    public List<string> RequiredActions { get; set; } = new();
}
```

### 4. 預測性分析系統

#### PredictiveAnalysisService.cs
```csharp
public class PredictiveAnalysisService
{
    public async Task<List<ExpenseForecast>> ForecastExpensesAsync(int monthsAhead = 6)
    public async Task<IncomeForecast> ForecastIncomeAsync(int monthsAhead = 6)
    public async Task<CashFlowProjection> ProjectCashFlowAsync(int monthsAhead = 12)
    public async Task<List<SeasonalTrend>> AnalyzeSeasonalTrendsAsync()
    public async Task<GoalAchievementPrediction> PredictGoalAchievementAsync(decimal targetAmount, DateTime targetDate)
}
```

### 5. 前端智能功能

#### index7.cshtml 新增智能分析頁籤
```html
<!-- 新增智能分析頁籤 -->
<li class="nav-item" role="presentation">
    <button class="nav-link" id="ai-tab" data-bs-toggle="tab" data-bs-target="#ai-insights" type="button" role="tab">
        <i class="fas fa-robot"></i> AI 洞察
    </button>
</li>

<!-- AI 洞察頁籤內容 -->
<div class="tab-pane fade" id="ai-insights" role="tabpanel">
    <!-- 財務健康分數 -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card border-primary">
                <div class="card-header bg-primary text-white">
                    <h6 class="mb-0">
                        <i class="fas fa-heartbeat"></i> 財務健康分數
                    </h6>
                </div>
                <div class="card-body" id="healthScoreCard">
                    <!-- 動態生成健康分數儀表板 -->
                </div>
            </div>
        </div>
    </div>
    
    <!-- 智能洞察卡片 -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-lightbulb text-warning"></i> 智能洞察
                    </h6>
                </div>
                <div class="card-body" id="smartInsightsContainer">
                    <!-- 動態生成智能洞察卡片 -->
                </div>
            </div>
        </div>
    </div>
    
    <!-- 個人化建議 -->
    <div class="row mb-4">
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-user-cog text-info"></i> 個人化建議
                    </h6>
                </div>
                <div class="card-body" id="personalizedRecommendations">
                    <!-- 動態生成個人化建議 -->
                </div>
            </div>
        </div>
        <div class="col-md-6">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-piggy-bank text-success"></i> 省錢機會
                    </h6>
                </div>
                <div class="card-body" id="savingsOpportunities">
                    <!-- 動態生成省錢建議 -->
                </div>
            </div>
        </div>
    </div>
    
    <!-- 異常偵測警報 -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-exclamation-triangle text-warning"></i> 異常偵測警報
                    </h6>
                </div>
                <div class="card-body" id="anomalyAlertsContainer">
                    <!-- 動態生成異常警報 -->
                </div>
            </div>
        </div>
    </div>
    
    <!-- 預測分析 -->
    <div class="row">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-crystal-ball text-purple"></i> 支出預測
                    </h6>
                </div>
                <div class="card-body">
                    <canvas id="expenseForecastChart"></canvas>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-chart-area text-info"></i> 現金流預測
                    </h6>
                </div>
                <div class="card-body">
                    <canvas id="cashFlowChart"></canvas>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### financial-ai.js
```javascript
// AI 財務洞察相關功能

// 載入 AI 洞察資料
async function loadAIInsights() {
    await Promise.all([
        loadFinancialHealthScore(),
        loadSmartInsights(),
        loadPersonalizedRecommendations(),
        loadAnomalyAlerts(),
        loadPredictiveAnalysis()
    ]);
}

// 載入財務健康分數
async function loadFinancialHealthScore() {
    // 實現健康分數載入和渲染
}

// 載入智能洞察
async function loadSmartInsights() {
    // 實現智能洞察載入和卡片渲染
}

// 載入異常警報
async function loadAnomalyAlerts() {
    // 實現異常警報載入和顯示
}

// 渲染健康分數儀表板
function renderHealthScoreGauge(healthScore) {
    // 使用 Chart.js 建立儀表板圖表
}

// 渲染支出預測圖表
function renderExpenseForecastChart(forecastData) {
    // 使用預測資料建立趨勢圖
}

// 處理洞察互動
function handleInsightInteraction(insightId, action) {
    // 處理使用者對洞察的互動（標記為已讀、執行建議等）
}
```

### 6. 預算管理頁面

#### Budget.cshtml
```html
@page
@model Demo.Pages.BudgetModel
@{
    ViewData["Title"] = "預算管理";
}

<div class="container-fluid">
    <!-- 預算概覽 -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <div class="d-flex justify-content-between align-items-center">
                        <h5 class="mb-0">
                            <i class="fas fa-calculator text-primary"></i> 預算管理
                        </h5>
                        <button type="button" class="btn btn-primary" onclick="showCreateBudgetModal()">
                            <i class="fas fa-plus"></i> 新增預算
                        </button>
                    </div>
                </div>
                <div class="card-body">
                    <!-- 預算摘要卡片 -->
                    <div class="row" id="budgetSummaryCards">
                        <!-- 動態生成預算摘要 -->
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- 預算進度追蹤 -->
    <div class="row mb-4">
        <div class="col-12">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-tasks"></i> 預算執行狀況
                    </h6>
                </div>
                <div class="card-body">
                    <div id="budgetProgressContainer">
                        <!-- 動態生成預算進度條 -->
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <!-- 預算建議 -->
    <div class="row">
        <div class="col-md-8">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-chart-bar"></i> 預算 vs 實際支出
                    </h6>
                </div>
                <div class="card-body">
                    <canvas id="budgetComparisonChart"></canvas>
                </div>
            </div>
        </div>
        <div class="col-md-4">
            <div class="card">
                <div class="card-header">
                    <h6 class="mb-0">
                        <i class="fas fa-lightbulb"></i> 預算建議
                    </h6>
                </div>
                <div class="card-body" id="budgetSuggestions">
                    <!-- 動態生成預算建議 -->
                </div>
            </div>
        </div>
    </div>
</div>
```

## 📝 開發檢核清單

### 後端開發
- [ ] 建立 `AnomalyDetectionService.cs` 異常偵測服務
- [ ] 建立 `BudgetManagementService.cs` 預算管理服務
- [ ] 建立 `FinancialInsightsService.cs` 財務洞察服務
- [ ] 建立 `PredictiveAnalysisService.cs` 預測分析服務
- [ ] 實現異常支出偵測演算法
- [ ] 實現預算追蹤和警報系統
- [ ] 實現 AI 財務建議生成
- [ ] 實現財務健康評分機制
- [ ] 實現支出預測和現金流分析

### 前端開發
- [ ] 新增 AI 洞察頁籤
- [ ] 建立預算管理頁面
- [ ] 實現財務健康分數儀表板
- [ ] 實現智能洞察卡片系統
- [ ] 實現異常警報通知
- [ ] 實現預測分析圖表
- [ ] 實現預算進度追蹤介面
- [ ] 實現個人化建議互動

### AI/ML 功能
- [ ] 實現統計學異常偵測
- [ ] 建立消費模式學習機制
- [ ] 實現個人化建議算法
- [ ] 建立財務健康評分模型
- [ ] 實現時間序列預測
- [ ] 建立季節性調整機制

### 測試
- [ ] 測試異常偵測準確性
- [ ] 測試預算管理功能
- [ ] 測試 AI 建議品質
- [ ] 測試預測分析準確度
- [ ] 測試財務健康評分合理性
- [ ] 測試系統整合穩定性

## ⚠️ 注意事項

1. **演算法準確性**：確保異常偵測和預測的準確性
2. **資料隱私**：保護使用者財務資料的隱私和安全
3. **計算效能**：AI 功能需要考慮計算資源的使用
4. **使用者體驗**：避免過度複雜的 AI 功能影響易用性
5. **建議品質**：確保 AI 建議的實用性和可行性

## 🚀 完成標準

- 異常支出偵測系統準確運作
- 預算管理功能完整可用
- AI 洞察提供有價值的建議
- 財務健康評分合理反映財務狀況
- 預測分析具有參考價值
- 個人化建議切合使用者需求
- 所有智能功能整合順暢
- 系統效能和穩定性良好

## 📅 預估開發時間

- AI/ML 服務開發：6-8 工作天
- 預算管理系統：4-5 工作天
- 前端智能介面：5-6 工作天
- 異常偵測系統：3-4 工作天
- 預測分析功能：3-4 工作天
- 整合測試除錯：3-4 工作天
- **總計：24-31 工作天**

## 🎯 長期擴展規劃

1. **機器學習模型訓練**：使用更多使用者資料訓練個人化模型
2. **外部資料整合**：整合銀行 API、股市資料等
3. **語音助手功能**：新增語音記帳和查詢功能
4. **行動應用同步**：開發行動應用並同步資料
5. **社群功能**：新增消費比較和理財社群功能
