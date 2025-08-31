using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

/// <summary>
/// 投資組合控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvestmentPortfolioController : ControllerBase
{
    private readonly InvestmentService _investmentService;
    private readonly StockPriceService _stockPriceService;

    public InvestmentPortfolioController(InvestmentService investmentService, StockPriceService stockPriceService)
    {
        _investmentService = investmentService;
        _stockPriceService = stockPriceService;
    }

    /// <summary>
    /// 取得所有投資組合
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Portfolio>>> GetPortfolios()
    {
        try
        {
            var portfolios = await _investmentService.GetPortfoliosAsync();
            return Ok(portfolios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得投資組合失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 取得指定投資組合
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Portfolio>> GetPortfolio(int id)
    {
        try
        {
            var portfolio = await _investmentService.GetPortfolioAsync(id);
            if (portfolio == null)
                return NotFound(new { message = "找不到指定的投資組合" });

            return Ok(portfolio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得投資組合失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 新增投資組合
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Portfolio>> CreatePortfolio([FromBody] Portfolio portfolio)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdPortfolio = await _investmentService.CreatePortfolioAsync(portfolio);
            return CreatedAtAction(nameof(GetPortfolio), new { id = createdPortfolio.Id }, createdPortfolio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "建立投資組合失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新投資組合
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Portfolio>> UpdatePortfolio(int id, [FromBody] Portfolio portfolio)
    {
        try
        {
            if (id != portfolio.Id)
                return BadRequest(new { message = "投資組合 ID 不符" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedPortfolio = await _investmentService.UpdatePortfolioAsync(portfolio);
            if (updatedPortfolio == null)
                return NotFound(new { message = "找不到指定的投資組合" });

            return Ok(updatedPortfolio);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新投資組合失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 刪除投資組合
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeletePortfolio(int id)
    {
        try
        {
            var success = await _investmentService.DeletePortfolioAsync(id);
            if (!success)
                return NotFound(new { message = "找不到指定的投資組合" });

            return Ok(new { message = "投資組合已刪除" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "刪除投資組合失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 取得投資分析資料
    /// </summary>
    [HttpGet("analysis")]
    public async Task<ActionResult<InvestmentAnalysis>> GetInvestmentAnalysis([FromQuery] int? portfolioId = null)
    {
        try
        {
            var analysis = await _investmentService.GetInvestmentAnalysisAsync(portfolioId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得投資分析失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 取得投資儀表板資料
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<InvestmentDashboard>> GetInvestmentDashboard()
    {
        try
        {
            var dashboard = await _investmentService.GetInvestmentDashboardAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得投資儀表板失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新所有股價
    /// </summary>
    [HttpPost("update-prices")]
    public async Task<ActionResult> UpdateStockPrices([FromBody] List<string>? symbols = null)
    {
        try
        {
            Dictionary<string, StockPrice> prices;
            
            if (symbols != null && symbols.Any())
            {
                prices = await _stockPriceService.GetStockPricesAsync(symbols);
            }
            else
            {
                await _investmentService.UpdateAllStockPricesAsync();
                return Ok(new { message = "所有股價已更新" });
            }

            return Ok(prices);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新股價失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 搜尋股票
    /// </summary>
    [HttpGet("search-stocks")]
    public async Task<ActionResult<List<StockSearchResult>>> SearchStocks([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return BadRequest(new { message = "搜尋關鍵字至少需要 2 個字元" });

            var results = await _stockPriceService.SearchStocksAsync(query);
            return Ok(results);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "搜尋股票失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 取得股票報價
    /// </summary>
    [HttpGet("stock-price/{symbol}")]
    public async Task<ActionResult<StockPrice>> GetStockPrice(string symbol)
    {
        try
        {
            var price = await _stockPriceService.GetSingleStockPriceAsync(symbol);
            if (price == null)
                return NotFound(new { message = "找不到指定股票的報價" });

            return Ok(price);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得股價失敗", error = ex.Message });
        }
    }
}
