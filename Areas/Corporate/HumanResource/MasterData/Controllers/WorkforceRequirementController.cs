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
        [AccessAction(
            "Read",
            "Read Workforce Requirement",
            Description = "Melihat data workforce requirement",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new WorkforceRequirementFilterMetadataResponse
            {
                DefaultFilter = new WorkforceRequirementDefaultFilterResponse(),
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
                UserTypes = new List<WorkforceRequirementUserTypeOptionResponse>
                {
                    new() { Value = UserType.Employee, Label = UserType.Employee.ToString() },
                    new() { Value = UserType.PermanentDoctor, Label = UserType.PermanentDoctor.ToString() },
                    new() { Value = UserType.GuestDoctor, Label = UserType.GuestDoctor.ToString() },
                    new() { Value = UserType.ExternalUser, Label = UserType.ExternalUser.ToString() }
                }
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
        [AccessAction(
            "Read",
            "Read Workforce Requirement Summary",
            Description = "Melihat ringkasan workforce requirement",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetSummary(
            [FromQuery] UserType? userType,
            [FromQuery] string? requirementCategory,
            [FromQuery] string? targetEntityName,
            [FromQuery] bool? isActive)
        {
            var query = BuildBaseQuery();

            query = ApplyFilters(
                query,
                search: null,
                userType,
                requirementCategory,
                targetEntityName,
                isRequired: null,
                isMultipleAllowed: null,
                isFileRequired: null,
                isNumberRequired: null,
                isIssueDateRequired: null,
                isExpiredDateRequired: null,
                isVerificationRequired: null,
                isProfileRequired: null,
                isActive
            );

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
        [AccessAction(
            "Read",
            "Read Workforce Requirement",
            Description = "Melihat data workforce requirement",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetWorkforceRequirements(
            [FromQuery] string? search,
            [FromQuery] UserType? userType,
            [FromQuery] string? requirementCategory,
            [FromQuery] string? targetEntityName,
            [FromQuery] bool? isRequired,
            [FromQuery] bool? isMultipleAllowed,
            [FromQuery] bool? isFileRequired,
            [FromQuery] bool? isNumberRequired,
            [FromQuery] bool? isIssueDateRequired,
            [FromQuery] bool? isExpiredDateRequired,
            [FromQuery] bool? isVerificationRequired,
            [FromQuery] bool? isProfileRequired,
            [FromQuery] bool? isActive,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilters(
                query,
                search,
                userType,
                requirementCategory,
                targetEntityName,
                isRequired,
                isMultipleAllowed,
                isFileRequired,
                isNumberRequired,
                isIssueDateRequired,
                isExpiredDateRequired,
                isVerificationRequired,
                isProfileRequired,
                isActive
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapResponse)
                .ToList();

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
        [ProducesResponseType(typeof(ApiResponse<List<WorkforceRequirementOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Workforce Requirement Options",
            Description = "Melihat pilihan workforce requirement",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            [FromQuery] UserType? userType,
            [FromQuery] string? requirementCategory,
            [FromQuery] string? targetEntityName,
            [FromQuery] bool? isRequired,
            [FromQuery] bool onlyActive = true)
        {
            var query = BuildBaseQuery();

            query = ApplyFilters(
                query,
                search,
                userType,
                requirementCategory,
                targetEntityName,
                isRequired,
                isMultipleAllowed: null,
                isFileRequired: null,
                isNumberRequired: null,
                isIssueDateRequired: null,
                isExpiredDateRequired: null,
                isVerificationRequired: null,
                isProfileRequired: null,
                isActive: onlyActive ? true : null
            );

            var data = await query
                .OrderBy(x => x.UserType)
                .ThenBy(x => x.RequirementCategory)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.RequirementName)
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

            return Ok(ApiResponse<List<WorkforceRequirementOptionResponse>>.Ok(
                data,
                "Data pilihan workforce requirement berhasil diambil."
            ));
        }

        [HttpGet("{workforceRequirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Workforce Requirement Detail",
            Description = "Melihat detail workforce requirement",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("WorkforceRequirement", "Read")]
        public async Task<IActionResult> GetWorkforceRequirementById(Guid workforceRequirementId)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == workforceRequirementId);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            var data = MapDetailResponse(entity);

            return Ok(ApiResponse<WorkforceRequirementDetailResponse>.Ok(
                data,
                "Detail workforce requirement berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Create",
            "Create Workforce Requirement",
            Description = "Membuat workforce requirement",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("WorkforceRequirement", "Create")]
        public async Task<IActionResult> CreateWorkforceRequirement([FromBody] CreateWorkforceRequirementRequest request)
        {
            var validation = await ValidateRequestAsync(
                request.UserType,
                request.RequirementCategory,
                request.RequirementCode,
                request.RequirementName,
                request.TargetEntityName,
                existingId: null
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstWorkforceRequirement
            {
                Id = Guid.NewGuid(),
                UserType = request.UserType,
                RequirementCategory = NormalizeRequiredText(request.RequirementCategory),
                RequirementCode = NormalizeRequiredText(request.RequirementCode).ToUpperInvariant(),
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
                IsActive = request.IsActive,
                Description = NormalizeNullableText(request.Description),
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstWorkforceRequirements.Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.CreateWorkforceRequirement",
                "Workforce requirement berhasil dibuat.",
                new { entity.Id, entity.UserType, entity.RequirementCategory, entity.RequirementCode }
            );

            return await GetWorkforceRequirementById(entity.Id);
        }

        [HttpPut("{workforceRequirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Requirement",
            Description = "Mengubah workforce requirement",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("WorkforceRequirement", "Update")]
        public async Task<IActionResult> UpdateWorkforceRequirement(
            Guid workforceRequirementId,
            [FromBody] UpdateWorkforceRequirementRequest request)
        {
            var entity = await _dbContext.MstWorkforceRequirements
                .FirstOrDefaultAsync(x => x.Id == workforceRequirementId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                request.UserType,
                request.RequirementCategory,
                request.RequirementCode,
                request.RequirementName,
                request.TargetEntityName,
                workforceRequirementId
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.Message
                ));
            }

            var now = DateTime.UtcNow;

            entity.UserType = request.UserType;
            entity.RequirementCategory = NormalizeRequiredText(request.RequirementCategory);
            entity.RequirementCode = NormalizeRequiredText(request.RequirementCode).ToUpperInvariant();
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
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.UpdateWorkforceRequirement",
                "Workforce requirement berhasil diubah.",
                new { entity.Id, entity.UserType, entity.RequirementCategory, entity.RequirementCode }
            );

            return await GetWorkforceRequirementById(entity.Id);
        }

        [HttpPatch("{workforceRequirementId:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceRequirementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Workforce Requirement Status",
            Description = "Mengubah status workforce requirement",
            AccessType = AccessTypes.Update,
            SortOrder = 4
        )]
        [AccessPermission("WorkforceRequirement", "Update")]
        public async Task<IActionResult> UpdateWorkforceRequirementStatus(
            Guid workforceRequirementId,
            [FromBody] UpdateWorkforceRequirementStatusRequest request)
        {
            var entity = await _dbContext.MstWorkforceRequirements
                .FirstOrDefaultAsync(x => x.Id == workforceRequirementId && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Workforce requirement tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.Description))
            {
                entity.Description = request.Description.Trim();
            }

            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.UpdateWorkforceRequirementStatus",
                "Status workforce requirement berhasil diubah.",
                new { entity.Id, entity.IsActive }
            );

            return await GetWorkforceRequirementById(entity.Id);
        }

        [HttpDelete("{workforceRequirementId:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Workforce Requirement",
            Description = "Menghapus workforce requirement",
            AccessType = AccessTypes.Delete,
            SortOrder = 5
        )]
        [AccessPermission("WorkforceRequirement", "Delete")]
        public async Task<IActionResult> DeleteWorkforceRequirement(
            Guid workforceRequirementId,
            [FromBody] DeleteWorkforceRequirementRequest? request = null)
        {
            var entity = await _dbContext.MstWorkforceRequirements
                .FirstOrDefaultAsync(x => x.Id == workforceRequirementId && !x.IsDelete);

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
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceRequirement.DeleteWorkforceRequirement",
                "Workforce requirement berhasil dihapus.",
                new { entity.Id, entity.UserType, entity.RequirementCategory, entity.RequirementCode }
            );

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Workforce requirement berhasil dihapus."
            ));
        }

        private IQueryable<MstWorkforceRequirement> BuildBaseQuery()
        {
            return _dbContext.MstWorkforceRequirements
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstWorkforceRequirement> ApplyFilters(
            IQueryable<MstWorkforceRequirement> query,
            string? search,
            UserType? userType,
            string? requirementCategory,
            string? targetEntityName,
            bool? isRequired,
            bool? isMultipleAllowed,
            bool? isFileRequired,
            bool? isNumberRequired,
            bool? isIssueDateRequired,
            bool? isExpiredDateRequired,
            bool? isVerificationRequired,
            bool? isProfileRequired,
            bool? isActive)
        {
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

            if (userType.HasValue)
            {
                query = query.Where(x => x.UserType == userType.Value);
            }

            if (!string.IsNullOrWhiteSpace(requirementCategory))
            {
                var selectedCategory = requirementCategory.Trim().ToLower();
                query = query.Where(x => x.RequirementCategory.ToLower() == selectedCategory);
            }

            if (!string.IsNullOrWhiteSpace(targetEntityName))
            {
                var selectedTargetEntity = targetEntityName.Trim().ToLower();
                query = query.Where(x =>
                    x.TargetEntityName != null &&
                    x.TargetEntityName.ToLower() == selectedTargetEntity);
            }

            if (isRequired.HasValue)
            {
                query = query.Where(x => x.IsRequired == isRequired.Value);
            }

            if (isMultipleAllowed.HasValue)
            {
                query = query.Where(x => x.IsMultipleAllowed == isMultipleAllowed.Value);
            }

            if (isFileRequired.HasValue)
            {
                query = query.Where(x => x.IsFileRequired == isFileRequired.Value);
            }

            if (isNumberRequired.HasValue)
            {
                query = query.Where(x => x.IsNumberRequired == isNumberRequired.Value);
            }

            if (isIssueDateRequired.HasValue)
            {
                query = query.Where(x => x.IsIssueDateRequired == isIssueDateRequired.Value);
            }

            if (isExpiredDateRequired.HasValue)
            {
                query = query.Where(x => x.IsExpiredDateRequired == isExpiredDateRequired.Value);
            }

            if (isVerificationRequired.HasValue)
            {
                query = query.Where(x => x.IsVerificationRequired == isVerificationRequired.Value);
            }

            if (isProfileRequired.HasValue)
            {
                query = query.Where(x => x.IsProfileRequired == isProfileRequired.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            return query;
        }

        private static IQueryable<MstWorkforceRequirement> ApplySorting(
            IQueryable<MstWorkforceRequirement> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLower() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "usertype" => isDescending
                    ? query.OrderByDescending(x => x.UserType)
                        .ThenBy(x => x.RequirementCategory)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.UserType)
                        .ThenBy(x => x.RequirementCategory)
                        .ThenBy(x => x.SortOrder),

                "requirementcategory" => isDescending
                    ? query.OrderByDescending(x => x.RequirementCategory)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.RequirementCategory)
                        .ThenBy(x => x.SortOrder),

                "requirementcode" => isDescending
                    ? query.OrderByDescending(x => x.RequirementCode)
                    : query.OrderBy(x => x.RequirementCode),

                "requirementname" => isDescending
                    ? query.OrderByDescending(x => x.RequirementName)
                    : query.OrderBy(x => x.RequirementName),

                "targetentityname" => isDescending
                    ? query.OrderByDescending(x => x.TargetEntityName)
                    : query.OrderBy(x => x.TargetEntityName),

                "isrequired" => isDescending
                    ? query.OrderByDescending(x => x.IsRequired)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.IsRequired)
                        .ThenBy(x => x.SortOrder),

                "isfilerequired" => isDescending
                    ? query.OrderByDescending(x => x.IsFileRequired)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.IsFileRequired)
                        .ThenBy(x => x.SortOrder),

                "isverificationrequired" => isDescending
                    ? query.OrderByDescending(x => x.IsVerificationRequired)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.IsVerificationRequired)
                        .ThenBy(x => x.SortOrder),

                "isprofilerequired" => isDescending
                    ? query.OrderByDescending(x => x.IsProfileRequired)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.IsProfileRequired)
                        .ThenBy(x => x.SortOrder),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                        .ThenBy(x => x.SortOrder)
                    : query.OrderBy(x => x.IsActive)
                        .ThenBy(x => x.SortOrder),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.UserType)
                        .ThenByDescending(x => x.RequirementCategory)
                        .ThenByDescending(x => x.RequirementName)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.UserType)
                        .ThenBy(x => x.RequirementCategory)
                        .ThenBy(x => x.RequirementName)
            };
        }

        private async Task<(bool IsValid, string Message)> ValidateRequestAsync(
            UserType userType,
            string requirementCategory,
            string requirementCode,
            string requirementName,
            string? targetEntityName,
            Guid? existingId)
        {
            if (!IsValidWorkforceUserType(userType))
            {
                return (false, "UserType hanya boleh Employee, PermanentDoctor, GuestDoctor, atau ExternalUser.");
            }

            if (string.IsNullOrWhiteSpace(requirementCategory))
            {
                return (false, "RequirementCategory wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(requirementCode))
            {
                return (false, "RequirementCode wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(requirementName))
            {
                return (false, "RequirementName wajib diisi.");
            }

            var normalizedCategory = NormalizeRequiredText(requirementCategory);
            var normalizedCode = NormalizeRequiredText(requirementCode).ToUpperInvariant();
            var normalizedTarget = NormalizeNullableText(targetEntityName);

            var duplicateQuery = _dbContext.MstWorkforceRequirements
                .AsNoTracking()
                .Where(x =>
                    x.UserType == userType &&
                    x.RequirementCategory.ToLower() == normalizedCategory.ToLower() &&
                    x.RequirementCode.ToLower() == normalizedCode.ToLower() &&
                    !x.IsDelete);

            if (normalizedTarget == null)
            {
                duplicateQuery = duplicateQuery.Where(x => x.TargetEntityName == null);
            }
            else
            {
                duplicateQuery = duplicateQuery.Where(x =>
                    x.TargetEntityName != null &&
                    x.TargetEntityName.ToLower() == normalizedTarget.ToLower());
            }

            if (existingId.HasValue)
            {
                duplicateQuery = duplicateQuery.Where(x => x.Id != existingId.Value);
            }

            var duplicateExists = await duplicateQuery.AnyAsync();

            if (duplicateExists)
            {
                return (false, "Workforce requirement dengan UserType, RequirementCategory, RequirementCode, dan TargetEntityName yang sama sudah ada.");
            }

            return (true, string.Empty);
        }

        private static bool IsValidWorkforceUserType(UserType userType)
        {
            return userType == UserType.Employee ||
                   userType == UserType.PermanentDoctor ||
                   userType == UserType.GuestDoctor ||
                   userType == UserType.ExternalUser;
        }

        private static WorkforceRequirementResponse MapResponse(MstWorkforceRequirement entity)
        {
            return new WorkforceRequirementResponse
            {
                Id = entity.Id,
                UserType = entity.UserType,
                UserTypeName = entity.UserType.ToString(),
                RequirementCategory = entity.RequirementCategory,
                RequirementCode = entity.RequirementCode,
                RequirementName = entity.RequirementName,
                IsRequired = entity.IsRequired,
                IsMultipleAllowed = entity.IsMultipleAllowed,
                IsFileRequired = entity.IsFileRequired,
                IsNumberRequired = entity.IsNumberRequired,
                IsIssueDateRequired = entity.IsIssueDateRequired,
                IsExpiredDateRequired = entity.IsExpiredDateRequired,
                IsVerificationRequired = entity.IsVerificationRequired,
                IsProfileRequired = entity.IsProfileRequired,
                TargetEntityName = entity.TargetEntityName,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
            };
        }

        private static WorkforceRequirementDetailResponse MapDetailResponse(MstWorkforceRequirement entity)
        {
            return new WorkforceRequirementDetailResponse
            {
                Id = entity.Id,
                UserType = entity.UserType,
                UserTypeName = entity.UserType.ToString(),
                RequirementCategory = entity.RequirementCategory,
                RequirementCode = entity.RequirementCode,
                RequirementName = entity.RequirementName,
                IsRequired = entity.IsRequired,
                IsMultipleAllowed = entity.IsMultipleAllowed,
                IsFileRequired = entity.IsFileRequired,
                IsNumberRequired = entity.IsNumberRequired,
                IsIssueDateRequired = entity.IsIssueDateRequired,
                IsExpiredDateRequired = entity.IsExpiredDateRequired,
                IsVerificationRequired = entity.IsVerificationRequired,
                IsProfileRequired = entity.IsProfileRequired,
                TargetEntityName = entity.TargetEntityName,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                CreateBy = entity.CreateBy,
                UpdateBy = entity.UpdateBy
            };
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0)
            {
                pageNumber = 1;
            }

            if (pageSize <= 0)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }
    }
}