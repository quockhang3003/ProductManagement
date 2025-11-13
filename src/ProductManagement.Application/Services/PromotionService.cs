using Microsoft.Extensions.Logging;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Events;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Repositories;

namespace ProductManagement.Application.Services;

public class PromotionService : IPromotionService
{
    private readonly IPromotionRepository _promotionRepo;
    private readonly IPromotionUsageRepository _usageRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<PromotionService> _logger;

    public PromotionService(
        IPromotionRepository promotionRepo,
        IPromotionUsageRepository usageRepo,
        IOrderRepository orderRepo,
        IMessagePublisher messagePublisher,
        ILogger<PromotionService> logger)
    {
        _promotionRepo = promotionRepo;
        _usageRepo = usageRepo;
        _orderRepo = orderRepo;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto dto)
    {
        // Validate code uniqueness
        var existing = await _promotionRepo.GetByCodeAsync(dto.Code);
        if (existing != null)
            throw new InvalidOperationException($"Promotion code '{dto.Code}' already exists");

        var promotion = new Promotion(
            dto.Code,
            dto.Name,
            dto.Description,
            dto.Type,
            dto.DiscountValue,
            dto.StartDate,
            dto.EndDate,
            dto.RequiresCouponCode,
            dto.IsStackable,
            dto.Priority);

        // Set optional properties
        if (dto.MaxDiscountAmount.HasValue)
            promotion.SetMaxDiscount(dto.MaxDiscountAmount.Value);

        if (dto.MinimumPurchaseAmount.HasValue)
            promotion.SetMinimumPurchase(dto.MinimumPurchaseAmount.Value);

        if (dto.MaxUsageCount.HasValue)
            promotion.SetMaxUsage(dto.MaxUsageCount.Value);

        if (dto.MaxUsagePerCustomer.HasValue)
            promotion.SetMaxUsagePerCustomer(dto.MaxUsagePerCustomer.Value);

        if (!string.IsNullOrEmpty(dto.TargetCustomerSegment))
            promotion.SetTargetSegment(dto.TargetCustomerSegment);

        // Add rules if any
        if (dto.Rules != null)
        {
            foreach (var ruleDto in dto.Rules)
            {
                var rule = new PromotionRule(
                    promotion.Id,
                    ruleDto.Type,
                    ruleDto.TargetProductIds,
                    ruleDto.MinQuantity,
                    ruleDto.MinAmount);
                promotion.AddRule(rule);
            }
        }

        await _promotionRepo.AddAsync(promotion);
        await PublishDomainEvents(promotion);

        _logger.LogInformation(
            "Promotion created: {Code} - {Name}, Type: {Type}, Discount: {Discount}",
            dto.Code, dto.Name, dto.Type, dto.DiscountValue);

        return MapToDto(promotion);
    }

    public async Task<PromotionDto?> GetPromotionByIdAsync(Guid id)
    {
        var promotion = await _promotionRepo.GetByIdAsync(id);
        return promotion == null ? null : MapToDto(promotion);
    }

    public async Task<PromotionDto?> GetPromotionByCodeAsync(string code)
    {
        var promotion = await _promotionRepo.GetByCodeAsync(code);
        return promotion == null ? null : MapToDto(promotion);
    }

    public async Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync()
    {
        var promotions = await _promotionRepo.GetActivePromotionsAsync();
        return promotions.Select(MapToDto);
    }

    public async Task ActivatePromotionAsync(Guid id)
    {
        var promotion = await _promotionRepo.GetByIdAsync(id);
        if (promotion == null)
            throw new PromotionNotFoundException(id);

        promotion.Activate();
        await _promotionRepo.UpdateAsync(promotion);
        await PublishDomainEvents(promotion);

        _logger.LogInformation("Promotion {Code} activated", promotion.Code);
    }

    public async Task DeactivatePromotionAsync(Guid id)
    {
        var promotion = await _promotionRepo.GetByIdAsync(id);
        if (promotion == null)
            throw new PromotionNotFoundException(id);

        promotion.Deactivate();
        await _promotionRepo.UpdateAsync(promotion);
        await PublishDomainEvents(promotion);

        _logger.LogInformation("Promotion {Code} deactivated", promotion.Code);
    }

