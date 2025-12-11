using Domain.DTO;
using Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HeardAboutController : ControllerBase
    {
        private readonly HeardAboutService _service;
        private readonly UserService _userService;

        public HeardAboutController(HeardAboutService service, UserService userService)
        {
            _service = service;
            _userService = userService;
        }

        [HttpGet("user/{userEmail}")]
        public async Task<IActionResult> GetByUserEmail(string userEmail)
        {
            try
            {
                var user = await _userService.GetUserByEmail(userEmail);
                if (user == null)
                    return NotFound(new { Message = "User not found" });

                var heardAbout = await _service.GetByUserIdAsync(user.Id);

                if (heardAbout == null)
                {
                    return Ok(new HeardAboutDTO());
                }

                return Ok(heardAbout);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = e.Message });
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] HeardAboutSaveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { Success = false, Message = "Invalid data", Errors = ModelState });

                if (string.IsNullOrEmpty(request.UserEmail))
                    return BadRequest(new { Success = false, Message = "User email is required" });

                var user = await _userService.GetUserByEmail(request.UserEmail);
                if (user == null)
                    return NotFound(new { Success = false, Message = "User not found" });

                var response = await _service.SaveOrUpdateAsync(user.Id, request);

                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception e)
            {
                return BadRequest(new { Success = false, Message = e.Message });
            }
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> Update(int userId, [FromBody] HeardAboutSaveRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var response = await _service.UpdateAsync(userId, request);
                return response.Success ? Ok(response) : BadRequest(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _service.DeleteAsync(id);
                return result
                    ? Ok(new { Message = "Application deleted successfully" })
                    : NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }
    }
}


