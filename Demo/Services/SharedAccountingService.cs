using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 共享記帳服務介面
/// </summary>
public interface ISharedAccountingService
{
    Task<List<SharedAccountingRecord>> GetRecordsAsync(string familyId, DateTime? startDate = null, DateTime? endDate = null);
    Task<SharedAccountingRecord?> GetRecordByIdAsync(string recordId);
    Task<string> CreateRecordAsync(SharedAccountingRecord record);
    Task<bool> UpdateRecordAsync(SharedAccountingRecord record);
    Task<bool> DeleteRecordAsync(string recordId, string userId);
    
    // 分攤相關
    Task<Dictionary<string, decimal>> CalculateSplitAmountsAsync(decimal totalAmount, string splitType, string familyId, Dictionary<string, decimal>? customSplit = null);
    Task<List<SharedAccountingRecord>> GetMemberRecordsAsync(string familyId, string userId, DateTime? startDate = null, DateTime? endDate = null);
    
    // 審核相關
    Task<List<SharedAccountingRecord>> GetPendingApprovalsAsync(string familyId);
    Task<bool> ApproveRecordAsync(string recordId, string approvedBy);
    Task<bool> RejectRecordAsync(string recordId, string rejectedBy, string reason);
    
    // 統計相關
    Task<FamilyStatistics> GetFamilyStatisticsAsync(string familyId, int year, int month);
    Task<Dictionary<string, decimal>> GetCategoryExpensesAsync(string familyId, DateTime startDate, DateTime endDate);
    Task<Dictionary<string, decimal>> GetMemberExpensesAsync(string familyId, DateTime startDate, DateTime endDate);
    
    // 預算相關
    Task<List<SharedBudget>> GetBudgetsAsync(string familyId);
    Task<SharedBudget?> GetCurrentBudgetAsync(string familyId);
    Task<string> CreateBudgetAsync(SharedBudget budget);
    Task<bool> UpdateBudgetAsync(SharedBudget budget);
    Task<bool> DeleteBudgetAsync(string budgetId);
}

/// <summary>
/// 共享記帳服務實作
/// </summary>
public class SharedAccountingService : ISharedAccountingService
{
    private readonly ILogger<SharedAccountingService> _logger;
    private readonly IFamilyService _familyService;
    private readonly string _recordsFilePath;
    private readonly string _budgetsFilePath;

    public SharedAccountingService(
        ILogger<SharedAccountingService> logger,
        IFamilyService familyService)
    {
        _logger = logger;
        _familyService = familyService;
        
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }

        _recordsFilePath = Path.Combine(appDataPath, "shared-accounting-records.json");
        _budgetsFilePath = Path.Combine(appDataPath, "shared-budgets.json");
        
