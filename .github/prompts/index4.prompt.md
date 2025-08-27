# 備忘錄系統開發規格書

## 系統概述
本系統提供備忘錄管理功能，包含列表展示和詳細頁面編輯。使用 ASP.NET Core Razor Pages 框架開發，採用 JSON 檔案作為資料儲存方案。

## 檔案結構
```
Demo/
├── Pages/
│   ├── index4.cshtml           # 備忘錄列表頁面
│   ├── index4.cshtml.cs        # 列表頁面後端邏輯
│   ├── index5.cshtml           # 備忘錄詳細/編輯頁面
│   └── index5.cshtml.cs        # 詳細頁面後端邏輯
├── Services/
│   └── NoteService.cs          # 備忘錄服務類別
└── App_Data/
    └── notes.json              # 資料檔案
```

## Part 1 - 備忘錄列表頁面 (index4)

### 1.1 頁面基本資訊
- **檔案名稱**: `index4.cshtml` / `index4.cshtml.cs`
- **路由**: `/index4`
- **標題**: "備忘錄列表"
- **功能**: 顯示所有備忘錄並提供基本操作功能

### 1.2 資料模型設計
```csharp
public class Note
{
    public int Id { get; set; }                    // 唯一識別碼
    public string Title { get; set; }              // 標題
    public string Content { get; set; }            // 內容
    public DateTime CreatedDate { get; set; }      // 建立日期
    public DateTime ModifiedDate { get; set; }     // 修改日期
}

public class NoteListViewModel
{
    public List<Note> Notes { get; set; }          // 當前頁面備忘錄清單
    public int CurrentPage { get; set; }           // 當前頁碼
    public int TotalPages { get; set; }            // 總頁數
    public int PageSize { get; set; } = 20;       // 每頁顯示數量
    public int TotalCount { get; set; }            // 總備忘錄數量
}
```

### 1.3 頁面佈局設計
#### 1.3.1 標題區域
- 顯示 "備忘錄列表" 主標題
- 右上角顯示「新增備忘錄」按鈕
- 統計資訊：顯示總共備忘錄數量

