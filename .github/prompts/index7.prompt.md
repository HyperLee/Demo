# 記帳系統開發規格書

## 專案概述
開發一個輕量級記帳系統，讓使用者可以記錄每日的收入與支出，提供月曆檢視、分類管理、報表統計等功能。採用 ASP.NET Core Razor Pages 架構，使用 JSON 檔案作為資料儲存。

## 技術規格
- **開發框架**: ASP.NET Core 8.0 Razor Pages
- **程式語言**: C# 13
- **資料儲存**: JSON 檔案 (App_Data 目錄)
- **前端技術**: Bootstrap 5, jQuery, HTML5, CSS3
- **檔案結構**: 遵循 ASP.NET Core 標準目錄結構

## 核心功能模組

### 1. 列表頁面檔案位置
- **前端**: `#file:index7.cshtml`
- **後端**: `#file:index7.cshtml.cs`
- **路由**: `/index7`

### 1.1 列表頁面功能描述
- **主要顯示**: 以月曆形式顯示每日的收入與支出記錄
- **篩選功能**: 提供按月、按年的時間篩選功能
- **統計資訊**: 頁面上方顯示當月支出與收入總額，以及淨收支
- **操作功能**:
  - 新增記錄：點擊跳轉到詳細編輯頁面 (index8)
  - 編輯記錄：點擊現有記錄跳轉到詳細編輯頁面
  - 刪除記錄：直接彈出確認對話框，確認後立即刪除
- **資料來源**: JSON 檔案 (輕量級架構，無需專業資料庫)

### 1.2 列表頁面技術規格
- **月曆元件**: 使用 HTML Table 或 Bootstrap Calendar 元件實作
- **資料繫結**: 使用 Razor Pages Model Binding
- **AJAX 支援**: 刪除操作使用 AJAX 避免頁面重新載入
- **響應式設計**: 支援手機、平板、桌機不同螢幕尺寸

### 1.3 列表頁面資料模型
```csharp
public class AccountingRecord
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } // "Income" 或 "Expense"
    public decimal Amount { get; set; } // 使用 decimal 避免浮點精度問題
    public string Category { get; set; }
    public string SubCategory { get; set; }
    public string PaymentMethod { get; set; }
    public string Note { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
```

### 2. 詳細編輯頁面檔案位置
- **前端**: `Pages/index8.cshtml`
- **後端**: `Pages/index8.cshtml.cs`
- **路由**: `/index8` 或 `/index8?id={recordId}`

### 2.1 詳細編輯頁面功能描述
- **新增模式**: 當無 ID 參數時，為新增模式，所有欄位為空白
- **編輯模式**: 當有 ID 參數時，為編輯模式，載入現有記錄資料
- **必填欄位**: 日期、類型（收入/支出）、金額、大分類
- **選填欄位**: 細分類、付款方式、備註

### 2.2 表單欄位規格

#### 2.2.1 日期欄位
- **元件類型**: HTML5 Date Input (`<input type="date">`)
- **預設值**: 當日日期
- **驗證**: 不可為空，不可為未來日期
- **格式**: YYYY-MM-DD

#### 2.2.2 收支類型
- **元件類型**: Radio Button 或 Toggle Switch
- **選項**: 收入 (Income) / 支出 (Expense)
- **預設值**: 支出
- **驗證**: 必選

#### 2.2.3 金額欄位
- **元件類型**: Number Input
- **資料類型**: `long` (避免 int 溢位問題)
- **格式化**: 自動添加千分位逗號 (1,000)
- **限制**: 僅允許正整數輸入，包含 0 (免費商品情況)
- **驗證**: 不可為空，不可為負數
- **最大值**: 2,147,483,647 (int.MaxValue)

#### 2.2.4 付款方式
- **元件類型**: Select Dropdown
- **選項**: 
  - 現金
  - 信用卡
  - 金融卡
  - 行動支付 (Apple Pay, Google Pay, LINE Pay)
  - 電子錢包 (悠遊卡, 一卡通)
  - 銀行轉帳
  - 其他
- **預設值**: 現金
- **擴充性**: 支援新增自訂付款方式

### 2.3 分類系統設計

