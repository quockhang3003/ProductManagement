namespace ProductManagement.Application.DTOs;

public record LowStockItemDto(
    Guid InventoryItemId,
    Guid ProductId,
    string ProductName,
    Guid WarehouseId,
    string WarehouseCode,
    int CurrentStock,
    int ReorderPoint,
    int ReorderQuantity
);