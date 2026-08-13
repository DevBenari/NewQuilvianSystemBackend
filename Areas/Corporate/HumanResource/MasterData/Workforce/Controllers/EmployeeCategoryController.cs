using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using EmployeeCategoryPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs.EmployeeCategoryResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/employee-categories")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Employee Category",
        AreaName = "Corporate",
        ControllerName = "EmployeeCategory",
        Description = "Corporate human resource master data employee category",
        SortOrder = 77)]
    [Tags("Corporate / Human Resource / Master Data / Employee Category")]
    public class EmployeeCategoryController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "ECT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmployeeCategoryController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeCategoryFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employee Category", Description = "Melihat metadata filter employee category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeCategory", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new EmployeeCategoryFilterMetadataResponse
            {
                DefaultFilter = new EmployeeCategoryDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<EmployeeCategorySortOptionResponse>
                {
                    new() { Value = "employeeCategoryCode", Label = "Kode kategori karyawan" },
                    new() { Value = "employeeCategoryName", Label = "Nama kategori karyawan" },
                    new() { Value = "workforceTypeName", Label = "Tipe tenaga kerja" },
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "EmployeeCategory.GetFilterMetadata", "Mengambil metadata filter employee category.", result);
            return Ok(ApiResponse<EmployeeCategoryFilterMetadataResponse>.Ok(result, "Metadata filter employee category berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeCategorySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employee Category", Description = "Melihat ringkasan employee category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeCategory", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new EmployeeCategorySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                ClinicalData = await query.CountAsync(x => x.IsClinical),
                RequiresCredentialingData = await query.CountAsync(x => x.RequiresCredentialing),
                WithoutWorkforceTypeData = await query.CountAsync(x => !x.WorkforceTypeId.HasValue)
            };
            return Ok(ApiResponse<EmployeeCategorySummaryResponse>.Ok(result, "Ringkasan employee category berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EmployeeCategoryPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employee Category", Description = "Melihat data employee category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeCategory", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? workforceTypeId,
            [FromQuery] bool? isClinical,
            [FromQuery] bool? requiresCredentialing,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "employeeCategoryName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), workforceTypeId, isClinical, requiresCredentialing, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<EmployeeCategoryPagedResult>.Ok(new EmployeeCategoryPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data employee category berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeCategoryOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employee Category", Description = "Melihat pilihan employee category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeCategory", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? workforceTypeId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), workforceTypeId, null, null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder).ThenBy(x => x.EmployeeCategoryName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new EmployeeCategoryOptionResponse
                {
                    Id = x.Id,
                    EmployeeCategoryCode = x.EmployeeCategoryCode,
                    EmployeeCategoryName = x.EmployeeCategoryName,
                    WorkforceTypeId = x.WorkforceTypeId,
                    WorkforceTypeName = x.WorkforceType != null ? x.WorkforceType.WorkforceTypeName : null,
                    IsClinical = x.IsClinical
                })
                .ToListAsync();

            return Ok(ApiResponse<EmployeeCategoryOptionPagedResponse>.Ok(new EmployeeCategoryOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan employee category berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmployeeCategoryDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Employee Category", Description = "Melihat detail employee category", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeCategory", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee category tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new EmployeeCategoryDetailResponse
            {
                Id = response.Id,
                EmployeeCategoryCode = response.EmployeeCategoryCode,
                EmployeeCategoryName = response.EmployeeCategoryName,
                Description = response.Description,
                WorkforceTypeId = response.WorkforceTypeId,
                WorkforceTypeCode = response.WorkforceTypeCode,
                WorkforceTypeName = response.WorkforceTypeName,
                IsClinical = response.IsClinical,
                RequiresCredentialing = response.RequiresCredentialing,
                SortOrder = response.SortOrder,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<EmployeeCategoryDetailResponse>.Ok(result, "Detail employee category berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Employee Category", Description = "Membuat employee category", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmployeeCategory", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeCategoryRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload employee category wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstEmployeeCategory
            {
                Id = Guid.NewGuid(),
                EmployeeCategoryCode = await GenerateCodeAsync(),
                EmployeeCategoryName = request.EmployeeCategoryName.Trim(),
                Description = NormalizeText(request.Description),
                WorkforceTypeId = NormalizeGuid(request.WorkforceTypeId),
                IsClinical = request.IsClinical,
                RequiresCredentialing = request.RequiresCredentialing,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmployeeCategory>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeCategory.Create",
                "Membuat data employee category.",
                new { entity.Id, entity.EmployeeCategoryCode, entity.EmployeeCategoryName, entity.WorkforceTypeId, entity.IsActive, entity.CreateDateTime, entity.CreateBy });

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.EmployeeCategoryCode, entity.EmployeeCategoryName }, "Employee category berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Employee Category", Description = "Mengubah employee category", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeCategory", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeCategoryRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload employee category wajib diisi."));

            var entity = await _dbContext.Set<MstEmployeeCategory>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee category tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.EmployeeCategoryName = request.EmployeeCategoryName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.WorkforceTypeId = NormalizeGuid(request.WorkforceTypeId);
            entity.IsClinical = request.IsClinical;
            entity.RequiresCredentialing = request.RequiresCredentialing;
            entity.SortOrder = request.SortOrder ?? entity.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeCategory.Update",
                "Mengubah data employee category.",
                new { entity.Id, entity.EmployeeCategoryCode, entity.EmployeeCategoryName, entity.WorkforceTypeId, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy });

            return Ok(ApiResponse<object>.Ok(null, "Employee category berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Employee Category Status", Description = "Mengubah status employee category", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmployeeCategory", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmployeeCategoryStatusRequest request)
        {
            var entity = await _dbContext.Set<MstEmployeeCategory>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee category tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status employee category berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Employee Category", Description = "Menghapus employee category", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmployeeCategory", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstEmployeeCategory>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employee category tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmployeeCategory.Delete",
                "Menghapus data employee category.",
                new { entity.Id, entity.EmployeeCategoryCode, entity.EmployeeCategoryName, entity.DeleteDateTime, entity.DeleteBy });

            return Ok(ApiResponse<object>.Ok(null, "Employee category berhasil dihapus."));
        }

        private IQueryable<MstEmployeeCategory> BaseQuery() =>
            _dbContext.Set<MstEmployeeCategory>().AsNoTracking().Include(x => x.WorkforceType).Where(x => !x.IsDelete);

        private static IQueryable<MstEmployeeCategory> ApplyFilter(
            IQueryable<MstEmployeeCategory> query, Guid? workforceTypeId, bool? isClinical, bool? requiresCredentialing, bool? isActive, string? search)
        {
            if (workforceTypeId.HasValue && workforceTypeId.Value != Guid.Empty) query = query.Where(x => x.WorkforceTypeId == workforceTypeId.Value);
            if (isClinical.HasValue) query = query.Where(x => x.IsClinical == isClinical.Value);
            if (requiresCredentialing.HasValue) query = query.Where(x => x.RequiresCredentialing == requiresCredentialing.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EmployeeCategoryCode.ToLower().Contains(keyword) ||
                    x.EmployeeCategoryName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.WorkforceType != null && x.WorkforceType.WorkforceTypeName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstEmployeeCategory> ApplySorting(IQueryable<MstEmployeeCategory> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "employeeCategoryName").Trim().ToLowerInvariant() switch
            {
                "employeecategorycode" => desc ? query.OrderByDescending(x => x.EmployeeCategoryCode) : query.OrderBy(x => x.EmployeeCategoryCode),
                "workforcetypename" => desc
                    ? query.OrderByDescending(x => x.WorkforceType != null ? x.WorkforceType.WorkforceTypeName : string.Empty).ThenBy(x => x.EmployeeCategoryName)
                    : query.OrderBy(x => x.WorkforceType != null ? x.WorkforceType.WorkforceTypeName : string.Empty).ThenBy(x => x.EmployeeCategoryName),
                "sortorder" => desc
                    ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.EmployeeCategoryName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.EmployeeCategoryName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.EmployeeCategoryName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.EmployeeCategoryName),
                _ => desc ? query.OrderByDescending(x => x.EmployeeCategoryName) : query.OrderBy(x => x.EmployeeCategoryName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateEmployeeCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmployeeCategoryName)) return (false, "Nama employee category wajib diisi.");
            if (request.SortOrder.HasValue && request.SortOrder.Value < 0) return (false, "Urutan tampil tidak boleh kurang dari 0.");

            var workforceTypeId = NormalizeGuid(request.WorkforceTypeId);
            if (workforceTypeId.HasValue)
            {
                var exists = await _dbContext.Set<MstWorkforceType>().AsNoTracking()
                    .AnyAsync(x => x.Id == workforceTypeId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Workforce type tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.EmployeeCategoryName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstEmployeeCategory>().AsNoTracking()
                .Where(x => !x.IsDelete && x.EmployeeCategoryName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama employee category sudah digunakan.");
            return (true, null);
        }

        private static EmployeeCategoryResponse MapResponse(MstEmployeeCategory x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            EmployeeCategoryCode = x.EmployeeCategoryCode,
            EmployeeCategoryName = x.EmployeeCategoryName,
            Description = x.Description,
            WorkforceTypeId = x.WorkforceTypeId,
            WorkforceTypeCode = x.WorkforceType?.WorkforceTypeCode,
            WorkforceTypeName = x.WorkforceType?.WorkforceTypeName,
            IsClinical = x.IsClinical,
            RequiresCredentialing = x.RequiresCredentialing,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstEmployeeCategory>().AsNoTracking()
                .Where(x => !x.IsDelete && x.EmployeeCategoryCode.StartsWith(CodePrefix))
                .Select(x => x.EmployeeCategoryCode).ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1; while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> ids)
        {
            var actorIds = ids.Where(x => x != Guid.Empty).Distinct().ToList();
            return await _dbContext.Users.AsNoTracking().Where(x => actorIds.Contains(x.Id))
                .Select(x => new { x.Id, Name = (string?)(x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode) })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string?> actors, Guid id) =>
            id == Guid.Empty ? null : actors.TryGetValue(id, out var name) ? name : null;

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<EmployeeCategoryCustomPeriodOptionResponse> BuildPeriodOptions() => new()
        {
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" },
            new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
    }
}
