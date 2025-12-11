using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface ILanguageProficiencyRepository
    {
        Task<IEnumerable<LanguageProficiency>> GetAllAsync();
        Task<IEnumerable<LanguageProficiency>> GetByUserIdAsync(int userId);
        Task AddAsync(LanguageProficiency languageProficiency);
        Task UpdateAsync(LanguageProficiency languageProficiency);
        Task DeleteAsync(int id);
    }
}
