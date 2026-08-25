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
    /// Layar episode rawat inap: membuka admisi, membetulkan isian, membatalkan, membaca
    /// daftar dan detail, mengalihkan DPJP, menugaskan perawat penanggung jawab, dan
    /// menetapkan kebutuhan isolasi.
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
    /// <b>Dua endpoint yang masih belum ada di sini.</b> Riwayat status
    /// (<c>GET /{id}/status-history</c>) milik <c>BE-RWI-028</c>, dan sesi koreksi milik
    /// <c>BE-RWI-030</c>. Keduanya tercantum pada api contract dan sengaja belum dibuka.
    /// </para>
    ///
    /// <para>
    /// <b>Kolom sensitif hanya pada detail.</b> Catatan admisi dan keterangan kebutuhan
    /// isolasi memuat informasi klinis. Keduanya muncul pada <c>GET /{id}</c> saja, tidak
    /// pernah pada daftar, dan tidak pernah masuk payload logger — permission matrix bagian
    /// 5.4.
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

        private readonly InpEpisodeService _episodeService;
        private readonly LoggerService _loggerService;

        public InpatientEpisodeController(
            InpEpisodeService episodeService,
            LoggerService loggerService)
        {
            _episodeService = episodeService;
            _loggerService = loggerService;
        }

        // =====================================================================
        // BE-RWI-009 — Daftar, detail, ringkasan, dan metadata penyaring
        // =====================================================================

        /// <summary>Mengambil pilihan penyaring beserta nilai bawaannya untuk layar daftar.</summary>
        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Episode", Description = "Melihat metadata filter episode rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.GetFilterMetadataAsync(cancellationToken);

            return Ok(ApiResponse<InpatientEpisodeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter episode rawat inap berhasil diambil."));
        }

        /// <summary>Ringkasan jumlah episode per status, memakai penyaring yang sama dengan daftar.</summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Episode", Description = "Melihat ringkasan episode rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] InpatientEpisodeListQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.GetEpisodeSummaryAsync(query, cancellationToken);

            return Ok(ApiResponse<InpatientEpisodeSummaryResponse>.Ok(
                result,
                "Ringkasan episode rawat inap berhasil diambil."));
        }

        /// <summary>
        /// Daftar episode, dapat disaring unit layanan, kelas perawatan, status, rentang
        /// tanggal, kebutuhan isolasi, dan nama pasien.
        /// </summary>
        /// <remarks>
        /// Pembacaan ini menjalankan perhitungan kedaluwarsa episode <c>Draft</c> lebih dulu,
        /// sehingga admisi yang sudah telantar melewati batas tidak lagi muncul sebagai
        /// admisi yang masih disiapkan.
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Episode", Description = "Melihat daftar episode rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetAll(
            [FromQuery] InpatientEpisodeListQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.GetEpisodeListAsync(query, cancellationToken);

            return Ok(ApiResponse<InpatientEpisodePagedResult>.Ok(
                result,
                "Daftar episode rawat inap berhasil diambil."));
        }

        /// <summary>
        /// Detail satu episode beserta DPJP aktif, perawat aktif, dan lokasi terkininya.
        /// </summary>
        /// <remarks>
        /// Lokasi terkini dibaca dari <c>InpBedPlacement</c> yang masih aktif, <b>bukan</b>
        /// dari kolom pada episode. Tidak ada kolom lokasi terakhir pada <c>InpEpisode</c>,
        /// dan tidak boleh ditambahkan walaupun query-nya lebih murah.
        /// </remarks>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Episode", Description = "Melihat detail episode rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var detail = await _episodeService.GetEpisodeDetailAsync(id, cancellationToken);

            if (detail == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Episode rawat inap tidak ditemukan."));
            }

            return Ok(ApiResponse<InpatientEpisodeDetailResponse>.Ok(
                detail,
                "Detail episode rawat inap berhasil diambil."));
        }

        // =====================================================================
        // BE-RWI-007 dan BE-RWI-008 — Membuka, mengubah, dan membatalkan admisi
        // =====================================================================

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
                User.GetUserId(),
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
                User.GetUserId(),
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
                User.GetUserId(),
                User.IsSupervisorOrWardHead(),
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

        // =====================================================================
        // BE-RWI-014 — Kebutuhan isolasi
        // =====================================================================

        /// <summary>Menetapkan atau mengubah kebutuhan isolasi episode.</summary>
        /// <remarks>
        /// Butir hak akses <c>SetIsolation</c> menjawab "boleh" untuk petugas admisi maupun
        /// dokter mana pun. Yang membedakan keduanya adalah status episode dan siapa DPJP
        /// aktifnya, dan itu diperiksa <c>GUARD-INP-04</c> di dalam service.
        ///
        /// <para>
        /// Payload logger sengaja tidak memuat <c>IsolationNote</c>; kolom itu bertanda
        /// sensitif karena memuat alasan klinis kebutuhan isolasi seorang pasien.
        /// </para>
        /// </remarks>
        [HttpPatch("{id:guid}/isolation-requirement")]
        [ProducesResponseType(typeof(ApiResponse<InpatientEpisodeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Set Inpatient Isolation Requirement", Description = "Menetapkan kebutuhan isolasi episode rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "SetIsolation")]
        public async Task<IActionResult> SetIsolationRequirement(
            Guid id,
            [FromBody] SetIsolationRequirementRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.SetIsolationRequirementAsync(
                id,
                request,
                User.GetUserId(),
                User.GetDoctorId(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientEpisode.SetIsolationRequirement",
                "Menetapkan kebutuhan isolasi episode rawat inap.",
                new
                {
                    EntityId = id,
                    Controller = "InpatientEpisode",
                    Action = "SetIsolationRequirement",
                    StatusCode = StatusCodes.Status200OK
                });

            return await OkWithDetailAsync(id, result.Message, result.Warnings, cancellationToken);
        }

        // =====================================================================
        // BE-RWI-017 — Penugasan DPJP
        // =====================================================================

        /// <summary>Mengalihkan DPJP. Menutup penugasan lama dan membuka penugasan baru.</summary>
        [HttpPost("{id:guid}/doctor-assignments")]
        [ProducesResponseType(typeof(ApiResponse<InpatientDoctorAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Handover Inpatient Doctor", Description = "Mengalihkan DPJP episode rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "Update")]
        public async Task<IActionResult> HandoverDoctor(
            Guid id,
            [FromBody] HandoverDoctorRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.HandoverDoctorAsync(
                id,
                request,
                User.GetUserId(),
                User.IsSupervisorOrWardHead(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientEpisode.HandoverDoctor",
                "Mengalihkan DPJP episode rawat inap.",
                new
                {
                    EntityId = id,
                    Controller = "InpatientEpisode",
                    Action = "HandoverDoctor",
                    StatusCode = StatusCodes.Status200OK
                });

            var assignments = await _episodeService.GetDoctorAssignmentsAsync(id, cancellationToken);
            var current = assignments.LastOrDefault(x => x.IsCurrent);

            return Ok(ApiResponse<InpatientDoctorAssignmentResponse>.Ok(current, result.Message));
        }

        /// <summary>Riwayat DPJP episode, urut nomor urut.</summary>
        [HttpGet("{id:guid}/doctor-assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<InpatientDoctorAssignmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Episode", Description = "Melihat riwayat DPJP episode rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetDoctorAssignments(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.GetDoctorAssignmentsAsync(id, cancellationToken);

            return Ok(ApiResponse<List<InpatientDoctorAssignmentResponse>>.Ok(
                result,
                "Riwayat DPJP berhasil diambil."));
        }

        // =====================================================================
        // BE-RWI-018 — Penugasan perawat penanggung jawab
        // =====================================================================

        /// <summary>Menugaskan atau mengganti perawat penanggung jawab.</summary>
        [HttpPost("{id:guid}/nurse-assignments")]
        [ProducesResponseType(typeof(ApiResponse<InpatientNurseAssignmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Assign Inpatient Nurse", Description = "Menugaskan perawat penanggung jawab episode rawat inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientEpisode", "Update")]
        public async Task<IActionResult> AssignNurse(
            Guid id,
            [FromBody] AssignNurseRequest request,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.AssignNurseAsync(
                id,
                request,
                User.GetUserId(),
                User.IsSupervisorOrWardHead(),
                cancellationToken);

            if (result.Status != InpEpisodeOperationStatus.Success)
            {
                return FromFailure(result);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientEpisode.AssignNurse",
                "Menugaskan perawat penanggung jawab episode rawat inap.",
                new
                {
                    EntityId = id,
                    Controller = "InpatientEpisode",
                    Action = "AssignNurse",
                    StatusCode = StatusCodes.Status200OK
                });

            var assignments = await _episodeService.GetNurseAssignmentsAsync(id, cancellationToken);
            var current = assignments.LastOrDefault(x => x.IsCurrent);

            return Ok(ApiResponse<InpatientNurseAssignmentResponse>.Ok(current, result.Message));
        }

        /// <summary>Riwayat perawat penanggung jawab, urut nomor urut.</summary>
        [HttpGet("{id:guid}/nurse-assignments")]
        [ProducesResponseType(typeof(ApiResponse<List<InpatientNurseAssignmentResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Episode", Description = "Melihat riwayat perawat penanggung jawab", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientEpisode", "Read")]
        public async Task<IActionResult> GetNurseAssignments(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var result = await _episodeService.GetNurseAssignmentsAsync(id, cancellationToken);

            return Ok(ApiResponse<List<InpatientNurseAssignmentResponse>>.Ok(
                result,
                "Riwayat perawat penanggung jawab berhasil diambil."));
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

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
    }
}
