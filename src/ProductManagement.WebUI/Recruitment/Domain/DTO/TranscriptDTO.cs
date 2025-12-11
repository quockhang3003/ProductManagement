using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class TranscriptDTO
    {
        public int Id { get; set; }
        public string UniversityCode { get; set; } = string.Empty;
        public string UniversityName { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedAt { get; set; }
    }
    public class TranscriptUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TranscriptId { get; set; }
    }
}
