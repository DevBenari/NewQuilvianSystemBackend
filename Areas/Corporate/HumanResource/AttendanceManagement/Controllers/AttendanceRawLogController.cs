using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseAttendanceRawLogPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs.AttendanceRawLogResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/raw-logs")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Raw Log",
        AreaName = "Corporate",
        ControllerName = "AttendanceRawLog",
        Description = "Corporate human resource attendance raw log ingestion and monitoring",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Raw Log")]
    public class AttendanceRawLogController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendanceRawLogService _service;
        private readonly LoggerService _loggerService;

        public AttendanceRawLogController(
            AttendanceRawLogService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRawLogFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Raw Log", Description = "Melihat metadata filter attendance raw log", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceRawLog", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = await _service.GetFilterMetadataAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceRawLog.GetFilterMetadata",
                "Mengambil metadata filter attendance raw log.",
                new
                {
                    DeviceOptionCount = result.AttendanceDeviceOptions.Count,
                    LocationOptionCount = result.AttendanceLocationOptions.Count
                });

            return Ok(ApiResponse<AttendanceRawLogFilterMetadataResponse>.Ok(
                result,
                "Metadata filter attendance raw log berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRawLogSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Raw Log", Description = "Melihat ringkasan attendance raw log", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceRawLog", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var result = await _service.GetSummaryAsync();
            return Ok(ApiResponse<AttendanceRawLogSummaryResponse>.Ok(
                result,
                "Ringkasan attendance raw log berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseAttendanceRawLogPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Raw Log", Description = "Melihat daftar attendance raw log", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceRawLog", "Read")]
        public async Task<IActionResult> GetRawLogs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? attendanceDeviceId,
            [FromQuery] Guid? attendanceLocationId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? eventType,
            [FromQuery] string? sourceType,
            [FromQuery] string? processingStatus,
            [FromQuery] bool? isMatched,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "eventAt",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _service.GetPagedAsync(new AttendanceRawLogQueryRequest
            {
                StartDate = startDate,
                EndDate = endDate,
                CustomPeriod = customPeriod,
                AttendanceDeviceId = attendanceDeviceId,
                AttendanceLocationId = attendanceLocationId,
                HospitalSiteId = hospitalSiteId,
                WorkforceProfileId = workforceProfileId,
                EventType = eventType,
                SourceType = sourceType,
                ProcessingStatus = processingStatus,
                IsMatched = isMatched,
                Search = search,
                SortBy = sortBy ?? "eventAt",
                SortDirection = sortDirection ?? "desc",
                PageNumber = pageNumber,
                PageSize = pageSize
            });

            return Ok(ApiResponse<ResponseAttendanceRawLogPagedResult>.Ok(
                result,
                "Data attendance raw log berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRawLogDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Raw Log", Description = "Melihat detail attendance raw log", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceRawLog", "Read")]
        public async Task<IActionResult> GetRawLogById(Guid id)
        {
            var result = await _service.GetDetailAsync(id);
            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Attendance raw log tidak ditemukan."));
            }

            return Ok(ApiResponse<AttendanceRawLogDetailResponse>.Ok(
                result,
                "Detail attendance raw log berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRawLogCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Attendance Raw Log", Description = "Menerima satu event attendance raw log", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceRawLog", "Create")]
        public async Task<IActionResult> CreateRawLog(
            [FromBody] CreateAttendanceRawLogRequest request)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.CreateAsync(
                request,
                actorUserId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString());

            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceRawLog.CreateRawLog",
                result.Message,
                new
                {
                    result.Data.Id,
                    result.Data.IsDuplicate,
                    result.Data.WorkforceProfileId,
                    result.Data.AttendanceDeviceId,
                    result.Data.EventAt,
                    result.Data.EventType,
                    result.Data.SourceType,
                    result.Data.ProcessingStatus
                });

            return StatusCode(
                result.StatusCode,
                ApiResponse<AttendanceRawLogCreateResponse>.Ok(result.Data, result.Message));
        }

        [HttpPost("batch")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRawLogBatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Attendance Raw Log Batch", Description = "Menerima batch event attendance raw log", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceRawLog", "Create")]
        public async Task<IActionResult> CreateRawLogBatch(
            [FromBody] CreateAttendanceRawLogBatchRequest request)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.CreateBatchAsync(
                request,
                actorUserId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString());

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceRawLog.CreateRawLogBatch",
                "Menerima batch attendance raw log.",
                new
                {
                    result.TotalItem,
                    result.SuccessCount,
                    result.DuplicateCount,
                    result.FailedCount
                });

            return Ok(ApiResponse<AttendanceRawLogBatchResponse>.Ok(
                result,
                result.FailedCount == 0
                    ? "Seluruh attendance raw log pada batch berhasil diterima."
                    : "Batch attendance raw log selesai diproses dengan sebagian item gagal."));
        }

        [HttpPost("{id:guid}/retry")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceRawLogRetryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Retry Attendance Raw Log", Description = "Mencoba ulang pencocokan identitas attendance raw log", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceRawLog", "Update")]
        public async Task<IActionResult> RetryRawLog(Guid id)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.RetryAsync(id, actorUserId);
            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceRawLog.RetryRawLog",
                result.Message,
                result.Data);

            return StatusCode(
                result.StatusCode,
                ApiResponse<AttendanceRawLogRetryResponse>.Ok(result.Data, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
