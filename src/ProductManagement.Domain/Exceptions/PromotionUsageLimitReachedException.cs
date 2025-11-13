namespace ProductManagement.Domain.Exceptions;

public class PromotionUsageLimitReachedException : DomainException
{
    public PromotionUsageLimitReachedException(string code, int maxUsage)
        : base($"Promotion '{code}' has reached its maximum usage limit of {maxUsage}")
    {
        Code = code;
        MaxUsage = maxUsage;
    }

    public string Code { get; }
    public int MaxUsage { get; }
}