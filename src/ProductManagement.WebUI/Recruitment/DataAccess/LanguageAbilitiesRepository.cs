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
    public class LanguageAbilitiesRepository : ILanguageAbilitiesRepository
    {
        private readonly IDbConnectionFactory _dbFactory;
        public LanguageAbilitiesRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }
        public async Task AddAsync(LanguageAbilities languageAbilities)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO LanguageAbilities (UserID, Language, SpeakingCode, ReadingCode, WritingCode, ListeningCode, CreatedAt)
                                VALUES(@UserID, @Language, @SpeakingCode, @ReadingCode, @WritingCode, @ListeningCode, @CreatedAt)";
            await conn.ExecuteAsync(sql, languageAbilities);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "UPDATE LanguageAbilities SET DeletedAt = @DeletedAt WHERD Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow});
        }

        public async Task<IEnumerable<LanguageAbilities>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<LanguageAbilities>("SELECT * FROM LanguageAbilities");
        }

        public async Task<IEnumerable<LanguageAbilities>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM LanguageAbilities WHERE UserID = @UserID AND DeletedAt IS NULL ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<LanguageAbilities>(sql, new { UserID = userId});
        }

        public async Task UpdateAsync(LanguageAbilities languageAbilities)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE LanguageAbilities SET 
                        Language = @Language,
                        SpeakingCode = @SpeakingCode,
                        ReadingCode = @ReadingCode,
                        WritingCode = @WritingCode,
                        ListeningCode = @ListeningCode,
                        UpdatedAt = @UpdatedAt
                        WHERE Id = @Id AND DeletedAt IS NULL";
            await conn.ExecuteAsync(sql, languageAbilities);
        }
    }
}
