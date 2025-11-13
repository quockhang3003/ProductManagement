using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;

namespace ProductManagement.WebUI.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [EnableRateLimiting("fixed")]
    public class InventoryApiController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;
        private readonly ILogger<InventoryApiController> _logger;

        public InventoryApiController(
            IInventoryService inventoryService,
            ILogger<InventoryApiController> logger)
        {
            _inventoryService = inventoryService;
            _logger = logger;
        }

        // ============================================
        // WAREHOUSE MANAGEMENT
        // ============================================

        /// <summary>
        /// Create a new warehouse
        /// </summary>
        [HttpPost("warehouses")]
        [EnableRateLimiting("token")]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<WarehouseDto>> CreateWarehouse(
            [FromBody] CreateWarehouseDto dto)
        {
            try
            {
                var warehouse = await _inventoryService.CreateWarehouseAsync(dto);

                return CreatedAtAction(
                    nameof(GetWarehouseById),
                    new { id = warehouse.Id },
                    warehouse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create warehouse {Code}", dto.Code);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all warehouses
        /// </summary>
        [HttpGet("warehouses")]
        [ProducesResponseType(typeof(IEnumerable<WarehouseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<WarehouseDto>>> GetAllWarehouses()
        {
            var warehouses = await _inventoryService.GetAllWarehousesAsync();
            return Ok(warehouses);
        }

        /// <summary>
        /// Get warehouse by ID
        /// </summary>
        [HttpGet("warehouses/{id}")]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WarehouseDto>> GetWarehouseById(Guid id)
        {
            var warehouse = await _inventoryService.GetWarehouseByIdAsync(id);

            if (warehouse == null)
            {
                return NotFound(new { error = "Warehouse not found" });
            }

            return Ok(warehouse);
        }

        /// <summary>
        /// Get warehouse by code
        /// </summary>
        [HttpGet("warehouses/code/{code}")]
        [ProducesResponseType(typeof(WarehouseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WarehouseDto>> GetWarehouseByCode(string code)
        {
            var warehouse = await _inventoryService.GetWarehouseByCodeAsync(code);

            if (warehouse == null)
            {
                return NotFound(new { error = $"Warehouse with code '{code}' not found" });
            }

            return Ok(warehouse);
        }

        // ============================================
        // INVENTORY ITEM MANAGEMENT
        // ============================================

        /// <summary>
        /// Create a new inventory item (add product to warehouse)
        /// </summary>
        [HttpPost("items")]
        [EnableRateLimiting("token")]
        [ProducesResponseType(typeof(InventoryItemDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryItemDto>> CreateInventoryItem(
            [FromBody] CreateInventoryItemDto dto)
        {
            try
            {
                var item = await _inventoryService.CreateInventoryItemAsync(dto);
                return CreatedAtAction(nameof(CreateInventoryItem), item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create inventory item");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get inventory for a specific product (across all warehouses)
        /// </summary>
        [HttpGet("products/{productId}")]
        [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventoryByProduct(
            Guid productId)
        {
            var items = await _inventoryService.GetInventoryByProductAsync(productId);
            return Ok(items);
        }

        /// <summary>
        /// Get all inventory items in a specific warehouse
        /// </summary>
        [HttpGet("warehouses/{warehouseId}/items")]
        [ProducesResponseType(typeof(IEnumerable<InventoryItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryItemDto>>> GetInventoryByWarehouse(
            Guid warehouseId)
        {
            var items = await _inventoryService.GetInventoryByWarehouseAsync(warehouseId);
            return Ok(items);
        }

        /// <summary>
        /// Get total available stock for a product (all warehouses combined)
        /// </summary>
        [HttpGet("products/{productId}/total-stock")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetTotalAvailableStock(Guid productId)
        {
            var totalStock = await _inventoryService.GetTotalAvailableStockAsync(productId);
            return Ok(new { productId, totalStock });
        }

        // ============================================
        // STOCK OPERATIONS
        // ============================================

        /// <summary>
        /// Adjust stock (manual adjustment with reason)
        /// </summary>
        [HttpPost("items/{inventoryItemId}/adjust")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AdjustStock(
            Guid inventoryItemId,
            [FromBody] AdjustStockDto dto)
        {
            try
            {
                await _inventoryService.AdjustStockAsync(inventoryItemId, dto);
                return Ok(new { message = "Stock adjusted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to adjust stock for item {ItemId}", inventoryItemId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Restock inventory item (add quantity)
        /// </summary>
        [HttpPost("items/{inventoryItemId}/restock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Restock(
            Guid inventoryItemId,
            [FromBody] RestockDto dto)
        {
            try
            {
                await _inventoryService.RestockAsync(inventoryItemId, dto.Quantity);
                return Ok(new { message = $"Restocked {dto.Quantity} units successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to restock item {ItemId}", inventoryItemId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Reserve stock for an order
        /// </summary>
        [HttpPost("reserve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReserveStock([FromBody] ReserveStockDto dto)
        {
            try
            {
                await _inventoryService.ReserveStockAsync(dto.OrderId, dto.Allocations);
                return Ok(new { message = "Stock reserved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to reserve stock for order {OrderId}", dto.OrderId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Release reservation (e.g., when order is cancelled)
        /// </summary>
        [HttpPost("release/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReleaseReservation(Guid orderId)
        {
            try
            {
                await _inventoryService.ReleaseReservationAsync(orderId);
                return Ok(new { message = "Reservation released successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to release reservation for order {OrderId}", orderId);
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Fulfill reservation (deduct stock when order ships)
        /// </summary>
        [HttpPost("fulfill/{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> FulfillReservation(Guid orderId)
        {
            try
            {
                await _inventoryService.FulfillReservationAsync(orderId);
                return Ok(new { message = "Reservation fulfilled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fulfill reservation for order {OrderId}", orderId);
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================
        // SMART STOCK ALLOCATION (THE CORE ALGORITHM!)
        // ============================================

        /// <summary>
        /// Allocate stock for an order using smart algorithm
        /// (Same city priority → Warehouse priority → Highest stock)
        /// Supports auto-split across multiple warehouses
        /// </summary>
        [HttpPost("allocate")]
        [ProducesResponseType(typeof(List<OrderItemAllocation>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<OrderItemAllocation>>> AllocateStockForOrder(
            [FromBody] AllocateStockRequest request)
        {
            try
            {
                var allocations = await _inventoryService.AllocateStockForOrderAsync(
                    request.Items,
                    request.CustomerCity);

                _logger.LogInformation(
                    "Stock allocated for order: {ItemCount} items, {WarehouseCount} warehouses",
                    request.Items.Count,
                    allocations.Select(a => a.WarehouseId).Distinct().Count());

                return Ok(new
                {
                    allocations,
                    summary = new
                    {
                        totalItems = request.Items.Count,
                        warehousesUsed = allocations.Select(a => a.WarehouseCode).Distinct().ToList(),
                        splitOrders = allocations.GroupBy(a => a.ProductId).Any(g => g.Count() > 1)
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to allocate stock");
                return BadRequest(new { error = ex.Message });
            }
        }

        // ============================================
        // STOCK TRANSFER
        // ============================================

        /// <summary>
        /// Request a stock transfer between warehouses
        /// </summary>
        [HttpPost("transfers")]
        [EnableRateLimiting("token")]
        [ProducesResponseType(typeof(StockTransferDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<StockTransferDto>> RequestTransfer(
            [FromBody] RequestStockTransferDto dto)
        {
            try
            {
                var transfer = await _inventoryService.RequestTransferAsync(dto);
                return CreatedAtAction(nameof(RequestTransfer), transfer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to request stock transfer");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Approve a pending stock transfer
        /// </summary>
        [HttpPost("transfers/{transferId}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveTransfer(
            Guid transferId,
            [FromBody] ApproveTransferDto dto)
        {
            try
            {
                await _inventoryService.ApproveTransferAsync(transferId, dto.ApprovedBy);
                return Ok(new { message = "Transfer approved successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to approve transfer {TransferId}", transferId);
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Complete a stock transfer (execute the transfer)
        /// </summary>
        [HttpPost("transfers/{transferId}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CompleteTransfer(Guid transferId)
        {
            try
            {
                await _inventoryService.CompleteTransferAsync(transferId);
                return Ok(new { message = "Transfer completed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to complete transfer {TransferId}", transferId);
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Cancel a stock transfer
        /// </summary>
        [HttpPost("transfers/{transferId}/cancel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CancelTransfer(
            Guid transferId,
            [FromBody] CancelTransferDto dto)
        {
            try
            {
                await _inventoryService.CancelTransferAsync(transferId, dto.Reason);
                return Ok(new { message = "Transfer cancelled successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to cancel transfer {TransferId}", transferId);
                return NotFound(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get all pending stock transfers
        /// </summary>
        [HttpGet("transfers/pending")]
        [ProducesResponseType(typeof(IEnumerable<StockTransferDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<StockTransferDto>>> GetPendingTransfers()
        {
            var transfers = await _inventoryService.GetPendingTransfersAsync();
            return Ok(transfers);
        }

        // ============================================
        // INVENTORY AUDIT
        // ============================================

        /// <summary>
        /// Perform inventory audit (physical count)
        /// </summary>
        [HttpPost("audits")]
        [ProducesResponseType(typeof(InventoryAuditDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InventoryAuditDto>> PerformAudit(
            [FromBody] PerformAuditDto dto)
        {
            try
            {
                var audit = await _inventoryService.PerformAuditAsync(dto);
                return CreatedAtAction(nameof(PerformAudit), audit);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform audit");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Get audit history for a product in a warehouse
        /// </summary>
        [HttpGet("audits")]
        [ProducesResponseType(typeof(IEnumerable<InventoryAuditDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InventoryAuditDto>>> GetAuditHistory(
            [FromQuery] Guid warehouseId,
            [FromQuery] Guid productId)
        {
            var audits = await _inventoryService.GetAuditHistoryAsync(warehouseId, productId);
            return Ok(audits);
        }

        // ============================================
        // ANALYTICS & REPORTS
        // ============================================

        /// <summary>
        /// Get inventory health report
        /// </summary>
        [HttpGet("health")]
        [ProducesResponseType(typeof(InventoryHealthReportDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<InventoryHealthReportDto>> GetInventoryHealthReport()
        {
            var report = await _inventoryService.GetInventoryHealthReportAsync();
            return Ok(report);
        }

        /// <summary>
        /// Get low stock items (below reorder point)
        /// </summary>
        [HttpGet("low-stock")]
        [ProducesResponseType(typeof(IEnumerable<LowStockItemDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<LowStockItemDto>>> GetLowStockItems()
        {
            var items = await _inventoryService.GetLowStockItemsAsync();
            return Ok(items);
        }
    }
}
