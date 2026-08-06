using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Shared.HumanResource.DTOs;
using QuilvianSystemBackend.Shared.HumanResource.Services;

namespace QuilvianSystemBackend.Areas.SelfServices.HumanResource.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/self-services/human-resource/shift-swap-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Shift Swap",
        AreaName = "SelfServices",
        ControllerName = "MyShiftSwap",
        Description = "Employee self-service shift swap requests",
        SortOrder = 11)]
    [Tags("Self Services / Human Resource / Shift Swap")]
    public class ShiftSwapSelfServiceController : ControllerBase
    {
        private readonly ShiftSwapService _service;
        private readonly ShiftSwapWorkflowIntegrationService _workflowService;
        private readonly HumanResourceContextService _contextService;

        public ShiftSwapSelfServiceController(
            ShiftSwapService service,
            ShiftSwapWorkflowIntegrationService workflowService,
            HumanResourceContextService contextService)
        {
            _service = service;
            _workflowService = workflowService;
            _contextService = contextService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read My Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<ShiftSwapFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata tukar shift berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read My Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetSummaryAsync(
                context.Context!.WorkforceProfileId,
                cancellationToken);

            return Ok(ApiResponse<ShiftSwapSummaryResponse>.Ok(
                result,
                "Ringkasan tukar shift berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read My Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? viewMode = "all",
            [FromQuery] string? requestStatus = null,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetPagedAsync(
                context.Context!.WorkforceProfileId,
                viewMode,
                requestStatus,
                startDate,
                endDate,
                search,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<ShiftSwapListResponse>>.Ok(
                result,
                "Daftar tukar shift berhasil diambil."));
        }

        [HttpGet("target-options")]
        [AccessAction("Read", "Read Shift Swap Target Options", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetTargetOptions(
            [FromQuery] string? search,
            [FromQuery] int take = 20,
            CancellationToken cancellationToken = default)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetEligibleTargetOptionsAsync(
                context.Context!.WorkforceProfileId!.Value,
                search,
                take,
                cancellationToken);

            return Ok(ApiResponse<List<ShiftSwapTargetOptionResponse>>.Ok(
                result,
                "Target employee yang eligible berhasil diambil."));
        }

        [HttpGet("assignment-options")]
        [AccessAction("Read", "Read My Shift Assignment Options", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetMyAssignmentOptions(
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetAssignmentOptionsAsync(
                context.Context!.WorkforceProfileId!.Value,
                startDate,
                endDate,
                cancellationToken);

            return Ok(ApiResponse<List<ShiftSwapAssignmentOptionResponse>>.Ok(
                result,
                "Shift assignment milik user berhasil diambil."));
        }

        [HttpGet("target-assignment-options")]
        [AccessAction("Read", "Read Target Shift Assignment Options", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetTargetAssignmentOptions(
            [FromQuery] Guid targetWorkforceProfileId,
            [FromQuery] DateOnly startDate,
            [FromQuery] DateOnly endDate,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var eligible = await _service.GetEligibleTargetOptionsAsync(
                context.Context!.WorkforceProfileId!.Value,
                null,
                100,
                cancellationToken);

            if (!eligible.Any(x => x.WorkforceProfileId == targetWorkforceProfileId))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Target employee tidak eligible untuk tukar shift dengan user login."));
            }

            var result = await _service.GetAssignmentOptionsAsync(
                targetWorkforceProfileId,
                startDate,
                endDate,
                cancellationToken);

            return Ok(ApiResponse<List<ShiftSwapAssignmentOptionResponse>>.Ok(
                result,
                "Shift assignment target berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read My Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetParticipantRequestAsync(id, cancellationToken);
            return owned.Error ?? ToActionResult(owned.Result!);
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read My Shift Swap Workflow", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetParticipantRequestAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.GetWorkflowAsync(
                id,
                owned.Context!.WorkforceProfileId,
                cancellationToken));
        }

        [HttpPost("validate-preview")]
        [AccessAction("Validate", "Validate My Shift Swap", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("MyShiftSwap", "Read")]
        public async Task<IActionResult> ValidatePreview(
            [FromBody] CreateShiftSwapSelfServiceRequest request,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            return ToActionResult(await _service.ValidatePreviewAsync(
                context.Context!.WorkforceProfileId!.Value,
                request,
                null,
                cancellationToken));
        }

        [HttpPost]
        [AccessAction("Create", "Create My Shift Swap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyShiftSwap", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateShiftSwapSelfServiceRequest request,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            return ToActionResult(await _service.CreateDraftAsync(
                context.Context!.WorkforceProfileId!.Value,
                context.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update My Shift Swap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyShiftSwap", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateShiftSwapSelfServiceRequest request,
            CancellationToken cancellationToken)
        {
            var owned = await GetRequesterRequestAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _service.UpdateDraftAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/submit-to-target")]
        [AccessAction("Submit", "Submit My Shift Swap To Target", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MyShiftSwap", "Submit")]
        public async Task<IActionResult> SubmitToTarget(
            Guid id,
            [FromBody] ShiftSwapSubmitToTargetRequest? request,
            CancellationToken cancellationToken)
        {
            var owned = await GetRequesterRequestAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _service.SubmitToTargetAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request?.Note,
                cancellationToken));
        }

        [HttpPost("{id:guid}/target-response")]
        [AccessAction("Respond", "Respond Shift Swap As Target", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MyShiftSwap", "Respond")]
        public async Task<IActionResult> TargetResponse(
            Guid id,
            [FromBody] ShiftSwapTargetResponseRequest request,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            return ToActionResult(await _service.RespondAsTargetAsync(
                id,
                context.Context!.WorkforceProfileId!.Value,
                context.Context.UserId,
                request.Accept,
                request.Notes,
                cancellationToken));
        }

        [HttpPost("{id:guid}/submit-approval")]
        [AccessAction("Submit", "Submit My Shift Swap For Manager Approval", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("MyShiftSwap", "Submit")]
        public async Task<IActionResult> SubmitApproval(
            Guid id,
            [FromBody] ShiftSwapWorkflowSubmitRequest? request,
            CancellationToken cancellationToken)
        {
            var owned = await GetRequesterRequestAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.SubmitForManagerApprovalAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpPost("{id:guid}/cancel")]
        [AccessAction("Cancel", "Cancel My Shift Swap", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("MyShiftSwap", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] ShiftSwapCancelRequest request,
            CancellationToken cancellationToken)
        {
            var owned = await GetRequesterRequestAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.CancelAsync(
                id,
                owned.Context!.WorkforceProfileId!.Value,
                owned.Context.UserId,
                request,
                cancellationToken));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete My Shift Swap", AccessType = AccessTypes.Delete, SortOrder = 8)]
        [AccessPermission("MyShiftSwap", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetRequesterRequestAsync(id, cancellationToken);
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

        private async Task<(HumanResourceUserContextDto? Context, SchedulingRequestServiceResult<ShiftSwapDetailResponse>? Result, IActionResult? Error)> GetParticipantRequestAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return (null, null, context.Error);

            var result = await _service.GetByIdAsync(
                id,
                context.Context!.WorkforceProfileId,
                cancellationToken);

            if (!result.Success || result.Data == null)
            {
                return (context.Context, result, ToActionResult(result));
            }

            if (!result.Data.IsRequester && !result.Data.IsTarget)
            {
                return (context.Context, null, NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau user login bukan participant.")));
            }

            return (context.Context, result, null);
        }

        private async Task<(HumanResourceUserContextDto? Context, SchedulingRequestServiceResult<ShiftSwapDetailResponse>? Result, IActionResult? Error)> GetRequesterRequestAsync(
            Guid id,
            CancellationToken cancellationToken)
        {
            var participant = await GetParticipantRequestAsync(id, cancellationToken);
            if (participant.Error != null) return participant;

            if (participant.Result?.Data?.IsRequester != true)
            {
                return (participant.Context, participant.Result, NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengajuan tukar shift tidak ditemukan atau user login bukan requester.")));
            }

            return participant;
        }

        private IActionResult ToActionResult<T>(SchedulingRequestServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
