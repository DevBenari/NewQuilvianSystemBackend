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
        Description = "Employee self-service attendance capture, summary, history, and detail",
        SortOrder = 2,
        VisibleInRoleAccess = false,
        IsSystemOnly = true)]
    [Tags("Self Services / Human Resource / Attendance")]
    public class AttendanceSelfServiceController : ControllerBase
    {
        private readonly AttendanceDailyQueryService _service;
        private readonly AttendanceSelfServiceCaptureService _captureService;

        public AttendanceSelfServiceController(
            AttendanceDailyQueryService service,
            AttendanceSelfServiceCaptureService captureService)
        {
            _service = service;
            _captureService = captureService;
        }

        // ============================================================
        // ATTENDANCE CAPTURE STATUS
        // Core Employee Self Service.
        // Tidak membutuhkan AccessPermission manual.
        // Security berasal dari:
        // - [Authorize]
        // - authenticated user
        // - workforce context
        // - self-data ownership
        // ============================================================

        [HttpGet("capture-status")]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceSelfServiceCaptureStatusResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Read",
            "Read My Attendance Capture Status",
            Description = "Melihat status Absen Masuk / Absen Pulang milik sendiri",
            AccessType = AccessTypes.Read,
            SortOrder = 1,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public async Task<IActionResult> GetCaptureStatus(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return UnauthorizedResult();
            }

            var result = await _captureService.GetStatusAsync(
                userId,
                cancellationToken);

            return ToCaptureActionResult(result);
        }

        // ============================================================
        // CHECK-IN
        // Employee hanya melakukan attendance untuk dirinya sendiri.
        // UserId / WorkforceProfileId tidak diterima dari request body.
        // ============================================================

        [HttpPost("check-in")]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceSelfServiceCaptureResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Create",
            "Employee Check-In",
            Description = "Merekam Absen Masuk employee login dengan validasi GPS/geofence",
            AccessType = AccessTypes.Create,
            SortOrder = 2,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public async Task<IActionResult> CheckIn(
            [FromBody] AttendanceSelfServiceCaptureRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return UnauthorizedResult();
            }

            var result = await _captureService.CheckInAsync(
                request,
                userId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString(),
                cancellationToken);

            return ToCaptureActionResult(result);
        }

        // ============================================================
        // CHECK-OUT
        // Check-out selalu explicit action.
        // Tidak otomatis hanya karena user logout / fingerprint.
        // ============================================================

        [HttpPost("check-out")]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceSelfServiceCaptureResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Create",
            "Employee Check-Out",
            Description = "Merekam Absen Pulang employee login dengan validasi GPS/geofence",
            AccessType = AccessTypes.Create,
            SortOrder = 2,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public async Task<IActionResult> CheckOut(
            [FromBody] AttendanceSelfServiceCaptureRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return UnauthorizedResult();
            }

            var result = await _captureService.CheckOutAsync(
                request,
                userId,
                HttpContext.Connection.RemoteIpAddress?.ToString(),
                Request.Headers["User-Agent"].ToString(),
                cancellationToken);

            return ToCaptureActionResult(result);
        }

        // ============================================================
        // FILTER METADATA
        // ============================================================

        [HttpGet("filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceDailyFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read My Attendance",
            Description = "Melihat metadata filter riwayat attendance milik sendiri",
            AccessType = AccessTypes.Read,
            SortOrder = 1,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public IActionResult GetMetadata()
        {
            return Ok(
                ApiResponse<AttendanceDailyFilterMetadataResponse>.Ok(
                    _service.GetMetadata(),
                    "Metadata attendance berhasil diambil."));
        }

        // ============================================================
        // MY ATTENDANCE SUMMARY
        // ============================================================

        [HttpGet("summary")]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceSelfServiceSummaryResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read My Attendance",
            Description = "Melihat ringkasan attendance milik sendiri",
            AccessType = AccessTypes.Read,
            SortOrder = 1,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? customPeriod,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return UnauthorizedResult();
            }

            var result = await _service.GetMySummaryAsync(
                userId,
                startDate,
                endDate,
                customPeriod,
                cancellationToken);

            return ToActionResult(result);
        }

        // ============================================================
        // MY ATTENDANCE HISTORY
        // ============================================================

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceDailyPagedResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read My Attendance",
            Description = "Melihat riwayat attendance milik sendiri",
            AccessType = AccessTypes.Read,
            SortOrder = 1,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
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

            if (userId == Guid.Empty)
            {
                return UnauthorizedResult();
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

            return ToActionResult(result);
        }

        // ============================================================
        // MY ATTENDANCE DETAIL
        // Service tetap harus memastikan record tersebut milik user login.
        // ============================================================

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<AttendanceDailyDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read My Attendance",
            Description = "Melihat detail attendance milik sendiri",
            AccessType = AccessTypes.Read,
            SortOrder = 1,
            VisibleInRoleAccess = false,
            IsSystemOnly = true)]
        public async Task<IActionResult> GetDetail(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            if (userId == Guid.Empty)
            {
                return UnauthorizedResult();
            }

            return ToActionResult(
                await _service.GetMyDetailAsync(
                    id,
                    userId,
                    cancellationToken));
        }

        // ============================================================
        // RESPONSE HELPERS
        // ============================================================

        private IActionResult ToActionResult<T>(
            AttendanceDailyQueryServiceResult<T> result)
        {
            return result.Success && result.Data != null
                ? StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(
                        result.Data,
                        result.Message))
                : StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(
                        result.StatusCode,
                        result.Message));
        }

        private IActionResult ToCaptureActionResult<T>(
            AttendanceRawLogServiceResult<T> result)
        {
            return result.Success && result.Data != null
                ? StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(
                        result.Data,
                        result.Message))
                : StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(
                        result.StatusCode,
                        result.Message));
        }

        private IActionResult UnauthorizedResult()
        {
            return Unauthorized(
                ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
        }

        private Guid GetCurrentUserId()
        {
            var value =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}