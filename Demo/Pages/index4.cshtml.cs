using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                return new JsonResult(new List<object>());
            }
        }
    }
}
