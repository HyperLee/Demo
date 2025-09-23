# 記帳系統語音輸入功能 Phase 2: 使用者體驗優化開發規格書

## 階段概述
**優先級**: 中  
**預估時程**: 1週  
**前置條件**: Phase 1 核心解析功能已完成  
**目標**: 優化語音輸入的使用者體驗，提供智能的解析結果處理和用戶互動機制

## 開發目標

### 主要目標
1. 建立分欄位信心度機制和視覺化展示
2. 實作漸進式解析流程，處理部分解析成功情況
3. 優化未解析內容的處理和引導
4. 建立用戶確認和修正流程
5. 提升整體語音輸入的易用性和可靠性

### 成功指標
- [ ] 分欄位信心度準確率 > 85%
- [ ] 部分解析成功率 > 90%（至少解析出3個主要欄位）
- [ ] 用戶確認流程 < 5步驟
- [ ] 用戶滿意度評分 > 4.0/5.0

## 技術實作細節

### 1. 信心度機制增強

#### 1.1 分欄位信心度計算優化
```csharp
/// <summary>
/// 信心度計算器 (Phase 2 增強版)
/// </summary>
public class FieldConfidenceCalculator
{
    private readonly ILogger<FieldConfidenceCalculator> _logger;
    
    public FieldConfidenceCalculator(ILogger<FieldConfidenceCalculator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 計算日期解析信心度
    /// </summary>
    public double CalculateDateConfidence(string originalText, DateTime? parsedDate, string matchedPattern)
    {
        try
        {
            if (!parsedDate.HasValue) return 0.0;

            double baseConfidence = 0.0;
            
            // 根據匹配模式給予基礎信心度
            switch (matchedPattern.ToLower())
            {
                case "relative": // 今天、昨天等
                    baseConfidence = 0.95;
                    break;
                case "absolute_full": // 2023年10月1日
                    baseConfidence = 0.90;
                    break;
                case "month_day": // 10月1日
                    baseConfidence = 0.75;
                    break;
                case "chinese_number": // 十月一日
                    baseConfidence = 0.65;
                    break;
                default:
                    baseConfidence = 0.50;
                    break;
            }

            // 日期合理性檢查
            var reasonabilityScore = CheckDateReasonability(parsedDate.Value);
            
            // 上下文一致性檢查
            var contextScore = CheckDateContextConsistency(originalText, parsedDate.Value);
            
            // 最終信心度 = 基礎信心度 * 合理性得分 * 上下文得分
            var finalConfidence = baseConfidence * reasonabilityScore * contextScore;
            
            return Math.Min(Math.Max(finalConfidence, 0.0), 1.0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "計算日期信心度時發生錯誤");
            return 0.0;
        }
    }

    /// <summary>
    /// 計算金額解析信心度
    /// </summary>
    public double CalculateAmountConfidence(string originalText, decimal? parsedAmount)
    {
        try
        {
            if (!parsedAmount.HasValue) return 0.0;

            double baseConfidence = 0.95; // 數字解析通常很準確
            
            // 金額合理性檢查
            var reasonabilityScore = CheckAmountReasonability(parsedAmount.Value);
            
            // 檢查是否有明確的金額關鍵字
            var keywordScore = CheckAmountKeywords(originalText);
            
            // 檢查數字格式的一致性
            var formatScore = CheckAmountFormat(originalText, parsedAmount.Value);
            
            var finalConfidence = baseConfidence * reasonabilityScore * keywordScore * formatScore;
            
            return Math.Min(Math.Max(finalConfidence, 0.0), 1.0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "計算金額信心度時發生錯誤");
            return 0.0;
        }
    }

    /// <summary>
    /// 計算分類解析信心度
    /// </summary>
    public double CalculateCategoryConfidence(string originalText, string? category, string? merchantName, string? description)
    {
        try
        {
            if (string.IsNullOrEmpty(category)) return 0.0;

            double baseConfidence = 0.0;
            
            // 檢查是否有直接的分類關鍵字匹配
            if (CheckDirectCategoryMatch(originalText, category))
            {
                baseConfidence = 0.90;
            }
            // 檢查是否透過商家名稱推斷分類
            else if (!string.IsNullOrEmpty(merchantName) && CheckMerchantCategoryMapping(merchantName, category))
            {
                baseConfidence = 0.75;
            }
            // 檢查是否透過描述關鍵字推斷分類
            else if (!string.IsNullOrEmpty(description) && CheckDescriptionCategoryMapping(description, category))
            {
                baseConfidence = 0.60;
            }
            else
            {
                baseConfidence = 0.30; // 預設或猜測
            }

            // 分類一致性檢查
            var consistencyScore = CheckCategoryConsistency(merchantName, description, category);
            
            var finalConfidence = baseConfidence * consistencyScore;
            
            return Math.Min(Math.Max(finalConfidence, 0.0), 1.0);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "計算分類信心度時發生錯誤");
            return 0.0;
        }
    }

    // === 私有輔助方法 ===

    private double CheckDateReasonability(DateTime date)
    {
        // 檢查日期是否在合理範圍內
        var now = DateTime.Now;
        var daysDiff = Math.Abs((date - now).TotalDays);
        
        if (daysDiff <= 7) return 1.0;      // 一週內
        if (daysDiff <= 30) return 0.9;     // 一個月內
        if (daysDiff <= 365) return 0.7;    // 一年內
        return 0.3;                         // 超過一年
    }

    private double CheckDateContextConsistency(string text, DateTime date)
    {
        // 檢查日期與文字中的時間線索是否一致
        var now = DateTime.Now;
        var isToday = date.Date == now.Date;
        var isYesterday = date.Date == now.Date.AddDays(-1);
        var isPast = date < now;
        
        // 檢查過去式和未來式動詞
        if (text.Contains("昨天") && isYesterday) return 1.0;
        if (text.Contains("今天") && isToday) return 1.0;
        if ((text.Contains("花了") || text.Contains("買了")) && isPast) return 1.0;
        if (text.Contains("將") && !isPast) return 1.0;
        
        return 0.8; // 沒有明確線索時的預設值
    }

    private double CheckAmountReasonability(decimal amount)
    {
        // 檢查金額是否在合理範圍內
        if (amount <= 0) return 0.0;
        if (amount <= 100) return 1.0;      // 小額消費
        if (amount <= 1000) return 0.95;    // 中等消費
        if (amount <= 10000) return 0.8;    // 較大消費
        if (amount <= 100000) return 0.6;   // 大額消費
        return 0.3;                         // 超大金額，可能有誤
    }

    private double CheckAmountKeywords(string text)
    {
        var amountKeywords = new[] { "元", "塊", "錢", "費", "花", "付", "cost", "$" };
        var keywordCount = amountKeywords.Count(keyword => text.Contains(keyword));
        
        return Math.Min(0.5 + (keywordCount * 0.2), 1.0);
    }

    private double CheckAmountFormat(string text, decimal amount)
    {
        // 檢查提取的數字格式是否一致
        var amountStr = amount.ToString();
        return text.Contains(amountStr) ? 1.0 : 0.8;
    }

    private bool CheckDirectCategoryMatch(string text, string category)
    {
        // 檢查文字中是否直接包含分類名稱
        return text.Contains(category, StringComparison.OrdinalIgnoreCase);
    }

    private bool CheckMerchantCategoryMapping(string merchantName, string category)
    {
        // 檢查商家與分類的對應關係
        var merchantCategoryMap = new Dictionary<string, string[]>
        {
            { "星巴克", new[] { "餐飲美食", "咖啡茶飲" } },
            { "7-11", new[] { "餐飲美食", "日用品", "交通運輸" } },
            { "麥當勞", new[] { "餐飲美食" } },
            { "加油站", new[] { "交通運輸" } }
        };

        return merchantCategoryMap.ContainsKey(merchantName) && 
               merchantCategoryMap[merchantName].Contains(category);
    }

    private bool CheckDescriptionCategoryMapping(string description, string category)
    {
        // 檢查描述與分類的對應關係
        var descriptionKeywords = new Dictionary<string, string[]>
        {
            { "餐飲美食", new[] { "吃", "喝", "咖啡", "午餐", "晚餐", "早餐", "飲料" } },
            { "交通運輸", new[] { "加油", "停車", "計程車", "公車", "捷運" } },
            { "娛樂休閒", new[] { "電影", "唱歌", "遊戲", "旅遊" } },
            { "服飾美容", new[] { "衣服", "鞋子", "化妝", "髮型" } }
        };

        if (descriptionKeywords.ContainsKey(category))
        {
            return descriptionKeywords[category].Any(keyword => 
                description.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    private double CheckCategoryConsistency(string? merchantName, string? description, string category)
    {
        // 檢查分類與其他資訊的一致性
        double consistencyScore = 1.0;

        // 如果有商家資訊，檢查一致性
        if (!string.IsNullOrEmpty(merchantName))
        {
            if (CheckMerchantCategoryMapping(merchantName, category))
            {
                consistencyScore *= 1.0;
            }
            else
            {
                consistencyScore *= 0.7; // 不一致時降低信心度
            }
        }

        // 如果有描述資訊，檢查一致性
        if (!string.IsNullOrEmpty(description))
        {
            if (CheckDescriptionCategoryMapping(description, category))
            {
                consistencyScore *= 1.0;
            }
            else
            {
                consistencyScore *= 0.8; // 輕微不一致
            }
        }

        return consistencyScore;
    }
}
```

