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
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/documents")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Document",
        AreaName = "Corporate",
        ControllerName = "WorkforceDocument",
        Description = "Workforce document management",
        SortOrder = 23
    )]
    [Tags("Corporate / Human Resource / Workforce / Document")]
    public class WorkforceDocumentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.Document";
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

        public WorkforceDocumentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentListResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Document",
            Description = "Melihat dokumen workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetDocuments(Guid workforceProfileId)
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

            var items = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .OrderByDescending(x => x.IsActive)
                .ThenByDescending(x => x.IsVerified)
                .ThenBy(x => x.DocumentType)
                .ThenBy(x => x.DocumentName)
                .Select(x => new WorkforceDocumentResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = profile.ProfileCode,
                    DisplayName = profile.DisplayName,
                    RequirementCode = x.RequirementCode,
                    DocumentType = x.DocumentType,
                    DocumentName = x.DocumentName,
                    DocumentNumber = x.DocumentNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsVerified = x.IsVerified,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
                    IsActive = x.IsActive,
                    Description = x.Description,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new WorkforceDocumentListResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalData = items.Count,
                ActiveData = items.Count(x => x.IsActive),
                VerifiedData = items.Count(x => x.IsVerified),
                ExpiredData = items.Count(x => x.IsExpired),
                DocumentWithFileData = items.Count(x => x.HasFile),
                Items = items
            };

            return Ok(ApiResponse<WorkforceDocumentListResponse>.Ok(
                result,
                "Data dokumen workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Document",
            Description = "Menambah dokumen workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceDocument", "Create")]
        public async Task<IActionResult> CreateDocument(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceDocumentRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = ValidateDocumentRequest(
                request.DocumentType,
                request.DocumentName,
                request.IssueDate,
                request.ExpiredDate,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);
            var normalizedDocumentNumber = NormalizeNullableText(request.DocumentNumber);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "Document",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedDocumentNumber))
            {
                var duplicate = await _dbContext.Set<WfpDocument>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.DocumentType == request.DocumentType.Trim() &&
                        x.DocumentNumber == normalizedDocumentNumber &&
                        !x.IsDelete);

                if (duplicate)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Document number dengan document type tersebut sudah terdaftar pada workforce profile ini."
                    ));
                }
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveDocumentFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpDocument
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = normalizedRequirementCode,
                DocumentType = request.DocumentType.Trim(),
                DocumentName = request.DocumentName.Trim(),
                DocumentNumber = normalizedDocumentNumber,
                IssueDate = request.IssueDate?.Date,
                ExpiredDate = request.ExpiredDate?.Date,
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

            _dbContext.Set<WfpDocument>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildDocumentResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDocument.CreateDocument",
                "Dokumen workforce berhasil dibuat.",
                new
                {
                    workforceProfileId,
                    entity.Id,
                    entity.RequirementCode,
                    entity.DocumentType,
                    entity.DocumentName
                }
            );

            return Ok(ApiResponse<WorkforceDocumentResponse>.Ok(
                response!,
                "Dokumen workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceDocumentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Document",
            Description = "Mengubah dokumen workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> UpdateDocument(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceDocumentRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            var validation = ValidateDocumentRequest(
                request.DocumentType,
                request.DocumentName,
                request.IssueDate,
                request.ExpiredDate,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen tidak valid."
                ));
            }

            var normalizedRequirementCode = NormalizeRequirementCodeOrNull(request.RequirementCode);
            var normalizedDocumentNumber = NormalizeNullableText(request.DocumentNumber);

            if (!string.IsNullOrWhiteSpace(normalizedRequirementCode))
            {
                var requirementValid = await ValidateRequirementCodeAsync(
                    profile.UserType,
                    "Document",
                    normalizedRequirementCode);

                if (!requirementValid.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        requirementValid.ErrorMessage ?? "RequirementCode tidak valid."
                    ));
                }
            }

            if (!string.IsNullOrWhiteSpace(normalizedDocumentNumber))
            {
                var duplicate = await _dbContext.Set<WfpDocument>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.Id != id &&
                        x.DocumentType == request.DocumentType.Trim() &&
                        x.DocumentNumber == normalizedDocumentNumber &&
                        !x.IsDelete);

                if (duplicate)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        "Document number dengan document type tersebut sudah terdaftar pada workforce profile ini."
                    ));
                }
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveDocumentFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveDocumentFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.RequirementCode = normalizedRequirementCode;
            entity.DocumentType = request.DocumentType.Trim();
            entity.DocumentName = request.DocumentName.Trim();
            entity.DocumentNumber = normalizedDocumentNumber;
            entity.IssueDate = request.IssueDate?.Date;
            entity.ExpiredDate = request.ExpiredDate?.Date;
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildDocumentResponseAsync(entity.Id);

            return Ok(ApiResponse<WorkforceDocumentResponse>.Ok(
                response!,
                "Dokumen workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Document",
            Description = "Mengubah status dokumen workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> UpdateDocumentStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceDocumentStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status dokumen workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Document",
            Description = "Verifikasi dokumen workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> VerifyDocument(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceDocumentRequest request)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Dokumen workforce berhasil diverifikasi."
                    : "Verifikasi dokumen workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Document",
            Description = "Download dokumen workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> DownloadDocument(
            Guid workforceProfileId,
            Guid id)
        {
            var document = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (document == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(document.FilePath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File dokumen workforce belum tersedia."
                ));
            }

            var physicalPath = ResolvePhysicalPath(document.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File fisik dokumen tidak ditemukan di server."
                ));
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = document.FileContentType ?? "application/octet-stream";
            }

            var downloadName = $"{document.DocumentName}{Path.GetExtension(physicalPath)}";

            return PhysicalFile(physicalPath, contentType, downloadName);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Document",
            Description = "Menghapus dokumen workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceDocument", "Delete")]
        public async Task<IActionResult> DeleteDocument(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Dokumen workforce berhasil dihapus."
            ));
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateDocumentRequest(
            string? documentType,
            string? documentName,
            DateTime? issueDate,
            DateTime? expiredDate,
            IFormFile? file)
        {
            if (string.IsNullOrWhiteSpace(documentType))
            {
                return (false, "DocumentType wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(documentName))
            {
                return (false, "DocumentName wajib diisi.");
            }

            if (issueDate.HasValue &&
                expiredDate.HasValue &&
                expiredDate.Value.Date < issueDate.Value.Date)
            {
                return (false, "ExpiredDate tidak boleh lebih kecil dari IssueDate.");
            }

            if (file == null)
            {
                return (true, null);
            }

            if (file.Length <= 0)
            {
                return (false, "File dokumen kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file dokumen maksimal 10 MB.");
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

        private async Task<(string FilePath, string? ContentType)> SaveDocumentFileAsync(
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

            var relativeFolder = Path.Combine("uploads", "workforce-documents", workforceProfileId.ToString());
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

        private async Task<WorkforceDocumentResponse?> BuildDocumentResponseAsync(Guid id)
        {
            var today = DateTime.UtcNow.Date;

            return await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new WorkforceDocumentResponse
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    ProfileCode = x.WorkforceProfile != null ? x.WorkforceProfile.ProfileCode : string.Empty,
                    DisplayName = x.WorkforceProfile != null ? x.WorkforceProfile.DisplayName : string.Empty,
                    RequirementCode = x.RequirementCode,
                    DocumentType = x.DocumentType,
                    DocumentName = x.DocumentName,
                    DocumentNumber = x.DocumentNumber,
                    IssueDate = x.IssueDate,
                    ExpiredDate = x.ExpiredDate,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                    IsVerified = x.IsVerified,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
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