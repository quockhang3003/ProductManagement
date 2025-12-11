using Domain.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfessionalQualificationController : ControllerBase
    {
        private readonly ProfessionalQualificationService _service;
        public ProfessionalQualificationController(ProfessionalQualificationService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        var userProfessionalQualification = await _service.GetByUserIdAsync(userId);
                        return Ok(userProfessionalQualification);
                    }
                }

                if (User.IsInRole("Admin"))
                {
                    var allProfessionalQualification = await _service.GetAllAsync();
                    return Ok(allProfessionalQualification);
                }

                return Unauthorized("Please login to view records");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("user/{userId}")]
        [Authorize(Policy ="UserOrAdmin")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(userIdClaim, out int currentUserId))
                    {
                        if (currentUserId == userId || User.IsInRole("Admin"))
                        {
                            var professionalQualifications = await _service.GetByUserIdAsync(userId);
                            return Ok(professionalQualifications);
                        }
                    }
                }

                return Unauthorized("Access denied");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddProfessionalQualification(ProfessionalQualificationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to add professional qualification");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest("Invalid user session");

                await _service.AddProfessionalQualification(dto, userId);
                return Ok(new { message = "Professional qualification added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProfessionalQualification(int id, ProfessionalQualificationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to update");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest("Invalid user session");

                dto.UserID = userId;
                await _service.UpdateProfessionalQualification(dto, id);
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProfessionalQualification(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to delete");

                await _service.DeleteProfessionalQualification(id);
                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
