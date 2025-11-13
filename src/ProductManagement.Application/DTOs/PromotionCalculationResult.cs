namespace ProductManagement.Application.DTOs;

public record PromotionCalculationResult(
    decimal OriginalAmount,
    decimal FinalAmount,
    decimal TotalDiscount,
    List<AppliedPromotionInfo> AppliedPromotions
);
