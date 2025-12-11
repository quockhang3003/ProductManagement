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
    public class RecruitmentProgramRepository : IRecruitmentProgramRepository
    {
        private readonly IDbConnectionFactory _dbFactory;

        public RecruitmentProgramRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public async Task<IEnumerable<RecruitmentProgram>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM RecruitmentProgram ORDER BY UpdatedOn DESC";
            return await conn.QueryAsync<RecruitmentProgram>(sql);
        }

        public async Task<RecruitmentProgram> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM RecruitmentProgram WHERE Id = @Id";
            return await conn.QueryFirstOrDefaultAsync<RecruitmentProgram>(sql, new { Id = id });
        }

        public async Task<int> AddAsync(RecruitmentProgram program)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"INSERT INTO RecruitmentProgram (Name, OpenDate, CloseDate, UpdatedOn) 
                       VALUES (@Name, @OpenDate, @CloseDate, @UpdatedOn);
                       SELECT CAST(SCOPE_IDENTITY() as int)";
            return await conn.ExecuteScalarAsync<int>(sql, program);
        }

        public async Task<bool> UpdateAsync(RecruitmentProgram program)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE RecruitmentProgram 
                       SET Name = @Name, 
                           OpenDate = @OpenDate, 
                           CloseDate = @CloseDate, 
                           UpdatedOn = @UpdatedOn 
                       WHERE Id = @Id";
            var result = await conn.ExecuteAsync(sql, program);
            return result > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "DELETE FROM RecruitmentProgram WHERE Id = @Id";
            var result = await conn.ExecuteAsync(sql, new { Id = id });
            return result > 0;
        }
        public async Task<int> GetTotalSubmittedByProgramIdAsync(int programId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"
                    SELECT COUNT(*)
                    FROM HeardAbout h
                    JOIN RecruitmentProgram rp ON rp.Id = h.RecruitmentProgramId
                    WHERE rp.Id = @ProgramId
                    AND h.CreatedAt >= rp.OpenDate
                    AND h.CreatedAt < DATEADD(day, 1, CAST(rp.CloseDate AS date));";

            return await conn.ExecuteScalarAsync<int>(sql, new { ProgramId = programId });
        }

        public async Task<RecruitmentProgram> GetActiveProgramAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"SELECT TOP 1 * 
                       FROM RecruitmentProgram 
                       WHERE CAST(GETDATE() AS DATE) BETWEEN CAST(OpenDate AS DATE) AND CAST(CloseDate AS DATE)
                       ORDER BY OpenDate DESC";
            return await conn.QueryFirstOrDefaultAsync<RecruitmentProgram>(sql);
        }

    }
}
