using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotoController : ControllerBase
    {
        private readonly UserPhotoService _service;


        public PhotoController(UserPhotoService service)
        {
            _service = service;
        }


        [HttpGet("base64/{userEmail}")]
        public async Task<IActionResult> GetUserPhotoBase64(string userEmail)
        {
            var photo = await _service.GetUserPhotoByEmailAsync(userEmail);
            if (photo == null || photo.FileData == null || photo.FileData.Length == 0)
                return NotFound("No photo found");

            var base64 = Convert.ToBase64String(photo.FileData);
            var imageSrc = $"data:{photo.ContentType};base64,{base64}";

            return Ok(new { imageSrc });
        }



        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserPhoto(int userId)
        {
            var photo = await _service.GetUserPhotoAsync(userId);
            return photo == null ? NotFound() : Ok(photo);
        }


        [HttpPost("upload")]
        public async Task<IActionResult> UploadPhoto([FromForm] PhotoUploadRequest request)
        {
            Console.WriteLine($"[DEBUG] Upload request: Email={request.UserEmail}, File={request.Photo?.FileName}");
            var result = await _service.UploadPhotoAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeletePhoto(int userId)
        {
            var result = await _service.DeletePhotoAsync(userId);
            return result ? Ok("Photo deleted successfully") : NotFound();
        }
    }
}
