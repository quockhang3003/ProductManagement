namespace ProductManagement.Application.DTOs;

public record InventoryAuditDto(
    Guid Id,
    Guid WarehouseId,
    Guid ProductId,
    int ExpectedQuantity,
    int ActualQuantity,
    int Variance,
    string AuditedBy,
    string? Notes,
    DateTime AuditedAt
);