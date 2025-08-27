using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Demo.Services;

namespace Demo.Pages
{
    /// <summary>
    /// 備忘錄列表頁面模型
    /// </summary>
    public class index4 : PageModel
    {
        private readonly ILogger<index4> _logger;
        private readonly IEnhancedMemoNoteService _noteService;

        /// <summary>
        /// 備忘錄列表檢視模型
        /// </summary>
        public NoteListViewModel ViewModel { get; set; } = new();

        /// <summary>
        /// 搜尋篩選模型
        /// </summary>
        [BindProperty(SupportsGet = true)]
        public SearchFilterModel SearchFilter { get; set; } = new();

        /// <summary>
        /// 所有標籤清單
        /// </summary>
        public List<Tag> AllTags { get; set; } = new();

        /// <summary>
        /// 所有分類清單
        /// </summary>
        public List<Category> AllCategories { get; set; } = new();

        public index4(ILogger<index4> logger, IEnhancedMemoNoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        }

        /// <summary>
        /// GET 請求處理
        /// </summary>
        /// <param name="page">頁碼，預設為 1</param>
        public async Task OnGetAsync(int page = 1)
        {
            try
            {
                // 確保頁碼至少為 1
                if (page < 1) page = 1;

                // 載入標籤和分類資料
                AllTags = await _noteService.GetAllTagsAsync();
                AllCategories = await _noteService.GetCategoriesAsync();

                var pageSize = 20;
                List<Note> notes;
                int totalCount;

                // 檢查是否有搜尋條件
                if (HasSearchCriteria())
                {
                    // 使用搜尋功能
                    notes = await _noteService.SearchNotesAsync(SearchFilter, page, pageSize);
                    totalCount = await _noteService.GetSearchResultCountAsync(SearchFilter);
                    _logger.LogInformation("搜尋備忘錄，關鍵字: {Keyword}, 結果數量: {Count}", SearchFilter.Keyword, totalCount);
                }
                else
                {
                    // 使用一般分頁功能
                    notes = await _noteService.GetNotesPagedAsync(page, pageSize);
                    totalCount = await _noteService.GetTotalCountAsync();
                }

                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // 設定檢視模型
                ViewModel = new NoteListViewModel
                {
                    Notes = notes,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };

                _logger.LogInformation("載入備忘錄列表，頁碼: {Page}, 總數量: {TotalCount}", page, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入備忘錄列表時發生錯誤");
                
                // 設定預設值避免頁面錯誤
                ViewModel = new NoteListViewModel 
                { 
                    Notes = new List<Note>(), 
                    CurrentPage = 1, 
                    TotalPages = 1, 
                    PageSize = 20, 
                    TotalCount = 0 
                };
                AllTags = new List<Tag>();
                AllCategories = new List<Category>();
                
                // 設定錯誤訊息
                TempData["ErrorMessage"] = "載入備忘錄列表時發生錯誤，請稍後再試。";
            }
        }

        /// <summary>
        /// 檢查是否有搜尋條件
        /// </summary>
        public bool HasSearchCriteria()
        {
            return !string.IsNullOrWhiteSpace(SearchFilter.Keyword) ||
                   SearchFilter.Tags.Any() ||
                   SearchFilter.StartDate.HasValue ||
                   SearchFilter.EndDate.HasValue ||
                   SearchFilter.CategoryId.HasValue;
        }

        /// <summary>
        /// POST 刪除備忘錄
        /// </summary>
        /// <param name="id">要刪除的備忘錄ID</param>
        /// <param name="page">當前頁碼</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id, int page = 1)
        {
            try
            {
                var success = await _noteService.DeleteNoteAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "備忘錄刪除成功！";
                    _logger.LogInformation("成功刪除備忘錄，ID: {Id}", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "備忘錄刪除失敗，可能已不存在。";
                    _logger.LogWarning("刪除備忘錄失敗，ID: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除備忘錄時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除備忘錄時發生錯誤，請稍後再試。";
            }

            return RedirectToPage("index4", new { page });
        }

        /// <summary>
        /// POST 搜尋備忘錄
        /// </summary>
        public IActionResult OnPostSearch()
        {
            // 重設到第一頁
            return RedirectToPage("index4", new 
            { 
                page = 1,
                SearchFilter.Keyword,
                SearchFilter.Tags,
                SearchFilter.StartDate,
                SearchFilter.EndDate,
                SearchFilter.SortBy,
                SearchFilter.SortOrder,
                SearchFilter.CategoryId
            });
        }

        /// <summary>
        /// POST 清除搜尋條件
        /// </summary>
        public IActionResult OnPostClearSearch()
        {
            return RedirectToPage("index4");
        }

        /// <summary>
        /// POST 批次操作
        /// </summary>
        public async Task<IActionResult> OnPostBatchOperationAsync(string operation, string selectedIds, string parameters = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedIds))
                {
                    TempData["ErrorMessage"] = "請選擇要操作的項目。";
                    return RedirectToPage("index4");
                }

                var noteIds = selectedIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(int.Parse)
                    .ToList();

                if (!noteIds.Any())
                {
                    TempData["ErrorMessage"] = "請選擇要操作的項目。";
                    return RedirectToPage("index4");
                }

                var request = new BatchOperationRequest
                {
                    NoteIds = noteIds,
                    Operation = Enum.Parse<BatchOperation>(operation),
                    Parameters = new Dictionary<string, object>()
                };

                // 解析參數
                if (!string.IsNullOrWhiteSpace(parameters))
                {
                    var paramPairs = parameters.Split('&');
                    foreach (var pair in paramPairs)
                    {
                        var keyValue = pair.Split('=');
                        if (keyValue.Length == 2)
                        {
                            // URL 解碼參數值
                            var decodedValue = System.Web.HttpUtility.UrlDecode(keyValue[1]);
                            request.Parameters[keyValue[0]] = decodedValue;
                        }
                    }
                }

                var result = await _noteService.ExecuteBatchOperationAsync(request);

                if (result.Success)
                {
                    TempData["SuccessMessage"] = $"批次操作成功完成，處理了 {result.ProcessedCount} 個項目。";
                    _logger.LogInformation("批次操作成功: {Operation}, 處理數量: {Count}", operation, result.ProcessedCount);
                }
                else
                {
                    TempData["ErrorMessage"] = $"批次操作失敗：{result.ErrorMessage}";
                    _logger.LogWarning("批次操作失敗: {Operation}, 錯誤: {Error}", operation, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "批次操作處理時發生錯誤: {Operation}", operation);
                TempData["ErrorMessage"] = "批次操作時發生錯誤，請稍後再試。";
            }

            return RedirectToPage("index4");
        }

        /// <summary>
        /// POST 建立分類
        /// </summary>
        public async Task<IActionResult> OnPostCreateCategoryAsync(string categoryName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    return new JsonResult(new { success = false, message = "分類名稱不能為空" });
                }

                if (categoryName.Length > 50)
                {
                    return new JsonResult(new { success = false, message = "分類名稱不能超過 50 個字元" });
                }

                // 檢查分類名稱是否已存在
                var existingCategories = await _noteService.GetCategoriesAsync();
                if (existingCategories.Any(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase)))
                {
                    return new JsonResult(new { success = false, message = "此分類名稱已存在" });
                }

