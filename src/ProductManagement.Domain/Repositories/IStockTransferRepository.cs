using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Repositories;

public interface IStockTransferRepository
{
    Task<StockTransfer?> GetByIdAsync(Guid id);
    Task<IEnumerable<StockTransfer>> GetAllAsync();
    Task<IEnumerable<StockTransfer>> GetByStatusAsync(StockTransferStatus status);
    Task<IEnumerable<StockTransfer>> GetByProductIdAsync(Guid productId);
    Task<IEnumerable<StockTransfer>> GetByWarehouseAsync(Guid warehouseId);
    Task<IEnumerable<StockTransfer>> GetPendingTransfersOlderThanAsync(TimeSpan age);
    Task<StockTransfer> AddAsync(StockTransfer transfer);
    Task UpdateAsync(StockTransfer transfer);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}