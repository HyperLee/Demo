using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

/// <summary>
/// 投資交易記錄控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvestmentTransactionsController : ControllerBase
{
    private readonly InvestmentService _investmentService;

    public InvestmentTransactionsController(InvestmentService investmentService)
    {
        _investmentService = investmentService;
    }

    /// <summary>
    /// 取得交易記錄清單
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Transaction>>> GetTransactions(
        [FromQuery] int? portfolioId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? symbol = null,
        [FromQuery] string? type = null)
    {
        try
        {
            var transactions = await _investmentService.GetTransactionsAsync(portfolioId, startDate, endDate);
            
            // 額外篩選條件
            if (!string.IsNullOrEmpty(symbol))
                transactions = transactions.Where(t => t.Symbol.Contains(symbol, StringComparison.OrdinalIgnoreCase)).ToList();
            
            if (!string.IsNullOrEmpty(type))
                transactions = transactions.Where(t => t.Type.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();

            return Ok(transactions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得交易記錄失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 新增交易記錄
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Transaction>> CreateTransaction([FromBody] Transaction transaction)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdTransaction = await _investmentService.CreateTransactionAsync(transaction);
            return CreatedAtAction(nameof(GetTransactions), createdTransaction);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "建立交易記錄失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 刪除交易記錄
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTransaction(int id)
    {
        try
        {
            var success = await _investmentService.DeleteTransactionAsync(id);
            if (!success)
                return NotFound(new { message = "找不到指定的交易記錄" });

            return Ok(new { message = "交易記錄已刪除" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "刪除交易記錄失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 取得投資組合的交易統計
    /// </summary>
    [HttpGet("portfolio/{portfolioId}/stats")]
    public async Task<ActionResult> GetPortfolioTransactionStats(int portfolioId)
    {
        try
        {
            var transactions = await _investmentService.GetTransactionsAsync(portfolioId);
            
            var stats = new
            {
                TotalTransactions = transactions.Count,
                BuyCount = transactions.Count(t => t.Type == "買入"),
                SellCount = transactions.Count(t => t.Type == "賣出"),
                DividendCount = transactions.Count(t => t.Type == "股息"),
                TotalBuyAmount = transactions.Where(t => t.Type == "買入").Sum(t => t.TotalAmount),
                TotalSellAmount = transactions.Where(t => t.Type == "賣出").Sum(t => t.TotalAmount),
                TotalDividend = transactions.Where(t => t.Type == "股息").Sum(t => t.TotalAmount),
                TotalFees = transactions.Sum(t => t.Fee),
                RecentTransactions = transactions.Take(5).ToList()
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得交易統計失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 匯出交易記錄
    /// </summary>
    [HttpGet("export")]
    public async Task<ActionResult> ExportTransactions(
        [FromQuery] int? portfolioId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string format = "csv")
    {
        try
        {
            var transactions = await _investmentService.GetTransactionsAsync(portfolioId, startDate, endDate);
            
            if (format.ToLower() == "csv")
            {
                var csv = GenerateTransactionsCsv(transactions);
                var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                
                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
            }
            else if (format.ToLower() == "json")
            {
                var json = System.Text.Json.JsonSerializer.Serialize(transactions, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                var fileName = $"transactions_{DateTime.Now:yyyyMMdd_HHmmss}.json";
                
                return File(System.Text.Encoding.UTF8.GetBytes(json), "application/json", fileName);
            }
            
            return BadRequest(new { message = "不支援的匯出格式" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "匯出交易記錄失敗", error = ex.Message });
        }
    }

    #region 私有方法

    private string GenerateTransactionsCsv(List<Transaction> transactions)
    {
        var csvBuilder = new System.Text.StringBuilder();
        csvBuilder.AppendLine("日期,類型,股票代號,數量,價格,總金額,手續費,備註");
        
        foreach (var transaction in transactions)
        {
            csvBuilder.AppendLine($"{transaction.Date:yyyy-MM-dd},{transaction.Type},{transaction.Symbol},{transaction.Quantity},{transaction.Price},{transaction.TotalAmount},{transaction.Fee},\"{transaction.Note}\"");
        }
        
        return csvBuilder.ToString();
    }

    #endregion
}
