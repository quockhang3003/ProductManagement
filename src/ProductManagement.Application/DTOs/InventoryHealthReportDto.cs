namespace ProductManagement.Application.DTOs;

public record InventoryHealthReportDto(
    int TotalItems,
    int LowStockItems,
    int OutOfStockItems,
    decimal TotalInventoryValue,
    DateTime GeneratedAt
);