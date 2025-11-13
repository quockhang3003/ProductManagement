using ProductManagement.Application.Services;

namespace ProductManagement.Application.DTOs;

public record ReserveStockDto(
    Guid OrderId,
    List<OrderItemAllocation> Allocations
);