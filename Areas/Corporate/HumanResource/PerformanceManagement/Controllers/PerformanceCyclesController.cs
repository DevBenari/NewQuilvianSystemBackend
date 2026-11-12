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
    /// REST API Controller untuk Siklus Kinerja & Penurunan Target Sasaran Mutu (Kontrak v21 - Tag 27.1)
    /// </summary>
    [ApiController]
    [Route("api/v1/corporate/hrd/performance")]
    [Authorize]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PERFORMANCE",
        moduleName: "Human Resource Performance",
        displayName: "Siklus, KPI & Sasaran Kinerja",
        AreaName = "Corporate",
        ControllerName = "PerformanceCycleGoal",
        Description = "Siklus penilaian, katalog KPI, dan kaskade sasaran kerja individu",
        SortOrder = 16)]
    [Tags("Corporate - HRD Performance Cycles & Goal Cascading")]
    public class PerformanceCyclesController : ControllerBase
    {
        private readonly IPerformanceCycleService _cycleService;
        private readonly IPerformanceGoalService _goalService;

        public PerformanceCyclesController(
            IPerformanceCycleService cycleService,
            IPerformanceGoalService goalService)
        {
            _cycleService = cycleService ?? throw new ArgumentNullException(nameof(cycleService));
            _goalService = goalService ?? throw new ArgumentNullException(nameof(goalService));
        }

        #region CYCLES
        /// <summary>
        /// Menampilkan daftar riwayat siklus evaluasi kinerja tahunan dan jadwal tenggat waktu (VR-PRF-01)
        /// </summary>
        [HttpGet("cycles")]
        [AccessAction("ViewCycle", "Melihat Siklus Kinerja", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PerformanceCycleGoal", "ViewCycle")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PerformanceCycleDto>>), 200)]
        public async Task<IActionResult> GetCycles(CancellationToken cancellationToken = default)
        {
            var result = await _cycleService.GetCyclesAsync(cancellationToken);
            return Ok(ApiResponse<List<PerformanceCycleDto>>.Ok(result, "Daftar siklus evaluasi kinerja berhasil diambil."));
        }

        /// <summary>
        /// Membuat dan membuka siklus penilaian kinerja baru (Tahunan / Semesteran) (VR-PRF-01)
        /// </summary>
        [HttpPost("cycles")]
        [AccessAction("ManageCycle", "Mengelola Siklus Kinerja", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("PerformanceCycleGoal", "ManageCycle")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<PerformanceCycleDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 409)]
        public async Task<IActionResult> CreateCycle([FromBody] CreatePerformanceCycleRequestDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _cycleService.CreateCycleAsync(dto, Guid.Empty, cancellationToken);
                return StatusCode(201, ApiResponse<PerformanceCycleDto>.Ok(result, "Siklus evaluasi kinerja berhasil dibuka."));
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(409, ApiResponse<string>.Fail(409, ex.Message));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region KPI CATALOGS
        /// <summary>
        /// Menampilkan master katalog metrik target KPI kuantitatif dan rubrik deskriptor Core Values BerAKHLAK
        /// </summary>
        [HttpGet("kpi-catalogs")]
        [AccessAction("ViewMetric", "Melihat Katalog KPI", AccessType = AccessTypes.Read, SortOrder = 3)]
        [AccessPermission("PerformanceCycleGoal", "ViewMetric")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<KpiMetricCatalogDto>>), 200)]
        public async Task<IActionResult> GetKpiCatalogs(
            [FromQuery] string? category = null,
            [FromQuery] string? targetProfession = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _cycleService.GetKpiCatalogsAsync(category, targetProfession, cancellationToken);
            return Ok(ApiResponse<List<KpiMetricCatalogDto>>.Ok(result, "Katalog indikator KPI dan Core Values BerAKHLAK berhasil diambil."));
        }

        /// <summary>
        /// Mendaftarkan atau mengubah master indikator target kinerja dan rubrik perilaku
        /// </summary>
        [HttpPost("kpi-catalogs")]
        [AccessAction("ManageMetric", "Mengelola Katalog KPI", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PerformanceCycleGoal", "ManageMetric")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<KpiMetricCatalogDto>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CreateKpiCatalog([FromBody] CreateKpiMetricCatalogRequestDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _cycleService.CreateKpiCatalogAsync(dto, Guid.Empty, cancellationToken);
                return StatusCode(201, ApiResponse<KpiMetricCatalogDto>.Ok(result, "Indikator kinerja berhasil didaftarkan ke katalog master."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion

        #region GOALS CASCADING
        /// <summary>
        /// Menampilkan rincian sasaran kerja target individu staf pada siklus aktif
        /// </summary>
        [HttpGet("goals/staff/{staffId}")]
        [AccessAction("ViewGoal", "Melihat Sasaran Individu", AccessType = AccessTypes.Read, SortOrder = 5)]
        [AccessPermission("PerformanceCycleGoal", "ViewGoal")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PerformanceGoalDto>>), 200)]
        public async Task<IActionResult> GetGoalsByStaff(
            [FromRoute] Guid staffId,
            [FromQuery] Guid cycleId,
            CancellationToken cancellationToken = default)
        {
            var result = await _goalService.GetGoalsByStaffAsync(staffId, cycleId, cancellationToken);
            return Ok(ApiResponse<List<PerformanceGoalDto>>.Ok(result, "Daftar sasaran kerja individu berhasil diambil."));
        }

        /// <summary>
        /// Menetapkan dan mengesahkan target sasaran individu yang diturunkan dari KPI instalasi (VR-PRF-02, VR-PRF-04)
        /// </summary>
        [HttpPost("goals/cascading")]
        [AccessAction("ManageGoal", "Mengotorisasi Sasaran Kerja", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("PerformanceCycleGoal", "ManageGoal")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<List<PerformanceGoalDto>>), 201)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        public async Task<IActionResult> CascadeGoals([FromBody] CascadePerformanceGoalsRequestDto dto, CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _goalService.CascadeGoalsAsync(dto, Guid.Empty, cancellationToken);
                return StatusCode(201, ApiResponse<List<PerformanceGoalDto>>.Ok(result, "Penurunan sasaran mutu individu berhasil ditetapkan dan dikunci 100%."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<string>.Fail(400, ex.Message));
            }
        }
        #endregion
    }
}
