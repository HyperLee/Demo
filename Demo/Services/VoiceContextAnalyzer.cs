using Demo.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Demo.Services;

/// <summary>
/// 語音上下文分析器 - Phase 3 智能化增強
/// </summary>
public class VoiceContextAnalyzer
{
    private readonly ILogger<VoiceContextAnalyzer> _logger;
    private readonly UserPreferenceLearningEngine _learningEngine;

    public VoiceContextAnalyzer(
        ILogger<VoiceContextAnalyzer> logger,
        UserPreferenceLearningEngine learningEngine)
    {
        _logger = logger;
        _learningEngine = learningEngine;
    }

    /// <summary>
    /// 分析語音輸入的上下文意圖
    /// </summary>
    public async Task<VoiceContextAnalysisResult> AnalyzeContextAsync(
        string voiceText, 
        VoiceContext? context, 
        int? userId)
    {
        try
        {
            var result = new VoiceContextAnalysisResult();
            
            // 1. 意圖識別
            result.Intent = IdentifyIntent(voiceText, context);
            
            // 2. 對話狀態分析
            result.ConversationState = AnalyzeConversationState(context);
            
            // 3. 修正欄位識別
            if (result.Intent == "Correction")
            {
                result.FieldsToCorrect = IdentifyFieldsToCorrect(voiceText, context?.PreviousResult);
            }
            
            // 4. 個人化上下文
            if (userId.HasValue)
            {
                var userPreferences = await _learningEngine.GetUserPreferencesAsync(userId.Value);
                result.PersonalizedContext = BuildPersonalizedContext(voiceText, userPreferences);
            }
            
            // 5. 對話建議
            result.ConversationalSuggestions = GenerateConversationalSuggestions(result);
            
            _logger.LogInformation("語音上下文分析完成：Intent={Intent}, State={State}", 
                result.Intent, result.ConversationState);
                
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "語音上下文分析失敗：{VoiceText}", voiceText);
            return new VoiceContextAnalysisResult 
            { 
                Intent = "NewRecord",
                ConversationState = "Initial",
                HasError = true,
                ErrorMessage = "上下文分析失敗"
            };
        }
    }

    /// <summary>
    /// 識別輸入意圖
    /// </summary>
    private string IdentifyIntent(string voiceText, VoiceContext? context)
    {
        // 修正意圖關鍵字
        var correctionKeywords = new[] 
        { 
            "不對", "錯了", "修正", "改成", "應該是", "不是", "重新", "更正" 
        };
        
        // 確認意圖關鍵字
        var confirmationKeywords = new[] 
        { 
            "對", "正確", "沒錯", "確認", "是的", "好的" 
        };
        
        // 澄清意圖關鍵字
        var clarificationKeywords = new[] 
        { 
            "什麼", "哪個", "多少", "什麼時候", "怎麼", "為什麼" 
        };

        if (correctionKeywords.Any(k => voiceText.Contains(k)))
        {
            return "Correction";
        }
        
        if (confirmationKeywords.Any(k => voiceText.Contains(k)) && context?.PreviousResult != null)
        {
            return "Confirmation";
        }
        
        if (clarificationKeywords.Any(k => voiceText.Contains(k)))
        {
            return "Clarification";
        }

        return "NewRecord";
    }

    /// <summary>
    /// 分析對話狀態
    /// </summary>
    private string AnalyzeConversationState(VoiceContext? context)
    {
        if (context?.SessionId == null)
            return "Initial";
            
        if (context.PreviousResult == null)
            return "Fresh";
            
        if (context.FieldsToCorrect?.Any() == true)
            return "Correcting";
            
        return "Continuing";
    }

    /// <summary>
    /// 識別需要修正的欄位
    /// </summary>
    private List<string> IdentifyFieldsToCorrect(string voiceText, VoiceParseResult? previousResult)
    {
        var fieldsToCorrect = new List<string>();
        
        if (previousResult == null)
            return fieldsToCorrect;
            
        // 金額修正
        if (Regex.IsMatch(voiceText, @"金額|錢|元|塊") && 
            Regex.IsMatch(voiceText, @"不對|錯|改|應該"))
        {
            fieldsToCorrect.Add("Amount");
        }
        
        // 分類修正
        if (Regex.IsMatch(voiceText, @"分類|類別") && 
            Regex.IsMatch(voiceText, @"不對|錯|改|應該"))
        {
            fieldsToCorrect.Add("Category");
        }
        
        // 日期修正
        if (Regex.IsMatch(voiceText, @"日期|時間|昨天|今天|明天") && 
            Regex.IsMatch(voiceText, @"不對|錯|改|應該"))
        {
            fieldsToCorrect.Add("Date");
        }
        
        // 付款方式修正
        if (Regex.IsMatch(voiceText, @"付款|支付|信用卡|現金|悠遊卡") && 
            Regex.IsMatch(voiceText, @"不對|錯|改|應該"))
        {
            fieldsToCorrect.Add("PaymentMethod");
        }

        return fieldsToCorrect;
    }

    /// <summary>
    /// 建立個人化上下文
    /// </summary>
    private PersonalizedContext BuildPersonalizedContext(string voiceText, UserVoicePreferences? preferences)
    {
        var context = new PersonalizedContext();
        
        if (preferences == null)
            return context;
            
        // 個人化關鍵字匹配
        foreach (var keyword in preferences.PersonalKeywords.Values)
        {
            if (voiceText.Contains(keyword.Keyword))
            {
                context.MatchedKeywords.Add(keyword);
            }
        }
        
        // 常用分類推薦
        foreach (var category in preferences.FrequentCategories.Values.OrderByDescending(c => c.UsageCount).Take(3))
        {
            if (category.TriggerWords.Any(word => voiceText.Contains(word)))
            {
                context.RecommendedCategories.Add(category);
            }
        }
        
        // 常用商家識別
        foreach (var merchant in preferences.FrequentMerchants.Values)
        {
            if (merchant.Aliases.Any(alias => voiceText.Contains(alias)) || 
                voiceText.Contains(merchant.MerchantName))
            {
                context.IdentifiedMerchants.Add(merchant);
            }
        }
        
        return context;
    }

    /// <summary>
    /// 生成對話建議
    /// </summary>
    private List<ConversationalSuggestion> GenerateConversationalSuggestions(VoiceContextAnalysisResult result)
    {
        var suggestions = new List<ConversationalSuggestion>();
        
        switch (result.Intent)
        {
            case "NewRecord":
                suggestions.Add(new ConversationalSuggestion
                {
                    Type = "Question",
                    Message = "我來幫您記錄這筆帳務，請說出金額和消費內容。",
                    SuggestedActions = new[] { "開始語音輸入", "手動輸入" }.ToList()
                });
                break;
                
            case "Correction":
                if (result.FieldsToCorrect.Any())
                {
                    var fields = string.Join("、", result.FieldsToCorrect.Select(f => GetFieldDisplayName(f)));
                    suggestions.Add(new ConversationalSuggestion
                    {
                        Type = "Confirmation",
                        Message = $"我了解您想要修正 {fields}，請告訴我正確的內容。",
                        SuggestedActions = new[] { "重新說一次", "手動修改" }.ToList()
                    });
                }
                break;
                
            case "Clarification":
                suggestions.Add(new ConversationalSuggestion
                {
                    Type = "Question",
                    Message = "我需要更多資訊來幫您記帳，請提供缺少的資料。",
                    SuggestedActions = new[] { "提供更多資訊", "查看建議" }.ToList()
                });
                break;
        }
        
        return suggestions;
    }

    /// <summary>
    /// 取得欄位顯示名稱
    /// </summary>
    private string GetFieldDisplayName(string fieldName)
    {
        return fieldName switch
        {
            "Amount" => "金額",
            "Category" => "分類",
            "Date" => "日期",
            "PaymentMethod" => "付款方式",
            "MerchantName" => "商家名稱",
            "Description" => "說明",
            _ => fieldName
        };
    }
}

#region 上下文分析結果模型

/// <summary>
/// 語音上下文分析結果
/// </summary>
public class VoiceContextAnalysisResult
{
    public string Intent { get; set; } = "NewRecord";
    public string ConversationState { get; set; } = "Initial";
    public List<string> FieldsToCorrect { get; set; } = new();
    public PersonalizedContext? PersonalizedContext { get; set; }
    public List<ConversationalSuggestion> ConversationalSuggestions { get; set; } = new();
    public bool HasError { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 個人化上下文
/// </summary>
public class PersonalizedContext
{
    public List<PersonalKeyword> MatchedKeywords { get; set; } = new();
    public List<CategoryMapping> RecommendedCategories { get; set; } = new();
    public List<VoiceMerchantMapping> IdentifiedMerchants { get; set; } = new();
}

/// <summary>
/// 對話建議
/// </summary>
public class ConversationalSuggestion
{
    public string Type { get; set; } = string.Empty; // Question, Confirmation, Suggestion
    public string Message { get; set; } = string.Empty;
    public List<string> SuggestedActions { get; set; } = new();
}

#endregion
