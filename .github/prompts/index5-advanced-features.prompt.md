# Index5 備忘錄編輯器 - 進階功能開發規格書 (JSON 輕量版)

## 📋 **專案概述**

基於現有的 `index5.cshtml` 備忘錄編輯器，針對個人使用的超輕量級網站，使用 JSON 檔案儲存，增強以下核心功能：
- ✅ 自動儲存草稿機制 (適合 JSON)
- ✅ Markdown 富文本編輯器 (適合 JSON)
- ❌ 版本控制與編輯歷史 (JSON 不適合，移除)
- ✅ 標籤分類管理系統 (簡化版，適合 JSON)
- ❌ 附件檔案上傳功能 (JSON 不適合大檔案，移除)

**設計原則**: 
- 使用 JSON 檔案作為唯一儲存方式
- 專注於個人使用體驗
- 保持輕量化架構
- 避免複雜的資料關聯

---

## 🎯 **功能規格詳述**

### 1. 自動儲存功能 (Auto-Save) ✅

#### 1.1 需求分析
- **目標**: 防止使用者意外遺失編輯內容
- **觸發條件**: 
  - 每 30 秒自動儲存一次
  - 使用者停止輸入後 5 秒觸發
  - 離開頁面前自動儲存
- **儲存範圍**: 標題、內容、標籤
- **儲存位置**: `App_Data/draft-memo.json`

#### 1.2 技術實作
```csharp
// JSON 檔案操作服務
public class JsonAutoSaveService
{
    private readonly string _draftFilePath = "App_Data/draft-memo.json";
    
    public async Task<bool> SaveDraftAsync(MemoViewModel draft);
    public async Task<MemoViewModel> GetDraftAsync();
    public async Task<bool> DeleteDraftAsync();
    private async Task<string> ReadJsonFileAsync(string filePath);
    private async Task WriteJsonFileAsync(string filePath, object data);
}
```

#### 1.3 前端實作
- **JavaScript 定時器**: `setInterval` 實作定期儲存
- **防抖機制**: 避免頻繁 API 呼叫
- **視覺回饋**: 儲存狀態指示器 (已儲存/儲存中/儲存失敗)
- **本地儲存**: 使用 `localStorage` 作為備援

#### 1.4 JSON 資料結構
```json
{
  "draftId": "guid-string",
  "memoId": "int|null",
  "title": "string",
  "content": "string", 
  "contentType": "markdown|plain",
  "tags": ["tag1", "tag2"],
  "lastSaved": "2025-08-28T10:30:00Z",
  "isTemp": true
}
```

---

### 2. Markdown 支援功能 ✅

#### 2.1 需求分析 (輕量化版本)
- **編輯器類型**: 簡單的雙欄式 (編輯/預覽)
- **支援語法**: 
  - 基本格式 (粗體、斜體、刪除線)
  - 標題層級 (H1-H6)
  - 清單 (有序/無序)
  - 連結 (不支援圖片，避免檔案管理複雜性)
  - 程式碼區塊
  - 引用
  - 分隔線

#### 2.2 技術選型 (輕量化)
- **前端編輯器**: 簡單的 textarea + 工具按鈕
- **渲染引擎**: Marked.js (輕量)
- **語法高亮**: Prism.js (核心版本)

#### 2.3 UI/UX 設計 (簡化版)
```html
<!-- 簡化工具列 -->
<div class="markdown-toolbar">
    <button data-action="bold" title="粗體 **text**">B</button>
    <button data-action="italic" title="斜體 *text*">I</button>
    <button data-action="heading" title="標題 # H1">H</button>
    <button data-action="link" title="連結 [text](url)">🔗</button>
    <button data-action="code" title="程式碼 `code`">{ }</button>
    <button data-action="list" title="清單 - item">•</button>
    <button data-action="quote" title="引用 > text">❝</button>
    <button data-action="preview" title="切換預覽">👁</button>
</div>

<!-- 編輯區域 -->
<div class="markdown-container">
    <textarea id="markdown-editor" class="markdown-input"></textarea>
    <div id="markdown-preview" class="markdown-preview hidden"></div>
</div>
```

