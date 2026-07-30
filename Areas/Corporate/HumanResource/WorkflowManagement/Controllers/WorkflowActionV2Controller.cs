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
        displayName: "Workflow Action",
        AreaName = "Corporate",
        ControllerName = "WorkflowActionV2",
        Description = "Corporate human resource workflow lifecycle actions",
        SortOrder = 2)]
    [Tags("Corporate / Human Resource / Workflow Management / Workflow Instance")]
    public class WorkflowActionV2Controller : ControllerBase
    {
        private readonly WorkflowService _service;

        public WorkflowActionV2Controller(WorkflowService service)
        {
            _service = service;
        }

        [HttpPost("{workflowInstanceId:guid}/assignments/{assignmentId:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Reject",
            "Reject Workflow Assignment",
            Description = "Menolak approver assignment aktif berdasarkan rejection reason",
            AccessType = AccessTypes.Update,
            SortOrder = 5)]
        [AccessPermission("WorkflowInstance", "Reject")]
        public async Task<IActionResult> Reject(
            Guid workflowInstanceId,
            Guid assignmentId,
            [FromBody] WorkflowRejectRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RejectAsync(
                workflowInstanceId,
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/assignments/{assignmentId:guid}/request-revision")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "RequestRevision",
            "Request Workflow Revision",
            Description = "Mengembalikan workflow kepada pemohon untuk direvisi",
            AccessType = AccessTypes.Update,
            SortOrder = 6)]
        [AccessPermission("WorkflowInstance", "RequestRevision")]
        public async Task<IActionResult> RequestRevision(
            Guid workflowInstanceId,
            Guid assignmentId,
            [FromBody] WorkflowRequestRevisionRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RequestRevisionAsync(
                workflowInstanceId,
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/assignments/{assignmentId:guid}/return")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Return",
            "Return Workflow Assignment",
            Description = "Mengembalikan workflow ke step sebelumnya",
            AccessType = AccessTypes.Update,
            SortOrder = 7)]
        [AccessPermission("WorkflowInstance", "Return")]
        public async Task<IActionResult> Return(
            Guid workflowInstanceId,
            Guid assignmentId,
            [FromBody] WorkflowReturnRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.ReturnAsync(
                workflowInstanceId,
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/assignments/{assignmentId:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Verify",
            "Verify Workflow Assignment",
            Description = "Memverifikasi assignment pada step verification",
            AccessType = AccessTypes.Update,
            SortOrder = 8)]
        [AccessPermission("WorkflowInstance", "Verify")]
        public async Task<IActionResult> Verify(
            Guid workflowInstanceId,
            Guid assignmentId,
            [FromBody] WorkflowVerifyRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.VerifyAsync(
                workflowInstanceId,
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{workflowInstanceId:guid}/assignments/{assignmentId:guid}/acknowledge")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Acknowledge",
            "Acknowledge Workflow Assignment",
            Description = "Menyatakan acknowledgement pada assignment aktif",
            AccessType = AccessTypes.Update,
            SortOrder = 9)]
        [AccessPermission("WorkflowInstance", "Acknowledge")]
        public async Task<IActionResult> Acknowledge(
            Guid workflowInstanceId,
            Guid assignmentId,
            [FromBody] WorkflowAcknowledgeRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.AcknowledgeAsync(
                workflowInstanceId,
                assignmentId,
                request,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Cancel",
            "Cancel Workflow Instance",
            Description = "Membatalkan workflow oleh pemohon sesuai workflow definition",
            AccessType = AccessTypes.Update,
            SortOrder = 10)]
        [AccessPermission("WorkflowInstance", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] WorkflowCancelRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/withdraw")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(
            "Withdraw",
            "Withdraw Workflow Instance",
            Description = "Menarik workflow yang sedang diproses oleh pemohon",
            AccessType = AccessTypes.Update,
            SortOrder = 11)]
        [AccessPermission("WorkflowInstance", "Withdraw")]
        public async Task<IActionResult> Withdraw(
            Guid id,
            [FromBody] WorkflowWithdrawRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.WithdrawAsync(id, request, cancellationToken);
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
