using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/dashboard")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Dashboard",
        AreaName = "Corporate",
        ControllerName = "AttendanceDashboard",
        Description = "Dashboard kehadiran lima kartu sesuai cakupan unit pengguna",
        SortOrder = 19)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Dashboard")]
    public class AttendanceDashboardController : ControllerBase
    {
        private readonly AttendanceDashboardService _service;

        public AttendanceDashboardController(AttendanceDashboardService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDashboardResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Read", "Read Attendance Dashboard", Description = "Melihat dashboard kehadiran unit dalam cakupan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDashboard", "Read")]
        public async Task<IActionResult> GetDashboard([FromQuery] AttendanceDashboardQueryRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDashboardAsync(request, DateTime.UtcNow, cancellationToken);
            return result.Success
                ? Ok(ApiResponse<AttendanceDashboardResponse>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        [HttpGet("cards/{cardCode}/details")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDashboardDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Dashboard", Description = "Melihat dashboard kehadiran unit dalam cakupan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDashboard", "Read")]
        public async Task<IActionResult> GetCardDetail(string cardCode, [FromQuery] AttendanceDashboardDetailQueryRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.GetCardDetailAsync(cardCode, request, DateTime.UtcNow, cancellationToken);
            return result.Success
                ? Ok(ApiResponse<AttendanceDashboardDetailResponse>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
