using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Repositories;

public interface IPaymentRepository
{
    Task<Payment?> GetByIdAsync(Guid id);
    Task<Payment?> GetByOrderIdAsync(Guid orderId);
    Task<IEnumerable<Payment>> GetAllAsync();
    Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status);
    Task<IEnumerable<Payment>> GetFailedPaymentsAsync();
    Task<IEnumerable<Payment>> GetExpiredPaymentsAsync();
    Task<IEnumerable<Payment>> GetPaymentsBetweenDatesAsync(DateTime from, DateTime to);
    Task<Payment> AddAsync(Payment payment);
    Task UpdateAsync(Payment payment);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
    Task<Dictionary<PaymentMethod, int>> GetPaymentCountByMethodAsync(DateTime from, DateTime to);
}