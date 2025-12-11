using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class Attachments
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string AttachmentName { get; set; } = string.Empty;

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
    }

}
