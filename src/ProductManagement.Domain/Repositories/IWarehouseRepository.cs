using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Repositories;

public interface IWarehouseRepository
{
    Task<Warehouse?> GetByIdAsync(Guid id);
    Task<Warehouse?> GetByCodeAsync(string code);
    Task<IEnumerable<Warehouse>> GetAllAsync();
    Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync();
    Task<Warehouse> AddAsync(Warehouse warehouse);
    Task UpdateAsync(Warehouse warehouse);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> CodeExistsAsync(string code);
}