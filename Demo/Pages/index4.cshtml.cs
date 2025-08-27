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
        private readonly IMemoNoteService _noteService;

        /// <summary>
        /// 備忘錄列表檢視模型
        /// </summary>
        public NoteListViewModel ViewModel { get; set; } = new();

        public index4(ILogger<index4> logger, IMemoNoteService noteService)
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

                // 取得總數量和分頁資訊
                var totalCount = await _noteService.GetTotalCountAsync();
                var pageSize = 20;
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                // 取得分頁資料
                var notes = await _noteService.GetNotesPagedAsync(page, pageSize);

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
                
                // 設定錯誤訊息
                TempData["ErrorMessage"] = "載入備忘錄列表時發生錯誤，請稍後再試。";
            }
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
    }
}