#### 2.3.1 支出分類結構
```json
{
  "expenseCategories": [
    {
      "id": 1,
      "name": "餐飲食品",
      "icon": "fas fa-utensils",
      "subCategories": [
        { "id": 101, "name": "早餐" },
        { "id": 102, "name": "午餐" },
        { "id": 103, "name": "晚餐" },
        { "id": 104, "name": "零食" },
        { "id": 105, "name": "飲料" }
      ]
    },
    {
      "id": 2,
      "name": "服飾美容",
      "icon": "fas fa-tshirt",
      "subCategories": [
        { "id": 201, "name": "上衣" },
        { "id": 202, "name": "褲子" },
        { "id": 203, "name": "外套" },
        { "id": 204, "name": "鞋子" },
        { "id": 205, "name": "包包" },
        { "id": 206, "name": "配件" },
        { "id": 207, "name": "美容保養" }
      ]
    },
    {
      "id": 3,
      "name": "居家生活",
      "icon": "fas fa-home",
      "subCategories": [
        { "id": 301, "name": "家電用品" },
        { "id": 302, "name": "房租" },
        { "id": 303, "name": "管理費" },
        { "id": 304, "name": "水電費" },
        { "id": 305, "name": "瓦斯費" },
        { "id": 306, "name": "網路費" },
        { "id": 307, "name": "日用品" }
      ]
    },
    {
      "id": 4,
      "name": "運輸交通",
      "icon": "fas fa-car",
      "subCategories": [
        { "id": 401, "name": "公共交通" },
        { "id": 402, "name": "計程車" },
        { "id": 403, "name": "共享運具" }
      ]
    },
    {
      "id": 5,
      "name": "教育學習",
      "icon": "fas fa-graduation-cap",
      "subCategories": [
        { "id": 501, "name": "課程費用" },
        { "id": 502, "name": "書籍教材" },
        { "id": 503, "name": "線上學習" }
      ]
    },
    {
      "id": 6,
      "name": "休閒娛樂",
      "icon": "fas fa-gamepad",
      "subCategories": [
        { "id": 601, "name": "電影" },
        { "id": 602, "name": "音樂" },
        { "id": 603, "name": "遊戲" },
        { "id": 604, "name": "旅遊" },
        { "id": 605, "name": "運動" }
      ]
    },
    {
      "id": 7,
      "name": "3C產品",
      "icon": "fas fa-mobile-alt",
      "subCategories": [
        { "id": 701, "name": "手機" },
        { "id": 702, "name": "電腦" },
        { "id": 703, "name": "平板" },
        { "id": 704, "name": "周邊配件" }
      ]
    },
    {
      "id": 8,
      "name": "圖書刊物",
      "icon": "fas fa-book",
      "subCategories": [
        { "id": 801, "name": "書籍" },
        { "id": 802, "name": "雜誌" },
        { "id": 803, "name": "報紙" }
      ]
    },
    {
      "id": 9,
      "name": "汽機車",
      "icon": "fas fa-motorcycle",
      "subCategories": [
        { "id": 901, "name": "油錢" },
        { "id": 902, "name": "保養" },
        { "id": 903, "name": "過路費" },
        { "id": 904, "name": "停車費" },
        { "id": 905, "name": "保險" }
      ]
    },
    {
      "id": 10,
      "name": "醫療保健",
      "icon": "fas fa-medkit",
      "subCategories": [
        { "id": 1001, "name": "診所就醫" },
        { "id": 1002, "name": "健康檢查" },
        { "id": 1003, "name": "勞健保" },
        { "id": 1004, "name": "保健食品" },
        { "id": 1005, "name": "藥品" }
      ]
    },
    {
      "id": 11,
      "name": "人情交際",
      "icon": "fas fa-handshake",
      "subCategories": [
        { "id": 1101, "name": "送禮" },
        { "id": 1102, "name": "招待" },
        { "id": 1103, "name": "交際應酬" },
        { "id": 1104, "name": "捐款" }
      ]
    },
    {
      "id": 12,
      "name": "理財投資",
      "icon": "fas fa-chart-line",
      "subCategories": [
        { "id": 1201, "name": "股票" },
        { "id": 1202, "name": "債券" },
        { "id": 1203, "name": "基金" },
        { "id": 1204, "name": "期貨" },
        { "id": 1205, "name": "保險" }
      ]
    }
  ]
}
```

#### 2.3.2 收入分類結構
```json
{
  "incomeCategories": [
    {
      "id": 13,
      "name": "薪資收入",
      "icon": "fas fa-money-bill-wave",
      "subCategories": [
        { "id": 1301, "name": "每月薪資" },
        { "id": 1302, "name": "加班費" },
        { "id": 1303, "name": "績效獎金" }
      ]
    },
    {
      "id": 14,
      "name": "獎金收入",
      "icon": "fas fa-trophy",
      "subCategories": [
        { "id": 1401, "name": "年終獎金" },
        { "id": 1402, "name": "三節獎金" },
        { "id": 1403, "name": "分紅" },
        { "id": 1404, "name": "業績獎金" }
      ]
    },
    {
      "id": 15,
      "name": "投資收益",
      "icon": "fas fa-piggy-bank",
      "subCategories": [
        { "id": 1501, "name": "股利" },
        { "id": 1502, "name": "利息" },
        { "id": 1503, "name": "資本利得" },
        { "id": 1504, "name": "租金收入" }
      ]
    },
    {
      "id": 16,
      "name": "其他收入",
      "icon": "fas fa-plus-circle",
      "subCategories": [
        { "id": 1601, "name": "兼職收入" },
        { "id": 1602, "name": "禮金" },
        { "id": 1603, "name": "退稅" },
        { "id": 1604, "name": "中獎" }
      ]
    }
  ]
}
```

