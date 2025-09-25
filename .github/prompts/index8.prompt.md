# 記帳系統語音輸入功能增強開發規格書

## 專案概述
調整語音輸入記帳行為，使語音輸入與手動新增的記帳欄位保持一致，提升用戶體驗和資料完整性。

## 檔案位置
- 前端頁面: `D:\Demo\Demo\Demo\Pages\index8.cshtml`
- 後端邏輯: `D:\Demo\Demo\Demo\Pages\index8.cshtml.cs`
- 語音模型: `D:\Demo\Demo\Demo\Models\VoiceModels.cs`
- 語音腳本: `D:\Demo\Demo\Demo\wwwroot\js\voice-input.js`

## 現狀分析

### 手動輸入欄位（完整）
1. **基本資訊**
   - 日期 (Date) - 必填，不可為未來
   - 收支類型 (Type) - 必填，Income/Expense

2. **金額資訊**
   - 金額 (Amount) - 必填，範圍 0.01-999,999,999
   - 付款方式 (PaymentMethod) - 必填，下拉選單

3. **分類資訊**
   - 大分類 (Category) - 必填，動態載入選項
   - 細分類 (SubCategory) - 選填，與大分類連動

4. **交易資訊**
   - 交易描述 (Description) - 必填，自由文字
   - 商家名稱 (Merchant) - 選填，自由文字

5. **其他資訊**
   - 備註 (Note) - 選填，最大500字元

### 語音輸入欄位（目前）
1. 收支類型 (Type) - 基本判斷
2. 金額 (Amount) - 數字解析
3. 類別 (Category) - 關鍵字匹配
4. 描述 (Description) - 簡單文字
5. 原始語音文字 (VoiceText)

### 技術架構分析
- **語音識別服務**: Web Speech API (webkitSpeechRecognition)
- **語音解析**: 正規表達式 + 關鍵字匹配
- **前端框架**: ASP.NET Core Razor Pages + Bootstrap + jQuery
- **語音模型**: VoiceParseResult, VoiceRecordRequest 等

## 需求描述

### 目標語音輸入範例
```
基本範例: "我花了100元買咖啡"
進階範例: "我在2023年10月1日花了100元買咖啡，用信用卡付款，類型是餐飲，商家名稱是星巴克，其他資訊是朋友聚會"
```

### 功能需求

#### 1. 語音解析能力增強
**優先級: 高**
- 日期解析: 支援多種日期格式
  - "今天"、"昨天"、"前天"
  - "10月1日"、"十月一日" 
  - "2023年10月1日"
- 付款方式識別: 關鍵字匹配
  - "現金"、"信用卡"、"悠遊卡"、"LINE Pay"等
- 商家名稱提取: 智能識別
  - "在星巴克"、"去7-11"、"麥當勞"等
- 描述與備註分離: 語意分析
  - 主要描述 vs 額外備註資訊

#### 2. 語音模型擴充
**優先級: 高**
```csharp
public class VoiceParseResult
{
    // 現有欄位保持不變
    public string OriginalText { get; set; }
    public string Type { get; set; }
    public decimal? Amount { get; set; }
    public string? Category { get; set; }
    public string Description { get; set; }
    
    // 新增欄位
    public DateTime? Date { get; set; }           // 日期解析
    public string? PaymentMethod { get; set; }    // 付款方式
    public string? SubCategory { get; set; }      // 細分類
    public string? MerchantName { get; set; }     // 商家名稱
    public string? Note { get; set; }             // 備註資訊
    
    // 解析信心度（分欄位）
    public Dictionary<string, double> FieldConfidence { get; set; }
    
    // 無法解析的內容
    public string? UnparsedContent { get; set; }
}
```

#### 3. 解析演算法優化
**優先級: 中**
- **日期解析邏輯**
  ```csharp
  private DateTime? ParseDateFromText(string text)
  {
      // 相對日期: 今天、昨天、前天
      // 絕對日期: X月X日、XXXX年X月X日
      // 中文數字轉換: 十月一日 -> 10月1日
  }
  ```

