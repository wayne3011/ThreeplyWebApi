using Microsoft.AspNetCore.Mvc;
using ThreeplyWebApi.Services;
using ThreeplyWebApi.Models;
using System.Text.Json;

namespace ThreeplyWebApi.Controllers
{
    [ApiController]
    [Route("controller")]
    public class GroupsController : ControllerBase
    {
        readonly private GroupsService _groupsService;
        public GroupsController(GroupsService schedulesService)
        {
            _groupsService = schedulesService;
        }
        [HttpGet]
        public async Task<string> Get()
        {
           var req = await _groupsService.GetAsync();
           
           return JsonSerializer.Serialize(req,new JsonSerializerOptions { WriteIndented = true});
        }
        [HttpGet("{groupName}")]
        public async Task<ActionResult<Schedule>> Get(string groupName)
        {
            var schedule = await _groupsService.GetAsync(groupName);
            if (schedule == null)
            {
                return NotFound();
            }
            return Ok(schedule);
        }
        [HttpPost]
        public async Task<IActionResult> Post(Group newGroup)
        {
            await _groupsService.CreateAsync(newGroup);
            return CreatedAtAction(nameof(Get), new { groupName = newGroup.groupName }, newGroup);
        }
    }
}
