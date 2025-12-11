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
    public class LanguageAbilitiesController : ControllerBase
    {
        private readonly LanguageAbilitiesService _service;
        public LanguageAbilitiesController(LanguageAbilitiesService service)
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
                        var userLanguageAbilities = await _service.GetByUserIdAsync(userId);
                        return Ok(userLanguageAbilities);
                    }
                }

                if (User.IsInRole("Admin"))
                {
                    var allLanguageAbilities = await _service.GetAllAsync();
                    return Ok(allLanguageAbilities);
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
                    var language = await _service.GetByUserIdAsync(userId);
                    return Ok(language);
                }

                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int currentUserId) && currentUserId == userId)
                {
                    var language = await _service.GetByUserIdAsync(userId);
                    return Ok(language);
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
        public async Task<IActionResult> AddLanguageAbilities(LanguageAbilitiesDTO dto)
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

                await _service.AddLanguageAbilities(dto, userId);
                return Ok(new { message = "Added successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateLanguageAbilities(int id, LanguageAbilitiesDTO dto)
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
                await _service.UpdateLanguageAbilities(id, dto);
                return Ok(new { message = "Updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLanguageAbilities(int id)
        {
            try
            {
                if (!User.Identity.IsAuthenticated)
                    return Unauthorized("Please login to delete");

                await _service.DeleteLanguageAbilities(id);
                return Ok(new { message = "Deleted successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
