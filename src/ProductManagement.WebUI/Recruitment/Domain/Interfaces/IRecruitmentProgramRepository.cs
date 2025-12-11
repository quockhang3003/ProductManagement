using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IRecruitmentProgramRepository
    {
        Task<IEnumerable<RecruitmentProgram>> GetAllAsync();
        Task<RecruitmentProgram> GetByIdAsync(int id);
        Task<int> AddAsync(RecruitmentProgram program);
        Task<bool> UpdateAsync(RecruitmentProgram program);
        Task<bool> DeleteAsync(int id);
        Task<int> GetTotalSubmittedByProgramIdAsync(int programId);
        Task<RecruitmentProgram> GetActiveProgramAsync();

    }
}
