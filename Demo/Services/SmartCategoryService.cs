using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Demo.Models;

namespace Demo.Services
{
    /// <summary>
    /// 智能分類服務 - 提供分類推薦和建議功能
    /// </summary>
    public class SmartCategoryService
    {
        private readonly TextAnalysisService _textAnalysis;
        private readonly string _rulesPath;
        private readonly string _trainingDataPath;
        private readonly string _merchantMappingPath;
        
        public SmartCategoryService(TextAnalysisService textAnalysis)
        {
            _textAnalysis = textAnalysis;
            _rulesPath = Path.Combine("App_Data", "category-rules.json");
            _trainingDataPath = Path.Combine("App_Data", "category-training.json");
            _merchantMappingPath = Path.Combine("App_Data", "merchant-mapping.json");
        }

        /// <summary>
        /// 主要分類推薦方法
        /// </summary>
        public async Task<List<CategorySuggestion>> SuggestCategoriesAsync(
            string description, 
            decimal amount, 
            string merchant = "",
            int maxSuggestions = 5)
        {
            var suggestions = new List<CategorySuggestion>();
            
            try
            {
                // 1. 規則引擎匹配
                var ruleBased = await GetRuleBasedSuggestionsAsync(description, merchant, amount);
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
                return ConsolidateAndRankSuggestions(suggestions, maxSuggestions);
            }
            catch (Exception ex)
            {
                // 記錄錯誤但不拋出異常
                Console.WriteLine($"智能分類建議發生錯誤: {ex.Message}");
                return new List<CategorySuggestion>();
            }
        }

        /// <summary>
        /// 規則引擎匹配
        /// </summary>
        private async Task<List<CategorySuggestion>> GetRuleBasedSuggestionsAsync(
            string description, string merchant, decimal amount)
        {
            var rules = await LoadCategoryRulesAsync();
            var suggestions = new List<CategorySuggestion>();
            
            foreach (var rule in rules.Where(r => r.IsActive))
            {
                var confidence = CalculateRuleMatch(rule, description, merchant, amount);
                if (confidence >= rule.MinConfidence)
                {
                    suggestions.Add(new CategorySuggestion
                    {
                        CategoryId = rule.CategoryId,
                        Confidence = confidence,
                        Reason = $"規則匹配: {rule.Name}",
                        SourceType = SuggestionSourceType.RuleBased
                    });

                    // 更新規則使用統計
                    rule.UsageCount++;
                    rule.LastUsed = DateTime.Now;
                }
            }

            // 保存規則更新
            if (suggestions.Any())
            {
                await SaveCategoryRulesAsync(rules);
            }
            
            return suggestions;
        }

        /// <summary>
        /// 關鍵字匹配
        /// </summary>
        private Task<List<CategorySuggestion>> GetKeywordBasedSuggestionsAsync(string description)
        {
            var suggestions = new List<CategorySuggestion>();
            var keywords = _textAnalysis.ExtractKeywords(description);
            
            // 基於預定義關鍵字對應
            var keywordMappings = GetKeywordCategoryMappings();
            
            foreach (var keyword in keywords)
            {
                foreach (var mapping in keywordMappings)
                {
                    if (mapping.Value.Any(k => k.Contains(keyword) || keyword.Contains(k)))
                    {
                        var confidence = CalculateKeywordConfidence(keyword, mapping.Value);
                        suggestions.Add(new CategorySuggestion
                        {
                            CategoryId = mapping.Key,
                            Confidence = confidence,
                            Reason = $"關鍵字匹配: {keyword}",
                            SourceType = SuggestionSourceType.KeywordBased
                        });
                    }
                }
            }
            
            return Task.FromResult(suggestions);
        }

