using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers
{
    /// <summary>
    /// Controller REST API Pengajuan Klaim Koordinasi Manfaat (COB), Settlement & Kliring Exit Clearance (Slice S-D3, BE-HRD-188 & 189).
    /// Tag Swagger: 'HR Benefits - COB Claims & Settlement'.
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/benefits/claims")]
    [Route("api/hr-benefits/claims")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_BENEFIT_MANAGEMENT",
        moduleName: "Human Resource Benefit Management",
        displayName: "Klaim Benefit Pegawai",
        AreaName = "Corporate",
        ControllerName = "BenefitClaim",
        Description = "Antrean klaim COB, klaim mandiri pegawai, verifikasi, dan penyelesaian santunan",
        SortOrder = 20)]
    [Tags("HR Benefits - COB Claims & Settlement")]
    public class BenefitClaimController : ControllerBase
    {
        private readonly IBenefitClaimService _claimService;

        public BenefitClaimController(IBenefitClaimService claimService)
        {
            _claimService = claimService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(claim, out var uid) && uid != Guid.Empty)
            {
                return uid;
            }

            // Tidak memakai aktor cadangan: jejak audit tanpa identitas asli lebih berbahaya
            // daripada permintaan yang ditolak.
            throw new UnauthorizedAccessException("Identitas user pada token tidak valid.");
        }

        private Guid GetCurrentWorkforceProfileId()
        {
            var claim = User.FindFirst("WorkforceProfileId")?.Value ?? User.FindFirst("EmployeeId")?.Value;
            return Guid.TryParse(claim, out var wfpId) ? wfpId : GetCurrentUserId();
        }

        /// <summary>
        /// Mengambil daftar pengajuan klaim biaya kesehatan COB dengan filter status (BE-HRD-188).
        /// </summary>
        [HttpGet]
        [AccessAction("Read", "Melihat Antrean Klaim Benefit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitClaim", "Read")]
        [ProducesResponseType(typeof(PagedResult<BenefitClaimSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetClaims(
            [FromQuery] BenefitClaimListQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.GetClaimsPagedAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil daftar klaim milik pegawai yang sedang login di portal ESS (BE-HRD-188).
        /// </summary>
        [HttpGet("my-claims")]
        [AccessAction("ViewOwn", "Melihat Klaim Benefit Sendiri", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("BenefitClaim", "ViewOwn")]
        [ProducesResponseType(typeof(List<BenefitClaimSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyClaims(CancellationToken cancellationToken)
        {
            var wfpId = GetCurrentWorkforceProfileId();
            var result = await _claimService.GetMyClaimsAsync(wfpId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil detail klaim, rincian biaya, nomor SEP BPJS, dan dokumen bukti (BE-HRD-188).
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Antrean Klaim Benefit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitClaim", "Read")]
        [ProducesResponseType(typeof(BenefitClaimDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClaimDetail(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _claimService.GetClaimDetailAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Pegawai mengajukan klaim penjamin sekunder (COB) di ESS wajib menyertakan nomor SEP BPJS (BE-HRD-188, BEN-B5, VR-BEN-16..18).
        /// </summary>
        [HttpPost]
        [AccessAction("SubmitClaim", "Mengajukan Klaim Benefit", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("BenefitClaim", "SubmitClaim")]
        [ProducesResponseType(typeof(BenefitClaimDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateEssClaim(
            [FromBody] CreateEssClaimRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            try
            {
                var wfpId = GetCurrentWorkforceProfileId();
                var result = await _claimService.CreateEssClaimAsync(
                    wfpId, request, GetCurrentUserId(), cancellationToken);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengunggah dokumen pendukung klaim (salinan SEP BPJS, kwitansi asli, perincian obat) (BE-HRD-188, VR-BEN-20..21).
        /// </summary>
        [HttpPost("{id:guid}/documents")]
        [AccessAction("SubmitClaim", "Mengajukan Klaim Benefit", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("BenefitClaim", "SubmitClaim")]
        [ProducesResponseType(typeof(ClaimDocumentDetailDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddClaimDocument(
            Guid id,
            [FromBody] ClaimDocumentUploadRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _claimService.AddClaimDocumentAsync(
                    id, request, GetCurrentUserId(), cancellationToken);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Verifikasi audit persetujuan/penolakan klaim sekunder oleh HR Benefit Officer (BE-HRD-189).
        /// </summary>
        [HttpPost("{id:guid}/verify")]
        [AccessAction("Verify", "Memverifikasi Klaim Benefit", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BenefitClaim", "Verify")]
        [ProducesResponseType(typeof(BenefitClaimDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyClaim(
            Guid id,
            [FromBody] VerifyClaimRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _claimService.VerifyClaimAsync(
                    id, request, GetCurrentUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Konfirmasi penyelesaian settlement santunan sekunder & delegasi ekses tagihan ke payroll (BE-HRD-189, AUDIT-BEN-10).
        /// </summary>
        [HttpPost("{id:guid}/settle")]
        [AccessAction("Settle", "Menyelesaikan Santunan Klaim", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("BenefitClaim", "Settle")]
        [ProducesResponseType(typeof(BenefitClaimDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SettleClaim(
            Guid id,
            [FromBody] SettleClaimRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _claimService.SettleClaimAsync(
                    id, request, GetCurrentUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Penonaktifan hak benefit kepesertaan saat pegawai keluar/resign (BE-HRD-189, BEN-B7, VR-BEN-23).
        /// </summary>
        [HttpPost("enrollments/{id:guid}/terminate")]
        [AccessAction("TerminateEnrollment", "Mengakhiri Kepesertaan Benefit", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("BenefitClaim", "TerminateEnrollment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> TerminateEnrollment(
            Guid id,
            [FromBody] TerminateBenefitRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _claimService.TerminateBenefitEnrollmentAsync(
                    id, request, GetCurrentUserId(), cancellationToken);
                return Ok(new { success = result, message = "Kepesertaan benefit berhasil diterminasi." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Pengecekan gerbang Exit Clearance Sub-20 atas tunggakan ekses tagihan medis di RS Quilvian (BE-HRD-189, BEN-B8, VR-BEN-24).
        /// </summary>
        [HttpGet("exit-clearance-check/{workforceProfileId:guid}")]
        [AccessAction("Read", "Melihat Antrean Klaim Benefit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitClaim", "Read")]
        [ProducesResponseType(typeof(ExitClearanceMedicalReceivableDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckExitClearanceMedicalReceivable(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            var result = await _claimService.CheckExitClearanceMedicalReceivableAsync(
                workforceProfileId, cancellationToken);
            return Ok(result);
        }
    }
}
