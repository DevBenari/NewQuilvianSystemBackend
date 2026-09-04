using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    /// <summary>
    /// Berkas rekam medis pasien: ringkasan, riwayat lintas kunjungan, dan detail dokumen.
    ///
    /// ATURAN YANG MENGIKAT SELURUH ENDPOINT DI SINI. Setiap pembukaan berkas melewati
    /// penilaian kewenangan dan pencatatan jejak LEBIH DULU. Bila penilaian menolak, atau
    /// pencatatan jejak gagal, isi rekam medis tidak dikembalikan sama sekali — bukan diambil
    /// lalu disembunyikan. Ini penerapan RM-DEC-005 dan RM-DEC-015.
    ///
    /// Controller ini TIDAK menyentuh <c>ApplicationDbContext</c> untuk membaca isi rekam medis.
    /// Seluruh pembacaan lewat <see cref="MedicalRecordTimelineService"/>, sesuai arsitektur
    /// bagian 5.9. Basis data hanya disentuh langsung untuk daftar pilihan penyaring, yang tidak
    /// memuat data pasien mana pun.
    ///
    /// CATATAN PRIBADI TIDAK ADA DI SINI. Tidak satu pun endpoint pada controller ini
    /// mengembalikan isi `PrivateNote`. Kolom itu hanya dapat dibuka lewat endpoint tersendiri
    /// dengan izin terpisah (`BE-15`, RM-DEC-022).
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/medical-records")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Medical Record",
        AreaName = "HealthServices",
        ControllerName = "MedicalRecord",
        Description = "Berkas rekam medis pasien: ringkasan, riwayat, dan detail dokumen",
        SortOrder = 1
    )]
    [Tags("Health Services / Medical Record Management / Medical Record")]
    public class MedicalRecordController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly MedicalRecordAccessAuditService _accessAuditService;
        private readonly MedicalRecordTimelineService _timelineService;

        public MedicalRecordController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            MedicalRecordAccessAuditService accessAuditService,
            MedicalRecordTimelineService timelineService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _accessAuditService = accessAuditService;
            _timelineService = timelineService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record", Description = "Melihat daftar pilihan penyaring rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecord", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken = default)
        {
            var keperluan = await _dbContext.Set<MstMedicalRecordAccessPurpose>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && !x.IsCancel && x.IsActive)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.PurposeName)
                .Select(x => new MedicalRecordAccessPurposeOptionResponse
                {
                    Id = x.Id,
                    PurposeCode = x.PurposeCode,
                    PurposeName = x.PurposeName,
                    IsFreeTextRequired = x.IsFreeTextRequired,
                    RequiresReview = x.RequiresReview,
                    Description = x.Description
                })
                .ToListAsync(cancellationToken);

            var hasil = new MedicalRecordFilterMetadataResponse
            {
                DocumentKinds = MedicalRecordTimelineService.SeluruhJenis
                    .Select(x => new MedicalRecordDocumentKindOptionResponse
                    {
                        Value = x,
                        Name = MedicalRecordTimelineService.NamaJenis(x),
                        IsIntegrityEnforced = ClinicalDocumentIntegrityService.DitegakkanUntuk(x)
                    })
                    .ToList(),
                AccessPurposes = keperluan,
                PageSizeDefault = MedicalRecordTimelineService.UkuranHalamanBawaan,
                PageSizeMax = MedicalRecordTimelineService.UkuranHalamanMaksimal,
                IsAccessPurposeMasterEmpty = keperluan.Count == 0
            };

            return Ok(ApiResponse<MedicalRecordFilterMetadataResponse>.Ok(
                hasil,
                hasil.IsAccessPurposeMasterEmpty
                    ? "Daftar pilihan berhasil diambil. PERHATIAN: master keperluan akses masih kosong, sehingga pembukaan rekam medis pasien di luar rawatan akan selalu ditolak."
                    : "Daftar pilihan berhasil diambil."));
        }

        [HttpGet("{patientId:guid}/summary")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Read", "Read Medical Record", Description = "Membuka berkas rekam medis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecord", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid patientId,
            [FromQuery] Guid? accessPurposeId = null,
            [FromQuery] string? accessReason = null,
            CancellationToken cancellationToken = default)
        {
            var (akses, penolakan) = await NilaiAksesAsync(
                patientId, MedicalRecordAccessScope.Summary,
                accessPurposeId, accessReason, cancellationToken);

            if (penolakan != null)
                return penolakan;

            var hasil = await _timelineService.GetSummaryAsync(patientId, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound, "Pasien tidak ditemukan."));
            }

            hasil.Access = KeteranganAkses(akses);

            await CatatPembukaanAsync("MedicalRecord.GetSummary", patientId, akses);

            return Ok(ApiResponse<MedicalRecordSummaryResponse>.Ok(
                hasil, "Ringkasan berkas rekam medis berhasil dibuka."));
        }

        [HttpGet("{patientId:guid}/timeline")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordTimelineResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Read", "Read Medical Record", Description = "Membuka riwayat rekam medis pasien", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecord", "Read")]
        public async Task<IActionResult> GetTimeline(
            Guid patientId,
            [FromQuery] List<ClinicalDocumentKind>? documentKinds = null,
            [FromQuery] Guid? encounterId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] bool includeCancelled = false,
            [FromQuery] bool newestFirst = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = MedicalRecordTimelineService.UkuranHalamanBawaan,
            [FromQuery] Guid? accessPurposeId = null,
            [FromQuery] string? accessReason = null,
            CancellationToken cancellationToken = default)
        {
            var (akses, penolakan) = await NilaiAksesAsync(
                patientId, MedicalRecordAccessScope.Timeline,
                accessPurposeId, accessReason, cancellationToken);

            if (penolakan != null)
                return penolakan;

            var hasil = await _timelineService.GetTimelineAsync(
                new MedicalRecordTimelineQuery
                {
                    PatientId = patientId,
                    DocumentKinds = documentKinds,
                    EncounterId = encounterId,
                    StartDate = startDate,
                    EndDate = endDate,
                    IncludeCancelled = includeCancelled,
                    NewestFirst = newestFirst,
                    Page = page,
                    PageSize = pageSize
                },
                cancellationToken);

            await CatatPembukaanAsync("MedicalRecord.GetTimeline", patientId, akses);

            var balasan = new MedicalRecordTimelineResponse
            {
                Page = hasil.Page,
                Access = KeteranganAkses(akses),
                RequestedKinds = hasil.RequestedKinds,
                FailedSources = hasil.FailedSources,
                IsTruncated = hasil.IsTruncated,
                IsComplete = hasil.IsComplete
            };

            return Ok(ApiResponse<MedicalRecordTimelineResponse>.Ok(
                balasan, PesanKelengkapan(hasil)));
        }

        [HttpGet("{patientId:guid}/documents/{documentKind}/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("Read", "Read Medical Record", Description = "Membuka detail dokumen rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecord", "Read")]
        public async Task<IActionResult> GetDocumentDetail(
            Guid patientId,
            ClinicalDocumentKind documentKind,
            Guid documentId,
            [FromQuery] Guid? accessPurposeId = null,
            [FromQuery] string? accessReason = null,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(documentKind))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "Jenis dokumen tidak dikenali."));
            }

            var (akses, penolakan) = await NilaiAksesAsync(
                patientId, MedicalRecordAccessScope.DocumentDetail,
                accessPurposeId, accessReason, cancellationToken);

            if (penolakan != null)
                return penolakan;

            var hasil = await _timelineService.GetDocumentDetailAsync(
                patientId, documentKind, documentId, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen tidak ditemukan pada berkas rekam medis pasien ini."));
            }

            hasil.Access = KeteranganAkses(akses);

            await CatatPembukaanAsync("MedicalRecord.GetDocumentDetail", patientId, akses);

            return Ok(ApiResponse<MedicalRecordDocumentDetailResponse>.Ok(
                hasil,
                hasil.IsIntegrityEnforced
                    ? "Detail dokumen berhasil dibuka."
                    : "Detail dokumen berhasil dibuka. Jenis dokumen ini belum tunduk aturan keutuhan rekam medis."));
        }

        [HttpGet("{patientId:guid}/documents/{documentKind}/{documentId:guid}/private-note")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordPrivateNoteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
        [AccessAction("ReadPrivateNote", "Read Medical Record Private Note", Description = "Membuka catatan pribadi klinisi pada dokumen rekam medis", AccessType = AccessTypes.Read, SortOrder = 2)]
        [AccessPermission("MedicalRecord", "ReadPrivateNote")]
        public async Task<IActionResult> GetPrivateNote(
            Guid patientId,
            ClinicalDocumentKind documentKind,
            Guid documentId,
            [FromQuery] Guid? accessPurposeId = null,
            [FromQuery] string? accessReason = null,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(documentKind))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest, "Jenis dokumen tidak dikenali."));
            }

            // Diperiksa SEBELUM jejak dicatat. Jenis dokumen yang memang tidak punya kolom
            // catatan pribadi bukan percobaan pembukaan berkas, melainkan permintaan yang keliru
            // bentuknya — mencatatnya akan mengotori angka tinjauan.
            if (!MedicalRecordTimelineService.MendukungCatatanPribadi(documentKind))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    $"Jenis dokumen {MedicalRecordTimelineService.NamaJenis(documentKind)} tidak memiliki catatan pribadi."));
            }

            var (akses, penolakan) = await NilaiAksesAsync(
                patientId, MedicalRecordAccessScope.PrivateNote,
                accessPurposeId, accessReason, cancellationToken);

            if (penolakan != null)
                return penolakan;

            var hasil = await _timelineService.GetPrivateNoteAsync(
                patientId, documentKind, documentId, cancellationToken);

            if (hasil == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen tidak ditemukan pada berkas rekam medis pasien ini."));
            }

            hasil.Access = KeteranganAkses(akses);

            // Isi catatan TIDAK ikut dicatat logger. Yang dicatat hanya bahwa pembukaan terjadi.
            await CatatPembukaanAsync("MedicalRecord.GetPrivateNote", patientId, akses);

            return Ok(ApiResponse<MedicalRecordPrivateNoteResponse>.Ok(
                hasil,
                hasil.HasPrivateNote
                    ? "Catatan pribadi berhasil dibuka. Pembukaan ini tercatat dan akan ditelaah unit rekam medis."
                    : "Dokumen ini tidak memuat catatan pribadi. Pembukaan tetap tercatat."));
        }

        // =====================================================================
        // Bagian bersama
        // =====================================================================

        /// <summary>
        /// Menilai kewenangan dan mencatat jejaknya SEBELUM isi rekam medis disentuh.
        ///
        /// Mengembalikan hasil penilaian beserta balasan penolakan bila tidak diizinkan. Selama
        /// balasan penolakan tidak kosong, pemanggil WAJIB mengembalikannya apa adanya dan tidak
        /// membaca isi rekam medis sedikit pun.
        /// </summary>
        private async Task<(MedicalRecordAccessResult Hasil, IActionResult? Penolakan)> NilaiAksesAsync(
            Guid patientId,
            MedicalRecordAccessScope scope,
            Guid? accessPurposeId,
            string? accessReason,
            CancellationToken cancellationToken)
        {
            var permintaan = new MedicalRecordAccessRequest(
                PatientId: patientId,
                UserId: GetCurrentUserId(),
                Scope: scope,
                AccessPurposeId: accessPurposeId,
                AccessReason: accessReason,
                IpAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                ClientInfo: Request.Headers.UserAgent.ToString(),
                RequestPath: Request.Path.Value);

            var hasil = await _accessAuditService.EvaluateAndRecordAsync(
                permintaan, DateTime.UtcNow, cancellationToken);

            if (hasil.IsAllowed)
                return (hasil, null);

            // Alasan akses TIDAK ikut dicatat logger: ia dapat mengungkap keadaan pasien.
            await _loggerService.WarningAsync(
                LogCategory,
                "MedicalRecord.AccessDenied",
                "Pembukaan berkas rekam medis ditolak.",
                new { EntityId = patientId, hasil.StatusCode, Scope = scope.ToString() });

            var penolakan = StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                hasil.StatusCode,
                hasil.ErrorMessage ?? "Berkas rekam medis tidak dapat dibuka."));

            return (hasil, penolakan);
        }

        private async Task CatatPembukaanAsync(
            string action,
            Guid patientId,
            MedicalRecordAccessResult akses)
        {
            await _loggerService.InfoAsync(
                LogCategory,
                action,
                "Berkas rekam medis dibuka.",
                new
                {
                    EntityId = patientId,
                    AccessLogId = akses.AccessLogId,
                    AccessType = akses.AccessType.ToString(),
                    akses.IsFlaggedForReview
                });
        }

        private static MedicalRecordAccessInfoResponse KeteranganAkses(
            MedicalRecordAccessResult akses) => new()
            {
                AccessLogId = akses.AccessLogId,
                AccessType = akses.AccessType,
                AccessTypeName = akses.AccessType == MedicalRecordAccessType.RoutineCare
                    ? "Akses Rawatan"
                    : "Akses Beralasan",
                HasActiveEncounter = akses.AccessType == MedicalRecordAccessType.RoutineCare,
                IsFlaggedForReview = akses.IsFlaggedForReview
            };

        /// <summary>
        /// Menyusun pesan yang menyatakan apakah daftar riwayat yang dikembalikan sudah lengkap.
        ///
        /// Pesan ini bukan hiasan. Daftar yang kurang satu sumber tetap berguna, tetapi hanya
        /// bila pembacanya tahu bahwa daftarnya kurang.
        /// </summary>
        private static string PesanKelengkapan(MedicalRecordTimelineResult hasil)
        {
            if (hasil.IsComplete)
                return "Riwayat rekam medis berhasil dibuka.";

            var pesan = new List<string>();

            if (hasil.FailedSources.Count > 0)
            {
                var nama = string.Join(", ", hasil.FailedSources.Select(x => x.DocumentKindName));
                pesan.Add($"jenis dokumen berikut gagal dibaca dan tidak ikut tampil: {nama}");
            }

            if (hasil.IsTruncated)
            {
                pesan.Add("sebagian dokumen tidak ikut terambil karena melampaui batas. " +
                          "Persempit rentang tanggal atau jenis dokumen");
            }

            return $"Riwayat rekam medis dibuka, tetapi TIDAK LENGKAP — {string.Join("; ", pesan)}.";
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
