# 記帳系統技術總結 - index7 & index8

> **專案版本**: ASP.NET Core 8.0  
> **開發日期**: 2025年8月  
> **主要功能**: 月曆檢視記帳系統、多格式報表匯出、動態表單驗證

## 📋 系統概述

記帳系統由兩個核心頁面組成，提供完整的個人財務管理解決方案：

- **index7.cshtml**: 記帳列表頁面 - 月曆檢視、統計分析、報表匯出
- **index8.cshtml**: 記帳編輯頁面 - 新增/修改記錄、動態表單驗證

## 🏗️ 技術架構

### 後端架構
```
Pages/
├── index7.cshtml               # 記帳列表頁面
├── index7.cshtml.cs            # 列表頁面邏輯
├── index8.cshtml               # 記帳編輯頁面
└── index8.cshtml.cs            # 編輯頁面邏輯

Services/
└── AccountingService.cs        # 記帳資料服務

Models/
└── AccountingModels.cs         # 記帳資料模型

Utilities/
└── PdfExportUtility.cs         # PDF匯出工具
```

### 前端技術棧
- **UI框架**: Bootstrap 5
- **JavaScript**: 原生 ES6+
- **圖標**: Font Awesome
- **樣式**: SCSS/CSS3
- **驗證**: ASP.NET Core Model Validation + 客戶端驗證

## 📊 index7 - 記帳列表頁面

### 核心功能特色

#### 🗓️ 月曆檢視設計
```csharp
public class CalendarDay
{
    public DateTime Date { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public List<AccountingRecord> Records { get; set; }
    public decimal DailyIncome { get; set; }
    public decimal DailyExpense { get; set; }
}
```

**技術亮點:**
- 動態產生月曆格局，支援跨月份瀏覽
- 每日收支統計即時計算
- 記錄數量徽章顯示
- 今日日期高亮標示

#### 📈 統計卡片系統
```html
<div class="col-lg-3 col-md-6 mb-3">
    <div class="card border-success shadow-sm">
        <div class="card-body text-center">
            <h6 class="card-title text-success mb-1">總收入</h6>
            <h4 class="text-success mb-0">NT$ @Model.MonthlySummary.TotalIncome.ToString("N0")</h4>
            <small class="text-muted">@Model.MonthlySummary.IncomeRecords 筆記錄</small>
        </div>
    </div>
</div>
```

**功能特色:**
- 即時統計：總收入、總支出、淨收支
- 視覺化指標：收入綠色、支出紅色、淨收支動態顏色
- 記錄數量統計
- 響應式卡片佈局

#### 📄 多格式匯出系統

**CSV匯出 - 中文編碼優化**
```csharp
private byte[] GenerateCsvExport(List<AccountingRecord> records)
{
    var csv = new StringBuilder();
    csv.AppendLine("日期,類型,大分類,細分類,金額,付款方式,備註");
    
    foreach (var record in records.OrderBy(r => r.Date))
    {
        var type = record.Type == "Income" ? "收入" : "支出";
        var escapedCategory = (record.Category ?? "").Replace("\"", "\"\"");
        // ... 處理特殊字元
        
        csv.AppendLine($"{record.Date:yyyy-MM-dd}," +
                      $"\"{type}\"," +
                      $"\"{escapedCategory}\"," +
                      // ...
                      );
    }
    
    // 關鍵：UTF-8 with BOM 解決中文亂碼
    var encoding = new UTF8Encoding(true);
    return encoding.GetBytes(csv.ToString());
}
```

**Excel匯出 - 多工作表設計**
```csharp
private Task<byte[]> GenerateExcelExport(List<AccountingRecord> records, ExportOptions options)
{
    using (var workbook = new XLWorkbook())
    {
        // 統計摘要工作表
        var summarySheet = workbook.Worksheets.Add("統計摘要");
        CreateSummarySheet(summarySheet, records, options);
        
        // 詳細記錄工作表
        var detailSheet = workbook.Worksheets.Add("詳細記錄");
        CreateDetailSheet(detailSheet, records);
        
        // 分類分析工作表
        var analysisSheet = workbook.Worksheets.Add("分類分析");
        CreateAnalysisSheet(analysisSheet, records);
        
        using (var stream = new MemoryStream())
        {
            workbook.SaveAs(stream);
            return Task.FromResult(stream.ToArray());
        }
    }
}
```

**PDF匯出 - 中文字型支援**
```csharp
private byte[] GeneratePdfExport(List<AccountingRecord> records, ExportOptions options)
{
    var htmlReport = GenerateAccountingHtmlReport(records, options);
    var pdfBytes = PdfExportUtility.ConvertHtmlToPdfWithChineseSupport(htmlReport, _logger);
    return pdfBytes;
}
```

