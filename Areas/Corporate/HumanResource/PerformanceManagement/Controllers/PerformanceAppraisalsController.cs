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
    /// REST API Controller untuk Evaluasi Kinerja Pegawai & Penilaian Berjenjang Tiga Tingkat (Kontrak v21 - Tag 27.2)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/performance/appraisals")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Penilaian Kinerja Pegawai",
        AreaName = "Corporate",
        ControllerName = "PerformanceAppraisal",
        Description = "Evaluasi mandiri, penilaian atasan, review lini kedua, dan konfirmasi hasil",
        SortOrder = 14)]
    [Tags("Corporate - HRD Performance Appraisals & Multi-Level Review")]
    public class PerformanceAppraisalsController : ControllerBase
    {
        private readonly IPerformanceAppraisalService _appraisalService;

        public PerformanceAppraisalsController(IPerformanceAppraisalService appraisalService)
        {
            _appraisalService = appraisalService ?? throw new ArgumentNullException(nameof(appraisalService));
        }

        /// <summary>
        /// Menampilkan lembar kerja evaluasi kinerja mandiri pegawai yang sedang login (ESS)
        /// </summary>
        [HttpGet("my-appraisal")]
        [AccessAction("ViewOwn", "Melihat Penilaian Kinerja Sendiri", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceAppraisal", "ViewOwn")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        public async Task<IActionResult> GetMyAppraisal(
            [FromQuery] Guid? workforceProfileId = null,
            [FromQuery] Guid? cycleId = null,
            CancellationToken cancellationToken = default)
        {
            var staffId = workforceProfileId ?? Guid.Empty;
            var cId = cycleId ?? Guid.Empty;
            var result = await _appraisalService.GetMyAppraisalAsync(staffId, cId, cancellationToken);
            return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Lembar kerja evaluasi mandiri berhasil diambil."));
        }

        /// <summary>
        /// Menyerahkan lembar refleksi diri evaluasi mandiri beserta unggahan dokumen bukti capaian (VR-PRF-22)
        /// </summary>
        [HttpPost("self-submit")]
        [AccessAction("SelfAssess", "Mengisi Evaluasi Mandiri", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PerformanceAppraisal", "SelfAssess")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitSelfAppraisal(
            [FromQuery] Guid? workforceProfileId = null,
            [FromBody] SubmitSelfAppraisalRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var staffId = workforceProfileId ?? Guid.Empty;
                var result = await _appraisalService.SubmitSelfAppraisalAsync(staffId, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Evaluasi mandiri pegawai berhasil diserahkan ke atasan langsung."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Menampilkan daftar lembar evaluasi staf bawahan yang menunggu penilaian atasan langsung (Karu)
        /// </summary>
        [HttpGet("subordinates")]
        [AccessAction("ViewSubordinates", "Melihat Penilaian Kinerja Bawahan", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("PerformanceAppraisal", "ViewSubordinates")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PerformanceAppraisalDto>>), 200)]
        public async Task<IActionResult> GetSubordinateAppraisals(
            [FromQuery] Guid? managerId = null,
            [FromQuery] Guid? cycleId = null,
            CancellationToken cancellationToken = default)
        {
            var mId = managerId ?? Guid.Empty;
            var cId = cycleId ?? Guid.Empty;
            var result = await _appraisalService.GetSubordinateAppraisalsAsync(mId, cId, cancellationToken);
            return Ok(ApiResponse<List<PerformanceAppraisalDto>>.Ok(result, "Daftar lembar evaluasi staf bawahan berhasil diambil."));
        }

        /// <summary>
        /// Menginput skor resmi 60% KPI hasil kerja dan 40% perilaku Core Values BerAKHLAK oleh Karu (VR-PRF-03, VR-PRF-04, PRF-B4)
        /// </summary>
        [HttpPost("{id}/manager-assess")]
        [AccessAction("ManagerAssess", "Menilai Kinerja Bawahan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceAppraisal", "ManagerAssess")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> SubmitManagerAppraisal(
            [FromRoute] Guid id,
            [FromQuery] Guid? managerId = null,
            [FromBody] SubmitManagerAppraisalRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var mId = managerId ?? Guid.Empty;
                var result = await _appraisalService.SubmitManagerAppraisalAsync(id, mId, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Penilaian kinerja atasan berhasil disimpan dengan formula 60:40."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Penelaahan dan otorisasi berkas evaluasi oleh atasan kedua (Kabid) sebelum kalibrasi (VR-PRF-04, PRF-B3)
        /// </summary>
        [HttpPost("{id}/second-line-review")]
        [AccessAction("SecondLineReview", "Mereview Penilaian (Lini Kedua)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("PerformanceAppraisal", "SecondLineReview")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> ProcessSecondLineReview(
            [FromRoute] Guid id,
            [FromQuery] Guid? reviewerId = null,
            [FromBody] ProcessSecondLineReviewDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var rId = reviewerId ?? Guid.Empty;
                var result = await _appraisalService.ProcessSecondLineReviewAsync(id, rId, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Penelaahan berkas evaluasi atasan kedua berhasil diproses."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }

        /// <summary>
        /// Konfirmasi penerimaan dan penandatanganan skor akhir resmi oleh pegawai via portal ESS
        /// </summary>
        [HttpPost("{id}/acknowledge")]
        [AccessAction("Acknowledge", "Mengonfirmasi Hasil Penilaian", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PerformanceAppraisal", "Acknowledge")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceAppraisalDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> AcknowledgeAppraisal(
            [FromRoute] Guid id,
            [FromQuery] Guid? staffId = null,
            [FromBody] AcknowledgeAppraisalRequestDto dto = null!,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var sId = staffId ?? Guid.Empty;
                var result = await _appraisalService.AcknowledgeAppraisalAsync(id, sId, dto, cancellationToken);
                return Ok(ApiResponse<PerformanceAppraisalDto>.Ok(result, "Penerimaan hasil evaluasi kinerja berhasil dikonfirmasi pegawai."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
    }
}
