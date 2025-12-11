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
    public class QuestionService
    {
        private readonly IQuestionRepository _repo;


        public QuestionService(IQuestionRepository repo)
        {
            _repo = repo;
        }


        public async Task<IEnumerable<QuestionDTO>> GetAllAsync()
        {
            var questions = await _repo.GetAllAsync();
            return questions.Select(q => new QuestionDTO
            {
                Id = q.Id,
                Text = q.Text,
                MaxWords = q.MaxWords,
                IsRequired = q.IsRequired,
                Order = q.Order,
                IsActive = q.IsActive
            });
        }


        public async Task<IEnumerable<QuestionDTO>> GetActiveQuestionsAsync()
        {
            var questions = await _repo.GetActiveQuestionsAsync();
            return questions.Select(q => new QuestionDTO
            {
                Id = q.Id,
                Text = q.Text,
                MaxWords = q.MaxWords,
                IsRequired = q.IsRequired,
                Order = q.Order,
                IsActive = q.IsActive
            });
        }


        public async Task<QuestionDTO?> GetByIdAsync(int id)
        {
            var question = await _repo.GetByIdAsync(id);
            if (question == null) return null;


            return new QuestionDTO
            {
                Id = question.Id,
                Text = question.Text,
                MaxWords = question.MaxWords,
                IsRequired = question.IsRequired,
                Order = question.Order,
                IsActive = question.IsActive
            };
        }


        public async Task<QuestionDTO> CreateAsync(QuestionDTO dto)
        {
            var question = new Question
            {
                Text = dto.Text,
                MaxWords = dto.MaxWords,
                IsRequired = dto.IsRequired,
                Order = dto.Order,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };


            var created = await _repo.CreateAsync(question);
            dto.Id = created.Id;
            return dto;
        }


        public async Task<QuestionDTO> UpdateAsync(QuestionDTO dto)
        {
            var question = new Question
            {
                Id = dto.Id,
                Text = dto.Text,
                MaxWords = dto.MaxWords,
                IsRequired = dto.IsRequired,
                Order = dto.Order,
                IsActive = dto.IsActive
            };


            await _repo.UpdateAsync(question);
            return dto;
        }


        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }
    }
}
