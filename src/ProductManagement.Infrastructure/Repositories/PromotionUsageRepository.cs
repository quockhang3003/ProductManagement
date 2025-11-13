using System;
using System.Collections.Generic;
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

public class PromotionUsageRepository : IPromotionUsageRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<PromotionUsageRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PromotionUsageRepository(
        DapperContext context, 
        ILogger<PromotionUsageRepository> logger)
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

    public async Task<IEnumerable<PromotionUsage>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, promotion_id as PromotionId, order_id as OrderId,
                       customer_email as CustomerEmail, order_total as OrderTotal,
                       discount_amount as DiscountAmount, used_at as UsedAt
                FROM promotion_usages 
                ORDER BY used_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PromotionUsage>(query);
        });
    }

    public async Task<IEnumerable<PromotionUsage>> GetByPromotionIdAsync(Guid promotionId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, promotion_id as PromotionId, order_id as OrderId,
                       customer_email as CustomerEmail, order_total as OrderTotal,
                       discount_amount as DiscountAmount, used_at as UsedAt
                FROM promotion_usages 
                WHERE promotion_id = @PromotionId
                ORDER BY used_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PromotionUsage>(
                query, new { PromotionId = promotionId });
        });
    }

    public async Task<IEnumerable<PromotionUsage>> GetByOrderIdAsync(Guid orderId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, promotion_id as PromotionId, order_id as OrderId,
                       customer_email as CustomerEmail, order_total as OrderTotal,
                       discount_amount as DiscountAmount, used_at as UsedAt
                FROM promotion_usages 
                WHERE order_id = @OrderId
                ORDER BY used_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PromotionUsage>(
                query, new { OrderId = orderId });
        });
    }

    public async Task<IEnumerable<PromotionUsage>> GetByCustomerEmailAsync(
        string customerEmail)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, promotion_id as PromotionId, order_id as OrderId,
                       customer_email as CustomerEmail, order_total as OrderTotal,
                       discount_amount as DiscountAmount, used_at as UsedAt
                FROM promotion_usages 
                WHERE customer_email = @CustomerEmail
                ORDER BY used_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PromotionUsage>(
                query, new { CustomerEmail = customerEmail });
        });
    }

    public async Task<int> GetCustomerUsageCountAsync(
        Guid promotionId, 
        string customerEmail)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT COUNT(*) 
                FROM promotion_usages 
                WHERE promotion_id = @PromotionId 
                  AND customer_email = @CustomerEmail";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(
                query, new { PromotionId = promotionId, CustomerEmail = customerEmail });
        });
    }

    public async Task<decimal> GetTotalDiscountGivenAsync(Guid promotionId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT COALESCE(SUM(discount_amount), 0)
                FROM promotion_usages 
                WHERE promotion_id = @PromotionId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(
                query, new { PromotionId = promotionId });
        });
    }

    public async Task<PromotionUsage> AddAsync(PromotionUsage usage)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO promotion_usages (
                    id, promotion_id, order_id, customer_email,
                    order_total, discount_amount, used_at
                ) VALUES (
                    @Id, @PromotionId, @OrderId, @CustomerEmail,
                    @OrderTotal, @DiscountAmount, @UsedAt
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                usage.Id,
                usage.PromotionId,
                usage.OrderId,
                usage.CustomerEmail,
                usage.OrderTotal,
                usage.DiscountAmount,
                usage.UsedAt
            });

            _logger.LogInformation(
                "Promotion usage recorded: Promotion {PromotionId}, Order {OrderId}, Discount {Discount:C}",
                usage.PromotionId, usage.OrderId, usage.DiscountAmount);
            
            return usage;
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT EXISTS(SELECT 1 FROM promotion_usages WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }
}