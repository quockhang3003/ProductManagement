namespace ProductManagement.Domain.Exceptions;

public class PromotionAlreadyExistsException : DomainException
{
    public PromotionAlreadyExistsException(string code)
        : base($"Promotion with code '{code}' already exists")
    {
        Code = code;
    }

    public string Code { get; }
}