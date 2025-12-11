using Azure.Core;
using Domain.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranscriptController : ControllerBase
    {
        private readonly TranscriptService _service;
        private readonly UserService _userService;


        public TranscriptController(TranscriptService service, UserService userService)
        {
            _service = service;
            _userService = userService;
        }


        [HttpGet("user/{userEmail}")]
        public async Task<IActionResult> GetByUserEmail(string userEmail)
        {
            try
            {
                var userId = await _userService.GetUserIdByEmailAsync(userEmail);
                if (userId == null)
                    return NotFound("User not found");


                var transcripts = await _service.GetByUserIdAsync(userId.Value);

                var transcriptDTOs = transcripts.Select(t => new TranscriptDTO
                {
                    Id = t.Id,
                    UniversityCode = t.UniversityCode,
                    UniversityName = t.UniversityName,
                    FileName = t.FileName,
                    ContentType = t.ContentType,
                    UploadedAt = t.UploadedAt
                }).ToList();


                return Ok(transcriptDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public class TranscriptUploadRequest
        {
            [FromForm]
            public IFormFile TranscriptFile { get; set; }


            [FromForm]
            public string UserEmail { get; set; }


            [FromForm]
            public string UniversityCode { get; set; }
        }


        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadTranscript([FromForm] TranscriptUploadRequest request)
        {
            try
            {
                if (request.TranscriptFile == null || request.TranscriptFile.Length == 0)
                    return BadRequest(new { Success = false, Message = "No file uploaded" });


                var userId = await _userService.GetUserIdByEmailAsync(request.UserEmail);
                if (userId == null)
                    return BadRequest(new { Success = false, Message = "User not found" });


                // Validate file
                const long maxFileSize = 2 * 1024 * 1024;
                if (request.TranscriptFile.Length > maxFileSize)
                    return BadRequest(new { Success = false, Message = "File size must be less than 2MB" });


                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "application/pdf" };
                if (!allowedTypes.Contains(request.TranscriptFile.ContentType.ToLower()))
                    return BadRequest(new { Success = false, Message = "Only JPG, PNG and PDF files are allowed" });


                // Convert file to byte array
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await request.TranscriptFile.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }


                var transcript = new Transcript
                {
                    UserId = userId.Value,
                    UniversityCode = request.UniversityCode,
                    FileName = request.TranscriptFile.FileName,
                    FileData = fileData,
                    ContentType = request.TranscriptFile.ContentType,
                    UploadedAt = DateTime.Now
                };


                var transcriptId = await _service.CreateAsync(transcript);


                return Ok(new TranscriptUploadResponse
                {
                    Success = true,
                    Message = "Transcript uploaded successfully",
                    TranscriptId = transcriptId
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }



        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewTranscript(int id)
        {
            try
            {
                var transcript = await _service.GetByIdAsync(id);
                if (transcript == null)
                    return NotFound("Transcript not found");


                return File(transcript.FileData, transcript.ContentType);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _service.DeleteAsync(id);
                return success ? Ok(new { Success = true, Message = "Transcript deleted successfully" })
                              : BadRequest(new { Success = false, Message = "Failed to delete transcript" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

    
    }
}
