using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceOrganizationAssignmentPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.DTOs.WorkforceOrganizationAssignmentResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/organization-assignments")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Organization Assignment",
        AreaName = "Corporate",
        ControllerName = "WorkforceOrganizationAssignment",
        Description = "Workforce organization assignment management",
        SortOrder = 20
    )]
    [Tags("Corporate / Human Resource / Workforce / Organization Assignment")]
    public class WorkforceOrganizationAssignmentController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.Workforce.OrganizationAssignment";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceOrganizationAssignmentController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Organization Assignment",
            Description = "Melihat metadata filter organization assignment workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Read")]
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

            var result = new WorkforceOrganizationAssignmentFilterMetadataResponse
            {
                DefaultFilter = new WorkforceOrganizationAssignmentDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceOrganizationAssignmentCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" },
                    new() { Value = "custom", Label = "Custom" }
                },
                SortOptions = new List<WorkforceOrganizationAssignmentSortOptionResponse>
                {
                    new() { Value = "effectiveStartDate", Label = "Tanggal efektif mulai" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal efektif selesai" },
                    new() { Value = "departmentName", Label = "Nama department" },
                    new() { Value = "positionName", Label = "Nama position" },
                    new() { Value = "isPrimary", Label = "Primary" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                RelationFilterInfo = new WorkforceOrganizationAssignmentRelationFilterInfoResponse(),
                FrontendGuide = new WorkforceOrganizationAssignmentFrontendGuideResponse()
            };

            return Ok(ApiResponse<WorkforceOrganizationAssignmentFilterMetadataResponse>.Ok(
                result,
                "Metadata filter organization assignment workforce berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Organization Assignment",
            Description = "Melihat ringkasan organization assignment workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Read")]
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

            var today = DateTime.UtcNow.Date;
            var query = BuildBaseQuery(workforceProfileId);

            var result = new WorkforceOrganizationAssignmentSummaryResponse
            {
                WorkforceProfileId = profile.Id,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                TotalOrganizationAssignment = await query.CountAsync(),
                ActiveOrganizationAssignment = await query.CountAsync(x => x.IsActive),
                InactiveOrganizationAssignment = await query.CountAsync(x => !x.IsActive),
                PrimaryOrganizationAssignment = await query.CountAsync(x => x.IsPrimary && x.IsActive),
                CurrentlyValidOrganizationAssignment = await query.CountAsync(x =>
                    x.IsActive &&
                    x.EffectiveStartDate.Date <= today &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredOrganizationAssignment = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value.Date < today)
            };

            return Ok(ApiResponse<WorkforceOrganizationAssignmentSummaryResponse>.Ok(
                result,
                "Ringkasan organization assignment workforce berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceOrganizationAssignmentPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Organization Assignment",
            Description = "Melihat organization assignment workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetOrganizationAssignments(
            Guid workforceProfileId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isExpired,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "effectiveStartDate",
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
            query = ApplyStandardFilter(query, departmentId, positionId, isPrimary, isActive, isExpired, search);

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(x => MapResponse(x, profile))
                .ToList();

            var result = new ResponseWorkforceOrganizationAssignmentPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceOrganizationAssignmentPagedResult>.Ok(
                result,
                "Data organization assignment workforce berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Organization Assignment",
            Description = "Melihat pilihan organization assignment workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetOptions(
            Guid workforceProfileId,
            [FromQuery] Guid? departmentId,
            [FromQuery] Guid? positionId,
            [FromQuery] bool? isPrimary,
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
                departmentId,
                positionId,
                isPrimary,
                onlyActive ? true : null,
                null,
                search);

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenByDescending(x => x.IsActive)
                .ThenBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty)
                .ThenBy(x => x.Position != null ? x.Position.PositionName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(x => new WorkforceOrganizationAssignmentOptionResponse
                {
                    Id = x.Id,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department?.DepartmentCode ?? string.Empty,
                    DepartmentName = x.Department?.DepartmentName ?? string.Empty,
                    PositionId = x.PositionId,
                    PositionCode = x.Position?.PositionCode ?? string.Empty,
                    PositionName = x.Position?.PositionName ?? string.Empty,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate
                })
                .ToList();

            var result = new WorkforceOrganizationAssignmentOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceOrganizationAssignmentOptionPagedResponse>.Ok(
                result,
                "Data pilihan organization assignment workforce berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Organization Assignment",
            Description = "Melihat detail organization assignment workforce profile",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Read")]
        public async Task<IActionResult> GetOrganizationById(Guid workforceProfileId, Guid id)
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
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>.Ok(
                MapDetailResponse(entity, profile),
                "Detail organization assignment workforce berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Workforce Organization Assignment",
            Description = "Menambah organization assignment workforce profile",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Create")]
        public async Task<IActionResult> CreateOrganizationAssignment(
            Guid workforceProfileId,
            [FromBody] CreateWorkforceOrganizationAssignmentRequest request)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                null,
                request.DepartmentId,
                request.PositionId,
                request.EffectiveStartDate,
                request.EffectiveEndDate);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data organization assignment tidak valid."
                ));
            }

            if (request.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Organization assignment primary harus aktif."
                ));
            }

            if (request.IsPrimary && request.EffectiveEndDate.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Organization assignment primary tidak boleh memiliki EffectiveEndDate."
                ));
            }

            var hasActiveAssignment = await _dbContext.Set<WfpOrganizationAssignment>()
                .AnyAsync(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.IsActive &&
                    !x.IsDelete);

            var shouldBePrimary = request.IsPrimary || (!hasActiveAssignment && request.IsActive);

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var entity = new WfpOrganizationAssignment
                {
                    Id = Guid.NewGuid(),
                    WorkforceProfileId = workforceProfileId,
                    DepartmentId = request.DepartmentId,
                    PositionId = request.PositionId,
                    IsPrimary = false,
                    IsActive = request.IsActive,
                    EffectiveStartDate = request.EffectiveStartDate.Date,
                    EffectiveEndDate = request.EffectiveEndDate?.Date,
                    Description = NormalizeNullableText(request.Description),
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<WfpOrganizationAssignment>().Add(entity);

                if (shouldBePrimary)
                {
                    await ApplyPrimaryOrganizationAsync(profile, entity, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                await _loggerService.InfoAsync(
                    LogCategory,
                    "WorkforceOrganization.CreateOrganization",
                    "Organization assignment workforce berhasil dibuat.",
                    new
                    {
                        workforceProfileId,
                        entity.Id,
                        entity.DepartmentId,
                        entity.PositionId,
                        entity.IsPrimary
                    }
                );

                return Ok(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>.Ok(
                    MapDetailResponse(entity, profile),
                    "Organization assignment workforce berhasil dibuat."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOrganization.CreateOrganization",
                    "Gagal membuat organization assignment workforce.",
                    ex,
                    new { workforceProfileId }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat organization assignment workforce."
                    )
                );
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Organization Assignment",
            Description = "Mengubah organization assignment workforce profile",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Update")]
        public async Task<IActionResult> UpdateOrganizationAssignment(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceOrganizationAssignmentRequest request)
        {
            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpOrganizationAssignment>()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (entity.IsPrimary && !request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh dilepas langsung. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            if ((entity.IsPrimary || request.IsPrimary) && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment harus aktif."
                ));
            }

            if ((entity.IsPrimary || request.IsPrimary) && request.EffectiveEndDate.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh memiliki EffectiveEndDate. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                id,
                request.DepartmentId,
                request.PositionId,
                request.EffectiveStartDate,
                request.EffectiveEndDate);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data organization assignment tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                entity.DepartmentId = request.DepartmentId;
                entity.PositionId = request.PositionId;
                entity.IsActive = request.IsActive;
                entity.EffectiveStartDate = request.EffectiveStartDate.Date;
                entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
                entity.Description = NormalizeNullableText(request.Description);
                entity.UpdateDateTime = now;
                entity.UpdateBy = actorUserId;

                if (request.IsPrimary)
                {
                    await ApplyPrimaryOrganizationAsync(profile, entity, now, actorUserId);
                }

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                entity.Department = await _dbContext.Set<MstDepartment>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.DepartmentId);

                entity.Position = await _dbContext.Set<MstPosition>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == entity.PositionId);

                return Ok(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>.Ok(
                    MapDetailResponse(entity, profile),
                    "Organization assignment workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOrganization.UpdateOrganization",
                    "Gagal mengubah organization assignment workforce.",
                    ex,
                    new { workforceProfileId, id }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat mengubah organization assignment workforce."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Organization Assignment",
            Description = "Mengubah status organization assignment workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Update")]
        public async Task<IActionResult> UpdateOrganizationStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWorkforceOrganizationAssignmentStatusRequest request)
        {
            var entity = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (entity.IsPrimary && !request.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh dinonaktifkan. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            if (entity.IsPrimary && request.EffectiveEndDate.HasValue)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh diberi EffectiveEndDate. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            if (request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = request.IsActive
                ? request.EffectiveEndDate?.Date
                : request.EffectiveEndDate?.Date ?? now.Date;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status organization assignment workforce berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/primary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Set Workforce Organization Assignment Primary",
            Description = "Menetapkan primary organization assignment workforce",
            AccessType = AccessTypes.Update,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Update")]
        public async Task<IActionResult> SetPrimaryOrganizationAssignment(
            Guid workforceProfileId,
            Guid id,
            [FromBody] SetWorkforceOrganizationAssignmentPrimaryRequest request)
        {
            if (!request.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "IsPrimary harus bernilai true."
                ));
            }

            var profile = await _dbContext.Set<MstWorkforceProfile>()
                .FirstOrDefaultAsync(x => x.Id == workforceProfileId && !x.IsDelete);

            if (profile == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce profile tidak ditemukan."
                ));
            }

            var entity = await _dbContext.Set<WfpOrganizationAssignment>()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (!entity.IsActive)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Assignment tidak aktif tidak bisa dijadikan primary."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await ApplyPrimaryOrganizationAsync(profile, entity, now, actorUserId);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(ApiResponse<WorkforceOrganizationAssignmentDetailResponse>.Ok(
                    MapDetailResponse(entity, profile),
                    "Primary organization assignment workforce berhasil diperbarui."
                ));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggerService.ErrorAsync(
                    LogCategory,
                    "WorkforceOrganization.SetPrimaryOrganization",
                    "Gagal menetapkan primary organization assignment workforce.",
                    ex,
                    new { workforceProfileId, id }
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat menetapkan primary organization assignment workforce."
                    )
                );
            }
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Organization Assignment",
            Description = "Menghapus organization assignment workforce",
            AccessType = AccessTypes.Delete,
            SortOrder = 6
        )]
        [AccessPermission("WorkforceOrganizationAssignment", "Delete")]
        public async Task<IActionResult> DeleteOrganizationAssignment(Guid workforceProfileId, Guid id)
        {
            var entity = await _dbContext.Set<WfpOrganizationAssignment>()
                .FirstOrDefaultAsync(x =>
                    x.Id == id &&
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization assignment workforce tidak ditemukan."
                ));
            }

            if (entity.IsPrimary)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Primary assignment tidak boleh dihapus. Jadikan assignment lain sebagai primary terlebih dahulu."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = false;
            entity.IsDelete = true;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Organization assignment workforce berhasil dihapus."
            ));
        }

        private IQueryable<WfpOrganizationAssignment> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Include(x => x.Department)
                .Include(x => x.Position)
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);
        }

        private static IQueryable<WfpOrganizationAssignment> ApplyDateFilter(
            IQueryable<WfpOrganizationAssignment> query,
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
                query = query.Where(x => x.EffectiveStartDate >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.EffectiveStartDate < end);
            }

            return query;
        }

        private static IQueryable<WfpOrganizationAssignment> ApplyStandardFilter(
            IQueryable<WfpOrganizationAssignment> query,
            Guid? departmentId,
            Guid? positionId,
            bool? isPrimary,
            bool? isActive,
            bool? isExpired,
            string? search)
        {
            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (positionId.HasValue && positionId.Value != Guid.Empty)
                query = query.Where(x => x.PositionId == positionId.Value);

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isExpired.HasValue)
            {
                var today = DateTime.UtcNow.Date;

                query = isExpired.Value
                    ? query.Where(x => x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today)
                    : query.Where(x => !x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.Department != null && (
                        x.Department.DepartmentCode.ToLower().Contains(keyword) ||
                        x.Department.DepartmentName.ToLower().Contains(keyword))) ||
                    (x.Position != null && (
                        x.Position.PositionCode.ToLower().Contains(keyword) ||
                        x.Position.PositionName.ToLower().Contains(keyword))) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpOrganizationAssignment> ApplySorting(
            IQueryable<WfpOrganizationAssignment> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "effectiveStartDate").Trim().ToLowerInvariant() switch
            {
                "effectivestartdate" => isDescending ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDescending ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "departmentname" => isDescending
                    ? query.OrderByDescending(x => x.Department != null ? x.Department.DepartmentName : string.Empty)
                    : query.OrderBy(x => x.Department != null ? x.Department.DepartmentName : string.Empty),
                "positionname" => isDescending
                    ? query.OrderByDescending(x => x.Position != null ? x.Position.PositionName : string.Empty)
                    : query.OrderBy(x => x.Position != null ? x.Position.PositionName : string.Empty),
                "isprimary" => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate),
                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.IsActive).ThenByDescending(x => x.EffectiveStartDate),
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                _ => isDescending
                    ? query.OrderByDescending(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.IsPrimary).ThenByDescending(x => x.EffectiveStartDate)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            Guid? excludeAssignmentId,
            Guid departmentId,
            Guid positionId,
            DateTime effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (departmentId == Guid.Empty)
                return (false, "Department wajib dipilih.");

            if (positionId == Guid.Empty)
                return (false, "Position wajib dipilih.");

            if (effectiveStartDate == default)
                return (false, "EffectiveStartDate wajib diisi.");

            if (effectiveEndDate.HasValue && effectiveEndDate.Value.Date < effectiveStartDate.Date)
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");

            var departmentExists = await _dbContext.Set<MstDepartment>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == departmentId && x.IsActive && !x.IsDelete);

            if (!departmentExists)
                return (false, "Department tidak valid atau tidak aktif.");

            var positionExists = await _dbContext.Set<MstPosition>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == positionId &&
                    x.DepartmentId == departmentId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!positionExists)
                return (false, "Position tidak valid, tidak aktif, atau tidak sesuai department.");

            var duplicateQuery = _dbContext.Set<WfpOrganizationAssignment>()
                .AsNoTracking()
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (excludeAssignmentId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeAssignmentId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Workforce profile sudah memiliki assignment aktif pada department dan position tersebut.");

            return (true, null);
        }

        private async Task ApplyPrimaryOrganizationAsync(
            MstWorkforceProfile profile,
            WfpOrganizationAssignment assignment,
            DateTime now,
            Guid actorUserId)
        {
            var currentPrimaries = await _dbContext.Set<WfpOrganizationAssignment>()
                .Where(x =>
                    x.WorkforceProfileId == profile.Id &&
                    x.Id != assignment.Id &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            assignment.IsPrimary = true;
            assignment.IsActive = true;
            assignment.EffectiveEndDate = null;
            assignment.UpdateDateTime = now;
            assignment.UpdateBy = actorUserId;

            profile.PrimaryDepartmentId = assignment.DepartmentId;
            profile.PrimaryPositionId = assignment.PositionId;
            profile.UpdateDateTime = now;
            profile.UpdateBy = actorUserId;

            if (profile.UserType == UserType.Employee)
            {
                var employee = await _dbContext.Set<MstEmployee>()
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == profile.Id &&
                        !x.IsDelete);

                if (employee != null)
                {
                    employee.PrimaryDepartmentId = assignment.DepartmentId;
                    employee.PrimaryPositionId = assignment.PositionId;
                    employee.UpdateDateTime = now;
                    employee.UpdateBy = actorUserId;
                }
            }

            if (profile.UserType == UserType.PermanentDoctor ||
                profile.UserType == UserType.GuestDoctor)
            {
                var doctor = await _dbContext.Set<MstDoctor>()
                    .FirstOrDefaultAsync(x =>
                        x.WorkforceProfileId == profile.Id &&
                        !x.IsDelete);

                if (doctor != null)
                {
                    doctor.PrimaryDepartmentId = assignment.DepartmentId;
                    doctor.PrimaryPositionId = assignment.PositionId;
                    doctor.UpdateDateTime = now;
                    doctor.UpdateBy = actorUserId;
                }
            }

            var linkedUser = await _dbContext.Users
                .FirstOrDefaultAsync(x => x.WorkforceProfileId == profile.Id);

            if (linkedUser != null)
            {
                linkedUser.PrimaryDepartmentId = assignment.DepartmentId;
                linkedUser.PrimaryPositionId = assignment.PositionId;
                linkedUser.UpdateDateTime = now;

                await SyncUserPrimaryOrganizationAsync(
                    linkedUser.Id,
                    assignment.DepartmentId,
                    assignment.PositionId,
                    assignment.EffectiveStartDate,
                    actorUserId);
            }
        }

        private async Task SyncUserPrimaryOrganizationAsync(
            Guid userId,
            Guid departmentId,
            Guid positionId,
            DateTime effectiveStartDate,
            Guid actorUserId)
        {
            var now = DateTime.UtcNow;
            var effectiveStartUtc = DateTime.SpecifyKind(
                effectiveStartDate.Date,
                DateTimeKind.Utc
            );

            var currentPrimaries = await _dbContext.Set<ApplicationUserOrganization>()
                .Where(x =>
                    x.UserId == userId &&
                    x.IsPrimary &&
                    !x.IsDelete)
                .ToListAsync();

            foreach (var item in currentPrimaries)
            {
                item.IsPrimary = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            var existing = await _dbContext.Set<ApplicationUserOrganization>()
                .FirstOrDefaultAsync(x =>
                    x.UserId == userId &&
                    x.DepartmentId == departmentId &&
                    x.PositionId == positionId);

            if (existing == null)
            {
                existing = new ApplicationUserOrganization
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    DepartmentId = departmentId,
                    PositionId = positionId,
                    IsPrimary = true,
                    IsActive = true,
                    EffectiveStartDate = effectiveStartUtc,
                    EffectiveEndDate = null,
                    Description = "Synced from workforce primary organization",
                    CreateDateTime = now,
                    CreateBy = actorUserId,
                    IsDelete = false,
                    IsCancel = false
                };

                _dbContext.Set<ApplicationUserOrganization>().Add(existing);
            }
            else
            {
                existing.IsPrimary = true;
                existing.IsActive = true;
                existing.EffectiveStartDate = effectiveStartUtc;
                existing.EffectiveEndDate = null;
                existing.Description = "Synced from workforce primary organization";
                existing.IsDelete = false;
                existing.DeleteDateTime = null;
                existing.DeleteBy = Guid.Empty;
                existing.IsCancel = false;
                existing.CancelDateTime = null;
                existing.CancelBy = Guid.Empty;
                existing.UpdateDateTime = now;
                existing.UpdateBy = actorUserId;
            }
        }

        private WorkforceOrganizationAssignmentResponse MapResponse(
            WfpOrganizationAssignment entity,
            MstWorkforceProfile profile)
        {
            var today = DateTime.UtcNow.Date;
            var isExpired = entity.EffectiveEndDate.HasValue &&
                entity.EffectiveEndDate.Value.Date < today;

            return new WorkforceOrganizationAssignmentResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                ProfileCode = profile.ProfileCode,
                DisplayName = profile.DisplayName,
                DepartmentId = entity.DepartmentId,
                DepartmentCode = entity.Department?.DepartmentCode ?? string.Empty,
                DepartmentName = entity.Department?.DepartmentName ?? string.Empty,
                PositionId = entity.PositionId,
                PositionCode = entity.Position?.PositionCode ?? string.Empty,
                PositionName = entity.Position?.PositionName ?? string.Empty,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsExpired = isExpired,
                IsCurrentlyValid = entity.IsActive &&
                    entity.EffectiveStartDate.Date <= today &&
                    !isExpired,
                Description = entity.Description,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private WorkforceOrganizationAssignmentDetailResponse MapDetailResponse(
            WfpOrganizationAssignment entity,
            MstWorkforceProfile profile)
        {
            var response = MapResponse(entity, profile);

            return new WorkforceOrganizationAssignmentDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                ProfileCode = response.ProfileCode,
                DisplayName = response.DisplayName,
                DepartmentId = response.DepartmentId,
                DepartmentCode = response.DepartmentCode,
                DepartmentName = response.DepartmentName,
                PositionId = response.PositionId,
                PositionCode = response.PositionCode,
                PositionName = response.PositionName,
                IsPrimary = response.IsPrimary,
                IsActive = response.IsActive,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                IsExpired = response.IsExpired,
                IsCurrentlyValid = response.IsCurrentlyValid,
                Description = response.Description,
                CreateDateTime = response.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime,
                CreateBy = entity.CreateBy,
                UpdateBy = entity.UpdateBy
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