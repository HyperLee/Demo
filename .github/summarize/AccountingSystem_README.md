# 會計系統 - 完整實作

## 系統概述

本會計系統為基於 ASP.NET Core 8.0 Razor Pages 的網頁應用程式，提供個人或小型企業的收支記錄管理功能。系統採用 JSON 檔案儲存資料，具備響應式設計，支援桌面和行動裝置使用。

## 核心功能特色

### 📊 帳務管理
- **收支記錄**：支援收入和支出兩種類型記錄
- **分類管理**：提供二階分類系統（大分類 > 細分類）
- **日期管理**：支援歷史記錄，不允許未來日期
- **金額驗證**：整數金額，最大支援 999,999,999
- **付款方式**：現金、信用卡、銀行轉帳、電子支付等

### 🗓️ 行事曆檢視
- **月曆界面**：直觀的月曆檢視，顯示每日收支狀況
- **每日統計**：顯示每日收入、支出和餘額
- **記錄預覽**：點擊日期可查看當日所有記錄
- **顏色標示**：收入為綠色，支出為紅色

### 📈 統計分析
- **月度統計**：當月總收入、總支出、結餘
- **每日平均**：平均每日收支狀況
- **視覺化**：使用顏色和圖示增強資料可讀性

### 📱 響應式設計
- **行動優化**：支援手機、平板電腦操作
- **Bootstrap 5**：現代化的使用者介面
- **適應性排版**：自動調整不同螢幕尺寸

## 技術架構

### 後端架構
```
Models/
├── AccountingModels.cs          # 資料模型定義
    ├── AccountingRecord         # 會計記錄主模型
    ├── AccountingCategory       # 分類定義模型  
    ├── MonthlySummary          # 月度統計模型
    ├── CalendarDay             # 日曆日期模型
    └── ExportOptions           # 匯出選項模型

Services/
├── IAccountingService.cs        # 服務介面
└── AccountingService.cs         # 業務邏輯服務
    ├── 資料載入/儲存
    ├── CRUD 操作
    ├── 行事曆資料處理
    └── 分類管理

Pages/
├── index7.cshtml               # 列表頁面 (行事曆檢視)
├── index7.cshtml.cs            # 列表頁面後端邏輯
├── index8.cshtml               # 編輯頁面 (新增/修改)
└── index8.cshtml.cs            # 編輯頁面後端邏輯
```

### 前端技術
- **ASP.NET Core Razor Pages**：伺服器端渲染
- **Bootstrap 5**：響應式框架
- **jQuery**：JavaScript 函式庫
- **Font Awesome**：圖示系統
- **CSS3**：自訂樣式和動畫效果

### 資料儲存
```json
App_Data/
├── categories.json             # 分類定義檔
├── notes.json                  # 會計記錄檔
└── exchange_rates.json         # 匯率資料檔 (預備功能)
```

## 頁面說明

### index7.cshtml - 主列表頁面
**功能特色：**
- 月曆檢視界面，直觀顯示每日收支
- 月度統計卡片，即時顯示財務狀況
- 記錄管理操作（新增、編輯、刪除）
- 匯出功能（CSV 格式）
- 月份導航控制項

**關鍵組件：**
```html
<!-- 統計卡片區域 -->
<div class="stats-cards">
    <div class="card stats-card income-card">
        <div class="card-body">
            <h4><i class="fas fa-arrow-up"></i> 本月收入</h4>
            <div class="display-6 text-success">NT$ @Model.MonthlySummary.TotalIncome.ToString("N0")</div>
        </div>
    </div>
</div>

<!-- 行事曆表格 -->
<table class="table table-bordered calendar-table">
    <!-- 動態產生月曆格局 -->
</table>
```

### index8.cshtml - 編輯頁面
**功能特色：**
- 新增/編輯記錄表單
- 即時表單驗證
- 階層式分類選擇
- AJAX 動態載入子分類
- 金額格式化與驗證
- 刪除確認對話框

**關鍵組件：**
```html
<!-- 收支類型選擇 -->
<div class="btn-group w-100" role="group">
    <input type="radio" asp-for="Record.Type" value="Expense" id="typeExpense" class="btn-check" />
    <label class="btn btn-outline-danger" for="typeExpense">
        <i class="fas fa-minus-circle"></i> 支出
    </label>
    
    <input type="radio" asp-for="Record.Type" value="Income" id="typeIncome" class="btn-check" />
    <label class="btn btn-outline-success" for="typeIncome">
        <i class="fas fa-plus-circle"></i> 收入
    </label>
</div>

<!-- 金額輸入 -->
<div class="input-group">
    <span class="input-group-text">NT$</span>
    <input asp-for="Record.Amount" type="number" class="form-control money-input" 
           placeholder="0" min="0" max="999999999" step="1" required />
</div>
```

## 資料模型設計

### AccountingRecord - 主要記錄模型
```csharp
public class AccountingRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    [Required(ErrorMessage = "日期為必填欄位")]
    public DateTime Date { get; set; } = DateTime.Today;
    
    [Required(ErrorMessage = "收支類型為必填欄位")]
    [RegularExpression("Income|Expense", ErrorMessage = "收支類型必須為 Income 或 Expense")]
    public string Type { get; set; } = "Expense";
    
    [Required(ErrorMessage = "大分類為必填欄位")]
    [StringLength(100, ErrorMessage = "大分類長度不可超過 100 字元")]
    public string Category { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "細分類長度不可超過 100 字元")]
    public string SubCategory { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "金額為必填欄位")]
    [Range(1, 999999999, ErrorMessage = "金額必須介於 1 到 999,999,999 之間")]
    public decimal Amount { get; set; }
    
    [StringLength(50, ErrorMessage = "付款方式長度不可超過 50 字元")]
    public string PaymentMethod { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "備註長度不可超過 500 字元")]
    public string Note { get; set; } = string.Empty;
}
```

