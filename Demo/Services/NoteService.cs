using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 標籤資料模型
/// </summary>
public class Tag
{
    /// <summary>
    /// 標籤 ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 標籤名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 標籤顏色
    /// </summary>
    public string Color { get; set; } = "#007bff";
    
    /// <summary>
    /// 標籤描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// 使用次數
    /// </summary>
    public int UsageCount { get; set; }
}

/// <summary>
/// 分類資料模型
/// </summary>
public class Category
{
    /// <summary>
    /// 分類 ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 分類名稱
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 分類描述
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// 父分類 ID
    /// </summary>
    public int? ParentId { get; set; }
    
    /// <summary>
    /// 分類圖示
    /// </summary>
    public string Icon { get; set; } = "fas fa-folder";
    
    /// <summary>
    /// 子分類
    /// </summary>
    public List<Category> Children { get; set; } = new();
}

/// <summary>
/// 排序方式
/// </summary>
public enum SortBy
{
    /// <summary>
    /// 建立日期
    /// </summary>
    CreatedDate,
    
    /// <summary>
    /// 修改日期
    /// </summary>
    ModifiedDate,
    
    /// <summary>
    /// 標題
    /// </summary>
    Title,
    
    /// <summary>
    /// 相關性
    /// </summary>
    Relevance
}

/// <summary>
/// 排序順序
/// </summary>
public enum SortOrder
{
    /// <summary>
    /// 升冪
    /// </summary>
    Asc,
    
    /// <summary>
    /// 降冪
    /// </summary>
    Desc
}

/// <summary>
/// 搜尋篩選模型
/// </summary>
public class SearchFilterModel
{
    /// <summary>
    /// 搜尋關鍵字
    /// </summary>
    public string? Keyword { get; set; }
    
    /// <summary>
    /// 標籤篩選
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// 開始日期
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 結束日期
    /// </summary>
    public DateTime? EndDate { get; set; }
    
    /// <summary>
    /// 排序方式
    /// </summary>
    public SortBy SortBy { get; set; } = SortBy.ModifiedDate;
    
    /// <summary>
    /// 排序順序
    /// </summary>
    public SortOrder SortOrder { get; set; } = SortOrder.Desc;
    
    /// <summary>
    /// 分類ID
    /// </summary>
    public int? CategoryId { get; set; }
}

/// <summary>
/// 批次操作類型
/// </summary>
public enum BatchOperation
{
    /// <summary>
    /// 刪除
    /// </summary>
    Delete,
    
    /// <summary>
    /// 新增標籤
    /// </summary>
    AddTag,
    
    /// <summary>
    /// 移除標籤
    /// </summary>
    RemoveTag,
    
    /// <summary>
    /// 更改分類
    /// </summary>
    ChangeCategory,
    
    /// <summary>
    /// 匯出
    /// </summary>
    Export
}

/// <summary>
/// 批次操作請求
/// </summary>
public class BatchOperationRequest
{
    /// <summary>
    /// 備忘錄 ID 清單
    /// </summary>
    public List<int> NoteIds { get; set; } = new();
    
    /// <summary>
    /// 操作類型
    /// </summary>
    public BatchOperation Operation { get; set; }
    
    /// <summary>
    /// 操作參數
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new();
}

/// <summary>
/// 批次操作結果
/// </summary>
public class BatchOperationResult
{
    /// <summary>
    /// 操作是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 處理筆數
    /// </summary>
    public int ProcessedCount { get; set; }
    
    /// <summary>
    /// 錯誤訊息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 備忘錄資料模型
/// </summary>
public class Note
{
    /// <summary>
    /// 唯一識別碼
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 內容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime ModifiedDate { get; set; }
    
    /// <summary>
    /// 分類ID
    /// </summary>
    public int? CategoryId { get; set; }
    
    /// <summary>
    /// 分類
    /// </summary>
    public Category? Category { get; set; }
    
    /// <summary>
    /// 標籤清單
    /// </summary>
    public List<Tag> Tags { get; set; } = new();
}

/// <summary>
/// 備忘錄列表檢視模型
/// </summary>
public class NoteListViewModel
{
    /// <summary>
    /// 當前頁面備忘錄清單
    /// </summary>
    public List<Note> Notes { get; set; } = new();