                var newCategory = await _noteService.CreateCategoryAsync(categoryName, null);
                
                _logger.LogInformation("成功建立新分類: {CategoryName}, ID: {CategoryId}", categoryName, newCategory.Id);
                
                return new JsonResult(new 
                { 
                    success = true, 
                    categoryId = newCategory.Id, 
                    categoryName = newCategory.Name 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立分類時發生錯誤: {CategoryName}", categoryName);
                return new JsonResult(new { success = false, message = "建立分類時發生錯誤，請稍後再試" });
            }
        }

        /// <summary>
        /// GET 標籤建議
        /// </summary>
        public async Task<IActionResult> OnGetTagSuggestionsAsync(string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return new JsonResult(new List<object>());
                }

                var allTags = await _noteService.GetAllTagsAsync();
                var suggestions = allTags
                    .Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Take(10)
                    .Select(t => new { id = t.Id, name = t.Name, color = t.Color })
                    .ToList();

                return new JsonResult(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得標籤建議時發生錯誤，查詢: {Query}", query);
                return new JsonResult(new List<object>());
            }
        }

        /// <summary>
        /// POST 匯出備忘錄
        /// </summary>
        public async Task<IActionResult> OnPostExportAsync(string format, string selectedIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedIds))
                {
                    TempData["ErrorMessage"] = "請選擇要匯出的項目。";
                    return RedirectToPage("index4");
                }

                var noteIds = selectedIds.Split(',')
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(int.Parse)
                    .ToList();

                if (!noteIds.Any())
                {
                    TempData["ErrorMessage"] = "請選擇要匯出的項目。";
                    return RedirectToPage("index4");
                }

                // 取得要匯出的備忘錄
                var allNotes = await _noteService.GetAllNotesAsync();
                var notesToExport = allNotes.Where(n => noteIds.Contains(n.Id)).ToList();

                if (!notesToExport.Any())
                {
                    TempData["ErrorMessage"] = "找不到要匯出的備忘錄。";
                    return RedirectToPage("index4");
                }

                byte[] fileData;
                string fileName;
                string contentType;

                switch (format.ToLower())
                {
                    case "pdf":
                        fileData = await GeneratePdfExport(notesToExport);
                        fileName = $"備忘錄匯出_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileData = await GenerateExcelExport(notesToExport);
                        fileName = $"備忘錄匯出_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "json":
                        fileData = await GenerateJsonExport(notesToExport);
                        fileName = $"備忘錄匯出_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        contentType = "application/json";
                        break;

                    default:
                        TempData["ErrorMessage"] = "不支援的匯出格式。";
                        return RedirectToPage("index4");
                }

                _logger.LogInformation("成功匯出 {Count} 則備忘錄為 {Format} 格式", notesToExport.Count, format.ToUpper());

                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯出備忘錄時發生錯誤，格式: {Format}", format);
                TempData["ErrorMessage"] = "匯出時發生錯誤，請稍後再試。";
                return RedirectToPage("index4");
            }
        }

        #region 私有方法

        /// <summary>
        /// 產生 PDF 匯出檔案
        /// </summary>
        private Task<byte[]> GeneratePdfExport(List<Note> notes)
        {
            // TODO: 實作 PDF 匯出功能
            // 這裡可以使用 iTextSharp 或其他 PDF 產生套件
            var htmlReport = GenerateHtmlReport(notes, "備忘錄 PDF 匯出");
            var htmlBytes = System.Text.Encoding.UTF8.GetBytes(htmlReport);
            
            // 暫時返回 HTML 內容作為 PDF（實際應該轉換為真正的 PDF）
            return Task.FromResult(htmlBytes);
        }

        /// <summary>
        /// 產生 Excel 匯出檔案
        /// </summary>
        private Task<byte[]> GenerateExcelExport(List<Note> notes)
        {
            // TODO: 實作 Excel 匯出功能
            // 這裡可以使用 ClosedXML 或 EPPlus 套件
            var csvContent = "標題,內容,標籤,分類,建立日期,修改日期\n";
            
            foreach (var note in notes)
            {
                var tags = string.Join("; ", note.Tags.Select(t => t.Name));
                var category = note.Category?.Name ?? "無分類";
                csvContent += $"\"{note.Title}\",\"{note.Content}\",\"{tags}\",\"{category}\",\"{note.CreatedDate:yyyy-MM-dd HH:mm}\",\"{note.ModifiedDate:yyyy-MM-dd HH:mm}\"\n";
            }
            
            // 暫時返回 CSV 內容（實際應該產生真正的 Excel 檔案）
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csvContent));
        }

        /// <summary>
        /// 產生 JSON 匯出檔案
        /// </summary>
        private Task<byte[]> GenerateJsonExport(List<Note> notes)
        {
            var exportData = new
            {
                ExportTime = DateTime.Now,
                TotalCount = notes.Count,
                Notes = notes.Select(n => new
                {
                    n.Id,
                    n.Title,
                    n.Content,
                    Tags = n.Tags.Select(t => new { t.Name, t.Color }),
                    Category = n.Category?.Name,
                    n.CreatedDate,
                    n.ModifiedDate
                })
            };

            var json = JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });

            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(json));
        }

        /// <summary>
        /// 產生 HTML 報告
        /// </summary>
        private string GenerateHtmlReport(List<Note> notes, string title)
        {
            var html = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>{title}</title>
    <style>
        body {{ font-family: Arial, sans-serif; margin: 40px; }}
        .header {{ text-align: center; margin-bottom: 30px; }}
        .note {{ margin-bottom: 30px; border-bottom: 1px solid #ccc; padding-bottom: 20px; }}
        .note-title {{ font-size: 18px; font-weight: bold; color: #333; }}
        .note-content {{ margin: 10px 0; line-height: 1.6; }}
        .note-meta {{ font-size: 12px; color: #666; }}
        .tags {{ margin: 5px 0; }}
        .tag {{ background: #007bff; color: white; padding: 2px 6px; border-radius: 3px; margin-right: 5px; }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>{title}</h1>
        <p>匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
        <p>總計：{notes.Count} 則備忘錄</p>
    </div>";

            foreach (var note in notes)
            {
                var tags = string.Join("", note.Tags.Select(t => $"<span class='tag'>{t.Name}</span>"));
                var category = note.Category?.Name ?? "無分類";
                
                html += $@"
    <div class='note'>
        <div class='note-title'>{note.Title}</div>
        <div class='note-content'>{note.Content.Replace("\n", "<br>")}</div>
        <div class='tags'>{tags}</div>
        <div class='note-meta'>
            分類：{category} | 
            建立：{note.CreatedDate:yyyy-MM-dd HH:mm} | 
            修改：{note.ModifiedDate:yyyy-MM-dd HH:mm}
        </div>
    </div>";
            }

            html += @"
</body>
</html>";

            return html;
        }

        #endregion
    }
}
