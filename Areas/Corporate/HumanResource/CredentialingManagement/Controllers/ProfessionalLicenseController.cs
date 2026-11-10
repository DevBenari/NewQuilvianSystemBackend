using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using System.Security.Claims;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/credentialing/licenses")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING",
        moduleName: "Human Resource Credentialing",
        displayName: "Lisensi Profesi (STR/SIP)",
        AreaName = "Corporate",
        ControllerName = "ProfessionalLicense",
        Description = "Registrasi dan perpanjangan STR/SIP, verifikasi sumber primer, dan radar kedaluwarsa",
        SortOrder = 11)]
    [Tags("HR Credentialing - Professional Licenses & PSV Radar")]
    [Authorize]
    public class ProfessionalLicenseController : ControllerBase
    {
        private readonly ICredentialVerificationService _verificationService;
        private readonly ILogger<ProfessionalLicenseController> _logger;

        public ProfessionalLicenseController(
            ICredentialVerificationService verificationService,
            ILogger<ProfessionalLicenseController> logger)
        {
            _verificationService = verificationService;
            _logger = logger;
        }

        private Guid GetUserId()
        {
            var rawId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? User.FindFirst("sub")?.Value
                     ?? User.FindFirst("id")?.Value;

            return Guid.TryParse(rawId, out var id) ? id : Guid.Empty;
        }

        /// <summary>
        /// Mendaftarkan lisensi nakes baru (STR Seumur Hidup / SIP 5 Tahun Faskes 1-3) (BE-HRD-210, VR-CRD-01)
        /// </summary>
        [HttpPost]
        [AccessAction("Create", "Mendaftarkan Lisensi Profesi", AccessType = AccessTypes.Create, SortOrder = 1)]
        [AccessPermission("ProfessionalLicense", "Create")]
        [ProducesResponseType(typeof(ProfessionalLicenseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RegisterLicense(
            [FromBody] RegisterProfessionalLicenseDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _verificationService.RegisterLicenseAsync(request, GetUserId(), cancellationToken);
                return CreatedAtAction(nameof(GetLicenseDetail), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal mendaftarkan lisensi nakes.");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Terjadi kesalahan internal server.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil rincian data lisensi profesional beserta status radar kedaluwarsa
        /// </summary>
        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Melihat Lisensi Profesi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("ProfessionalLicense", "Read")]
        [ProducesResponseType(typeof(ProfessionalLicenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLicenseDetail(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _verificationService.GetLicenseDetailAsync(id, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil seluruh lisensi profesi milik satu tenaga kerja spesifik
        /// </summary>
        [HttpGet("employee/{employeeId:guid}")]
        [AccessAction("Read", "Melihat Lisensi Profesi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("ProfessionalLicense", "Read")]
        [ProducesResponseType(typeof(List<ProfessionalLicenseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetLicensesByEmployee(
            [FromRoute] Guid employeeId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _verificationService.GetLicensesByEmployeeAsync(employeeId, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Melakukan audit Verifikasi Sumber Primer (PSV) ke portal KKI / SatuSehat SDMK Kemenkes (BE-HRD-208, VR-CRD-02)
        /// </summary>
        [HttpPost("{id:guid}/verify-psv")]
        [AccessAction("VerifyPsv", "Memverifikasi Sumber Primer", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ProfessionalLicense", "VerifyPsv")]
        [ProducesResponseType(typeof(PsvVerificationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> VerifyPsv(
            [FromRoute] Guid id,
            [FromBody] PsvVerificationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (id != request.LicenseId)
            {
                return BadRequest(new { message = "ID lisensi pada URL tidak sesuai dengan body payload." });
            }

            try
            {
                var result = await _verificationService.PerformPrimarySourceVerificationAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gagal melakukan verifikasi sumber primer untuk lisensi {LicenseId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Terjadi kesalahan internal audit PSV.", detail = ex.Message });
            }
        }

        /// <summary>
        /// Mengajukan perpanjangan lisensi SIP 5 tahun baru (BE-HRD-208)
        /// </summary>
        [HttpPost("{id:guid}/renew")]
        [AccessAction("Renew", "Memperpanjang Lisensi Profesi", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ProfessionalLicense", "Renew")]
        [ProducesResponseType(typeof(ProfessionalLicenseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RenewLicense(
            [FromRoute] Guid id,
            [FromBody] RenewProfessionalLicenseDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _verificationService.RenewLicenseAsync(id, request, GetUserId(), cancellationToken);
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
        /// Mengambil radar pemantauan lisensi yang mendekati kedaluwarsa (H-90, H-60, H-30, H-7, ExpiredH0) (BE-HRD-209, CRD-B2)
        /// </summary>
        [HttpGet("radar/expiring")]
        [AccessAction("Read", "Melihat Lisensi Profesi", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("ProfessionalLicense", "Read")]
        [ProducesResponseType(typeof(List<LicenseRadarAlertDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExpiringRadarList(
            [FromQuery] string? radarLevel = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _verificationService.GetLicenseExpiringRadarListAsync(radarLevel, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Pemicu on-demand eksekusi Fail-Closed untuk lisensi yang kedaluwarsa (H-0) (INT-HR-69, INT-HR-71)
        /// </summary>
        [HttpPost("radar/execute-fail-closed")]
        [AccessAction("ExecuteFailClosed", "Menjalankan Penguncian Lisensi Kedaluwarsa", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("ProfessionalLicense", "ExecuteFailClosed")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> ExecuteFailClosed(
            CancellationToken cancellationToken = default)
        {
            var affectedCount = await _verificationService.ExecuteFailClosedForExpiredLicensesAsync(cancellationToken);
            return Ok(new
            {
                message = $"Tindakan Fail-Closed selesai dieksekusi. Total lisensi diproses: {affectedCount}",
                affectedLicensesCount = affectedCount,
                executedAt = DateTime.UtcNow
            });
        }
    }
}
