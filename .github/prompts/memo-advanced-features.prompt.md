# 備忘錄系統進階功能開發規格書

## 專案概述
**專案名稱**: 備忘錄系統進階功能擴展  
**基礎版本**: index4 + index5 備忘錄系統  
**開發階段**: Phase 2 - 功能增強  
**預計完成**: 2025年Q2  
**技術架構**: ASP.NET Core 8.0 + Razor Pages + Bootstrap 5 + Enhanced Services

---

## 功能需求總覽

### 🎯 **核心擴展功能**
1. **🔍 搜尋和篩選系統** - 全文檢索、標籤篩選、日期範圍篩選
2. **⚡ 批次操作功能** - 批次刪除、批次移動、批次標籤管理
3. **🏷️ 標籤和分類系統** - 多層級分類、智慧標籤、顏色管理
4. **📊 匯出功能** - PDF報告、Excel試算表、JSON備份

### 📋 **優先級排序**
| 優先級 | 功能模組 | 預估工時 | 重要性 |
|-------|---------|---------|--------|
| P0 | 搜尋和篩選 | 40小時 | 極高 |
| P1 | 標籤和分類 | 32小時 | 高 |
| P2 | 批次操作 | 24小時 | 中高 |
| P3 | 匯出功能 | 16小時 | 中 |

---

## Feature 1: 搜尋和篩選系統

### 📋 **功能規格**
#### 1.1 全文檢索搜尋
- **搜尋範圍**: 標題、內容、標籤
- **搜尋方式**: 即時搜尋（300ms防抖）
- **關鍵字高亮**: 搜尋結果中關鍵字螢光標記
- **搜尋歷史**: 保存最近10次搜尋記錄

#### 1.2 進階篩選器
```csharp
public class SearchFilterModel
{
    public string? Keyword { get; set; }              // 關鍵字
    public List<string> Tags { get; set; } = new();  // 標籤篩選
    public DateTime? StartDate { get; set; }          // 開始日期
    public DateTime? EndDate { get; set; }            // 結束日期
    public SortBy SortBy { get; set; }               // 排序方式
    public SortOrder SortOrder { get; set; }         // 排序順序
}

public enum SortBy
{
    CreatedDate,    // 建立日期
    ModifiedDate,   // 修改日期
    Title,          // 標題
    Relevance      // 相關性
}
```

### 🎨 **使用者介面設計**
#### 搜尋列設計
```html
<div class="search-container">
    <div class="input-group input-group-lg">
        <span class="input-group-text">
            <i class="fas fa-search"></i>
        </span>
        <input type="text" class="form-control" 
               placeholder="搜尋備忘錄標題、內容或標籤..." 
               id="searchInput" />
        <button class="btn btn-outline-secondary" type="button" 
                data-bs-toggle="collapse" data-bs-target="#advancedFilters">
            <i class="fas fa-filter"></i> 進階篩選
        </button>
    </div>
</div>
```

#### 進階篩選面板
- **摺疊式設計**: 預設隱藏，點擊展開
- **標籤選擇器**: 多選核取方塊 + 標籤顏色顯示
- **日期範圍選擇**: DatePicker 元件
- **排序選項**: 下拉選單選擇

### 🔧 **技術實作**
#### 後端搜尋邏輯
```csharp
public async Task<List<Note>> SearchNotesAsync(SearchFilterModel filter, int page, int pageSize)
{
    var query = await LoadNotesAsync();
    
    // 關鍵字搜尋
    if (!string.IsNullOrWhiteSpace(filter.Keyword))
    {
        query = query.Where(n => 
            n.Title.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) ||
            n.Content.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase) ||
            n.Tags.Any(t => t.Name.Contains(filter.Keyword, StringComparison.OrdinalIgnoreCase))
        ).ToList();
    }
    
    // 標籤篩選
    if (filter.Tags.Any())
    {
        query = query.Where(n => 
            n.Tags.Any(t => filter.Tags.Contains(t.Name))
        ).ToList();
    }
    
    // 日期範圍篩選
    if (filter.StartDate.HasValue)
    {
        query = query.Where(n => n.CreatedDate >= filter.StartDate.Value).ToList();
    }
    
    // 排序
    query = filter.SortBy switch
    {
        SortBy.CreatedDate => filter.SortOrder == SortOrder.Desc 
            ? query.OrderByDescending(n => n.CreatedDate).ToList()
            : query.OrderBy(n => n.CreatedDate).ToList(),
        SortBy.Title => filter.SortOrder == SortOrder.Desc 
            ? query.OrderByDescending(n => n.Title).ToList()
            : query.OrderBy(n => n.Title).ToList(),
        _ => query.OrderByDescending(n => n.CreatedDate).ToList()
    };
    
    return query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
}
```

