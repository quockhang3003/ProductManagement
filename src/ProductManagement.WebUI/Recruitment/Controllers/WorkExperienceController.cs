using Domain.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;
using System.Security.Claims;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkExperienceController : ControllerBase
    {
        private readonly WorkExperienceService _service;
        public WorkExperienceController(WorkExperienceService service)
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
                        var userWorkExperience = await _service.GetByUserIdAsync(userId);
                        return Ok(userWorkExperience);
                    }
                }

                if (User.IsInRole("Admin"))
                {
                    var allWorkExperience = await _service.GetAllAsync();
                    return Ok(allWorkExperience);
                }

                return Unauthorized("Please login to view records");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("user/{userId}")]
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
                            var workExperience = await _service.GetByUserIdAsync(userId);
                            return Ok(workExperience);
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
        public async Task<IActionResult> AddWorkExperience(WorkExperienceDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to add work experience");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest("Invalid user session");

                await _service.AddWorkExperience(dto, userId);
                return Ok(new { message = "Work experience added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorkExperience(int id, WorkExperienceDTO dto)
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
                await _service.UpdateWorkExperience(dto, id);
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorkExperience(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to delete");

                await _service.DeleteWorkExperience(id);
                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
