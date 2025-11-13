namespace ProductManagement.Domain.Exceptions;

public class CustomerPromotionLimitReachedException : DomainException
{
    public CustomerPromotionLimitReachedException(string code, string customerEmail, int maxUsage)
        : base($"Customer {customerEmail} has reached the maximum usage limit ({maxUsage}) for promotion '{code}'")
    {
        Code = code;
        CustomerEmail = customerEmail;
        MaxUsage = maxUsage;
    }

    public string Code { get; }
    public string CustomerEmail { get; }
    public int MaxUsage { get; }
}