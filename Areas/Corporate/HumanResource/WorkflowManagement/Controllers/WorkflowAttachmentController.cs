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
        displayName: "Workflow Attachment",
        AreaName = "Corporate",
        ControllerName = "WorkflowAttachment",
        Description = "Corporate human resource workflow attachment management",
        SortOrder = 4)]
    [Tags("Corporate / Human Resource / Workflow Management / Workflow Attachment")]
    public class WorkflowAttachmentController : ControllerBase
    {
        private readonly WorkflowAttachmentService _service;

        public WorkflowAttachmentController(WorkflowAttachmentService service)
        {
            _service = service;
        }

        [HttpGet("attachments/filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowAttachmentFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workflow Attachment",
            Description = "Melihat metadata upload dan filter workflow attachment",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowAttachment", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();

            return Ok(
                ApiResponse<WorkflowAttachmentFilterMetadataResponse>.Ok(
                    result,
                    "Metadata workflow attachment berhasil diambil."));
        }

        [HttpGet("{workflowInstanceId:guid}/attachments")]
        [ProducesResponseType(
            typeof(ApiResponse<PagedResult<WorkflowAttachmentListResponse>>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workflow Attachment",
            Description = "Melihat daftar lampiran workflow",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowAttachment", "Read")]
        public async Task<IActionResult> GetAttachments(
            Guid workflowInstanceId,
            [FromQuery] Guid? workflowStepInstanceId,
            [FromQuery] Guid? approvalActionId,
            [FromQuery] Guid? workflowCommentId,
            [FromQuery] string? attachmentCategory,
            [FromQuery] bool? isConfidential,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetAttachmentsAsync(
                workflowInstanceId,
                workflowStepInstanceId,
                approvalActionId,
                workflowCommentId,
                attachmentCategory,
                isConfidential,
                search,
                pageNumber,
                pageSize,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/attachments")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(25L * 1024L * 1024L)]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowAttachmentListResponse>),
            StatusCodes.Status201Created)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Create",
            "Upload Workflow Attachment",
            Description = "Mengunggah lampiran workflow, step, action, atau comment",
            AccessType = AccessTypes.Create,
            SortOrder = 2)]
        [AccessPermission("WorkflowAttachment", "Create")]
        public async Task<IActionResult> UploadAttachment(
            Guid workflowInstanceId,
            [FromForm] UploadWorkflowAttachmentRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.UploadAttachmentAsync(
                workflowInstanceId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet("{workflowInstanceId:guid}/attachments/{attachmentId:guid}/download")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status403Forbidden)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Download Workflow Attachment",
            Description = "Mengunduh file lampiran workflow",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("WorkflowAttachment", "Read")]
        public async Task<IActionResult> DownloadAttachment(
            Guid workflowInstanceId,
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDownloadAsync(
                workflowInstanceId,
                attachmentId,
                cancellationToken);

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

        [HttpDelete("{workflowInstanceId:guid}/attachments/{attachmentId:guid}")]
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
            "Delete Workflow Attachment",
            Description = "Melakukan soft delete metadata dan menghapus file fisik workflow",
            AccessType = AccessTypes.Delete,
            SortOrder = 3)]
        [AccessPermission("WorkflowAttachment", "Delete")]
        public async Task<IActionResult> DeleteAttachment(
            Guid workflowInstanceId,
            Guid attachmentId,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.DeleteAttachmentAsync(
                workflowInstanceId,
                attachmentId,
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
