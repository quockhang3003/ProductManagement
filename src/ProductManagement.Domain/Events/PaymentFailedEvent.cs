namespace ProductManagement.Domain.Events;

public record PaymentFailedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Method,
    string FailureReason,
    int RetryCount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}