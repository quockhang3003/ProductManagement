namespace ProductManagement.Domain.Events;

public record OrderCancelledEvent(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    string PreviousStatus,
    string CancellationReason,
    List<OrderItemData> Items
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}