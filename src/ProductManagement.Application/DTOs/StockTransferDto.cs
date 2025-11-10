namespace ProductManagement.Application.DTOs;

public record StockTransferDto(
    Guid Id,
    Guid ProductId,
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    int Quantity,
    string Status,
    string? Notes,
    string RequestedBy,
    string? ApprovedBy,
    DateTime RequestedAt,
    DateTime? ApprovedAt,
    DateTime? CompletedAt,
    DateTime? CancelledAt,
    string? CancellationReason
);