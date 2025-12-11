using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IProfessionalQualificationRepository
    {
        Task<IEnumerable<ProfessionalQualification>> GetAllAsync();
        Task<IEnumerable<ProfessionalQualification>> GetByUserIdAsync(int userId);
        Task AddAsync(ProfessionalQualification professionalQualification);
        Task UpdateAsync(ProfessionalQualification professionalQualification);
        Task DeleteAsync(int id);
    }
}
