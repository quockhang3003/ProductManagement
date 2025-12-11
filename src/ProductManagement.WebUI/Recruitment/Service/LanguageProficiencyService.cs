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
    public class LanguageProficiencyService
    {
        private readonly ILanguageProficiencyRepository _repo;
        public LanguageProficiencyService(ILanguageProficiencyRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<LanguageProficiency>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<LanguageProficiency>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);
        public async Task AddLanguageProficiency(LanguageProficiencyDTO dto, int userId)
        {
            var languageProficiency = new LanguageProficiency
            {
                UserID = userId,
                LanguageProficiencyTest = dto.LanguageProficiencyTest,
                Result = dto.Result,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(languageProficiency);
        }
        public async Task UpdateLanguageProficiency(LanguageProficiencyDTO dto, int id)
        {
            var languageProficiency = new LanguageProficiency
            {
                Id = id,
                LanguageProficiencyTest = dto.LanguageProficiencyTest,
                Result = dto.Result,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.UpdateAsync(languageProficiency);
        }
        public async Task DeleteLanguageProficiency(int id)
        { 
            await _repo.DeleteAsync(id); 
        }
    }
}
