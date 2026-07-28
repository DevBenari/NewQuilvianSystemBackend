using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums.HumanResource;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_CREDENTIALING",
        moduleName: "Human Resource Credentialing",
        displayName: "Workforce Credential License",
        AreaName = "Corporate",
        ControllerName = "WorkforceCredentialLicense",
        Description = "Corporate human resource workforce credential license",
        SortOrder = 2)]
    [Tags("Corporate / Human Resource / Credentialing Management / Credential License")]
    public class WfpCredentialLicenseController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Credentialing";
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpCredentialLicenseController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseFilterMetadataResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License", Description = "Melihat metadata filter lisensi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetFilterMetadata(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var typeOptions = await _dbContext.Set<MstLicenseType>()
                .AsNoTracking()
                .Include(x => x.Profession)
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.LicenseTypeName)
                .Take(500)
                .Select(x => new WfpLicenseTypeOptionResponse
                {
                    Id = x.Id,
                    Code = x.LicenseTypeCode,
                    Name = x.LicenseTypeName,
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    IssuingAuthority = x.IssuingAuthority,
                    RegulatoryBody = x.RegulatoryBody,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsRenewable = x.IsRenewable,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    Label = x.LicenseTypeCode + " - " + x.LicenseTypeName
                })
                .ToListAsync(cancellationToken);

            var result = new WfpCredentialLicenseFilterMetadataResponse
            {
                DefaultFilter = new WfpCredentialLicenseDefaultFilterResponse(),
                LicenseTypeOptions = typeOptions,
                VerificationStatusOptions = BuildEnumOptions<CredentialVerificationStatus>(),
                SortOptions = new List<WfpCredentialLicenseSortOptionResponse>
                {
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "licenseType", Label = "Jenis lisensi" },
                    new() { Value = "licenseNumber", Label = "Nomor lisensi" },
                    new() { Value = "verificationStatus", Label = "Status verifikasi" },
                    new() { Value = "isPrimary", Label = "Lisensi utama" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isRevoked", Label = "Dicabut" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpCredentialLicenseFilterMetadataResponse>.Ok(
                result,
                "Metadata filter lisensi workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License", Description = "Melihat ringkasan lisensi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var today = DateTime.UtcNow.Date;
            var expiringLimit = today.AddDays(90);
            var query = _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpCredentialLicenseSummaryResponse
            {
                TotalLicense = await query.CountAsync(cancellationToken),
                ActiveLicense = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveLicense = await query.CountAsync(x => !x.IsActive, cancellationToken),
                PrimaryLicense = await query.CountAsync(x => x.IsPrimary && x.IsActive && !x.IsRevoked, cancellationToken),
                VerifiedLicense = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedLicense = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                RevokedLicense = await query.CountAsync(x => x.IsRevoked, cancellationToken),
                ExpiredLicense = await query.CountAsync(x => x.ExpiredDate < today, cancellationToken),
                ExpiringSoonLicense = await query.CountAsync(x =>
                    x.IsActive &&
                    !x.IsRevoked &&
                    x.ExpiredDate >= today &&
                    x.ExpiredDate <= expiringLimit,
                    cancellationToken)
            };

            return Ok(ApiResponse<WfpCredentialLicenseSummaryResponse>.Ok(
                result,
                "Ringkasan lisensi workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpCredentialLicenseResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License", Description = "Melihat data lisensi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetCredentialLicenses(
            Guid workforceProfileId,
            [FromQuery] Guid? licenseTypeId,
            [FromQuery] CredentialVerificationStatus? verificationStatus,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isRevoked,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isActive,
            [FromQuery] int? expiringWithinDays,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "expiredDate",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                licenseTypeId,
                verificationStatus,
                isPrimary,
                isVerified,
                isRevoked,
                isExpired,
                isActive,
                expiringWithinDays,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(
                entities.SelectMany(x => new[]
                {
                    x.CreateBy,
                    x.VerifiedByUserId ?? Guid.Empty,
                    x.RevokedByUserId ?? Guid.Empty
                }),
                cancellationToken);

            var result = new PagedResult<WfpCredentialLicenseResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpCredentialLicenseResponse>>.Ok(
                result,
                "Data lisensi workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Credential License", Description = "Melihat detail lisensi workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetCredentialLicenseById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(
                new[]
                {
                    entity.CreateBy,
                    entity.UpdateBy,
                    entity.VerifiedByUserId ?? Guid.Empty,
                    entity.RevokedByUserId ?? Guid.Empty
                },
                cancellationToken);

            return Ok(ApiResponse<WfpCredentialLicenseDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail lisensi workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Credential License", Description = "Membuat lisensi workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceCredentialLicense", "Create")]
        public async Task<IActionResult> CreateCredentialLicense(
            Guid workforceProfileId,
            [FromBody] CreateWfpCredentialLicenseRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
                return WorkforceProfileNotFound();

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var master = await FindLicenseTypeAsync(request.LicenseTypeId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.IsPrimary)
                    await UnsetOtherPrimaryAsync(workforceProfileId, null, now, actorUserId, cancellationToken);

                var entity = new WfpCredentialLicense
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    LicenseTypeId = NormalizeNullableGuid(request.LicenseTypeId),
                    CredentialingRequirementId = NormalizeNullableGuid(request.CredentialingRequirementId),
                    RequirementCode = NormalizeNullableText(request.RequirementCode),
                    LicenseType = master?.LicenseTypeName ?? request.LicenseType.Trim(),
                    LicenseNumber = request.LicenseNumber.Trim(),
                    Issuer = NormalizeNullableText(request.Issuer) ?? master?.IssuingAuthority,
                    PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                    IssueDate = NormalizeUtcDate(request.IssueDate),
                    ExpiredDate = NormalizeUtcDate(request.ExpiredDate),
                    VerificationStatus = request.VerificationStatus,
                    IsPrimary = request.IsPrimary,
                    IsVerified = request.IsVerified,
                    VerifiedAt = request.IsVerified ? now : null,
                    VerifiedByUserId = request.IsVerified ? actorUserId : null,
                    VerificationNotes = NormalizeNullableText(request.VerificationNotes),
                    IsRevoked = false,
                    BlocksSchedulingWhenInvalid = request.BlocksSchedulingWhenInvalid,
                    BlocksClinicalServiceWhenInvalid = request.BlocksClinicalServiceWhenInvalid,
                    FilePath = NormalizeNullableText(request.FilePath),
                    FileContentType = NormalizeNullableText(request.FileContentType),
                    Description = NormalizeNullableText(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpCredentialLicense>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceCredentialLicense.CreateCredentialLicense",
                    "Membuat lisensi workforce.",
                    new { entity.Id, entity.WorkforceProfileId, entity.LicenseType, entity.LicenseNumber, entity.IsPrimary });

                return await GetCredentialLicenseById(workforceProfileId, entity.Id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Credential License", Description = "Mengubah lisensi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> UpdateCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpCredentialLicenseRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            if (entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(400, "Lisensi yang sudah dicabut tidak dapat diubah."));

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var master = await FindLicenseTypeAsync(request.LicenseTypeId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.IsPrimary)
                    await UnsetOtherPrimaryAsync(workforceProfileId, id, now, actorUserId, cancellationToken);

                entity.LicenseTypeId = NormalizeNullableGuid(request.LicenseTypeId);
                entity.CredentialingRequirementId = NormalizeNullableGuid(request.CredentialingRequirementId);
                entity.RequirementCode = NormalizeNullableText(request.RequirementCode);
                entity.LicenseType = master?.LicenseTypeName ?? request.LicenseType.Trim();
                entity.LicenseNumber = request.LicenseNumber.Trim();
                entity.Issuer = NormalizeNullableText(request.Issuer) ?? master?.IssuingAuthority;
                entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
                entity.IssueDate = NormalizeUtcDate(request.IssueDate);
                entity.ExpiredDate = NormalizeUtcDate(request.ExpiredDate);
                entity.VerificationStatus = request.VerificationStatus;
                entity.IsPrimary = request.IsPrimary;
                entity.IsVerified = request.IsVerified;
                entity.VerifiedAt = request.IsVerified ? entity.VerifiedAt ?? now : null;
                entity.VerifiedByUserId = request.IsVerified ? entity.VerifiedByUserId ?? actorUserId : null;
                entity.VerificationNotes = NormalizeNullableText(request.VerificationNotes);
                entity.BlocksSchedulingWhenInvalid = request.BlocksSchedulingWhenInvalid;
                entity.BlocksClinicalServiceWhenInvalid = request.BlocksClinicalServiceWhenInvalid;
                entity.FilePath = NormalizeNullableText(request.FilePath);
                entity.FileContentType = NormalizeNullableText(request.FileContentType);
                entity.Description = NormalizeNullableText(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return await GetCredentialLicenseById(workforceProfileId, id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Credential License", Description = "Mengubah status lisensi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> UpdateCredentialLicenseStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpCredentialLicenseStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            if (request.IsActive && entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(400, "Lisensi yang sudah dicabut tidak dapat diaktifkan kembali."));

            entity.IsActive = request.IsActive;
            if (!request.IsActive)
                entity.IsPrimary = false;

            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetCredentialLicenseById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Credential License", Description = "Mengatur lisensi utama workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> SetPrimaryCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWfpCredentialLicensePrimaryRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            if (request.IsPrimary && (!entity.IsActive || entity.IsRevoked || entity.ExpiredDate.Date < DateTime.UtcNow.Date))
                return BadRequest(ApiResponse<object>.Fail(400, "Hanya lisensi aktif, belum dicabut, dan belum kedaluwarsa yang dapat dijadikan utama."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsPrimary)
                    await UnsetOtherPrimaryAsync(workforceProfileId, id, now, actorUserId, cancellationToken);

                entity.IsPrimary = request.IsPrimary;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return await GetCredentialLicenseById(workforceProfileId, id, cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Credential License", Description = "Memverifikasi lisensi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> VerifyCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpCredentialLicenseRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            if (request.IsVerified && entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(400, "Lisensi yang sudah dicabut tidak dapat diverifikasi."));

            if (request.IsVerified && entity.ExpiredDate.Date < DateTime.UtcNow.Date)
                return BadRequest(ApiResponse<object>.Fail(400, "Lisensi yang sudah kedaluwarsa tidak dapat diverifikasi sebagai valid."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsVerified = request.IsVerified;
            if (request.VerificationStatus.HasValue)
                entity.VerificationStatus = request.VerificationStatus.Value;
            entity.VerifiedAt = request.IsVerified ? now : null;
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.VerificationNotes = NormalizeNullableText(request.VerificationNotes);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetCredentialLicenseById(workforceProfileId, id, cancellationToken);
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<WfpCredentialLicenseDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Revoke Workforce Credential License", Description = "Mencabut lisensi workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> RevokeCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RevokeWfpCredentialLicenseRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            if (entity.IsRevoked)
                return BadRequest(ApiResponse<object>.Fail(400, "Lisensi sudah dicabut sebelumnya."));

            if (string.IsNullOrWhiteSpace(request.RevocationReason))
                return BadRequest(ApiResponse<object>.Fail(400, "Alasan pencabutan wajib diisi."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsRevoked = true;
            entity.RevokedAt = now;
            entity.RevokedByUserId = actorUserId;
            entity.RevocationReason = request.RevocationReason.Trim();
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.IsVerified = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetCredentialLicenseById(workforceProfileId, id, cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Credential License", Description = "Menghapus lisensi workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforceCredentialLicense", "Delete")]
        public async Task<IActionResult> DeleteCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await FindEntityAsync(workforceProfileId, id, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Lisensi workforce tidak ditemukan."));

            var usedByPrivilege = await _dbContext.Set<WfpClinicalPrivilege>()
                .AsNoTracking()
                .AnyAsync(x => x.CredentialLicenseId == id && !x.IsDelete, cancellationToken);

            if (usedByPrivilege)
                return BadRequest(ApiResponse<object>.Fail(400, "Lisensi masih digunakan oleh hak klinis dan tidak dapat dihapus."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCredentialLicense.DeleteCredentialLicense",
                "Menghapus lisensi workforce.",
                new { entity.Id, entity.WorkforceProfileId });

            return Ok(ApiResponse<object>.Ok(null, "Lisensi workforce berhasil dihapus."));
        }

        private IQueryable<WfpCredentialLicense> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.LicenseTypeMaster)
                    .ThenInclude(x => x!.Profession)
                .Include(x => x.VerifiedByUser)
                .Include(x => x.RevokedByUser)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpCredentialLicense> ApplyFilter(
            IQueryable<WfpCredentialLicense> query,
            Guid? licenseTypeId,
            CredentialVerificationStatus? verificationStatus,
            bool? isPrimary,
            bool? isVerified,
            bool? isRevoked,
            bool? isExpired,
            bool? isActive,
            int? expiringWithinDays,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (licenseTypeId.HasValue && licenseTypeId.Value != Guid.Empty)
                query = query.Where(x => x.LicenseTypeId == licenseTypeId.Value);
            if (verificationStatus.HasValue)
                query = query.Where(x => x.VerificationStatus == verificationStatus.Value);
            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);
            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isRevoked.HasValue)
                query = query.Where(x => x.IsRevoked == isRevoked.Value);
            if (isExpired.HasValue)
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate < today)
                    : query.Where(x => x.ExpiredDate >= today);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (expiringWithinDays.HasValue && expiringWithinDays.Value >= 0)
            {
                var limit = today.AddDays(Math.Min(expiringWithinDays.Value, 3650));
                query = query.Where(x =>
                    !x.IsRevoked &&
                    x.ExpiredDate >= today &&
                    x.ExpiredDate <= limit);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.LicenseType.ToLower().Contains(keyword) ||
                    x.LicenseNumber.ToLower().Contains(keyword) ||
                    (x.Issuer != null && x.Issuer.ToLower().Contains(keyword)) ||
                    (x.PracticeLocation != null && x.PracticeLocation.ToLower().Contains(keyword)) ||
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    (x.LicenseTypeMaster != null && x.LicenseTypeMaster.LicenseTypeCode.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpCredentialLicense> ApplySorting(
            IQueryable<WfpCredentialLicense> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "expiredDate").Trim().ToLowerInvariant() switch
            {
                "issuedate" => desc ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate),
                "licensetype" => desc ? query.OrderByDescending(x => x.LicenseType) : query.OrderBy(x => x.LicenseType),
                "licensenumber" => desc ? query.OrderByDescending(x => x.LicenseNumber) : query.OrderBy(x => x.LicenseNumber),
                "verificationstatus" => desc ? query.OrderByDescending(x => x.VerificationStatus) : query.OrderBy(x => x.VerificationStatus),
                "isprimary" => desc ? query.OrderByDescending(x => x.IsPrimary) : query.OrderBy(x => x.IsPrimary),
                "isverified" => desc ? query.OrderByDescending(x => x.IsVerified) : query.OrderBy(x => x.IsVerified),
                "isrevoked" => desc ? query.OrderByDescending(x => x.IsRevoked) : query.OrderBy(x => x.IsRevoked),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? currentId,
            CreateWfpCredentialLicenseRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseType))
                return (false, "Jenis lisensi wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.LicenseNumber))
                return (false, "Nomor lisensi wajib diisi.");
            if (request.IssueDate == default)
                return (false, "Tanggal terbit wajib diisi.");
            if (request.ExpiredDate == default)
                return (false, "Tanggal kedaluwarsa wajib diisi.");
            if (request.ExpiredDate.Date < request.IssueDate.Date)
                return (false, "Tanggal kedaluwarsa tidak boleh lebih kecil dari tanggal terbit.");

            var master = await FindLicenseTypeAsync(request.LicenseTypeId, cancellationToken);
            if (request.LicenseTypeId.HasValue && master == null)
                return (false, "Master jenis lisensi tidak ditemukan atau tidak aktif.");

            if (request.IsVerified && master?.RequiresDocument == true && string.IsNullOrWhiteSpace(request.FilePath))
                return (false, "Dokumen lisensi wajib tersedia sebelum data diverifikasi.");

            var requirementId = NormalizeNullableGuid(request.CredentialingRequirementId);
            if (requirementId.HasValue)
            {
                var requirementExists = await _dbContext.Set<MstCredentialingRequirement>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == requirementId.Value && x.IsActive && !x.IsDelete, cancellationToken);

                if (!requirementExists)
                    return (false, "Credentialing requirement tidak ditemukan atau tidak aktif.");
            }

            var normalizedType = master?.LicenseTypeName ?? request.LicenseType.Trim();
            var normalizedNumber = request.LicenseNumber.Trim();
            var duplicate = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == normalizedType &&
                    x.LicenseNumber == normalizedNumber &&
                    x.Id != currentId &&
                    !x.IsDelete,
                    cancellationToken);

            if (duplicate)
                return (false, "Jenis dan nomor lisensi yang sama sudah tersedia untuk workforce ini.");

            return (true, null);
        }

        private async Task<MstLicenseType?> FindLicenseTypeAsync(Guid? id, CancellationToken cancellationToken)
        {
            id = NormalizeNullableGuid(id);
            if (!id.HasValue)
                return null;

            return await _dbContext.Set<MstLicenseType>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id.Value && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task UnsetOtherPrimaryAsync(
            Guid workforceProfileId,
            Guid? exceptId,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<WfpCredentialLicense>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsPrimary &&
                    !x.IsDelete);

            if (exceptId.HasValue)
                query = query.Where(x => x.Id != exceptId.Value);

            var items = await query.ToListAsync(cancellationToken);
            foreach (var item in items)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<WfpCredentialLicense?> FindEntityAsync(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete,
                    cancellationToken);
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid id, CancellationToken cancellationToken)
        {
            return id != Guid.Empty && await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == id && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private IActionResult WorkforceProfileNotFound() =>
            NotFound(ApiResponse<object>.Fail(404, "Profil tenaga kerja tidak ditemukan."));

        private static WfpCredentialLicenseResponse MapResponse(
            WfpCredentialLicense x,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var today = DateTime.UtcNow.Date;
            var days = (int)(x.ExpiredDate.Date - today).TotalDays;
            return new WfpCredentialLicenseResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                LicenseTypeId = x.LicenseTypeId,
                LicenseTypeCode = x.LicenseTypeMaster?.LicenseTypeCode,
                LicenseTypeMasterName = x.LicenseTypeMaster?.LicenseTypeName,
                ProfessionId = x.LicenseTypeMaster?.ProfessionId,
                ProfessionCode = x.LicenseTypeMaster?.Profession?.ProfessionCode,
                ProfessionName = x.LicenseTypeMaster?.Profession?.ProfessionName,
                CredentialingRequirementId = x.CredentialingRequirementId,
                RequirementCode = x.RequirementCode,
                LicenseType = x.LicenseType,
                LicenseNumber = x.LicenseNumber,
                Issuer = x.Issuer,
                PracticeLocation = x.PracticeLocation,
                IssueDate = x.IssueDate,
                ExpiredDate = x.ExpiredDate,
                IsExpired = x.ExpiredDate.Date < today,
                DaysUntilExpiry = days,
                VerificationStatus = x.VerificationStatus,
                VerificationStatusName = BuildEnumLabel(x.VerificationStatus.ToString()),
                IsPrimary = x.IsPrimary,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = GetActorName(actorNames, x.VerifiedByUserId ?? Guid.Empty),
                VerificationNotes = x.VerificationNotes,
                IsRevoked = x.IsRevoked,
                RevokedAt = x.RevokedAt,
                RevokedByUserId = x.RevokedByUserId,
                RevokedByUserName = GetActorName(actorNames, x.RevokedByUserId ?? Guid.Empty),
                RevocationReason = x.RevocationReason,
                BlocksSchedulingWhenInvalid = x.BlocksSchedulingWhenInvalid,
                BlocksClinicalServiceWhenInvalid = x.BlocksClinicalServiceWhenInvalid,
                FilePath = x.FilePath,
                FileContentType = x.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy)
            };
        }

        private static WfpCredentialLicenseDetailResponse MapDetailResponse(
            WfpCredentialLicense x,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var value = MapResponse(x, actorNames);
            return new WfpCredentialLicenseDetailResponse
            {
                Id = value.Id,
                WorkforceProfileId = value.WorkforceProfileId,
                WorkforceProfileCode = value.WorkforceProfileCode,
                WorkforceDisplayName = value.WorkforceDisplayName,
                LicenseTypeId = value.LicenseTypeId,
                LicenseTypeCode = value.LicenseTypeCode,
                LicenseTypeMasterName = value.LicenseTypeMasterName,
                ProfessionId = value.ProfessionId,
                ProfessionCode = value.ProfessionCode,
                ProfessionName = value.ProfessionName,
                CredentialingRequirementId = value.CredentialingRequirementId,
                RequirementCode = value.RequirementCode,
                LicenseType = value.LicenseType,
                LicenseNumber = value.LicenseNumber,
                Issuer = value.Issuer,
                PracticeLocation = value.PracticeLocation,
                IssueDate = value.IssueDate,
                ExpiredDate = value.ExpiredDate,
                IsExpired = value.IsExpired,
                DaysUntilExpiry = value.DaysUntilExpiry,
                VerificationStatus = value.VerificationStatus,
                VerificationStatusName = value.VerificationStatusName,
                IsPrimary = value.IsPrimary,
                IsVerified = value.IsVerified,
                VerifiedAt = value.VerifiedAt,
                VerifiedByUserId = value.VerifiedByUserId,
                VerifiedByUserName = value.VerifiedByUserName,
                VerificationNotes = value.VerificationNotes,
                IsRevoked = value.IsRevoked,
                RevokedAt = value.RevokedAt,
                RevokedByUserId = value.RevokedByUserId,
                RevokedByUserName = value.RevokedByUserName,
                RevocationReason = value.RevocationReason,
                BlocksSchedulingWhenInvalid = value.BlocksSchedulingWhenInvalid,
                BlocksClinicalServiceWhenInvalid = value.BlocksClinicalServiceWhenInvalid,
                FilePath = value.FilePath,
                FileContentType = value.FileContentType,
                HasFile = value.HasFile,
                Description = value.Description,
                IsActive = value.IsActive,
                CreateDateTime = value.CreateDateTime,
                CreateBy = value.CreateBy,
                CreateByName = value.CreateByName,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds,
            CancellationToken cancellationToken)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name, cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actorNames, Guid actorId) =>
            actorId == Guid.Empty
                ? null
                : actorNames.TryGetValue(actorId, out var value) ? value : null;

        private static List<WfpCredentialLicenseEnumOptionResponse> BuildEnumOptions<TEnum>()
            where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(x => new WfpCredentialLicenseEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = BuildEnumLabel(x.ToString())
                })
                .ToList();
        }

        private static string BuildEnumLabel(string value) =>
            Regex.Replace(value, "([a-z0-9])([A-Z])", "$1 $2");

        private Guid GetCurrentUserId()
        {
            var text = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value) =>
            !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static DateTime NormalizeUtcDate(DateTime value) =>
            DateTime.SpecifyKind(value.Date, DateTimeKind.Utc);
    }
}
