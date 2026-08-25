using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Layar admisi rawat inap. Petugas admisi membuka admisi, membetulkan isian yang salah
    /// selagi masih disiapkan, dan membatalkan admisi yang tidak jadi berjalan.
    /// </summary>
    /// <remarks>
    /// <b>Tidak ada endpoint yang menyetel status episode secara bebas.</b> Setiap perpindahan
    /// status punya endpoint bermakna sendiri, dan seluruhnya lewat satu method di
    /// <see cref="InpEpisodeService"/>. Endpoint bergaya
    /// <c>PATCH /episodes/{id}/status</c> yang menerima nilai status apa saja sengaja tidak
    /// disediakan: ia akan melubangi riwayat status, dan seluruh laporan pengecualian yang
    /// dibaca dari riwayat itu ikut salah tanpa ada yang menyadarinya.
    ///
    /// <para>
    /// Endpoint baca — daftar, detail, ringkasan, penyaring, dan riwayat status — belum ada di
    /// sini. Ia milik task berikutnya, yang juga menentukan kolom mana yang boleh tampil pada
    /// daftar dan kolom mana yang hanya boleh tampil pada detail.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/episodes")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Episode",
        AreaName = "HealthServices",
        ControllerName = "InpatientEpisode",
        Description = "Mengelola episode rawat inap, dari admisi dibuka sampai admisi dibatalkan",
        SortOrder = 10
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Episode")]
    public class InpatientEpisodeController : ControllerBase
    {
        private const string LogCategory = "HealthServices.InPatientManagement.Episode";

        /// <summary>
        /// Peran yang boleh membatalkan episode yang sudah berjalan.
        /// </summary>
        /// <remarks>
        /// Nama peran di repository ini adalah data yang disiapkan admin, bukan daftar tetap
        /// di dalam kode, sehingga daftar ini adalah asumsi yang perlu dikonfirmasi pemilik
        /// modul. Selama <c>BE-RWI-011</c> belum ada, tidak satu pun episode dapat mencapai
        /// status <c>Admitted</c>, sehingga penjaga ini belum punya jalur yang benar-benar
        /// terpakai. Ia ditulis sekarang supaya kewenangannya tidak terlupa saat penempatan
        /// pasien dibuka.
        /// </remarks>
        private static readonly string[] SupervisorOrWardHeadRoles =
        {
            "SuperAdmin",
            "Supervisor",
            "KepalaRuangan",
            "Kepala Ruangan"
        };

        private readonly InpEpisodeService _episodeService;
        private readonly LoggerService _loggerService;

        public InpatientEpisodeController(
            InpEpisodeService episodeService,
            LoggerService loggerService)
        {
            _episodeService = episodeService;
            _loggerService = loggerService;
        }

        /// <summary>Membuka admisi. Episode lahir <c>Draft</c> dan DPJP pertama ditetapkan.</summary>
        /// <remarks>
        /// Kunjungan boleh ditunjuk, boleh juga dikosongkan. Bila dikosongkan, sistem membuat
        /// kunjungan bertipe rawat inap sendiri — inilah jalur pasien datang langsung, dan
        /// petugas tidak diminta mengisi form kedua.
        ///
        /// <para>
        /// <b>Admisi ganda bukan penolakan.</b> Pasien yang sudah punya admisi lain yang masih
        /// disiapkan tetap dapat diadmisikan, disertai peringatan pada kolom
        /// <c>warnings</c>. Petugas yang memutuskan, bukan sistem: admisi kedua bisa saja
        /// memang disengaja, dan menolaknya akan menyandera pekerjaan yang sah.
        /// </para>
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Inpatient Episode", Description = "Membuka admisi rawat inap", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InpatientEpisode", "Create")]
        public async Task<IActionResult> OpenAdmission(
            [FromBody] OpenAdmissionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.OpenAdmissionAsync(
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientEpisode.OpenAdmission",
                "Membuka admisi rawat inap.",
                new
                {
                    EntityId = result.Episode!.Id,
                    Controller = "InpatientEpisode",
                    Action = "OpenAdmission",
                    StatusCode = StatusCodes.Status200OK
                });

            return await OkWithDetailAsync(
                result.Episode!.Id,
                result.Message,
                result.Warnings,
                cancellationToken);
        }

        /// <summary>Membetulkan isian admisi selagi episode masih <c>Draft</c>.</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Inpatient Episode", Description = "Mengubah isian admisi rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "Update")]
        public async Task<IActionResult> UpdateAdmission(
            Guid id,
            [FromBody] UpdateAdmissionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.UpdateAdmissionAsync(
                id,
                request,
                GetCurrentUserId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientEpisode.UpdateAdmission",
                "Mengubah isian admisi rawat inap.",
                new
                {
                    EntityId = id,
                    Controller = "InpatientEpisode",
                    Action = "UpdateAdmission",
                    StatusCode = StatusCodes.Status200OK
                });

            return await OkWithDetailAsync(id, result.Message, result.Warnings, cancellationToken);
        }

        /// <summary>
        /// Membatalkan admisi. Pemesanan dan penempatan ikut dilepas dalam tindakan yang sama.
        /// </summary>
        /// <remarks>
        /// Alasan wajib diisi, dan alasan yang hanya berisi tanda baca ditolak. Barisnya tidak
        /// dihapus, hanya ditandai batal, sehingga tetap dapat ditelusuri saat diaudit.
        /// </remarks>
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Cancel Inpatient Episode", Description = "Membatalkan admisi rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "Update")]
        public async Task<IActionResult> CancelAdmission(
            Guid id,
            [FromBody] CancelAdmissionRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.CancelAdmissionAsync(
                id,
                request,
                GetCurrentUserId(),
                IsSupervisorOrWardHead(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientEpisode.CancelAdmission",
                "Membatalkan admisi rawat inap.",
                new
                {
                    EntityId = id,
                    Controller = "InpatientEpisode",
                    Action = "CancelAdmission",
                    StatusCode = StatusCodes.Status200OK
                });

            return await OkWithDetailAsync(id, result.Message, result.Warnings, cancellationToken);
        }

        private async Task<IActionResult> OkWithDetailAsync(
            Guid episodeId,
            string message,
            List<string> warnings,
            CancellationToken cancellationToken)
        {
            var detail = await _episodeService.GetDetailResponseAsync(
                episodeId,
                warnings,
                cancellationToken);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Episode rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<InpatientEpisodeDetailResponse>.Ok(detail, message));
        }

        /// <summary>
        /// Menerjemahkan penolakan service menjadi kode status yang sudah ditetapkan kontrak.
        /// Kode tidak boleh ditentukan controller sendiri: aturan bisnislah yang menentukan
        /// apakah sesuatu adalah isian yang kurang, tabrakan keadaan, atau penolakan aturan.
        /// </summary>
        private IActionResult FromFailure(InpEpisodeOperationResult result)
        {
            return result.Status switch
            {
                InpEpisodeOperationStatus.Invalid => BadRequest(
                    ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, result.Message)),

                InpEpisodeOperationStatus.Forbidden => StatusCode(
                    StatusCodes.Status403Forbidden,
                    ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, result.Message)),

                InpEpisodeOperationStatus.NotFound => NotFound(
                    ApiResponse<object>.Fail(StatusCodes.Status404NotFound, result.Message)),

                InpEpisodeOperationStatus.Conflict => Conflict(
                    ApiResponse<object>.Fail(StatusCodes.Status409Conflict, result.Message)),

                _ => StatusCode(
                    StatusCodes.Status422UnprocessableEntity,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        result.Message))
            };
        }

        private bool IsSupervisorOrWardHead()
        {
            return SupervisorOrWardHeadRoles.Any(User.IsInRole);
        }

        private Guid GetCurrentUserId()
        {
            var value =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
