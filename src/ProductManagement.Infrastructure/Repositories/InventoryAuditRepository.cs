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

public class InventoryAuditRepository : IInventoryAuditRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<InventoryAuditRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public InventoryAuditRepository(DapperContext context, ILogger<InventoryAuditRepository> logger)
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

    public async Task<InventoryAudit?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       expected_quantity as ExpectedQuantity, actual_quantity as ActualQuantity,
                       variance as Variance, audited_by as AuditedBy, notes,
                       audited_at as AuditedAt
                FROM inventory_audits 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<InventoryAudit>(query, new { Id = id });
        });
    }

    public async Task<IEnumerable<InventoryAudit>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       expected_quantity as ExpectedQuantity, actual_quantity as ActualQuantity,
                       variance as Variance, audited_by as AuditedBy, notes,
                       audited_at as AuditedAt
                FROM inventory_audits 
                ORDER BY audited_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryAudit>(query);
        });
    }

    public async Task<IEnumerable<InventoryAudit>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       expected_quantity as ExpectedQuantity, actual_quantity as ActualQuantity,
                       variance as Variance, audited_by as AuditedBy, notes,
                       audited_at as AuditedAt
                FROM inventory_audits 
                WHERE warehouse_id = @WarehouseId
                ORDER BY audited_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryAudit>(query, new { WarehouseId = warehouseId });
        });
    }

    public async Task<IEnumerable<InventoryAudit>> GetByProductIdAsync(Guid productId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       expected_quantity as ExpectedQuantity, actual_quantity as ActualQuantity,
                       variance as Variance, audited_by as AuditedBy, notes,
                       audited_at as AuditedAt
                FROM inventory_audits 
                WHERE product_id = @ProductId
                ORDER BY audited_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryAudit>(query, new { ProductId = productId });
        });
    }

    public async Task<IEnumerable<InventoryAudit>> GetByWarehouseAndProductAsync(
        Guid warehouseId, 
        Guid productId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       expected_quantity as ExpectedQuantity, actual_quantity as ActualQuantity,
                       variance as Variance, audited_by as AuditedBy, notes,
                       audited_at as AuditedAt
                FROM inventory_audits 
                WHERE warehouse_id = @WarehouseId AND product_id = @ProductId
                ORDER BY audited_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryAudit>(query, 
                new { WarehouseId = warehouseId, ProductId = productId });
        });
    }

    public async Task<IEnumerable<InventoryAudit>> GetAuditsWithVarianceAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       expected_quantity as ExpectedQuantity, actual_quantity as ActualQuantity,
                       variance as Variance, audited_by as AuditedBy, notes,
                       audited_at as AuditedAt
                FROM inventory_audits 
                WHERE variance != 0
                ORDER BY ABS(variance) DESC, audited_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryAudit>(query);
        });
    }

    public async Task<InventoryAudit> AddAsync(InventoryAudit audit)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO inventory_audits (
                    id, warehouse_id, product_id, expected_quantity, actual_quantity,
                    audited_by, notes, audited_at
                ) VALUES (
                    @Id, @WarehouseId, @ProductId, @ExpectedQuantity, @ActualQuantity,
                    @AuditedBy, @Notes, @AuditedAt
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, audit);

            _logger.LogInformation(
                "Inventory audit created: Warehouse {WarehouseId}, Product {ProductId}, Variance: {Variance}",
                audit.WarehouseId, audit.ProductId, audit.Variance);
            
            return audit;
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM inventory_audits WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }
}