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
    public class LanguageProficiencyRepository : ILanguageProficiencyRepository
    {
        private readonly IDbConnectionFactory _dbFactory;
        public LanguageProficiencyRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task AddAsync(LanguageProficiency languageProficiency)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO LanguageProficiency(UserID, LanguageProficiencyTest, Result, CreatedAt)
                                VALUES(@UserID, @LanguageProficiencyTest, @Result, @CreatedAt)";
            await conn.ExecuteAsync(sql, languageProficiency);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "UPDATE LanguageProficiency SET DeletedAt = @DeletedAt WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow});
        }

        public async Task<IEnumerable<LanguageProficiency>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<LanguageProficiency>("SELECT * FROM LanguageProficiency");
        }

        public async Task<IEnumerable<LanguageProficiency>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM LanguageProficiency WHERE UserID = @UserID AND DeletedAt IS NULL ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<LanguageProficiency>(sql, new { UserID = userId});
        }

        public async Task UpdateAsync(LanguageProficiency languageProficiency)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE LanguageProficiency SET
                        LanguageProficiencyTest = @LanguageProficiencyTest,
                        Result = @Result,
                        UpdatedAt = @UpdatedAt
                        WHERE Id = @Id AND DeletedAt IS NULL";
            await conn.ExecuteAsync(sql, languageProficiency);
        }
    }
}
