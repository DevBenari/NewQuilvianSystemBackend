using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Services;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Route("api/v1/corporate/hrd/clinical-privileges")]
    [Tags("HR Credentialing - Real-Time SIMRS Gate & Emergency Override")]
    [Authorize]
    public class ClinicalPrivilegeGateController : ControllerBase
    {
        private readonly IClinicalPrivilegeGateService _gateService;
        private readonly ILogger<ClinicalPrivilegeGateController> _logger;

        public ClinicalPrivilegeGateController(
            IClinicalPrivilegeGateService gateService,
            ILogger<ClinicalPrivilegeGateController> logger)
        {
            _gateService = gateService;
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
        /// Validasi gerbang wewenang klinis real-time SIMRS EMR (BE-HRD-216, INT-HR-69, HRD-DEC-005)
        /// Memvalidasi status SIP aktif, SPK 3 tahun, dan derajat supervisi Level 1 Mandiri s.d. Level 4 Hard-Stop
        /// </summary>
        [HttpPost("validate-gate")]
        [ProducesResponseType(typeof(ClinicalPrivilegeGateValidationResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ValidateGate(
            [FromBody] ClinicalPrivilegeGateValidationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _gateService.ValidateClinicalGateAsync(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Persetujuan digital (Co-Signature) oleh dokter supervisor dalam tempo 24 jam (BE-HRD-217, VR-CRD-07)
        /// </summary>
        [HttpPost("co-sign")]
        [ProducesResponseType(typeof(CoSignatureResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CoSign(
            [FromBody] CoSignatureActionDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _gateService.ProcessCoSignatureAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Perekaman bypass darurat penyelamatan nyawa (Life-Saving Emergency Override) ke KMKP (BE-HRD-218, INT-HR-70)
        /// </summary>
        [HttpPost("emergency-override")]
        [ProducesResponseType(typeof(EmergencyOverrideLogDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RecordEmergencyOverride(
            [FromBody] ClinicalEmergencyOverrideRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _gateService.RecordLifeSavingOverrideAsync(request, GetUserId(), cancellationToken);
                return CreatedAtAction(nameof(GetEmergencyOverrideLogs), new { id = result.Id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Audit dan putusan Komite Mutu & Keselamatan Pasien (KMKP) atas tindakan Life-Saving Override (BE-HRD-218)
        /// </summary>
        [HttpPost("emergency-overrides/review")]
        [ProducesResponseType(typeof(EmergencyOverrideLogDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReviewEmergencyOverride(
            [FromBody] ReviewEmergencyOverrideDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _gateService.ReviewEmergencyOverrideByKmkpAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Mengambil daftar seluruh log audit bypass darurat penyelamatan nyawa (Life-Saving Override) untuk KMKP
        /// </summary>
        [HttpGet("emergency-overrides")]
        [ProducesResponseType(typeof(List<EmergencyOverrideLogDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEmergencyOverrideLogs(
            [FromQuery] bool? pendingOnly = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _gateService.GetEmergencyOverrideLogsAsync(pendingOnly, cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Otorisasi fast-track kewenangan klinis sementara (Temporary Privilege) dokter tamu / darurat bencana (BE-HRD-219, HRD-DEC-150)
        /// </summary>
        [HttpPost("temporary-privilege")]
        [ProducesResponseType(typeof(TemporaryPrivilegeResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GrantTemporaryPrivilege(
            [FromBody] GrantTemporaryPrivilegeDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var result = await _gateService.GrantTemporaryPrivilegeAsync(request, GetUserId(), cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
