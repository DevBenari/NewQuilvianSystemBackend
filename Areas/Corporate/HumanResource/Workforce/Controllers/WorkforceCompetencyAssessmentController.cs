using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Net.Http.Headers;
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

using ResponseWorkforceCompetencyAssessmentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceCompetencyAssessmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/competency-assessments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Competency Assessment",
        AreaName = "Corporate",
        ControllerName = "WorkforceCompetencyAssessment",
        Description = "Corporate human resource workforce competency assessment",
        SortOrder = 12
    )]
    [Tags("Corporate / Human Resource / Workforce / Competency Assessment")]
    public class WorkforceCompetencyAssessmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.CompetencyAssessment";
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

        public WorkforceCompetencyAssessmentController(
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
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat metadata filter competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetFilterMetadata(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var result = new WorkforceCompetencyAssessmentFilterMetadataResponse
            {
                DefaultFilter = new WorkforceCompetencyAssessmentDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<WorkforceCompetencyAssessmentSortOptionResponse>
                {
                    new() { Value = "assessmentDate", Label = "Tanggal assessment" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "competencyCode", Label = "Kode kompetensi" },
                    new() { Value = "competencyName", Label = "Nama kompetensi" },
                    new() { Value = "competencyCategory", Label = "Kategori kompetensi" },
                    new() { Value = "competencyLevel", Label = "Level kompetensi" },
                    new() { Value = "resultStatus", Label = "Hasil assessment" },
                    new() { Value = "expiredDate", Label = "Tanggal expired" },
                    new() { Value = "isVerified", Label = "Terverifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                CompetencyCategories = BuildEnumOptions<CompetencyCategory>(),
                CompetencyLevels = BuildEnumOptions<CompetencyLevel>(),
                ResultStatuses = BuildEnumOptions<CompetencyAssessmentResultStatus>(),
                MatrixStatuses = BuildMatrixStatusOptions(),
                FrontendGuide = BuildFrontendGuide()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCompetencyAssessment.GetFilterMetadata",
                "Mengambil metadata filter competency assessment workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceCompetencyAssessmentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter competency assessment workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat ringkasan competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetSummary(Guid workforceProfileId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var today = DateTime.UtcNow.Date;
            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceCompetencyAssessmentSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                TotalAssessment = await query.CountAsync(),
                ActiveAssessment = await query.CountAsync(x => x.IsActive),
                InactiveAssessment = await query.CountAsync(x => !x.IsActive),
                VerifiedAssessment = await query.CountAsync(x => x.IsVerified),
                UnverifiedAssessment = await query.CountAsync(x => !x.IsVerified),
                PassedAssessment = await query.CountAsync(x => x.ResultStatus == CompetencyAssessmentResultStatus.Passed),
                FailedAssessment = await query.CountAsync(x => x.ResultStatus == CompetencyAssessmentResultStatus.Failed),
                NeedTrainingAssessment = await query.CountAsync(x => x.ResultStatus == CompetencyAssessmentResultStatus.NeedTraining),
                NotAssessedAssessment = await query.CountAsync(x => x.ResultStatus == CompetencyAssessmentResultStatus.NotAssessed),
                ExpiredAssessment = await query.CountAsync(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today),
                AssessmentWithFile = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                AssessmentWithoutFile = await query.CountAsync(x => x.FilePath == null || x.FilePath == string.Empty)
            };

            return Ok(ApiResponse<WorkforceCompetencyAssessmentSummaryResponse>.Ok(
                result,
                "Ringkasan competency assessment workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceCompetencyAssessmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetAssessments(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? competencyId,
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] CompetencyLevel? competencyLevel,
            [FromQuery] CompetencyAssessmentResultStatus? resultStatus,
            [FromQuery] Guid? assessedByUserId,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "assessmentDate",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

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
            query = ApplyStandardFilter(query, competencyId, competencyCategory, competencyLevel, resultStatus, assessedByUserId, isVerified, isActive, isExpired, search);

            var totalData = await query.CountAsync();

            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new AssessmentProjection
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    AssessmentDate = x.AssessmentDate,
                    CompetencyLevel = x.CompetencyLevel,
                    ResultStatus = x.ResultStatus,
                    AssessedByUserId = x.AssessedByUserId,
                    AssessedByUserName = x.AssessedByUser != null ? x.AssessedByUser.DisplayName : null,
                    ExpiredDate = x.ExpiredDate,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    Notes = x.Notes,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateBy = x.UpdateBy
                })
                .ToListAsync();

            var items = rows.Select(x => MapResponse(x, profile)).ToList();

            var result = new ResponseWorkforceCompetencyAssessmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceCompetencyAssessmentPagedResult>.Ok(
                result,
                "Data competency assessment workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat pilihan competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] Guid? competencyId,
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] CompetencyAssessmentResultStatus? resultStatus,
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
            query = ApplyStandardFilter(query, competencyId, competencyCategory, competencyLevel: null, resultStatus, assessedByUserId: null, isVerified, onlyActive ? true : null, isExpired: null, search);

            var totalData = await query.CountAsync();
            var today = DateTime.UtcNow.Date;

            var items = await query
                .OrderByDescending(x => x.AssessmentDate)
                .ThenBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceCompetencyAssessmentOptionResponse
                {
                    Id = x.Id,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    AssessmentDate = x.AssessmentDate,
                    CompetencyLevel = x.CompetencyLevel,
                    ResultStatus = x.ResultStatus,
                    ExpiredDate = x.ExpiredDate,
                    IsExpired = x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today,
                    IsVerified = x.IsVerified,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty
                })
                .ToListAsync();

            var result = new WorkforceCompetencyAssessmentOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceCompetencyAssessmentOptionPagedResponse>.Ok(
                result,
                "Data pilihan competency assessment workforce berhasil diambil."
            ));
        }

        [HttpGet("{assessmentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Melihat detail competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetAssessmentById(Guid workforceProfileId, Guid assessmentId)
        {
            var response = await BuildAssessmentDetailResponseAsync(workforceProfileId, assessmentId);

            if (response == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>.Ok(
                response,
                "Detail competency assessment workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Competency Assessment", Description = "Membuat competency assessment workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceCompetencyAssessment", "Create")]
        public async Task<IActionResult> CreateAssessment(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceCompetencyAssessmentRequest request)
        {
            var validation = await ValidateAssessmentRequestAsync(
                workforceProfileId,
                excludeId: null,
                competencyId: request.CompetencyId,
                assessedByUserId: request.AssessedByUserId,
                assessmentDate: request.AssessmentDate,
                expiredDate: request.ExpiredDate,
                competencyLevel: request.CompetencyLevel,
                resultStatus: request.ResultStatus,
                file: request.File
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveAssessmentFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new WfpCompetencyAssessment
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CompetencyId = request.CompetencyId,
                AssessmentDate = request.AssessmentDate.Date,
                CompetencyLevel = request.CompetencyLevel,
                ResultStatus = request.ResultStatus,
                AssessedByUserId = NormalizeNullableGuid(request.AssessedByUserId),
                ExpiredDate = request.ExpiredDate?.Date,
                FilePath = filePath,
                FileContentType = fileContentType,
                Notes = NormalizeNullableText(request.Notes),
                IsVerified = request.IsVerified,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.WfpCompetencyAssessments.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceCompetencyAssessment.CreateAssessment",
                "Competency assessment workforce berhasil dibuat.",
                new { entity.Id, entity.WorkforceProfileId, entity.CompetencyId }
            );

            var response = await BuildAssessmentDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>.Ok(
                response!,
                "Competency assessment workforce berhasil dibuat."
            ));
        }

        [HttpPut("{assessmentId:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Competency Assessment", Description = "Mengubah competency assessment workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessment(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromForm] UpdateWorkforceCompetencyAssessmentRequest request)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x => x.Id == assessmentId && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            var validation = await ValidateAssessmentRequestAsync(
                workforceProfileId,
                excludeId: assessmentId,
                competencyId: request.CompetencyId,
                assessedByUserId: request.AssessedByUserId,
                assessmentDate: request.AssessmentDate,
                expiredDate: request.ExpiredDate,
                competencyLevel: request.CompetencyLevel,
                resultStatus: request.ResultStatus,
                file: request.File
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            if (request.ReplaceExistingFile && request.File == null)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
                entity.FilePath = null;
                entity.FileContentType = null;
            }

            if (request.File != null && (request.ReplaceExistingFile || string.IsNullOrWhiteSpace(entity.FilePath)))
            {
                if (request.ReplaceExistingFile)
                    DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveAssessmentFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            entity.CompetencyId = request.CompetencyId;
            entity.AssessmentDate = request.AssessmentDate.Date;
            entity.CompetencyLevel = request.CompetencyLevel;
            entity.ResultStatus = request.ResultStatus;
            entity.AssessedByUserId = NormalizeNullableGuid(request.AssessedByUserId);
            entity.ExpiredDate = request.ExpiredDate?.Date;
            entity.Notes = NormalizeNullableText(request.Notes);
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildAssessmentDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>.Ok(
                response!,
                "Competency assessment workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{assessmentId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Competency Assessment Status", Description = "Mengubah status aktif competency assessment workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> UpdateAssessmentStatus(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromBody] UpdateWorkforceCompetencyAssessmentStatusRequest request)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x => x.Id == assessmentId && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildAssessmentDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>.Ok(
                response!,
                "Status competency assessment workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{assessmentId:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Verify Workforce Competency Assessment", Description = "Verifikasi competency assessment workforce", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission("WorkforceCompetencyAssessment", "Update")]
        public async Task<IActionResult> VerifyAssessment(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromBody] VerifyWorkforceCompetencyAssessmentRequest request)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x => x.Id == assessmentId && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.Notes = NormalizeNullableText(request.Notes) ?? entity.Notes;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = await BuildAssessmentDetailResponseAsync(workforceProfileId, entity.Id);

            return Ok(ApiResponse<WorkforceCompetencyAssessmentDetailResponse>.Ok(
                response!,
                request.IsVerified
                    ? "Competency assessment workforce berhasil diverifikasi."
                    : "Verifikasi competency assessment workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{assessmentId:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Preview file competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> PreviewAssessmentFile(Guid workforceProfileId, Guid assessmentId)
        {
            var fileValidation = await GetAssessmentFileAsync(workforceProfileId, assessmentId);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File competency assessment tidak ditemukan."
                ));
            }

            Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = fileValidation.FileName
            }.ToString();

            var stream = new FileStream(fileValidation.PhysicalPath!, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(stream, fileValidation.ContentType!, enableRangeProcessing: true);
        }

        [HttpGet("{assessmentId:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Competency Assessment", Description = "Download file competency assessment workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> DownloadAssessmentFile(Guid workforceProfileId, Guid assessmentId)
        {
            var fileValidation = await GetAssessmentFileAsync(workforceProfileId, assessmentId);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File competency assessment tidak ditemukan."
                ));
            }

            var stream = new FileStream(fileValidation.PhysicalPath!, FileMode.Open, FileAccess.Read, FileShare.Read);

            return File(stream, fileValidation.ContentType!, fileValidation.DownloadName, enableRangeProcessing: true);
        }

        [HttpDelete("{assessmentId:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Competency Assessment File", Description = "Menghapus file competency assessment workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceCompetencyAssessment", "Delete")]
        public async Task<IActionResult> DeleteAssessmentFile(
            Guid workforceProfileId,
            Guid assessmentId,
            [FromBody] DeleteWorkforceCompetencyAssessmentFileRequest? request = null)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x => x.Id == assessmentId && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            if (request?.DeletePhysicalFile ?? true)
                DeletePhysicalFileIfExists(entity.FilePath);

            entity.FilePath = null;
            entity.FileContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "File competency assessment workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{assessmentId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Workforce Competency Assessment", Description = "Menghapus competency assessment workforce", AccessType = AccessTypes.Delete, SortOrder = 7)]
        [AccessPermission("WorkforceCompetencyAssessment", "Delete")]
        public async Task<IActionResult> DeleteAssessment(Guid workforceProfileId, Guid assessmentId)
        {
            var entity = await _dbContext.WfpCompetencyAssessments
                .FirstOrDefaultAsync(x => x.Id == assessmentId && x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Competency assessment tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Competency assessment berhasil dihapus."
            ));
        }

        [HttpGet("matrix")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceCompetencyMatrixResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Competency Matrix", Description = "Melihat competency matrix workforce", AccessType = AccessTypes.Read, SortOrder = 8)]
        [AccessPermission("WorkforceCompetencyAssessment", "Read")]
        public async Task<IActionResult> GetCompetencyMatrix(
            Guid workforceProfileId,
            [FromQuery] CompetencyCategory? competencyCategory,
            [FromQuery] string? matrixStatus,
            [FromQuery] string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var profile = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null
                })
                .FirstOrDefaultAsync();

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

            var response = new WorkforceCompetencyMatrixResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                PrimaryPositionId = profile.PrimaryPositionId,
                PrimaryPositionName = profile.PrimaryPositionName,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            if (!profile.PrimaryPositionId.HasValue || profile.PrimaryPositionId.Value == Guid.Empty)
            {
                return Ok(ApiResponse<WorkforceCompetencyMatrixResponse>.Ok(
                    response,
                    "Workforce belum memiliki primary position."
                ));
            }

            var today = DateTime.UtcNow.Date;

            var requirements = await _dbContext.MstPositionCompetencyRequirements
                .AsNoTracking()
                .Where(x => x.PositionId == profile.PrimaryPositionId.Value && x.IsActive && !x.IsDelete)
                .OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                .Select(x => new
                {
                    x.Id,
                    x.PositionId,
                    PositionName = x.Position != null ? x.Position.PositionName : string.Empty,
                    x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    x.IsRequired,
                    x.MinimumLevel,
                    x.IsCertificationRequired,
                    x.IsTrainingRequired
                })
                .ToListAsync();

            var assessments = await _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && x.IsActive && !x.IsDelete)
                .OrderByDescending(x => x.AssessmentDate)
                .ToListAsync();

            var allItems = new List<WorkforceCompetencyMatrixItemResponse>();

            foreach (var requirement in requirements)
            {
                var latestAssessment = assessments
                    .Where(x => x.CompetencyId == requirement.CompetencyId)
                    .OrderByDescending(x => x.AssessmentDate)
                    .FirstOrDefault();

                var isExpired = latestAssessment?.ExpiredDate.HasValue == true &&
                                latestAssessment.ExpiredDate.Value.Date < today;

                var isPassed = latestAssessment != null &&
                               latestAssessment.ResultStatus == CompetencyAssessmentResultStatus.Passed &&
                               latestAssessment.IsVerified &&
                               !isExpired;

                var isLevelMet = latestAssessment != null &&
                                 (int)latestAssessment.CompetencyLevel >= (int)requirement.MinimumLevel;

                var status = ResolveMatrixStatus(latestAssessment, isPassed, isLevelMet, isExpired);

                allItems.Add(new WorkforceCompetencyMatrixItemResponse
                {
                    RequirementId = requirement.Id,
                    PositionId = requirement.PositionId,
                    PositionName = requirement.PositionName,
                    CompetencyId = requirement.CompetencyId,
                    CompetencyCode = requirement.CompetencyCode,
                    CompetencyName = requirement.CompetencyName,
                    CompetencyCategory = requirement.CompetencyCategory,
                    IsRequired = requirement.IsRequired,
                    MinimumLevel = requirement.MinimumLevel,
                    IsCertificationRequired = requirement.IsCertificationRequired,
                    IsTrainingRequired = requirement.IsTrainingRequired,
                    LatestAssessmentId = latestAssessment?.Id,
                    LatestCompetencyLevel = latestAssessment?.CompetencyLevel,
                    LatestResultStatus = latestAssessment?.ResultStatus,
                    LatestAssessmentDate = latestAssessment?.AssessmentDate,
                    ExpiredDate = latestAssessment?.ExpiredDate,
                    IsVerified = latestAssessment?.IsVerified ?? false,
                    IsExpired = isExpired,
                    IsPassed = isPassed,
                    IsLevelMet = isLevelMet,
                    Status = status
                });
            }

            response.TotalRequirement = allItems.Count;
            response.CompletedRequirement = allItems.Count(x => x.Status == "Completed");
            response.MissingRequirement = allItems.Count(x => x.Status == "Missing");
            response.NeedTrainingRequirement = allItems.Count(x => x.Status == "NeedTraining");
            response.ExpiredRequirement = allItems.Count(x => x.Status == "Expired");
            response.FailedRequirement = allItems.Count(x => x.Status == "Failed");
            response.NeedVerificationRequirement = allItems.Count(x => x.Status == "NeedVerification");
            response.NotMetRequirement = allItems.Count(x => x.Status == "NotMet");

            if (competencyCategory.HasValue)
                allItems = allItems.Where(x => x.CompetencyCategory == competencyCategory.Value).ToList();

            if (!string.IsNullOrWhiteSpace(matrixStatus))
            {
                var selectedStatus = matrixStatus.Trim();
                allItems = allItems.Where(x => string.Equals(x.Status, selectedStatus, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                allItems = allItems.Where(x =>
                    x.CompetencyCode.ToLower().Contains(keyword) ||
                    x.CompetencyName.ToLower().Contains(keyword) ||
                    x.PositionName.ToLower().Contains(keyword) ||
                    x.Status.ToLower().Contains(keyword)).ToList();
            }

            response.TotalData = allItems.Count;
            response.TotalPage = (int)Math.Ceiling(response.TotalData / (double)pageSize);
            response.Items = allItems
                .OrderBy(x => x.Status == "Missing" ? 0 : 1)
                .ThenBy(x => x.CompetencyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(ApiResponse<WorkforceCompetencyMatrixResponse>.Ok(
                response,
                "Competency matrix workforce berhasil diambil."
            ));
        }

        private IQueryable<WfpCompetencyAssessment> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpCompetencyAssessment> ApplyDateFilter(
            IQueryable<WfpCompetencyAssessment> query,
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
                var today = DateTime.UtcNow.Date;

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

        private static IQueryable<WfpCompetencyAssessment> ApplyStandardFilter(
            IQueryable<WfpCompetencyAssessment> query,
            Guid? competencyId,
            CompetencyCategory? competencyCategory,
            CompetencyLevel? competencyLevel,
            CompetencyAssessmentResultStatus? resultStatus,
            Guid? assessedByUserId,
            bool? isVerified,
            bool? isActive,
            bool? isExpired,
            string? search)
        {
            var today = DateTime.UtcNow.Date;

            if (competencyId.HasValue && competencyId.Value != Guid.Empty)
                query = query.Where(x => x.CompetencyId == competencyId.Value);

            if (competencyCategory.HasValue)
                query = query.Where(x => x.Competency != null && x.Competency.CompetencyCategory == competencyCategory.Value);

            if (competencyLevel.HasValue)
                query = query.Where(x => x.CompetencyLevel == competencyLevel.Value);

            if (resultStatus.HasValue)
                query = query.Where(x => x.ResultStatus == resultStatus.Value);

            if (assessedByUserId.HasValue && assessedByUserId.Value != Guid.Empty)
                query = query.Where(x => x.AssessedByUserId == assessedByUserId.Value);

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isExpired.HasValue)
            {
                query = isExpired.Value
                    ? query.Where(x => x.ExpiredDate.HasValue && x.ExpiredDate.Value.Date < today)
                    : query.Where(x => !x.ExpiredDate.HasValue || x.ExpiredDate.Value.Date >= today);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    (x.Competency != null && x.Competency.CompetencyCode.ToLower().Contains(keyword)) ||
                    (x.Competency != null && x.Competency.CompetencyName.ToLower().Contains(keyword)) ||
                    (x.AssessedByUser != null && x.AssessedByUser.DisplayName.ToLower().Contains(keyword)) ||
                    (x.Notes != null && x.Notes.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpCompetencyAssessment> ApplySorting(
            IQueryable<WfpCompetencyAssessment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "assessmentDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "competencycode" => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyCode : string.Empty)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyCode : string.Empty),
                "competencyname" => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyName : string.Empty),
                "competencycategory" => isDescending
                    ? query.OrderByDescending(x => x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other)
                    : query.OrderBy(x => x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other),
                "competencylevel" => isDescending ? query.OrderByDescending(x => x.CompetencyLevel) : query.OrderBy(x => x.CompetencyLevel),
                "resultstatus" => isDescending ? query.OrderByDescending(x => x.ResultStatus) : query.OrderBy(x => x.ResultStatus),
                "expireddate" => isDescending ? query.OrderByDescending(x => x.ExpiredDate) : query.OrderBy(x => x.ExpiredDate),
                "isverified" => isDescending ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.AssessmentDate) : query.OrderBy(x => x.IsVerified).ThenByDescending(x => x.AssessmentDate),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.AssessmentDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.AssessmentDate),
                _ => isDescending ? query.OrderByDescending(x => x.AssessmentDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.AssessmentDate).ThenBy(x => x.CreateDateTime)
            };
        }

        private async Task<WorkforceCompetencyAssessmentDetailResponse?> BuildAssessmentDetailResponseAsync(Guid workforceProfileId, Guid assessmentId)
        {
            var profile = await GetWorkforceProfileHeaderAsync(workforceProfileId);

            if (profile == null)
                return null;

            var row = await BuildBaseQuery(workforceProfileId)
                .Where(x => x.Id == assessmentId)
                .Select(x => new AssessmentProjection
                {
                    Id = x.Id,
                    WorkforceProfileId = x.WorkforceProfileId,
                    CompetencyId = x.CompetencyId,
                    CompetencyCode = x.Competency != null ? x.Competency.CompetencyCode : string.Empty,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : string.Empty,
                    CompetencyCategory = x.Competency != null ? x.Competency.CompetencyCategory : CompetencyCategory.Other,
                    AssessmentDate = x.AssessmentDate,
                    CompetencyLevel = x.CompetencyLevel,
                    ResultStatus = x.ResultStatus,
                    AssessedByUserId = x.AssessedByUserId,
                    AssessedByUserName = x.AssessedByUser != null ? x.AssessedByUser.DisplayName : null,
                    ExpiredDate = x.ExpiredDate,
                    FilePath = x.FilePath,
                    FileContentType = x.FileContentType,
                    Notes = x.Notes,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime,
                    CreateBy = x.CreateBy,
                    UpdateBy = x.UpdateBy
                })
                .FirstOrDefaultAsync();

            if (row == null)
                return null;

            return MapDetailResponse(row, profile);
        }

        private async Task<(bool IsValid, string Message)> ValidateAssessmentRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            Guid competencyId,
            Guid? assessedByUserId,
            DateTime assessmentDate,
            DateTime? expiredDate,
            CompetencyLevel competencyLevel,
            CompetencyAssessmentResultStatus resultStatus,
            IFormFile? file)
        {
            var profileExists = await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (!profileExists)
                return (false, "Workforce profile tidak ditemukan.");

            if (competencyId == Guid.Empty)
                return (false, "Competency wajib dipilih.");

            var competencyExists = await _dbContext.MstCompetencies
                .AsNoTracking()
                .AnyAsync(x => x.Id == competencyId && x.IsActive && !x.IsDelete);

            if (!competencyExists)
                return (false, "Competency tidak ditemukan atau tidak aktif.");

            if (assessmentDate == default)
                return (false, "AssessmentDate wajib diisi.");

            if (expiredDate.HasValue && expiredDate.Value.Date < assessmentDate.Date)
                return (false, "ExpiredDate tidak boleh lebih kecil dari AssessmentDate.");

            if (!Enum.IsDefined(typeof(CompetencyLevel), competencyLevel))
                return (false, "CompetencyLevel tidak valid. Gunakan filters/metadata untuk melihat pilihan enum.");

            if (!Enum.IsDefined(typeof(CompetencyAssessmentResultStatus), resultStatus))
                return (false, "ResultStatus tidak valid. Gunakan filters/metadata untuk melihat pilihan enum.");

            if (assessedByUserId.HasValue && assessedByUserId.Value != Guid.Empty)
            {
                var userExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == assessedByUserId.Value && x.IsActive);

                if (!userExists)
                    return (false, "AssessedByUser tidak ditemukan atau tidak aktif.");
            }

            if (file != null)
            {
                var fileValidation = ValidateFile(file);

                if (!fileValidation.IsValid)
                    return (false, fileValidation.ErrorMessage ?? "File tidak valid.");
            }

            var duplicateQuery = _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.CompetencyId == competencyId &&
                    x.AssessmentDate.Date == assessmentDate.Date &&
                    !x.IsDelete);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Assessment untuk competency dan tanggal tersebut sudah terdaftar pada workforce profile ini.");

            return (true, string.Empty);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
                return (false, "File competency assessment kosong.");

            if (file.Length > MaxFileSizeBytes)
                return (false, "Ukuran file competency assessment maksimal 10 MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, JPEG, PNG, DOC, DOCX, XLS, atau XLSX.");

            return (true, null);
        }

        private async Task<(string FilePath, string? ContentType)> SaveAssessmentFileAsync(Guid workforceProfileId, IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-competency-assessments", workforceProfileId.ToString());
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

        private async Task<FileResolveResult> GetAssessmentFileAsync(Guid workforceProfileId, Guid assessmentId)
        {
            var assessment = await _dbContext.WfpCompetencyAssessments
                .AsNoTracking()
                .Where(x => x.Id == assessmentId && x.WorkforceProfileId == workforceProfileId && !x.IsDelete)
                .Select(x => new
                {
                    x.FilePath,
                    x.FileContentType,
                    CompetencyName = x.Competency != null ? x.Competency.CompetencyName : "competency-assessment"
                })
                .FirstOrDefaultAsync();

            if (assessment == null)
                return FileResolveResult.Invalid("Competency assessment tidak ditemukan.");

            if (string.IsNullOrWhiteSpace(assessment.FilePath))
                return FileResolveResult.Invalid("File competency assessment belum tersedia.");

            var physicalPath = ResolvePhysicalPath(assessment.FilePath);

            if (!System.IO.File.Exists(physicalPath))
                return FileResolveResult.Invalid("File fisik competency assessment tidak ditemukan di server.");

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
                contentType = assessment.FileContentType ?? "application/octet-stream";

            var extension = Path.GetExtension(physicalPath);
            var fileName = Path.GetFileName(physicalPath);
            var downloadName = $"COMPETENCY_ASSESSMENT_{SanitizeFileName(assessment.CompetencyName)}{extension}";

            return FileResolveResult.Valid(physicalPath, contentType, fileName, downloadName);
        }

        private void DeletePhysicalFileIfExists(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var physicalPath = ResolvePhysicalPath(filePath);

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }

        private string ResolvePhysicalPath(string filePath)
        {
            var storage = GetFileStoragePaths();
            var normalizedFilePath = filePath.Replace("\\", "/").Trim();
            var publicPrefix = storage.PublicRequestPath.TrimEnd('/');

            string relativePath;

            if (normalizedFilePath.StartsWith(publicPrefix + "/", StringComparison.OrdinalIgnoreCase))
                relativePath = normalizedFilePath[(publicPrefix.Length + 1)..];
            else
                relativePath = normalizedFilePath.TrimStart('/');

            var fullPath = Path.GetFullPath(Path.Combine(storage.RootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())));
            var rootPath = Path.GetFullPath(storage.RootPath);

            if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Path file tidak valid.");

            return fullPath;
        }

        private (string RootPath, string PublicRequestPath) GetFileStoragePaths()
        {
            var publicRequestPath = _configuration["FileStorage:PublicRequestPath"] ?? "/uploads";

            if (!publicRequestPath.StartsWith('/'))
                publicRequestPath = "/" + publicRequestPath;

            publicRequestPath = publicRequestPath.TrimEnd('/');

            var configuredRoot = _configuration["FileStorage:UploadRootPath"];

            if (!string.IsNullOrWhiteSpace(configuredRoot))
            {
                Directory.CreateDirectory(configuredRoot);
                return (configuredRoot, publicRequestPath);
            }

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");

            var rootPath = Path.Combine(webRootPath, publicRequestPath.TrimStart('/'));
            Directory.CreateDirectory(rootPath);

            return (rootPath, publicRequestPath);
        }

        private WorkforceCompetencyAssessmentResponse MapResponse(AssessmentProjection row, WorkforceProfileHeader profile)
        {
            var today = DateTime.UtcNow.Date;
            var hasFile = !string.IsNullOrWhiteSpace(row.FilePath);

            return new WorkforceCompetencyAssessmentResponse
            {
                Id = row.Id,
                WorkforceProfileId = row.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                CompetencyId = row.CompetencyId,
                CompetencyCode = row.CompetencyCode,
                CompetencyName = row.CompetencyName,
                CompetencyCategory = row.CompetencyCategory,
                AssessmentDate = row.AssessmentDate,
                CompetencyLevel = row.CompetencyLevel,
                ResultStatus = row.ResultStatus,
                AssessedByUserId = row.AssessedByUserId,
                AssessedByUserName = row.AssessedByUserName,
                ExpiredDate = row.ExpiredDate,
                IsExpired = row.ExpiredDate.HasValue && row.ExpiredDate.Value.Date < today,
                FilePath = row.FilePath,
                FileContentType = row.FileContentType,
                FileName = hasFile ? Path.GetFileName(row.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(row.WorkforceProfileId, row.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(row.WorkforceProfileId, row.Id, "download") : null,
                HasFile = hasFile,
                Notes = row.Notes,
                IsVerified = row.IsVerified,
                IsActive = row.IsActive,
                CreateDateTime = row.CreateDateTime
            };
        }

        private WorkforceCompetencyAssessmentDetailResponse MapDetailResponse(AssessmentProjection row, WorkforceProfileHeader profile)
        {
            var response = MapResponse(row, profile);

            return new WorkforceCompetencyAssessmentDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                UserType = response.UserType,
                CompetencyId = response.CompetencyId,
                CompetencyCode = response.CompetencyCode,
                CompetencyName = response.CompetencyName,
                CompetencyCategory = response.CompetencyCategory,
                AssessmentDate = response.AssessmentDate,
                CompetencyLevel = response.CompetencyLevel,
                ResultStatus = response.ResultStatus,
                AssessedByUserId = response.AssessedByUserId,
                AssessedByUserName = response.AssessedByUserName,
                ExpiredDate = response.ExpiredDate,
                IsExpired = response.IsExpired,
                FilePath = response.FilePath,
                FileContentType = response.FileContentType,
                FileName = response.FileName,
                FilePreviewUrl = response.FilePreviewUrl,
                FileDownloadUrl = response.FileDownloadUrl,
                HasFile = response.HasFile,
                Notes = response.Notes,
                IsVerified = response.IsVerified,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                UpdateDateTime = row.UpdateDateTime,
                CreateBy = row.CreateBy,
                UpdateBy = row.UpdateBy
            };
        }

        private async Task<WorkforceProfileHeader?> GetWorkforceProfileHeaderAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .Where(x => x.Id == workforceProfileId && !x.IsDelete)
                .Select(x => new WorkforceProfileHeader
                {
                    Id = x.Id,
                    ProfileCode = x.ProfileCode,
                    DisplayName = x.DisplayName,
                    UserType = x.UserType,
                    PrimaryPositionId = x.PrimaryPositionId,
                    PrimaryPositionName = x.PrimaryPosition != null ? x.PrimaryPosition.PositionName : null
                })
                .FirstOrDefaultAsync();
        }

        private async Task<bool> ProfileExistsAsync(Guid workforceProfileId)
        {
            return await _dbContext.MstWorkforceProfiles
                .AsNoTracking()
                .AnyAsync(x => x.Id == workforceProfileId && !x.IsDelete);
        }

        private static List<WorkforceCompetencyAssessmentCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<WorkforceCompetencyAssessmentCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }

        private static List<WorkforceCompetencyAssessmentEnumOptionResponse> BuildEnumOptions<TEnum>() where TEnum : struct, Enum
        {
            return Enum.GetValues<TEnum>()
                .Select(x => new WorkforceCompetencyAssessmentEnumOptionResponse
                {
                    Value = Convert.ToInt32(x),
                    Name = x.ToString(),
                    Label = x.ToString(),
                    Description = ResolveEnumDescription(x.ToString())
                })
                .ToList();
        }

        private static List<WorkforceCompetencyAssessmentMatrixStatusOptionResponse> BuildMatrixStatusOptions()
        {
            return new List<WorkforceCompetencyAssessmentMatrixStatusOptionResponse>
            {
                new() { Value = "Completed", Label = "Completed", Description = "Assessment passed, verified, belum expired, dan level memenuhi minimum." },
                new() { Value = "Missing", Label = "Missing", Description = "Belum ada assessment untuk competency requirement." },
                new() { Value = "NeedTraining", Label = "Need Training", Description = "Assessment menunjukkan perlu training." },
                new() { Value = "Expired", Label = "Expired", Description = "Assessment sudah melewati expired date." },
                new() { Value = "Failed", Label = "Failed", Description = "Assessment failed." },
                new() { Value = "NeedVerification", Label = "Need Verification", Description = "Assessment ada tetapi belum diverifikasi." },
                new() { Value = "NotMet", Label = "Not Met", Description = "Assessment ada tetapi level belum memenuhi minimum requirement." }
            };
        }

        private static List<WorkforceCompetencyAssessmentGuideResponse> BuildFrontendGuide()
        {
            return new List<WorkforceCompetencyAssessmentGuideResponse>
            {
                new() { FieldName = "CompetencyId", Label = "Kompetensi", Description = "Dropdown dari master competency options.", Example = "Pilih kompetensi sesuai requirement position." },
                new() { FieldName = "CompetencyLevel", Label = "Level Kompetensi", Description = "Gunakan enum level dari metadata.", Example = "None, Basic, Intermediate, Advanced, Expert." },
                new() { FieldName = "ResultStatus", Label = "Hasil Assessment", Description = "Gunakan enum result status dari metadata.", Example = "Passed, Failed, NeedTraining, NotAssessed." },
                new() { FieldName = "AssessedByUserId", Label = "Assessor", Description = "User penilai. Nullable jika belum ditentukan.", Example = "User HR, kepala unit, komite medik, atau supervisor." },
                new() { FieldName = "File", Label = "File Assessment", Description = "Upload hasil assessment. Untuk preview gunakan FilePreviewUrl dari response.", Example = "PDF/JPG/PNG/DOCX maksimal 10 MB." }
            };
        }

        private static string ResolveEnumDescription(string name)
        {
            return name switch
            {
                "None" => "Belum memiliki level kompetensi.",
                "Basic" => "Level dasar.",
                "Intermediate" => "Level menengah.",
                "Advanced" => "Level lanjutan.",
                "Expert" => "Level ahli.",
                "NotAssessed" => "Belum dilakukan assessment.",
                "Passed" => "Lulus assessment.",
                "Failed" => "Tidak lulus assessment.",
                "NeedTraining" => "Perlu training atau pengembangan lanjutan.",
                _ => name
            };
        }

        private static string ResolveMatrixStatus(
            WfpCompetencyAssessment? latestAssessment,
            bool isPassed,
            bool isLevelMet,
            bool isExpired)
        {
            if (latestAssessment == null)
                return "Missing";

            if (isExpired)
                return "Expired";

            if (latestAssessment.ResultStatus == CompetencyAssessmentResultStatus.NeedTraining)
                return "NeedTraining";

            if (latestAssessment.ResultStatus == CompetencyAssessmentResultStatus.Failed)
                return "Failed";

            if (!latestAssessment.IsVerified)
                return "NeedVerification";

            if (isPassed && isLevelMet)
                return "Completed";

            return "NotMet";
        }

        private string BuildEndpointUrl(Guid workforceProfileId, Guid assessmentId, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/competency-assessments/{assessmentId}/{action}";

            return $"{Request.Scheme}://{Request.Host}{path}";
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

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

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
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
                ? "competency-assessment"
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

        private sealed class WorkforceProfileHeader
        {
            public Guid Id { get; set; }
            public string ProfileCode { get; set; } = string.Empty;
            public string DisplayName { get; set; } = string.Empty;
            public UserType UserType { get; set; }
            public Guid? PrimaryPositionId { get; set; }
            public string? PrimaryPositionName { get; set; }
        }

        private sealed class AssessmentProjection
        {
            public Guid Id { get; set; }
            public Guid WorkforceProfileId { get; set; }
            public Guid CompetencyId { get; set; }
            public string CompetencyCode { get; set; } = string.Empty;
            public string CompetencyName { get; set; } = string.Empty;
            public CompetencyCategory CompetencyCategory { get; set; }
            public DateTime AssessmentDate { get; set; }
            public CompetencyLevel CompetencyLevel { get; set; }
            public CompetencyAssessmentResultStatus ResultStatus { get; set; }
            public Guid? AssessedByUserId { get; set; }
            public string? AssessedByUserName { get; set; }
            public DateTime? ExpiredDate { get; set; }
            public string? FilePath { get; set; }
            public string? FileContentType { get; set; }
            public string? Notes { get; set; }
            public bool IsVerified { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreateDateTime { get; set; }
            public DateTime? UpdateDateTime { get; set; }
            public Guid CreateBy { get; set; }
            public Guid UpdateBy { get; set; }
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
