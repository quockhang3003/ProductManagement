namespace ProductManagement.Domain.Events;

public record OrderItemData(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);