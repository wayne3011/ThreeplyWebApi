using Microsoft.AspNetCore.Mvc;
using ThreeplyWebApi.Services;
using ThreeplyWebApi.Models;

namespace ThreeplyWebApi.Controllers
{
    [ApiController]
    [Route("controller")]
    public class SchedulesController : ControllerBase
    {
        readonly private SchedulesService _schedulesService;
        public SchedulesController(SchedulesService schedulesService)
        {
            _schedulesService = schedulesService;
        }
        [HttpGet]
        public async Task<List<Schedule>> Get() => await _schedulesService.GetAsync();
        [HttpGet("{groupName}")]
        public async Task<ActionResult<Schedule>> Get(string groupName)
        {
            var schedule = await _schedulesService.GetAsync(groupName);
            if (schedule == null)
            {
                return NotFound();
            }
            return Ok(schedule);
        }
        [HttpPost]
        public async Task<IActionResult> Post(Schedule newSchedule)
        {
            await _schedulesService.CreateAsync(newSchedule);
            return CreatedAtAction(nameof(Get), new { groupName = newSchedule.groupName }, newSchedule);
        }
    }
}
