using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/corporate/human-resource/attendance/settings")]
    [Route("api/v1/corporate/human-resource/attendance-management/settings")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Setting Administration",
        AreaName = "Corporate",
        ControllerName = "AttendanceSetting",
        Description = "Manajemen parameter toleransi presensi, grace period, dan batasan operasional presensi rumah sakit",
        SortOrder = 25)]
    [Tags("HR Attendance - Settings")]
    public class AttendanceSettingController : ControllerBase
    {
        private readonly AttendanceSettingService _service;

        public AttendanceSettingController(AttendanceSettingService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet]
        [AccessAction("Read", "Read Attendance Setting", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceSetting", "Read")]
        public async Task<IActionResult> GetSetting(
            [FromQuery] string hospitalCode = "RSMMC",
            CancellationToken cancellationToken = default)
        {
            return ToActionResult(await _service.GetSettingAsync(hospitalCode, cancellationToken));
        }

        [HttpPut]
        [AccessAction("Update", "Update Attendance Setting", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AttendanceSetting", "Update")]
        public async Task<IActionResult> UpdateSetting(
            [FromBody] UpdateAttendanceSettingRequest request,
            [FromQuery] string hospitalCode = "RSMMC",
            CancellationToken cancellationToken = default)
        {
            if (!TryGetUserId(out var userId)) return UnauthorizedResponse();
            return ToActionResult(await _service.UpdateSettingAsync(hospitalCode, request, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(AttendanceSettingServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }
    }
}