#### 2.3.3 分類選擇器設計
- **大分類**: Select Dropdown，根據收支類型動態載入
- **細分類**: 根據大分類選擇動態更新的 Select Dropdown
- **自訂分類**: 提供「新增自訂分類」選項，可擴充現有分類
- **級聯選擇**: 大分類變更時，細分類自動重設為第一項

#### 2.2.5 備註欄位
- **元件類型**: Textarea
- **字數限制**: 500字元
- **預設值**: 空白
- **驗證**: 非必填
- **功能**: 支援換行輸入

### 2.4 表單驗證規則
- **前端驗證**: JavaScript 即時驗證，提升使用者體驗
- **後端驗證**: C# Model Validation，確保資料安全性
- **錯誤訊息**: 中文化錯誤提示訊息
- **驗證項目**:
  - 日期不可為空且不可為未來日期
  - 金額必須為正整數
  - 大分類必須選擇
  - 備註字數不可超過限制

### 3. 資料儲存架構

### 3.1 JSON 檔案結構
- **記錄檔案**: `App_Data/accounting-records.json`
- **分類檔案**: `App_Data/categories.json`
- **設定檔案**: `App_Data/accounting-settings.json`

### 3.2 資料服務設計
```csharp
/// <summary>
/// 記帳資料服務
/// </summary>
public interface IAccountingService
{
    Task<List<AccountingRecord>> GetRecordsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<AccountingRecord?> GetRecordByIdAsync(int id);
    Task<int> CreateRecordAsync(AccountingRecord record);
    Task<bool> UpdateRecordAsync(AccountingRecord record);
    Task<bool> DeleteRecordAsync(int id);
    Task<List<Category>> GetCategoriesAsync(string type);
    Task<MonthlySummary> GetMonthlySummaryAsync(int year, int month);
}
```

### 4. 使用者介面設計

### 4.1 列表頁面佈局
```html
<!-- 統計區塊 -->
<div class="summary-section">
    <div class="row">
        <div class="col-md-3">總收入: NT$ {MonthlyIncome:N0}</div>
        <div class="col-md-3">總支出: NT$ {MonthlyExpense:N0}</div>
        <div class="col-md-3">淨收支: NT$ {NetIncome:N0}</div>
        <div class="col-md-3">
            <button class="btn btn-primary">新增記錄</button>
        </div>
    </div>
</div>

<!-- 篩選區塊 -->
<div class="filter-section">
    <div class="row">
        <div class="col-md-4">
            <select id="yearFilter">年度篩選</select>
        </div>
        <div class="col-md-4">
            <select id="monthFilter">月份篩選</select>
        </div>
        <div class="col-md-4">
            <button class="btn btn-info">匯出報表</button>
        </div>
    </div>
</div>

<!-- 月曆檢視 -->
<div class="calendar-section">
    <table class="table table-bordered calendar">
        <!-- 月曆內容 -->
    </table>
</div>
```

### 4.2 編輯頁面佈局
```html
<form method="post">
    <!-- 基本資訊 -->
    <div class="row">
        <div class="col-md-6">
            <label>日期 <span class="required">*</span></label>
            <input type="date" asp-for="Record.Date" class="form-control" required>
        </div>
        <div class="col-md-6">
            <label>收支類型 <span class="required">*</span></label>
            <div class="form-check-inline">
                <input type="radio" asp-for="Record.Type" value="Income" id="typeIncome">
                <label for="typeIncome">收入</label>
            </div>
            <div class="form-check-inline">
                <input type="radio" asp-for="Record.Type" value="Expense" id="typeExpense">
                <label for="typeExpense">支出</label>
            </div>
        </div>
    </div>
    
    <!-- 金額與分類 -->
    <div class="row">
        <div class="col-md-6">
            <label>金額 <span class="required">*</span></label>
            <input type="number" asp-for="Record.Amount" class="form-control money-input" min="0" required>
        </div>
        <div class="col-md-6">
            <label>付款方式</label>
            <select asp-for="Record.PaymentMethod" class="form-control">
                <option value="現金">現金</option>
                <option value="信用卡">信用卡</option>
                <option value="金融卡">金融卡</option>
                <option value="行動支付">行動支付</option>
            </select>
        </div>
    </div>
    
    <!-- 分類選擇 -->
    <div class="row">
        <div class="col-md-6">
            <label>大分類 <span class="required">*</span></label>
            <select asp-for="Record.Category" class="form-control" id="categorySelect" required>
                <option value="">請選擇分類</option>
            </select>
        </div>
        <div class="col-md-6">
            <label>細分類</label>
            <select asp-for="Record.SubCategory" class="form-control" id="subCategorySelect">
                <option value="">請選擇細分類</option>
            </select>
        </div>
    </div>
    
    <!-- 備註 -->
    <div class="row">
        <div class="col-md-12">
            <label>備註</label>
            <textarea asp-for="Record.Note" class="form-control" rows="3" maxlength="500"></textarea>
        </div>
    </div>
    
    <!-- 操作按鈕 -->
    <div class="form-actions">
        <button type="submit" class="btn btn-success">儲存</button>
        <a href="/index7" class="btn btn-secondary">取消</a>
    </div>
</form>
```

