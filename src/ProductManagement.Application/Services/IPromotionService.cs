using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Services;

public interface IPromotionService
{
    // Promotion management
    Task<PromotionDto> CreatePromotionAsync(CreatePromotionDto dto);
    Task<PromotionDto?> GetPromotionByIdAsync(Guid id);
    Task<PromotionDto?> GetPromotionByCodeAsync(string code);
    Task<IEnumerable<PromotionDto>> GetActivePromotionsAsync();
    Task ActivatePromotionAsync(Guid id);
    Task DeactivatePromotionAsync(Guid id);
    
    // Promotion application (THE CORE ALGORITHM)
    Task<PromotionCalculationResult> CalculateBestPromotionsAsync(
        decimal orderTotal,
        List<Guid> productIds,
        string? customerEmail,
        string? customerSegment,
        string? couponCode = null);
    
    Task<bool> ValidatePromotionCodeAsync(string code, string? customerEmail);
    Task<PromotionDto> ApplyPromotionToOrderAsync(Guid orderId, string promotionCode);
    
    // Usage tracking
    Task<int> GetCustomerUsageCountAsync(Guid promotionId, string customerEmail);
    Task<IEnumerable<PromotionUsageDto>> GetPromotionUsageHistoryAsync(Guid promotionId);
    
    // Analytics
    Task<PromotionAnalyticsDto> GetPromotionAnalyticsAsync(Guid promotionId);
    Task<IEnumerable<PromotionEffectivenessDto>> GetPromotionEffectivenessReportAsync();
}