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

### 3. 標籤系統功能 ✅ (與現有系統整合)

#### 3.1 需求分析 (基於現有 index4 標籤系統)
- **基礎標籤操作**: 沿用 `index4` 現有的標籤 CRUD 功能
- **標籤選擇**: 在 `index5` 編輯頁面新增標籤選擇器
- **標籤顯示**: 顯示已分配給該備忘錄的標籤
- **標籤建議**: 使用現有的 `OnGetTagSuggestionsAsync` API
- **資料一致性**: 確保兩頁使用相同的標籤資料來源

#### 3.2 現有資料結構 (無需變更)
```json
// App_Data/tags.json (現有結構)
{
  "tags": [
    {
      "id": 1,
      "name": "工作",
      "color": "#007bff", 
      "description": "工作相關備忘錄",
      "usageCount": 5,
      "createdDate": "2025-08-28T10:30:00Z"
    }
  ]
}

// memo-notes.json (現有結構，Note 類別已包含 Tags)
{
  "notes": [
    {
      "id": 1,
      "title": "會議記錄",
      "content": "...",
      "tags": [
        {
          "id": 1,
          "name": "工作",
          "color": "#007bff",
          "description": "工作相關備忘錄",
          "createdDate": "2025-08-28T10:30:00Z",
          "usageCount": 5
        }
      ],
      "createdDate": "2025-08-28T10:30:00Z",
      "modifiedDate": "2025-08-28T12:00:00Z"
    }
  ]
}
```

#### 3.3 需要擴充的部分 (index5 專用)

**A. NoteEditViewModel 擴充**
```csharp
// 需擴充現有的 NoteEditViewModel
public class NoteEditViewModel
{
    // ... 現有屬性 ...
    
    /// <summary>
    /// 選中的標籤 ID 清單
    /// </summary>
    public List<int> SelectedTagIds { get; set; } = new();
    
    /// <summary>
    /// 可用的標籤清單 (for UI 顯示)
    /// </summary>
    public List<Tag> AvailableTags { get; set; } = new();
}
```

**B. index5 後端方法擴充**
```csharp
// 在 index5.cshtml.cs 中新增
public List<Tag> AllTags { get; set; } = new();

// OnGetAsync 方法中載入標籤
AllTags = await _noteService.GetAllTagsAsync(); // 使用現有 API

// 編輯模式時載入備忘錄的標籤
ViewModel.SelectedTagIds = note.Tags.Select(t => t.Id).ToList();
ViewModel.AvailableTags = AllTags;

// 儲存時處理標籤關聯
// 使用現有的 AddTagToNoteAsync 和 RemoveTagFromNoteAsync
```

#### 3.4 UI 元件 (輕量化標籤選擇器)
```html
<!-- 標籤選擇區域 (新增到 index5.cshtml) -->
<div class="mb-4">
    <label class="form-label">標籤</label>
    
    <!-- 標籤輸入和建議 -->
    <div class="tag-input-container">
        <input type="text" 
               class="form-control" 
               id="tagInput" 
               placeholder="輸入標籤名稱或選擇現有標籤..." 
               list="tagSuggestions" 
               autocomplete="off">
        
        <datalist id="tagSuggestions">
            @foreach (var tag in Model.AllTags)
            {
                <option value="@tag.Name" data-tag-id="@tag.Id" data-color="@tag.Color">
            }
        </datalist>
        
        <button type="button" class="btn btn-outline-primary btn-sm ms-2" onclick="addSelectedTag()">
            新增標籤
        </button>
    </div>
    
    <!-- 已選擇的標籤 -->
    <div class="selected-tags mt-3" id="selectedTags">
        @if (Model.ViewModel.IsEditMode)
        {
            @foreach (var tagId in Model.ViewModel.SelectedTagIds)
            {
                var tag = Model.AllTags.FirstOrDefault(t => t.Id == tagId);
                if (tag != null)
                {
                    <span class="badge tag-badge me-2 mb-2" 
                          style="background-color: @tag.Color" 
                          data-tag-id="@tag.Id">
                        @tag.Name
                        <button type="button" 
                                class="btn-close btn-close-white ms-2" 
                                onclick="removeTag(@tag.Id)"></button>
                        <input type="hidden" name="ViewModel.SelectedTagIds" value="@tag.Id" />
                    </span>
                }
            }
        }
    </div>
</div>
```

