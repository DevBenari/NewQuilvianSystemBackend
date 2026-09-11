using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/radiology-management/rad-reports")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_RADIOLOGY_MANAGEMENT",
        moduleName: "Health Service Radiology Management",
        displayName: "Rad Report",
        AreaName = "HealthServices",
        ControllerName = "RadReport",
        Description = "Penulisan, pengesahan, dan perilisan hasil bacaan radiologi",
        SortOrder = 6
    )]
    [Tags("Health Services / Radiology Management / Rad Report")]
    public class RadReportController : ControllerBase
    {
        private readonly RadReportService _radReportService;

        public RadReportController(RadReportService radReportService)
        {
            _radReportService = radReportService;
        }

        /* ================================================================ *
         * Pembacaan
         * ================================================================ */

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<RadReportFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat daftar pilihan penyaring hasil bacaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = _radReportService.GetFilterMetadata();

            return Ok(ApiResponse<RadReportFilterMetadataResponse>.Ok(
                hasil, "Metadata filter hasil bacaan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<RadReportSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat rekap hasil bacaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken = default)
        {
            var hasil = await _radReportService.GetSummaryAsync(cancellationToken);

            var pesan = hasil.BelumDirilis == 0
                ? "Seluruh hasil bacaan sudah sampai ke dokter pengirim."
                : $"{hasil.BelumDirilis} hasil bacaan belum sampai ke dokter pengirim.";

            return Ok(ApiResponse<RadReportSummaryResponse>.Ok(hasil, pesan));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<RadReportListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat daftar hasil bacaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public async Task<IActionResult> GetList(
            [FromQuery] string? search = null,
            [FromQuery] Guid? radStudyId = null,
            [FromQuery] Guid? radOrderId = null,
            [FromQuery] Guid? encounterId = null,
            [FromQuery] RadReportStatus? reportStatus = null,
            [FromQuery] bool? belumDirilis = null,
            [FromQuery] string? sortBy = null,
            [FromQuery] string? sortDirection = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radReportService.GetPagedAsync(
                new RadReportPagedQuery
                {
                    Search = search,
                    RadStudyId = radStudyId,
                    RadOrderId = radOrderId,
                    EncounterId = encounterId,
                    ReportStatus = reportStatus,
                    BelumDirilis = belumDirilis,
                    SortBy = sortBy,
                    SortDirection = sortDirection,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                },
                cancellationToken);

            return Ok(ApiResponse<PagedResult<RadReportListResponse>>.Ok(
                hasil, "Daftar hasil bacaan berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat satu hasil bacaan beserta versi yang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public Task<IActionResult> GetById(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.GetByIdAsync(id, cancellationToken),
                "Rincian hasil bacaan berhasil diambil.");

        // Riwayat versi. Hanya bertambah — tidak ada endpoint ubah maupun hapus versi, dan
        // tidak boleh ada. Riwayat klinis yang dapat dihapus tidak dapat dipakai menjawab
        // "apa yang dibaca dokter itu waktu itu", dan itu pertanyaan pertama ketika sebuah
        // keputusan klinis ditinjau ulang.
        [HttpGet("{id:guid}/versions")]
        [ProducesResponseType(typeof(ApiResponse<List<RadReportVersionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat seluruh versi satu hasil bacaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public Task<IActionResult> GetVersions(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.GetVersionsAsync(id, cancellationToken),
                "Riwayat versi hasil bacaan berhasil diambil.");

        [HttpGet("by-study/{radStudyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat hasil bacaan atas satu pemeriksaan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public Task<IActionResult> GetByStudy(
            Guid radStudyId,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.GetByStudyAsync(radStudyId, cancellationToken),
                "Hasil bacaan pemeriksaan berhasil diambil.");

        // Dipakai rekam medis — RAD-DEC-006, RAD-INT-001 bagian 2. Dua hal yang menentukan:
        //
        // 1. Rekam medis MEMBACA, tidak menyalin. Tidak ada endpoint yang menyerahkan isi bacaan
        //    untuk disimpan modul lain, dan tidak boleh ada — salinan yang lupa diperbarui
        //    membuat dokter membaca bacaan yang sudah diralat.
        // 2. Bacaan yang belum pernah dirilis TIDAK ikut terbawa (FR-RAD-032). Penyaringnya ada
        //    di service, bukan di sini.
        //
        // Kunjungan tanpa bacaan dijawab 200 dengan daftar kosong, bukan 404: belum adanya
        // bacaan adalah keadaan yang wajar, dan membedakannya dari kunjungan yang tidak ada
        // justru yang penting bagi pembaca rekam medis.
        [HttpGet("by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<RadReportListResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Rad Report", Description = "Melihat seluruh hasil bacaan satu kunjungan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("RadReport", "Read")]
        public async Task<IActionResult> GetByEncounter(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _radReportService.GetByEncounterAsync(encounterId, cancellationToken);

            var pesan = hasil.Count == 0
                ? "Kunjungan ini belum memiliki hasil bacaan radiologi yang sudah dirilis."
                : $"{hasil.Count} hasil bacaan radiologi ditemukan pada kunjungan ini.";

            return Ok(ApiResponse<List<RadReportListResponse>>.Ok(hasil, pesan));
        }

        /* ================================================================ *
         * Penulisan
         * ================================================================ */

        [HttpPost("by-study/{radStudyId:guid}/draft")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Rad Report", Description = "Menulis draf hasil bacaan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("RadReport", "Create")]
        public Task<IActionResult> CreateDraft(
            Guid radStudyId,
            [FromBody] CreateRadReportDraftRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.CreateDraftAsync(radStudyId, request, cancellationToken),
                "Draf hasil bacaan berhasil disimpan.");

        [HttpPut("{id:guid}/draft")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Rad Report", Description = "Mengubah draf hasil bacaan yang belum disahkan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("RadReport", "Update")]
        public Task<IActionResult> UpdateDraft(
            Guid id,
            [FromBody] UpdateRadReportDraftRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.UpdateDraftAsync(id, request, cancellationToken),
                "Draf hasil bacaan berhasil diperbarui.");

        /* ================================================================ *
         * Pengesahan dan rilis
         * ================================================================ */

        // Pengesahan. Penanda hak akses di bawah ini hanya menjawab "boleh MENCOBA mengesahkan".
        // Ia tidak pernah membandingkan siapa yang menulis draf yang sedang disahkan, sehingga
        // aturan RAD-DEC-003 wajib diperiksa ulang di dalam service — dan memang diperiksa di
        // sana. Menyandarkan aturan itu pada atribut ini saja akan membuat residen dapat
        // mengesahkan drafnya sendiri begitu ia diberi hak Validate untuk keperluan lain.
        [HttpPost("{id:guid}/validate")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Validate", "Validate Rad Report", Description = "Mengesahkan hasil bacaan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("RadReport", "Validate")]
        public Task<IActionResult> Validate(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.ValidateAsync(id, cancellationToken),
                "Hasil bacaan berhasil disahkan.");

        [HttpPost("{id:guid}/release")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Release", "Release Rad Report", Description = "Merilis hasil bacaan ke dokter pengirim", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("RadReport", "Release")]
        public Task<IActionResult> Release(
            Guid id,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.ReleaseAsync(id, cancellationToken),
                "Hasil bacaan berhasil dirilis ke dokter pengirim.");

        /* ================================================================ *
         * Koreksi berversi
         * ================================================================ */

        // Koreksi tidak pernah menimpa. Endpoint ini hanya MENAMBAH satu versi baru yang
        // menunjuk versi yang digantikannya; isi versi lama tidak disentuh, dan statusnya baru
        // berpindah menjadi Superseded ketika koreksinya benar-benar dirilis.
        //
        // Tidak ada PUT maupun DELETE atas versi mana pun pada controller ini, dan itu
        // disengaja — RJ-BIL-GATE-DEC-004.
        [HttpPost("{id:guid}/amendments")]
        [ProducesResponseType(typeof(ApiResponse<RadReportDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Amend", "Amend Rad Report", Description = "Menulis draf koreksi atas hasil bacaan yang sudah dirilis", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission("RadReport", "Amend")]
        public Task<IActionResult> CreateAmendment(
            Guid id,
            [FromBody] CreateRadReportAmendmentRequest request,
            CancellationToken cancellationToken = default) =>
            Execute(() => _radReportService.CreateAmendmentAsync(id, request, cancellationToken),
                "Draf koreksi hasil bacaan berhasil disimpan.");

        // Memetakan hasil tindakan menjadi status HTTP sesuai RAD-API-001 bagian 4. Bentuknya
        // disamakan persis dengan RadSafetyRuleController supaya kedua controller radiologi
        // tidak menjawab keadaan yang sama dengan kode yang berbeda.
        //
        // Forbidden menjadi 403 dan bukan 400: tidak ada perbaikan isian yang dapat menolong,
        // yang salah adalah siapa yang menekan tombolnya. BusinessRule menjadi 422: bentuk
        // permintaannya benar, keadaan data yang dirujuknya yang menolak.
        private async Task<IActionResult> Execute<T>(
            Func<Task<RadOperationResult<T>>> action,
            string successMessage)
        {
            var result = await action();

            return result.Kind switch
            {
                RadOperationResultKind.Success =>
                    Ok(ApiResponse<T>.Ok(result.Value, successMessage)),

                RadOperationResultKind.NotFound =>
                    NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        result.ErrorMessage ?? "Data tidak ditemukan.",
                        new { Code = result.ErrorCode })),

                RadOperationResultKind.Forbidden =>
                    StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                        StatusCodes.Status403Forbidden,
                        result.ErrorMessage ?? "Anda tidak berwenang melakukan tindakan ini.",
                        new { Code = result.ErrorCode })),

                RadOperationResultKind.Conflict =>
                    Conflict(ApiResponse<object>.Fail(
                        StatusCodes.Status409Conflict,
                        result.ErrorMessage ?? "Tindakan bertabrakan dengan keadaan data saat ini.",
                        new { Code = result.ErrorCode })),

                RadOperationResultKind.BusinessRule or
                RadOperationResultKind.SafetyBlocked or
                RadOperationResultKind.PolicyNotConfigured =>
                    UnprocessableEntity(ApiResponse<object>.Fail(
                        StatusCodes.Status422UnprocessableEntity,
                        result.ErrorMessage ?? "Prasyarat belum terpenuhi.",
                        new { Code = result.ErrorCode })),

                _ => BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    result.ErrorMessage ?? "Permintaan tidak valid.",
                    new { Code = result.ErrorCode })),
            };
        }
    }
}
