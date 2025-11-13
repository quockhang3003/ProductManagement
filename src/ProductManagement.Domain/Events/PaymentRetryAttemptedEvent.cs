namespace ProductManagement.Domain.Events;

public record PaymentRetryAttemptedEvent(
    Guid PaymentId,
    Guid OrderId,
    int RetryCount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}