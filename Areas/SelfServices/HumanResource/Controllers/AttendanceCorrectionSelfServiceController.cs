using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.AttendanceManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/attendance-corrections")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Attendance Correction",
        AreaName = "SelfServices",
        ControllerName = "MyAttendanceCorrection",
        Description = "Employee self-service attendance correction request, evidence, and workflow",
        SortOrder = 3)]
    [Tags("Self Services / Human Resource / Attendance Correction")]
    public class AttendanceCorrectionSelfServiceController : ControllerBase
    {
        private const string LogCategory = "SelfServices.HumanResource.AttendanceCorrection";
        private readonly AttendanceCorrectionService _service;
        private readonly LoggerService _loggerService;

        public AttendanceCorrectionSelfServiceController(
            AttendanceCorrectionService service,
            LoggerService loggerService)
        {
            _service = service;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat metadata pengajuan koreksi attendance", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceCorrection", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<AttendanceCorrectionFilterMetadataResponse>.Ok(
                _service.GetMetadata(),
                "Metadata koreksi attendance berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat ringkasan koreksi attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceCorrection", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] AttendanceCorrectionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            var result = await _service.GetSummaryAsync(request, actorUserId, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionSummaryResponse>.Ok(
                result,
                "Ringkasan koreksi attendance berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat daftar koreksi attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceCorrection", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] AttendanceCorrectionQueryRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            var result = await _service.GetPagedAsync(request, actorUserId, cancellationToken);
            return Ok(ApiResponse<AttendanceCorrectionPagedResponse>.Ok(
                result,
                "Data koreksi attendance berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Attendance Correction", Description = "Melihat detail koreksi attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceCorrection", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.GetDetailAsync(id, actorUserId, cancellationToken));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AttendanceCorrectionCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create My Attendance Correction", Description = "Membuat draft koreksi attendance milik sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyAttendanceCorrection", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateAttendanceCorrectionRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            var result = await _service.CreateAsync(request, actorUserId, cancellationToken);
            if (result.Success && result.Data != null)
            {
                await _loggerService.InfoAsync(
                    LogCategory,
                    "MyAttendanceCorrection.Create",
                    "Membuat draft koreksi attendance employee self service.",
                    result.Data);
            }

            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update My Attendance Correction", Description = "Mengubah draft koreksi attendance milik sendiri", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyAttendanceCorrection", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateAttendanceCorrectionRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.UpdateAsync(id, request, actorUserId, cancellationToken));
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit My Attendance Correction", Description = "Submit koreksi attendance ke generic workflow", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MyAttendanceCorrection", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] AttendanceCorrectionSubmitRequest? request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.SubmitAsync(id, request, actorUserId, cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel My Attendance Correction", Description = "Membatalkan atau menarik koreksi attendance", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MyAttendanceCorrection", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] AttendanceCorrectionCancelRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.CancelAsync(id, request, actorUserId, cancellationToken));
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read My Attendance Correction Workflow", Description = "Melihat workflow koreksi attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceCorrection", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.GetWorkflowAsync(id, actorUserId, cancellationToken));
        }

        [HttpPost("{id:guid}/evidence")]
        [Consumes("multipart/form-data")]
        [AccessAction("Update", "Upload My Attendance Correction Evidence", Description = "Mengunggah evidence koreksi attendance", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyAttendanceCorrection", "Update")]
        public async Task<IActionResult> UploadEvidence(
            Guid id,
            [FromForm] AttendanceCorrectionEvidenceUploadRequest request,
            CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.UploadEvidenceAsync(
                id,
                request.File,
                actorUserId,
                cancellationToken));
        }

        [HttpGet("{id:guid}/evidence/download")]
        [AccessAction("Read", "Download My Attendance Correction Evidence", Description = "Mengunduh evidence koreksi attendance milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyAttendanceCorrection", "Read")]
        public async Task<IActionResult> DownloadEvidence(Guid id, CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            var result = await _service.ResolveEvidenceDownloadAsync(id, actorUserId, cancellationToken);
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

        [HttpDelete("{id:guid}/evidence")]
        [AccessAction("Update", "Delete My Attendance Correction Evidence", Description = "Menghapus evidence koreksi attendance milik sendiri", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyAttendanceCorrection", "Update")]
        public async Task<IActionResult> DeleteEvidence(Guid id, CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.DeleteEvidenceAsync(id, actorUserId, cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete My Attendance Correction", Description = "Menghapus draft koreksi attendance milik sendiri", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("MyAttendanceCorrection", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var actorUserId = GetCurrentUserId();
            if (actorUserId == Guid.Empty) return UnauthorizedResult();

            return ToActionResult(await _service.DeleteDraftAsync(id, actorUserId, cancellationToken));
        }

        private IActionResult ToActionResult<T>(AttendanceCorrectionServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data!, result.Message))
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
