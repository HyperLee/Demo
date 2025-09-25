# 記帳系統語音輸入功能 Phase 3: 智能化增強開發規格書

## 階段概述
**優先級**: 低  
**預估時程**: 1週  
**前置條件**: Phase 1 & Phase 2 已完成  
**目標**: 實作智能學習機制，提供個人化的語音解析體驗和智能化功能

## 開發目標

### 主要目標
1. 建立用戶行為學習和個人化偏好系統
2. 實作智能關鍵字字典和上下文理解
3. 開發對話式語音輸入和多輪修正功能
4. 建立解析品質自動改善機制
5. 提供進階的語音輸入體驗

### 成功指標
- [ ] 個人化解析準確度提升 > 15%
- [ ] 智能學習回饋機制運作正常
- [ ] 對話式語音輸入可用
- [ ] 解析速度維持在 < 3秒
- [ ] 用戶黏著度提升

## 技術實作細節

### 1. 個人化學習系統

#### 1.1 用戶偏好模型設計
```csharp
/// <summary>
/// 用戶語音偏好模型
/// </summary>
public class UserVoicePreferences
{
    public int UserId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    /// <summary>
    /// 個人化關鍵字字典
    /// </summary>
    public Dictionary<string, PersonalKeyword> PersonalKeywords { get; set; } = new();

    /// <summary>
    /// 常用分類映射
    /// </summary>
    public Dictionary<string, CategoryMapping> FrequentCategories { get; set; } = new();

    /// <summary>
    /// 常用商家映射
    /// </summary>
    public Dictionary<string, MerchantMapping> FrequentMerchants { get; set; } = new();

    /// <summary>
    /// 付款方式偏好
    /// </summary>
    public List<PaymentPreference> PaymentPreferences { get; set; } = new();

    /// <summary>
    /// 語音習慣分析
    /// </summary>
    public VoiceBehaviorAnalysis BehaviorAnalysis { get; set; } = new();

    /// <summary>
    /// 學習統計資訊
    /// </summary>
    public LearningStatistics Statistics { get; set; } = new();
}

/// <summary>
/// 個人化關鍵字
/// </summary>
public class PersonalKeyword
{
    public string Keyword { get; set; } = string.Empty;
    public string MappedValue { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty; // Category, Merchant, PaymentMethod 等
    public int UsageCount { get; set; }
    public double SuccessRate { get; set; }
    public DateTime LastUsed { get; set; }
    public double Confidence { get; set; }
}

/// <summary>
/// 分類映射
/// </summary>
public class CategoryMapping
{
    public string TriggerPhrase { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? SubCategoryName { get; set; }
    public int UsageCount { get; set; }
    public double AccuracyRate { get; set; }
    public List<string> RelatedKeywords { get; set; } = new();
}

/// <summary>
/// 商家映射
/// </summary>
public class MerchantMapping
{
    public string MerchantName { get; set; } = string.Empty;
    public List<string> Aliases { get; set; } = new(); // 用戶常用的別名
    public string PreferredCategory { get; set; } = string.Empty;
    public string PreferredPaymentMethod { get; set; } = string.Empty;
    public int VisitCount { get; set; }
    public decimal AverageAmount { get; set; }
}

/// <summary>
/// 付款方式偏好
/// </summary>
public class PaymentPreference
{
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CategoryContext { get; set; } // 在特定分類下的偏好
    public string? MerchantContext { get; set; } // 在特定商家的偏好
    public double UsageFrequency { get; set; }
    public int Priority { get; set; } // 1=最高優先級
}

/// <summary>
/// 語音行為分析
/// </summary>
public class VoiceBehaviorAnalysis
{
    /// <summary>
    /// 常用語音模式
    /// </summary>
    public List<string> CommonPatterns { get; set; } = new();

    /// <summary>
    /// 平均語音長度
    /// </summary>
    public double AverageSpeechLength { get; set; }

    /// <summary>
    /// 常用時段
    /// </summary>
    public Dictionary<int, int> UsageByHour { get; set; } = new();

    /// <summary>
    /// 語音複雜度偏好
    /// </summary>
    public string ComplexityPreference { get; set; } = "Medium"; // Simple, Medium, Complex

    /// <summary>
    /// 錯誤修正模式
    /// </summary>
    public string CorrectionStyle { get; set; } = "Immediate"; // Immediate, Batch, Manual
}

/// <summary>
/// 學習統計
/// </summary>
public class LearningStatistics
{
    public int TotalVoiceInputs { get; set; }
    public int SuccessfulParses { get; set; }
    public int UserCorrections { get; set; }
    public double AccuracyImprovement { get; set; }
    public DateTime LastLearningUpdate { get; set; }
    public int PersonalKeywordsCount { get; set; }
}
```

