using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Enums;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseWorkforceRequirementPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.DTOs.WorkforceRequirementResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/workforce-requirements")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Requirement",
        AreaName = "Corporate",
        ControllerName = "WorkforceRequirement",
        Description = "Corporate human resource master data workforce requirement",
        SortOrder = 7
    )]
    [Tags("Corporate / Human Resource / Master Data / Workforce Requirement")]
    public class WorkforceRequirementController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "WFR-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly List<string> RequirementCategoryOptions = new()
        {
            "Document",
            "Education",
            "Training",
            "Certification",
            "License",
            "ClinicalPrivilege",
            "HealthRecord",
            "BankAccount",
            "TransportAllowance",
            "Payroll",
            "Tax",
            "Insurance",
            "Organization",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceRequirementController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Requirement", Description = "Melihat metadata filter workforce requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new WorkforceRequirementFilterMetadataResponse
            {
                DefaultFilter = new WorkforceRequirementDefaultFilterResponse(),
                CustomPeriods = new List<WorkforceRequirementCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<WorkforceRequirementSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "userType", Label = "Tipe user" },
                    new() { Value = "requirementCategory", Label = "Kategori requirement" },
                    new() { Value = "requirementCode", Label = "Kode requirement" },
                    new() { Value = "requirementName", Label = "Nama requirement" },
                    new() { Value = "targetEntityName", Label = "Target entity" },
                    new() { Value = "isRequired", Label = "Wajib" },
                    new() { Value = "isFileRequired", Label = "Wajib file" },
                    new() { Value = "isVerificationRequired", Label = "Wajib verifikasi" },
                    new() { Value = "isProfileRequired", Label = "Wajib profile" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                UserTypes = BuildUserTypeOptions(),
                RequirementCategories = RequirementCategoryOptions
                    .OrderBy(x => x)
                    .ToList(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.GetFilterMetadata",
                "Mengambil metadata filter workforce requirement.",
                result
            );

            return Ok(ApiResponse<WorkforceRequirementFilterMetadataResponse>.Ok(
                result,
                "Metadata filter workforce requirement berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Requirement Summary", Description = "Melihat ringkasan workforce requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new WorkforceRequirementSummaryResponse
            {
                TotalRequirement = await query.CountAsync(),
                ActiveRequirement = await query.CountAsync(x => x.IsActive),
                InactiveRequirement = await query.CountAsync(x => !x.IsActive),
                RequiredRequirement = await query.CountAsync(x => x.IsRequired),
                OptionalRequirement = await query.CountAsync(x => !x.IsRequired),
                MultipleAllowedRequirement = await query.CountAsync(x => x.IsMultipleAllowed),
                FileRequiredRequirement = await query.CountAsync(x => x.IsFileRequired),
                NumberRequiredRequirement = await query.CountAsync(x => x.IsNumberRequired),
                IssueDateRequiredRequirement = await query.CountAsync(x => x.IsIssueDateRequired),
                ExpiredDateRequiredRequirement = await query.CountAsync(x => x.IsExpiredDateRequired),
                VerificationRequiredRequirement = await query.CountAsync(x => x.IsVerificationRequired),
                ProfileRequiredRequirement = await query.CountAsync(x => x.IsProfileRequired),
                EmployeeRequirement = await query.CountAsync(x => x.UserType == UserType.Employee),
                PermanentDoctorRequirement = await query.CountAsync(x => x.UserType == UserType.PermanentDoctor),
                GuestDoctorRequirement = await query.CountAsync(x => x.UserType == UserType.GuestDoctor),
                ExternalUserRequirement = await query.CountAsync(x => x.UserType == UserType.ExternalUser)
            };

            return Ok(ApiResponse<WorkforceRequirementSummaryResponse>.Ok(
                result,
                "Ringkasan workforce requirement berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseWorkforceRequirementPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Requirement", Description = "Melihat data workforce requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetWorkforceRequirements(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] UserType? userType,
            [FromQuery] string? requirementCategory,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(
                query,
                userType,
                requirementCategory,
                isActive,
                search
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceRequirementResponse
                {
                    Id = x.Id,
                    UserType = x.UserType,
                    UserTypeName = x.UserType.ToString(),
                    RequirementCategory = x.RequirementCategory,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    IsRequired = x.IsRequired,
                    IsMultipleAllowed = x.IsMultipleAllowed,
                    IsFileRequired = x.IsFileRequired,
                    IsNumberRequired = x.IsNumberRequired,
                    IsIssueDateRequired = x.IsIssueDateRequired,
                    IsExpiredDateRequired = x.IsExpiredDateRequired,
                    IsVerificationRequired = x.IsVerificationRequired,
                    IsProfileRequired = x.IsProfileRequired,
                    TargetEntityName = x.TargetEntityName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync();

            var result = new ResponseWorkforceRequirementPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseWorkforceRequirementPagedResult>.Ok(
                result,
                "Data workforce requirement berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Requirement Options", Description = "Melihat pilihan workforce requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] UserType? userType,
            [FromQuery] string? requirementCategory,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                userType,
                requirementCategory,
                onlyActive ? true : null,
                search
            );

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.UserType)
                .ThenBy(x => x.RequirementCategory)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.RequirementName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkforceRequirementOptionResponse
                {
                    Id = x.Id,
                    UserType = x.UserType,
                    UserTypeName = x.UserType.ToString(),
                    RequirementCategory = x.RequirementCategory,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    IsRequired = x.IsRequired,
                    IsMultipleAllowed = x.IsMultipleAllowed,
                    IsFileRequired = x.IsFileRequired,
                    IsNumberRequired = x.IsNumberRequired,
                    IsIssueDateRequired = x.IsIssueDateRequired,
                    IsExpiredDateRequired = x.IsExpiredDateRequired,
                    IsVerificationRequired = x.IsVerificationRequired,
                    IsProfileRequired = x.IsProfileRequired,
                    TargetEntityName = x.TargetEntityName,
                    SortOrder = x.SortOrder
                })
                .ToListAsync();

            var result = new WorkforceRequirementOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkforceRequirementOptionPagedResponse>.Ok(
                result,
                "Data pilihan workforce requirement berhasil diambil."
            ));
        }

        [HttpGet("{workforceRequirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Requirement Detail", Description = "Melihat detail workforce requirement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetWorkforceRequirementById(Guid workforceRequirementId)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == workforceRequirementId)
                .Select(x => new WorkforceRequirementDetailResponse
                {
                    Id = x.Id,
                    UserType = x.UserType,
                    UserTypeName = x.UserType.ToString(),
                    RequirementCategory = x.RequirementCategory,
                    RequirementCode = x.RequirementCode,
                    RequirementName = x.RequirementName,
                    IsRequired = x.IsRequired,
                    IsMultipleAllowed = x.IsMultipleAllowed,
                    IsFileRequired = x.IsFileRequired,
                    IsNumberRequired = x.IsNumberRequired,
                    IsIssueDateRequired = x.IsIssueDateRequired,
                    IsExpiredDateRequired = x.IsExpiredDateRequired,
                    IsVerificationRequired = x.IsVerificationRequired,
                    IsProfileRequired = x.IsProfileRequired,
                    TargetEntityName = x.TargetEntityName,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault(),

                    Description = x.Description,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : (Guid?)x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u =>
                                u.DisplayName ??
                                u.UserName ??
                                u.Email ??
                                u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<WorkforceRequirementDetailResponse>.Ok(
                data,
                "Detail workforce requirement berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Requirement", Description = "Membuat workforce requirement", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceRequirement", "Create")]
        public async Task<IActionResult> CreateWorkforceRequirement([FromBody] CreateWorkforceRequirementRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workforce requirement tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstWorkforceRequirement
            {
                Id = Guid.NewGuid(),
                UserType = request.UserType,
                RequirementCategory = NormalizeRequiredText(request.RequirementCategory),
                RequirementCode = await GenerateRequirementCodeAsync(),
                RequirementName = NormalizeRequiredText(request.RequirementName),
                IsRequired = request.IsRequired,
                IsMultipleAllowed = request.IsMultipleAllowed,
                IsFileRequired = request.IsFileRequired,
                IsNumberRequired = request.IsNumberRequired,
                IsIssueDateRequired = request.IsIssueDateRequired,
                IsExpiredDateRequired = request.IsExpiredDateRequired,
                IsVerificationRequired = request.IsVerificationRequired,
                IsProfileRequired = request.IsProfileRequired,
                TargetEntityName = NormalizeNullableText(request.TargetEntityName),
                SortOrder = request.SortOrder,
                IsActive = true,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkforceRequirement>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new WorkforceRequirementCreateResponse
            {
                Id = entity.Id,
                UserType = entity.UserType,
                UserTypeName = entity.UserType.ToString(),
                RequirementCategory = entity.RequirementCategory,
                RequirementCode = entity.RequirementCode,
                RequirementName = entity.RequirementName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.CreateWorkforceRequirement",
                "Membuat data workforce requirement.",
                result
            );

            return Ok(ApiResponse<WorkforceRequirementCreateResponse>.Ok(
                result,
                "Workforce requirement berhasil dibuat."
            ));
        }

        [HttpPut("{workforceRequirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Requirement", Description = "Mengubah workforce requirement", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceRequirement", "Update")]
        public async Task<IActionResult> UpdateWorkforceRequirement(
            Guid workforceRequirementId,
            [FromBody] UpdateWorkforceRequirementRequest request)
        {
            var entity = await _dbContext.Set<MstWorkforceRequirement>()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceRequirementId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(workforceRequirementId, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data workforce requirement tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.UserType = request.UserType;
            entity.RequirementCategory = NormalizeRequiredText(request.RequirementCategory);
            entity.RequirementName = NormalizeRequiredText(request.RequirementName);
            entity.IsRequired = request.IsRequired;
            entity.IsMultipleAllowed = request.IsMultipleAllowed;
            entity.IsFileRequired = request.IsFileRequired;
            entity.IsNumberRequired = request.IsNumberRequired;
            entity.IsIssueDateRequired = request.IsIssueDateRequired;
            entity.IsExpiredDateRequired = request.IsExpiredDateRequired;
            entity.IsVerificationRequired = request.IsVerificationRequired;
            entity.IsProfileRequired = request.IsProfileRequired;
            entity.TargetEntityName = NormalizeNullableText(request.TargetEntityName);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.Description = NormalizeNullableText(request.Description);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.UpdateWorkforceRequirement",
                "Mengubah data workforce requirement.",
                new
                {
                    entity.Id,
                    entity.UserType,
                    entity.RequirementCategory,
                    entity.RequirementCode,
                    entity.RequirementName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Workforce requirement berhasil diperbarui."
            ));
        }

        [HttpPatch("{workforceRequirementId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Requirement Status", Description = "Mengubah status workforce requirement", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceRequirement", "Update")]
        public async Task<IActionResult> UpdateWorkforceRequirementStatus(
            Guid workforceRequirementId,
            [FromBody] UpdateWorkforceRequirementStatusRequest request)
        {
            var entity = await _dbContext.Set<MstWorkforceRequirement>()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceRequirementId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = request.Description.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.UpdateWorkforceRequirementStatus",
                "Mengubah status workforce requirement.",
                new { entity.Id, entity.RequirementCode, entity.IsActive }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status workforce requirement berhasil diperbarui."
            ));
        }

        [HttpDelete("{workforceRequirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Requirement", Description = "Menghapus workforce requirement", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkforceRequirement", "Delete")]
        public async Task<IActionResult> DeleteWorkforceRequirement(
            Guid workforceRequirementId,
            [FromBody] DeleteWorkforceRequirementRequest? request = null)
        {
            var entity = await _dbContext.Set<MstWorkforceRequirement>()
                .FirstOrDefaultAsync(x =>
                    x.Id == workforceRequirementId &&
                    !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
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

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.DeleteWorkforceRequirement",
                "Menghapus data workforce requirement.",
                new
                {
                    entity.Id,
                    entity.UserType,
                    entity.RequirementCategory,
                    entity.RequirementCode,
                    entity.RequirementName,
                    entity.DeleteDateTime
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Workforce requirement berhasil dihapus."
            ));
        }

        private IQueryable<MstWorkforceRequirement> BuildBaseQuery()
        {
            return _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstWorkforceRequirement> ApplyDateFilter(
            IQueryable<MstWorkforceRequirement> query,
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

        private static IQueryable<MstWorkforceRequirement> ApplyStandardFilter(
            IQueryable<MstWorkforceRequirement> query,
            UserType? userType,
            string? requirementCategory,
            bool? isActive,
            string? search)
        {
            if (userType.HasValue)
                query = query.Where(x => x.UserType == userType.Value);

            if (!string.IsNullOrWhiteSpace(requirementCategory))
            {
                var selectedCategory = requirementCategory.Trim().ToLower();
                query = query.Where(x => x.RequirementCategory.ToLower() == selectedCategory);
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.RequirementCategory.ToLower().Contains(keyword) ||
                    x.RequirementCode.ToLower().Contains(keyword) ||
                    x.RequirementName.ToLower().Contains(keyword) ||
                    (x.TargetEntityName != null && x.TargetEntityName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstWorkforceRequirement> ApplySorting(
            IQueryable<MstWorkforceRequirement> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "usertype" => isDescending ? query.OrderByDescending(x => x.UserType).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.UserType).ThenBy(x => x.SortOrder),
                "requirementcategory" => isDescending ? query.OrderByDescending(x => x.RequirementCategory).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.RequirementCategory).ThenBy(x => x.SortOrder),
                "requirementcode" => isDescending ? query.OrderByDescending(x => x.RequirementCode) : query.OrderBy(x => x.RequirementCode),
                "requirementname" => isDescending ? query.OrderByDescending(x => x.RequirementName) : query.OrderBy(x => x.RequirementName),
                "targetentityname" => isDescending ? query.OrderByDescending(x => x.TargetEntityName) : query.OrderBy(x => x.TargetEntityName),
                "isrequired" => isDescending ? query.OrderByDescending(x => x.IsRequired).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.IsRequired).ThenBy(x => x.SortOrder),
                "isfilerequired" => isDescending ? query.OrderByDescending(x => x.IsFileRequired).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.IsFileRequired).ThenBy(x => x.SortOrder),
                "isverificationrequired" => isDescending ? query.OrderByDescending(x => x.IsVerificationRequired).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.IsVerificationRequired).ThenBy(x => x.SortOrder),
                "isprofilerequired" => isDescending ? query.OrderByDescending(x => x.IsProfileRequired).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.IsProfileRequired).ThenBy(x => x.SortOrder),
                "isactive" => isDescending ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.SortOrder) : query.OrderBy(x => x.IsActive).ThenBy(x => x.SortOrder),
                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.UserType).ThenByDescending(x => x.RequirementCategory).ThenByDescending(x => x.RequirementName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.UserType).ThenBy(x => x.RequirementCategory).ThenBy(x => x.RequirementName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateWorkforceRequirementRequest request)
        {
            if (!IsValidWorkforceUserType(request.UserType))
                return (false, "UserType hanya boleh Employee, PermanentDoctor, GuestDoctor, atau ExternalUser.");

            if (string.IsNullOrWhiteSpace(request.RequirementCategory))
                return (false, "Requirement category wajib diisi.");

            if (!RequirementCategoryOptions.Contains(request.RequirementCategory.Trim(), StringComparer.OrdinalIgnoreCase))
                return (false, "Requirement category tidak valid. Gunakan nilai dari endpoint filters/metadata.");

            if (string.IsNullOrWhiteSpace(request.RequirementName))
                return (false, "Requirement name wajib diisi.");

            var normalizedCategory = NormalizeRequiredText(request.RequirementCategory).ToLower();
            var normalizedName = NormalizeRequiredText(request.RequirementName).ToLower();
            var normalizedTarget = NormalizeComparableText(request.TargetEntityName);

            var duplicateQuery = _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.UserType == request.UserType &&
                    x.RequirementCategory.ToLower() == normalizedCategory &&
                    x.RequirementName.ToLower() == normalizedName &&
                    ((x.TargetEntityName ?? string.Empty).Trim().ToLower() == normalizedTarget));

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Workforce requirement dengan UserType, RequirementCategory, RequirementName, dan TargetEntityName yang sama sudah ada.");

            return (true, null);
        }

        private async Task<string> GenerateRequirementCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstWorkforceRequirement>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.RequirementCode.StartsWith(CodePrefix))
                .Select(x => x.RequirementCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;
            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static List<WorkforceRequirementUserTypeOptionResponse> BuildUserTypeOptions()
        {
            return new List<WorkforceRequirementUserTypeOptionResponse>
            {
                new() { Value = UserType.Employee, Label = UserType.Employee.ToString() },
                new() { Value = UserType.PermanentDoctor, Label = UserType.PermanentDoctor.ToString() },
                new() { Value = UserType.GuestDoctor, Label = UserType.GuestDoctor.ToString() },
                new() { Value = UserType.ExternalUser, Label = UserType.ExternalUser.ToString() }
            };
        }

        private static bool IsValidWorkforceUserType(UserType userType)
        {
            return userType == UserType.Employee ||
                   userType == UserType.PermanentDoctor ||
                   userType == UserType.GuestDoctor ||
                   userType == UserType.ExternalUser;
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

        private static string NormalizeComparableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim().ToLower();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}