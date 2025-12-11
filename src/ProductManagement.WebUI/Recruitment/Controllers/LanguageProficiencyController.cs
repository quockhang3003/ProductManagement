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
    public class LanguageProficiencyController : ControllerBase
    {
        private readonly LanguageProficiencyService _service;
        public LanguageProficiencyController(LanguageProficiencyService service)
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
                        var userLanguageProficiency = await _service.GetByUserIdAsync(userId);
                        return Ok(userLanguageProficiency);
                    }
                }

                if (User.IsInRole("Admin"))
                {
                    var allLanguageProficiency = await _service.GetAllAsync();
                    return Ok(allLanguageProficiency);
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
                if (User.IsInRole("Admin"))
                {
                    var prof = await _service.GetByUserIdAsync(userId);
                    return Ok(prof);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int currentUserId) && currentUserId == userId)
                {
                    var prof = await _service.GetByUserIdAsync(userId);
                    return Ok(prof);
                }

                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddLanguageProficiency(LanguageProficiencyDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to add");

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                    return BadRequest("Invalid user session");

                await _service.AddLanguageProficiency(dto, userId);
                return Ok(new { message = "Added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLanguageProficiency(int id, LanguageProficiencyDTO dto)
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
                await _service.UpdateLanguageProficiency(dto, id);
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguageProficiency(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to delete");

                await _service.DeleteLanguageProficiency(id);
                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
