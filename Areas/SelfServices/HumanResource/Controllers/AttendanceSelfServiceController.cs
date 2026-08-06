using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/attendance")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Attendance",
        AreaName = "SelfServices",
        ControllerName = "MyAttendance",
        Description = "Employee self-service attendance summary, history, and detail",
        SortOrder = 2)]
    [Tags("Self Services / Human Resource / Attendance")]
    public class AttendanceSelfServiceController : ControllerBase
    {
        private readonly AttendanceDailyQueryService _service;

        public AttendanceSelfServiceController(AttendanceDailyQueryService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance", Description = "Melihat metadata filter riwayat attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendance", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<AttendanceDailyFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata attendance berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceSelfServiceSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance", Description = "Melihat ringkasan attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendance", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? customPeriod,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return UnauthorizedResult();

            var result = await _service.GetMySummaryAsync(
                userId,
                startDate,
                endDate,
                customPeriod,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance", Description = "Melihat riwayat attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendance", "Read")]
        public async Task<IActionResult> GetHistory(
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
            if (userId == Guid.Empty) return UnauthorizedResult();

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

            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceDailyDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance", Description = "Melihat detail attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendance", "Read")]
        public async Task<IActionResult> GetDetail(Guid id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.GetMyDetailAsync(id, userId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(AttendanceDailyQueryServiceResult<T> result)
        {
            return result.Success && result.Data != null
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private IActionResult UnauthorizedResult()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
