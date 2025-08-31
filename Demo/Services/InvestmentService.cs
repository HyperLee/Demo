using Demo.Models;
using System.Text.Json;

namespace Demo.Services;

/// <summary>
/// 投資管理服務
/// </summary>
public class InvestmentService
{
    private readonly string _portfoliosPath;
    private readonly string _holdingsPath;
    private readonly string _transactionsPath;
    private readonly StockPriceService _stockPriceService;

    public InvestmentService(StockPriceService stockPriceService)
    {
        _stockPriceService = stockPriceService;
        var appDataPath = Path.Combine(Directory.GetCurrentDirectory(), "App_Data");
        _portfoliosPath = Path.Combine(appDataPath, "portfolios.json");
        _holdingsPath = Path.Combine(appDataPath, "holdings.json");
        _transactionsPath = Path.Combine(appDataPath, "transactions.json");
        
        // 確保目錄存在
        Directory.CreateDirectory(appDataPath);
        InitializeDataFiles();
    }

    #region 投資組合管理

    public async Task<List<Portfolio>> GetPortfoliosAsync()
    {
        if (!File.Exists(_portfoliosPath))
            return new List<Portfolio>();

        var json = await File.ReadAllTextAsync(_portfoliosPath);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<Portfolio>>>(json);
        return data?["portfolios"] ?? new List<Portfolio>();
    }

    public async Task<Portfolio?> GetPortfolioAsync(int id)
    {
        var portfolios = await GetPortfoliosAsync();
        return portfolios.FirstOrDefault(p => p.Id == id);
    }

    public async Task<Portfolio> CreatePortfolioAsync(Portfolio portfolio)
    {
        var portfolios = await GetPortfoliosAsync();
        portfolio.Id = portfolios.Any() ? portfolios.Max(p => p.Id) + 1 : 1;
        portfolio.CreatedAt = DateTime.Now;
        
        portfolios.Add(portfolio);
        await SavePortfoliosAsync(portfolios);
        
        return portfolio;
    }

    public async Task<Portfolio?> UpdatePortfolioAsync(Portfolio portfolio)
    {
        var portfolios = await GetPortfoliosAsync();
        var existingPortfolio = portfolios.FirstOrDefault(p => p.Id == portfolio.Id);
        
        if (existingPortfolio != null)
        {
            existingPortfolio.Name = portfolio.Name;
            existingPortfolio.Description = portfolio.Description;
            
            await SavePortfoliosAsync(portfolios);
            await UpdatePortfolioMetricsAsync(portfolio.Id);
            
            return existingPortfolio;
        }
        
        return null;
    }

    public async Task<bool> DeletePortfolioAsync(int id)
    {
        var portfolios = await GetPortfoliosAsync();
        var portfolio = portfolios.FirstOrDefault(p => p.Id == id);
        
        if (portfolio != null)
        {
            portfolios.Remove(portfolio);
            await SavePortfoliosAsync(portfolios);
            
            // 刪除相關的持倉和交易記錄
            await DeletePortfolioHoldingsAsync(id);
            await DeletePortfolioTransactionsAsync(id);
            
            return true;
        }
        
        return false;
    }

    #endregion

    #region 持倉管理

    public async Task<List<Holding>> GetHoldingsAsync(int? portfolioId = null)
    {
        if (!File.Exists(_holdingsPath))
            return new List<Holding>();

        var json = await File.ReadAllTextAsync(_holdingsPath);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<Holding>>>(json);
        var holdings = data?["holdings"] ?? new List<Holding>();
        
        if (portfolioId.HasValue)
            holdings = holdings.Where(h => h.PortfolioId == portfolioId.Value).ToList();
        
        return holdings;
    }

    public async Task<Holding?> GetHoldingAsync(int id)
    {
        var holdings = await GetHoldingsAsync();
        return holdings.FirstOrDefault(h => h.Id == id);
    }

