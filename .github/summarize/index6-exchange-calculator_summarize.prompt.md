# 台幣外幣匯率計算器 (index6.cshtml) - 技術總結

> 🏦 完整的台灣銀行匯率整合系統，提供即時匯率查詢、雙向計算和使用者友善的匯率管理體驗

---

## 📋 系統概述

**頁面定位**: `index6.cshtml` - 台幣與外幣匯率計算器
**核心功能**: 整合台灣銀行官方 CSV API，提供即時匯率查詢、雙向匯率計算、資料快取管理
**技術層級**: 企業級匯率計算系統，支援7種主要貨幣的精確計算

## 🎯 核心功能架構

### 💱 雙向匯率計算系統
- **台幣 → 外幣**: 使用現金賣出匯率或即期賣出匯率
- **外幣 → 台幣**: 使用現金買入匯率或即期買入匯率
- **智能匯率選擇**: 自動選擇最適合的匯率類型進行計算
- **精確度保證**: 所有計算保留小數點後6位，避免浮點數精度損失

### 📊 即時匯率顯示系統
- **四種匯率顯示**: 即期買入/賣出、現金買入/賣出
- **視覺化匯率卡片**: 色彩編碼的匯率資訊展示
- **匯率使用說明**: 動態顯示計算將使用的匯率類型
- **資料驗證狀態**: 即時檢查匯率資料完整性

### 🔄 資料管理與快取系統
- **API 整合**: 台灣銀行官方 CSV API (`https://rate.bot.com.tw/xrt/flcsv/0/day`)
- **本地快取**: JSON 檔案儲存 (`App_Data/exchange_rates.json`)
- **資料過期檢測**: 24小時過期提醒機制
- **降級策略**: API 失敗時自動使用本地快取

## 🏗️ 技術架構詳解

### 後端服務層架構

**ExchangeRateService.cs**:
```csharp
public class ExchangeRateService
{
    // CSV API 資料獲取
    public async Task<ExchangeRateData?> FetchExchangeRatesAsync()
    
    // CSV 格式解析
    private ExchangeRateData ParseCsvData(string csvContent)
    
    // 匯率計算邏輯
    public async Task<ExchangeCalculationResult> CalculateExchangeAsync(decimal amount, string from, string to)
    
    // 本地快取管理
    public async Task SaveExchangeRatesAsync(ExchangeRateData data)
    public async Task<ExchangeRateData?> LoadExchangeRatesAsync()
}
```

**支援貨幣清單**:
- USD (美金) 🇺🇸
- JPY (日圓) 🇯🇵  
- CNY (人民幣) 🇨🇳
- EUR (歐元) 🇪🇺
- GBP (英鎊) 🇬🇧
- HKD (港幣) 🇭🇰
- AUD (澳幣) 🇦🇺

### 資料模型設計

**ExchangeRate.cs**:
```csharp
public class ExchangeRate
{
    public string CurrencyCode { get; set; }    // 貨幣代碼
    public string CurrencyName { get; set; }    // 中文名稱
    public decimal BuyRate { get; set; }        // 即期買入 (6位小數)
    public decimal SellRate { get; set; }       // 即期賣出 (6位小數)
    public decimal CashBuyRate { get; set; }    // 現金買入 (6位小數)
    public decimal CashSellRate { get; set; }   // 現金賣出 (6位小數)
}

public class ExchangeRateData
{
    public DateTime LastUpdated { get; set; }   // 最後更新時間
    public string Source { get; set; }          // 資料來源識別
    public List<ExchangeRate> Rates { get; set; } // 匯率清單
}
```

### PageModel 設計模式

**index6.cshtml.cs**:
```csharp
public class index6 : PageModel
{
    // 核心屬性
    [BindProperty] public decimal Amount { get; set; }
    [BindProperty] public string ToCurrency { get; set; }
    [BindProperty] public bool IsTwdToForeign { get; set; }
    
    // 顯示屬性
    public decimal CurrentBuyRate { get; set; }
    public decimal CurrentSellRate { get; set; }
    public bool HasValidRateData { get; set; }
    
    // Handler 方法
    public async Task OnGetAsync()                    // AJAX 查詢支援
    public async Task<IActionResult> OnPostCalculateAsync() // 計算處理
    public async Task<IActionResult> OnPostUpdateRatesAsync() // 更新匯率
    public async Task<IActionResult> OnPostClearAsync()    // 清除表單
}
```

## 🎨 前端使用者體驗設計

### 響應式介面佈局
- **Bootstrap 5 架構**: 完全響應式設計，支援桌面/行動裝置
- **視覺層次設計**: 漸層卡片、色彩編碼、陰影效果
- **互動反饋機制**: 懸停動畫、載入狀態、確認提示

