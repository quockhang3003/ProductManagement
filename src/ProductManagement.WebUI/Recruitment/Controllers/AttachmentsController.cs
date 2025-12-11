using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using Domain.Entities;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentsController : ControllerBase
    {
        private readonly AttachmentsService _service;
        private readonly UserService _userService;
        private readonly ILogger<AttachmentsController> _logger;

        public AttachmentsController(
            AttachmentsService service,
            UserService userService,
            ILogger<AttachmentsController> logger)
        {
            _service = service;
            _userService = userService;
            _logger = logger;
        }

        [HttpGet("user/{userEmail}")]
        public async Task<IActionResult> GetByUserEmail(string userEmail)
        {
            try
            {
                _logger.LogInformation("Fetching attachments for user {UserEmail}", userEmail);

                var user = await _userService.GetUserByEmail(userEmail);
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserEmail}", userEmail);
                    return NotFound("User not found");
                }

                var attachments = await _service.GetByUserIdAsync(user.Id);

                var attachmentDTOs = attachments.Select(a => new AttachmentsDTO
                {
                    Id = a.Id,
                    AttachmentName = a.AttachmentName,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    UploadedAt = a.UploadedAt
                }).ToList();

                _logger.LogInformation("Found {Count} attachments for user {UserEmail}", attachmentDTOs.Count, userEmail);

                return Ok(attachmentDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching attachments for {UserEmail}", userEmail);
                return BadRequest($"Error: {ex.Message}");
            }
        }

        public class AttachmentUploadRequest
        {
            [FromForm(Name = "AttachmentFile")]
            public IFormFile AttachmentFile { get; set; }

            [FromForm(Name = "UserEmail")]
            public string UserEmail { get; set; }

            [FromForm(Name = "AttachmentName")]
            public string AttachmentName { get; set; }
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadAttachment([FromForm] AttachmentUploadRequest request)
        {
            try
            {
                _logger.LogInformation("Upload attempt by {UserEmail}, file {FileName}",
                    request.UserEmail, request.AttachmentFile?.FileName);

                if (request.AttachmentFile == null || request.AttachmentFile.Length == 0)
                {
                    _logger.LogWarning("Upload failed: No file uploaded by {UserEmail}", request.UserEmail);
                    return BadRequest(new { Success = false, Message = "No file uploaded" });
                }

                var user = await _userService.GetUserByEmail(request.UserEmail);
                if (user == null)
                {
                    _logger.LogWarning("Upload failed: User not found {UserEmail}", request.UserEmail);
                    return BadRequest(new { Success = false, Message = "User not found" });
                }

                // Validate file
                const long maxFileSize = 2 * 1024 * 1024; // 2MB
                if (request.AttachmentFile.Length > maxFileSize)
                {
                    _logger.LogWarning("Upload failed: File too large ({Size} bytes) from {UserEmail}",
                        request.AttachmentFile.Length, request.UserEmail);
                    return BadRequest(new { Success = false, Message = "File size must be less than 2MB" });
                }

                var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "application/pdf" };
                if (!allowedTypes.Contains(request.AttachmentFile.ContentType.ToLower()))
                {
                    _logger.LogWarning("Upload failed: Invalid content type {ContentType} from {UserEmail}",
                        request.AttachmentFile.ContentType, request.UserEmail);
                    return BadRequest(new { Success = false, Message = "Only JPG, PNG and PDF files are allowed" });
                }

                
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await request.AttachmentFile.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                var attachment = new Attachments
                {
                    UserId = user.Id,
                    AttachmentName = request.AttachmentName,
                    FileName = request.AttachmentFile.FileName.Trim(),
                    FileData = fileData,
                    ContentType = request.AttachmentFile.ContentType.Trim(),
                    UploadedAt = DateTime.Now
                };


                var attachmentId = await _service.CreateAsync(attachment);

                _logger.LogInformation("Upload success: Attachment {AttachmentId} uploaded by {UserEmail}",
                    attachmentId, request.UserEmail);

                return Ok(new AttachmentUploadResponse
                {
                    Success = true,
                    Message = "Attachment uploaded successfully",
                    AttachmentId = attachmentId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Upload failed: Exception while uploading attachment for {UserEmail}", request.UserEmail);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet("view/{id}")]
        public async Task<IActionResult> ViewAttachment(int id)
        {
            var attachment = await _service.GetByIdAsync(id);
            if (attachment == null)
                return NotFound("Attachment not found");

            return File(attachment.FileData, attachment.ContentType);
        }



        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Delete attempt for attachment {Id}", id);

                var success = await _service.DeleteAsync(id);
                if (success)
                {
                    _logger.LogInformation("Attachment {Id} deleted successfully", id);
                    return Ok(new { Success = true, Message = "Attachment deleted successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to delete attachment {Id}", id);
                    return BadRequest(new { Success = false, Message = "Failed to delete attachment" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting attachment {Id}", id);
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}
