using ProductManagement.Domain.Entities;

namespace ProductManagement.Domain.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByIdWithItemsAsync(Guid id);
    Task<IEnumerable<Order>> GetAllAsync();
    Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email);
    Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status);
    Task<Order> AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetOrderCountByStatusAsync(OrderStatus status);
    Task<decimal> GetTotalRevenueAsync();
}