    // ============================================
    // THE CORE ALGORITHM: Best Promotion Selection
    // ============================================
    public async Task<PromotionCalculationResult> CalculateBestPromotionsAsync(
        decimal orderTotal,
        List<Guid> productIds,
        string? customerEmail,
        string? customerSegment,
        string? couponCode = null)
    {
        var eligiblePromotions = new List<Promotion>();

        // Get all active promotions
        var activePromotions = await _promotionRepo.GetActivePromotionsAsync();

        // If coupon code provided, prioritize it
        if (!string.IsNullOrEmpty(couponCode))
        {
            var couponPromotion = activePromotions.FirstOrDefault(
                p => p.Code.Equals(couponCode, StringComparison.OrdinalIgnoreCase));

            if (couponPromotion != null && couponPromotion.RequiresCouponCode)
            {
                var customerUsage = await GetCustomerUsageCountAsync(
                    couponPromotion.Id, customerEmail ?? "");

                if (couponPromotion.CanApplyToOrder(
                    orderTotal, customerEmail, customerUsage, customerSegment))
                {
                    eligiblePromotions.Add(couponPromotion);
                }
            }
        }

        // Get auto-apply promotions (no coupon required)
        var autoPromotions = activePromotions.Where(p => !p.RequiresCouponCode);

        foreach (var promotion in autoPromotions)
        {
            var customerUsage = await GetCustomerUsageCountAsync(
                promotion.Id, customerEmail ?? "");

            if (promotion.CanApplyToOrder(
                orderTotal, customerEmail, customerUsage, customerSegment))
            {
                eligiblePromotions.Add(promotion);
            }
        }

        if (!eligiblePromotions.Any())
        {
            return new PromotionCalculationResult(
                orderTotal,
                orderTotal,
                0,
                new List<AppliedPromotionInfo>());
        }

        // ============================================
        // STRATEGY 1: Find best single promotion
        // ============================================
        var bestSinglePromotion = eligiblePromotions
            .Select(p => new
            {
                Promotion = p,
                Discount = p.CalculateDiscount(orderTotal, productIds)
            })
            .OrderByDescending(x => x.Discount)
            .First();

        // ============================================
        // STRATEGY 2: Try stacking promotions
        // ============================================
        var stackablePromotions = eligiblePromotions
            .Where(p => p.IsStackable)
            .OrderByDescending(p => p.Priority)
            .ToList();

        decimal stackedDiscount = 0;
        var stackedPromotions = new List<AppliedPromotionInfo>();

        if (stackablePromotions.Any())
        {
            var remainingTotal = orderTotal;

            foreach (var promotion in stackablePromotions)
            {
                var discount = promotion.CalculateDiscount(remainingTotal, productIds);
                
                if (discount > 0)
                {
                    stackedDiscount += discount;
                    stackedPromotions.Add(new AppliedPromotionInfo(
                        promotion.Id,
                        promotion.Code,
                        promotion.Name,
                        promotion.Type.ToString(),
                        discount));

                    remainingTotal -= discount;

                    if (remainingTotal <= 0)
                        break;
                }
            }
        }

        // ============================================
        // Choose best strategy
        // ============================================
        var appliedPromotions = new List<AppliedPromotionInfo>();
        decimal totalDiscount;

        if (stackedDiscount > bestSinglePromotion.Discount)
        {
            // Stacking is better
            totalDiscount = stackedDiscount;
            appliedPromotions = stackedPromotions;

            _logger.LogInformation(
                "Best strategy: STACKING {Count} promotions, Total discount: {Discount:C}",
                stackedPromotions.Count, stackedDiscount);
        }
        else
        {
            // Single promotion is better
            totalDiscount = bestSinglePromotion.Discount;
            appliedPromotions.Add(new AppliedPromotionInfo(
                bestSinglePromotion.Promotion.Id,
                bestSinglePromotion.Promotion.Code,
                bestSinglePromotion.Promotion.Name,
                bestSinglePromotion.Promotion.Type.ToString(),
                bestSinglePromotion.Discount));

            _logger.LogInformation(
                "Best strategy: SINGLE promotion {Code}, Discount: {Discount:C}",
                bestSinglePromotion.Promotion.Code, bestSinglePromotion.Discount);
        }

        var finalAmount = orderTotal - totalDiscount;

        // Publish event
        await _messagePublisher.PublishAsync(
            new BestPromotionSelectedEvent(
                Guid.NewGuid(), // Order ID would be passed in real scenario
                appliedPromotions.Select(p => new PromotionApplicationResult(
                    p.PromotionId, p.Code, p.Discount)).ToList(),
                totalDiscount,
                orderTotal,
                finalAmount),
            "promotion-events");

        return new PromotionCalculationResult(
            orderTotal,
            finalAmount,
            totalDiscount,
            appliedPromotions);
    }

    public async Task<bool> ValidatePromotionCodeAsync(string code, string? customerEmail)
    {
        var promotion = await _promotionRepo.GetByCodeAsync(code);
        if (promotion == null || !promotion.IsValid())
            return false;

        if (promotion.MaxUsagePerCustomer.HasValue && !string.IsNullOrEmpty(customerEmail))
        {
            var customerUsage = await GetCustomerUsageCountAsync(promotion.Id, customerEmail);
            if (customerUsage >= promotion.MaxUsagePerCustomer.Value)
                return false;
        }

        return true;
    }

