using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using Demo.Models;

namespace Demo.Services
{
    /// <summary>
    /// 文字分析服務 - 提供 TF-IDF 計算、相似度分析和關鍵字提取功能
    /// </summary>
    public class TextAnalysisService
    {
        private readonly Dictionary<string, List<string>> _keywordDictionary;
        private readonly List<string> _stopWords;
        private readonly Dictionary<string, string> _merchantAliases;

        public TextAnalysisService()
        {
            _keywordDictionary = LoadKeywordDictionary();
            _stopWords = LoadStopWords();
            _merchantAliases = LoadMerchantAliases();
        }

        /// <summary>
        /// 計算兩個文字之間的相似度 (使用 TF-IDF + 餘弦相似度)
        /// </summary>
        public double CalculateSimilarity(string text1, string text2)
        {
            if (string.IsNullOrWhiteSpace(text1) || string.IsNullOrWhiteSpace(text2))
                return 0.0;

            var tokens1 = TokenizeAndNormalize(text1);
            var tokens2 = TokenizeAndNormalize(text2);

            if (tokens1.Count == 0 || tokens2.Count == 0)
                return 0.0;

            var vector1 = CreateTfIdfVector(tokens1, new List<List<string>> { tokens1, tokens2 });
            var vector2 = CreateTfIdfVector(tokens2, new List<List<string>> { tokens1, tokens2 });

            return CalculateCosineSimilarity(vector1, vector2);
        }

        /// <summary>
        /// 文字正規化和分詞
        /// </summary>
        public List<string> TokenizeAndNormalize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            // 正規化文字
            var normalized = text.ToLower()
                .Replace("超商", "便利商店")
                .Replace("小七", "7-eleven")
                .Replace("7-11", "7-eleven")
                .Replace("全家", "familymart")
                .Replace("早餐店", "早餐")
                .Replace("晚餐店", "晚餐")
                .Replace("咖啡廳", "咖啡")
                .Replace("飲料店", "飲料");

            // 移除標點符號和特殊字符
            normalized = Regex.Replace(normalized, @"[^\w\s\u4e00-\u9fff]", " ");

            // 分詞
            var tokens = normalized.Split(new[] { ' ', '\t', '\n', '\r' }, 
                StringSplitOptions.RemoveEmptyEntries);

            // 過濾停用詞和短詞
            return tokens
                .Where(t => t.Length > 1 && !_stopWords.Contains(t))
                .ToList();
        }

        /// <summary>
        /// 創建 TF-IDF 向量
        /// </summary>
        private Dictionary<string, double> CreateTfIdfVector(List<string> tokens, List<List<string>> corpus)
        {
            var vector = new Dictionary<string, double>();
            var termFrequency = CalculateTermFrequency(tokens);

            foreach (var term in termFrequency.Keys)
            {
                var tf = termFrequency[term];
                var idf = CalculateInverseDocumentFrequency(term, corpus);
                vector[term] = tf * idf;
            }

            return vector;
        }

        /// <summary>
        /// 計算詞頻 (TF)
        /// </summary>
        private Dictionary<string, double> CalculateTermFrequency(List<string> tokens)
        {
            var termFreq = new Dictionary<string, double>();
            
            foreach (var token in tokens)
            {
                termFreq[token] = termFreq.GetValueOrDefault(token, 0) + 1;
            }

            // 正規化為相對頻率
            foreach (var key in termFreq.Keys.ToList())
            {
                termFreq[key] = termFreq[key] / tokens.Count;
            }

            return termFreq;
        }

        /// <summary>
        /// 計算逆向文件頻率 (IDF)
        /// </summary>
        private double CalculateInverseDocumentFrequency(string term, List<List<string>> corpus)
        {
            var documentsWithTerm = corpus.Count(doc => doc.Contains(term));
            
            if (documentsWithTerm == 0)
                return 0;

            return Math.Log((double)corpus.Count / documentsWithTerm);
        }

