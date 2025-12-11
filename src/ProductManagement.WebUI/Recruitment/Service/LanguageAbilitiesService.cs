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
    public class LanguageAbilitiesService
    {
        private readonly ILanguageAbilitiesRepository _repo;
        public LanguageAbilitiesService(ILanguageAbilitiesRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<LanguageAbilities>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<LanguageAbilities>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);
        public async Task AddLanguageAbilities(LanguageAbilitiesDTO dto, int userId)
        {
            var languageAbilities = new LanguageAbilities
            {
                UserID = userId,
                Language = dto.Language,
                SpeakingCode = dto.SpeakingCode,
                WritingCode = dto.WritingCode,
                ReadingCode = dto.ReadingCode,
                ListeningCode = dto.ListeningCode,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(languageAbilities);
        }
        public async Task UpdateLanguageAbilities(int id, LanguageAbilitiesDTO dto)
        {
            var languageAbilities = new LanguageAbilities
            {
                Id = id,
                Language = dto.Language,
                SpeakingCode = dto.SpeakingCode,
                WritingCode = dto.WritingCode,
                ReadingCode = dto.ReadingCode,
                ListeningCode = dto.ListeningCode,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.UpdateAsync(languageAbilities);
        }
        public async Task DeleteLanguageAbilities(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
