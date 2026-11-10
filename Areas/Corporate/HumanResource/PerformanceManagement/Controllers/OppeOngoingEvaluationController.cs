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
    /// REST API Controller untuk Evaluasi Praktik Profesional Berkelanjutan (OPPE)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/clinical-performance/oppe")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Evaluasi Berkelanjutan OPPE",
        AreaName = "Corporate",
        ControllerName = "OppeOngoingEvaluation",
        Description = "Periode dan indikator OPPE, rapor dokter, finalisasi, dan penilaian sejawat",
        SortOrder = 12)]
    [Tags("Corporate - HRD OPPE Clinical Performance Evaluation")]
    public class OppeOngoingEvaluationController : ControllerBase
    {
        private readonly IOppeDataAggregationService _oppeService;

        public OppeOngoingEvaluationController(IOppeDataAggregationService oppeService)
        {
            _oppeService = oppeService ?? throw new ArgumentNullException(nameof(oppeService));
        }

        #region METRIC CATALOGS
        /// <summary>
        /// Mengambil master katalog indikator kinerja klinis OPPE 6 pilar ACGME/KARS
        /// </summary>
        [HttpGet("metric-catalogs")]
        [AccessAction("ViewPeriod", "Melihat Periode & Indikator OPPE", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OppeOngoingEvaluation", "ViewPeriod")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<OppeMetricCatalogDto>>), 200)]
        public async Task<IActionResult> GetMetricCatalogs(
            [FromQuery] string? specialtyGroup = null,
            [FromQuery] string? competencyDomain = null,
            CancellationToken cancellationToken = default)
        {
            var list = await _oppeService.GetMetricCatalogsAsync(specialtyGroup, competencyDomain, cancellationToken);
            return Ok(ApiResponse<List<OppeMetricCatalogDto>>.Ok(list, "Katalog indikator kinerja klinis OPPE berhasil diambil."));
        }

        /// <summary>
        /// Mendaftarkan indikator performa klinis baru atau mengatur ambang batas KSM
        /// </summary>
        [HttpPost("metric-catalogs")]
        [AccessAction("ManagePeriod", "Mengelola Periode & Indikator OPPE", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OppeOngoingEvaluation", "ManagePeriod")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OppeMetricCatalogDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateMetricCatalog(
            [FromBody] CreateOppeMetricCatalogRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi data input gagal."));
            }

            try
            {
                var creatorId = Guid.Empty;
                var result = await _oppeService.CreateMetricCatalogAsync(request, creatorId, cancellationToken);
                return CreatedAtAction(nameof(GetMetricCatalogs), ApiResponse<OppeMetricCatalogDto>.Ok(result, "Indikator mutu klinis OPPE berhasil didaftarkan."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region EVALUATION PERIODS
        /// <summary>
        /// Menampilkan daftar siklus periode evaluasi kinerja klinis OPPE berkala
        /// </summary>
        [HttpGet("periods")]
        [AccessAction("ViewPeriod", "Melihat Periode & Indikator OPPE", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OppeOngoingEvaluation", "ViewPeriod")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<OppeEvaluationPeriodDto>>), 200)]
        public async Task<IActionResult> GetPeriods(
            [FromQuery] int? year = null,
            CancellationToken cancellationToken = default)
        {
            var list = await _oppeService.GetEvaluationPeriodsAsync(year, cancellationToken);
            return Ok(ApiResponse<List<OppeEvaluationPeriodDto>>.Ok(list, "Daftar periode evaluasi OPPE berhasil diambil."));
        }

        /// <summary>
        /// Membuka siklus periode evaluasi OPPE baru dengan validasi rasio 60:40 (VR-OPP-01)
        /// </summary>
        [HttpPost("periods")]
        [AccessAction("ManagePeriod", "Mengelola Periode & Indikator OPPE", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OppeOngoingEvaluation", "ManagePeriod")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OppeEvaluationPeriodDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreatePeriod(
            [FromBody] CreateOppeEvaluationPeriodRequestDto request,
            CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<string>.Fail(400, "Validasi data input gagal."));
            }

            try
            {
                var creatorId = Guid.Empty;
                var result = await _oppeService.CreateEvaluationPeriodAsync(request, creatorId, cancellationToken);
                return StatusCode(201, ApiResponse<OppeEvaluationPeriodDto>.Ok(result, "Periode evaluasi kinerja klinis OPPE berhasil dibuka."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menginisiasi lembar draf evaluasi rapor OPPE untuk seluruh nakes pada periode aktif
        /// </summary>
        [HttpPost("periods/{id}/initiate-staff")]
        [AccessAction("ManagePeriod", "Mengelola Periode & Indikator OPPE", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("OppeOngoingEvaluation", "ManagePeriod")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<int>), 200)]
        public async Task<IActionResult> InitiateStaff(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var count = await _oppeService.InitiateStaffEvaluationsForPeriodAsync(id, Guid.Empty, cancellationToken);
                return Ok(ApiResponse<int>.Ok(count, $"Berhasil menginisiasi draf rapor OPPE untuk {count} orang nakes."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region STAFF EVALUATION & SIMRS AGGREGATION
        /// <summary>
        /// Mengambil rincian lembar rapor OPPE staf dan skor per indikator 6 pilar
        /// </summary>
        [HttpGet("staff-evaluations/{id}")]
        [AccessAction("ViewEvaluation", "Melihat Rapor OPPE", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("OppeOngoingEvaluation", "ViewEvaluation")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OppeStaffEvaluationDto>), 200)]
        public async Task<IActionResult> GetStaffEvaluation(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _oppeService.GetStaffEvaluationByIdAsync(id, cancellationToken);
                return Ok(ApiResponse<OppeStaffEvaluationDto>.Ok(result, "Lembar evaluasi OPPE staf berhasil diambil."));
            }
            catch (Exception ex)
            {
                return NotFound(ApiResponse<string>.Fail(404, ex.Message));
            }
        }

        /// <summary>
        /// Menjalankan agregasi otomatis penarikan data indikator kuantitatif SIMRS/KMKP 60%
        /// </summary>
        [HttpPost("staff-evaluations/{id}/aggregate-simrs")]
        [AccessAction("CalculateEvaluation", "Menghitung Agregasi Nilai OPPE", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OppeOngoingEvaluation", "CalculateEvaluation")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OppeStaffEvaluationDto>), 200)]
        public async Task<IActionResult> AggregateSimrs(
            [FromRoute] Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _oppeService.AggregateSimrsQuantitativeMetricsAsync(id, Guid.Empty, cancellationToken);
                return Ok(ApiResponse<OppeStaffEvaluationDto>.Ok(result, "Agregasi data indikator kuantitatif SIMRS/KMKP (bobot 60%) berhasil dihitung."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Pengesahan final rapor OPPE tahunan oleh Kepala KSM
        /// </summary>
        [HttpPost("staff-evaluations/{id}/finalize")]
        [AccessAction("FinalizeEvaluation", "Mengesahkan Rapor OPPE", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("OppeOngoingEvaluation", "FinalizeEvaluation")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OppeStaffEvaluationDto>), 200)]
        public async Task<IActionResult> FinalizeEvaluation(
            [FromRoute] Guid id,
            [FromBody] FinalizeOppeStaffEvaluationDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _oppeService.FinalizeStaffEvaluationAsync(id, request, cancellationToken);
                return Ok(ApiResponse<OppeStaffEvaluationDto>.Ok(result, "Rapor evaluasi kinerja OPPE berhasil disahkan oleh Kepala KSM."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Konfirmasi penerimaan dan pengakuan rapor OPPE oleh nakes via portal ESS
        /// </summary>
        [HttpPost("staff-evaluations/{id}/acknowledge")]
        [AccessAction("AcknowledgeEvaluation", "Mengonfirmasi Rapor OPPE", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("OppeOngoingEvaluation", "AcknowledgeEvaluation")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<OppeStaffEvaluationDto>), 200)]
        public async Task<IActionResult> AcknowledgeEvaluation(
            [FromRoute] Guid id,
            [FromBody] AcknowledgeOppeStaffEvaluationDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _oppeService.AcknowledgeStaffEvaluationAsync(id, request, cancellationToken);
                return Ok(ApiResponse<OppeStaffEvaluationDto>.Ok(result, "Konfirmasi penerimaan rapor OPPE berhasil dicatat di sistem."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region PEER REVIEWS
        /// <summary>
        /// Mengambil daftar berkas telaah sejawat kualitatif yang ditugaskan ke dokter evaluator
        /// </summary>
        [HttpGet("peer-reviews/assigned/{evaluatorId}")]
        [AccessAction("AssessPeerReview", "Mengisi Penilaian Sejawat", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("OppeOngoingEvaluation", "AssessPeerReview")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<AssignedPeerReviewDto>>), 200)]
        public async Task<IActionResult> GetAssignedPeerReviews(
            [FromRoute] Guid evaluatorId,
            CancellationToken cancellationToken = default)
        {
            var list = await _oppeService.GetAssignedPeerReviewsAsync(evaluatorId, cancellationToken);
            return Ok(ApiResponse<List<AssignedPeerReviewDto>>.Ok(list, "Daftar tugas telaah sejawat berhasil diambil."));
        }

        /// <summary>
        /// Menyerahkan formulir skor Likert dan telaah kualitatif telaah sejawat (Anti-Self-Review VR-OPP-03)
        /// </summary>
        [HttpPost("peer-reviews/{id}/submit")]
        [AccessAction("AssessPeerReview", "Mengisi Penilaian Sejawat", AccessType = AccessTypes.Update, SortOrder = 7)]
        [AccessPermission("OppeOngoingEvaluation", "AssessPeerReview")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitPeerReview(
            [FromRoute] Guid id,
            [FromBody] SubmitPeerReviewEvaluationRequestDto request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var success = await _oppeService.SubmitPeerReviewScoresAsync(id, request, cancellationToken);
                return Ok(ApiResponse<bool>.Ok(success, "Penilaian telaah sejawat kualitatif (bobot 40%) berhasil disimpan."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion
    }
}