        /// <summary>
        /// 計算餘弦相似度
        /// </summary>
        private double CalculateCosineSimilarity(Dictionary<string, double> vector1, Dictionary<string, double> vector2)
        {
            var commonTerms = vector1.Keys.Intersect(vector2.Keys).ToList();
            
            if (commonTerms.Count == 0)
                return 0.0;

            double dotProduct = commonTerms.Sum(term => vector1[term] * vector2[term]);
            double magnitude1 = Math.Sqrt(vector1.Values.Sum(x => x * x));
            double magnitude2 = Math.Sqrt(vector2.Values.Sum(x => x * x));

            if (magnitude1 == 0 || magnitude2 == 0)
                return 0.0;

            return dotProduct / (magnitude1 * magnitude2);
        }

        /// <summary>
        /// 提取文字特徵
        /// </summary>
        public CategoryFeatures ExtractFeatures(string description, decimal amount, string merchant)
        {
            var features = new CategoryFeatures
            {
                Keywords = ExtractKeywords(description ?? string.Empty),
                MerchantType = ClassifyMerchantType(merchant ?? string.Empty),
                AmountRange = ClassifyAmountRange(amount),
                TimePattern = ExtractTimePattern(DateTime.Now),
                TextLength = description?.Length ?? 0,
                HasNumbers = ContainsNumbers(description ?? string.Empty),
                Language = DetectLanguage(description ?? string.Empty),
                ExtractedEntities = ExtractEntities(description ?? string.Empty)
            };

            return features;
        }

        /// <summary>
        /// 提取關鍵字
        /// </summary>
        public List<string> ExtractKeywords(string text)
        {
            var tokens = TokenizeAndNormalize(text);
            var keywords = new List<string>();

            // 直接匹配已知關鍵字
            foreach (var token in tokens)
            {
                foreach (var categoryKeywords in _keywordDictionary.Values)
                {
                    if (categoryKeywords.Contains(token))
                    {
                        keywords.Add(token);
                        break;
                    }
                }
            }

            // 提取重要的 n-gram
            keywords.AddRange(ExtractImportantNGrams(tokens));

            return keywords.Distinct().ToList();
        }

        /// <summary>
        /// 分類商家類型
        /// </summary>
        public string ClassifyMerchantType(string merchant)
        {
            if (string.IsNullOrWhiteSpace(merchant))
                return "未知";

            var normalizedMerchant = merchant.ToLower();

            // 檢查別名對應
            foreach (var alias in _merchantAliases)
            {
                if (normalizedMerchant.Contains(alias.Key))
                {
                    return alias.Value;
                }
            }

            // 基於關鍵字分類
            var merchantTypes = new Dictionary<string, List<string>>
            {
                ["便利商店"] = new List<string> { "7-eleven", "familymart", "全家", "小七", "萊爾富", "ok超商" },
                ["餐廳"] = new List<string> { "餐廳", "restaurant", "麥當勞", "肯德基", "必勝客" },
                ["咖啡廳"] = new List<string> { "星巴克", "咖啡", "starbucks", "cama", "路易莎" },
                ["超市"] = new List<string> { "家樂福", "大潤發", "全聯", "好市多", "costco" },
                ["加油站"] = new List<string> { "中油", "台塑", "shell", "加油站" },
                ["藥局"] = new List<string> { "藥局", "藥妝", "屈臣氏", "康是美" }
            };

            foreach (var type in merchantTypes)
            {
                if (type.Value.Any(keyword => normalizedMerchant.Contains(keyword)))
                {
                    return type.Key;
                }
            }

            return "一般商家";
        }

        /// <summary>
        /// 分類金額範圍
        /// </summary>
        public string ClassifyAmountRange(decimal amount)
        {
            return amount switch
            {
                <= 100 => "小額",
                <= 500 => "中額",
                <= 1000 => "較大額",
                <= 3000 => "大額",
                _ => "特大額"
            };
        }