#### 前端即時搜尋
```javascript
let searchTimeout;
const searchInput = document.getElementById('searchInput');

searchInput.addEventListener('input', function() {
    clearTimeout(searchTimeout);
    searchTimeout = setTimeout(() => {
        performSearch(this.value);
    }, 300);
});

function performSearch(keyword) {
    const formData = new FormData();
    formData.append('keyword', keyword);
    
    fetch('/index4?handler=Search', {
        method: 'POST',
        body: formData,
        headers: {
            'RequestVerificationToken': getAntiForgeryToken()
        }
    })
    .then(response => response.json())
    .then(data => updateSearchResults(data));
}
```

---

## Feature 2: 標籤和分類系統

### 📋 **資料模型設計**
```csharp
public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#007bff";    // 標籤顏色
    public string? Description { get; set; }           // 標籤描述
    public DateTime CreatedDate { get; set; }
    public int UsageCount { get; set; }               // 使用次數
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? ParentId { get; set; }                // 父分類ID
    public string Icon { get; set; } = "fas fa-folder"; // 分類圖示
    public List<Category> Children { get; set; } = new(); // 子分類
}

public class NoteTag
{
    public int NoteId { get; set; }
    public int TagId { get; set; }
    public Note Note { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}

// 擴展 Note 模型
public class Note
{
    // ... 現有屬性
    public int? CategoryId { get; set; }              // 分類ID
    public Category? Category { get; set; }           // 分類導航屬性
    public List<NoteTag> NoteTags { get; set; } = new(); // 標籤關聯
    public List<Tag> Tags => NoteTags.Select(nt => nt.Tag).ToList(); // 標籤快速存取
}
```

### 🎨 **標籤管理介面**
#### 標籤選擇器組件
```html
<div class="tag-selector">
    <div class="tag-input-container">
        <input type="text" class="form-control" id="tagInput" 
               placeholder="新增標籤..." />
        <div class="tag-suggestions"></div>
    </div>
    
    <div class="selected-tags">
        <!-- 動態產生的標籤 -->
    </div>
    
    <div class="popular-tags">
        <h6>熱門標籤</h6>
        <!-- 熱門標籤快速選擇 -->
    </div>
</div>
```

#### 標籤樣式設計
```css
.tag {
    display: inline-block;
    padding: 0.25rem 0.75rem;
    margin: 0.25rem;
    background-color: var(--tag-color);
    color: white;
    border-radius: 50px;
    font-size: 0.875rem;
    border: none;
    cursor: pointer;
    transition: all 0.3s ease;
}

.tag:hover {
    transform: scale(1.05);
    box-shadow: 0 2px 4px rgba(0,0,0,0.2);
}

.tag .remove-tag {
    margin-left: 0.5rem;
    cursor: pointer;
}
```

### 🔧 **標籤自動建議**
```javascript
class TagAutoComplete {
    constructor(inputElement, suggestionsElement) {
        this.input = inputElement;
        this.suggestions = suggestionsElement;
        this.allTags = [];
        this.init();
    }
    
    async loadTags() {
        const response = await fetch('/api/tags');
        this.allTags = await response.json();
    }
    
    showSuggestions(query) {
        const matches = this.allTags
            .filter(tag => tag.name.toLowerCase().includes(query.toLowerCase()))
            .slice(0, 5);
            
        this.suggestions.innerHTML = matches
            .map(tag => `
                <div class="suggestion-item" data-tag-id="${tag.id}">
                    <span class="tag" style="--tag-color: ${tag.color}">
                        ${tag.name}
                    </span>
                    <small class="text-muted">(${tag.usageCount} 次使用)</small>
                </div>
            `).join('');
    }
}
```

