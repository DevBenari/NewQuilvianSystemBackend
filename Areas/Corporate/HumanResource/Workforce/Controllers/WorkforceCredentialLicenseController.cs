using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/credential-licenses")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Credential License",
        AreaName = "Corporate",
        ControllerName = "WorkforceCredentialLicense",
        Description = "Workforce credential license management",
        SortOrder = 27
    )]
    [Tags("Corporate / Human Resource / Workforce / Credential License")]
    public class WorkforceCredentialLicenseController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.CredentialLicense";
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions =
        {
            ".pdf",
            ".jpg",
            ".jpeg",
            ".png",
            ".doc",
            ".docx",
            ".xls",
            ".xlsx"
        };

        private static readonly HashSet<string> AllowedLicenseTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "STR",
            "SIP",
            "SIK",
            "SIPP",
            "SIPA",
            "SIPB"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IWebHostEnvironment _environment;

        public WorkforceCredentialLicenseController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Credential License",
            Description = "Melihat credential license workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetCredentialLicenses(
            Guid workforceProfileId,
            [FromQuery] string? licenseType,
            [FromQuery] CredentialVerificationStatus? verificationStatus,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isExpired)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;
            var normalizedLicenseType = NormalizeLicenseTypeOrNull(licenseType);

            var query = _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (!string.IsNullOrWhiteSpace(normalizedLicenseType))
            {
                query = query.Where(x => x.LicenseType == normalizedLicenseType);
            }

            if (verificationStatus.HasValue)
            {
                query = query.Where(x => x.VerificationStatus == verificationStatus.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(x => x.IsVerified == isVerified.Value);
            }

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate < today)
                    : query.Where(x => x.ExpiredDate >= today);
            }

            var rawItems = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.ExpiredDate)
                .ThenBy(x => x.LicenseType)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    x.RequirementCode,
                    x.LicenseType,
                    x.LicenseNumber,
                    x.Issuer,
                    x.IssueDate,
                    x.ExpiredDate,
                    x.PracticeLocation,
                    x.FilePath,
                    x.FileContentType,
                    x.VerificationStatus,
                    x.IsVerified,
                    x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                    x.VerifiedAt,
                    x.VerificationNote,
                    x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    x.RejectedAt,
                    x.RejectedReason,
                    x.RevokedByUserId,
                    RevokedByUserName = x.RevokedByUser != null ? x.RevokedByUser.DisplayName : null,
                    x.RevokedAt,
                    x.RevokedReason,
                    x.IsPrimary,
                    x.IsActive,
                    x.Description,
                    x.CreateDateTime
                })
                .ToListAsync();

            var items = rawItems.Select(x => new WorkforceCredentialLicenseResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = x.RequirementCode,
                LicenseType = x.LicenseType,
                LicenseNumber = x.LicenseNumber,
                Issuer = x.Issuer,
                IssueDate = x.IssueDate,
                ExpiredDate = x.ExpiredDate,
                PracticeLocation = x.PracticeLocation,
                FilePath = x.FilePath,
                FileContentType = x.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                VerificationStatus = ResolveRuntimeStatus(x.VerificationStatus, x.ExpiredDate, x.IsVerified),
                IsVerified = x.IsVerified,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByUserName = x.VerifiedByUserName,
                VerifiedAt = x.VerifiedAt,
                VerificationNote = x.VerificationNote,
                RejectedByUserId = x.RejectedByUserId,
                RejectedByUserName = x.RejectedByUserName,
                RejectedAt = x.RejectedAt,
                RejectedReason = x.RejectedReason,
                RevokedByUserId = x.RevokedByUserId,
                RevokedByUserName = x.RevokedByUserName,
                RevokedAt = x.RevokedAt,
                RevokedReason = x.RevokedReason,
                IsPrimary = x.IsPrimary,
                IsExpired = x.ExpiredDate.Date < today,
                IsCurrentlyValid = x.IsActive && x.IsVerified &&
                    x.VerificationStatus == CredentialVerificationStatus.Verified &&
                    x.ExpiredDate.Date >= today,
                IsActive = x.IsActive,
                Description = x.Description,
                CreateDateTime = x.CreateDateTime
            }).ToList();

            var result = new WorkforceCredentialLicenseListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                PendingVerificationData = items.Count(x => x.VerificationStatus == CredentialVerificationStatus.PendingVerification),
                RejectedData = items.Count(x => x.VerificationStatus == CredentialVerificationStatus.Rejected),
                RevokedData = items.Count(x => x.VerificationStatus == CredentialVerificationStatus.Revoked),
                ExpiredData = items.Count(x => x.IsExpired),
                CurrentlyValidData = items.Count(x => x.IsCurrentlyValid),
                CredentialLicenseWithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCredentialLicenseListResponse>.Ok(
                result,
                "Data credential license workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Credential License",
            Description = "Melihat detail credential license workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> GetCredentialLicenseById(Guid workforceProfileId, Guid id)
        {
            var response = await BuildCredentialLicenseResponseAsync(id, workforceProfileId);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response,
                "Detail credential license workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Credential License",
            Description = "Menambah credential license workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceCredentialLicense", "Create")]
        public async Task<IActionResult> CreateCredentialLicense(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceCredentialLicenseRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateCredentialLicenseRequest(
                request.LicenseType,
                request.LicenseNumber,
                request.IssueDate,
                request.ExpiredDate,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data credential license tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);
            var normalizedLicenseType = NormalizeLicenseTypeOrNull(request.LicenseType) ?? string.Empty;
            var normalizedLicenseNumber = NormalizeNullableText(request.LicenseNumber) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "License",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            var duplicate = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == normalizedLicenseType &&
                    x.LicenseNumber == normalizedLicenseNumber &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "LicenseType dan LicenseNumber sudah terdaftar pada workforce profile ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveCredentialLicenseFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            if (request.IsPrimary)
            {
                await DeactivateOtherPrimaryAsync(workforceProfileId, normalizedLicenseType, null, now, actorUserId);
            }

            var entity = new WfpCredentialLicense
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = normalizedRequirementCode,
                LicenseType = normalizedLicenseType,
                LicenseNumber = normalizedLicenseNumber,
                Issuer = NormalizeNullableText(request.Issuer),
                IssueDate = request.IssueDate.Date,
                ExpiredDate = request.ExpiredDate.Date,
                PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                FilePath = filePath,
                FileContentType = fileContentType,
                VerificationStatus = CredentialVerificationStatus.Unverified,
                IsVerified = false,
                IsPrimary = request.IsPrimary,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpCredentialLicense>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id, workforceProfileId);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCredentialLicense.CreateCredentialLicense",
                "Credential license workforce berhasil dibuat.",
                new
                {
                    WorkforceProfileId = workforceProfileId,
                    entity.Id,
                    entity.RequirementCode,
                    entity.LicenseType,
                    entity.LicenseNumber
                }
            );

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Credential license workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Mengubah credential license workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> UpdateCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceCredentialLicenseRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (entity.IsVerified || entity.VerificationStatus == CredentialVerificationStatus.Verified)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license yang sudah verified tidak boleh diubah. Gunakan revoke jika license tidak berlaku lagi."
                ));
            }

            var validation = ValidateCredentialLicenseRequest(
                request.LicenseType,
                request.LicenseNumber,
                request.IssueDate,
                request.ExpiredDate,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data credential license tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);
            var normalizedLicenseType = NormalizeLicenseTypeOrNull(request.LicenseType) ?? string.Empty;
            var normalizedLicenseNumber = NormalizeNullableText(request.LicenseNumber) ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "License",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            var duplicate = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == normalizedLicenseType &&
                    x.LicenseNumber == normalizedLicenseNumber &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "LicenseType dan LicenseNumber sudah terdaftar pada workforce profile ini."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            if (request.ReplaceExistingFile && request.File == null)
            {
                entity.FilePath = null;
                entity.FileContentType = null;
            }

            if (request.File != null)
            {
                var savedFile = await SaveCredentialLicenseFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            if (request.IsPrimary)
            {
                await DeactivateOtherPrimaryAsync(workforceProfileId, normalizedLicenseType, id, now, actorUserId);
            }

            entity.RequirementCode = normalizedRequirementCode;
            entity.LicenseType = normalizedLicenseType;
            entity.LicenseNumber = normalizedLicenseNumber;
            entity.Issuer = NormalizeNullableText(request.Issuer);
            entity.IssueDate = request.IssueDate.Date;
            entity.ExpiredDate = request.ExpiredDate.Date;
            entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
            entity.IsPrimary = request.IsPrimary;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Credential license workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Mengubah status aktif credential license workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> UpdateCredentialLicenseStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceCredentialLicenseStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Status credential license workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Verifikasi credential license workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> VerifyCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceCredentialLicenseRequest request)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license nonaktif tidak bisa diverifikasi."
                ));
            }

            if (entity.ExpiredDate.Date < DateTime.UtcNow.Date)
            {
                entity.IsVerified = false;
                entity.VerificationStatus = CredentialVerificationStatus.Expired;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();
                await _dbContext.SaveChangesAsync();

                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license sudah expired dan tidak bisa diverifikasi."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = true;
            entity.VerificationStatus = CredentialVerificationStatus.Verified;
            entity.VerifiedByUserId = actorUserId;
            entity.VerifiedAt = now;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.RejectedByUserId = null;
            entity.RejectedAt = null;
            entity.RejectedReason = null;
            entity.RevokedByUserId = null;
            entity.RevokedAt = null;
            entity.RevokedReason = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Credential license workforce berhasil diverifikasi."
            ));
        }

        [HttpPatch("{id:guid}/reject")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Reject verifikasi credential license workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> RejectCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RejectWorkforceCredentialLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RejectedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan reject wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = false;
            entity.VerificationStatus = CredentialVerificationStatus.Rejected;
            entity.RejectedByUserId = actorUserId;
            entity.RejectedAt = now;
            entity.RejectedReason = request.RejectedReason.Trim();
            entity.VerifiedByUserId = null;
            entity.VerifiedAt = null;
            entity.VerificationNote = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Credential license workforce berhasil di-reject."
            ));
        }

        [HttpPatch("{id:guid}/revoke")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCredentialLicenseResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Revoke credential license workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCredentialLicense", "Update")]
        public async Task<IActionResult> RevokeCredentialLicense(
            Guid workforceProfileId,
            Guid id,
            [FromBody] RevokeWorkforceCredentialLicenseRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RevokedReason))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Alasan revoke wajib diisi."
                ));
            }

            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsVerified = false;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.VerificationStatus = CredentialVerificationStatus.Revoked;
            entity.RevokedByUserId = actorUserId;
            entity.RevokedAt = now;
            entity.RevokedReason = request.RevokedReason.Trim();
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id, workforceProfileId);

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Credential license workforce berhasil di-revoke."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Credential License",
            Description = "Download file credential license workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> DownloadCredentialLicense(Guid workforceProfileId, Guid id)
        {
            var license = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (license == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(license.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license belum memiliki file."
                ));
            }

            var relativePath = license.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var physicalPath = Path.Combine(rootPath, relativePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File credential license tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = license.FileContentType ?? "application/octet-stream";
            }

            var fileName = Path.GetFileName(physicalPath);
            var bytes = await System.IO.File.ReadAllBytesAsync(physicalPath);

            return File(bytes, contentType, fileName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction(
            "Delete",
            "Delete Workforce Credential License",
            Description = "Menghapus credential license workforce profile",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceCredentialLicense", "Delete")]
        public async Task<IActionResult> DeleteCredentialLicense(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpCredentialLicense>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Credential license workforce tidak ditemukan."
                ));
            }

            if (entity.IsVerified || entity.VerificationStatus == CredentialVerificationStatus.Verified)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Credential license verified tidak boleh dihapus. Gunakan revoke agar audit trail tetap aman."
                ));
            }

            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Credential license workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<WorkforceCredentialLicenseResponse?> BuildCredentialLicenseResponseAsync(
            Guid id,
            Guid workforceProfileId)
        {
            var today = DateTime.UtcNow.Date;

            var item = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    x.RequirementCode,
                    x.LicenseType,
                    x.LicenseNumber,
                    x.Issuer,
                    x.IssueDate,
                    x.ExpiredDate,
                    x.PracticeLocation,
                    x.FilePath,
                    x.FileContentType,
                    x.VerificationStatus,
                    x.IsVerified,
                    x.VerifiedByUserId,
                    VerifiedByUserName = x.VerifiedByUser != null ? x.VerifiedByUser.DisplayName : null,
                    x.VerifiedAt,
                    x.VerificationNote,
                    x.RejectedByUserId,
                    RejectedByUserName = x.RejectedByUser != null ? x.RejectedByUser.DisplayName : null,
                    x.RejectedAt,
                    x.RejectedReason,
                    x.RevokedByUserId,
                    RevokedByUserName = x.RevokedByUser != null ? x.RevokedByUser.DisplayName : null,
                    x.RevokedAt,
                    x.RevokedReason,
                    x.IsPrimary,
                    x.IsActive,
                    x.Description,
                    x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (item == null)
            {
                return null;
            }

            return new WorkforceCredentialLicenseResponse
            {
                Id = item.Id,
                WorkforceProfileId = item.WorkforceProfileId,
                ProfileCode = item.ProfileCode,
                DisplayName = item.DisplayName,
                RequirementCode = item.RequirementCode,
                LicenseType = item.LicenseType,
                LicenseNumber = item.LicenseNumber,
                Issuer = item.Issuer,
                IssueDate = item.IssueDate,
                ExpiredDate = item.ExpiredDate,
                PracticeLocation = item.PracticeLocation,
                FilePath = item.FilePath,
                FileContentType = item.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(item.FilePath),
                VerificationStatus = ResolveRuntimeStatus(item.VerificationStatus, item.ExpiredDate, item.IsVerified),
                IsVerified = item.IsVerified,
                VerifiedByUserId = item.VerifiedByUserId,
                VerifiedByUserName = item.VerifiedByUserName,
                VerifiedAt = item.VerifiedAt,
                VerificationNote = item.VerificationNote,
                RejectedByUserId = item.RejectedByUserId,
                RejectedByUserName = item.RejectedByUserName,
                RejectedAt = item.RejectedAt,
                RejectedReason = item.RejectedReason,
                RevokedByUserId = item.RevokedByUserId,
                RevokedByUserName = item.RevokedByUserName,
                RevokedAt = item.RevokedAt,
                RevokedReason = item.RevokedReason,
                IsPrimary = item.IsPrimary,
                IsExpired = item.ExpiredDate.Date < today,
                IsCurrentlyValid = item.IsActive && item.IsVerified &&
                    item.VerificationStatus == CredentialVerificationStatus.Verified &&
                    item.ExpiredDate.Date >= today,
                IsActive = item.IsActive,
                Description = item.Description,
                CreateDateTime = item.CreateDateTime
            };
        }

        private static (bool IsValid, string? ErrorMessage) ValidateCredentialLicenseRequest(
            string licenseType,
            string licenseNumber,
            DateTime issueDate,
            DateTime expiredDate,
            IFormFile? file)
        {
            var normalizedLicenseType = NormalizeLicenseTypeOrNull(licenseType);

            if (string.IsNullOrWhiteSpace(normalizedLicenseType))
            {
                return (false, "LicenseType wajib diisi.");
            }

            if (!AllowedLicenseTypes.Contains(normalizedLicenseType))
            {
                return (false, "LicenseType tidak valid. Gunakan STR, SIP, SIK, SIPP, SIPA, atau SIPB.");
            }

            if (string.IsNullOrWhiteSpace(licenseNumber))
            {
                return (false, "LicenseNumber wajib diisi.");
            }

            if (issueDate == default)
            {
                return (false, "IssueDate wajib diisi.");
            }

            if (expiredDate == default)
            {
                return (false, "ExpiredDate wajib diisi.");
            }

            if (issueDate.Date > expiredDate.Date)
            {
                return (false, "IssueDate tidak boleh lebih besar dari ExpiredDate.");
            }

            if (file != null)
            {
                if (file.Length <= 0)
                {
                    return (false, "File credential license kosong.");
                }

                if (file.Length > MaxFileSizeBytes)
                {
                    return (false, "Ukuran file credential license maksimal 10 MB.");
                }

                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!AllowedExtensions.Contains(extension))
                {
                    return (false, "Format file tidak didukung. Gunakan PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, atau XLSX.");
                }
            }

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequirementCodeAsync(
            UserType userType,
            string category,
            string requirementCode)
        {
            var exists = await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserType == userType &&
                    x.RequirementCategory == category &&
                    x.RequirementCode == requirementCode &&
                    x.IsActive &&
                    !x.IsDelete);

            return exists
                ? (true, null)
                : (false, $"RequirementCode '{requirementCode}' tidak ditemukan untuk user type ini.");
        }

        private async Task<(string FilePath, string? ContentType)> SaveCredentialLicenseFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var rootPath = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
            var relativeFolder = Path.Combine("uploads", "workforce-credential-licenses", workforceProfileId.ToString());
            var physicalFolder = Path.Combine(rootPath, relativeFolder);

            if (!Directory.Exists(physicalFolder))
            {
                Directory.CreateDirectory(physicalFolder);
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var physicalPath = Path.Combine(physicalFolder, fileName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var filePath = "/" + Path.Combine(relativeFolder, fileName).Replace(Path.DirectorySeparatorChar, '/');

            return (filePath, file.ContentType);
        }

        private async Task DeactivateOtherPrimaryAsync(
            Guid workforceProfileId,
            string licenseType,
            Guid? excludeId,
            DateTime now,
            Guid actorUserId)
        {
            var otherPrimaries = await _dbContext.Set<WfpCredentialLicense>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == licenseType &&
                    x.Id != excludeId &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in otherPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static CredentialVerificationStatus ResolveRuntimeStatus(
            CredentialVerificationStatus status,
            DateTime expiredDate,
            bool isVerified)
        {
            if (status == CredentialVerificationStatus.Revoked ||
                status == CredentialVerificationStatus.Rejected)
            {
                return status;
            }

            if (expiredDate.Date < DateTime.UtcNow.Date)
            {
                return CredentialVerificationStatus.Expired;
            }

            return isVerified ? CredentialVerificationStatus.Verified : status;
        }

        private static string? NormalizeRequirementCodeOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeLicenseTypeOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}