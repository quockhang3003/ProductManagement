using Dapper;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class SystemConfigurationRepository: ISystemConfigurationRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public SystemConfigurationRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<SystemConfiguration>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM SystemConfiguration ORDER BY Type, DisplayOrder, DisplayName";
            return await conn.QueryAsync<SystemConfiguration>(sql);
        }

        public async Task<IEnumerable<SystemConfiguration>> GetByTypeAsync(string type)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM SystemConfiguration WHERE Type = @Type ORDER BY DisplayOrder, DisplayName";
            return await conn.QueryAsync<SystemConfiguration>(sql, new { Type = type });
        }

        public async Task<IEnumerable<SystemConfiguration>> GetActiveByTypeAsync(string type)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM SystemConfiguration WHERE Type = @Type AND IsActive = 1 ORDER BY DisplayOrder, DisplayName";
            return await conn.QueryAsync<SystemConfiguration>(sql, new { Type = type });
        }

        public async Task<SystemConfiguration> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM SystemConfiguration WHERE Id = @Id";
            return await conn.QueryFirstOrDefaultAsync<SystemConfiguration>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(SystemConfiguration config)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO SystemConfiguration (Type, Code, DisplayName, DisplayOrder, IsActive, CreatedAt, UpdatedAt)
                       VALUES (@Type, @Code, @DisplayName, @DisplayOrder, @IsActive, @CreatedAt, @UpdatedAt);
                       SELECT CAST(SCOPE_IDENTITY() as int)";
            return await conn.ExecuteScalarAsync<int>(sql, config);
        }

        public async Task<bool> UpdateAsync(SystemConfiguration config)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE SystemConfiguration 
                       SET DisplayName = @DisplayName,
                           DisplayOrder = @DisplayOrder,
                           IsActive = @IsActive,
                           UpdatedAt = @UpdatedAt
                       WHERE Id = @Id";
            var result = await conn.ExecuteAsync(sql, config);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "DELETE FROM SystemConfiguration WHERE Id = @Id";
            var result = await conn.ExecuteAsync(sql, new { Id = id });
            return result > 0;
        }

        public async Task<bool> ExistsAsync(string type, string code, int? excludeId = null)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"SELECT COUNT(*) 
                       FROM SystemConfiguration 
                       WHERE Type = @Type AND Code = @Code";

            if (excludeId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
            }

            var count = await conn.ExecuteScalarAsync<int>(sql, new { Type = type, Code = code, ExcludeId = excludeId });
            return count > 0;
        }

        public async Task<IEnumerable<string>> GetAllTypesAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT DISTINCT Type FROM SystemConfiguration ORDER BY Type";
            return await conn.QueryAsync<string>(sql);
        }
    }
}
