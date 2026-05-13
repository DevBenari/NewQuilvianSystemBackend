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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/educations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Education",
        AreaName = "Corporate",
        ControllerName = "WorkforceEducation",
        Description = "Workforce education management",
        SortOrder = 24
    )]
    [Tags("Corporate / Human Resource / Workforce / Education")]
    public class WorkforceEducationController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Education";
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

        public WorkforceEducationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Education",
            Description = "Melihat pendidikan workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetEducations(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var items = await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsVerified)
                .ThenByDescending(x => x.GraduationYear)
                .ThenBy(x => x.EducationLevel)
                .Select(x => new WorkforceEducationResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    RequirementCode = x.RequirementCode,
                    EducationLevel = x.EducationLevel,
                    InstitutionName = x.InstitutionName,
                    Major = x.Major,
                    GraduationYear = x.GraduationYear,
                    CertificateNumber = x.CertificateNumber,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceEducationListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                EducationWithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceEducationListResponse>.Ok(
                result,
                "Data pendidikan workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Education",
            Description = "Menambah pendidikan workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceEducation", "Create")]
        public async Task<IActionResult> CreateEducation(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceEducationRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateEducationRequest(
                request.EducationLevel,
                request.InstitutionName,
                request.GraduationYear,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pendidikan tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "Education",
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
                var duplicate = await _dbContext.Set<WfpEducation>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.CertificateNumber == normalizedCertificateNumber &&
                        !x.IsDelete);

                if (duplicate)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Nomor ijazah/sertifikat pendidikan sudah terdaftar pada workforce profile ini."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveEducationFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpEducation
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = normalizedRequirementCode,
                EducationLevel = request.EducationLevel.Trim(),
                InstitutionName = request.InstitutionName.Trim(),
                Major = NormalizeNullableText(request.Major),
                GraduationYear = request.GraduationYear,
                CertificateNumber = normalizedCertificateNumber,
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

            _dbContext.Set<WfpEducation>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildEducationResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEducation.CreateEducation",
                "Pendidikan workforce berhasil dibuat.",
                new
                {
                    workforceProfileId,
                    entity.Id,
                    entity.RequirementCode,
                    entity.EducationLevel,
                    entity.InstitutionName
                }
            );

            return Ok(ApiResponse<WorkforceEducationResponse>.Ok(
                response!,
                "Pendidikan workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Education",
            Description = "Mengubah pendidikan workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> UpdateEducation(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceEducationRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            var validation = ValidateEducationRequest(
                request.EducationLevel,
                request.InstitutionName,
                request.GraduationYear,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pendidikan tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "Education",
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
                var duplicate = await _dbContext.Set<WfpEducation>()
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
                        "Nomor ijazah/sertifikat pendidikan sudah terdaftar pada workforce profile ini."
                    ));
                }
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveEducationFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveEducationFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.RequirementCode = normalizedRequirementCode;
            entity.EducationLevel = request.EducationLevel.Trim();
            entity.InstitutionName = request.InstitutionName.Trim();
            entity.Major = NormalizeNullableText(request.Major);
            entity.GraduationYear = request.GraduationYear;
            entity.CertificateNumber = normalizedCertificateNumber;
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildEducationResponseAsync(entity.Id);

            return Ok(ApiResponse<WorkforceEducationResponse>.Ok(
                response!,
                "Pendidikan workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Education",
            Description = "Mengubah status pendidikan workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> UpdateEducationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceEducationStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status pendidikan workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Education",
            Description = "Verifikasi pendidikan workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> VerifyEducation(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceEducationRequest request)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Pendidikan workforce berhasil diverifikasi."
                    : "Verifikasi pendidikan workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Education",
            Description = "Download file pendidikan workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> DownloadEducation(
            Guid workforceProfileId,
            Guid id)
        {
            var education = await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (education == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(education.FilePath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File pendidikan workforce belum tersedia."
                ));
            }

            var physicalPath = ResolvePhysicalPath(education.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File fisik pendidikan tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = education.FileContentType ?? "application/octet-stream";
            }

            var downloadName = $"{education.EducationLevel}_{education.InstitutionName}{Path.GetExtension(physicalPath)}";

            return PhysicalFile(physicalPath, contentType, downloadName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Education",
            Description = "Menghapus pendidikan workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceEducation", "Delete")]
        public async Task<IActionResult> DeleteEducation(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Pendidikan workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateEducationRequest(
            string? educationLevel,
            string? institutionName,
            int? graduationYear,
            IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(educationLevel))
            {
                return (false, "EducationLevel wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(institutionName))
            {
                return (false, "InstitutionName wajib diisi.");
            }

            if (graduationYear.HasValue)
            {
                var currentYear = DateTime.UtcNow.Year + 1;

                if (graduationYear.Value < 1900 || graduationYear.Value > currentYear)
                {
                    return (false, $"GraduationYear harus berada di antara 1900 dan {currentYear}.");
                }
            }

            if (file == null)
            {
                return (true, null);
            }

            if (file.Length <= 0)
            {
                return (false, "File pendidikan kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file pendidikan maksimal 10 MB.");
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

        private async Task<(string FilePath, string? ContentType)> SaveEducationFileAsync(
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

            var relativeFolder = Path.Combine("uploads", "workforce-educations", workforceProfileId.ToString());
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

        private async Task<WorkforceEducationResponse?> BuildEducationResponseAsync(Guid id)
        {
            return await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceEducationResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    RequirementCode = x.RequirementCode,
                    EducationLevel = x.EducationLevel,
                    InstitutionName = x.InstitutionName,
                    Major = x.Major,
                    GraduationYear = x.GraduationYear,
                    CertificateNumber = x.CertificateNumber,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsVerified = x.IsVerified,
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