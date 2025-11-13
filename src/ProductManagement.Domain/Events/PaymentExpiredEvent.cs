namespace ProductManagement.Domain.Events;

public record PaymentExpiredEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}