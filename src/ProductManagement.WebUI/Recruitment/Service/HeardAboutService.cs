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
    public class HeardAboutService
    {
        private readonly IHeardAboutRepository _heardAboutRepo;
        private readonly IRecruitmentProgramRepository _programRepo;

        public HeardAboutService(
            IHeardAboutRepository heardAboutRepo,
            IRecruitmentProgramRepository programRepo)
        {
            _heardAboutRepo = heardAboutRepo;
            _programRepo = programRepo;
        }

        public async Task<HeardAboutDTO> GetByUserIdAsync(int userId)
        {
            var heardAbout = await _heardAboutRepo.GetByUserIdAsync(userId);
            if (heardAbout == null)
                return null;

            string programName = null;
            if (heardAbout.RecruitmentProgramId.HasValue)
            {
                var program = await _programRepo.GetByIdAsync(heardAbout.RecruitmentProgramId.Value);
                programName = program?.Name;
            }

            return new HeardAboutDTO
            {
                Id = heardAbout.Id,
                RecruitmentProgramId = heardAbout.RecruitmentProgramId,
                RecruitmentProgramName = programName,
                KpmgWebsite = heardAbout.KpmgWebsite,
                UniversityClubs = heardAbout.UniversityClubs,
                Others = heardAbout.Others,
                FacebookGroup = heardAbout.FacebookGroup,
                UniversityWebsite = heardAbout.UniversityWebsite,
                KpmgPresentation = heardAbout.KpmgPresentation,
                KpmgSocialMedia = heardAbout.KpmgSocialMedia,
                Tiktok = heardAbout.Tiktok,
                CareerTalk = heardAbout.CareerTalk,
                Professor = heardAbout.Professor,
                CampusAmbassadors = heardAbout.CampusAmbassadors,
                AgreeToTerms = heardAbout.AgreeToTerms,
                CreatedAt = heardAbout.CreatedAt
            };
        }

        public async Task<HeardAboutSaveResponse> SaveOrUpdateAsync(int userId, HeardAboutSaveRequest request)
        {
            try
            {
                var existing = await _heardAboutRepo.GetByUserIdAsync(userId);

                if (existing == null)
                {
                    return await CreateAsync(userId, ConvertToSaveRequest(request));
                }
                else
                {
                    return await UpdateAsync(userId, ConvertToSaveRequest(request));
                }
            }
            catch (Exception e)
            {
                return new HeardAboutSaveResponse
                {
                    Success = false,
                    Message = $"Error saving application: {e.Message}"
                };
            }
        }

        private HeardAboutSaveRequest ConvertToSaveRequest(HeardAboutSaveRequest request)
        {
            return new HeardAboutSaveRequest
            {
                KpmgWebsite = request.KpmgWebsite,
                UniversityClubs = request.UniversityClubs,
                Others = request.Others,
                FacebookGroup = request.FacebookGroup,
                UniversityWebsite = request.UniversityWebsite,
                KpmgPresentation = request.KpmgPresentation,
                KpmgSocialMedia = request.KpmgSocialMedia,
                Tiktok = request.Tiktok,
                CareerTalk = request.CareerTalk,
                Professor = request.Professor,
                CampusAmbassadors = request.CampusAmbassadors,
                AgreeToTerms = request.AgreeToTerms
            };
        }

        public async Task<HeardAboutSaveResponse> CreateAsync(int userId, HeardAboutSaveRequest request)
        {
            var existing = await _heardAboutRepo.GetByUserIdAsync(userId);
            if (existing != null)
            {
                return new HeardAboutSaveResponse
                {
                    Success = false,
                    Message = "You have already submitted your application"
                };
            }

            // Get active program
            var activeProgram = await _programRepo.GetActiveProgramAsync();
            if (activeProgram == null)
            {
                return new HeardAboutSaveResponse
                {
                    Success = false,
                    Message = "No active recruitment program found. Please try again later."
                };
            }

            var heardAbout = new HeardAbout
            {
                UserId = userId,
                RecruitmentProgramId = activeProgram.Id,
                KpmgWebsite = request.KpmgWebsite,
                UniversityClubs = request.UniversityClubs,
                Others = request.Others,
                FacebookGroup = request.FacebookGroup,
                UniversityWebsite = request.UniversityWebsite,
                KpmgPresentation = request.KpmgPresentation,
                KpmgSocialMedia = request.KpmgSocialMedia,
                Tiktok = request.Tiktok,
                CareerTalk = request.CareerTalk,
                Professor = request.Professor,
                CampusAmbassadors = request.CampusAmbassadors,
                AgreeToTerms = request.AgreeToTerms,
                CreatedAt = DateTime.Now
            };

            var id = await _heardAboutRepo.CreateAsync(heardAbout);

            return new HeardAboutSaveResponse
            {
                Success = true,
                Message = $"Application submitted successfully for {activeProgram.Name}"
            };
        }

        public async Task<HeardAboutSaveResponse> UpdateAsync(int userId, HeardAboutSaveRequest request)
        {
            var existing = await _heardAboutRepo.GetByUserIdAsync(userId);
            if (existing == null)
            {
                return new HeardAboutSaveResponse
                {
                    Success = false,
                    Message = "No existing application found"
                };
            }

            // Update data (giữ nguyên RecruitmentProgramId)
            existing.KpmgWebsite = request.KpmgWebsite;
            existing.UniversityClubs = request.UniversityClubs;
            existing.Others = request.Others;
            existing.FacebookGroup = request.FacebookGroup;
            existing.UniversityWebsite = request.UniversityWebsite;
            existing.KpmgPresentation = request.KpmgPresentation;
            existing.KpmgSocialMedia = request.KpmgSocialMedia;
            existing.Tiktok = request.Tiktok;
            existing.CareerTalk = request.CareerTalk;
            existing.Professor = request.Professor;
            existing.CampusAmbassadors = request.CampusAmbassadors;
            existing.AgreeToTerms = request.AgreeToTerms;

            var result = await _heardAboutRepo.UpdateAsync(existing);

            return new HeardAboutSaveResponse
            {
                Success = result,
                Message = result ? "Application updated successfully" : "Failed to update application"
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _heardAboutRepo.DeleteAsync(id);
        }
    }
}
