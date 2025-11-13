using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;

namespace ProductManagement.Domain.Repositories;

public interface IPaymentGatewayRepository
{
    Task<PaymentGatewayConfig?> GetByIdAsync(Guid id);
    Task<PaymentGatewayConfig?> GetByPaymentMethodAsync(PaymentMethod method);
    Task<IEnumerable<PaymentGatewayConfig>> GetAllAsync();
    Task<IEnumerable<PaymentGatewayConfig>> GetActiveGatewaysAsync();
    Task<PaymentGatewayConfig> AddAsync(PaymentGatewayConfig gateway);
    Task UpdateAsync(PaymentGatewayConfig gateway);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}