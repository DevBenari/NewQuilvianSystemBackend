using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceTrainingRecordPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceTrainingRecordResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/training-records")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Training Record",
        AreaName = "Corporate",
        ControllerName = "WorkforceTrainingRecord",
        Description = "Workforce training record management",
        SortOrder = 25
    )]
    [Tags("Corporate / Human Resource / Workforce / Training Record")]
    public class WorkforceTrainingRecordController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.TrainingRecord";
        private const string CodePrefix = "TRN-RSMMC-";
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

        public WorkforceTrainingRecordController(
            ApplicationDbContext dbContext,
            LoggerService loggerService,
            IWebHostEnvironment environment)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
            _environment = environment;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTrainingRecordFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Melihat metadata filter training record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
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

            var result = new WorkforceTrainingRecordFilterMetadataResponse
            {
                DefaultFilter = new WorkforceTrainingRecordDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceTrainingRecordCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" },
                    new() { Value = "custom", Label = "Custom" }
                },
                SortOptions = new List<WorkforceTrainingRecordSortOptionResponse>
                {
                    new() { Value = "startDate", Label = "Tanggal mulai" },
                    new() { Value = "endDate", Label = "Tanggal selesai" },
                    new() { Value = "trainingType", Label = "Tipe training" },
                    new() { Value = "trainingName", Label = "Nama training" },
                    new() { Value = "organizer", Label = "Organizer" },
                    new() { Value = "creditPoint", Label = "Credit point" },
                    new() { Value = "isVerified", Label = "Status verifikasi" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                TrainingTypeOptions = BuildTrainingTypeOptions(),
                CodeInfo = new WorkforceTrainingRecordCodeInfoResponse(),
                FileUploadInfo = new WorkforceTrainingRecordFileUploadInfoResponse(),
                FrontendGuide = new WorkforceTrainingRecordFrontendGuideResponse()
            };

            return Ok(ApiResponse<WorkforceTrainingRecordFilterMetadataResponse>.Ok(
                result,
                "Metadata filter training record workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTrainingRecordSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Melihat ringkasan training record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
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

            var result = new WorkforceTrainingRecordSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalTrainingRecord = await query.CountAsync(),
                ActiveTrainingRecord = await query.CountAsync(x => x.IsActive),
                InactiveTrainingRecord = await query.CountAsync(x => !x.IsActive),
                VerifiedTrainingRecord = await query.CountAsync(x => x.IsVerified),
                UnverifiedTrainingRecord = await query.CountAsync(x => !x.IsVerified),
                TrainingWithFile = await query.CountAsync(x => !string.IsNullOrWhiteSpace(x.FilePath)),
                TotalCreditPoint = await query
                    .Where(x => x.IsActive)
                    .SumAsync(x => x.CreditPoint)
            };

            return Ok(ApiResponse<WorkforceTrainingRecordSummaryResponse>.Ok(
                result,
                "Ringkasan training record workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceTrainingRecordPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Melihat training record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetTrainingRecords(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] WorkforceTrainingRecordType? trainingType,
            [FromQuery] bool? isVerified,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "startDate",
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
            query = ApplyStandardFilter(query, trainingType, isVerified, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(x => MapResponse(x, profile))
                .ToList();

            var result = new ResponseWorkforceTrainingRecordPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceTrainingRecordPagedResult>.Ok(
                result,
                "Data training record workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTrainingRecordOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Melihat pilihan training record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] WorkforceTrainingRecordType? trainingType,
            [FromQuery] bool? isVerified,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
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
            query = ApplyStandardFilter(
                query,
                trainingType,
                isVerified,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.StartDate)
                .ThenBy(x => x.TrainingName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(x => new WorkforceTrainingRecordOptionResponse
                {
                    Id = x.Id,
                    RequirementCode = x.RequirementCode,
                    TrainingType = x.TrainingType,
                    TrainingName = x.TrainingName,
                    Organizer = x.Organizer,
                    StartDate = x.StartDate,
                    EndDate = x.EndDate,
                    CreditPoint = x.CreditPoint,
                    IsVerified = x.IsVerified,
                    IsActive = x.IsActive
                })
                .ToList();

            var result = new WorkforceTrainingRecordOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceTrainingRecordOptionPagedResponse>.Ok(
                result,
                "Data pilihan training record workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTrainingRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Melihat detail training record workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> GetTrainingRecordById(Guid workforceProfileId, Guid id)
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
                    "Training record workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceTrainingRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Detail training record workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTrainingRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Workforce Training Record",
            Description = "Menambah training record workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceTrainingRecord", "Create")]
        public async Task<IActionResult> CreateTrainingRecord(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceTrainingRecordRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateCreateOrUpdateRequestAsync(
                workforceProfileId,
                null,
                request.TrainingType,
                request.TrainingName,
                request.StartDate,
                request.EndDate,
                request.CertificateNumber,
                request.CreditPoint,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data training record tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveTrainingFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpTrainingRecord
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                RequirementCode = await GenerateRequirementCodeAsync(),
                TrainingType = request.TrainingType.ToString(),
                TrainingName = request.TrainingName.Trim(),
                Organizer = NormalizeNullableText(request.Organizer),
                Location = NormalizeNullableText(request.Location),
                StartDate = request.StartDate.Date,
                EndDate = request.EndDate?.Date,
                CertificateNumber = NormalizeNullableText(request.CertificateNumber),
                CreditPoint = request.CreditPoint,
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

            _dbContext.Set<WfpTrainingRecord>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceTrainingRecord.CreateTrainingRecord",
                "Training record workforce berhasil dibuat.",
                new
                {
                    workforceProfileId,
                    entity.Id,
                    entity.RequirementCode,
                    entity.TrainingType,
                    entity.TrainingName
                }
            );

            return Ok(ApiResponse<WorkforceTrainingRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Training record workforce berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTrainingRecordDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Training Record",
            Description = "Mengubah training record workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> UpdateTrainingRecord(
            Guid workforceProfileId,
            Guid id,
            [FromForm] UpdateWorkforceTrainingRecordRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpTrainingRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Training record workforce tidak ditemukan."
                ));
            }

            var validation = await ValidateCreateOrUpdateRequestAsync(
                workforceProfileId,
                id,
                request.TrainingType,
                request.TrainingName,
                request.StartDate,
                request.EndDate,
                request.CertificateNumber,
                request.CreditPoint,
                request.File);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data training record tidak valid."
                ));
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);

                var savedFile = await SaveTrainingFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveTrainingFileAsync(workforceProfileId, request.File);

                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }

            if (string.IsNullOrWhiteSpace(entity.RequirementCode))
            {
                entity.RequirementCode = await GenerateRequirementCodeAsync();
            }

            entity.TrainingType = request.TrainingType.ToString();
            entity.TrainingName = request.TrainingName.Trim();
            entity.Organizer = NormalizeNullableText(request.Organizer);
            entity.Location = NormalizeNullableText(request.Location);
            entity.StartDate = request.StartDate.Date;
            entity.EndDate = request.EndDate?.Date;
            entity.CertificateNumber = NormalizeNullableText(request.CertificateNumber);
            entity.CreditPoint = request.CreditPoint;
            entity.IsVerified = request.IsVerified;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<WorkforceTrainingRecordDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Training record workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Training Record",
            Description = "Mengubah status training record workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> UpdateTrainingRecordStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceTrainingRecordStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpTrainingRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Training record workforce tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status training record workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/verify")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Verify Workforce Training Record",
            Description = "Verifikasi training record workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> VerifyTrainingRecord(
            Guid workforceProfileId,
            Guid id,
            [FromBody] VerifyWorkforceTrainingRecordRequest request)
        {
            var entity = await _dbContext.Set<WfpTrainingRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Training record workforce tidak ditemukan."
                ));
            }

            entity.IsVerified = request.IsVerified;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                request.IsVerified
                    ? "Training record workforce berhasil diverifikasi."
                    : "Verifikasi training record workforce berhasil dibatalkan."
            ));
        }

        [HttpGet("{id:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Preview file training record workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> PreviewTrainingRecord(
            Guid workforceProfileId,
            Guid id)
        {
            var fileResult = await ResolveTrainingRecordFileAsync(workforceProfileId, id);

            if (!fileResult.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileResult.ErrorMessage ?? "File training record workforce tidak ditemukan."
                ));
            }

            Response.Headers["Content-Disposition"] = "inline";

            return PhysicalFile(
                fileResult.PhysicalPath!,
                fileResult.ContentType!,
                enableRangeProcessing: true);
        }

        [HttpGet("{id:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Training Record",
            Description = "Download file training record workforce",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceTrainingRecord", "Read")]
        public async Task<IActionResult> DownloadTrainingRecord(
            Guid workforceProfileId,
            Guid id)
        {
            var fileResult = await ResolveTrainingRecordFileAsync(workforceProfileId, id);

            if (!fileResult.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileResult.ErrorMessage ?? "File training record workforce tidak ditemukan."
                ));
            }

            return PhysicalFile(
                fileResult.PhysicalPath!,
                fileResult.ContentType!,
                fileResult.DownloadName!,
                enableRangeProcessing: true);
        }

        [HttpDelete("{id:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Training Record",
            Description = "Menghapus file training record workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceTrainingRecord", "Update")]
        public async Task<IActionResult> DeleteTrainingRecordFile(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpTrainingRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Training record workforce tidak ditemukan."
                ));
            }

            if (string.IsNullOrWhiteSpace(entity.FilePath))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "File training record workforce belum tersedia."
                ));
            }

            DeletePhysicalFileIfExists(entity.FilePath);

            entity.FilePath = null;
            entity.FileContentType = null;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "File training record workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Training Record",
            Description = "Menghapus training record workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 7
        )]
        [AccessPermission("WorkforceTrainingRecord", "Delete")]
        public async Task<IActionResult> DeleteTrainingRecord(
            Guid workforceProfileId,
            Guid id)
        {
            var entity = await _dbContext.Set<WfpTrainingRecord>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Training record workforce tidak ditemukan."
                ));
            }

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Training record workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpTrainingRecord> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpTrainingRecord> ApplyDateFilter(
            IQueryable<WfpTrainingRecord> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (!string.IsNullOrWhiteSpace(customPeriod) &&
                !string.Equals(customPeriod.Trim(), "custom", StringComparison.OrdinalIgnoreCase) &&
                !startDate.HasValue &&
                !endDate.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        startDate = today;
                        endDate = today;
                        break;

                    case "last7days":
                        startDate = today.AddDays(-6);
                        endDate = today;
                        break;

                    case "thismonth":
                        startDate = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        break;

                    case "lastmonth":
                        var thisMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        startDate = thisMonthStart.AddMonths(-1);
                        endDate = thisMonthStart.AddDays(-1);
                        break;
                }
            }

            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.StartDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.StartDate < end);
            }

            return query;
        }

        private static IQueryable<WfpTrainingRecord> ApplyStandardFilter(
            IQueryable<WfpTrainingRecord> query,
            WorkforceTrainingRecordType? trainingType,
            bool? isVerified,
            bool? isActive,
            string? search)
        {
            if (trainingType.HasValue && trainingType.Value != WorkforceTrainingRecordType.Unknown)
            {
                var typeName = trainingType.Value.ToString();
                query = query.Where(x => x.TrainingType == typeName);
            }

            if (isVerified.HasValue)
                query = query.Where(x => x.IsVerified == isVerified.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.TrainingName.ToLower().Contains(keyword) ||
                    x.TrainingType.ToLower().Contains(keyword) ||
                    (x.Organizer != null && x.Organizer.ToLower().Contains(keyword)) ||
                    (x.Location != null && x.Location.ToLower().Contains(keyword)) ||
                    (x.CertificateNumber != null && x.CertificateNumber.ToLower().Contains(keyword)) ||
                    (x.RequirementCode != null && x.RequirementCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpTrainingRecord> ApplySorting(
            IQueryable<WfpTrainingRecord> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "startDate").Trim().ToLowerInvariant() switch
            {
                "startdate" => isDescending ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate),
                "enddate" => isDescending ? query.OrderByDescending(x => x.EndDate) : query.OrderBy(x => x.EndDate),
                "trainingtype" => isDescending ? query.OrderByDescending(x => x.TrainingType) : query.OrderBy(x => x.TrainingType),
                "trainingname" => isDescending ? query.OrderByDescending(x => x.TrainingName) : query.OrderBy(x => x.TrainingName),
                "organizer" => isDescending ? query.OrderByDescending(x => x.Organizer) : query.OrderBy(x => x.Organizer),
                "creditpoint" => isDescending ? query.OrderByDescending(x => x.CreditPoint) : query.OrderBy(x => x.CreditPoint),
                "isverified" => isDescending
                    ? query.OrderByDescending(x => x.IsVerified).ThenByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.IsVerified).ThenByDescending(x => x.StartDate),
                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.StartDate)
                    : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.StartDate),
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => isDescending
                    ? query.OrderByDescending(x => x.StartDate).ThenBy(x => x.TrainingName)
                    : query.OrderBy(x => x.StartDate).ThenBy(x => x.TrainingName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateOrUpdateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeId,
            WorkforceTrainingRecordType trainingType,
            string? trainingName,
            DateTime startDate,
            DateTime? endDate,
            string? certificateNumber,
            decimal creditPoint,
            IFormFile? file)
        {
            if (!Enum.IsDefined(typeof(WorkforceTrainingRecordType), trainingType) ||
                trainingType == WorkforceTrainingRecordType.Unknown)
            {
                return (false, "TrainingType wajib dipilih dan tidak boleh Unknown.");
            }

            if (string.IsNullOrWhiteSpace(trainingName))
            {
                return (false, "TrainingName wajib diisi.");
            }

            if (startDate == default)
            {
                return (false, "StartDate wajib diisi.");
            }

            if (endDate.HasValue && endDate.Value.Date < startDate.Date)
            {
                return (false, "EndDate tidak boleh lebih kecil dari StartDate.");
            }

            if (creditPoint < 0)
            {
                return (false, "CreditPoint tidak boleh kurang dari 0.");
            }

            var normalizedCertificateNumber = NormalizeNullableText(certificateNumber);

            if (!string.IsNullOrWhiteSpace(normalizedCertificateNumber))
            {
                var duplicateQuery = _dbContext.Set<WfpTrainingRecord>()
                    .AsNoTracking()
                    .Where(x =>
                        x.WorkforceProfileId == workforceProfileId &&
                        x.CertificateNumber == normalizedCertificateNumber &&
                        !x.IsDelete);

                if (excludeId.HasValue)
                    duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateQuery.AnyAsync())
                    return (false, "Nomor sertifikat training sudah terdaftar pada workforce profile ini.");
            }

            if (file != null)
            {
                var fileValidation = ValidateFile(file);

                if (!fileValidation.IsValid)
                    return fileValidation;
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
            {
                return (false, "File training kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file training maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, PNG, DOC, DOCX, XLS, atau XLSX.");
            }

            return (true, null);
        }

        private async Task<string> GenerateRequirementCodeAsync()
        {
            var latestCodes = await _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .Where(x =>
                    x.RequirementCode != null &&
                    x.RequirementCode.StartsWith(CodePrefix) &&
                    !x.IsDelete)
                .Select(x => x.RequirementCode!)
                .ToListAsync();

            var maxNumber = 0;

            foreach (var code in latestCodes)
            {
                var numberText = code.Replace(CodePrefix, string.Empty);

                if (int.TryParse(numberText, out var number) && number > maxNumber)
                {
                    maxNumber = number;
                }
            }

            return $"{CodePrefix}{maxNumber + 1:00000}";
        }

        private async Task<(bool IsValid, string? ErrorMessage, string? PhysicalPath, string? ContentType, string? DownloadName)> ResolveTrainingRecordFileAsync(
            Guid workforceProfileId,
            Guid id)
        {
            var training = await _dbContext.Set<WfpTrainingRecord>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (training == null)
            {
                return (false, "Training record workforce tidak ditemukan.", null, null, null);
            }

            if (string.IsNullOrWhiteSpace(training.FilePath))
            {
                return (false, "File training record workforce belum tersedia.", null, null, null);
            }

            var physicalPath = ResolvePhysicalPath(training.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return (false, "File fisik training tidak ditemukan di server.", null, null, null);
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = training.FileContentType ?? "application/octet-stream";
            }

            var safeTrainingName = training.TrainingName
                .Replace("/", "-")
                .Replace("\\", "-");

            var downloadName = $"{training.RequirementCode ?? "Training"}_{safeTrainingName}{Path.GetExtension(physicalPath)}";

            return (true, null, physicalPath, contentType, downloadName);
        }

        private async Task<(string FilePath, string? ContentType)> SaveTrainingFileAsync(
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

            var relativeFolder = Path.Combine(
                "uploads",
                "workforce-training-records",
                workforceProfileId.ToString());

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
                return;

            var physicalPath = ResolvePhysicalPath(filePath);

            if (System.IO.File.Exists(physicalPath))
                System.IO.File.Delete(physicalPath);
        }

        private string ResolvePhysicalPath(string filePath)
        {
            var normalizedPath = filePath
                .TrimStart('/')
                .Replace("/", Path.DirectorySeparatorChar.ToString());

            var webRootPath = _environment.WebRootPath;

            if (string.IsNullOrWhiteSpace(webRootPath))
            {
                webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
            }

            return Path.Combine(webRootPath, normalizedPath);
        }

        private WorkforceTrainingRecordResponse MapResponse(
            WfpTrainingRecord entity,
            MstWorkforceProfile profile)
        {
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);
            var basePath = $"/api/v1/corporate/human-resource/workforce-profiles/{entity.WorkforceProfileId}/training-records/{entity.Id}";

            return new WorkforceTrainingRecordResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                RequirementCode = entity.RequirementCode,
                TrainingType = entity.TrainingType,
                TrainingName = entity.TrainingName,
                Organizer = entity.Organizer,
                Location = entity.Location,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                CertificateNumber = entity.CertificateNumber,
                CreditPoint = entity.CreditPoint,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                HasFile = hasFile,
                FilePreviewUrl = hasFile ? $"{basePath}/preview" : null,
                FileDownloadUrl = hasFile ? $"{basePath}/download" : null,
                IsVerified = entity.IsVerified,
                IsActive = entity.IsActive,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private WorkforceTrainingRecordDetailResponse MapDetailResponse(
            WfpTrainingRecord entity,
            MstWorkforceProfile profile)
        {
            var response = MapResponse(entity, profile);

            return new WorkforceTrainingRecordDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                RequirementCode = response.RequirementCode,
                TrainingType = response.TrainingType,
                TrainingName = response.TrainingName,
                Organizer = response.Organizer,
                Location = response.Location,
                StartDate = response.StartDate,
                EndDate = response.EndDate,
                CertificateNumber = response.CertificateNumber,
                CreditPoint = response.CreditPoint,
                FilePath = response.FilePath,
                FileContentType = response.FileContentType,
                HasFile = response.HasFile,
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

        private static List<WorkforceTrainingRecordTypeOptionResponse> BuildTrainingTypeOptions()
        {
            return new List<WorkforceTrainingRecordTypeOptionResponse>
            {
                new()
                {
                    Value = WorkforceTrainingRecordType.Seminar,
                    ValueName = WorkforceTrainingRecordType.Seminar.ToString(),
                    Label = "Seminar",
                    Description = "Kegiatan seminar ilmiah/profesional.",
                    TrainingNameExamples = new List<string>
                    {
                        "Seminar Keselamatan Pasien",
                        "Seminar Manajemen Risiko Klinis",
                        "Seminar Update Regulasi Kesehatan"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.Workshop,
                    ValueName = WorkforceTrainingRecordType.Workshop.ToString(),
                    Label = "Workshop",
                    Description = "Pelatihan praktik singkat dengan aktivitas hands-on.",
                    TrainingNameExamples = new List<string>
                    {
                        "Workshop PMKP",
                        "Workshop Clinical Pathway",
                        "Workshop Komunikasi Efektif"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.Course,
                    ValueName = WorkforceTrainingRecordType.Course.ToString(),
                    Label = "Course",
                    Description = "Kursus/pelatihan formal dengan materi terstruktur.",
                    TrainingNameExamples = new List<string>
                    {
                        "BTCLS",
                        "BLS",
                        "ACLS",
                        "ATLS",
                        "PPGD"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.Webinar,
                    ValueName = WorkforceTrainingRecordType.Webinar.ToString(),
                    Label = "Webinar",
                    Description = "Pelatihan/seminar online.",
                    TrainingNameExamples = new List<string>
                    {
                        "Webinar Pencegahan Infeksi",
                        "Webinar Rekam Medis Elektronik",
                        "Webinar Patient Safety"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.InHouseTraining,
                    ValueName = WorkforceTrainingRecordType.InHouseTraining.ToString(),
                    Label = "In House Training",
                    Description = "Pelatihan internal yang diselenggarakan rumah sakit.",
                    TrainingNameExamples = new List<string>
                    {
                        "IHT APAR",
                        "IHT Hand Hygiene",
                        "IHT Code Blue",
                        "IHT Orientasi Karyawan Baru"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.Conference,
                    ValueName = WorkforceTrainingRecordType.Conference.ToString(),
                    Label = "Conference",
                    Description = "Konferensi/kongres profesi.",
                    TrainingNameExamples = new List<string>
                    {
                        "Kongres Perhimpunan Dokter Spesialis",
                        "Hospital Expo Conference",
                        "Nursing Conference"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.E_Learning,
                    ValueName = WorkforceTrainingRecordType.E_Learning.ToString(),
                    Label = "E-Learning",
                    Description = "Pelatihan mandiri melalui LMS atau platform digital.",
                    TrainingNameExamples = new List<string>
                    {
                        "E-Learning K3RS",
                        "E-Learning Etik dan Kepatuhan",
                        "E-Learning Privacy Data Pasien"
                    }
                },
                new()
                {
                    Value = WorkforceTrainingRecordType.Other,
                    ValueName = WorkforceTrainingRecordType.Other.ToString(),
                    Label = "Other",
                    Description = "Jenis training lainnya.",
                    TrainingNameExamples = new List<string>
                    {
                        "Pelatihan lainnya"
                    }
                }
            };
        }

        private async Task<MstWorkforceProfile?> GetProfileAsync(Guid workforceProfileId)
        {
            return await _dbContext.Set<MstWorkforceProfile>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
