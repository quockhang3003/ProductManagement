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
    public class RecruitmentProgramService
    {
        private readonly IRecruitmentProgramRepository _repo;

        public RecruitmentProgramService(IRecruitmentProgramRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<RecruitmentProgramDTO>> GetAllAsync()
        {
            var programs = await _repo.GetAllAsync();
            var programDTOs = new List<RecruitmentProgramDTO>();

            foreach (var program in programs)
            {
                // Count submissions theo ProgramId
                var totalSubmitted = await _repo.GetTotalSubmittedByProgramIdAsync(program.Id);

                programDTOs.Add(new RecruitmentProgramDTO
                {
                    Id = program.Id,
                    Name = program.Name,
                    OpenDate = program.OpenDate,
                    CloseDate = program.CloseDate,
                    UpdatedOn = program.UpdatedOn,
                    TotalSubmitted = totalSubmitted
                });
            }

            return programDTOs;
        }

        public async Task<RecruitmentProgramDTO> GetByIdAsync(int id)
        {
            var program = await _repo.GetByIdAsync(id);
            if (program == null) return null;

            var totalSubmitted = await _repo.GetTotalSubmittedByProgramIdAsync(program.Id);

            return new RecruitmentProgramDTO
            {
                Id = program.Id,
                Name = program.Name,
                OpenDate = program.OpenDate,
                CloseDate = program.CloseDate,
                UpdatedOn = program.UpdatedOn,
                TotalSubmitted = totalSubmitted
            };
        }

        public async Task<int> AddAsync(CreateRecruitmentProgramDTO dto)
        {
            var program = new RecruitmentProgram
            {
                Name = dto.Name,
                OpenDate = dto.OpenDate,
                CloseDate = dto.CloseDate,
                UpdatedOn = DateTime.Now
            };

            return await _repo.AddAsync(program);
        }

        public async Task<bool> UpdateAsync(UpdateRecruitmentProgramDTO dto)
        {
            var program = new RecruitmentProgram
            {
                Id = dto.Id,
                Name = dto.Name,
                OpenDate = dto.OpenDate,
                CloseDate = dto.CloseDate,
                UpdatedOn = DateTime.Now
            };

            return await _repo.UpdateAsync(program);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repo.DeleteAsync(id);
        }

        public async Task<RecruitmentProgramDTO> GetActiveProgramAsync()
        {
            var program = await _repo.GetActiveProgramAsync();
            if (program == null) return null;

            var totalSubmitted = await _repo.GetTotalSubmittedByProgramIdAsync(program.Id);

            return new RecruitmentProgramDTO
            {
                Id = program.Id,
                Name = program.Name,
                OpenDate = program.OpenDate,
                CloseDate = program.CloseDate,
                UpdatedOn = program.UpdatedOn,
                TotalSubmitted = totalSubmitted
            };
        }

    }
}
