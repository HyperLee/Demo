# 台幣與外幣匯率計算器 - 開發規格書

## 專案概述
建立一個台幣與外國貨幣匯率計算器，提供即時匯率查詢與計算功能，資料來源為台灣銀行官方匯率。

## 檔案位置
- 前端視圖: `#file:index6.cshtml`
- 後端邏輯: `#file:index6.cshtml.cs` 
- 匯率資料: `App_Data/exchange_rates.json`

## 功能需求

### 1. 核心功能
- **台### 4. 使用者友善訊息
- 中文錯誤訊息
- 具體的錯誤原因說明
- 建議解決方案
- **匯率時效性警告**: "⚠️ 注意：匯率資料已超過24小時未更新，建議點擊「更新匯率」取得最新資料"
- **CSV API 錯誤提醒**: "⚠️ 無法取得最新匯率資料，目前使用本地快取資料，請稍後再試"
- **日期時間格式統一**: 所有日期時間顯示格式保持一致*: 輸入台幣金額，選擇目標外幣，計算兌換金額
- **外幣換算台幣**: 輸入外幣金額，選擇來源外幣，計算台幣金額
- **雙向計算**: 支援台幣轉外幣及外幣轉台幣
- **即時匯率更新**: 手動更新最新匯率資料

### 2. 支援貨幣
初期支援主要外幣：
- 美元 (USD)
- 日圓 (JPY) 
- 人民幣 (CNY)
- 歐元 (EUR)
- 英鎊 (GBP)
- 港幣 (HKD)
- 澳幣 (AUD)

### 3. 資料來源
- **官方來源**: 台灣銀行匯率 CSV API `https://rate.bot.com.tw/xrt/flcsv/0/day`
- **抓取方式**: 使用 HttpClient 直接呼叫 CSV API 獲取即時匯率
- **資料格式**: CSV 格式，包含貨幣代碼、現金買入匯率、現金賣出匯率等欄位
- **儲存格式**: 解析後轉換為 JSON 格式儲存於 `App_Data/exchange_rates.json`

## 技術規格

### 1. 匯率資料結構
```json
{
  "lastUpdated": "2025-08-28T10:30:00Z",
  "source": "台灣銀行CSV API",
  "rates": [
    {
      "currencyCode": "USD",
      "currencyName": "美金",
      "buyRate": 31.250000,
      "sellRate": 31.350000,
      "cashBuyRate": 31.180000,
      "cashSellRate": 31.420000
    }
  ]
}
```

### 2. CSV 資料處理
```csharp
// CSV 欄位格式參考（台銀格式）
// 幣別,現金買入,現金賣出,即期買入,即期賣出,發布時間
// USD,31.18,31.42,31.25,31.35,2025-08-28 10:30:00

public class CsvExchangeRateParser
{
    public ExchangeRateData ParseCsvData(string csvContent)
    {
        var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var rates = new List<ExchangeRate>();
        
        foreach(var line in lines.Skip(1)) // 跳過標題行
        {
            var fields = line.Split(',');
            if(fields.Length >= 5)
            {
                rates.Add(new ExchangeRate 
                {
                    CurrencyCode = fields[0].Trim(),
                    CashBuyRate = decimal.Parse(fields[1]),
                    CashSellRate = decimal.Parse(fields[2]),
                    BuyRate = decimal.Parse(fields[3]),
                    SellRate = decimal.Parse(fields[4])
                });
            }
        }
        
        return new ExchangeRateData { Rates = rates, LastUpdated = DateTime.Now };
    }
}
```

### 2. 計算精度
- **小數點精度**: 計算結果保留到小數點後第六位
- **四捨五入**: 使用 `Math.Round()` 方法處理
- **避免浮點誤差**: 使用 `decimal` 型別進行計算

### 3. 匯率類型選擇
- 預設使用「現金賣出匯率」(Cash Sell Rate) 進行台幣轉外幣計算
- 預設使用「現金買入匯率」(Cash Buy Rate) 進行外幣轉台幣計算

### 4. HTTP 用戶端設定
```csharp
public class ExchangeRateService
{
    private readonly HttpClient _httpClient;
    private const string TaiwanBankCsvUrl = "https://rate.bot.com.tw/xrt/flcsv/0/day";
    
    public async Task<string> FetchExchangeRatesCsvAsync()
    {
        try 
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Exchange Rate Calculator");
            var response = await _httpClient.GetAsync(TaiwanBankCsvUrl);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            // 記錄錯誤並返回本地快取資料
            throw new ExchangeRateException("無法取得最新匯率資料", ex);
        }
    }
}
```

## UI/UX 設計規格

### 1. 頁面佈局
```
┌─────────────────────────────────────────┐
│              匯率計算器                  │
├─────────────────────────────────────────┤
│ 📅 今日日期: 2025年8月28日 (三)           │
│ 📈 匯率更新: 2025-08-28 10:30:00         │
│                                         │
│ 計算方式: ○ 台幣→外幣 ○ 外幣→台幣        │
│                                         │
│ 金額: [_____________] [貨幣選擇下拉選單]   │
│                                         │
│ 兌換為: [貨幣選擇下拉選單]                │
│                                         │
│ [計算] [清除] [更新匯率]                  │
│                                         │
│ 計算結果: ________________               │
│                                         │
│ ⚠️  提醒: 匯率資料僅供參考，實際匯率請以銀行公告為準 │
└─────────────────────────────────────────┘
```

