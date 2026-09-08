using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseNursingInterventionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.NursingInterventionListItem>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Catatan tindakan keperawatan pada perawatan rawat inap - <c>CAP-014</c>,
    /// <c>BE-RWI-061</c>, <c>BE-RWI-062</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Yang dijawab controller ini.</b> Perawat mencatat tindakan yang benar-benar dilakukan
    /// beserta waktu dan hasilnya. Jaringan yang buruk tidak lagi menghasilkan tindakan ganda
    /// pada rekam medis: permintaan berulang dengan kunci yang sama menjawab <c>200</c> beserta
    /// baris yang <b>sudah</b> ada.
    /// </para>
    /// <para>
    /// <b>Bentuknya transaksi.</b> Tidak ada <c>GET /options</c>, tidak ada
    /// <c>PATCH /{id}/status</c> generik, dan tidak ada <c>DELETE /{id}</c>. Tindakan yang sudah
    /// dilakukan tidak dihapus; yang salah dibetulkan lewat koreksi bernomor.
    /// </para>
    /// <para>
    /// <b>Controller tidak menyentuh <c>ApplicationDbContext</c></b> - <c>QBE-SVC-001</c>.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/nursing-interventions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Nursing Intervention",
        AreaName = "HealthServices",
        ControllerName = "NursingIntervention",
        Description = "Catatan tindakan keperawatan pasien rawat inap",
        SortOrder = 7
    )]
    [Tags("Health Services / Clinical Management / Nursing Intervention")]
    public class NursingInterventionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        private readonly LoggerService _loggerService;
        private readonly NursingInterventionService _interventionService;

        public NursingInterventionController(
            LoggerService loggerService,
            NursingInterventionService interventionService)
        {
            _loggerService = loggerService;
            _interventionService = interventionService;
        }

        /// <summary>Mencatat satu tindakan keperawatan yang sudah dilakukan.</summary>
        /// <remarks>
        /// <para>
        /// <c>FR-KEP-018</c>, <c>FR-KEP-019</c>. Kunci permintaan boleh dikirim pada badan
        /// permintaan atau lewat header <c>Idempotency-Key</c>.
        /// </para>
        /// <para>
        /// <b>Contoh nyata.</b> Ns. Sari menekan Simpan pada pencatatan pemasangan infus, lalu
        /// jaringan putus sebelum balasan sampai. Ia menekan Simpan lagi. Dengan kunci yang sama,
        /// permintaan kedua dijawab <c>200</c> beserta catatan yang sudah tersimpan - <b>bukan</b>
        /// <c>201</c> yang melahirkan baris kedua, dan <b>bukan</b> <c>409</c> yang membuat layar
        /// menampilkan galat padahal tindakannya sudah tercatat dengan benar.
        /// </para>
        /// <para>
        /// <b>Dua penolakan waktu.</b> Waktu tindakan di masa depan ditolak <c>400</c>
        /// (<c>VAL-KEP-13</c>), dan waktu sebelum pasien masuk kamar juga ditolak <c>400</c>
        /// (<c>VAL-KEP-14</c>). Keduanya menjaga lini masa pasien tetap terbaca seperti yang
        /// benar-benar terjadi.
        /// </para>
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<NursingInterventionResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<NursingInterventionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Nursing Intervention", Description = "Mencatat tindakan keperawatan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("NursingIntervention", "Create")]
        public async Task<IActionResult> CreateNursingIntervention(
            [FromBody] CreateNursingInterventionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _interventionService.RecordAsync(
                request,
                ResolveIdempotencyKey(request.IdempotencyKey),
                User,
                actorUserId,
                cancellationToken);

            if (!hasil.IsSuccess || hasil.Intervention == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Tindakan keperawatan tidak dapat dicatat."
                ));
            }

            // Hasil tindakan bersifat sensitif dan tidak ikut masuk payload logger.
            await _loggerService.InfoAsync(
                LogCategory,
                "NursingIntervention.CreateNursingIntervention",
                hasil.IsReplay
                    ? "Kiriman ulang tindakan keperawatan dengan kunci yang sama."
                    : "Mencatat tindakan keperawatan.",
                new
                {
                    EntityId = hasil.Intervention.Id,
                    hasil.Intervention.InpEpisodeId,
                    hasil.Intervention.PerformedByEmployeeId,
                    hasil.IsReplay,
                    Controller = "NursingIntervention",
                    Action = "Create"
                });

            var response = await _interventionService.ToResponseAsync(
                hasil.Intervention, hasil.IsReplay, cancellationToken);

            return StatusCode(hasil.StatusCode,
                ApiResponse<NursingInterventionResponse>.Ok(
                    response,
                    hasil.IsReplay
                        ? "Tindakan keperawatan sudah tercatat sebelumnya."
                        : "Tindakan keperawatan berhasil dicatat."));
        }

        /// <summary>Daftar tindakan satu perawatan, terurut waktu tindakan.</summary>
        /// <remarks>
        /// <c>FR-KEP-020</c>. Terurut menurut <b>waktu tindakan</b>, bukan waktu pencatatan,
        /// supaya perkembangan pasien terbaca seperti yang benar-benar terjadi.
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResponseNursingInterventionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nursing Intervention", Description = "Melihat daftar tindakan keperawatan satu perawatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NursingIntervention", "Read")]
        public async Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] Guid? performedBy = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _interventionService.GetByEpisodeAsync(
                episodeId, from, to, performedBy, pageNumber, pageSize, cancellationToken);

            return Ok(ApiResponse<ResponseNursingInterventionPagedResult>.Ok(
                hasil, "Daftar tindakan keperawatan berhasil diambil."));
        }

        // =====================================================================
        // BE-RWI-062 - penyuntingan, finalisasi, koreksi, dan keadaan tagihan
        // =====================================================================

        /// <summary>Menyunting catatan tindakan selama catatannya belum final.</summary>
        /// <remarks>
        /// <c>VAL-KEP-06</c>, <c>state-transition-matrix.md</c> bagian 3. Hanya penulisnya yang
        /// boleh menyunting, dan hanya sebelum catatan final. Percobaan menyunting catatan yang
        /// sudah final ditolak dengan arahan memakai koreksi - isi aslinya tidak boleh berubah
        /// sedikit pun.
        /// </remarks>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<NursingInterventionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Nursing Intervention", Description = "Menyunting catatan tindakan keperawatan yang belum final", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NursingIntervention", "Update")]
        public async Task<IActionResult> UpdateNursingIntervention(
            Guid id,
            [FromBody] UpdateNursingInterventionRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _interventionService.UpdateAsync(id, request, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Intervention == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Catatan tindakan tidak dapat diubah."
                ));
            }

            return Ok(ApiResponse<NursingInterventionResponse>.Ok(
                await _interventionService.ToResponseAsync(hasil.Intervention, false, cancellationToken),
                "Catatan tindakan keperawatan berhasil diubah."));
        }

        /// <summary>Menyatakan catatan tindakan final sehingga tidak dapat disunting diam-diam.</summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-062</c>, <c>AC-CAP014-03</c>, <c>INT-KEP-06</c>. Perpindahan ini sekaligus
        /// <b>mendaftarkan</b> catatan ke mesin keutuhan rekam medis sebagai dokumen
        /// <c>Procedure</c> tertanda tangan, dalam <c>SaveChanges</c> yang sama. Bila
        /// pendaftaran gagal, finalisasi ikut batal dan catatannya tetap berkeadaan tercatat.
        /// </para>
        /// <para>
        /// <b>Keadaan tagihan tidak ikut berubah.</b> Kedua mesin status hidup berdampingan dan
        /// tidak saling mengunci - <c>AC-CAP014-02</c>.
        /// </para>
        /// </remarks>
        [HttpPatch("{id:guid}/finalize")]
        [ProducesResponseType(typeof(ApiResponse<NursingInterventionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Nursing Intervention", Description = "Menyatakan catatan tindakan keperawatan final", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("NursingIntervention", "Update")]
        public async Task<IActionResult> FinalizeNursingIntervention(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _interventionService.FinalizeAsync(
                id,
                actorUserId,
                deviceInfo: Request.Headers.UserAgent.ToString(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            if (!hasil.IsSuccess || hasil.Intervention == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Catatan tindakan tidak dapat difinalkan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "NursingIntervention.FinalizeNursingIntervention",
                "Menyatakan catatan tindakan keperawatan final.",
                new
                {
                    EntityId = hasil.Intervention.Id,
                    hasil.Intervention.InpEpisodeId,
                    hasil.Intervention.FinalizedAt,
                    hasil.Intervention.FinalizedByUserId,
                    Controller = "NursingIntervention",
                    Action = "Update"
                });

            return Ok(ApiResponse<NursingInterventionResponse>.Ok(
                await _interventionService.ToResponseAsync(hasil.Intervention, false, cancellationToken),
                hasil.IsReplay
                    ? "Catatan tindakan keperawatan sudah final sebelumnya."
                    : "Catatan tindakan keperawatan berhasil difinalkan."));
        }

        /// <summary>Menambah koreksi pada catatan tindakan yang sudah final.</summary>
        /// <remarks>
        /// <para>
        /// <c>BE-RWI-062</c>, <c>RWI-DEC-091</c>, <c>RWI-AC-176</c>. Isi asli <b>tidak berubah
        /// sedikit pun</b>, dan status catatan <b>tetap</b> final sesudah berapa kali pun
        /// dikoreksi. Nilai status <c>Amended</c> sudah dicabut kontrak <c>0.3.0</c>.
        /// </para>
        /// <para>
        /// <b>Endpoint ini tidak menyimpan apa pun sendiri.</b> Ia meneruskan ke
        /// <c>ClinicalNoteAddendumService</c> milik <c>MedicalRecordManagement</c> dengan jenis
        /// dokumen <c>Procedure</c>.
        /// </para>
        /// <para>
        /// Koreksi atas nama penulis lain - misalnya oleh kepala ruangan ketika penulisnya
        /// berhalangan - memakai endpoint pengganti milik <c>MedicalRecordManagement</c>, karena
        /// aturan pengganti beserta penetapan berhalangannya dimiliki modul itu.
        /// </para>
        /// </remarks>
        [HttpPost("{id:guid}/addendums")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAddendumResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Amend", "Amend Nursing Intervention", Description = "Menambahkan koreksi pada catatan tindakan yang sudah final", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("NursingIntervention", "Amend")]
        public async Task<IActionResult> CreateAddendum(
            Guid id,
            [FromBody] CreateInterventionAddendumRequest request,
            CancellationToken cancellationToken = default)
        {
            var tindakan = await _interventionService.FindAsync(id, cancellationToken);

            if (tindakan == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan tindakan keperawatan tidak ditemukan."
                ));
            }

            var actorUserId = GetCurrentUserId();

            var (hasil, addendum) = await _interventionService.CreateAddendumAsync(
                id,
                request,
                actorUserId,
                deviceInfo: Request.Headers.UserAgent.ToString(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                cancellationToken);

            if (!hasil.IsAllowed || addendum == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Koreksi tidak dapat ditambahkan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "NursingIntervention.CreateAddendum",
                "Menambahkan koreksi pada catatan tindakan yang sudah final.",
                new
                {
                    EntityId = id,
                    addendum.IntegrityId,
                    addendum.Sequence,
                    Controller = "NursingIntervention",
                    Action = "Amend"
                });

            var nama = await _interventionService.GetAuthorNamesAsync(
                new[] { addendum.AuthorUserId }, cancellationToken);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<ClinicalNoteAddendumResponse>.Ok(
                    ToAddendumResponse(addendum, nama),
                    "Koreksi berhasil ditambahkan."));
        }

        /// <summary>Daftar koreksi satu catatan tindakan, terurut nomor.</summary>
        /// <remarks>
        /// <c>RWI-AC-176</c>. Dibaca ruang kerja keperawatan supaya isi asli dan koreksinya
        /// tampil bersebelahan - pembaca berikutnya melihat keduanya, bukan hanya yang terakhir.
        /// </remarks>
        [HttpGet("{id:guid}/addendums")]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalNoteAddendumResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Nursing Intervention", Description = "Melihat koreksi satu catatan tindakan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NursingIntervention", "Read")]
        public async Task<IActionResult> GetAddendums(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var tindakan = await _interventionService.FindAsync(id, cancellationToken);

            if (tindakan == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan tindakan keperawatan tidak ditemukan."
                ));
            }

            var daftar = await _interventionService.ListAddendumsAsync(id, cancellationToken);

            var nama = await _interventionService.GetAuthorNamesAsync(
                daftar.Select(x => x.AuthorUserId).ToList(), cancellationToken);

            return Ok(ApiResponse<List<ClinicalNoteAddendumResponse>>.Ok(
                daftar.Select(x => ToAddendumResponse(x, nama)).ToList(),
                "Daftar koreksi catatan tindakan berhasil diambil."));
        }

        /// <summary>Keadaan pengiriman tagihan satu tindakan.</summary>
        /// <remarks>
        /// <para>
        /// <c>AC-CAP014-02</c>, <c>INT-KEP-05</c>. Balasannya sengaja memuat <b>kedua</b> mesin
        /// status sekaligus, supaya pembaca langsung melihat bahwa keduanya tidak saling
        /// mengunci.
        /// </para>
        /// <para>
        /// <b>Contoh nyata.</b> Ns. Sari memasang infus pukul 02.00 saat sistem tagihan sedang
        /// mati. Yang terbaca di sini: catatan klinis berkeadaan <c>Recorded</c> atau
        /// <c>Finalized</c>, penanda pengiriman <c>Failed</c>, beserta kalimat yang menegaskan
        /// bahwa catatan klinisnya tetap tersimpan dan pengirimannya dapat dicoba ulang.
        /// </para>
        /// </remarks>
        [HttpGet("{id:guid}/billing-dispatch")]
        [ProducesResponseType(typeof(ApiResponse<BillingDispatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Nursing Intervention", Description = "Melihat keadaan pengiriman tagihan satu tindakan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NursingIntervention", "Read")]
        public async Task<IActionResult> GetBillingDispatch(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _interventionService.GetBillingDispatchAsync(id, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan tindakan keperawatan tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<BillingDispatchResponse>.Ok(
                hasil, "Keadaan pengiriman tagihan berhasil diambil."));
        }

        private static ClinicalNoteAddendumResponse ToAddendumResponse(
            MrcClinicalNoteAddendum addendum,
            IReadOnlyDictionary<Guid, string?> nama) => new()
            {
                Id = addendum.Id,
                IntegrityId = addendum.IntegrityId,
                Sequence = addendum.Sequence,
                AuthorUserId = addendum.AuthorUserId,
                AuthorName = nama.GetValueOrDefault(addendum.AuthorUserId),
                IsSubstituteAuthor = addendum.IsSubstituteAuthor,
                DelegationId = addendum.DelegationId,
                AddendumText = addendum.AddendumText,
                CorrectionReason = addendum.CorrectionReason,
                SignedAt = addendum.SignedAt
            };

        /// <summary>
        /// Kunci permintaan dari badan permintaan, atau dari header <c>Idempotency-Key</c>.
        /// </summary>
        /// <remarks>
        /// Kosong dibiarkan kosong, bukan diisi nilai acak. Kunci acak yang dibuat server tidak
        /// menjaga apa pun: kiriman ulang akan membawa kunci baru dan tetap melahirkan baris
        /// kedua.
        /// </remarks>
        private string? ResolveIdempotencyKey(string? dariBadanPermintaan)
        {
            if (!string.IsNullOrWhiteSpace(dariBadanPermintaan))
                return dariBadanPermintaan.Trim();

            var dariHeader = Request.Headers["Idempotency-Key"].ToString();

            return string.IsNullOrWhiteSpace(dariHeader) ? null : dariHeader.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
