using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Demo.Models;
using Demo.Services;
using System.Text.Json;

namespace Demo.Pages
{
    public class HabitsModel : PageModel
    {
        private readonly HabitService _habitService;

        [BindProperty]
        public CreateHabitRequest NewHabit { get; set; } = new CreateHabitRequest();

        public HabitsPageModel PageData { get; set; } = new HabitsPageModel();

        public HabitsModel(HabitService habitService)
        {
            _habitService = habitService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                PageData = await _habitService.GetHabitsPageModelAsync();
                return Page();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "載入習慣資料時發生錯誤，請稍後再試。";
                Console.WriteLine($"OnGetAsync 錯誤: {ex.Message}");
                return Page();
            }
        }

        /// <summary>
        /// 新增習慣
        /// </summary>
        public async Task<IActionResult> OnPostCreateHabitAsync()
        {
            if (!ModelState.IsValid)
            {
                PageData = await _habitService.GetHabitsPageModelAsync();
                TempData["ErrorMessage"] = "請填入正確的習慣資訊。";
                return Page();
            }

            try
            {
                var habit = new Habit
                {
                    Name = NewHabit.Name,
                    Description = NewHabit.Description,
                    IconClass = string.IsNullOrWhiteSpace(NewHabit.IconClass) ? "fas fa-star" : NewHabit.IconClass,
                    CategoryId = NewHabit.CategoryId,
                    Frequency = NewHabit.Frequency,
                    TargetEndDate = NewHabit.TargetEndDate,
                    Color = string.IsNullOrWhiteSpace(NewHabit.Color) ? "#007bff" : NewHabit.Color,
                    TargetCount = NewHabit.TargetCount > 0 ? NewHabit.TargetCount : 1,
                    Tags = NewHabit.Tags ?? new List<string>()
                };

                var success = await _habitService.CreateHabitAsync(habit);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "習慣新增成功！";
                    return RedirectToPage();
                }
                else
                {
                    TempData["ErrorMessage"] = "新增習慣時發生錯誤，請稍後再試。";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "新增習慣時發生錯誤，請稍後再試。";
                Console.WriteLine($"OnPostCreateHabitAsync 錯誤: {ex.Message}");
            }

            PageData = await _habitService.GetHabitsPageModelAsync();
            return Page();
        }

        /// <summary>
        /// 標記習慣完成
        /// </summary>
        public async Task<IActionResult> OnPostMarkCompleteAsync([FromBody] MarkCompleteRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.HabitId))
                {
                    return new JsonResult(new { success = false, message = "習慣ID不能為空" });
                }

                var success = await _habitService.MarkHabitCompleteAsync(
                    request.HabitId, 
                    request.Date == DateTime.MinValue ? DateTime.Today : request.Date, 
                    request.Notes ?? string.Empty
                );

                if (success)
                {
                    return new JsonResult(new { success = true, message = "習慣已標記為完成！" });
                }
                else
                {
                    return new JsonResult(new { success = false, message = "標記完成時發生錯誤" });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPostMarkCompleteAsync 錯誤: {ex.Message}");
                return new JsonResult(new { success = false, message = "處理請求時發生錯誤" });
            }
        }

        /// <summary>
        /// 刪除習慣
        /// </summary>
        public async Task<IActionResult> OnPostDeleteHabitAsync(string habitId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(habitId))
                {
                    return new JsonResult(new { success = false, message = "習慣ID不能為空" });
                }

                var success = await _habitService.DeleteHabitAsync(habitId);

                if (success)
                {
                    TempData["SuccessMessage"] = "習慣已刪除。";
                    return RedirectToPage();
                }
                else
                {
                    TempData["ErrorMessage"] = "刪除習慣時發生錯誤。";
                    return RedirectToPage();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnPostDeleteHabitAsync 錯誤: {ex.Message}");
                TempData["ErrorMessage"] = "刪除習慣時發生錯誤，請稍後再試。";
                return RedirectToPage();
            }
        }

        /// <summary>
        /// 取得習慣進度資料 (API)
        /// </summary>
        public async Task<IActionResult> OnGetHabitProgressAsync(string habitId, int days = 7)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(habitId))
                {
                    return new JsonResult(new { success = false, message = "習慣ID不能為空" });
                }

                var endDate = DateTime.Today;
                var startDate = endDate.AddDays(-days + 1);
                var records = await _habitService.GetHabitRecordsAsync(habitId, startDate, endDate);
                
                var progressData = new List<object>();
                for (var date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    var record = records.FirstOrDefault(r => r.Date.Date == date.Date);
                    progressData.Add(new 
                    {
                        date = date.ToString("yyyy-MM-dd"),
                        completed = record?.CompletedCount ?? 0,
                        isCompleted = record != null && record.CompletedCount > 0
                    });
                }

                return new JsonResult(new { success = true, data = progressData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnGetHabitProgressAsync 錯誤: {ex.Message}");
                return new JsonResult(new { success = false, message = "取得進度資料時發生錯誤" });
            }
        }

        /// <summary>
        /// 取得習慣統計資料 (API)
        /// </summary>
        public async Task<IActionResult> OnGetHabitStatsAsync(string habitId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(habitId))
                {
                    return new JsonResult(new { success = false, message = "習慣ID不能為空" });
                }

                var currentStreak = await _habitService.GetCurrentStreakAsync(habitId);
                var longestStreak = await _habitService.GetLongestStreakForHabitAsync(habitId);
                var completionRate = await _habitService.GetCompletionRateAsync(habitId, 30);
                var totalCompletions = await _habitService.GetTotalCompletionsAsync(habitId);

                var stats = new
                {
                    currentStreak = currentStreak,
                    longestStreak = longestStreak,
                    completionRate = completionRate,
                    totalCompletions = totalCompletions
                };

                return new JsonResult(new { success = true, data = stats });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnGetHabitStatsAsync 錯誤: {ex.Message}");
                return new JsonResult(new { success = false, message = "取得統計資料時發生錯誤" });
            }
        }

        /// <summary>
        /// 取得分類清單 (API)
        /// </summary>
        public async Task<IActionResult> OnGetCategoriesAsync()
        {
            try
            {
                var categories = await _habitService.GetAllCategoriesAsync();
                return new JsonResult(new { success = true, data = categories });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnGetCategoriesAsync 錯誤: {ex.Message}");
                return new JsonResult(new { success = false, message = "取得分類資料時發生錯誤" });
            }
        }

        /// <summary>
        /// 取得週進度圖表資料 (API)
        /// </summary>
        public async Task<IActionResult> OnGetWeeklyProgressAsync()
        {
            try
            {
                var weeklyProgress = await _habitService.GetWeeklyProgressAsync();
                
                var chartData = new
                {
                    labels = weeklyProgress.Select(p => p.Date.ToString("MM/dd")).ToArray(),
                    datasets = new[]
                    {
                        new
                        {
                            label = "完成率 (%)",
                            data = weeklyProgress.Select(p => p.SuccessRate).ToArray(),
                            borderColor = "rgb(75, 192, 192)",
                            backgroundColor = "rgba(75, 192, 192, 0.2)",
                            tension = 0.1
                        }
                    }
                };

                return new JsonResult(new { success = true, data = chartData });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnGetWeeklyProgressAsync 錯誤: {ex.Message}");
                return new JsonResult(new { success = false, message = "取得週進度資料時發生錯誤" });
            }
        }
    }
}
