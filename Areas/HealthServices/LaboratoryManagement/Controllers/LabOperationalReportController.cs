using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Laporan operasional Laboratorium bagi kepala instalasi dan manajemen (<c>LAB-API-v1</c>
    /// <c>r37</c> bagian 32, <c>LAB-DEC-159</c>).
    ///
    /// <b>Izin tersendiri</b> (<c>LAB-DEC-160</c>, <c>LAB-PERM-v1</c> revision 12): laporan
    /// merangkum seluruh pasien dan seluruh petugas, sehingga hak baca daftar Laboratorium tidak
    /// membukanya. Tanpa kebijakan <c>LabOperationalReport</c>, setiap pengguna menerima <c>403</c>.
    ///
    /// Membuka laporan <b>tidak</b> dicatat, sesuai konvensi <c>GET</c> (A7.9); hanya unduhan yang
    /// dicatat — aksi <c>Export</c>, dipasang <c>BE-LAB-86</c>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-operational-reports")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Operational Report",
        AreaName = "HealthServices",
        ControllerName = "LabOperationalReport",
        Description = "Laporan operasional laboratorium: jumlah pemeriksaan, penolakan wadah, dan waktu penyelesaian",
        SortOrder = 24
    )]
    [Tags("Health Services / Laboratory Management / Lab Operational Report")]
    public class LabOperationalReportController : ControllerBase
    {
        // Satu teks bagi setiap endpoint Read — registri hak akses menampilkan satu baris per
        // kunci memakai teks [AccessAction] yang pertama terbaca.
        private const string ReadDescription = "Membuka laporan operasional laboratorium beserta metadata penyaringnya";

        private readonly LabOperationalReportService _labOperationalReportService;

        public LabOperationalReportController(LabOperationalReportService labOperationalReportService)
        {
            _labOperationalReportService = labOperationalReportService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabOperationalReportFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Operational Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOperationalReport", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = LabFilterMetadataFactory.LabOperationalReport();

            return Ok(ApiResponse<LabOperationalReportFilterMetadataResponse>.Ok(
                result, "Metadata penyaring laporan operasional berhasil diambil."));
        }

        // Jumlah pemeriksaan yang DIRILIS pada periode itu, per disiplin dan per jenis pemeriksaan
        // (LAB-DEC-159 butir 2). Periode wajib, awal tidak sesudah akhir, paling panjang 366 hari.
        [HttpGet("examination-count")]
        [ProducesResponseType(typeof(ApiResponse<LabExaminationCountReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Operational Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOperationalReport", "Read")]
        public async Task<IActionResult> GetExaminationCount(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOperationalReportService.GetExaminationCountAsync(query, cancellationToken);

                return Ok(ApiResponse<LabExaminationCountReportResponse>.Ok(
                    result, "Laporan jumlah pemeriksaan berhasil dibentuk."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                // VAL-147, VAL-148 (400) dan VAL-149 (422).
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }
    }
}
