namespace ProductManagement.Application.DTOs;

public record PromotionUsageDto(
    Guid Id,
    Guid PromotionId,
    Guid OrderId,
    string? CustomerEmail,
    decimal OrderTotal,
    decimal DiscountAmount,
    DateTime UsedAt
);