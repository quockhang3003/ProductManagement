namespace ProductManagement.Domain.Events;

public record StockRestockedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    int OldQuantity,
    int NewQuantity,
    int AddedQuantity
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}