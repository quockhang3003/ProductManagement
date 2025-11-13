using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Repositories;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByIdAsync(Guid id);
    Task<InventoryItem?> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId);
    Task<IEnumerable<InventoryItem>> GetAllAsync();
    Task<IEnumerable<InventoryItem>> GetByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<InventoryItem>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync();
    Task<int> GetTotalAvailableStockAsync(Guid productId);
    Task<InventoryItem> AddAsync(InventoryItem item);
    Task UpdateAsync(InventoryItem item);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    
    // For reservations
    Task<List<(InventoryItem Item, int Quantity)>> GetReservationsByOrderAsync(Guid orderId);
}