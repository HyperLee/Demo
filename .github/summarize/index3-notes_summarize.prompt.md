---
description: index3 註記功能技術詳細說明
created: 2025年8月27日
version: 1.0
feature: 日期註記系統
storage: JSON 檔案
---

# index3 註記功能技術總結

## 🎯 功能概述

為 `index3` 月曆頁面新增的個人化註記功能，採用階段2實作方案（JSON 檔案儲存），適合個人使用情境。使用者可以針對任意日期新增、修改、刪除文字註記。

## 🔴 核心檔案位置

### 📁 重要檔案清單
```
專案結構 (D:\Demo\Demo\):

🔧 核心實作檔案:
├── Demo/Services/NoteService.cs           # 註記服務核心邏輯
├── Demo/Pages/index3.cshtml.cs            # PageModel 後端邏輯整合
├── Demo/Pages/index3.cshtml               # 前端 UI 註記表單
└── Demo/Program.cs                        # 依賴注入設定

📄 資料儲存檔案 (🔴 重要備份目標):
└── Demo/App_Data/notes.json               # 註記資料持久化檔案
    完整路徑: D:\Demo\Demo\Demo\App_Data\notes.json
```

## 🏗️ 架構設計

### 三層架構
1. **展示層 (Presentation)**：`index3.cshtml` - 註記表單 UI
2. **業務邏輯層 (Business)**：`Index3Model` - PageModel 處理
3. **資料存取層 (Data Access)**：`INoteService` - 檔案 I/O 抽象

### 依賴注入架構
```csharp
// Program.cs - 服務注入
builder.Services.AddSingleton<INoteService, JsonNoteService>();

// Index3Model - 構造函式注入
public Index3Model(ILogger<Index3Model> logger, INoteService noteService)
{
    this.logger = logger;
    this.noteService = noteService;
}
```

## 📊 資料格式設計

### JSON 檔案結構
**🔴 檔案位置**: `D:\Demo\Demo\Demo\App_Data\notes.json`
```json
{
  "2025-08-27": "重要會議 - 與客戶討論專案需求",
  "2025-08-28": "生日聚會\n記得帶禮物",
  "2025-09-01": "專案截止日",
  "2025-12-25": "聖誕節假期"
}
```

### 資料格式規範
- **鍵格式**: `yyyy-MM-dd` (ISO 8601 標準)
- **值格式**: UTF-8 編碼純文字，支援換行符
- **檔案編碼**: UTF-8 with BOM
- **檔案權限**: 應用程式讀寫權限

## 🔧 核心技術實作

### 1. INoteService 介面設計
```csharp
public interface INoteService
{
    /// <summary>取得指定日期的註記</summary>
    Task<string?> GetNoteAsync(DateOnly date);
    
    /// <summary>儲存或更新指定日期的註記</summary>
    Task SaveNoteAsync(DateOnly date, string note);
    
    /// <summary>刪除指定日期的註記</summary>
    Task DeleteNoteAsync(DateOnly date);
}
```

### 2. JsonNoteService 實作重點
```csharp
public sealed class JsonNoteService : INoteService
{
    // 🔴 重要: 檔案路徑固定化，防止路徑遍歷攻擊
    private readonly string notesFilePath = Path.Combine(
        Directory.GetCurrentDirectory(), 
        "App_Data", 
        "notes.json"
    );
    
    // 🔴 重要: 執行緒安全鎖，防止併發檔案操作衝突
    private readonly SemaphoreSlim fileLock = new(1, 1);
    
    // 🔴 重要: 非同步檔案 I/O 操作
    public async Task SaveNoteAsync(DateOnly date, string note)
    {
        await fileLock.WaitAsync();  // 取得排他鎖
        try
        {
            var notes = await LoadNotesAsync();
            var key = date.ToString("yyyy-MM-dd");
            
            if (string.IsNullOrWhiteSpace(note))
                notes.Remove(key);      // 空內容視為刪除操作
            else
                notes[key] = note.Trim();
                
            await SaveNotesToFileAsync(notes);
        }
        finally
        {
            fileLock.Release();  // 確保鎖被釋放
        }
    }
}
```