#### 2.4 JSON 儲存處理
- **儲存格式**: Markdown 原始碼存於 memo 的 content 欄位
- **內容類型**: 在 memo JSON 中增加 `contentType: "markdown"` 欄位
- **前端渲染**: 客戶端即時渲染，無需後端 API

---

### 3. 標籤系統功能 ✅ (簡化版)

#### 3.1 需求分析 (JSON 適配)
- **標籤建立**: 動態新增標籤
- **標籤管理**: 簡單的增刪改
- **標籤分類**: 單層結構 (避免複雜巢狀)
- **顏色標記**: 預設顏色集合選擇
- **使用統計**: 簡單的使用次數統計

#### 3.2 JSON 資料結構
```json
// App_Data/tags.json
{
  "tags": [
    {
      "id": 1,
      "name": "工作",
      "color": "#007bff",
      "description": "工作相關備忘錄",
      "usageCount": 5,
      "createdAt": "2025-08-28T10:30:00Z",
      "isActive": true
    },
    {
      "id": 2,
      "name": "個人",
      "color": "#28a745",
      "description": "個人生活記錄",
      "usageCount": 3,
      "createdAt": "2025-08-28T11:00:00Z",
      "isActive": true
    }
  ]
}

// memo-notes.json 中的標籤關聯 (現有結構擴充)
{
  "notes": [
    {
      "id": 1,
      "title": "會議記錄",
      "content": "...",
      "contentType": "markdown",
      "tags": [1, 2], // 標籤 ID 陣列
      "createdDate": "2025-08-28T10:30:00Z",
      "modifiedDate": "2025-08-28T12:00:00Z"
    }
  ]
}
```

#### 3.3 API 實作 (簡化)
```csharp
public class JsonTagService
{
    private readonly string _tagsFilePath = "App_Data/tags.json";
    
    public async Task<List<Tag>> GetAllTagsAsync();
    public async Task<Tag> CreateTagAsync(TagCreateModel model);
    public async Task<bool> UpdateTagAsync(int tagId, TagUpdateModel model);
    public async Task<bool> DeleteTagAsync(int tagId);
    public async Task<bool> IncrementUsageAsync(int tagId);
}
```

#### 3.4 UI 元件 (輕量化)
```html
<!-- 標籤選擇器 -->
<div class="tag-selector">
    <input type="text" class="tag-input" placeholder="輸入新標籤或選擇現有標籤" list="tag-suggestions">
    <datalist id="tag-suggestions">
        <option value="工作">工作</option>
        <option value="個人">個人</option>
    </datalist>
    
    <div class="selected-tags">
        <span class="tag-chip" data-tag-id="1" style="background-color: #007bff;">
            工作 <button class="tag-remove" type="button">×</button>
        </span>
    </div>
</div>

<!-- 標籤管理面板 -->
<div class="tag-management-simple">
    <div class="tag-list">
        <div class="tag-item">
            <span class="tag-color" style="background-color: #007bff;"></span>
            <span class="tag-name">工作</span>
            <span class="tag-usage">(5)</span>
            <button class="btn-edit">編輯</button>
            <button class="btn-delete">刪除</button>
        </div>
    </div>
</div>
```

---

### ❌ 移除的功能說明

#### 版本控制功能 (不適合 JSON)
**移除原因**: 
- JSON 檔案不適合儲存大量歷史版本資料
- 版本比較功能需要複雜的資料結構
- 個人使用情境下，簡單的自動儲存已足夠

**替代方案**: 
- 使用自動儲存的草稿功能
- 手動備份重要檔案 (匯出功能)

#### 附件檔案上傳功能 (不適合 JSON)
**移除原因**:
- JSON 不適合儲存二進位檔案資料
- 檔案管理會增加系統複雜度
- 個人使用可透過外部連結方式處理

**替代方案**:
- 支援 Markdown 連結語法 `[檔案名稱](檔案路徑)`
- 可連結到本地檔案或雲端儲存連結

---

## 🏗️ **輕量化架構設計**

