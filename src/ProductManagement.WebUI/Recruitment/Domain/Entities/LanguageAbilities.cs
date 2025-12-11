using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class LanguageAbilities
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string Language {  get; set; }
        [MaxLength(50)]
        public string? SpeakingCode { get; set; }

        [MaxLength(50)]
        public string? ReadingCode { get; set; }

        [MaxLength(50)]
        public string? WritingCode { get; set; }

        [MaxLength(50)]
        public string? ListeningCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