### 3. JSON 序列化設定
```csharp
private static readonly JsonSerializerOptions JsonOptions = new()
{
    WriteIndented = true,  // 格式化輸出，便於人工檢視
    Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping  // 支援中文字符
};
```

## 🖥️ 前端 UI 設計

### 註記表單結構
```html
<!-- 🔴 重要: 只有選取日期時才顯示註記區域 -->
@if (Model.SelectedDate is not null)
{
    <div class="card shadow-sm mt-3" style="border-left: 4px solid #28a745;">
        <div class="card-body">
            <!-- 註記輸入表單 -->
            <form method="post" class="row g-3">
                <!-- 隱藏欄位保持日期參數 -->
                <input type="hidden" name="Year" value="@Model.Year" />
                <input type="hidden" name="Month" value="@Model.Month" />
                <input type="hidden" name="Day" value="@Model.Day" />
                
                <!-- 多行文字輸入框 -->
                <textarea name="NoteText" rows="3" 
                          placeholder="在此輸入您的備註..."
                          style="resize: vertical; min-height: 80px;">
                    @Model.NoteText
                </textarea>
                
                <!-- 操作按鈕群組 -->
                <button type="submit" formaction="/index3?handler=SaveNote">
                    儲存備註
                </button>
                
                <button type="submit" formaction="/index3?handler=DeleteNote"
                        onclick="return confirm('確定要刪除這個備註嗎？')">
                    刪除備註
                </button>
            </form>
            
            <!-- 目前儲存的註記顯示區 -->
            @if (!string.IsNullOrWhiteSpace(Model.SelectedDateNote))
            {
                <div style="white-space: pre-wrap;">@Model.SelectedDateNote</div>
            }
        </div>
    </div>
}
```