#### 1.3.2 備忘錄清單區域
- 每則備忘錄獨立顯示為一個卡片區塊
- 奇偶數列使用不同背景色區分（提高辨識度）
  - 奇數列：淺灰色背景 (#f8f9fa)
  - 偶數列：白色背景 (#ffffff)
- 每個卡片包含：
  - 標題（限制顯示50字元，超過顯示...）
  - 內容摘要（限制顯示100字元，超過顯示...）
  - 建立日期
  - 修改日期
  - 操作按鈕（查看、刪除）

#### 1.3.3 分頁控制區域
- 位於頁面底部
- 顯示當前頁碼/總頁數
- 包含：首頁、上一頁、下一頁、末頁按鈕
- 頁碼導航（顯示當前頁前後各2頁）

### 1.4 功能需求
#### 1.4.1 資料展示功能
- ✅ 從 JSON 檔案讀取所有備忘錄資料
- ✅ 按建立日期倒序排列（最新的在最前面）
- ✅ 分頁顯示，每頁20則備忘錄
- ✅ 奇偶數列不同背景色區分
- ✅ 標題和內容超長時截斷顯示

#### 1.4.2 導航功能
- ✅ 點擊備忘錄標題或「查看」按鈕 → 跳轉到詳細頁面 (index5)
- ✅ 點擊「新增備忘錄」按鈕 → 跳轉到新增頁面 (index5)
- ✅ 分頁導航功能

#### 1.4.3 操作功能
- ✅ 刪除備忘錄（含確認對話框）
- ✅ 刪除成功後自動重新整理頁面

### 1.5 使用者介面設計
#### 1.5.1 響應式設計
- 支援手機、平板、桌面版面
- 在小螢幕上調整卡片佈局和按鈕大小

#### 1.5.2 互動設計
- 滑鼠 hover 時卡片有陰影效果
- 按鈕有 hover 和 active 狀態
- 刪除確認對話框使用 Bootstrap Modal

#### 1.5.3 顏色配置
- 主色調：Bootstrap 預設色彩
- 奇數列背景：#f8f9fa
- 偶數列背景：#ffffff
- 邊框顏色：#dee2e6
- 按鈕顏色：新增(primary)、查看(info)、刪除(danger)

### 1.6 錯誤處理
- JSON 檔案不存在時自動建立空檔案
- 資料讀取失敗時顯示錯誤訊息
- 刪除失敗時顯示錯誤提示

## Part 2 - 備忘錄詳細頁面 (index5)

### 2.1 頁面基本資訊
- **檔案名稱**: `index5.cshtml` / `index5.cshtml.cs`
- **路由**: `/index5?id={noteId}` (編輯模式) / `/index5` (新增模式)
- **功能**: 新增或編輯備忘錄

### 2.2 資料模型設計
```csharp
public class NoteEditViewModel
{
    public int Id { get; set; }                    // 備忘錄ID（0表示新增）
    public string Title { get; set; }              // 標題
    public string Content { get; set; }            // 內容
    public bool IsEditMode { get; set; }           // 是否為編輯模式
    public DateTime CreatedDate { get; set; }      // 建立日期
    public DateTime ModifiedDate { get; set; }     // 修改日期
}
```

### 2.3 頁面功能需求
#### 2.3.1 新增模式 (id 參數不存在或為空)
- 頁面標題：「新增備忘錄」
- 表單欄位為空白狀態
- 建立日期和修改日期設為當前時間

#### 2.3.2 編輯模式 (id 參數存在)
- 頁面標題：「編輯備忘錄」
- 根據 ID 載入現有資料到表單
- 修改日期更新為當前時間

#### 2.3.3 表單設計
- 標題輸入框（必填，最大長度200字元）
- 內容文字區域（必填，最大長度2000字元）
- 儲存按鈕
- 取消按鈕（返回列表頁面）

#### 2.3.4 驗證規則
- 標題不能為空
- 標題長度不超過200字元
- 內容不能為空
- 內容長度不超過2000字元

### 2.4 操作流程
1. **新增流程**：列表頁面點擊「新增」→ 進入新增頁面 → 填寫表單 → 儲存 → 返回列表頁面
2. **編輯流程**：列表頁面點擊「查看」→ 進入編輯頁面 → 修改表單 → 儲存 → 返回列表頁面

## Part 3 - 資料服務層 (NoteService)

### 3.1 服務類別設計
```csharp
public class NoteService
{
    private readonly string _dataFilePath;
    
    public NoteService()
    {
        _dataFilePath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data", "notes.json");
    }
    
    // 主要方法
    public List<Note> GetAllNotes()                          // 取得所有備忘錄
    public Note GetNoteById(int id)                          // 根據ID取得備忘錄
    public List<Note> GetNotesPaged(int page, int pageSize)  // 分頁取得備忘錄
    public int GetTotalCount()                               // 取得總數量
    public int AddNote(Note note)                            // 新增備忘錄
    public bool UpdateNote(Note note)                        // 更新備忘錄
    public bool DeleteNote(int id)                           // 刪除備忘錄
    private void SaveNotes(List<Note> notes)                 // 儲存到JSON檔案
    private List<Note> LoadNotes()                           // 從JSON檔案載入
}
```

### 3.2 JSON 檔案格式
```json
[
  {
    "Id": 1,
    "Title": "範例標題",
    "Content": "範例內容",
    "CreatedDate": "2025-01-27T10:00:00",
    "ModifiedDate": "2025-01-27T10:00:00"
  }
]
```

## Part 4 - 技術規範

### 4.1 開發環境
- **框架**: ASP.NET Core 8.0
- **UI框架**: Bootstrap 5
- **前端**: Razor Pages + JavaScript
- **資料儲存**: JSON 檔案

### 4.2 程式碼規範
- 使用 C# 命名慣例
- 所有公開方法需要 XML 文件註解
- 錯誤處理使用 try-catch 包裝
- 使用 async/await 處理 I/O 操作

### 4.3 安全性考量
- 輸入驗證和 XSS 防護
- 檔案路徑驗證
- CSRF 保護

### 4.4 效能考量
- JSON 檔案大小限制（建議不超過10MB）
- 分頁載入避免一次載入大量資料
- 適當的快取策略

## Part 5 - 測試計畫

### 5.1 功能測試項目
- [ ] 備忘錄列表正確顯示
- [ ] 分頁功能正常運作
- [ ] 新增備忘錄功能
- [ ] 編輯備忘錄功能
- [ ] 刪除備忘錄功能
- [ ] 表單驗證功能
- [ ] 響應式設計測試

### 5.2 錯誤情境測試
- [ ] JSON 檔案不存在
- [ ] JSON 檔案格式錯誤
- [ ] 磁碟空間不足
- [ ] 無效的備忘錄ID
- [ ] 超長文字輸入

## Part 6 - 部署說明

### 6.1 檔案部署
- 確保 App_Data 資料夾存在且有寫入權限
- JSON 檔案會自動建立

### 6.2 設定檔
- 無需特殊設定，使用預設 ASP.NET Core 設定即可