### 前端架構 (JSON 版本)
```
index5.cshtml
├── Components/
│   ├── AutoSaveIndicator/
│   ├── MarkdownEditor/
│   └── TagSelector/
├── Scripts/
│   ├── auto-save.js
│   ├── markdown-editor.js
│   └── tag-manager.js
└── Styles/
    ├── markdown-editor.css
    └── tag-system.css
```

### 後端架構 (JSON 服務)
```csharp
Services/
├── JsonAutoSaveService.cs
├── JsonTagService.cs
└── JsonFileService.cs (共用基礎服務)

Controllers/
├── AutoSaveController.cs
└── TagController.cs

Models/
├── MemoViewModel.cs (擴充)
├── Tag.cs
├── DraftMemo.cs
└── JsonResponse.cs
```

### JSON 檔案結構
```
App_Data/
├── memo-notes.json          (現有，擴充標籤支援)
├── tags.json               (新增，標籤管理)
├── draft-memo.json         (新增，草稿儲存)
└── system-config.json      (新增，系統設定)
```

---

## 📊 **JSON 檔案設計**

### 現有檔案擴充
```json
// memo-notes.json (擴充現有結構)
{
  "notes": [
    {
      "id": 1,
      "title": "會議記錄",
      "content": "## 今日會議重點\n- 討論專案進度\n- 確認下週目標",
      "contentType": "markdown", // 新增欄位
      "tags": [1, 2],            // 新增欄位，標籤 ID 陣列
      "createdDate": "2025-08-28T10:30:00Z",
      "modifiedDate": "2025-08-28T12:00:00Z"
    }
  ]
}
```

### 新增檔案
```json
// tags.json (標籤管理)
{
  "tags": [
    {
      "id": 1,
      "name": "工作",
      "color": "#007bff",
      "description": "工作相關備忘錄",
      "usageCount": 5,
      "createdAt": "2025-08-28T10:30:00Z",
      "isActive": true
    }
  ],
  "nextId": 2
}

// draft-memo.json (草稿儲存)
{
  "draftId": "temp-123456",
  "memoId": null,
  "title": "未完成的備忘錄",
  "content": "這是一個草稿內容...",
  "contentType": "markdown",
  "tags": [1],
  "lastSaved": "2025-08-28T14:30:00Z",
  "isTemp": true
}

// system-config.json (系統設定)
{
  "autoSave": {
    "enabled": true,
    "intervalSeconds": 30,
    "delayAfterStop": 5
  },
  "markdown": {
    "enabled": true,
    "defaultMode": "edit",
    "theme": "default"
  },
  "tags": {
    "maxTagsPerMemo": 10,
    "defaultColors": ["#007bff", "#28a745", "#dc3545", "#ffc107", "#6c757d"]
  }
}
```

---

## 🚀 **開發階段規劃 (輕量化版本)**

### Phase 1: 基礎功能 (1-2 週)
1. **自動儲存功能**
   - 建立 `JsonAutoSaveService`
   - 實作基本草稿儲存到 `draft-memo.json`
   - 前端定時器機制
   - 視覺狀態指示器
   - localStorage 備援機制

### Phase 2: 標籤系統 (1 週)
2. **標籤系統實作**
   - 建立 `JsonTagService`
   - 實作 `tags.json` 管理
   - 擴充現有 `memo-notes.json` 結構
   - 標籤選擇器 UI 元件
   - 標籤管理介面

### Phase 3: Markdown 編輯器 (1-2 週)
3. **Markdown 支援**
   - 整合 Marked.js 渲染引擎
   - 實作簡化版編輯器工具列
   - 編輯/預覽模式切換
   - 快捷鍵支援
   - 語法高亮 (Prism.js)

### Phase 4: 整合與優化 (1 週)
4. **系統整合**
   - 功能整合測試
   - UI/UX 調整
   - 效能優化
   - 錯誤處理完善

---

## 🧪 **測試策略 (輕量化)**

### 功能測試
- **自動儲存**: 定時器觸發、防抖機制、錯誤處理
- **標籤管理**: CRUD 操作、JSON 檔案完整性
- **Markdown**: 渲染正確性、XSS 防護
- **JSON 檔案**: 資料完整性、檔案鎖定處理

