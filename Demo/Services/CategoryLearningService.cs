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
    /// 分類學習服務 - 負責從用戶回饋中學習和改進分類準確度
    /// </summary>
    public class CategoryLearningService
    {
        private readonly string _trainingDataPath;
        private readonly string _rulesPath;
        private readonly string _merchantMappingPath;
        private readonly TextAnalysisService _textAnalysis;

        public CategoryLearningService(TextAnalysisService textAnalysis)
        {
            _textAnalysis = textAnalysis;
            _trainingDataPath = Path.Combine("App_Data", "category-training.json");
            _rulesPath = Path.Combine("App_Data", "category-rules.json");
            _merchantMappingPath = Path.Combine("App_Data", "merchant-mapping.json");
        }

        /// <summary>
        /// 從用戶回饋中學習
        /// </summary>
        public async Task LearnFromFeedbackAsync(CategoryFeedback feedback)
        {
            try
            {
                var trainingData = await LoadTrainingDataAsync();
                
                // 提取特徵
                var features = _textAnalysis.ExtractFeatures(feedback.Description, feedback.Amount, feedback.Merchant);
                
                // 新增訓練資料
                var newTrainingData = new CategoryTrainingData
                {
                    Description = feedback.Description,
                    Amount = feedback.Amount,
                    Merchant = feedback.Merchant,
                    CategoryId = feedback.CategoryId,
                    IsCorrect = feedback.IsCorrect,
                    UserId = feedback.UserId,
                    CreatedAt = feedback.Timestamp,
                    Features = features
                };

                trainingData.Add(newTrainingData);
                await SaveTrainingDataAsync(trainingData);

                // 如果是正確的分類，更新相關規則和對應
                if (feedback.IsCorrect)
                {
                    await UpdateRuleWeightsAsync(feedback);
                    await UpdateMerchantMappingAsync(feedback);
                }
                else
                {
                    // 如果是錯誤的分類，記錄以便調整規則權重
                    await AdjustRuleWeightsForIncorrectFeedbackAsync(feedback);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"學習回饋時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 從訓練資料自動生成規則
        /// </summary>
        public async Task GenerateRulesFromTrainingDataAsync()
        {
            try
            {
                var trainingData = await LoadTrainingDataAsync();
                var correctData = trainingData.Where(t => t.IsCorrect).ToList();
                
                if (correctData.Count < 10) // 需要足夠的資料才能生成規則
                    return;
                
                // 按分類群組
                var categoryGroups = correctData.GroupBy(t => t.CategoryId);
                
                foreach (var group in categoryGroups)
                {
                    if (group.Count() >= 3) // 每個分類至少要有3筆資料
                    {
                        await GenerateRulesForCategoryAsync(group.Key, group.ToList());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"生成規則時發生錯誤: {ex.Message}");
            }
        }

        /// <summary>
        /// 為特定分類生成規則
        /// </summary>
        private async Task GenerateRulesForCategoryAsync(string categoryId, List<CategoryTrainingData> categoryData)
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
                    Name = $"自動生成規則 - {categoryId} ({DateTime.Now:yyyy-MM-dd})",
                    CategoryId = categoryId,
                    Keywords = keywordFrequency.Take(10).Select(kv => kv.Key).ToList(),
                    MerchantPatterns = commonMerchants.Take(5).ToList(),
                    MinAmount = amountRanges.Min > 0 ? amountRanges.Min : null,
                    MaxAmount = amountRanges.Max > amountRanges.Min ? amountRanges.Max : null,
                    MinConfidence = CalculateMinConfidenceForRule(categoryData.Count),
                    Priority = CalculateRulePriority(categoryData.Count),
                    UsageCount = 0,
                    CreatedAt = DateTime.Now
                };

                await SaveRuleAsync(rule);
            }
        }

        /// <summary>
        /// 分析關鍵字頻率
        /// </summary>
        private Dictionary<string, int> AnalyzeKeywordFrequency(List<CategoryTrainingData> data)
        {
            var keywords = new Dictionary<string, int>();
            
            foreach (var item in data)
            {
                var tokens = _textAnalysis.TokenizeAndNormalize(item.Description);
                foreach (var token in tokens)
                {
                    if (token.Length > 2) // 忽略太短的詞
                    {
                        keywords[token] = keywords.GetValueOrDefault(token, 0) + 1;
                    }
                }
            }
            
            // 只保留出現頻率較高的關鍵字
            var threshold = Math.Max(2, data.Count / 3);
            return keywords
                .Where(kv => kv.Value >= threshold)
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        /// <summary>
        /// 分析商家模式
        /// </summary>
        private List<string> AnalyzeMerchantPatterns(List<CategoryTrainingData> data)
        {
            var merchants = data
                .Where(d => !string.IsNullOrWhiteSpace(d.Merchant))
                .GroupBy(d => d.Merchant.ToLower())
                .Where(g => g.Count() >= 2) // 至少出現2次
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .ToList();

            return merchants;
        }

        /// <summary>
        /// 分析金額範圍
        /// </summary>
        private (decimal Min, decimal Max) AnalyzeAmountRanges(List<CategoryTrainingData> data)
        {
            var amounts = data.Select(d => d.Amount).Where(a => a > 0).ToList();
            
            if (!amounts.Any())
                return (0, 0);

            var sortedAmounts = amounts.OrderBy(a => a).ToList();
            
            // 使用四分位數來確定範圍，排除極端值
            var q1Index = sortedAmounts.Count / 4;
            var q3Index = (sortedAmounts.Count * 3) / 4;
            
            var min = q1Index < sortedAmounts.Count ? sortedAmounts[q1Index] : sortedAmounts.First();
            var max = q3Index < sortedAmounts.Count ? sortedAmounts[q3Index] : sortedAmounts.Last();
            
            return (min, max);
        }

        /// <summary>
        /// 計算規則最小信心度
        /// </summary>
        private double CalculateMinConfidenceForRule(int sampleCount)
        {
            return sampleCount switch
            {
                >= 20 => 0.8,
                >= 10 => 0.7,
                >= 5 => 0.6,
                _ => 0.5
            };
        }

        /// <summary>
        /// 計算規則優先級
        /// </summary>
        private int CalculateRulePriority(int sampleCount)
        {
            return sampleCount switch
            {
                >= 50 => 3,
                >= 20 => 2,
                >= 10 => 1,
                _ => 0
            };
        }

        /// <summary>
        /// 更新規則權重
        /// </summary>
        private async Task UpdateRuleWeightsAsync(CategoryFeedback feedback)
        {
            var rules = await LoadRulesAsync();
            
            foreach (var rule in rules.Where(r => r.CategoryId == feedback.CategoryId))
            {
                // 檢查規則是否匹配這筆回饋
                if (IsRuleMatching(rule, feedback))
                {
                    rule.UsageCount++;
                    rule.LastUsed = DateTime.Now;
                    
                    // 提升匹配規則的信心度
                    rule.MinConfidence = Math.Min(0.95, rule.MinConfidence + 0.01);
                }
            }
            
            await SaveRulesAsync(rules);
        }

        /// <summary>
        /// 更新商家對應
        /// </summary>
        private async Task UpdateMerchantMappingAsync(CategoryFeedback feedback)
        {
            if (string.IsNullOrWhiteSpace(feedback.Merchant))
                return;

            var mappings = await LoadMerchantMappingsAsync();
            var normalizedMerchant = feedback.Merchant.ToLower().Trim();
            
            // 檢查是否已存在對應
            var existingMapping = mappings.FirstOrDefault(m => 
                m.MerchantName.ToLower() == normalizedMerchant ||
                m.Aliases.Any(a => a.ToLower() == normalizedMerchant));
            
            if (existingMapping != null)
            {
                // 更新現有對應
                if (existingMapping.CategoryId != feedback.CategoryId)
                {
                    existingMapping.CategoryId = feedback.CategoryId;
                    existingMapping.Confidence = Math.Min(1.0, existingMapping.Confidence + 0.1);
                }
            }
            else
            {
                // 建立新的商家對應
                var newMapping = new MerchantMapping
                {
                    MerchantName = feedback.Merchant,
                    StandardName = _textAnalysis.ClassifyMerchantType(feedback.Merchant),
                    CategoryId = feedback.CategoryId,
                    MerchantType = _textAnalysis.ClassifyMerchantType(feedback.Merchant),
                    Confidence = 0.6,
                    IsVerified = false
                };
                
                mappings.Add(newMapping);
            }
            
            await SaveMerchantMappingsAsync(mappings);
        }

        /// <summary>
        /// 調整錯誤回饋的規則權重
        /// </summary>
        private async Task AdjustRuleWeightsForIncorrectFeedbackAsync(CategoryFeedback feedback)
        {
            var rules = await LoadRulesAsync();
            
            // 找到可能導致錯誤分類的規則
            foreach (var rule in rules)
            {
                if (IsRuleMatching(rule, feedback))
                {
                    // 降低匹配錯誤分類的規則信心度
                    rule.MinConfidence = Math.Max(0.1, rule.MinConfidence - 0.05);
                    
                    // 如果信心度太低，停用規則
                    if (rule.MinConfidence < 0.3)
                    {
                        rule.IsActive = false;
                    }
                }
            }
            
            await SaveRulesAsync(rules);
        }

        /// <summary>
        /// 檢查規則是否匹配回饋
        /// </summary>
        private bool IsRuleMatching(CategoryRule rule, CategoryFeedback feedback)
        {
            // 關鍵字匹配檢查
            var keywordMatch = rule.Keywords.Any(k => 
                feedback.Description.Contains(k, StringComparison.OrdinalIgnoreCase));
            
            // 商家匹配檢查
            var merchantMatch = string.IsNullOrWhiteSpace(feedback.Merchant) || 
                rule.MerchantPatterns.Any(p => 
                    feedback.Merchant.Contains(p, StringComparison.OrdinalIgnoreCase));
            
            // 金額範圍檢查
            var amountMatch = (!rule.MinAmount.HasValue || feedback.Amount >= rule.MinAmount) &&
                             (!rule.MaxAmount.HasValue || feedback.Amount <= rule.MaxAmount);
            
            return keywordMatch || (merchantMatch && amountMatch);
        }

        /// <summary>
        /// 評估模型準確度
        /// </summary>
        public async Task<ModelAccuracyReport> EvaluateModelAccuracyAsync()
        {
            try
            {
                var trainingData = await LoadTrainingDataAsync();
                var testData = trainingData.TakeLast(Math.Min(100, trainingData.Count)).ToList();
                
                if (!testData.Any())
                {
                    return new ModelAccuracyReport
                    {
                        OverallAccuracy = 0,
                        TotalTestCases = 0,
                        CorrectPredictions = 0,
                        EvaluationDate = DateTime.Now
                    };
                }
                
                int correctPredictions = 0;
                var categoryAccuracy = new Dictionary<string, (int correct, int total)>();
                
                // 建立臨時的智能分類服務來測試
                var smartCategoryService = new SmartCategoryService(_textAnalysis);
                
                foreach (var testItem in testData)
                {
                    var suggestions = await smartCategoryService.SuggestCategoriesAsync(
                        testItem.Description, 
                        testItem.Amount, 
                        testItem.Merchant, 
                        1);
                        
                    var topSuggestion = suggestions.FirstOrDefault();
                    var isCorrect = topSuggestion?.CategoryId == testItem.CategoryId;
                    
                    if (isCorrect)
                    {
                        correctPredictions++;
                    }
                    
                    // 分類別統計
                    var categoryKey = testItem.CategoryId;
                    if (!categoryAccuracy.ContainsKey(categoryKey))
                    {
                        categoryAccuracy[categoryKey] = (0, 0);
                    }
                    
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
                    EvaluationDate = DateTime.Now,
                    DetailedPerformance = GenerateDetailedPerformance(categoryAccuracy)
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"評估模型準確度時發生錯誤: {ex.Message}");
                return new ModelAccuracyReport
                {
                    OverallAccuracy = 0,
                    EvaluationDate = DateTime.Now
                };
            }
        }

        /// <summary>
        /// 生成詳細效能報告
        /// </summary>
        private List<CategoryPerformance> GenerateDetailedPerformance(
            Dictionary<string, (int correct, int total)> categoryAccuracy)
        {
            return categoryAccuracy.Select(kv => new CategoryPerformance
            {
                CategoryId = kv.Key,
                CategoryName = GetCategoryName(kv.Key),
                TotalCases = kv.Value.total,
                CorrectPredictions = kv.Value.correct,
                Accuracy = (double)kv.Value.correct / kv.Value.total,
                AverageConfidence = 0.8 // 這裡可以進一步計算平均信心度
            }).ToList();
        }

        /// <summary>
        /// 取得分類名稱
        /// </summary>
        private string GetCategoryName(string categoryId)
        {
            var categoryNames = new Dictionary<string, string>
            {
                ["food"] = "餐飲",
                ["transport"] = "交通",
                ["shopping"] = "購物",
                ["medical"] = "醫療",
                ["entertainment"] = "娛樂",
                ["daily"] = "日常"
            };

            return categoryNames.TryGetValue(categoryId, out var name) ? name : "其他";
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
        /// 保存訓練資料
        /// </summary>
        private async Task SaveTrainingDataAsync(List<CategoryTrainingData> data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_trainingDataPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存訓練資料失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 載入規則
        /// </summary>
        private async Task<List<CategoryRule>> LoadRulesAsync()
        {
            if (!File.Exists(_rulesPath))
                return new List<CategoryRule>();

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
        /// 保存規則
        /// </summary>
        private async Task SaveRulesAsync(List<CategoryRule> rules)
        {
            try
            {
                var json = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_rulesPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存規則失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存單一規則
        /// </summary>
        private async Task SaveRuleAsync(CategoryRule rule)
        {
            var rules = await LoadRulesAsync();
            rules.Add(rule);
            await SaveRulesAsync(rules);
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
        /// 保存商家對應
        /// </summary>
        private async Task SaveMerchantMappingsAsync(List<MerchantMapping> mappings)
        {
            try
            {
                var json = JsonSerializer.Serialize(mappings, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(_merchantMappingPath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存商家對應失敗: {ex.Message}");
            }
        }
    }
}
