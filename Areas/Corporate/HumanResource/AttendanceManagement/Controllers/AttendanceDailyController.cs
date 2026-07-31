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
    [Route("api/v1/corporate/human-resource/attendance/dailies")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Daily",
        AreaName = "Corporate",
        ControllerName = "AttendanceDaily",
        Description = "Corporate human resource attendance daily query and self-service history",
        SortOrder = 4)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Daily")]
    public class AttendanceDailyController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendanceDailyQueryService _service;
        private readonly LoggerService _loggerService;

        public AttendanceDailyController(
            AttendanceDailyQueryService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat metadata filter attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<AttendanceDailyFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata filter attendance daily berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat ringkasan attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AttendanceDailyQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceDailySummaryResponse>.Ok(
                result,
                "Ringkasan attendance daily berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat daftar attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetAttendanceDailies(
            [FromQuery] AttendanceDailyQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceDailyPagedResponse>.Ok(
                result,
                "Data attendance daily berhasil diambil."));
        }

        [HttpGet("payroll-readiness")]
        [ProducesResponseType(typeof(ApiResponse<AttendancePayrollReadinessPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Payroll Readiness", Description = "Melihat kesiapan attendance untuk payroll", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetPayrollReadiness(
            [FromQuery] AttendanceDailyQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPayrollReadinessAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendancePayrollReadinessPagedResponse>.Ok(
                result,
                "Kesiapan attendance untuk payroll berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat detail attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetAttendanceDailyById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceDaily.GetAttendanceDailyById",
                "Mengambil detail attendance daily.",
                new { result.Id, result.WorkforceProfileId, result.AttendanceDate });

            return Ok(ApiResponse<AttendanceDailyDetailResponse>.Ok(
                result,
                "Detail attendance daily berhasil diambil."));
        }

        [HttpGet("{id:guid}/segments")]
        [ProducesResponseType(typeof(ApiResponse<List<AttendanceDailySegmentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat segment attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetSegments(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSegmentsAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan."));
            }

            return Ok(ApiResponse<List<AttendanceDailySegmentResponse>>.Ok(
                result,
                "Segment attendance daily berhasil diambil."));
        }

        [HttpGet("{id:guid}/exceptions")]
        [ProducesResponseType(typeof(ApiResponse<List<AttendanceDailyExceptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat exception attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetExceptions(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetExceptionsAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan."));
            }

            return Ok(ApiResponse<List<AttendanceDailyExceptionResponse>>.Ok(
                result,
                "Exception attendance daily berhasil diambil."));
        }

        [HttpGet("{id:guid}/raw-logs")]
        [ProducesResponseType(typeof(ApiResponse<List<AttendanceDailyRawLogResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Daily", Description = "Melihat source raw log attendance daily", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDaily", "Read")]
        public async Task<IActionResult> GetRawLogs(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetRawLogsAsync(id, cancellationToken);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance daily tidak ditemukan."));
            }

            return Ok(ApiResponse<List<AttendanceDailyRawLogResponse>>.Ok(
                result,
                "Source raw log attendance daily berhasil diambil."));
        }

        [HttpGet("my-history/summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceSelfServiceSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Attendance History", Description = "Melihat ringkasan attendance user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDailySelfService", "Read")]
        public async Task<IActionResult> GetMySummary(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? customPeriod,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.GetMySummaryAsync(
                userId,
                startDate,
                endDate,
                customPeriod,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<AttendanceSelfServiceSummaryResponse>.Ok(
                result.Data,
                result.Message));
        }

        [HttpGet("my-history")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Attendance History", Description = "Melihat riwayat attendance user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDailySelfService", "Read")]
        public async Task<IActionResult> GetMyHistory(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? attendanceStatus,
            [FromQuery] string? processingStatus,
            [FromQuery] bool? isLate,
            [FromQuery] bool? isEarlyLeave,
            [FromQuery] bool? hasMissingPunch,
            [FromQuery] string? search,
            [FromQuery] string sortBy = "attendanceDate",
            [FromQuery] string sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.GetMyHistoryAsync(
                userId,
                startDate,
                endDate,
                customPeriod,
                attendanceStatus,
                processingStatus,
                isLate,
                isEarlyLeave,
                hasMissingPunch,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<AttendanceDailyPagedResponse>.Ok(
                result.Data,
                result.Message));
        }

        [HttpGet("my-history/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Attendance History", Description = "Melihat detail attendance user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceDailySelfService", "Read")]
        public async Task<IActionResult> GetMyAttendanceDetail(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.GetMyDetailAsync(id, userId, cancellationToken);
            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return Ok(ApiResponse<AttendanceDailyDetailResponse>.Ok(
                result.Data,
                result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