#### 1.2 個人化學習服務
```csharp
/// <summary>
/// 個人化學習服務
/// </summary>
public interface IPersonalizationService
{
    Task<UserVoicePreferences> GetUserPreferencesAsync(int userId);
    Task UpdateUserPreferencesAsync(UserVoicePreferences preferences);
    Task LearnFromUserCorrectionAsync(int userId, VoiceParseResult originalResult, AccountingRecord correctedRecord);
    Task<VoiceParseResult> ApplyPersonalizationAsync(int userId, VoiceParseResult baseResult);
    Task<List<string>> GetPersonalizedSuggestionsAsync(int userId, string partialInput);
}

/// <summary>
/// 個人化學習服務實作
/// </summary>
public class PersonalizationService : IPersonalizationService
{
    private readonly ILogger<PersonalizationService> _logger;
    private readonly IDataService _dataService;
    private readonly IMemoryCache _cache;

    public PersonalizationService(
        ILogger<PersonalizationService> logger,
        IDataService dataService,
        IMemoryCache cache)
    {
        _logger = logger;
        _dataService = dataService;
        _cache = cache;
    }

    /// <summary>
    /// 從用戶修正中學習
    /// </summary>
    public async Task LearnFromUserCorrectionAsync(int userId, VoiceParseResult originalResult, AccountingRecord correctedRecord)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId);

            // 學習分類映射
            await LearnCategoryMappingAsync(preferences, originalResult.OriginalText, correctedRecord);

            // 學習商家映射
            await LearnMerchantMappingAsync(preferences, originalResult.OriginalText, correctedRecord);

            // 學習付款方式偏好
            await LearnPaymentPreferenceAsync(preferences, correctedRecord);

            // 學習個人化關鍵字
            await LearnPersonalKeywordsAsync(preferences, originalResult.OriginalText, correctedRecord);

            // 更新統計資訊
            UpdateLearningStatistics(preferences);

            // 儲存偏好設定
            await UpdateUserPreferencesAsync(preferences);

            _logger.LogInformation("用戶 {UserId} 的學習更新完成", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從用戶修正學習時發生錯誤，用戶ID: {UserId}", userId);
        }
    }

    /// <summary>
    /// 套用個人化設定到解析結果
    /// </summary>
    public async Task<VoiceParseResult> ApplyPersonalizationAsync(int userId, VoiceParseResult baseResult)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId);
            var enhancedResult = baseResult.DeepCopy(); // 假設有深拷貝方法

            // 應用個人化關鍵字
            await ApplyPersonalKeywordsAsync(preferences, enhancedResult);

            // 應用分類偏好
            await ApplyCategoryPreferencesAsync(preferences, enhancedResult);

            // 應用商家偏好
            await ApplyMerchantPreferencesAsync(preferences, enhancedResult);

            // 應用付款方式偏好
            await ApplyPaymentPreferencesAsync(preferences, enhancedResult);

            // 調整信心度基於個人化程度
            AdjustConfidenceBasedOnPersonalization(preferences, enhancedResult);

            return enhancedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "應用個人化設定時發生錯誤，用戶ID: {UserId}", userId);
            return baseResult; // 發生錯誤時返回原始結果
        }
    }

    /// <summary>
    /// 學習分類映射
    /// </summary>
    private async Task LearnCategoryMappingAsync(UserVoicePreferences preferences, string originalText, AccountingRecord correctedRecord)
    {
        try
        {
            // 提取可能的觸發詞彙
            var triggerPhrases = ExtractCategoryTriggerPhrases(originalText);

            foreach (var phrase in triggerPhrases)
            {
                var key = phrase.ToLower();
                if (preferences.FrequentCategories.ContainsKey(key))
                {
                    var mapping = preferences.FrequentCategories[key];
                    mapping.UsageCount++;
                    
                    // 檢查是否正確映射
                    if (mapping.CategoryName == correctedRecord.Category)
                    {
                        mapping.AccuracyRate = (mapping.AccuracyRate * (mapping.UsageCount - 1) + 1.0) / mapping.UsageCount;
                    }
                    else
                    {
                        mapping.AccuracyRate = (mapping.AccuracyRate * (mapping.UsageCount - 1) + 0.0) / mapping.UsageCount;
                        // 更新為正確的分類
                        mapping.CategoryName = correctedRecord.Category;
                        mapping.SubCategoryName = correctedRecord.SubCategory;
                    }
                }
                else
                {
                    // 建立新的映射
                    preferences.FrequentCategories[key] = new CategoryMapping
                    {
                        TriggerPhrase = phrase,
                        CategoryName = correctedRecord.Category,
                        SubCategoryName = correctedRecord.SubCategory,
                        UsageCount = 1,
                        AccuracyRate = 1.0,
                        RelatedKeywords = ExtractRelatedKeywords(originalText)
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "學習分類映射時發生錯誤");
        }
    }

    /// <summary>
    /// 提取分類觸發詞彙
    /// </summary>
    private List<string> ExtractCategoryTriggerPhrases(string text)
    {
        var phrases = new List<string>();
        
        // 簡單的關鍵字提取邏輯
        var keywords = new[]
        {
            "咖啡", "早餐", "午餐", "晚餐", "飲料", "零食",
            "加油", "停車", "計程車", "公車", "捷運",
            "電影", "唱歌", "遊戲", "購物", "美容",
            "醫生", "藥品", "健檢", "保健"
        };

        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword))
            {
                phrases.Add(keyword);
            }
        }

        return phrases;
    }

    /// <summary>
    /// 提取相關關鍵字
    /// </summary>
    private List<string> ExtractRelatedKeywords(string text)
    {
        // 這裡可以實作更複雜的NLP邏輯來提取相關詞彙
        var words = text.Split(new[] { ' ', '，', '。', '！', '？' }, StringSplitOptions.RemoveEmptyEntries);
        return words.Where(w => w.Length > 1).Take(5).ToList();
    }

    /// <summary>
    /// 應用個人化關鍵字
    /// </summary>
    private async Task ApplyPersonalKeywordsAsync(UserVoicePreferences preferences, VoiceParseResult result)
    {
        try
        {
            foreach (var personalKeyword in preferences.PersonalKeywords.Values)
            {
                if (result.OriginalText.Contains(personalKeyword.Keyword, StringComparison.OrdinalIgnoreCase))
                {
                    switch (personalKeyword.FieldType.ToLower())
                    {
                        case "category":
                            result.Category = personalKeyword.MappedValue;
                            result.FieldConfidence["Category"] = Math.Max(
                                result.FieldConfidence.GetValueOrDefault("Category", 0),
                                personalKeyword.Confidence);
                            break;
                        case "merchant":
                            result.MerchantName = personalKeyword.MappedValue;
                            result.FieldConfidence["MerchantName"] = Math.Max(
                                result.FieldConfidence.GetValueOrDefault("MerchantName", 0),
                                personalKeyword.Confidence);
                            break;
                        case "paymentmethod":
                            result.PaymentMethod = personalKeyword.MappedValue;
                            result.FieldConfidence["PaymentMethod"] = Math.Max(
                                result.FieldConfidence.GetValueOrDefault("PaymentMethod", 0),
                                personalKeyword.Confidence);
                            break;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "應用個人化關鍵字時發生錯誤");
        }
    }

    /// <summary>
    /// 應用付款方式偏好
    /// </summary>
    private async Task ApplyPaymentPreferencesAsync(UserVoicePreferences preferences, VoiceParseResult result)
    {
        try
        {
            // 如果沒有解析出付款方式，嘗試根據偏好推測
            if (string.IsNullOrEmpty(result.PaymentMethod))
            {
                var applicablePreferences = preferences.PaymentPreferences
                    .Where(p => string.IsNullOrEmpty(p.CategoryContext) || p.CategoryContext == result.Category)
                    .Where(p => string.IsNullOrEmpty(p.MerchantContext) || p.MerchantContext == result.MerchantName)
                    .OrderBy(p => p.Priority)
                    .ThenByDescending(p => p.UsageFrequency);

                var topPreference = applicablePreferences.FirstOrDefault();
                if (topPreference != null)
                {
                    result.PaymentMethod = topPreference.PaymentMethod;
                    result.FieldConfidence["PaymentMethod"] = Math.Min(topPreference.UsageFrequency, 0.8);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "應用付款方式偏好時發生錯誤");
        }
    }
}
```

