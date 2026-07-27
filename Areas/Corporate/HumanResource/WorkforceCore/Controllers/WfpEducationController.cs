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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/educations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE_CORE",
        moduleName: "Human Resource Workforce Core",
        displayName: "Workforce Education",
        AreaName = "Corporate",
        ControllerName = "WorkforceEducation",
        Description = "Corporate human resource workforce education",
        SortOrder = 5
    )]
    [Tags("Corporate / Human Resource / Workforce Core / Education")]
    public class WfpEducationController : ControllerBase
    {
        private static readonly string[] EducationLevels =
        {
            "Elementary", "JuniorHighSchool", "SeniorHighSchool", "Diploma1", "Diploma2",
            "Diploma3", "Diploma4", "Bachelor", "Professional", "Master", "Specialist",
            "Doctorate", "PostDoctorate", "Other"
        };

        private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedContentTypes = { "application/pdf", "image/jpeg", "image/png" };
        private const long MaximumFileSizeBytes = 10 * 1024 * 1024;
        private const string LogCategory = "Corporate.HumanResource.WorkforceCore";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;

        public WfpEducationController(
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
        [ProducesResponseType(typeof(ApiResponse<WfpEducationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat metadata filter pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken)
        {
            var countries = await _dbContext.MstCountries
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete)
                .OrderBy(x => x.CountryName)
                .Select(x => new WfpEducationCountryOptionResponse
                {
                    Id = x.Id,
                    CountryCode = x.CountryCode,
                    CountryName = x.CountryName,
                    Label = x.CountryName
                })
                .ToListAsync(cancellationToken);

            var result = new WfpEducationFilterMetadataResponse
            {
                MaximumFileSizeBytes = MaximumFileSizeBytes,
                MaximumFileSizeLabel = "10 MB",
                DefaultFilter = new WfpEducationDefaultFilterResponse(),
                EducationLevelOptions = EducationLevels
                    .Select(x => new WfpEducationStringOptionResponse { Value = x, Label = BuildEducationLevelLabel(x) })
                    .ToList(),
                CountryOptions = countries,
                SortOptions = new List<WfpEducationStringOptionResponse>
                {
                    new() { Value = "isHighestEducation", Label = "Pendidikan tertinggi" },
                    new() { Value = "educationLevel", Label = "Jenjang pendidikan" },
                    new() { Value = "institutionName", Label = "Nama institusi" },
                    new() { Value = "graduationYear", Label = "Tahun lulus" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                AllowedFileExtensions = AllowedExtensions.ToList(),
                AllowedContentTypes = AllowedContentTypes.ToList()
            };

            return Ok(ApiResponse<WfpEducationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter pendidikan workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationSummaryResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat ringkasan pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpEducationSummaryResponse
            {
                TotalEducation = await query.CountAsync(cancellationToken),
                ActiveEducation = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveEducation = await query.CountAsync(x => !x.IsActive, cancellationToken),
                HighestEducation = await query.CountAsync(x => x.IsHighestEducation, cancellationToken),
                VerifiedEducation = await query.CountAsync(x => x.IsVerified, cancellationToken),
                UnverifiedEducation = await query.CountAsync(x => !x.IsVerified, cancellationToken),
                EducationWithCertificate = await query.CountAsync(x => x.CertificateNumber != null, cancellationToken),
                EducationWithFile = await query.CountAsync(x => x.FilePath != null, cancellationToken)
            };

            return Ok(ApiResponse<WfpEducationSummaryResponse>.Ok(result, "Ringkasan pendidikan workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpEducationResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat data pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetEducations(
            Guid workforceProfileId,
            [FromQuery] string? educationLevel,
            [FromQuery] Guid? countryId,
            [FromQuery] bool? isHighestEducation,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "isHighestEducation",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyFilter(query, educationLevel, countryId, isHighestEducation, isVerified, isActive, search);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var actorNames = await GetActorNameMapAsync(
                entities.SelectMany(x => new[] { x.CreateBy, x.VerifiedByUserId ?? Guid.Empty }),
                cancellationToken);

            return Ok(ApiResponse<PagedResult<WfpEducationResponse>>.Ok(
                new PagedResult<WfpEducationResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = entities.Select(x => MapResponse(x, actorNames)).ToList()
                },
                "Data pendidikan workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat detail pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetEducationById(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));
            }

            var actorNames = await GetActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy, entity.VerifiedByUserId ?? Guid.Empty },
                cancellationToken);

            return Ok(ApiResponse<WfpEducationDetailResponse>.Ok(
                MapDetailResponse(entity, actorNames),
                "Detail pendidikan workforce berhasil diambil."));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Create", "Create Workforce Education", Description = "Membuat pendidikan workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceEducation", "Create")]
        public async Task<IActionResult> CreateEducation(
            Guid workforceProfileId,
            [FromForm] CreateWfpEducationRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Profil tenaga kerja tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, null, request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data pendidikan workforce tidak valid."));
            }

            StoredFileResult? storedFile = null;
            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);
                if (!fileValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, fileValidation.ErrorMessage ?? "File pendidikan tidak valid."));
                }
                storedFile = await SaveFileAsync(workforceProfileId, request.File, cancellationToken);
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsHighestEducation)
                {
                    await UnsetOtherHighestAsync(workforceProfileId, null, now, actorUserId, cancellationToken);
                }

                var entity = new WfpEducation
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    RequirementCode = NormalizeNullableText(request.RequirementCode),
                    EducationLevel = NormalizeEducationLevel(request.EducationLevel),
                    InstitutionName = request.InstitutionName.Trim(),
                    Major = NormalizeNullableText(request.Major),
                    GraduationYear = request.GraduationYear,
                    StartDate = request.StartDate?.Date,
                    EndDate = request.EndDate?.Date,
                    CountryId = NormalizeNullableGuid(request.CountryId),
                    CertificateNumber = NormalizeNullableText(request.CertificateNumber),
                    FilePath = storedFile?.PublicPath,
                    FileContentType = storedFile?.ContentType,
                    IsHighestEducation = request.IsHighestEducation,
                    IsVerified = false,
                    Description = NormalizeNullableText(request.Description),
                    IsActive = request.IsActive,
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpEducation>().Add(entity);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceEducation.CreateEducation",
                    "Pendidikan workforce berhasil dibuat.",
                    new { entity.Id, entity.WorkforceProfileId, entity.EducationLevel, entity.InstitutionName, entity.IsHighestEducation });

                return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Pendidikan workforce berhasil dibuat.", cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                if (storedFile != null) DeletePhysicalFileIfExists(storedFile.PublicPath);
                throw;
            }
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Education", Description = "Mengubah pendidikan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> UpdateEducation(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWfpEducationRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, id, request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data pendidikan workforce tidak valid."));
            }

            if (request.File != null && !request.ReplaceExistingFile && !string.IsNullOrWhiteSpace(entity.FilePath))
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Pendidikan sudah memiliki file. Aktifkan ReplaceExistingFile untuk mengganti file."));
            }

            StoredFileResult? storedFile = null;
            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);
                if (!fileValidation.IsValid)
                {
                    return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, fileValidation.ErrorMessage ?? "File pendidikan tidak valid."));
                }
                storedFile = await SaveFileAsync(workforceProfileId, request.File, cancellationToken);
            }

            var oldFilePath = entity.FilePath;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                if (request.IsHighestEducation)
                {
                    await UnsetOtherHighestAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.RequirementCode = NormalizeNullableText(request.RequirementCode);
                entity.EducationLevel = NormalizeEducationLevel(request.EducationLevel);
                entity.InstitutionName = request.InstitutionName.Trim();
                entity.Major = NormalizeNullableText(request.Major);
                entity.GraduationYear = request.GraduationYear;
                entity.StartDate = request.StartDate?.Date;
                entity.EndDate = request.EndDate?.Date;
                entity.CountryId = NormalizeNullableGuid(request.CountryId);
                entity.CertificateNumber = NormalizeNullableText(request.CertificateNumber);
                entity.IsHighestEducation = request.IsHighestEducation;
                entity.Description = NormalizeNullableText(request.Description);
                entity.IsActive = request.IsActive;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                if (storedFile != null)
                {
                    entity.FilePath = storedFile.PublicPath;
                    entity.FileContentType = storedFile.ContentType;
                    entity.IsVerified = false;
                    entity.VerifiedAt = null;
                    entity.VerifiedByUserId = null;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                if (storedFile != null && !string.IsNullOrWhiteSpace(oldFilePath))
                {
                    DeletePhysicalFileIfExists(oldFilePath);
                }

                return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Pendidikan workforce berhasil diperbarui.", cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                if (storedFile != null) DeletePhysicalFileIfExists(storedFile.PublicPath);
                throw;
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Education", Description = "Mengubah status pendidikan workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> UpdateEducationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpEducationStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));

            entity.IsActive = request.IsActive;
            if (!request.IsActive) entity.IsHighestEducation = false;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "Status pendidikan workforce berhasil diperbarui.", cancellationToken);
        }

        [HttpPatch("{id:guid}/highest")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Set Highest Workforce Education", Description = "Menetapkan pendidikan tertinggi workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> SetHighestEducation(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWfpEducationHighestRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));
            if (request.IsHighestEducation && !entity.IsActive)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Pendidikan tidak aktif tidak dapat dijadikan pendidikan tertinggi."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                if (request.IsHighestEducation)
                {
                    await UnsetOtherHighestAsync(workforceProfileId, id, now, actorUserId, cancellationToken);
                }

                entity.IsHighestEducation = request.IsHighestEducation;
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }

            return await BuildDetailResultAsync(
                entity.Id,
                workforceProfileId,
                request.IsHighestEducation ? "Pendidikan tertinggi berhasil ditetapkan." : "Status pendidikan tertinggi berhasil dibatalkan.",
                cancellationToken);
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Verify Workforce Education", Description = "Memverifikasi pendidikan workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> VerifyEducation(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWfpEducationRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));

            var actorUserId = GetCurrentUserId();
            entity.IsVerified = request.IsVerified;
            entity.VerifiedAt = request.IsVerified ? DateTime.UtcNow : null;
            entity.VerifiedByUserId = request.IsVerified ? actorUserId : null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return await BuildDetailResultAsync(
                entity.Id,
                workforceProfileId,
                request.IsVerified ? "Pendidikan workforce berhasil diverifikasi." : "Verifikasi pendidikan workforce berhasil dibatalkan.",
                cancellationToken);
        }

        [HttpGet("{id:guid}/file")]
        [AccessAction("Read", "Read Workforce Education", Description = "Mengunduh file pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> DownloadEducationFile(Guid workforceProfileId, Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));

            var physicalPath = ResolvePhysicalPath(entity.FilePath);
            if (physicalPath == null || !System.IO.File.Exists(physicalPath))
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "File pendidikan workforce tidak ditemukan."));

            var stream = new FileStream(physicalPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var downloadName = $"{SanitizeFileName(entity.InstitutionName)}{Path.GetExtension(physicalPath)}";
            return File(stream, entity.FileContentType ?? "application/octet-stream", downloadName, enableRangeProcessing: true);
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<WfpEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Education File", Description = "Menghapus file pendidikan workforce", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkforceEducation", "Delete")]
        public async Task<IActionResult> DeleteEducationFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWfpEducationFileRequest? request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));

            var oldPath = entity.FilePath;
            entity.FilePath = null;
            entity.FileContentType = null;
            entity.IsVerified = false;
            entity.VerifiedAt = null;
            entity.VerifiedByUserId = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (request?.DeletePhysicalFile ?? true) DeletePhysicalFileIfExists(oldPath);
            return await BuildDetailResultAsync(entity.Id, workforceProfileId, "File pendidikan workforce berhasil dihapus.", cancellationToken);
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Education", Description = "Menghapus pendidikan workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceEducation", "Delete")]
        public async Task<IActionResult> DeleteEducation(
            Guid workforceProfileId,
            Guid id,
            [FromQuery] bool deletePhysicalFile = true,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete, cancellationToken);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Pendidikan workforce tidak ditemukan."));

            var filePath = entity.FilePath;
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsHighestEducation = false;
            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;
            await _dbContext.SaveChangesAsync(cancellationToken);

            if (deletePhysicalFile) DeletePhysicalFileIfExists(filePath);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEducation.DeleteEducation",
                "Pendidikan workforce berhasil dihapus.",
                new { entity.Id, entity.WorkforceProfileId, entity.EducationLevel, entity.InstitutionName });

            return Ok(ApiResponse<object>.Ok(null, "Pendidikan workforce berhasil dihapus."));
        }

        private IQueryable<WfpEducation> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Country)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpEducation> ApplyFilter(
            IQueryable<WfpEducation> query,
            string? educationLevel,
            Guid? countryId,
            bool? isHighestEducation,
            bool? isVerified,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(educationLevel))
                query = query.Where(x => x.EducationLevel.ToLower() == educationLevel.Trim().ToLower());
            if (countryId.HasValue && countryId.Value != Guid.Empty)
                query = query.Where(x => x.CountryId == countryId.Value);
            if (isHighestEducation.HasValue)
                query = query.Where(x => x.IsHighestEducation == isHighestEducation.Value);
            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EducationLevel.ToLower().Contains(keyword) ||
                    x.InstitutionName.ToLower().Contains(keyword) ||
                    (x.Major != null && x.Major.ToLower().Contains(keyword)) ||
                    (x.CertificateNumber != null && x.CertificateNumber.ToLower().Contains(keyword)) ||
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    (x.Country != null && x.Country.CountryName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpEducation> ApplySorting(
            IQueryable<WfpEducation> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = !string.Equals(sortDirection?.Trim(), "asc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "isHighestEducation").Trim().Replace("_", string.Empty).ToLowerInvariant() switch
            {
                "educationlevel" => desc ? query.OrderByDescending(x => x.EducationLevel).ThenBy(x => x.InstitutionName) : query.OrderBy(x => x.EducationLevel).ThenBy(x => x.InstitutionName),
                "institutionname" => desc ? query.OrderByDescending(x => x.InstitutionName) : query.OrderBy(x => x.InstitutionName),
                "graduationyear" => desc ? query.OrderByDescending(x => x.GraduationYear) : query.OrderBy(x => x.GraduationYear),
                "isverified" => desc ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.GraduationYear) : query.OrderBy(x => x.IsVerified).ThenByDescending(x => x.GraduationYear),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.GraduationYear) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.GraduationYear),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc
                    ? query.OrderByDescending(x => x.IsHighestEducation).ThenByDescending(x => x.GraduationYear).ThenBy(x => x.InstitutionName)
                    : query.OrderBy(x => x.IsHighestEducation).ThenBy(x => x.GraduationYear).ThenBy(x => x.InstitutionName)
            };
        }

        private WfpEducationResponse MapResponse(WfpEducation x, IReadOnlyDictionary<Guid, string> actorNames)
        {
            return new WfpEducationResponse
            {
                Id = x.Id,
                WorkforceProfileId = x.WorkforceProfileId,
                WorkforceProfileCode = x.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = x.WorkforceProfile?.DisplayName ?? string.Empty,
                RequirementCode = x.RequirementCode,
                EducationLevel = x.EducationLevel,
                InstitutionName = x.InstitutionName,
                Major = x.Major,
                GraduationYear = x.GraduationYear,
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                CountryId = x.CountryId,
                CountryCode = x.Country?.CountryCode,
                CountryName = x.Country?.CountryName,
                CertificateNumber = x.CertificateNumber,
                FilePath = x.FilePath,
                FileUrl = BuildPublicFileUrl(x.FilePath),
                FileContentType = x.FileContentType,
                HasFile = !string.IsNullOrWhiteSpace(x.FilePath),
                IsHighestEducation = x.IsHighestEducation,
                IsVerified = x.IsVerified,
                VerifiedAt = x.VerifiedAt,
                VerifiedByUserId = x.VerifiedByUserId,
                VerifiedByName = GetActorName(actorNames, x.VerifiedByUserId),
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetActorName(actorNames, x.CreateBy)
            };
        }

        private WfpEducationDetailResponse MapDetailResponse(WfpEducation x, IReadOnlyDictionary<Guid, string> actorNames)
        {
            var b = MapResponse(x, actorNames);
            return new WfpEducationDetailResponse
            {
                Id = b.Id,
                WorkforceProfileId = b.WorkforceProfileId,
                WorkforceProfileCode = b.WorkforceProfileCode,
                WorkforceDisplayName = b.WorkforceDisplayName,
                RequirementCode = b.RequirementCode,
                EducationLevel = b.EducationLevel,
                InstitutionName = b.InstitutionName,
                Major = b.Major,
                GraduationYear = b.GraduationYear,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                CountryId = b.CountryId,
                CountryCode = b.CountryCode,
                CountryName = b.CountryName,
                CertificateNumber = b.CertificateNumber,
                FilePath = b.FilePath,
                FileUrl = b.FileUrl,
                FileContentType = b.FileContentType,
                HasFile = b.HasFile,
                IsHighestEducation = b.IsHighestEducation,
                IsVerified = b.IsVerified,
                VerifiedAt = b.VerifiedAt,
                VerifiedByUserId = b.VerifiedByUserId,
                VerifiedByName = b.VerifiedByName,
                Description = b.Description,
                IsActive = b.IsActive,
                CreateDateTime = b.CreateDateTime,
                CreateBy = b.CreateBy,
                CreateByName = b.CreateByName,
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = GetActorName(actorNames, x.UpdateBy)
            };
        }

        private async Task<IActionResult> BuildDetailResultAsync(Guid id, Guid workforceProfileId, string message, CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId).FirstAsync(x => x.Id == id, cancellationToken);
            var actorNames = await GetActorNameMapAsync(
                new[] { entity.CreateBy, entity.UpdateBy, entity.VerifiedByUserId ?? Guid.Empty },
                cancellationToken);
            return Ok(ApiResponse<WfpEducationDetailResponse>.Ok(MapDetailResponse(entity, actorNames), message));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWfpEducationRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.EducationLevel))
                return (false, "Jenjang pendidikan wajib diisi.");
            if (string.IsNullOrWhiteSpace(request.InstitutionName))
                return (false, "Nama institusi wajib diisi.");
            if (request.StartDate.HasValue && request.EndDate.HasValue && request.EndDate.Value.Date < request.StartDate.Value.Date)
                return (false, "Tanggal selesai tidak boleh lebih kecil dari tanggal mulai.");
            if (request.GraduationYear.HasValue && (request.GraduationYear.Value < 1900 || request.GraduationYear.Value > DateTime.UtcNow.Year + 10))
                return (false, "Tahun kelulusan tidak valid.");
            if (request.EndDate.HasValue && request.GraduationYear.HasValue && request.EndDate.Value.Year != request.GraduationYear.Value)
                return (false, "Tahun kelulusan harus sesuai dengan tahun tanggal selesai pendidikan.");

            if (request.CountryId.HasValue && request.CountryId.Value != Guid.Empty)
            {
                var countryExists = await _dbContext.MstCountries.AsNoTracking()
                    .AnyAsync(x => x.Id == request.CountryId.Value && x.IsActive && !x.IsDelete, cancellationToken);
                if (!countryExists) return (false, "Negara tidak ditemukan atau sudah tidak aktif.");
            }

            if (!string.IsNullOrWhiteSpace(request.CertificateNumber))
            {
                var certificate = request.CertificateNumber.Trim().ToLower();
                var duplicate = await _dbContext.Set<WfpEducation>().AsNoTracking()
                    .AnyAsync(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.Id != excludeId &&
                        !x.IsDelete &&
                        x.CertificateNumber != null &&
                        x.CertificateNumber.ToLower() == certificate,
                        cancellationToken);
                if (duplicate) return (false, "Nomor sertifikat pendidikan sudah digunakan oleh workforce ini.");
            }

            return (true, null);
        }

        private async Task UnsetOtherHighestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            DateTime now,
            Guid actorUserId,
            CancellationToken cancellationToken)
        {
            var rows = await _dbContext.Set<WfpEducation>()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.Id != excludeId &&
                    x.IsHighestEducation &&
                    !x.IsDelete)
                .ToListAsync(cancellationToken);
            foreach (var row in rows)
            {
                row.IsHighestEducation = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actorUserId;
            }
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0) return (false, "File pendidikan kosong.");
            if (file.Length > MaximumFileSizeBytes) return (false, "Ukuran file pendidikan maksimal 10 MB.");
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)) return (false, "Format file harus PDF, JPG, JPEG, atau PNG.");
            if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase)) return (false, "Content type file tidak diizinkan.");
            return (true, null);
        }

        private async Task<StoredFileResult> SaveFileAsync(Guid workforceProfileId, IFormFile file, CancellationToken cancellationToken)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var relativeDirectory = Path.Combine("human-resource", "workforce", workforceProfileId.ToString(), "educations");
            var physicalDirectory = Path.Combine(GetUploadRootPath(), relativeDirectory);
            Directory.CreateDirectory(physicalDirectory);
            var physicalPath = Path.Combine(physicalDirectory, storedFileName);
            await using (var output = new FileStream(physicalPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(output, cancellationToken);
            }
            var publicPath = $"{GetPublicRequestPath()}/{relativeDirectory.Replace(Path.DirectorySeparatorChar, '/')}/{storedFileName}";
            return new StoredFileResult(publicPath, file.ContentType);
        }

        private string GetUploadRootPath()
        {
            var configured = _configuration["FileStorage:UploadRootPath"];
            var path = string.IsNullOrWhiteSpace(configured) ? Path.Combine(_environment.ContentRootPath, "uploads") : configured.Trim();
            if (!Path.IsPathRooted(path)) path = Path.Combine(_environment.ContentRootPath, path);
            path = Path.GetFullPath(path);
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetPublicRequestPath()
        {
            var value = (_configuration["FileStorage:PublicRequestPath"] ?? "/uploads").Replace("\\", "/").Trim();
            if (!value.StartsWith('/')) value = "/" + value;
            return value.TrimEnd('/');
        }

        private string? ResolvePhysicalPath(string? publicPath)
        {
            if (string.IsNullOrWhiteSpace(publicPath)) return null;
            var normalized = publicPath.Replace("\\", "/");
            var requestPath = GetPublicRequestPath();
            if (normalized.StartsWith(requestPath, StringComparison.OrdinalIgnoreCase)) normalized = normalized[requestPath.Length..];
            normalized = normalized.TrimStart('/');
            var root = GetUploadRootPath();
            var candidate = Path.GetFullPath(Path.Combine(root, normalized.Replace('/', Path.DirectorySeparatorChar)));
            return candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase) ? candidate : null;
        }

        private void DeletePhysicalFileIfExists(string? publicPath)
        {
            var path = ResolvePhysicalPath(publicPath);
            if (path != null && System.IO.File.Exists(path)) System.IO.File.Delete(path);
        }

        private string? BuildPublicFileUrl(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath)) return null;
            if (filePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || filePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return filePath;
            var baseUrl = _configuration["FileStorage:PublicBaseUrl"];
            return !string.IsNullOrWhiteSpace(baseUrl)
                ? $"{baseUrl.TrimEnd('/')}/{filePath.TrimStart('/')}"
                : $"{Request.Scheme}://{Request.Host}/{filePath.TrimStart('/')}";
        }

        private async Task<bool> WorkforceProfileExistsAsync(Guid workforceProfileId, CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty && await _dbContext.MstWorkforceProfiles.AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && x.IsActive && !x.IsDelete, cancellationToken);
        }

        private async Task<Dictionary<Guid, string>> GetActorNameMapAsync(IEnumerable<Guid> ids, CancellationToken cancellationToken)
        {
            var userIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            if (userIds.Count == 0) return new Dictionary<Guid, string>();
            return await _dbContext.Users.AsNoTracking()
                .Where(x => userIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode, cancellationToken);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> names, Guid? id)
            => id.HasValue && id.Value != Guid.Empty && names.TryGetValue(id.Value, out var name) ? name : null;

        private Guid GetCurrentUserId()
        {
            var text = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(text, out var id) ? id : Guid.Empty;
        }

        private static string NormalizeEducationLevel(string value)
        {
            var match = EducationLevels.FirstOrDefault(x => x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
            return match ?? value.Trim();
        }

        private static string BuildEducationLevelLabel(string value) => value switch
        {
            "Elementary" => "SD/Sederajat",
            "JuniorHighSchool" => "SMP/Sederajat",
            "SeniorHighSchool" => "SMA/SMK/Sederajat",
            "Diploma1" => "Diploma I",
            "Diploma2" => "Diploma II",
            "Diploma3" => "Diploma III",
            "Diploma4" => "Diploma IV",
            "Bachelor" => "Sarjana (S1)",
            "Professional" => "Profesi",
            "Master" => "Magister (S2)",
            "Specialist" => "Spesialis",
            "Doctorate" => "Doktor (S3)",
            "PostDoctorate" => "Postdoctoral",
            _ => "Lainnya"
        };

        private static string SanitizeFileName(string value)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(value.Select(ch => invalid.Contains(ch) ? '_' : ch)).Trim();
        }

        private static Guid? NormalizeNullableGuid(Guid? value) => value.HasValue && value.Value != Guid.Empty ? value.Value : null;
        private static string? NormalizeNullableText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 100);
            return (pageNumber, pageSize);
        }

        private sealed record StoredFileResult(string PublicPath, string ContentType);
    }
}
