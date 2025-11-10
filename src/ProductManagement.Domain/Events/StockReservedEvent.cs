namespace ProductManagement.Domain.Events;

public record StockReservedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    int ReservedQuantity,
    int TotalReserved,
    Guid OrderId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}