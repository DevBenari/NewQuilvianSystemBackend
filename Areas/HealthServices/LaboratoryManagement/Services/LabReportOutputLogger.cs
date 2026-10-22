using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Pencatat <b>keluaran</b> laporan Laboratorium <c>S16b</c> — unduhan dan cetak (<c>LAB-PERM-v1</c> rev 17
    /// 19.5, <c>LAB-DEC-233</c>, <c>LAB-DEC-248</c>, <c>BE-LAB-98</c>). Satu bentuk log bagi seluruh titik
    /// keluaran, supaya tidak bercabang antar-controller.
    ///
    /// <para>
    /// <b>Isi di teks pesan.</b> <see cref="LoggerService"/> tidak menuliskan objek payload ke log, sehingga
    /// laporan, periode, penyaring, dan jumlah baris ditulis di pesan; pelaku dan waktu diisi pencatat.
    /// Payload tidak dikirim sama sekali — ruas bernama <c>Id</c>/<c>Name</c>/<c>Path</c> akan menimpa pelaku.
    /// </para>
    ///
    /// <para>
    /// <b>Dilarang</b>: No. RM, nama pasien, alasan pembatalan, isi berkas (A8.12).
    /// </para>
    /// </summary>
    public class LabReportOutputLogger
    {
        private const string LogCategory = "HealthServices.LaboratoryManagement";

        private readonly LoggerService _loggerService;

        public LabReportOutputLogger(LoggerService loggerService)
        {
            _loggerService = loggerService;
        }

        /// <summary>Unduhan berkas terkirim — aksi <c>&lt;Resource&gt;.Export</c>.</summary>
        public Task LogExportAsync(string resource, string reportKey, LabReportPeriodResponse period, string filters, int rowCount) =>
            _loggerService.AuditAsync(LogCategory, resource + ".Export", $"{reportKey} — {Rentang(period)} — {filters} — {rowCount} baris");

        /// <summary>Cetak diminta lewat tombol Cetak, sebelum dialog cetak dibuka — aksi <c>&lt;Resource&gt;.Print</c>.</summary>
        public Task LogPrintAsync(string resource, string reportKey, LabReportPeriodResponse period, string filters) =>
            _loggerService.AuditAsync(LogCategory, resource + ".Print", $"{reportKey} — {Rentang(period)} — {filters} — cetak");

        /// <summary>
        /// Membuka daftar laporan yang memuat identitas pasien — aksi <c>&lt;Resource&gt;.Read</c>
        /// (<c>02-backend-architecture.md</c> 32.10 no. 1, disetujui 2026-10-20, <c>BE-LAB-99</c>).
        /// </summary>
        public Task LogReadAsync(string resource, string reportKey, LabReportPeriodResponse period, string filters, int pageNumber) =>
            _loggerService.AuditAsync(LogCategory, resource + ".Read", $"{reportKey} — {Rentang(period)} — {filters} — halaman {pageNumber}");

        private static string Rentang(LabReportPeriodResponse period) =>
            $"{period.StartDate:yyyy-MM-dd}..{period.EndDate:yyyy-MM-dd}";
    }
}
