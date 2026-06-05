using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceEducationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceEducationResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/educations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
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
        private const string CodePrefix = "EDU-RSMMC-";
        private const int CodeNumberLength = 5;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
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
        private readonly IConfiguration _configuration;

        public WorkforceEducationController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
            _configuration = configuration;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat metadata filter pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceEducationFilterMetadataResponse
            {
                DefaultFilter = new WorkforceEducationDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceEducationCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceEducationSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "requirementCode", Label = "Kode pendidikan" },
                    new() { Value = "educationLevel", Label = "Jenjang pendidikan" },
                    new() { Value = "institutionName", Label = "Institusi" },
                    new() { Value = "major", Label = "Jurusan" },
                    new() { Value = "graduationYear", Label = "Tahun lulus" },
                    new() { Value = "certificateNumber", Label = "Nomor ijazah/sertifikat" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EducationLevelOptions = BuildEducationLevelOptions(),
                FrontendGuide = new List<string>
                {
                    "EducationLevel adalah enum/dropdown jenjang pendidikan, bukan input text bebas.",
                    "InstitutionName adalah nama kampus/sekolah/institusi, contoh: Universitas Indonesia, Poltekkes Kemenkes, SMK Kesehatan.",
                    "Major adalah jurusan/prodi, contoh: Kedokteran, Keperawatan, Farmasi, Rekam Medis, Teknik Informatika.",
                    "RequirementCode dibuat otomatis oleh backend dengan format EDU-RSMMC-00001, jadi frontend tidak perlu mengirim RequirementCode.",
                    "Gunakan FilePreviewUrl untuk preview modal/iframe dan FileDownloadUrl untuk download."
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEducation.GetFilterMetadata",
                "Mengambil metadata filter pendidikan workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceEducationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter pendidikan workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat ringkasan pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceEducationSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalEducation = await query.CountAsync(),
                ActiveEducation = await query.CountAsync(x => x.IsActive),
                InactiveEducation = await query.CountAsync(x => !x.IsActive),
                VerifiedEducation = await query.CountAsync(x => x.IsVerified),
                UnverifiedEducation = await query.CountAsync(x => !x.IsVerified),
                EducationWithFile = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                EducationWithoutFile = await query.CountAsync(x => x.FilePath == null || x.FilePath == string.Empty),
                DiplomaEducation = await query.CountAsync(x => x.EducationLevel == "D1" || x.EducationLevel == "D2" || x.EducationLevel == "D3" || x.EducationLevel == "D4"),
                BachelorEducation = await query.CountAsync(x => x.EducationLevel == "S1"),
                ProfessionEducation = await query.CountAsync(x => x.EducationLevel == "PROFESSION"),
                MasterEducation = await query.CountAsync(x => x.EducationLevel == "S2"),
                DoctoralEducation = await query.CountAsync(x => x.EducationLevel == "S3"),
                SpecialistEducation = await query.CountAsync(x => x.EducationLevel == "SPECIALIST_1" || x.EducationLevel == "SPECIALIST_2")
            };

            return Ok(ApiResponse<WorkforceEducationSummaryResponse>.Ok(
                result,
                "Ringkasan pendidikan workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceEducationPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat pendidikan workforce profile", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetEducations(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] WorkforceEducationLevel? educationLevel,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, educationLevel, isVerified, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(x => MapResponse(x, profile))
                .ToList();

            var result = new ResponseWorkforceEducationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceEducationPagedResult>.Ok(
                result,
                "Data pendidikan workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat pilihan pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] WorkforceEducationLevel? educationLevel,
            [FromQuery] bool? isVerified,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery(workforceProfileId);
            query = ApplyStandardFilter(query, educationLevel, isVerified, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsVerified)
                .ThenByDescending(x => x.GraduationYear)
                .ThenBy(x => x.EducationLevel)
                .ThenBy(x => x.InstitutionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceEducationOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    EducationLevel = x.EducationLevel,
                    InstitutionName = x.InstitutionName,
                    Major = x.Major,
                    GraduationYear = x.GraduationYear,
                    CertificateNumber = x.CertificateNumber,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty,
                    IsVerified = x.IsVerified
                })
                .ToListAsync();

            var result = new WorkforceEducationOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceEducationOptionPagedResponse>.Ok(
                result,
                "Data pilihan pendidikan workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Education", Description = "Melihat detail pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> GetEducationById(Guid workforceProfileId, Guid id)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceEducationDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Detail pendidikan workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Education", Description = "Menambah pendidikan workforce profile", AccessType = AccessTypes.Create, SortOrder = 2)]
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

            var validation = await ValidateRequestAsync(workforceProfileId, null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pendidikan tidak valid."
                ));
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
                RequirementCode = await GenerateEducationCodeAsync(),
                EducationLevel = NormalizeEducationLevel(request.EducationLevel),
                InstitutionName = NormalizeRequiredText(request.InstitutionName),
                Major = NormalizeNullableText(request.Major),
                GraduationYear = request.GraduationYear,
                CertificateNumber = NormalizeNullableText(request.CertificateNumber),
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

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEducation.CreateEducation",
                "Pendidikan workforce berhasil dibuat.",
                new { workforceProfileId, entity.Id, entity.RequirementCode }
            );

            return Ok(ApiResponse<WorkforceEducationDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Pendidikan workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEducationDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Education", Description = "Mengubah pendidikan workforce profile", AccessType = AccessTypes.Update, SortOrder = 3)]
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
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data pendidikan tidak valid."
                ));
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
            else if (request.ReplaceExistingFile && request.File == null)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
                entity.FilePath = null;
                entity.FileContentType = null;
            }

            entity.EducationLevel = NormalizeEducationLevel(request.EducationLevel);
            entity.InstitutionName = NormalizeRequiredText(request.InstitutionName);
            entity.Major = NormalizeNullableText(request.Major);
            entity.GraduationYear = request.GraduationYear;
            entity.CertificateNumber = NormalizeNullableText(request.CertificateNumber);
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<WorkforceEducationDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Pendidikan workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Education", Description = "Mengubah status pendidikan workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> UpdateEducationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceEducationStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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
        [AccessAction("Update", "Verify Workforce Education", Description = "Verifikasi pendidikan workforce", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceEducation", "Update")]
        public async Task<IActionResult> VerifyEducation(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceEducationRequest request)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Education", Description = "Preview file pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> PreviewEducation(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetEducationFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File pendidikan workforce tidak ditemukan."
                ));
            }

            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = fileValidation.FileName
            }.ToString();

            var stream = new FileStream(
                fileValidation.PhysicalPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(stream, fileValidation.ContentType!, enableRangeProcessing: true);
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Education", Description = "Download file pendidikan workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEducation", "Read")]
        public async Task<IActionResult> DownloadEducation(Guid workforceProfileId, Guid id)
        {
            var fileValidation = await GetEducationFileAsync(workforceProfileId, id);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File pendidikan workforce tidak ditemukan."
                ));
            }

            var stream = new FileStream(
                fileValidation.PhysicalPath!,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read
            );

            return File(
                stream,
                fileValidation.ContentType!,
                fileValidation.DownloadName,
                enableRangeProcessing: true
            );
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Education File", Description = "Menghapus file pendidikan workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceEducation", "Delete")]
        public async Task<IActionResult> DeleteEducationFile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] DeleteWorkforceEducationFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pendidikan workforce tidak ditemukan."
                ));
            }

            if (request?.DeletePhysicalFile ?? true)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
            }

            entity.FilePath = null;
            entity.FileContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "File pendidikan workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Education", Description = "Menghapus pendidikan workforce", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceEducation", "Delete")]
        public async Task<IActionResult> DeleteEducation(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpEducation>()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

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
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Pendidikan workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpEducation> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpEducation> ApplyDateFilter(
            IQueryable<WfpEducation> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue && !endDate.HasValue && !string.IsNullOrWhiteSpace(customPeriod))
            {
                var now = DateTime.UtcNow;
                var today = now.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x => x.CreateDateTime >= today && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x => x.CreateDateTime >= today.AddDays(-6) && x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        query = query.Where(x => x.CreateDateTime >= thisMonthStart && x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        var lastMonthStart = currentMonthStart.AddMonths(-1);
                        query = query.Where(x => x.CreateDateTime >= lastMonthStart && x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<WfpEducation> ApplyStandardFilter(
            IQueryable<WfpEducation> query,
            WorkforceEducationLevel? educationLevel,
            bool? isVerified,
            bool? isActive,
            string? search)
        {
            if (educationLevel.HasValue && educationLevel.Value != WorkforceEducationLevel.Unknown)
            {
                var selectedLevel = NormalizeEducationLevel(educationLevel.Value);
                query = query.Where(x => x.EducationLevel == selectedLevel);
            }

            if (isVerified.HasValue)
            {
                query = query.Where(x => x.IsVerified == isVerified.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    x.EducationLevel.ToLower().Contains(keyword) ||
                    x.InstitutionName.ToLower().Contains(keyword) ||
                    (x.Major != null && x.Major.ToLower().Contains(keyword)) ||
                    (x.CertificateNumber != null && x.CertificateNumber.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpEducation> ApplySorting(
            IQueryable<WfpEducation> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "requirementcode" => isDescending ? query.OrderByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementCode),
                "educationlevel" => isDescending ? query.OrderByDescending(x => x.EducationLevel).ThenBy(x => x.InstitutionName) : query.OrderBy(x => x.EducationLevel).ThenBy(x => x.InstitutionName),
                "institutionname" => isDescending ? query.OrderByDescending(x => x.InstitutionName) : query.OrderBy(x => x.InstitutionName),
                "major" => isDescending ? query.OrderByDescending(x => x.Major) : query.OrderBy(x => x.Major),
                "graduationyear" => isDescending ? query.OrderByDescending(x => x.GraduationYear).ThenBy(x => x.InstitutionName) : query.OrderBy(x => x.GraduationYear).ThenBy(x => x.InstitutionName),
                "certificatenumber" => isDescending ? query.OrderByDescending(x => x.CertificateNumber) : query.OrderBy(x => x.CertificateNumber),
                "isverified" => isDescending ? query.OrderByDescending(x => x.IsVerified).ThenBy(x => x.InstitutionName) : query.OrderBy(x => x.IsVerified).ThenBy(x => x.InstitutionName),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.InstitutionName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.InstitutionName),
                _ => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenBy(x => x.InstitutionName) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.InstitutionName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            CreateWorkforceEducationRequest request)
        {
            var profileExists = await ProfileExistsAsync(workforceProfileId);

            if (!profileExists)
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (!Enum.IsDefined(typeof(WorkforceEducationLevel), request.EducationLevel) ||
                request.EducationLevel == WorkforceEducationLevel.Unknown)
            {
                return (false, "EducationLevel wajib dipilih dan tidak boleh Unknown.");
            }

            if (string.IsNullOrWhiteSpace(request.InstitutionName))
            {
                return (false, "InstitutionName wajib diisi.");
            }

            if (request.GraduationYear.HasValue)
            {
                var maxYear = DateTime.UtcNow.Year + 1;

                if (request.GraduationYear.Value < 1900 || request.GraduationYear.Value > maxYear)
                {
                    return (false, $"GraduationYear harus berada di antara 1900 dan {maxYear}.");
                }
            }

            if (request.File != null)
            {
                var fileValidation = ValidateFile(request.File);

                if (!fileValidation.IsValid)
                {
                    return fileValidation;
                }
            }

            var normalizedCertificateNumber = NormalizeNullableText(request.CertificateNumber);

            if (!string.IsNullOrWhiteSpace(normalizedCertificateNumber))
            {
                var duplicateCertificateNumberQuery = _dbContext.Set<WfpEducation>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.CertificateNumber == normalizedCertificateNumber &&
                        !x.IsDelete);

                if (excludeId.HasValue)
                {
                    duplicateCertificateNumberQuery = duplicateCertificateNumberQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateCertificateNumberQuery.AnyAsync())
                {
                    return (false, "Nomor ijazah/sertifikat pendidikan sudah terdaftar pada workforce profile ini.");
                }
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
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

        private async Task<string> GenerateEducationCodeAsync()
        {
            var existingCodes = await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .Where(x => x.RequirementCode != null && x.RequirementCode.StartsWith(CodePrefix))
                .Select(x => x.RequirementCode!)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<(string FilePath, string? ContentType)> SaveEducationFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-educations", workforceProfileId.ToString());
            var absoluteFolder = Path.Combine(storage.RootPath, relativeFolder);

            Directory.CreateDirectory(absoluteFolder);

            var absolutePath = Path.Combine(absoluteFolder, fileName);

            await using (var stream = new FileStream(absolutePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var publicPath = CombineUrlPath(storage.PublicRequestPath, relativeFolder.Replace("\\", "/"), fileName);

            return (publicPath, file.ContentType);
        }

        private async Task<FileResolveResult> GetEducationFileAsync(Guid workforceProfileId, Guid id)
        {
            var education = await _dbContext.Set<WfpEducation>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (education == null)
            {
                return FileResolveResult.Invalid("Pendidikan workforce tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(education.FilePath))
            {
                return FileResolveResult.Invalid("File pendidikan workforce belum tersedia.");
            }

            var physicalPath = ResolvePhysicalPath(education.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return FileResolveResult.Invalid("File fisik pendidikan tidak ditemukan di server.");
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = education.FileContentType ?? "application/octet-stream";
            }

            var extension = Path.GetExtension(physicalPath);
            var safeInstitutionName = SanitizeFileName(education.InstitutionName);
            var fileName = Path.GetFileName(physicalPath);
            var downloadName = $"{education.RequirementCode ?? "EDUCATION"}_{safeInstitutionName}{extension}";

            return FileResolveResult.Valid(physicalPath, contentType, fileName, downloadName);
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
            var storage = GetFileStoragePaths();
            var normalizedFilePath = filePath.Replace("\\", "/").Trim();
            var publicPrefix = storage.PublicRequestPath.TrimEnd('/');

            string relativePath;

            if (normalizedFilePath.StartsWith(publicPrefix + "/", StringComparison.OrdinalIgnoreCase))
            {
                relativePath = normalizedFilePath[(publicPrefix.Length + 1)..];
            }
            else
            {
                relativePath = normalizedFilePath.TrimStart('/');
            }

            var fullPath = Path.GetFullPath(Path.Combine(storage.RootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())));
            var rootPath = Path.GetFullPath(storage.RootPath);

            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Path file tidak valid.");
            }

            return fullPath;
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            if (!publicRequestPath.StartsWith('/'))
            {
                publicRequestPath = "/" + publicRequestPath;
            }

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                Directory.CreateDirectory(configuredRoot);
                return (configuredRoot, publicRequestPath);
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            var rootPath = Path.Combine(webRootPath, publicRequestPath.TrimStart('/'));
            Directory.CreateDirectory(rootPath);

            return (rootPath, publicRequestPath);
        }

        private WorkforceEducationResponse MapResponse(WfpEducation entity, MstWorkforceProfile profile)
        {
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);

            return new WorkforceEducationResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = entity.RequirementCode,
                EducationLevel = entity.EducationLevel,
                InstitutionName = entity.InstitutionName,
                Major = entity.Major,
                GraduationYear = entity.GraduationYear,
                CertificateNumber = entity.CertificateNumber,
                HasFile = hasFile,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                FileName = hasFile ? Path.GetFileName(entity.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                IsVerified = entity.IsVerified,
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private WorkforceEducationDetailResponse MapDetailResponse(WfpEducation entity, MstWorkforceProfile profile)
        {
            var response = MapResponse(entity, profile);

            return new WorkforceEducationDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                RequirementCode = response.RequirementCode,
                EducationLevel = response.EducationLevel,
                InstitutionName = response.InstitutionName,
                Major = response.Major,
                GraduationYear = response.GraduationYear,
                CertificateNumber = response.CertificateNumber,
                HasFile = response.HasFile,
                FilePath = response.FilePath,
                FileContentType = response.FileContentType,
                FileName = response.FileName,
                FilePreviewUrl = response.FilePreviewUrl,
                FileDownloadUrl = response.FileDownloadUrl,
                IsVerified = response.IsVerified,
                IsActive = response.IsActive,
                Description = response.Description,
                CreateDateTime = response.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                CreateBy = entity.CreateBy,
                UpdateBy = entity.UpdateBy
            };
        }

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/educations/{id}/{action}";

            return $"{Request.Scheme}://{Request.Host}{path}";
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private async Task<bool> ProfileExistsAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
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

        private static string NormalizeEducationLevel(WorkforceEducationLevel value)
        {
            return value.ToString().Trim().ToUpperInvariant();
        }

        private static string NormalizeRequiredText(string value)
        {
            return value.Trim();
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string SanitizeFileName(string value)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var cleaned = new string(value.Select(ch => invalidChars.Contains(ch) ? '-' : ch).ToArray());

            return string.IsNullOrWhiteSpace(cleaned)
                ? "education"
                : cleaned.Trim();
        }

        private static string CombineUrlPath(params string[] parts)
        {
            return "/" + string.Join(
                "/",
                parts
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(x => x.Trim('/'))
            );
        }

        private static List<WorkforceEducationLevelOptionResponse> BuildEducationLevelOptions()
        {
            return new List<WorkforceEducationLevelOptionResponse>
            {
                new()
                {
                    Value = WorkforceEducationLevel.SMA,
                    Code = "SMA",
                    Label = "SMA",
                    Description = "Sekolah Menengah Atas.",
                    UsuallyRequiresCertificateNumber = false
                },
                new()
                {
                    Value = WorkforceEducationLevel.SMK,
                    Code = "SMK",
                    Label = "SMK",
                    Description = "Sekolah Menengah Kejuruan.",
                    UsuallyRequiresCertificateNumber = false
                },
                new()
                {
                    Value = WorkforceEducationLevel.D1,
                    Code = "D1",
                    Label = "Diploma 1",
                    Description = "Pendidikan Diploma 1.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.D2,
                    Code = "D2",
                    Label = "Diploma 2",
                    Description = "Pendidikan Diploma 2.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.D3,
                    Code = "D3",
                    Label = "Diploma 3",
                    Description = "Pendidikan Diploma 3.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.D4,
                    Code = "D4",
                    Label = "Diploma 4",
                    Description = "Pendidikan Diploma 4/Sarjana Terapan.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.S1,
                    Code = "S1",
                    Label = "Sarjana / S1",
                    Description = "Pendidikan Sarjana.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.PROFESSION,
                    Code = "PROFESSION",
                    Label = "Profesi",
                    Description = "Pendidikan profesi seperti Dokter, Ners, Apoteker, dan profesi kesehatan lain.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.S2,
                    Code = "S2",
                    Label = "Magister / S2",
                    Description = "Pendidikan Magister.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.S3,
                    Code = "S3",
                    Label = "Doktor / S3",
                    Description = "Pendidikan Doktor.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.SPECIALIST_1,
                    Code = "SPECIALIST_1",
                    Label = "Spesialis 1",
                    Description = "Pendidikan dokter spesialis tahap 1.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.SPECIALIST_2,
                    Code = "SPECIALIST_2",
                    Label = "Spesialis 2 / Konsultan",
                    Description = "Pendidikan subspesialis/konsultan.",
                    UsuallyRequiresCertificateNumber = true
                },
                new()
                {
                    Value = WorkforceEducationLevel.OTHER,
                    Code = "OTHER",
                    Label = "Lainnya",
                    Description = "Pendidikan lainnya yang belum masuk kategori utama.",
                    UsuallyRequiresCertificateNumber = false
                }
            };
        }

        private sealed class FileResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public string? PhysicalPath { get; private set; }
            public string? ContentType { get; private set; }
            public string? FileName { get; private set; }
            public string? DownloadName { get; private set; }

            public static FileResolveResult Valid(
                string physicalPath,
                string contentType,
                string fileName,
                string downloadName)
            {
                return new FileResolveResult
                {
                    IsValid = true,
                    PhysicalPath = physicalPath,
                    ContentType = contentType,
                    FileName = fileName,
                    DownloadName = downloadName
                };
            }

            public static FileResolveResult Invalid(string errorMessage)
            {
                return new FileResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
