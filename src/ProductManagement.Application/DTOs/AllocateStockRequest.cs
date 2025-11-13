using ProductManagement.Application.Services;

namespace ProductManagement.Application.DTOs;

public record AllocateStockRequest(
    List<OrderItemRequest> Items,
    string CustomerCity
);