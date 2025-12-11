using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Education
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "User ID is required")]
        public int UserID { get; set; }
        [Required]
        [MaxLength(50)]
        public string UniversityCode { get; set; } = string.Empty;
        [Required]
        [MaxLength(50)]
        public string MajorCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DegreeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter location")]
        public string Location { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter GPA")]
        [Range(0.01, 4.0, ErrorMessage = "GPA must be between 0.01 and 4.0")]
        public decimal GPA { get; set; }

        [Required(ErrorMessage = "Please enter maximum GPA")]
        [Range(1, 10, ErrorMessage = "Out of must be between 1 and 10")]
        [NotMapped]
        public decimal OutOf { get; set; }

        [Required(ErrorMessage = "Please select graduation month")]
        public int GraduationMonth { get; set; } 

        [Required(ErrorMessage = "Please select graduation year")]
        [Range(1900, 2030, ErrorMessage = "Please select a valid graduation year")]
        public int GraduationYear { get; set; }

        [NotMapped]
        public string FormattedGraduationDate => $"{GraduationMonth:D2}/{GraduationYear}";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
