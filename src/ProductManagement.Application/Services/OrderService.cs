using Microsoft.Extensions.Logging;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Messaging;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Domain.Repositories;

namespace ProductManagement.Application.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<OrderService> _logger;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IMessagePublisher messagePublisher,
        ILogger<OrderService> logger)
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public async Task<OrderDto?> GetOrderByIdAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id);
        return order == null ? null : MapToDto(order);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _orderRepository.GetAllAsync();
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByCustomerEmailAsync(string email)
    {
        var orders = await _orderRepository.GetByCustomerEmailAsync(email);
        return orders.Select(MapToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
    {
        if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            throw new ArgumentException($"Invalid order status: {status}");

        var orders = await _orderRepository.GetByStatusAsync(orderStatus);
        return orders.Select(MapToDto);
    }

    public async Task<OrderDto> CreateOrderAsync(CreateOrderDto dto)
    {
        // Validate and get products
        var orderItems = new List<OrderItem>();
        
        foreach (var item in dto.Items)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            
            if (product == null)
                throw new ProductNotFoundException(item.ProductId);

            if (!product.IsActive)
                throw new InvalidOperationException($"Product {product.Name} is not active");

            if (product.Stock < item.Quantity)
                throw new InsufficientStockException(
                    product.Id,
                    product.Name,
                    item.Quantity,
                    product.Stock
                );

            orderItems.Add(new OrderItem(
                product.Id,
                product.Name,
                item.Quantity,
                product.Price
            ));
        }

        // Create order
        var order = new Order(
            dto.CustomerName,
            dto.CustomerEmail,
            dto.ShippingAddress,
            dto.PhoneNumber,
            orderItems
        );

        await _orderRepository.AddAsync(order);

        // Deduct stock for each product
        foreach (var item in orderItems)
        {
            var product = await _productRepository.GetByIdAsync(item.ProductId);
            if (product != null)
            {
                product.UpdateStock(-item.Quantity);
                await _productRepository.UpdateAsync(product);
                
                // Publish product stock events
                await PublishDomainEvents(product);
            }
        }

        // Publish order events
        await PublishDomainEvents(order);

        _logger.LogInformation(
            "Order {OrderId} created for {CustomerEmail} with {ItemCount} items, total: {Total:C}",
            order.Id, order.CustomerEmail, orderItems.Count, order.TotalAmount
        );

        return MapToDto(order);
    }

    public async Task ConfirmOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new OrderNotFoundException(id);

        order.Confirm();
        await _orderRepository.UpdateAsync(order);
        await PublishDomainEvents(order);

        _logger.LogInformation("Order {OrderId} confirmed", id);
    }

    public async Task ShipOrderAsync(Guid id, ShipOrderDto dto)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new OrderNotFoundException(id);

        order.Ship(dto.TrackingNumber);
        await _orderRepository.UpdateAsync(order);
        await PublishDomainEvents(order);

        _logger.LogInformation(
            "Order {OrderId} shipped with tracking: {TrackingNumber}",
            id, dto.TrackingNumber
        );
    }

    public async Task DeliverOrderAsync(Guid id)
    {
        var order = await _orderRepository.GetByIdAsync(id);
        if (order == null)
            throw new OrderNotFoundException(id);

        order.Deliver();
        await _orderRepository.UpdateAsync(order);
        await PublishDomainEvents(order);

        _logger.LogInformation("Order {OrderId} delivered", id);
    }

    public async Task CancelOrderAsync(Guid id, CancelOrderDto dto)
    {
        var order = await _orderRepository.GetByIdWithItemsAsync(id);
        if (order == null)
            throw new OrderNotFoundException(id);

        order.Cancel(dto.Reason);

        // Restore stock for cancelled orders (if not yet shipped/delivered)
        if (order.Status != OrderStatus.Delivered && order.Status != OrderStatus.Shipping)
        {
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.UpdateStock(item.Quantity); // Return stock
                    await _productRepository.UpdateAsync(product);
                    await PublishDomainEvents(product);
                }
            }
        }

        await _orderRepository.UpdateAsync(order);
        await PublishDomainEvents(order);

        _logger.LogInformation(
            "Order {OrderId} cancelled. Reason: {Reason}",
            id, dto.Reason
        );
    }

    public async Task<Dictionary<string, int>> GetOrderStatisticsAsync()
    {
        var stats = new Dictionary<string, int>();
        
        foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus)))
        {
            var count = await _orderRepository.GetOrderCountByStatusAsync(status);
            stats[status.ToString()] = count;
        }

        return stats;
    }

    private async Task PublishDomainEvents(Order order)
    {
        foreach (var domainEvent in order.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "order-events");
        }
        order.ClearDomainEvents();
    }

    private async Task PublishDomainEvents(Product product)
    {
        foreach (var domainEvent in product.DomainEvents)
        {
            await _messagePublisher.PublishAsync(domainEvent, "product-events");
        }
        product.ClearDomainEvents();
    }

    private static OrderDto MapToDto(Order order) => new(
        order.Id,
        order.CustomerName,
        order.CustomerEmail,
        order.ShippingAddress,
        order.PhoneNumber,
        order.Status.ToString(),
        order.TotalAmount,
        order.CreatedAt,
        order.ConfirmedAt,
        order.ShippedAt,
        order.DeliveredAt,
        order.CancelledAt,
        order.CancellationReason,
        order.TrackingNumber,
        order.Items.Select(i => new OrderItemDto(
            i.Id,
            i.ProductId,
            i.ProductName,
            i.Quantity,
            i.UnitPrice,
            i.SubTotal
        )).ToList()
    );
}