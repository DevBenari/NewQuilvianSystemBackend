using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/certifications")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Certification",
        AreaName = "Corporate",
        ControllerName = "WorkforceCertification",
        Description = "Workforce certification management",
        SortOrder = 26
    )]
    [Tags("Corporate / Human Resource / Workforce / Certification")]
    public class WorkforceCertificationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Certification";
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

        public WorkforceCertificationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Certification",
            Description = "Melihat certification workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> GetCertifications(Guid workforceProfileId)
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

            var items = await _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.IsLifetime)
                .ThenBy(x => x.ExpiredDate)
                .ThenBy(x => x.CertificationName)
                .Select(x => new WorkforceCertificationResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    RequirementCode = x.RequirementCode,
                    CertificationType = x.CertificationType,
                    CertificationName = x.CertificationName,
                    Issuer = x.Issuer,
                    CertificateNumber = x.CertificateNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    IsLifetime = x.IsLifetime,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsVerified = x.IsVerified,
                    IsExpired = !x.IsLifetime &&
                                x.ExpiredDate.HasValue &&
                                x.ExpiredDate.Value.Date < today,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceCertificationListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                ExpiredData = items.Count(x => x.IsExpired),
                CertificationWithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCertificationListResponse>.Ok(
                result,
                "Data certification workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Certification",
            Description = "Menambah certification workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceCertification", "Create")]
        public async Task<IActionResult> CreateCertification(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceCertificationRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateCertificationRequest(
                request.CertificationType,
                request.CertificationName,
                request.IssueDate,
                request.ExpiredDate,
                request.IsLifetime,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data certification tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "Certification",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            var normalizedCertificateNumber = NormalizeNullableText(request.CertificateNumber);

            if (!string.IsNullOrWhiteSpace(normalizedCertificateNumber))
            {
                var duplicate = await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.CertificateNumber == normalizedCertificateNumber &&
                        !x.IsDelete);

                if (duplicate)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Nomor sertifikat certification sudah terdaftar pada workforce profile ini."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveCertificationFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpCertification
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = normalizedRequirementCode,
                CertificationType = request.CertificationType.Trim(),
                CertificationName = request.CertificationName.Trim(),
                Issuer = NormalizeNullableText(request.Issuer),
                CertificateNumber = normalizedCertificateNumber,
                IssueDate = request.IssueDate.Date,
                ExpiredDate = request.IsLifetime ? null : request.ExpiredDate?.Date,
                IsLifetime = request.IsLifetime,
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

            _dbContext.Set<WfpCertification>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCertificationResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCertification.CreateCertification",
                "Certification workforce berhasil dibuat.",
                new
                {
                    workforceProfileId,
                    entity.Id,
                    entity.RequirementCode,
                    entity.CertificationType,
                    entity.CertificationName
                }
            );

            return Ok(ApiResponse<WorkforceCertificationResponse>.Ok(
                response!,
                "Certification workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCertificationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Certification",
            Description = "Mengubah certification workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> UpdateCertification(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceCertificationRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            var validation = ValidateCertificationRequest(
                request.CertificationType,
                request.CertificationName,
                request.IssueDate,
                request.ExpiredDate,
                request.IsLifetime,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data certification tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "Certification",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            var normalizedCertificateNumber = NormalizeNullableText(request.CertificateNumber);

            if (!string.IsNullOrWhiteSpace(normalizedCertificateNumber))
            {
                var duplicate = await _dbContext.Set<WfpCertification>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.Id != id &&
                        x.CertificateNumber == normalizedCertificateNumber &&
                        !x.IsDelete);

                if (duplicate)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Nomor sertifikat certification sudah terdaftar pada workforce profile ini."
                    ));
                }
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveCertificationFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveCertificationFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.RequirementCode = normalizedRequirementCode;
            entity.CertificationType = request.CertificationType.Trim();
            entity.CertificationName = request.CertificationName.Trim();
            entity.Issuer = NormalizeNullableText(request.Issuer);
            entity.CertificateNumber = normalizedCertificateNumber;
            entity.IssueDate = request.IssueDate.Date;
            entity.ExpiredDate = request.IsLifetime ? null : request.ExpiredDate?.Date;
            entity.IsLifetime = request.IsLifetime;
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildCertificationResponseAsync(entity.Id);

            return Ok(ApiResponse<WorkforceCertificationResponse>.Ok(
                response!,
                "Certification workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Certification",
            Description = "Mengubah status certification workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> UpdateCertificationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceCertificationStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status certification workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Certification",
            Description = "Verifikasi certification workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceCertification", "Update")]
        public async Task<IActionResult> VerifyCertification(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceCertificationRequest request)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Certification workforce berhasil diverifikasi."
                    : "Verifikasi certification workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Certification",
            Description = "Download file certification workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceCertification", "Read")]
        public async Task<IActionResult> DownloadCertification(
            Guid workforceProfileId,
            Guid id)
        {
            var certification = await _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (certification == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(certification.FilePath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File certification workforce belum tersedia."
                ));
            }

            var physicalPath = ResolvePhysicalPath(certification.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File fisik certification tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = certification.FileContentType ?? "application/octet-stream";
            }

            var safeCertificationName = certification.CertificationName
                .Replace("/", "-")
                .Replace("\\", "-");

            var downloadName = $"{certification.CertificationType}_{safeCertificationName}{Path.GetExtension(physicalPath)}";

            return PhysicalFile(physicalPath, contentType, downloadName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Certification",
            Description = "Menghapus certification workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceCertification", "Delete")]
        public async Task<IActionResult> DeleteCertification(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpCertification>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Certification workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Certification workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateCertificationRequest(
            string? certificationType,
            string? certificationName,
            DateTime issueDate,
            DateTime? expiredDate,
            bool isLifetime,
            IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(certificationType))
            {
                return (false, "CertificationType wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(certificationName))
            {
                return (false, "CertificationName wajib diisi.");
            }

            if (issueDate == default)
            {
                return (false, "IssueDate wajib diisi.");
            }

            if (!isLifetime &&
                expiredDate.HasValue &&
                expiredDate.Value.Date < issueDate.Date)
            {
                return (false, "ExpiredDate tidak boleh lebih kecil dari IssueDate.");
            }

            if (file == null)
            {
                return (true, null);
            }

            if (file.Length <= 0)
            {
                return (false, "File certification kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file certification maksimal 10 MB.");
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

        private async Task<(string FilePath, string? ContentType)> SaveCertificationFileAsync(
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

            var relativeFolder = Path.Combine("uploads", "workforce-certifications", workforceProfileId.ToString());
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

        private async Task<WorkforceCertificationResponse?> BuildCertificationResponseAsync(Guid id)
        {
            var today = DateTime.UtcNow.Date;

            return await _dbContext.Set<WfpCertification>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceCertificationResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    RequirementCode = x.RequirementCode,
                    CertificationType = x.CertificationType,
                    CertificationName = x.CertificationName,
                    Issuer = x.Issuer,
                    CertificateNumber = x.CertificateNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    IsLifetime = x.IsLifetime,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsVerified = x.IsVerified,
                    IsExpired = !x.IsLifetime &&
                                x.ExpiredDate.HasValue &&
                                x.ExpiredDate.Value.Date < today,
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