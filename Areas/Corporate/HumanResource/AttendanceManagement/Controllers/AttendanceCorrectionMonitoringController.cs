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
    [Route("api/v1/corporate/human-resource/attendance/correction-monitoring")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Correction Monitoring",
        AreaName = "Corporate",
        ControllerName = "AttendanceCorrectionMonitoring",
        Description = "Corporate human resource attendance correction admin review and monitoring",
        SortOrder = 6)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Correction Monitoring")]
    public class AttendanceCorrectionMonitoringController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendanceCorrectionMonitoringService _service;
        private readonly LoggerService _loggerService;

        public AttendanceCorrectionMonitoringController(
            AttendanceCorrectionMonitoringService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction Monitoring", Description = "Melihat metadata filter monitoring attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<AttendanceCorrectionMonitoringFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata monitoring attendance correction berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction Monitoring", Description = "Melihat ringkasan monitoring attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionMonitoringSummaryResponse>.Ok(
                result,
                "Ringkasan monitoring attendance correction berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction Monitoring", Description = "Melihat daftar monitoring attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Read")]
        public async Task<IActionResult> GetMonitoringItems(
            [FromQuery] AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionMonitoringPagedResponse>.Ok(
                result,
                "Data monitoring attendance correction berhasil diambil."));
        }

        [HttpGet("attention")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction Attention Queue", Description = "Melihat attendance correction yang memerlukan perhatian admin", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Read")]
        public async Task<IActionResult> GetAttentionQueue(
            [FromQuery] AttendanceCorrectionMonitoringQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAttentionAsync(request, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionMonitoringPagedResponse>.Ok(
                result,
                "Queue attendance correction yang memerlukan perhatian berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Correction Monitoring", Description = "Melihat detail monitoring attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Read")]
        public async Task<IActionResult> GetDetail(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetDetailAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/workflow-health")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringWorkflowHealthResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Correction Workflow Health", Description = "Melihat kesehatan workflow attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Read")]
        public async Task<IActionResult> GetWorkflowHealth(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetWorkflowHealthAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/synchronize")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionSynchronizationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Synchronize", "Synchronize Attendance Correction Monitoring", Description = "Menyinkronkan attendance correction dengan workflow", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Synchronize")]
        public async Task<IActionResult> Synchronize(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.SynchronizeAsync(id, actorUserId, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "AttendanceCorrectionMonitoring.Synchronize",
                    "Menyinkronkan attendance correction dengan workflow.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/retry-apply")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionApplyResponse>), StatusCodes.Status200OK)]
        [AccessAction("Apply", "Retry Apply Attendance Correction", Description = "Mencoba kembali penerapan attendance correction yang telah disetujui", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Apply")]
        public async Task<IActionResult> RetryApply(
            Guid id,
            [FromBody] AttendanceCorrectionApplyRequest? request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.RetryApplyAsync(
                id,
                request?.Note,
                actorUserId,
                cancellationToken);

            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "AttendanceCorrectionMonitoring.RetryApply",
                    "Mencoba kembali penerapan attendance correction.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPost("bulk/synchronize")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringBatchResponse>), StatusCodes.Status200OK)]
        [AccessAction("Synchronize", "Bulk Synchronize Attendance Correction", Description = "Menyinkronkan beberapa attendance correction dengan workflow", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Synchronize")]
        public async Task<IActionResult> BulkSynchronize(
            [FromBody] AttendanceCorrectionMonitoringBatchRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.BulkSynchronizeAsync(
                request,
                actorUserId,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceCorrectionMonitoring.BulkSynchronize",
                "Menyinkronkan beberapa attendance correction dengan workflow.",
                new { result.TotalItem, result.SuccessCount, result.FailedCount });

            return Ok(ApiResponse<AttendanceCorrectionMonitoringBatchResponse>.Ok(
                result,
                "Bulk synchronize attendance correction selesai diproses."));
        }

        [HttpPost("bulk/retry-apply")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionMonitoringBatchResponse>), StatusCodes.Status200OK)]
        [AccessAction("Apply", "Bulk Retry Apply Attendance Correction", Description = "Mencoba kembali penerapan beberapa attendance correction", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceCorrectionMonitoring", "Apply")]
        public async Task<IActionResult> BulkRetryApply(
            [FromBody] AttendanceCorrectionMonitoringBatchRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            var result = await _service.BulkRetryApplyAsync(
                request,
                actorUserId,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "AttendanceCorrectionMonitoring.BulkRetryApply",
                "Mencoba kembali penerapan beberapa attendance correction.",
                new { result.TotalItem, result.SuccessCount, result.FailedCount });

            return Ok(ApiResponse<AttendanceCorrectionMonitoringBatchResponse>.Ok(
                result,
                "Bulk retry apply attendance correction selesai diproses."));
        }

        private IActionResult ToActionResult<T>(AttendanceCorrectionServiceResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(result.Data!, result.Message));
            }

            return StatusCode(
                result.StatusCode,
                ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
