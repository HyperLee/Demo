using Demo.Models;

namespace Demo.Services
{
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
                if (!parsedDate.HasValue)
                    return 0.0;

                double confidence = 0.0;

                // 1. 基於匹配模式的基礎信心度
                confidence += GetPatternConfidence(matchedPattern);

                // 2. 日期合理性檢查
                confidence *= CheckDateReasonability(parsedDate.Value);

                // 3. 上下文一致性檢查
                confidence *= CheckDateContextConsistency(originalText, parsedDate.Value);

                return Math.Min(Math.Max(confidence, 0.0), 1.0);
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
                if (!parsedAmount.HasValue || parsedAmount <= 0)
                    return 0.0;

                double confidence = 0.8; // 數字解析通常比較準確

                // 1. 金額合理性檢查
                confidence *= CheckAmountReasonability(parsedAmount.Value);

                // 2. 金額相關關鍵字檢查
                confidence *= CheckAmountKeywords(originalText);

                // 3. 格式一致性檢查
                confidence *= CheckAmountFormat(originalText, parsedAmount.Value);

                return Math.Min(Math.Max(confidence, 0.0), 1.0);
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
                if (string.IsNullOrEmpty(category))
                    return 0.0;

                double confidence = 0.5; // 基礎信心度

                // 1. 直接分類匹配
                if (CheckDirectCategoryMatch(originalText, category))
                    confidence += 0.3;

                // 2. 商家-分類對應檢查
                if (!string.IsNullOrEmpty(merchantName) && CheckMerchantCategoryMapping(merchantName, category))
                    confidence += 0.2;

                // 3. 描述-分類對應檢查
                if (!string.IsNullOrEmpty(description) && CheckDescriptionCategoryMapping(description, category))
                    confidence += 0.1;

                // 4. 分類一致性檢查
                confidence *= CheckCategoryConsistency(merchantName, description, category);

                return Math.Min(Math.Max(confidence, 0.0), 1.0);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "計算分類信心度時發生錯誤");
                return 0.0;
            }
        }

        /// <summary>
        /// 計算收支類型信心度
        /// </summary>
        public double CalculateTypeConfidence(string originalText, string type)
        {
            try
            {
                var incomeKeywords = new[] { "收入", "賺", "薪水", "獎金", "紅包", "利息", "分紅", "投資收益" };
                var expenseKeywords = new[] { "支出", "花", "買", "付", "給", "消費", "開銷" };

                double confidence = 0.5; // 預設信心度

                if (type == "Income")
                {
                    var matchCount = incomeKeywords.Count(keyword => originalText.Contains(keyword));
                    confidence = Math.Min(0.5 + (matchCount * 0.2), 1.0);
                }
                else if (type == "Expense")
                {
                    var matchCount = expenseKeywords.Count(keyword => originalText.Contains(keyword));
                    confidence = Math.Min(0.5 + (matchCount * 0.15), 1.0);
                }

                return confidence;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "計算收支類型信心度時發生錯誤");
                return 0.3;
            }
        }

        /// <summary>
        /// 計算付款方式信心度
        /// </summary>
        public double CalculatePaymentMethodConfidence(string originalText, string paymentMethod)
        {
            try
            {
                if (string.IsNullOrEmpty(paymentMethod))
                    return 0.0;

                // 直接匹配檢查
                if (originalText.Contains(paymentMethod, StringComparison.OrdinalIgnoreCase))
                    return 0.9;

                // 關鍵字匹配檢查
                var paymentKeywords = new Dictionary<string, string[]>
                {
                    { "現金", new[] { "現金", "cash", "付現" } },
                    { "信用卡", new[] { "信用卡", "刷卡", "credit", "visa", "master" } },
                    { "悠遊卡", new[] { "悠遊卡", "easycard" } },
                    { "一卡通", new[] { "一卡通", "ipass" } }
                };

                if (paymentKeywords.ContainsKey(paymentMethod))
                {
                    var keywords = paymentKeywords[paymentMethod];
                    var matchCount = keywords.Count(keyword => originalText.Contains(keyword, StringComparison.OrdinalIgnoreCase));
                    return matchCount > 0 ? 0.8 : 0.3;
                }

                return 0.3;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "計算付款方式信心度時發生錯誤");
                return 0.0;
            }
        }

        // === 私有輔助方法 ===

        private double GetPatternConfidence(string pattern)
        {
            return pattern switch
            {
                "relative" => 0.9,    // 相對日期 (今天、昨天)
                "full" => 0.95,       // 完整日期格式
                "monthDay" => 0.8,    // 月日格式
                "chinese" => 0.7,     // 中文數字格式
                _ => 0.5
            };
        }

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
                { "星巴克", new[] { "餐飲美食", "飲料" } },
                { "麥當勞", new[] { "餐飲美食", "速食" } },
                { "全聯", new[] { "日用品", "生活用品" } },
                { "加油站", new[] { "交通運輸", "油費" } }
            };

            return merchantCategoryMap.ContainsKey(merchantName) && 
                   merchantCategoryMap[merchantName].Contains(category);
        }

        private bool CheckDescriptionCategoryMapping(string description, string category)
        {
            // 檢查描述與分類的對應關係
            var descriptionKeywords = new Dictionary<string, string[]>
            {
                { "餐飲美食", new[] { "吃", "喝", "早餐", "午餐", "晚餐", "咖啡" } },
                { "交通運輸", new[] { "加油", "停車", "計程車", "公車", "捷運" } },
                { "娛樂休閒", new[] { "電影", "唱歌", "遊戲", "旅遊" } },
                { "服飾美容", new[] { "衣服", "鞋子", "化妝品", "美容" } }
            };

            if (descriptionKeywords.ContainsKey(category))
            {
                var keywords = descriptionKeywords[category];
                return keywords.Any(keyword => description.Contains(keyword));
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
                if (!CheckMerchantCategoryMapping(merchantName, category))
                {
                    consistencyScore *= 0.8; // 不一致時降低信心度
                }
            }

            // 如果有描述資訊，檢查一致性
            if (!string.IsNullOrEmpty(description))
            {
                if (!CheckDescriptionCategoryMapping(description, category))
                {
                    consistencyScore *= 0.9; // 輕微降低信心度
                }
            }

            return consistencyScore;
        }
    }
}
