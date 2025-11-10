namespace ProductManagement.Domain.Events;

public record WarehouseDeactivatedEvent(
    Guid WarehouseId,
    string Code,
    string Name
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}