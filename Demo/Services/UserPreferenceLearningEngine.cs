using Demo.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 用戶偏好學習引擎 - Phase 3 智能化學習核心
/// </summary>
public class UserPreferenceLearningEngine
{
    private readonly ILogger<UserPreferenceLearningEngine> _logger;
    private readonly string _dataPath;

    public UserPreferenceLearningEngine(ILogger<UserPreferenceLearningEngine> logger)
    {
        _logger = logger;
        _dataPath = Path.Combine("App_Data", "user-voice-preferences.json");
    }

    /// <summary>
    /// 獲取用戶語音偏好
    /// </summary>
    public async Task<UserVoicePreferences?> GetUserPreferencesAsync(int userId)
    {
        try
        {
            if (!File.Exists(_dataPath))
            {
                return await CreateDefaultPreferencesAsync(userId);
            }

            var jsonContent = await File.ReadAllTextAsync(_dataPath);
            var allPreferences = JsonSerializer.Deserialize<Dictionary<string, UserVoicePreferences>>(jsonContent) 
                                ?? new Dictionary<string, UserVoicePreferences>();

            var userKey = userId.ToString();
            if (!allPreferences.ContainsKey(userKey))
            {
                return await CreateDefaultPreferencesAsync(userId);
            }

            return allPreferences[userKey];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取用戶語音偏好失敗：UserId={UserId}", userId);
            return await CreateDefaultPreferencesAsync(userId);
        }
    }

    /// <summary>
    /// 保存用戶語音偏好
    /// </summary>
    public async Task SaveUserPreferencesAsync(UserVoicePreferences preferences)
    {
        try
        {
            var allPreferences = new Dictionary<string, UserVoicePreferences>();
            
            if (File.Exists(_dataPath))
            {
                var jsonContent = await File.ReadAllTextAsync(_dataPath);
                allPreferences = JsonSerializer.Deserialize<Dictionary<string, UserVoicePreferences>>(jsonContent) 
                               ?? new Dictionary<string, UserVoicePreferences>();
            }

            preferences.UpdatedAt = DateTime.Now;
            allPreferences[preferences.UserId.ToString()] = preferences;

            var options = new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            await File.WriteAllTextAsync(_dataPath, JsonSerializer.Serialize(allPreferences, options));
            
            _logger.LogInformation("用戶語音偏好已保存：UserId={UserId}", preferences.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存用戶語音偏好失敗：UserId={UserId}", preferences.UserId);
        }
    }

    /// <summary>
    /// 學習用戶修正行為
    /// </summary>
    public async Task LearnFromUserCorrectionAsync(int userId, string originalValue, string correctedValue, string fieldName)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId) ?? await CreateDefaultPreferencesAsync(userId);
            
            // 記錄學習事件
            var learningEvent = new LearningEvent
            {
                EventType = "UserCorrection",
                FieldName = fieldName,
                OriginalValue = originalValue,
                CorrectedValue = correctedValue,
                ConfidenceImpact = CalculateConfidenceImpact(originalValue, correctedValue)
            };
            
            preferences.Statistics.RecentLearningEvents.Add(learningEvent);
            preferences.Statistics.TotalLearningEvents++;
            preferences.Statistics.LastLearningEvent = DateTime.Now;
            
            // 保持最近 100 個學習事件
            if (preferences.Statistics.RecentLearningEvents.Count > 100)
            {
                preferences.Statistics.RecentLearningEvents.RemoveAt(0);
            }
            
            // 學習個人化關鍵字
            await LearnPersonalKeywordAsync(preferences, correctedValue, fieldName);
            
            // 學習分類偏好
            if (fieldName == "Category")
            {
                await LearnCategoryPreferenceAsync(preferences, correctedValue);
            }
            
            await SaveUserPreferencesAsync(preferences);
            
