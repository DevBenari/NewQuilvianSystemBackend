using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Services;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs;
using QuilvianSystemBackend.Areas.SelfServices.HumanResource.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

using EmployeeProfileChangePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.SelfServices.HumanResource.DTOs.EmployeeProfileChangeListResponse>;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/profile-changes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Profile Change",
        AreaName = "SelfServices",
        ControllerName = "MyProfileChange",
        Description = "Employee self-service profile change request and generic workflow submission",
        SortOrder = 9)]
    [Tags("Self Services / Human Resource / Profile Change")]
    public class EmployeeProfileChangeSelfServiceController : ControllerBase
    {
        private readonly EmployeeProfileChangeService _service;
        private readonly EmployeeProfileChangeWorkflowIntegrationService _workflowIntegrationService;
        private readonly HumanResourceContextService _humanResourceContextService;

        public EmployeeProfileChangeSelfServiceController(
            EmployeeProfileChangeService service,
            EmployeeProfileChangeWorkflowIntegrationService workflowIntegrationService,
            HumanResourceContextService humanResourceContextService)
        {
            _service = service;
            _workflowIntegrationService = workflowIntegrationService;
            _humanResourceContextService = humanResourceContextService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileChangeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Profile Change", Description = "Melihat metadata pengajuan perubahan profil", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyProfileChange", "Read")]
        public IActionResult GetMetadata()
        {
            var metadata = _service.GetFilterMetadata();
            metadata.DefaultFilter.WorkforceProfileId = null;
            metadata.DefaultFilter.RequestedByUserId = null;

            return Ok(ApiResponse<EmployeeProfileChangeFilterMetadataResponse>.Ok(
                metadata,
                "Metadata perubahan profil berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileChangeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Profile Change", Description = "Melihat ringkasan perubahan profil milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyProfileChange", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var contextResult = await ResolveContextAsync(cancellationToken);
            if (contextResult.Error != null) return contextResult.Error;

            var result = await _service.GetSummaryAsync(
                contextResult.Context!.WorkforceProfileId,
                cancellationToken);

            return Ok(ApiResponse<EmployeeProfileChangeSummaryResponse>.Ok(
                result,
                "Ringkasan perubahan profil berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileChangePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Profile Change", Description = "Melihat daftar perubahan profil milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyProfileChange", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] string? requestStatus,
            [FromQuery] string? requestCategory,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var contextResult = await ResolveContextAsync(cancellationToken);
            if (contextResult.Error != null) return contextResult.Error;

            var result = await _service.GetPagedAsync(
                startDate,
                endDate,
                period,
                contextResult.Context!.WorkforceProfileId,
                requestStatus,
                requestCategory,
                null,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<EmployeeProfileChangePagedResult>.Ok(
                result,
                "Data perubahan profil berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileChangeResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Profile Change", Description = "Melihat detail perubahan profil milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyProfileChange", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            return owned.Error ?? ToActionResult(owned.Result!);
        }

        [HttpGet("{id:guid}/workflow")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileChangeWorkflowLinkResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read My Profile Change Workflow", Description = "Melihat workflow perubahan profil milik sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyProfileChange", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowIntegrationService.GetWorkflowAsync(
                id,
                cancellationToken));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<EmployeeProfileChangeResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create My Profile Change", Description = "Membuat draft perubahan profil milik sendiri", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyProfileChange", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmployeeProfileChangeRequest request,
            CancellationToken cancellationToken)
        {
            var contextResult = await ResolveContextAsync(cancellationToken);
            if (contextResult.Error != null) return contextResult.Error;

            request.WorkforceProfileId = contextResult.Context!.WorkforceProfileId!.Value;

            return ToActionResult(await _service.CreateDraftAsync(
                request,
                contextResult.Context.UserId,
                cancellationToken));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update My Profile Change", Description = "Mengubah draft atau revisi perubahan profil milik sendiri", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyProfileChange", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateEmployeeProfileChangeRequest request,
            CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _service.UpdateDraftAsync(
                id,
                request,
                owned.Context!.UserId,
                cancellationToken));
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit My Profile Change", Description = "Submit perubahan profil ke generic workflow", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MyProfileChange", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] EmployeeProfileChangeWorkflowSubmitRequest? request,
            CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowIntegrationService.SubmitAsync(
                id,
                request,
                owned.Context!.UserId,
                cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel My Profile Change", Description = "Membatalkan atau menarik perubahan profil", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MyProfileChange", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] EmployeeProfileChangeWorkflowCancelRequest? request,
            CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowIntegrationService.CancelAsync(
                id,
                request,
                owned.Context!.UserId,
                cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete My Profile Change", Description = "Menghapus draft perubahan profil milik sendiri", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("MyProfileChange", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _service.DeleteAsync(
                id,
                owned.Context!.UserId,
                cancellationToken));
        }

        private async Task<(HumanResourceUserContextDto? Context, IActionResult? Error)> ResolveContextAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var context = await _humanResourceContextService.GetCurrentAsync(cancellationToken);
                if (!context.WorkforceProfileId.HasValue)
                {
                    return (null, NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        "Akun user belum terhubung dengan workforce profile.")));
                }

                return (context, null);
            }
            catch (UnauthorizedAccessException exception)
            {
                return (null, Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    exception.Message)));
            }
        }

        private async Task<(
            HumanResourceUserContextDto? Context,
            EmployeeProfileChangeServiceResult<EmployeeProfileChangeResponse>? Result,
            IActionResult? Error)> GetOwnedAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var contextResult = await ResolveContextAsync(cancellationToken);
            if (contextResult.Error != null)
            {
                return (null, null, contextResult.Error);
            }

            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (!result.Success || result.Data == null)
            {
                return (contextResult.Context, result, ToActionResult(result));
            }

            if (result.Data.WorkforceProfileId != contextResult.Context!.WorkforceProfileId)
            {
                return (
                    contextResult.Context,
                    null,
                    NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        "Pengajuan perubahan profil tidak ditemukan atau bukan milik user login.")));
            }

            return (contextResult.Context, result, null);
        }

        private IActionResult ToActionResult<T>(EmployeeProfileChangeServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
