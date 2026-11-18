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
    [Route("api/v1/corporate/human-resource/attendance/dispensations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Dispensation",
        AreaName = "Corporate",
        ControllerName = "AttendanceDispensation",
        Description = "Dispensasi terlambat/pulang cepat: catat atas nama pegawai unit dan keputusan atasan langsung",
        SortOrder = 18)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Dispensation")]
    public class AttendanceDispensationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";
        private readonly AttendanceDispensationService _service;
        private readonly LoggerService _loggerService;

        public AttendanceDispensationController(AttendanceDispensationService service, LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DispensationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Dispensation", Description = "Melihat dispensasi unit dalam cakupan dan yang menunggu keputusan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDispensation", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default) =>
            Ok(ApiResponse<DispensationFilterMetadataResponse>.Ok(await _service.GetMetadataAsync(cancellationToken), "Metadata filter dispensasi berhasil diambil."));

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DispensationResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Dispensation", Description = "Melihat dispensasi unit dalam cakupan dan yang menunggu keputusan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDispensation", "Read")]
        public async Task<IActionResult> GetList([FromQuery] DispensationQueryRequest request, CancellationToken cancellationToken = default) =>
            ToResult(await _service.ListAsync(CurrentUserId(), request, DateTime.UtcNow, cancellationToken));

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DispensationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Dispensation", Description = "Melihat dispensasi unit dalam cakupan dan yang menunggu keputusan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDispensation", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default) =>
            ToResult(await _service.GetAsync(CurrentUserId(), id, cancellationToken));

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DispensationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Attendance Dispensation", Description = "Mencatat dispensasi atas nama pegawai unit sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceDispensation", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateDispensationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateOnBehalfAsync(CurrentUserId(), request, DateTime.UtcNow, cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDispensation.Create", result.Data!, "dicatat atas nama pegawai");
            return ToResult(result);
        }

        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<DispensationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Approve", "Approve Attendance Dispensation", Description = "Menyetujui atau menolak dispensasi sebagai atasan langsung yang ditetapkan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceDispensation", "Approve")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] DispensationDecisionRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.ApproveAsync(CurrentUserId(), id, request, cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDispensation.Approve", result.Data!, "disetujui");
            return ToResult(result);
        }

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<DispensationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Approve", "Approve Attendance Dispensation", Description = "Menyetujui atau menolak dispensasi sebagai atasan langsung yang ditetapkan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceDispensation", "Approve")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectDispensationRequest request, CancellationToken cancellationToken = default)
        {
            var result = await _service.RejectAsync(CurrentUserId(), id, request, cancellationToken);
            if (result.Success) await AuditAsync("AttendanceDispensation.Reject", result.Data!, "ditolak");
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
