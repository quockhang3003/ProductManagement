using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class UserQuestionAnswerDTO
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        public string Answer { get; set; } = string.Empty;

        public DateTime? UpdatedAt { get; set; }
    }

    public class UserQuestionAnswerCreateDTO
    {
        [Required]
        public int QuestionId { get; set; }

        [Required]
        public string Answer { get; set; } = string.Empty;
    }

    public class UserQuestionAnswerUpdateDTO
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Answer { get; set; } = string.Empty;
    }

    public class QuestionWithAnswerDTO
    {
        public int Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public int MaxWords { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public bool IsActive { get; set; }
        public string Answer { get; set; } = string.Empty;
        public DateTime? AnswerUpdatedAt { get; set; }
        public bool HasError { get; set; }
        public bool IsSaved { get; set; }
    }
}
