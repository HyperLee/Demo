# Phase 1 語音輸入核心解析功能測試指南

## 實作完成項目

### ✅ 已完成
1. **VoiceModels.cs 擴充**
   - 新增 Phase 1 增強版 VoiceParseResult 模型
   - 支援 9 個完整欄位：日期、類型、金額、付款方式、大分類、細分類、描述、商家、備註
   - 新增各欄位信心度追蹤 (FieldConfidence)
   - 新增解析步驟追蹤 (ParsedSteps)
   - 新增未解析內容識別 (UnparsedContent)

2. **index8.cshtml.cs 後端解析邏輯重構**
   - 實作主要解析方法 ParseVoiceTextAsync (Phase 1 重構版)
   - 新增專用解析方法：
     - PreprocessVoiceText: 預處理語音文字
     - ParseAmountFromText: 金額解析
     - ParseDateFromText: 日期解析 (支援相對/絕對日期)
     - ParseTypeFromText: 收支類型解析
     - ParsePaymentMethodFromText: 付款方式解析
     - ParseMerchantNameFromText: 商家名稱解析
     - ParseCategoryFromTextAsync: 分類解析 (考慮商家名稱)
     - ParseDescriptionAndNoteFromText: 描述備註分離
     - CalculateOverallConfidence: 整體信心度計算
     - IdentifyUnparsedContent: 未解析內容識別

3. **index8.cshtml 前端UI更新**
   - 全新的語音解析結果預覽區域
   - 整體信心度進度條顯示
   - 9個欄位的詳細解析結果展示
   - 各欄位信心度和狀態指示器
   - 未解析內容警告顯示
   - 套用/忽略按鈕操作

4. **JavaScript 增強功能**
   - displayParseResult: 顯示增強版解析結果
   - updateOverallConfidence: 更新整體信心度顯示
   - updateFieldResult: 更新單一欄位結果顯示
   - applyParseResultToForm: 套用解析結果到表單
   - 支援各種日期格式轉換和表單填入

## 測試案例

### 測試案例 1: 完整資訊語音輸入
**輸入**: "我昨天在星巴克用信用卡花了150元買咖啡"
**預期結果**:
- Date: 昨天日期 (信心度: 90%)
- Type: Expense (信心度: 70%)
- Amount: 150 (信心度: 90%)
- PaymentMethod: 信用卡 (信心度: 90%)
- MerchantName: 星巴克 (信心度: 80%)
- Category: 餐飲美食 (信心度: 80%)
- Description: 咖啡 (信心度: 60%)

### 測試案例 2: 收入記錄
**輸入**: "10月1日收入3000元薪水"
**預期結果**:
- Date: 今年10月1日 (信心度: 80%)
- Type: Income (信心度: 80%)
- Amount: 3000 (信心度: 90%)
- Category: 薪資收入 (信心度: 70%)
- Description: 薪水 (信心度: 60%)

### 測試案例 3: 部分資訊缺失
**輸入**: "買了500元的東西"
**預期結果**:
- Type: Expense (信心度: 70%)
- Amount: 500 (信心度: 90%)
- Description: 東西 (信心度: 30%)
- UnparsedContent: 顯示無法解析的部分

## 手動測試步驟

1. **啟動應用程式**
   ```bash
   dotnet run --project Demo/Demo.csproj
   ```

2. **開啟語音記帳頁面**
   - 瀏覽器開啟 `https://localhost:5001/index8`

3. **測試語音輸入**
   - 點擊語音輸入按鈕
   - 說出測試案例中的語句
   - 檢查解析結果預覽區域是否正確顯示

4. **驗證解析結果**
   - 檢查各欄位解析值是否正確
   - 檢查信心度指示器顏色 (綠色≥70%, 黃色≥40%, 紅色<40%)
   - 檢查整體信心度進度條

5. **測試套用功能**
   - 點擊「套用到表單」按鈕
   - 檢查表單欄位是否正確填入
   - 驗證日期、類型、金額等欄位

## 效能要求驗證

- [ ] 解析時間 < 2 秒
- [ ] 基本解析準確度 ≥ 60%
- [ ] 向下相容性確保
- [ ] 無記憶體洩漏
- [ ] 錯誤處理完整

## 已知限制

1. **模糊匹配**: 目前僅實作簡化版，未來可加強
2. **商家資料庫**: 目前為硬編碼清單，未來可擴充為動態資料庫
3. **中文數字**: 僅支援一到三十一的轉換
4. **複雜語句**: 目前主要針對單一交易記錄，複雜語句支援有限

## 下一階段準備

Phase 1 完成後，將為 Phase 2 (使用者體驗優化) 奠定基礎：
- 分欄位信心度機制 ✅ (已完成)
- 漸進式解析流程
- 用戶確認互動設計
- 智能提示和建議

---

**Phase 1 狀態**: ✅ 開發完成，準備測試  
**核心功能**: 9欄位完整解析，信心度機制，UI增強  
**下一步**: 用戶測試和優化調整
