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
using QuilvianSystemBackend.Services.Security;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/clinical-note-addendums")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Clinical Note Addendum",
        AreaName = "HealthServices",
        ControllerName = "ClinicalNoteAddendum",
        Description = "Koreksi catatan klinis yang sudah terkunci",
        SortOrder = 3
    )]
    [Tags("Health Services / Medical Record Management / Clinical Note Addendum")]
    public class ClinicalNoteAddendumController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly ClinicalNoteAddendumService _addendumService;
        private readonly AccessPermissionService _accessPermissionService;

        public ClinicalNoteAddendumController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            ClinicalNoteAddendumService addendumService,
            AccessPermissionService accessPermissionService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _addendumService = addendumService;
            _accessPermissionService = accessPermissionService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAddendumFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Addendum", Description = "Melihat daftar pilihan penyaring koreksi catatan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAddendum", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = new ClinicalNoteAddendumFilterMetadataResponse
            {
                DocumentKinds = MedicalRecordTimelineService.SeluruhJenis
                    .Select(x => new MedicalRecordDocumentKindOptionResponse
                    {
                        Value = x,
                        Name = MedicalRecordTimelineService.NamaJenis(x),
                        IsIntegrityEnforced = ClinicalDocumentIntegrityService.DitegakkanUntuk(x)
                    })
                    .ToList(),
                SortOptions =
                [
                    new() { Value = "sequence", Label = "Urutan koreksi" },
                    new() { Value = "signedAt", Label = "Tanggal ditandatangani" }
                ],
                SortDirections = ["asc", "desc"],
                PageSizeOptions = [10, 25, 50, 100],
                QueryParameters =
                [
                    new()
                    {
                        Name = "documentKind",
                        Type = "enum",
                        Required = "Yes",
                        Description = "Jenis dokumen yang dikoreksi.",
                        Example = "ProgressNote"
                    },
                    new()
                    {
                        Name = "documentId",
                        Type = "guid",
                        Required = "Yes",
                        Description = "Id dokumen pada tabel asalnya."
                    }
                ]
            };

            return Ok(ApiResponse<ClinicalNoteAddendumFilterMetadataResponse>.Ok(
                hasil, "Metadata filter koreksi catatan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAddendumSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Addendum", Description = "Melihat rekap koreksi catatan klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAddendum", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<TrxClinicalNoteAddendum>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var hasil = new ClinicalNoteAddendumSummaryResponse
            {
                TotalAddendum = await query.CountAsync(),
                BySubstituteAuthor = await query.CountAsync(x => x.IsSubstituteAuthor),
                ByOriginalAuthor = await query.CountAsync(x => !x.IsSubstituteAuthor),
                DocumentWithAddendum = await query.Select(x => x.IntegrityId).Distinct().CountAsync()
            };

            return Ok(ApiResponse<ClinicalNoteAddendumSummaryResponse>.Ok(
                hasil, "Rekap koreksi catatan klinis berhasil diambil."));
        }

        [HttpGet("by-document/{documentKind}/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<List<ClinicalNoteAddendumResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Addendum", Description = "Melihat koreksi catatan klinis", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAddendum", "Read")]
        public async Task<IActionResult> ListByDocument(
            ClinicalDocumentKind documentKind,
            Guid documentId)
        {
            var daftar = await _addendumService.ListByDocumentAsync(documentKind, documentId);

            var namaPenulis = await AmbilNamaPenggunaAsync(
                daftar.Select(x => x.AuthorUserId).Distinct().ToList());

            var response = daftar.Select(x => new ClinicalNoteAddendumResponse
            {
                Id = x.Id,
                IntegrityId = x.IntegrityId,
                Sequence = x.Sequence,
                AuthorUserId = x.AuthorUserId,
                AuthorName = namaPenulis.GetValueOrDefault(x.AuthorUserId),
                IsSubstituteAuthor = x.IsSubstituteAuthor,
                DelegationId = x.DelegationId,
                AddendumText = x.AddendumText,
                CorrectionReason = x.CorrectionReason,
                SignedAt = x.SignedAt
            }).ToList();

            return Ok(ApiResponse<List<ClinicalNoteAddendumResponse>>.Ok(
                response, "Daftar koreksi catatan berhasil diambil."));
        }

        [HttpGet("authority/{documentKind}/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<AddendumAuthorityResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Addendum", Description = "Memeriksa kewenangan membuat koreksi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAddendum", "Read")]
        public async Task<IActionResult> GetAuthority(
            ClinicalDocumentKind documentKind,
            Guid documentId)
        {
            var kewenangan = await _addendumService.ResolveAuthorityAsync(
                documentKind,
                documentId,
                GetCurrentUserId(),
                await PunyaKewenanganPenggantiAsync(),
                DateTime.UtcNow);

            return Ok(ApiResponse<AddendumAuthorityResponse>.Ok(
                kewenangan, "Kewenangan koreksi berhasil diperiksa."));
        }

        [HttpPost("by-document/{documentKind}/{documentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAddendumResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("Create", "Create Clinical Note Addendum", Description = "Menambahkan koreksi pada catatan sendiri yang sudah terkunci", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ClinicalNoteAddendum", "Create")]
        public Task<IActionResult> Create(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            [FromBody] CreateClinicalNoteAddendumRequest request)
            => BuatAddendumAsync(documentKind, documentId, request,
                                 sebagaiPengganti: false);

        [HttpPost("by-document/{documentKind}/{documentId:guid}/as-substitute")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAddendumResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [AccessAction("CreateAsSubstitute", "Create Clinical Note Addendum As Substitute", Description = "Menambahkan koreksi menggantikan penulis yang berhalangan", AccessType = AccessTypes.Create, SortOrder = 3)]
        [AccessPermission("ClinicalNoteAddendum", "CreateAsSubstitute")]
        public Task<IActionResult> CreateAsSubstitute(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            [FromBody] CreateClinicalNoteAddendumRequest request)
            => BuatAddendumAsync(documentKind, documentId, request,
                                 sebagaiPengganti: true);

        private async Task<IActionResult> BuatAddendumAsync(
            ClinicalDocumentKind documentKind,
            Guid documentId,
            CreateClinicalNoteAddendumRequest request,
            bool sebagaiPengganti)
        {
            var actorUserId = GetCurrentUserId();

            // AuthorUserId, IsSubstituteAuthor, dan DelegationId SENGAJA tidak diterima dari
            // klien. Bila boleh dikirim, pembuat addendum dapat mengaku sebagai orang lain —
            // persis celah RM-CAP-012 yang sedang ditutup modul ini.
            var (hasil, addendum) = await _addendumService.CreateAsync(
                documentKind,
                documentId,
                actorUserId,
                actorHasSubstituteAuthority: sebagaiPengganti,
                request.AddendumText,
                request.CorrectionReason,
                deviceInfo: Request.Headers.UserAgent.ToString(),
                ipAddress: HttpContext.Connection.RemoteIpAddress?.ToString(),
                nowUtc: DateTime.UtcNow);

            if (!hasil.IsAllowed || addendum == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Koreksi tidak dapat ditambahkan."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                sebagaiPengganti
                    ? "ClinicalNoteAddendum.CreateAsSubstitute"
                    : "ClinicalNoteAddendum.Create",
                "Koreksi catatan klinis ditambahkan.",
                new { EntityId = addendum.Id, addendum.IntegrityId, addendum.Sequence });

            var nama = await AmbilNamaPenggunaAsync([addendum.AuthorUserId]);

            var response = new ClinicalNoteAddendumResponse
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

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<ClinicalNoteAddendumResponse>.Ok(
                    response, "Koreksi berhasil ditambahkan."));
        }

        /// <summary>
        /// Memeriksa apakah pengguna berwenang membuat addendum sebagai pengganti.
        ///
        /// Dipakai hanya untuk endpoint pemeriksaan kewenangan, supaya layar tahu tombol mana
        /// yang boleh ditampilkan. Pada endpoint pembuatan, kewenangannya sudah ditegakkan
        /// atribut hak akses pada endpoint terpisah.
        /// </summary>
        private Task<bool> PunyaKewenanganPenggantiAsync()
            => _accessPermissionService.HasAccessAsync(
                User, "ClinicalNoteAddendum", "CreateAsSubstitute");

        private async Task<Dictionary<Guid, string>> AmbilNamaPenggunaAsync(List<Guid> userIds)
        {
            if (userIds.Count == 0)
                return [];

            return await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName);
        }

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
