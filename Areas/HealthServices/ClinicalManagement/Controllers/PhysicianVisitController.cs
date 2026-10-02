using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePhysicianVisitPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs.PhysicianVisitListItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Kejadian kunjungan dokter ke pasien — <c>CAP-025</c>, <c>BE-RWI-048</c>,
    /// <c>BE-RWI-049</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Kenapa visite punya controller sendiri.</b> Menghitung visite dari catatan yang
    /// ditulis dokter dilarang <c>INV-DOK-07</c>. Dokter yang mendatangi pasien tetapi belum
    /// sempat menulis apa pun tetap benar-benar datang, dan dokter yang menulis tiga catatan
    /// dalam satu kunjungan tetap datang sekali. Kejadian dan catatan karena itu dua hal
    /// berbeda, dan hitungan visite hanya boleh diturunkan dari kejadian.
    /// </para>
    /// <para>
    /// <b>Tidak ada jalur menyunting waktu maupun peran, dan tidak ada penghapusan.</b> Kejadian
    /// menyatakan fakta kedatangan; mengubah waktunya berarti fakta yang berbeda —
    /// <c>RWI-DEC-085</c>. Koreksi dilakukan dengan membatalkan kejadian yang salah beserta
    /// alasannya, lalu mencatat kejadian baru yang menunjuk kejadian yang digantikannya. Yang
    /// dibatalkan <b>tetap tersimpan</b> dan tetap tampil pada riwayat — <c>INV-DOK-08</c>.
    /// </para>
    /// <para>
    /// <b>Bentuk endpoint mengikuti tetangganya.</b> Pembatalan memakai
    /// <c>PATCH /{id}/cancel</c>, sama seperti <c>DoctorConsultation</c>,
    /// <c>PatientAssessment</c>, dan <c>PatientProcedure</c> pada modul yang sama, dan sama
    /// seperti <c>api-contract.md</c> bagian 4 yang disetujui.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/physician-visits")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Physician Visit",
        AreaName = "HealthServices",
        ControllerName = "PhysicianVisit",
        Description = "Kejadian kunjungan dokter ke pasien rawat inap",
        SortOrder = 5
    )]
    [Tags("Health Services / Clinical Management / Physician Visit")]
    public class PhysicianVisitController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Clinical";

        /// <summary>
        /// Kalimat penolakan <c>VAL-DOK-08</c>, apa adanya seperti pada validation matrix.
        /// </summary>
        private const string PenolakanBukanDokter = "Visite hanya dapat dicatat dokter.";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly PhysicianVisitService _physicianVisitService;
        private readonly InpatientClinicalContextService _inpatientClinicalContextService;

        public PhysicianVisitController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            PhysicianVisitService physicianVisitService,
            InpatientClinicalContextService inpatientClinicalContextService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _physicianVisitService = physicianVisitService;
            _inpatientClinicalContextService = inpatientClinicalContextService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Physician Visit", Description = "Melihat metadata filter kejadian visite dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PhysicianVisit", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = new PhysicianVisitFilterMetadataResponse
            {
                DefaultFilter = new PhysicianVisitDefaultFilterResponse(),
                SortOptions =
                [
                    new() { Value = "visitDateTime", Label = "Waktu kedatangan" },
                    new() { Value = "createDateTime", Label = "Waktu pencatatan" }
                ],
                SortDirections = ["asc", "desc"],
                PageSizeOptions = [10, 25, 50, 100],
                VisitRoleOptions = Enum.GetValues<PhysicianVisitRole>()
                    .Select(x => new PhysicianVisitEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = NamaPeran(x)
                    })
                    .ToList(),
                VisitStatusOptions = Enum.GetValues<PhysicianVisitStatus>()
                    .Select(x => new PhysicianVisitEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = NamaStatus(x)
                    })
                    .ToList()
            };

            return Ok(ApiResponse<PhysicianVisitFilterMetadataResponse>.Ok(
                hasil, "Metadata filter kejadian visite berhasil diambil."));
        }

        /// <summary>
        /// Ringkasan hitungan visite, disaring perawatan atau kunjungan.
        /// </summary>
        /// <remarks>
        /// <c>INV-DOK-07</c>, <c>state-transition-matrix.md</c> bagian 5.3. Ini satu-satunya
        /// tempat hitungan visite dijawab. Menyediakan hitungan kedua yang diturunkan dari
        /// sumber lain akan melahirkan dua angka yang berpotensi berselisih.
        /// </remarks>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Physician Visit", Description = "Melihat rekap kejadian visite dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PhysicianVisit", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? inpEpisodeId = null,
            [FromQuery] Guid? encounterId = null,
            [FromQuery] Guid? doctorId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            CancellationToken cancellationToken = default)
        {
            var query = _physicianVisitService.Query(
                inpEpisodeId, encounterId, doctorId, from, to, includeCancelled: true);

            var berlaku = query.Where(x => x.VisitStatus == PhysicianVisitStatus.Recorded);

            var hasil = new PhysicianVisitSummaryResponse
            {
                TotalCount = await query.CountAsync(cancellationToken),
                RecordedCount = await berlaku.CountAsync(cancellationToken),
                CancelledCount = await query.CountAsync(
                    x => x.VisitStatus == PhysicianVisitStatus.Cancelled, cancellationToken),
                DistinctDoctorCount = await berlaku
                    .Select(x => x.DoctorId).Distinct().CountAsync(cancellationToken),
                LastVisitDateTime = await berlaku
                    .OrderByDescending(x => x.VisitDateTime)
                    .Select(x => (DateTime?)x.VisitDateTime)
                    .FirstOrDefaultAsync(cancellationToken)
            };

            return Ok(ApiResponse<PhysicianVisitSummaryResponse>.Ok(
                hasil, "Rekap kejadian visite berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePhysicianVisitPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Physician Visit", Description = "Melihat daftar kejadian visite dokter", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PhysicianVisit", "Read")]
        public Task<IActionResult> GetVisits(
            [FromQuery] Guid? inpEpisodeId = null,
            [FromQuery] Guid? encounterId = null,
            [FromQuery] Guid? doctorId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] bool includeCancelled = true,
            [FromQuery] string sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
            => BacaRiwayatAsync(
                inpEpisodeId, encounterId, doctorId, from, to, includeCancelled,
                sortDirection, pageNumber, pageSize, cancellationToken);

        /// <summary>
        /// Riwayat kejadian visite satu perawatan, terurut waktu kedatangan.
        /// </summary>
        /// <remarks>
        /// Kejadian yang dibatalkan <b>ikut ditampilkan</b> beserta alasannya, karena
        /// <c>INV-DOK-08</c> menuntut auditor tetap dapat melihat bahwa pernah ada catatan yang
        /// dibatalkan. Yang tidak menghitungnya adalah ringkasan, bukan riwayat.
        /// </remarks>
        [HttpGet("episodes/{episodeId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ResponsePhysicianVisitPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Physician Visit", Description = "Melihat riwayat visite satu perawatan rawat inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PhysicianVisit", "Read")]
        public Task<IActionResult> GetByEpisode(
            Guid episodeId,
            [FromQuery] Guid? doctorId = null,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] bool includeCancelled = true,
            [FromQuery] string sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
            => BacaRiwayatAsync(
                episodeId, encounterId: null, doctorId, from, to, includeCancelled,
                sortDirection, pageNumber, pageSize, cancellationToken);

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Physician Visit", Description = "Melihat satu kejadian visite beserta tautan dokumennya", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PhysicianVisit", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var visit = await _physicianVisitService.FindAsync(id, cancellationToken);

            if (visit == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Kejadian visite tidak ditemukan."));
            }

            return Ok(ApiResponse<PhysicianVisitResponse>.Ok(
                await BuatResponseAsync(visit, cancellationToken),
                "Kejadian visite berhasil diambil."));
        }

        /// <summary>
        /// Mencatat satu kejadian visite dokter.
        /// </summary>
        /// <remarks>
        /// <para>
        /// <b>Kunci permintaan wajib terisi</b> — <c>VAL-DOK-27</c>. Ia boleh datang dari badan
        /// permintaan atau dari header <c>Idempotency-Key</c>. Tanpa kunci, tombol Simpan yang
        /// tertekan dua kali melahirkan dua kunjungan yang tidak pernah terjadi, dan
        /// <c>INV-DOK-06</c> tidak dapat dijamin.
        /// </para>
        /// <para>
        /// <b>Dua visite nyata pada tanggal yang sama tetap menghasilkan dua baris</b> —
        /// <c>RWI-DEC-085</c>. Tidak ada penolakan berdasarkan pasangan perawatan, dokter, dan
        /// tanggal; yang dijaga unique hanyalah kunci permintaan.
        /// </para>
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
        [AccessAction("Create", "Create Physician Visit", Description = "Mencatat kejadian kunjungan dokter ke pasien", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PhysicianVisit", "Create")]
        public async Task<IActionResult> RecordVisit(
            [FromBody] CreatePhysicianVisitRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            // VAL-DOK-08. Kewenangan mencatat visite diturunkan dari DATA - penautan akun ke
            // baris dokter - bukan dari nama peran. Pengguna yang tidak terhubung ke dokter mana
            // pun ditolak tanpa satu baris kode pun yang menyebut kata "perawat".
            var actorDoctorId = await ResolveCurrentDoctorIdAsync(cancellationToken);

            if (actorDoctorId == null)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden, PenolakanBukanDokter));
            }

            var doctorId = request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty
                ? request.DoctorId.Value
                : actorDoctorId.Value;

            // Kebijakan pencatatan visite ATAS NAMA dokter lain belum ada - gerbang terbuka pada
            // roadmap bagian 5. Bawaan yang aman dipilih: seorang dokter hanya mencatat visite
            // miliknya sendiri.
            if (doctorId != actorDoctorId.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(
                    StatusCodes.Status403Forbidden,
                    "Visite hanya dapat dicatat oleh dokter yang melakukannya sendiri."));
            }

            var waktuKedatangan = NormalizeToUtc(request.VisitDateTime) ?? DateTime.UtcNow;

            // Konteks perawatan menjawab pasien, perawatan, kelayakan status, dan kewenangan
            // dokter sekaligus - VAL-DOK-01 sampai VAL-DOK-03, VAL-DOK-06, dan VAL-DOK-26.
            var konteks = await _inpatientClinicalContextService.ResolveAsync(
                request.EncounterId,
                expectedPatientId: request.PatientId,
                expectedEpisodeId: request.InpEpisodeId,
                doctorId: doctorId,
                forNewDocument: true,
                atUtc: waktuKedatangan,
                cancellationToken: cancellationToken);

            if (!konteks.IsResolved)
            {
                return StatusCode(konteks.StatusCode, ApiResponse<object>.Fail(
                    konteks.StatusCode,
                    konteks.ErrorMessage ?? "Konteks perawatan rawat inap tidak dapat dibentuk."));
            }

            var hasil = await _physicianVisitService.RecordAsync(
                new RecordPhysicianVisitCommand
                {
                    EncounterId = request.EncounterId,
                    InpEpisodeId = konteks.Context!.EpisodeId,
                    PatientId = konteks.Context.PatientId,
                    DoctorId = doctorId,
                    VisitDateTime = waktuKedatangan,
                    VisitRole = request.VisitRole,
                    ConsultationId = request.ConsultationId,
                    ProgressNoteId = request.ProgressNoteId,
                    PatientProcedureId = request.PatientProcedureId,
                    Note = request.Note,
                    IdempotencyKey = ResolveIdempotencyKey(request.IdempotencyKey),
                    CorrectsVisitId = request.CorrectsVisitId
                },
                actorUserId,
                cancellationToken);

            if (!hasil.IsSuccess || hasil.Visit == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Kejadian visite tidak dapat dicatat."));
            }

            // Catatan bebas dokter dan alasan pembatalan bersifat sensitif dan TIDAK ikut masuk
            // payload logger - permission-audit-matrix.md bagian 5.
            await _loggerService.InfoAsync(
                LogCategory,
                "PhysicianVisit.RecordVisit",
                hasil.IsReplay
                    ? "Kiriman ulang kejadian visite dengan kunci yang sama."
                    : "Mencatat kejadian visite dokter.",
                new
                {
                    EntityId = hasil.Visit.Id,
                    hasil.Visit.PhysicianVisitNumber,
                    hasil.Visit.InpEpisodeId,
                    hasil.Visit.DoctorId,
                    hasil.Visit.VisitDateTime,
                    VisitRole = hasil.Visit.VisitRole.ToString(),
                    IsReplay = hasil.IsReplay
                });

            var response = await BuatResponseAsync(hasil.Visit, cancellationToken);

            return StatusCode(hasil.StatusCode, ApiResponse<PhysicianVisitResponse>.Ok(
                response,
                hasil.IsReplay
                    ? "Kejadian visite sudah tercatat sebelumnya dengan kunci permintaan yang sama."
                    : "Kejadian visite berhasil dicatat."));
        }

        /// <summary>
        /// Membatalkan satu kejadian visite yang salah catat, beserta alasannya.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-049</c>, <c>INV-DOK-08</c>. Barisnya <b>tidak dihapus</b>: ia tetap tampil
        /// pada riwayat dengan penanda batal beserta alasannya, dan berhenti ikut dihitung.
        /// Pembatalan kedua atas kejadian yang sama ditolak <c>409</c> — <c>VAL-DOK-29</c>.
        /// </remarks>
        [HttpPatch("{id:guid}/cancel")]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Cancel", "Cancel Physician Visit", Description = "Membatalkan kejadian visite yang salah catat", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PhysicianVisit", "Cancel")]
        public async Task<IActionResult> CancelVisit(
            Guid id,
            [FromBody] CancelPhysicianVisitRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _physicianVisitService.CancelAsync(
                id, request.CancelReason, actorUserId, cancellationToken);

            if (!hasil.IsSuccess || hasil.Visit == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Kejadian visite tidak dapat dibatalkan."));
            }

            // Alasan pembatalan sensitif dan tidak ikut masuk payload logger.
            await _loggerService.InfoAsync(
                LogCategory,
                "PhysicianVisit.CancelVisit",
                "Membatalkan kejadian visite dokter.",
                new
                {
                    EntityId = hasil.Visit.Id,
                    hasil.Visit.PhysicianVisitNumber,
                    hasil.Visit.InpEpisodeId,
                    hasil.Visit.CancelledAt,
                    hasil.Visit.CancelledByUserId
                });

            return Ok(ApiResponse<PhysicianVisitResponse>.Ok(
                await BuatResponseAsync(hasil.Visit, cancellationToken),
                "Kejadian visite berhasil dibatalkan."));
        }

        /// <summary>
        /// Menautkan catatan dokter, catatan terpadu, atau tindakan pada kejadian visite.
        /// </summary>
        /// <remarks>
        /// Hanya tautan yang dapat diubah. Tidak ada jalur mana pun yang menyunting waktu maupun
        /// peran kejadian — <c>RWI-DEC-085</c>, dibuktikan uji arsitektur.
        /// </remarks>
        [HttpPatch("{id:guid}/links")]
        [ProducesResponseType(typeof(ApiResponse<PhysicianVisitResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Update", "Update Physician Visit Links", Description = "Menautkan dokumen klinis pada kejadian visite", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PhysicianVisit", "Update")]
        public async Task<IActionResult> UpdateLinks(
            Guid id,
            [FromBody] UpdatePhysicianVisitLinksRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var hasil = await _physicianVisitService.UpdateLinksAsync(
                id,
                request.ConsultationId,
                request.ProgressNoteId,
                request.PatientProcedureId,
                actorUserId,
                cancellationToken);

            if (!hasil.IsSuccess || hasil.Visit == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Tautan dokumen tidak dapat disimpan."));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "PhysicianVisit.UpdateLinks",
                "Menautkan dokumen klinis pada kejadian visite.",
                new
                {
                    EntityId = hasil.Visit.Id,
                    hasil.Visit.ConsultationId,
                    hasil.Visit.ProgressNoteId,
                    hasil.Visit.PatientProcedureId
                });

            return Ok(ApiResponse<PhysicianVisitResponse>.Ok(
                await BuatResponseAsync(hasil.Visit, cancellationToken),
                "Tautan dokumen kejadian visite berhasil disimpan."));
        }

        // =====================================================================
        // Pembantu
        // =====================================================================

        private async Task<IActionResult> BacaRiwayatAsync(
            Guid? episodeId,
            Guid? encounterId,
            Guid? doctorId,
            DateTime? from,
            DateTime? to,
            bool includeCancelled,
            string sortDirection,
            int pageNumber,
            int pageSize,
            CancellationToken cancellationToken)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _physicianVisitService.Query(
                episodeId, encounterId, doctorId, from, to, includeCancelled);

            var totalData = await query.CountAsync(cancellationToken);

            var menurun = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            var daftar = await (menurun
                    ? query.OrderByDescending(x => x.VisitDateTime)
                    : query.OrderBy(x => x.VisitDateTime))
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var namaDokter = await AmbilNamaDokterAsync(
                daftar.Select(x => x.DoctorId).Distinct().ToList(), cancellationToken);

            var namaPengguna = await AmbilNamaPenggunaAsync(
                daftar.Select(x => x.RecordedByUserId).Distinct().ToList(), cancellationToken);

            var items = daftar.Select(x => new PhysicianVisitListItemResponse
            {
                Id = x.Id,
                PhysicianVisitNumber = x.PhysicianVisitNumber,
                InpEpisodeId = x.InpEpisodeId,
                EncounterId = x.EncounterId,
                DoctorId = x.DoctorId,
                DoctorName = namaDokter.GetValueOrDefault(x.DoctorId),
                VisitDateTime = x.VisitDateTime,
                VisitRole = x.VisitRole,
                VisitRoleName = NamaPeran(x.VisitRole),
                VisitStatus = x.VisitStatus,
                VisitStatusName = NamaStatus(x.VisitStatus),
                RecordedByUserId = x.RecordedByUserId,
                RecordedByName = namaPengguna.GetValueOrDefault(x.RecordedByUserId),
                RecordedAt = x.CreateDateTime,
                ConsultationId = x.ConsultationId,
                ProgressNoteId = x.ProgressNoteId,
                PatientProcedureId = x.PatientProcedureId,
                HasLinkedDocument =
                    x.ConsultationId.HasValue ||
                    x.ProgressNoteId.HasValue ||
                    x.PatientProcedureId.HasValue,
                CancelReason = x.CancelReason,
                CancelledAt = x.CancelledAt,
                CorrectsVisitId = x.CorrectsVisitId
            }).ToList();

            var hasil = new ResponsePhysicianVisitPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = pageSize == 0 ? 0 : (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponsePhysicianVisitPagedResult>.Ok(
                hasil, "Riwayat kejadian visite berhasil diambil."));
        }

        private async Task<PhysicianVisitResponse> BuatResponseAsync(
            CliPhysicianVisit visit,
            CancellationToken cancellationToken)
        {
            var namaDokter = await AmbilNamaDokterAsync([visit.DoctorId], cancellationToken);

            var idPengguna = new List<Guid> { visit.RecordedByUserId };

            if (visit.CancelledByUserId.HasValue)
                idPengguna.Add(visit.CancelledByUserId.Value);

            var namaPengguna = await AmbilNamaPenggunaAsync(idPengguna, cancellationToken);

            var response = new PhysicianVisitResponse
            {
                Id = visit.Id,
                PhysicianVisitNumber = visit.PhysicianVisitNumber,
                EncounterId = visit.EncounterId,
                InpEpisodeId = visit.InpEpisodeId,
                PatientId = visit.PatientId,
                DoctorId = visit.DoctorId,
                DoctorName = namaDokter.GetValueOrDefault(visit.DoctorId),
                VisitDateTime = visit.VisitDateTime,
                VisitRole = visit.VisitRole,
                VisitRoleName = NamaPeran(visit.VisitRole),
                VisitStatus = visit.VisitStatus,
                VisitStatusName = NamaStatus(visit.VisitStatus),
                ConsultationId = visit.ConsultationId,
                ProgressNoteId = visit.ProgressNoteId,
                PatientProcedureId = visit.PatientProcedureId,
                Note = visit.Note,
                RecordedByUserId = visit.RecordedByUserId,
                RecordedByName = namaPengguna.GetValueOrDefault(visit.RecordedByUserId),
                RecordedAt = visit.CreateDateTime,
                CancelledAt = visit.CancelledAt,
                CancelledByUserId = visit.CancelledByUserId,
                CancelledByName = visit.CancelledByUserId.HasValue
                    ? namaPengguna.GetValueOrDefault(visit.CancelledByUserId.Value)
                    : null,
                CancelReason = visit.CancelReason,
                CorrectsVisitId = visit.CorrectsVisitId
            };

            // AvailableActions adalah bantuan tampilan, bukan pengaman. Tidak ada "Edit" di
            // sini, dan itu bukan kelalaian: jalurnya memang tidak pernah dibuat.
            if (visit.VisitStatus == PhysicianVisitStatus.Recorded)
            {
                response.AvailableActions.Add("Cancel");
                response.AvailableActions.Add("UpdateLinks");
            }

            return response;
        }

        /// <summary>
        /// Kunci permintaan dari badan permintaan, atau dari header <c>Idempotency-Key</c>.
        /// </summary>
        /// <remarks>
        /// Kosong dibiarkan kosong, bukan diisi nilai acak. Service yang menolaknya, sehingga
        /// aturan <c>VAL-DOK-27</c> tinggal di satu tempat.
        /// </remarks>
        private string ResolveIdempotencyKey(string? dariBadanPermintaan)
        {
            if (!string.IsNullOrWhiteSpace(dariBadanPermintaan))
                return dariBadanPermintaan.Trim();

            var dariHeader = Request.Headers["Idempotency-Key"].ToString();

            return string.IsNullOrWhiteSpace(dariHeader) ? string.Empty : dariHeader.Trim();
        }

        /// <summary>
        /// Menemukan baris dokter yang melekat pada pengguna yang sedang masuk.
        /// </summary>
        /// <remarks>
        /// <c>VAL-DOK-08</c>. Urutannya sama persis dengan
        /// <c>PatientAssessmentController.ResolveCurrentDoctorIdAsync</c>: klaim identitas
        /// dokter lebih dulu, lalu penautan lewat profil tenaga kerja, lalu surel. Ketiganya
        /// bersandar pada data, bukan pada nama peran.
        /// </remarks>
        private async Task<Guid?> ResolveCurrentDoctorIdAsync(CancellationToken cancellationToken)
        {
            var doctorIdClaim = User.FindFirstValue("doctor_id") ?? User.FindFirstValue("DoctorId");

            if (Guid.TryParse(doctorIdClaim, out var dariKlaimDokter) && dariKlaimDokter != Guid.Empty)
            {
                var adaDokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == dariKlaimDokter && !x.IsDelete && x.IsActive,
                              cancellationToken);

                if (adaDokter)
                    return dariKlaimDokter;
            }

            var workforceClaim = User.FindFirstValue("workforce_profile_id")
                                 ?? User.FindFirstValue("WorkforceProfileId");

            Guid? workforceProfileId =
                Guid.TryParse(workforceClaim, out var dariKlaimProfil) && dariKlaimProfil != Guid.Empty
                    ? dariKlaimProfil
                    : null;

            var currentUserId = GetCurrentUserId();

            var pengguna = currentUserId == Guid.Empty
                ? null
                : await _dbContext.Users
                    .AsNoTracking()
                    .Where(x => x.Id == currentUserId)
                    .Select(x => new { x.WorkforceProfileId, x.Email })
                    .FirstOrDefaultAsync(cancellationToken);

            workforceProfileId ??= pengguna?.WorkforceProfileId;

            if (workforceProfileId.HasValue && workforceProfileId.Value != Guid.Empty)
            {
                var dokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId.Value &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dokter.HasValue)
                    return dokter;
            }

            if (!string.IsNullOrWhiteSpace(pengguna?.Email))
            {
                var surel = pengguna.Email.ToLower();

                var dokter = await _dbContext.Set<MstDoctor>()
                    .AsNoTracking()
                    .Where(x =>
                        x.Email != null &&
                        x.Email.ToLower() == surel &&
                        !x.IsDelete &&
                        x.IsActive)
                    .Select(x => (Guid?)x.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                if (dokter.HasValue)
                    return dokter;
            }

            return null;
        }

        private async Task<Dictionary<Guid, string>> AmbilNamaDokterAsync(
            List<Guid> doctorIds,
            CancellationToken cancellationToken)
        {
            if (doctorIds.Count == 0)
                return [];

            return await _dbContext.Set<MstDoctor>()
                .AsNoTracking()
                .Where(x => doctorIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.FullName, cancellationToken);
        }

        private async Task<Dictionary<Guid, string>> AmbilNamaPenggunaAsync(
            List<Guid> userIds,
            CancellationToken cancellationToken)
        {
            var bersih = userIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (bersih.Count == 0)
                return [];

            return await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => bersih.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName, cancellationToken);
        }

        private static string NamaPeran(PhysicianVisitRole peran) => peran switch
        {
            PhysicianVisitRole.Dpjp => "DPJP",
            PhysicianVisitRole.Consultant => "Konsulen",
            PhysicianVisitRole.OnCall => "Dokter Jaga",
            _ => peran.ToString()
        };

        private static string NamaStatus(PhysicianVisitStatus status) => status switch
        {
            PhysicianVisitStatus.Recorded => "Tercatat",
            PhysicianVisitStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        private static DateTime? NormalizeToUtc(DateTime? value)
        {
            if (!value.HasValue)
                return null;

            return value.Value.Kind switch
            {
                DateTimeKind.Utc => value.Value,
                DateTimeKind.Local => value.Value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
