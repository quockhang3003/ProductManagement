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

public class StockTransferRepository : IStockTransferRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<StockTransferRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public StockTransferRepository(DapperContext context, ILogger<StockTransferRepository> logger)
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

    public async Task<StockTransfer?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, product_id as ProductId, from_warehouse_id as FromWarehouseId,
                       to_warehouse_id as ToWarehouseId, quantity, status, notes,
                       requested_by as RequestedBy, approved_by as ApprovedBy,
                       requested_at as RequestedAt, approved_at as ApprovedAt,
                       completed_at as CompletedAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason
                FROM stock_transfers 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<StockTransfer>(query, new { Id = id });
        });
    }

    public async Task<IEnumerable<StockTransfer>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, product_id as ProductId, from_warehouse_id as FromWarehouseId,
                       to_warehouse_id as ToWarehouseId, quantity, status, notes,
                       requested_by as RequestedBy, approved_by as ApprovedBy,
                       requested_at as RequestedAt, approved_at as ApprovedAt,
                       completed_at as CompletedAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason
                FROM stock_transfers 
                ORDER BY requested_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<StockTransfer>(query);
        });
    }

    public async Task<IEnumerable<StockTransfer>> GetByStatusAsync(StockTransferStatus status)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, product_id as ProductId, from_warehouse_id as FromWarehouseId,
                       to_warehouse_id as ToWarehouseId, quantity, status, notes,
                       requested_by as RequestedBy, approved_by as ApprovedBy,
                       requested_at as RequestedAt, approved_at as ApprovedAt,
                       completed_at as CompletedAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason
                FROM stock_transfers 
                WHERE status = @Status
                ORDER BY requested_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<StockTransfer>(query, new { Status = (int)status });
        });
    }

    public async Task<IEnumerable<StockTransfer>> GetByProductIdAsync(Guid productId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, product_id as ProductId, from_warehouse_id as FromWarehouseId,
                       to_warehouse_id as ToWarehouseId, quantity, status, notes,
                       requested_by as RequestedBy, approved_by as ApprovedBy,
                       requested_at as RequestedAt, approved_at as ApprovedAt,
                       completed_at as CompletedAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason
                FROM stock_transfers 
                WHERE product_id = @ProductId
                ORDER BY requested_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<StockTransfer>(query, new { ProductId = productId });
        });
    }

    public async Task<IEnumerable<StockTransfer>> GetByWarehouseAsync(Guid warehouseId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, product_id as ProductId, from_warehouse_id as FromWarehouseId,
                       to_warehouse_id as ToWarehouseId, quantity, status, notes,
                       requested_by as RequestedBy, approved_by as ApprovedBy,
                       requested_at as RequestedAt, approved_at as ApprovedAt,
                       completed_at as CompletedAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason
                FROM stock_transfers 
                WHERE from_warehouse_id = @WarehouseId 
                   OR to_warehouse_id = @WarehouseId
                ORDER BY requested_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<StockTransfer>(query, new { WarehouseId = warehouseId });
        });
    }

    public async Task<IEnumerable<StockTransfer>> GetPendingTransfersOlderThanAsync(TimeSpan age)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, product_id as ProductId, from_warehouse_id as FromWarehouseId,
                       to_warehouse_id as ToWarehouseId, quantity, status, notes,
                       requested_by as RequestedBy, approved_by as ApprovedBy,
                       requested_at as RequestedAt, approved_at as ApprovedAt,
                       completed_at as CompletedAt, cancelled_at as CancelledAt,
                       cancellation_reason as CancellationReason
                FROM stock_transfers 
                WHERE status = 0 
                  AND requested_at < (CURRENT_TIMESTAMP - INTERVAL '@AgeDays days')
                ORDER BY requested_at";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<StockTransfer>(query, new { AgeDays = age.TotalDays });
        });
    }

    public async Task<StockTransfer> AddAsync(StockTransfer transfer)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO stock_transfers (
                    id, product_id, from_warehouse_id, to_warehouse_id, quantity,
                    status, notes, requested_by, requested_at
                ) VALUES (
                    @Id, @ProductId, @FromWarehouseId, @ToWarehouseId, @Quantity,
                    @Status, @Notes, @RequestedBy, @RequestedAt
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                transfer.Id,
                transfer.ProductId,
                transfer.FromWarehouseId,
                transfer.ToWarehouseId,
                transfer.Quantity,
                Status = (int)transfer.Status,
                transfer.Notes,
                transfer.RequestedBy,
                transfer.RequestedAt
            });

            _logger.LogInformation("Stock transfer {Id} created", transfer.Id);
            return transfer;
        });
    }

    public async Task UpdateAsync(StockTransfer transfer)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE stock_transfers 
                SET status = @Status,
                    approved_by = @ApprovedBy,
                    approved_at = @ApprovedAt,
                    completed_at = @CompletedAt,
                    cancelled_at = @CancelledAt,
                    cancellation_reason = @CancellationReason
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new
            {
                transfer.Id,
                Status = (int)transfer.Status,
                transfer.ApprovedBy,
                transfer.ApprovedAt,
                transfer.CompletedAt,
                transfer.CancelledAt,
                transfer.CancellationReason
            });

            _logger.LogInformation("Stock transfer {Id} updated to status {Status}", 
                transfer.Id, transfer.Status);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM stock_transfers WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });

            _logger.LogInformation("Stock transfer {Id} deleted", id);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM stock_transfers WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }
}