    public async Task<Holding> CreateHoldingAsync(Holding holding)
    {
        var holdings = await GetHoldingsAsync();
        holding.Id = holdings.Any() ? holdings.Max(h => h.Id) + 1 : 1;
        
        // 計算市場價值和損益
        holding.MarketValue = holding.Quantity * holding.CurrentPrice;
        holding.GainLoss = holding.MarketValue - (holding.Quantity * holding.AverageCost);
        holding.GainLossPercentage = holding.AverageCost > 0 
            ? ((holding.CurrentPrice - holding.AverageCost) / holding.AverageCost) * 100 
            : 0;
        
        holdings.Add(holding);
        await SaveHoldingsAsync(holdings);
        
        // 更新投資組合統計
        await UpdatePortfolioMetricsAsync(holding.PortfolioId);
        
        return holding;
    }

    public async Task<Holding?> UpdateHoldingAsync(Holding holding)
    {
        var holdings = await GetHoldingsAsync();
        var existingHolding = holdings.FirstOrDefault(h => h.Id == holding.Id);
        
        if (existingHolding != null)
        {
            existingHolding.Symbol = holding.Symbol;
            existingHolding.Name = holding.Name;
            existingHolding.Type = holding.Type;
            existingHolding.Quantity = holding.Quantity;
            existingHolding.AverageCost = holding.AverageCost;
            existingHolding.CurrentPrice = holding.CurrentPrice;
            existingHolding.LastUpdated = DateTime.Now;
            
            // 重新計算市場價值和損益
            existingHolding.MarketValue = existingHolding.Quantity * existingHolding.CurrentPrice;
            existingHolding.GainLoss = existingHolding.MarketValue - (existingHolding.Quantity * existingHolding.AverageCost);
            existingHolding.GainLossPercentage = existingHolding.AverageCost > 0 
                ? ((existingHolding.CurrentPrice - existingHolding.AverageCost) / existingHolding.AverageCost) * 100 
                : 0;
            
            await SaveHoldingsAsync(holdings);
            await UpdatePortfolioMetricsAsync(existingHolding.PortfolioId);
            
            return existingHolding;
        }
        
        return null;
    }

    public async Task<bool> DeleteHoldingAsync(int id)
    {
        var holdings = await GetHoldingsAsync();
        var holding = holdings.FirstOrDefault(h => h.Id == id);
        
        if (holding != null)
        {
            holdings.Remove(holding);
            await SaveHoldingsAsync(holdings);
            await UpdatePortfolioMetricsAsync(holding.PortfolioId);
            
            return true;
        }
        
        return false;
    }

    #endregion

    #region 交易記錄管理

    public async Task<List<Transaction>> GetTransactionsAsync(int? portfolioId = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        if (!File.Exists(_transactionsPath))
            return new List<Transaction>();

        var json = await File.ReadAllTextAsync(_transactionsPath);
        var data = JsonSerializer.Deserialize<Dictionary<string, List<Transaction>>>(json);
        var transactions = data?["transactions"] ?? new List<Transaction>();
        
        if (portfolioId.HasValue)
            transactions = transactions.Where(t => t.PortfolioId == portfolioId.Value).ToList();
        
        if (startDate.HasValue)
            transactions = transactions.Where(t => t.Date >= startDate.Value).ToList();
            
        if (endDate.HasValue)
            transactions = transactions.Where(t => t.Date <= endDate.Value).ToList();
        
        return transactions.OrderByDescending(t => t.Date).ToList();
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        var transactions = await GetTransactionsAsync();
        transaction.Id = transactions.Any() ? transactions.Max(t => t.Id) + 1 : 1;
        transaction.CreatedAt = DateTime.Now;
        
        // 計算總金額
        transaction.TotalAmount = transaction.Quantity * transaction.Price;
        
        transactions.Add(transaction);
        await SaveTransactionsAsync(transactions);
        
        // 更新相關持倉
        await UpdateHoldingFromTransactionAsync(transaction);
        
        return transaction;
    }

    public async Task<bool> DeleteTransactionAsync(int id)
    {
        var transactions = await GetTransactionsAsync();
        var transaction = transactions.FirstOrDefault(t => t.Id == id);
        
        if (transaction != null)
        {
            transactions.Remove(transaction);
            await SaveTransactionsAsync(transactions);
            
            // 重新計算相關持倉
            await RecalculateHoldingFromTransactionsAsync(transaction.PortfolioId, transaction.Symbol);
            
            return true;
        }
        
        return false;
    }

    #endregion

    #region 分析與統計

