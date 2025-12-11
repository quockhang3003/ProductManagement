using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class HeardAboutDTO
    {
        public int Id { get; set; }
        public int? RecruitmentProgramId { get; set; }
        public string RecruitmentProgramName { get; set; }
        public bool KpmgWebsite { get; set; }
        public bool UniversityClubs { get; set; }
        public bool Others { get; set; }
        public bool FacebookGroup { get; set; }
        public bool UniversityWebsite { get; set; }
        public bool KpmgPresentation { get; set; }
        public bool KpmgSocialMedia { get; set; }
        public bool Tiktok { get; set; }
        public bool CareerTalk { get; set; }
        public bool Professor { get; set; }
        public bool CampusAmbassadors { get; set; }
        public bool AgreeToTerms { get; set; }
        public bool HasAnySelection()
        {
            return KpmgWebsite || UniversityClubs || Others || FacebookGroup ||
                   UniversityWebsite || KpmgPresentation || KpmgSocialMedia || Tiktok ||
                   CareerTalk || Professor || CampusAmbassadors;
        }
        public DateTime CreatedAt { get; set; }
    }
    public class HeardAboutSaveRequest
    {
        public string UserEmail { get; set; } = string.Empty;
        public bool KpmgWebsite { get; set; }
        public bool UniversityClubs { get; set; }
        public bool Others { get; set; }
        public bool FacebookGroup { get; set; }
        public bool UniversityWebsite { get; set; }
        public bool KpmgPresentation { get; set; }
        public bool KpmgSocialMedia { get; set; }
        public bool Tiktok { get; set; }
        public bool CareerTalk { get; set; }
        public bool Professor { get; set; }
        public bool CampusAmbassadors { get; set; }
        public bool AgreeToTerms { get; set; }
    }

    public class HeardAboutSaveResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
