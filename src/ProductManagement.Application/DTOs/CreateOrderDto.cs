namespace ProductManagement.Application.DTOs;

public record CreateOrderDto(
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    string? PhoneNumber,
    List<CreateOrderItemDto> Items
);

public record CreateOrderItemDto(
    Guid ProductId,
    int Quantity
);