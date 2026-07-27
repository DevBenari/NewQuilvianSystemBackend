using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

using EmployeeProfileChangePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs.EmployeeProfileChangeListResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/employee-profile-changes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Employee Profile Change",
        AreaName = "Corporate",
        ControllerName = "EmployeeProfileChange",
        Description = "Corporate human resource employee profile change request, verification, approval, dan apply",
        SortOrder = 20)]
    [Tags("Corporate / Human Resource / Workforce Core / Employee Profile Change")]
    public class EmployeeProfileChangeController : ControllerBase
    {
        private readonly EmployeeProfileChangeService _service;

        public EmployeeProfileChangeController(
            EmployeeProfileChangeService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee Profile Change",
            Description = "Melihat metadata employee profile change",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("EmployeeProfileChange", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();

            return Ok(
                ApiResponse<EmployeeProfileChangeFilterMetadataResponse>.Ok(
                    result,
                    "Metadata employee profile change berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeSummaryResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee Profile Change",
            Description = "Melihat ringkasan employee profile change",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("EmployeeProfileChange", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? workforceProfileId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(
                workforceProfileId,
                cancellationToken);

            return Ok(
                ApiResponse<EmployeeProfileChangeSummaryResponse>.Ok(
                    result,
                    "Ringkasan employee profile change berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangePagedResult>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Employee Profile Change",
            Description = "Melihat daftar employee profile change",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("EmployeeProfileChange", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? requestStatus,
            [FromQuery] string? requestCategory,
            [FromQuery] Guid? requestedByUserId,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                startDate,
                endDate,
                period,
                workforceProfileId,
                requestStatus,
                requestCategory,
                requestedByUserId,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(
                ApiResponse<EmployeeProfileChangePagedResult>.Ok(
                    result,
                    "Data employee profile change berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Employee Profile Change",
            Description = "Melihat detail employee profile change",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("EmployeeProfileChange", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Employee Profile Change",
            Description = "Membuat draft employee profile change",
            AccessType = AccessTypes.Create,
            SortOrder = 2)]
        [AccessPermission("EmployeeProfileChange", "Create")]
        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateEmployeeProfileChangeRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.CreateDraftAsync(
                request,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Employee Profile Change",
            Description = "Mengubah draft atau revisi employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> UpdateDraft(
            Guid id,
            [FromBody] UpdateEmployeeProfileChangeRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.UpdateDraftAsync(
                id,
                request,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Submit Employee Profile Change",
            Description = "Submit employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 4)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] EmployeeProfileChangeActionNoteRequest? request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.SubmitAsync(
                id,
                request?.Note,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/start-verification")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Verify",
            "Start Employee Profile Change Verification",
            Description = "Memulai verifikasi employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 5)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> StartVerification(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.StartVerificationAsync(
                id,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/verifications/{verificationId:guid}/decision")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Verify",
            "Verify Employee Profile Change",
            Description = "Memberikan keputusan verifikasi employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 6)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> DecideVerification(
            Guid id,
            Guid verificationId,
            [FromForm] EmployeeProfileChangeVerificationDecisionRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.DecideVerificationAsync(
                id,
                verificationId,
                request,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet("{id:guid}/verifications/{verificationId:guid}/evidence")]
        [AccessAction(
            "Read",
            "Read Employee Profile Change Evidence",
            Description = "Mengunduh bukti verifikasi employee profile change",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("EmployeeProfileChange", "Read")]
        public async Task<IActionResult> DownloadEvidence(
            Guid id,
            Guid verificationId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetEvidenceFileAsync(
                id,
                verificationId,
                cancellationToken);

            if (!result.Success || result.Data == default)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<object>.Fail(
                        result.StatusCode,
                        result.Message));
            }

            var file = result.Data;
            var stream = new FileStream(
                file.PhysicalPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            return File(
                stream,
                file.ContentType,
                file.FileName,
                enableRangeProcessing: true);
        }

        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Approve",
            "Approve Employee Profile Change",
            Description = "Menyetujui employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 7)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] EmployeeProfileChangeActionNoteRequest? request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.ApproveAsync(
                id,
                request?.Note,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Reject",
            "Reject Employee Profile Change",
            Description = "Menolak employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 8)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] EmployeeProfileChangeRejectRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.RejectAsync(
                id,
                request.Reason,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/request-revision")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Request Employee Profile Change Revision",
            Description = "Meminta revisi employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 9)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> RequestRevision(
            Guid id,
            [FromBody] EmployeeProfileChangeRevisionRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.RequestRevisionAsync(
                id,
                request.Reason,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/apply")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeApplyResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Apply",
            "Apply Employee Profile Change",
            Description = "Menerapkan employee profile change ke data workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 10)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> Apply(
            Guid id,
            [FromBody] ApplyEmployeeProfileChangeRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.ApplyAsync(
                id,
                request,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(
            typeof(ApiResponse<EmployeeProfileChangeResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Cancel",
            "Cancel Employee Profile Change",
            Description = "Membatalkan employee profile change",
            AccessType = AccessTypes.Update,
            SortOrder = 11)]
        [AccessPermission("EmployeeProfileChange", "Update")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] EmployeeProfileChangeActionNoteRequest? request,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.CancelAsync(
                id,
                request?.Note,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Employee Profile Change",
            Description = "Menghapus employee profile change secara soft delete",
            AccessType = AccessTypes.Delete,
            SortOrder = 12)]
        [AccessPermission("EmployeeProfileChange", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            if (!TryGetCurrentUserId(out var actorUserId))
                return UnauthorizedResponse();

            var result = await _service.DeleteAsync(
                id,
                actorUserId,
                cancellationToken);

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(
            EmployeeProfileChangeServiceResult<T> result)
        {
            if (result.Success)
            {
                return Ok(
                    ApiResponse<T>.Ok(
                        result.Data,
                        result.Message));
            }

            return StatusCode(
                result.StatusCode,
                ApiResponse<object>.Fail(
                    result.StatusCode,
                    result.Message));
        }

        private bool TryGetCurrentUserId(out Guid userId)
        {
            var value =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out userId) &&
                   userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(
                ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    "Identitas user login tidak valid."));
        }
    }
}