        /// <summary>
        /// 提取時間模式
        /// </summary>
        public string ExtractTimePattern(DateTime date)
        {
            var hour = date.Hour;
            return hour switch
            {
                >= 6 and < 10 => "早晨",
                >= 10 and < 14 => "上午",
                >= 14 and < 18 => "下午",
                >= 18 and < 22 => "晚上",
                _ => "深夜"
            };
        }

        /// <summary>
        /// 檢查是否包含數字
        /// </summary>
        public bool ContainsNumbers(string text)
        {
            return !string.IsNullOrWhiteSpace(text) && text.Any(char.IsDigit);
        }

        /// <summary>
        /// 偵測語言
        /// </summary>
        public string DetectLanguage(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return "unknown";

            // 簡單的中文檢測
            var chineseCharCount = text.Count(c => c >= '\u4e00' && c <= '\u9fff');
            var totalChars = text.Length;

            return chineseCharCount > totalChars * 0.3 ? "zh-TW" : "en";
        }

        /// <summary>
        /// 提取實體
        /// </summary>
        private List<string> ExtractEntities(string text)
        {
            var entities = new List<string>();
            
            if (string.IsNullOrWhiteSpace(text))
                return entities;

            // 提取貨幣金額
            var amountPattern = @"\$?\d+(?:\.\d{2})?";
            var amounts = Regex.Matches(text, amountPattern);
            entities.AddRange(amounts.Cast<Match>().Select(m => $"金額:{m.Value}"));

            // 提取時間
            var timePattern = @"\d{1,2}:\d{2}";
            var times = Regex.Matches(text, timePattern);
            entities.AddRange(times.Cast<Match>().Select(m => $"時間:{m.Value}"));

            return entities;
        }

        /// <summary>
        /// 提取重要的 N-Gram
        /// </summary>
        private List<string> ExtractImportantNGrams(List<string> tokens)
        {
            var ngrams = new List<string>();

            // 2-gram
            for (int i = 0; i < tokens.Count - 1; i++)
            {
                var bigram = $"{tokens[i]} {tokens[i + 1]}";
                ngrams.Add(bigram);
            }

            return ngrams.Take(5).ToList(); // 限制數量
        }

        /// <summary>
        /// 載入關鍵字字典
        /// </summary>
        private Dictionary<string, List<string>> LoadKeywordDictionary()
        {
            return new Dictionary<string, List<string>>
            {
                ["餐飲"] = new List<string> { "早餐", "午餐", "晚餐", "宵夜", "餐廳", "小吃", "飲料", "咖啡", "茶", "便當" },
                ["交通"] = new List<string> { "捷運", "公車", "計程車", "uber", "油錢", "停車", "高速公路", "過路費" },
                ["購物"] = new List<string> { "衣服", "鞋子", "包包", "化妝品", "書籍", "文具", "電子產品" },
                ["醫療"] = new List<string> { "看病", "買藥", "健檢", "牙醫", "眼科", "藥局", "醫院", "診所" },
                ["娛樂"] = new List<string> { "電影", "ktv", "遊戲", "旅遊", "運動", "健身", "spa" },
                ["日常"] = new List<string> { "日用品", "清潔用品", "衛生紙", "洗髮精", "牙膏", "肥皂" }
            };
        }

        /// <summary>
        /// 載入停用詞
        /// </summary>
        private List<string> LoadStopWords()
        {
            return new List<string>
            {
                "的", "了", "在", "是", "我", "有", "和", "就", "不", "人", "都", "一", "一個", "上", "也", "很", "到", "說", "要", "去", "你", "會", "著", "沒", "看", "好", "自己", "這"
            };
        }

        /// <summary>
        /// 載入商家別名
        /// </summary>
        private Dictionary<string, string> LoadMerchantAliases()
        {
            return new Dictionary<string, string>
            {
                ["小七"] = "便利商店",
                ["7-11"] = "便利商店",
                ["全家"] = "便利商店",
                ["麥當勞"] = "速食店",
                ["肯德基"] = "速食店",
                ["星巴克"] = "咖啡廳",
                ["家樂福"] = "超市",
                ["全聯"] = "超市"
            };
        }
    }
}
