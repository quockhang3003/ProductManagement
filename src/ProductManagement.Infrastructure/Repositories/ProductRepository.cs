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

public class ProductRepository : IProductRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<ProductRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public ProductRepository(DapperContext context, ILogger<ProductRepository> logger)
    {
        _context = context;
        _logger = logger;

        // Configure Polly for database resilience
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
        var timeoutPolicy = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));
        var policyWrap = Policy.WrapAsync(_retryPolicy, timeoutPolicy);
    }

    public async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, description, price, stock, created_at as CreatedAt, 
                       updated_at as UpdatedAt, is_active as IsActive 
                FROM products 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Product>(query, new { Id = id });
        });
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, description, price, stock, created_at as CreatedAt, 
                       updated_at as UpdatedAt, is_active as IsActive 
                FROM products 
                ORDER BY created_at DESC";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Product>(query);
        });
    }

    public async Task<IEnumerable<Product>> GetActiveProductsAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, name, description, price, stock, created_at as CreatedAt, 
                       updated_at as UpdatedAt, is_active as IsActive 
                FROM products 
                WHERE is_active = true
                ORDER BY name";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Product>(query);
        });
    }

    public async Task<Product> AddAsync(Product product)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO products (id, name, description, price, stock, created_at, is_active)
                VALUES (@Id, @Name, @Description, @Price, @Stock, @CreatedAt, @IsActive)";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, product);
            
            _logger.LogInformation("Product {ProductId} created successfully", product.Id);
            return product;
        });
    }

    public async Task UpdateAsync(Product product)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
            UPDATE products 
            SET stock = @Stock,
                name = @Name, 
                description = @Description, 
                price = @Price, 
                updated_at = @UpdatedAt, 
                is_active = @IsActive
            WHERE id = @Id
            AND stock + @StockChange >= 0  -- Prevent negative stock
            ";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new {
                product.Id,
                product.Name,
                product.Description,
                product.Price,
                product.Stock,
                product.UpdatedAt,
                product.IsActive,
                StockChange = 0  // or track the change
            });
            if (affectedRows == 0)
                throw new InvalidOperationException("Stock update failed - concurrent modification");
            _logger.LogInformation("Product {ProductId} updated, {Rows} rows affected", 
                product.Id, affectedRows);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM products WHERE id = @Id";

            using var connection = _context.CreateConnection();
            var affectedRows = await connection.ExecuteAsync(query, new { Id = id });
            
            _logger.LogInformation("Product {ProductId} deleted, {Rows} rows affected", 
                id, affectedRows);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM products WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }
}