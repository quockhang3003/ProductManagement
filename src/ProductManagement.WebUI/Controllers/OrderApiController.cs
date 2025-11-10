using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Exceptions;

namespace ProductManagement.WebUI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderApiController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly ILogger<OrderApiController> _logger;

    public OrderApiController(
        IOrderService orderService,
        ILogger<OrderApiController> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetAll()
    {
        try
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all orders");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get order by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<OrderDto>> GetById(Guid id)
    {
        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            
            if (order == null)
                return NotFound(new { message = $"Order with ID {id} not found" });

            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get orders by customer email
    /// </summary>
    [HttpGet("customer/{email}")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByCustomerEmail(string email)
    {
        try
        {
            var orders = await _orderService.GetOrdersByCustomerEmailAsync(email);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders for {Email}", email);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get orders by status (Pending, Confirmed, Shipping, Delivered, Cancelled)
    /// </summary>
    [HttpGet("status/{status}")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<IEnumerable<OrderDto>>> GetByStatus(string status)
    {
        try
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status {Status}", status);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Get order statistics
    /// </summary>
    [HttpGet("statistics")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<Dictionary<string, int>>> GetStatistics()
    {
        try
        {
            var stats = await _orderService.GetOrderStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting order statistics");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Create new order
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("token")]
    public async Task<ActionResult<OrderDto>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var order = await _orderService.CreateOrderAsync(dto);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = order.Id }, 
                order);
        }
        catch (ProductNotFoundException ex)
        {
            _logger.LogWarning(ex, "Product not found when creating order");
            return NotFound(new { message = ex.Message });
        }
        catch (InsufficientStockException ex)
        {
            _logger.LogWarning(ex, "Insufficient stock for order");
            return BadRequest(new 
            { 
                message = ex.Message,
                productId = ex.ProductId,
                productName = ex.ProductName,
                requested = ex.RequestedQuantity,
                available = ex.AvailableQuantity
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Confirm order (Pending → Confirmed)
    /// </summary>
    [HttpPost("{id}/confirm")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        try
        {
            await _orderService.ConfirmOrderAsync(id);
            return NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Ship order (Confirmed → Shipping)
    /// </summary>
    [HttpPost("{id}/ship")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Ship(Guid id, [FromBody] ShipOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _orderService.ShipOrderAsync(id, dto);
            return NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error shipping order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Mark order as delivered (Shipping → Delivered)
    /// </summary>
    [HttpPost("{id}/deliver")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Deliver(Guid id)
    {
        try
        {
            await _orderService.DeliverOrderAsync(id);
            return NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error delivering order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    /// <summary>
    /// Cancel order (Any status except Delivered → Cancelled)
    /// </summary>
    [HttpPost("{id}/cancel")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _orderService.CancelOrderAsync(id, dto);
            return NoContent();
        }
        catch (OrderNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling order {OrderId}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
}