### 前端互動設計

#### 月曆互動功能
```css
.calendar-day {
    position: relative;
    padding: 8px;
    border: 1px solid #dee2e6 !important;
    background-color: white;
    transition: all 0.3s ease;
}

.calendar-day.today {
    background-color: #e3f2fd;
    border-color: #2196f3 !important;
}
```

#### 刪除確認機制
```javascript
async function executeDelete() {
    const confirmBtn = document.getElementById('confirmDeleteBtn');
    const originalText = confirmBtn.innerHTML;
    
    try {
        confirmBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> 刪除中...';
        confirmBtn.disabled = true;
        
        const response = await fetch('/index7?handler=DeleteRecord', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ id: deleteRecordId })
        });
        
        const result = await response.json();
        if (result.success) {
            location.reload();
        }
    } finally {
        confirmBtn.innerHTML = originalText;
        confirmBtn.disabled = false;
    }
}
```

## ✏️ index8 - 記帳編輯頁面

### 資料模型設計

#### 檢視模型 (ViewModel)
```csharp
public class AccountingRecordViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "請選擇日期")]
    [Display(Name = "日期")]
    public DateTime Date { get; set; }

    [Required(ErrorMessage = "請選擇收支類型")]
    [Display(Name = "收支類型")]
    public string Type { get; set; } = string.Empty;

    [Required(ErrorMessage = "請輸入金額")]
    [Range(0.01, 999999999, ErrorMessage = "金額必須介於 0.01 到 999,999,999 之間")]
    [Display(Name = "金額")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "請選擇大分類")]
    [Display(Name = "大分類")]
    public string Category { get; set; } = string.Empty;

    [Display(Name = "細分類")]
    public string? SubCategory { get; set; }

    [Required(ErrorMessage = "請選擇付款方式")]
    [Display(Name = "付款方式")]
    public string PaymentMethod { get; set; } = "現金";

    [MaxLength(500, ErrorMessage = "備註不可超過 500 字元")]
    [Display(Name = "備註")]
    public string? Note { get; set; }
}
```

### 動態表單功能

#### AJAX 子分類載入
```csharp
public async Task<IActionResult> OnGetSubCategoriesAsync(string category, string type)
{
    try
    {
        var categories = await _accountingService.GetCategoriesAsync(type);
        var selectedCategory = categories.FirstOrDefault(c => c.Name == category);
        
        if (selectedCategory?.SubCategories?.Any() == true)
        {
            var subCategories = selectedCategory.SubCategories.Select(sc => new { 
                value = sc.Name, 
                text = sc.Name 
            }).ToList();
            
            return new JsonResult(subCategories);
        }
        
        return new JsonResult(new List<object>());
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "取得子分類時發生錯誤");
        return new JsonResult(new { error = "取得子分類時發生錯誤" });
    }
}
```

#### 前端動態載入實作
```javascript
async function loadSubCategories(category, type) {
    const subCategorySelect = document.getElementById('subCategorySelect');
    
    if (!category) {
        subCategorySelect.innerHTML = '<option value="">請選擇細分類</option>';
        return;
    }

    try {
        const response = await fetch(`/index8?handler=SubCategories&category=${encodeURIComponent(category)}&type=${encodeURIComponent(type)}`);
        const subCategories = await response.json();
        
        subCategorySelect.innerHTML = '<option value="">請選擇細分類</option>';
        
        subCategories.forEach(sub => {
            const option = document.createElement('option');
            option.value = sub.value;
            option.textContent = sub.text;
            subCategorySelect.appendChild(option);
        });
    } catch (error) {
        console.error('載入子分類時發生錯誤:', error);
        subCategorySelect.innerHTML = '<option value="">載入失敗</option>';
    }
}
```

### 金額處理系統

#### 即時金額驗證
```javascript
async function validateAmount() {
    const amountInput = document.querySelector('.money-input');
    const amount = parseFloat(amountInput.value);
    
    if (!amount || amount <= 0) {
        showAmountError("金額必須大於 0");
        return;
    }

    if (amount > 999999999) {
        showAmountError("金額不可超過 999,999,999");
        return;
    }

    try {
        const response = await fetch('/index8?handler=ValidateAmount', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ amount: amount })
        });
        
        const result = await response.json();
        if (!result.valid) {
            showAmountError(result.message);
        } else {
            clearAmountError();
        }
    } catch (error) {
        console.error('驗證金額時發生錯誤:', error);
    }
}
```

