namespace ProductManagement.Domain.Exceptions;

public class PromotionNotFoundException : DomainException
{
    public PromotionNotFoundException(Guid promotionId)
        : base($"Promotion with ID {promotionId} was not found")
    {
        PromotionId = promotionId;
    }

    public PromotionNotFoundException(string code)
        : base($"Promotion with code '{code}' was not found")
    {
        Code = code;
    }

    public Guid? PromotionId { get; }
    public string? Code { get; }
}