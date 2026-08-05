using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/attendance/correction-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Correction Administration",
        AreaName = "Corporate",
        ControllerName = "AttendanceCorrection",
        Description = "HR monitoring, workflow synchronization, application, and evidence review for attendance correction",
        SortOrder = 5)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Correction Administration")]
    public class AttendanceCorrectionController : ControllerBase
    {
        private readonly AttendanceCorrectionService _service;

        public AttendanceCorrectionController(AttendanceCorrectionService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction", Description = "Melihat metadata filter attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public IActionResult GetFilterMetadata()
        {
            return Ok(ApiResponse<AttendanceCorrectionFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata filter attendance correction berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction", Description = "Melihat ringkasan attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AttendanceCorrectionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(request, null, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionSummaryResponse>.Ok(
                result,
                "Ringkasan attendance correction berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction", Description = "Melihat daftar attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> GetCorrectionRequests(
            [FromQuery] AttendanceCorrectionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetPagedAsync(request, null, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionPagedResponse>.Ok(
                result,
                "Data attendance correction berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Correction", Description = "Melihat detail attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetDetailAsync(id, null, cancellationToken));
        }

        [HttpGet("{id:guid}/workflow")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionWorkflowLinkResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction Workflow", Description = "Melihat workflow attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetWorkflowAsync(id, null, cancellationToken));
        }

        [HttpPost("{id:guid}/workflow/synchronize")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionSynchronizationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Synchronize", "Synchronize Attendance Correction Workflow", Description = "Menyinkronkan status attendance correction dengan workflow", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("AttendanceCorrection", "Synchronize")]
        public async Task<IActionResult> SynchronizeWorkflow(Guid id, CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
            {
                return Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
            }

            return ToActionResult(await _service.SynchronizeAsync(id, actorUserId, cancellationToken));
        }

        [HttpPost("{id:guid}/apply")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionApplyResponse>), StatusCodes.Status200OK)]
        [AccessAction("Apply", "Apply Attendance Correction", Description = "Menerapkan attendance correction yang telah disetujui", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceCorrection", "Apply")]
        public async Task<IActionResult> Apply(
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

            return ToActionResult(await _service.ApplyAsync(id, request, actorUserId, cancellationToken));
        }

        [HttpGet("{id:guid}/evidence/download")]
        [AccessAction("Read", "Download Attendance Correction Evidence", Description = "Mengunduh evidence attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> DownloadEvidence(Guid id, CancellationToken cancellationToken)
        {
            var result = await _service.ResolveEvidenceDownloadAsync(id, null, cancellationToken);
            if (!result.Success || result.Data == null)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(result.StatusCode, result.Message));
            }

            return PhysicalFile(
                result.Data.PhysicalPath,
                result.Data.ContentType,
                result.Data.FileName,
                enableRangeProcessing: true);
        }

        private IActionResult ToActionResult<T>(AttendanceCorrectionServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                        User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
