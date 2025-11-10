namespace ProductManagement.Domain.Events;

public record WarehouseCreatedEvent(
    Guid WarehouseId,
    string Code,
    string Name,
    string City,
    string State
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}