#### 金額格式化顯示
```javascript
function updateAmountDisplay() {
    const amountInput = document.querySelector('.money-input');
    const value = amountInput.value;
    
    if (value && !isNaN(value) && value !== '') {
        const numericValue = parseInt(value.replace(/[^\d]/g, ''));
        if (!isNaN(numericValue) && numericValue > 0) {
            // 顯示格式化的金額（千分位）
            const formattedValue = numericValue.toLocaleString();
            updateAmountDisplay(formattedValue);
        }
    }
}
```

### 表單驗證機制

#### 客戶端驗證
```javascript
function validateForm() {
    let isValid = true;
    const form = document.getElementById('recordForm');
    
    // 清除先前的驗證樣式
    form.querySelectorAll('.is-invalid').forEach(el => el.classList.remove('is-invalid'));
    
    // 驗證日期
    const dateInput = document.querySelector('input[name="Record.Date"]');
    if (!dateInput.value) {
        dateInput.classList.add('is-invalid');
        isValid = false;
    } else {
        const selectedDate = new Date(dateInput.value);
        const today = new Date();
        today.setHours(23, 59, 59, 999);
        
        if (selectedDate > today) {
            dateInput.classList.add('is-invalid');
            showError("記錄日期不可為未來日期");
            isValid = false;
        }
    }
    
    // 驗證收支類型
    const typeChecked = document.querySelector('input[name="Record.Type"]:checked');
    if (!typeChecked) {
        showError("請選擇收支類型");
        isValid = false;
    }
    
    // 驗證金額
    const amountInput = document.querySelector('.money-input');
    const amount = parseFloat(amountInput.value);
    if (!amount || amount <= 0) {
        amountInput.classList.add('is-invalid');
        showError("請輸入有效金額");
        isValid = false;
    }
    
    // 驗證大分類
    const categorySelect = document.getElementById('categorySelect');
    if (!categorySelect.value) {
        categorySelect.classList.add('is-invalid');
        showError("請選擇大分類");
        isValid = false;
    }
    
    return isValid;
}
```

#### 伺服器端驗證
```csharp
public async Task<IActionResult> OnPostAsync()
{
    try
    {
        await LoadSelectListsAsync();

        if (!ModelState.IsValid)
        {
            ViewData["Title"] = PageTitle;
            return Page();
        }

        // 驗證日期不可為未來
        if (Record.Date.Date > DateTime.Today)
        {
            ModelState.AddModelError("Record.Date", "記錄日期不可為未來日期");
            ViewData["Title"] = PageTitle;
            return Page();
        }

        // 驗證金額
        if (Record.Amount <= 0)
        {
            ModelState.AddModelError("Record.Amount", "金額必須大於 0");
            ViewData["Title"] = PageTitle;
            return Page();
        }

        // 建立記帳記錄物件
        var accountingRecord = new AccountingRecord
        {
            Id = Record.Id,
            Date = Record.Date,
            Type = Record.Type,
            Amount = Record.Amount,
            Category = Record.Category,
            SubCategory = Record.SubCategory ?? string.Empty,
            PaymentMethod = Record.PaymentMethod,
            Note = Record.Note ?? string.Empty
        };

        // 執行新增或更新邏輯...
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "儲存記帳記錄時發生錯誤");
        ModelState.AddModelError("", "儲存時發生錯誤，請稍後再試。");
        ViewData["Title"] = PageTitle;
        return Page();
    }
}
```

## 🎨 UI/UX 設計特色

### 響應式設計
```css
@media (max-width: 768px) {
    .calendar-table {
        font-size: 0.7rem;
    }
    
    .calendar-day {
        height: 80px;
        padding: 4px;
    }
    
    .record-item {
        font-size: 0.65rem;
    }
    
    .record-detail {
        display: none; /* 手機版隱藏詳細資訊 */
    }
}

@media (max-width: 576px) {
    .container {
        padding-left: 10px;
        padding-right: 10px;
    }
    
    .card-body {
        padding: 1rem;
    }
    
    .btn-lg {
        padding: 0.5rem 1rem;
        font-size: 1rem;
    }
}
```

### 互動式動畫效果
```css
.card {
    transition: box-shadow 0.3s ease;
}

.card:hover {
    box-shadow: 0 0.25rem 0.75rem rgba(0, 0, 0, 0.1) !important;
}

.btn-check:checked + .btn-outline-success,
.btn-check:checked + .btn-outline-danger {
    transform: scale(1.02);
    box-shadow: 0 0.25rem 0.5rem rgba(0, 0, 0, 0.1);
}

.money-input:focus {
    border-color: #28a745;
    box-shadow: 0 0 0 0.2rem rgba(40, 167, 69, 0.25);
}
```

