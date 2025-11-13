using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Entities;

public class PromotionRule
{
    public Guid Id { get; private set; }
    public Guid PromotionId { get; private set; }
    public PromotionRuleType Type { get; private set; }
    public string? TargetProductIds { get; private set; }  // JSON array
    public string? TargetCategoryIds { get; private set; }  // JSON array
    public decimal? MinQuantity { get; private set; }
    public decimal? MinAmount { get; private set; }
    public string? CustomCondition { get; private set; }  // JSON expression

    // Constructor for Dapper
    private PromotionRule() { }

    public PromotionRule(
        Guid promotionId,
        PromotionRuleType type,
        string? targetProductIds = null,
        decimal? minQuantity = null,
        decimal? minAmount = null)
    {
        Id = Guid.NewGuid();
        PromotionId = promotionId;
        Type = type;
        TargetProductIds = targetProductIds;
        MinQuantity = minQuantity;
        MinAmount = minAmount;
    }

    public bool IsValid(decimal orderTotal, List<Guid> productIds)
    {
        switch (Type)
        {
            case PromotionRuleType.MinimumPurchase:
                return MinAmount.HasValue && orderTotal >= MinAmount.Value;

            case PromotionRuleType.SpecificProducts:
                if (string.IsNullOrEmpty(TargetProductIds))
                    return true;
                var targetIds = ParseProductIds(TargetProductIds);
                return productIds.Any(id => targetIds.Contains(id));

            case PromotionRuleType.MinimumQuantity:
                return MinQuantity.HasValue && productIds.Count >= MinQuantity.Value;

            case PromotionRuleType.ExcludeProducts:
                if (string.IsNullOrEmpty(TargetProductIds))
                    return true;
                var excludedIds = ParseProductIds(TargetProductIds);
                return !productIds.Any(id => excludedIds.Contains(id));

            default:
                return true;
        }
    }

    private List<Guid> ParseProductIds(string json)
    {
        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(json) 
                ?? new List<Guid>();
        }
        catch
        {
            return new List<Guid>();
        }
    }
}