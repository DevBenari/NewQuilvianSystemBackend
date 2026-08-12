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

using EmploymentStatusPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs.EmploymentStatusResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/employment-statuses")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Employment Status",
        AreaName = "Corporate",
        ControllerName = "EmploymentStatus",
        Description = "Corporate human resource master data employment status",
        SortOrder = 79)]
    [Tags("Corporate / Human Resource / Master Data / Employment Status")]
    public class EmploymentStatusController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "EMS-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmploymentStatusController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentStatusFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Status", Description = "Melihat metadata filter employment status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentStatus", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new EmploymentStatusFilterMetadataResponse
            {
                DefaultFilter = new EmploymentStatusDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<EmploymentStatusSortOptionResponse>
                {
                    new() { Value = "employmentStatusCode", Label = "Kode status kepegawaian" },
                    new() { Value = "employmentStatusName", Label = "Nama status kepegawaian" },
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "EmploymentStatus.GetFilterMetadata", "Mengambil metadata filter employment status.", result);
            return Ok(ApiResponse<EmploymentStatusFilterMetadataResponse>.Ok(result, "Metadata filter employment status berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentStatusSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Status", Description = "Melihat ringkasan employment status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentStatus", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new EmploymentStatusSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                ActiveEmploymentData = await query.CountAsync(x => x.IsActiveEmployment),
                TerminalStatusData = await query.CountAsync(x => x.IsTerminalStatus)
            };
            return Ok(ApiResponse<EmploymentStatusSummaryResponse>.Ok(result, "Ringkasan employment status berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EmploymentStatusPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Status", Description = "Melihat data employment status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentStatus", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActiveEmployment,
            [FromQuery] bool? isTerminalStatus,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "employmentStatusName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isActiveEmployment, isTerminalStatus, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<EmploymentStatusPagedResult>.Ok(new EmploymentStatusPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data employment status berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentStatusOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Status", Description = "Melihat pilihan employment status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentStatus", "Read")]
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
                .OrderBy(x => x.SortOrder).ThenBy(x => x.EmploymentStatusName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new EmploymentStatusOptionResponse
                {
                    Id = x.Id,
                    EmploymentStatusCode = x.EmploymentStatusCode,
                    EmploymentStatusName = x.EmploymentStatusName,
                    IsActiveEmployment = x.IsActiveEmployment,
                    IsTerminalStatus = x.IsTerminalStatus
                })
                .ToListAsync();

            return Ok(ApiResponse<EmploymentStatusOptionPagedResponse>.Ok(new EmploymentStatusOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan employment status berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentStatusDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Employment Status", Description = "Melihat detail employment status", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentStatus", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment status tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new EmploymentStatusDetailResponse
            {
                Id = response.Id,
                EmploymentStatusCode = response.EmploymentStatusCode,
                EmploymentStatusName = response.EmploymentStatusName,
                Description = response.Description,
                IsActiveEmployment = response.IsActiveEmployment,
                IsSchedulable = response.IsSchedulable,
                IsPayrollEligible = response.IsPayrollEligible,
                IsTerminalStatus = response.IsTerminalStatus,
                SortOrder = response.SortOrder,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<EmploymentStatusDetailResponse>.Ok(result, "Detail employment status berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Employment Status", Description = "Membuat employment status", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmploymentStatus", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmploymentStatusRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload employment status wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstEmploymentStatus
            {
                Id = Guid.NewGuid(),
                EmploymentStatusCode = await GenerateCodeAsync(),
                EmploymentStatusName = request.EmploymentStatusName.Trim(),
                Description = NormalizeText(request.Description),
                IsActiveEmployment = request.IsActiveEmployment,
                IsSchedulable = request.IsSchedulable,
                IsPayrollEligible = request.IsPayrollEligible,
                IsTerminalStatus = request.IsTerminalStatus,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmploymentStatus>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmploymentStatus.Create",
                "Membuat data employment status.",
                new { entity.Id, entity.EmploymentStatusCode, entity.EmploymentStatusName, entity.IsActive, entity.CreateDateTime, entity.CreateBy });

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.EmploymentStatusCode, entity.EmploymentStatusName }, "Employment status berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Employment Status", Description = "Mengubah employment status", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmploymentStatus", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmploymentStatusRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload employment status wajib diisi."));

            var entity = await _dbContext.Set<MstEmploymentStatus>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment status tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.EmploymentStatusName = request.EmploymentStatusName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.IsActiveEmployment = request.IsActiveEmployment;
            entity.IsSchedulable = request.IsSchedulable;
            entity.IsPayrollEligible = request.IsPayrollEligible;
            entity.IsTerminalStatus = request.IsTerminalStatus;
            entity.SortOrder = request.SortOrder ?? entity.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmploymentStatus.Update",
                "Mengubah data employment status.",
                new { entity.Id, entity.EmploymentStatusCode, entity.EmploymentStatusName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy });

            return Ok(ApiResponse<object>.Ok(null, "Employment status berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Employment Status Status", Description = "Mengubah status employment status", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmploymentStatus", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmploymentStatusStatusRequest request)
        {
            var entity = await _dbContext.Set<MstEmploymentStatus>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment status tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status employment status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Employment Status", Description = "Menghapus employment status", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmploymentStatus", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstEmploymentStatus>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment status tidak ditemukan."));

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
                "EmploymentStatus.Delete",
                "Menghapus data employment status.",
                new { entity.Id, entity.EmploymentStatusCode, entity.EmploymentStatusName, entity.DeleteDateTime, entity.DeleteBy });

            return Ok(ApiResponse<object>.Ok(null, "Employment status berhasil dihapus."));
        }

        private IQueryable<MstEmploymentStatus> BaseQuery() =>
            _dbContext.Set<MstEmploymentStatus>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstEmploymentStatus> ApplyFilter(IQueryable<MstEmploymentStatus> query, bool? isActiveEmployment, bool? isTerminalStatus, bool? isActive, string? search)
        {
            if (isActiveEmployment.HasValue) query = query.Where(x => x.IsActiveEmployment == isActiveEmployment.Value);
            if (isTerminalStatus.HasValue) query = query.Where(x => x.IsTerminalStatus == isTerminalStatus.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EmploymentStatusCode.ToLower().Contains(keyword) ||
                    x.EmploymentStatusName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstEmploymentStatus> ApplySorting(IQueryable<MstEmploymentStatus> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "employmentStatusName").Trim().ToLowerInvariant() switch
            {
                "employmentstatuscode" => desc ? query.OrderByDescending(x => x.EmploymentStatusCode) : query.OrderBy(x => x.EmploymentStatusCode),
                "sortorder" => desc
                    ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.EmploymentStatusName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.EmploymentStatusName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.EmploymentStatusName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.EmploymentStatusName),
                _ => desc ? query.OrderByDescending(x => x.EmploymentStatusName) : query.OrderBy(x => x.EmploymentStatusName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateEmploymentStatusRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmploymentStatusName)) return (false, "Nama employment status wajib diisi.");
            if (request.SortOrder.HasValue && request.SortOrder.Value < 0) return (false, "Urutan tampil tidak boleh kurang dari 0.");
            if (request.IsTerminalStatus && request.IsActiveEmployment) return (false, "Status terminal tidak boleh ditandai sebagai kepegawaian aktif.");

            var normalizedName = request.EmploymentStatusName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstEmploymentStatus>().AsNoTracking()
                .Where(x => !x.IsDelete && x.EmploymentStatusName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama employment status sudah digunakan.");
            return (true, null);
        }

        private static EmploymentStatusResponse MapResponse(MstEmploymentStatus x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            EmploymentStatusCode = x.EmploymentStatusCode,
            EmploymentStatusName = x.EmploymentStatusName,
            Description = x.Description,
            IsActiveEmployment = x.IsActiveEmployment,
            IsSchedulable = x.IsSchedulable,
            IsPayrollEligible = x.IsPayrollEligible,
            IsTerminalStatus = x.IsTerminalStatus,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstEmploymentStatus>().AsNoTracking()
                .Where(x => !x.IsDelete && x.EmploymentStatusCode.StartsWith(CodePrefix))
                .Select(x => x.EmploymentStatusCode).ToListAsync();
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

        private static List<EmploymentStatusCustomPeriodOptionResponse> BuildPeriodOptions() => new()
        {
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" },
            new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
    }
}
