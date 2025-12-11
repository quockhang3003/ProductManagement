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
    public class WorkExperienceService
    {
        private readonly IWorkExperienceRepository _repo;
        public WorkExperienceService(IWorkExperienceRepository repo)
        {
            _repo = repo;
        }
        public async Task<IEnumerable<WorkExperience>> GetAllAsync() => await _repo.GetAllAsync();
        public async Task<IEnumerable<WorkExperience>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);

        public async Task AddWorkExperience(WorkExperienceDTO dto, int userId)
        {
            var workExperience = new WorkExperience
            {
                UserID = userId,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CompanyName = dto.CompanyName,
                JobTitle = dto.JobTitle,
                EmploymentTypeCode = dto.EmploymentTypeCode,
                MainDuties = dto.MainDuties,
                Achievement = dto.Achievement,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(workExperience);
        }
        public async Task UpdateWorkExperience(WorkExperienceDTO dto, int id)
        {
            var workExperience = new WorkExperience
            {
                Id = id,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CompanyName = dto.CompanyName,
                JobTitle = dto.JobTitle,
                EmploymentTypeCode = dto.EmploymentTypeCode,
                MainDuties = dto.MainDuties,
                Achievement = dto.Achievement,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.UpdateAsync(workExperience);
        }
        public async Task DeleteWorkExperience(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}