    public async Task<InvestmentAnalysis> GetInvestmentAnalysisAsync(int? portfolioId = null)
    {
        var holdings = await GetHoldingsAsync(portfolioId);
        var analysis = new InvestmentAnalysis();
        
        analysis.TotalAssets = holdings.Sum(h => h.MarketValue);
        analysis.TotalCost = holdings.Sum(h => h.Quantity * h.AverageCost);
        analysis.TotalGainLoss = analysis.TotalAssets - analysis.TotalCost;
        analysis.TotalReturn = analysis.TotalCost > 0 ? (analysis.TotalGainLoss / analysis.TotalCost) * 100 : 0;
        analysis.HoldingsCount = holdings.Count;
        
        // 資產配置分析
        var typeGroups = holdings.GroupBy(h => h.Type);
        foreach (var group in typeGroups)
        {
            var value = group.Sum(h => h.MarketValue);
            analysis.AssetAllocations.Add(new AssetAllocation
            {
                Type = group.Key,
                Value = value,
                Percentage = analysis.TotalAssets > 0 ? (value / analysis.TotalAssets) * 100 : 0
            });
        }
        
        return analysis;
    }

    public async Task<InvestmentDashboard> GetInvestmentDashboardAsync()
    {
        var dashboard = new InvestmentDashboard();
        
        dashboard.Analysis = await GetInvestmentAnalysisAsync();
        dashboard.Portfolios = await GetPortfoliosAsync();
        dashboard.TopHoldings = (await GetHoldingsAsync()).OrderByDescending(h => h.MarketValue).Take(10).ToList();
        dashboard.RecentTransactions = (await GetTransactionsAsync()).Take(10).ToList();
        
        return dashboard;
    }

    #endregion

    #region 股價更新

    public async Task UpdateAllStockPricesAsync()
    {
        var holdings = await GetHoldingsAsync();
        var symbols = holdings.Select(h => h.Symbol).Distinct().ToList();
        
        var prices = await _stockPriceService.GetStockPricesAsync(symbols);
        
        foreach (var holding in holdings)
        {
            if (prices.ContainsKey(holding.Symbol))
            {
                var price = prices[holding.Symbol];
                holding.CurrentPrice = price.Price;
                holding.LastUpdated = price.LastUpdated;
                
                // 重新計算市場價值和損益
                holding.MarketValue = holding.Quantity * holding.CurrentPrice;
                holding.GainLoss = holding.MarketValue - (holding.Quantity * holding.AverageCost);
                holding.GainLossPercentage = holding.AverageCost > 0 
                    ? ((holding.CurrentPrice - holding.AverageCost) / holding.AverageCost) * 100 
                    : 0;
            }
        }
        
        await SaveHoldingsAsync(holdings);
        
        // 更新所有投資組合的統計
        var portfolios = await GetPortfoliosAsync();
        foreach (var portfolio in portfolios)
        {
            await UpdatePortfolioMetricsAsync(portfolio.Id);
        }
    }

    #endregion

    #region 私有方法

