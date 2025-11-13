namespace ProductManagement.Application.DTOs;

public record AppliedPromotionInfo(
    Guid PromotionId,
    string Code,
    string Name,
    string Type,
    decimal Discount
);