### JavaScript 增強功能
```javascript
// 計算方式切換時自動更新
$('input[name="IsTwdToForeign"]').change(function() {
    updateFromCurrency();
    updateRateDisplay(); // AJAX 更新匯率顯示
});

// 貨幣選擇變更時即時更新匯率
$('#ToCurrency').change(function() {
    updateFromCurrency();
    updateRateDisplay(); // 避免頁面重新載入
});

// AJAX 匯率顯示更新
function updateRateDisplay() {
    $.get(window.location.pathname, {
        IsTwdToForeign: isTwdToForeign,
        ToCurrency: toCurrency,
        Amount: amount
    }).done(function(data) {
        // 部分更新匯率卡片內容
    });
}
```

### 使用者體驗優化
- **無縫切換**: 計算方式變更時不重新載入頁面
- **即時驗證**: 表單輸入即時驗證與視覺反饋
- **智能提示**: 匯率資料狀態即時顯示
- **操作確認**: 重要操作提供載入動畫與狀態提示

## 🔐 資料安全與可靠性

### 輸入驗證機制
```csharp
[BindProperty]
[Range(0.01, double.MaxValue, ErrorMessage = "金額必須大於0")]
public decimal Amount { get; set; }
```

### 錯誤處理策略
- **API 降級**: 台銀 API 失敗時使用本地快取
- **資料驗證**: 匯率資料完整性檢查
- **使用者友善錯誤**: 明確的錯誤訊息與解決建議
- **執行緒安全**: 檔案讀寫操作使用適當的同步機制

### 資料持久化設計
- **檔案格式**: UTF-8 編碼 JSON 檔案
- **儲存位置**: `App_Data/exchange_rates.json`
- **備份策略**: 建議定期備份匯率資料檔案
- **檔案權限**: 應用程式需要完整讀寫權限

## 📈 效能最佳化

### HTTP 用戶端最佳化
```csharp
public ExchangeRateService(HttpClient httpClient, ...)
{
    _httpClient = httpClient;
    // 設定 User-Agent 避免被封鎖
    _httpClient.DefaultRequestHeaders.Add("User-Agent", "Exchange Rate Calculator/1.0");
}
```

### 計算精度保證
```csharp
private decimal ParseDecimal(string value)
{
    if (decimal.TryParse(value?.Trim(), out var result))
    {
        return Math.Round(result, 6); // 統一保留6位小數
    }
    return 0m;
}
```

### AJAX 部分更新
- **避免全頁重新載入**: 使用 AJAX 更新匯率顯示區域
- **智能 DOM 更新**: 只更新變更的內容區塊
- **快取友善**: 支援瀏覽器快取機制

## 🔧 設定與部署

### 環境需求
- **.NET 8.0**: ASP.NET Core Razor Pages
- **HttpClient**: 用於台銀 API 整合
- **檔案系統權限**: App_Data 資料夾完整讀寫權限

### 相依注入設定
```csharp
// Program.cs
builder.Services.AddHttpClient<ExchangeRateService>();
```

### 重要檔案清單
- `Pages/index6.cshtml`: 前端頁面與 JavaScript
- `Pages/index6.cshtml.cs`: PageModel 與業務邏輯  
- `Services/ExchangeRateService.cs`: 匯率服務核心
- `Models/ExchangeRate.cs`: 資料模型定義
- `App_Data/exchange_rates.json`: 匯率資料快取 🔴

## 🚀 未來擴充建議

### 功能增強方向
- **更多貨幣支援**: 擴充支援貨幣清單
- **歷史匯率查詢**: 實作匯率歷史資料功能
- **匯率走勢圖表**: 整合圖表庫顯示匯率趨勢
- **匯率提醒功能**: 實作匯率到價提醒機制

### 技術升級選項
- **資料庫整合**: 替換 JSON 檔案為關聯式資料庫
- **快取策略升級**: 實作 Redis 或 MemoryCache
- **API 版本管理**: 支援多個匯率資料來源
- **即時更新機制**: 整合 SignalR 實現即時匯率推送

## 🎖️ 技術亮點總結

1. **🏦 官方資料整合**: 直接整合台灣銀行官方 CSV API
2. **🎯 精確計算邏輯**: 6位小數精度，避免浮點數誤差
3. **🔄 智能降級機制**: API 失敗時自動使用本地快取
4. **📱 響應式設計**: 完全支援行動裝置與桌面環境
5. **⚡ AJAX 增強**: 無縫的使用者體驗，避免頁面重新載入
6. **🛡️ 健壯錯誤處理**: 完整的異常處理與使用者友善提示
7. **🎨 現代化 UI**: Bootstrap 5 + 自訂樣式的專業介面設計

---

**建立日期**: 2025-08-28  
**技術版本**: ASP.NET Core 8.0 + Bootstrap 5  
**資料來源**: 台灣銀行官方匯率 API  
**維護等級**: 企業級匯率計算系統 🏦