            _logger.LogInformation("從用戶修正中學習：UserId={UserId}, Field={FieldName}, Correction={Original}->{Corrected}", 
                userId, fieldName, originalValue, correctedValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "從用戶修正學習失敗：UserId={UserId}", userId);
        }
    }

    /// <summary>
    /// 學習語音輸入模式
    /// </summary>
    public async Task LearnVoicePatternAsync(int userId, string voiceText, VoiceParseResult parseResult)
    {
        try
        {
            var preferences = await GetUserPreferencesAsync(userId) ?? await CreateDefaultPreferencesAsync(userId);
            
            // 更新語音輸入統計
            preferences.VoiceBehaviorPattern.InputStats.TotalInputs++;
            if (parseResult.IsSuccess)
            {
                preferences.VoiceBehaviorPattern.InputStats.SuccessfulParses++;
            }
            
            // 更新平均信心度
            var totalConfidence = preferences.VoiceBehaviorPattern.InputStats.AverageConfidence * 
                                (preferences.VoiceBehaviorPattern.InputStats.TotalInputs - 1) + 
                                parseResult.ParseConfidence;
            preferences.VoiceBehaviorPattern.InputStats.AverageConfidence = 
                totalConfidence / preferences.VoiceBehaviorPattern.InputStats.TotalInputs;
            
            // 學習常用短語
            if (!preferences.VoiceBehaviorPattern.CommonPhrases.Contains(voiceText) && 
                parseResult.ParseConfidence > 0.8)
            {
                preferences.VoiceBehaviorPattern.CommonPhrases.Add(voiceText);
                
                // 保持最近 50 個常用短語
                if (preferences.VoiceBehaviorPattern.CommonPhrases.Count > 50)
                {
                    preferences.VoiceBehaviorPattern.CommonPhrases.RemoveAt(0);
                }
            }
            
            // 更新時段使用統計
            var hour = DateTime.Now.Hour.ToString();
            if (preferences.VoiceBehaviorPattern.InputStats.HourlyUsage.ContainsKey(hour))
            {
                preferences.VoiceBehaviorPattern.InputStats.HourlyUsage[hour]++;
            }
            else
            {
                preferences.VoiceBehaviorPattern.InputStats.HourlyUsage[hour] = 1;
            }
            
            // 更新分類使用統計
            if (!string.IsNullOrEmpty(parseResult.Category))
            {
                if (preferences.VoiceBehaviorPattern.InputStats.CategoryUsage.ContainsKey(parseResult.Category))
                {
                    preferences.VoiceBehaviorPattern.InputStats.CategoryUsage[parseResult.Category]++;
                }
                else
                {
                    preferences.VoiceBehaviorPattern.InputStats.CategoryUsage[parseResult.Category] = 1;
                }
            }
            
            await SaveUserPreferencesAsync(preferences);
            
            _logger.LogDebug("語音模式學習完成：UserId={UserId}, Success={Success}", userId, parseResult.IsSuccess);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "語音模式學習失敗：UserId={UserId}", userId);
        }
    }

    /// <summary>
    /// 獲取個人化建議
    /// </summary>
    public async Task<List<ParseSuggestion>> GetPersonalizedSuggestionsAsync(int userId, string voiceText)
    {
        var suggestions = new List<ParseSuggestion>();
        
        try
        {
            var preferences = await GetUserPreferencesAsync(userId);
            if (preferences == null) return suggestions;
            
            // 基於個人化關鍵字的建議
            foreach (var keyword in preferences.PersonalKeywords.Values)
            {
                if (voiceText.Contains(keyword.Keyword))
                {
                    suggestions.Add(new ParseSuggestion
                    {
                        FieldName = keyword.Category,
                        SuggestedValue = keyword.MappedValue,
                        Confidence = keyword.Confidence,
                        Reason = $"基於您的個人化關鍵字「{keyword.Keyword}」",
                        Type = SuggestionType.PersonalizedLearning
                    });
                }
            }
            
            // 基於常用分類的建議
            foreach (var category in preferences.FrequentCategories.Values
                .Where(c => c.TriggerWords.Any(word => voiceText.Contains(word)))
                .OrderByDescending(c => c.Confidence))
            {
                suggestions.Add(new ParseSuggestion
                {
                    FieldName = "Category",
                    SuggestedValue = category.CategoryName,
                    Confidence = category.Confidence,
                    Reason = $"基於您常用的分類模式",
                    Type = SuggestionType.PersonalizedLearning
                });
            }
            
            // 基於商家識別的建議
            foreach (var merchant in preferences.FrequentMerchants.Values)
            {
                if (merchant.Aliases.Any(alias => voiceText.Contains(alias)) || 
                    voiceText.Contains(merchant.MerchantName))
                {
                    suggestions.Add(new ParseSuggestion
                    {
                        FieldName = "MerchantName",
                        SuggestedValue = merchant.MerchantName,
                        Confidence = 0.9,
                        Reason = $"識別到常用商家",
                        Type = SuggestionType.PersonalizedLearning
                    });
                    
                    if (!string.IsNullOrEmpty(merchant.PreferredCategory))
                    {
                        suggestions.Add(new ParseSuggestion
                        {
                            FieldName = "Category",
                            SuggestedValue = merchant.PreferredCategory,
                            Confidence = 0.8,
                            Reason = $"基於在「{merchant.MerchantName}」的消費習慣",
                            Type = SuggestionType.PersonalizedLearning
                        });
                    }
                }
            }
            
            return suggestions.OrderByDescending(s => s.Confidence).Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "獲取個人化建議失敗：UserId={UserId}", userId);
            return suggestions;
        }
    }

    #region 私有方法

    /// <summary>
    /// 建立預設用戶偏好
    /// </summary>
    private async Task<UserVoicePreferences> CreateDefaultPreferencesAsync(int userId)
    {
        var preferences = new UserVoicePreferences
        {
            UserId = userId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        
        // 初始化預設個人化關鍵字
        InitializeDefaultKeywords(preferences);
        
        await SaveUserPreferencesAsync(preferences);
        return preferences;
    }

    /// <summary>
    /// 初始化預設關鍵字
    /// </summary>
    private void InitializeDefaultKeywords(UserVoicePreferences preferences)
    {
        // 預設金額關鍵字
        var amountKeywords = new[] { "元", "塊", "塊錢" };
        foreach (var keyword in amountKeywords)
        {
            preferences.PersonalKeywords[keyword] = new PersonalKeyword
            {
                Keyword = keyword,
                MappedValue = keyword,
                Category = "Amount",
                Confidence = 0.8,
                UsageCount = 0
            };
        }
        
        // 預設付款方式關鍵字
        var paymentKeywords = new Dictionary<string, string>
        {
            ["現金"] = "現金",
            ["刷卡"] = "信用卡",
            ["悠遊卡"] = "悠遊卡",
            ["轉帳"] = "轉帳"
        };
        
        foreach (var kvp in paymentKeywords)
        {
            preferences.PersonalKeywords[kvp.Key] = new PersonalKeyword
            {
                Keyword = kvp.Key,
                MappedValue = kvp.Value,
                Category = "PaymentMethod",
                Confidence = 0.9,
                UsageCount = 0
            };
        }
    }

    /// <summary>
    /// 計算信心度影響
    /// </summary>
    private double CalculateConfidenceImpact(string originalValue, string correctedValue)
    {
        if (string.IsNullOrEmpty(originalValue) || string.IsNullOrEmpty(correctedValue))
            return 0.1;
            
        // 根據相似度計算影響程度
        var similarity = CalculateStringSimilarity(originalValue, correctedValue);
        return Math.Max(0.1, 1.0 - similarity);
    }

    /// <summary>
    /// 計算字串相似度
    /// </summary>
    private double CalculateStringSimilarity(string a, string b)
    {
        if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
            return 0.0;
            
        var longer = a.Length > b.Length ? a : b;
        var shorter = a.Length > b.Length ? b : a;
        
        if (longer.Length == 0)
            return 1.0;
            
        var editDistance = CalculateLevenshteinDistance(longer, shorter);
        return (longer.Length - editDistance) / (double)longer.Length;
    }

    /// <summary>
    /// 計算編輯距離
    /// </summary>
    private int CalculateLevenshteinDistance(string a, string b)
    {
        var distances = new int[a.Length + 1, b.Length + 1];
        
        for (var i = 0; i <= a.Length; distances[i, 0] = i++) { }
        for (var j = 0; j <= b.Length; distances[0, j] = j++) { }
        
        for (var i = 1; i <= a.Length; i++)
        {
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                distances[i, j] = Math.Min(
                    Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                    distances[i - 1, j - 1] + cost);
            }
        }
        
        return distances[a.Length, b.Length];
    }

    /// <summary>
    /// 學習個人化關鍵字
    /// </summary>
    private Task LearnPersonalKeywordAsync(UserVoicePreferences preferences, string value, string category)
    {
        if (string.IsNullOrEmpty(value)) return Task.CompletedTask;
        
        var key = $"{category}_{value}";
        if (preferences.PersonalKeywords.ContainsKey(key))
        {
            preferences.PersonalKeywords[key].UsageCount++;
            preferences.PersonalKeywords[key].LastUsed = DateTime.Now;
            preferences.PersonalKeywords[key].Confidence = Math.Min(1.0, 
                preferences.PersonalKeywords[key].Confidence + 0.1);
        }
        else
        {
            preferences.PersonalKeywords[key] = new PersonalKeyword
            {
                Keyword = value,
                MappedValue = value,
                Category = category,
                Confidence = 0.6,
                UsageCount = 1,
                LastUsed = DateTime.Now
            };
        }
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// 學習分類偏好
    /// </summary>
    private Task LearnCategoryPreferenceAsync(UserVoicePreferences preferences, string category)
    {
        if (string.IsNullOrEmpty(category)) return Task.CompletedTask;
        
        if (preferences.FrequentCategories.ContainsKey(category))
        {
            preferences.FrequentCategories[category].UsageCount++;
            preferences.FrequentCategories[category].LastUsed = DateTime.Now;
            preferences.FrequentCategories[category].Confidence = Math.Min(1.0,
                preferences.FrequentCategories[category].Confidence + 0.05);
        }
        else
        {
            preferences.FrequentCategories[category] = new CategoryMapping
            {
                CategoryName = category,
                TriggerWords = new List<string> { category },
                Confidence = 0.5,
                UsageCount = 1,
                LastUsed = DateTime.Now
            };
        }
        
        return Task.CompletedTask;
    }

    #endregion
}
