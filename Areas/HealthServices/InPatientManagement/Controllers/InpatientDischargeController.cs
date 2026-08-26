using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Helpers;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Layar pemulangan: keputusan pasien boleh pulang, dan resume pulang beserta tanda
    /// tangannya.
    /// </summary>
    /// <remarks>
    /// <b>Tiga penjaga kewenangan per pasien bekerja di sini.</b> <c>GUARD-INP-02</c> menjaga
    /// keputusan pulang dan <c>GUARD-INP-03</c> menjaga penandatanganan resume; keduanya
    /// menuntut pemohon adalah <b>DPJP aktif episode itu</b>, bukan sekadar seorang dokter.
    /// Mesin hak akses tidak dapat menjaga hal itu, karena ia hanya mengenal peran, bukan
    /// hubungan antara seorang dokter dan seorang pasien.
    ///
    /// <para>
    /// <b>Isi resume tidak pernah masuk payload logger.</b> Diagnosis, ringkasan tindakan,
    /// instruksi kontrol, dan ringkasan klinis semuanya bertanda sensitif pada permission
    /// matrix bagian 5.4. Yang dicatat hanya identitas baris, nama controller, nama action,
    /// dan kode status.
    /// </para>
    ///
    /// <para>
    /// <b>Dua urutan yang mudah terbalik.</b> Pertama, keputusan pulang <b>tidak</b> melepas
    /// tempat tidur — pelepasannya menunggu kepergian fisik dicatat atau episode ditutup.
    /// Kedua, pencatatan kepergian fisik melepas tempat tidur <b>tanpa</b> menutup episode dan
    /// <b>tanpa</b> menulis riwayat status, karena status episode memang tidak berubah.
    /// </para>
    ///
    /// <para>
    /// <b>Jalan keluar supervisor menembus satu syarat saja</b>, yaitu kelayakan keuangan.
    /// Keempat syarat penutupan lainnya tetap menahan siapa pun.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/discharges")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Discharge",
        AreaName = "HealthServices",
        ControllerName = "InpatientDischarge",
        Description = "Keputusan pasien boleh pulang dan resume pulang rawat inap",
        SortOrder = 14
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Discharge")]
    public class InpatientDischargeController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.Discharge";

        private readonly InpDischargeService _dischargeService;
        private readonly InpEpisodeService _episodeService;
        private readonly LoggerService _loggerService;

        public InpatientDischargeController(
            InpDischargeService dischargeService,
            InpEpisodeService episodeService,
            LoggerService loggerService)
        {
            _dischargeService = dischargeService;
            _episodeService = episodeService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // BE-RWI-020 — Keputusan pasien boleh pulang
        // =====================================================================

        /// <summary>DPJP aktif menyatakan pasien boleh pulang beserta cara pulangnya.</summary>
        /// <remarks>
        /// Tempat tidur <b>belum</b> dilepas pada langkah ini. Episode menjadi
        /// <c>DischargePending</c>, pasien tetap muncul pada census, dan salinan status tempat
        /// tidur tidak berubah sampai kepergian fisiknya dicatat atau episodenya ditutup.
        /// </remarks>
        [HttpPost("{episodeId:guid}/decide")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Decide Inpatient Discharge", Description = "Menyatakan pasien rawat inap boleh pulang", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientDischarge", "Update")]
        public async Task<IActionResult> DecideDischarge(
            Guid episodeId,
            [FromBody] DecideDischargeRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.DecideDischargeAsync(
                episodeId,
                request,
                User.GetUserId(),
                User.GetDoctorId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromEpisodeFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.DecideDischarge",
                "Menyatakan pasien rawat inap boleh pulang.",
                new
                {
                    EntityId = episodeId,
                    Controller = "InpatientDischarge",
                    Action = "DecideDischarge",
                    StatusCode = StatusCodes.Status200OK
                });

            var detail = await _episodeService.GetDetailResponseAsync(
                episodeId,
                null,
                cancellationToken);

            return Ok(ApiResponse<InpatientEpisodeDetailResponse>.Ok(detail, result.Message));
        }

        // =====================================================================
        // BE-RWI-021 dan BE-RWI-022 — Resume pulang, tanda tangan, dan versinya
        // =====================================================================

        /// <summary>
        /// Mengambil resume pulang episode, beserta daftar versi sebelumnya bila diminta.
        /// </summary>
        /// <remarks>
        /// Kirim <c>includeRevisions=true</c> untuk menyertakan seluruh versi yang sudah
        /// digantikan, urut waktu. Versi yang tersimpan tidak dapat diubah maupun dihapus, dan
        /// tidak ada endpoint yang menyediakannya.
        /// </remarks>
        [HttpGet("{episodeId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<DischargeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Discharge", Description = "Melihat resume pulang rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientDischarge", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid episodeId,
            [FromQuery] bool includeRevisions = false,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.GetSummaryAsync(
                episodeId,
                includeRevisions,
                cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Resume pulang belum disusun."));
            }

            return Ok(ApiResponse<DischargeSummaryResponse>.Ok(
                result,
                "Resume pulang berhasil diambil."));
        }

        /// <summary>Menyusun atau memperbarui resume pulang.</summary>
        /// <remarks>
        /// Resume yang sudah ditandatangani <b>tidak</b> dapat diubah lewat endpoint ini.
        /// Perubahannya adalah amandemen rekam medis, dan hanya diterima ketika supervisor
        /// sudah membuka sesi koreksi pada episode tersebut — pada saat itu versi sebelumnya
        /// disimpan otomatis.
        /// </remarks>
        [HttpPut("{episodeId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<DischargeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Upsert Inpatient Discharge Summary", Description = "Menyusun atau memperbarui resume pulang", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientDischarge", "Update")]
        public async Task<IActionResult> UpsertSummary(
            Guid episodeId,
            [FromBody] UpsertDischargeSummaryRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.UpsertSummaryAsync(
                episodeId,
                request,
                User.GetUserId(),
                User.GetDoctorId(),
                User.IsSupervisor(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromSummaryFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.UpsertSummary",
                "Menyimpan resume pulang rawat inap.",
                new
                {
                    EntityId = result.SummaryId,
                    Controller = "InpatientDischarge",
                    Action = "UpsertSummary",
                    StatusCode = StatusCodes.Status200OK
                });

            var summary = await _dischargeService.GetSummaryAsync(
                episodeId,
                includeRevisions: false,
                cancellationToken);

            return Ok(ApiResponse<DischargeSummaryResponse>.Ok(summary, result.Message));
        }

        /// <summary>DPJP aktif menandatangani resume pulang.</summary>
        [HttpPatch("{episodeId:guid}/summary/sign")]
        [ProducesResponseType(typeof(ApiResponse<DischargeSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Sign Inpatient Discharge Summary", Description = "Menandatangani resume pulang rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientDischarge", "Sign")]
        public async Task<IActionResult> SignSummary(
            Guid episodeId,
            [FromBody] SignDischargeSummaryRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.SignSummaryAsync(
                episodeId,
                request,
                User.GetUserId(),
                User.GetDoctorId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromSummaryFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.SignSummary",
                "Menandatangani resume pulang rawat inap.",
                new
                {
                    EntityId = result.SummaryId,
                    Controller = "InpatientDischarge",
                    Action = "SignSummary",
                    StatusCode = StatusCodes.Status200OK
                });

            var summary = await _dischargeService.GetSummaryAsync(
                episodeId,
                includeRevisions: false,
                cancellationToken);

            return Ok(ApiResponse<DischargeSummaryResponse>.Ok(summary, result.Message));
        }

        // =====================================================================
        // BE-RWI-023 — Daftar periksa administrasi
        // =====================================================================

        /// <summary>Daftar butir administrasi beserta status penandaannya.</summary>
        /// <remarks>
        /// Butir yang sudah dinonaktifkan admin tetap muncul bila episode ini pernah
        /// menandainya — penandaan lama tidak pernah hilang, walaupun butirnya tidak lagi
        /// menahan penutupan.
        /// </remarks>
        [HttpGet("{episodeId:guid}/clearance")]
        [ProducesResponseType(typeof(ApiResponse<ClearanceChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Discharge", Description = "Melihat daftar periksa administrasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientDischarge", "Read")]
        public async Task<IActionResult> GetClearanceChecklist(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.GetClearanceChecklistAsync(
                episodeId,
                cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Episode rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<ClearanceChecklistResponse>.Ok(
                result,
                "Daftar periksa administrasi berhasil diambil."));
        }

        /// <summary>Menandai satu butir daftar periksa administrasi.</summary>
        [HttpPost("{episodeId:guid}/clearance/{itemId:guid}/mark")]
        [ProducesResponseType(typeof(ApiResponse<ClearanceChecklistResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Mark Inpatient Clearance Item", Description = "Menandai butir administrasi rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientDischarge", "Update")]
        public async Task<IActionResult> MarkClearanceItem(
            Guid episodeId,
            Guid itemId,
            [FromBody] MarkClearanceItemRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.MarkClearanceItemAsync(
                episodeId,
                itemId,
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromSummaryFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.MarkClearanceItem",
                "Menandai butir administrasi rawat inap.",
                new
                {
                    EntityId = episodeId,
                    Controller = "InpatientDischarge",
                    Action = "MarkClearanceItem",
                    StatusCode = StatusCodes.Status200OK
                });

            var checklist = await _dischargeService.GetClearanceChecklistAsync(
                episodeId,
                cancellationToken);

            return Ok(ApiResponse<ClearanceChecklistResponse>.Ok(checklist, result.Message));
        }

        // =====================================================================
        // BE-RWI-024 — Kelayakan keuangan
        // =====================================================================

        /// <summary>Petugas kasir atau billing menandai kelayakan keuangan.</summary>
        /// <remarks>
        /// Penandaan ini <b>manual</b>. Nilainya bergantung pada disiplin petugas kasir, bukan
        /// pada angka tagihan yang sebenarnya, karena `BillingManagement` belum punya kemampuan
        /// transaksi — `RWI-RISK-003`, diterima secara sadar dan bersifat sementara.
        /// </remarks>
        [HttpPost("{episodeId:guid}/financial-clearance")]
        [ProducesResponseType(typeof(ApiResponse<FinancialClearanceResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Mark Inpatient Financial Clearance", Description = "Menandai kelayakan keuangan rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientFinancialClearance", "Update")]
        public async Task<IActionResult> MarkFinancialClearance(
            Guid episodeId,
            [FromBody] MarkFinancialClearanceRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.MarkFinancialClearanceAsync(
                episodeId,
                request,
                User.GetUserId(),
                User.IsCashierOrBilling(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromSummaryFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.MarkFinancialClearance",
                "Menandai kelayakan keuangan rawat inap.",
                new
                {
                    EntityId = episodeId,
                    Controller = "InpatientDischarge",
                    Action = "MarkFinancialClearance",
                    StatusCode = StatusCodes.Status200OK
                });

            var clearance = await _dischargeService.GetFinancialClearanceAsync(
                episodeId,
                cancellationToken);

            return Ok(ApiResponse<FinancialClearanceResponse>.Ok(clearance, result.Message));
        }

        // =====================================================================
        // BE-RWI-025 dan BE-RWI-026 — Penutupan episode
        // =====================================================================

        /// <summary>Memeriksa kelima syarat penutupan dan menampilkan mana yang belum terpenuhi.</summary>
        /// <remarks>
        /// Jawabannya berupa <b>daftar syarat</b>, bukan boolean tunggal. Petugas perlu tahu apa
        /// yang harus dikejar, bukan hanya bahwa tombol tutup masih mati.
        /// </remarks>
        [HttpGet("{episodeId:guid}/closure-readiness")]
        [ProducesResponseType(typeof(ApiResponse<ClosureReadinessResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Discharge", Description = "Memeriksa kesiapan penutupan episode", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientDischarge", "Read")]
        public async Task<IActionResult> GetClosureReadiness(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.EvaluateClosureReadinessAsync(
                episodeId,
                cancellationToken);

            if (result == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Episode rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<ClosureReadinessResponse>.Ok(
                result,
                "Kesiapan penutupan episode berhasil diperiksa."));
        }

        /// <summary>Menutup episode dan melepas tempat tidur.</summary>
        [HttpPost("{episodeId:guid}/close")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Close Inpatient Episode", Description = "Menutup episode rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "Close")]
        public async Task<IActionResult> CloseEpisode(
            Guid episodeId,
            [FromBody] CloseEpisodeRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.CloseEpisodeAsync(
                episodeId,
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromEpisodeFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.CloseEpisode",
                "Menutup episode rawat inap.",
                new
                {
                    EntityId = episodeId,
                    Controller = "InpatientDischarge",
                    Action = "CloseEpisode",
                    StatusCode = StatusCodes.Status200OK
                });

            var detail = await _episodeService.GetDetailResponseAsync(
                episodeId,
                null,
                cancellationToken);

            return Ok(ApiResponse<InpatientEpisodeDetailResponse>.Ok(detail, result.Message));
        }

        /// <summary>Supervisor menutup episode menembus gerbang keuangan.</summary>
        /// <remarks>
        /// Jalan keluar ini menembus <b>hanya</b> syarat kelayakan keuangan. Keempat syarat
        /// lainnya tetap menahan, dan tidak ada satu pun peran yang dapat melewatinya.
        /// </remarks>
        [HttpPost("{episodeId:guid}/close-with-override")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Close Inpatient Episode With Override", Description = "Menutup episode menembus gerbang kelayakan keuangan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "CloseOverride")]
        public async Task<IActionResult> CloseEpisodeWithOverride(
            Guid episodeId,
            [FromBody] CloseEpisodeOverrideRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.CloseWithOverrideAsync(
                episodeId,
                request,
                User.GetUserId(),
                User.IsSupervisor(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromEpisodeFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.CloseEpisodeWithOverride",
                "Menutup episode rawat inap menembus gerbang kelayakan keuangan.",
                new
                {
                    EntityId = episodeId,
                    Controller = "InpatientDischarge",
                    Action = "CloseEpisodeWithOverride",
                    StatusCode = StatusCodes.Status200OK
                });

            var detail = await _episodeService.GetDetailResponseAsync(
                episodeId,
                null,
                cancellationToken);

            return Ok(ApiResponse<InpatientEpisodeDetailResponse>.Ok(detail, result.Message));
        }

        // =====================================================================
        // BE-RWI-027 — Kepergian fisik pasien
        // =====================================================================

        /// <summary>
        /// Mencatat pasien sudah meninggalkan ruangan. Melepas tempat tidur seketika
        /// <b>tanpa</b> menutup episode.
        /// </summary>
        /// <remarks>
        /// Endpoint ini <b>tidak dapat dibatalkan</b>. Pasien yang ternyata belum jadi pulang
        /// menjalani admisi baru — <c>RWI-RULE-036</c>.
        /// </remarks>
        [HttpPost("{episodeId:guid}/record-departure")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Record Inpatient Departure", Description = "Mencatat kepergian fisik pasien rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientDischarge", "RecordDeparture")]
        public async Task<IActionResult> RecordDeparture(
            Guid episodeId,
            [FromBody] RecordDepartureRequest? request,
            CancellationToken cancellationToken = default)
        {
            var result = await _dischargeService.RecordPatientDepartureAsync(
                episodeId,
                request,
                User.GetUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromEpisodeFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientDischarge.RecordDeparture",
                "Mencatat kepergian fisik pasien rawat inap.",
                new
                {
                    EntityId = episodeId,
                    Controller = "InpatientDischarge",
                    Action = "RecordDeparture",
                    StatusCode = StatusCodes.Status200OK
                });

            var detail = await _episodeService.GetDetailResponseAsync(
                episodeId,
                null,
                cancellationToken);

            return Ok(ApiResponse<InpatientEpisodeDetailResponse>.Ok(detail, result.Message));
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private IActionResult FromEpisodeFailure(InpEpisodeOperationResult result)
        {
            return BuildFailure(result.Status, result.Message);
        }

        private IActionResult FromSummaryFailure(InpDischargeSummaryOperationResult result)
        {
            return BuildFailure(result.Status, result.Message);
        }

        private IActionResult BuildFailure(InpEpisodeOperationStatus status, string message)
        {
            return status switch
            {
                InpEpisodeOperationStatus.Invalid => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, message)),

                InpEpisodeOperationStatus.Forbidden => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, message)),

                InpEpisodeOperationStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, message)),

                InpEpisodeOperationStatus.Conflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, message)),

                _ => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, message))
            };
        }
    }
}
