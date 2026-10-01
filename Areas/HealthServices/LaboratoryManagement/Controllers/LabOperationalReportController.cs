using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

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
    ///
    /// <b>Unduhan memakai izin tersendiri</b> (23.10 butir 2): mengunduh membawa data keluar sistem,
    /// sehingga pemegang <c>Read</c> saja tidak dapat mengunduh. <c>AccessType</c> aksi <c>Export</c>
    /// bernilai <c>Read</c> karena <see cref="AccessTypes"/> hanya mengenal empat jenis — itu jenisnya,
    /// bukan izin bacanya.
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
        private const string ExportDescription = "Mengunduh laporan operasional laboratorium sebagai berkas CSV";
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly LabOperationalReportService _labOperationalReportService;
        private readonly LabReportCsvWriter _labReportCsvWriter;
        private readonly LoggerService _loggerService;

        public LabOperationalReportController(
            LabOperationalReportService labOperationalReportService,
            LabReportCsvWriter labReportCsvWriter,
            LoggerService loggerService)
        {
            _labOperationalReportService = labOperationalReportService;
            _labReportCsvWriter = labReportCsvWriter;
            _loggerService = loggerService;
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

        // Angka penolakan wadah per disiplin menurut TANGGAL KEPUTUSAN kelayakan, beserta rincian
        // per alasan (LAB-DEC-159 butir 3). Ketiga disiplin terhitung.
        [HttpGet("specimen-rejection")]
        [ProducesResponseType(typeof(ApiResponse<LabSpecimenRejectionReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Operational Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOperationalReport", "Read")]
        public async Task<IActionResult> GetSpecimenRejection(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOperationalReportService.GetSpecimenRejectionAsync(query, cancellationToken);

                return Ok(ApiResponse<LabSpecimenRejectionReportResponse>.Ok(
                    result, "Laporan penolakan wadah berhasil dibentuk."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                // VAL-147, VAL-148 (400) dan VAL-149 (422).
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // Waktu penyelesaian dari wadah layak sampai hasil dirilis, per disiplin dan kesegeraan
        // (LAB-DEC-159 butir 4). Terlambat memakai batas dan perbandingan yang sama dengan daftar
        // pantau keterlambatan cito (INV-57).
        [HttpGet("turnaround-time")]
        [ProducesResponseType(typeof(ApiResponse<LabTurnaroundTimeReportResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Read", "Read Lab Operational Report", Description = ReadDescription, AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabOperationalReport", "Read")]
        public async Task<IActionResult> GetTurnaroundTime(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var result = await _labOperationalReportService.GetTurnaroundTimeAsync(query, cancellationToken);

                return Ok(ApiResponse<LabTurnaroundTimeReportResponse>.Ok(
                    result, "Laporan waktu penyelesaian berhasil dibentuk."));
            }
            catch (LabOperationalReportValidationException ex)
            {
                // VAL-147, VAL-148 (400) dan VAL-149 (422).
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }

        // ======================================================================
        // Unduhan CSV — LabOperationalReport : Export (BE-LAB-86)
        // ======================================================================

        [HttpGet("examination-count/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "text/csv")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Operational Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabOperationalReport", "Export")]
        public Task<IActionResult> ExportExaminationCount(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default) =>
            UnduhAsync(
                query,
                "examination-count",
                "laporan-jumlah-pemeriksaan",
                () => _labOperationalReportService.GetExaminationCountAsync(query, cancellationToken),
                laporan => laporan.Period,
                _labReportCsvWriter.Write);

        [HttpGet("specimen-rejection/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "text/csv")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Operational Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabOperationalReport", "Export")]
        public Task<IActionResult> ExportSpecimenRejection(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default) =>
            UnduhAsync(
                query,
                "specimen-rejection",
                "laporan-penolakan-wadah",
                () => _labOperationalReportService.GetSpecimenRejectionAsync(query, cancellationToken),
                laporan => laporan.Period,
                _labReportCsvWriter.Write);

        [HttpGet("turnaround-time/export")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "text/csv")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Export", "Export Lab Operational Report", Description = ExportDescription, AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("LabOperationalReport", "Export")]
        public Task<IActionResult> ExportTurnaroundTime(
            [FromQuery] LabOperationalReportQuery query,
            CancellationToken cancellationToken = default) =>
            UnduhAsync(
                query,
                "turnaround-time",
                "laporan-waktu-penyelesaian",
                () => _labOperationalReportService.GetTurnaroundTimeAsync(query, cancellationToken),
                laporan => laporan.Period,
                _labReportCsvWriter.Write);

        /// <summary>
        /// Satu jalur bagi ketiga unduhan: laporan dibentuk oleh fungsi yang <b>sama</b> dengan layar
        /// (penjaga periode ikut), ditulis menjadi CSV, lalu dicatat <b>satu kali</b> — sesudah berkasnya
        /// jadi, sehingga unduhan yang gagal tidak meninggalkan jejak palsu.
        ///
        /// <b>Isi log</b> (<c>LAB-PERM-v1</c> rev 12 14.5): jenis laporan, periode, disiplin, dan jumlah
        /// baris — di teks pesan, sebab <see cref="LoggerService"/> tidak menuliskan objek payload ke log.
        /// Pelaku dan waktu diisi pencatat. <b>Dilarang</b>: isi berkas, nama pasien, No. RM, nilai hasil.
        /// Nama ruas payload sengaja tidak memakai <c>Id</c>, <c>Name</c>, maupun <c>Path</c>: pencatat
        /// memakainya untuk menimpa pelaku dan alamat permintaan.
        /// </summary>
        private async Task<IActionResult> UnduhAsync<TLaporan>(
            LabOperationalReportQuery query,
            string jenisLaporan,
            string awalanNamaBerkas,
            Func<Task<TLaporan>> bentukLaporan,
            Func<TLaporan, LabReportPeriodResponse> periodeLaporan,
            Func<TLaporan, LabCsvFile> tulisCsv)
        {
            try
            {
                var laporan = await bentukLaporan();
                var berkas = tulisCsv(laporan);
                var periode = periodeLaporan(laporan);
                var rentang = $"{periode.StartDate:yyyy-MM-dd}..{periode.EndDate:yyyy-MM-dd}";
                var disiplin = query.Discipline?.ToString() ?? "seluruh disiplin";

                await _loggerService.AuditAsync(
                    LogCategory,
                    "LabOperationalReport.Export",
                    $"{jenisLaporan} — {rentang} — {disiplin} — {berkas.RowCount} baris",
                    new
                    {
                        ReportType = jenisLaporan,
                        periode.StartDate,
                        periode.EndDate,
                        Discipline = disiplin,
                        RowCount = berkas.RowCount
                    });

                return File(
                    berkas.Content,
                    "text/csv",
                    $"{awalanNamaBerkas}_{periode.StartDate:yyyy-MM-dd}_{periode.EndDate:yyyy-MM-dd}.csv");
            }
            catch (LabOperationalReportValidationException ex)
            {
                // VAL-147, VAL-148 (400) dan VAL-149 (422) — penjaga yang sama dengan layar.
                return StatusCode(ex.StatusCode, ApiResponse<object>.Fail(ex.StatusCode, ex.Message));
            }
        }
    }
}
