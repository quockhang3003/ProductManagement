namespace ProductManagement.Domain.Events;

public record ProductCreatedEvent(
    Guid ProductId,
    string Name,
    decimal Price,
    int Stock
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}