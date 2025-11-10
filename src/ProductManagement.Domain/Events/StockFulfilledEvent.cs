namespace ProductManagement.Domain.Events;

public record StockFulfilledEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    int FulfilledQuantity,
    int RemainingStock,
    Guid OrderId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}