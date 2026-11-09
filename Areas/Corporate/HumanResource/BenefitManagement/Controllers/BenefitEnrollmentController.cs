using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Controllers
{
    /// <summary>
    /// Controller REST API Administrasi Kepesertaan Asuransi & Pendaftaran Tanggungan Keluarga ESS (Slice S-D3, BE-HRD-185).
    /// Mengimplementasikan seluruh endpoint Kontrak v16 dengan tag Swagger 'HR Benefits - Enrollments & Dependents'.
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/benefits")]
    [Route("api/hr-benefits")]
    [Authorize]
    [Tags("HR Benefits - Enrollments & Dependents")]
    public class BenefitEnrollmentController : ControllerBase
    {
        private readonly IBenefitEnrollmentService _enrollmentService;

        public BenefitEnrollmentController(IBenefitEnrollmentService enrollmentService)
        {
            _enrollmentService = enrollmentService;
        }

        private Guid GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var uid) ? uid : Guid.Parse("11111111-1111-1111-1111-111111111111");
        }

        private Guid GetCurrentWorkforceProfileId()
        {
            var claim = User.FindFirst("WorkforceProfileId")?.Value ?? User.FindFirst("EmployeeId")?.Value;
            return Guid.TryParse(claim, out var wfpId) ? wfpId : GetCurrentUserId();
        }

        // =========================================================================
        // 1. ADMINISTRASI KEPESERTAAN ASURANSI PEGAWAI RS (BE-HRD-182)
        // =========================================================================

        /// <summary>
        /// Mengambil daftar kepesertaan asuransi pegawai rumah sakit dengan filter status aktivasi (BE-HRD-182, FR-BEN-06).
        /// </summary>
        [HttpGet("enrollments")]
        [ProducesResponseType(typeof(PagedResult<EmployeeEnrollmentListItemDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEnrollments(
            [FromQuery] EmployeeEnrollmentListQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _enrollmentService.GetEnrollmentsPagedAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil detail kepesertaan asuransi pegawai beserta daftar tanggungan terdaftar (BE-HRD-182).
        /// </summary>
        [HttpGet("enrollments/{id:guid}")]
        [ProducesResponseType(typeof(BenefitEnrollmentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEnrollmentDetail(
            Guid id,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _enrollmentService.GetEnrollmentDetailAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengaktifkan paket lengkap BPJS Kesehatan/Swasta pasca-lulus masa percobaan 3 bulan (BE-HRD-182, BEN-B1, VR-BEN-13).
        /// </summary>
        [HttpPost("enrollments/{id:guid}/activate-probation")]
        [ProducesResponseType(typeof(BenefitEnrollmentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateProbation(
            Guid id,
            [FromBody] ActivateProbationRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _enrollmentService.ActivateProbationEnrollmentAsync(
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
        /// Memperbarui nomor kartu BPJS Kesehatan, BPJS Ketenagakerjaan, atau nomor polis swasta (BE-HRD-182, VR-BEN-09, VR-BEN-10).
        /// </summary>
        [HttpPut("enrollments/{id:guid}/policy-numbers")]
        [ProducesResponseType(typeof(BenefitEnrollmentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePolicyNumbers(
            Guid id,
            [FromBody] UpdatePolicyNumbersRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _enrollmentService.UpdatePolicyNumbersAsync(
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

        // =========================================================================
        // 2. MEJA KERJA VERIFIKASI BERKAS FISIK KEPENDUDUKAN HR (BE-HRD-184)
        // =========================================================================

        /// <summary>
        /// Mengambil antrean pendaftaran anggota keluarga ESS yang membutuhkan verifikasi dokumen HR (BE-HRD-184, FR-BEN-10).
        /// </summary>
        [HttpGet("dependents/pending-verification")]
        [ProducesResponseType(typeof(PagedResult<PendingDependentVerificationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPendingDependentVerifications(
            [FromQuery] PendingDependentVerificationQuery query,
            CancellationToken cancellationToken)
        {
            var result = await _enrollmentService.GetPendingDependentVerificationsAsync(query, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Persetujuan atau penolakan verifikasi berkas KK/Akta tanggungan keluarga oleh HR (BE-HRD-184, VR-BEN-25, AUDIT-BEN-05).
        /// </summary>
        [HttpPost("dependents/{id:guid}/verify")]
        [ProducesResponseType(typeof(BenefitDependentDetailDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyDependent(
            Guid id,
            [FromBody] VerifyDependentRequestDto request,
            CancellationToken cancellationToken)
        {
            try
            {
                var result = await _enrollmentService.VerifyDependentAsync(
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

        // =========================================================================
        // 3. PORTAL MANDIRI ESS STAF & KUOTA TANGGUNGAN (BE-HRD-181, BE-HRD-183)
        // =========================================================================

        /// <summary>
        /// Mengambil daftar anggota keluarga yang terdaftar pada profil pegawai yang sedang login (BE-HRD-181, FR-BEN-07).
        /// </summary>
        [HttpGet("dependents/my-family")]
        [ProducesResponseType(typeof(List<EmployeeDependentSummaryDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFamilyDependents(CancellationToken cancellationToken)
        {
            var wfpId = GetCurrentWorkforceProfileId();
            var result = await _enrollmentService.GetMyFamilyDependentsAsync(wfpId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Mengambil ringkasan kuota tanggungan keluarga aktif dan sisa kuota (maks 1 pasangan + 3 anak) (BE-HRD-181, BEN-B2).
        /// </summary>
        [HttpGet("dependents/my-family/quota")]
        [ProducesResponseType(typeof(EmployeeFamilyQuotaSummaryDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyFamilyQuota(CancellationToken cancellationToken)
        {
            var wfpId = GetCurrentWorkforceProfileId();
            var result = await _enrollmentService.GetMyFamilyQuotaSummaryAsync(wfpId, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Pegawai mendaftarkan pasangan sah atau anak melalui portal ESS berlampiran berkas kependudukan (BE-HRD-181, BE-HRD-183, BEN-B2, BEN-B3).
        /// </summary>
        [HttpPost("dependents/my-family")]
        [ProducesResponseType(typeof(EmployeeDependentSummaryDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
        public async Task<IActionResult> CreateEssDependent(
            [FromBody] CreateEssDependentRequestDto request,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            try
            {
                var wfpId = GetCurrentWorkforceProfileId();
                var result = await _enrollmentService.CreateEssDependentAsync(
                    wfpId, request, GetCurrentUserId(), cancellationToken);
                return StatusCode(StatusCodes.Status201Created, result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
