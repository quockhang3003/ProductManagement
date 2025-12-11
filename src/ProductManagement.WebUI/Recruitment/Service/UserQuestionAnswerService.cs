using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class UserQuestionAnswerService
    {
        private readonly IUserQuestionAnswerRepository _answerRepo;
        private readonly IQuestionRepository _questionRepo;


        public UserQuestionAnswerService(IUserQuestionAnswerRepository answerRepo, IQuestionRepository questionRepo)
        {
            _answerRepo = answerRepo;
            _questionRepo = questionRepo;
        }


        public async Task<IEnumerable<UserQuestionAnswerDTO>> GetByUserIdAsync(int userId)
        {
            var answers = await _answerRepo.GetByUserIdAsync(userId);
            return answers.Select(a => new UserQuestionAnswerDTO
            {
                Id = a.Id,
                UserId = a.UserId,
                QuestionId = a.QuestionId,
                Answer = a.Answer,
                UpdatedAt = a.UpdatedAt
            });
        }


        public async Task<IEnumerable<QuestionWithAnswerDTO>> GetQuestionsWithAnswersAsync(int userId)
        {
            var questions = await _questionRepo.GetActiveQuestionsAsync();
            var answers = await _answerRepo.GetByUserIdAsync(userId);
            var answerDict = answers.ToDictionary(a => a.QuestionId, a => a);


            return questions.Select(q => new QuestionWithAnswerDTO
            {
                Id = q.Id,
                Text = q.Text,
                MaxWords = q.MaxWords,
                IsRequired = q.IsRequired,
                Order = q.Order,
                IsActive = q.IsActive,
                Answer = answerDict.ContainsKey(q.Id) ? answerDict[q.Id].Answer : "",
                AnswerUpdatedAt = answerDict.ContainsKey(q.Id) ? answerDict[q.Id].UpdatedAt : null,
                IsSaved = answerDict.ContainsKey(q.Id)
            });
        }


        public async Task<UserQuestionAnswerDTO> SaveAnswerAsync(int userId, UserQuestionAnswerCreateDTO dto)
        {
            var existing = await _answerRepo.GetByUserAndQuestionAsync(userId, dto.QuestionId);

            if (existing != null)
            {
                // Update existing answer
                existing.Answer = dto.Answer;
                var updated = await _answerRepo.UpdateAsync(existing);
                return new UserQuestionAnswerDTO
                {
                    Id = updated.Id,
                    UserId = updated.UserId,
                    QuestionId = updated.QuestionId,
                    Answer = updated.Answer,
                    UpdatedAt = updated.UpdatedAt
                };
            }
            else
            {
                // Create new answer
                var answer = new UserQuestionAnswer
                {
                    UserId = userId,
                    QuestionId = dto.QuestionId,
                    Answer = dto.Answer,
                    CreatedAt = DateTime.UtcNow
                };


                var created = await _answerRepo.CreateAsync(answer);
                return new UserQuestionAnswerDTO
                {
                    Id = created.Id,
                    UserId = created.UserId,
                    QuestionId = created.QuestionId,
                    Answer = created.Answer,
                    UpdatedAt = created.UpdatedAt
                };
            }
        }
        public async Task<List<UserQuestionAnswerDTO>> SaveMultipleAnswersAsync(int userId, List<UserQuestionAnswerCreateDTO> answers)
        {
            var result = new List<UserQuestionAnswerDTO>();

            foreach (var answerDto in answers)
            {
                var saved = await SaveAnswerAsync(userId, answerDto);
                result.Add(saved);
            }

            return result;
        }


        public async Task<bool> DeleteAnswerAsync(int id, int userId)
        {
            var existing = await _answerRepo.GetByUserAndQuestionAsync(userId, id);
            if (existing == null) return false;

            return await _answerRepo.DeleteAsync(existing.Id);
        }


        public async Task<bool> DeleteAllUserAnswersAsync(int userId)
        {
            return await _answerRepo.DeleteByUserIdAsync(userId);
        }
    }
}
