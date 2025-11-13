using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using Polly;
using Polly.Retry;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Repositories;
using ProductManagement.Infrastructure.Data;

namespace ProductManagement.Infrastructure.Repositories;

public class PromotionRepository : IPromotionRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<PromotionRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PromotionRepository(
        DapperContext context, 
        ILogger<PromotionRepository> logger)
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

    public async Task<Promotion?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string promotionQuery = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                WHERE id = @Id";

            const string rulesQuery = @"
                SELECT id, promotion_id as PromotionId, type, 
                       target_product_ids as TargetProductIds,
                       target_category_ids as TargetCategoryIds,
                       min_quantity as MinQuantity, min_amount as MinAmount,
                       custom_condition as CustomCondition
                FROM promotion_rules 
                WHERE promotion_id = @PromotionId";

            using var connection = _context.CreateConnection();
            
            var promotion = await connection.QueryFirstOrDefaultAsync<Promotion>(
                promotionQuery, new { Id = id });
            
            if (promotion != null)
            {
                var rules = await connection.QueryAsync<PromotionRule>(
                    rulesQuery, new { PromotionId = id });
                
                foreach (var rule in rules)
                {
                    promotion.AddRule(rule);
                }
            }
            
            return promotion;
        });
    }

    public async Task<Promotion?> GetByCodeAsync(string code)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string promotionQuery = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                WHERE UPPER(code) = UPPER(@Code)";

            const string rulesQuery = @"
                SELECT id, promotion_id as PromotionId, type, 
                       target_product_ids as TargetProductIds,
                       target_category_ids as TargetCategoryIds,
                       min_quantity as MinQuantity, min_amount as MinAmount,
                       custom_condition as CustomCondition
                FROM promotion_rules 
                WHERE promotion_id = @PromotionId";

            using var connection = _context.CreateConnection();
            
            var promotion = await connection.QueryFirstOrDefaultAsync<Promotion>(
                promotionQuery, new { Code = code });
            
            if (promotion != null)
            {
                var rules = await connection.QueryAsync<PromotionRule>(
                    rulesQuery, new { PromotionId = promotion.Id });
                
                foreach (var rule in rules)
                {
                    promotion.AddRule(rule);
                }
            }
            
            return promotion;
        });
    }

    public async Task<IEnumerable<Promotion>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                ORDER BY priority DESC, created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Promotion>(query);
        });
    }

    public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                WHERE is_active = true
                  AND start_date <= CURRENT_TIMESTAMP
                  AND end_date >= CURRENT_TIMESTAMP
                  AND (max_usage_count IS NULL OR current_usage_count < max_usage_count)
                ORDER BY priority DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Promotion>(query);
        });
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsByTypeAsync(PromotionType type)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                WHERE type = @Type
                ORDER BY priority DESC, created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Promotion>(query, new { Type = (int)type });
        });
    }

    public async Task<IEnumerable<Promotion>> GetPromotionsByDateRangeAsync(
        DateTime from, 
        DateTime to)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                WHERE start_date <= @To AND end_date >= @From
                ORDER BY start_date";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Promotion>(query, new { From = from, To = to });
        });
    }

    public async Task<IEnumerable<Promotion>> GetStackablePromotionsAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, description, type, discount_value as DiscountValue,
                       max_discount_amount as MaxDiscountAmount, 
                       minimum_purchase_amount as MinimumPurchaseAmount,
                       is_active as IsActive, is_stackable as IsStackable, 
                       priority, start_date as StartDate, end_date as EndDate,
                       max_usage_count as MaxUsageCount, 
                       current_usage_count as CurrentUsageCount,
                       max_usage_per_customer as MaxUsagePerCustomer,
                       requires_coupon_code as RequiresCouponCode,
                       created_at as CreatedAt,
                       target_customer_segment as TargetCustomerSegment
                FROM promotions 
                WHERE is_stackable = true
                  AND is_active = true
                  AND start_date <= CURRENT_TIMESTAMP
                  AND end_date >= CURRENT_TIMESTAMP
                ORDER BY priority DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Promotion>(query);
        });
    }

    public async Task<Promotion> AddAsync(Promotion promotion)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string promotionQuery = @"
                INSERT INTO promotions (
                    id, code, name, description, type, discount_value,
                    max_discount_amount, minimum_purchase_amount,
                    is_active, is_stackable, priority, start_date, end_date,
                    max_usage_count, current_usage_count, max_usage_per_customer,
                    requires_coupon_code, created_at, target_customer_segment
                ) VALUES (
                    @Id, @Code, @Name, @Description, @Type, @DiscountValue,
                    @MaxDiscountAmount, @MinimumPurchaseAmount,
                    @IsActive, @IsStackable, @Priority, @StartDate, @EndDate,
                    @MaxUsageCount, @CurrentUsageCount, @MaxUsagePerCustomer,
                    @RequiresCouponCode, @CreatedAt, @TargetCustomerSegment
                )";

            const string ruleQuery = @"
                INSERT INTO promotion_rules (
                    id, promotion_id, type, target_product_ids, 
                    target_category_ids, min_quantity, min_amount, custom_condition
                ) VALUES (
                    @Id, @PromotionId, @Type, @TargetProductIds,
                    @TargetCategoryIds, @MinQuantity, @MinAmount, @CustomCondition
                )";

            using var connection = _context.CreateConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(promotionQuery, new
                {
                    promotion.Id,
                    promotion.Code,
                    promotion.Name,
                    promotion.Description,
                    Type = (int)promotion.Type,
                    promotion.DiscountValue,
                    promotion.MaxDiscountAmount,
                    promotion.MinimumPurchaseAmount,
                    promotion.IsActive,
                    promotion.IsStackable,
                    promotion.Priority,
                    promotion.StartDate,
                    promotion.EndDate,
                    promotion.MaxUsageCount,
                    promotion.CurrentUsageCount,
                    promotion.MaxUsagePerCustomer,
                    promotion.RequiresCouponCode,
                    promotion.CreatedAt,
                    promotion.TargetCustomerSegment
                }, transaction);

                foreach (var rule in promotion.Rules)
                {
                    await connection.ExecuteAsync(ruleQuery, new
                    {
                        rule.Id,
                        rule.PromotionId,
                        Type = (int)rule.Type,
                        rule.TargetProductIds,
                        rule.TargetCategoryIds,
                        rule.MinQuantity,
                        rule.MinAmount,
                        rule.CustomCondition
                    }, transaction);
                }

                transaction.Commit();

                _logger.LogInformation(
                    "Promotion {Code} created with {RuleCount} rules", 
                    promotion.Code, promotion.Rules.Count);
                
                return promotion;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        });
    }

    public async Task UpdateAsync(Promotion promotion)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE promotions 
                SET name = @Name,
                    description = @Description,
                    discount_value = @DiscountValue,
                    max_discount_amount = @MaxDiscountAmount,
                    minimum_purchase_amount = @MinimumPurchaseAmount,
                    is_active = @IsActive,
                    is_stackable = @IsStackable,
                    priority = @Priority,
                    end_date = @EndDate,
                    max_usage_count = @MaxUsageCount,
                    current_usage_count = @CurrentUsageCount,
                    max_usage_per_customer = @MaxUsagePerCustomer,
                    target_customer_segment = @TargetCustomerSegment
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                promotion.Id,
                promotion.Name,
                promotion.Description,
                promotion.DiscountValue,
                promotion.MaxDiscountAmount,
                promotion.MinimumPurchaseAmount,
                promotion.IsActive,
                promotion.IsStackable,
                promotion.Priority,
                promotion.EndDate,
                promotion.MaxUsageCount,
                promotion.CurrentUsageCount,
                promotion.MaxUsagePerCustomer,
                promotion.TargetCustomerSegment
            });

            _logger.LogInformation("Promotion {PromotionId} updated", promotion.Id);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM promotions WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });

            _logger.LogInformation("Promotion {PromotionId} deleted", id);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM promotions WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT EXISTS(SELECT 1 FROM promotions WHERE UPPER(code) = UPPER(@Code))";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Code = code });
        });
    }
}