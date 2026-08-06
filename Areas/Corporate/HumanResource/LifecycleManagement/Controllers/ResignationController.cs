using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/lifecycle-management/resignation-requests")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LIFECYCLE_MANAGEMENT",
        moduleName: "Human Resource Lifecycle Management",
        displayName: "Resignation Administration",
        AreaName = "Corporate",
        ControllerName = "ResignationRequest",
        Description = "HR administration and lifecycle handoff for resignation requests",
        SortOrder = 20)]
    [Tags("Corporate / Human Resource / Lifecycle Management / Resignation Administration")]
    public class ResignationController : ControllerBase
    {
        private readonly ResignationRequestService _service;
        private readonly ResignationWorkflowIntegrationService _workflowService;
        private readonly ResignationLifecycleHandoffService _handoffService;

        public ResignationController(
            ResignationRequestService service,
            ResignationWorkflowIntegrationService workflowService,
            ResignationLifecycleHandoffService handoffService)
        {
            _service = service;
            _workflowService = workflowService;
            _handoffService = handoffService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ResignationRequest", "Read")]
        public IActionResult GetMetadata()
        {
            return Ok(ApiResponse<ResignationFilterMetadataResponse>.Ok(
                _service.GetFilterMetadata(),
                "Metadata resign berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ResignationRequest", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? workforceProfileId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(workforceProfileId, cancellationToken);
            return Ok(ApiResponse<ResignationSummaryResponse>.Ok(
                result,
                "Ringkasan resign berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ResignationRequest", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] Guid? workforceProfileId,
            [FromQuery] string? requestStatus,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? search,
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                workforceProfileId,
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
                "Daftar resign berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Resignation", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ResignationRequest", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _service.GetByIdAsync(id, cancellationToken));
        }

        [HttpGet("{id:guid}/workflow")]
        [AccessAction("Read", "Read Resignation Workflow", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ResignationRequest", "Read")]
        public async Task<IActionResult> GetWorkflow(Guid id, CancellationToken cancellationToken)
        {
            return ToActionResult(await _workflowService.GetWorkflowAsync(id, cancellationToken));
        }

        [HttpPost("{id:guid}/workflow/synchronize")]
        [AccessAction("Synchronize", "Synchronize Resignation Workflow", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ResignationRequest", "Update")]
        public async Task<IActionResult> Synchronize(Guid id, CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            return ToActionResult(await _workflowService.SynchronizeAsync(
                id,
                actorUserId,
                cancellationToken));
        }

        [HttpPost("{id:guid}/handoff")]
        [AccessAction("Handoff", "Handoff Resignation To Offboarding", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ResignationRequest", "Update")]
        public async Task<IActionResult> Handoff(
            Guid id,
            [FromBody] ResignationHandoffRequest request,
            CancellationToken cancellationToken)
        {
            if (!TryGetUserId(out var actorUserId)) return UnauthorizedResponse();
            return ToActionResult(await _handoffService.HandoffAsync(
                id,
                request,
                actorUserId,
                cancellationToken));
        }

        private IActionResult ToActionResult<T>(ResignationServiceResult<T> result)
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
