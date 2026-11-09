using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Controllers
{
    /// <summary>
    /// REST API Controller untuk Pelatihan Wajib Akreditasi RS STARKES KPS 8 & Radar Kedaluwarsa Bertingkat
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/learning")]
    [Tags("Corporate - HRD Mandatory Hospital Training & Certification")]
    public class MandatoryHospitalTrainingController : ControllerBase
    {
        private readonly IMandatoryTrainingRuleService _ruleService;
        private readonly ITrainingExpiryRadarService _radarService;

        public MandatoryHospitalTrainingController(
            IMandatoryTrainingRuleService ruleService,
            ITrainingExpiryRadarService radarService)
        {
            _ruleService = ruleService ?? throw new ArgumentNullException(nameof(ruleService));
            _radarService = radarService ?? throw new ArgumentNullException(nameof(radarService));
        }

        #region MANDATORY TRAINING RULES (KPS 8)
        /// <summary>
        /// Menampilkan daftar katalog master pelatihan wajib akreditasi STARKES KPS 8 & masa berlaku (VR-TRN-01)
        /// </summary>
        [HttpGet("mandatory-rules")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<MandatoryTrainingRuleDto>>), 200)]
        public async Task<IActionResult> GetMandatoryRules(CancellationToken cancellationToken = default)
        {
            var rules = await _ruleService.GetMandatoryRulesAsync(cancellationToken);
            return Ok(ApiResponse<List<MandatoryTrainingRuleDto>>.Ok(rules, "Katalog aturan pelatihan wajib akreditasi STARKES berhasil diambil."));
        }

        /// <summary>
        /// Mendaftarkan aturan pelatihan wajib baru atau memperbarui masa berlaku/ambang kelulusan (VR-TRN-02)
        /// </summary>
        [HttpPost("mandatory-rules")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<MandatoryTrainingRuleDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateOrUpdateRule(
            [FromBody] CreateMandatoryTrainingRuleRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi formulir aturan pelatihan gagal."));
            }

            try
            {
                var rule = await _ruleService.CreateOrUpdateRuleAsync(request, cancellationToken);
                return StatusCode(201, ApiResponse<MandatoryTrainingRuleDto>.Ok(rule, "Aturan pelatihan wajib akreditasi berhasil disimpan."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat menyimpan aturan pelatihan: {ex.Message}"));
            }
        }
        #endregion

        #region EXPIRING RADAR (H-90, H-60, H-30, OVERDUE)
        /// <summary>
        /// Menampilkan dashboard radar staf berisiko kedaluwarsa pelatihan wajib (H-90, H-60, H-30, Overdue H-0) (VR-TRN-09)
        /// </summary>
        [HttpGet("certificates/expiring-radar")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CertificateExpiryRadarSummaryDto>), 200)]
        public async Task<IActionResult> GetExpiringRadar(CancellationToken cancellationToken = default)
        {
            var summary = await _radarService.GetExpiringRadarSummaryAsync(cancellationToken);
            return Ok(ApiResponse<CertificateExpiryRadarSummaryDto>.Ok(summary, "Data radar kedaluwarsa pelatihan wajib berhasil diambil."));
        }

        /// <summary>
        /// Mengirimkan notifikasi peringatan perpanjangan diklat massal via WhatsApp/Email/ESS ke staf (VR-TRN-09)
        /// </summary>
        [HttpPost("certificates/expiring-radar/broadcast-alerts")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<BroadcastExpiryAlertResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> BroadcastAlerts(
            [FromBody] BroadcastExpiryAlertRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi parameter broadcast peringatan gagal."));
            }

            var result = await _radarService.BroadcastAlertsAsync(request, cancellationToken);
            return Ok(ApiResponse<BroadcastExpiryAlertResponseDto>.Ok(result, "Penyiaran notifikasi peringatan kedaluwarsa berhasil diproses."));
        }
        #endregion

        #region SHIFT BUDDY SAFEGUARD & PORTFOLIO
        /// <summary>
        /// Menandai flag Overdue dan menginstruksikan penugasan pendamping (buddy system) ke Sub-06 & pembekuan RKK ke Sub-14 (VR-TRN-10..12)
        /// </summary>
        [HttpPost("certificates/overdue/{staffId:guid}/shift-buddy-safeguard")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<ShiftBuddySafeguardResponseDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> EnforceShiftBuddySafeguard(
            [FromRoute] Guid staffId,
            [FromBody] AssignShiftBuddySafeguardRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi formulir pengamanan shift dinas gagal."));
            }

            try
            {
                var result = await _radarService.EnforceShiftBuddySafeguardAsync(staffId, request, cancellationToken);
                return Ok(ApiResponse<ShiftBuddySafeguardResponseDto>.Ok(result, "Pengamanan Shift Buddy System berhasil diaktifkan."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat pengamanan shift dinas: {ex.Message}"));
            }
        }

        /// <summary>
        /// Menampilkan seluruh portofolio sertifikat kompetensi dan status keaktifan milik staf
        /// </summary>
        [HttpGet("certificates/staff/{staffId:guid}")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<StaffCertificatePortfolioDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        public async Task<IActionResult> GetStaffPortfolio(
            [FromRoute] Guid staffId,
            CancellationToken cancellationToken = default)
        {
            var portfolio = await _radarService.GetStaffPortfolioAsync(staffId, cancellationToken);
            if (portfolio == null)
            {
                return NotFound(ApiResponse<string>.Fail(404, $"Profil pegawai dengan ID {staffId} tidak ditemukan."));
            }

            return Ok(ApiResponse<StaffCertificatePortfolioDto>.Ok(portfolio, "Portofolio sertifikat kompetensi pegawai berhasil diambil."));
        }
        #endregion
    }
}
