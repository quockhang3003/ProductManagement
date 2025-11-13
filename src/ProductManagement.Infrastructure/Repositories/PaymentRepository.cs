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
using ProductManagement.Domain.Enum;
using ProductManagement.Domain.Repositories;
using ProductManagement.Infrastructure.Data;

namespace ProductManagement.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<PaymentRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public PaymentRepository(DapperContext context, ILogger<PaymentRepository> logger)
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

    public async Task<Payment?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, order_id as OrderId, amount, currency, method, status,
                       payment_gateway as PaymentGateway, transaction_id as TransactionId,
                       authorization_code as AuthorizationCode, created_at as CreatedAt,
                       authorized_at as AuthorizedAt, captured_at as CapturedAt,
                       failed_at as FailedAt, refunded_at as RefundedAt,
                       failure_reason as FailureReason, retry_count as RetryCount,
                       expires_at as ExpiresAt, is_installment as IsInstallment,
                       installment_months as InstallmentMonths, installment_fee as InstallmentFee
                FROM payments 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Payment>(query, new { Id = id });
        });
    }

    public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, order_id as OrderId, amount, currency, method, status,
                       payment_gateway as PaymentGateway, transaction_id as TransactionId,
                       authorization_code as AuthorizationCode, created_at as CreatedAt,
                       authorized_at as AuthorizedAt, captured_at as CapturedAt,
                       failed_at as FailedAt, refunded_at as RefundedAt,
                       failure_reason as FailureReason, retry_count as RetryCount,
                       expires_at as ExpiresAt, is_installment as IsInstallment,
                       installment_months as InstallmentMonths, installment_fee as InstallmentFee
                FROM payments 
                WHERE order_id = @OrderId";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Payment>(query, new { OrderId = orderId });
        });
    }

    public async Task<IEnumerable<Payment>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, order_id as OrderId, amount, currency, method, status,
                       payment_gateway as PaymentGateway, transaction_id as TransactionId,
                       authorization_code as AuthorizationCode, created_at as CreatedAt,
                       authorized_at as AuthorizedAt, captured_at as CapturedAt,
                       failed_at as FailedAt, refunded_at as RefundedAt,
                       failure_reason as FailureReason, retry_count as RetryCount,
                       expires_at as ExpiresAt, is_installment as IsInstallment,
                       installment_months as InstallmentMonths, installment_fee as InstallmentFee
                FROM payments 
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Payment>(query);
        });
    }

    public async Task<IEnumerable<Payment>> GetByStatusAsync(PaymentStatus status)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, order_id as OrderId, amount, currency, method, status,
                       payment_gateway as PaymentGateway, transaction_id as TransactionId,
                       authorization_code as AuthorizationCode, created_at as CreatedAt,
                       authorized_at as AuthorizedAt, captured_at as CapturedAt,
                       failed_at as FailedAt, refunded_at as RefundedAt,
                       failure_reason as FailureReason, retry_count as RetryCount,
                       expires_at as ExpiresAt, is_installment as IsInstallment,
                       installment_months as InstallmentMonths, installment_fee as InstallmentFee
                FROM payments 
                WHERE status = @Status
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Payment>(query, new { Status = (int)status });
        });
    }

    public async Task<IEnumerable<Payment>> GetFailedPaymentsAsync()
    {
        return await GetByStatusAsync(PaymentStatus.Failed);
    }

    public async Task<IEnumerable<Payment>> GetExpiredPaymentsAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, order_id as OrderId, amount, currency, method, status,
                       payment_gateway as PaymentGateway, transaction_id as TransactionId,
                       authorization_code as AuthorizationCode, created_at as CreatedAt,
                       authorized_at as AuthorizedAt, captured_at as CapturedAt,
                       failed_at as FailedAt, refunded_at as RefundedAt,
                       failure_reason as FailureReason, retry_count as RetryCount,
                       expires_at as ExpiresAt, is_installment as IsInstallment,
                       installment_months as InstallmentMonths, installment_fee as InstallmentFee
                FROM payments 
                WHERE status = @PendingStatus 
                  AND expires_at < CURRENT_TIMESTAMP
                ORDER BY expires_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Payment>(query, 
                new { PendingStatus = (int)PaymentStatus.Pending });
        });
    }

    public async Task<IEnumerable<Payment>> GetPaymentsBetweenDatesAsync(DateTime from, DateTime to)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, order_id as OrderId, amount, currency, method, status,
                       payment_gateway as PaymentGateway, transaction_id as TransactionId,
                       authorization_code as AuthorizationCode, created_at as CreatedAt,
                       authorized_at as AuthorizedAt, captured_at as CapturedAt,
                       failed_at as FailedAt, refunded_at as RefundedAt,
                       failure_reason as FailureReason, retry_count as RetryCount,
                       expires_at as ExpiresAt, is_installment as IsInstallment,
                       installment_months as InstallmentMonths, installment_fee as InstallmentFee
                FROM payments 
                WHERE created_at >= @From AND created_at <= @To
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Payment>(query, new { From = from, To = to });
        });
    }

    public async Task<Payment> AddAsync(Payment payment)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO payments (
                    id, order_id, amount, currency, method, status,
                    payment_gateway, transaction_id, authorization_code,
                    created_at, authorized_at, captured_at, failed_at, refunded_at,
                    failure_reason, retry_count, expires_at,
                    is_installment, installment_months, installment_fee
                ) VALUES (
                    @Id, @OrderId, @Amount, @Currency, @Method, @Status,
                    @PaymentGateway, @TransactionId, @AuthorizationCode,
                    @CreatedAt, @AuthorizedAt, @CapturedAt, @FailedAt, @RefundedAt,
                    @FailureReason, @RetryCount, @ExpiresAt,
                    @IsInstallment, @InstallmentMonths, @InstallmentFee
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                payment.Id,
                payment.OrderId,
                payment.Amount,
                payment.Currency,
                Method = (int)payment.Method,
                Status = (int)payment.Status,
                payment.PaymentGateway,
                payment.TransactionId,
                payment.AuthorizationCode,
                payment.CreatedAt,
                payment.AuthorizedAt,
                payment.CapturedAt,
                payment.FailedAt,
                payment.RefundedAt,
                payment.FailureReason,
                payment.RetryCount,
                payment.ExpiresAt,
                payment.IsInstallment,
                payment.InstallmentMonths,
                payment.InstallmentFee
            });

            _logger.LogInformation("Payment {PaymentId} created for Order {OrderId}", 
                payment.Id, payment.OrderId);
            
            return payment;
        });
    }

    public async Task UpdateAsync(Payment payment)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE payments 
                SET status = @Status,
                    payment_gateway = @PaymentGateway,
                    transaction_id = @TransactionId,
                    authorization_code = @AuthorizationCode,
                    authorized_at = @AuthorizedAt,
                    captured_at = @CapturedAt,
                    failed_at = @FailedAt,
                    refunded_at = @RefundedAt,
                    failure_reason = @FailureReason,
                    retry_count = @RetryCount,
                    expires_at = @ExpiresAt
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                payment.Id,
                Status = (int)payment.Status,
                payment.PaymentGateway,
                payment.TransactionId,
                payment.AuthorizationCode,
                payment.AuthorizedAt,
                payment.CapturedAt,
                payment.FailedAt,
                payment.RefundedAt,
                payment.FailureReason,
                payment.RetryCount,
                payment.ExpiresAt
            });

            _logger.LogInformation("Payment {PaymentId} updated to status {Status}", 
                payment.Id, payment.Status);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM payments WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });

            _logger.LogInformation("Payment {PaymentId} deleted", id);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM payments WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }

    public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT COALESCE(SUM(amount), 0)
                FROM payments 
                WHERE status = @CapturedStatus
                  AND captured_at >= @From 
                  AND captured_at <= @To";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<decimal>(query, 
                new { CapturedStatus = (int)PaymentStatus.Captured, From = from, To = to });
        });
    }

    public async Task<Dictionary<PaymentMethod, int>> GetPaymentCountByMethodAsync(
        DateTime from, 
        DateTime to)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT method, COUNT(*) as count
                FROM payments 
                WHERE created_at >= @From AND created_at <= @To
                GROUP BY method";

            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<dynamic>(query, new { From = from, To = to });
            
            return result.ToDictionary(
                r => (PaymentMethod)r.method,
                r => (int)r.count);
        });
    }
}