### 整合測試
- **檔案操作**: 並發讀寫安全性
- **前後端整合**: API 回應正確性
- **瀏覽器相容性**: 基本功能在主流瀏覽器運作

### 效能測試
- **JSON 檔案大小**: 測試大量備忘錄載入
- **自動儲存頻率**: 避免過度 I/O 操作
- **前端渲染**: Markdown 渲染效能

---

## 📋 **驗收標準 (輕量化版本)**

### 功能驗收
- [ ] 自動儲存每 30 秒正常運作，草稿存於 `draft-memo.json`
- [ ] Markdown 編輯器支援基本語法（標題、粗體、斜體、清單、連結、程式碼）
- [ ] 標籤系統支援增刪改查，資料存於 `tags.json`
- [ ] 標籤與備忘錄正確關聯，存於擴充的 `memo-notes.json`
- [ ] 所有 JSON 檔案操作具備錯誤處理機制

### 效能驗收
- [ ] 頁面載入時間 < 3 秒
- [ ] 自動儲存回應時間 < 1 秒
- [ ] Markdown 即時預覽延遲 < 500ms
- [ ] JSON 檔案讀寫操作 < 200ms

### 相容性驗收
- [ ] 支援 Chrome、Firefox、Safari、Edge
- [ ] 響應式設計適配行動裝置
- [ ] 支援基本鍵盤快捷鍵 (Ctrl+S 儲存、Ctrl+P 預覽等)

---

## 📚 **技術文件 (簡化版)**

### API 文件
- JSON 檔案操作 API 說明
- 錯誤回應格式定義
- 前端 JavaScript API 參考

### 開發者指南
- JSON 檔案結構說明
- 程式碼規範 (C# + JavaScript)
- 本地開發環境設定

### 使用者手冊
- Markdown 語法快速指南
- 標籤使用說明
- 自動儲存機制說明

---

## 🔧 **維護與監控 (輕量化)**

### 檔案管理
- JSON 檔案備份策略 (定期複製到備份資料夾)
- 檔案大小監控 (避免單一 JSON 檔案過大)
- 檔案完整性檢查 (JSON 格式驗證)

### 錯誤處理
- JSON 解析錯誤處理
- 檔案鎖定衝突處理
- 磁碟空間不足處理

### 效能監控
- JSON 檔案讀寫時間記錄
- 記憶體使用量監控
- 前端渲染效能追蹤

---

## 📝 **備註 (JSON 輕量版本)**

1. **JSON 檔案限制**:
   - 單一 JSON 檔案建議不超過 10MB
   - 併發讀寫需要檔案鎖定機制
   - 大量資料時考慮分割檔案策略

2. **安全性考量**:
   - Markdown 內容的 XSS 防護
   - JSON 檔案的存取權限控制
   - 輸入驗證與清理

3. **使用者體驗**:
   - 響應式設計適配行動裝置
   - 離線功能 (使用 localStorage)
   - 鍵盤快捷鍵支援

4. **效能最佳化**:
   - JSON 檔案快取機制
   - 延遲載入大型內容
   - 前端渲染最佳化

5. **未來擴展性**:
   - 保留資料匯出功能 (JSON/CSV/Markdown)
   - 考慮雲端同步整合
   - 支援批次操作功能

---

## 💡 **適合 JSON 的功能總結**

### ✅ 保留的功能 (適合 JSON)
- **自動儲存**: 簡單的草稿機制
- **Markdown 支援**: 純文本儲存，前端渲染
- **標籤系統**: 簡化版本，單層結構

### ❌ 移除的功能 (不適合 JSON)
- **版本控制**: 資料量大，結構複雜
- **檔案附件**: 二進位檔案處理

### 🔄 **簡化的替代方案**
- **版本控制** → **自動備份** + **匯出功能**
- **檔案附件** → **連結引用** + **外部儲存**

---

*此開發規格書專為使用 JSON 檔案的超輕量級個人備忘錄系統設計，確保所有功能都適合檔案儲存架構。*
