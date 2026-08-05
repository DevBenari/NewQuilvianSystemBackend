using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/calendar")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Calendar",
        AreaName = "Corporate",
        ControllerName = "LeaveCalendar",
        Description = "Approved leave calendar for HR and manager team",
        SortOrder = 8)]
    [Tags("Corporate / Human Resource / Leave Management / Leave Calendar")]
    public class LeaveCalendarController : ControllerBase
    {
        private readonly LeaveCalendarService _service;

        public LeaveCalendarController(LeaveCalendarService service)
        {
            _service = service;
        }

        [HttpGet]
        [AccessAction(
            "Read",
            "Read Leave Calendar",
            Description = "Melihat leave calendar seluruh employee sesuai filter",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveCalendar", "Read")]
        public async Task<IActionResult> GetAdminCalendar(
            [FromQuery] LeaveCalendarQueryRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetAdminCalendarAsync(request, cancellationToken));
        }

        [HttpGet("team-calendar")]
        [AccessAction(
            "Read",
            "Read Team Leave Calendar",
            Description = "Melihat kalender cuti direct report manager login",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("LeaveTeamCalendar", "Read")]
        public async Task<IActionResult> GetTeamCalendar(
            [FromQuery] LeaveCalendarQueryRequest request,
            CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetTeamCalendarAsync(
                GetCurrentUserId(),
                request,
                cancellationToken));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private IActionResult ToActionResult<T>(LeaveRequestServiceResult<T> result)
        {
            var response = result.Success
                ? ApiResponse<T>.Ok(result.Data, result.Message)
                : ApiResponse<T>.Fail(result.StatusCode, result.Message);
            return StatusCode(result.StatusCode, response);
        }
    }
}