    public async Task<PromotionDto> ApplyPromotionToOrderAsync(Guid orderId, string promotionCode)
    {
        var order = await _orderRepo.GetByIdAsync(orderId);
        if (order == null)
            throw new OrderNotFoundException(orderId);

        var promotion = await _promotionRepo.GetByCodeAsync(promotionCode);
        if (promotion == null)
            throw new PromotionNotFoundException(promotionCode);

        var customerUsage = await GetCustomerUsageCountAsync(
            promotion.Id, order.CustomerEmail);

        if (!promotion.CanApplyToOrder(
            order.TotalAmount, order.CustomerEmail, customerUsage, null))
        {
            throw new InvalidOperationException(
                $"Promotion {promotionCode} cannot be applied to this order");
        }

        // Calculate discount (simplified - real implementation would get product IDs)
        var discount = promotion.CalculateDiscount(order.TotalAmount, new List<Guid>());

        // Record usage
        var usage = new PromotionUsage(
            promotion.Id,
            orderId,
            order.CustomerEmail,
            order.TotalAmount,
            discount);

        await _usageRepo.AddAsync(usage);

        // Update promotion usage count
        promotion.IncrementUsage(order.CustomerEmail);
        await _promotionRepo.UpdateAsync(promotion);
        await PublishDomainEvents(promotion);

        _logger.LogInformation(
            "Promotion {Code} applied to Order {OrderId}, Discount: {Discount:C}",
            promotionCode, orderId, discount);

        return MapToDto(promotion);
    }

    public async Task<int> GetCustomerUsageCountAsync(Guid promotionId, string customerEmail)
    {
        if (string.IsNullOrEmpty(customerEmail))
            return 0;

        return await _usageRepo.GetCustomerUsageCountAsync(promotionId, customerEmail);
    }

    public async Task<IEnumerable<PromotionUsageDto>> GetPromotionUsageHistoryAsync(Guid promotionId)
    {
        var usages = await _usageRepo.GetByPromotionIdAsync(promotionId);
        return usages.Select(MapToDto);
    }

    public async Task<PromotionAnalyticsDto> GetPromotionAnalyticsAsync(Guid promotionId)
    {
        var promotion = await _promotionRepo.GetByIdAsync(promotionId);
        if (promotion == null)
            throw new PromotionNotFoundException(promotionId);

        var usages = await _usageRepo.GetByPromotionIdAsync(promotionId);

        var totalUsageCount = usages.Count();
        var totalDiscountGiven = usages.Sum(u => u.DiscountAmount);
        var totalOrderValue = usages.Sum(u => u.OrderTotal);
        var uniqueCustomers = usages.Select(u => u.CustomerEmail).Distinct().Count();
        var averageDiscount = totalUsageCount > 0 ? totalDiscountGiven / totalUsageCount : 0;

        return new PromotionAnalyticsDto(
            promotionId,
            promotion.Code,
            promotion.Name,
            totalUsageCount,
            totalDiscountGiven,
            totalOrderValue,
            uniqueCustomers,
            averageDiscount,
            promotion.StartDate,
            promotion.EndDate,
            promotion.IsActive
        );
    }

    public async Task<IEnumerable<PromotionEffectivenessDto>> GetPromotionEffectivenessReportAsync()
    {
        var allPromotions = await _promotionRepo.GetAllAsync();
        var results = new List<PromotionEffectivenessDto>();

        foreach (var promotion in allPromotions)
        {
            var usages = await _usageRepo.GetByPromotionIdAsync(promotion.Id);
            
            var totalUsage = usages.Count();
            var totalDiscount = usages.Sum(u => u.DiscountAmount);
            var totalRevenue = usages.Sum(u => u.OrderTotal);
            var conversionRate = promotion.MaxUsageCount.HasValue 
                ? (decimal)totalUsage / promotion.MaxUsageCount.Value * 100
                : 0;

            results.Add(new PromotionEffectivenessDto(
                promotion.Id,
                promotion.Code,
                promotion.Name,
                promotion.Type.ToString(),
                totalUsage,
                totalDiscount,
                totalRevenue,
                conversionRate
            ));
        }

        return results.OrderByDescending(r => r.TotalRevenue);
    }

    private async Task PublishDomainEvents(Promotion promotion)
    {
        foreach (var domainEvent in promotion.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "promotion-events");
        }
        promotion.ClearDomainEvents();
    }

    private static PromotionDto MapToDto(Promotion promotion) => new(
        promotion.Id,
        promotion.Code,
        promotion.Name,
        promotion.Description,
        promotion.Type.ToString(),
        promotion.DiscountValue,
        promotion.MaxDiscountAmount,
        promotion.MinimumPurchaseAmount,
        promotion.IsActive,
        promotion.IsStackable,
        promotion.Priority,
        promotion.StartDate,
        promotion.EndDate,
        promotion.MaxUsageCount,
        promotion.CurrentUsageCount,
        promotion.MaxUsagePerCustomer,
        promotion.RequiresCouponCode,
        promotion.TargetCustomerSegment
    );

    private static PromotionUsageDto MapToDto(PromotionUsage usage) => new(
        usage.Id,
        usage.PromotionId,
        usage.OrderId,
        usage.CustomerEmail,
        usage.OrderTotal,
        usage.DiscountAmount,
        usage.UsedAt
    );
}