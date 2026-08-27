using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Daftar pantau Rawat Inap: keadaan yang perlu dibetulkan orang, bukan ditolak sistem.
    /// </summary>
    /// <remarks>
    /// <b>Lima daftar pantau.</b> <c>GET /monitoring/isolation-mismatch</c> dibuka
    /// <c>BE-RWI-015</c>; empat sisanya — <c>pending-closures</c>,
    /// <c>closures-without-financial-clearance</c>, <c>unassigned-nurse-episodes</c>, dan
    /// <c>bed-drift</c> — dibuka <c>BE-RWI-029</c>.
    ///
    /// <para>
    /// <b>Daftar pantau ketiga pada <c>RWI-RULE-023</c> sengaja tidak ada.</b> Kepatuhan
    /// pengkajian awal dan verifikasi CPPT bergantung pada slice dokumentasi klinis yang masih
    /// menunggu <c>DEC-INP-001</c>.
    /// </para>
    ///
    /// <para>
    /// <b>Kenapa daftar pantau ada.</b> Ketika kondisi klinis berubah di tengah perawatan,
    /// pencatatannya tidak pernah ditahan. Fakta klinis dicatat lebih dulu, lalu sistem
    /// menunjukkan bahwa penempatannya perlu dibetulkan. Menahan pencatatan demi menjaga
    /// aturan penempatan adalah urutan yang terbalik — <c>RWI-RULE-012</c> bagian A aturan 7.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/monitoring")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Monitoring",
        AreaName = "HealthServices",
        ControllerName = "InpatientMonitoring",
        Description = "Daftar pantau keadaan rawat inap yang perlu dibetulkan",
        SortOrder = 13
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Monitoring")]
    public class InpatientMonitoringController : ControllerBase
    {
        private readonly InpCensusQueryService _censusQueryService;

        public InpatientMonitoringController(InpCensusQueryService censusQueryService)
        {
            _censusQueryService = censusQueryService;
        }

        /// <summary>
        /// Daftar episode yang kebutuhan isolasinya tidak cocok dengan sifat tempat tidur yang
        /// sedang ditempatinya.
        /// </summary>
        /// <remarks>
        /// Memuat dua arah sekaligus: pasien yang membutuhkan isolasi tetapi berada di tempat
        /// tidur biasa, dan pasien yang tidak membutuhkan isolasi tetapi menempati tempat
        /// tidur isolasi. Daftar yang kosong mengembalikan daftar kosong, bukan galat.
        /// </remarks>
        [HttpGet("isolation-mismatch")]
        [ProducesResponseType(typeof(ApiResponse<IsolationMismatchPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Monitoring", Description = "Melihat daftar pantau penempatan tidak sesuai kebutuhan isolasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientMonitoring", "Read")]
        public async Task<IActionResult> GetIsolationMismatch(
            [FromQuery] IsolationMismatchQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetIsolationMismatchAsync(query, cancellationToken);

            return Ok(ApiResponse<IsolationMismatchPagedResult>.Ok(
                result,
                "Daftar pantau penempatan tidak sesuai berhasil diambil."));
        }

        /// <summary>
        /// Daftar episode yang sudah boleh pulang tetapi belum ditutup melewati ambang waktu.
        /// </summary>
        /// <remarks>
        /// Ambangnya dibaca dari <c>MstInpatientSetting.PendingClosureThresholdHours</c> setiap
        /// pembacaan, sehingga angka yang diubah admin berlaku pada pembacaan berikutnya.
        ///
        /// <para>
        /// Daftar ini lahir dari <c>RWI-RULE-010</c>: yang memutuskan pulang dan yang menutup
        /// episode adalah orang yang berbeda. Tanpa daftar ini, episode yang menggantung hanya
        /// ditemukan ketika ada yang mengeluh.
        /// </para>
        /// </remarks>
        [HttpGet("pending-closures")]
        [ProducesResponseType(typeof(ApiResponse<PendingClosurePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Monitoring", Description = "Melihat daftar pantau penutupan tertunda", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientMonitoring", "Read")]
        public async Task<IActionResult> GetPendingClosures(
            [FromQuery] InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetPendingClosuresAsync(query, cancellationToken);

            return Ok(ApiResponse<PendingClosurePagedResult>.Ok(
                result,
                "Daftar pantau penutupan tertunda berhasil diambil."));
        }

        /// <summary>Daftar episode yang ditutup menembus gerbang kelayakan keuangan.</summary>
        /// <remarks>
        /// Setiap baris di sini adalah keputusan supervisor yang melewati satu syarat
        /// penutupan. Daftar yang panjang berarti gerbang keuangan sedang tidak berfungsi
        /// sebagaimana dimaksud — dan itu perlu diketahui sebelum menjadi kebiasaan.
        /// </remarks>
        [HttpGet("closures-without-financial-clearance")]
        [ProducesResponseType(typeof(ApiResponse<OverrideClosurePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Monitoring", Description = "Melihat daftar pantau penutupan menembus gerbang keuangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientMonitoring", "Read")]
        public async Task<IActionResult> GetOverrideClosures(
            [FromQuery] InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetOverrideClosuresAsync(query, cancellationToken);

            return Ok(ApiResponse<OverrideClosurePagedResult>.Ok(
                result,
                "Daftar pantau penutupan menembus gerbang keuangan berhasil diambil."));
        }

        /// <summary>Daftar episode aktif yang belum punya perawat penanggung jawab.</summary>
        /// <remarks>
        /// Ini pasangan dari keputusan <b>tidak menahan</b> pada <c>RWI-DEC-032</c>: episode
        /// tetap berjalan tanpa perawat, dan ketiadaannya terlihat di sini alih-alih
        /// menghalangi pekerjaan.
        /// </remarks>
        [HttpGet("unassigned-nurse-episodes")]
        [ProducesResponseType(typeof(ApiResponse<UnassignedNursePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Monitoring", Description = "Melihat daftar episode tanpa perawat penanggung jawab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientMonitoring", "Read")]
        public async Task<IActionResult> GetUnassignedNurseEpisodes(
            [FromQuery] InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetUnassignedNurseEpisodesPagedAsync(
                query,
                cancellationToken);

            return Ok(ApiResponse<UnassignedNursePagedResult>.Ok(
                result,
                "Daftar episode tanpa perawat penanggung jawab berhasil diambil."));
        }

        /// <summary>
        /// Laporan selisih antara salinan status tempat tidur dan catatan penempatan.
        /// </summary>
        /// <remarks>
        /// <b>Ini satu-satunya pengawas atas satu-satunya arah tulis lintas modul.</b>
        /// <c>MstBed.BedStatus</c> adalah salinan; sumber kebenarannya
        /// <c>InpBedPlacement</c> dan <c>InpBedReservation</c>. Laporan ini hanya berguna bila
        /// ada yang membacanya secara berkala — bila tidak, salinan akan menyimpang diam-diam
        /// sampai seorang pasien ditempatkan di tempat tidur yang sudah ada orangnya.
        ///
        /// <para>
        /// Keempat keadaan yang merupakan wewenang admin — pembersihan, perbaikan, diblokir,
        /// dan nonaktif — tidak dihitung sebagai selisih, karena modul Rawat Inap memang tidak
        /// pernah menuliskannya.
        /// </para>
        /// </remarks>
        [HttpGet("bed-drift")]
        [ProducesResponseType(typeof(ApiResponse<BedDriftPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Monitoring", Description = "Melihat laporan selisih salinan status tempat tidur", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientMonitoring", "Read")]
        public async Task<IActionResult> GetBedDrift(
            [FromQuery] InpatientMonitoringQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetBedDriftAsync(query, cancellationToken);

            return Ok(ApiResponse<BedDriftPagedResult>.Ok(
                result,
                "Laporan selisih salinan status tempat tidur berhasil diambil."));
        }
    }
}
