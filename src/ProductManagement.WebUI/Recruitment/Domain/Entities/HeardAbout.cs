using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class HeardAbout
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? RecruitmentProgramId { get; set; }

        [Display(Name = "KPMG Website")]
        public bool KpmgWebsite { get; set; }

        [Display(Name = "University Clubs")]
        public bool UniversityClubs { get; set; }

        [Display(Name = "Others")]
        public bool Others { get; set; }

        [Display(Name = "Facebook Group")]
        public bool FacebookGroup { get; set; }

        [Display(Name = "University Website")]
        public bool UniversityWebsite { get; set; }

        [Display(Name = "KPMG Presentation")]
        public bool KpmgPresentation { get; set; }

        [Display(Name = "KPMG Social Media")]
        public bool KpmgSocialMedia { get; set; }

        [Display(Name = "Tiktok")]
        public bool Tiktok { get; set; }

        [Display(Name = "Career Talk")]
        public bool CareerTalk { get; set; }

        [Display(Name = "Professor")]
        public bool Professor { get; set; }

        [Display(Name = "Campus Ambassadors")]
        public bool CampusAmbassadors { get; set; }

        public bool AgreeToTerms { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
