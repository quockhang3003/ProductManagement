using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;

namespace ProductManagement.WebUI.Controllers
{
    [ApiController]
[Route("api/promotions")]
[EnableRateLimiting("fixed")]
public class PromotionApiController : ControllerBase
{
    private readonly IPromotionService _promotionService;
    private readonly ILogger<PromotionApiController> _logger;

    public PromotionApiController(
        IPromotionService promotionService,
        ILogger<PromotionApiController> logger)
    {
        _promotionService = promotionService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new promotion
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("token")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromotionDto>> CreatePromotion(
        [FromBody] CreatePromotionDto dto)
    {
        try
        {
            var promotion = await _promotionService.CreatePromotionAsync(dto);
            
            return CreatedAtAction(
                nameof(GetPromotionById), 
                new { id = promotion.Id }, 
                promotion);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create promotion {Code}", dto.Code);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get promotion by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionDto>> GetPromotionById(Guid id)
    {
        var promotion = await _promotionService.GetPromotionByIdAsync(id);
        
        if (promotion == null)
        {
            return NotFound(new { error = "Promotion not found" });
        }

        return Ok(promotion);
    }

    /// <summary>
    /// Get promotion by code
    /// </summary>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionDto>> GetPromotionByCode(string code)
    {
        var promotion = await _promotionService.GetPromotionByCodeAsync(code);
        
        if (promotion == null)
        {
            return NotFound(new { error = $"Promotion code '{code}' not found" });
        }

        return Ok(promotion);
    }

    /// <summary>
    /// Get all active promotions
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<PromotionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromotionDto>>> GetActivePromotions()
    {
        var promotions = await _promotionService.GetActivePromotionsAsync();
        return Ok(promotions);
    }

    /// <summary>
    /// Activate a promotion
    /// </summary>
    [HttpPost("{id}/activate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivatePromotion(Guid id)
    {
        try
        {
            await _promotionService.ActivatePromotionAsync(id);
            return Ok(new { message = "Promotion activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to activate promotion {PromotionId}", id);
            return NotFound(new { error = "Promotion not found" });
        }
    }

    /// <summary>
    /// Deactivate a promotion
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivatePromotion(Guid id)
    {
        try
        {
            await _promotionService.DeactivatePromotionAsync(id);
            return Ok(new { message = "Promotion deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deactivate promotion {PromotionId}", id);
            return NotFound(new { error = "Promotion not found" });
        }
    }

    /// <summary>
    /// Calculate best promotions for an order (THE CORE FEATURE!)
    /// </summary>
    [HttpPost("calculate")]
    [ProducesResponseType(typeof(PromotionCalculationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromotionCalculationResult>> CalculateBestPromotions(
        [FromBody] CalculatePromotionRequest request)
    {
        try
        {
            var result = await _promotionService.CalculateBestPromotionsAsync(
                request.OrderTotal,
                request.ProductIds,
                request.CustomerEmail,
                request.CustomerSegment,
                request.CouponCode);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to calculate best promotions");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Validate a promotion code
    /// </summary>
    [HttpPost("validate/{code}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<bool>> ValidatePromotionCode(
        string code,
        [FromQuery] string? customerEmail = null)
    {
        var isValid = await _promotionService.ValidatePromotionCodeAsync(
            code, customerEmail);
        
        return Ok(new 
        { 
            code, 
            isValid, 
            message = isValid ? "Promotion code is valid" : "Promotion code is invalid or expired"
        });
    }

    /// <summary>
    /// Apply promotion to an order
    /// </summary>
    [HttpPost("apply")]
    [ProducesResponseType(typeof(PromotionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromotionDto>> ApplyPromotionToOrder(
        [FromBody] ApplyPromotionRequest request)
    {
        try
        {
            var promotion = await _promotionService.ApplyPromotionToOrderAsync(
                request.OrderId, request.PromotionCode);
            
            return Ok(promotion);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(
                ex, 
                "Failed to apply promotion {Code} to order {OrderId}", 
                request.PromotionCode, 
                request.OrderId);
            
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error applying promotion {Code} to order {OrderId}", 
                request.PromotionCode, 
                request.OrderId);
            
            return NotFound(new { error = "Promotion or order not found" });
        }
    }

    /// <summary>
    /// Get customer usage count for a promotion
    /// </summary>
    [HttpGet("{id}/usage/{customerEmail}")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    public async Task<ActionResult<int>> GetCustomerUsageCount(
        Guid id,
        string customerEmail)
    {
        var count = await _promotionService.GetCustomerUsageCountAsync(id, customerEmail);
        return Ok(new { promotionId = id, customerEmail, usageCount = count });
    }

    /// <summary>
    /// Get promotion usage history
    /// </summary>
    [HttpGet("{id}/usage")]
    [ProducesResponseType(typeof(IEnumerable<PromotionUsageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromotionUsageDto>>> GetPromotionUsageHistory(
        Guid id)
    {
        var usages = await _promotionService.GetPromotionUsageHistoryAsync(id);
        return Ok(usages);
    }

    /// <summary>
    /// Get promotion analytics
    /// </summary>
    [HttpGet("{id}/analytics")]
    [ProducesResponseType(typeof(PromotionAnalyticsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PromotionAnalyticsDto>> GetPromotionAnalytics(Guid id)
    {
        try
        {
            var analytics = await _promotionService.GetPromotionAnalyticsAsync(id);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get analytics for promotion {PromotionId}", id);
            return NotFound(new { error = "Promotion not found" });
        }
    }

    /// <summary>
    /// Get promotion effectiveness report
    /// </summary>
    [HttpGet("effectiveness")]
    [ProducesResponseType(typeof(IEnumerable<PromotionEffectivenessDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PromotionEffectivenessDto>>> 
        GetPromotionEffectivenessReport()
    {
        var report = await _promotionService.GetPromotionEffectivenessReportAsync();
        return Ok(report);
    }
}

// ============================================
// Request DTOs (add to DTOs file)
// ============================================

public record CalculatePromotionRequest(
    decimal OrderTotal,
    List<Guid> ProductIds,
    string? CustomerEmail = null,
    string? CustomerSegment = null,
    string? CouponCode = null
);

public record ApplyPromotionRequest(
    Guid OrderId,
    string PromotionCode
);
}
