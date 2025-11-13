using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Repositories;

public interface IPromotionRepository
{
    Task<Promotion?> GetByIdAsync(Guid id);
    Task<Promotion?> GetByCodeAsync(string code);
    Task<IEnumerable<Promotion>> GetAllAsync();
    Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
    Task<IEnumerable<Promotion>> GetPromotionsByTypeAsync(PromotionType type);
    Task<IEnumerable<Promotion>> GetPromotionsByDateRangeAsync(DateTime from, DateTime to);
    Task<IEnumerable<Promotion>> GetStackablePromotionsAsync();
    Task<Promotion> AddAsync(Promotion promotion);
    Task UpdateAsync(Promotion promotion);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CodeExistsAsync(string code);
}