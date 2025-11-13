using ProductManagement.Domain.Enum;

namespace ProductManagement.Application.DTOs;

public record CreatePromotionRuleDto(
    PromotionRuleType Type,
    string? TargetProductIds = null,
    decimal? MinQuantity = null,
    decimal? MinAmount = null
);