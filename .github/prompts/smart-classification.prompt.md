# 智能分類開發規格書

## 📋 專案概述
開發智能分類系統，提升記帳便利性。透過機器學習和規則引擎，自動分析交易描述、金額、商家等資訊，智能推薦或自動分配適當的支出分類，減少手動分類的工作量，提高記帳效率。

## 🎯 開發目標
- 建立智能分類推薦引擎
- 實現自動分類規則管理
- 提供分類學習和訓練功能
- 建立分類準確度統計
- 支援手動校正和回饋機制

## 🔧 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, jQuery, Select2, HTML5, CSS3
- **AI/ML**: 簡化版機器學習演算法 (TF-IDF + 相似度計算)
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 📂 檔案結構規劃

### 新增檔案
```
Services/
├── SmartCategoryService.cs      # 智能分類服務
├── CategoryLearningService.cs   # 分類學習服務
├── TextAnalysisService.cs       # 文字分析服務

Models/
├── SmartCategoryModels.cs       # 智能分類相關模型

App_Data/
├── category-rules.json          # 分類規則資料
├── category-training.json       # 分類訓練資料
├── merchant-mapping.json        # 商家對應資料
├── keyword-dictionary.json      # 關鍵字字典

wwwroot/js/
├── smart-category.js           # 智能分類前端邏輯

wwwroot/css/
├── smart-category.css          # 智能分類樣式
```

### 修改檔案
```
Pages/
├── index8.cshtml               # 新增智能分類功能
├── index8.cshtml.cs           # 加入智能分類邏輯

Services/
├── AccountingService.cs        # 整合智能分類功能
```

## 🎨 核心功能模組

### 1. 智能分類推薦系統

#### 1.1 分類推薦演算法
```csharp
namespace Demo.Services
{
    public class SmartCategoryService
    {
        private readonly TextAnalysisService _textAnalysis;
        private readonly CategoryLearningService _learning;

        // 主要推薦方法
        public async Task<List<CategorySuggestion>> SuggestCategoriesAsync(
            string description, 
            decimal amount, 
            string merchant = "")
        {
            var suggestions = new List<CategorySuggestion>();
            
            // 1. 規則引擎匹配
            var ruleBased = await GetRuleBasedSuggestionsAsync(description, merchant);
            suggestions.AddRange(ruleBased);
            
            // 2. 關鍵字匹配
            var keywordBased = await GetKeywordBasedSuggestionsAsync(description);
            suggestions.AddRange(keywordBased);
            
            // 3. 歷史記錄相似度匹配
            var historyBased = await GetHistoryBasedSuggestionsAsync(description, amount);
            suggestions.AddRange(historyBased);
            
            // 4. 商家匹配
            var merchantBased = await GetMerchantBasedSuggestionsAsync(merchant);
            suggestions.AddRange(merchantBased);
            
            // 5. 金額範圍匹配
            var amountBased = await GetAmountBasedSuggestionsAsync(amount);
            suggestions.AddRange(amountBased);
            
            // 合併和排序建議
            return ConsolidateAndRankSuggestions(suggestions);
        }

        // 規則引擎匹配
        private async Task<List<CategorySuggestion>> GetRuleBasedSuggestionsAsync(
            string description, string merchant)
        {
            var rules = await LoadCategoryRulesAsync();
            var suggestions = new List<CategorySuggestion>();
            
            foreach (var rule in rules)
            {
                var confidence = CalculateRuleMatch(rule, description, merchant);
                if (confidence > rule.MinConfidence)
                {
                    suggestions.Add(new CategorySuggestion
                    {
                        CategoryId = rule.CategoryId,
                        Confidence = confidence,
                        Reason = $"規則匹配: {rule.Name}",
                        SourceType = SuggestionSourceType.RuleBased
                    });
                }
            }
            
            return suggestions;
        }

        // 文字相似度計算
        private async Task<List<CategorySuggestion>> GetHistoryBasedSuggestionsAsync(
            string description, decimal amount)
        {
            var historicalData = await LoadHistoricalDataAsync();
            var suggestions = new List<CategorySuggestion>();
            
            foreach (var record in historicalData)
            {
                var similarity = _textAnalysis.CalculateSimilarity(description, record.Description);
                var amountSimilarity = CalculateAmountSimilarity(amount, record.Amount);
                
                var combinedScore = (similarity * 0.7) + (amountSimilarity * 0.3);
                
                if (combinedScore > 0.6)
                {
                    suggestions.Add(new CategorySuggestion
                    {
                        CategoryId = record.CategoryId,
                        Confidence = combinedScore,
                        Reason = $"相似記錄: {record.Description}",
                        SourceType = SuggestionSourceType.HistoryBased
                    });
                }
            }
            
            return suggestions.OrderByDescending(s => s.Confidence).Take(3).ToList();
        }
    }
}
```