#### 1.2 整體信心度計算增強
```csharp
/// <summary>
/// 計算整體解析信心度 (Phase 2 增強版)
/// </summary>
private double CalculateOverallConfidence(VoiceParseResult result)
{
    try
    {
        var fieldConfidences = result.FieldConfidence;
        var fieldWeights = new Dictionary<string, double>
        {
            { "Amount", 0.25 },        // 金額權重最高
            { "Type", 0.20 },          // 收支類型次之
            { "Category", 0.20 },      // 分類重要
            { "Date", 0.15 },          // 日期重要
            { "Description", 0.10 },   // 描述中等
            { "PaymentMethod", 0.05 }, // 付款方式較低
            { "MerchantName", 0.03 },  // 商家名稱較低
            { "SubCategory", 0.01 },   // 細分類最低
            { "Note", 0.01 }           // 備註最低
        };

        double weightedSum = 0.0;
        double totalWeight = 0.0;

        // 計算加權平均信心度
        foreach (var field in fieldConfidences)
        {
            if (fieldWeights.ContainsKey(field.Key))
            {
                var weight = fieldWeights[field.Key];
                weightedSum += field.Value * weight;
                totalWeight += weight;
            }
        }

        if (totalWeight == 0) return 0.0;

        var baseConfidence = weightedSum / totalWeight;

        // 完整性加成：解析出的欄位越多，整體信心度越高
        var completenessBonus = CalculateCompletenessBonus(result);
        
        // 一致性加成：各欄位間的一致性
        var consistencyBonus = CalculateConsistencyBonus(result);

        var finalConfidence = baseConfidence * (1 + completenessBonus + consistencyBonus);
        
        return Math.Min(Math.Max(finalConfidence, 0.0), 1.0);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "計算整體信心度時發生錯誤");
        return 0.0;
    }
}

/// <summary>
/// 計算完整性加成
/// </summary>
private double CalculateCompletenessBonus(VoiceParseResult result)
{
    var coreFields = new[] { "Amount", "Type", "Category", "Description" };
    var parsedCoreFields = coreFields.Count(field => 
        result.FieldConfidence.ContainsKey(field) && result.FieldConfidence[field] > 0.3);
    
    // 核心欄位解析比例
    var coreCompleteness = (double)parsedCoreFields / coreFields.Length;
    
    return coreCompleteness * 0.1; // 最多10%加成
}

/// <summary>
/// 計算一致性加成
/// </summary>
private double CalculateConsistencyBonus(VoiceParseResult result)
{
    double consistencyScore = 0.0;
    
    // 檢查商家與分類的一致性
    if (!string.IsNullOrEmpty(result.MerchantName) && !string.IsNullOrEmpty(result.Category))
    {
        // 這裡可以實作商家與分類的一致性檢查邏輯
        consistencyScore += 0.05;
    }
    
    // 檢查描述與分類的一致性
    if (!string.IsNullOrEmpty(result.Description) && !string.IsNullOrEmpty(result.Category))
    {
        // 這裡可以實作描述與分類的一致性檢查邏輯
        consistencyScore += 0.05;
    }
    
    return Math.Min(consistencyScore, 0.1); // 最多10%加成
}
```

