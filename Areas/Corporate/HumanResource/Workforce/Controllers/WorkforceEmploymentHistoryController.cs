using Microsoft.AspNetCore.Authorization;
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

using ResponseWorkforceEmploymentHistoryPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceEmploymentHistoryResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/employment-histories")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_WORKFORCE",
        moduleName: "Human Resource Workforce",
        displayName: "Workforce Employment History",
        AreaName = "Corporate",
        ControllerName = "WorkforceEmploymentHistory",
        Description = "Corporate human resource workforce employment history",
        SortOrder = 11
    )]
    [Tags("Corporate / Human Resource / Workforce / Employment History")]
    public class WorkforceEmploymentHistoryController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.EmploymentHistory";
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

        public WorkforceEmploymentHistoryController(
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
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat metadata filter employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
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

            var result = new WorkforceEmploymentHistoryFilterMetadataResponse
            {
                DefaultFilter = new WorkforceEmploymentHistoryDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceEmploymentHistoryCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceEmploymentHistorySortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "effectiveDate", Label = "Tanggal efektif" },
                    new() { Value = "endDate", Label = "Tanggal akhir" },
                    new() { Value = "historyType", Label = "Jenis riwayat" },
                    new() { Value = "oldDepartmentName", Label = "Department lama" },
                    new() { Value = "newDepartmentName", Label = "Department baru" },
                    new() { Value = "oldPositionName", Label = "Jabatan lama" },
                    new() { Value = "newPositionName", Label = "Jabatan baru" },
                    new() { Value = "oldStatus", Label = "Status lama" },
                    new() { Value = "newStatus", Label = "Status baru" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                HistoryTypeOptions = BuildHistoryTypeOptions(),
                StatusExamples = new List<WorkforceEmploymentStatusExampleResponse>
                {
                    new() { Category = "Employment Status", Values = new List<string> { "Probation", "Contract", "Permanent", "PartTime", "Internship", "Terminated", "Resigned", "Retired" } },
                    new() { Category = "Clinical Assignment", Values = new List<string> { "ActivePractice", "NonPractice", "OnLeave", "Suspended", "CredentialPending" } }
                },
                FrontendGuide = new List<string>
                {
                    "Gunakan HistoryType sebagai enum/dropdown, bukan text bebas.",
                    "Gunakan old/new department dan old/new position untuk riwayat mutasi/promosi/perubahan jabatan.",
                    "Gunakan OldStatus dan NewStatus untuk perubahan status kerja seperti Probation ke Permanent.",
                    "Endpoint ini hanya untuk WfpEmploymentHistory. ContractHistory tidak dipakai di DTO/controller ini.",
                    "Gunakan FilePreviewUrl untuk preview modal/iframe dan FileDownloadUrl untuk download dokumen pendukung."
                }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEmploymentHistory.GetFilterMetadata",
                "Mengambil metadata filter employment history workforce.",
                new { workforceProfileId, profile.ProfileCode }
            );

            return Ok(ApiResponse<WorkforceEmploymentHistoryFilterMetadataResponse>.Ok(
                result,
                "Metadata filter employment history workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat ringkasan employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
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

            var result = new WorkforceEmploymentHistorySummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                TotalHistory = await query.CountAsync(),
                ActiveHistory = await query.CountAsync(x => x.IsActive),
                InactiveHistory = await query.CountAsync(x => !x.IsActive),
                JoinHistory = await query.CountAsync(x => x.HistoryType == EmploymentHistoryType.Join),
                MutationHistory = await query.CountAsync(x =>
                    x.HistoryType == EmploymentHistoryType.Mutation ||
                    x.HistoryType == EmploymentHistoryType.DepartmentTransfer),
                PromotionHistory = await query.CountAsync(x => x.HistoryType == EmploymentHistoryType.Promotion),
                StatusChangeHistory = await query.CountAsync(x => x.HistoryType == EmploymentHistoryType.StatusChange),
                ResignOrTerminationHistory = await query.CountAsync(x =>
                    x.HistoryType == EmploymentHistoryType.Resign ||
                    x.HistoryType == EmploymentHistoryType.Termination ||
                    x.HistoryType == EmploymentHistoryType.Retirement ||
                    x.HistoryType == EmploymentHistoryType.ContractEnded),
                WithFileHistory = await query.CountAsync(x => x.FilePath != null && x.FilePath != string.Empty),
                ApprovedHistory = await query.CountAsync(x => x.ApprovedByUserId.HasValue || x.ApprovedAt.HasValue)
            };

            return Ok(ApiResponse<WorkforceEmploymentHistorySummaryResponse>.Ok(
                result,
                "Ringkasan employment history workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceEmploymentHistoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat data employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
        public async Task<IActionResult> GetEmploymentHistories(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] EmploymentHistoryType? historyType,
            [FromQuery] Guid? newDepartmentId,
            [FromQuery] Guid? newPositionId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveDate",
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
            query = ApplyStandardFilter(query, historyType, newDepartmentId, newPositionId, isActive, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, profile, actorNames))
                .ToList();

            var result = new ResponseWorkforceEmploymentHistoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceEmploymentHistoryPagedResult>.Ok(
                result,
                "Data employment history workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat pilihan employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] EmploymentHistoryType? historyType,
            [FromQuery] Guid? newDepartmentId,
            [FromQuery] Guid? newPositionId,
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
            query = ApplyStandardFilter(query, historyType, newDepartmentId, newPositionId, onlyActive ? true : null, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.EffectiveDate)
                .ThenByDescending(x => x.CreateDateTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceEmploymentHistoryOptionResponse
                {
                    Id = x.Id,
                    HistoryType = x.HistoryType,
                    HistoryTypeName = x.HistoryType.ToString(),
                    NewDepartmentName = x.NewDepartment != null ? x.NewDepartment.DepartmentName : null,
                    NewPositionName = x.NewPosition != null ? x.NewPosition.PositionName : null,
                    OldStatus = x.OldStatus,
                    NewStatus = x.NewStatus,
                    EffectiveDate = x.EffectiveDate,
                    HasFile = x.FilePath != null && x.FilePath != string.Empty,
                    IsApproved = x.ApprovedByUserId.HasValue || x.ApprovedAt.HasValue
                })
                .ToListAsync();

            var result = new WorkforceEmploymentHistoryOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceEmploymentHistoryOptionPagedResponse>.Ok(
                result,
                "Data pilihan employment history workforce berhasil diambil."
            ));
        }

        [HttpGet("{historyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Melihat detail employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
        public async Task<IActionResult> GetEmploymentHistoryById(Guid workforceProfileId, Guid historyId)
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
                .FirstOrDefaultAsync(x => x.Id == historyId);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, profile, actorNames);
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceEmploymentHistoryDetailResponse>.Ok(
                data,
                "Detail employment history workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Employment History", Description = "Membuat employment history workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceEmploymentHistory", "Create")]
        public async Task<IActionResult> CreateEmploymentHistory(
            Guid workforceProfileId,
            [FromForm] CreateWorkforceEmploymentHistoryRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data employment history tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            string? filePath = null;
            string? fileContentType = null;

            if (request.File != null)
            {
                var savedFile = await SaveEmploymentHistoryFileAsync(workforceProfileId, request.File);
                filePath = savedFile.FilePath;
                fileContentType = savedFile.ContentType;
            }

            var entity = new WfpEmploymentHistory
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                HistoryType = request.HistoryType,
                OldDepartmentId = NormalizeNullableGuid(request.OldDepartmentId),
                NewDepartmentId = NormalizeNullableGuid(request.NewDepartmentId),
                OldPositionId = NormalizeNullableGuid(request.OldPositionId),
                NewPositionId = NormalizeNullableGuid(request.NewPositionId),
                OldStatus = NormalizeNullableText(request.OldStatus),
                NewStatus = NormalizeNullableText(request.NewStatus),
                EffectiveDate = request.EffectiveDate.Date,
                EndDate = request.EndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                ApprovedByUserId = NormalizeNullableGuid(request.ApprovedByUserId),
                ApprovedAt = request.ApprovedAt,
                FilePath = filePath,
                FileContentType = fileContentType,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<WfpEmploymentHistory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var responseEntity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceEmploymentHistory.CreateEmploymentHistory",
                "Employment history workforce berhasil dibuat.",
                new { entity.Id, entity.WorkforceProfileId, entity.HistoryType }
            );

            var actorNames = await GetActorNameMapAsync(new[]
            {
                responseEntity.CreateBy,
                responseEntity.UpdateBy
            });

            var data = MapDetailResponse(responseEntity, profile, actorNames);
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceEmploymentHistoryDetailResponse>.Ok(
                data,
                "Employment history workforce berhasil dibuat."
            ));
        }

        [HttpPut("{historyId:guid}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Employment History", Description = "Mengubah employment history workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceEmploymentHistory", "Update")]
        public async Task<IActionResult> UpdateEmploymentHistory(
            Guid workforceProfileId,
            Guid historyId,
            [FromForm] UpdateWorkforceEmploymentHistoryRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpEmploymentHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(workforceProfileId, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data employment history tidak valid."
                ));
            }

            if (request.File != null && request.ReplaceExistingFile)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
                var savedFile = await SaveEmploymentHistoryFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.File != null && string.IsNullOrWhiteSpace(entity.FilePath))
            {
                var savedFile = await SaveEmploymentHistoryFileAsync(workforceProfileId, request.File);
                entity.FilePath = savedFile.FilePath;
                entity.FileContentType = savedFile.ContentType;
            }
            else if (request.ReplaceExistingFile && request.File == null)
            {
                DeletePhysicalFileIfExists(entity.FilePath);
                entity.FilePath = null;
                entity.FileContentType = null;
            }

            entity.HistoryType = request.HistoryType;
            entity.OldDepartmentId = NormalizeNullableGuid(request.OldDepartmentId);
            entity.NewDepartmentId = NormalizeNullableGuid(request.NewDepartmentId);
            entity.OldPositionId = NormalizeNullableGuid(request.OldPositionId);
            entity.NewPositionId = NormalizeNullableGuid(request.NewPositionId);
            entity.OldStatus = NormalizeNullableText(request.OldStatus);
            entity.NewStatus = NormalizeNullableText(request.NewStatus);
            entity.EffectiveDate = request.EffectiveDate.Date;
            entity.EndDate = request.EndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.ApprovedByUserId = NormalizeNullableGuid(request.ApprovedByUserId);
            entity.ApprovedAt = request.ApprovedAt;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var responseEntity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == entity.Id);

            var actorNames = await GetActorNameMapAsync(new[]
            {
                responseEntity.CreateBy,
                responseEntity.UpdateBy
            });

            var data = MapDetailResponse(responseEntity, profile, actorNames);
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceEmploymentHistoryDetailResponse>.Ok(
                data,
                "Employment history workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{historyId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceEmploymentHistoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Employment History Status", Description = "Mengubah status employment history workforce", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceEmploymentHistory", "Update")]
        public async Task<IActionResult> UpdateEmploymentHistoryStatus(
            Guid workforceProfileId,
            Guid historyId,
            [FromBody] UpdateWorkforceEmploymentHistoryStatusRequest request)
        {
            var profile = await GetProfileAsync(workforceProfileId);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpEmploymentHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var responseEntity = await BuildBaseQuery(workforceProfileId)
                .FirstAsync(x => x.Id == entity.Id);

            var actorNames = await GetActorNameMapAsync(new[]
            {
                responseEntity.CreateBy,
                responseEntity.UpdateBy
            });

            var data = MapDetailResponse(responseEntity, profile, actorNames);
            NormalizeAudit(data);

            return Ok(ApiResponse<WorkforceEmploymentHistoryDetailResponse>.Ok(
                data,
                "Status employment history workforce berhasil diperbarui."
            ));
        }

        [HttpGet("{historyId:guid}/preview")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Preview file employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
        public async Task<IActionResult> PreviewEmploymentHistoryFile(Guid workforceProfileId, Guid historyId)
        {
            var fileValidation = await GetEmploymentHistoryFileAsync(workforceProfileId, historyId);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File employment history workforce tidak ditemukan."
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

        [HttpGet("{historyId:guid}/download")]
        [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Employment History", Description = "Download file employment history workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceEmploymentHistory", "Read")]
        public async Task<IActionResult> DownloadEmploymentHistoryFile(Guid workforceProfileId, Guid historyId)
        {
            var fileValidation = await GetEmploymentHistoryFileAsync(workforceProfileId, historyId);

            if (!fileValidation.IsValid)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    fileValidation.ErrorMessage ?? "File employment history workforce tidak ditemukan."
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

        [HttpDelete("{historyId:guid}/file")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Employment History File", Description = "Menghapus file employment history workforce", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkforceEmploymentHistory", "Delete")]
        public async Task<IActionResult> DeleteEmploymentHistoryFile(
            Guid workforceProfileId,
            Guid historyId,
            [FromBody] DeleteWorkforceEmploymentHistoryFileRequest? request = null)
        {
            var entity = await _dbContext.Set<WfpEmploymentHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
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
                "File employment history workforce berhasil dihapus."
            ));
        }

        [HttpDelete("{historyId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Employment History", Description = "Menghapus employment history workforce", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("WorkforceEmploymentHistory", "Delete")]
        public async Task<IActionResult> DeleteEmploymentHistory(Guid workforceProfileId, Guid historyId)
        {
            var entity = await _dbContext.Set<WfpEmploymentHistory>()
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Employment history tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Employment history workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpEmploymentHistory> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .Include(x => x.OldDepartment)
                .Include(x => x.NewDepartment)
                .Include(x => x.OldPosition)
                .Include(x => x.NewPosition)
                .Include(x => x.ApprovedByUser)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpEmploymentHistory> ApplyDateFilter(
            IQueryable<WfpEmploymentHistory> query,
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

        private static IQueryable<WfpEmploymentHistory> ApplyStandardFilter(
            IQueryable<WfpEmploymentHistory> query,
            EmploymentHistoryType? historyType,
            Guid? newDepartmentId,
            Guid? newPositionId,
            bool? isActive,
            string? search)
        {
            if (historyType.HasValue && historyType.Value != EmploymentHistoryType.Unknown)
            {
                query = query.Where(x => x.HistoryType == historyType.Value);
            }

            if (newDepartmentId.HasValue && newDepartmentId.Value != Guid.Empty)
            {
                query = query.Where(x => x.NewDepartmentId == newDepartmentId.Value);
            }

            if (newPositionId.HasValue && newPositionId.Value != Guid.Empty)
            {
                query = query.Where(x => x.NewPositionId == newPositionId.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                var matchedHistoryTypes = Enum.GetValues(typeof(EmploymentHistoryType))
                    .Cast<EmploymentHistoryType>()
                    .Where(x => x.ToString().ToLowerInvariant().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    matchedHistoryTypes.Contains(x.HistoryType) ||
                    (x.OldDepartment != null && x.OldDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.NewDepartment != null && x.NewDepartment.DepartmentName.ToLower().Contains(keyword)) ||
                    (x.OldPosition != null && x.OldPosition.PositionName.ToLower().Contains(keyword)) ||
                    (x.NewPosition != null && x.NewPosition.PositionName.ToLower().Contains(keyword)) ||
                    (x.OldStatus != null && x.OldStatus.ToLower().Contains(keyword)) ||
                    (x.NewStatus != null && x.NewStatus.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpEmploymentHistory> ApplySorting(
            IQueryable<WfpEmploymentHistory> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "effectiveDate").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.CreateDateTime).ThenBy(x => x.EffectiveDate),
                "historytype" => isDescending ? query.OrderByDescending(x => x.HistoryType).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.HistoryType).ThenByDescending(x => x.EffectiveDate),
                "enddate" => isDescending ? query.OrderByDescending(x => x.EndDate).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.EndDate).ThenByDescending(x => x.EffectiveDate),
                "olddepartmentname" => isDescending ? query.OrderByDescending(x => x.OldDepartment != null ? x.OldDepartment.DepartmentName : string.Empty).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.OldDepartment != null ? x.OldDepartment.DepartmentName : string.Empty).ThenByDescending(x => x.EffectiveDate),
                "newdepartmentname" => isDescending ? query.OrderByDescending(x => x.NewDepartment != null ? x.NewDepartment.DepartmentName : string.Empty).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.NewDepartment != null ? x.NewDepartment.DepartmentName : string.Empty).ThenByDescending(x => x.EffectiveDate),
                "oldpositionname" => isDescending ? query.OrderByDescending(x => x.OldPosition != null ? x.OldPosition.PositionName : string.Empty).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.OldPosition != null ? x.OldPosition.PositionName : string.Empty).ThenByDescending(x => x.EffectiveDate),
                "newpositionname" => isDescending ? query.OrderByDescending(x => x.NewPosition != null ? x.NewPosition.PositionName : string.Empty).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.NewPosition != null ? x.NewPosition.PositionName : string.Empty).ThenByDescending(x => x.EffectiveDate),
                "oldstatus" => isDescending ? query.OrderByDescending(x => x.OldStatus).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.OldStatus).ThenByDescending(x => x.EffectiveDate),
                "newstatus" => isDescending ? query.OrderByDescending(x => x.NewStatus).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.NewStatus).ThenByDescending(x => x.EffectiveDate),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveDate) : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveDate),
                _ => isDescending ? query.OrderByDescending(x => x.EffectiveDate).ThenByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.EffectiveDate).ThenByDescending(x => x.CreateDateTime)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            CreateWorkforceEmploymentHistoryRequest request)
        {
            if (!await ProfileExistsAsync(workforceProfileId))
            {
                return (false, "Workforce profile tidak ditemukan.");
            }

            if (!Enum.IsDefined(typeof(EmploymentHistoryType), request.HistoryType) ||
                request.HistoryType == EmploymentHistoryType.Unknown)
            {
                return (false, "HistoryType wajib dipilih dan tidak boleh Unknown.");
            }

            if (request.EffectiveDate == default)
            {
                return (false, "EffectiveDate wajib diisi.");
            }

            if (request.EndDate.HasValue && request.EndDate.Value.Date < request.EffectiveDate.Date)
            {
                return (false, "EndDate tidak boleh lebih kecil dari EffectiveDate.");
            }

            var relationValidation = await ValidateRelationsAsync(
                request.OldDepartmentId,
                request.NewDepartmentId,
                request.OldPositionId,
                request.NewPositionId);

            if (!relationValidation.IsValid)
            {
                return relationValidation;
            }

            if (request.ApprovedAt.HasValue &&
                (!request.ApprovedByUserId.HasValue || request.ApprovedByUserId.Value == Guid.Empty))
            {
                return (false, "ApprovedByUserId wajib diisi jika ApprovedAt diisi.");
            }

            if (request.ApprovedByUserId.HasValue && request.ApprovedByUserId.Value != Guid.Empty)
            {
                var userExists = await _dbContext.Users
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == request.ApprovedByUserId.Value && x.IsActive);

                if (!userExists)
                {
                    return (false, "ApprovedByUser tidak ditemukan atau tidak aktif.");
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

            return (true, null);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRelationsAsync(
            Guid? oldDepartmentId,
            Guid? newDepartmentId,
            Guid? oldPositionId,
            Guid? newPositionId)
        {
            var oldDepartment = NormalizeNullableGuid(oldDepartmentId);
            var newDepartment = NormalizeNullableGuid(newDepartmentId);
            var oldPosition = NormalizeNullableGuid(oldPositionId);
            var newPosition = NormalizeNullableGuid(newPositionId);

            if (oldDepartment.HasValue)
            {
                var exists = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == oldDepartment.Value && !x.IsDelete);

                if (!exists)
                {
                    return (false, "OldDepartmentId tidak ditemukan.");
                }
            }

            if (newDepartment.HasValue)
            {
                var exists = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == newDepartment.Value && !x.IsDelete);

                if (!exists)
                {
                    return (false, "NewDepartmentId tidak ditemukan.");
                }
            }

            if (oldPosition.HasValue)
            {
                var position = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == oldPosition.Value && !x.IsDelete);

                if (position == null)
                {
                    return (false, "OldPositionId tidak ditemukan.");
                }

                if (oldDepartment.HasValue && position.DepartmentId != oldDepartment.Value)
                {
                    return (false, "OldPositionId tidak sesuai dengan OldDepartmentId.");
                }
            }

            if (newPosition.HasValue)
            {
                var position = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == newPosition.Value && !x.IsDelete);

                if (position == null)
                {
                    return (false, "NewPositionId tidak ditemukan.");
                }

                if (newDepartment.HasValue && position.DepartmentId != newDepartment.Value)
                {
                    return (false, "NewPositionId tidak sesuai dengan NewDepartmentId.");
                }
            }

            return (true, null);
        }

        private static (bool IsValid, string? ErrorMessage) ValidateFile(IFormFile file)
        {
            if (file.Length <= 0)
            {
                return (false, "File employment history kosong.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return (false, "Ukuran file employment history maksimal 10 MB.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Format file tidak didukung. Gunakan PDF, JPG, PNG, DOC, DOCX, XLS, atau XLSX.");
            }

            return (true, null);
        }

        private async Task<(string FilePath, string? ContentType)> SaveEmploymentHistoryFileAsync(
            Guid workforceProfileId,
            IFormFile file)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var storage = GetFileStoragePaths();
            var relativeFolder = Path.Combine("workforce-employment-histories", workforceProfileId.ToString());
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

        private async Task<FileResolveResult> GetEmploymentHistoryFileAsync(Guid workforceProfileId, Guid historyId)
        {
            var history = await _dbContext.Set<WfpEmploymentHistory>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == historyId &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (history == null)
            {
                return FileResolveResult.Invalid("Employment history tidak ditemukan.");
            }

            if (string.IsNullOrWhiteSpace(history.FilePath))
            {
                return FileResolveResult.Invalid("File employment history workforce belum tersedia.");
            }

            var physicalPath = ResolvePhysicalPath(history.FilePath);

            if (!System.IO.File.Exists(physicalPath))
            {
                return FileResolveResult.Invalid("File fisik employment history tidak ditemukan di server.");
            }

            var provider = new FileExtensionContentTypeProvider();

            if (!provider.TryGetContentType(physicalPath, out var contentType))
            {
                contentType = history.FileContentType ?? "application/octet-stream";
            }

            var extension = Path.GetExtension(physicalPath);
            var fileName = Path.GetFileName(physicalPath);
            var safeHistoryName = SanitizeFileName(history.HistoryType.ToString());
            var downloadName = $"EMPLOYMENT_HISTORY_{safeHistoryName}_{history.EffectiveDate:yyyyMMdd}{extension}";

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

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private WorkforceEmploymentHistoryResponse MapResponse(
            WfpEmploymentHistory entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var hasFile = !string.IsNullOrWhiteSpace(entity.FilePath);

            return new WorkforceEmploymentHistoryResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                UserType = profile.UserType,
                HistoryType = entity.HistoryType,
                OldDepartmentId = entity.OldDepartmentId,
                OldDepartmentCode = entity.OldDepartment?.DepartmentCode,
                OldDepartmentName = entity.OldDepartment?.DepartmentName,
                NewDepartmentId = entity.NewDepartmentId,
                NewDepartmentCode = entity.NewDepartment?.DepartmentCode,
                NewDepartmentName = entity.NewDepartment?.DepartmentName,
                OldPositionId = entity.OldPositionId,
                OldPositionCode = entity.OldPosition?.PositionCode,
                OldPositionName = entity.OldPosition?.PositionName,
                NewPositionId = entity.NewPositionId,
                NewPositionCode = entity.NewPosition?.PositionCode,
                NewPositionName = entity.NewPosition?.PositionName,
                OldStatus = entity.OldStatus,
                NewStatus = entity.NewStatus,
                EffectiveDate = entity.EffectiveDate,
                EndDate = entity.EndDate,
                Description = entity.Description,
                ApprovedByUserId = entity.ApprovedByUserId,
                ApprovedByUserName = entity.ApprovedByUser?.DisplayName,
                ApprovedAt = entity.ApprovedAt,
                FilePath = entity.FilePath,
                FileContentType = entity.FileContentType,
                FileName = hasFile ? Path.GetFileName(entity.FilePath) : null,
                FilePreviewUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "preview") : null,
                FileDownloadUrl = hasFile ? BuildEndpointUrl(entity.WorkforceProfileId, entity.Id, "download") : null,
                HasFile = hasFile,
                IsApproved = entity.ApprovedByUserId.HasValue || entity.ApprovedAt.HasValue,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private WorkforceEmploymentHistoryDetailResponse MapDetailResponse(
            WfpEmploymentHistory entity,
            MstWorkforceProfile profile,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var response = MapResponse(entity, profile, actorNames);

            return new WorkforceEmploymentHistoryDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                UserType = response.UserType,
                HistoryType = response.HistoryType,
                OldDepartmentId = response.OldDepartmentId,
                OldDepartmentCode = response.OldDepartmentCode,
                OldDepartmentName = response.OldDepartmentName,
                NewDepartmentId = response.NewDepartmentId,
                NewDepartmentCode = response.NewDepartmentCode,
                NewDepartmentName = response.NewDepartmentName,
                OldPositionId = response.OldPositionId,
                OldPositionCode = response.OldPositionCode,
                OldPositionName = response.OldPositionName,
                NewPositionId = response.NewPositionId,
                NewPositionCode = response.NewPositionCode,
                NewPositionName = response.NewPositionName,
                OldStatus = response.OldStatus,
                NewStatus = response.NewStatus,
                EffectiveDate = response.EffectiveDate,
                EndDate = response.EndDate,
                Description = response.Description,
                ApprovedByUserId = response.ApprovedByUserId,
                ApprovedByUserName = response.ApprovedByUserName,
                ApprovedAt = response.ApprovedAt,
                FilePath = response.FilePath,
                FileContentType = response.FileContentType,
                FileName = response.FileName,
                FilePreviewUrl = response.FilePreviewUrl,
                FileDownloadUrl = response.FileDownloadUrl,
                HasFile = response.HasFile,
                IsApproved = response.IsApproved,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static void NormalizeAudit(WorkforceEmploymentHistoryDetailResponse data)
        {
            if (data.UpdateDateTime.HasValue &&
                data.UpdateDateTime.Value == DateTime.MinValue)
            {
                data.UpdateDateTime = null;
            }

            if (!data.CreateBy.HasValue || data.CreateBy.Value == Guid.Empty)
            {
                data.CreateBy = null;
                data.CreateByName = null;
            }

            if (!data.UpdateBy.HasValue || data.UpdateBy.Value == Guid.Empty)
            {
                data.UpdateBy = null;
                data.UpdateByName = null;
            }
        }

        private string BuildEndpointUrl(Guid workforceProfileId, Guid id, string action)
        {
            var pathBase = Request.PathBase.HasValue ? Request.PathBase.Value : string.Empty;
            var path = $"{pathBase}/api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId}/employment-histories/{id}/{action}";

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
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
            {
                return null;
            }

            return value.Value;
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
                ? "employment-history"
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

        private static List<WorkforceEmploymentHistoryTypeOptionResponse> BuildHistoryTypeOptions()
        {
            return Enum.GetValues(typeof(EmploymentHistoryType))
                .Cast<EmploymentHistoryType>()
                .Where(x => x != EmploymentHistoryType.Unknown)
                .OrderBy(x => Convert.ToInt32(x))
                .Select(x => new WorkforceEmploymentHistoryTypeOptionResponse
                {
                    Value = x,
                    Code = x.ToString(),
                    Label = ToDisplayLabel(x.ToString()),
                    Description = GetHistoryTypeDescription(x)
                })
                .ToList();
        }

        private static string GetHistoryTypeDescription(EmploymentHistoryType value)
        {
            return value switch
            {
                EmploymentHistoryType.Join => "Riwayat mulai bergabung sebagai workforce.",
                EmploymentHistoryType.Mutation => "Riwayat mutasi umum antar unit/jabatan.",
                EmploymentHistoryType.DepartmentTransfer => "Riwayat perpindahan department/unit kerja.",
                EmploymentHistoryType.Promotion => "Riwayat promosi jabatan atau level.",
                EmploymentHistoryType.StatusChange => "Riwayat perubahan status kerja.",
                EmploymentHistoryType.Resign => "Riwayat resign/mengundurkan diri.",
                EmploymentHistoryType.Termination => "Riwayat pemutusan hubungan kerja.",
                EmploymentHistoryType.Retirement => "Riwayat pensiun.",
                EmploymentHistoryType.ContractEnded => "Riwayat kontrak berakhir.",
                _ => "Riwayat employment lainnya."
            };
        }

        private static string ToDisplayLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var chars = new List<char> { value[0] };

            for (var i = 1; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && !char.IsWhiteSpace(value[i - 1]))
                {
                    chars.Add(' ');
                }

                chars.Add(value[i]);
            }

            return new string(chars.ToArray());
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
