using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Security.Cryptography;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/documents")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Document",
        AreaName = "Corporate",
        ControllerName = "WorkforceDocument",
        Description = "Corporate human resource workforce document",
        SortOrder = 4
    )]
    [Tags("Corporate / Human Resource / Workforce Core / Document")]
    public class WfpDocumentController : ControllerBase
    {
        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedContentTypes =
        {
            "application/pdf", "image/jpeg", "image/png"
        };

        private static readonly string[] DocumentTypeOptions =
        {
            "Identity", "Employment", "Contract", "Education", "Certification",
            "License", "Tax", "Bank", "Insurance", "Medical", "Other"
        };

        private const long MaximumFileSizeBytes = 10 * 1024 * 1024;
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public WfpDocumentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IConfiguration configuration,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _configuration = configuration;
            _environment = environment;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat metadata filter dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpDocumentFilterMetadataResponse
            {
                MaximumFileSizeBytes = MaximumFileSizeBytes,
                MaximumFileSizeLabel = "10 MB",
                DefaultFilter = new WfpDocumentDefaultFilterResponse(),
                DocumentTypeOptions = DocumentTypeOptions
                    .Select(x => new WfpDocumentStringOptionResponse { Value = x, Label = BuildDocumentTypeLabel(x) })
                    .ToList(),
                CustomPeriods = new List<WfpDocumentStringOptionResponse>
                {
                    new() { Value = "custom", Label = "Custom Date Range" },
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WfpDocumentStringOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "documentType", Label = "Jenis dokumen" },
                    new() { Value = "documentName", Label = "Nama dokumen" },
                    new() { Value = "issueDate", Label = "Tanggal terbit" },
                    new() { Value = "expiredDate", Label = "Tanggal kedaluwarsa" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isConfidential", Label = "Kerahasiaan" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                AllowedFileExtensions = AllowedExtensions.ToList(),
                AllowedContentTypes = AllowedContentTypes.ToList()
            };

            return Ok(ApiResponse<WfpDocumentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter dokumen workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat ringkasan dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var today = DateTime.UtcNow.Date;
            var next30Days = today.AddDays(30);
            var query = _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpDocumentSummaryResponse
            {
                TotalDocument = await query.CountAsync(cancellationToken),
                ActiveDocument = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveDocument = await query.CountAsync(x => !x.IsActive, cancellationToken),
                VerifiedDocument = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedDocument = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                ConfidentialDocument = await query.CountAsync(x => x.IsConfidential, cancellationToken),
                DocumentWithFile = await query.CountAsync(x => x.FilePath != null, cancellationToken),
                ExpiredDocument = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < today, cancellationToken),
                ExpiringWithin30Days = await query.CountAsync(x =>
                    x.ExpiredDate.HasValue &&
                    x.ExpiredDate.Value >= today &&
                    x.ExpiredDate.Value <= next30Days,
                    cancellationToken)
            };

            return Ok(ApiResponse<WfpDocumentSummaryResponse>.Ok(
                result,
                "Ringkasan dokumen workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpDocumentResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat data dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetDocuments(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? documentType,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isConfidential,
            [FromQuery] bool? hasFile,
            [FromQuery] bool? isExpired,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);
            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."));
            }

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, dateRange);
            query = ApplyFilter(query, documentType, isVerified, isConfidential, hasFile, isExpired, isActive, search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(
                entities.SelectMany(x => new[] { x.CreateBy, x.VerifiedByUserId ?? Guid.Empty }),
                cancellationToken);

            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<PagedResult<WfpDocumentResponse>>.Ok(
                new PagedResult<WfpDocumentResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data dokumen workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Document", Description = "Melihat detail dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> GetDocumentById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy, entity.VerifiedByUserId ?? Guid.Empty },
                cancellationToken);

            return Ok(ApiResponse<WfpDocumentDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail dokumen workforce berhasil diambil."));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Document", Description = "Membuat dokumen workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceDocument", "Create")]
        public async Task<IActionResult> CreateDocument(
            Guid workforceProfileId,
            [FromForm] CreateWfpDocumentRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = ValidateRequest(request.DocumentType, request.DocumentName, request.IssueDate, request.ExpiredDate);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen workforce tidak valid."));
            }

            var duplicateExists = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    x.DocumentType.ToLower() == request.DocumentType.Trim().ToLower() &&
                    x.DocumentNumber != null &&
                    request.DocumentNumber != null &&
                    x.DocumentNumber.ToLower() == request.DocumentNumber.Trim().ToLower(),
                    cancellationToken);

            if (duplicateExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nomor dokumen dengan jenis yang sama sudah digunakan oleh workforce ini."));
            }

            StoredFileResult? storedFile = null;
            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);
                if (!fileValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        fileValidation.ErrorMessage ?? "File dokumen tidak valid."));
                }

                storedFile = await SaveFileAsync(workforceProfileId, request.File, cancellationToken);
            }

            try
            {
                var now = DateTime.UtcNow;
                var actorUserId = GetCurrentUserId();
                var entity = new WfpDocument
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    RequirementCode = NormalizeNullableText(request.RequirementCode),
                    DocumentType = NormalizeDocumentType(request.DocumentType),
                    DocumentName = request.DocumentName.Trim(),
                    DocumentNumber = NormalizeNullableText(request.DocumentNumber),
                    IssueDate = request.IssueDate?.Date,
                    ExpiredDate = request.ExpiredDate?.Date,
                    IssuingAuthority = NormalizeNullableText(request.IssuingAuthority),
                    FilePath = storedFile?.PublicPath,
                    FileContentType = storedFile?.ContentType,
                    OriginalFileName = storedFile?.OriginalFileName,
                    StoredFileName = storedFile?.StoredFileName,
                    FileSizeBytes = storedFile?.FileSizeBytes,
                    FileChecksum = storedFile?.Checksum,
                    IsConfidential = request.IsConfidential,
                    IsVerified = false,
                    Description = NormalizeNullableText(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpDocument>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceDocument.CreateDocument",
                    "Dokumen workforce berhasil dibuat.",
                    new { entity.Id, entity.WorkforceProfileId, entity.DocumentType, entity.DocumentName, entity.DocumentNumber, entity.IsConfidential });

                return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Dokumen workforce berhasil dibuat.", cancellationToken);
            }
            catch
            {
                if (storedFile != null)
                {
                    DeletePhysicalFileIfExists(storedFile.PublicPath);
                }

                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Document", Description = "Mengubah dokumen workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> UpdateDocument(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWfpDocumentRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Dokumen workforce tidak ditemukan."));
            }

            var validation = ValidateRequest(request.DocumentType, request.DocumentName, request.IssueDate, request.ExpiredDate);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data dokumen workforce tidak valid."));
            }

            if (request.File != null && !request.ReplaceExistingFile && !string.IsNullOrWhiteSpace(entity.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dokumen sudah memiliki file. Aktifkan ReplaceExistingFile untuk mengganti file."));
            }

            var duplicateExists = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id != id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete &&
                    x.DocumentType.ToLower() == request.DocumentType.Trim().ToLower() &&
                    x.DocumentNumber != null &&
                    request.DocumentNumber != null &&
                    x.DocumentNumber.ToLower() == request.DocumentNumber.Trim().ToLower(),
                    cancellationToken);

            if (duplicateExists)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Nomor dokumen dengan jenis yang sama sudah digunakan oleh workforce ini."));
            }

            StoredFileResult? storedFile = null;
            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);
                if (!fileValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        fileValidation.ErrorMessage ?? "File dokumen tidak valid."));
                }

                storedFile = await SaveFileAsync(workforceProfileId, request.File, cancellationToken);
            }

            var oldFilePath = entity.FilePath;

            try
            {
                entity.RequirementCode = NormalizeNullableText(request.RequirementCode);
                entity.DocumentType = NormalizeDocumentType(request.DocumentType);
                entity.DocumentName = request.DocumentName.Trim();
                entity.DocumentNumber = NormalizeNullableText(request.DocumentNumber);
                entity.IssueDate = request.IssueDate?.Date;
                entity.ExpiredDate = request.ExpiredDate?.Date;
                entity.IssuingAuthority = NormalizeNullableText(request.IssuingAuthority);
                entity.IsConfidential = request.IsConfidential;
                entity.Description = NormalizeNullableText(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = DateTime.UtcNow;
                entity.UpdateBy = GetCurrentUserId();

                if (storedFile != null)
                {
                    entity.FilePath = storedFile.PublicPath;
                    entity.FileContentType = storedFile.ContentType;
                    entity.OriginalFileName = storedFile.OriginalFileName;
                    entity.StoredFileName = storedFile.StoredFileName;
                    entity.FileSizeBytes = storedFile.FileSizeBytes;
                    entity.FileChecksum = storedFile.Checksum;
                    entity.IsVerified = false;
                    entity.VerifiedAt = null;
                    entity.VerifiedByUserId = null;
                    entity.VerificationNote = null;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);

                if (storedFile != null && !string.IsNullOrWhiteSpace(oldFilePath))
                {
                    DeletePhysicalFileIfExists(oldFilePath);
                }

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceDocument.UpdateDocument",
                    "Dokumen workforce berhasil diperbarui.",
                    new { entity.Id, entity.WorkforceProfileId, entity.DocumentType, entity.DocumentName, FileReplaced = storedFile != null });

                return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Dokumen workforce berhasil diperbarui.", cancellationToken);
            }
            catch
            {
                if (storedFile != null)
                {
                    DeletePhysicalFileIfExists(storedFile.PublicPath);
                }

                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Document", Description = "Mengubah status dokumen workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> UpdateDocumentStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpDocumentStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen workforce tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Status dokumen workforce berhasil diperbarui.", cancellationToken);
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Document", Description = "Memverifikasi dokumen workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceDocument", "Update")]
        public async Task<IActionResult> VerifyDocument(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpDocumentRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen workforce tidak ditemukan."));
            }

            if (request.IsVerified && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dokumen belum memiliki file sehingga tidak dapat diverifikasi."));
            }

            var actorUserId = GetCurrentUserId();
            entity.IsVerified = request.IsVerified;
            entity.VerifiedAt = request.IsVerified ? DateTime.UtcNow : null;
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.VerificationNote = NormalizeNullableText(request.VerificationNote);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildDetailResultAsync(
                entity.Id,
                workforceProfileId,
                request.IsVerified ? "Dokumen workforce berhasil diverifikasi." : "Verifikasi dokumen workforce berhasil dibatalkan.",
                cancellationToken);
        }

        [HttpGet("{id:guid}/file")]
        [AccessAction("Read", "Read Workforce Document", Description = "Mengunduh file dokumen workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceDocument", "Read")]
        public async Task<IActionResult> DownloadDocumentFile(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen workforce tidak ditemukan."));
            }

            var physicalPath = ResolvePhysicalPath(entity.FilePath);
            if (physicalPath == null || !System.IO.File.Exists(physicalPath))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "File dokumen workforce tidak ditemukan."));
            }

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return File(
                stream,
                entity.FileContentType ?? "application/octet-stream",
                entity.OriginalFileName ?? entity.StoredFileName ?? Path.GetFileName(physicalPath),
                enableRangeProcessing: true);
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<WfpDocumentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Document File", Description = "Menghapus file dokumen workforce", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkforceDocument", "Delete")]
        public async Task<IActionResult> DeleteDocumentFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWfpDocumentFileRequest? request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen workforce tidak ditemukan."));
            }

            var oldPath = entity.FilePath;
            entity.FilePath = null;
            entity.FileContentType = null;
            entity.OriginalFileName = null;
            entity.StoredFileName = null;
            entity.FileSizeBytes = null;
            entity.FileChecksum = null;
            entity.IsVerified = false;
            entity.VerifiedAt = null;
            entity.VerifiedByUserId = null;
            entity.VerificationNote = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (request?.DeletePhysicalFile ?? true)
            {
                DeletePhysicalFileIfExists(oldPath);
            }

            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "File dokumen workforce berhasil dihapus.", cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Document", Description = "Menghapus dokumen workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceDocument", "Delete")]
        public async Task<IActionResult> DeleteDocument(
            Guid workforceProfileId,
            Guid id,
            [FromQuery] bool deletePhysicalFile = true,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<WfpDocument>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Dokumen workforce tidak ditemukan."));
            }

            var filePath = entity.FilePath;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (deletePhysicalFile)
            {
                DeletePhysicalFileIfExists(filePath);
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceDocument.DeleteDocument",
                "Dokumen workforce berhasil dihapus.",
                new { entity.Id, entity.WorkforceProfileId, entity.DocumentType, entity.DocumentName, deletePhysicalFile });

            return Ok(ApiResponse<object>.Ok(null, "Dokumen workforce berhasil dihapus."));
        }

        private IQueryable<WfpDocument> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpDocument>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpDocument> ApplyDateFilter(IQueryable<WfpDocument> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            return query;
        }

        private static IQueryable<WfpDocument> ApplyFilter(
            IQueryable<WfpDocument> query,
            string? documentType,
            bool? isVerified,
            bool? isConfidential,
            bool? hasFile,
            bool? isExpired,
            bool? isActive,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (!string.IsNullOrWhiteSpace(documentType))
                query = query.Where(x => x.DocumentType.ToLower() == documentType.Trim().ToLower());
            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isConfidential.HasValue)
                query = query.Where(x => x.IsConfidential == isConfidential.Value);
            if (hasFile.HasValue)
                query = hasFile.Value ? query.Where(x => x.FilePath != null) : query.Where(x => x.FilePath == null);
            if (isExpired.HasValue)
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value < today)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate.Value >= today);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DocumentType.ToLower().Contains(keyword) ||
                    x.DocumentName.ToLower().Contains(keyword) ||
                    (x.DocumentNumber != null && x.DocumentNumber.ToLower().Contains(keyword)) ||
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    (x.IssuingAuthority != null && x.IssuingAuthority.ToLower().Contains(keyword)) ||
                    (x.OriginalFileName != null && x.OriginalFileName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpDocument> ApplySorting(
            IQueryable<WfpDocument> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = !string.Equals(sortDirection?.Trim(), "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "createDateTime").Trim().Replace("_", string.Empty).ToLowerInvariant() switch
            {
                "documenttype" => desc ? query.OrderByDescending(x => x.DocumentType).ThenBy(x => x.DocumentName) : query.OrderBy(x => x.DocumentType).ThenBy(x => x.DocumentName),
                "documentname" => desc ? query.OrderByDescending(x => x.DocumentName) : query.OrderBy(x => x.DocumentName),
                "issuedate" => desc ? query.OrderByDescending(x => x.IssueDate) : query.OrderBy(x => x.IssueDate),
                "expireddate" => desc ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isverified" => desc ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.IsVerified).ThenByDescending(x => x.CreateDateTime),
                "isconfidential" => desc ? query.OrderByDescending(x => x.IsConfidential).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.IsConfidential).ThenByDescending(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.DocumentName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.DocumentName)
            };
        }

        private WfpDocumentResponse MapResponse(WfpDocument x, IReadOnlyDictionary<Guid, string> actorNames)
        {
            var today = DateTime.UtcNow.Date;
            var next30Days = today.AddDays(30);
            return new WfpDocumentResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                RequirementCode = x.RequirementCode,
                DocumentType = x.DocumentType,
                DocumentName = x.DocumentName,
                DocumentNumber = x.DocumentNumber,
                IssueDate = x.IssueDate,
                ExpiredDate = x.ExpiredDate,
                IssuingAuthority = x.IssuingAuthority,
                FilePath = x.FilePath,
                FileUrl = BuildPublicFileUrl(x.FilePath),
                FileContentType = x.FileContentType,
                OriginalFileName = x.OriginalFileName,
                StoredFileName = x.StoredFileName,
                FileSizeBytes = x.FileSizeBytes,
                FileChecksum = x.FileChecksum,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value < today,
                IsExpiringWithin30Days = x.ExpiredDate.HasValue && x.ExpiredDate.Value >= today && x.ExpiredDate.Value <= next30Days,
                IsConfidential = x.IsConfidential,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByName = GetActorName(actorNames, x.VerifiedByUserId),
                VerificationNote = x.VerificationNote,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy)
            };
        }

        private WfpDocumentDetailResponse MapDetailResponse(WfpDocument x, IReadOnlyDictionary<Guid, string> actorNames)
        {
            var baseResponse = MapResponse(x, actorNames);
            return new WfpDocumentDetailResponse
            {
                Id = baseResponse.Id,
                WorkforceProfileId = baseResponse.WorkforceProfileId,
                WorkforceProfileCode = baseResponse.WorkforceProfileCode,
                WorkforceDisplayName = baseResponse.WorkforceDisplayName,
                RequirementCode = baseResponse.RequirementCode,
                DocumentType = baseResponse.DocumentType,
                DocumentName = baseResponse.DocumentName,
                DocumentNumber = baseResponse.DocumentNumber,
                IssueDate = baseResponse.IssueDate,
                ExpiredDate = baseResponse.ExpiredDate,
                IssuingAuthority = baseResponse.IssuingAuthority,
                FilePath = baseResponse.FilePath,
                FileUrl = baseResponse.FileUrl,
                FileContentType = baseResponse.FileContentType,
                OriginalFileName = baseResponse.OriginalFileName,
                StoredFileName = baseResponse.StoredFileName,
                FileSizeBytes = baseResponse.FileSizeBytes,
                FileChecksum = baseResponse.FileChecksum,
                HasFile = baseResponse.HasFile,
                IsExpired = baseResponse.IsExpired,
                IsExpiringWithin30Days = baseResponse.IsExpiringWithin30Days,
                IsConfidential = baseResponse.IsConfidential,
                IsVerified = baseResponse.IsVerified,
                VerifiedAt = baseResponse.VerifiedAt,
                VerifiedByUserId = baseResponse.VerifiedByUserId,
                VerifiedByName = baseResponse.VerifiedByName,
                VerificationNote = baseResponse.VerificationNote,
                Description = baseResponse.Description,
                IsActive = baseResponse.IsActive,
                CreateDateTime = baseResponse.CreateDateTime,
                CreateBy = baseResponse.CreateBy,
                CreateByName = baseResponse.CreateByName,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<IActionResult> BuildDetailResultAsync(Guid id, Guid workforceProfileId, string message, CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == id, cancellationToken);
            var actorNames = await GetActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy, entity.VerifiedByUserId ?? Guid.Empty },
                cancellationToken);
            return Ok(ApiResponse<WfpDocumentDetailResponse>.Ok(MapDetailResponse(entity, actorNames), message));
        }

        private static (bool IsValid, string? ErrorMessage) ValidateRequest(
            string? documentType,
            string? documentName,
            DateTime? issueDate,
            DateTime? expiredDate)
        {
            if (string.IsNullOrWhiteSpace(documentType))
                return (false, "Jenis dokumen wajib diisi.");
            if (string.IsNullOrWhiteSpace(documentName))
                return (false, "Nama dokumen wajib diisi.");
            if (issueDate.HasValue && expiredDate.HasValue && expiredDate.Value.Date < issueDate.Value.Date)
                return (false, "Tanggal kedaluwarsa tidak boleh lebih kecil dari tanggal terbit.");
            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
                return (false, "File dokumen kosong.");
            if (file.Length > MaximumFileSizeBytes)
                return (false, "Ukuran file dokumen maksimal 10 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
                return (false, "Format file harus PDF, JPG, JPEG, atau PNG.");
            if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
                return (false, "Content type file tidak diizinkan.");
            return (true, null);
        }

        private async Task<StoredFileResult> SaveFileAsync(Guid workforceProfileId, IFormFile file, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativeDirectory = Path.Combine("human-resource", "workforce", workforceProfileId.ToString(), "documents");
            var physicalDirectory = Path.Combine(GetUploadRootPath(), relativeDirectory);
            Directory.CreateDirectory(physicalDirectory);
            var physicalPath = Path.Combine(physicalDirectory, storedFileName);

            await using (var output = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(output, cancellationToken);
            }

            string checksum;
            await using (var checksumStream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using var sha256 = SHA256.Create();
                var hash = await sha256.ComputeHashAsync(checksumStream, cancellationToken);
                checksum = Convert.ToHexString(hash).ToLowerInvariant();
            }

            var requestPath = GetPublicRequestPath();
            var publicPath = $"{requestPath}/{relativeDirectory.Replace(Path.DirectorySeparatorChar, '/')}/{storedFileName}";

            return new StoredFileResult(
                publicPath,
                file.ContentType,
                Path.GetFileName(file.FileName),
                storedFileName,
                file.Length,
                checksum);
        }

        private string GetUploadRootPath()
        {
            var configured = _configuration["FileStorage:UploadRootPath"];
            var path = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(_environment.ContentRootPath, "uploads")
                : configured.Trim();
            if (!Path.IsPathRooted(path))
                path = Path.Combine(_environment.ContentRootPath, path);
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetPublicRequestPath()
        {
            var value = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";
            value = value.Replace("\\", "/").Trim();
            if (!value.StartsWith('/')) value = "/" + value;
            return value.TrimEnd('/');
        }

        private string? ResolvePhysicalPath(string? publicPath)
        {
            if (string.IsNullOrWhiteSpace(publicPath)) return null;
            var normalized = publicPath.Replace("\\", "/");
            var requestPath = GetPublicRequestPath();
            if (normalized.StartsWith(requestPath, StringComparison.OrdinalIgnoreCase))
                normalized = normalized[requestPath.Length..];
            normalized = normalized.TrimStart('/');
            var root = GetUploadRootPath();
            var candidate = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
            return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? candidate : null;
        }

        private void DeletePhysicalFileIfExists(string? publicPath)
        {
            var path = ResolvePhysicalPath(publicPath);
            if (path != null && System.IO.File.Exists(path))
                System.IO.File.Delete(path);
        }

        private string? BuildPublicFileUrl(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                return filePath;
            var baseUrl = _configuration["FileStorage:PublicBaseUrl"];
            return !string.IsNullOrWhiteSpace(baseUrl)
                ? $"{baseUrl.TrimEnd('/')}/{filePath.TrimStart('/')}"
                : $"{Request.Scheme}://{Request.Host}/{filePath.TrimStart('/')}";
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty && await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task<Dictionary<Guid, string>> GetActorNameMapAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            var userIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            if (userIds.Count == 0) return new Dictionary<Guid, string>();
            return await _dbContext.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(
                    x => x.Id,
                    x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode,
                    cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> names, Guid? id)
        {
            return id.HasValue && id.Value != Guid.Empty && names.TryGetValue(id.Value, out var name) ? name : null;
        }

        private Guid GetCurrentUserId()
        {
            var text = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        private static string NormalizeDocumentType(string value)
        {
            var match = DocumentTypeOptions.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            return match ?? value.Trim();
        }

        private static string BuildDocumentTypeLabel(string value) => value switch
        {
            "Identity" => "Identitas",
            "Employment" => "Kepegawaian",
            "Contract" => "Kontrak",
            "Education" => "Pendidikan",
            "Certification" => "Sertifikasi",
            "License" => "Lisensi",
            "Tax" => "Pajak",
            "Bank" => "Bank",
            "Insurance" => "Asuransi",
            "Medical" => "Medis",
            _ => "Lainnya"
        };

        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;
            DateTime? start = startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null;
            DateTime? endExclusive = endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null;

            if (!string.IsNullOrWhiteSpace(customPeriod) && !customPeriod.Equals("custom", StringComparison.OrdinalIgnoreCase))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today": start = today; endExclusive = today.AddDays(1); break;
                    case "last7days": start = today.AddDays(-6); endExclusive = today.AddDays(1); break;
                    case "thismonth": start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc); endExclusive = start.Value.AddMonths(1); break;
                    case "lastmonth": var current = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc); start = current.AddMonths(-1); endExclusive = current; break;
                    default: return DateRangeResult.Invalid("Custom period tidak valid.");
                }
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("Tanggal mulai tidak boleh lebih besar dari tanggal akhir.");
            return DateRangeResult.Valid(start, endExclusive);
        }

        private sealed record StoredFileResult(
            string PublicPath,
            string ContentType,
            string OriginalFileName,
            string StoredFileName,
            long FileSizeBytes,
            string Checksum);

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }
            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive) => new() { IsValid = true, Start = start, EndExclusive = endExclusive };
            public static DateRangeResult Invalid(string message) => new() { IsValid = false, ErrorMessage = message };
        }
    }
}
