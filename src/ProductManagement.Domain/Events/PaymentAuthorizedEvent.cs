namespace ProductManagement.Domain.Events;

public record PaymentAuthorizedEvent(
    Guid PaymentId,
    Guid OrderId,
    decimal Amount,
    string Method,
    string TransactionId
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}