### 2. 漸進式解析流程

#### 2.1 解析狀態管理
```csharp
/// <summary>
/// 解析狀態枚舉
/// </summary>
public enum ParseState
{
    NotStarted,     // 尚未開始
    Parsing,        // 解析中
    Completed,      // 完全解析完成
    PartialSuccess, // 部分成功
    Failed,         // 解析失敗
    RequiresInput   // 需要用戶輸入
}

/// <summary>
/// 漸進式解析管理器
/// </summary>
public class ProgressiveParseManager
{
    private readonly ILogger<ProgressiveParseManager> _logger;
    
    public ProgressiveParseManager(ILogger<ProgressiveParseManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 評估解析狀態
    /// </summary>
    public ParseState EvaluateParseState(VoiceParseResult result)
    {
        try
        {
            // 檢查核心欄位
            var coreFields = new[] { "Amount", "Type", "Category" };
            var parsedCoreFields = coreFields.Count(field => 
                result.FieldConfidence.ContainsKey(field) && 
                result.FieldConfidence[field] > 0.4);

            // 檢查所有欄位
            var allFields = new[] { "Amount", "Type", "Category", "Date", "Description", 
                                  "PaymentMethod", "MerchantName", "SubCategory", "Note" };
            var parsedAllFields = allFields.Count(field => 
                result.FieldConfidence.ContainsKey(field) && 
                result.FieldConfidence[field] > 0.3);

            // 判斷解析狀態
            if (parsedCoreFields == coreFields.Length && parsedAllFields >= 6)
            {
                return ParseState.Completed; // 完全成功
            }
            else if (parsedCoreFields >= 2)
            {
                return ParseState.PartialSuccess; // 部分成功
            }
            else if (parsedCoreFields >= 1)
            {
                return ParseState.RequiresInput; // 需要輸入
            }
            else
            {
                return ParseState.Failed; // 失敗
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "評估解析狀態時發生錯誤");
            return ParseState.Failed;
        }
    }

    /// <summary>
    /// 生成缺失欄位提示
    /// </summary>
    public List<MissingFieldHint> GenerateMissingFieldHints(VoiceParseResult result)
    {
        var hints = new List<MissingFieldHint>();

        try
        {
            // 檢查缺失的核心欄位
            if (!result.FieldConfidence.ContainsKey("Amount") || result.FieldConfidence["Amount"] < 0.4)
            {
                hints.Add(new MissingFieldHint
                {
                    FieldName = "Amount",
                    DisplayName = "金額",
                    Suggestion = "請補充金額資訊，例如：「100元」",
                    Priority = HintPriority.High,
                    Icon = "fas fa-coins"
                });
            }

            if (!result.FieldConfidence.ContainsKey("Category") || result.FieldConfidence["Category"] < 0.4)
            {
                hints.Add(new MissingFieldHint
                {
                    FieldName = "Category",
                    DisplayName = "分類",
                    Suggestion = "請補充分類資訊，例如：「餐飲」、「交通」",
                    Priority = HintPriority.High,
                    Icon = "fas fa-folder"
                });
            }

            // 檢查缺失的次要欄位
            if (!result.FieldConfidence.ContainsKey("Date") || result.FieldConfidence["Date"] < 0.4)
            {
                hints.Add(new MissingFieldHint
                {
                    FieldName = "Date",
                    DisplayName = "日期",
                    Suggestion = "請補充日期資訊，例如：「今天」、「昨天」",
                    Priority = HintPriority.Medium,
                    Icon = "fas fa-calendar"
                });
            }

            if (!result.FieldConfidence.ContainsKey("PaymentMethod") || result.FieldConfidence["PaymentMethod"] < 0.4)
            {
                hints.Add(new MissingFieldHint
                {
                    FieldName = "PaymentMethod",
                    DisplayName = "付款方式",
                    Suggestion = "請補充付款方式，例如：「現金」、「信用卡」",
                    Priority = HintPriority.Low,
                    Icon = "fas fa-credit-card"
                });
            }

            return hints.OrderByDescending(h => h.Priority).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "生成缺失欄位提示時發生錯誤");
            return new List<MissingFieldHint>();
        }
    }
}

/// <summary>
/// 缺失欄位提示模型
/// </summary>
public class MissingFieldHint
{
    public string FieldName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Suggestion { get; set; } = string.Empty;
    public HintPriority Priority { get; set; }
    public string Icon { get; set; } = string.Empty;
}

/// <summary>
/// 提示優先級
/// </summary>
public enum HintPriority
{
    Low = 1,
    Medium = 2,
    High = 3
}
```

