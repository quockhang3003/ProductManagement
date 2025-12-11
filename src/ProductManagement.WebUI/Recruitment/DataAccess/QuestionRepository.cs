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
    public class QuestionRepository : IQuestionRepository
    {
        private readonly IDbConnectionFactory _dbFactory;


        public QuestionRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }


        public async Task<IEnumerable<Question>> GetAllAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<Question>("SELECT * FROM Question ORDER BY [Order], Id");
        }


        public async Task<Question?> GetByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<Question>(
                "SELECT * FROM Question WHERE Id = @Id", new { Id = id });
        }


        public async Task<IEnumerable<Question>> GetActiveQuestionsAsync()
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<Question>(
                "SELECT * FROM Question WHERE IsActive = 1 ORDER BY [Order], Id");
        }


        public async Task<Question> CreateAsync(Question question)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"
               INSERT INTO Question (Text, MaxWords, IsRequired, [Order], IsActive, CreatedAt)
               VALUES (@Text, @MaxWords, @IsRequired, @Order, @IsActive, @CreatedAt);
               SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await conn.QuerySingleAsync<int>(sql, question);
            question.Id = id;
            return question;
        }


        public async Task<Question> UpdateAsync(Question question)
        {
            using var conn = _dbFactory.CreateConnection();
            question.UpdatedAt = DateTime.UtcNow;

            var sql = @"
               UPDATE Question
               SET Text = @Text, MaxWords = @MaxWords, IsRequired = @IsRequired,
                   [Order] = @Order, IsActive = @IsActive, UpdatedAt = @UpdatedAt
               WHERE Id = @Id";

            await conn.ExecuteAsync(sql, question);
            return question;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var affectedRows = await conn.ExecuteAsync("DELETE FROM Question WHERE Id = @Id", new { Id = id });
            return affectedRows > 0;
        }
    }
}