#### 1.2 文字分析服務
```csharp
public class TextAnalysisService
{
    private readonly Dictionary<string, string[]> _keywordDictionary;

    public TextAnalysisService()
    {
        _keywordDictionary = LoadKeywordDictionary();
    }

    // TF-IDF 向量計算
    public double CalculateSimilarity(string text1, string text2)
    {
        var tokens1 = TokenizeAndNormalize(text1);
        var tokens2 = TokenizeAndNormalize(text2);
        
        var vector1 = CreateTfIdfVector(tokens1);
        var vector2 = CreateTfIdfVector(tokens2);
        
        return CalculateCosineSimilarity(vector1, vector2);
    }

    // 文字正規化和分詞
    private List<string> TokenizeAndNormalize(string text)
    {
        // 移除標點符號、轉小寫、分詞
        var normalized = text.ToLower()
            .Replace("超商", "便利商店")
            .Replace("小七", "7-eleven")
            .Replace("全家", "familymart");
            
        var tokens = normalized.Split(new[] { ' ', ',', '-', '|' }, 
            StringSplitOptions.RemoveEmptyEntries);
            
        return tokens.Where(t => t.Length > 1).ToList();
    }

    // 提取關鍵特徵
    public CategoryFeatures ExtractFeatures(string description, decimal amount, string merchant)
    {
        return new CategoryFeatures
        {
            Keywords = ExtractKeywords(description),
            MerchantType = ClassifyMerchantType(merchant),
            AmountRange = ClassifyAmountRange(amount),
            TimePattern = ExtractTimePattern(DateTime.Now),
            TextLength = description.Length,
            HasNumbers = ContainsNumbers(description),
            Language = DetectLanguage(description)
        };
    }
}
```

### 2. 記帳頁面智能分類整合

#### 2.1 修改新增記帳表單 (index8.cshtml)
```html
<!-- 智能分類推薦區域 -->
<div class="smart-category-section mb-3" id="smartCategorySection" style="display: none;">
    <label class="form-label">
        <i class="fas fa-magic text-primary"></i> 智能分類建議
    </label>
    <div id="categorySuggestions" class="category-suggestions">
        <!-- 動態載入分類建議 -->
    </div>
    <div class="mt-2">
        <small class="text-muted">
            <i class="fas fa-lightbulb"></i> 
            點選建議的分類或手動選擇分類
        </small>
    </div>
</div>

<!-- 修改原有分類選擇 -->
<div class="mb-3">
    <label for="categoryId" class="form-label">支出分類 <span class="text-danger">*</span></label>
    <div class="input-group">
        <select class="form-select" id="categoryId" name="categoryId" required>
            <option value="">請選擇分類</option>
            @foreach (var category in Model.Categories)
            {
                <option value="@category.Id" data-icon="@category.IconClass">
                    @category.Name
                </option>
            }
        </select>
        <button type="button" class="btn btn-outline-secondary" id="smartCategoryBtn" 
                data-bs-toggle="tooltip" title="智能分類建議">
            <i class="fas fa-magic"></i>
        </button>
    </div>
    <div class="smart-category-feedback mt-2" id="categoryFeedback" style="display: none;">
        <div class="d-flex align-items-center">
            <small class="text-muted me-2">分類是否正確？</small>
            <button type="button" class="btn btn-sm btn-success me-1" id="feedbackCorrect">
                <i class="fas fa-thumbs-up"></i>
            </button>
            <button type="button" class="btn btn-sm btn-danger" id="feedbackIncorrect">
                <i class="fas fa-thumbs-down"></i>
            </button>
        </div>
    </div>
</div>

<!-- 自動學習提示 -->
<div class="alert alert-info" id="learningAlert" style="display: none;">
    <i class="fas fa-graduation-cap"></i>
    <strong>學習中...</strong> 系統正在從您的操作中學習，下次會提供更準確的建議！
</div>
```

