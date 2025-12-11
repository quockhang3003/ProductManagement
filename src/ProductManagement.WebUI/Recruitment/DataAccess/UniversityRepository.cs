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
    public class UniversityRepository : IUniversityRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public UniversityRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<University>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<University>("SELECT * FROM University");
        }

        public async Task<IEnumerable<University>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();

            const string sql = @"
                SELECT 
                    sc.Id, 
                    sc.Code,
                    sc.DisplayName as UniversityName
                FROM SystemConfiguration sc
                WHERE sc.Type = 'University'
                  AND sc.Code IN (
                      SELECT DISTINCT e.UniversityCode
                      FROM Education e
                      WHERE e.UserId = @UserId
                        AND e.DeletedAt IS NULL
                  )
                ORDER BY sc.DisplayOrder, sc.DisplayName";

            return await conn.QueryAsync<University>(sql, new { UserId = userId });
        }




        public async Task<University?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<University>(
                "SELECT * FROM University WHERE Id = @Id",
                new { Id = id });
        }

        public async Task<int> CreateAsync(University university)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                INSERT INTO University (UserId, UniversityName, Program, Degree, GPA, OutOf, CreatedAt)
                VALUES (@UserId, @UniversityName, @Program, @Degree, @GPA, @OutOf, @CreatedAt);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await conn.QuerySingleAsync<int>(sql, university);
        }

        public async Task<bool> UpdateAsync(University university)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = @"
                UPDATE University 
                SET UniversityName = @UniversityName, Program = @Program, 
                    Degree = @Degree, GPA = @GPA, OutOf = @OutOf
                WHERE Id = @Id";
            var affectedRows = await conn.ExecuteAsync(sql, university);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            const string sql = "DELETE FROM University WHERE Id = @Id";
            var affectedRows = await conn.ExecuteAsync(sql, new { Id = id });
            return affectedRows > 0;
        }
    }
}
