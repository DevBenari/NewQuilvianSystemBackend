using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enum;
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
        public async Task<IActionResult> GetCredentialLicenses(Guid workforceProfileId)
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

            var items = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.ExpiredDate)
                .ThenBy(x => x.LicenseType)
                .Select(x => new WorkforceCredentialLicenseResponse
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
                    IsVerified = x.IsVerified,
                    IsExpired = x.ExpiredDate.Date < today,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceCredentialLicenseListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                ExpiredData = items.Count(x => x.IsExpired),
                CredentialLicenseWithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCredentialLicenseListResponse>.Ok(
                result,
                "Data credential license workforce berhasil diambil."
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

            var normalizedLicenseNumber = NormalizeNullableText(request.LicenseNumber) ?? string.Empty;

            var duplicate = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.LicenseType == request.LicenseType.Trim() &&
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

            var entity = new WfpCredentialLicense
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = normalizedRequirementCode,
                LicenseType = request.LicenseType.Trim(),
                LicenseNumber = normalizedLicenseNumber,
                Issuer = NormalizeNullableText(request.Issuer),
                IssueDate = request.IssueDate.Date,
                ExpiredDate = request.ExpiredDate.Date,
                PracticeLocation = NormalizeNullableText(request.PracticeLocation),
                FilePath = filePath,
                FileContentType = fileContentType,
                IsVerified = request.IsVerified,
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpCredentialLicense>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCredentialLicense.CreateCredentialLicense",
                "Credential license workforce berhasil dibuat.",
                new
                {
                    workforceProfileId,
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
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

            var normalizedLicenseNumber = NormalizeNullableText(request.LicenseNumber) ?? string.Empty;

            var duplicate = await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != id &&
                    x.LicenseType == request.LicenseType.Trim() &&
                    x.LicenseNumber == normalizedLicenseNumber &&
                    !x.IsDelete);

            if (duplicate)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "LicenseType dan LicenseNumber sudah terdaftar pada workforce profile ini."
                ));
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveCredentialLicenseFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveCredentialLicenseFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.RequirementCode = normalizedRequirementCode;
            entity.LicenseType = request.LicenseType.Trim();
            entity.LicenseNumber = normalizedLicenseNumber;
            entity.Issuer = NormalizeNullableText(request.Issuer);
            entity.IssueDate = request.IssueDate.Date;
            entity.ExpiredDate = request.ExpiredDate.Date;
            entity.PracticeLocation = NormalizeNullableText(request.PracticeLocation);
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildCredentialLicenseResponseAsync(entity.Id);

            return Ok(ApiResponse<WorkforceCredentialLicenseResponse>.Ok(
                response!,
                "Credential license workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Mengubah status credential license workforce",
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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status credential license workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Credential License",
            Description = "Verifikasi credential license workforce",
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

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Credential license workforce berhasil diverifikasi."
                    : "Verifikasi credential license workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Credential License",
            Description = "Download file credential license workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCredentialLicense", "Read")]
        public async Task<IActionResult> DownloadCredentialLicense(
            Guid workforceProfileId,
            Guid id)
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
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File credential license workforce belum tersedia."
                ));
            }

            var physicalPath = ResolvePhysicalPath(license.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File fisik credential license tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = license.FileContentType ?? "application/octet-stream";
            }

            var safeLicenseNumber = license.LicenseNumber
                .Replace("/", "-")
                .Replace("\\", "-");

            var downloadName = $"{license.LicenseType}_{safeLicenseNumber}{Path.GetExtension(physicalPath)}";

            return PhysicalFile(physicalPath, contentType, downloadName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Credential License",
            Description = "Menghapus credential license workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceCredentialLicense", "Delete")]
        public async Task<IActionResult> DeleteCredentialLicense(
            Guid workforceProfileId,
            Guid id)
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

            entity.IsActive = false;
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

        private static (bool IsValid, string? ErrorMessage) ValidateCredentialLicenseRequest(
            string? licenseType,
            string? licenseNumber,
            DateTime issueDate,
            DateTime expiredDate,
            IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(licenseType))
            {
                return (false, "LicenseType wajib diisi.");
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

            if (expiredDate.Date < issueDate.Date)
            {
                return (false, "ExpiredDate tidak boleh lebih kecil dari IssueDate.");
            }

            if (file == null)
            {
                return (true, null);
            }

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
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, PNG, DOC, DOCX, XLS, atau XLSX.");
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

            if (!exists)
            {
                return (false, $"RequirementCode {requirementCode} tidak terdaftar untuk kategori {category}.");
            }

            return (true, null);
        }

        private async Task<(string FilePath, string? ContentType)> SaveCredentialLicenseFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var relativeFolder = Path.Combine("uploads", "workforce-credential-licenses", workforceProfileId.ToString());
            var absoluteFolder = Path.Combine(webRootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            var absolutePath = Path.Combine(absoluteFolder, fileName);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var relativePath = "/" + Path.Combine(relativeFolder, fileName).Replace("\\", "/");

            return (relativePath, file.ContentType);
        }

        private void DeletePhysicalFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return;
            }

            var physicalPath = ResolvePhysicalPath(filePath);

            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }
        }

        private string ResolvePhysicalPath(string filePath)
        {
            var normalizedPath = filePath.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString());

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            return Path.Combine(webRootPath, normalizedPath);
        }

        private async Task<WorkforceCredentialLicenseResponse?> BuildCredentialLicenseResponseAsync(Guid id)
        {
            var today = DateTime.UtcNow.Date;

            return await _dbContext.Set<WfpCredentialLicense>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceCredentialLicenseResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
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
                    IsVerified = x.IsVerified,
                    IsExpired = x.ExpiredDate.Date < today,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();
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

        private static string? NormalizeRequirementCodeOrNull(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant().Replace(" ", "_");
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}