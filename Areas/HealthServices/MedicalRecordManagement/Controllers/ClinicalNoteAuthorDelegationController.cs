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

using ResponseAuthorDelegationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs.AuthorDelegationResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-record-management/clinical-note-author-delegations")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MEDICAL_RECORD",
        moduleName: "Health Service Medical Record",
        displayName: "Clinical Note Author Delegation",
        AreaName = "HealthServices",
        ControllerName = "ClinicalNoteAuthorDelegation",
        Description = "Penetapan penulis catatan klinis yang berhalangan",
        SortOrder = 4
    )]
    [Tags("Health Services / Medical Record Management / Clinical Note Author Delegation")]
    public class ClinicalNoteAuthorDelegationController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MedicalRecord";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly ClinicalNoteAuthorDelegationService _delegationService;

        public ClinicalNoteAuthorDelegationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            ClinicalNoteAuthorDelegationService delegationService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _delegationService = delegationService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAuthorDelegationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Author Delegation", Description = "Melihat daftar pilihan penyaring penetapan berhalangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAuthorDelegation", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var hasil = new ClinicalNoteAuthorDelegationFilterMetadataResponse
            {
                Triggers = Enum.GetValues<AuthorDelegationTrigger>()
                    .Select(x => new MedicalRecordEnumOptionResponse
                    {
                        Value = (int)x,
                        Name = x.ToString(),
                        Label = NamaPemicuPenetapan(x)
                    })
                    .ToList(),
                SortOptions =
                [
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "validFrom", Label = "Berlaku mulai" },
                    new() { Value = "validUntil", Label = "Berlaku sampai" }
                ],
                SortDirections = ["asc", "desc"],
                PageSizeOptions = [10, 25, 50, 100],
                QueryParameters =
                [
                    new()
                    {
                        Name = "originalAuthorUserId",
                        Type = "guid",
                        Description = "Menyaring penetapan untuk satu penulis tertentu."
                    },
                    new()
                    {
                        Name = "isActive",
                        Type = "boolean",
                        Description = "Menyaring penetapan yang masih membuka jalur koreksi.",
                        Example = "true"
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

            return Ok(ApiResponse<ClinicalNoteAuthorDelegationFilterMetadataResponse>.Ok(
                hasil, "Metadata filter penetapan berhalangan berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ClinicalNoteAuthorDelegationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Author Delegation", Description = "Melihat rekap penetapan penulis berhalangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAuthorDelegation", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var sekarang = DateTime.UtcNow;

            var query = _dbContext.Set<TrxClinicalNoteAuthorDelegation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var hasil = new ClinicalNoteAuthorDelegationSummaryResponse
            {
                TotalDelegation = await query.CountAsync(),
                ActiveDelegation = await query.CountAsync(
                    x => x.IsActive
                         && x.RevokedAt == null
                         && (x.ValidUntil == null || x.ValidUntil >= sekarang)),
                ExpiredDelegation = await query.CountAsync(
                    x => x.RevokedAt == null
                         && x.ValidUntil != null
                         && x.ValidUntil < sekarang),
                RevokedDelegation = await query.CountAsync(x => x.RevokedAt != null),
                ByUnitHeadGrant = await query.CountAsync(
                    x => x.Trigger == AuthorDelegationTrigger.UnitHeadGrant),
                ByInactiveAccount = await query.CountAsync(
                    x => x.Trigger == AuthorDelegationTrigger.InactiveAccount)
            };

            return Ok(ApiResponse<ClinicalNoteAuthorDelegationSummaryResponse>.Ok(
                hasil, "Rekap penetapan penulis berhalangan berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseAuthorDelegationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Clinical Note Author Delegation", Description = "Melihat penetapan penulis berhalangan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ClinicalNoteAuthorDelegation", "Read")]
        public async Task<IActionResult> GetDelegations(
            [FromQuery] Guid? originalAuthorUserId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            (pageNumber, pageSize) = NormalizePaging(pageNumber, pageSize);
            var sekarang = DateTime.UtcNow;

            var query = _dbContext.Set<TrxClinicalNoteAuthorDelegation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (originalAuthorUserId.HasValue && originalAuthorUserId.Value != Guid.Empty)
                query = query.Where(x => x.OriginalAuthorUserId == originalAuthorUserId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            var totalData = await query.CountAsync();

            var daftar = await query
                .OrderByDescending(x => x.ValidFrom)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var nama = await AmbilNamaPenggunaAsync(daftar
                .SelectMany(x => new[] { x.OriginalAuthorUserId, x.GrantedByUserId ?? Guid.Empty })
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList());

            var items = daftar.Select(x => ToResponse(x, nama, sekarang)).ToList();

            var hasil = new ResponseAuthorDelegationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseAuthorDelegationPagedResult>.Ok(
                hasil, "Daftar penetapan berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<AuthorDelegationResponse>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction("Create", "Create Clinical Note Author Delegation", Description = "Menetapkan penulis catatan berhalangan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ClinicalNoteAuthorDelegation", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateAuthorDelegationRequest request)
        {
            var actorUserId = GetCurrentUserId();
            var sekarang = DateTime.UtcNow;

            var (hasil, penetapan) = await _delegationService.CreateAsync(
                request.OriginalAuthorUserId,
                actorUserId,
                request.GrantReason,
                request.ValidUntil,
                sekarang);

            if (!hasil.IsAllowed || penetapan == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Penetapan tidak dapat dibuat."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalNoteAuthorDelegation.Create",
                "Penetapan penulis berhalangan dibuat.",
                new { EntityId = penetapan.Id, penetapan.OriginalAuthorUserId, penetapan.ValidUntil });

            var nama = await AmbilNamaPenggunaAsync(
                [penetapan.OriginalAuthorUserId, actorUserId]);

            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<AuthorDelegationResponse>.Ok(
                    ToResponse(penetapan, nama, sekarang),
                    "Penetapan berhasil dibuat."));
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<AuthorDelegationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Revoke Clinical Note Author Delegation", Description = "Mencabut penetapan penulis berhalangan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ClinicalNoteAuthorDelegation", "Update")]
        public async Task<IActionResult> Revoke(Guid id, [FromBody] RevokeAuthorDelegationRequest request)
        {
            var actorUserId = GetCurrentUserId();
            var sekarang = DateTime.UtcNow;

            var (hasil, penetapan) = await _delegationService.RevokeAsync(id, actorUserId, sekarang);

            if (!hasil.IsAllowed || penetapan == null)
            {
                return StatusCode(hasil.StatusCode, ApiResponse<object>.Fail(
                    hasil.StatusCode,
                    hasil.ErrorMessage ?? "Penetapan tidak dapat dicabut."
                ));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "ClinicalNoteAuthorDelegation.Revoke",
                "Penetapan penulis berhalangan dicabut.",
                new { EntityId = penetapan.Id, request.RevokeReason });

            var nama = await AmbilNamaPenggunaAsync(
                [penetapan.OriginalAuthorUserId, actorUserId]);

            return Ok(ApiResponse<AuthorDelegationResponse>.Ok(
                ToResponse(penetapan, nama, sekarang), "Penetapan berhasil dicabut."));
        }

        private static AuthorDelegationResponse ToResponse(
            TrxClinicalNoteAuthorDelegation penetapan,
            Dictionary<Guid, string> nama,
            DateTime sekarang) => new()
            {
                Id = penetapan.Id,
                OriginalAuthorUserId = penetapan.OriginalAuthorUserId,
                OriginalAuthorName = nama.GetValueOrDefault(penetapan.OriginalAuthorUserId),
                Trigger = penetapan.Trigger,
                TriggerName = penetapan.Trigger == AuthorDelegationTrigger.InactiveAccount
                    ? "Akun Nonaktif"
                    : "Penetapan Kepala Unit",
                GrantedByUserId = penetapan.GrantedByUserId,
                GrantedByName = penetapan.GrantedByUserId.HasValue
                    ? nama.GetValueOrDefault(penetapan.GrantedByUserId.Value)
                    : null,
                GrantReason = penetapan.GrantReason,
                ValidFrom = penetapan.ValidFrom,
                ValidUntil = penetapan.ValidUntil,
                RevokedAt = penetapan.RevokedAt,
                IsActive = penetapan.IsActive,
                IsCurrentlyValid = penetapan.IsActive
                                   && penetapan.RevokedAt == null
                                   && penetapan.ValidFrom <= sekarang
                                   && (!penetapan.ValidUntil.HasValue
                                       || penetapan.ValidUntil.Value >= sekarang)
            };

        private async Task<Dictionary<Guid, string>> AmbilNamaPenggunaAsync(List<Guid> userIds)
        {
            var bersih = userIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (bersih.Count == 0)
                return [];

            return await _dbContext.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(x => bersih.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static string NamaPemicuPenetapan(AuthorDelegationTrigger pemicu) => pemicu switch
        {
            AuthorDelegationTrigger.InactiveAccount => "Akun Nonaktif",
            AuthorDelegationTrigger.UnitHeadGrant => "Penetapan Kepala Unit",
            _ => pemicu.ToString()
        };

        private Guid GetCurrentUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userId, out var id) ? id : Guid.Empty;
        }
    }
}
