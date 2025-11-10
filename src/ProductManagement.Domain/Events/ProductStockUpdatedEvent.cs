namespace ProductManagement.Domain.Events;

public record ProductStockUpdatedEvent(
    Guid ProductId,
    int OldStock,
    int NewStock,
    int Change
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}