namespace ProductManagement.Domain.Enum;

public enum PromotionRuleType
{
    MinimumPurchase = 0,
    SpecificProducts = 1,
    MinimumQuantity = 2,
    ExcludeProducts = 3,
    SpecificCategory = 4,
    DayOfWeek = 5,
    TimeOfDay = 6
}