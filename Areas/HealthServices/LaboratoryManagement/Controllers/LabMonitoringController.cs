using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Tiga daftar pantau sejajar, satu untuk setiap disiplin (<c>LAB-DEC-025</c>).
    ///
    /// Ketiganya adalah jalur tersendiri, bukan satu jalur berpenyaring. Petugas Patologi
    /// Anatomi membuka menunya sendiri dan langsung melihat pekerjaannya — tanpa memilih
    /// disiplin lebih dahulu, dan tanpa kemungkinan salah memilih.
    ///
    /// Seluruh grup ini <b>baca saja</b>, dan isinya diturunkan dari <c>LabOrder.Discipline</c>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-monitoring")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Monitoring",
        AreaName = "HealthServices",
        ControllerName = "LabMonitoring",
        Description = "Daftar pantau pesanan laboratorium per disiplin",
        SortOrder = 8
    )]
    [Tags("Health Services / Laboratory Management / Lab Monitoring")]
    public class LabMonitoringController : ControllerBase
    {
        private readonly LabMonitoringService _labMonitoringService;

        public LabMonitoringController(LabMonitoringService labMonitoringService)
        {
            _labMonitoringService = labMonitoringService;
        }

        [HttpGet("clinical-pathology")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabMonitoringItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Monitoring", Description = "Melihat daftar pantau Patologi Klinik", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabMonitoring", "Read")]
        public Task<IActionResult> GetClinicalPathology(
            [FromQuery] LabMonitoringQuery query,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(LabDiscipline.ClinicalPathology, query, "Patologi Klinik", cancellationToken);

        [HttpGet("anatomic-pathology")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabMonitoringItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Monitoring", Description = "Melihat daftar pantau Patologi Anatomi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabMonitoring", "Read")]
        public Task<IActionResult> GetAnatomicPathology(
            [FromQuery] LabMonitoringQuery query,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(LabDiscipline.AnatomicalPathology, query, "Patologi Anatomi", cancellationToken);

        [HttpGet("microbiology")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabMonitoringItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Monitoring", Description = "Melihat daftar pantau Mikrobiologi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabMonitoring", "Read")]
        public Task<IActionResult> GetMicrobiology(
            [FromQuery] LabMonitoringQuery query,
            CancellationToken cancellationToken = default) =>
            ExecuteAsync(LabDiscipline.Microbiology, query, "Mikrobiologi", cancellationToken);

        private async Task<IActionResult> ExecuteAsync(
            LabDiscipline discipline,
            LabMonitoringQuery query,
            string namaDisiplin,
            CancellationToken cancellationToken)
        {
            if (query.StartDate.HasValue && query.EndDate.HasValue &&
                query.StartDate.Value > query.EndDate.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh melewati tanggal akhir."));
            }

            var hasil = await _labMonitoringService.GetByDisciplineAsync(discipline, query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabMonitoringItemResponse>>.Ok(
                hasil, $"Daftar pantau {namaDisiplin} berhasil diambil."));
        }
    }
}