---

## Feature 3: 批次操作功能

### 📋 **功能規格**
#### 3.1 批次選擇機制
- **全選/取消全選**: 頁面頂部主控核取方塊
- **單項選擇**: 每列備忘錄前的核取方塊
- **範圍選擇**: Shift+點擊選擇範圍
- **選擇計數**: 即時顯示已選擇項目數量

#### 3.2 批次操作選單
```csharp
public enum BatchOperation
{
    Delete,           // 批次刪除
    AddTag,          // 批次新增標籤
    RemoveTag,       // 批次移除標籤
    ChangeCategory,  // 批次更改分類
    Export           // 批次匯出
}

public class BatchOperationRequest
{
    public List<int> NoteIds { get; set; } = new();
    public BatchOperation Operation { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
}
```

### 🎨 **批次操作介面**
#### 批次工具列
```html
<div class="batch-toolbar" id="batchToolbar" style="display: none;">
    <div class="d-flex justify-content-between align-items-center">
        <div>
            <span class="selected-count">已選擇 <strong id="selectedCount">0</strong> 項</span>
        </div>
        
        <div class="btn-group">
            <button class="btn btn-outline-primary" onclick="batchAddTag()">
                <i class="fas fa-tag"></i> 加標籤
            </button>
            <button class="btn btn-outline-info" onclick="batchChangeCategory()">
                <i class="fas fa-folder"></i> 更改分類
            </button>
            <button class="btn btn-outline-success" onclick="batchExport()">
                <i class="fas fa-download"></i> 匯出
            </button>
            <button class="btn btn-outline-danger" onclick="batchDelete()">
                <i class="fas fa-trash"></i> 刪除
            </button>
        </div>
        
        <button class="btn btn-outline-secondary" onclick="clearSelection()">
            取消選擇
        </button>
    </div>
</div>
```

### 🔧 **批次處理邏輯**
#### 後端批次處理
```csharp
public async Task<BatchOperationResult> ExecuteBatchOperationAsync(BatchOperationRequest request)
{
    var result = new BatchOperationResult();
    
    try
    {
        switch (request.Operation)
        {
            case BatchOperation.Delete:
                result = await BatchDeleteAsync(request.NoteIds);
                break;
                
            case BatchOperation.AddTag:
                var tagName = request.Parameters["tagName"].ToString();
                result = await BatchAddTagAsync(request.NoteIds, tagName);
                break;
                
            case BatchOperation.ChangeCategory:
                var categoryId = int.Parse(request.Parameters["categoryId"].ToString());
                result = await BatchChangeCategoryAsync(request.NoteIds, categoryId);
                break;
        }
    }
    catch (Exception ex)
    {
        result.Success = false;
        result.ErrorMessage = ex.Message;
        _logger.LogError(ex, "批次操作失敗: {Operation}", request.Operation);
    }
    
    return result;
}

private async Task<BatchOperationResult> BatchDeleteAsync(List<int> noteIds)
{
    var notes = await LoadNotesAsync();
    var notesToDelete = notes.Where(n => noteIds.Contains(n.Id)).ToList();
    
    foreach (var note in notesToDelete)
    {
        notes.Remove(note);
    }
    
    await SaveNotesAsync(notes);
    
    return new BatchOperationResult 
    { 
        Success = true, 
        ProcessedCount = notesToDelete.Count 
    };
}
```

#### 前端選擇管理
```javascript
class BatchSelector {
    constructor() {
        this.selectedItems = new Set();
        this.init();
    }
    
    init() {
        // 主控核取方塊
        document.getElementById('masterCheckbox').addEventListener('change', (e) => {
            this.toggleAll(e.target.checked);
        });
        
        // 個別核取方塊
        document.querySelectorAll('.item-checkbox').forEach(checkbox => {
            checkbox.addEventListener('change', (e) => {
                this.toggleItem(e.target.value, e.target.checked);
            });
        });
    }
    
    toggleItem(noteId, checked) {
        if (checked) {
            this.selectedItems.add(noteId);
        } else {
            this.selectedItems.delete(noteId);
        }
        
        this.updateUI();
    }
    
    updateUI() {
        const count = this.selectedItems.size;
        document.getElementById('selectedCount').textContent = count;
        document.getElementById('batchToolbar').style.display = count > 0 ? 'block' : 'none';
    }
}
```

