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
    [Route("api/v1/self-services/human-resource/schedule-change-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_EMPLOYEE_SELF_SERVICE",
        moduleName: "Human Resource Employee Self Service",
        displayName: "My Schedule Change",
        AreaName = "SelfServices",
        ControllerName = "MyScheduleChange",
        Description = "Employee self-service schedule change requests",
        SortOrder = 10)]
    [Tags("Self Services / Human Resource / Schedule Change")]
    public class ScheduleChangeSelfServiceController : ControllerBase
    {
        private readonly ScheduleChangeService _service;
        private readonly ScheduleChangeWorkflowIntegrationService _workflowService;
        private readonly HumanResourceContextService _contextService;

        public ScheduleChangeSelfServiceController(
            ScheduleChangeService service,
            ScheduleChangeWorkflowIntegrationService workflowService,
            HumanResourceContextService contextService)
        {
            _service = service;
            _workflowService = workflowService;
            _contextService = contextService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read My Schedule Change", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<ScheduleChangeFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata perubahan jadwal berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read My Schedule Change", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetSummaryAsync(
                context.Context!.WorkforceProfileId,
                cancellationToken);

            return Ok(ApiResponse<ScheduleChangeSummaryResponse>.Ok(
                result,
                "Ringkasan perubahan jadwal berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read My Schedule Change", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] string? requestStatus,
            [FromQuery] string? requestType,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
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
                requestType,
                startDate,
                endDate,
                search,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<PagedResult<ScheduleChangeListResponse>>.Ok(
                result,
                "Daftar perubahan jadwal berhasil diambil."));
        }

        [HttpGet("schedule-options")]
        [AccessAction("Read", "Read My Schedule Options", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> GetScheduleOptions(
            [FromQuery] DateOnly date,
            CancellationToken cancellationToken)
        {
            var context = await ResolveContextAsync(cancellationToken);
            if (context.Error != null) return context.Error;

            var result = await _service.GetMyScheduleOptionsAsync(
                context.Context!.WorkforceProfileId!.Value,
                date,
                cancellationToken);

            return Ok(ApiResponse<List<ScheduleChangeOptionResponse>>.Ok(
                result,
                "Pilihan work schedule berhasil diambil."));
        }

        [HttpGet("shift-options")]
        [AccessAction("Read", "Read Shift Options", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> GetShiftOptions(
            [FromQuery] Guid? workScheduleId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetAvailableShiftOptionsAsync(
                workScheduleId,
                cancellationToken);

            return Ok(ApiResponse<List<ScheduleChangeOptionResponse>>.Ok(
                result,
                "Pilihan shift berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read My Schedule Change", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            return owned.Error ?? ToActionResult(owned.Result!);
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read My Schedule Change Workflow", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            var owned = await GetOwnedAsync(id, cancellationToken);
            if (owned.Error != null) return owned.Error;

            return ToActionResult(await _workflowService.GetWorkflowAsync(id, cancellationToken));
        }

        [HttpPost("validate-preview")]
        [AccessAction("Validate", "Validate My Schedule Change", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("MyScheduleChange", "Read")]
        public async Task<IActionResult> ValidatePreview(
            [FromBody] CreateScheduleChangeSelfServiceRequest request,
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
        [AccessAction("Create", "Create My Schedule Change", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("MyScheduleChange", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateScheduleChangeSelfServiceRequest request,
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
        [AccessAction("Update", "Update My Schedule Change", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("MyScheduleChange", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateScheduleChangeSelfServiceRequest request,
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
        [AccessAction("Submit", "Submit My Schedule Change", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("MyScheduleChange", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] ScheduleChangeSubmitRequest? request,
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
        [AccessAction("Cancel", "Cancel My Schedule Change", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("MyScheduleChange", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] ScheduleChangeCancelRequest request,
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
        [AccessAction("Delete", "Delete My Schedule Change", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("MyScheduleChange", "Delete")]
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

        private async Task<(HumanResourceUserContextDto? Context, SchedulingRequestServiceResult<ScheduleChangeDetailResponse>? Result, IActionResult? Error)> GetOwnedAsync(
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
                    "Pengajuan perubahan jadwal tidak ditemukan atau bukan milik user login.")));
            }

            return (context.Context, result, null);
        }

        private IActionResult ToActionResult<T>(SchedulingRequestServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