#### 2.2 解析結果處理 API 增強
```csharp
/// <summary>
/// 解析語音輸入 (Phase 2 增強版)
/// </summary>
public async Task<IActionResult> OnPostParseVoiceInputAsync([FromBody] VoiceParseRequest request)
{
    try
    {
        if (string.IsNullOrWhiteSpace(request.VoiceText))
        {
            return new JsonResult(new VoiceParseResponse
            {
                IsSuccess = false,
                ErrorMessage = "語音文字不可為空",
                ParseState = ParseState.Failed
            });
        }

        // Phase 1 的基本解析
        var parseResult = await ParseVoiceTextAsync(request.VoiceText);
        
        // Phase 2 的狀態評估和提示生成
        var parseState = _progressiveParseManager.EvaluateParseState(parseResult);
        var missingFieldHints = _progressiveParseManager.GenerateMissingFieldHints(parseResult);

        // 根據解析狀態決定回應
        var response = new VoiceParseResponse
        {
            IsSuccess = parseState != ParseState.Failed,
            ParseResult = parseResult,
            ParseState = parseState,
            MissingFieldHints = missingFieldHints,
            NextStepSuggestion = GenerateNextStepSuggestion(parseState, missingFieldHints)
        };

        return new JsonResult(response);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "解析語音輸入時發生錯誤");
        return new JsonResult(new VoiceParseResponse
        {
            IsSuccess = false,
            ErrorMessage = "語音解析失敗，請重試",
            ParseState = ParseState.Failed
        });
    }
}

/// <summary>
/// 語音解析回應模型 (Phase 2)
/// </summary>
public class VoiceParseResponse
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public VoiceParseResult? ParseResult { get; set; }
    public ParseState ParseState { get; set; }
    public List<MissingFieldHint> MissingFieldHints { get; set; } = new();
    public string? NextStepSuggestion { get; set; }
}

/// <summary>
/// 生成下一步建議
/// </summary>
private string GenerateNextStepSuggestion(ParseState parseState, List<MissingFieldHint> hints)
{
    return parseState switch
    {
        ParseState.Completed => "解析完成！您可以直接套用到表單或進行微調。",
        ParseState.PartialSuccess => $"已解析部分資訊。建議補充：{string.Join("、", hints.Take(2).Select(h => h.DisplayName))}",
        ParseState.RequiresInput => $"需要更多資訊。請補充：{string.Join("、", hints.Where(h => h.Priority == HintPriority.High).Select(h => h.DisplayName))}",
        ParseState.Failed => "解析失敗，請重新描述或手動填入資訊。",
        _ => "請檢查解析結果並確認資訊是否正確。"
    };
}
```

