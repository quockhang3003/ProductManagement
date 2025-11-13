namespace ProductManagement.Domain.Events;

public record PromotionDeactivatedEvent(
    Guid PromotionId,
    string Code,
    string Name
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}