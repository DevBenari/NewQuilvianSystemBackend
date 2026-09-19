using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
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
            NormalkanRentangTanggal(query);

            if (query.StartDate.HasValue && query.EndDate.HasValue &&
                query.StartDate.Value > query.EndDate.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh melewati tanggal akhir."));
            }

            // VAL-76 sengaja TIDAK ditulis sebagai penjaga di sini, dan itu hasil pemeriksaan,
            // bukan kelalaian.
            //
            // Penjaga `Enum.IsDefined` sempat ditulis, lalu dibuktikan **tidak pernah
            // tercapai**: pengikatan query ASP.NET Core sendiri sudah menjalankan pemeriksaan
            // itu, sehingga `?dateCategory=99` maupun `=0` ditolak `400` sebelum satu baris pun
            // di sini berjalan — diverifikasi 2026-09-17 terhadap aplikasi yang berjalan.
            //
            // Penjaga yang tidak pernah tercapai lebih buruk daripada tidak ada: ia terbaca
            // seolah menjadi penegaknya, dan pembaca berikutnya akan memelihara pesan yang tidak
            // pernah sampai ke siapa pun. Bentuk jawaban yang benar-benar diterima pemanggil
            // dicatat pada `LAB-API-v1` bagian 13.4.

            var hasil = await _labMonitoringService.GetByDisciplineAsync(discipline, query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabMonitoringItemResponse>>.Ok(
                hasil, $"Daftar pantau {namaDisiplin} berhasil diambil."));
        }

        /// <summary>
        /// Aturannya tinggal bersama pada <see cref="LabQueryDateRange"/> — enam tempat yang
        /// menyalin aturan yang sama pasti bercabang, dan cabangnya tidak menimbulkan galat,
        /// hanya tanggal yang salah pada satu layar dan benar pada layar lain.
        /// </summary>
        private static void NormalkanRentangTanggal(LabMonitoringQuery query)
        {
            (query.StartDate, query.EndDate) =
                LabQueryDateRange.Normalize(query.StartDate, query.EndDate);
        }
    }
}
