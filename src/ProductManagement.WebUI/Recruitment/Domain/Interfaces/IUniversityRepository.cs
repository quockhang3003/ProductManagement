using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IUniversityRepository
    {
        Task<IEnumerable<University>> GetAllAsync();
        Task<IEnumerable<University>> GetByUserIdAsync(int userId);
        Task<University?> GetByIdAsync(int id);
        Task<int> CreateAsync(University university);
        Task<bool> UpdateAsync(University university);
        Task<bool> DeleteAsync(int id);
    }
}