    private async Task SavePortfoliosAsync(List<Portfolio> portfolios)
    {
        var data = new Dictionary<string, List<Portfolio>> { ["portfolios"] = portfolios };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_portfoliosPath, json);
    }

    private async Task SaveHoldingsAsync(List<Holding> holdings)
    {
        var data = new Dictionary<string, List<Holding>> { ["holdings"] = holdings };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_holdingsPath, json);
    }

    private async Task SaveTransactionsAsync(List<Transaction> transactions)
    {
        var data = new Dictionary<string, List<Transaction>> { ["transactions"] = transactions };
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(_transactionsPath, json);
    }

    private async Task UpdatePortfolioMetricsAsync(int portfolioId)
    {
        var portfolios = await GetPortfoliosAsync();
        var portfolio = portfolios.FirstOrDefault(p => p.Id == portfolioId);
        
        if (portfolio != null)
        {
            var holdings = await GetHoldingsAsync(portfolioId);
            
            portfolio.TotalValue = holdings.Sum(h => h.MarketValue);
            portfolio.TotalCost = holdings.Sum(h => h.Quantity * h.AverageCost);
            portfolio.TotalGainLoss = portfolio.TotalValue - portfolio.TotalCost;
            portfolio.TotalGainLossPercentage = portfolio.TotalCost > 0 
                ? (portfolio.TotalGainLoss / portfolio.TotalCost) * 100 
                : 0;
            
            await SavePortfoliosAsync(portfolios);
        }
    }

    private async Task DeletePortfolioHoldingsAsync(int portfolioId)
    {
        var holdings = await GetHoldingsAsync();
        holdings.RemoveAll(h => h.PortfolioId == portfolioId);
        await SaveHoldingsAsync(holdings);
    }

    private async Task DeletePortfolioTransactionsAsync(int portfolioId)
    {
        var transactions = await GetTransactionsAsync();
        transactions.RemoveAll(t => t.PortfolioId == portfolioId);
        await SaveTransactionsAsync(transactions);
    }

    private async Task UpdateHoldingFromTransactionAsync(Transaction transaction)
    {
        var holdings = await GetHoldingsAsync(transaction.PortfolioId);
        var holding = holdings.FirstOrDefault(h => h.Symbol == transaction.Symbol);
        
        if (holding != null)
        {
            if (transaction.Type == "買入")
            {
                var totalCost = (holding.Quantity * holding.AverageCost) + (transaction.Quantity * transaction.Price);
                holding.Quantity += transaction.Quantity;
                holding.AverageCost = holding.Quantity > 0 ? totalCost / holding.Quantity : 0;
            }
            else if (transaction.Type == "賣出")
            {
                holding.Quantity -= transaction.Quantity;
                if (holding.Quantity <= 0)
                {
                    holdings.Remove(holding);
                }
            }
            
            if (holding.Quantity > 0)
            {
                holding.MarketValue = holding.Quantity * holding.CurrentPrice;
                holding.GainLoss = holding.MarketValue - (holding.Quantity * holding.AverageCost);
                holding.GainLossPercentage = holding.AverageCost > 0 
                    ? ((holding.CurrentPrice - holding.AverageCost) / holding.AverageCost) * 100 
                    : 0;
            }
            
            await SaveHoldingsAsync(holdings);
            await UpdatePortfolioMetricsAsync(transaction.PortfolioId);
        }
    }

    private async Task RecalculateHoldingFromTransactionsAsync(int portfolioId, string symbol)
    {
        var transactions = await GetTransactionsAsync(portfolioId);
        var symbolTransactions = transactions.Where(t => t.Symbol == symbol).OrderBy(t => t.Date).ToList();
        
        var holdings = await GetHoldingsAsync(portfolioId);
        var holding = holdings.FirstOrDefault(h => h.Symbol == symbol);
        
        if (holding != null && symbolTransactions.Any())
        {
            // 重新計算平均成本和數量
            int totalQuantity = 0;
            decimal totalCost = 0;
            
            foreach (var tx in symbolTransactions)
            {
                if (tx.Type == "買入")
                {
                    totalQuantity += tx.Quantity;
                    totalCost += tx.Quantity * tx.Price;
                }
                else if (tx.Type == "賣出")
                {
                    totalQuantity -= tx.Quantity;
                }
            }
            
            holding.Quantity = totalQuantity;
            holding.AverageCost = totalQuantity > 0 ? totalCost / totalQuantity : 0;
            
            holding.MarketValue = holding.Quantity * holding.CurrentPrice;
            holding.GainLoss = holding.MarketValue - (holding.Quantity * holding.AverageCost);
            holding.GainLossPercentage = holding.AverageCost > 0 
                ? ((holding.CurrentPrice - holding.AverageCost) / holding.AverageCost) * 100 
                : 0;
            
            await SaveHoldingsAsync(holdings);
            await UpdatePortfolioMetricsAsync(portfolioId);
        }
    }

    private void InitializeDataFiles()
    {
        if (!File.Exists(_portfoliosPath))
        {
            var portfolios = new Dictionary<string, List<Portfolio>> { ["portfolios"] = new List<Portfolio>() };
            var json = JsonSerializer.Serialize(portfolios, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_portfoliosPath, json);
        }
        
        if (!File.Exists(_holdingsPath))
        {
            var holdings = new Dictionary<string, List<Holding>> { ["holdings"] = new List<Holding>() };
            var json = JsonSerializer.Serialize(holdings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_holdingsPath, json);
        }
        
        if (!File.Exists(_transactionsPath))
        {
            var transactions = new Dictionary<string, List<Transaction>> { ["transactions"] = new List<Transaction>() };
            var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_transactionsPath, json);
        }
    }

    #endregion
}
