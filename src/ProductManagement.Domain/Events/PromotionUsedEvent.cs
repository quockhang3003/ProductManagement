namespace ProductManagement.Domain.Events;

public record PromotionUsedEvent(
    Guid PromotionId,
    string Code,
    string? CustomerEmail,
    int TotalUsageCount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}