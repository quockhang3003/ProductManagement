namespace ProductManagement.Domain.Events;

public record BestPromotionSelectedEvent(
    Guid OrderId,
    List<PromotionApplicationResult> AppliedPromotions,
    decimal TotalDiscount,
    decimal OriginalAmount,
    decimal FinalAmount
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}