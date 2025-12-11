using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class QuestionDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int MaxWords { get; set; }
        public bool IsRequired { get; set; } = true;
        public int Order { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
