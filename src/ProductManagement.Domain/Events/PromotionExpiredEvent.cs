namespace ProductManagement.Domain.Events;

public record PromotionExpiredEvent(
    Guid PromotionId,
    string Code,
    string Name,
    int TotalUsageCount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}