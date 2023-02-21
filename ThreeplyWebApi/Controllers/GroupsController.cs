using Newtonsoft.Json;
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
        public async Task<List<Group>> Get() => await _groupsService.GetAsync();

        [HttpGet("{groupName}")]
        public async Task<ActionResult<Group>> Get(string groupName)
        {
            var group = await _groupsService.GetAsync(groupName);
            if (group == null)
            {
                return NotFound();
            }
            return Ok(group);
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Group newGroup) 
        {
            await _groupsService.CreateAsync(newGroup);
            return CreatedAtAction(nameof(Get), new { groupName = newGroup.GroupName }, newGroup);

        }
        [HttpPut]
        public async Task<IActionResult> Update(string groupName, Group newGroup)
        {
            var group = await _groupsService.GetAsync(groupName);
            if (group == null)
            {
                return NotFound();
            }
            newGroup.Id = group.Id;
            await _groupsService.UpdateAsync(groupName, newGroup);
            return NoContent();
        }
    }
}
