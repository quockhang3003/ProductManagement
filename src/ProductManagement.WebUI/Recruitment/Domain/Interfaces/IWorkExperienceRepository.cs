using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IWorkExperienceRepository
    {
        Task<IEnumerable<WorkExperience>> GetAllAsync();
        Task<IEnumerable<WorkExperience>> GetByUserIdAsync(int userId);
        Task AddAsync (WorkExperience workExperience);
        Task UpdateAsync (WorkExperience workExperience);
        Task DeleteAsync (int Id);
    }
}
