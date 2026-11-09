using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/hrd/workflow")]
    [Tags("Corporate - HRD Workflow Approval Actions")]
    public class WorkflowApprovalActionsController : ControllerBase
    {
        private readonly IWorkflowApprovalActionService _actionService;
        private readonly IBulkApprovalService _bulkApprovalService;
        private readonly IWorkflowResubmitService _resubmitService;

        public WorkflowApprovalActionsController(
            IWorkflowApprovalActionService actionService,
            IBulkApprovalService bulkApprovalService,
            IWorkflowResubmitService resubmitService)
        {
            _actionService = actionService;
            _bulkApprovalService = bulkApprovalService;
            _resubmitService = resubmitService;
        }

        private Guid GetCurrentEmployeeId()
        {
            var claim = User.FindFirst("EmployeeId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var empId)) return empId;
            return Guid.Empty;
        }

        /// <summary>
        /// Menyetujui permohonan pada tahapan tertentu oleh approver sah (FR-WF-10, HRD-DEC-186)
        /// </summary>
        [HttpPost("approvals/{stepInstanceId}/approve")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowActionV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Approve([FromRoute] Guid stepInstanceId, [FromBody] ApproveActionRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _actionService.ApproveAsync(stepInstanceId, request, empId);
                return Ok(ApiResponse<WorkflowActionV2ResponseDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WorkflowActionV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<WorkflowActionV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Menolak permohonan dengan alasan penolakan wajib minimal 15 karakter (FR-WF-11, VR-WF-05)
        /// </summary>
        [HttpPost("approvals/{stepInstanceId}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowActionV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reject([FromRoute] Guid stepInstanceId, [FromBody] RejectActionRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _actionService.RejectAsync(stepInstanceId, request, empId);
                return Ok(ApiResponse<WorkflowActionV2ResponseDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WorkflowActionV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<WorkflowActionV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Mengembalikan draf permohonan ke staf pemohon untuk koreksi data/revisi (FR-WF-12, HRD-DEC-186)
        /// </summary>
        [HttpPost("approvals/{stepInstanceId}/return")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowActionV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReturnForRevision([FromRoute] Guid stepInstanceId, [FromBody] ReturnForRevisionRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _actionService.ReturnForRevisionAsync(stepInstanceId, request, empId);
                return Ok(ApiResponse<WorkflowActionV2ResponseDto>.Ok(result, result.Message));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WorkflowActionV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<WorkflowActionV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Melakukan persetujuan massal bersyarat untuk permohonan berisiko rendah bersih (FR-WF-18, FR-WF-19, HRD-DEC-187)
        /// </summary>
        [HttpPost("approvals/bulk-approve")]
        [ProducesResponseType(typeof(ApiResponse<BulkApproveResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BulkApprove([FromBody] BulkApproveRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _bulkApprovalService.ExecuteBulkApprovalAsync(request, empId);
                return Ok(ApiResponse<BulkApproveResultDto>.Ok(result, $"Persetujuan massal selesai: {result.TotalApproved} disetujui, {result.TotalSkipped} dilewati."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<BulkApproveResultDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<BulkApproveResultDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }

        /// <summary>
        /// Mengajukan ulang permohonan yang telah diperbaiki staf pasca revisi (FR-WF-22, VR-WF-06)
        /// </summary>
        [HttpPost("instances/{id}/resubmit")]
        [ProducesResponseType(typeof(ApiResponse<WorkflowInstanceV2ResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Resubmit([FromRoute] Guid id, [FromBody] ResubmitWorkflowRequestDto request)
        {
            try
            {
                var empId = GetCurrentEmployeeId();
                var result = await _resubmitService.ResubmitAsync(id, request, empId);
                return Ok(ApiResponse<WorkflowInstanceV2ResponseDto>.Ok(result, "Draf perbaikan berhasil diajukan ulang ke tahapan review"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<WorkflowInstanceV2ResponseDto>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ApiResponse<WorkflowInstanceV2ResponseDto>.Fail(StatusCodes.Status500InternalServerError, ex.Message));
            }
        }
    }
}
