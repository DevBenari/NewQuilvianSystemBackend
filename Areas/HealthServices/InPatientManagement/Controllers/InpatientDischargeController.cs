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
    /// <b>Yang belum ada di sini.</b> Daftar periksa administrasi (<c>BE-RWI-023</c>),
    /// kelayakan keuangan (<c>BE-RWI-024</c>), pemeriksaan lima syarat penutupan dan penutupan
    /// episode (<c>BE-RWI-025</c> dan <c>BE-RWI-026</c>), serta pencatatan kepergian fisik
    /// (<c>BE-RWI-027</c>) semuanya milik task berikutnya.
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
