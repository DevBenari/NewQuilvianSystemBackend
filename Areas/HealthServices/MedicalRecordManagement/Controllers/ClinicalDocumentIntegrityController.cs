using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseUnsignedDocumentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs.UnsignedDocumentResponse>;

// BE-RWI-092. Daftar "Catatan Saya": catatan terkunci milik penulis yang sedang masuk.
using ResponseAuthoredDocumentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs.AuthoredDocumentItem>;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/clinical-document-integrities")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Clinical Document Integrity",
        AreaName = "HealthServices",
        ControllerName = "ClinicalDocumentIntegrity",
        Description = "Keutuhan dan keabsahan dokumen klinis pada berkas rekam medis",
        SortOrder = 2
    )]
    [Tags("Health Services / Medical Record Management / Clinical Document Integrity")]
    public class ClinicalDocumentIntegrityController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly ClinicalDocumentIntegrityService _integrityService;

        public ClinicalDocumentIntegrityController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            ClinicalDocumentIntegrityService integrityService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _integrityService = integrityService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalDocumentIntegrityFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat daftar pilihan penyaring keutuhan dokumen", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = new ClinicalDocumentIntegrityFilterMetadataResponse
            {
                DocumentKinds = MedicalRecordTimelineService.SeluruhJenis
                    .Select(x => new MedicalRecordDocumentKindOptionResponse
                    {
                        Value = x,
                        Name = MedicalRecordTimelineService.NamaJenis(x),
                        IsIntegrityEnforced = ClinicalDocumentIntegrityService.DitegakkanUntuk(x)
                    })
                    .ToList(),
                IntegrityStatuses = Enum.GetValues<ClinicalDocumentIntegrityStatus>()
                    .Select(x => new MedicalRecordEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = MedicalRecordTimelineService.NamaStatusKeutuhan(x)
                    })
                    .ToList(),
                LockTriggers = Enum.GetValues<ClinicalDocumentLockTrigger>()
                    .Select(x => new MedicalRecordEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = NamaPemicu(x)
                    })
                    .ToList(),
                SortOptions =
                [
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "signedAt", Label = "Tanggal ditandatangani" },
                    new() { Value = "lockedAt", Label = "Tanggal terkunci" },
                    new() { Value = "integrityStatus", Label = "Status keutuhan" },
                    new() { Value = "documentKind", Label = "Jenis dokumen" }
                ],
                SortDirections = ["asc", "desc"],
                PageSizeOptions = [10, 25, 50, 100],
                QueryParameters =
                [
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

            return Ok(ApiResponse<ClinicalDocumentIntegrityFilterMetadataResponse>.Ok(
                hasil, "Metadata filter keutuhan dokumen berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalDocumentIntegritySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat rekap keutuhan dokumen klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] Guid? patientId = null,
            [FromQuery] Guid? encounterId = null)
        {
            var query = _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (patientId.HasValue && patientId.Value != Guid.Empty)
                query = query.Where(x => x.PatientId == patientId.Value);

            if (encounterId.HasValue && encounterId.Value != Guid.Empty)
                query = query.Where(x => x.EncounterId == encounterId.Value);

            var ditegakkan = MedicalRecordTimelineService.SeluruhJenis
                .Count(ClinicalDocumentIntegrityService.DitegakkanUntuk);

            var hasil = new ClinicalDocumentIntegritySummaryResponse
            {
                TotalDocument = await query.CountAsync(),
                DraftDocument = await query.CountAsync(
                    x => x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft),
                SignedDocument = await query.CountAsync(
                    x => x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Signed),
                LockedUnsignedDocument = await query.CountAsync(
                    x => x.IntegrityStatus == ClinicalDocumentIntegrityStatus.LockedUnsigned),
                CancelledDocument = await query.CountAsync(
                    x => x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Cancelled),
                UnknownAuthorDocument = await query.CountAsync(x => !x.IsAuthorKnown),
                TotalAddendum = await query.SumAsync(x => x.AddendumCount),
                EnforcedDocumentKind = ditegakkan,
                NotEnforcedDocumentKind = MedicalRecordTimelineService.SeluruhJenis.Count - ditegakkan
            };

            return Ok(ApiResponse<ClinicalDocumentIntegritySummaryResponse>.Ok(
                hasil, "Rekap keutuhan dokumen klinis berhasil diambil."));
        }

        [HttpGet("by-document/{documentKind}/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalDocumentIntegrityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat status keutuhan dokumen klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public async Task<IActionResult> GetByDocument(
            ClinicalDocumentKind documentKind,
            Guid documentId)
        {
            var keutuhan = await _integrityService.FindAsync(documentKind, documentId);

            if (keutuhan == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Catatan belum terdaftar pada daftar keutuhan."
                ));
            }

            var response = await ToResponseAsync(keutuhan);

            return Ok(ApiResponse<ClinicalDocumentIntegrityResponse>.Ok(
                response, "Status keutuhan catatan berhasil diambil."));
        }

        [HttpPost("by-document/{documentKind}/{documentId:guid}/sign")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalDocumentIntegrityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Sign Clinical Document", Description = "Menandatangani dan mengunci catatan klinis", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("ClinicalDocumentIntegrity", "Update")]
        public async Task<IActionResult> Sign(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            [FromBody] SignClinicalDocumentRequest request)
        {
            if (!request.IsConfirmed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Penandatanganan perlu dikonfirmasi lebih dulu."
                ));
            }

            var actorUserId = GetCurrentUserId();

            // Perangkat dan alamat jaringan diambil server dari permintaan, bukan dari kiriman
            // klien. Bila dikirim klien, nilainya dapat dipalsukan dan kehilangan makna sebagai
            // bukti (RM-DEC-021).
            var (hasil, keutuhan) = await _integrityService.SignAsync(
                documentKind,
                documentId,
                actorUserId,
                deviceInfo: Request.Headers.UserAgent.ToString(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                nowUtc: DateTime.UtcNow);

            if (!hasil.IsAllowed || keutuhan == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Catatan tidak dapat ditandatangani."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalDocumentIntegrity.Sign",
                "Catatan klinis ditandatangani dan dikunci.",
                new { EntityId = keutuhan.Id, keutuhan.DocumentKind, keutuhan.DocumentId });

            var response = await ToResponseAsync(keutuhan);

            return Ok(ApiResponse<ClinicalDocumentIntegrityResponse>.Ok(
                response, "Catatan berhasil ditandatangani dan dikunci."));
        }

        [HttpGet("my-unsigned")]
        [ProducesResponseType(typeof(ApiResponse<ResponseUnsignedDocumentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat catatan sendiri yang belum ditandatangani", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public async Task<IActionResult> GetMyUnsigned(
            [FromQuery] ClinicalDocumentServiceContext serviceContext = ClinicalDocumentServiceContext.All,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(serviceContext))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Konteks layanan tidak dikenal."));
            }

            var actorUserId = GetCurrentUserId();
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var hasil = await _integrityService.GetAuthoredDraftsAsync(
                actorUserId,
                serviceContext,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<ResponseUnsignedDocumentPagedResult>.Ok(
                hasil, "Daftar catatan yang belum ditandatangani berhasil diambil."));
        }

        [HttpGet("my-authored")]
        [ProducesResponseType(typeof(ApiResponse<ResponseAuthoredDocumentPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat catatan terkunci milik penulis yang sedang masuk", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public async Task<IActionResult> GetMyAuthored(
            [FromQuery] ClinicalDocumentIntegrityStatus? status = null,
            [FromQuery] ClinicalDocumentServiceContext serviceContext = ClinicalDocumentServiceContext.All,
            [FromQuery] DateTime? from = null,
            [FromQuery] DateTime? to = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if ((status.HasValue && !Enum.IsDefined(status.Value)) ||
                !Enum.IsDefined(serviceContext))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Status catatan atau konteks layanan tidak dikenal."));
            }

            if (from.HasValue && to.HasValue && from.Value > to.Value)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Tanggal awal tidak boleh sesudah tanggal akhir."));
            }

            var actorUserId = GetCurrentUserId();
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var hasil = await _integrityService.GetAuthoredDocumentsAsync(
                actorUserId,
                status,
                serviceContext,
                from,
                to,
                pageNumber,
                pageSize,
                cancellationToken);

            return Ok(ApiResponse<ResponseAuthoredDocumentPagedResult>.Ok(
                hasil, "Daftar catatan milik penulis berhasil diambil."));
        }

        [HttpGet("by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalDocumentIntegrityResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat keutuhan dokumen per kunjungan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public async Task<IActionResult> GetByEncounter(Guid encounterId)
        {
            var daftar = await _dbContext.Set<MrcClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.EncounterId == encounterId && !x.IsDelete)
                .OrderBy(x => x.CreateDateTime)
                .ToListAsync();

            var response = new List<ClinicalDocumentIntegrityResponse>();
            foreach (var keutuhan in daftar)
            {
                response.Add(await ToResponseAsync(keutuhan));
            }

            return Ok(ApiResponse<List<ClinicalDocumentIntegrityResponse>>.Ok(
                response, "Keutuhan dokumen pada kunjungan berhasil diambil."));
        }

        private async Task<ClinicalDocumentIntegrityResponse> ToResponseAsync(
            MrcClinicalDocumentIntegrity keutuhan)
        {
            var namaPenulis = await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => x.Id == keutuhan.AuthorUserId)
                .Select(x => x.DisplayName)
                .FirstOrDefaultAsync();

            return new ClinicalDocumentIntegrityResponse
            {
                Id = keutuhan.Id,
                DocumentKind = keutuhan.DocumentKind,
                DocumentKindName = keutuhan.DocumentKind.ToString(),
                DocumentId = keutuhan.DocumentId,
                PatientId = keutuhan.PatientId,
                EncounterId = keutuhan.EncounterId,
                IntegrityStatus = keutuhan.IntegrityStatus,
                IntegrityStatusName = NamaStatus(keutuhan.IntegrityStatus),
                AuthorUserId = keutuhan.AuthorUserId,
                AuthorName = namaPenulis,
                IsAuthorKnown = keutuhan.IsAuthorKnown,
                SignedAt = keutuhan.SignedAt,
                SignatureDeviceInfo = keutuhan.SignatureDeviceInfo,
                LockedAt = keutuhan.LockedAt,
                LockTrigger = keutuhan.LockTrigger,
                LockTriggerName = keutuhan.LockTrigger.HasValue
                    ? NamaPemicu(keutuhan.LockTrigger.Value)
                    : null,
                AddendumCount = keutuhan.AddendumCount,
                IsMutable = keutuhan.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft
            };
        }

        private static string NamaStatus(ClinicalDocumentIntegrityStatus status) => status switch
        {
            ClinicalDocumentIntegrityStatus.Draft => "Draf",
            ClinicalDocumentIntegrityStatus.Signed => "Ditandatangani",
            ClinicalDocumentIntegrityStatus.LockedUnsigned => "Terkunci, Tidak Ditandatangani",
            ClinicalDocumentIntegrityStatus.Cancelled => "Dibatalkan",
            _ => status.ToString()
        };

        private static string NamaPemicu(ClinicalDocumentLockTrigger pemicu) => pemicu switch
        {
            ClinicalDocumentLockTrigger.AuthorSigned => "Ditandatangani Penulis",
            ClinicalDocumentLockTrigger.EncounterClosed => "Kunjungan Ditutup",
            ClinicalDocumentLockTrigger.BackfillEncounterClosed => "Pengisian Data Lama",
            ClinicalDocumentLockTrigger.DocumentCancelled => "Dokumen Dibatalkan",
            _ => pemicu.ToString()
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
