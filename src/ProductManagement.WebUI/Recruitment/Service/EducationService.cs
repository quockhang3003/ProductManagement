using Domain.DTO;
using Domain.Entities;
using Domain.DTO;
using Domain.Entities;
using Domain.Interfaces;

namespace Service
{
    public class EducationService
    {
        private readonly IEducationRepository _repo;

        public EducationService(IEducationRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<Education>> GetAllAsync() => await _repo.GetAllAsync();

        public async Task<IEnumerable<Education>> GetByUserIdAsync(int userId) => await _repo.GetByUserIdAsync(userId);

        public async Task<Education?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

        public async Task AddEducation(EducationDTO dto, int userId)
        {
            var education = new Education
            {
                UserID = userId,
                UniversityCode = dto.UniversityCode,
                MajorCode = dto.MajorCode,
                DegreeCode = dto.DegreeCode,
                Location = dto.Location,
                GPA = dto.GPA,
                GraduationMonth = dto.GraduationMonth,
                GraduationYear = dto.GraduationYear,
                CreatedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(education);
        }

        public async Task UpdateEducation(int id, EducationDTO dto)
        {
            var education = new Education
            {
                Id = id,
                UserID = dto.UserID.Value,
                UniversityCode = dto.UniversityCode,
                MajorCode = dto.MajorCode,
                DegreeCode = dto.DegreeCode,
                Location = dto.Location,
                GPA = dto.GPA,
                GraduationMonth = dto.GraduationMonth,
                GraduationYear = dto.GraduationYear,
                UpdatedAt = DateTime.UtcNow
            };
            await _repo.UpdateAsync(education);
        }

        public async Task DeleteEducation(int id)
        {
            await _repo.DeleteAsync(id);
        }
    }
}