        // 初始化預設資料
        _ = Task.Run(InitializeDefaultDataAsync);
    }

    #region 記帳記錄管理

    public async Task<List<SharedAccountingRecord>> GetRecordsAsync(string familyId, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var familyRecords = records.Where(r => r.FamilyId == familyId).ToList();
            
            if (startDate.HasValue)
            {
                familyRecords = familyRecords.Where(r => r.Date >= startDate.Value).ToList();
            }
            
            if (endDate.HasValue)
            {
                familyRecords = familyRecords.Where(r => r.Date <= endDate.Value).ToList();
            }
            
            return familyRecords.OrderByDescending(r => r.Date).ThenByDescending(r => r.CreatedAt).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭 {FamilyId} 記帳記錄時發生錯誤", familyId);
            return new List<SharedAccountingRecord>();
        }
    }

    public async Task<SharedAccountingRecord?> GetRecordByIdAsync(string recordId)
    {
        try
        {
            var records = await LoadRecordsAsync();
            return records.FirstOrDefault(r => r.Id == recordId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得記帳記錄 {RecordId} 時發生錯誤", recordId);
            return null;
        }
    }

    public async Task<string> CreateRecordAsync(SharedAccountingRecord record)
    {
        try
        {
            // 驗證家庭群組是否存在
            var family = await _familyService.GetFamilyByIdAsync(record.FamilyId);
            if (family == null)
            {
                throw new InvalidOperationException("家庭群組不存在");
            }

            // 驗證記錄者是否為家庭成員
            var member = await _familyService.GetMemberAsync(record.FamilyId, record.UserId);
            if (member == null || !member.IsActive)
            {
                throw new InvalidOperationException("不是家庭成員或成員已停用");
            }

            // 檢查權限
            if (!member.Permissions.CanAddExpense)
            {
                throw new UnauthorizedAccessException("沒有新增支出的權限");
            }

            var records = await LoadRecordsAsync();
            
            // 生成新 ID
            record.Id = $"record_{Guid.NewGuid():N}";
            record.UserNickname = member.Nickname;
            record.CreatedAt = DateTime.UtcNow;
            record.LastModifiedAt = DateTime.UtcNow;
            record.LastModifiedBy = record.UserId;

            // 檢查是否需要審核
            if (record.Type == "支出" && 
                family.Settings.RequireApprovalForLargeExpense && 
                record.Amount >= family.Settings.LargeExpenseThreshold)
            {
                record.NeedsApproval = true;
                record.Status = "待審核";
            }

            // 處理分攤
            if (record.SplitDetails == null || !record.SplitDetails.Any())
            {
                record.SplitDetails = await CalculateSplitAmountsAsync(
                    record.Amount, 
                    record.SplitType, 
                    record.FamilyId);
            }

            records.Add(record);
            await SaveRecordsAsync(records);

            _logger.LogInformation("建立共享記帳記錄: {RecordId}, {FamilyId}, {Amount}", 
                record.Id, record.FamilyId, record.Amount);
            return record.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立共享記帳記錄時發生錯誤");
            throw;
        }
    }

    public async Task<bool> UpdateRecordAsync(SharedAccountingRecord record)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var existingRecord = records.FirstOrDefault(r => r.Id == record.Id);
            
            if (existingRecord == null)
                return false;

            // 檢查權限
            var member = await _familyService.GetMemberAsync(record.FamilyId, record.LastModifiedBy);
            if (member == null || (!member.Permissions.CanEditExpense && existingRecord.UserId != record.LastModifiedBy))
            {
                throw new UnauthorizedAccessException("沒有編輯此記錄的權限");
            }

            // 更新記錄
            var index = records.IndexOf(existingRecord);
            record.LastModifiedAt = DateTime.UtcNow;
            records[index] = record;

            await SaveRecordsAsync(records);

            _logger.LogInformation("更新共享記帳記錄: {RecordId}", record.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新共享記帳記錄 {RecordId} 時發生錯誤", record.Id);
            throw;
        }
    }

    public async Task<bool> DeleteRecordAsync(string recordId, string userId)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var record = records.FirstOrDefault(r => r.Id == recordId);
            
            if (record == null)
                return false;

            // 檢查權限
            var member = await _familyService.GetMemberAsync(record.FamilyId, userId);
            if (member == null || (!member.Permissions.CanDeleteExpense && record.UserId != userId))
            {
                throw new UnauthorizedAccessException("沒有刪除此記錄的權限");
            }

            records.Remove(record);
            await SaveRecordsAsync(records);

            _logger.LogInformation("刪除共享記帳記錄: {RecordId}", recordId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除共享記帳記錄 {RecordId} 時發生錯誤", recordId);
            throw;
        }
    }

    #endregion

    #region 分攤相關

    public async Task<Dictionary<string, decimal>> CalculateSplitAmountsAsync(
        decimal totalAmount, 
        string splitType, 
        string familyId, 
        Dictionary<string, decimal>? customSplit = null)
    {
        try
        {
            var splitDetails = new Dictionary<string, decimal>();
            var members = await _familyService.GetFamilyMembersAsync(familyId);
            var activeMembers = members.Where(m => m.IsActive).ToList();

            switch (splitType.ToLower())
            {
                case "平均分攤":
                    var equalAmount = totalAmount / activeMembers.Count;
                    foreach (var member in activeMembers)
                    {
                        splitDetails[member.UserId] = equalAmount;
                    }
                    break;

                case "自訂分攤":
                    if (customSplit != null)
                    {
                        splitDetails = customSplit;
                    }
                    break;

                case "我支付":
                default:
                    // 預設為記錄者支付全額，但這裡無法確定記錄者，所以返回空字典
                    // 實際使用時應該在調用前設定
                    break;
            }

            return splitDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "計算分攤金額時發生錯誤");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<List<SharedAccountingRecord>> GetMemberRecordsAsync(
        string familyId, 
        string userId, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        try
        {
            var allRecords = await GetRecordsAsync(familyId, startDate, endDate);
            return allRecords.Where(r => r.UserId == userId || r.SplitDetails.ContainsKey(userId)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得成員 {UserId} 記帳記錄時發生錯誤", userId);
            return new List<SharedAccountingRecord>();
        }
    }

    #endregion

    #region 審核相關

    public async Task<List<SharedAccountingRecord>> GetPendingApprovalsAsync(string familyId)
    {
        try
        {
            var records = await LoadRecordsAsync();
            return records.Where(r => r.FamilyId == familyId && r.Status == "待審核").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得待審核記錄時發生錯誤");
            return new List<SharedAccountingRecord>();
        }
    }

    public async Task<bool> ApproveRecordAsync(string recordId, string approvedBy)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var record = records.FirstOrDefault(r => r.Id == recordId);
            
            if (record == null || record.Status != "待審核")
                return false;

            // 檢查審核者權限
            var member = await _familyService.GetMemberAsync(record.FamilyId, approvedBy);
            if (member == null || member.Role != "admin")
            {
                throw new UnauthorizedAccessException("沒有審核權限");
            }

            record.Status = "已確認";
            record.ApprovedBy.Add(approvedBy);
            record.LastModifiedAt = DateTime.UtcNow;
            record.LastModifiedBy = approvedBy;

            await SaveRecordsAsync(records);

            _logger.LogInformation("審核通過記錄: {RecordId}, 審核者: {ApprovedBy}", recordId, approvedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "審核記錄 {RecordId} 時發生錯誤", recordId);
            throw;
        }
    }

    public async Task<bool> RejectRecordAsync(string recordId, string rejectedBy, string reason)
    {
        try
        {
            var records = await LoadRecordsAsync();
            var record = records.FirstOrDefault(r => r.Id == recordId);
            
            if (record == null || record.Status != "待審核")
                return false;

            // 檢查審核者權限
            var member = await _familyService.GetMemberAsync(record.FamilyId, rejectedBy);
            if (member == null || member.Role != "admin")
            {
                throw new UnauthorizedAccessException("沒有審核權限");
            }

            record.Status = "已拒絕";
            record.Description += $" (拒絕原因: {reason})";
            record.LastModifiedAt = DateTime.UtcNow;
            record.LastModifiedBy = rejectedBy;

            await SaveRecordsAsync(records);

            _logger.LogInformation("拒絕記錄: {RecordId}, 審核者: {RejectedBy}", recordId, rejectedBy);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拒絕記錄 {RecordId} 時發生錯誤", recordId);
            throw;
        }
    }

    #endregion

    #region 統計相關

    public async Task<FamilyStatistics> GetFamilyStatisticsAsync(string familyId, int year, int month)
    {
        try
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var records = await GetRecordsAsync(familyId, startDate, endDate);
            var confirmedRecords = records.Where(r => r.Status == "已確認").ToList();

            var statistics = new FamilyStatistics
            {
                FamilyId = familyId,
                Period = $"{year}-{month:D2}",
                TotalIncome = confirmedRecords.Where(r => r.Type == "收入").Sum(r => r.Amount),
                TotalExpense = confirmedRecords.Where(r => r.Type == "支出").Sum(r => r.Amount),
                CategoryExpenses = await GetCategoryExpensesAsync(familyId, startDate, endDate),
                MemberExpenses = await GetMemberExpensesAsync(familyId, startDate, endDate),
                PendingApprovals = records.Count(r => r.Status == "待審核")
            };

            // 計算預算使用率
            var currentBudget = await GetCurrentBudgetAsync(familyId);
            if (currentBudget != null && currentBudget.TotalBudget > 0)
            {
                statistics.BudgetUsage = (statistics.TotalExpense / currentBudget.TotalBudget) * 100;
            }

            return statistics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭統計時發生錯誤");
            return new FamilyStatistics { FamilyId = familyId };
        }
    }

    public async Task<Dictionary<string, decimal>> GetCategoryExpensesAsync(string familyId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await GetRecordsAsync(familyId, startDate, endDate);
            return records
                .Where(r => r.Type == "支出" && r.Status == "已確認")
                .GroupBy(r => r.Category)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得類別支出統計時發生錯誤");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<Dictionary<string, decimal>> GetMemberExpensesAsync(string familyId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var records = await GetRecordsAsync(familyId, startDate, endDate);
            return records
                .Where(r => r.Type == "支出" && r.Status == "已確認")
                .GroupBy(r => r.UserNickname)
                .ToDictionary(g => g.Key, g => g.Sum(r => r.Amount));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得成員支出統計時發生錯誤");
            return new Dictionary<string, decimal>();
        }
    }

    #endregion

    #region 預算相關

    public async Task<List<SharedBudget>> GetBudgetsAsync(string familyId)
    {
        try
        {
            var budgets = await LoadBudgetsAsync();
            return budgets.Where(b => b.FamilyId == familyId).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得家庭預算清單時發生錯誤");
            return new List<SharedBudget>();
        }
    }

    public async Task<SharedBudget?> GetCurrentBudgetAsync(string familyId)
    {
        try
        {
            var budgets = await GetBudgetsAsync(familyId);
            var currentDate = DateTime.Now;
            
            return budgets.FirstOrDefault(b => 
                b.Year == currentDate.Year && 
                (b.Period == "yearly" || b.Month == currentDate.Month));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取得當前預算時發生錯誤");
            return null;
        }
    }

    public async Task<string> CreateBudgetAsync(SharedBudget budget)
    {
        try
        {
            var budgets = await LoadBudgetsAsync();
            
            budget.Id = $"budget_{Guid.NewGuid():N}";
            budget.CreatedAt = DateTime.UtcNow;
            
            // 計算總預算
            budget.TotalBudget = budget.Categories.Values.Sum(c => c.Budget);
            
            budgets.Add(budget);
            await SaveBudgetsAsync(budgets);

            _logger.LogInformation("建立共享預算: {BudgetId}", budget.Id);
            return budget.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "建立共享預算時發生錯誤");
            throw;
        }
    }

    public async Task<bool> UpdateBudgetAsync(SharedBudget budget)
    {
        try
        {
            var budgets = await LoadBudgetsAsync();
            var index = budgets.FindIndex(b => b.Id == budget.Id);
            
            if (index == -1)
                return false;

            // 重新計算總預算
            budget.TotalBudget = budget.Categories.Values.Sum(c => c.Budget);
            
            budgets[index] = budget;
            await SaveBudgetsAsync(budgets);

            _logger.LogInformation("更新共享預算: {BudgetId}", budget.Id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新共享預算時發生錯誤");
            return false;
        }
    }

    public async Task<bool> DeleteBudgetAsync(string budgetId)
    {
        try
        {
            var budgets = await LoadBudgetsAsync();
            var budget = budgets.FirstOrDefault(b => b.Id == budgetId);
            
            if (budget == null)
                return false;

            budgets.Remove(budget);
            await SaveBudgetsAsync(budgets);

            _logger.LogInformation("刪除共享預算: {BudgetId}", budgetId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刪除共享預算時發生錯誤");
            return false;
        }
    }

    #endregion

    #region 私有方法

    private async Task<List<SharedAccountingRecord>> LoadRecordsAsync()
    {
        if (!File.Exists(_recordsFilePath))
            return new List<SharedAccountingRecord>();

        var json = await File.ReadAllTextAsync(_recordsFilePath);
        var data = JsonSerializer.Deserialize<SharedRecordsData>(json);
        return data?.SharedRecords ?? new List<SharedAccountingRecord>();
    }

    private async Task SaveRecordsAsync(List<SharedAccountingRecord> records)
    {
        var data = new SharedRecordsData { SharedRecords = records };
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true, 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
        };
        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(_recordsFilePath, json);
    }

    private async Task<List<SharedBudget>> LoadBudgetsAsync()
    {
        if (!File.Exists(_budgetsFilePath))
            return new List<SharedBudget>();

        var json = await File.ReadAllTextAsync(_budgetsFilePath);
        var data = JsonSerializer.Deserialize<SharedBudgetsData>(json);
        return data?.SharedBudgets ?? new List<SharedBudget>();
    }

    private async Task SaveBudgetsAsync(List<SharedBudget> budgets)
    {
        var data = new SharedBudgetsData { SharedBudgets = budgets };
        var options = new JsonSerializerOptions 
        { 
            WriteIndented = true, 
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping 
        };
        var json = JsonSerializer.Serialize(data, options);
        await File.WriteAllTextAsync(_budgetsFilePath, json);
    }

    private async Task InitializeDefaultDataAsync()
    {
        try
        {
            await LoadRecordsAsync();
            await LoadBudgetsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "初始化共享記帳服務預設資料時發生錯誤");
        }
    }

    #endregion
}

/// <summary>
/// 共享記帳記錄資料容器
/// </summary>
internal class SharedRecordsData
{
    public List<SharedAccountingRecord> SharedRecords { get; set; } = new();
}

/// <summary>
/// 共享預算資料容器
/// </summary>
internal class SharedBudgetsData
{
    public List<SharedBudget> SharedBudgets { get; set; } = new();
}
