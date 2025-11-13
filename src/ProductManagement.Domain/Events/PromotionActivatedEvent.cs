namespace ProductManagement.Domain.Events;

public record PromotionActivatedEvent(
    Guid PromotionId,
    string Code,
    string Name
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
