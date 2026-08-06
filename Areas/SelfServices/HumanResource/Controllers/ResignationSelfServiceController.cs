using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/resignation-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Resignation",
        AreaName = "SelfServices",
        ControllerName = "MyResignation",
        Description = "Employee self-service resignation requests",
        SortOrder = 12)]
    [Tags("Self Services / Human Resource / Resignation")]
    public class ResignationSelfServiceController : ControllerBase
    {
        private readonly ResignationRequestService _service;
        private readonly ResignationWorkflowIntegrationService _workflowService;
        private readonly HumanResourceContextService _contextService;

        public ResignationSelfServiceController(
            ResignationRequestService service,
            ResignationWorkflowIntegrationService workflowService,
            HumanResourceContextService contextService)
        {
            _service = service;
            _workflowService = workflowService;
            _contextService = contextService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read My Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyResignation", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<ResignationFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata pengajuan resign berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read My Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyResignation", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetSummaryAsync(
                context.Context!.WorkforceProfileId,
                cancellationToken);

            return Ok(ApiResponse<ResignationSummaryResponse>.Ok(
                result,
                "Ringkasan pengajuan resign berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read My Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyResignation", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? requestStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? search,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetPagedAsync(
                context.Context!.WorkforceProfileId,
                requestStatus,
                startDate,
                endDate,
                search,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<ResignationListResponse>>.Ok(
                result,
                "Daftar pengajuan resign berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read My Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyResignation", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            return owned.Error ?? ToActionResult(owned.Result!);
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read My Resignation Workflow", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyResignation", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.GetWorkflowAsync(id, cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create My Resignation", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyResignation", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateResignationSelfServiceRequest request,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            return ToActionResult(await _service.CreateDraftAsync(
                context.Context!.WorkforceProfileId!.Value,
                context.Context.EmployeeId,
                context.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update My Resignation", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyResignation", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateResignationSelfServiceRequest request,
            CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _service.UpdateDraftAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/submit")]
        [AccessAction("Submit", "Submit My Resignation", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MyResignation", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] ResignationSubmitRequest? request,
            CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.SubmitAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel My Resignation", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MyResignation", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] ResignationCancelRequest request,
            CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.CancelAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete My Resignation", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("MyResignation", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _service.DeleteAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                cancellationToken));
        }

        private async Task<(HumanResourceUserContextDto? Context, IActionResult? Error)> ResolveContextAsync(
            CancellationToken cancellationToken)
        {
            try
            {
                var context = await _contextService.GetCurrentAsync(cancellationToken);
                if (!context.WorkforceProfileId.HasValue)
                {
                    return (null, NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        "Akun user belum terhubung dengan workforce profile.")));
                }

                return (context, null);
            }
            catch (UnauthorizedAccessException ex)
            {
                return (null, Unauthorized(ApiResponse<object>.Fail(
                    StatusCodes.Status401Unauthorized,
                    ex.Message)));
            }
        }

        private async Task<(HumanResourceUserContextDto? Context, ResignationServiceResult<ResignationDetailResponse>? Result, IActionResult? Error)> GetOwnedAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return (null, null, context.Error);

            var result = await _service.GetByIdAsync(id, cancellationToken);
            if (!result.Success || result.Data == null)
            {
                return (context.Context, result, ToActionResult(result));
            }

            if (result.Data.WorkforceProfileId != context.Context!.WorkforceProfileId)
            {
                return (context.Context, null, NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan resign tidak ditemukan atau bukan milik user login.")));
            }

            return (context.Context, result, null);
        }

        private IActionResult ToActionResult<T>(ResignationServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
