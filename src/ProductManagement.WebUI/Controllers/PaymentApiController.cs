using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;

namespace ProductManagement.WebUI.Controllers
{
    [ApiController]
    [Route("api/payments")]
    [EnableRateLimiting("fixed")]
    public class PaymentApiController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentApiController> _logger;

    public PaymentApiController(
        IPaymentService paymentService,
        ILogger<PaymentApiController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    /// <summary>
    /// Create a new payment for an order
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("token")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> CreatePayment(
        [FromBody] CreatePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.CreatePaymentAsync(dto);
            
            return CreatedAtAction(
                nameof(GetPaymentById), 
                new { id = payment.Id }, 
                payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to create payment for order {OrderId}", dto.OrderId);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Authorize a pending payment
    /// </summary>
    [HttpPost("{id}/authorize")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> AuthorizePayment(
        Guid id,
        [FromBody] AuthorizePaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.AuthorizePaymentAsync(id, dto);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to authorize payment {PaymentId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error authorizing payment {PaymentId}", id);
            return NotFound(new { error = "Payment not found" });
        }
    }

    /// <summary>
    /// Capture an authorized payment
    /// </summary>
    [HttpPost("{id}/capture")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> CapturePayment(Guid id)
    {
        try
        {
            var payment = await _paymentService.CapturePaymentAsync(id);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to capture payment {PaymentId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing payment {PaymentId}", id);
            return NotFound(new { error = "Payment not found" });
        }
    }

    /// <summary>
    /// Retry a failed payment
    /// </summary>
    [HttpPost("{id}/retry")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RetryPayment(Guid id)
    {
        try
        {
            await _paymentService.RetryFailedPaymentAsync(id);
            return Ok(new { message = "Payment retry initiated" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to retry payment {PaymentId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrying payment {PaymentId}", id);
            return NotFound(new { error = "Payment not found" });
        }
    }

    /// <summary>
    /// Refund a captured payment (full or partial)
    /// </summary>
    [HttpPost("{id}/refund")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentDto>> RefundPayment(
        Guid id,
        [FromBody] RefundPaymentDto dto)
    {
        try
        {
            var payment = await _paymentService.RefundPaymentAsync(id, dto);
            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to refund payment {PaymentId}", id);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment {PaymentId}", id);
            return NotFound(new { error = "Payment not found" });
        }
    }

    /// <summary>
    /// Get payment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPaymentById(Guid id)
    {
        var payment = await _paymentService.GetPaymentByIdAsync(id);
        
        if (payment == null)
        {
            return NotFound(new { error = "Payment not found" });
        }

        return Ok(payment);
    }

    /// <summary>
    /// Get payment by order ID
    /// </summary>
    [HttpGet("order/{orderId}")]
    [ProducesResponseType(typeof(PaymentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(Guid orderId)
    {
        var payment = await _paymentService.GetPaymentByOrderIdAsync(orderId);
        
        if (payment == null)
        {
            return NotFound(new { error = "Payment not found for this order" });
        }

        return Ok(payment);
    }

    /// <summary>
    /// Get payments by status
    /// </summary>
    [HttpGet("status/{status}")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetPaymentsByStatus(
        string status)
    {
        try
        {
            var payments = await _paymentService.GetPaymentsByStatusAsync(status);
            return Ok(payments);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all failed payments
    /// </summary>
    [HttpGet("failed")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetFailedPayments()
    {
        var payments = await _paymentService.GetFailedPaymentsAsync();
        return Ok(payments);
    }

    /// <summary>
    /// Get expired payments
    /// </summary>
    [HttpGet("expired")]
    [ProducesResponseType(typeof(IEnumerable<PaymentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentDto>>> GetExpiredPayments()
    {
        var payments = await _paymentService.GetExpiredPaymentsAsync();
        return Ok(payments);
    }

    /// <summary>
    /// Get payment analytics for a date range
    /// </summary>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(PaymentAnalyticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaymentAnalyticsDto>> GetPaymentAnalytics(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var analytics = await _paymentService.GetPaymentAnalyticsAsync(from, to);
        return Ok(analytics);
    }

    /// <summary>
    /// Get total revenue for a date range
    /// </summary>
    [HttpGet("revenue")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    public async Task<ActionResult<decimal>> GetTotalRevenue(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var revenue = await _paymentService.GetTotalRevenueAsync(from, to);
        return Ok(new { revenue, from, to });
    }

    /// <summary>
    /// Add a new payment gateway configuration
    /// </summary>
    [HttpPost("gateways")]
    [EnableRateLimiting("token")]
    [ProducesResponseType(typeof(PaymentGatewayConfigDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaymentGatewayConfigDto>> AddPaymentGateway(
        [FromBody] CreatePaymentGatewayDto dto)
    {
        try
        {
            var gateway = await _paymentService.AddPaymentGatewayAsync(dto);
            return CreatedAtAction(nameof(AddPaymentGateway), gateway);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add payment gateway {Name}", dto.Name);
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all active payment gateways
    /// </summary>
    [HttpGet("gateways")]
    [ProducesResponseType(typeof(IEnumerable<PaymentGatewayConfigDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PaymentGatewayConfigDto>>> GetActiveGateways()
    {
        var gateways = await _paymentService.GetActivePaymentGatewaysAsync();
        return Ok(gateways);
    }
}
}
