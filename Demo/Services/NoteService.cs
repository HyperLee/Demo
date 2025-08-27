using System.Text.Json;

namespace Demo.Services;

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
public sealed class JsonMemoNoteService : IMemoNoteService
{
    private readonly string _dataFilePath;
    private readonly ILogger<JsonMemoNoteService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    public JsonMemoNoteService(ILogger<JsonMemoNoteService> logger)
    {
        _logger = logger;
        
        // 設定資料檔案路徑
        var dataDirectory = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        Directory.CreateDirectory(dataDirectory);
        _dataFilePath = Path.Combine(dataDirectory, "memo-notes.json");
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
}
