using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseAccessLogPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs.MedicalRecordAccessLogResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    /// <summary>
    /// Layar tinjauan jejak akses rekam medis.
    ///
    /// TIDAK ADA endpoint membuat, mengubah, maupun menghapus jejak — dan itu bukan kelalaian.
    /// Jejak hanya dibuat sistem saat rekam medis dibuka, tidak pernah oleh permintaan manusia.
    /// Satu-satunya perubahan yang diizinkan adalah menandainya sudah ditinjau, yang menambah
    /// keterangan tanpa mengubah isi jejaknya.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/medical-record-access-logs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Medical Record Access Log",
        AreaName = "HealthServices",
        ControllerName = "MedicalRecordAccessLog",
        Description = "Jejak dan tinjauan akses berkas rekam medis",
        SortOrder = 5
    )]
    [Tags("Health Services / Medical Record Management / Medical Record Access Log")]
    public class MedicalRecordAccessLogController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly MedicalRecordAccessReviewService _reviewService;

        public MedicalRecordAccessLogController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            MedicalRecordAccessReviewService reviewService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _reviewService = reviewService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessLogFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Access Log", Description = "Melihat daftar pilihan penyaring jejak akses", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessLog", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = new MedicalRecordAccessLogFilterMetadataResponse
            {
                AccessTypes = Enum.GetValues<MedicalRecordAccessType>()
                    .Select(x => new MedicalRecordEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = x == MedicalRecordAccessType.RoutineCare
                            ? "Akses Rawatan"
                            : "Akses Beralasan"
                    })
                    .ToList(),
                AccessScopes = Enum.GetValues<MedicalRecordAccessScope>()
                    .Select(x => new MedicalRecordEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = NamaCakupan(x)
                    })
                    .ToList(),
                SortOptions =
                [
                    new() { Value = "accessedAt", Label = "Waktu akses" },
                    new() { Value = "accessType", Label = "Jenis akses" },
                    new() { Value = "accessScope", Label = "Bagian yang dibuka" },
                    new() { Value = "reviewedAt", Label = "Waktu ditinjau" }
                ],
                SortDirections = ["asc", "desc"],
                PageSizeOptions = [10, 25, 50, 100],
                QueryParameters =
                [
                    new()
                    {
                        Name = "patientId",
                        Type = "guid",
                        Description = "Menyaring jejak untuk satu pasien tertentu."
                    },
                    new()
                    {
                        Name = "userId",
                        Type = "guid",
                        Description = "Menyaring jejak untuk satu pengguna tertentu."
                    },
                    new()
                    {
                        Name = "accessType",
                        Type = "enum",
                        Description = "Akses rawatan atau akses beralasan.",
                        Example = "2"
                    },
                    new()
                    {
                        Name = "isFlaggedForReview",
                        Type = "boolean",
                        Description = "Menyaring akses yang ditandai perlu ditinjau.",
                        Example = "true"
                    },
                    new()
                    {
                        Name = "startDate",
                        Type = "date",
                        Description = "Batas awal rentang waktu akses."
                    },
                    new()
                    {
                        Name = "endDate",
                        Type = "date",
                        Description = "Batas akhir rentang waktu akses."
                    },
                    new()
                    {
                        Name = "pageNumber",
                        Type = "integer",
                        Description = "Halaman, dimulai dari 1.",
                        Example = "1"
                    },
                    new()
                    {
                        Name = "pageSize",
                        Type = "integer",
                        Description = "Jumlah baris per halaman. Bawaan 25, paling besar 100.",
                        Example = "25"
                    }
                ]
            };

            return Ok(ApiResponse<MedicalRecordAccessLogFilterMetadataResponse>.Ok(
                hasil, "Metadata filter jejak akses berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseAccessLogPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Access Log", Description = "Melihat jejak akses rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessLog", "Read")]
        public Task<IActionResult> GetLogs(
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? userId = null,
            [FromQuery] MedicalRecordAccessType? accessType = null,
            [FromQuery] bool? isFlaggedForReview = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
            => AmbilDaftarAsync(patientId, userId, accessType, isFlaggedForReview,
                                startDate, endDate, hanyaBelumDitinjau: false,
                                pageNumber, pageSize);

        /// <summary>
        /// Antrean akses yang ditandai perlu ditinjau dan belum dinilai siapa pun.
        ///
        /// Penyaring waktu dan jenis akses hanya dapat MEMPERSEMPIT antrean, tidak pernah
        /// melebarkannya: `isFlaggedForReview` dan syarat "belum ditinjau" tetap dipatok di
        /// sini, bukan dikirim pemanggil. Definisi "perlu ditinjau" adalah aturan privasi
        /// milik server; begitu ia dapat dipilih lewat kueri, antrean berhenti berarti apa pun.
        /// </summary>
        [HttpGet("pending-review")]
        [ProducesResponseType(typeof(ApiResponse<ResponseAccessLogPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Access Log", Description = "Melihat antrean akses yang perlu ditinjau", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessLog", "Read")]
        public Task<IActionResult> GetPendingReview(
            [FromQuery] MedicalRecordAccessType? accessType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
            => AmbilDaftarAsync(null, null, accessType, isFlaggedForReview: true,
                                startDate, endDate, hanyaBelumDitinjau: true,
                                pageNumber, pageSize);

        [HttpPatch("{id:guid}/mark-reviewed")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessLogResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Mark Medical Record Access Reviewed", Description = "Menandai akses rekam medis sudah ditinjau", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicalRecordAccessLog", "Update")]
        public async Task<IActionResult> MarkReviewed(
            Guid id,
            [FromBody] MarkAccessReviewedRequest request)
        {
            var actorUserId = GetCurrentUserId();

            var (hasil, jejak) = await _reviewService.MarkReviewedAsync(
                id, actorUserId, request.ReviewNote, DateTime.UtcNow);

            if (!hasil.IsAllowed || jejak == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Akses tidak dapat ditandai sudah ditinjau."
                ));
            }

            // Catatan tinjauan TIDAK ikut dicatat logger: ia dapat memuat keterangan yang
            // menyinggung keadaan pasien.
            await _loggerService.InfoAsync(
                LogCategory,
                "MedicalRecordAccessLog.MarkReviewed",
                "Jejak akses ditandai sudah ditinjau.",
                new { EntityId = jejak.Id });

            var response = (await LengkapiAsync([ToResponse(jejak)])).Single();

            return Ok(ApiResponse<MedicalRecordAccessLogResponse>.Ok(
                response, "Akses berhasil ditandai sudah ditinjau."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MedicalRecordAccessSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medical Record Access Log", Description = "Melihat rekap akses rekam medis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicalRecordAccessLog", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var akhir = endDate ?? DateTime.UtcNow;
            var awal = startDate ?? akhir.AddDays(-30);

            var hasil = await _reviewService.SummaryAsync(awal, akhir);

            return Ok(ApiResponse<MedicalRecordAccessSummaryResponse>.Ok(
                hasil, "Rekap akses rekam medis berhasil diambil."));
        }

        private async Task<IActionResult> AmbilDaftarAsync(
            Guid? patientId,
            Guid? userId,
            MedicalRecordAccessType? accessType,
            bool? isFlaggedForReview,
            DateTime? startDate,
            DateTime? endDate,
            bool hanyaBelumDitinjau,
            int pageNumber,
            int pageSize)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _dbContext.Set<MrcAccessLog>().AsNoTracking();

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (userId.HasValue && userId.Value != Guid.Empty)
                query = query.Where(x => x.UserId == userId.Value);

            if (accessType.HasValue)
                query = query.Where(x => x.AccessType == accessType.Value);

            if (isFlaggedForReview.HasValue)
                query = query.Where(x => x.IsFlaggedForReview == isFlaggedForReview.Value);

            if (hanyaBelumDitinjau)
                query = query.Where(x => x.ReviewedAt == null);

            if (startDate.HasValue)
                query = query.Where(x => x.AccessedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(x => x.AccessedAt <= endDate.Value);

            var totalData = await query.CountAsync();

            var daftar = await query
                .OrderByDescending(x => x.AccessedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = await LengkapiAsync(daftar.Select(ToResponse).ToList());

            var hasil = new ResponseAccessLogPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseAccessLogPagedResult>.Ok(
                hasil, "Daftar jejak akses berhasil diambil."));
        }

        private static MedicalRecordAccessLogResponse ToResponse(MrcAccessLog x) => new()
        {
            Id = x.Id,
            PatientId = x.PatientId,
            UserId = x.UserId,
            UserDisplayNameSnapshot = x.UserDisplayNameSnapshot,
            AccessType = x.AccessType,
            AccessTypeName = x.AccessType == MedicalRecordAccessType.RoutineCare
                ? "Akses Rawatan"
                : "Akses Beralasan",
            AccessScope = x.AccessScope,
            AccessScopeName = NamaCakupan(x.AccessScope),
            AccessPurposeId = x.AccessPurposeId,
            AccessReason = x.AccessReason,
            HasActiveEncounter = x.HasActiveEncounter,
            IsFlaggedForReview = x.IsFlaggedForReview,
            ReviewedAt = x.ReviewedAt,
            ReviewedByUserId = x.ReviewedByUserId,
            ReviewNote = x.ReviewNote,
            AccessedAt = x.AccessedAt,
            IpAddress = x.IpAddress,
            ClientInfo = x.ClientInfo
        };

        /// <summary>
        /// Melengkapi nama pasien, nama keperluan, dan nama peninjau.
        ///
        /// Diambil terpisah supaya query utama tetap sederhana dan daftar tetap terbaca
        /// walaupun salah satu rujukan tidak ditemukan.
        /// </summary>
        private async Task<List<MedicalRecordAccessLogResponse>> LengkapiAsync(
            List<MedicalRecordAccessLogResponse> items)
        {
            if (items.Count == 0) return items;

            var patientIds = items.Select(x => x.PatientId).Distinct().ToList();
            var purposeIds = items.Where(x => x.AccessPurposeId.HasValue)
                .Select(x => x.AccessPurposeId!.Value).Distinct().ToList();
            var reviewerIds = items.Where(x => x.ReviewedByUserId.HasValue)
                .Select(x => x.ReviewedByUserId!.Value).Distinct().ToList();

            var pasien = await _dbContext.Set<MstPatient>().AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToListAsync();

            var keperluan = purposeIds.Count == 0
                ? []
                : await _dbContext.Set<MstMedicalRecordAccessPurpose>().AsNoTracking()
                    .Where(x => purposeIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.PurposeName })
                    .ToListAsync();

            var peninjau = reviewerIds.Count == 0
                ? []
                : await _dbContext.Set<ApplicationUser>().AsNoTracking()
                    .Where(x => reviewerIds.Contains(x.Id))
                    .Select(x => new { x.Id, x.DisplayName })
                    .ToListAsync();

            foreach (var item in items)
            {
                var p = pasien.FirstOrDefault(x => x.Id == item.PatientId);
                item.PatientName = p?.FullName;
                item.MedicalRecordNumber = p?.MedicalRecordNumber;

                if (item.AccessPurposeId.HasValue)
                    item.AccessPurposeName = keperluan
                        .FirstOrDefault(x => x.Id == item.AccessPurposeId.Value)?.PurposeName;

                if (item.ReviewedByUserId.HasValue)
                    item.ReviewedByName = peninjau
                        .FirstOrDefault(x => x.Id == item.ReviewedByUserId.Value)?.DisplayName;
            }

            return items;
        }

        private static string NamaCakupan(MedicalRecordAccessScope cakupan) => cakupan switch
        {
            MedicalRecordAccessScope.Summary => "Ringkasan Berkas",
            MedicalRecordAccessScope.Timeline => "Riwayat Lintas Kunjungan",
            MedicalRecordAccessScope.DocumentDetail => "Detail Dokumen",
            MedicalRecordAccessScope.PrivateNote => "Catatan Pribadi",
            _ => cakupan.ToString()
        };

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