- **付款方式匹配**
  ```csharp
  private string? ParsePaymentMethod(string text)
  {
      // 關鍵字字典匹配
      // 支援模糊匹配（編輯距離）
  }
  ```

- **商家名稱提取**
  ```csharp
  private string? ParseMerchantName(string text)
  {
      // 常見商家名稱字典
      // 位置介詞處理: "在", "去", "到"
  }
  ```

#### 4. 使用者界面調整
**優先級: 中**
- 語音解析結果預覽增強
- 欄位對應視覺化顯示
- 部分解析成功的處理流程
- 解析信心度顯示

#### 5. 智能學習機制
**優先級: 低**
- 用戶修正回饋學習
- 個人化關鍵字字典
- 使用習慣分析

## 技術實作規劃

### Phase 1: 核心解析功能 (優先級: 高)
1. **VoiceParseResult 模型擴充**
   - 新增所需欄位
   - 向下相容性確保

2. **解析方法重構**
   - ParseVoiceTextAsync 方法增強
   - 新增各欄位專用解析方法
   - 錯誤處理和降級策略

3. **前端展示調整**
   - 語音解析結果預覽UI更新
   - 表單自動填入邏輯完善

### Phase 2: 使用者體驗優化 (優先級: 中)
1. **解析信心度機制**
   - 分欄位信心度計算
   - 低信心度欄位標示
   - 用戶確認流程

2. **漸進式解析**
   - 部分解析成功處理
   - 未解析內容提示
   - 手動補充引導

### Phase 3: 智能化增強 (優先級: 低)
1. **個人化學習**
   - 用戶偏好記錄
   - 常用詞彙學習
   - 解析準確度提升

2. **上下文理解**
   - 對話式語音輸入
   - 多輪修正支援

## 風險評估與限制

### 技術限制
1. **Web Speech API 限制**
   - 瀏覽器相容性（僅 Chrome/Edge）
   - 網路依賴性
   - 語音識別準確度

2. **自然語言處理複雜度**
   - 中文語音解析挑戰
   - 方言和口音差異
   - 語意歧義處理

### 解決方案
1. **降級策略**
   - 部分解析成功時的處理
   - 手動補充引導
   - 傳統手動輸入備援

2. **準確度提升**
   - 多輪確認機制
   - 解析結果預覽
   - 用戶修正學習

## 驗收標準

### 基本功能
- [ ] 語音輸入能解析日期、金額、類別、付款方式
- [ ] 解析結果正確填入對應表單欄位
- [ ] 部分解析失敗時能引導手動補充
- [ ] 保持現有手動輸入功能不受影響

### 體驗指標
- [ ] 語音解析準確度 > 70%
- [ ] 完整資訊解析率 > 50%
- [ ] 解析時間 < 3秒
- [ ] 用戶確認流程 < 5步驟

### 相容性
- [ ] Chrome 瀏覽器支援
- [ ] Edge 瀏覽器支援
- [ ] 手機響應式設計
- [ ] 既有資料格式相容

## 測試案例

### 語音輸入測試範例
```
1. 基本案例: "我花了50元買早餐"
2. 完整案例: "昨天用信用卡在7-11花了120元買午餐"
3. 複雜案例: "10月1日在星巴克用LINE Pay花了150元買咖啡，和朋友聚會"
4. 邊界案例: "收入3000元薪水獎金"
5. 錯誤案例: "買了很多東西花了錢"
```

### 預期解析結果
每個測試案例都應明確定義預期的解析結果和降級處理方式。

## 開發時程預估
- **Phase 1**: 2週（核心功能）
- **Phase 2**: 1週（體驗優化）  
- **Phase 3**: 1週（智能增強）
- **測試調優**: 1週

**總計**: 約5週開發時程
