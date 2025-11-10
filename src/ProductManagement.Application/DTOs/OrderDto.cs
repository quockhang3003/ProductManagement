namespace ProductManagement.Application.DTOs;

public record OrderDto(
    Guid Id,
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    string? PhoneNumber,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAt,
    DateTime? ConfirmedAt,
    DateTime? ShippedAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    string? TrackingNumber,
    List<OrderItemDto> Items
);

public record OrderItemDto(
    Guid Id,
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal SubTotal
);