using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Demo.Models;
using Demo.Pages;
using System.Text.Json;

namespace Demo.Tests
{
    /// <summary>
    /// Phase 1 語音解析功能測試類別
    /// </summary>
    public class VoiceParsingPhase1Tests
    {
        /// <summary>
        /// 測試完整語音解析案例
        /// </summary>
        [Test]
        public async Task TestCompleteVoiceParsing()
        {
            // 測試案例: "我昨天在星巴克用信用卡花了150元買咖啡"
            var testInput = "我昨天在星巴克用信用卡花了150元買咖啡";
            
            // 這裡應該呼叫語音解析邏輯
            // 實際實作中需要建立測試環境
            
            Console.WriteLine($"測試輸入: {testInput}");
            Console.WriteLine("預期解析結果:");
            Console.WriteLine("- 日期: 昨天");
            Console.WriteLine("- 類型: Expense");
            Console.WriteLine("- 金額: 150");
            Console.WriteLine("- 付款方式: 信用卡");
            Console.WriteLine("- 商家: 星巴克");
            Console.WriteLine("- 分類: 餐飲美食");
            Console.WriteLine("- 描述: 咖啡");
        }

        /// <summary>
        /// 測試日期解析功能
        /// </summary>
        [Test]
        public async Task TestDateParsing()
        {
            var testCases = new[]
            {
                "今天",
                "昨天", 
                "前天",
                "10月1日",
                "2023年10月1日",
                "十月一日"
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"測試日期解析: {testCase}");
                // 實際測試會在這裡進行
            }
        }

        /// <summary>
        /// 測試金額解析功能
        /// </summary>
        [Test]
        public async Task TestAmountParsing()
        {
            var testCases = new[]
            {
                "150元",
                "3000塊",
                "花了500",
                "500塊錢"
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"測試金額解析: {testCase}");
                // 實際測試會在這裡進行
            }
        }

        /// <summary>
        /// 測試付款方式解析功能
        /// </summary>
        [Test]
        public async Task TestPaymentMethodParsing()
        {
            var testCases = new[]
            {
                "用信用卡",
                "刷卡",
                "現金付款",
                "LINE Pay",
                "悠遊卡"
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"測試付款方式解析: {testCase}");
                // 實際測試會在這裡進行
            }
        }

        /// <summary>
        /// 測試商家名稱解析功能
        /// </summary>
        [Test]
        public async Task TestMerchantNameParsing()
        {
            var testCases = new[]
            {
                "在星巴克",
                "去7-11",
                "到麥當勞",
                "從全家"
            };

            foreach (var testCase in testCases)
            {
                Console.WriteLine($"測試商家名稱解析: {testCase}");
                // 實際測試會在這裡進行
            }
        }

        /// <summary>
        /// 測試整體信心度計算
        /// </summary>
        [Test]
        public async Task TestConfidenceCalculation()
        {
            var mockResult = new VoiceParseResult
            {
                Amount = 150,
                Type = "Expense",
                Category = "餐飲美食",
                PaymentMethod = "信用卡",
                MerchantName = "星巴克",
                Description = "咖啡"
            };

            // 設定各欄位信心度
            mockResult.FieldConfidence["Amount"] = 0.9;
            mockResult.FieldConfidence["Type"] = 0.7;
            mockResult.FieldConfidence["Category"] = 0.8;
            mockResult.FieldConfidence["PaymentMethod"] = 0.9;
            mockResult.FieldConfidence["MerchantName"] = 0.8;
            mockResult.FieldConfidence["Description"] = 0.6;

            Console.WriteLine("測試信心度計算:");
            Console.WriteLine($"各欄位信心度: {JsonSerializer.Serialize(mockResult.FieldConfidence)}");
            Console.WriteLine("預期整體信心度: > 0.75");
        }

        /// <summary>
        /// 測試錯誤處理
        /// </summary>
        [Test]
        public async Task TestErrorHandling()
        {
            var invalidInputs = new[]
            {
                "",
                null,
                "無法解析的文字",
                "只有文字沒有數字"
            };

            foreach (var input in invalidInputs)
            {
                Console.WriteLine($"測試錯誤處理: '{input}'");
                // 實際測試會檢查錯誤處理機制
            }
        }
    }

    /// <summary>
    /// 測試屬性標註
    /// </summary>
    public class TestAttribute : Attribute
    {
    }
}

// 執行說明:
// 這是測試範例程式，展示了 Phase 1 功能的測試案例
// 實際執行需要設定測試環境並整合到專案的測試框架中
// 
// 測試重點:
// 1. 完整語音解析流程
// 2. 各個解析方法的單元測試
// 3. 信心度計算驗證
// 4. 錯誤處理機制
// 5. 效能測試（解析時間 < 2秒）