### 顏色系統設計
```css
.income {
    color: #28a745 !important; /* 收入：綠色 */
}

.expense {
    color: #dc3545 !important; /* 支出：紅色 */
}

.calendar-day.today {
    background-color: #e3f2fd; /* 今日：淺藍色 */
    border-color: #2196f3 !important;
}

.calendar-day.other-month {
    background-color: #f8f9fa; /* 非當月：淺灰色 */
    color: #6c757d;
}
```

## 🔧 技術特色與創新

### 1. 中文編碼最佳化
- **CSV匯出**: 使用 UTF-8 with BOM 編碼，解決 Excel 開啟中文亂碼問題
- **PDF匯出**: 整合中文字型支援，確保繁體中文正確顯示
- **Excel匯出**: 原生支援中文字元，無需額外處理

### 2. 大數值處理機制
```csharp
[Range(0.01, 999999999, ErrorMessage = "金額必須介於 0.01 到 999,999,999 之間")]
public decimal Amount { get; set; }
```
- 支援最高 9.99 億金額
- 使用 `decimal` 型別確保精確度
- 千分位格式化顯示 (`N0` 格式)

### 3. 錯誤處理與回退機制
```csharp
case "excel":
    try
    {
        fileData = await GenerateExcelExport(records, exportOptions);
        fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Excel 匯出失敗，回退到 CSV 格式");
        fileData = GenerateCsvExport(records);
        fileName = $"記帳報表_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        contentType = "text/csv";
    }
    break;
```

### 4. 記憶體最佳化
```csharp
using (var workbook = new XLWorkbook())
{
    // 工作表建立邏輯...
    
    using (var stream = new MemoryStream())
    {
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
} // 自動釋放資源
```

## 📈 效能最佳化

### 1. 資料載入策略
- 按月份載入資料，減少記憶體使用
- LINQ 查詢最佳化，避免 N+1 問題
- 非同步處理，提升使用者體驗

### 2. 前端效能
```javascript
// 使用節流機制避免頻繁 AJAX 請求
let validateTimeout;
amountInput.addEventListener('input', function() {
    clearTimeout(validateTimeout);
    validateTimeout = setTimeout(validateAmount, 500);
});
```

### 3. 快取策略
- 分類選項快取，減少重複載入
- 靜態資源版本控制
- 瀏覽器快取最佳化

## 🔒 安全性設計

### 1. CSRF 防護
```html
@Html.AntiForgeryToken()
```

### 2. 輸入驗證
- 伺服器端模型驗證
- 客戶端即時驗證
- SQL 注入防護（使用 ORM）

### 3. 資料清理
```csharp
var escapedCategory = (record.Category ?? "").Replace("\"", "\"\"");
var escapedNote = (record.Note ?? "").Replace("\"", "\"\"");
```

## 🧪 測試策略

### 單元測試覆蓋
- 資料模型驗證測試
- 服務層邏輯測試
- 匯出功能測試

### 整合測試
- 頁面載入測試
- 表單提交測試
- AJAX 功能測試

## 📚 學習價值與技術洞察

### 1. ASP.NET Core 最佳實務
- Razor Pages 架構應用
- Model Binding 與驗證
- 依賴注入設計模式

### 2. 前端整合技術
- Bootstrap 5 響應式設計
- JavaScript ES6+ 特性應用
- CSS3 動畫與互動效果

### 3. 資料處理技術
- Excel 操作 (ClosedXML)
- PDF 生成 (iText7)
- CSV 編碼處理

### 4. 使用者體驗設計
- 漸進式表單驗證
- 載入狀態指示
- 錯誤提示與回饋機制

## 🚀 未來擴展建議

### 功能擴展
1. **圖表分析**: 集成 Chart.js 提供視覺化分析
2. **預算管理**: 添加月度預算設定與追蹤
3. **標籤系統**: 支援多標籤分類
4. **匯入功能**: 支援 CSV/Excel 資料匯入

### 技術優化
1. **快取機制**: Redis 快取熱點資料
2. **分頁載入**: 大資料量分頁處理
3. **離線支援**: PWA 離線功能
4. **多語言支援**: i18n 國際化

---

## 📖 參考資源

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Bootstrap 5 Documentation](https://getbootstrap.com/docs/5.0)
- [ClosedXML Documentation](https://closedxml.readthedocs.io)
- [iText7 Documentation](https://itextpdf.com/itext7)

---

*此技術文件詳細記錄了記帳系統的完整實作過程，包含所有核心功能、技術決策和最佳實務。適合作為 ASP.NET Core 企業級應用開發的參考範例。*