    /// <summary>
    /// 當前頁碼
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 總頁數
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// 每頁顯示數量
    /// </summary>
    public int PageSize { get; set; } = 20;

    /// <summary>
    /// 總備忘錄數量
    /// </summary>
    public int TotalCount { get; set; }
}

/// <summary>
/// 備忘錄編輯檢視模型
/// </summary>
public class NoteEditViewModel
{
    /// <summary>
    /// 備忘錄ID（0表示新增）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 標題
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 內容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 是否為編輯模式
    /// </summary>
    public bool IsEditMode { get; set; }

    /// <summary>
    /// 建立日期
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// 修改日期
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// 選中的標籤 ID 清單
    /// </summary>
    public List<int> SelectedTagIds { get; set; } = new();

    /// <summary>
    /// 可用的標籤清單 (for UI 顯示)
    /// </summary>
    public List<Tag> AvailableTags { get; set; } = new();

    /// <summary>
    /// 選中的分類 ID
    /// </summary>
    public int? CategoryId { get; set; }

    /// <summary>
    /// 可用的分類清單 (for UI 顯示)
    /// </summary>
    public List<Category> AvailableCategories { get; set; } = new();
}

/// <summary>
/// 備忘錄服務介面
/// </summary>
public interface IMemoNoteService
{
    /// <summary>
    /// 取得所有備忘錄
    /// </summary>
    Task<List<Note>> GetAllNotesAsync();

    /// <summary>
    /// 根據ID取得備忘錄
    /// </summary>
    Task<Note?> GetNoteByIdAsync(int id);

    /// <summary>
    /// 分頁取得備忘錄
    /// </summary>
    Task<List<Note>> GetNotesPagedAsync(int page, int pageSize);

    /// <summary>
    /// 取得總數量
    /// </summary>
    Task<int> GetTotalCountAsync();

    /// <summary>
    /// 新增備忘錄
    /// </summary>
    Task<int> AddNoteAsync(Note note);

    /// <summary>
    /// 更新備忘錄
    /// </summary>
    Task<bool> UpdateNoteAsync(Note note);

    /// <summary>
    /// 刪除備忘錄
    /// </summary>
    Task<bool> DeleteNoteAsync(int id);
}

/// <summary>
/// 增強型備忘錄服務介面
/// </summary>
public interface IEnhancedMemoNoteService : IMemoNoteService
{
    /// <summary>
    /// 搜尋備忘錄
    /// </summary>
    Task<List<Note>> SearchNotesAsync(SearchFilterModel filter, int page, int pageSize);
    
    /// <summary>
    /// 取得搜尋結果總數
    /// </summary>
    Task<int> GetSearchResultCountAsync(SearchFilterModel filter);
    
    /// <summary>
    /// 取得所有標籤
    /// </summary>
    Task<List<Tag>> GetAllTagsAsync();
    
    /// <summary>
    /// 建立標籤
    /// </summary>
    Task<Tag> CreateTagAsync(string name, string color);
    
    /// <summary>
    /// 為備忘錄新增標籤
    /// </summary>
    Task<bool> AddTagToNoteAsync(int noteId, int tagId);
    
    /// <summary>
    /// 從備忘錄移除標籤
    /// </summary>
    Task<bool> RemoveTagFromNoteAsync(int noteId, int tagId);
    
    /// <summary>
    /// 取得所有分類
    /// </summary>
    Task<List<Category>> GetCategoriesAsync();
    
    /// <summary>
    /// 建立分類
    /// </summary>
    Task<Category> CreateCategoryAsync(string name, int? parentId);
    
    /// <summary>
    /// 刪除標籤
    /// </summary>
    Task<bool> DeleteTagAsync(int tagId);
    
    /// <summary>
    /// 刪除分類
    /// </summary>
    Task<bool> DeleteCategoryAsync(int categoryId);
    
    /// <summary>
    /// 執行批次操作
    /// </summary>
    Task<BatchOperationResult> ExecuteBatchOperationAsync(BatchOperationRequest request);
    
    /// <summary>
    /// 匯出為 PDF
    /// </summary>
    Task<byte[]> ExportToPdfAsync(List<int> noteIds);
    
