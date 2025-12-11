using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class LanguageAbilitiesDTO
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        [Required(ErrorMessage = "Language is required")]
        public string Language { get; set; } = string.Empty;

        [Required(ErrorMessage = "Speaking level is required")]
        public string SpeakingCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reading level is required")]
        public string ReadingCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Writing level is required")]
        public string WritingCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Listening level is required")]
        public string ListeningCode { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
