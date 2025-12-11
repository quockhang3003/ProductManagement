using Dapper;
using Domain.Entities;
using Domain.Interfaces;

namespace DataAccess
{
    public class EducationRepository : IEducationRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public EducationRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<Education>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Education WHERE DeletedAt IS NULL ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<Education>(sql);
        }

        public async Task<IEnumerable<Education>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Education WHERE UserID = @UserID AND DeletedAt IS NULL ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<Education>(sql, new { UserID = userId });
        }

        public async Task<Education?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Education WHERE Id = @Id AND DeletedAt IS NULL";
            return await conn.QueryFirstOrDefaultAsync<Education>(sql, new { Id = id });
        }

        public async Task AddAsync(Education education)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO Education (
                            UserID, UniversityCode, MajorCode, DegreeCode, Location, GPA,
                            GraduationMonth, GraduationYear, CreatedAt) 
                        VALUES (
                            @UserID, @UniversityCode, @MajorCode, @DegreeCode, @Location, @GPA,
                            @GraduationMonth, @GraduationYear, @CreatedAt)";
            await conn.ExecuteAsync(sql, education);
        }

        public async Task UpdateAsync(Education education)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE Education SET
                            UniversityCode = @UniversityCode,
                           MajorCode = @MajorCode,
                           DegreeCode = @DegreeCode,
                            Location = @Location,
                            GPA = @GPA,
                            GraduationMonth = @GraduationMonth,
                            GraduationYear = @GraduationYear,
                            UpdatedAt = @UpdatedAt
                        WHERE Id = @Id AND DeletedAt IS NULL";
            await conn.ExecuteAsync(sql, education);
        }

        public async Task DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "UPDATE Education SET DeletedAt = @DeletedAt WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = id, DeletedAt = DateTime.UtcNow });
        }
    }
}