        /// <summary>
        /// 歷史記錄相似度匹配
        /// </summary>
        private async Task<List<CategorySuggestion>> GetHistoryBasedSuggestionsAsync(
            string description, decimal amount)
        {
            var trainingData = await LoadTrainingDataAsync();
            var suggestions = new List<CategorySuggestion>();
            
            // 只考慮正確的歷史記錄
            var correctRecords = trainingData.Where(t => t.IsCorrect).ToList();
            
            foreach (var record in correctRecords)
            {
                var textSimilarity = _textAnalysis.CalculateSimilarity(description, record.Description);
                var amountSimilarity = CalculateAmountSimilarity(amount, record.Amount);
                
                // 組合相似度 (文字權重70%, 金額權重30%)
                var combinedScore = (textSimilarity * 0.7) + (amountSimilarity * 0.3);
                
                if (combinedScore > 0.6)
                {
                    suggestions.Add(new CategorySuggestion
                    {
                        CategoryId = record.CategoryId,
                        Confidence = combinedScore,
                        Reason = $"相似記錄: {record.Description.Substring(0, Math.Min(20, record.Description.Length))}...",
                        SourceType = SuggestionSourceType.HistoryBased
                    });
                }
            }
            
            return suggestions.OrderByDescending(s => s.Confidence).Take(3).ToList();
        }

        /// <summary>
        /// 商家匹配
        /// </summary>
        private async Task<List<CategorySuggestion>> GetMerchantBasedSuggestionsAsync(string merchant)
        {
            if (string.IsNullOrWhiteSpace(merchant))
                return new List<CategorySuggestion>();

            var suggestions = new List<CategorySuggestion>();
            var merchantMappings = await LoadMerchantMappingsAsync();
            
            foreach (var mapping in merchantMappings)
            {
                if (IsMerchantMatch(merchant, mapping))
                {
                    suggestions.Add(new CategorySuggestion
                    {
                        CategoryId = mapping.CategoryId,
                        Confidence = mapping.Confidence,
                        Reason = $"商家匹配: {mapping.StandardName}",
                        SourceType = SuggestionSourceType.MerchantBased
                    });
                }
            }
            
            return suggestions;
        }

        /// <summary>
        /// 金額範圍匹配
        /// </summary>
        private Task<List<CategorySuggestion>> GetAmountBasedSuggestionsAsync(decimal amount)
        {
            var suggestions = new List<CategorySuggestion>();
            var amountRangeMappings = GetAmountRangeMappings();
            
            foreach (var mapping in amountRangeMappings)
            {
                if (amount >= mapping.MinAmount && amount <= mapping.MaxAmount)
                {
                    suggestions.Add(new CategorySuggestion
                    {
                        CategoryId = mapping.CategoryId,
                        Confidence = 0.3, // 金額匹配的信心度較低
                        Reason = $"金額範圍: {mapping.MinAmount}-{mapping.MaxAmount}",
                        SourceType = SuggestionSourceType.AmountBased
                    });
                }
            }
            
            return Task.FromResult(suggestions);
        }

        /// <summary>
        /// 計算規則匹配度
        /// </summary>
        private double CalculateRuleMatch(CategoryRule rule, string description, string merchant, decimal amount)
        {
            double score = 0.0;
            int factors = 0;

            // 關鍵字匹配
            if (rule.Keywords.Any())
            {
                var matchingKeywords = rule.Keywords.Count(k => 
                    description.Contains(k, StringComparison.OrdinalIgnoreCase));
                score += (double)matchingKeywords / rule.Keywords.Count * 0.4;
                factors++;
            }

            // 商家模式匹配
            if (rule.MerchantPatterns.Any() && !string.IsNullOrWhiteSpace(merchant))
            {
                var merchantMatch = rule.MerchantPatterns.Any(p => 
                    merchant.Contains(p, StringComparison.OrdinalIgnoreCase));
                score += merchantMatch ? 0.3 : 0.0;
                factors++;
            }

            // 金額範圍匹配
            if (rule.MinAmount.HasValue || rule.MaxAmount.HasValue)
            {
                bool amountMatch = true;
                if (rule.MinAmount.HasValue && amount < rule.MinAmount.Value)
                    amountMatch = false;
                if (rule.MaxAmount.HasValue && amount > rule.MaxAmount.Value)
                    amountMatch = false;
                
                score += amountMatch ? 0.3 : 0.0;
                factors++;
            }

            return factors > 0 ? score / factors : 0.0;
        }