### 2. 智能上下文理解

#### 2.1 對話式語音輸入
```csharp
/// <summary>
/// 對話式語音輸入管理器
/// </summary>
public class ConversationalVoiceManager
{
    private readonly ILogger<ConversationalVoiceManager> _logger;
    private readonly Dictionary<string, ConversationSession> _activeSessions = new();

    public ConversationalVoiceManager(ILogger<ConversationalVoiceManager> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 處理對話式語音輸入
    /// </summary>
    public async Task<ConversationalVoiceResponse> ProcessConversationalInputAsync(string sessionId, string voiceInput, VoiceParseResult baseResult)
    {
        try
        {
            var session = GetOrCreateSession(sessionId);
            
            // 分析語音輸入類型
            var inputType = AnalyzeInputType(voiceInput, session);
            
            switch (inputType)
            {
                case ConversationalInputType.NewRecord:
                    return await HandleNewRecordAsync(session, voiceInput, baseResult);
                
                case ConversationalInputType.Correction:
                    return await HandleCorrectionAsync(session, voiceInput);
                
                case ConversationalInputType.Addition:
                    return await HandleAdditionAsync(session, voiceInput);
                
                case ConversationalInputType.Confirmation:
                    return await HandleConfirmationAsync(session, voiceInput);
                
                case ConversationalInputType.Query:
                    return await HandleQueryAsync(session, voiceInput);
                
                default:
                    return await HandleUnknownInputAsync(session, voiceInput);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理對話式語音輸入時發生錯誤");
            return new ConversationalVoiceResponse
            {
                IsSuccess = false,
                ErrorMessage = "處理語音輸入時發生錯誤"
            };
        }
    }

    /// <summary>
    /// 分析輸入類型
    /// </summary>
    private ConversationalInputType AnalyzeInputType(string input, ConversationSession session)
    {
        var lowerInput = input.ToLower();

        // 修正關鍵字
        var correctionKeywords = new[] { "不是", "錯了", "修正", "改成", "應該是" };
        if (correctionKeywords.Any(k => lowerInput.Contains(k)))
        {
            return ConversationalInputType.Correction;
        }

        // 補充關鍵字
        var additionKeywords = new[] { "還有", "另外", "補充", "加上" };
        if (additionKeywords.Any(k => lowerInput.Contains(k)))
        {
            return ConversationalInputType.Addition;
        }

        // 確認關鍵字
        var confirmationKeywords = new[] { "對", "正確", "沒錯", "確認", "好的" };
        if (confirmationKeywords.Any(k => lowerInput.Contains(k)))
        {
            return ConversationalInputType.Confirmation;
        }

        // 查詢關鍵字
        var queryKeywords = new[] { "什麼", "哪個", "多少", "幾點", "查詢" };
        if (queryKeywords.Any(k => lowerInput.Contains(k)))
        {
            return ConversationalInputType.Query;
        }

        // 如果會話中沒有活躍記錄，視為新記錄
        if (session.CurrentRecord == null)
        {
            return ConversationalInputType.NewRecord;
        }

        // 預設為新記錄
        return ConversationalInputType.NewRecord;
    }

    /// <summary>
    /// 處理修正輸入
    /// </summary>
    private async Task<ConversationalVoiceResponse> HandleCorrectionAsync(ConversationSession session, string input)
    {
        try
        {
            if (session.CurrentRecord == null)
            {
                return new ConversationalVoiceResponse
                {
                    IsSuccess = false,
                    Message = "目前沒有可以修正的記錄。請先新增一筆記錄。"
                };
            }

            // 解析修正內容
            var correction = ParseCorrectionInput(input);
            
            // 應用修正
            ApplyCorrectionToRecord(session.CurrentRecord, correction);
            
            // 記錄修正歷史
            session.CorrectionHistory.Add(new CorrectionRecord
            {
                Timestamp = DateTime.Now,
                OriginalInput = input,
                Field = correction.Field,
                OldValue = correction.OldValue,
                NewValue = correction.NewValue
            });

            return new ConversationalVoiceResponse
            {
                IsSuccess = true,
                Message = $"已將{correction.Field}從「{correction.OldValue}」修正為「{correction.NewValue}」",
                UpdatedRecord = session.CurrentRecord,
                RequiresConfirmation = true,
                SuggestedResponse = "請確認修正後的內容是否正確？"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "處理修正輸入時發生錯誤");
            return new ConversationalVoiceResponse
            {
                IsSuccess = false,
                ErrorMessage = "處理修正時發生錯誤"
            };
        }
    }
}

/// <summary>
/// 對話式輸入類型
/// </summary>
public enum ConversationalInputType
{
    NewRecord,      // 新記錄
    Correction,     // 修正
    Addition,       // 補充
    Confirmation,   // 確認
    Query,          // 查詢
    Unknown         // 未知
}

/// <summary>
/// 對話會話
/// </summary>
public class ConversationSession
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.Now;
    public DateTime LastActivity { get; set; } = DateTime.Now;
    public VoiceParseResult? CurrentRecord { get; set; }
    public List<string> ConversationHistory { get; set; } = new();
    public List<CorrectionRecord> CorrectionHistory { get; set; } = new();
    public Dictionary<string, object> Context { get; set; } = new();
    public ConversationState State { get; set; } = ConversationState.WaitingForInput;
}

/// <summary>
/// 對話狀態
/// </summary>
public enum ConversationState
{
    WaitingForInput,        // 等待輸入
    ProcessingInput,        // 處理中
    WaitingForConfirmation, // 等待確認
    WaitingForCorrection,   // 等待修正
    Completed              // 完成
}

/// <summary>
/// 修正記錄
/// </summary>
public class CorrectionRecord
{
    public DateTime Timestamp { get; set; }
    public string OriginalInput { get; set; } = string.Empty;
    public string Field { get; set; } = string.Empty;
    public string OldValue { get; set; } = string.Empty;
    public string NewValue { get; set; } = string.Empty;
}

/// <summary>
/// 對話式語音回應
/// </summary>
public class ConversationalVoiceResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorMessage { get; set; }
    public VoiceParseResult? UpdatedRecord { get; set; }
    public bool RequiresConfirmation { get; set; }
    public string? SuggestedResponse { get; set; }
    public List<string> Suggestions { get; set; } = new();
}
```

