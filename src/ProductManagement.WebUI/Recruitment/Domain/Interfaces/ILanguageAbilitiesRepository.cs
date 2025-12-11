using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ILanguageAbilitiesRepository
    {
        Task<IEnumerable<LanguageAbilities>> GetAllAsync();
        Task<IEnumerable<LanguageAbilities>> GetByUserIdAsync(int userId);
        Task AddAsync(LanguageAbilities languageAbilities);
        Task UpdateAsync(LanguageAbilities languageAbilities);
        Task DeleteAsync(int id);
    }
}
