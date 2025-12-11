using Domain.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Service;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecruitmentProgramController : ControllerBase
    {
        private readonly RecruitmentProgramService _service;

        public RecruitmentProgramController(RecruitmentProgramService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var programs = await _service.GetAllAsync();
                return programs == null ? NotFound() : Ok(programs);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var program = await _service.GetByIdAsync(id);
                return program == null ? NotFound() : Ok(program);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActiveProgram()
        {
            try
            {
                var program = await _service.GetActiveProgramAsync();
                return program == null
                    ? NotFound(new { Message = "No active recruitment program found" })
                    : Ok(program);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateRecruitmentProgramDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var id = await _service.AddAsync(dto);
                return Ok(new { Id = id, Message = "Recruitment program added successfully" });
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRecruitmentProgramDTO dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var result = await _service.UpdateAsync(dto);
                return result
                    ? Ok(new { Message = "Recruitment program updated successfully" })
                    : NotFound();
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
                    ? Ok(new { Message = "Recruitment program deleted successfully" })
                    : NotFound();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

    }
}