### 3. 智能建議系統

#### 3.1 預測輸入建議
```csharp
/// <summary>
/// 智能建議服務
/// </summary>
public interface ISmartSuggestionService
{
    Task<List<SmartSuggestion>> GetInputSuggestionsAsync(int userId, string partialInput);
    Task<List<SmartSuggestion>> GetCategorySuggestionsAsync(int userId, VoiceParseResult partialResult);
    Task<List<SmartSuggestion>> GetMerchantSuggestionsAsync(int userId, string category, decimal? amount);
    Task UpdateSuggestionUsageAsync(int userId, SmartSuggestion suggestion, bool wasUsed);
}

/// <summary>
/// 智能建議實作
/// </summary>
public class SmartSuggestionService : ISmartSuggestionService
{
    private readonly ILogger<SmartSuggestionService> _logger;
    private readonly IPersonalizationService _personalizationService;
    private readonly IDataService _dataService;

    public SmartSuggestionService(
        ILogger<SmartSuggestionService> logger,
        IPersonalizationService personalizationService,
        IDataService dataService)
    {
        _logger = logger;
        _personalizationService = personalizationService;
        _dataService = dataService;
    }

    /// <summary>
    /// 取得輸入建議
    /// </summary>
    public async Task<List<SmartSuggestion>> GetInputSuggestionsAsync(int userId, string partialInput)
    {
        try
        {
            var suggestions = new List<SmartSuggestion>();
            var preferences = await _personalizationService.GetUserPreferencesAsync(userId);

            // 基於歷史記錄的建議
            var historicalSuggestions = await GetHistoricalSuggestionsAsync(userId, partialInput);
            suggestions.AddRange(historicalSuggestions);

            // 基於個人化關鍵字的建議
            var personalizedSuggestions = GetPersonalizedSuggestions(preferences, partialInput);
            suggestions.AddRange(personalizedSuggestions);

            // 基於時間的建議
            var timeBased = GetTimeBasedSuggestions(preferences, partialInput);
            suggestions.AddRange(timeBased);

            // 基於語意相似性的建議
            var semanticSuggestions = await GetSemanticSuggestionsAsync(partialInput);
            suggestions.AddRange(semanticSuggestions);

            // 排序和去重
            return suggestions
                .GroupBy(s => s.Text)
                .Select(g => g.OrderByDescending(s => s.Confidence).First())
                .OrderByDescending(s => s.Confidence)
                .Take(8)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得輸入建議時發生錯誤");
            return new List<SmartSuggestion>();
        }
    }

    /// <summary>
    /// 取得歷史建議
    /// </summary>
    private async Task<List<SmartSuggestion>> GetHistoricalSuggestionsAsync(int userId, string partialInput)
    {
        try
        {
            // 從用戶的歷史記錄中找出相似的輸入
            var recentRecords = await _dataService.GetRecentRecordsAsync(userId, 100);
            var suggestions = new List<SmartSuggestion>();

            foreach (var record in recentRecords)
            {
                // 建構完整的語音輸入範例
                var suggestion = ConstructSuggestionFromRecord(record, partialInput);
                if (suggestion != null)
                {
                    suggestions.Add(suggestion);
                }
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "取得歷史建議時發生錯誤");
            return new List<SmartSuggestion>();
        }
    }

    /// <summary>
    /// 從記錄建構建議
    /// </summary>
    private SmartSuggestion? ConstructSuggestionFromRecord(AccountingRecord record, string partialInput)
    {
        try
        {
            // 根據記錄資訊建構語音輸入範例
            var suggestionText = $"我{(record.Type == "Expense" ? "花了" : "收入")}{record.Amount}元";
            
            if (!string.IsNullOrEmpty(record.Note))
            {
                suggestionText += record.Note;
            }
            
            if (!string.IsNullOrEmpty(record.PaymentMethod) && record.PaymentMethod != "現金")
            {
                suggestionText += $"，用{record.PaymentMethod}";
            }

            // 計算與部分輸入的相似度
            var similarity = CalculateTextSimilarity(partialInput, suggestionText);
            
            if (similarity < 0.3) return null; // 相似度太低，不提供建議

            return new SmartSuggestion
            {
                Text = suggestionText,
                Type = SmartSuggestionType.Historical,
                Confidence = similarity,
                Metadata = new Dictionary<string, object>
                {
                    ["RecordId"] = record.Id,
                    ["Category"] = record.Category,
                    ["Amount"] = record.Amount
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "建構建議時發生錯誤");
            return null;
        }
    }

    /// <summary>
    /// 計算文字相似度
    /// </summary>
    private double CalculateTextSimilarity(string text1, string text2)
    {
        // 簡單的相似度計算 - 可以用更複雜的演算法替換
        var words1 = text1.ToLower().Split(' ', '，', '。');
        var words2 = text2.ToLower().Split(' ', '，', '。');
        
        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();
        
        return union == 0 ? 0 : (double)intersection / union;
    }

    /// <summary>
    /// 取得基於時間的建議
    /// </summary>
    private List<SmartSuggestion> GetTimeBasedSuggestions(UserVoicePreferences preferences, string partialInput)
    {
        var suggestions = new List<SmartSuggestion>();
        var currentHour = DateTime.Now.Hour;

        try
        {
            // 基於時間的常見活動建議
            var timeBasedTemplates = new Dictionary<int, List<string>>
            {
                { 7, new List<string> { "我花了40元吃早餐", "我買了30元的咖啡" } },
                { 12, new List<string> { "我花了80元吃午餐", "我點了100元的便當" } },
                { 18, new List<string> { "我花了120元吃晚餐", "我去超市買了200元的菜" } },
                { 22, new List<string> { "我買了50元的宵夜", "我叫了外送花了150元" } }
            };

            // 找出最接近的時間範圍
            var relevantHour = timeBasedTemplates.Keys
                .OrderBy(h => Math.Abs(h - currentHour))
                .FirstOrDefault();

            if (timeBasedTemplates.ContainsKey(relevantHour))
            {
                foreach (var template in timeBasedTemplates[relevantHour])
                {
                    var similarity = CalculateTextSimilarity(partialInput, template);
                    if (similarity > 0.2)
                    {
                        suggestions.Add(new SmartSuggestion
                        {
                            Text = template,
                            Type = SmartSuggestionType.TimeBased,
                            Confidence = similarity * 0.8, // 時間建議的信心度稍低
                            Metadata = new Dictionary<string, object>
                            {
                                ["TimeHour"] = relevantHour,
                                ["SuggestionReason"] = "基於時間的常見活動"
                            }
                        });
                    }
                }
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "取得時間建議時發生錯誤");
            return suggestions;
        }
    }
}

/// <summary>
/// 智能建議模型
/// </summary>
public class SmartSuggestion
{
    public string Text { get; set; } = string.Empty;
    public SmartSuggestionType Type { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
    public string? Description { get; set; }
    public string? Icon { get; set; }
}

/// <summary>
/// 建議類型
/// </summary>
public enum SmartSuggestionType
{
    Historical,     // 歷史記錄
    Personalized,   // 個人化
    TimeBased,      // 時間相關
    Semantic,       // 語意相關
    Template        // 範本
}
```

