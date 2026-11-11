using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Services;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Controllers
{
    /// <summary>
    /// REST API Controller untuk Kualitas Rekredensialing, Kalkulator Komposit 3 Tahun, dan Sinkronisasi SIMRS EMR
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/credentialing/recredentialing")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Rekredensialing Berbasis Mutu",
        AreaName = "Corporate",
        ControllerName = "RecredentialingQuality",
        Description = "Scorecard komposit 3 tahun, override tier, putusan rekredensialing, dan kirim ke SIMRS",
        SortOrder = 13)]
    [Tags("Corporate - HRD Recredentialing Quality & Composite Scorecard")]
    public class RecredentialingQualityController : ControllerBase
    {
        private readonly IRecredentialingCompositeProvider _compositeProvider;
        private readonly ISimrsClinicalPrivilegeDispatcher _simrsDispatcher;

        public RecredentialingQualityController(
            IRecredentialingCompositeProvider compositeProvider,
            ISimrsClinicalPrivilegeDispatcher simrsDispatcher)
        {
            _compositeProvider = compositeProvider ?? throw new ArgumentNullException(nameof(compositeProvider));
            _simrsDispatcher = simrsDispatcher ?? throw new ArgumentNullException(nameof(simrsDispatcher));
        }

        #region SCORECARD & TIER CLASSIFICATION
        /// <summary>
        /// Menampilkan kartu rapor komposit 3 tahunan (25%-35%-40%) dan rekomendasi tier kelayakan (INT-HR-75)
        /// </summary>
        [HttpGet("scorecards/{staffId:guid}")]
        [AccessAction("Read", "Melihat Scorecard Rekredensialing", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RecredentialingQuality", "Read")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<RecredentialingScorecardDto>), 200)]
        public async Task<IActionResult> GetScorecard(
            [FromRoute] Guid staffId,
            CancellationToken cancellationToken = default)
        {
            var scorecard = await _compositeProvider.GetScorecardAsync(staffId, cancellationToken);
            return Ok(ApiResponse<RecredentialingScorecardDto>.Ok(scorecard, "Rapor komposit 3 tahun rekredensialing berhasil diambil."));
        }

        /// <summary>
        /// Menghitung kalkulasi komposit 3 tahun secara deterministik dari riwayat OPPE & insiden FPPE (VR-OPP-15..18)
        /// </summary>
        [HttpPost("scorecards/{staffId:guid}/calculate")]
        [AccessAction("Decide", "Memutus Rekredensialing", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("RecredentialingQuality", "Decide")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<RecredentialingScorecardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CalculateScorecard(
            [FromRoute] Guid staffId,
            [FromBody] CalculateRecredScorecardRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi parameter kalkulasi komposit gagal."));
            }

            var scorecard = await _compositeProvider.CalculateScorecardAsync(staffId, request, cancellationToken);
            return Ok(ApiResponse<RecredentialingScorecardDto>.Ok(scorecard, "Kalkulasi rapor komposit 3 tahun dan klasifikasi tier berhasil diproses."));
        }

        /// <summary>
        /// Penyesuaian diskresi tier kelayakan oleh Ketua Komite Medik dengan justifikasi tertulis (VR-OPP-19)
        /// </summary>
        [HttpPost("scorecards/{staffId:guid}/override-tier")]
        [AccessAction("Decide", "Memutus Rekredensialing", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("RecredentialingQuality", "Decide")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<RecredentialingScorecardDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> OverrideTier(
            [FromRoute] Guid staffId,
            [FromBody] OverrideRecredTierRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi formulir diskresi tier gagal."));
            }

            try
            {
                var result = await _compositeProvider.OverrideTierAsync(staffId, request, cancellationToken);
                return Ok(ApiResponse<RecredentialingScorecardDto>.Ok(result, "Diskresi penyesuaian tier kelayakan berhasil diterapkan dan dicatat ke log audit."));
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
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat penyesuaian tier: {ex.Message}"));
            }
        }
        #endregion

        #region VERDICTS & SIMRS DISPATCH
        /// <summary>
        /// Menerbitkan berita acara putusan sah sidang pleno rekredensialing (VR-OPP-20)
        /// </summary>
        [HttpPost("verdicts")]
        [AccessAction("Decide", "Memutus Rekredensialing", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("RecredentialingQuality", "Decide")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<RecredVerdictResponseDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> IssueVerdict(
            [FromBody] CreateRecredVerdictRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi formulir putusan sidang rekredensialing gagal."));
            }

            try
            {
                var verdict = await _compositeProvider.IssueVerdictAsync(request, cancellationToken);
                return StatusCode(201, ApiResponse<RecredVerdictResponseDto>.Ok(verdict, "Putusan sidang rekredensialing berhasil disahkan secara resmi oleh Komite Medik."));
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
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat penerbitan putusan: {ex.Message}"));
            }
        }

        /// <summary>
        /// Mengirimkan instruksi pembaruan wewenang Level 1-4 ke modul EMR SIMRS (INT-HR-76, VR-OPP-21, VR-OPP-22, VR-OPP-25)
        /// </summary>
        [HttpPost("verdicts/{id:guid}/dispatch-simrs")]
        [AccessAction("DispatchSimrs", "Mengirim Putusan ke SIMRS", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RecredentialingQuality", "DispatchSimrs")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<SimrsPrivilegeSyncAuditLogDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> DispatchToSimrs(
            [FromRoute] Guid id,
            [FromBody] DispatchSimrsPrivilegeRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi payload instruksi wewenang SIMRS gagal."));
            }

            try
            {
                var auditLog = await _simrsDispatcher.DispatchToSimrsAsync(id, request, cancellationToken);
                return Ok(ApiResponse<SimrsPrivilegeSyncAuditLogDto>.Ok(auditLog, "Instruksi pembaruan wewenang klinis berhasil dikirimkan ke modul SIMRS EMR."));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<string>.Fail(500, $"Terjadi kegagalan saat transmisi ke SIMRS: {ex.Message}"));
            }
        }

        /// <summary>
        /// Menampilkan log riwayat transmisi sinkronisasi wewenang ke SIMRS EMR
        /// </summary>
        [HttpGet("simrs-sync/audit-logs")]
        [AccessAction("ViewSyncAudit", "Melihat Log Sinkronisasi SIMRS", AccessType = AccessTypes.Read, SortOrder = 4)]
        [AccessPermission("RecredentialingQuality", "ViewSyncAudit")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<SimrsPrivilegeSyncAuditLogDto>>), 200)]
        public async Task<IActionResult> GetSyncAuditLogs(CancellationToken cancellationToken = default)
        {
            var logs = await _simrsDispatcher.GetSyncAuditLogsAsync(cancellationToken);
            return Ok(ApiResponse<List<SimrsPrivilegeSyncAuditLogDto>>.Ok(logs, "Log riwayat transmisi sinkronisasi wewenang SIMRS berhasil diambil."));
        }
        #endregion
    }
}
