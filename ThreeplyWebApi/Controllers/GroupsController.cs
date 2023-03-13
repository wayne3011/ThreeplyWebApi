using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using ThreeplyWebApi.Services;
using ThreeplyWebApi.Controllers.Extensions;
using ThreeplyWebApi.Models;
using System.Text.Json;
using ThreeplyWebApi.Services.ScheduleParser.ScheduleParserExceptions;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using ThreeplyWebApi.Controllers.AuthenticationScheme;
using ThreeplyWebApi.Models.GroupModel;
using ZstdSharp.Unsafe;

namespace ThreeplyWebApi.Controllers
{
    [ApiController]
    [Route("Groups")]
    [Authorize(AuthenticationSchemes = GeneralUserAuthenticationSchemeOptions.Name)]
    public class GroupsController : ControllerBase
    {
        readonly private GroupsService _groupsService;
        readonly private ILogger<GroupsController> _logger;
        public GroupsController(GroupsService schedulesService, ILogger<GroupsController> logger)
        {
            _groupsService = schedulesService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<List<Group>> Get() {
            var group = await _groupsService.GetAsync();
            _logger.LogInformation("GET GroupController UserId:{UserId}", HttpContext.User.Identity.Name);
            return group;
        } 

        [HttpGet("{groupName}")]
        public async Task<ActionResult<Group>> Get(string groupName)
        {
            try
            {
                var group = await _groupsService.GetAsync(groupName);

                _logger.LogInformation("GET/{GroupName} GroupController UserId:{UserId}", groupName, HttpContext.User.Identity.Name);

                return Ok(group);
            }
            catch (ScheduleParserException ex)
            {
                _logger.LogError("ScheduleParserException GET/{GroupName} GroupsController UserId:{UserId}, {exception}", groupName, HttpContext.User.Identity.Name,ex);
                return ScheduleParserProblemResult(ex);            
            }
            catch (TimeoutException)
            {
                _logger.LogError("Group Database Timeout GET/{GroupName}, GroupsController UserId:{UserId}", groupName, HttpContext.User.Identity.Name);
                return new StatusCodeResult(500);
            }
            catch (Exception)
            {
                return new StatusCodeResult(500);
            }
        }
        [HttpGet()]
        [Route("GetGroupValidity/{groupName}")]
        public async Task<GroupValidation> GetGroupValidity(string groupName)
        {
            GroupValidation groupValidation = await _groupsService.GetGroupValidationAsync(groupName);
            _logger.LogInformation("GetGroupValidity/{GroupName} GroupController UserId:{UserId} Result:{isValid:}",groupName,HttpContext.User.Identity.Name, groupValidation.isValid);
            return groupValidation;
        }
        static private JsonResult ScheduleParserProblemResult(ScheduleParserException ex)
        {
            ProblemDetails problemDetails = ex.toProblemDetails();
            JsonResult responce = new(problemDetails)
            {
                StatusCode = problemDetails.Status,
                ContentType = "application/json+problem; charset=utf-8"
            };
            return responce;
        }
    }
}
