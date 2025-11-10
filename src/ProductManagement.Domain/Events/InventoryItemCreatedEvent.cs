namespace ProductManagement.Domain.Events;

public record InventoryItemCreatedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    int InitialQuantity
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
;