#### 3.5 JavaScript 整合 (複用 index4 邏輯)
```javascript
// 標籤相關函式 (簡化版，沿用 index4 概念)
function addSelectedTag() {
    const input = document.getElementById('tagInput');
    const tagName = input.value.trim();
    
    if (!tagName) return;
    
    // 檢查是否為現有標籤
    const existingTag = findTagByName(tagName);
    if (existingTag) {
        addTagToSelected(existingTag);
    } else {
        // 建立新標籤 (使用現有 API)
        createNewTag(tagName, '#007bff');
    }
    
    input.value = '';
}

function createNewTag(tagName, tagColor) {
    // 使用 index4 現有的 CreateTag API
    fetch('/index4?handler=CreateTag', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': getToken()
        },
        body: `tagName=${encodeURIComponent(tagName)}&tagColor=${encodeURIComponent(tagColor)}`
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            const newTag = {
                id: data.tagId,
                name: data.tagName,
                color: data.tagColor
            };
            addTagToSelected(newTag);
            updateTagSuggestions(newTag);
        }
    });
}

function removeTag(tagId) {
    const tagElement = document.querySelector(`[data-tag-id="${tagId}"]`);
    if (tagElement) {
        tagElement.remove();
    }
}
```

#### 3.6 與現有系統的整合點

**A. 共用服務層**
- 使用現有的 `IEnhancedMemoNoteService.GetAllTagsAsync()`
- 使用現有的 `IEnhancedMemoNoteService.CreateTagAsync()`
- 使用現有的標籤關聯方法

**B. 資料一致性**
- 兩頁面使用相同的 `tags.json` 檔案
- 標籤的 `UsageCount` 在兩頁面都會正確更新
- 標籤的 CRUD 操作在兩頁面都保持同步

**C. UI 風格一致性**
- 沿用 `index4` 的標籤顏色系統
- 使用相同的 Bootstrap 樣式類別
- 保持標籤顯示格式一致

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

## 🏗️ **輕量化架構設計 (整合版本)**

### 前端架構 (基於現有系統擴充)
```
index5.cshtml (擴充現有檔案)
├── Components/ (新增)
│   ├── AutoSaveIndicator/
│   ├── MarkdownEditor/
│   └── TagSelector/ (整合 index4 標籤邏輯)
├── Scripts/ (新增)
│   ├── auto-save.js
│   ├── markdown-editor.js
│   └── tag-integration.js (橋接兩頁標籤功能)
└── Styles/ (新增)
    ├── markdown-editor.css
    └── tag-integration.css
```

### 後端架構 (最小擴充)
```csharp
Services/
├── NoteService.cs (現有，已包含標籤功能)
├── JsonAutoSaveService.cs (新增)
└── JsonFileService.cs (新增，共用基礎服務)

Controllers/ (無需新增控制器)
// 使用現有的 index4 和 index5 頁面模型

Models/ (最小擴充)
├── NoteEditViewModel.cs (擴充現有，增加標籤支援)
├── DraftMemo.cs (新增)
└── SystemConfig.cs (新增)
```

### 檔案結構 (最小變動)
```
App_Data/
├── memo-notes.json        (現有，無需變更)
├── tags.json              (現有，由 index4 建立)
├── categories.json        (現有，無需變更)
├── draft-memo.json        (新增，草稿儲存)
└── system-config.json     (新增，系統設定)
```

---

## 📊 **JSON 檔案設計 (與現有系統整合)**

### 現有檔案 (無需變更)
```json
// App_Data/tags.json (現有檔案，index4 已建立)
// 此檔案由現有的 NoteService 管理，無需修改

// App_Data/memo-notes.json (現有結構)  
// Note 類別已包含 Tags 屬性，無需修改
```

### 新增檔案 (僅新增功能需要)
```json
// App_Data/draft-memo.json (草稿儲存)
{
  "draftId": "temp-123456",
  "memoId": null,
  "title": "未完成的備忘錄",
  "content": "這是一個草稿內容...",
  "contentType": "markdown",
  "selectedTagIds": [1, 3], // 與現有標籤系統相容
  "lastSaved": "2025-08-28T14:30:00Z",
  "isTemp": true
}

// App_Data/system-config.json (系統設定)
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
    "enableAutoSuggestion": true
  }
}
```

