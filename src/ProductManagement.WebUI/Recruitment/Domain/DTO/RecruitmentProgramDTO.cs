using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class RecruitmentProgramDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
        public DateTime UpdatedOn { get; set; }
        public int TotalSubmitted { get; set; }
    }
    public class CreateRecruitmentProgramDTO
    {
        public string Name { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
    }

    public class UpdateRecruitmentProgramDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? OpenDate { get; set; }
        public DateTime? CloseDate { get; set; }
    }

}
