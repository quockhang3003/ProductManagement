namespace ProductManagement.Domain.Events;

public record OrderDeliveredEvent(
    Guid OrderId,
    string CustomerName,
    string CustomerEmail,
    decimal TotalAmount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}