### 3. 前端使用者體驗優化

#### 3.1 漸進式解析 UI 增強
```html
<!-- 語音解析狀態指示器 -->
<div id="voiceParseProgress" class="mt-3 d-none">
    <div class="card border-info">
        <div class="card-header bg-info text-white">
            <div class="d-flex justify-content-between align-items-center">
                <h6 class="mb-0">
                    <i class="fas fa-microphone"></i> 語音解析進度
                </h6>
                <span id="parseStateText" class="badge bg-light text-dark">解析中...</span>
            </div>
        </div>
        <div class="card-body">
            <!-- 整體進度條 -->
            <div class="mb-3">
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <small class="text-muted">整體完成度</small>
                    <small id="overallProgressText" class="text-muted">0%</small>
                </div>
                <div class="progress" style="height: 10px;">
                    <div id="overallProgressBar" class="progress-bar progress-bar-striped progress-bar-animated" 
                         role="progressbar" style="width: 0%" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100">
                    </div>
                </div>
            </div>

            <!-- 欄位解析狀態列表 -->
            <div id="fieldParseStatus" class="row">
                <!-- 動態生成的欄位狀態 -->
            </div>
        </div>
    </div>
</div>

<!-- 缺失欄位提示區域 -->
<div id="missingFieldsAlert" class="mt-3 d-none">
    <div class="alert alert-warning">
        <div class="d-flex align-items-start">
            <i class="fas fa-exclamation-triangle me-2 mt-1"></i>
            <div class="flex-grow-1">
                <h6 class="alert-heading mb-2">需要補充資訊</h6>
                <div id="missingFieldsList">
                    <!-- 動態生成的缺失欄位列表 -->
                </div>
                <div class="mt-3">
                    <button type="button" class="btn btn-sm btn-outline-warning" id="quickFillBtn">
                        <i class="fas fa-magic"></i> 智能填補
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-secondary" id="continueWithPartialBtn">
                        <i class="fas fa-forward"></i> 繼續使用部分結果
                    </button>
                    <button type="button" class="btn btn-sm btn-outline-primary" id="rerecordBtn">
                        <i class="fas fa-microphone"></i> 重新錄音
                    </button>
                </div>
            </div>
        </div>
    </div>
</div>

<!-- 信心度詳細檢視 -->
<div id="confidenceDetails" class="mt-3 d-none">
    <div class="card border-secondary">
        <div class="card-header">
            <h6 class="mb-0">
                <i class="fas fa-chart-bar"></i> 解析信心度詳情
                <small class="text-muted">- 點選查看各欄位解析品質</small>
            </h6>
        </div>
        <div class="card-body">
            <div id="confidenceChart" class="mb-3">
                <!-- 信心度圖表 -->
            </div>
            <div class="row" id="confidenceBreakdown">
                <!-- 分欄位信心度詳情 -->
            </div>
        </div>
    </div>
</div>
```

