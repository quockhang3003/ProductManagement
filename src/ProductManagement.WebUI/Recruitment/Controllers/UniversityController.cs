using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UniversityController : ControllerBase
    {
        private readonly UniversityService _service;
        private readonly UserService _userService;

        public UniversityController(UniversityService service, UserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var university = await _service.GetAllAsync();
                return university == null ? NotFound() : Ok(university);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("user/{userEmail}")]
        public async Task<IActionResult> GetByUserEmail(string userEmail)
        {
            try
            {
                var user = await _userService.GetUserByEmail(userEmail);
                if (user == null)
                    return NotFound("User not found");

                var universities = await _service.GetByUserIdAsync(user.Id);

                var universityDTOs = universities.Select(u => new SystemConfigurationDTO
                {
                    Id = u.Id,
                    Code = u.UniversityName, 
                    DisplayName = u.UniversityName,
                    Type = "University",
                    IsActive = true
                }).ToList();

                return Ok(universityDTOs);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }

    }
}
