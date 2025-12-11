using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Transcript
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        [Required]
        [MaxLength(50)]
        public string UniversityCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "File Name")]
        public string FileName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "File Data")]
        public byte[] FileData { get; set; } = Array.Empty<byte>();

        [Required]
        [Display(Name = "Content Type")]
        public string ContentType { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; }

        public University? University { get; set; }

        [NotMapped]
        public string UniversityName { get; set; } = string.Empty;
    }

}
