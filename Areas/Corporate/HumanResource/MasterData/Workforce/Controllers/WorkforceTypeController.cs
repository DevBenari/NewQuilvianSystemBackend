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

using WorkforceTypePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs.WorkforceTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/workforce-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Workforce Type",
        AreaName = "Corporate",
        ControllerName = "WorkforceType",
        Description = "Corporate human resource master data workforce type",
        SortOrder = 76)]
    [Tags("Corporate / Human Resource / Master Data / Workforce Type")]
    public class WorkforceTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "WFT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WorkforceTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Type", Description = "Melihat metadata filter workforce type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new WorkforceTypeFilterMetadataResponse
            {
                DefaultFilter = new WorkforceTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<WorkforceTypeSortOptionResponse>
                {
                    new() { Value = "workforceTypeCode", Label = "Kode tipe tenaga kerja" },
                    new() { Value = "workforceTypeName", Label = "Nama tipe tenaga kerja" },
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "WorkforceType.GetFilterMetadata", "Mengambil metadata filter workforce type.", result);
            return Ok(ApiResponse<WorkforceTypeFilterMetadataResponse>.Ok(result, "Metadata filter workforce type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Type", Description = "Melihat ringkasan workforce type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new WorkforceTypeSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                InternalData = await query.CountAsync(x => x.IsInternal),
                ClinicalData = await query.CountAsync(x => x.IsClinical)
            };
            return Ok(ApiResponse<WorkforceTypeSummaryResponse>.Ok(result, "Ringkasan workforce type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Type", Description = "Melihat data workforce type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceType", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isInternal,
            [FromQuery] bool? isClinical,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "workforceTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isInternal, isClinical, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<WorkforceTypePagedResult>.Ok(new WorkforceTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data workforce type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Type", Description = "Melihat pilihan workforce type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), null, null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder).ThenBy(x => x.WorkforceTypeName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new WorkforceTypeOptionResponse
                {
                    Id = x.Id,
                    WorkforceTypeCode = x.WorkforceTypeCode,
                    WorkforceTypeName = x.WorkforceTypeName,
                    IsInternal = x.IsInternal,
                    IsClinical = x.IsClinical
                })
                .ToListAsync();

            return Ok(ApiResponse<WorkforceTypeOptionPagedResponse>.Ok(new WorkforceTypeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan workforce type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WorkforceTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Workforce Type", Description = "Melihat detail workforce type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforceType", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Workforce type tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new WorkforceTypeDetailResponse
            {
                Id = response.Id,
                WorkforceTypeCode = response.WorkforceTypeCode,
                WorkforceTypeName = response.WorkforceTypeName,
                Description = response.Description,
                IsInternal = response.IsInternal,
                IsClinical = response.IsClinical,
                SortOrder = response.SortOrder,
                IsActive = response.IsActive,
                EmployeeCategoryCount = response.EmployeeCategoryCount,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<WorkforceTypeDetailResponse>.Ok(result, "Detail workforce type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Workforce Type", Description = "Membuat workforce type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforceType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateWorkforceTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload workforce type wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstWorkforceType
            {
                Id = Guid.NewGuid(),
                WorkforceTypeCode = await GenerateCodeAsync(),
                WorkforceTypeName = request.WorkforceTypeName.Trim(),
                Description = NormalizeText(request.Description),
                IsInternal = request.IsInternal,
                IsClinical = request.IsClinical,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkforceType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceType.Create",
                "Membuat data workforce type.",
                new { entity.Id, entity.WorkforceTypeCode, entity.WorkforceTypeName, entity.IsActive, entity.CreateDateTime, entity.CreateBy });

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.WorkforceTypeCode, entity.WorkforceTypeName }, "Workforce type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Type", Description = "Mengubah workforce type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforceType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkforceTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload workforce type wajib diisi."));

            var entity = await _dbContext.Set<MstWorkforceType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Workforce type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.WorkforceTypeName = request.WorkforceTypeName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.IsInternal = request.IsInternal;
            entity.IsClinical = request.IsClinical;
            entity.SortOrder = request.SortOrder ?? entity.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforceType.Update",
                "Mengubah data workforce type.",
                new { entity.Id, entity.WorkforceTypeCode, entity.WorkforceTypeName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy });

            return Ok(ApiResponse<object>.Ok(null, "Workforce type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Workforce Type Status", Description = "Mengubah status workforce type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkforceType", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateWorkforceTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstWorkforceType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Workforce type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status workforce type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Workforce Type", Description = "Menghapus workforce type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkforceType", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstWorkforceType>().Include(x => x.EmployeeCategories)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Workforce type tidak ditemukan."));
            if (entity.EmployeeCategories.Any(x => !x.IsDelete))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Workforce type tidak dapat dihapus karena masih digunakan oleh employee category."));

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
                "WorkforceType.Delete",
                "Menghapus data workforce type.",
                new { entity.Id, entity.WorkforceTypeCode, entity.WorkforceTypeName, entity.DeleteDateTime, entity.DeleteBy });

            return Ok(ApiResponse<object>.Ok(null, "Workforce type berhasil dihapus."));
        }

        private IQueryable<MstWorkforceType> BaseQuery() =>
            _dbContext.Set<MstWorkforceType>().AsNoTracking().Include(x => x.EmployeeCategories).Where(x => !x.IsDelete);

        private static IQueryable<MstWorkforceType> ApplyFilter(IQueryable<MstWorkforceType> query, bool? isInternal, bool? isClinical, bool? isActive, string? search)
        {
            if (isInternal.HasValue) query = query.Where(x => x.IsInternal == isInternal.Value);
            if (isClinical.HasValue) query = query.Where(x => x.IsClinical == isClinical.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.WorkforceTypeCode.ToLower().Contains(keyword) ||
                    x.WorkforceTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstWorkforceType> ApplySorting(IQueryable<MstWorkforceType> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "workforceTypeName").Trim().ToLowerInvariant() switch
            {
                "workforcetypecode" => desc ? query.OrderByDescending(x => x.WorkforceTypeCode) : query.OrderBy(x => x.WorkforceTypeCode),
                "sortorder" => desc
                    ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.WorkforceTypeName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.WorkforceTypeName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.WorkforceTypeName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.WorkforceTypeName),
                _ => desc ? query.OrderByDescending(x => x.WorkforceTypeName) : query.OrderBy(x => x.WorkforceTypeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateWorkforceTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.WorkforceTypeName)) return (false, "Nama workforce type wajib diisi.");
            if (request.SortOrder.HasValue && request.SortOrder.Value < 0) return (false, "Urutan tampil tidak boleh kurang dari 0.");

            var normalizedName = request.WorkforceTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstWorkforceType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.WorkforceTypeName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama workforce type sudah digunakan.");
            return (true, null);
        }

        private static WorkforceTypeResponse MapResponse(MstWorkforceType x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            WorkforceTypeCode = x.WorkforceTypeCode,
            WorkforceTypeName = x.WorkforceTypeName,
            Description = x.Description,
            IsInternal = x.IsInternal,
            IsClinical = x.IsClinical,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            EmployeeCategoryCount = x.EmployeeCategories.Count(y => !y.IsDelete),
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstWorkforceType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.WorkforceTypeCode.StartsWith(CodePrefix))
                .Select(x => x.WorkforceTypeCode).ToListAsync();
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

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<WorkforceTypeCustomPeriodOptionResponse> BuildPeriodOptions() => new()
        {
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" },
            new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
    }
}