#### 3.2 JavaScript UX 增強
```javascript
/**
 * Phase 2 使用者體驗增強類
 */
class VoiceInputUXEnhancer {
    constructor(voiceInputInstance) {
        this.voiceInput = voiceInputInstance;
        this.parseState = 'NotStarted';
        this.currentParseResult = null;
        this.progressAnimation = null;
    }

    /**
     * 處理解析結果並更新UI (Phase 2)
     */
    handleParseResult(response) {
        try {
            this.currentParseResult = response.ParseResult;
            this.parseState = response.ParseState;

            // 顯示解析進度
            this.showParseProgress(response);

            // 根據解析狀態採取不同的處理策略
            switch (response.ParseState) {
                case 'Completed':
                    this.handleCompleteSuccess(response);
                    break;
                case 'PartialSuccess':
                    this.handlePartialSuccess(response);
                    break;
                case 'RequiresInput':
                    this.handleRequiresInput(response);
                    break;
                case 'Failed':
                    this.handleParseFailed(response);
                    break;
                default:
                    this.handleUnknownState(response);
                    break;
            }
        } catch (error) {
            console.error('處理解析結果時發生錯誤:', error);
            this.showError('處理解析結果時發生錯誤');
        }
    }

    /**
     * 顯示解析進度
     */
    showParseProgress(response) {
        const progressDiv = document.getElementById('voiceParseProgress');
        const stateText = document.getElementById('parseStateText');
        
        // 顯示進度區域
        progressDiv.classList.remove('d-none');
        
        // 更新狀態文字
        const stateDisplayMap = {
            'Completed': '解析完成',
            'PartialSuccess': '部分成功', 
            'RequiresInput': '需要補充',
            'Failed': '解析失敗'
        };
        
        stateText.textContent = stateDisplayMap[response.ParseState] || '未知狀態';
        stateText.className = `badge ${this.getStateColorClass(response.ParseState)}`;

        // 更新整體進度
        this.updateOverallProgress(response.ParseResult);
        
        // 更新欄位狀態
        this.updateFieldStatus(response.ParseResult);
    }

    /**
     * 更新整體進度
     */
    updateOverallProgress(parseResult) {
        const progressBar = document.getElementById('overallProgressBar');
        const progressText = document.getElementById('overallProgressText');
        
        const progress = Math.round(parseResult.ParseConfidence * 100);
        
        // 動畫效果
        if (this.progressAnimation) {
            this.progressAnimation.cancel();
        }
        
        this.progressAnimation = progressBar.animate([
            { width: '0%' },
            { width: `${progress}%` }
        ], {
            duration: 1000,
            easing: 'ease-out',
            fill: 'forwards'
        });
        
        progressBar.style.width = `${progress}%`;
        progressBar.setAttribute('aria-valuenow', progress);
        progressText.textContent = `${progress}%`;
        
        // 根據進度設定顏色
        progressBar.className = 'progress-bar progress-bar-striped';
        if (progress >= 80) {
            progressBar.classList.add('bg-success');
        } else if (progress >= 60) {
            progressBar.classList.add('bg-warning');
        } else {
            progressBar.classList.add('bg-danger');
        }
    }

    /**
     * 更新欄位解析狀態
     */
    updateFieldStatus(parseResult) {
        const statusContainer = document.getElementById('fieldParseStatus');
        statusContainer.innerHTML = '';

        const fields = [
            { key: 'Date', name: '日期', icon: 'fas fa-calendar' },
            { key: 'Type', name: '類型', icon: 'fas fa-exchange-alt' },
            { key: 'Amount', name: '金額', icon: 'fas fa-coins' },
            { key: 'Category', name: '分類', icon: 'fas fa-folder' },
            { key: 'PaymentMethod', name: '付款', icon: 'fas fa-credit-card' },
            { key: 'Description', name: '描述', icon: 'fas fa-edit' }
        ];

        fields.forEach(field => {
            const confidence = parseResult.FieldConfidence[field.key] || 0;
            const status = this.getFieldStatus(confidence);
            
            const fieldElement = document.createElement('div');
            fieldElement.className = 'col-md-4 mb-2';
            fieldElement.innerHTML = `
                <div class="d-flex align-items-center">
                    <i class="${field.icon} me-2 ${status.color}"></i>
                    <div class="flex-grow-1">
                        <small class="text-muted">${field.name}</small>
                        <div class="progress" style="height: 4px;">
                            <div class="progress-bar ${status.progressColor}" 
                                 style="width: ${Math.round(confidence * 100)}%"></div>
                        </div>
                    </div>
                    <small class="ms-2 ${status.color}">${status.text}</small>
                </div>
            `;
            
            statusContainer.appendChild(fieldElement);
        });
    }

    /**
     * 處理完全成功的情況
     */
    handleCompleteSuccess(response) {
        // 隱藏缺失欄位提示
        document.getElementById('missingFieldsAlert').classList.add('d-none');
        
        // 顯示成功訊息
        this.showSuccess('語音解析完成！所有主要資訊都已成功識別。');
        
        // 自動套用到表單（延遲2秒）
        setTimeout(() => {
            this.voiceInput.applyParseResultToForm(response.ParseResult);
        }, 2000);
    }

    /**
     * 處理部分成功的情況
     */
    handlePartialSuccess(response) {
        // 顯示缺失欄位提示
        this.showMissingFieldsAlert(response.MissingFieldHints, response.NextStepSuggestion);
        
        // 顯示部分成功訊息
        this.showWarning(`部分解析成功。${response.NextStepSuggestion}`);
    }

    /**
     * 處理需要輸入的情況
     */
    handleRequiresInput(response) {
        // 顯示缺失欄位提示
        this.showMissingFieldsAlert(response.MissingFieldHints, response.NextStepSuggestion);
        
        // 顯示需要輸入訊息
        this.showInfo(`需要更多資訊。${response.NextStepSuggestion}`);
    }

    /**
     * 處理解析失敗的情況
     */
    handleParseFailed(response) {
        // 隱藏進度區域
        document.getElementById('voiceParseProgress').classList.add('d-none');
        
        // 顯示失敗訊息
        this.showError(response.ErrorMessage || '語音解析失敗，請重新嘗試或手動填入。');
        
        // 提供重新錄音選項
        this.showRetryOptions();
    }

    /**
     * 顯示缺失欄位提示
     */
    showMissingFieldsAlert(hints, suggestion) {
        const alertDiv = document.getElementById('missingFieldsAlert');
        const hintsList = document.getElementById('missingFieldsList');
        
        hintsList.innerHTML = '';
        
        hints.forEach(hint => {
            const hintElement = document.createElement('div');
            hintElement.className = 'mb-2';
            hintElement.innerHTML = `
                <div class="d-flex align-items-start">
                    <i class="${hint.Icon} me-2 mt-1 ${this.getPriorityColor(hint.Priority)}"></i>
                    <div>
                        <strong>${hint.DisplayName}</strong>
                        <br>
                        <small class="text-muted">${hint.Suggestion}</small>
                    </div>
                </div>
            `;
            hintsList.appendChild(hintElement);
        });
        
        alertDiv.classList.remove('d-none');
        
        // 綁定按鈕事件
        this.bindMissingFieldButtons();
    }

    /**
     * 綁定缺失欄位按鈕事件
     */
    bindMissingFieldButtons() {
        // 智能填補按鈕
        document.getElementById('quickFillBtn').onclick = () => {
            this.performQuickFill();
        };
        
        // 繼續使用部分結果按鈕
        document.getElementById('continueWithPartialBtn').onclick = () => {
            this.continueWithPartialResult();
        };
        
        // 重新錄音按鈕
        document.getElementById('rerecordBtn').onclick = () => {
            this.voiceInput.resetAndRetry();
        };
    }

    /**
     * 執行智能填補
     */
    performQuickFill() {
        try {
            // 使用預設值或智能推測填補缺失欄位
            const result = { ...this.currentParseResult };
            
            // 日期預設為今天
            if (!result.Date) {
                result.Date = new Date().toISOString().split('T')[0];
                result.FieldConfidence.Date = 0.8;
            }
            
            // 付款方式預設為現金
            if (!result.PaymentMethod) {
                result.PaymentMethod = '現金';
                result.FieldConfidence.PaymentMethod = 0.6;
            }
            
            // 套用增強後的結果
            this.voiceInput.applyParseResultToForm(result);
            
            this.showSuccess('已使用智能填補完成缺失欄位！');
        } catch (error) {
            console.error('智能填補時發生錯誤:', error);
            this.showError('智能填補失敗，請手動填入');
        }
    }

    /**
     * 繼續使用部分結果
     */
    continueWithPartialResult() {
        try {
            this.voiceInput.applyParseResultToForm(this.currentParseResult);
            this.showInfo('已套用部分解析結果，請手動完成剩餘欄位');
        } catch (error) {
            console.error('套用部分結果時發生錯誤:', error);
            this.showError('套用結果失敗');
        }
    }

    // === 輔助方法 ===

    getStateColorClass(state) {
        const colorMap = {
            'Completed': 'bg-success text-white',
            'PartialSuccess': 'bg-warning text-dark',
            'RequiresInput': 'bg-info text-white',
            'Failed': 'bg-danger text-white'
        };
        return colorMap[state] || 'bg-secondary text-white';
    }

    getFieldStatus(confidence) {
        if (confidence >= 0.7) {
            return {
                text: '✓',
                color: 'text-success',
                progressColor: 'bg-success'
            };
        } else if (confidence >= 0.4) {
            return {
                text: '?',
                color: 'text-warning',
                progressColor: 'bg-warning'
            };
        } else {
            return {
                text: '✗',
                color: 'text-danger',
                progressColor: 'bg-danger'
            };
        }
    }

    getPriorityColor(priority) {
        const colorMap = {
            'High': 'text-danger',
            'Medium': 'text-warning',
            'Low': 'text-info'
        };
        return colorMap[priority] || 'text-secondary';
    }

    showSuccess(message) {
        // 實作成功訊息顯示
        console.log('Success:', message);
    }

    showWarning(message) {
        // 實作警告訊息顯示
        console.log('Warning:', message);
    }

    showInfo(message) {
        // 實作資訊訊息顯示
        console.log('Info:', message);
    }

    showError(message) {
        // 實作錯誤訊息顯示
        console.log('Error:', message);
    }

    showRetryOptions() {
        // 實作重試選項顯示
        console.log('Showing retry options');
    }
}
```

