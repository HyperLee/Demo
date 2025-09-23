using Demo.Models;

namespace Demo.Services
{
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
                // 檢查是否有重大解析錯誤
                if (!result.IsSuccess || !string.IsNullOrEmpty(result.ErrorMessage))
                {
                    return ParseState.Failed;
                }

                // 計算核心欄位完整性
                var coreFieldsPresent = GetCoreFieldsCompleteCount(result);
                var totalCoreFields = 4; // Amount, Type, Category, Description

                // 檢查整體信心度
                var overallConfidence = result.ParseConfidence;

                // 決定解析狀態
                if (coreFieldsPresent == totalCoreFields && overallConfidence >= 0.8)
                {
                    return ParseState.Completed;
                }
                else if (coreFieldsPresent >= 3 && overallConfidence >= 0.6)
                {
                    return ParseState.PartialSuccess;
                }
                else if (coreFieldsPresent >= 2 || overallConfidence >= 0.4)
                {
                    return ParseState.RequiresInput;
                }
                else
                {
                    return ParseState.Failed;
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
                // 檢查核心欄位
                if (!result.Amount.HasValue || result.Amount <= 0)
                {
                    hints.Add(new MissingFieldHint
                    {
                        FieldName = "Amount",
                        DisplayName = "金額",
                        Suggestion = "請說明消費或收入的具體金額，例如：\"150元\" 或 \"一千五百塊\"",
                        Priority = HintPriority.High,
                        Icon = "fas fa-dollar-sign"
                    });
                }

                if (string.IsNullOrEmpty(result.Category))
                {
                    hints.Add(new MissingFieldHint
                    {
                        FieldName = "Category", 
                        DisplayName = "分類",
                        Suggestion = "請說明這筆記錄的類別，例如：\"餐飲\"、\"交通\"、\"購物\"",
                        Priority = HintPriority.High,
                        Icon = "fas fa-tags"
                    });
                }

                if (string.IsNullOrEmpty(result.Description) || result.Description == "語音記帳")
                {
                    hints.Add(new MissingFieldHint
                    {
                        FieldName = "Description",
                        DisplayName = "描述",
                        Suggestion = "請描述這筆記錄的內容，例如：\"買午餐\"、\"加油\"、\"看電影\"",
                        Priority = HintPriority.Medium,
                        Icon = "fas fa-comment"
                    });
                }

                // 檢查次要欄位
                if (!result.Date.HasValue)
                {
                    hints.Add(new MissingFieldHint
                    {
                        FieldName = "Date",
                        DisplayName = "日期",
                        Suggestion = "請說明記錄日期，例如：\"今天\"、\"昨天\"、\"1月15號\"",
                        Priority = HintPriority.Medium,
                        Icon = "fas fa-calendar"
                    });
                }

                if (string.IsNullOrEmpty(result.PaymentMethod))
                {
                    hints.Add(new MissingFieldHint
                    {
                        FieldName = "PaymentMethod",
                        DisplayName = "付款方式",
                        Suggestion = "請說明付款方式，例如：\"現金\"、\"信用卡\"、\"悠遊卡\"",
                        Priority = HintPriority.Low,
                        Icon = "fas fa-credit-card"
                    });
                }

                if (string.IsNullOrEmpty(result.MerchantName))
                {
                    hints.Add(new MissingFieldHint
                    {
                        FieldName = "MerchantName",
                        DisplayName = "商家名稱",
                        Suggestion = "請說明商家或地點，例如：\"7-11\"、\"星巴克\"、\"全聯\"",
                        Priority = HintPriority.Low,
                        Icon = "fas fa-store"
                    });
                }

                // 按優先級排序
                return hints.OrderByDescending(h => h.Priority).ThenBy(h => h.DisplayName).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成缺失欄位提示時發生錯誤");
                return new List<MissingFieldHint>();
            }
        }

        /// <summary>
        /// 生成下一步建議
        /// </summary>
        public string GenerateNextStepSuggestion(ParseState parseState, List<MissingFieldHint> hints)
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

        /// <summary>
        /// 檢查信心度是否足夠
        /// </summary>
        public bool IsConfidenceAcceptable(VoiceParseResult result, double threshold = 0.6)
        {
            return result.ParseConfidence >= threshold;
        }

        /// <summary>
        /// 評估是否需要用戶確認
        /// </summary>
        public bool RequiresUserConfirmation(VoiceParseResult result)
        {
            // 低信心度的重要欄位需要確認
            return result.FieldConfidence.Any(kvp => 
                IsImportantField(kvp.Key) && kvp.Value < 0.7);
        }

        /// <summary>
        /// 獲取建議的修正欄位
        /// </summary>
        public List<string> GetSuggestedFieldsForReview(VoiceParseResult result)
        {
            var fieldsToReview = new List<string>();

            foreach (var fieldConfidence in result.FieldConfidence)
            {
                if (fieldConfidence.Value < 0.6)
                {
                    fieldsToReview.Add(fieldConfidence.Key);
                }
            }

            return fieldsToReview.OrderBy(field => result.FieldConfidence.GetValueOrDefault(field, 0)).ToList();
        }

        // === 私有輔助方法 ===

        private int GetCoreFieldsCompleteCount(VoiceParseResult result)
        {
            int count = 0;

            if (result.Amount.HasValue && result.Amount > 0) count++;
            if (!string.IsNullOrEmpty(result.Type)) count++;
            if (!string.IsNullOrEmpty(result.Category)) count++;
            if (!string.IsNullOrEmpty(result.Description) && result.Description != "語音記帳") count++;

            return count;
        }

        private bool IsImportantField(string fieldName)
        {
            var importantFields = new[] { "Amount", "Type", "Category", "Description" };
            return importantFields.Contains(fieldName);
        }
    }
}
