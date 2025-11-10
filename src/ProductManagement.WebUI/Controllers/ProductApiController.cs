using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;

namespace ProductManagement.WebUI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductApiController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductApiController> _logger;

    public ProductApiController(
        IProductService productService,
        ILogger<ProductApiController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    [HttpGet]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAll()
    {
        try
        {
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get active products only
    /// </summary>
    [HttpGet("active")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetActive()
    {
        try
        {
            var products = await _productService.GetActiveProductsAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [EnableRateLimiting("sliding")]
    public async Task<ActionResult<ProductDto>> GetById(Guid id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
                return NotFound(new { message = $"Product with ID {id} not found" });

            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [EnableRateLimiting("token")]
    public async Task<ActionResult<ProductDto>> Create([FromBody] CreateProductDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _productService.CreateProductAsync(dto);
            
            return CreatedAtAction(
                nameof(GetById), 
                new { id = product.Id }, 
                product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update product
    /// </summary>
    [HttpPut("{id}")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto)
    {
        try
        {
            if (id != dto.Id)
                return BadRequest(new { message = "ID mismatch" });

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _productService.UpdateProductAsync(dto);
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Update product stock
    /// </summary>
    [HttpPatch("{id}/stock")]
    [EnableRateLimiting("concurrency")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity)
    {
        try
        {
            await _productService.UpdateStockAsync(id, quantity);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Delete product
    /// </summary>
    [HttpDelete("{id}")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _productService.DeleteProductAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Activate product
    /// </summary>
    [HttpPost("{id}/activate")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Activate(Guid id)
    {
        try
        {
            await _productService.ActivateProductAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate product
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [EnableRateLimiting("token")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        try
        {
            await _productService.DeactivateProductAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating product {ProductId}", id);
            return BadRequest(new { message = ex.Message });
        }
    }
}