### CSS 樣式特色
- **綠色主題**: 使用 Bootstrap 成功色系 (#28a745)
- **左邊框強調**: `border-left: 4px solid #28a745`
- **換行保持**: `white-space: pre-wrap` 顯示原始換行
- **垂直調整**: `resize: vertical` 允許調整文字區域高度

## 🔄 業務流程

### 1. GET 請求流程
```mermaid
graph LR
    A[用戶選取日期] --> B[OnGetAsync()]
    B --> C[設定 SelectedDate]
    C --> D[呼叫 GetNoteAsync()]
    D --> E[載入註記內容]
    E --> F[預填入表單]
    F --> G[渲染頁面]
```

### 2. POST 儲存流程
```mermaid
graph LR
    A[用戶提交表單] --> B[OnPostSaveNoteAsync()]
    B --> C[驗證日期參數]
    C --> D[呼叫 SaveNoteAsync()]
    D --> E[更新 JSON 檔案]
    E --> F[RedirectToPage 保持選取狀態]
```

### 3. POST 刪除流程
```mermaid
graph LR
    A[用戶確認刪除] --> B[OnPostDeleteNoteAsync()]
    B --> C[驗證日期參數]
    C --> D[呼叫 DeleteNoteAsync()]
    D --> E[從 JSON 移除項目]
    E --> F[RedirectToPage 保持選取狀態]
```

## 🛡️ 安全與可靠性

### 安全措施
1. **路徑安全**: 檔案路徑硬編碼，防止路徑遍歷攻擊
2. **參數驗證**: 所有日期參數經過後端驗證
3. **XSS 防護**: Razor Pages 自動 HTML 編碼
4. **檔案權限**: 限制在 App_Data 資料夾內

### 錯誤處理
```csharp
private async Task<Dictionary<string, string>> LoadNotesAsync()
{
    try
    {
        if (!File.Exists(notesFilePath))
            return new Dictionary<string, string>();  // 檔案不存在時返回空字典

        var json = await File.ReadAllTextAsync(notesFilePath);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) 
               ?? new Dictionary<string, string>();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "載入註記檔案時發生錯誤：{FilePath}", notesFilePath);
        return new Dictionary<string, string>();  // 發生錯誤時返回空字典
    }
}
```

### 併發控制
- **SemaphoreSlim**: 防止多個請求同時寫入檔案
- **非同步鎖**: 使用 `await fileLock.WaitAsync()` 
- **異常安全**: `try-finally` 確保鎖被釋放

## 📊 效能考量

### 優化策略
1. **非同步 I/O**: 所有檔案操作使用 `async/await`
2. **記憶體效率**: 使用 `Dictionary<string, string>` 而非複雜物件
3. **序列化最佳化**: 使用 `System.Text.Json` 而非 Newtonsoft.Json

### 效能限制
- **檔案大小**: 單一 JSON 檔案，不適合超過 10MB 的資料量
- **記憶體使用**: 整個檔案載入記憶體，大檔案時需考慮分頁
- **I/O 延遲**: 每次操作都涉及磁碟讀寫

## 🔧 維護與監控

### 🔴 重要維護工作
1. **定期備份**: 每日備份 `App_Data/notes.json`
2. **檔案大小監控**: 注意檔案增長趨勢，避免過大
3. **權限檢查**: 確保應用程式對 App_Data 資料夾有讀寫權限
4. **日誌監控**: 觀察註記操作的錯誤率

### 監控指標
- **註記總數**: JSON 檔案中的鍵值對數量
- **檔案大小**: notes.json 檔案大小變化
- **操作頻率**: 儲存/刪除操作的頻率統計
- **錯誤率**: 檔案 I/O 操作的失敗率

## 🚀 未來擴展方向

### 短期改進 (1-3 個月)
1. **搜尋功能**: 在註記內容中搜尋關鍵字
2. **匯出功能**: 支援匯出為 CSV 或 PDF
3. **備份機制**: 自動建立註記檔案備份

### 中期改進 (3-6 個月)  
1. **分類標籤**: 為註記添加分類和標籤系統
2. **富文本**: 支援格式化文字、連結等
3. **提醒通知**: 整合瀏覽器通知 API

### 長期改進 (6+ 個月)
1. **資料庫遷移**: 升級至 SQLite 或 SQL Server
2. **雲端同步**: 整合 OneDrive 或 Google Drive
3. **多用戶支援**: 支援個人帳戶和權限管理

## 📋 部署檢查清單

### 🔴 部署前必檢項目
- [ ] 確認 `App_Data` 資料夾存在且有讀寫權限
- [ ] 檢查 `Program.cs` 中的服務注入設定
- [ ] 驗證 JSON 序列化設定支援中文字符
- [ ] 測試併發存取情境 (多個瀏覽器分頁同時操作)
- [ ] 確認錯誤處理邏輯 (檔案損壞、權限不足等)

### 🔴 部署後驗證項目
- [ ] 新增註記功能正常
- [ ] 修改註記功能正常
- [ ] 刪除註記功能正常
- [ ] 頁面重整後註記內容保持
- [ ] 不同日期間切換註記內容正確顯示
- [ ] 空白註記自動刪除功能正常

## 📈 成功指標

### 功能指標
- ✅ 註記新增成功率 > 99%
- ✅ 註記載入速度 < 500ms
- ✅ 檔案 I/O 錯誤率 < 0.1%
- ✅ 併發操作無資料遺失

### 用戶體驗指標
- ✅ 表單提交後保持日期選取狀態
- ✅ 註記內容支援換行顯示  
- ✅ 操作確認 (刪除註記需要確認)
- ✅ 錯誤訊息友善易懂

## 📄 相關文件

### 技術文件
- **主要總結**: `index3_summarize.prompt.md` - 完整技術總結
- **API 文件**: 程式碼內的 XML 文件註釋
- **架構圖**: 本文件內的 Mermaid 流程圖

### 🔴 重要提醒
**此功能採用檔案儲存，請務必:**
1. **定期備份** `App_Data/notes.json` 檔案
2. **監控檔案大小**，避免過度增長影響效能  
3. **檢查檔案權限**，確保應用程式可正常存取
4. **測試災難恢復**，模擬檔案損壞時的處理流程

註記功能已成功整合至 index3 月曆頁面，提供了實用的個人化日程管理體驗！