### 5. 技術實作要點

### 5.1 金額格式化
```javascript
// 金額輸入格式化
$('.money-input').on('input', function() {
    let value = $(this).val().replace(/,/g, '');
    if (!isNaN(value) && value !== '') {
        $(this).val(parseInt(value).toLocaleString());
    }
});
```

### 5.2 分類級聯選擇
```javascript
// 大分類變更時更新細分類
$('#categorySelect').on('change', function() {
    const categoryId = $(this).val();
    const type = $('input[name="Record.Type"]:checked').val();
    
    $.ajax({
        url: '/api/categories/subcategories',
        data: { categoryId, type },
        success: function(data) {
            $('#subCategorySelect').html('<option value="">請選擇細分類</option>');
            data.forEach(sub => {
                $('#subCategorySelect').append(`<option value="${sub.name}">${sub.name}</option>`);
            });
        }
    });
});
```

### 5.3 月曆資料載入
```csharp
// 後端月曆資料準備
public async Task<IActionResult> OnGetAsync(int year = 0, int month = 0)
{
    var currentDate = DateTime.Now;
    ViewYear = year == 0 ? currentDate.Year : year;
    ViewMonth = month == 0 ? currentDate.Month : month;
    
    var startDate = new DateTime(ViewYear, ViewMonth, 1);
    var endDate = startDate.AddMonths(1).AddDays(-1);
    
    Records = await _accountingService.GetRecordsAsync(startDate, endDate);
    MonthlySummary = await _accountingService.GetMonthlySummaryAsync(ViewYear, ViewMonth);
    
    return Page();
}
```

### 6. 效能最佳化

### 6.1 資料載入最佳化
- 使用分頁載入避免大量資料造成效能問題
- 實作資料快取機制減少檔案讀取次數
- 使用非同步操作避免 UI 封鎖

### 6.2 前端最佳化
- 使用 CDN 載入第三方函式庫
- 壓縮 CSS 和 JavaScript 檔案
- 實作圖片延遲載入

### 7. 安全性考量

### 7.1 輸入驗證
- 前後端雙重驗證
- SQL 注入防護（雖使用 JSON，但仍需注意）
- XSS 攻擊防護

### 7.2 資料保護
- 檔案權限設定
- 敏感資料加密儲存
- 定期備份機制

### 8. 測試計畫

### 8.1 單元測試
- 資料服務方法測試
- 驗證邏輯測試
- 計算邏輯測試

### 8.2 整合測試
- 頁面互動測試
- AJAX 功能測試
- 檔案讀寫測試

### 8.3 使用者測試
- 新增記錄流程測試
- 編輯記錄流程測試
- 刪除記錄流程測試
- 報表統計準確性測試

### 9. 部署與維護

### 9.1 部署需求
- .NET 8.0 Runtime
- IIS 或 Kestrel 伺服器
- 檔案系統讀寫權限

### 9.2 維護計畫
- 定期資料備份
- 效能監控
- 錯誤日誌記錄
- 使用者回饋收集

## 開發時程規劃

### Phase 1: 基礎架構 (3天)
- 專案環境設定
- 資料模型定義
- 基本服務建立

### Phase 2: 列表頁面 (5天)
- 月曆檢視實作
- 篩選功能開發
- 統計功能實作

### Phase 3: 編輯頁面 (7天)
- 表單設計與實作
- 驗證邏輯開發
- 分類系統實作

### Phase 4: 整合測試 (3天)
- 功能整合測試
- 效能調校
- Bug 修復

### Phase 5: 部署上線 (2天)
- 生產環境部署
- 使用者驗收測試
- 文件整理

**總預估開發時間**: 20個工作天

## 注意事項
1. 金額欄位使用 `long` 型別避免溢位，支援大額記錄
2. 日期處理需考慮時區問題
3. JSON 檔案操作需考慮並發存取問題
4. 分類系統設計需具備擴充性
5. 使用者介面需支援響應式設計
6. 實作適當的錯誤處理和日誌記錄機制