        /// <summary>
        /// 計算關鍵字信心度
        /// </summary>
        private double CalculateKeywordConfidence(string keyword, List<string> categoryKeywords)
        {
            // 完全匹配
            if (categoryKeywords.Contains(keyword))
                return 0.8;
                
            // 部分匹配
            var partialMatches = categoryKeywords.Count(k => 
                k.Contains(keyword) || keyword.Contains(k));
                
            return Math.Min(0.6, partialMatches * 0.2);
        }

        /// <summary>
        /// 計算金額相似度
        /// </summary>
        private double CalculateAmountSimilarity(decimal amount1, decimal amount2)
        {
            if (amount1 == 0 && amount2 == 0)
                return 1.0;
                
            var ratio = (double)(Math.Min(amount1, amount2) / Math.Max(amount1, amount2));
            return Math.Max(0, ratio - 0.2); // 20% 以下的差異被視為不相似
        }

        /// <summary>
        /// 檢查商家匹配
        /// </summary>
        private bool IsMerchantMatch(string merchant, MerchantMapping mapping)
        {
            var normalizedMerchant = merchant.ToLower();
            var normalizedStandard = mapping.StandardName.ToLower();
            
            // 完全匹配
            if (normalizedMerchant.Contains(normalizedStandard))
                return true;
                
            // 別名匹配
            return mapping.Aliases.Any(alias => 
                normalizedMerchant.Contains(alias.ToLower()));
        }

        /// <summary>
        /// 合併和排序建議
        /// </summary>
        private List<CategorySuggestion> ConsolidateAndRankSuggestions(
            List<CategorySuggestion> suggestions, int maxSuggestions)
        {
            // 按分類分組，取最高信心度
            var groupedSuggestions = suggestions
                .GroupBy(s => s.CategoryId)
                .Select(g => new CategorySuggestion
                {
                    CategoryId = g.Key,
                    Confidence = g.Max(x => x.Confidence),
                    Reason = g.OrderByDescending(x => x.Confidence).First().Reason,
                    SourceType = g.OrderByDescending(x => x.Confidence).First().SourceType
                })
                .OrderByDescending(s => s.Confidence)
                .Take(maxSuggestions)
                .ToList();

            // 填充分類名稱和圖示
            foreach (var suggestion in groupedSuggestions)
            {
                var categoryInfo = GetCategoryInfo(suggestion.CategoryId);
                suggestion.CategoryName = categoryInfo.Name;
                suggestion.IconClass = categoryInfo.IconClass;
            }

            return groupedSuggestions;
        }

        /// <summary>
        /// 載入分類規則
        /// </summary>
        private async Task<List<CategoryRule>> LoadCategoryRulesAsync()
        {
            if (!File.Exists(_rulesPath))
            {
                var defaultRules = CreateDefaultRules();
                await SaveCategoryRulesAsync(defaultRules);
                return defaultRules;
            }

            try
            {
                var json = await File.ReadAllTextAsync(_rulesPath);
                return JsonSerializer.Deserialize<List<CategoryRule>>(json) ?? new List<CategoryRule>();
            }
            catch
            {
                return new List<CategoryRule>();
            }
        }

