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
    [Route("api/v1/corporate/human-resource/attendance/processing")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Processing",
        AreaName = "Corporate",
        ControllerName = "AttendanceProcessing",
        Description = "Corporate human resource attendance processing service",
        SortOrder = 3)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Processing")]
    public class AttendanceProcessingController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendanceProcessingService _service;
        private readonly LoggerService _loggerService;

        public AttendanceProcessingController(
            AttendanceProcessingService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Processing", Description = "Melihat metadata attendance processing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceProcessing", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<AttendanceProcessingMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata attendance processing berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Processing", Description = "Melihat ringkasan attendance processing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceProcessing", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);
            return Ok(ApiResponse<AttendanceProcessingSummaryResponse>.Ok(
                result,
                "Ringkasan attendance processing berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingRunPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Processing", Description = "Melihat daftar attendance processing run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceProcessing", "Read")]
        public async Task<IActionResult> GetRuns(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? processingMode,
            [FromQuery] string? runStatus,
            [FromQuery] string? triggerSource,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "startedAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetRunsAsync(
                new AttendanceProcessingRunQueryRequest
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ProcessingMode = processingMode,
                    RunStatus = runStatus,
                    TriggerSource = triggerSource,
                    WorkforceProfileId = workforceProfileId,
                    HospitalSiteId = hospitalSiteId,
                    OrganizationUnitId = organizationUnitId,
                    DepartmentId = departmentId,
                    Search = search,
                    SortBy = sortBy ?? "startedAt",
                    SortDirection = sortDirection ?? "desc",
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                cancellationToken);

            return Ok(ApiResponse<AttendanceProcessingRunPagedResponse>.Ok(
                result,
                "Data attendance processing run berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingRunDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Processing", Description = "Melihat detail attendance processing run", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceProcessing", "Read")]
        public async Task<IActionResult> GetRunById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetRunDetailAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance processing run tidak ditemukan."));
            }

            return Ok(ApiResponse<AttendanceProcessingRunDetailResponse>.Ok(
                result,
                "Detail attendance processing run berhasil diambil."));
        }

        [HttpPost("process")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Process Attendance", Description = "Memproses attendance satu workforce dan tanggal", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceProcessing", "Create")]
        public async Task<IActionResult> ProcessSingle(
            [FromBody] ProcessAttendanceSingleRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.ProcessSingleAsync(
                request,
                actorUserId,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceProcessing.ProcessSingle",
                result.Message,
                new
                {
                    result.Data.ProcessingRunId,
                    result.Data.RunNumber,
                    result.Data.RunStatus,
                    result.Data.TargetCount,
                    result.Data.SuccessCount,
                    result.Data.FailedCount,
                    result.Data.SkippedCount
                });

            return StatusCode(
                result.StatusCode,
                ApiResponse<AttendanceProcessingExecutionResponse>.Ok(result.Data, result.Message));
        }

        [HttpPost("process-range")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Process Attendance Range", Description = "Memproses attendance dalam rentang tanggal dan filter workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceProcessing", "Create")]
        public async Task<IActionResult> ProcessRange(
            [FromBody] ProcessAttendanceRangeRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.ProcessRangeAsync(
                request,
                actorUserId,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceProcessing.ProcessRange",
                result.Message,
                new
                {
                    result.Data.ProcessingRunId,
                    result.Data.RunNumber,
                    result.Data.RunStatus,
                    result.Data.StartDate,
                    result.Data.EndDate,
                    result.Data.TargetCount,
                    result.Data.SuccessCount,
                    result.Data.FailedCount,
                    result.Data.SkippedCount
                });

            return StatusCode(
                result.StatusCode,
                ApiResponse<AttendanceProcessingExecutionResponse>.Ok(result.Data, result.Message));
        }

        [HttpPost("attendance-dailies/{attendanceDailyId:guid}/reprocess")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceProcessingExecutionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Reprocess Attendance", Description = "Menghitung ulang attendance daily", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceProcessing", "Update")]
        public async Task<IActionResult> ReprocessDaily(
            Guid attendanceDailyId,
            [FromBody] ReprocessAttendanceDailyRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.ReprocessDailyAsync(
                attendanceDailyId,
                request,
                actorUserId,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceProcessing.ReprocessDaily",
                result.Message,
                new
                {
                    attendanceDailyId,
                    result.Data.ProcessingRunId,
                    result.Data.RunNumber,
                    result.Data.RunStatus
                });

            return StatusCode(
                result.StatusCode,
                ApiResponse<AttendanceProcessingExecutionResponse>.Ok(result.Data, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
