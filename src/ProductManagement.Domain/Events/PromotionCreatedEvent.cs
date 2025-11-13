namespace ProductManagement.Domain.Events;

public record PromotionCreatedEvent(
    Guid PromotionId,
    string Code,
    string Name,
    string Type,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate
) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}