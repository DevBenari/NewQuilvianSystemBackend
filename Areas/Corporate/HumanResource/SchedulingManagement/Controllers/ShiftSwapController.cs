using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.SchedulingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/scheduling-management/shift-swap-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_SCHEDULING_MANAGEMENT",
        moduleName: "Human Resource Scheduling Management",
        displayName: "Shift Swap Administration",
        AreaName = "Corporate",
        ControllerName = "ShiftSwapRequest",
        Description = "HR and scheduling administration for shift swap requests",
        SortOrder = 21)]
    [Tags("Corporate / Human Resource / Scheduling Management / Shift Swap Administration")]
    public class ShiftSwapController : ControllerBase
    {
        private readonly ShiftSwapService _service;
        private readonly ShiftSwapWorkflowIntegrationService _workflowService;

        public ShiftSwapController(
            ShiftSwapService service,
            ShiftSwapWorkflowIntegrationService workflowService)
        {
            _service = service;
            _workflowService = workflowService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftSwapRequest", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<ShiftSwapFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata tukar shift berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftSwapRequest", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? workforceProfileId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(workforceProfileId, cancellationToken);
            return Ok(ApiResponse<ShiftSwapSummaryResponse>.Ok(
                result,
                "Ringkasan tukar shift berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftSwapRequest", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? requestStatus,
            [FromQuery] DateOnly? startDate,
            [FromQuery] DateOnly? endDate,
            [FromQuery] string? search,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                workforceProfileId,
                "all",
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

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Shift Swap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftSwapRequest", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetByIdAsync(id, null, cancellationToken));
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read Shift Swap Workflow", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ShiftSwapRequest", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _workflowService.GetWorkflowAsync(id, null, cancellationToken));
        }

        [HttpPost("{id:guid}/apply")]
        [AccessAction("Apply", "Apply Shift Swap", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ShiftSwapRequest", "Update")]
        public async Task<IActionResult> Apply(
            Guid id,
            [FromBody] ShiftSwapApplyRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            return ToActionResult(await _service.ApplyAsync(
                id,
                actorUserId,
                request.Notes,
                cancellationToken));
        }

        [HttpPost("{id:guid}/workflow/synchronize")]
        [AccessAction("Synchronize", "Synchronize Shift Swap Workflow", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ShiftSwapRequest", "Update")]
        public async Task<IActionResult> Synchronize(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            return ToActionResult(await _workflowService.SynchronizeAsync(
                id,
                actorUserId,
                cancellationToken));
        }

        private IActionResult ToActionResult<T>(SchedulingRequestServiceResult<T> result)
        {
            return result.Success
                ? StatusCode(result.StatusCode, ApiResponse<T>.Ok(result.Data, result.Message))
                : StatusCode(result.StatusCode, ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }

        private bool TryGetUserId(out Guid userId)
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out userId) && userId != Guid.Empty;
        }

        private IActionResult UnauthorizedResponse()
        {
            return Unauthorized(ApiResponse<object>.Fail(
                StatusCodes.Status401Unauthorized,
                "Identitas user login tidak valid."));
        }
    }
}
