using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUserQuestionAnswerRepository
    {
        Task<IEnumerable<UserQuestionAnswer>> GetByUserIdAsync(int userId);
        Task<UserQuestionAnswer?> GetByUserAndQuestionAsync(int userId, int questionId);
        Task<UserQuestionAnswer> CreateAsync(UserQuestionAnswer answer);
        Task<UserQuestionAnswer> UpdateAsync(UserQuestionAnswer answer);
        Task<bool> DeleteAsync(int id);
        Task<bool> DeleteByUserIdAsync(int userId);
        Task<IEnumerable<UserQuestionAnswer>> GetUserAnswersWithQuestionsAsync(int userId);
    }
}
