namespace ProductManagement.Domain.Events;

public record PromotionLimitReachedEvent(
    Guid PromotionId,
    string Code,
    int MaxUsageCount,
    int CurrentUsageCount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}