using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTO
{
    public class UserPhotoDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
    }


    public class PhotoUploadRequest
    {
        public string UserEmail { get; set; } 
        public IFormFile Photo { get; set; }
    }


    public class PhotoUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PhotoUrl { get; set; }
        public int PhotoId { get; set; }
    }


}
