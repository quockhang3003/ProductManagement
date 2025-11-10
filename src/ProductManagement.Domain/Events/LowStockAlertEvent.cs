namespace ProductManagement.Domain.Events;

public record LowStockAlertEvent(
    Guid ProductId,
    Guid WarehouseId,
    int CurrentStock,
    int ReorderPoint,
    int ReorderQuantity
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}