using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/attendance-dispensations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Attendance Dispensation",
        AreaName = "SelfServices",
        ControllerName = "MyAttendanceDispensation",
        Description = "Pegawai mengajukan dan membatalkan dispensasi terlambat/pulang cepat miliknya",
        SortOrder = 11)]
    [Tags("Self Services / Human Resource / Attendance Dispensation")]
    public class AttendanceDispensationSelfServiceController : ControllerBase
    {
        private const string LogCategory = "SelfServices.HumanResource.AttendanceDispensation";
        private readonly AttendanceDispensationService _service;
        private readonly LoggerService _loggerService;

        public AttendanceDispensationSelfServiceController(AttendanceDispensationService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DispensationResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance Dispensation", Description = "Melihat dispensasi milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceDispensation", "Read")]
        public async Task<IActionResult> GetMine([FromQuery] MyDispensationQueryRequest request, CancellationToken cancellationToken = default) =>
            ToResult(await _service.ListMineAsync(CurrentUserId(), request, DateTime.UtcNow, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DispensationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create My Attendance Dispensation", Description = "Mengajukan dispensasi untuk diri sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyAttendanceDispensation", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateMyDispensationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateMineAsync(CurrentUserId(), request, DateTime.UtcNow, cancellationToken);
            if (result.Success) await AuditAsync("MyAttendanceDispensation.Create", result.Data!, "diajukan");
            return ToResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<DispensationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel My Attendance Dispensation", Description = "Membatalkan dispensasi sendiri yang masih diajukan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyAttendanceDispensation", "Cancel")]
        public async Task<IActionResult> Cancel(Guid id, [FromBody] DispensationDecisionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CancelMineAsync(CurrentUserId(), id, request, cancellationToken);
            if (result.Success) await AuditAsync("MyAttendanceDispensation.Cancel", result.Data!, "dibatalkan");
            return ToResult(result);
        }

        // Matriks 30.5: ID dispensasi, ID pegawai, tanggal, status baru; pelaku dari sesi. Alasan TIDAK dicatat.
        private Task AuditAsync(string action, DispensationResponse d, string verb) =>
            _loggerService.AuditAsync(LogCategory, action,
                $"Dispensasi {d.Id} pegawai {d.WorkforceProfileId} tanggal {d.AttendanceDate:yyyy-MM-dd} {verb}; status {d.Status}; penyetuju {d.ApproverWorkforceProfileId}.");

        private IActionResult ToResult<T>(AttendanceDispensationServiceResult<T> result) =>
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
