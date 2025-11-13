namespace ProductManagement.Application.DTOs;

public record PromotionEffectivenessDto(
    Guid PromotionId,
    string Code,
    string Name,
    string Type,
    int TotalUsage,
    decimal TotalDiscount,
    decimal TotalRevenue,
    decimal ConversionRate
);