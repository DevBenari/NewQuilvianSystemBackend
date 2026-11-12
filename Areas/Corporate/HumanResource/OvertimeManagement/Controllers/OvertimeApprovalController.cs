using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.OvertimeManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/overtime/approvals")]
    [Route("api/overtime/approvals")]
    [Tags("HR Overtime - Approvals & Escalations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_OVERTIME",
        moduleName: "Human Resource Overtime",
        displayName: "Overtime Approvals & Escalations",
        AreaName = "Corporate",
        ControllerName = "OvertimeApproval",
        Description = "Persetujuan bertingkat operasional ruangan, pencegahan self-approval, dan eskalasi Direktur Pelayanan Medik",
        SortOrder = 3)]
    public class OvertimeApprovalController : ControllerBase
    {
        private readonly IOvertimeApprovalWorkflowService _approvalService;

        public OvertimeApprovalController(IOvertimeApprovalWorkflowService approvalService)
        {
            _approvalService = approvalService ?? throw new ArgumentNullException(nameof(approvalService));
        }

        [HttpGet("inbox")]
        [AccessAction("View", "Melihat Kotak Masuk Persetujuan", Description = "Melihat daftar SPKL bawahan yang menunggu persetujuan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OvertimeApproval", "View")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<OvertimeApprovalInboxItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInbox(
            [FromQuery] string? status = null,
            [FromQuery] Guid? unitId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.GetInboxAsync(actorUserId, status, unitId, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<OvertimeApprovalInboxItemDto>>.Ok(result, "Kotak masuk persetujuan lembur berhasil diambil."));
        }

        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Menyetujui SPKL", Description = "Menyetujui permohonan SPKL staf (Anti-Self-Approval)", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OvertimeApproval", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApprovePlan(
            Guid id,
            [FromBody] ApproveOvertimeCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.ApprovePlanAsync(id, command, actorUserId, cancellationToken);
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimeApprovalResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimeApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpPost("{id:guid}/reject")]
        [AccessAction("Approve", "Menolak SPKL", Description = "Menolak permohonan SPKL staf dengan alasan resmi", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OvertimeApproval", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectPlan(
            Guid id,
            [FromBody] RejectOvertimeCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.RejectPlanAsync(id, command, actorUserId, cancellationToken);
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimeApprovalResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimeApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpGet("medical-director-inbox")]
        [AccessAction("MedicalDirectorApprove", "Melihat Kotak Masuk Direktur Medis", Description = "Melihat daftar eskalasi lembur darurat cito yang melampaui kuota PP 35/2021", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<MedicalDirectorOvertimeEscalationDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMedicalDirectorInbox(
            [FromQuery] string? urgency = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var result = await _approvalService.GetMedicalDirectorInboxAsync(urgency, page, pageSize, cancellationToken);
            return Ok(ApiResponse<PagedResult<MedicalDirectorOvertimeEscalationDto>>.Ok(result, "Kotak masuk eskalasi Direktur Medis berhasil diambil."));
        }

        [HttpPost("{id:guid}/medical-director-approve")]
        [AccessAction("MedicalDirectorApprove", "Otorisasi Dispensasi Direktur Medis", Description = "Menerbitkan otorisasi dispensasi khusus darurat penyelamatan nyawa pasien", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApproveMedicalDirector(
            Guid id,
            [FromBody] ApproveMedicalDirectorEscalationCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.ApproveMedicalDirectorEscalationAsync(id, command, actorUserId, cancellationToken);
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimeApprovalResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimeApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        [HttpPost("{id:guid}/medical-director-reject")]
        [AccessAction("MedicalDirectorApprove", "Menolak Dispensasi Direktur Medis", Description = "Menolak permohonan dispensasi lembur darurat cito melampaui kuota", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("OvertimeApproval", "MedicalDirectorApprove")]
        [ProducesResponseType(typeof(ApiResponse<OvertimeApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RejectMedicalDirector(
            Guid id,
            [FromBody] RejectOvertimeCommand command,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();
            var result = await _approvalService.RejectMedicalDirectorEscalationAsync(id, command?.RejectionReason ?? "Ditolak oleh Direktur Pelayanan Medik", actorUserId, cancellationToken);
            return StatusCode(
                result.StatusCode,
                result.Success
                    ? ApiResponse<OvertimeApprovalResultDto>.Ok(result.Data!, result.Message)
                    : ApiResponse<OvertimeApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
