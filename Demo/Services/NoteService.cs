using System.Text.Json;

namespace Demo.Services;

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