#### 2.2 分類建議卡片樣板
```html
<!-- 分類建議卡片樣板 -->
<script type="text/template" id="suggestionCardTemplate">
    <div class="suggestion-card" data-category-id="{{categoryId}}" data-confidence="{{confidence}}">
        <div class="suggestion-content">
            <div class="suggestion-icon">
                <i class="{{iconClass}}"></i>
            </div>
            <div class="suggestion-info">
                <div class="suggestion-name">{{categoryName}}</div>
                <div class="suggestion-reason">{{reason}}</div>
            </div>
            <div class="suggestion-confidence">
                <div class="confidence-bar">
                    <div class="confidence-fill" style="width: {{confidencePercent}}%"></div>
                </div>
                <small>{{confidencePercent}}%</small>
            </div>
        </div>
        <button type="button" class="btn btn-sm btn-primary apply-suggestion">
            採用
        </button>
    </div>
</script>
```

### 3. 前端智能分類功能 (smart-category.js)

```javascript
class SmartCategoryManager {
    constructor() {
        this.suggestions = [];
        this.isLearningMode = false;
        this.initializeEventListeners();
    }

    initializeEventListeners() {
        // 描述輸入時觸發智能分類
        $('#description').on('input debounce', this.triggerSmartCategory.bind(this));
        $('#merchant').on('input debounce', this.triggerSmartCategory.bind(this));
        $('#amount').on('input debounce', this.triggerSmartCategory.bind(this));
        
        // 智能分類按鈕
        $('#smartCategoryBtn').on('click', this.requestSmartCategory.bind(this));
        
        // 採用建議
        $(document).on('click', '.apply-suggestion', this.applySuggestion.bind(this));
        
        // 回饋按鈕
        $('#feedbackCorrect').on('click', () => this.submitFeedback(true));
        $('#feedbackIncorrect').on('click', () => this.submitFeedback(false));
        
        // 分類手動選擇時顯示回饋選項
        $('#categoryId').on('change', this.handleManualCategoryChange.bind(this));
    }

    // 防抖觸發智能分類
    triggerSmartCategory = debounce(() => {
        this.requestSmartCategory();
    }, 500);

    async requestSmartCategory() {
        const description = $('#description').val().trim();
        const amount = parseFloat($('#amount').val()) || 0;
        const merchant = $('#merchant').val().trim();

        if (!description && !merchant) {
            this.hideSuggestions();
            return;
        }

        this.showLoadingState();

        try {
            const response = await fetch('/api/smart-category/suggest', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    description: description,
                    amount: amount,
                    merchant: merchant
                })
            });

            if (response.ok) {
                this.suggestions = await response.json();
                this.displaySuggestions();
            }
        } catch (error) {
            console.error('智能分類請求失敗:', error);
            this.hideLoadingState();
        }
    }

    displaySuggestions() {
        const container = $('#categorySuggestions');
        const template = $('#suggestionCardTemplate').html();
        
        container.empty();
        
        if (this.suggestions.length === 0) {
            container.html(`
                <div class="text-muted text-center py-3">
                    <i class="fas fa-search"></i> 找不到合適的分類建議
                </div>
            `);
        } else {
            this.suggestions.forEach(suggestion => {
                const card = template
                    .replace(/\{\{categoryId\}\}/g, suggestion.categoryId)
                    .replace(/\{\{categoryName\}\}/g, suggestion.categoryName)
                    .replace(/\{\{iconClass\}\}/g, suggestion.iconClass)
                    .replace(/\{\{reason\}\}/g, suggestion.reason)
                    .replace(/\{\{confidence\}\}/g, suggestion.confidence)
                    .replace(/\{\{confidencePercent\}\}/g, Math.round(suggestion.confidence * 100));
                
                container.append(card);
            });
        }
        
        $('#smartCategorySection').show();
        this.hideLoadingState();
    }

    applySuggestion(event) {
        const card = $(event.target).closest('.suggestion-card');
        const categoryId = card.data('category-id');
        const confidence = card.data('confidence');
        
        // 設定分類選擇
        $('#categoryId').val(categoryId).trigger('change');
        
        // 顯示回饋選項
        $('#categoryFeedback').show();
        
        // 高亮選中的建議
        $('.suggestion-card').removeClass('selected');
        card.addClass('selected');
        
        // 記錄選擇供學習使用
        this.recordSuggestionUsage(categoryId, confidence);
        
        this.showSuccessMessage('已採用智能分類建議！');
    }

    async submitFeedback(isCorrect) {
        const categoryId = $('#categoryId').val();
        const description = $('#description').val();
        const amount = parseFloat($('#amount').val()) || 0;
        const merchant = $('#merchant').val();
        
        try {
            await fetch('/api/smart-category/feedback', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    categoryId: categoryId,
                    description: description,
                    amount: amount,
                    merchant: merchant,
                    isCorrect: isCorrect
                })
            });
            
            if (isCorrect) {
                this.showLearningAlert('感謝回饋！系統已記錄這個正確分類。');
            } else {
                this.showLearningAlert('感謝回饋！系統將調整分類建議邏輯。');
            }
            
            $('#categoryFeedback').hide();
            
        } catch (error) {
            console.error('回饋提交失敗:', error);
        }
    }

    showSuccessMessage(message) {
        // 顯示成功訊息
        const alert = $(`
            <div class="alert alert-success alert-dismissible fade show" role="alert">
                <i class="fas fa-check-circle"></i> ${message}
                <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
            </div>
        `);
        
        $('#smartCategorySection').prepend(alert);
        
        setTimeout(() => {
            alert.fadeOut();
        }, 3000);
    }

    showLearningAlert(message) {
        $('#learningAlert').find('strong').next().text(' ' + message);
        $('#learningAlert').show().delay(3000).fadeOut();
    }
}

// 工具函式
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// 初始化
$(document).ready(function() {
    window.smartCategoryManager = new SmartCategoryManager();
});
```

