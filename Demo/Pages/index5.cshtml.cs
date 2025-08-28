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
        private readonly IEnhancedMemoNoteService _noteService;

        /// <summary>
        /// 備忘錄編輯檢視模型
        /// </summary>
        [BindProperty]
        public NoteEditViewModel ViewModel { get; set; } = new();

        /// <summary>
        /// 所有標籤清單
        /// </summary>
        public List<Tag> AllTags { get; set; } = new();

        /// <summary>
        /// 所有分類清單
        /// </summary>
        public List<Category> AllCategories { get; set; } = new();

        public index5(ILogger<index5> logger, IEnhancedMemoNoteService noteService)
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
                // 載入標籤和分類資料
                AllTags = await _noteService.GetAllTagsAsync();
                AllCategories = await _noteService.GetCategoriesAsync();

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
                        ModifiedDate = note.ModifiedDate,
                        CategoryId = note.CategoryId,
                        SelectedTagIds = note.Tags.Select(t => t.Id).ToList(),
                        AvailableTags = AllTags,
                        AvailableCategories = AllCategories
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
                        ModifiedDate = DateTime.Now,
                        AvailableTags = AllTags,
                        AvailableCategories = AllCategories
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
                // 重新載入標籤和分類資料 (表單提交後需要重新載入)
                AllTags = await _noteService.GetAllTagsAsync();
                AllCategories = await _noteService.GetCategoriesAsync();
                
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
                    // 重新設定 AvailableTags 和 AvailableCategories 供 UI 顯示
                    ViewModel.AvailableTags = AllTags;
                    ViewModel.AvailableCategories = AllCategories;
                    return Page();
                }

                var note = new Note
                {
                    Id = ViewModel.Id,
                    Title = ViewModel.Title.Trim(),
                    Content = ViewModel.Content.Trim(),
                    CategoryId = ViewModel.CategoryId
                };

                if (ViewModel.IsEditMode)
                {
                    // 更新現有備忘錄
                    var success = await _noteService.UpdateNoteAsync(note);
                    if (success)
                    {
                        // 處理標籤關聯更新
                        await UpdateNoteTagsAsync(note.Id, ViewModel.SelectedTagIds);
                        
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
                    
                    // 處理標籤關聯新增
                    await UpdateNoteTagsAsync(newId, ViewModel.SelectedTagIds);
                    
                    TempData["SuccessMessage"] = "備忘錄已成功新增。";
                    _logger.LogInformation("成功新增備忘錄，ID: {Id}, 標題: {Title}", newId, note.Title);
                }

                return RedirectToPage("index4");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "儲存備忘錄時發生錯誤，ID: {Id}, 標題: {Title}", ViewModel.Id, ViewModel.Title);
                TempData["ErrorMessage"] = "儲存備忘錄時發生錯誤，請稍後再試。";
                
                // 重新設定 AvailableTags 和 AvailableCategories 供 UI 顯示
                ViewModel.AvailableTags = AllTags;
                ViewModel.AvailableCategories = AllCategories;
                return Page();
            }
        }

        /// <summary>
        /// 更新備忘錄的標籤關聯
        /// </summary>
        private async Task UpdateNoteTagsAsync(int noteId, List<int> selectedTagIds)
        {
            try
            {
                // 取得目前的標籤
                var currentNote = await _noteService.GetNoteByIdAsync(noteId);
                var currentTagIds = currentNote?.Tags.Select(t => t.Id).ToList() ?? new List<int>();

                // 找出要新增的標籤 (在 selectedTagIds 中但不在 currentTagIds 中)
                var tagsToAdd = selectedTagIds.Except(currentTagIds).ToList();
                
                // 找出要移除的標籤 (在 currentTagIds 中但不在 selectedTagIds 中)
                var tagsToRemove = currentTagIds.Except(selectedTagIds).ToList();

                // 新增標籤
                foreach (var tagId in tagsToAdd)
                {
                    await _noteService.AddTagToNoteAsync(noteId, tagId);
                }

                // 移除標籤
                foreach (var tagId in tagsToRemove)
                {
                    await _noteService.RemoveTagFromNoteAsync(noteId, tagId);
                }

                _logger.LogInformation("更新備忘錄標籤完成，ID: {NoteId}, 新增: {AddCount}, 移除: {RemoveCount}", 
                    noteId, tagsToAdd.Count, tagsToRemove.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新備忘錄標籤時發生錯誤，NoteId: {NoteId}", noteId);
                // 不拋出異常，讓主要儲存操作能夠完成
            }
        }

        /// <summary>
        /// POST 取消編輯
        /// </summary>
        public IActionResult OnPostCancel()
        {
            return RedirectToPage("index4");
        }

        /// <summary>
        /// 建立新標籤 (AJAX)
        /// </summary>
        public async Task<IActionResult> OnPostCreateTagAsync(string tagName, string tagColor = "#007bff")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    return new JsonResult(new { success = false, message = "標籤名稱不能為空" });
                }

                var newTag = await _noteService.CreateTagAsync(tagName.Trim(), tagColor);
                
                _logger.LogInformation("成功建立新標籤，ID: {Id}, 名稱: {Name}", newTag.Id, newTag.Name);
                
                return new JsonResult(new { 
                    success = true, 
                    tagId = newTag.Id, 
                    tagName = newTag.Name, 
                    tagColor = newTag.Color 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立標籤時發生錯誤，名稱: {TagName}", tagName);
                return new JsonResult(new { success = false, message = "建立標籤時發生錯誤" });
            }
        }

        /// <summary>
        /// 建立新分類 (AJAX)
        /// </summary>
        public async Task<IActionResult> OnPostCreateCategoryAsync(string categoryName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    return new JsonResult(new { success = false, message = "分類名稱不能為空" });
                }

                var newCategory = await _noteService.CreateCategoryAsync(categoryName.Trim(), null);
                
                _logger.LogInformation("成功建立新分類，ID: {Id}, 名稱: {Name}", newCategory.Id, newCategory.Name);
                
                return new JsonResult(new { 
                    success = true, 
                    categoryId = newCategory.Id, 
                    categoryName = newCategory.Name 
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "建立分類時發生錯誤，名稱: {CategoryName}", categoryName);
                return new JsonResult(new { success = false, message = "建立分類時發生錯誤" });
            }
        }
    }
}