    /// <summary>
    /// 匯出為 Excel
    /// </summary>
    Task<byte[]> ExportToExcelAsync(List<int> noteIds);
}

/// <summary>
/// 處理日期註記的儲存和讀取，使用 JSON 檔案作為持久化儲存。
/// </summary>
public interface INoteService
{
    /// <summary>
    /// 取得指定日期的註記。
    /// </summary>
    Task<string?> GetNoteAsync(DateOnly date);
    
    /// <summary>
    /// 儲存或更新指定日期的註記。
    /// </summary>
    Task SaveNoteAsync(DateOnly date, string note);
    
    /// <summary>
    /// 刪除指定日期的註記。
    /// </summary>
    Task DeleteNoteAsync(DateOnly date);
    
    /// <summary>
    /// 取得指定月份內有註記的所有日期。
    /// </summary>
    Task<HashSet<DateOnly>> GetNoteDatesInMonthAsync(int year, int month);
}

/// <summary>
/// JSON 檔案為基礎的註記服務實作。
/// </summary>
public sealed class JsonNoteService : INoteService
{
    private readonly string notesFilePath;
    private readonly ILogger<JsonNoteService> logger;
    private readonly SemaphoreSlim fileLock = new(1, 1);

    public JsonNoteService(ILogger<JsonNoteService> logger)
    {
        this.logger = logger;
        // 將註記檔案存放在 App_Data 資料夾
        var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        Directory.CreateDirectory(dataDirectory);
        notesFilePath = Path.Combine(dataDirectory, "notes.json");
    }

    /// <summary>
    /// 取得指定日期的註記。
    /// </summary>
    public async Task<string?> GetNoteAsync(DateOnly date)
    {
        await fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var key = date.ToString("yyyy-MM-dd");
            return notes.TryGetValue(key, out var note) ? note : null;
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// 儲存或更新指定日期的註記。
    /// </summary>
    public async Task SaveNoteAsync(DateOnly date, string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            await DeleteNoteAsync(date);
            return;
        }

        await fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var key = date.ToString("yyyy-MM-dd");
            notes[key] = note.Trim();
            await SaveNotesToFileAsync(notes);
            
            logger.LogInformation("註記已儲存：{Date} = {Note}", key, note);
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// 刪除指定日期的註記。
    /// </summary>
    public async Task DeleteNoteAsync(DateOnly date)
    {
        await fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var key = date.ToString("yyyy-MM-dd");
            
            if (notes.Remove(key))
            {
                await SaveNotesToFileAsync(notes);
                logger.LogInformation("註記已刪除：{Date}", key);
            }
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// 取得指定月份內有註記的所有日期。
    /// </summary>
    public async Task<HashSet<DateOnly>> GetNoteDatesInMonthAsync(int year, int month)
    {
        await fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var noteDates = new HashSet<DateOnly>();
            
            foreach (var key in notes.Keys)
            {
                if (DateOnly.TryParse(key, out var date) && 
                    date.Year == year && 
                    date.Month == month)
                {
                    noteDates.Add(date);
                }
            }
            
            return noteDates;
        }
        finally
        {
            fileLock.Release();
        }
    }

    /// <summary>
    /// 從 JSON 檔案載入註記資料。
    /// </summary>
    private async Task<Dictionary<string, string>> LoadNotesAsync()
    {
        try
        {
            if (!File.Exists(notesFilePath))
            {
                return new Dictionary<string, string>();
            }

            var json = await File.ReadAllTextAsync(notesFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new Dictionary<string, string>();
            }

            var notes = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
            return notes ?? new Dictionary<string, string>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "載入註記檔案時發生錯誤：{FilePath}", notesFilePath);
            return new Dictionary<string, string>();
        }
    }

    /// <summary>
    /// 將註記資料儲存至 JSON 檔案。
    /// </summary>
    private async Task SaveNotesToFileAsync(Dictionary<string, string> notes)
    {
        try
        {
            var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            await File.WriteAllTextAsync(notesFilePath, json);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "儲存註記檔案時發生錯誤：{FilePath}", notesFilePath);
            throw;
        }
    }
}

