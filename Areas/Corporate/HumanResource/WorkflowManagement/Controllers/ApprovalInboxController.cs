using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

using ApprovalInboxPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs.ApprovalInboxItemResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/approval-inbox")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFLOW",
        moduleName: "Human Resource Workflow",
        displayName: "Approval Inbox",
        AreaName = "Corporate",
        ControllerName = "ApprovalInbox",
        Description = "Corporate human resource approval inbox based on workflow approver assignment",
        SortOrder = 5)]
    [Tags("Corporate / Human Resource / Workflow Management / Approval Inbox")]
    public class ApprovalInboxController : ControllerBase
    {
        private readonly ApprovalInboxService _service;

        public ApprovalInboxController(ApprovalInboxService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalInboxFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Approval Inbox",
            Description = "Melihat metadata filter approval inbox",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalInbox", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();

            return Ok(
                ApiResponse<ApprovalInboxFilterMetadataResponse>.Ok(
                    result,
                    "Metadata filter approval inbox berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalInboxSummaryResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status401Unauthorized)]
        [AccessAction(
            "Read",
            "Read Approval Inbox",
            Description = "Melihat ringkasan approval inbox user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalInbox", "Read")]
        public async Task<IActionResult> GetSummary(
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(cancellationToken);
            return ToActionResult(result);
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalInboxPagedResult>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Approval Inbox",
            Description = "Melihat daftar approval assignment user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalInbox", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] string? view = "open",
            [FromQuery] Guid? workflowDefinitionId = null,
            [FromQuery] string? workflowCode = null,
            [FromQuery] string? referenceType = null,
            [FromQuery] string? assignmentStatus = null,
            [FromQuery] string? stepType = null,
            [FromQuery] string? dueStatus = null,
            [FromQuery] bool? isDelegated = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "dueAt",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetPagedAsync(
                startDate,
                endDate,
                period,
                view,
                workflowDefinitionId,
                workflowCode,
                referenceType,
                assignmentStatus,
                stepType,
                dueStatus,
                isDelegated,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet("delegated-to-me")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalInboxPagedResult>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Delegated Approval Inbox",
            Description = "Melihat approval assignment yang didelegasikan kepada user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalInbox", "Read")]
        public async Task<IActionResult> GetDelegatedToMe(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] string? view = "open",
            [FromQuery] string? assignmentStatus = null,
            [FromQuery] string? stepType = null,
            [FromQuery] string? dueStatus = null,
            [FromQuery] string? search = null,
            [FromQuery] string? sortBy = "dueAt",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var result = await _service.GetDelegatedToMeAsync(
                startDate,
                endDate,
                period,
                view,
                assignmentStatus,
                stepType,
                dueStatus,
                search,
                sortBy,
                sortDirection,
                pageNumber,
                pageSize,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet("{assignmentId:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalInboxDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Approval Inbox Detail",
            Description = "Melihat detail approval assignment milik user login",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalInbox", "Read")]
        public async Task<IActionResult> GetById(
            Guid assignmentId,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(
                assignmentId,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{assignmentId:guid}/approve")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Approve",
            "Approve From Approval Inbox",
            Description = "Menyetujui assignment langsung dari approval inbox",
            AccessType = AccessTypes.Update,
            SortOrder = 2)]
        [AccessPermission("ApprovalInbox", "Approve")]
        public async Task<IActionResult> Approve(
            Guid assignmentId,
            [FromBody] WorkflowApproveRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.ApproveAsync(
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{assignmentId:guid}/reject")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Reject",
            "Reject From Approval Inbox",
            Description = "Menolak assignment langsung dari approval inbox",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("ApprovalInbox", "Reject")]
        public async Task<IActionResult> Reject(
            Guid assignmentId,
            [FromBody] WorkflowRejectRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RejectAsync(
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{assignmentId:guid}/request-revision")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "RequestRevision",
            "Request Revision From Approval Inbox",
            Description = "Meminta revisi langsung dari approval inbox",
            AccessType = AccessTypes.Update,
            SortOrder = 4)]
        [AccessPermission("ApprovalInbox", "RequestRevision")]
        public async Task<IActionResult> RequestRevision(
            Guid assignmentId,
            [FromBody] WorkflowRequestRevisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RequestRevisionAsync(
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{assignmentId:guid}/return")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Return",
            "Return From Approval Inbox",
            Description = "Mengembalikan workflow ke step sebelumnya dari approval inbox",
            AccessType = AccessTypes.Update,
            SortOrder = 5)]
        [AccessPermission("ApprovalInbox", "Return")]
        public async Task<IActionResult> Return(
            Guid assignmentId,
            [FromBody] WorkflowReturnRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.ReturnAsync(
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{assignmentId:guid}/verify")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Verify",
            "Verify From Approval Inbox",
            Description = "Memverifikasi assignment dari approval inbox",
            AccessType = AccessTypes.Update,
            SortOrder = 6)]
        [AccessPermission("ApprovalInbox", "Verify")]
        public async Task<IActionResult> Verify(
            Guid assignmentId,
            [FromBody] WorkflowVerifyRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.VerifyAsync(
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{assignmentId:guid}/acknowledge")]
        [ProducesResponseType(
            typeof(ApiResponse<WorkflowInstanceDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Acknowledge",
            "Acknowledge From Approval Inbox",
            Description = "Melakukan acknowledgement dari approval inbox",
            AccessType = AccessTypes.Update,
            SortOrder = 7)]
        [AccessPermission("ApprovalInbox", "Acknowledge")]
        public async Task<IActionResult> Acknowledge(
            Guid assignmentId,
            [FromBody] WorkflowAcknowledgeRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.AcknowledgeAsync(
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
                    ApiResponse<T>.Ok(result.Data!, result.Message));
            }

            return StatusCode(
                result.StatusCode,
                ApiResponse<object>.Fail(result.StatusCode, result.Message));
        }
    }
}
