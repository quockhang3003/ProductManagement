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
    public class UserQuestionAnswerController : ControllerBase
    {
        private readonly UserQuestionAnswerService _service;


        public UserQuestionAnswerController(UserQuestionAnswerService service)
        {
            _service = service;
        }

        [HttpGet("my-answers")]
        public async Task<IActionResult> GetMyAnswers()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User ID not found in token");


                var answers = await _service.GetByUserIdAsync(userId.Value);
                return Ok(answers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


        [HttpGet("questions-with-answers")]
        public async Task<IActionResult> GetQuestionsWithAnswers()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User ID not found in token");


                var result = await _service.GetQuestionsWithAnswersAsync(userId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUserAnswers(int userId)
        {
            try
            {
                var answers = await _service.GetByUserIdAsync(userId);
                return Ok(answers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


        [HttpGet("user/{userId}/questions-with-answers")]
        [Authorize(Policy ="AdminOnly")]
        public async Task<IActionResult> GetUserQuestionsWithAnswers(int userId)
        {
            try
            {
                var result = await _service.GetQuestionsWithAnswersAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }
        [HttpPost("save-answer")]
        public async Task<IActionResult> SaveAnswer([FromBody] UserQuestionAnswerCreateDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User ID not found in token");


                var result = await _service.SaveAnswerAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


        // Save multiple answers at once
        [HttpPost("save-multiple")]
        public async Task<IActionResult> SaveMultipleAnswers([FromBody] List<UserQuestionAnswerCreateDTO> answers)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);


                if (answers == null || !answers.Any())
                    return BadRequest("No answers provided");


                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User ID not found in token");


                var result = await _service.SaveMultipleAnswersAsync(userId.Value, answers);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


        // Delete answer
        [HttpDelete("answer/{questionId}")]
        public async Task<IActionResult> DeleteAnswer(int questionId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User ID not found in token");


                var result = await _service.DeleteAnswerAsync(questionId, userId.Value);
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


        // Delete all user's answers
        [HttpDelete("my-answers")]
        public async Task<IActionResult> DeleteAllMyAnswers()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized("User ID not found in token");


                var result = await _service.DeleteAllUserAnswersAsync(userId.Value);
                return result ? NoContent() : NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error", details = ex.Message });
            }
        }


        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
                return userId;
            return null;
        }
    }
}
