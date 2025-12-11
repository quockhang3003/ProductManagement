using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemConfigurationController : ControllerBase
    {
        private readonly SystemConfigurationService _service;

        public SystemConfigurationController(SystemConfigurationService service)
        {
            _service = service;
        }

        [HttpGet("grouped")]
        public async Task<IActionResult> GetAllGrouped()
        {
            try
            {
                var result = await _service.GetAllGroupedAsync();
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("types")]
        public async Task<IActionResult> GetAllTypes()
        {
            try
            {
                var types = await _service.GetAllTypesAsync();
                return Ok(types);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("type/{type}")]
        public async Task<IActionResult> GetByType(string type)
        {
            try
            {
                var result = await _service.GetByTypeAsync(type);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }

        [HttpGet("type/{type}/active")]
        public async Task<IActionResult> GetActiveByType(string type)
        {
            try
            {
                var result = await _service.GetActiveByTypeAsync(type);
                return Ok(result);
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateSystemConfigurationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (success, message, id) = await _service.AddAsync(dto);

                return success
                    ? Ok(new { Id = id, Message = message })
                    : BadRequest(new { Message = message });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateSystemConfigurationDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (success, message) = await _service.UpdateAsync(dto);

                return success
                    ? Ok(new { Message = message })
                    : BadRequest(new { Message = message });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (success, message) = await _service.DeleteAsync(id);

                return success
                    ? Ok(new { Message = message })
                    : BadRequest(new { Message = message });
            }
            catch (Exception e)
            {
                return BadRequest(new { Message = $"Error: {e.Message}" });
            }
        }
    }
}
