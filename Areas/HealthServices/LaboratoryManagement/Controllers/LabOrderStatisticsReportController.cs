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
    /// Statistik Pesanan Laboratorium (<c>LAB-API-v1</c> <c>r49</c> 44.3, <c>LAB-PERM-v1</c> rev 17 bagian 19,
    /// <c>BE-LAB-98</c>). Izin <b>tersendiri</b> (<c>LAB-DEC-231</c>): hak baca daftar Laboratorium maupun
    /// <c>LabOperationalReport</c> tidak membukanya. Lihat dan keluaran dipisah; cetak memakai izin unduh
    /// (<c>LAB-DEC-248</c>). Tanpa kebijakan yang diberikan admin, setiap pengguna menerima <c>403</c>.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-order-statistics-reports")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Order Statistics Report",
        AreaName = "HealthServices",
        ControllerName = "LabOrderStatisticsReport",
        Description = "Statistik pesanan laboratorium: jumlah pesanan dan pemeriksaan per jenis kunjungan, unit layanan, dan disiplin",
        SortOrder = 25
    )]
    [Tags("Health Services / Laboratory Management / Lab Order Statistics Report")]
    public class LabOrderStatisticsReportController : ControllerBase
    {
        // Satu teks per kunci — registri hak akses menampilkan satu baris per kunci memakai teks
        // [AccessAction] yang pertama terbaca.
        private const string ReadDescription = "Membuka Statistik Pesanan laboratorium beserta metadata penyaringnya";
        private const string ExportDescription = "Mengunduh Statistik Pesanan sebagai berkas Excel dan mencatat cetaknya";
        private const string Resource = "LabOrderStatisticsReport";
        private const string ReportKey = "lab-order-statistics-report";

        private readonly LabOrderStatisticsReportService _service;
        private readonly LabReportXlsxWriter _xlsxWriter;
        private readonly LabReportOutputLogger _outputLogger;

        public LabOrderStatisticsReportController(
            LabOrderStatisticsReportService service,
            LabReportXlsxWriter xlsxWriter,
            LabReportOutputLogger outputLogger)
        {
            _service = service;
            _xlsxWriter = xlsxWriter;
            _outputLogger = outputLogger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabOrderStatisticsReportFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Order Statistics Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrderStatisticsReport", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetFilterMetadataAsync(cancellationToken);

            return Ok(ApiResponse<LabOrderStatisticsReportFilterMetadataResponse>.Ok(
                result, "Metadata penyaring statistik pesanan berhasil diambil."));
        }

        // Beban permintaan per jenis kunjungan → unit layanan → disiplin menurut WAKTU PESANAN DIMINTA
        // (LAB-DEC-242). Definisi hitung sama dengan Beranda (INV-59).
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LabOrderStatisticsReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Order Statistics Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOrderStatisticsReport", "Read")]
        public async Task<IActionResult> Get(
            [FromQuery] LabOrderStatisticsReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetAsync(query, cancellationToken);

                return Ok(ApiResponse<LabOrderStatisticsReportResponse>.Ok(
                    result, "Statistik pesanan berhasil dibentuk."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                // VAL-147, VAL-148 (400) dan VAL-149 (422).
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // Berkas .xlsx dari respons yang SAMA dengan layar (RPT-002), dicatat tanpa data pasien.
        [HttpGet("export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, LabReportXlsxWriter.ContentType)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Order Statistics Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabOrderStatisticsReport", "Export")]
        public async Task<IActionResult> Export(
            [FromQuery] LabOrderStatisticsReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var laporan = await _service.GetAsync(query, cancellationToken);
                var berkas = _xlsxWriter.Write(laporan);
                var penyaring = await _service.DescribeFiltersAsync(query, cancellationToken);

                await _outputLogger.LogExportAsync(Resource, ReportKey, laporan.Period, penyaring, berkas.RowCount);

                return File(
                    berkas.Content,
                    LabReportXlsxWriter.ContentType,
                    $"statistik-pesanan_{laporan.Period.StartDate:yyyy-MM-dd}_{laporan.Period.EndDate:yyyy-MM-dd}.xlsx");
            }
            catch (LabOperationalReportValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // Dipanggil layar SEBELUM membuka dialog cetak (LAB-DEC-248). Layar hanya membuka dialog sesudah 200;
        // 403 atau galat lain membuat cetak tidak dibuka (fail-closed). Tidak membentuk laporan ulang.
        [HttpPost("print-log")]
        [ProducesResponseType(typeof(ApiResponse<LabReportPrintLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Order Statistics Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabOrderStatisticsReport", "Export")]
        public async Task<IActionResult> LogPrint(
            [FromBody] LabOrderStatisticsReportPrintRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var periode = LabOperationalReportService.ResolvePeriod(request);
                var penyaring = await _service.DescribeFiltersAsync(request, cancellationToken);

                await _outputLogger.LogPrintAsync(Resource, ReportKey, periode.Response, penyaring);

                return Ok(ApiResponse<LabReportPrintLogResponse>.Ok(
                    new LabReportPrintLogResponse { LoggedAt = DateTime.UtcNow },
                    "Cetak statistik pesanan berhasil dicatat."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }
    }
}
