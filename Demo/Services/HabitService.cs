using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Demo.Models;

namespace Demo.Services
{
    /// <summary>
    /// 習慣管理服務
    /// </summary>
    public class HabitService
    {
        private readonly string _habitsPath;
        private readonly string _recordsPath;
        private readonly string _categoriesPath;
        private readonly JsonSerializerOptions _jsonOptions;

        public HabitService()
        {
            _habitsPath = Path.Combine("App_Data", "habits.json");
            _recordsPath = Path.Combine("App_Data", "habit-records.json");
            _categoriesPath = Path.Combine("App_Data", "habit-categories.json");
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // 確保資料目錄和檔案存在
            EnsureDataFiles();
        }

        #region 習慣管理方法

        /// <summary>
        /// 取得所有習慣
        /// </summary>
        public async Task<List<Habit>> GetAllHabitsAsync()
        {
            try
            {
                if (!File.Exists(_habitsPath))
                    return new List<Habit>();

                var json = await File.ReadAllTextAsync(_habitsPath);
                var habits = JsonSerializer.Deserialize<List<Habit>>(json, _jsonOptions) ?? new List<Habit>();
                return habits.Where(h => h.IsActive).OrderBy(h => h.Name).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取習慣資料時發生錯誤: {ex.Message}");
                return new List<Habit>();
            }
        }

        /// <summary>
        /// 根據 ID 取得習慣
        /// </summary>
        public async Task<Habit?> GetHabitByIdAsync(string id)
        {
            var habits = await GetAllHabitsAsync();
            return habits.FirstOrDefault(h => h.Id == id);
        }

