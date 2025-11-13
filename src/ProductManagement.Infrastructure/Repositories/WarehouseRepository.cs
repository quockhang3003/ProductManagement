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

public class WarehouseRepository : IWarehouseRepository
{
    private readonly DapperContext _context;
    private readonly ILogger<WarehouseRepository> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    public WarehouseRepository(DapperContext context, ILogger<WarehouseRepository> logger)
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

    public async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, address, city, state, zip_code as ZipCode,
                       contact_person as ContactPerson, phone, is_active as IsActive,
                       priority, created_at as CreatedAt
                FROM warehouses 
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Warehouse>(query, new { Id = id });
        });
    }

    public async Task<Warehouse?> GetByCodeAsync(string code)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, address, city, state, zip_code as ZipCode,
                       contact_person as ContactPerson, phone, is_active as IsActive,
                       priority, created_at as CreatedAt
                FROM warehouses 
                WHERE code = @Code";

            using var connection = _context.CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<Warehouse>(query, new { Code = code });
        });
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, address, city, state, zip_code as ZipCode,
                       contact_person as ContactPerson, phone, is_active as IsActive,
                       priority, created_at as CreatedAt
                FROM warehouses 
                ORDER BY priority, name";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Warehouse>(query);
        });
    }

    public async Task<IEnumerable<Warehouse>> GetActiveWarehousesAsync()
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                SELECT id, code, name, address, city, state, zip_code as ZipCode,
                       contact_person as ContactPerson, phone, is_active as IsActive,
                       priority, created_at as CreatedAt
                FROM warehouses 
                WHERE is_active = true
                ORDER BY priority, name";

            using var connection = _context.CreateConnection();
            return await connection.QueryAsync<Warehouse>(query);
        });
    }

    public async Task<Warehouse> AddAsync(Warehouse warehouse)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                INSERT INTO warehouses (
                    id, code, name, address, city, state, zip_code,
                    contact_person, phone, is_active, priority, created_at
                ) VALUES (
                    @Id, @Code, @Name, @Address, @City, @State, @ZipCode,
                    @ContactPerson, @Phone, @IsActive, @Priority, @CreatedAt
                )";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, warehouse);

            _logger.LogInformation("Warehouse {Code} created successfully", warehouse.Code);
            return warehouse;
        });
    }

    public async Task UpdateAsync(Warehouse warehouse)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = @"
                UPDATE warehouses 
                SET name = @Name,
                    address = @Address,
                    city = @City,
                    state = @State,
                    zip_code = @ZipCode,
                    contact_person = @ContactPerson,
                    phone = @Phone,
                    is_active = @IsActive,
                    priority = @Priority
                WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, warehouse);

            _logger.LogInformation("Warehouse {Code} updated", warehouse.Code);
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "DELETE FROM warehouses WHERE id = @Id";

            using var connection = _context.CreateConnection();
            await connection.ExecuteAsync(query, new { Id = id });

            _logger.LogInformation("Warehouse {Id} deleted", id);
        });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM warehouses WHERE id = @Id)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
        });
    }

    public async Task<bool> CodeExistsAsync(string code)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            const string query = "SELECT EXISTS(SELECT 1 FROM warehouses WHERE code = @Code)";

            using var connection = _context.CreateConnection();
            return await connection.ExecuteScalarAsync<bool>(query, new { Code = code });
        });
    }
}