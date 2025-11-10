namespace ProductManagement.Domain.Events;

public record OrderShippedEvent(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    string TrackingNumber
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}