using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/services/documents")]
    [Tags("HR Services - Document Issuance & Verification")]
    public class EmployeeDocumentIssuanceController : ControllerBase
    {
        private readonly IEmployeeDocumentIssuanceService _issuanceService;

        public EmployeeDocumentIssuanceController(IEmployeeDocumentIssuanceService issuanceService)
        {
            _issuanceService = issuanceService;
        }

        private Guid GetCurrentUserId()
        {
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out var g) ? g : Guid.Empty;
        }

        private bool IsHrStaff()
        {
            return User.IsInRole("SuperAdmin") || User.IsInRole("HRAdmin") || User.IsInRole("HRManager") || User.IsInRole("HRStaff");
        }

        /// <summary>
        /// Pengajuan permohonan surat keterangan dinas resmi kepegawaian (Anti-Manual Guard Enforced - VR-HRS-06)
        /// </summary>
        [HttpPost("request")]
        [Authorize]
        public async Task<IActionResult> RequestDocument(
            [FromQuery] Guid workforceProfileId,
            [FromBody] SubmitDocumentRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _issuanceService.RequestDocumentAsync(workforceProfileId, request, userId, cancellationToken);
                return CreatedAtAction(nameof(GetIssuanceDetail), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Persetujuan Tier 1 oleh Supervisor Kompensasi & Benefit untuk dokumen finansial (HRS-W4, VR-HRS-07, VR-HRS-08)
        /// </summary>
        [HttpPost("{id:guid}/cb-approve")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager,HRStaff")]
        public async Task<IActionResult> CbSupervisorApprove(
            Guid id,
            [FromBody] CbSupervisorApprovalDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _issuanceService.CbSupervisorApproveAsync(id, request, userId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Otorisasi Final & Penerbitan Surat Dinas Resmi oleh Manajer SDM / Direktur (Anti-Self-Approval Enforced - VR-HRS-08)
        /// </summary>
        [HttpPost("{id:guid}/authorize-issue")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> FinalAuthorizeAndIssue(
            Guid id,
            [FromBody] FinalAuthorizeAndIssueDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _issuanceService.FinalAuthorizeAndIssueAsync(id, request, userId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil antrean seluruh permohonan & penerbitan naskah dinas kepegawaian
        /// </summary>
        [HttpGet("queue")]
        [Authorize]
        public async Task<IActionResult> GetIssuancesQueue(
            [FromQuery] string? status = null,
            [FromQuery] bool? isFinancial = null,
            [FromQuery] Guid? workforceProfileId = null,
            CancellationToken cancellationToken = default)
        {
            var list = await _issuanceService.GetIssuancesQueueAsync(status, isFinancial, workforceProfileId, cancellationToken);
            return Ok(list);
        }

        /// <summary>
        /// Mengambil rincian data penerbitan naskah dinas kepegawaian
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetIssuanceDetail(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _issuanceService.GetIssuanceDetailAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengunduh berkas fisik resmi naskah dinas kepegawaian
        /// </summary>
        [HttpGet("{id:guid}/download")]
        [Authorize]
        public async Task<IActionResult> DownloadIssuedDocument(Guid id, CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var isStaff = IsHrStaff();
                var (fileBytes, contentType, fileName) = await _issuanceService.DownloadIssuedDocumentPdfAsync(id, userId, isStaff, cancellationToken);
                return File(fileBytes, contentType, fileName);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mencabut / membatalkan surat naskah dinas resmi dengan nomor BAP (Revocation)
        /// </summary>
        [HttpPost("{id:guid}/revoke")]
        [Authorize(Roles = "SuperAdmin,HRAdmin,HRManager")]
        public async Task<IActionResult> RevokeDocument(
            Guid id,
            [FromBody] RevokeDocumentRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _issuanceService.RevokeDocumentAsync(id, request, userId, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Endpoint Publik: Verifikasi keabsahan naskah dinas resmi RS Quilvian via SHA-256 Hash / QR Code (HRS-B5)
        /// Catatan: Nominal finansial disamarkan secara default (Masked).
        /// </summary>
        [HttpGet("public-verify/{hash}")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyPublicDocument(string hash, CancellationToken cancellationToken = default)
        {
            var result = await _issuanceService.VerifyPublicDocumentAsync(hash, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Endpoint Publik: Membuka kunci rincian nominal gaji dengan 4-digit PIN keamanan terenkripsi (VR-HRS-09)
        /// Dilindungi Rate Limiter Anti-Brute-Force (VR-HRS-12: max 3 kali salah percobaan per 30 menit).
        /// </summary>
        [HttpPost("unlock-financial")]
        [AllowAnonymous]
        public async Task<IActionResult> UnlockFinancialDetails(
            [FromBody] UnlockFinancialDetailsRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
                var result = await _issuanceService.UnlockFinancialDetailsAsync(request, clientIp, cancellationToken);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
