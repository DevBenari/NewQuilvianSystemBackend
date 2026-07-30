using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

using ApprovalDelegationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs.ApprovalDelegationListResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/approval-delegations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFLOW",
        moduleName: "Human Resource Workflow",
        displayName: "Approval Delegation",
        AreaName = "Corporate",
        ControllerName = "ApprovalDelegation",
        Description = "Corporate human resource operational approval delegation",
        SortOrder = 4)]
    [Tags("Corporate / Human Resource / Workflow Management / Approval Delegation")]
    public class ApprovalDelegationController : ControllerBase
    {
        private readonly ApprovalDelegationService _service;

        public ApprovalDelegationController(ApprovalDelegationService service)
        {
            _service = service;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationFilterMetadataResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Approval Delegation",
            Description = "Melihat metadata filter approval delegation",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalDelegation", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = _service.GetFilterMetadata();
            return Ok(
                ApiResponse<ApprovalDelegationFilterMetadataResponse>.Ok(
                    result,
                    "Metadata filter approval delegation berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationSummaryResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Approval Delegation",
            Description = "Melihat ringkasan approval delegation",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalDelegation", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] Guid? delegatorUserId,
            [FromQuery] Guid? delegateUserId,
            [FromQuery] Guid? approvalDelegationPolicyId,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] string? delegationStatus,
            [FromQuery] bool? appliesToAllWorkflows,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetSummaryAsync(
                startDate,
                endDate,
                period,
                delegatorUserId,
                delegateUserId,
                approvalDelegationPolicyId,
                workflowDefinitionId,
                workflowStepId,
                delegationStatus,
                appliesToAllWorkflows,
                isActive,
                search,
                cancellationToken);

            return ToActionResult(result);
        }

        [HttpGet]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationPagedResult>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Approval Delegation",
            Description = "Melihat daftar approval delegation",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalDelegation", "Read")]
        public async Task<IActionResult> GetPaged(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? period,
            [FromQuery] Guid? delegatorUserId,
            [FromQuery] Guid? delegateUserId,
            [FromQuery] Guid? approvalDelegationPolicyId,
            [FromQuery] Guid? workflowDefinitionId,
            [FromQuery] Guid? workflowStepId,
            [FromQuery] string? delegationStatus,
            [FromQuery] bool? appliesToAllWorkflows,
            [FromQuery] bool? isActive,
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
                delegatorUserId,
                delegateUserId,
                approvalDelegationPolicyId,
                workflowDefinitionId,
                workflowStepId,
                delegationStatus,
                appliesToAllWorkflows,
                isActive,
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
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Approval Delegation",
            Description = "Melihat detail approval delegation",
            AccessType = AccessTypes.Read,
            SortOrder = 1)]
        [AccessPermission("ApprovalDelegation", "Read")]
        public async Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Create",
            "Create Approval Delegation",
            Description = "Membuat draft approval delegation",
            AccessType = AccessTypes.Create,
            SortOrder = 2)]
        [AccessPermission("ApprovalDelegation", "Create")]
        public async Task<IActionResult> CreateDraft(
            [FromBody] CreateApprovalDelegationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CreateDraftAsync(request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status400BadRequest)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status404NotFound)]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status409Conflict)]
        [AccessAction(
            "Update",
            "Update Approval Delegation",
            Description = "Mengubah draft approval delegation",
            AccessType = AccessTypes.Update,
            SortOrder = 3)]
        [AccessPermission("ApprovalDelegation", "Update")]
        public async Task<IActionResult> UpdateDraft(
            Guid id,
            [FromBody] UpdateApprovalDelegationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.UpdateDraftAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/submit")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Submit",
            "Submit Approval Delegation",
            Description = "Mengajukan approval delegation",
            AccessType = AccessTypes.Update,
            SortOrder = 4)]
        [AccessPermission("ApprovalDelegation", "Submit")]
        public async Task<IActionResult> Submit(
            Guid id,
            [FromBody] SubmitApprovalDelegationRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.SubmitAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/approve")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Approve",
            "Approve Approval Delegation",
            Description = "Menyetujui approval delegation tanpa generic approval workflow",
            AccessType = AccessTypes.Update,
            SortOrder = 5)]
        [AccessPermission("ApprovalDelegation", "Approve")]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveApprovalDelegationRequest? request,
            CancellationToken cancellationToken)
        {
            var result = await _service.ApproveAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/reject")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Reject",
            "Reject Approval Delegation",
            Description = "Menolak approval delegation tanpa generic approval workflow",
            AccessType = AccessTypes.Update,
            SortOrder = 6)]
        [AccessPermission("ApprovalDelegation", "Reject")]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectApprovalDelegationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RejectAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/activate")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Activate",
            "Activate Approval Delegation",
            Description = "Mengaktifkan approval delegation yang sudah disetujui",
            AccessType = AccessTypes.Update,
            SortOrder = 7)]
        [AccessPermission("ApprovalDelegation", "Activate")]
        public async Task<IActionResult> Activate(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.ActivateAsync(id, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/revoke")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Revoke",
            "Revoke Approval Delegation",
            Description = "Mencabut approval delegation aktif",
            AccessType = AccessTypes.Update,
            SortOrder = 8)]
        [AccessPermission("ApprovalDelegation", "Revoke")]
        public async Task<IActionResult> Revoke(
            Guid id,
            [FromBody] RevokeApprovalDelegationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.RevokeAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpPost("{id:guid}/cancel")]
        [ProducesResponseType(
            typeof(ApiResponse<ApprovalDelegationDetailResponse>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Cancel",
            "Cancel Approval Delegation",
            Description = "Membatalkan approval delegation yang belum aktif",
            AccessType = AccessTypes.Update,
            SortOrder = 9)]
        [AccessPermission("ApprovalDelegation", "Cancel")]
        public async Task<IActionResult> Cancel(
            Guid id,
            [FromBody] CancelApprovalDelegationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _service.CancelAsync(id, request, cancellationToken);
            return ToActionResult(result);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(
            typeof(ApiResponse<object>),
            StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Approval Delegation",
            Description = "Menghapus approval delegation secara soft delete",
            AccessType = AccessTypes.Delete,
            SortOrder = 10)]
        [AccessPermission("ApprovalDelegation", "Delete")]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(id, cancellationToken);
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
