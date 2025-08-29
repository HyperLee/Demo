using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 記帳資料服務介面
/// </summary>
public interface IAccountingService
{
    Task<List<AccountingRecord>> GetRecordsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<AccountingRecord?> GetRecordByIdAsync(int id);
    Task<int> CreateRecordAsync(AccountingRecord record);
    Task<bool> UpdateRecordAsync(AccountingRecord record);
    Task<bool> DeleteRecordAsync(int id);
    Task<List<AccountingCategory>> GetCategoriesAsync(string type);
    Task<MonthlySummary> GetMonthlySummaryAsync(int year, int month);
    Task<List<CalendarDay>> GetCalendarDataAsync(int year, int month);
    Task<List<string>> GetPaymentMethodsAsync();
    Task<bool> CreateCategoryAsync(string name, string type, string icon = "fas fa-folder");
    Task<bool> CreateSubCategoryAsync(string categoryName, string subCategoryName, string type);
    Task<bool> DeleteCategoryAsync(int categoryId, string type);
    Task<bool> DeleteSubCategoryAsync(int categoryId, int subCategoryId, string type);
}

/// <summary>
/// 記帳資料服務實作
/// </summary>
public class AccountingService : IAccountingService
{
    private readonly ILogger<AccountingService> _logger;
    private readonly string _recordsFilePath;
    private readonly string _categoriesFilePath;
    private readonly string _settingsFilePath;

    public AccountingService(ILogger<AccountingService> logger)
    {
        _logger = logger;
        
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _recordsFilePath = Path.Combine(appDataPath, "accounting-records.json");
        _categoriesFilePath = Path.Combine(appDataPath, "accounting-categories.json");
        _settingsFilePath = Path.Combine(appDataPath, "accounting-settings.json");
        
        // 初始化預設資料
        _ = Task.Run(InitializeDefaultDataAsync);
    }

    public async Task<List<AccountingRecord>> GetRecordsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var records = await LoadRecordsAsync();
            
            if (startDate.HasValue)
            {
                records = records.Where(r => r.Date >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                records = records.Where(r => r.Date <= endDate.Value).ToList();
            }
            
            return records.OrderByDescending(r => r.Date).ThenByDescending(r => r.CreatedDate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得記帳記錄時發生錯誤");
            return new List<AccountingRecord>();
        }
    }

    public async Task<AccountingRecord?> GetRecordByIdAsync(int id)
    {
        try
        {
            var records = await LoadRecordsAsync();
            return records.FirstOrDefault(r => r.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得記帳記錄 {Id} 時發生錯誤", id);
            return null;
        }
    }

    public async Task<int> CreateRecordAsync(AccountingRecord record)
    {
        try
        {
            var records = await LoadRecordsAsync();
            
            // 生成新 ID
            record.Id = records.Any() ? records.Max(r => r.Id) + 1 : 1;
            record.CreatedDate = DateTime.Now;
            record.ModifiedDate = DateTime.Now;
            
            records.Add(record);
            await SaveRecordsAsync(records);
            
            _logger.LogInformation("成功建立記帳記錄 {Id}", record.Id);
            return record.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立記帳記錄時發生錯誤");
            throw;
        }
    }

    public async Task<bool> UpdateRecordAsync(AccountingRecord record)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var existingRecord = records.FirstOrDefault(r => r.Id == record.Id);
            
            if (existingRecord == null)
            {
                return false;
            }
            
            // 更新資料，保留原建立日期
            record.CreatedDate = existingRecord.CreatedDate;
            record.ModifiedDate = DateTime.Now;
            
            var index = records.IndexOf(existingRecord);
            records[index] = record;
            
            await SaveRecordsAsync(records);
            
            _logger.LogInformation("成功更新記帳記錄 {Id}", record.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新記帳記錄 {Id} 時發生錯誤", record.Id);
            return false;
        }
    }

    public async Task<bool> DeleteRecordAsync(int id)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var recordToDelete = records.FirstOrDefault(r => r.Id == id);
            
            if (recordToDelete == null)
            {
                return false;
            }
            
            records.Remove(recordToDelete);
            await SaveRecordsAsync(records);
            
