using System.ComponentModel.DataAnnotations;

namespace Demo.Models;

/// <summary>
/// 投資組合模型
/// </summary>
public class Portfolio
{
    public int Id { get; set; }
    
    [Required(ErrorMessage = "組合名稱不能為空")]
    [StringLength(100, ErrorMessage = "組合名稱不能超過100個字元")]
    public string Name { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "描述不能超過500個字元")]
    public string Description { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    
    public decimal TotalValue { get; set; }
    
    public decimal TotalCost { get; set; }
    
    public decimal TotalGainLoss { get; set; }
    
    public decimal TotalGainLossPercentage { get; set; }
}

/// <summary>
/// 投資持倉模型
/// </summary>
public class Holding
{
    public int Id { get; set; }
    
    public int PortfolioId { get; set; }
    
    [Required(ErrorMessage = "股票代號不能為空")]
    [StringLength(20, ErrorMessage = "股票代號不能超過20個字元")]
    public string Symbol { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "股票名稱不能為空")]
    [StringLength(100, ErrorMessage = "股票名稱不能超過100個字元")]
    public string Name { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "投資類型不能為空")]
    public string Type { get; set; } = string.Empty;
    
    [Range(0, double.MaxValue, ErrorMessage = "持股數量必須大於0")]
    public int Quantity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "平均成本必須大於0")]
    public decimal AverageCost { get; set; }
    
    public decimal CurrentPrice { get; set; }
    
    public decimal MarketValue { get; set; }
    
    public decimal GainLoss { get; set; }
    
    public decimal GainLossPercentage { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

/// <summary>
/// 交易記錄模型
/// </summary>
public class Transaction
{
    public int Id { get; set; }
    
    public int PortfolioId { get; set; }
    
    [Required(ErrorMessage = "股票代號不能為空")]
    [StringLength(20, ErrorMessage = "股票代號不能超過20個字元")]
    public string Symbol { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "交易類型不能為空")]
    public string Type { get; set; } = string.Empty; // 買入、賣出、股息
    
    [Range(1, int.MaxValue, ErrorMessage = "數量必須大於0")]
    public int Quantity { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "價格必須大於0")]
    public decimal Price { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    [Range(0, double.MaxValue, ErrorMessage = "手續費不能為負數")]
    public decimal Fee { get; set; }
    
    public DateTime Date { get; set; } = DateTime.Today;
    
    [StringLength(500, ErrorMessage = "備註不能超過500個字元")]
    public string Note { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

/// <summary>
/// 股價資訊模型
/// </summary>
public class StockPrice
{
    public string Symbol { get; set; } = string.Empty;
    
    public decimal Price { get; set; }
    
    public decimal Change { get; set; }
    
    public string ChangePercent { get; set; } = string.Empty;
    
    public decimal Volume { get; set; }
    
    public DateTime LastUpdated { get; set; } = DateTime.Now;
}

/// <summary>
/// 投資分析模型
/// </summary>
public class InvestmentAnalysis
{
    public decimal TotalAssets { get; set; }
    
    public decimal TotalCost { get; set; }
    
    public decimal TotalGainLoss { get; set; }
    
    public decimal TotalReturn { get; set; }
    
    public int HoldingsCount { get; set; }
    
    public List<AssetAllocation> AssetAllocations { get; set; } = new();
    
    public List<PortfolioPerformance> PerformanceHistory { get; set; } = new();
}

/// <summary>
/// 資產配置模型
/// </summary>
public class AssetAllocation
{
    public string Type { get; set; } = string.Empty;
    
    public decimal Value { get; set; }
    
    public decimal Percentage { get; set; }
}

/// <summary>
/// 投資組合績效歷史
/// </summary>
public class PortfolioPerformance
{
    public DateTime Date { get; set; }
    
    public decimal TotalValue { get; set; }
    
    public decimal DailyReturn { get; set; }
}

/// <summary>
/// 股票查詢結果模型
/// </summary>
public class StockSearchResult
{
    public string Symbol { get; set; } = string.Empty;
    
    public string Name { get; set; } = string.Empty;
    
    public string Exchange { get; set; } = string.Empty;
    
    public string Currency { get; set; } = string.Empty;
}

/// <summary>
/// 投資儀表板資料模型
/// </summary>
public class InvestmentDashboard
{
    public InvestmentAnalysis Analysis { get; set; } = new();
    
    public List<Portfolio> Portfolios { get; set; } = new();
    
    public List<Holding> TopHoldings { get; set; } = new();
    
    public List<Transaction> RecentTransactions { get; set; } = new();
    
    public List<StockPrice> WatchList { get; set; } = new();
}

/// <summary>
/// 投資報告模型
/// </summary>
public class InvestmentReport
{
    public string ReportType { get; set; } = string.Empty;
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    public InvestmentAnalysis Summary { get; set; } = new();
    
    public List<Transaction> Transactions { get; set; } = new();
    
    public List<Holding> Holdings { get; set; } = new();
    
    public Dictionary<string, decimal> MonthlyReturns { get; set; } = new();
}
