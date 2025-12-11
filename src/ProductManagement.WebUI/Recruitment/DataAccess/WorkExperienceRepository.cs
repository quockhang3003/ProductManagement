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
    public class WorkExperienceRepository : IWorkExperienceRepository
    {
        private readonly IDbConnectionFactory _dbFactory;
        public WorkExperienceRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task AddAsync(WorkExperience workExperience)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO WorkExperience(UserID, StartDate, EndDate, CompanyName, JobTitle, EmploymentTypeCode, MainDuties, Achievement, CreatedAt)
                                VALUES(@UserID, @StartDate, @EndDate, @CompanyName, @JobTitle, @EmploymentTypeCode, @MainDuties, @Achievement, @CreatedAt)";
            await conn.ExecuteAsync(sql, workExperience);
        }

        public async Task DeleteAsync(int Id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "UPDATE WorkExperience SET DeletedAt = @DeletedAt WHERE Id = @Id";
            await conn.ExecuteAsync(sql, new { Id = Id, DeletedAt = DateTime.UtcNow });
        }

        public async Task<IEnumerable<WorkExperience>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<WorkExperience>("SELECT * FROM WorkExperience");
        }

        public async Task<IEnumerable<WorkExperience>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM WorkExperience WHERE UserID = @UserID AND DeletedAt IS NULL ORDER BY CreatedAt DESC";
            return await conn.QueryAsync<WorkExperience>(sql, new { UserID = userId });
        }

        public async Task UpdateAsync(WorkExperience workExperience)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE WorkExperience SET
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        CompanyName = @CompanyName,
                        JobTitle = @JobTitle,
                        EmploymentTypeCode = @EmploymentTypeCode,
                        MainDuties = @MainDuties,
                        Achievement = @Achievement
                        UpdatedAt = @UpdatedAt
                        WHERE Id = @Id AND DeletedAt IS NULL";
            await conn.ExecuteAsync(sql, workExperience);
        }
    }
}
