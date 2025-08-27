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
                ViewModel = new NoteListViewModel();
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
        /// <param name="id">要刪除的備忘錄 ID</param>
        /// <param name="page">當前頁碼</param>
        public async Task<IActionResult> OnPostDeleteAsync(int id, int page = 1)
        {
            try
            {
                var success = await _noteService.DeleteNoteAsync(id);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "備忘錄已成功刪除。";
                    _logger.LogInformation("成功刪除備忘錄，ID: {Id}", id);
                }
                else
                {
                    TempData["ErrorMessage"] = "找不到指定的備忘錄。";
                    _logger.LogWarning("嘗試刪除不存在的備忘錄，ID: {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刪除備忘錄時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "刪除備忘錄時發生錯誤，請稍後再試。";
            }

            // 重新導向到列表頁面，保持當前頁碼
            return RedirectToPage("index4", new { page = page });
        }

        /// <summary>
        /// POST 搜尋處理
        /// </summary>
        public IActionResult OnPostSearch()
        {
            try
            {
                // 重新導向到 GET 請求，使用查詢字串保持搜尋條件
                var routeValues = new RouteValueDictionary { { "page", 1 } };
                
                if (!string.IsNullOrWhiteSpace(SearchFilter.Keyword))
                    routeValues.Add("SearchFilter.Keyword", SearchFilter.Keyword);
                
                if (SearchFilter.Tags.Any())
                {
                    for (int i = 0; i < SearchFilter.Tags.Count; i++)
                    {
                        routeValues.Add($"SearchFilter.Tags[{i}]", SearchFilter.Tags[i]);
                    }
                }
                
                if (SearchFilter.StartDate.HasValue)
                    routeValues.Add("SearchFilter.StartDate", SearchFilter.StartDate.Value.ToString("yyyy-MM-dd"));
                
                if (SearchFilter.EndDate.HasValue)
                    routeValues.Add("SearchFilter.EndDate", SearchFilter.EndDate.Value.ToString("yyyy-MM-dd"));
                
                if (SearchFilter.CategoryId.HasValue)
                    routeValues.Add("SearchFilter.CategoryId", SearchFilter.CategoryId.Value);
                
                routeValues.Add("SearchFilter.SortBy", SearchFilter.SortBy);
                routeValues.Add("SearchFilter.SortOrder", SearchFilter.SortOrder);

                return RedirectToPage("index4", routeValues);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "搜尋處理時發生錯誤");
                TempData["ErrorMessage"] = "搜尋時發生錯誤，請稍後再試。";
                return Page();
            }
        }

        /// <summary>
        /// POST 清除搜尋
        /// </summary>
        public IActionResult OnPostClearSearch()
        {
            return RedirectToPage("index4");
        }

        /// <summary>
        /// POST 批次操作處理
        /// </summary>
        public async Task<IActionResult> OnPostBatchOperationAsync(string operation, string selectedIds, string parameters = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedIds))
                {
                    TempData["ErrorMessage"] = "請至少選擇一個項目。";
                    return RedirectToPage("index4");
                }

                var noteIds = selectedIds.Split(',').Select(int.Parse).ToList();
                
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
                            request.Parameters[keyValue[0]] = keyValue[1];
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
        /// AJAX 取得標籤建議
        /// </summary>
        public async Task<IActionResult> OnGetTagSuggestionsAsync(string query)
        {
            try
            {
                var allTags = await _noteService.GetAllTagsAsync();
                var suggestions = allTags
                    .Where(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .Take(5)
                    .Select(t => new { id = t.Id, name = t.Name, color = t.Color, usageCount = t.UsageCount })
                    .ToList();

                return new JsonResult(suggestions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得標籤建議時發生錯誤");
            }

            return RedirectToPage("index4");
        }

        /// <summary>
        /// POST 匯出處理
        /// </summary>
        public async Task<IActionResult> OnPostExportAsync(string format, string selectedIds)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(selectedIds))
                {
                    TempData["ErrorMessage"] = "請至少選擇一個項目進行匯出。";
                    return RedirectToPage("index4");
                }

                var noteIds = selectedIds.Split(',').Select(int.Parse).ToList();
                var notes = new List<Note>();

                foreach (var noteId in noteIds)
                {
                    var note = await _noteService.GetNoteByIdAsync(noteId);
                    if (note != null)
                    {
                        notes.Add(note);
                    }
                }

                if (!notes.Any())
                {
                    TempData["ErrorMessage"] = "沒有找到要匯出的備忘錄。";
                    return RedirectToPage("index4");
                }

                byte[] fileData;
                string fileName;
                string contentType;

                switch (format?.ToLower())
                {
                    case "pdf":
                        fileData = await GeneratePdfExport(notes);
                        fileName = $"備忘錄匯出_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
                        contentType = "application/pdf";
                        break;

                    case "excel":
                        fileData = await GenerateExcelExport(notes);
                        fileName = $"備忘錄匯出_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                        contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;

                    case "json":
                        fileData = await GenerateJsonExport(notes);
                        fileName = $"備忘錄匯出_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                        contentType = "application/json";
                        break;

                    default:
                        TempData["ErrorMessage"] = "不支援的匯出格式。";
                        return RedirectToPage("index4");
                }

                _logger.LogInformation("匯出 {Count} 則備忘錄為 {Format} 格式", notes.Count, format);
                return File(fileData, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "匯出備忘錄時發生錯誤: {Format}", format);
                TempData["ErrorMessage"] = "匯出時發生錯誤，請稍後再試。";
                return RedirectToPage("index4");
            }
        }

        /// <summary>
        /// 產生 PDF 匯出
        /// </summary>
        private Task<byte[]> GeneratePdfExport(List<Note> notes)
        {
            // 簡單的 HTML to PDF 實作（實際專案中建議使用專業的 PDF 函式庫）
            var html = GenerateHtmlReport(notes, "PDF 匯出報告");
            
            // 暫時返回 HTML 轉換為 UTF-8 字節
            // 實際使用時可以集成 iTextSharp、PuppeteerSharp 等
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(html));
        }

        /// <summary>
        /// 產生 Excel 匯出
        /// </summary>
        private Task<byte[]> GenerateExcelExport(List<Note> notes)
        {
            // 簡單的 CSV 格式（實際專案中建議使用 EPPlus 或 NPOI）
            var csv = new System.Text.StringBuilder();
            
            // 標題行
            csv.AppendLine("ID,標題,內容,標籤,分類,建立日期,修改日期");
            
            foreach (var note in notes)
            {
                var tags = string.Join(";", note.Tags.Select(t => t.Name));
                var category = note.Category?.Name ?? "";
                
                csv.AppendLine($"{note.Id},\"{EscapeCsv(note.Title)}\",\"{EscapeCsv(note.Content)}\",\"{tags}\",\"{category}\",{note.CreatedDate:yyyy-MM-dd HH:mm:ss},{note.ModifiedDate:yyyy-MM-dd HH:mm:ss}");
            }
            
            return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(csv.ToString()));
        }

        /// <summary>
        /// 產生 JSON 匯出
        /// </summary>
        private Task<byte[]> GenerateJsonExport(List<Note> notes)
        {
            var exportData = new
            {
                ExportDate = DateTime.Now,
                ExportVersion = "1.0",
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
            var html = new System.Text.StringBuilder();
            
            html.AppendLine("<!DOCTYPE html>");
            html.AppendLine("<html>");
            html.AppendLine("<head>");
            html.AppendLine("<meta charset='utf-8'>");
            html.AppendLine($"<title>{title}</title>");
            html.AppendLine("<style>");
            html.AppendLine("body { font-family: 'Microsoft JhengHei', Arial, sans-serif; margin: 20px; }");
            html.AppendLine("h1 { color: #333; border-bottom: 2px solid #007bff; }");
            html.AppendLine(".note { border: 1px solid #ddd; margin: 20px 0; padding: 15px; border-radius: 5px; }");
            html.AppendLine(".note-title { color: #007bff; font-size: 18px; font-weight: bold; margin-bottom: 10px; }");
            html.AppendLine(".note-content { margin: 10px 0; line-height: 1.6; }");
            html.AppendLine(".note-meta { font-size: 12px; color: #666; margin-top: 10px; }");
            html.AppendLine(".tag { background: #007bff; color: white; padding: 2px 6px; border-radius: 3px; margin-right: 5px; font-size: 11px; }");
            html.AppendLine("</style>");
            html.AppendLine("</head>");
            html.AppendLine("<body>");
            
            html.AppendLine($"<h1>{title}</h1>");
            html.AppendLine($"<p>匯出時間：{DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>");
            html.AppendLine($"<p>備忘錄數量：{notes.Count} 則</p>");
            html.AppendLine("<hr>");
            
            foreach (var note in notes)
            {
                html.AppendLine("<div class='note'>");
                html.AppendLine($"<div class='note-title'>{System.Web.HttpUtility.HtmlEncode(note.Title)}</div>");
                html.AppendLine($"<div class='note-content'>{System.Web.HttpUtility.HtmlEncode(note.Content).Replace("\n", "<br>")}</div>");
                
                if (note.Tags.Any())
                {
                    html.AppendLine("<div>");
                    foreach (var tag in note.Tags)
                    {
                        html.AppendLine($"<span class='tag' style='background-color: {tag.Color}'>{System.Web.HttpUtility.HtmlEncode(tag.Name)}</span>");
                    }
                    html.AppendLine("</div>");
                }
                
                html.AppendLine("<div class='note-meta'>");
                html.AppendLine($"建立時間：{note.CreatedDate:yyyy-MM-dd HH:mm:ss} | ");
                html.AppendLine($"修改時間：{note.ModifiedDate:yyyy-MM-dd HH:mm:ss}");
                if (note.Category != null)
                {
                    html.AppendLine($" | 分類：{System.Web.HttpUtility.HtmlEncode(note.Category.Name)}");
                }
                html.AppendLine("</div>");
                html.AppendLine("</div>");
            }
            
            html.AppendLine("</body>");
            html.AppendLine("</html>");
            
            return html.ToString();
        }

        /// <summary>
        /// CSV 字串跳脫處理
        /// </summary>
        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            
            value = value.Replace("\"", "\"\""); // 跳脫雙引號
            if (value.Contains(",") || value.Contains("\n") || value.Contains("\r"))
            {
                value = $"\"{value}\""; // 包含逗號或換行符時用雙引號包圍
            }
            
            return value;
        }
    }
}