### 4. 資料模型定義 (SmartCategoryModels.cs)

```csharp
namespace Demo.Models
{
    public class CategorySuggestion
    {
        public string CategoryId { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
        public double Confidence { get; set; }
        public string Reason { get; set; } = string.Empty;
        public SuggestionSourceType SourceType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum SuggestionSourceType
    {
        RuleBased,      // 規則引擎
        KeywordBased,   // 關鍵字匹配
        HistoryBased,   // 歷史相似度
        MerchantBased,  // 商家匹配
        AmountBased,    // 金額範圍
        MachineLearning // 機器學習
    }

    public class CategoryRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public List<string> Keywords { get; set; } = new List<string>();
        public List<string> MerchantPatterns { get; set; } = new List<string>();
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public double MinConfidence { get; set; } = 0.7;
        public int Priority { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? LastUsed { get; set; }
        public int UsageCount { get; set; } = 0;
    }

    public class CategoryTrainingData
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public bool IsCorrect { get; set; } = true;
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public CategoryFeatures Features { get; set; } = new CategoryFeatures();
    }

    public class CategoryFeatures
    {
        public List<string> Keywords { get; set; } = new List<string>();
        public string MerchantType { get; set; } = string.Empty;
        public string AmountRange { get; set; } = string.Empty;
        public string TimePattern { get; set; } = string.Empty;
        public int TextLength { get; set; }
        public bool HasNumbers { get; set; }
        public string Language { get; set; } = "zh-TW";
        public List<string> ExtractedEntities { get; set; } = new List<string>();
    }

    public class MerchantMapping
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string MerchantName { get; set; } = string.Empty;
        public string StandardName { get; set; } = string.Empty;
        public string CategoryId { get; set; } = string.Empty;
        public string MerchantType { get; set; } = string.Empty;
        public List<string> Aliases { get; set; } = new List<string>();
        public double Confidence { get; set; } = 1.0;
        public bool IsVerified { get; set; } = false;
    }

    public class SmartCategoryRequest
    {
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
        public int MaxSuggestions { get; set; } = 5;
    }

    public class CategoryFeedback
    {
        public string CategoryId { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Merchant { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }
}
```

