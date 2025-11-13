using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Services;

public interface IPaymentService
{
    // Payment lifecycle
    Task<PaymentDto> CreatePaymentAsync(CreatePaymentDto dto);
    Task<PaymentDto> AuthorizePaymentAsync(Guid paymentId, AuthorizePaymentDto dto);
    Task<PaymentDto> CapturePaymentAsync(Guid paymentId);
    Task RetryFailedPaymentAsync(Guid paymentId);
    Task<PaymentDto> RefundPaymentAsync(Guid paymentId, RefundPaymentDto dto);
    
    // Queries
    Task<PaymentDto?> GetPaymentByIdAsync(Guid id);
    Task<PaymentDto?> GetPaymentByOrderIdAsync(Guid orderId);
    Task<IEnumerable<PaymentDto>> GetPaymentsByStatusAsync(string status);
    Task<IEnumerable<PaymentDto>> GetFailedPaymentsAsync();
    Task<IEnumerable<PaymentDto>> GetExpiredPaymentsAsync();
    
    // Analytics
    Task<PaymentAnalyticsDto> GetPaymentAnalyticsAsync(DateTime from, DateTime to);
    Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to);
    
    // Payment gateway management
    Task<PaymentGatewayConfigDto> AddPaymentGatewayAsync(CreatePaymentGatewayDto dto);
    Task<IEnumerable<PaymentGatewayConfigDto>> GetActivePaymentGatewaysAsync();
}