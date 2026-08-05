using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/leave/calendar")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Leave Calendar",
        AreaName = "SelfServices",
        ControllerName = "MyLeaveCalendar",
        Description = "Employee self-service approved leave calendar",
        SortOrder = 6)]
    [Tags("Self Services / Human Resource / Leave Calendar")]
    public class LeaveCalendarSelfServiceController : ControllerBase
    {
        private readonly LeaveCalendarService _service;

        public LeaveCalendarSelfServiceController(LeaveCalendarService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction("Read", "Read My Leave Calendar", Description = "Melihat kalender cuti milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyLeaveCalendar", "Read")]
        public async Task<IActionResult> GetCalendar(
            [FromQuery] LeaveCalendarQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetMyCalendarAsync(
                GetCurrentUserId(),
                request,
                cancellationToken);

            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<object>.Ok(result.Data!, result.Message)
                    : ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