            _logger.LogInformation("成功刪除記帳記錄 {Id}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除記帳記錄 {Id} 時發生錯誤", id);
            return false;
        }
    }

    public async Task<List<AccountingCategory>> GetCategoriesAsync(string type)
    {
        try
        {
            var categories = await LoadCategoriesAsync();
            return categories.Where(c => c.Type == type).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得分類資料時發生錯誤");
            return new List<AccountingCategory>();
        }
    }

    public async Task<MonthlySummary> GetMonthlySummaryAsync(int year, int month)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var records = await GetRecordsAsync(startDate, endDate);
            
            var incomeRecords = records.Where(r => r.Type == "Income").ToList();
            var expenseRecords = records.Where(r => r.Type == "Expense").ToList();
            
            return new MonthlySummary
            {
                Year = year,
                Month = month,
                TotalIncome = incomeRecords.Sum(r => r.Amount),
                TotalExpense = expenseRecords.Sum(r => r.Amount),
                TotalRecords = records.Count,
                IncomeRecords = incomeRecords.Count,
                ExpenseRecords = expenseRecords.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得月度統計 {Year}-{Month} 時發生錯誤", year, month);
            return new MonthlySummary { Year = year, Month = month };
        }
    }

    public async Task<List<CalendarDay>> GetCalendarDataAsync(int year, int month)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            // 取得月曆的開始和結束日期（包含上下月的日期）
            var calendarStart = startDate.AddDays(-(int)startDate.DayOfWeek);
            var calendarEnd = endDate.AddDays(6 - (int)endDate.DayOfWeek);
            
            var records = await GetRecordsAsync(calendarStart, calendarEnd);
            var today = DateTime.Today;
            
            var calendarDays = new List<CalendarDay>();
            
            for (var date = calendarStart; date <= calendarEnd; date = date.AddDays(1))
            {
                var dayRecords = records.Where(r => r.Date.Date == date.Date).ToList();
                
                calendarDays.Add(new CalendarDay
                {
                    Date = date,
                    IsCurrentMonth = date.Month == month,
                    IsToday = date.Date == today,
                    Records = dayRecords
                });
            }
            
            return calendarDays;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得月曆資料 {Year}-{Month} 時發生錯誤", year, month);
            return new List<CalendarDay>();
        }
    }

    public async Task<List<string>> GetPaymentMethodsAsync()
    {
        await Task.CompletedTask;
        return new List<string>
        {
            "現金",
            "信用卡",
            "金融卡",
            "Apple Pay",
            "Google Pay", 
            "LINE Pay",
            "悠遊卡",
            "一卡通",
            "銀行轉帳",
            "其他"
        };
    }

    public async Task<bool> CreateCategoryAsync(string name, string type, string icon = "fas fa-folder")
    {
        try
        {
            var categories = await LoadCategoriesAsync();
            
            // 檢查是否已存在相同名稱的分類
            if (categories.Any(c => c.Name == name && c.Type == type))
            {
                _logger.LogWarning("分類 {Name} 已存在於 {Type} 類型中", name, type);
                return false;
            }

            // 取得新的ID
            var maxId = categories.Any() ? categories.Max(c => c.Id) : 0;
            
            var newCategory = new AccountingCategory
            {
                Id = maxId + 1,
                Name = name,
                Type = type,
                Icon = icon,
                SubCategories = new List<AccountingSubCategory>()
            };

            categories.Add(newCategory);
            await SaveCategoriesAsync(categories);
            
            _logger.LogInformation("成功建立新分類 {Name} ({Type})", name, type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立分類 {Name} 時發生錯誤", name);
            return false;
        }
    }

    public async Task<bool> CreateSubCategoryAsync(string categoryName, string subCategoryName, string type)
    {
        try
        {
            var categories = await LoadCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Name == categoryName && c.Type == type);
            
            if (category == null)
            {
                _logger.LogWarning("找不到分類 {CategoryName} ({Type})", categoryName, type);
                return false;
            }

            // 檢查是否已存在相同名稱的子分類
            if (category.SubCategories.Any(sc => sc.Name == subCategoryName))
            {
                _logger.LogWarning("子分類 {SubCategoryName} 已存在於分類 {CategoryName} 中", subCategoryName, categoryName);
                return false;
            }

            // 取得新的子分類ID
            var maxSubId = category.SubCategories.Any() ? category.SubCategories.Max(sc => sc.Id) : 0;
            
            var newSubCategory = new AccountingSubCategory
            {
                Id = maxSubId + 1,
                Name = subCategoryName
            };

            category.SubCategories.Add(newSubCategory);
            await SaveCategoriesAsync(categories);
            
            _logger.LogInformation("成功建立新子分類 {SubCategoryName} 於分類 {CategoryName} ({Type})", subCategoryName, categoryName, type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立子分類 {SubCategoryName} 時發生錯誤", subCategoryName);
            return false;
        }
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, string type)
    {
        try
        {
            var categories = await LoadCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == categoryId && c.Type == type);
            
            if (category == null)
            {
                _logger.LogWarning("找不到要刪除的分類 ID: {CategoryId} ({Type})", categoryId, type);
                return false;
            }

            // 檢查是否有記錄使用此分類
            var records = await LoadRecordsAsync();
            if (records.Any(r => r.Category == category.Name && r.Type == type))
            {
                _logger.LogWarning("無法刪除分類 {CategoryName}，仍有記錄使用此分類", category.Name);
                return false;
            }

            categories.Remove(category);
            await SaveCategoriesAsync(categories);
            
            _logger.LogInformation("成功刪除分類 {CategoryName} ({Type})", category.Name, type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除分類 ID: {CategoryId} 時發生錯誤", categoryId);
            return false;
        }
    }

    public async Task<bool> DeleteSubCategoryAsync(int categoryId, int subCategoryId, string type)
    {
        try
        {
            var categories = await LoadCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == categoryId && c.Type == type);
            
            if (category == null)
            {
                _logger.LogWarning("找不到分類 ID: {CategoryId} ({Type})", categoryId, type);
                return false;
            }

            var subCategory = category.SubCategories.FirstOrDefault(sc => sc.Id == subCategoryId);
            if (subCategory == null)
            {
                _logger.LogWarning("找不到要刪除的子分類 ID: {SubCategoryId} 於分類 {CategoryName}", subCategoryId, category.Name);
                return false;
            }

            // 檢查是否有記錄使用此子分類
            var records = await LoadRecordsAsync();
            if (records.Any(r => r.Category == category.Name && r.SubCategory == subCategory.Name && r.Type == type))
            {
                _logger.LogWarning("無法刪除子分類 {SubCategoryName}，仍有記錄使用此子分類", subCategory.Name);
                return false;
            }

            category.SubCategories.Remove(subCategory);
            await SaveCategoriesAsync(categories);
            
            _logger.LogInformation("成功刪除子分類 {SubCategoryName} 於分類 {CategoryName} ({Type})", subCategory.Name, category.Name, type);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除子分類 ID: {SubCategoryId} 時發生錯誤", subCategoryId);
            return false;
        }
    }

    #region 私有方法

    private async Task<List<AccountingRecord>> LoadRecordsAsync()
    {
        try
        {
            if (!File.Exists(_recordsFilePath))
            {
                return new List<AccountingRecord>();
            }

            var json = await File.ReadAllTextAsync(_recordsFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<AccountingRecord>();
            }

            return JsonSerializer.Deserialize<List<AccountingRecord>>(json) ?? new List<AccountingRecord>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入記帳記錄檔案時發生錯誤");
            return new List<AccountingRecord>();
        }
    }

    private async Task SaveRecordsAsync(List<AccountingRecord> records)
    {
        try
        {
            var json = JsonSerializer.Serialize(records, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            await File.WriteAllTextAsync(_recordsFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存記帳記錄檔案時發生錯誤");
            throw;
        }
    }

    private async Task<List<AccountingCategory>> LoadCategoriesAsync()
    {
        try
        {
            if (!File.Exists(_categoriesFilePath))
            {
                return new List<AccountingCategory>();
            }

            var json = await File.ReadAllTextAsync(_categoriesFilePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<AccountingCategory>();
            }

            return JsonSerializer.Deserialize<List<AccountingCategory>>(json) ?? new List<AccountingCategory>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "載入分類檔案時發生錯誤");
            return new List<AccountingCategory>();
        }
    }

    private async Task SaveCategoriesAsync(List<AccountingCategory> categories)
    {
        try
        {
            var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            await File.WriteAllTextAsync(_categoriesFilePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "儲存分類檔案時發生錯誤");
            throw;
        }
    }

    private async Task InitializeDefaultDataAsync()
    {
        try
        {
            // 初始化預設分類
            var categories = await LoadCategoriesAsync();
            if (!categories.Any())
            {
                await CreateDefaultCategoriesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化預設資料時發生錯誤");
        }
    }

    private async Task CreateDefaultCategoriesAsync()
    {
        var defaultCategories = new List<AccountingCategory>
        {
            // 支出分類
            new AccountingCategory
            {
                Id = 1,
                Name = "餐飲食品",
                Type = "Expense",
                Icon = "fas fa-utensils",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 101, Name = "早餐" },
                    new() { Id = 102, Name = "午餐" },
                    new() { Id = 103, Name = "晚餐" },
                    new() { Id = 104, Name = "零食" },
                    new() { Id = 105, Name = "飲料" }
                }
            },
            new AccountingCategory
            {
                Id = 2,
                Name = "服飾美容",
                Type = "Expense",
                Icon = "fas fa-tshirt",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 201, Name = "上衣" },
                    new() { Id = 202, Name = "褲子" },
                    new() { Id = 203, Name = "外套" },
                    new() { Id = 204, Name = "鞋子" },
                    new() { Id = 205, Name = "包包" },
                    new() { Id = 206, Name = "配件" },
                    new() { Id = 207, Name = "美容保養" }
                }
            },
            new AccountingCategory
            {
                Id = 3,
                Name = "居家生活",
                Type = "Expense",
                Icon = "fas fa-home",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 301, Name = "家電用品" },
                    new() { Id = 302, Name = "房租" },
                    new() { Id = 303, Name = "管理費" },
                    new() { Id = 304, Name = "水電費" },
                    new() { Id = 305, Name = "瓦斯費" },
                    new() { Id = 306, Name = "網路費" },
                    new() { Id = 307, Name = "日用品" }
                }
            },
            new AccountingCategory
            {
                Id = 4,
                Name = "運輸交通",
                Type = "Expense",
                Icon = "fas fa-car",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 401, Name = "公共交通" },
                    new() { Id = 402, Name = "計程車" },
                    new() { Id = 403, Name = "共享運具" }
                }
            },
            new AccountingCategory
            {
                Id = 5,
                Name = "醫療保健",
                Type = "Expense",
                Icon = "fas fa-medkit",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 501, Name = "診所就醫" },
                    new() { Id = 502, Name = "健康檢查" },
                    new() { Id = 503, Name = "勞健保" },
                    new() { Id = 504, Name = "保健食品" },
                    new() { Id = 505, Name = "藥品" }
                }
            },
            // 收入分類
            new AccountingCategory
            {
                Id = 13,
                Name = "薪資收入",
                Type = "Income",
                Icon = "fas fa-money-bill-wave",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 1301, Name = "每月薪資" },
                    new() { Id = 1302, Name = "加班費" },
                    new() { Id = 1303, Name = "績效獎金" }
                }
            },
            new AccountingCategory
            {
                Id = 14,
                Name = "獎金收入",
                Type = "Income",
                Icon = "fas fa-trophy",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 1401, Name = "年終獎金" },
                    new() { Id = 1402, Name = "三節獎金" },
                    new() { Id = 1403, Name = "分紅" },
                    new() { Id = 1404, Name = "業績獎金" }
                }
            },
            new AccountingCategory
            {
                Id = 15,
                Name = "投資收益",
                Type = "Income",
                Icon = "fas fa-piggy-bank",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 1501, Name = "股利" },
                    new() { Id = 1502, Name = "利息" },
                    new() { Id = 1503, Name = "資本利得" },
                    new() { Id = 1504, Name = "租金收入" }
                }
            },
            new AccountingCategory
            {
                Id = 16,
                Name = "其他收入",
                Type = "Income",
                Icon = "fas fa-plus-circle",
                SubCategories = new List<AccountingSubCategory>
                {
                    new() { Id = 1601, Name = "兼職收入" },
                    new() { Id = 1602, Name = "禮金" },
                    new() { Id = 1603, Name = "退稅" },
                    new() { Id = 1604, Name = "中獎" }
                }
            }
        };

        await SaveCategoriesAsync(defaultCategories);
        _logger.LogInformation("成功建立預設分類資料");
    }

    #endregion
}