---

## Feature 4: 匯出功能

### 📋 **匯出格式支援**
#### 4.1 PDF 報告匯出
- **版面配置**: A4 直向版面
- **內容結構**: 標題、建立日期、內容、標籤
- **樣式設計**: 專業報告樣式
- **批次匯出**: 多個備忘錄合併成一份 PDF

#### 4.2 Excel 試算表匯出
- **欄位對應**:
  ```
  Column A: ID
  Column B: 標題
  Column C: 內容
  Column D: 分類
  Column E: 標籤
  Column F: 建立日期
  Column G: 修改日期
  ```
- **格式化**: 日期格式、儲存格寬度自動調整
- **篩選功能**: 包含 Excel 篩選器

### 🔧 **技術實作**
#### PDF 匯出服務
```csharp
public interface IPdfExportService
{
    Task<byte[]> ExportNotesToPdfAsync(List<Note> notes, PdfExportOptions options);
}

public class PdfExportService : IPdfExportService
{
    public async Task<byte[]> ExportNotesToPdfAsync(List<Note> notes, PdfExportOptions options)
    {
        using var document = new PdfDocument();
        var page = document.AddPage();
        var graphics = XGraphics.FromPdfPage(page);
        
        // PDF 生成邏輯
        var font = new XFont("Microsoft JhengHei", 12);
        var yPosition = 50;
        
        foreach (var note in notes)
        {
            // 標題
            graphics.DrawString(note.Title, font, XBrushes.Black, 
                new XRect(50, yPosition, page.Width - 100, 30), XStringFormats.TopLeft);
            yPosition += 40;
            
            // 內容
            var contentRect = new XRect(50, yPosition, page.Width - 100, 200);
            graphics.DrawString(note.Content, font, XBrushes.DarkGray, 
                contentRect, XStringFormats.TopLeft);
            
            yPosition += 220;
        }
        
        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }
}
```

#### Excel 匯出服務
```csharp
public class ExcelExportService
{
    public async Task<byte[]> ExportNotesToExcelAsync(List<Note> notes)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("備忘錄");
        
        // 標題列
        worksheet.Cells[1, 1].Value = "ID";
        worksheet.Cells[1, 2].Value = "標題";
        worksheet.Cells[1, 3].Value = "內容";
        worksheet.Cells[1, 4].Value = "分類";
        worksheet.Cells[1, 5].Value = "標籤";
        worksheet.Cells[1, 6].Value = "建立日期";
        worksheet.Cells[1, 7].Value = "修改日期";
        
        // 資料列
        for (int i = 0; i < notes.Count; i++)
        {
            var note = notes[i];
            var row = i + 2;
            
            worksheet.Cells[row, 1].Value = note.Id;
            worksheet.Cells[row, 2].Value = note.Title;
            worksheet.Cells[row, 3].Value = note.Content;
            worksheet.Cells[row, 4].Value = note.Category?.Name ?? "";
            worksheet.Cells[row, 5].Value = string.Join(", ", note.Tags.Select(t => t.Name));
            worksheet.Cells[row, 6].Value = note.CreatedDate;
            worksheet.Cells[row, 7].Value = note.ModifiedDate;
        }
        
        // 自動調整欄寬
        worksheet.Cells.AutoFitColumns();
        
        return package.GetAsByteArray();
    }
}
```

---

## 整合與架構

### 🏗️ **服務層擴展**
```csharp
public interface IEnhancedMemoNoteService : IMemoNoteService
{
    // 搜尋功能
    Task<List<Note>> SearchNotesAsync(SearchFilterModel filter, int page, int pageSize);
    Task<int> GetSearchResultCountAsync(SearchFilterModel filter);
    
    // 標籤管理
    Task<List<Tag>> GetAllTagsAsync();
    Task<Tag> CreateTagAsync(string name, string color);
    Task<bool> AddTagToNoteAsync(int noteId, int tagId);
    Task<bool> RemoveTagFromNoteAsync(int noteId, int tagId);
    
    // 分類管理
    Task<List<Category>> GetCategoriesAsync();
    Task<Category> CreateCategoryAsync(string name, int? parentId);
    
    // 批次操作
    Task<BatchOperationResult> ExecuteBatchOperationAsync(BatchOperationRequest request);
    
    // 匯出功能
    Task<byte[]> ExportToPdfAsync(List<int> noteIds);
    Task<byte[]> ExportToExcelAsync(List<int> noteIds);
}
```