### 5. 分類學習服務 (CategoryLearningService.cs)

```csharp
namespace Demo.Services
{
    public class CategoryLearningService
    {
        private readonly string _trainingDataPath;
        private readonly string _rulesPath;

        public CategoryLearningService()
        {
            _trainingDataPath = Path.Combine("App_Data", "category-training.json");
            _rulesPath = Path.Combine("App_Data", "category-rules.json");
        }

        // 學習用戶回饋
        public async Task LearnFromFeedbackAsync(CategoryFeedback feedback)
        {
            var trainingData = await LoadTrainingDataAsync();
            
            // 新增訓練資料
            var newTrainingData = new CategoryTrainingData
            {
                Description = feedback.Description,
                Amount = feedback.Amount,
                Merchant = feedback.Merchant,
                CategoryId = feedback.CategoryId,
                IsCorrect = feedback.IsCorrect,
                UserId = feedback.UserId,
                CreatedAt = feedback.Timestamp
            };

            trainingData.Add(newTrainingData);
            await SaveTrainingDataAsync(trainingData);

            // 如果是正確的分類，更新規則權重
            if (feedback.IsCorrect)
            {
                await UpdateRuleWeightsAsync(feedback);
                await UpdateMerchantMappingAsync(feedback);
            }
        }

        // 自動生成新規則
        public async Task GenerateRulesFromTrainingDataAsync()
        {
            var trainingData = await LoadTrainingDataAsync();
            var correctData = trainingData.Where(t => t.IsCorrect).ToList();
            
            // 按分類群組
            var categoryGroups = correctData.GroupBy(t => t.CategoryId);
            
            foreach (var group in categoryGroups)
            {
                await GenerateRulesForCategoryAsync(group.Key, group.ToList());
            }
        }

        private async Task GenerateRulesForCategoryAsync(
            string categoryId, 
            List<CategoryTrainingData> categoryData)
        {
            // 分析關鍵字頻率
            var keywordFrequency = AnalyzeKeywordFrequency(categoryData);
            var commonMerchants = AnalyzeMerchantPatterns(categoryData);
            var amountRanges = AnalyzeAmountRanges(categoryData);

            // 生成新規則
            if (keywordFrequency.Count > 0)
            {
                var rule = new CategoryRule
                {
                    Name = $"自動生成規則 - {categoryId}",
                    CategoryId = categoryId,
                    Keywords = keywordFrequency.Take(10).Select(kv => kv.Key).ToList(),
                    MerchantPatterns = commonMerchants.Take(5).ToList(),
                    MinAmount = amountRanges.Min,
                    MaxAmount = amountRanges.Max,
                    MinConfidence = 0.6,
                    Priority = CalculateRulePriority(categoryData.Count)
                };

                await SaveRuleAsync(rule);
            }
        }

        // 分析關鍵字頻率
        private Dictionary<string, int> AnalyzeKeywordFrequency(
            List<CategoryTrainingData> data)
        {
            var keywords = new Dictionary<string, int>();
            
            foreach (var item in data)
            {
                var tokens = TokenizeText(item.Description);
                foreach (var token in tokens)
                {
                    keywords[token] = keywords.GetValueOrDefault(token, 0) + 1;
                }
            }
            
            return keywords
                .Where(kv => kv.Value >= 2) // 至少出現2次
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        // 更新模型準確度
        public async Task<ModelAccuracyReport> EvaluateModelAccuracyAsync()
        {
            var trainingData = await LoadTrainingDataAsync();
            var testData = trainingData.TakeLast(100).ToList(); // 取最後100筆作為測試
            
            int correctPredictions = 0;
            var categoryAccuracy = new Dictionary<string, (int correct, int total)>();
            
            foreach (var testItem in testData)
            {
                var suggestions = await GenerateSuggestionsAsync(
                    testItem.Description, 
                    testItem.Amount, 
                    testItem.Merchant);
                    
                var topSuggestion = suggestions.FirstOrDefault();
                if (topSuggestion?.CategoryId == testItem.CategoryId)
                {
                    correctPredictions++;
                }
                
                // 分類別統計
                var categoryKey = testItem.CategoryId;
                if (!categoryAccuracy.ContainsKey(categoryKey))
                {
                    categoryAccuracy[categoryKey] = (0, 0);
                }
                
                var isCorrect = topSuggestion?.CategoryId == testItem.CategoryId;
                categoryAccuracy[categoryKey] = (
                    categoryAccuracy[categoryKey].correct + (isCorrect ? 1 : 0),
                    categoryAccuracy[categoryKey].total + 1
                );
            }
            
            return new ModelAccuracyReport
            {
                OverallAccuracy = (double)correctPredictions / testData.Count,
                CategoryAccuracy = categoryAccuracy.ToDictionary(
                    kv => kv.Key,
                    kv => (double)kv.Value.correct / kv.Value.total
                ),
                TotalTestCases = testData.Count,
                CorrectPredictions = correctPredictions,
                EvaluationDate = DateTime.Now
            };
        }
    }

    public class ModelAccuracyReport
    {
        public double OverallAccuracy { get; set; }
        public Dictionary<string, double> CategoryAccuracy { get; set; } = new();
        public int TotalTestCases { get; set; }
        public int CorrectPredictions { get; set; }
        public DateTime EvaluationDate { get; set; }
    }
}
```

