using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class AttachmentsDTO
    {
        public int Id { get; set; }
        public string AttachmentName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
    public class AttachmentUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int AttachmentId { get; set; }
    }
}
