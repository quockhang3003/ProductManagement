using ProductManagement.Domain.Enum;

namespace ProductManagement.Application.DTOs;

public record CreatePromotionDto(
    string Code,
    string Name,
    string Description,
    PromotionType Type,
    decimal DiscountValue,
    DateTime StartDate,
    DateTime EndDate,
    bool RequiresCouponCode = false,
    bool IsStackable = false,
    int Priority = 0,
    decimal? MaxDiscountAmount = null,
    decimal? MinimumPurchaseAmount = null,
    int? MaxUsageCount = null,
    int? MaxUsagePerCustomer = null,
    string? TargetCustomerSegment = null,
    List<CreatePromotionRuleDto>? Rules = null
);
