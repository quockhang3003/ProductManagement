using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class WorkExperienceDTO
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CompanyName { get; set; }
        public string JobTitle { get; set; }
        public string? EmploymentTypeCode { get; set; }

        [MaxLength(500, ErrorMessage = "Main duties must not exceed 500 characters")]
        public string MainDuties { get; set; } = string.Empty;
        public string Achievement { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeleteAt { get; set; }
    }
}
