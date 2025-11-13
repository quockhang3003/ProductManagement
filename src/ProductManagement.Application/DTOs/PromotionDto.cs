namespace ProductManagement.Application.DTOs;

public record PromotionDto(
    Guid Id,
    string Code,
    string Name,
    string Description,
    string Type,
    decimal DiscountValue,
    decimal? MaxDiscountAmount,
    decimal? MinimumPurchaseAmount,
    bool IsActive,
    bool IsStackable,
    int Priority,
    DateTime StartDate,
    DateTime EndDate,
    int? MaxUsageCount,
    int CurrentUsageCount,
    int? MaxUsagePerCustomer,
    bool RequiresCouponCode,
    string? TargetCustomerSegment
);