### 分類系統設計
```json
{
  "ExpenseCategories": [
    {
      "name": "日常生活",
      "subcategories": ["飲食", "交通", "娛樂", "購物", "其他"]
    },
    {
      "name": "固定支出", 
      "subcategories": ["房租", "水電費", "電信費", "保險", "貸款"]
    }
  ],
  "IncomeCategories": [
    {
      "name": "主要收入",
      "subcategories": ["薪資", "獎金", "兼職", "其他"]
    },
    {
      "name": "投資收入",
      "subcategories": ["股利", "利息", "租金", "其他"]
    }
  ]
}
```

## 服務層架構

### IAccountingService 介面
```csharp
public interface IAccountingService
{
    Task<List<AccountingRecord>> LoadRecordsAsync();
    Task SaveRecordsAsync(List<AccountingRecord> records);
    Task<List<AccountingCategory>> LoadCategoriesAsync();
    Task<MonthlySummary> GetMonthlySummaryAsync(int year, int month);
    Task<List<CalendarDay>> GetCalendarDataAsync(int year, int month);
    Task<AccountingRecord?> GetRecordByIdAsync(string id);
    Task<string> AddRecordAsync(AccountingRecord record);
    Task<bool> UpdateRecordAsync(AccountingRecord record);
    Task<bool> DeleteRecordAsync(string id);
}
```

### AccountingService 實作
**核心功能：**
- JSON 檔案管理
- 資料快取機制
- 異常處理
- 記錄驗證
- 統計計算

## 前端互動設計

### JavaScript 功能
```javascript
// 階層式分類載入
async function loadSubCategories(category, type) {
    const response = await fetch(`/index8?handler=SubCategories&category=${encodeURIComponent(category)}&type=${encodeURIComponent(type)}`);
    const subCategories = await response.json();
    // 更新子分類選單
}

// 表單驗證
function validateForm() {
    let isValid = true;
    
    // 驗證日期
    if (!dateInput.value) {
        dateInput.classList.add('is-invalid');
        isValid = false;
    }
    
    // 驗證金額
    if (!amount || amount <= 0) {
        amountInput.classList.add('is-invalid');
        isValid = false;
    }
    
    return isValid;
}
```

### CSS 自訂樣式
```css
.calendar-day {
    min-height: 120px;
    vertical-align: top;
    background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
}

.record-item {
    display: block;
    margin: 2px 0;
    padding: 2px 4px;
    font-size: 0.75rem;
    border-radius: 3px;
    text-decoration: none;
}

.income-item {
    background-color: rgba(25, 135, 84, 0.1);
    color: #198754;
    border-left: 3px solid #198754;
}

.expense-item {
    background-color: rgba(220, 53, 69, 0.1);
    color: #dc3545;
    border-left: 3px solid #dc3545;
}
```

## 安裝與執行

### 系統需求
- .NET 8.0 SDK
- Visual Studio 2022 或 Visual Studio Code
- 現代化瀏覽器 (Chrome, Firefox, Safari, Edge)

### 執行步驟
```bash
# 1. 複製專案
git clone <repository-url>
cd Demo

# 2. 還原套件
dotnet restore

# 3. 建構專案
dotnet build

# 4. 執行應用程式
dotnet run

# 5. 開啟瀏覽器
# https://localhost:7001/index7 (列表頁面)
# https://localhost:7001/index8 (編輯頁面)
```

### 初始設定
系統首次啟動時會自動：
1. 建立 `App_Data` 目錄
2. 初始化 `categories.json` 分類檔案
3. 建立空的 `notes.json` 記錄檔案

## 未來功能擴展

### 短期目標
- [ ] 匯入/匯出 Excel 功能
- [ ] 圖表統計分析
- [ ] 預算設定與監控
- [ ] 搜尋與篩選功能

### 中期目標
- [ ] 多使用者支援
- [ ] 資料庫整合 (SQL Server/SQLite)
- [ ] 行動應用程式 (PWA)
- [ ] 自動備份機制

### 長期目標
- [ ] 銀行帳戶連接
- [ ] AI 支出分析
- [ ] 投資組合管理
- [ ] 多幣別支援

## 技術亮點

### 程式碼品質
- **C# 13** 最新語法特性
- **Nullable Reference Types** 空值安全
- **異步程式設計** 效能最佳化
- **相依性注入** 架構解耦
- **錯誤處理** 完整的異常管理

### 使用者體驗
- **響應式設計** 跨裝置相容
- **即時驗證** 即時回饋錯誤
- **視覺化回饋** 動畫與過渡效果
- **直觀操作** 符合使用習慣的介面設計

### 資料安全
- **輸入驗證** 前後端雙重驗證
- **XSS 防護** 自動編碼輸出
- **CSRF 保護** 表單令牌驗證

## 開發團隊與維護

**開發技術棧：**
- ASP.NET Core 8.0
- C# 13
- Bootstrap 5
- jQuery 3.x
- Font Awesome 6

**程式碼標準：**
- 遵循 Microsoft C# 編碼規範
- XML 文件註解
- 單元測試覆蓋
- 程式碼審查流程

---

## 聯絡資訊

如有任何問題或建議，請透過以下方式聯絡：
- 📧 Email: [your-email@example.com]
- 🐛 Issue Tracker: [GitHub Issues URL]
- 📖 Wiki: [Documentation URL]

**最後更新日期：** 2024年12月19日
**版本：** 1.0.0
