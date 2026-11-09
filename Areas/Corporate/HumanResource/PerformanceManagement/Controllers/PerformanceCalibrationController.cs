using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Sidang Kalibrasi Manajemen & Resolusi Sengketa Nilai (Kontrak v21 - Tag 27.3)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/performance")]
    [Tags("Corporate - HRD Performance Calibration & Dispute Resolution")]
    public class PerformanceCalibrationController : ControllerBase
    {
        private readonly IPerformanceCalibrationService _calibrationService;
        private readonly IPerformanceDisputeService _disputeService;

        public PerformanceCalibrationController(
            IPerformanceCalibrationService calibrationService,
            IPerformanceDisputeService disputeService)
        {
            _calibrationService = calibrationService ?? throw new ArgumentNullException(nameof(calibrationService));
            _disputeService = disputeService ?? throw new ArgumentNullException(nameof(disputeService));
        }

        #region CALIBRATION
        /// <summary>
        /// Menampilkan sebaran kurva nilai aktual RS terhadap pedoman distribusi fleksibel (10-15% A, 70-80% B, 5-10% C/D) (PRF-B6)
        /// </summary>
        [HttpGet("calibration/distribution-matrix")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CalibrationDistributionMatrixDto>), 200)]
        public async Task<IActionResult> GetDistributionMatrix(
            [FromQuery] Guid cycleId,
            CancellationToken cancellationToken = default)
        {
            var result = await _calibrationService.GetDistributionMatrixAsync(cycleId, cancellationToken);
            return Ok(ApiResponse<CalibrationDistributionMatrixDto>.Ok(result, "Matriks distribusi kurva nilai kalibrasi berhasil diambil."));
        }

        /// <summary>
        /// Menjadwalkan dan membuka sidang pleno kalibrasi manajemen rumah sakit
        /// </summary>
        [HttpPost("calibration/sessions")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<CalibrationSessionDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateCalibrationSession(
            [FromBody] CreateCalibrationSessionRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _calibrationService.CreateSessionAsync(dto, Guid.Empty, cancellationToken);
                return StatusCode(201, ApiResponse<CalibrationSessionDto>.Ok(result, "Sidang pleno kalibrasi berhasil dijadwalkan."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Penyesuaian skor kinerja pegawai oleh Komite Kalibrasi dengan kewajiban nota pertimbangan tertulis min 50 karakter (VR-PRF-05, VR-PRF-12)
        /// </summary>
        [HttpPost("calibration/override-score")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> OverrideScore(
            [FromBody] OverrideCalibrationScoreRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _calibrationService.OverrideScoreAsync(dto, Guid.Empty, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(result, "Penyesuaian skor hasil kalibrasi berhasil disimpan dengan nota justifikasi komite."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Pengesahan final hasil kalibrasi nilai RS dan instruksi rilis massal ke portal ESS pegawai
        /// </summary>
        [HttpPost("calibration/sessions/{id}/finalize")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> FinalizeSession(
            [FromRoute] Guid id,
            [FromBody] FinalizeCalibrationSessionRequestDto dto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _calibrationService.FinalizeSessionAsync(id, dto, Guid.Empty, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(result, "Sidang pleno kalibrasi disahkan; nilai resmi dirilis serentak ke portal ESS."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region DISPUTES
        /// <summary>
        /// Pengajuan sanggah banding nilai kinerja resmi oleh pegawai (maksimal H+7 pasca rilis nilai, VR-PRF-12, HRD-DEC-169)
        /// </summary>
        [HttpPost("disputes/submit")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AppraisalDisputeDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitDispute(
            [FromQuery] Guid? staffId = null,
            [FromBody] SubmitAppraisalDisputeRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sId = staffId ?? Guid.Empty;
                var result = await _disputeService.SubmitDisputeAsync(dto, sId, cancellationToken);
                return StatusCode(201, ApiResponse<AppraisalDisputeDto>.Ok(result, "Permohonan sanggah banding nilai resmi berhasil diregistrasi."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menerbitkan putusan final majelis banding Direktur SDM (maksimal H+14 kalender, VR-PRF-13)
        /// </summary>
        [HttpPost("disputes/{id}/adjudicate")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AppraisalDisputeDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> AdjudicateDispute(
            [FromRoute] Guid id,
            [FromQuery] Guid? directorId = null,
            [FromBody] AdjudicateDisputeRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var dId = directorId ?? Guid.Empty;
                var result = await _disputeService.AdjudicateDisputeAsync(id, dto, dId, cancellationToken);
                return Ok(ApiResponse<AppraisalDisputeDto>.Ok(result, "Putusan majelis sanggah banding Direktur SDM berhasil disahkan."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion
    }
}
