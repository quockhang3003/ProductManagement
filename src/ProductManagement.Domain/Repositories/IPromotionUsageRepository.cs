using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Repositories;

public interface IPromotionUsageRepository
{
    Task<IEnumerable<PromotionUsage>> GetAllAsync();
    Task<IEnumerable<PromotionUsage>> GetByPromotionIdAsync(Guid promotionId);
    Task<IEnumerable<PromotionUsage>> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PromotionUsage>> GetByCustomerEmailAsync(string customerEmail);
    Task<int> GetCustomerUsageCountAsync(Guid promotionId, string customerEmail);
    Task<decimal> GetTotalDiscountGivenAsync(Guid promotionId);
    Task<PromotionUsage> AddAsync(PromotionUsage usage);
    Task<bool> ExistsAsync(Guid id);

}