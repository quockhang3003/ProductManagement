namespace ProductManagement.Domain.Entities;

public class PromotionUsage
{
    public Guid Id { get; private set; }
    public Guid PromotionId { get; private set; }
    public Guid OrderId { get; private set; }
    public string? CustomerEmail { get; private set; }
    public decimal OrderTotal { get; private set; }
    public decimal DiscountAmount { get; private set; }
    public DateTime UsedAt { get; private set; }

    // Constructor for Dapper
    private PromotionUsage() { }

    public PromotionUsage(
        Guid promotionId,
        Guid orderId,
        string? customerEmail,
        decimal orderTotal,
        decimal discountAmount)
    {
        Id = Guid.NewGuid();
        PromotionId = promotionId;
        OrderId = orderId;
        CustomerEmail = customerEmail;
        OrderTotal = orderTotal;
        DiscountAmount = discountAmount;
        UsedAt = DateTime.UtcNow;
    }
}