using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Repositories;
using ProductManagement.Infrastructure.Data;

namespace ProductManagement.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<OrderRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public OrderRepository(DapperContext context, ILogger<OrderRepository> logger)
    {
        _context = context;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<NpgsqlException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Database retry {RetryCount} after {TimeSpan}s due to {Exception}",
                        retryCount, timeSpan.TotalSeconds, exception.Message);
                });
    }

    public async Task<Order?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, customer_name as CustomerName, customer_email as CustomerEmail,
                       shipping_address as ShippingAddress, phone_number as PhoneNumber,
                       status as Status, total_amount as TotalAmount, created_at as CreatedAt,
                       confirmed_at as ConfirmedAt, shipped_at as ShippedAt,
                       delivered_at as DeliveredAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason, tracking_number as TrackingNumber
                FROM orders 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Order>(query, new { Id = id });
        });
    }

    public async Task<Order?> GetByIdWithItemsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string orderQuery = @"
                SELECT id, customer_name as CustomerName, customer_email as CustomerEmail,
                       shipping_address as ShippingAddress, phone_number as PhoneNumber,
                       status as Status, total_amount as TotalAmount, created_at as CreatedAt,
                       confirmed_at as ConfirmedAt, shipped_at as ShippedAt,
                       delivered_at as DeliveredAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason, tracking_number as TrackingNumber
                FROM orders 
                WHERE id = @Id";

            const string itemsQuery = @"
                SELECT id, order_id as OrderId, product_id as ProductId,
                       product_name as ProductName, quantity as Quantity,
                       unit_price as UnitPrice
                FROM order_items
                WHERE order_id = @OrderId";

            using var connection = _context.CreateConnection();
            
            var order = await connection.QueryFirstOrDefaultAsync<Order>(orderQuery, new { Id = id });
            if (order == null) return null;

            var items = await connection.QueryAsync<OrderItem>(itemsQuery, new { OrderId = id });
            
            // Use reflection to set private field (workaround for Dapper)
            var itemsField = typeof(Order).GetField("_items", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            itemsField?.SetValue(order, items.ToList());

            return order;
        });
    }

    public async Task<IEnumerable<Order>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, customer_name as CustomerName, customer_email as CustomerEmail,
                       shipping_address as ShippingAddress, phone_number as PhoneNumber,
                       status as Status, total_amount as TotalAmount, created_at as CreatedAt,
                       confirmed_at as ConfirmedAt, shipped_at as ShippedAt,
                       delivered_at as DeliveredAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason, tracking_number as TrackingNumber
                FROM orders 
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Order>(query);
        });
    }

    public async Task<IEnumerable<Order>> GetByCustomerEmailAsync(string email)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, customer_name as CustomerName, customer_email as CustomerEmail,
                       shipping_address as ShippingAddress, phone_number as PhoneNumber,
                       status as Status, total_amount as TotalAmount, created_at as CreatedAt,
                       confirmed_at as ConfirmedAt, shipped_at as ShippedAt,
                       delivered_at as DeliveredAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason, tracking_number as TrackingNumber
                FROM orders 
                WHERE customer_email = @Email
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Order>(query, new { Email = email });
        });
    }

    public async Task<IEnumerable<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, customer_name as CustomerName, customer_email as CustomerEmail,
                       shipping_address as ShippingAddress, phone_number as PhoneNumber,
                       status as Status, total_amount as TotalAmount, created_at as CreatedAt,
                       confirmed_at as ConfirmedAt, shipped_at as ShippedAt,
                       delivered_at as DeliveredAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason, tracking_number as TrackingNumber
                FROM orders 
                WHERE status = @Status
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Order>(query, new { Status = (int)status });
        });
    }

    public async Task<Order> AddAsync(Order order)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert order
                const string orderQuery = @"
                    INSERT INTO orders (
                        id, customer_name, customer_email, shipping_address, phone_number,
                        status, total_amount, created_at
                    ) VALUES (
                        @Id, @CustomerName, @CustomerEmail, @ShippingAddress, @PhoneNumber,
                        @Status, @TotalAmount, @CreatedAt
                    )";

                await connection.ExecuteAsync(orderQuery, new
                {
                    order.Id,
                    order.CustomerName,
                    order.CustomerEmail,
                    order.ShippingAddress,
                    order.PhoneNumber,
                    Status = (int)order.Status,
                    order.TotalAmount,
                    order.CreatedAt
                }, transaction);

                // Insert order items
                const string itemQuery = @"
                    INSERT INTO order_items (
                        id, order_id, product_id, product_name, quantity, unit_price
                    ) VALUES (
                        @Id, @OrderId, @ProductId, @ProductName, @Quantity, @UnitPrice
                    )";

                foreach (var item in order.Items)
                {
                    await connection.ExecuteAsync(itemQuery, new
                    {
                        item.Id,
                        OrderId = order.Id,
                        item.ProductId,
                        item.ProductName,
                        item.Quantity,
                        item.UnitPrice
                    }, transaction);
                }

                transaction.Commit();
                _logger.LogInformation("Order {OrderId} created successfully", order.Id);
                
                return order;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    public async Task UpdateAsync(Order order)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE orders 
                SET status = @Status,
                    confirmed_at = @ConfirmedAt,
                    shipped_at = @ShippedAt,
                    delivered_at = @DeliveredAt,
                    cancelled_at = @CancelledAt,
                    cancellation_reason = @CancellationReason,
                    tracking_number = @TrackingNumber
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                order.Id,
                Status = (int)order.Status,
                order.ConfirmedAt,
                order.ShippedAt,
                order.DeliveredAt,
                order.CancelledAt,
                order.CancellationReason,
                order.TrackingNumber
            });

            _logger.LogInformation("Order {OrderId} updated to status {Status}", 
                order.Id, order.Status);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            using var connection = _context.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(
                    "DELETE FROM order_items WHERE order_id = @Id", 
                    new { Id = id }, 
                    transaction);

                await connection.ExecuteAsync(
                    "DELETE FROM orders WHERE id = @Id", 
                    new { Id = id }, 
                    transaction);

                transaction.Commit();
                _logger.LogInformation("Order {OrderId} deleted", id);
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM orders WHERE id = @Id)";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }

    public async Task<int> GetOrderCountByStatusAsync(OrderStatus status)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT COUNT(*) FROM orders WHERE status = @Status";
            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, new { Status = (int)status });
        });
    }

    public async Task<decimal> GetTotalRevenueAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT COALESCE(SUM(total_amount), 0)
                FROM orders 
                WHERE status = @Status";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(query, 
                new { Status = (int)OrderStatus.Delivered });
        });
    }
}