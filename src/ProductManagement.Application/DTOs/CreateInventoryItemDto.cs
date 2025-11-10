namespace ProductManagement.Application.DTOs;

public record CreateInventoryItemDto(
    Guid WarehouseId,
    Guid ProductId,
    int InitialQuantity,
    int ReorderPoint = 10,
    int ReorderQuantity = 50
);