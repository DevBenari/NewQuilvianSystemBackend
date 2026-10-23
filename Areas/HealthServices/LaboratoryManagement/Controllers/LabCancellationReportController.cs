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
    /// Laporan Pembatalan Order &amp; Pemeriksaan (<c>LAB-API-v1</c> <c>r49</c> 44.2, <c>LAB-PERM-v1</c> rev 17 bagian 19,
    /// <c>BE-LAB-99</c>). Memuat <b>identitas pasien</b>, sehingga izinnya tersendiri dan bawaannya tertutup
    /// (<c>LAB-DEC-239</c>): hak baca daftar Laboratorium maupun izin laporan lain tidak membukanya. Setiap pembukaan
    /// daftar dicatat tanpa data pasien (32.10 no. 1). Unduhan <c>.xlsx</c> dan pencatat cetak (<c>BE-LAB-100</c>) memakai
    /// izin <c>Export</c> yang terpisah, dan keduanya dicatat tanpa pasien maupun alasan.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-cancellation-reports")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Cancellation Report",
        AreaName = "HealthServices",
        ControllerName = "LabCancellationReport",
        Description = "Laporan pembatalan pesanan dan pemeriksaan laboratorium: siapa membatalkan apa, kapan, dan mengapa",
        SortOrder = 26
    )]
    [Tags("Health Services / Laboratory Management / Lab Cancellation Report")]
    public class LabCancellationReportController : ControllerBase
    {
        // Satu teks per kunci — registri hak akses menampilkan satu baris per kunci memakai teks
        // [AccessAction] yang pertama terbaca.
        private const string ReadDescription = "Membuka Laporan Pembatalan laboratorium, detailnya, dan metadata penyaringnya";
        private const string ExportDescription = "Mengunduh Laporan Pembatalan sebagai berkas Excel dan mencatat cetaknya";
        private const string Resource = "LabCancellationReport";
        private const string ReportKey = "lab-cancellation-report";

        private readonly LabCancellationReportService _service;
        private readonly LabReportXlsxWriter _xlsxWriter;
        private readonly LabReportOutputLogger _outputLogger;

        public LabCancellationReportController(
            LabCancellationReportService service,
            LabReportXlsxWriter xlsxWriter,
            LabReportOutputLogger outputLogger)
        {
            _service = service;
            _xlsxWriter = xlsxWriter;
            _outputLogger = outputLogger;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LabCancellationReportFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Cancellation Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCancellationReport", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default)
        {
            var result = await _service.GetFilterMetadataAsync(cancellationToken);

            return Ok(ApiResponse<LabCancellationReportFilterMetadataResponse>.Ok(
                result, "Metadata penyaring laporan pembatalan berhasil diambil."));
        }

        // Petugas yang membatalkan pada periode terpilih — pilihan penyaring "Dibatalkan oleh".
        [HttpGet("cancelled-by-options")]
        [ProducesResponseType(typeof(ApiResponse<List<LabCancelledByOptionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Cancellation Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCancellationReport", "Read")]
        public async Task<IActionResult> GetCancelledByOptions(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetCancelledByOptionsAsync(query, cancellationToken);

                return Ok(ApiResponse<List<LabCancelledByOptionResponse>>.Ok(
                    result, "Pilihan petugas pembatal berhasil diambil."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // Satu baris per peristiwa Order.Cancel / Examination.Cancel menurut WAKTU PEMBATALAN (INV-58), terbaru di
        // atas. Total atas seluruh hasil penyaring. Pembukaan dicatat tanpa data pasien.
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<LabCancellationReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Cancellation Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCancellationReport", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] LabCancellationReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetListAsync(query, cancellationToken);
                var penyaring = await _service.DescribeFiltersAsync(query, cancellationToken);

                await _outputLogger.LogReadAsync(Resource, ReportKey, result.Period, penyaring, result.PageNumber);

                return Ok(ApiResponse<LabCancellationReportResponse>.Ok(
                    result, "Laporan pembatalan berhasil dibentuk."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                // VAL-147, VAL-148 (400) dan VAL-149 (422).
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // Detail satu peristiwa di bawah izin laporan (32.10 no. 4): pembaca tidak membutuhkan izin daftar Laboratorium.
        [HttpGet("{historyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LabCancellationReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Lab Cancellation Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabCancellationReport", "Read")]
        public async Task<IActionResult> GetDetail(
            Guid historyId,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _service.GetDetailAsync(historyId, cancellationToken);

                return Ok(ApiResponse<LabCancellationReportDetailResponse>.Ok(
                    result, "Detail peristiwa pembatalan berhasil diambil."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
            }
        }

        // ======================================================================
        // Keluaran — LabCancellationReport : Export (BE-LAB-100, LAB-DEC-233, LAB-DEC-248)
        // ======================================================================

        // Seluruh baris hasil penyaring sebagai .xlsx — dibentuk dari fungsi yang SAMA dengan daftar (RPT-002), paging
        // diabaikan. Berkas memuat identitas pasien; log unduhan TIDAK.
        [HttpGet("export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, LabReportXlsxWriter.ContentType)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Cancellation Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabCancellationReport", "Export")]
        public async Task<IActionResult> Export(
            [FromQuery] LabCancellationReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var laporan = await _service.GetAllForExportAsync(query, cancellationToken);
                var berkas = _xlsxWriter.Write(laporan);
                var penyaring = await _service.DescribeFiltersAsync(query, cancellationToken);

                await _outputLogger.LogExportAsync(Resource, ReportKey, laporan.Period, penyaring, berkas.RowCount);

                return File(
                    berkas.Content,
                    LabReportXlsxWriter.ContentType,
                    $"laporan-pembatalan_{laporan.Period.StartDate:yyyy-MM-dd}_{laporan.Period.EndDate:yyyy-MM-dd}.xlsx");
            }
            catch (LabOperationalReportValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // Dipanggil layar SEBELUM membuka dialog cetak (LAB-DEC-248). Layar hanya membuka dialog sesudah 200; 403 atau
        // galat lain membuat cetak tidak dibuka (fail-closed). Tidak membentuk laporan ulang.
        [HttpPost("print-log")]
        [ProducesResponseType(typeof(ApiResponse<LabReportPrintLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Cancellation Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabCancellationReport", "Export")]
        public async Task<IActionResult> LogPrint(
            [FromBody] LabCancellationReportPrintRequest request,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var periode = LabOperationalReportService.ResolvePeriod(request);
                var penyaring = await _service.DescribeFiltersAsync(request, cancellationToken);

                await _outputLogger.LogPrintAsync(Resource, ReportKey, periode.Response, penyaring);

                return Ok(ApiResponse<LabReportPrintLogResponse>.Ok(
                    new LabReportPrintLogResponse { LoggedAt = DateTime.UtcNow },
                    "Cetak laporan pembatalan berhasil dicatat."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }
    }
}