### 2. 使用者體驗
- **響應式設計**: 支援桌面與行動裝置
- **即時回饋**: 輸入時即時顯示格式提示
- **錯誤處理**: 友善的錯誤訊息顯示
- **載入狀態**: 更新匯率時顯示載入動畫
- **日期資訊顯示**: 
  - 在頁面頂部顯眼位置顯示當日日期（含星期）
  - 清楚標示匯率資料最後更新時間
  - 使用圖示增強視覺識別（📅 📈）
  - 當匯率資料超過24小時未更新時，顯示警告提醒
- **資料時效性提醒**: 在頁面底部加入免責聲明，提醒使用者匯率僅供參考

## 後端邏輯規格

### 1. PageModel 方法
```csharp
public class Index6Model : PageModel
{
    // 屬性
    public decimal Amount { get; set; }
    public string FromCurrency { get; set; }
    public string ToCurrency { get; set; }
    public decimal Result { get; set; }
    public bool IsError { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime CurrentDate { get; set; }          // 當日日期
    public DateTime? LastUpdated { get; set; }         // 匯率最後更新時間
    public string DayOfWeekChinese { get; set; }       // 中文星期顯示
    public bool IsRateDataStale { get; set; }          // 匯率資料是否過期
    
    // 方法
    public async Task OnGetAsync()           // 頁面載入
    public async Task OnPostCalculateAsync() // 匯率計算
    public async Task OnPostUpdateRatesAsync() // 更新匯率
    public async Task OnPostClearAsync()     // 清除表單
    private string GetChineseDayOfWeek(DayOfWeek dayOfWeek) // 取得中文星期
    private bool CheckRateDataFreshness()    // 檢查匯率資料時效性
}
```

### 2. 資料服務
- **ExchangeRateService**: 負責匯率資料的 CRUD 操作
- **CsvParsingService**: 負責解析台銀 CSV 格式資料
- **HttpClientService**: 負責呼叫台銀 CSV API
- **CalculationService**: 負責匯率計算邏輯

## 錯誤處理

### 1. 輸入驗證
- 金額必須為正數
- 金額不能為空值
- 貨幣代碼必須有效
- 來源與目標貨幣不能相同
- **CSV 資料完整性驗證**: 確保 CSV 各欄位資料格式正確

### 2. 系統錯誤
- 網路連線失敗處理
- 資料檔案讀寫錯誤
- **CSV 解析錯誤**: 處理 CSV 格式異常或欄位缺失
- **API 回應錯誤**: 台銀 CSV API 無回應或格式變更
- 計算溢位處理
- **匯率資料時效性檢查**: 當資料超過24小時未更新時顯示警告

### 3. 日期與時間處理
- **時區處理**: 統一使用台灣時區 (UTC+8)
- **日期格式**: 中文格式顯示 "2025年8月28日 (三)"
- **時間格式**: 24小時制 "HH:mm:ss"
- **星期顯示**: 中文星期 (一、二、三、四、五、六、日)

### 3. 使用者友善訊息
- 中文錯誤訊息
- 具體的錯誤原因說明
- 建議解決方案
- **匯率時效性警告**: "⚠️ 注意：匯率資料已超過24小時未更新，建議點擊「更新匯率」取得最新資料"
- **日期時間格式統一**: 所有日期時間顯示格式保持一致

## 效能考量

### 1. 快取機制
- 匯率資料快取 30 分鐘
- 避免頻繁的網路請求
- 本地 JSON 檔案作為備份

### 2. 非同步處理
- 匯率更新使用非同步方法
- 避免阻塞 UI 操作
- 適當的逾時處理

## 測試規格

### 1. 單元測試
- 匯率計算邏輯測試
- 資料驗證測試
- 錯誤處理測試
- **CSV 解析功能測試**: 測試各種 CSV 格式情境
- **API 呼叫模擬測試**: 使用 Mock 測試 HTTP 請求

### 2. 整合測試
- **CSV API 連線測試**: 確認台銀 API 可正常存取
- **資料解析整合測試**: 完整的 CSV 下載→解析→儲存流程測試
- 檔案讀寫測試
- 使用者介面互動測試

## 部署與維運

### 1. 檔案權限
- `App_Data/exchange_rates.json` 需要讀寫權限
- 確保 Web 應用程式可存取資料檔案

### 2. 監控項目
- 匯率更新成功率
- 計算準確性
- 系統回應時間
- 錯誤發生頻率
- **匯率資料時效性**: 監控匯率資料更新頻率，確保資料即時性
- **使用者互動**: 記錄「更新匯率」按鈕點擊頻率，了解使用者對資料即時性的需求

## 未來擴充規劃

### 1. 功能擴充
- 支援更多貨幣類型
- 歷史匯率查詢
- 匯率走勢圖表
- 匯率提醒功能

### 2. 技術改進
- 多資料來源整合
- 即時匯率推送
- 行動版 APP
- API 服務提供

---
**開發優先級**: P1 (高優先級)  
**預估開發時間**: 3-5 工作天  
**維護週期**: 每週檢查匯率資料準確性
