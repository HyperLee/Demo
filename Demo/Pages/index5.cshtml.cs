using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Demo.Services;

namespace Demo.Pages
{
    /// <summary>
    /// 備忘錄編輯頁面模型
    /// </summary>
    public class index5 : PageModel
    {
        private readonly ILogger<index5> _logger;
        private readonly IMemoNoteService _noteService;

        /// <summary>
        /// 備忘錄編輯檢視模型
        /// </summary>
        [BindProperty]
        public NoteEditViewModel ViewModel { get; set; } = new();

        public index5(ILogger<index5> logger, IMemoNoteService noteService)
        {
            _logger = logger;
            _noteService = noteService;
        }

        /// <summary>
        /// GET 請求處理
        /// </summary>
        /// <param name="id">備忘錄ID，若為空則為新增模式</param>
        public async Task<IActionResult> OnGetAsync(int? id)
        {
            try
            {
                if (id.HasValue && id.Value > 0)
                {
                    // 編輯模式
                    var note = await _noteService.GetNoteByIdAsync(id.Value);
                    if (note is null)
                    {
                        _logger.LogWarning("找不到指定的備忘錄，ID: {Id}", id.Value);
                        TempData["ErrorMessage"] = "找不到指定的備忘錄。";
                        return RedirectToPage("index4");
                    }

                    ViewModel = new NoteEditViewModel
                    {
                        Id = note.Id,
                        Title = note.Title,
                        Content = note.Content,
                        IsEditMode = true,
                        CreatedDate = note.CreatedDate,
                        ModifiedDate = note.ModifiedDate
                    };

                    _logger.LogInformation("載入備忘錄編輯模式，ID: {Id}, 標題: {Title}", note.Id, note.Title);
                }
                else
                {
                    // 新增模式
                    ViewModel = new NoteEditViewModel
                    {
                        Id = 0,
                        Title = string.Empty,
                        Content = string.Empty,
                        IsEditMode = false,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now
                    };

                    _logger.LogInformation("進入備忘錄新增模式");
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "載入備忘錄頁面時發生錯誤，ID: {Id}", id);
                TempData["ErrorMessage"] = "載入備忘錄時發生錯誤，請稍後再試。";
                return RedirectToPage("index4");
            }
        }

        /// <summary>
        /// POST 儲存備忘錄
        /// </summary>
        public async Task<IActionResult> OnPostSaveAsync()
        {
            try
            {
                // 手動驗證
                if (string.IsNullOrWhiteSpace(ViewModel.Title))
                {
                    ModelState.AddModelError("ViewModel.Title", "標題不能為空。");
                }
                else if (ViewModel.Title.Length > 200)
                {
                    ModelState.AddModelError("ViewModel.Title", "標題長度不能超過200字元。");
                }

                if (string.IsNullOrWhiteSpace(ViewModel.Content))
                {
                    ModelState.AddModelError("ViewModel.Content", "內容不能為空。");
                }
                else if (ViewModel.Content.Length > 2000)
                {
                    ModelState.AddModelError("ViewModel.Content", "內容長度不能超過2000字元。");
                }

                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var note = new Note
                {
                    Id = ViewModel.Id,
                    Title = ViewModel.Title.Trim(),
                    Content = ViewModel.Content.Trim()
                };

                if (ViewModel.IsEditMode)
                {
                    // 更新現有備忘錄
                    var success = await _noteService.UpdateNoteAsync(note);
                    if (success)
                    {
                        TempData["SuccessMessage"] = "備忘錄已成功更新。";
                        _logger.LogInformation("成功更新備忘錄，ID: {Id}, 標題: {Title}", note.Id, note.Title);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "找不到指定的備忘錄。";
                        _logger.LogWarning("嘗試更新不存在的備忘錄，ID: {Id}", note.Id);
                    }
                }
                else
                {
                    // 新增備忘錄
                    var newId = await _noteService.AddNoteAsync(note);
                    TempData["SuccessMessage"] = "備忘錄已成功新增。";
                    _logger.LogInformation("成功新增備忘錄，ID: {Id}, 標題: {Title}", newId, note.Title);
                }

                return RedirectToPage("index4");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存備忘錄時發生錯誤，ID: {Id}, 標題: {Title}", ViewModel.Id, ViewModel.Title);
                TempData["ErrorMessage"] = "儲存備忘錄時發生錯誤，請稍後再試。";
                return Page();
            }
        }

        /// <summary>
        /// POST 取消編輯
        /// </summary>
        public IActionResult OnPostCancel()
        {
            return RedirectToPage("index4");
        }
    }
}