### 4. 前端智能化功能

#### 4.1 智能輸入助手
```html
<!-- 智能輸入助手面板 -->
<div id="smartInputAssistant" class="mt-3 d-none">
    <div class="card border-primary">
        <div class="card-header bg-gradient-primary text-white">
            <div class="d-flex justify-content-between align-items-center">
                <h6 class="mb-0">
                    <i class="fas fa-brain"></i> 智能輸入助手
                </h6>
                <button type="button" class="btn btn-sm btn-outline-light" id="toggleAssistant">
                    <i class="fas fa-chevron-up"></i>
                </button>
            </div>
        </div>
        <div class="card-body" id="assistantBody">
            <!-- 快速建議區域 -->
            <div class="mb-3">
                <label class="form-label">
                    <i class="fas fa-magic"></i> 智能建議
                    <small class="text-muted">- 基於您的使用習慣</small>
                </label>
                <div id="quickSuggestions" class="d-flex flex-wrap gap-2">
                    <!-- 動態生成建議按鈕 -->
                </div>
            </div>

            <!-- 對話式輸入區域 -->
            <div class="mb-3">
                <label class="form-label">
                    <i class="fas fa-comments"></i> 對話式輸入
                </label>
                <div id="conversationArea" class="border rounded p-3 bg-light" style="min-height: 120px; max-height: 200px; overflow-y: auto;">
                    <div id="conversationHistory">
                        <!-- 對話歷史 -->
                    </div>
                    <div class="input-group mt-2">
                        <input type="text" id="conversationInput" class="form-control" 
                               placeholder="請說明您要記錄的內容...">
                        <button type="button" class="btn btn-primary" id="sendConversationBtn">
                            <i class="fas fa-paper-plane"></i>
                        </button>
                    </div>
                </div>
            </div>

            <!-- 學習進度顯示 -->
            <div class="mb-3">
                <label class="form-label">
                    <i class="fas fa-graduation-cap"></i> 個人化學習進度
                </label>
                <div class="row">
                    <div class="col-6">
                        <div class="text-center">
                            <div class="h5 text-primary mb-1" id="learningAccuracy">0%</div>
                            <small class="text-muted">解析準確度</small>
                        </div>
                    </div>
                    <div class="col-6">
                        <div class="text-center">
                            <div class="h5 text-success mb-1" id="personalKeywords">0</div>
                            <small class="text-muted">個人化關鍵字</small>
                        </div>
                    </div>
                </div>
                <div class="progress mt-2" style="height: 6px;">
                    <div id="learningProgressBar" class="progress-bar bg-gradient-success" 
                         style="width: 0%"></div>
                </div>
            </div>

            <!-- 使用統計 -->
            <div class="d-none" id="usageStats">
                <label class="form-label">
                    <i class="fas fa-chart-line"></i> 使用統計
                </label>
                <div class="row text-center">
                    <div class="col-4">
                        <div class="h6 mb-1" id="totalVoiceInputs">0</div>
                        <small class="text-muted">總次數</small>
                    </div>
                    <div class="col-4">
                        <div class="h6 mb-1" id="successfulParses">0</div>
                        <small class="text-muted">成功解析</small>
                    </div>
                    <div class="col-4">
                        <div class="h6 mb-1" id="accuracyImprovement">+0%</div>
                        <small class="text-muted">準確度提升</small>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
```

