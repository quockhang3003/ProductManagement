namespace ProductManagement.Application.DTOs;

public record RequestStockTransferDto(
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    int Quantity,
    string RequestedBy,
    string? Notes
);