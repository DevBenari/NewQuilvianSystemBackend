using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/device-incidents")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Device Incident",
        AreaName = "Corporate",
        ControllerName = "AttendanceDeviceIncident",
        Description = "Kejadian gangguan mesin absen sebagai rujukan koreksi presensi; tidak membuat scan",
        SortOrder = 16)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Device Incident")]
    public class AttendanceDeviceIncidentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";
        private readonly AttendanceDeviceIncidentService _service;
        private readonly LoggerService _loggerService;

        public AttendanceDeviceIncidentController(AttendanceDeviceIncidentService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DeviceIncidentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Device Incident", Description = "Melihat riwayat gangguan mesin absen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDeviceIncident", "Read")]
        public async Task<IActionResult> GetList([FromQuery] DeviceIncidentQueryRequest request, CancellationToken cancellationToken = default) =>
            ToResult(await _service.ListAsync(request, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DeviceIncidentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Attendance Device Incident", Description = "Mencatat kejadian gangguan mesin absen", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceDeviceIncident", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateDeviceIncidentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateAsync(request, CurrentUserId(), cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDeviceIncident.Create", result.Data!, "dicatat");
            return ToResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<DeviceIncidentResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Attendance Device Incident", Description = "Membatalkan kejadian gangguan yang salah catat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceDeviceIncident", "Cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelDeviceIncidentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CancelAsync(id, request, CurrentUserId(), cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDeviceIncident.Cancel", result.Data!, "dibatalkan");
            return ToResult(result);
        }

        // Matriks 30.5: ID kejadian, ID mesin, rentang jam; pelaku dari sesi.
        private Task AuditAsync(string action, DeviceIncidentResponse d, string verb) =>
            _loggerService.AuditAsync(LogCategory, action,
                $"Kejadian gangguan {d.Id} mesin {d.AttendanceDeviceId} ({d.AttendanceDeviceCode}) {d.StartAt:yyyy-MM-ddTHH:mm}Z s.d. {d.EndAt:yyyy-MM-ddTHH:mm}Z {verb}; status {d.Status}.");

        private IActionResult ToResult<T>(DeviceIncidentServiceResult<T> result) =>
            result.Success
                ? Ok(ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
