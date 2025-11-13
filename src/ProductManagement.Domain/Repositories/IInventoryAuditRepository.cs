using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Repositories;

public interface IInventoryAuditRepository
{
    Task<InventoryAudit?> GetByIdAsync(Guid id);
    Task<IEnumerable<InventoryAudit>> GetAllAsync();
    Task<IEnumerable<InventoryAudit>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<InventoryAudit>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<InventoryAudit>> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId);
    Task<IEnumerable<InventoryAudit>> GetAuditsWithVarianceAsync();
    Task<InventoryAudit> AddAsync(InventoryAudit audit);
    Task<bool> ExistsAsync(Guid id);
}