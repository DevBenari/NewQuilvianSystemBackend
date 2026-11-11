using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LeaveManagement.Controllers
{
    /// <summary>
    /// Controller kotak masuk persetujuan cuti atasan berindikator kuota pelayanan (Safe Staffing Coverage Warning)
    /// dan verifikasi medis K3RS (BE-HRD-123, BE-HRD-124, HRD-DEC-111, HRD-DEC-112, VR-LEV-21).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/leave/approvals")]
    [Route("api/corporate/human-resource/leave/approvals")]
    [Route("api/v1/leave/approvals")]
    [Route("api/leave/approvals")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_LEAVE",
        moduleName: "Human Resource Leave",
        displayName: "Leave Approvals & Coverage",
        AreaName = "Corporate",
        ControllerName = "LeaveApproval",
        Description = "Kotak masuk persetujuan cuti bawahan, aksi mitigasi kuota staf, dan verifikasi berkas sakit K3RS",
        SortOrder = 10)]
    [Tags("HR Leave - Approvals & Coverage")]
    public class LeaveApprovalController : ControllerBase
    {
        private readonly LeaveApprovalService _approvalService;
        private readonly MedicalCertificateVerificationService _medicalService;

        public LeaveApprovalController(
            LeaveApprovalService approvalService,
            MedicalCertificateVerificationService medicalService)
        {
            _approvalService = approvalService;
            _medicalService = medicalService;
        }

        /// <summary>
        /// Kotak masuk persetujuan cuti bawahan disertai status Safe Staffing Coverage Warning.
        /// </summary>
        [HttpGet("inbox")]
        [AccessAction("View", "Melihat Kotak Masuk Persetujuan", Description = "Melihat permohonan cuti bawahan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LeaveApproval", "View")]
        [ProducesResponseType(typeof(ApiResponse<List<LeaveApprovalInboxItemDto>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetInbox(
            [FromQuery] Guid? unitId,
            [FromQuery] string? status,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.GetApprovalInboxAsync(userId, unitId, status, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<List<LeaveApprovalInboxItemDto>>.Ok(result.Data, result.Message)
                : ApiResponse<List<LeaveApprovalInboxItemDto>>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Menyetujui permohonan cuti (mewajibkan catatan mitigasi perawat pengganti jika warning aktif).
        /// </summary>
        [HttpPost("{id:guid}/approve")]
        [AccessAction("Approve", "Menyetujui Permohonan Cuti", Description = "Menyetujui cuti bawahan", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("LeaveApproval", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<LeaveApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Approve(
            Guid id,
            [FromBody] ApproveLeaveRequestCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.ApproveAsync(id, userId, command, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveApprovalResultDto>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Menolak permohonan cuti bawahan dengan alasan operasional sah.
        /// </summary>
        [HttpPost("{id:guid}/reject")]
        [AccessAction("Approve", "Menolak Permohonan Cuti", Description = "Menolak permohonan cuti", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LeaveApproval", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<LeaveApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Reject(
            Guid id,
            [FromBody] RejectLeaveRequestCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.RejectAsync(id, userId, command, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveApprovalResultDto>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Meminta revisi tanggal/durasi cuti kepada staf pemohon.
        /// </summary>
        [HttpPost("{id:guid}/revise")]
        [AccessAction("Approve", "Meminta Revisi Cuti", Description = "Mengembalikan permohonan cuti untuk revisi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LeaveApproval", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<LeaveApprovalResultDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Revise(
            Guid id,
            [FromBody] ReviseLeaveRequestCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _approvalService.ReviseAsync(id, userId, command, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<LeaveApprovalResultDto>.Ok(result.Data, result.Message)
                : ApiResponse<LeaveApprovalResultDto>.Fail(result.StatusCode, result.Message));
        }

        /// <summary>
        /// Verifikasi keabsahan surat keterangan sakit dokter oleh Petugas K3RS / HR Medis (HRD-DEC-112).
        /// </summary>
        [HttpPost("attachments/{attachmentId:guid}/verify-medical")]
        [AccessAction("Approve", "Verifikasi Surat Sakit K3RS", Description = "Verifikasi keabsahan surat dokter oleh K3RS", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("LeaveApproval", "Approve")]
        [ProducesResponseType(typeof(ApiResponse<MedicalCertificateDetailDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyMedicalAttachment(
            Guid attachmentId,
            [FromBody] VerifyMedicalCertificateCommand command,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            var result = await _medicalService.VerifyCertificateAsync(attachmentId, userId, command, cancellationToken);

            return StatusCode(result.StatusCode, result.Success
                ? ApiResponse<MedicalCertificateDetailDto>.Ok(result.Data, result.Message)
                : ApiResponse<MedicalCertificateDetailDto>.Fail(result.StatusCode, result.Message));
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(raw, out var parsed) ? parsed : Guid.Empty;
        }
    }
}
