using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workflow-instances")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFLOW",
        moduleName: "Human Resource Workflow",
        displayName: "Workflow Comment",
        AreaName = "Corporate",
        ControllerName = "WorkflowComment",
        Description = "Corporate human resource workflow comments and discussion",
        SortOrder = 3)]
    [Tags("Corporate / Human Resource / Workflow Management / Workflow Comment")]
    public class WorkflowCommentController : ControllerBase
    {
        private readonly WorkflowCommentService _service;

        public WorkflowCommentController(WorkflowCommentService service)
        {
            _service = service;
        }

        [HttpGet("comments/filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowCommentFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workflow Comment",
            Description = "Melihat metadata filter workflow comment",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowComment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();

            return Ok(
                ApiResponse<WorkflowCommentFilterMetadataResponse>.Ok(
                    result,
                    "Metadata filter workflow comment berhasil diambil."));
        }

        [HttpGet("{workflowInstanceId:guid}/comments")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<WorkflowCommentListResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workflow Comment",
            Description = "Melihat daftar komentar workflow",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowComment", "Read")]
        public async Task<IActionResult> GetComments(
            Guid workflowInstanceId,
            [FromQuery] Guid? workflowStepInstanceId,
            [FromQuery] Guid? parentCommentId,
            [FromQuery] string? commentType,
            [FromQuery] bool? isInternalComment,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetCommentsAsync(
                workflowInstanceId,
                workflowStepInstanceId,
                parentCommentId,
                commentType,
                isInternalComment,
                search,
                pageNumber,
                pageSize,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/comments")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowCommentListResponse>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Create",
            "Create Workflow Comment",
            Description = "Menambahkan komentar atau balasan pada workflow",
            AccessType = AccessTypes.Create,
            SortOrder = 2)]
        [AccessPermission("WorkflowComment", "Create")]
        public async Task<IActionResult> CreateComment(
            Guid workflowInstanceId,
            [FromBody] CreateWorkflowCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.CreateCommentAsync(
                workflowInstanceId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPut("{workflowInstanceId:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowCommentListResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Update",
            "Update Workflow Comment",
            Description = "Mengubah komentar workflow milik user login",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("WorkflowComment", "Update")]
        public async Task<IActionResult> UpdateComment(
            Guid workflowInstanceId,
            Guid commentId,
            [FromBody] UpdateWorkflowCommentRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.UpdateCommentAsync(
                workflowInstanceId,
                commentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpDelete("{workflowInstanceId:guid}/comments/{commentId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Delete",
            "Delete Workflow Comment",
            Description = "Melakukan soft delete komentar workflow milik user login",
            AccessType = AccessTypes.Delete,
            SortOrder = 4)]
        [AccessPermission("WorkflowComment", "Delete")]
        public async Task<IActionResult> DeleteComment(
            Guid workflowInstanceId,
            Guid commentId,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.DeleteCommentAsync(
                workflowInstanceId,
                commentId,
                cancellationToken);

            return ToActionResult(result);
        }

        private IActionResult ToActionResult<T>(WorkflowServiceResult<T> result)
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
    }
}
