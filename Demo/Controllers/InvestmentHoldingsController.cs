using Demo.Models;
using Demo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Demo.Controllers;

/// <summary>
/// 投資持倉控制器
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class InvestmentHoldingsController : ControllerBase
{
    private readonly InvestmentService _investmentService;

    public InvestmentHoldingsController(InvestmentService investmentService)
    {
        _investmentService = investmentService;
    }

    /// <summary>
    /// 取得持倉清單
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Holding>>> GetHoldings([FromQuery] int? portfolioId = null)
    {
        try
        {
            var holdings = await _investmentService.GetHoldingsAsync(portfolioId);
            return Ok(holdings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得持倉清單失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 取得指定持倉
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Holding>> GetHolding(int id)
    {
        try
        {
            var holding = await _investmentService.GetHoldingAsync(id);
            if (holding == null)
                return NotFound(new { message = "找不到指定的持倉" });

            return Ok(holding);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "取得持倉失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 新增持倉
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Holding>> CreateHolding([FromBody] Holding holding)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var createdHolding = await _investmentService.CreateHoldingAsync(holding);
            return CreatedAtAction(nameof(GetHolding), new { id = createdHolding.Id }, createdHolding);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "建立持倉失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 更新持倉
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<Holding>> UpdateHolding(int id, [FromBody] Holding holding)
    {
        try
        {
            if (id != holding.Id)
                return BadRequest(new { message = "持倉 ID 不符" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var updatedHolding = await _investmentService.UpdateHoldingAsync(holding);
            if (updatedHolding == null)
                return NotFound(new { message = "找不到指定的持倉" });

            return Ok(updatedHolding);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "更新持倉失敗", error = ex.Message });
        }
    }

    /// <summary>
    /// 刪除持倉
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteHolding(int id)
    {
        try
        {
            var success = await _investmentService.DeleteHoldingAsync(id);
            if (!success)
                return NotFound(new { message = "找不到指定的持倉" });

            return Ok(new { message = "持倉已刪除" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "刪除持倉失敗", error = ex.Message });
        }
    }
}