        /// <summary>
        /// 保存分類規則
        /// </summary>
        private async Task SaveCategoryRulesAsync(List<CategoryRule> rules)
        {
            try
            {
                var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_rulesPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存分類規則失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 載入訓練資料
        /// </summary>
        private async Task<List<CategoryTrainingData>> LoadTrainingDataAsync()
        {
            if (!File.Exists(_trainingDataPath))
                return new List<CategoryTrainingData>();

            try
            {
                var json = await File.ReadAllTextAsync(_trainingDataPath);
                return JsonSerializer.Deserialize<List<CategoryTrainingData>>(json) ?? new List<CategoryTrainingData>();
            }
            catch
            {
                return new List<CategoryTrainingData>();
            }
        }

        /// <summary>
        /// 載入商家對應
        /// </summary>
        private async Task<List<MerchantMapping>> LoadMerchantMappingsAsync()
        {
            if (!File.Exists(_merchantMappingPath))
                return new List<MerchantMapping>();

            try
            {
                var json = await File.ReadAllTextAsync(_merchantMappingPath);
                return JsonSerializer.Deserialize<List<MerchantMapping>>(json) ?? new List<MerchantMapping>();
            }
            catch
            {
                return new List<MerchantMapping>();
            }
        }

        /// <summary>
        /// 建立預設規則
        /// </summary>
        private List<CategoryRule> CreateDefaultRules()
        {
            return new List<CategoryRule>
            {
                new CategoryRule
                {
                    Name = "餐飲類規則",
                    CategoryId = "food",
                    Keywords = new List<string> { "早餐", "午餐", "晚餐", "宵夜", "咖啡", "飲料", "便當" },
                    MerchantPatterns = new List<string> { "餐廳", "小吃", "麥當勞", "肯德基", "星巴克" },
                    Priority = 1
                },
                new CategoryRule
                {
                    Name = "交通類規則",
                    CategoryId = "transport",
                    Keywords = new List<string> { "捷運", "公車", "計程車", "uber", "油錢", "停車費" },
                    MerchantPatterns = new List<string> { "中油", "台塑", "停車場" },
                    Priority = 1
                },
                new CategoryRule
                {
                    Name = "購物類規則",
                    CategoryId = "shopping",
                    Keywords = new List<string> { "衣服", "鞋子", "包包", "化妝品", "書籍" },
                    MerchantPatterns = new List<string> { "百貨", "商場", "網購", "蝦皮", "momo" },
                    Priority = 1
                }
            };
        }

        /// <summary>
        /// 取得關鍵字分類對應
        /// </summary>
        private Dictionary<string, List<string>> GetKeywordCategoryMappings()
        {
            return new Dictionary<string, List<string>>
            {
                ["food"] = new List<string> { "早餐", "午餐", "晚餐", "宵夜", "餐廳", "小吃", "飲料", "咖啡", "茶", "便當", "麵", "飯", "湯" },
                ["transport"] = new List<string> { "捷運", "公車", "計程車", "uber", "油錢", "停車", "高速公路", "過路費", "機票", "火車" },
                ["shopping"] = new List<string> { "衣服", "鞋子", "包包", "化妝品", "書籍", "文具", "電子產品", "手機", "電腦" },
                ["medical"] = new List<string> { "看病", "買藥", "健檢", "牙醫", "眼科", "藥局", "醫院", "診所", "藥品" },
                ["entertainment"] = new List<string> { "電影", "ktv", "遊戲", "旅遊", "運動", "健身", "spa", "按摩", "唱歌" },
                ["daily"] = new List<string> { "日用品", "清潔用品", "衛生紙", "洗髮精", "牙膏", "肥皂", "洗衣精" }
            };
        }

        /// <summary>
        /// 取得金額範圍對應
        /// </summary>
        private List<AmountRangeMapping> GetAmountRangeMappings()
        {
            return new List<AmountRangeMapping>
            {
                new AmountRangeMapping { CategoryId = "food", MinAmount = 50, MaxAmount = 500 },
                new AmountRangeMapping { CategoryId = "transport", MinAmount = 20, MaxAmount = 200 },
                new AmountRangeMapping { CategoryId = "daily", MinAmount = 30, MaxAmount = 300 },
                new AmountRangeMapping { CategoryId = "entertainment", MinAmount = 200, MaxAmount = 2000 }
            };
        }

        /// <summary>
        /// 取得分類資訊
        /// </summary>
        private (string Name, string IconClass) GetCategoryInfo(string categoryId)
        {
            var categoryMappings = new Dictionary<string, (string, string)>
            {
                ["food"] = ("餐飲", "fas fa-utensils"),
                ["transport"] = ("交通", "fas fa-car"),
                ["shopping"] = ("購物", "fas fa-shopping-cart"),
                ["medical"] = ("醫療", "fas fa-heartbeat"),
                ["entertainment"] = ("娛樂", "fas fa-gamepad"),
                ["daily"] = ("日常", "fas fa-home")
            };

            return categoryMappings.TryGetValue(categoryId, out var info) ? 
                info : ("其他", "fas fa-question");
        }
    }

    /// <summary>
    /// 金額範圍對應類別
    /// </summary>
    public class AmountRangeMapping
    {
        public string CategoryId { get; set; } = string.Empty;
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
    }
}
