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
    [Route("api/v1/corporate/human-resource/attendance/correction-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_ATTENDANCE",
        moduleName: "Human Resource Attendance",
        displayName: "Attendance Correction Request",
        AreaName = "Corporate",
        ControllerName = "AttendanceCorrection",
        Description = "Corporate human resource attendance correction request and workflow integration",
        SortOrder = 5)]
    [Tags("Corporate / Human Resource / Attendance Management / Attendance Correction")]
    public class AttendanceCorrectionController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.AttendanceManagement";

        private readonly AttendanceCorrectionService _service;
        private readonly LoggerService _loggerService;

        public AttendanceCorrectionController(
            AttendanceCorrectionService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
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

        [HttpGet("my-requests/summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat ringkasan attendance correction user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionSelfService", "Read")]
        public async Task<IActionResult> GetMySummary(
            [FromQuery] AttendanceCorrectionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.GetSummaryAsync(request, actorUserId, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionSummaryResponse>.Ok(
                result,
                "Ringkasan attendance correction user login berhasil diambil."));
        }

        [HttpGet("my-requests")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat daftar attendance correction user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionSelfService", "Read")]
        public async Task<IActionResult> GetMyRequests(
            [FromQuery] AttendanceCorrectionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.GetPagedAsync(request, actorUserId, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionPagedResponse>.Ok(
                result,
                "Data attendance correction user login berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Attendance Correction", Description = "Melihat detail attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetDetailAsync(id, null, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("my-requests/{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat detail attendance correction user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionSelfService", "Read")]
        public async Task<IActionResult> GetMyRequestById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.GetDetailAsync(id, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Attendance Correction", Description = "Membuat draft attendance correction", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("AttendanceCorrectionSelfService", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateAttendanceCorrectionRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.CreateAsync(request, actorUserId, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "AttendanceCorrection.Create",
                    "Membuat draft attendance correction.",
                    result.Data);
            }
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Attendance Correction", Description = "Mengubah draft attendance correction", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceCorrectionSelfService", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateAttendanceCorrectionRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.UpdateAsync(id, request, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Attendance Correction", Description = "Menghapus draft attendance correction", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("AttendanceCorrectionSelfService", "Delete")]
        public async Task<IActionResult> DeleteDraft(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.DeleteDraftAsync(id, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionWorkflowResponse>), StatusCodes.Status200OK)]
        [AccessAction("Submit", "Submit Attendance Correction", Description = "Mengajukan attendance correction ke workflow", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("AttendanceCorrectionSelfService", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] AttendanceCorrectionSubmitRequest? request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.SubmitAsync(id, request, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionWorkflowResponse>), StatusCodes.Status200OK)]
        [AccessAction("Cancel", "Cancel Attendance Correction", Description = "Membatalkan atau menarik attendance correction", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("AttendanceCorrectionSelfService", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] AttendanceCorrectionCancelRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.CancelAsync(id, request, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/workflow")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionWorkflowLinkResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Attendance Correction Workflow", Description = "Melihat workflow attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> GetWorkflow(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetWorkflowAsync(id, null, cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("my-requests/{id:guid}/workflow")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionWorkflowLinkResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance Correction Workflow", Description = "Melihat workflow attendance correction user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionSelfService", "Read")]
        public async Task<IActionResult> GetMyWorkflow(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.GetWorkflowAsync(id, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/workflow/synchronize")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionSynchronizationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Synchronize", "Synchronize Attendance Correction Workflow", Description = "Menyinkronkan status attendance correction dengan workflow", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("AttendanceCorrection", "Update")]
        public async Task<IActionResult> SynchronizeWorkflow(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.SynchronizeAsync(id, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/apply")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionApplyResponse>), StatusCodes.Status200OK)]
        [AccessAction("Apply", "Apply Attendance Correction", Description = "Menerapkan attendance correction yang telah disetujui", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("AttendanceCorrection", "Update")]
        public async Task<IActionResult> Apply(
            Guid id,
            [FromBody] AttendanceCorrectionApplyRequest? request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.ApplyAsync(id, request, actorUserId, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/evidence")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionEvidenceResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Upload Attendance Correction Evidence", Description = "Mengunggah evidence attendance correction", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceCorrectionSelfService", "Update")]
        public async Task<IActionResult> UploadEvidence(
            Guid id,
            [FromForm] AttendanceCorrectionEvidenceUploadRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.UploadEvidenceAsync(
                id,
                request.File,
                actorUserId,
                cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/evidence/download")]
        [AccessAction("Read", "Download Attendance Correction Evidence", Description = "Mengunduh evidence attendance correction", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrection", "Read")]
        public async Task<IActionResult> DownloadEvidence(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.ResolveEvidenceDownloadAsync(id, null, cancellationToken);
            if (!result.Success || result.Data == null)
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

            return PhysicalFile(
                result.Data.PhysicalPath,
                result.Data.ContentType,
                result.Data.FileName,
                enableRangeProcessing: true);
        }

        [HttpGet("my-requests/{id:guid}/evidence/download")]
        [AccessAction("Read", "Download My Attendance Correction Evidence", Description = "Mengunduh evidence attendance correction user login", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("AttendanceCorrectionSelfService", "Read")]
        public async Task<IActionResult> DownloadMyEvidence(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.ResolveEvidenceDownloadAsync(id, actorUserId, cancellationToken);
            if (!result.Success || result.Data == null)
                return StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));

            return PhysicalFile(
                result.Data.PhysicalPath,
                result.Data.ContentType,
                result.Data.FileName,
                enableRangeProcessing: true);
        }

        [HttpDelete("{id:guid}/evidence")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Delete Attendance Correction Evidence", Description = "Menghapus evidence attendance correction", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("AttendanceCorrectionSelfService", "Update")]
        public async Task<IActionResult> DeleteEvidence(
            Guid id,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty)
                return Unauthorized(ApiResponse<object>.Fail(StatusCodes.Status401Unauthorized, "Identitas user login tidak valid."));

            var result = await _service.DeleteEvidenceAsync(id, actorUserId, cancellationToken);
            return ToActionResult(result);
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
