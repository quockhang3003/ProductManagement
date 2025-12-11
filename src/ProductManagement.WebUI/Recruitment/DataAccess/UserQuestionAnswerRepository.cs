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
    public class UserQuestionAnswerRepository : IUserQuestionAnswerRepository
    {
        private readonly IDbConnectionFactory _dbFactory;


        public UserQuestionAnswerRepository(IDbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }


        public async Task<IEnumerable<UserQuestionAnswer>> GetByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryAsync<UserQuestionAnswer>(
                "SELECT * FROM UserQuestionAnswer WHERE UserId = @UserId ORDER BY QuestionId",
                new { UserId = userId });
        }


        public async Task<UserQuestionAnswer?> GetByUserAndQuestionAsync(int userId, int questionId)
        {
            using var conn = _dbFactory.CreateConnection();
            return await conn.QueryFirstOrDefaultAsync<UserQuestionAnswer>(
                "SELECT * FROM UserQuestionAnswer WHERE UserId = @UserId AND QuestionId = @QuestionId",
                new { UserId = userId, QuestionId = questionId });
        }


        public async Task<UserQuestionAnswer> CreateAsync(UserQuestionAnswer answer)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"
               INSERT INTO UserQuestionAnswer (UserId, QuestionId, Answer, CreatedAt)
               VALUES (@UserId, @QuestionId, @Answer, @CreatedAt);
               SELECT CAST(SCOPE_IDENTITY() as int)";

            var id = await conn.QuerySingleAsync<int>(sql, answer);
            answer.Id = id;
            return answer;
        }


        public async Task<UserQuestionAnswer> UpdateAsync(UserQuestionAnswer answer)
        {
            using var conn = _dbFactory.CreateConnection();
            answer.UpdatedAt = DateTime.UtcNow;

            var sql = @"
               UPDATE UserQuestionAnswer
               SET Answer = @Answer, UpdatedAt = @UpdatedAt
               WHERE Id = @Id";

            await conn.ExecuteAsync(sql, answer);
            return answer;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var affectedRows = await conn.ExecuteAsync("DELETE FROM UserQuestionAnswer WHERE Id = @Id", new { Id = id });
            return affectedRows > 0;
        }


        public async Task<bool> DeleteByUserIdAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var affectedRows = await conn.ExecuteAsync("DELETE FROM UserQuestionAnswer WHERE UserId = @UserId", new { UserId = userId });
            return affectedRows > 0;
        }


        public async Task<IEnumerable<UserQuestionAnswer>> GetUserAnswersWithQuestionsAsync(int userId)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"
               SELECT uqa.*, q.Text, q.MaxWords, q.IsRequired, q.[Order], q.IsActive
               FROM UserQuestionAnswer uqa
               INNER JOIN Questions q ON uqa.QuestionId = q.Id
               WHERE uqa.UserId = @UserId
               ORDER BY q.[Order], q.Id";

            return await conn.QueryAsync<UserQuestionAnswer, Question, UserQuestionAnswer>(
                sql,
                (answer, question) =>
                {
                    answer.Question = question;
                    return answer;
                },
                new { UserId = userId },
                splitOn: "Text"
            );
        }
    }
}
