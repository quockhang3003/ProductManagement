using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Events;

namespace ProductManagement.Domain.Entities;

public class Promotion
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<PromotionRule> _rules = new();
    
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    public IReadOnlyCollection<PromotionRule> Rules => _rules.AsReadOnly();

    public Guid Id { get; private set; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public PromotionType Type { get; private set; }
    public decimal DiscountValue { get; private set; }  // Percentage or fixed amount
    public decimal? MaxDiscountAmount { get; private set; }  // Cap for percentage discounts
    public decimal? MinimumPurchaseAmount { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsStackable { get; private set; }  // Can combine with other promotions
    public int Priority { get; private set; }  // Higher priority applies first
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int? MaxUsageCount { get; private set; }
    public int CurrentUsageCount { get; private set; }
    public int? MaxUsagePerCustomer { get; private set; }
    public bool RequiresCouponCode { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? TargetCustomerSegment { get; private set; }  // VIP, Regular, New, etc.

    // Constructor for Dapper
    private Promotion()
    {
        Code = string.Empty;
        Name = string.Empty;
        Description = string.Empty;
    }

    public Promotion(
        string code,
        string name,
        string description,
        PromotionType type,
        decimal discountValue,
        DateTime startDate,
        DateTime endDate,
        bool requiresCouponCode = false,
        bool isStackable = false,
        int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Promotion code is required");

        if (discountValue <= 0)
            throw new ArgumentException("Discount value must be positive");

        if (type == PromotionType.Percentage && discountValue > 100)
            throw new ArgumentException("Percentage discount cannot exceed 100%");

        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date");

        Id = Guid.NewGuid();
        Code = code.ToUpper();
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Type = type;
        DiscountValue = discountValue;
        StartDate = startDate;
        EndDate = endDate;
        RequiresCouponCode = requiresCouponCode;
        IsStackable = isStackable;
        Priority = priority;
        IsActive = true;
        CurrentUsageCount = 0;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new PromotionCreatedEvent(
            Id, Code, Name, Type.ToString(), DiscountValue, StartDate, EndDate));
    }

    public bool IsValid(DateTime? atDate = null)
    {
        var checkDate = atDate ?? DateTime.UtcNow;
        
        if (!IsActive)
            return false;

        if (checkDate < StartDate || checkDate > EndDate)
            return false;

        if (MaxUsageCount.HasValue && CurrentUsageCount >= MaxUsageCount.Value)
            return false;

        return true;
    }

    public bool CanApplyToOrder(
        decimal orderTotal,
        string? customerEmail,
        int customerUsageCount,
        string? customerSegment)
    {
        if (!IsValid())
            return false;

        if (MinimumPurchaseAmount.HasValue && orderTotal < MinimumPurchaseAmount.Value)
            return false;

        if (MaxUsagePerCustomer.HasValue && customerUsageCount >= MaxUsagePerCustomer.Value)
            return false;

        if (!string.IsNullOrEmpty(TargetCustomerSegment) && 
            customerSegment != TargetCustomerSegment)
            return false;

        return true;
    }

    public decimal CalculateDiscount(decimal orderTotal, List<Guid> productIds)
    {
        if (!IsValid() || orderTotal <= 0)
            return 0;

        // Check rules
        if (_rules.Any() && !_rules.All(r => r.IsValid(orderTotal, productIds)))
            return 0;

        decimal discount = 0;

        switch (Type)
        {
            case PromotionType.Percentage:
                discount = orderTotal * (DiscountValue / 100);
                if (MaxDiscountAmount.HasValue && discount > MaxDiscountAmount.Value)
                    discount = MaxDiscountAmount.Value;
                break;

            case PromotionType.FixedAmount:
                discount = DiscountValue;
                break;

            case PromotionType.FreeShipping:
                discount = 0; // Shipping discount handled separately
                break;

            case PromotionType.BuyOneGetOne:
                // BOGO logic (simplified)
                discount = CalculateBOGODiscount(orderTotal, productIds);
                break;
        }

        return Math.Min(discount, orderTotal); // Never exceed order total
    }

    public void IncrementUsage(string? customerEmail = null)
    {
        CurrentUsageCount++;
        
        AddDomainEvent(new PromotionUsedEvent(
            Id, Code, customerEmail, CurrentUsageCount));
    }

    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new PromotionActivatedEvent(Id, Code, Name));
    }

    public void Deactivate()
    {
        IsActive = false;
        AddDomainEvent(new PromotionDeactivatedEvent(Id, Code, Name));
    }

    public void AddRule(PromotionRule rule)
    {
        _rules.Add(rule);
    }

    public void SetMaxUsage(int maxUsage)
    {
        MaxUsageCount = maxUsage;
    }

    public void SetMaxUsagePerCustomer(int maxUsage)
    {
        MaxUsagePerCustomer = maxUsage;
    }

    public void SetMinimumPurchase(decimal amount)
    {
        MinimumPurchaseAmount = amount;
    }

    public void SetMaxDiscount(decimal amount)
    {
        MaxDiscountAmount = amount;
    }

    public void SetTargetSegment(string segment)
    {
        TargetCustomerSegment = segment;
    }

    private decimal CalculateBOGODiscount(decimal orderTotal, List<Guid> productIds)
    {
        // Simplified BOGO: Get 50% off second item
        // Real implementation would need product prices
        return orderTotal * 0.25m; // Approximate
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
    private void AddDomainEvent(IDomainEvent eventItem) => _domainEvents.Add(eventItem);
}