        /// <summary>
        /// 新增習慣
        /// </summary>
        public async Task<bool> CreateHabitAsync(Habit habit)
        {
            try
            {
                var habits = await GetAllHabitsFromFileAsync();
                habit.Id = Guid.NewGuid().ToString();
                habit.CreatedAt = DateTime.Now;
                habits.Add(habit);

                await SaveHabitsAsync(habits);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"新增習慣時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新習慣
        /// </summary>
        public async Task<bool> UpdateHabitAsync(Habit habit)
        {
            try
            {
                var habits = await GetAllHabitsFromFileAsync();
                var existingHabit = habits.FirstOrDefault(h => h.Id == habit.Id);
                
                if (existingHabit == null)
                    return false;

                // 保留原始建立時間
                habit.CreatedAt = existingHabit.CreatedAt;
                
                var index = habits.IndexOf(existingHabit);
                habits[index] = habit;

                await SaveHabitsAsync(habits);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新習慣時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 刪除習慣（軟刪除）
        /// </summary>
        public async Task<bool> DeleteHabitAsync(string id)
        {
            try
            {
                var habits = await GetAllHabitsFromFileAsync();
                var habit = habits.FirstOrDefault(h => h.Id == id);
                
                if (habit == null)
                    return false;

                habit.IsActive = false;
                await SaveHabitsAsync(habits);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除習慣時發生錯誤: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 習慣記錄方法

        /// <summary>
        /// 標記習慣為完成
        /// </summary>
        public async Task<bool> MarkHabitCompleteAsync(string habitId, DateTime date, string notes = "")
        {
            try
            {
                var records = await GetAllRecordsAsync();
                var dateOnly = date.Date;
                
                var existingRecord = records.FirstOrDefault(r => r.HabitId == habitId && r.Date.Date == dateOnly);
                
                if (existingRecord != null)
                {
                    existingRecord.CompletedCount++;
                    existingRecord.Notes = notes;
                }
                else
                {
                    var newRecord = new HabitRecord
                    {
                        HabitId = habitId,
                        Date = dateOnly,
                        CompletedCount = 1,
                        Notes = notes,
                        CreatedAt = DateTime.Now
                    };
                    records.Add(newRecord);
                }

                await SaveRecordsAsync(records);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"標記習慣完成時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 取得習慣記錄
        /// </summary>
        public async Task<List<HabitRecord>> GetHabitRecordsAsync(string habitId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var records = await GetAllRecordsAsync();
            var query = records.Where(r => r.HabitId == habitId);

            if (startDate.HasValue)
                query = query.Where(r => r.Date >= startDate.Value.Date);
            
            if (endDate.HasValue)
                query = query.Where(r => r.Date <= endDate.Value.Date);

            return query.OrderBy(r => r.Date).ToList();
        }

        /// <summary>
        /// 檢查習慣今天是否已完成
        /// </summary>
        public async Task<bool> IsHabitCompletedTodayAsync(string habitId)
        {
            var today = DateTime.Today;
            var records = await GetHabitRecordsAsync(habitId, today, today);
            return records.Any(r => r.CompletedCount > 0);
        }

        /// <summary>
        /// 取得今天已完成的次數
        /// </summary>
        public async Task<int> GetTodayCompletedCountAsync(string habitId)
        {
            var today = DateTime.Today;
            var records = await GetHabitRecordsAsync(habitId, today, today);
            return records.Sum(r => r.CompletedCount);
        }

        #endregion

        #region 統計分析方法

        /// <summary>
        /// 取得習慣頁面模型
        /// </summary>
        public async Task<HabitsPageModel> GetHabitsPageModelAsync()
        {
            var habits = await GetAllHabitsAsync();
            var categories = await GetAllCategoriesAsync();
            var todayHabits = new List<HabitViewModel>();

            foreach (var habit in habits)
            {
                var category = categories.FirstOrDefault(c => c.Id == habit.CategoryId) ?? new HabitCategory { Name = "未分類" };
                var currentStreak = await GetCurrentStreakAsync(habit.Id);
                var completionRate = await GetCompletionRateAsync(habit.Id, 30);
                var totalCompletions = await GetTotalCompletionsAsync(habit.Id);
                var isTodayCompleted = await IsHabitCompletedTodayAsync(habit.Id);
                var todayCompletedCount = await GetTodayCompletedCountAsync(habit.Id);

                todayHabits.Add(new HabitViewModel
                {
                    Id = habit.Id,
                    Name = habit.Name,
                    Description = habit.Description,
                    IconClass = habit.IconClass,
                    Category = category,
                    CurrentStreak = currentStreak,
                    TotalCompletions = totalCompletions,
                    CompletionRate = completionRate,
                    IsTodayCompleted = isTodayCompleted,
                    Color = habit.Color,
                    TargetCount = habit.TargetCount,
                    TodayCompleted = todayCompletedCount,
                    Frequency = habit.Frequency,
                    Tags = habit.Tags
                });
            }

            var todayCompleted = todayHabits.Count(h => h.IsTodayCompleted);
            var weeklySuccessRate = await GetWeeklySuccessRateAsync();
            var longestStreak = await GetLongestStreakAsync();
            var weeklyProgress = await GetWeeklyProgressAsync();

            return new HabitsPageModel
            {
                TodayHabits = todayHabits,
                Categories = categories,
                TodayCompleted = todayCompleted,
                TotalHabits = habits.Count,
                WeeklySuccessRate = weeklySuccessRate,
                LongestStreak = longestStreak,
                WeeklyProgress = weeklyProgress
            };
        }

        /// <summary>
        /// 取得目前連續天數
        /// </summary>
        public async Task<int> GetCurrentStreakAsync(string habitId)
        {
            var records = await GetHabitRecordsAsync(habitId);
            if (!records.Any()) return 0;

            var streak = 0;
            var currentDate = DateTime.Today;
            
            // 從今天開始往回檢查
            while (true)
            {
                var record = records.FirstOrDefault(r => r.Date.Date == currentDate);
                if (record == null || record.CompletedCount == 0)
                    break;
                
                streak++;
                currentDate = currentDate.AddDays(-1);
            }

            return streak;
        }

        /// <summary>
        /// 取得完成率
        /// </summary>
        public async Task<double> GetCompletionRateAsync(string habitId, int days = 30)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-days + 1);
            var records = await GetHabitRecordsAsync(habitId, startDate, endDate);
            
            var completedDays = records.Count(r => r.CompletedCount > 0);
            return Math.Round((double)completedDays / days * 100, 1);
        }

        /// <summary>
        /// 取得總完成次數
        /// </summary>
        public async Task<int> GetTotalCompletionsAsync(string habitId)
        {
            var records = await GetHabitRecordsAsync(habitId);
            return records.Sum(r => r.CompletedCount);
        }

        /// <summary>
        /// 取得週成功率
        /// </summary>
        public async Task<double> GetWeeklySuccessRateAsync()
        {
            var habits = await GetAllHabitsAsync();
            if (!habits.Any()) return 0;

            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-6); // 過去7天

            var totalPossible = habits.Count * 7;
            var totalCompleted = 0;

            foreach (var habit in habits)
            {
                var records = await GetHabitRecordsAsync(habit.Id, startDate, endDate);
                totalCompleted += records.Count(r => r.CompletedCount > 0);
            }

            return totalPossible > 0 ? Math.Round((double)totalCompleted / totalPossible * 100, 1) : 0;
        }

        /// <summary>
        /// 取得最長連續天數
        /// </summary>
        public async Task<int> GetLongestStreakAsync()
        {
            var habits = await GetAllHabitsAsync();
            var longestStreak = 0;

            foreach (var habit in habits)
            {
                var streak = await GetLongestStreakForHabitAsync(habit.Id);
                if (streak > longestStreak)
                    longestStreak = streak;
            }

            return longestStreak;
        }

        /// <summary>
        /// 取得特定習慣的最長連續天數
        /// </summary>
        public async Task<int> GetLongestStreakForHabitAsync(string habitId)
        {
            var records = await GetHabitRecordsAsync(habitId);
            if (!records.Any()) return 0;

            var maxStreak = 0;
            var currentStreak = 0;
            var expectedDate = records.First().Date;

            foreach (var record in records.OrderBy(r => r.Date))
            {
                if (record.Date == expectedDate && record.CompletedCount > 0)
                {
                    currentStreak++;
                    maxStreak = Math.Max(maxStreak, currentStreak);
                }
                else
                {
                    currentStreak = record.CompletedCount > 0 ? 1 : 0;
                }
                
                expectedDate = record.Date.AddDays(1);
            }

            return maxStreak;
        }

        /// <summary>
        /// 取得週進度資料
        /// </summary>
        public async Task<List<HabitProgressData>> GetWeeklyProgressAsync()
        {
            var habits = await GetAllHabitsAsync();
            var progressData = new List<HabitProgressData>();
            
            for (int i = 6; i >= 0; i--)
            {
                var date = DateTime.Today.AddDays(-i);
                var completedCount = 0;
                
                foreach (var habit in habits)
                {
                    var isCompleted = await IsHabitCompletedOnDateAsync(habit.Id, date);
                    if (isCompleted) completedCount++;
                }

                var totalCount = habits.Count;
                var successRate = totalCount > 0 ? Math.Round((double)completedCount / totalCount * 100, 1) : 0;

                progressData.Add(new HabitProgressData
                {
                    Date = date,
                    CompletedCount = completedCount,
                    TotalCount = totalCount,
                    SuccessRate = successRate
                });
            }

            return progressData;
        }

        /// <summary>
        /// 檢查習慣在特定日期是否完成
        /// </summary>
        public async Task<bool> IsHabitCompletedOnDateAsync(string habitId, DateTime date)
        {
            var records = await GetHabitRecordsAsync(habitId, date.Date, date.Date);
            return records.Any(r => r.CompletedCount > 0);
        }

        #endregion

        #region 分類管理方法

        /// <summary>
        /// 取得所有分類
        /// </summary>
        public async Task<List<HabitCategory>> GetAllCategoriesAsync()
        {
            try
            {
                if (!File.Exists(_categoriesPath))
                    return GetDefaultCategories();

                var json = await File.ReadAllTextAsync(_categoriesPath);
                var categories = JsonSerializer.Deserialize<List<HabitCategory>>(json, _jsonOptions) ?? GetDefaultCategories();
                return categories.OrderBy(c => c.SortOrder).ThenBy(c => c.Name).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取分類資料時發生錯誤: {ex.Message}");
                return GetDefaultCategories();
            }
        }

        /// <summary>
        /// 新增分類
        /// </summary>
        public async Task<bool> CreateCategoryAsync(HabitCategory category)
        {
            try
            {
                var categories = await GetAllCategoriesAsync();
                category.Id = Guid.NewGuid().ToString();
                categories.Add(category);

                await SaveCategoriesAsync(categories);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"新增分類時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 更新分類
        /// </summary>
        public async Task<bool> UpdateCategoryAsync(HabitCategory category)
        {
            try
            {
                var categories = await GetAllCategoriesAsync();
                var existingCategory = categories.FirstOrDefault(c => c.Id == category.Id);
                
                if (existingCategory == null)
                    return false;

                var index = categories.IndexOf(existingCategory);
                categories[index] = category;

                await SaveCategoriesAsync(categories);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新分類時發生錯誤: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 刪除分類
        /// </summary>
        public async Task<bool> DeleteCategoryAsync(string id)
        {
            try
            {
                var categories = await GetAllCategoriesAsync();
                var category = categories.FirstOrDefault(c => c.Id == id);
                
                if (category == null)
                    return false;

                categories.Remove(category);
                await SaveCategoriesAsync(categories);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"刪除分類時發生錯誤: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 確保資料檔案存在
        /// </summary>
        private void EnsureDataFiles()
        {
            var directory = Path.GetDirectoryName(_habitsPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            if (!File.Exists(_habitsPath))
                File.WriteAllText(_habitsPath, "[]");

            if (!File.Exists(_recordsPath))
                File.WriteAllText(_recordsPath, "[]");

            if (!File.Exists(_categoriesPath))
            {
                var defaultCategories = GetDefaultCategories();
                var json = JsonSerializer.Serialize(defaultCategories, _jsonOptions);
                File.WriteAllText(_categoriesPath, json);
            }
        }

        /// <summary>
        /// 從檔案讀取所有習慣（包括已刪除的）
        /// </summary>
        private async Task<List<Habit>> GetAllHabitsFromFileAsync()
        {
            try
            {
                if (!File.Exists(_habitsPath))
                    return new List<Habit>();

                var json = await File.ReadAllTextAsync(_habitsPath);
                return JsonSerializer.Deserialize<List<Habit>>(json, _jsonOptions) ?? new List<Habit>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取習慣檔案時發生錯誤: {ex.Message}");
                return new List<Habit>();
            }
        }

        /// <summary>
        /// 儲存習慣資料
        /// </summary>
        private async Task SaveHabitsAsync(List<Habit> habits)
        {
            var json = JsonSerializer.Serialize(habits, _jsonOptions);
            await File.WriteAllTextAsync(_habitsPath, json);
        }

        /// <summary>
        /// 取得所有記錄
        /// </summary>
        private async Task<List<HabitRecord>> GetAllRecordsAsync()
        {
            try
            {
                if (!File.Exists(_recordsPath))
                    return new List<HabitRecord>();

                var json = await File.ReadAllTextAsync(_recordsPath);
                return JsonSerializer.Deserialize<List<HabitRecord>>(json, _jsonOptions) ?? new List<HabitRecord>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"讀取記錄檔案時發生錯誤: {ex.Message}");
                return new List<HabitRecord>();
            }
        }

        /// <summary>
        /// 儲存記錄資料
        /// </summary>
        private async Task SaveRecordsAsync(List<HabitRecord> records)
        {
            var json = JsonSerializer.Serialize(records, _jsonOptions);
            await File.WriteAllTextAsync(_recordsPath, json);
        }

        /// <summary>
        /// 儲存分類資料
        /// </summary>
        private async Task SaveCategoriesAsync(List<HabitCategory> categories)
        {
            var json = JsonSerializer.Serialize(categories, _jsonOptions);
            await File.WriteAllTextAsync(_categoriesPath, json);
        }

        /// <summary>
        /// 取得預設分類
        /// </summary>
        private List<HabitCategory> GetDefaultCategories()
        {
            return new List<HabitCategory>
            {
                new HabitCategory { Id = "health", Name = "健康", IconClass = "fas fa-heart", Color = "#e74c3c", SortOrder = 1 },
                new HabitCategory { Id = "fitness", Name = "運動", IconClass = "fas fa-dumbbell", Color = "#f39c12", SortOrder = 2 },
                new HabitCategory { Id = "learning", Name = "學習", IconClass = "fas fa-graduation-cap", Color = "#3498db", SortOrder = 3 },
                new HabitCategory { Id = "work", Name = "工作", IconClass = "fas fa-briefcase", Color = "#2ecc71", SortOrder = 4 },
                new HabitCategory { Id = "personal", Name = "個人", IconClass = "fas fa-user", Color = "#9b59b6", SortOrder = 5 },
                new HabitCategory { Id = "social", Name = "社交", IconClass = "fas fa-users", Color = "#1abc9c", SortOrder = 6 },
                new HabitCategory { Id = "other", Name = "其他", IconClass = "fas fa-star", Color = "#95a5a6", SortOrder = 99 }
            };
        }

        #endregion
    }
}
