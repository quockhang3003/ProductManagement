using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IEducationRepository
    {
        Task<IEnumerable<Education>> GetAllAsync();
        Task<IEnumerable<Education>> GetByUserIdAsync(int userId);
        Task<Education?> GetByIdAsync(int id);
        Task AddAsync(Education education);
        Task UpdateAsync(Education education);
        Task DeleteAsync(int id);
    }
}
