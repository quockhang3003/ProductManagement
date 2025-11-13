namespace ProductManagement.Domain.Exceptions;

public class PromotionNotValidException : DomainException
{
    public PromotionNotValidException(string code, string reason)
        : base($"Promotion '{code}' is not valid: {reason}")
    {
        Code = code;
        Reason = reason;
    }

    public string Code { get; }
    public string Reason { get; }
}