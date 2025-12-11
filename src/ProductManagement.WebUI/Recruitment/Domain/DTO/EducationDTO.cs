using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class EducationDTO
    {
        public int Id { get; set; }
        public int? UserID { get; set; }
        [Required(ErrorMessage = "University is required")]
        public string UniversityCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Major is required")]
        public string MajorCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Degree is required")]
        public string DegreeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter GPA")]
        [Range(0.01, 4.0, ErrorMessage = "GPA must be between 0.01 and 4.0")]
        public decimal GPA { get; set; }

        [Required(ErrorMessage = "Please enter maximum GPA")]
        [Range(1, 10, ErrorMessage = "Out of must be between 1 and 10")]
        public decimal OutOf { get; set; }

        [Required(ErrorMessage = "Please select graduation month")]
        public int GraduationMonth { get; set; }

        [Required(ErrorMessage = "Please select graduation year")]
        [Range(1900, 2030, ErrorMessage = "Please select a valid graduation year")]
        public int GraduationYear { get; set; }
        
    }
}
