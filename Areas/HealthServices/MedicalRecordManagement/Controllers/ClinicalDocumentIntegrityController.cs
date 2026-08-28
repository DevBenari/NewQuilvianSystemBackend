using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
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
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var actorUserId = GetCurrentUserId();
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);

            var query = _dbContext.Set<TrxClinicalDocumentIntegrity>()
                .AsNoTracking()
                .Where(x => x.AuthorUserId == actorUserId
                            && x.IntegrityStatus == ClinicalDocumentIntegrityStatus.Draft
                            && !x.IsDelete);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new UnsignedDocumentResponse
                {
                    IntegrityId = x.Id,
                    DocumentKind = x.DocumentKind,
                    DocumentKindName = x.DocumentKind.ToString(),
                    DocumentId = x.DocumentId,
                    PatientId = x.PatientId,
                    EncounterId = x.EncounterId,
                    CreatedAt = x.CreateDateTime
                })
                .ToListAsync();

            await LengkapiIdentitasAsync(items);

            var hasil = new ResponseUnsignedDocumentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseUnsignedDocumentPagedResult>.Ok(
                hasil, "Daftar catatan yang belum ditandatangani berhasil diambil."));
        }

        [HttpGet("by-encounter/{encounterId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalDocumentIntegrityResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Document Integrity", Description = "Melihat keutuhan dokumen per kunjungan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalDocumentIntegrity", "Read")]
        public async Task<IActionResult> GetByEncounter(Guid encounterId)
        {
            var daftar = await _dbContext.Set<TrxClinicalDocumentIntegrity>()
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
            TrxClinicalDocumentIntegrity keutuhan)
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

        /// <summary>
        /// Melengkapi nama pasien dan nomor kunjungan pada daftar.
        ///
        /// Diambil terpisah, bukan lewat join, supaya query utama tetap sederhana dan daftar
        /// tetap terbaca walaupun salah satu rujukan tidak ditemukan.
        /// </summary>
        private async Task LengkapiIdentitasAsync(List<UnsignedDocumentResponse> items)
        {
            if (items.Count == 0) return;

            var patientIds = items.Select(x => x.PatientId).Distinct().ToList();
            var encounterIds = items.Select(x => x.EncounterId).Distinct().ToList();

            var pasien = await _dbContext.Set<MstPatient>()
                .AsNoTracking()
                .Where(x => patientIds.Contains(x.Id))
                .Select(x => new { x.Id, x.FullName, x.MedicalRecordNumber })
                .ToListAsync();

            var kunjungan = await _dbContext.Set<TrxPatientEncounter>()
                .AsNoTracking()
                .Where(x => encounterIds.Contains(x.Id))
                .Select(x => new { x.Id, x.EncounterNumber })
                .ToListAsync();

            foreach (var item in items)
            {
                var p = pasien.FirstOrDefault(x => x.Id == item.PatientId);
                item.PatientName = p?.FullName;
                item.MedicalRecordNumber = p?.MedicalRecordNumber;
                item.EncounterNumber = kunjungan.FirstOrDefault(x => x.Id == item.EncounterId)?.EncounterNumber;
            }
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
