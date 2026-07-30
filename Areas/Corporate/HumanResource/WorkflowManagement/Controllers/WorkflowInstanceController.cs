using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

using WorkflowInstancePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs.WorkflowInstanceListResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workflow-instances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFLOW",
        moduleName: "Human Resource Workflow",
        displayName: "Workflow Instance",
        AreaName = "Corporate",
        ControllerName = "WorkflowInstance",
        Description = "Corporate human resource generic workflow engine",
        SortOrder = 1)]
    [Tags("Corporate / Human Resource / Workflow Management / Workflow Instance")]
    public class WorkflowInstanceController : ControllerBase
    {
        private readonly WorkflowService _service;

        public WorkflowInstanceController(WorkflowService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workflow Instance",
            Description = "Melihat metadata filter workflow instance",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowInstance", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();

            return Ok(
                ApiResponse<WorkflowFilterMetadataResponse>.Ok(
                    result,
                    "Metadata filter workflow instance berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowSummaryResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Workflow Instance",
            Description = "Melihat ringkasan workflow instance",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowInstance", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] string? workflowCode,
            [FromQuery] string? referenceType,
            [FromQuery] Guid? requestedByWorkforceProfileId,
            [FromQuery] string? workflowStatus,
            [FromQuery] string? currentStepCode,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(
                startDate,
                endDate,
                period,
                workflowDefinitionId,
                workflowCode,
                referenceType,
                requestedByWorkforceProfileId,
                workflowStatus,
                currentStepCode,
                search,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstancePagedResult>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Workflow Instance",
            Description = "Melihat daftar workflow instance",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowInstance", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] string? workflowCode,
            [FromQuery] string? referenceType,
            [FromQuery] Guid? requestedByWorkforceProfileId,
            [FromQuery] string? workflowStatus,
            [FromQuery] string? currentStepCode,
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
                workflowDefinitionId,
                workflowCode,
                referenceType,
                requestedByWorkforceProfileId,
                workflowStatus,
                currentStepCode,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workflow Instance",
            Description = "Melihat detail workflow instance",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowInstance", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Create",
            "Create Workflow Instance",
            Description = "Membuat draft workflow instance dan approver assignment",
            AccessType = AccessTypes.Create,
            SortOrder = 2)]
        [AccessPermission("WorkflowInstance", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkflowInstanceRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Submit",
            "Submit Workflow Instance",
            Description = "Submit draft workflow dan mengaktifkan step pertama",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("WorkflowInstance", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] WorkflowSubmitRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(
                id,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/assignments/{assignmentId:guid}/approve")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Approve",
            "Approve Workflow Assignment",
            Description = "Menyetujui approver assignment aktif",
            AccessType = AccessTypes.Update,
            SortOrder = 4)]
        [AccessPermission("WorkflowInstance", "Approve")]
        public async Task<IActionResult> Approve(
            Guid workflowInstanceId,
            Guid assignmentId,
            [FromBody] WorkflowApproveRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.ApproveAsync(
                workflowInstanceId,
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(WorkflowServiceResult<T> result)
        {
            if (result.Success)
            {
                return StatusCode(
                    result.StatusCode,
                    ApiResponse<T>.Ok(
                        result.Data!,
                        result.Message));
            }

            return StatusCode(
                result.StatusCode,
                ApiResponse<object>.Fail(
                    result.StatusCode,
                    result.Message));
        }
    }
}
