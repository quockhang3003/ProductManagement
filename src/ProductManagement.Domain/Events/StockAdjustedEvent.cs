namespace ProductManagement.Domain.Events;

public record StockAdjustedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    int OldQuantity,
    int NewQuantity,
    int Adjustment,
    string Reason,
    string UserId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}