#### 4.2 JavaScript 智能化功能
```javascript
/**
 * 智能輸入助手類 (Phase 3)
 */
class SmartInputAssistant {
    constructor(voiceInputInstance) {
        this.voiceInput = voiceInputInstance;
        this.conversationSession = null;
        this.personalizedSuggestions = [];
        this.learningData = {};
        
        this.init();
    }

    /**
     * 初始化智能助手
     */
    init() {
        this.loadPersonalizationData();
        this.setupEventListeners();
        this.startPeriodicUpdates();
    }

    /**
     * 載入個人化資料
     */
    async loadPersonalizationData() {
        try {
            const response = await fetch('/api/personalization/preferences', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                this.learningData = await response.json();
                this.updateLearningDisplay();
                this.generateQuickSuggestions();
            }
        } catch (error) {
            console.error('載入個人化資料時發生錯誤:', error);
        }
    }

    /**
     * 更新學習進度顯示
     */
    updateLearningDisplay() {
        try {
            const stats = this.learningData.Statistics || {};
            
            // 更新準確度
            const accuracy = Math.round((stats.SuccessfulParses / Math.max(stats.TotalVoiceInputs, 1)) * 100);
            document.getElementById('learningAccuracy').textContent = `${accuracy}%`;
            
            // 更新個人化關鍵字數量
            document.getElementById('personalKeywords').textContent = stats.PersonalKeywordsCount || 0;
            
            // 更新學習進度條
            const progressBar = document.getElementById('learningProgressBar');
            const progress = Math.min(accuracy, 100);
            progressBar.style.width = `${progress}%`;
            
            // 更新使用統計
            if (stats.TotalVoiceInputs > 0) {
                document.getElementById('usageStats').classList.remove('d-none');
                document.getElementById('totalVoiceInputs').textContent = stats.TotalVoiceInputs;
                document.getElementById('successfulParses').textContent = stats.SuccessfulParses;
                document.getElementById('accuracyImprovement').textContent = 
                    `+${Math.round(stats.AccuracyImprovement * 100)}%`;
            }
        } catch (error) {
            console.error('更新學習顯示時發生錯誤:', error);
        }
    }

    /**
     * 生成快速建議
     */
    async generateQuickSuggestions() {
        try {
            const response = await fetch('/api/suggestions/quick', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    partialInput: '',
                    context: 'quick_start'
                })
            });

            if (response.ok) {
                const suggestions = await response.json();
                this.displayQuickSuggestions(suggestions);
            }
        } catch (error) {
            console.error('生成快速建議時發生錯誤:', error);
        }
    }

    /**
     * 顯示快速建議
     */
    displayQuickSuggestions(suggestions) {
        const container = document.getElementById('quickSuggestions');
        container.innerHTML = '';

        suggestions.slice(0, 6).forEach(suggestion => {
            const button = document.createElement('button');
            button.type = 'button';
            button.className = 'btn btn-outline-primary btn-sm';
            button.innerHTML = `
                <i class="${this.getSuggestionIcon(suggestion.Type)}"></i>
                ${suggestion.Text}
                <small class="ms-1 opacity-75">${Math.round(suggestion.Confidence * 100)}%</small>
            `;
            
            button.onclick = () => {
                this.applySuggestion(suggestion);
            };
            
            container.appendChild(button);
        });
    }

    /**
     * 應用建議
     */
    async applySuggestion(suggestion) {
        try {
            // 記錄建議使用
            await this.recordSuggestionUsage(suggestion, true);
            
            // 開始語音解析
            const parseResponse = await fetch('/api/voice/parse', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    VoiceText: suggestion.Text,
                    Context: 'suggestion_applied'
                })
            });

            if (parseResponse.ok) {
                const result = await parseResponse.json();
                this.voiceInput.displayParseResult(result);
                
                // 顯示成功訊息
                this.showMessage('已套用建議並完成解析！', 'success');
            } else {
                throw new Error('解析建議時發生錯誤');
            }
        } catch (error) {
            console.error('應用建議時發生錯誤:', error);
            this.showMessage('應用建議時發生錯誤', 'error');
        }
    }

    /**
     * 處理對話式輸入
     */
    async handleConversationalInput(input) {
        try {
            if (!this.conversationSession) {
                this.conversationSession = {
                    id: this.generateSessionId(),
                    startTime: new Date(),
                    history: []
                };
            }

            // 顯示用戶輸入
            this.addConversationMessage('user', input);

            // 發送到後端處理
            const response = await fetch('/api/voice/conversational', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    sessionId: this.conversationSession.id,
                    input: input
                })
            });

            if (response.ok) {
                const result = await response.json();
                
                // 顯示系統回應
                this.addConversationMessage('assistant', result.Message);
                
                if (result.IsSuccess && result.UpdatedRecord) {
                    // 更新解析結果
                    this.voiceInput.displayParseResult(result);
                }

                if (result.RequiresConfirmation) {
                    // 顯示確認按鈕
                    this.showConfirmationButtons(result);
                }
            } else {
                throw new Error('處理對話輸入時發生錯誤');
            }
        } catch (error) {
            console.error('處理對話式輸入時發生錯誤:', error);
            this.addConversationMessage('assistant', '抱歉，我沒有理解您的意思，請再試一次。');
        }
    }

    /**
     * 添加對話訊息
     */
    addConversationMessage(sender, message) {
        const historyDiv = document.getElementById('conversationHistory');
        const messageDiv = document.createElement('div');
        messageDiv.className = `mb-2 ${sender === 'user' ? 'text-end' : 'text-start'}`;
        
        messageDiv.innerHTML = `
            <div class="d-inline-block p-2 rounded ${sender === 'user' ? 'bg-primary text-white' : 'bg-light'}">
                <small class="d-block opacity-75">
                    ${sender === 'user' ? '您' : '助手'} - ${new Date().toLocaleTimeString()}
                </small>
                <div>${message}</div>
            </div>
        `;
        
        historyDiv.appendChild(messageDiv);
        historyDiv.scrollTop = historyDiv.scrollHeight;
    }

    /**
     * 智能錯誤修正
     */
    async performIntelligentCorrection(originalResult, userCorrection) {
        try {
            // 發送學習請求
            const response = await fetch('/api/personalization/learn', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    originalResult: originalResult,
                    correctedData: userCorrection,
                    learningType: 'user_correction'
                })
            });

            if (response.ok) {
                const learningResult = await response.json();
                
                // 更新本地學習資料
                this.learningData = learningResult.updatedPreferences;
                this.updateLearningDisplay();
                
                // 重新生成建議
                this.generateQuickSuggestions();
                
                this.showMessage('謝謝您的修正！系統已經學習並改善。', 'success');
            }
        } catch (error) {
            console.error('執行智能修正時發生錯誤:', error);
        }
    }

    /**
     * 設定事件監聽器
     */
    setupEventListeners() {
        // 對話輸入
        document.getElementById('sendConversationBtn').onclick = () => {
            const input = document.getElementById('conversationInput');
            if (input.value.trim()) {
                this.handleConversationalInput(input.value.trim());
                input.value = '';
            }
        };

        // Enter 鍵發送
        document.getElementById('conversationInput').onkeypress = (e) => {
            if (e.key === 'Enter') {
                document.getElementById('sendConversationBtn').click();
            }
        };

        // 助手面板切換
        document.getElementById('toggleAssistant').onclick = () => {
            const body = document.getElementById('assistantBody');
            const icon = document.querySelector('#toggleAssistant i');
            
            if (body.style.display === 'none') {
                body.style.display = 'block';
                icon.className = 'fas fa-chevron-up';
            } else {
                body.style.display = 'none';
                icon.className = 'fas fa-chevron-down';
            }
        };
    }

    /**
     * 啟動定期更新
     */
    startPeriodicUpdates() {
        // 每5分鐘更新一次學習資料
        setInterval(() => {
            this.loadPersonalizationData();
        }, 300000);
    }

    // === 輔助方法 ===

    getSuggestionIcon(type) {
        const iconMap = {
            'Historical': 'fas fa-history',
            'Personalized': 'fas fa-user',
            'TimeBased': 'fas fa-clock',
            'Semantic': 'fas fa-brain',
            'Template': 'fas fa-file-alt'
        };
        return iconMap[type] || 'fas fa-lightbulb';
    }

    generateSessionId() {
        return 'session_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
    }

    async recordSuggestionUsage(suggestion, wasUsed) {
        try {
            await fetch('/api/suggestions/usage', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    suggestion: suggestion,
                    wasUsed: wasUsed,
                    timestamp: new Date().toISOString()
                })
            });
        } catch (error) {
            console.error('記錄建議使用時發生錯誤:', error);
        }
    }

    showMessage(message, type) {
        // 實作訊息顯示邏輯
        console.log(`${type}: ${message}`);
    }
}
```

