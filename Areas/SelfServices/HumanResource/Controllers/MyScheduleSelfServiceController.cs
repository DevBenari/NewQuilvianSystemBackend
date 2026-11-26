using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/my-schedule")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Schedule",
        AreaName = "SelfServices",
        ControllerName = "MySchedule",
        Description = "Employee self-service published schedule and unit roster",
        SortOrder = 10)]
    [Tags("Self Services / Human Resource / My Schedule")]
    public class MyScheduleSelfServiceController : ControllerBase
    {
        private readonly MyScheduleSelfService _service;

        public MyScheduleSelfServiceController(MyScheduleSelfService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Read My Schedule", Description = "Melihat jadwal terbit milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MySchedule", "Read")]
        public async Task<IActionResult> GetMySchedule([FromQuery] MyScheduleQueryRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetMyScheduleAsync(GetCurrentUserId(), request, DateTime.UtcNow, cancellationToken);
            return result.Success
                ? Ok(ApiResponse<List<MyScheduleDayResponse>>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        [HttpGet("unit-roster")]
        [AccessAction("Read", "Read My Schedule", Description = "Melihat roster terbit unit sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MySchedule", "Read")]
        public async Task<IActionResult> GetMyUnitRoster([FromQuery] MyUnitRosterQueryRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetMyUnitRosterAsync(GetCurrentUserId(), request, DateTime.UtcNow, cancellationToken);
            return result.Success
                ? Ok(ApiResponse<MyUnitRosterResponse>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
