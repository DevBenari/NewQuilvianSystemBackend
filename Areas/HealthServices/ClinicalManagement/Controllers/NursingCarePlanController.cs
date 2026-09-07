using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Rencana asuhan keperawatan satu perawatan rawat inap - <c>CAP-013</c>,
    /// <c>BE-RWI-059</c>, <c>BE-RWI-060</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang dijawab controller ini.</b> Perawat menetapkan masalah keperawatan pasien beserta
    /// tujuan dan rencana tindakannya di dalam sistem, bukan di kertas. Satu perawatan memiliki
    /// tepat satu rencana asuhan; yang banyak adalah butir masalahnya.
    /// </para>
    /// <para>
    /// <b>Bentuknya transaksi, bukan master data.</b> Karena itu tidak ada <c>GET /options</c>,
    /// tidak ada <c>PATCH /{id}/status</c> generik, dan tidak ada <c>DELETE /{id}</c>. Butir
    /// masalah ditutup lewat aksi bernama beserta alasannya, dan barisnya tetap tersimpan -
    /// <c>CAP-013</c> aturan 6 melarang penutupan butir menghapus tindakan dan evaluasi yang
    /// sudah dikerjakan.
    /// </para>
    /// <para>
    /// <b>Controller tidak menyentuh <c>ApplicationDbContext</c>.</b> Seluruh perintah dan
    /// pembacaan dimiliki <c>NursingCarePlanService</c>, sesuai <c>QBE-SVC-001</c>.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/nursing-care-plans")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Nursing Care Plan",
        AreaName = "HealthServices",
        ControllerName = "NursingCarePlan",
        Description = "Rencana asuhan keperawatan pasien rawat inap",
        SortOrder = 6
    )]
    [Tags("Health Services / Clinical Management / Nursing Care Plan")]
    public class NursingCarePlanController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly LoggerService _loggerService;
        private readonly NursingCarePlanService _carePlanService;

        public NursingCarePlanController(
            LoggerService loggerService,
            NursingCarePlanService carePlanService)
        {
            _loggerService = loggerService;
            _carePlanService = carePlanService;
        }

        /// <summary>Membuka rencana asuhan bagi satu perawatan rawat inap.</summary>
        /// <remarks>
        /// <para>
        /// <c>FR-KEP-012</c>. Perawatan yang belum benar-benar <c>Admitted</c> ditolak
        /// <c>422</c>: pasien yang belum masuk kamar belum punya asuhan untuk direncanakan.
        /// Perawatan yang sudah memiliki rencana ditolak <c>409</c>, dan penjaganya berlapis
        /// dua - pemeriksaan di sini serta unique parsial pada database, karena pemeriksaan
        /// aplikasi saja tidak dapat mencegah dua permintaan yang tiba bersamaan.
        /// </para>
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NursingCarePlanResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Nursing Care Plan", Description = "Membuka rencana asuhan keperawatan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("NursingCarePlan", "Create")]
        public async Task<IActionResult> CreateCarePlan(
            [FromBody] CreateNursingCarePlanRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _carePlanService.OpenAsync(request, User, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.CarePlan == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Rencana asuhan tidak dapat dibuka."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "NursingCarePlan.CreateCarePlan",
                "Membuka rencana asuhan keperawatan.",
                new
                {
                    EntityId = hasil.CarePlan.Id,
                    hasil.CarePlan.InpEpisodeId,
                    hasil.CarePlan.OpenedByEmployeeId,
                    Controller = "NursingCarePlan",
                    Action = "Create"
                });

            var response = await _carePlanService.ToResponseAsync(
                hasil.CarePlan, Array.Empty<Models.CliNursingCarePlanItem>(), cancellationToken);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<NursingCarePlanResponse>.Ok(response, "Rencana asuhan berhasil dibuka."));
        }

        /// <summary>Rencana asuhan satu perawatan beserta seluruh butirnya.</summary>
        /// <remarks>
        /// <c>AC-CAP013-03</c>. Pembacaan <b>tidak pernah</b> ditolak karena perawatannya sudah
        /// ditutup: riwayat asuhan pasien tetap terbaca selamanya. Yang ditolak setelah
        /// penutupan hanyalah perubahan.
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NursingCarePlanResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Nursing Care Plan", Description = "Melihat rencana asuhan keperawatan satu perawatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NursingCarePlan", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            CancellationToken cancellationToken = default)
        {
            var response = await _carePlanService.GetByEpisodeAsync(episodeId, cancellationToken);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Perawatan ini belum memiliki rencana asuhan."
                ));
            }

            return Ok(ApiResponse<NursingCarePlanResponse>.Ok(
                response, "Rencana asuhan berhasil diambil."));
        }

        /// <summary>Menambah satu masalah keperawatan beserta tujuan dan rencana tindakannya.</summary>
        /// <remarks>
        /// <para>
        /// <c>FR-KEP-013</c>, <c>FR-KEP-015</c>. Masalah keperawatan ditulis sebagai teks;
        /// katalog SDKI/SLKI/SIKI belum diputuskan dipakai atau tidak (<c>OQ-RI-011</c>),
        /// sehingga layar <b>tidak boleh</b> mengunci bentuknya ke katalog yang belum ada.
        /// </para>
        /// <para>
        /// <c>SourceAssessmentId</c> menjawab <c>AC-CAP013-01</c>: butir dapat dikaitkan ke
        /// temuan pengkajian asalnya, sehingga pembaca berikutnya tahu masalah ini datang dari
        /// mana. Pengkajian milik perawatan lain ditolak <c>400</c> - penjaga salah pasien.
        /// </para>
        /// </remarks>
        [HttpPost("{id:guid}/items")]
        [ProducesResponseType(typeof(ApiResponse<CarePlanItemResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Nursing Care Plan", Description = "Menambah masalah keperawatan pada rencana asuhan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NursingCarePlan", "Update")]
        public async Task<IActionResult> CreateCarePlanItem(
            Guid id,
            [FromBody] CreateCarePlanItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _carePlanService.AddItemAsync(id, request, User, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Item == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Masalah keperawatan tidak dapat ditambahkan."
                ));
            }

            // Isi masalah keperawatan bersifat sensitif dan tidak ikut masuk payload logger.
            await _loggerService.InfoAsync(
                LogCategory,
                "NursingCarePlan.CreateCarePlanItem",
                "Menambah masalah keperawatan pada rencana asuhan.",
                new
                {
                    EntityId = hasil.Item.Id,
                    hasil.Item.CarePlanId,
                    hasil.Item.SourceAssessmentId,
                    Controller = "NursingCarePlan",
                    Action = "Update"
                });

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<CarePlanItemResponse>.Ok(
                    await _carePlanService.ToItemResponseAsync(hasil.Item, cancellationToken),
                    "Masalah keperawatan berhasil ditambahkan."));
        }

        /// <summary>Memperbarui satu butir; versi sebelumnya tersalin, bukan ditimpa.</summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-060</c>, <c>FR-KEP-014</c>, <c>AC-CAP013-02</c>, <c>RWI-AC-177</c>.
        /// Perubahan rencana asuhan adalah <b>perkembangan klinis</b>, bukan pembetulan
        /// kesalahan, sehingga ia memakai mesin versi dan <b>bukan</b> mesin addendum milik
        /// <c>MedicalRecordManagement</c> - <c>RWI-DEC-091</c>. Tidak satu baris addendum pun
        /// terbentuk dari endpoint ini.
        /// </para>
        /// <para>
        /// <b>Contoh nyata.</b> Butir "risiko jatuh tinggi" ditulis Ns. Sari pukul 08.00. Pukul
        /// 15.00 Ns. Dewi memperbaruinya menjadi "risiko jatuh sedang". Yang terjadi: versi 1
        /// tersimpan pada riwayat berbunyi "risiko jatuh tinggi" <b>atas nama Ns. Sari pukul
        /// 08.00</b>, butirnya naik ke versi 2 atas nama Ns. Dewi, dan tidak ada satu pun isi
        /// lama yang hilang.
        /// </para>
        /// </remarks>
        [HttpPut("items/{itemId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CarePlanItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Nursing Care Plan", Description = "Memperbarui butir rencana asuhan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NursingCarePlan", "Update")]
        public async Task<IActionResult> UpdateCarePlanItem(
            Guid itemId,
            [FromBody] UpdateCarePlanItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _carePlanService.UpdateItemAsync(
                itemId, request, User, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Item == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Butir rencana asuhan tidak dapat diperbarui."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "NursingCarePlan.UpdateCarePlanItem",
                "Memperbarui butir rencana asuhan; versi sebelumnya tersalin.",
                new
                {
                    EntityId = hasil.Item.Id,
                    hasil.Item.CarePlanId,
                    hasil.Item.VersionNumber,
                    Controller = "NursingCarePlan",
                    Action = "Update"
                });

            return Ok(ApiResponse<CarePlanItemResponse>.Ok(
                await _carePlanService.ToItemResponseAsync(hasil.Item, cancellationToken),
                "Butir rencana asuhan berhasil diperbarui."));
        }

        /// <summary>Riwayat versi satu butir rencana asuhan.</summary>
        /// <remarks>
        /// <c>BE-RWI-060</c>, <c>AC-CAP013-02</c>. Setiap baris menyebut penulis dan waktu
        /// <b>versi itu sendiri</b>, bukan perawat yang memperbaruinya. Riwayat ini tetap terbaca
        /// setelah perawatan pasien ditutup - <c>AC-CAP013-03</c>.
        /// </remarks>
        [HttpGet("items/{itemId:guid}/revisions")]
        [ProducesResponseType(typeof(ApiResponse<List<CarePlanItemRevisionResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Nursing Care Plan", Description = "Melihat riwayat versi butir rencana asuhan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NursingCarePlan", "Read")]
        public async Task<IActionResult> GetCarePlanItemRevisions(
            Guid itemId,
            CancellationToken cancellationToken = default)
        {
            var revisi = await _carePlanService.GetRevisionsAsync(itemId, cancellationToken);

            if (revisi == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Butir rencana asuhan tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<List<CarePlanItemRevisionResponse>>.Ok(
                revisi, "Riwayat versi butir rencana asuhan berhasil diambil."));
        }

        /// <summary>Mencatat evaluasi hasil asuhan pada satu butir.</summary>
        /// <remarks>
        /// <c>FR-KEP-015</c>. Evaluasi inilah dasar yang dituntut <c>VAL-KEP-16</c> sebelum satu
        /// masalah boleh dinyatakan teratasi.
        /// </remarks>
        [HttpPatch("items/{itemId:guid}/evaluate")]
        [ProducesResponseType(typeof(ApiResponse<CarePlanItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Nursing Care Plan", Description = "Mencatat evaluasi hasil asuhan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NursingCarePlan", "Update")]
        public async Task<IActionResult> EvaluateCarePlanItem(
            Guid itemId,
            [FromBody] EvaluateCarePlanItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _carePlanService.EvaluateItemAsync(
                itemId, request, User, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Item == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Evaluasi tidak dapat dicatat."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "NursingCarePlan.EvaluateCarePlanItem",
                "Mencatat evaluasi hasil asuhan.",
                new
                {
                    EntityId = hasil.Item.Id,
                    hasil.Item.CarePlanId,
                    hasil.Item.LastEvaluatedAt,
                    Controller = "NursingCarePlan",
                    Action = "Update"
                });

            return Ok(ApiResponse<CarePlanItemResponse>.Ok(
                await _carePlanService.ToItemResponseAsync(hasil.Item, cancellationToken),
                "Evaluasi hasil asuhan berhasil dicatat."));
        }

        /// <summary>Menutup satu masalah keperawatan tanpa menghapus jejaknya.</summary>
        /// <remarks>
        /// <para>
        /// <c>VAL-KEP-16</c>, <c>state-transition-matrix.md</c> bagian 2. Menutup butir sebagai
        /// <b>teratasi</b> menuntut sekurang-kurangnya satu catatan evaluasi; tanpa itu
        /// permintaannya ditolak <c>400</c> dan butirnya <b>tetap</b> masih dikerjakan.
        /// Menghentikan butir karena tidak lagi relevan tidak menuntut evaluasi, tetapi tetap
        /// menuntut alasan.
        /// </para>
        /// <para>
        /// <b>Contoh nyata.</b> Ns. Sari hendak menutup masalah "risiko jatuh tinggi" Tn. Budi
        /// sebagai teratasi, padahal belum satu pun evaluasi tercatat. Yang terjadi: permintaan
        /// ditolak dengan kalimat <i>"Butir ini belum punya catatan evaluasi, sehingga belum
        /// dapat dinyatakan tercapai."</i>, dan butirnya masih berkeadaan dikerjakan - bukan
        /// setengah tertutup. Setelah Ns. Sari mencatat evaluasi "pasien mampu berjalan mandiri
        /// tanpa terjatuh selama 3 hari", penutupan barulah diterima.
        /// </para>
        /// </remarks>
        [HttpPatch("items/{itemId:guid}/close")]
        [ProducesResponseType(typeof(ApiResponse<CarePlanItemResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Update", "Update Nursing Care Plan", Description = "Menutup masalah keperawatan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NursingCarePlan", "Update")]
        public async Task<IActionResult> CloseCarePlanItem(
            Guid itemId,
            [FromBody] CloseCarePlanItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _carePlanService.CloseItemAsync(itemId, request, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Item == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Masalah keperawatan tidak dapat ditutup."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "NursingCarePlan.CloseCarePlanItem",
                "Menutup masalah keperawatan.",
                new
                {
                    EntityId = hasil.Item.Id,
                    hasil.Item.CarePlanId,
                    hasil.Item.ItemStatus,
                    hasil.Item.ResolvedAt,
                    Controller = "NursingCarePlan",
                    Action = "Update"
                });

            return Ok(ApiResponse<CarePlanItemResponse>.Ok(
                await _carePlanService.ToItemResponseAsync(hasil.Item, cancellationToken),
                "Masalah keperawatan berhasil ditutup."));
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