/// <summary>
/// JSON 檔案為基礎的備忘錄服務實作
/// </summary>
public sealed class JsonMemoNoteService : IEnhancedMemoNoteService
{
    private readonly string _dataFilePath;
    private readonly string _tagsFilePath;
    private readonly string _categoriesFilePath;
    private readonly ILogger<JsonMemoNoteService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonMemoNoteService(ILogger<JsonMemoNoteService> logger)
    {
        _logger = logger;
        
        // 設定資料檔案路徑
        var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _dataFilePath = Path.Combine(dataDirectory, "memo-notes.json");
        _tagsFilePath = Path.Combine(dataDirectory, "tags.json");
        _categoriesFilePath = Path.Combine(dataDirectory, "categories.json");
    }

    /// <summary>
    /// 取得所有備忘錄
    /// </summary>
    public async Task<List<Note>> GetAllNotesAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            return notes.OrderByDescending(n => n.CreatedDate).ToList();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 根據ID取得備忘錄
    /// </summary>
    public async Task<Note?> GetNoteByIdAsync(int id)
    {
        var notes = await GetAllNotesAsync();
        return notes.FirstOrDefault(n => n.Id == id);
    }

    /// <summary>
    /// 分頁取得備忘錄
    /// </summary>
    public async Task<List<Note>> GetNotesPagedAsync(int page, int pageSize)
    {
        var allNotes = await GetAllNotesAsync();
        return allNotes
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    /// <summary>
    /// 取得總數量
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        var notes = await GetAllNotesAsync();
        return notes.Count;
    }

