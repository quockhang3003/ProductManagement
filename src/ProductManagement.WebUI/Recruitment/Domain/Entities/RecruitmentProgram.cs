using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class RecruitmentProgram
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Recruitment Program Name")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Open Date")]
        public DateTime? OpenDate { get; set; }

        [Display(Name = "Close Date")]
        public DateTime? CloseDate { get; set; }

        public DateTime UpdatedOn { get; set; }

    }
}
