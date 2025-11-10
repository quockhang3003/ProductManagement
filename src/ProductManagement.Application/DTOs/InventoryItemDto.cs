namespace ProductManagement.Application.DTOs;

public record InventoryItemDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseCode,
    Guid ProductId,
    string ProductName,
    int QuantityOnHand,
    int QuantityReserved,
    int QuantityAvailable,
    int ReorderPoint,
    int ReorderQuantity,
    DateTime LastRestockedAt
);