---

## 🚀 **開發階段規劃 (整合版本)**

### Phase 1: 基礎整合 (1 週)
1. **NoteEditViewModel 標籤支援**
   - 擴充現有的 `NoteEditViewModel` 增加標籤屬性
   - 在 `index5.cshtml.cs` 載入標籤資料
   - 確保與 `index4` 標籤系統完全相容

2. **自動儲存基礎功能**
   - 建立 `JsonAutoSaveService` (不與標籤系統衝突)
   - 實作基本草稿儲存到 `draft-memo.json`
   - 前端定時器機制，避免與現有表單衝突

### Phase 2: UI 整合 (1 週)  
3. **index5 標籤選擇器**
   - 在 `index5.cshtml` 新增標籤選擇元件
   - 複用 `index4` 的標籤建議 API (`OnGetTagSuggestionsAsync`)
   - 整合標籤建立功能 (使用 `index4` 的 `OnPostCreateTagAsync`)
   - 確保標籤 UI 風格與 `index4` 一致

4. **資料流整合測試**
   - 測試標籤在兩頁面間的資料一致性
   - 確認標籤的 `UsageCount` 正確更新
   - 驗證草稿功能不會干擾正式儲存

### Phase 3: Markdown 功能 (1-2 週)
5. **Markdown 編輯器**
   - 整合 Marked.js 渲染引擎
   - 在現有的 `textarea` 基礎上新增 Markdown 支援
   - 實作簡化版編輯器工具列 (不影響現有字元計數功能)
   - 編輯/預覽模式切換

6. **自動儲存完整功能**
   - 整合標籤資料到自動儲存
   - Markdown 內容的草稿儲存
   - 視覺狀態指示器
   - localStorage 備援機制

### Phase 4: 最終整合 (1 週)
7. **系統整合與測試**
   - 功能整合測試 (特別關注標籤系統相容性)
   - 與 `index4` 列表頁面的互動測試  
   - UI/UX 統一性調整
   - 效能優化和錯誤處理完善

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

## 💡 **適合 JSON 的功能總結 (整合版)**

### ✅ 保留的功能 (與現有系統整合)
- **自動儲存**: 簡單的草稿機制，不干擾現有儲存邏輯
- **Markdown 支援**: 純文本儲存，前端渲染，相容現有內容
- **標籤系統**: 完全整合 `index4` 現有標籤功能，確保資料一致性

### ❌ 移除的功能 (不適合 JSON 或已有替代)
- **版本控制**: 資料量大，結構複雜，個人使用不必要
- **檔案附件**: 二進位檔案處理，JSON 不適合

### 🔄 **整合策略**

**標籤系統整合**:
- `index4`: 標籤管理中心 (建立、編輯、刪除、批次操作)
- `index5`: 標籤選擇和分配 (選擇現有標籤、快速建立新標籤)
- **共用**: 相同的資料服務、API 端點、檔案格式

**資料流向**:
```
index4 (列表頁) ←→ tags.json ←→ index5 (編輯頁)
     ↓                               ↓
memo-notes.json ←←←←←←←←←←←←←←←←←←→→→→→→
     ↓
index4 (顯示標籤) ←←←←←←← index5 (儲存標籤關聯)
```

**相容性保證**:
- 使用現有的 `IEnhancedMemoNoteService` 介面
- 沿用現有的標籤資料結構 (`Tag` 類別)
- 保持標籤顏色系統一致性
- 確保 `UsageCount` 在兩頁面正確同步

### 📋 **核心原則**

1. **最小侵入性**: 盡量不修改現有 `index4` 程式碼
2. **資料一致性**: 兩頁面使用相同的資料來源和服務
3. **功能互補性**: `index4` 專注管理，`index5` 專注使用
4. **向下相容**: 所有變更都不能破壞現有功能

---

*此開發規格書專為與現有 index4 標籤系統完全整合的超輕量級個人備忘錄系統設計，確保兩頁面功能互通且資料一致。*