    /// <summary>
    /// 新增備忘錄
    /// </summary>
    public async Task<int> AddNoteAsync(Note note)
    {
        await _fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            
            // 產生新的 ID
            var newId = notes.Count > 0 ? notes.Max(n => n.Id) + 1 : 1;
            note.Id = newId;
            note.CreatedDate = DateTime.Now;
            note.ModifiedDate = DateTime.Now;

            notes.Add(note);
            await SaveNotesAsync(notes);

            _logger.LogInformation("新增備忘錄成功，ID: {Id}, 標題: {Title}", note.Id, note.Title);
            return note.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "新增備忘錄時發生錯誤");
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 更新備忘錄
    /// </summary>
    public async Task<bool> UpdateNoteAsync(Note note)
    {
        await _fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var existingNote = notes.FirstOrDefault(n => n.Id == note.Id);
            
            if (existingNote is null)
            {
                return false;
            }

            existingNote.Title = note.Title;
            existingNote.Content = note.Content;
            existingNote.ModifiedDate = DateTime.Now;

            await SaveNotesAsync(notes);

            _logger.LogInformation("更新備忘錄成功，ID: {Id}, 標題: {Title}", note.Id, note.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新備忘錄時發生錯誤，ID: {Id}", note.Id);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 刪除備忘錄
    /// </summary>
    public async Task<bool> DeleteNoteAsync(int id)
    {
        await _fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var noteToRemove = notes.FirstOrDefault(n => n.Id == id);
            
            if (noteToRemove is null)
            {
                return false;
            }

            notes.Remove(noteToRemove);
            await SaveNotesAsync(notes);

            _logger.LogInformation("刪除備忘錄成功，ID: {Id}, 標題: {Title}", id, noteToRemove.Title);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除備忘錄時發生錯誤，ID: {Id}", id);
            throw;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 從JSON檔案載入備忘錄
    /// </summary>
    private async Task<List<Note>> LoadNotesAsync()
    {
        try
        {
            if (!File.Exists(_dataFilePath))
            {
                _logger.LogInformation("備忘錄檔案不存在，建立新檔案: {FilePath}", _dataFilePath);
                return new List<Note>();
            }

            var json = await File.ReadAllTextAsync(_dataFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Note>();
            }

            var notes = JsonSerializer.Deserialize<List<Note>>(json) ?? new List<Note>();
            
            // 載入分類資料以供關聯
            var categories = await LoadCategoriesAsync();
            
            // 為每個備忘錄載入分類關聯
            foreach (var note in notes)
            {
                if (note.CategoryId.HasValue)
                {
                    note.Category = categories.FirstOrDefault(c => c.Id == note.CategoryId.Value);
                }
            }
            
            return notes;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON 格式錯誤，建立新的備忘錄清單");
            return new List<Note>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入備忘錄檔案時發生錯誤: {FilePath}", _dataFilePath);
            throw;
        }
    }

    /// <summary>
    /// 儲存備忘錄到JSON檔案
    /// </summary>
    private async Task SaveNotesAsync(List<Note> notes)
    {
        try
        {
            var json = JsonSerializer.Serialize(notes, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(_dataFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存備忘錄檔案時發生錯誤: {FilePath}", _dataFilePath);
            throw;
        }
    }

    #region 進階功能實作

    /// <summary>
    /// 搜尋備忘錄
    /// </summary>
    public async Task<List<Note>> SearchNotesAsync(SearchFilterModel filter, int page, int pageSize)
    {
        var allNotes = await GetAllNotesAsync();
        var query = allNotes.AsEnumerable();

        // 關鍵字搜尋
        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.ToLowerInvariant();
            query = query.Where(n => 
                n.Title.ToLowerInvariant().Contains(keyword) ||
                n.Content.ToLowerInvariant().Contains(keyword) ||
                n.Tags.Any(t => t.Name.ToLowerInvariant().Contains(keyword))
            );
        }

        // 標籤篩選
        if (filter.Tags.Any())
        {
            query = query.Where(n => 
                n.Tags.Any(t => filter.Tags.Contains(t.Name))
            );
        }

        // 日期範圍篩選
        if (filter.StartDate.HasValue)
        {
            query = query.Where(n => n.CreatedDate >= filter.StartDate.Value);
        }

        if (filter.EndDate.HasValue)
        {
            query = query.Where(n => n.CreatedDate <= filter.EndDate.Value.AddDays(1));
        }

        // 分類篩選
        if (filter.CategoryId.HasValue)
        {
            query = query.Where(n => n.CategoryId == filter.CategoryId.Value);
        }

        // 排序
        query = filter.SortBy switch
        {
            SortBy.CreatedDate => filter.SortOrder == SortOrder.Desc 
                ? query.OrderByDescending(n => n.CreatedDate)
                : query.OrderBy(n => n.CreatedDate),
            SortBy.ModifiedDate => filter.SortOrder == SortOrder.Desc 
                ? query.OrderByDescending(n => n.ModifiedDate)
                : query.OrderBy(n => n.ModifiedDate),
            SortBy.Title => filter.SortOrder == SortOrder.Desc 
                ? query.OrderByDescending(n => n.Title)
                : query.OrderBy(n => n.Title),
            _ => query.OrderByDescending(n => n.ModifiedDate)
        };

        // 分頁
        return query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    /// <summary>
    /// 取得搜尋結果總數
    /// </summary>
    public async Task<int> GetSearchResultCountAsync(SearchFilterModel filter)
    {
        var results = await SearchNotesAsync(filter, 1, int.MaxValue);
        return results.Count;
    }

    /// <summary>
    /// 取得所有標籤
    /// </summary>
    public async Task<List<Tag>> GetAllTagsAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            return await LoadTagsAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 建立標籤
    /// </summary>
    public async Task<Tag> CreateTagAsync(string name, string color)
    {
        await _fileLock.WaitAsync();
        try
        {
            var tags = await LoadTagsAsync();
            
            var existingTag = tags.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (existingTag != null)
            {
                return existingTag;
            }

            var newTag = new Tag
            {
                Id = tags.Count > 0 ? tags.Max(t => t.Id) + 1 : 1,
                Name = name,
                Color = color,
                CreatedDate = DateTime.Now,
                UsageCount = 0
            };

            tags.Add(newTag);
            await SaveTagsAsync(tags);

            return newTag;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 為備忘錄新增標籤
    /// </summary>
    public async Task<bool> AddTagToNoteAsync(int noteId, int tagId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var note = notes.FirstOrDefault(n => n.Id == noteId);
            if (note == null) return false;

            var tags = await LoadTagsAsync();
            var tag = tags.FirstOrDefault(t => t.Id == tagId);
            if (tag == null) return false;

            if (!note.Tags.Any(t => t.Id == tagId))
            {
                note.Tags.Add(tag);
                tag.UsageCount++;
                
                await SaveNotesAsync(notes);
                await SaveTagsAsync(tags);
            }

            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 從備忘錄移除標籤
    /// </summary>
    public async Task<bool> RemoveTagFromNoteAsync(int noteId, int tagId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var notes = await LoadNotesAsync();
            var note = notes.FirstOrDefault(n => n.Id == noteId);
            if (note == null) return false;

            var tagToRemove = note.Tags.FirstOrDefault(t => t.Id == tagId);
            if (tagToRemove != null)
            {
                note.Tags.Remove(tagToRemove);
                
                var tags = await LoadTagsAsync();
                var tag = tags.FirstOrDefault(t => t.Id == tagId);
                if (tag != null)
                {
                    tag.UsageCount = Math.Max(0, tag.UsageCount - 1);
                    await SaveTagsAsync(tags);
                }
                
                await SaveNotesAsync(notes);
            }

            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 取得所有分類
    /// </summary>
    public async Task<List<Category>> GetCategoriesAsync()
    {
        await _fileLock.WaitAsync();
        try
        {
            return await LoadCategoriesAsync();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 建立分類
    /// </summary>
    public async Task<Category> CreateCategoryAsync(string name, int? parentId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var categories = await LoadCategoriesAsync();
            
            var newCategory = new Category
            {
                Id = categories.Count > 0 ? categories.Max(c => c.Id) + 1 : 1,
                Name = name,
                ParentId = parentId
            };

            categories.Add(newCategory);
            await SaveCategoriesAsync(categories);

            return newCategory;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 刪除標籤
    /// </summary>
    public async Task<bool> DeleteTagAsync(int tagId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var tags = await LoadTagsAsync();
            var tagToDelete = tags.FirstOrDefault(t => t.Id == tagId);
            
            if (tagToDelete == null)
            {
                return false;
            }

            // 檢查是否有備忘錄使用此標籤
            var notes = await LoadNotesAsync();
            var notesWithTag = notes.Where(n => n.Tags.Any(t => t.Id == tagId)).ToList();
            
            if (notesWithTag.Any())
            {
                // 從所有使用此標籤的備忘錄中移除此標籤
                foreach (var note in notesWithTag)
                {
                    note.Tags.RemoveAll(t => t.Id == tagId);
                    note.ModifiedDate = DateTime.Now;
                }
                
                // 儲存更新後的備忘錄
                await SaveNotesAsync(notes);
            }

            // 從標籤列表中移除
            tags.Remove(tagToDelete);
            await SaveTagsAsync(tags);

            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 刪除分類
    /// </summary>
    public async Task<bool> DeleteCategoryAsync(int categoryId)
    {
        await _fileLock.WaitAsync();
        try
        {
            var categories = await LoadCategoriesAsync();
            var categoryToDelete = categories.FirstOrDefault(c => c.Id == categoryId);
            
            if (categoryToDelete == null)
            {
                return false;
            }

            // 檢查是否有子分類
            var hasChildren = categories.Any(c => c.ParentId == categoryId);
            if (hasChildren)
            {
                throw new InvalidOperationException("無法刪除包含子分類的分類，請先刪除子分類");
            }

            // 檢查是否有備忘錄使用此分類
            var notes = await LoadNotesAsync();
            var notesWithCategory = notes.Where(n => n.CategoryId == categoryId).ToList();
            
            if (notesWithCategory.Any())
            {
                // 將使用此分類的備忘錄的分類設為null
                foreach (var note in notesWithCategory)
                {
                    note.CategoryId = null;
                    note.Category = null;
                    note.ModifiedDate = DateTime.Now;
                }
                
                // 儲存更新後的備忘錄
                await SaveNotesAsync(notes);
            }

            // 從分類列表中移除
            categories.Remove(categoryToDelete);
            await SaveCategoriesAsync(categories);

            return true;
        }
        finally
        {
            _fileLock.Release();
        }
    }

    /// <summary>
    /// 執行批次操作
    /// </summary>
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
                    if (request.Parameters.TryGetValue("tagName", out var tagNameObj) && 
                        request.Parameters.TryGetValue("tagColor", out var tagColorObj))
                    {
                        result = await BatchAddTagAsync(request.NoteIds, tagNameObj.ToString()!, tagColorObj.ToString()!);
                    }
                    break;

                case BatchOperation.ChangeCategory:
                    if (request.Parameters.TryGetValue("categoryId", out var categoryIdObj))
                    {
                        var categoryId = Convert.ToInt32(categoryIdObj);
                        result = await BatchChangeCategoryAsync(request.NoteIds, categoryId);
                    }
                    break;

                default:
                    result.Success = false;
                    result.ErrorMessage = "不支援的批次操作類型";
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

    /// <summary>
    /// 匯出為 PDF（目前返回空陣列，待實作）
    /// </summary>
    public async Task<byte[]> ExportToPdfAsync(List<int> noteIds)
    {
        // 暫時實作，實際使用時需要安裝 PDF 產生套件
        await Task.Delay(1);
        return Array.Empty<byte>();
    }

    /// <summary>
    /// 匯出為 Excel（目前返回空陣列，待實作）
    /// </summary>
    public async Task<byte[]> ExportToExcelAsync(List<int> noteIds)
    {
        // 暫時實作，實際使用時需要安裝 Excel 產生套件
        await Task.Delay(1);
        return Array.Empty<byte>();
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 載入標籤資料
    /// </summary>
    private async Task<List<Tag>> LoadTagsAsync()
    {
        try
        {
            if (!File.Exists(_tagsFilePath))
            {
                return new List<Tag>();
            }

            var json = await File.ReadAllTextAsync(_tagsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Tag>();
            }

            var tags = JsonSerializer.Deserialize<List<Tag>>(json) ?? new List<Tag>();
            return tags;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入標籤檔案時發生錯誤: {FilePath}", _tagsFilePath);
            return new List<Tag>();
        }
    }

    /// <summary>
    /// 儲存標籤資料
    /// </summary>
    private async Task SaveTagsAsync(List<Tag> tags)
    {
        try
        {
            var json = JsonSerializer.Serialize(tags, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(_tagsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存標籤檔案時發生錯誤: {FilePath}", _tagsFilePath);
            throw;
        }
    }

    /// <summary>
    /// 載入分類資料
    /// </summary>
    private async Task<List<Category>> LoadCategoriesAsync()
    {
        try
        {
            if (!File.Exists(_categoriesFilePath))
            {
                return new List<Category>();
            }

            var json = await File.ReadAllTextAsync(_categoriesFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Category>();
            }

            var categories = JsonSerializer.Deserialize<List<Category>>(json) ?? new List<Category>();
            return categories;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入分類檔案時發生錯誤: {FilePath}", _categoriesFilePath);
            return new List<Category>();
        }
    }

    /// <summary>
    /// 儲存分類資料
    /// </summary>
    private async Task SaveCategoriesAsync(List<Category> categories)
    {
        try
        {
            var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            
            await File.WriteAllTextAsync(_categoriesFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存分類檔案時發生錯誤: {FilePath}", _categoriesFilePath);
            throw;
        }
    }

    /// <summary>
    /// 批次刪除
    /// </summary>
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

    /// <summary>
    /// 批次新增標籤
    /// </summary>
    private async Task<BatchOperationResult> BatchAddTagAsync(List<int> noteIds, string tagName, string tagColor)
    {
        var tag = await CreateTagAsync(tagName, tagColor);
        var processedCount = 0;

        foreach (var noteId in noteIds)
        {
            if (await AddTagToNoteAsync(noteId, tag.Id))
            {
                processedCount++;
            }
        }

        return new BatchOperationResult 
        { 
            Success = true, 
            ProcessedCount = processedCount 
        };
    }

    /// <summary>
    /// 批次更改分類
    /// </summary>
    private async Task<BatchOperationResult> BatchChangeCategoryAsync(List<int> noteIds, int categoryId)
    {
        var notes = await LoadNotesAsync();
        var processedCount = 0;

        foreach (var noteId in noteIds)
        {
            var note = notes.FirstOrDefault(n => n.Id == noteId);
            if (note != null)
            {
                note.CategoryId = categoryId;
                processedCount++;
            }
        }

        if (processedCount > 0)
        {
            await SaveNotesAsync(notes);
        }

        return new BatchOperationResult 
        { 
            Success = true, 
            ProcessedCount = processedCount 
        };
    }

    #endregion
}
