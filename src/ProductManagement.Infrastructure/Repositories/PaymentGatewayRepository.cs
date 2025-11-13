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

public class PaymentGatewayRepository : IPaymentGatewayRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<PaymentGatewayRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PaymentGatewayRepository(
        DapperContext context, 
        ILogger<PaymentGatewayRepository> logger)
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

    public async Task<PaymentGatewayConfig?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, supported_method as SupportedMethod, 
                       is_active as IsActive, api_key as ApiKey, 
                       api_secret as ApiSecret, webhook_url as WebhookUrl,
                       timeout_seconds as TimeoutSeconds, 
                       transaction_fee_percentage as TransactionFeePercentage,
                       minimum_amount as MinimumAmount, 
                       maximum_amount as MaximumAmount
                FROM payment_gateway_configs 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<PaymentGatewayConfig>(
                query, new { Id = id });
        });
    }

    public async Task<PaymentGatewayConfig?> GetByPaymentMethodAsync(PaymentMethod method)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, supported_method as SupportedMethod, 
                       is_active as IsActive, api_key as ApiKey, 
                       api_secret as ApiSecret, webhook_url as WebhookUrl,
                       timeout_seconds as TimeoutSeconds, 
                       transaction_fee_percentage as TransactionFeePercentage,
                       minimum_amount as MinimumAmount, 
                       maximum_amount as MaximumAmount
                FROM payment_gateway_configs 
                WHERE supported_method = @Method 
                  AND is_active = true
                LIMIT 1";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<PaymentGatewayConfig>(
                query, new { Method = (int)method });
        });
    }

    public async Task<IEnumerable<PaymentGatewayConfig>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, supported_method as SupportedMethod, 
                       is_active as IsActive, api_key as ApiKey, 
                       api_secret as ApiSecret, webhook_url as WebhookUrl,
                       timeout_seconds as TimeoutSeconds, 
                       transaction_fee_percentage as TransactionFeePercentage,
                       minimum_amount as MinimumAmount, 
                       maximum_amount as MaximumAmount
                FROM payment_gateway_configs 
                ORDER BY name";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PaymentGatewayConfig>(query);
        });
    }

    public async Task<IEnumerable<PaymentGatewayConfig>> GetActiveGatewaysAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, supported_method as SupportedMethod, 
                       is_active as IsActive, api_key as ApiKey, 
                       api_secret as ApiSecret, webhook_url as WebhookUrl,
                       timeout_seconds as TimeoutSeconds, 
                       transaction_fee_percentage as TransactionFeePercentage,
                       minimum_amount as MinimumAmount, 
                       maximum_amount as MaximumAmount
                FROM payment_gateway_configs 
                WHERE is_active = true
                ORDER BY name";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<PaymentGatewayConfig>(query);
        });
    }

    public async Task<PaymentGatewayConfig> AddAsync(PaymentGatewayConfig gateway)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO payment_gateway_configs (
                    id, name, supported_method, is_active, api_key, 
                    api_secret, webhook_url, timeout_seconds,
                    transaction_fee_percentage, minimum_amount, maximum_amount
                ) VALUES (
                    @Id, @Name, @SupportedMethod, @IsActive, @ApiKey,
                    @ApiSecret, @WebhookUrl, @TimeoutSeconds,
                    @TransactionFeePercentage, @MinimumAmount, @MaximumAmount
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                gateway.Id,
                gateway.Name,
                SupportedMethod = (int)gateway.SupportedMethod,
                gateway.IsActive,
                gateway.ApiKey,
                gateway.ApiSecret,
                gateway.WebhookUrl,
                gateway.TimeoutSeconds,
                gateway.TransactionFeePercentage,
                gateway.MinimumAmount,
                gateway.MaximumAmount
            });

            _logger.LogInformation(
                "Payment gateway {Name} added for method {Method}", 
                gateway.Name, gateway.SupportedMethod);
            
            return gateway;
        });
    }

    public async Task UpdateAsync(PaymentGatewayConfig gateway)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE payment_gateway_configs 
                SET name = @Name,
                    is_active = @IsActive,
                    api_key = @ApiKey,
                    api_secret = @ApiSecret,
                    webhook_url = @WebhookUrl,
                    timeout_seconds = @TimeoutSeconds,
                    transaction_fee_percentage = @TransactionFeePercentage,
                    minimum_amount = @MinimumAmount,
                    maximum_amount = @MaximumAmount
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                gateway.Id,
                gateway.Name,
                gateway.IsActive,
                gateway.ApiKey,
                gateway.ApiSecret,
                gateway.WebhookUrl,
                gateway.TimeoutSeconds,
                gateway.TransactionFeePercentage,
                gateway.MinimumAmount,
                gateway.MaximumAmount
            });

            _logger.LogInformation("Payment gateway {GatewayId} updated", gateway.Id);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM payment_gateway_configs WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });

            _logger.LogInformation("Payment gateway {GatewayId} deleted", id);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT EXISTS(SELECT 1 FROM payment_gateway_configs WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }
}