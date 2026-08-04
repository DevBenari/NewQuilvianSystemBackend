using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/scheduler-jobs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Scheduler Job",
        AreaName = "Corporate",
        ControllerName = "AttendanceSchedulerJob",
        Description = "Corporate human resource attendance processing scheduler",
        SortOrder = 9)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Scheduler")]
    public class AttendanceSchedulerController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";
        private readonly AttendanceSchedulerService _service;
        private readonly LoggerService _loggerService;

        public AttendanceSchedulerController(
            AttendanceSchedulerService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Attendance Scheduler", Description = "Melihat metadata attendance scheduler", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduler", "Read")]
        public IActionResult GetMetadata() =>
            Ok(ApiResponse<AttendanceSchedulerMetadataResponse>.Ok(_service.GetMetadata(), "Metadata attendance scheduler berhasil diambil."));

        [HttpGet("summary")]
        [AccessAction("Read", "Read Attendance Scheduler", Description = "Melihat ringkasan attendance scheduler", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduler", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AttendanceSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceSchedulerSummaryResponse>.Ok(data, "Ringkasan attendance scheduler berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Attendance Scheduler", Description = "Melihat daftar attendance scheduler job", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduler", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] AttendanceSchedulerJobQueryRequest request,
            CancellationToken cancellationToken = default)
        {
            var data = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceSchedulerJobPagedResponse>.Ok(data, "Data attendance scheduler job berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Attendance Scheduler", Description = "Melihat detail attendance scheduler job", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceScheduler", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken = default)
        {
            var data = await _service.GetDetailAsync(id, cancellationToken);
            return data == null
                ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Attendance scheduler job tidak ditemukan."))
                : Ok(ApiResponse<AttendanceSchedulerJobDetailResponse>.Ok(data, "Detail attendance scheduler job berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Attendance Scheduler", Description = "Menambahkan attendance processing scheduler job", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceScheduler", "Create")]
        public async Task<IActionResult> Enqueue(
            [FromBody] EnqueueAttendanceProcessingJobRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            var result = await _service.EnqueueAsync(request, actor, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(LogCategory, "AttendanceScheduler.Enqueue", "Menambahkan attendance scheduler job.", result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/retry")]
        [AccessAction("Retry", "Retry Attendance Scheduler", Description = "Menjadwalkan ulang attendance scheduler job", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceScheduler", "Retry")]
        public async Task<IActionResult> Retry(Guid id, CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _service.RetryAsync(id, actor, cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel Attendance Scheduler", Description = "Membatalkan attendance scheduler job", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AttendanceScheduler", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelAttendanceSchedulerJobRequest request,
            CancellationToken cancellationToken = default)
        {
            var actor = GetCurrentUserId();
            if (actor == Guid.Empty) return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));
            return ToActionResult(await _service.CancelAsync(id, request, actor, cancellationToken));
        }

        private IActionResult ToActionResult<T>(AttendancePeriodSchedulerServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