## 測試規劃

### 個人化學習測試
```
測試案例 1: 個人化關鍵字學習
- 用戶輸入: "去小七買飲料"
- 系統學習: "小七" -> "7-11", 分類 -> "餐飲美食"
- 預期: 後續輸入"小七"時自動識別為"7-11"

測試案例 2: 付款方式偏好學習
- 用戶在特定商家總是使用信用卡
- 預期: 系統自動建議該付款方式

測試案例 3: 對話式修正
- 用戶: "我花了100元吃早餐"
- 系統: 解析結果
- 用戶: "不是早餐，是午餐"
- 預期: 系統正確修正並學習
```

### 智能化功能測試
1. **建議準確性測試**
   - 測試個人化建議的相關性
   - 驗證時間基礎建議的準確性
   - 測試語意相似建議

2. **學習效果測試**
   - 測試準確度改善趨勢
   - 驗證個人化關鍵字累積
   - 測試錯誤修正學習

3. **對話式互動測試**
   - 測試多輪對話處理
   - 驗證上下文理解能力
   - 測試修正和確認流程

## 交付標準

### 功能完整性
- [ ] 個人化學習系統運作正常
- [ ] 智能建議系統準確有效
- [ ] 對話式語音輸入可用
- [ ] 學習效果可視化展示

### 智能化指標
- [ ] 個人化準確度提升 > 15%
- [ ] 建議採用率 > 40%
- [ ] 對話成功率 > 80%
- [ ] 學習回饋週期 < 24小時

### 使用者體驗
- [ ] 智能功能直覺易用
- [ ] 學習進度清晰展示
- [ ] 個人化程度可感知
- [ ] 整體滿意度 > 4.2/5.0

### 效能與穩定性
- [ ] 解析時間維持 < 3秒
- [ ] 記憶體使用合理
- [ ] 無重大錯誤或當機
- [ ] 資料安全無虞

---

**Phase 3 開發時程**: 1週  
**負責範圍**: 智能化增強  
**成功標準**: 個人化準確度提升 > 15%，用戶滿意度 > 4.2/5.0

## 長期發展方向
- 整合更進階的 NLP 技術
- 支援多語言語音輸入
- 增加語音命令功能
- 開發離線語音處理能力