### 6. 樣式設計 (smart-category.css)

```css
/* 智能分類樣式 */
.smart-category-section {
    background: #f8f9fa;
    border-radius: 8px;
    padding: 1rem;
    border-left: 4px solid #007bff;
}

.category-suggestions {
    display: flex;
    flex-direction: column;
    gap: 0.75rem;
}

.suggestion-card {
    background: white;
    border: 1px solid #dee2e6;
    border-radius: 6px;
    padding: 0.75rem;
    display: flex;
    align-items: center;
    justify-content: space-between;
    cursor: pointer;
    transition: all 0.2s ease;
}

.suggestion-card:hover {
    border-color: #007bff;
    box-shadow: 0 2px 4px rgba(0,0,0,0.1);
}

.suggestion-card.selected {
    border-color: #28a745;
    background-color: #d4edda;
}

.suggestion-content {
    display: flex;
    align-items: center;
    flex: 1;
    gap: 0.75rem;
}

.suggestion-icon {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    background: #007bff;
    color: white;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 0.9rem;
}

.suggestion-info {
    flex: 1;
}

.suggestion-name {
    font-weight: 500;
    margin-bottom: 0.25rem;
}

.suggestion-reason {
    font-size: 0.8rem;
    color: #6c757d;
}

.suggestion-confidence {
    display: flex;
    flex-direction: column;
    align-items: center;
    min-width: 60px;
}

.confidence-bar {
    width: 40px;
    height: 4px;
    background: #e9ecef;
    border-radius: 2px;
    overflow: hidden;
    margin-bottom: 0.25rem;
}

.confidence-fill {
    height: 100%;
    background: linear-gradient(90deg, #ffc107 0%, #28a745 100%);
    transition: width 0.3s ease;
}

.apply-suggestion {
    margin-left: 1rem;
    min-width: 60px;
}

/* 回饋區域 */
.smart-category-feedback {
    background: #fff3cd;
    border: 1px solid #ffeaa7;
    border-radius: 4px;
    padding: 0.5rem;
}

.smart-category-feedback button {
    border-radius: 50%;
    width: 28px;
    height: 28px;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
}

/* 載入狀態 */
.smart-category-loading {
    text-align: center;
    padding: 1rem;
    color: #6c757d;
}

.smart-category-loading .spinner-border {
    width: 1.5rem;
    height: 1.5rem;
    margin-right: 0.5rem;
}

/* 學習提醒 */
#learningAlert {
    animation: slideInDown 0.3s ease;
}

@keyframes slideInDown {
    from {
        opacity: 0;
        transform: translateY(-10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

/* 響應式設計 */
@media (max-width: 768px) {
    .suggestion-card {
        flex-direction: column;
        align-items: flex-start;
        gap: 0.5rem;
    }
    
    .suggestion-content {
        width: 100%;
    }
    
    .apply-suggestion {
        align-self: flex-end;
        margin-left: 0;
    }
}

/* 深色模式支援 */
[data-bs-theme="dark"] .smart-category-section {
    background: #2d3748;
    border-left-color: #4299e1;
}

[data-bs-theme="dark"] .suggestion-card {
    background: #4a5568;
    border-color: #2d3748;
}

[data-bs-theme="dark"] .suggestion-card:hover {
    border-color: #4299e1;
}

[data-bs-theme="dark"] .suggestion-card.selected {
    background-color: #2d5a3d;
    border-color: #38a169;
}
```

