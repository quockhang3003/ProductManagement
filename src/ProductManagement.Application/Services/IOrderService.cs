using ProductManagement.Application.DTOs;

namespace ProductManagement.Application.Services;

public interface IOrderService
{
    Task<OrderDto?> GetOrderByIdAsync(Guid id);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<IEnumerable<OrderDto>> GetOrdersByCustomerEmailAsync(string email);
    Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status);
    Task<OrderDto> CreateOrderAsync(CreateOrderDto dto);
    Task ConfirmOrderAsync(Guid id);
    Task ShipOrderAsync(Guid id, ShipOrderDto dto);
    Task DeliverOrderAsync(Guid id);
    Task CancelOrderAsync(Guid id, CancelOrderDto dto);
    Task<Dictionary<string, int>> GetOrderStatisticsAsync();
}