## 測試規劃

### 信心度測試案例
```
測試案例 1: 高信心度完整解析
輸入: "昨天在星巴克用信用卡花了150元買咖啡"
預期:
- 所有主要欄位信心度 > 0.7
- 整體信心度 > 0.8
- 狀態: Completed

測試案例 2: 中等信心度部分解析
輸入: "花了50元吃早餐"
預期:
- Amount: 0.9, Type: 0.8, Category: 0.6
- 缺失: Date, PaymentMethod, MerchantName
- 狀態: PartialSuccess

測試案例 3: 低信心度需要輸入
輸入: "買了東西"
預期:
- 大部分欄位信心度 < 0.4
- 狀態: RequiresInput
- 提供明確的缺失欄位提示
```

### 使用者體驗測試
1. **漸進式解析流程測試**
   - 驗證不同解析狀態的UI表現
   - 測試缺失欄位提示的準確性
   - 驗證智能填補功能

2. **信心度視覺化測試**
   - 測試進度條動畫效果
   - 驗證欄位狀態指示器
   - 測試顏色編碼的一致性

3. **互動流程測試**
   - 測試各種操作按鈕功能
   - 驗證錯誤處理和回復機制
   - 測試用戶確認流程

## 交付標準

### 功能完整性
- [ ] 分欄位信心度機制實作完成
- [ ] 漸進式解析流程正常運作
- [ ] 缺失欄位提示準確有效
- [ ] 智能填補功能可用

### 使用者體驗
- [ ] 解析進度視覺化清晰
- [ ] 操作流程直覺易懂
- [ ] 錯誤處理友善適當
- [ ] 響應時間 < 1秒

### 品質指標
- [ ] 信心度計算準確率 > 85%
- [ ] 部分解析成功率 > 90%
- [ ] 用戶滿意度 > 4.0/5.0
- [ ] 無嚴重UI/UX問題

## 後續階段準備
Phase 2 完成後，將為 Phase 3 (智能化增強) 建立基礎：
- 用戶行為數據收集
- 個人化偏好設定
- 機器學習準備工作

---

**Phase 2 開發時程**: 1週  
**負責範圍**: 使用者體驗優化  
**成功標準**: 用戶滿意度 > 4.0/5.0，操作流程 < 5步驟