## 🎯 開發階段規劃

### Phase 1: 基礎智能分類 (2-3 週)
- [x] 建立基本的智能分類服務架構
- [x] 實作關鍵字匹配和規則引擎
- [x] 整合到記帳頁面
- [x] 基本的分類建議顯示

### Phase 2: 學習和優化 (2-3 週)
- [ ] 實作用戶回饋學習機制
- [ ] 建立訓練資料收集系統
- [ ] 實作歷史相似度匹配
- [ ] 商家自動對應功能

### Phase 3: 進階功能 (2-3 週)
- [ ] 模型準確度統計和報告
- [ ] 自動規則生成功能
- [ ] 批次分類功能
- [ ] 管理介面和規則編輯

## 📝 測試規格

### 功能測試
1. 智能分類推薦準確度
2. 用戶回饋學習效果
3. 規則引擎邏輯正確性
4. 批次處理效能
5. 邊界條件處理

### 使用者體驗測試
1. 分類建議響應時間
2. 介面操作直觀性
3. 回饋機制易用性
4. 行動裝置體驗

## 🚀 部署注意事項

1. 初始化預設分類規則和關鍵字字典
2. 確保訓練資料檔案讀寫權限
3. 設定合理的快取策略提升效能
4. 監控分類準確度和使用統計

## 📚 相關文件
- [TF-IDF 演算法說明](https://zh.wikipedia.org/wiki/Tf-idf)
- [餘弦相似度計算](https://zh.wikipedia.org/wiki/餘弦相似性)
- [機器學習最佳實務](https://docs.microsoft.com/zh-tw/azure/machine-learning/)
