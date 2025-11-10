namespace ProductManagement.Domain.Events;

public record StockReservationReleasedEvent(
    Guid InventoryItemId,
    Guid WarehouseId,
    Guid ProductId,
    int ReleasedQuantity,
    int RemainingReserved,
    Guid OrderId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}