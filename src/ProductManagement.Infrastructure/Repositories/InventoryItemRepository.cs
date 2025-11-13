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

public class InventoryItemRepository : IInventoryItemRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<InventoryItemRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public InventoryItemRepository(DapperContext context, ILogger<InventoryItemRepository> logger)
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

    public async Task<InventoryItem?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       quantity_on_hand as QuantityOnHand, quantity_reserved as QuantityReserved,
                       reorder_point as ReorderPoint, reorder_quantity as ReorderQuantity,
                       last_restocked_at as LastRestockedAt, created_at as CreatedAt,
                       updated_at as UpdatedAt
                FROM inventory_items 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<InventoryItem>(query, new { Id = id });
        });
    }

    public async Task<InventoryItem?> GetByWarehouseAndProductAsync(Guid warehouseId, Guid productId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       quantity_on_hand as QuantityOnHand, quantity_reserved as QuantityReserved,
                       reorder_point as ReorderPoint, reorder_quantity as ReorderQuantity,
                       last_restocked_at as LastRestockedAt, created_at as CreatedAt,
                       updated_at as UpdatedAt
                FROM inventory_items 
                WHERE warehouse_id = @WarehouseId AND product_id = @ProductId";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<InventoryItem>(
                query, new { WarehouseId = warehouseId, ProductId = productId });
        });
    }

    public async Task<IEnumerable<InventoryItem>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       quantity_on_hand as QuantityOnHand, quantity_reserved as QuantityReserved,
                       reorder_point as ReorderPoint, reorder_quantity as ReorderQuantity,
                       last_restocked_at as LastRestockedAt, created_at as CreatedAt,
                       updated_at as UpdatedAt
                FROM inventory_items 
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryItem>(query);
        });
    }

    public async Task<IEnumerable<InventoryItem>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       quantity_on_hand as QuantityOnHand, quantity_reserved as QuantityReserved,
                       reorder_point as ReorderPoint, reorder_quantity as ReorderQuantity,
                       last_restocked_at as LastRestockedAt, created_at as CreatedAt,
                       updated_at as UpdatedAt
                FROM inventory_items 
                WHERE warehouse_id = @WarehouseId
                ORDER BY product_id";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryItem>(query, new { WarehouseId = warehouseId });
        });
    }

    public async Task<IEnumerable<InventoryItem>> GetByProductIdAsync(Guid productId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       quantity_on_hand as QuantityOnHand, quantity_reserved as QuantityReserved,
                       reorder_point as ReorderPoint, reorder_quantity as ReorderQuantity,
                       last_restocked_at as LastRestockedAt, created_at as CreatedAt,
                       updated_at as UpdatedAt
                FROM inventory_items 
                WHERE product_id = @ProductId
                ORDER BY warehouse_id";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryItem>(query, new { ProductId = productId });
        });
    }

    public async Task<IEnumerable<InventoryItem>> GetLowStockItemsAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, warehouse_id as WarehouseId, product_id as ProductId,
                       quantity_on_hand as QuantityOnHand, quantity_reserved as QuantityReserved,
                       reorder_point as ReorderPoint, reorder_quantity as ReorderQuantity,
                       last_restocked_at as LastRestockedAt, created_at as CreatedAt,
                       updated_at as UpdatedAt
                FROM inventory_items 
                WHERE (quantity_on_hand - quantity_reserved) <= reorder_point
                ORDER BY (quantity_on_hand - quantity_reserved)";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<InventoryItem>(query);
        });
    }

    public async Task<int> GetTotalAvailableStockAsync(Guid productId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT COALESCE(SUM(quantity_on_hand - quantity_reserved), 0)
                FROM inventory_items 
                WHERE product_id = @ProductId";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<int>(query, new { ProductId = productId });
        });
    }

    public async Task<InventoryItem> AddAsync(InventoryItem item)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO inventory_items (
                    id, warehouse_id, product_id, quantity_on_hand, quantity_reserved,
                    reorder_point, reorder_quantity, last_restocked_at, created_at
                ) VALUES (
                    @Id, @WarehouseId, @ProductId, @QuantityOnHand, @QuantityReserved,
                    @ReorderPoint, @ReorderQuantity, @LastRestockedAt, @CreatedAt
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, item);

            _logger.LogInformation(
                "Inventory item created: Warehouse {WarehouseId}, Product {ProductId}",
                item.WarehouseId, item.ProductId);
            
            return item;
        });
    }

    public async Task UpdateAsync(InventoryItem item)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE inventory_items 
                SET quantity_on_hand = @QuantityOnHand,
                    quantity_reserved = @QuantityReserved,
                    reorder_point = @ReorderPoint,
                    reorder_quantity = @ReorderQuantity,
                    last_restocked_at = @LastRestockedAt,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, item);

            _logger.LogInformation("Inventory item {Id} updated", item.Id);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM inventory_items WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });

            _logger.LogInformation("Inventory item {Id} deleted", id);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM inventory_items WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }

    public async Task<List<(InventoryItem Item, int Quantity)>> GetReservationsByOrderAsync(Guid orderId)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT 
                    ii.id, ii.warehouse_id as WarehouseId, ii.product_id as ProductId,
                    ii.quantity_on_hand as QuantityOnHand, ii.quantity_reserved as QuantityReserved,
                    ii.reorder_point as ReorderPoint, ii.reorder_quantity as ReorderQuantity,
                    ii.last_restocked_at as LastRestockedAt, ii.created_at as CreatedAt,
                    ii.updated_at as UpdatedAt,
                    sr.quantity
                FROM stock_reservations sr
                JOIN inventory_items ii ON sr.inventory_item_id = ii.id
                WHERE sr.order_id = @OrderId
                  AND sr.fulfilled_at IS NULL
                  AND sr.released_at IS NULL";

            using var connection = _context.CreateConnection();
            var result = await connection.QueryAsync<(InventoryItem, int)>(query, new { OrderId = orderId });
            return result.ToList();
        });
    }
}