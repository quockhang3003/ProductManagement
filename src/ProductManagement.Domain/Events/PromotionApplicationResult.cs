namespace ProductManagement.Domain.Events;

public record PromotionApplicationResult(
    Guid PromotionId,
    string Code,
    decimal Discount
);