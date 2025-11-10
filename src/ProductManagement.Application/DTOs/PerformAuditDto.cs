namespace ProductManagement.Application.DTOs;

public record PerformAuditDto(
    Guid WarehouseId,
    Guid ProductId,
    int ActualQuantity,
    string AuditedBy,
    string? Notes
);