### 📊 **資料庫遷移計畫**
由於功能複雜度增加，建議遷移到 SQL Server：

```sql
-- 標籤表
CREATE TABLE Tags (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(50) NOT NULL,
    Color NVARCHAR(7) DEFAULT '#007bff',
    Description NVARCHAR(200),
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    UsageCount INT DEFAULT 0
);

-- 分類表
CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300),
    ParentId INT REFERENCES Categories(Id),
    Icon NVARCHAR(50) DEFAULT 'fas fa-folder',
    CreatedDate DATETIME2 DEFAULT GETDATE()
);

-- 備忘錄標籤關聯表
CREATE TABLE NoteTags (
    NoteId INT NOT NULL,
    TagId INT NOT NULL,
    PRIMARY KEY (NoteId, TagId),
    FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
);
```

---

## 開發時程規劃

### 📅 **Phase 2.1: 搜尋功能 (4週)**
- **Week 1**: 後端搜尋邏輯開發
- **Week 2**: 前端搜尋介面實作
- **Week 3**: 進階篩選器開發
- **Week 4**: 效能優化與測試

### 📅 **Phase 2.2: 標籤系統 (4週)**
- **Week 1**: 資料模型設計與遷移
- **Week 2**: 標籤管理介面開發
- **Week 3**: 標籤選擇器組件
- **Week 4**: 自動建議與整合測試

### 📅 **Phase 2.3: 批次操作 (3週)**
- **Week 1**: 批次選擇機制
- **Week 2**: 批次操作邏輯
- **Week 3**: 使用者介面與測試

### 📅 **Phase 2.4: 匯出功能 (2週)**
- **Week 1**: PDF/Excel 匯出服務
- **Week 2**: 匯出介面與整合

---

## 測試策略

### 🧪 **測試範圍**
- **單元測試**: 所有新增服務方法
- **整合測試**: API 端點和資料庫操作
- **效能測試**: 搜尋和批次操作效能
- **使用者測試**: 介面易用性測試

### ✅ **品質檢查清單**
- [ ] 所有功能單元測試覆蓋率 > 90%
- [ ] 搜尋響應時間 < 500ms (1000筆資料)
- [ ] 批次操作支援最多 100 筆資料
- [ ] PDF 匯出支援中文字型
- [ ] Excel 匯出相容性測試
- [ ] 行動裝置適配測試
- [ ] 瀏覽器相容性測試 (Chrome, Firefox, Edge, Safari)

---

## 部署與維護

### 🚀 **部署需求**
- SQL Server 2019+ 或 PostgreSQL 12+
- .NET 8.0 運行環境
- 額外套件依賴:
  ```xml
  <PackageReference Include="EPPlus" Version="7.0.0" />
  <PackageReference Include="PdfSharp" Version="6.0.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
  ```

### 📊 **監控指標**
- 搜尋查詢效能
- 批次操作成功率
- 匯出檔案大小統計
- 標籤使用頻率分析

---

## 成功指標 (KPI)

### 📈 **使用者體驗指標**
- 搜尋功能使用率 > 60%
- 批次操作採用率 > 30%
- 標籤平均使用數量 > 2個/備忘錄
- 匯出功能使用率 > 20%

### ⚡ **效能指標**
- 搜尋回應時間 < 500ms
- 批次刪除處理時間 < 2s (100筆)
- PDF 匯出時間 < 5s (50筆)
- 頁面載入時間增幅 < 10%

---

**總結**: 此進階功能擴展將使備忘錄系統從基本的 CRUD 應用程式提升為功能完整的知識管理系統。通過搜尋篩選、標籤分類、批次操作和匯出功能，大幅提升使用者的生產力和系統的實用性。整個開發週期預計 13 週，建議採用敏捷開發方式，每個 Phase 結束後進行使用者回饋收集和功能調整。
