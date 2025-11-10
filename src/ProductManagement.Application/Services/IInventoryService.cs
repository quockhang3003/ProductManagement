using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Services;

public interface IInventoryService
{
    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseDto dto);
    Task<IEnumerable<WarehouseDto>> GetAllWarehousesAsync();
    Task<WarehouseDto?> GetWarehouseByIdAsync(Guid id);
    Task<WarehouseDto?> GetWarehouseByCodeAsync(string code);
    
    // Inventory Item Management
    Task<InventoryItemDto> CreateInventoryItemAsync(CreateInventoryItemDto dto);
    Task<IEnumerable<InventoryItemDto>> GetInventoryByProductAsync(Guid productId);
    Task<IEnumerable<InventoryItemDto>> GetInventoryByWarehouseAsync(Guid warehouseId);
    Task<int> GetTotalAvailableStockAsync(Guid productId);
    
    // Stock Operations
    Task AdjustStockAsync(Guid inventoryItemId, AdjustStockDto dto);
    Task RestockAsync(Guid inventoryItemId, int quantity);
    Task ReserveStockAsync(Guid orderId, List<OrderItemAllocation> allocations);
    Task ReleaseReservationAsync(Guid orderId);
    Task FulfillReservationAsync(Guid orderId);
    
    // Stock Transfer
    Task<StockTransferDto> RequestTransferAsync(RequestStockTransferDto dto);
    Task ApproveTransferAsync(Guid transferId, string approvedBy);
    Task CompleteTransferAsync(Guid transferId);
    Task CancelTransferAsync(Guid transferId, string reason);
    Task<IEnumerable<StockTransferDto>> GetPendingTransfersAsync();
    
    // Stock Allocation (Smart algorithm)
    Task<List<OrderItemAllocation>> AllocateStockForOrderAsync(
        List<OrderItemRequest> items, 
        string customerCity);
    
    // Audit
    Task<InventoryAuditDto> PerformAuditAsync(PerformAuditDto dto);
    Task<IEnumerable<InventoryAuditDto>> GetAuditHistoryAsync(Guid warehouseId, Guid productId);
    
    // Analytics
    Task<InventoryHealthReportDto> GetInventoryHealthReportAsync();
    Task<IEnumerable<LowStockItemDto>> GetLowStockItemsAsync();
}