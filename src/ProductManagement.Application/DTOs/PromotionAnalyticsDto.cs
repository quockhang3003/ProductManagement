namespace ProductManagement.Application.DTOs;

public record PromotionAnalyticsDto(
    Guid PromotionId,
    string Code,
    string Name,
    int TotalUsageCount,
    decimal TotalDiscountGiven,
    decimal TotalOrderValue,
    int UniqueCustomers,
    decimal AverageDiscount,
    DateTime StartDate,
    DateTime EndDate,
    bool IsActive
);