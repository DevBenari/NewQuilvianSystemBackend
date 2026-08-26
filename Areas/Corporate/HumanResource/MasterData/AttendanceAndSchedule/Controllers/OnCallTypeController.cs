using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using OnCallTypePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.DTOs.OnCallTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.AttendanceAndSchedule.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/on-call-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "On Call Type",
        AreaName = "Corporate",
        ControllerName = "OnCallType",
        Description = "Corporate human resource master data on call type",
        SortOrder = 81)]
    [Tags("Corporate / Human Resource / Master Data / On Call Type")]
    public class OnCallTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "OCT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public OnCallTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<OnCallTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read On Call Type", Description = "Melihat metadata filter on call type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new OnCallTypeFilterMetadataResponse
            {
                DefaultFilter = new OnCallTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<OnCallTypeSortOptionResponse>
                {
                    new() { Value = "onCallTypeCode", Label = "Kode jenis on call" },
                    new() { Value = "onCallTypeName", Label = "Nama jenis on call" },
                    new() { Value = "responseTimeMinutes", Label = "Waktu respons (menit)" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "OnCallType.GetFilterMetadata", "Mengambil metadata filter on call type.", result);
            return Ok(ApiResponse<OnCallTypeFilterMetadataResponse>.Ok(result, "Metadata filter on call type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<OnCallTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read On Call Type", Description = "Melihat ringkasan on call type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new OnCallTypeSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                RemoteAllowedData = await query.CountAsync(x => x.IsRemoteAllowed),
                AllowanceEligibleData = await query.CountAsync(x => x.IsAllowanceEligible)
            };
            return Ok(ApiResponse<OnCallTypeSummaryResponse>.Ok(result, "Ringkasan on call type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<OnCallTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read On Call Type", Description = "Melihat data on call type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallType", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isRemoteAllowed,
            [FromQuery] bool? isAllowanceEligible,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "onCallTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isRemoteAllowed, isAllowanceEligible, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<OnCallTypePagedResult>.Ok(new OnCallTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data on call type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<OnCallTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read On Call Type", Description = "Melihat pilihan on call type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallType", "Read")]
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
                .OrderBy(x => x.OnCallTypeName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new OnCallTypeOptionResponse
                {
                    Id = x.Id,
                    OnCallTypeCode = x.OnCallTypeCode,
                    OnCallTypeName = x.OnCallTypeName,
                    IsRemoteAllowed = x.IsRemoteAllowed,
                    IsAllowanceEligible = x.IsAllowanceEligible
                })
                .ToListAsync();

            return Ok(ApiResponse<OnCallTypeOptionPagedResponse>.Ok(new OnCallTypeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan on call type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<OnCallTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read On Call Type", Description = "Melihat detail on call type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OnCallType", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "On call type tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new OnCallTypeDetailResponse
            {
                Id = response.Id,
                OnCallTypeCode = response.OnCallTypeCode,
                OnCallTypeName = response.OnCallTypeName,
                Description = response.Description,
                ResponseTimeMinutes = response.ResponseTimeMinutes,
                MinimumCallHours = response.MinimumCallHours,
                MaximumCallHours = response.MaximumCallHours,
                IsRemoteAllowed = response.IsRemoteAllowed,
                RequiresOnSitePresence = response.RequiresOnSitePresence,
                CountsAsWorkingTime = response.CountsAsWorkingTime,
                IsAllowanceEligible = response.IsAllowanceEligible,
                IsActive = response.IsActive,
                ShiftCount = response.ShiftCount,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<OnCallTypeDetailResponse>.Ok(result, "Detail on call type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create On Call Type", Description = "Membuat on call type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OnCallType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateOnCallTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload on call type wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstOnCallType
            {
                Id = Guid.NewGuid(),
                OnCallTypeCode = await GenerateCodeAsync(),
                OnCallTypeName = request.OnCallTypeName.Trim(),
                Description = NormalizeText(request.Description),
                ResponseTimeMinutes = request.ResponseTimeMinutes,
                MinimumCallHours = request.MinimumCallHours,
                MaximumCallHours = request.MaximumCallHours,
                IsRemoteAllowed = request.IsRemoteAllowed,
                RequiresOnSitePresence = request.RequiresOnSitePresence,
                CountsAsWorkingTime = request.CountsAsWorkingTime,
                IsAllowanceEligible = request.IsAllowanceEligible,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstOnCallType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "OnCallType.Create",
                "Membuat data on call type.",
                new { entity.Id, entity.OnCallTypeCode, entity.OnCallTypeName, entity.IsActive, entity.CreateDateTime, entity.CreateBy });

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.OnCallTypeCode, entity.OnCallTypeName }, "On call type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update On Call Type", Description = "Mengubah on call type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OnCallType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOnCallTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload on call type wajib diisi."));

            var entity = await _dbContext.Set<MstOnCallType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "On call type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.OnCallTypeName = request.OnCallTypeName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.ResponseTimeMinutes = request.ResponseTimeMinutes;
            entity.MinimumCallHours = request.MinimumCallHours;
            entity.MaximumCallHours = request.MaximumCallHours;
            entity.IsRemoteAllowed = request.IsRemoteAllowed;
            entity.RequiresOnSitePresence = request.RequiresOnSitePresence;
            entity.CountsAsWorkingTime = request.CountsAsWorkingTime;
            entity.IsAllowanceEligible = request.IsAllowanceEligible;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "OnCallType.Update",
                "Mengubah data on call type.",
                new { entity.Id, entity.OnCallTypeCode, entity.OnCallTypeName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy });

            return Ok(ApiResponse<object>.Ok(null, "On call type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update On Call Type Status", Description = "Mengubah status on call type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OnCallType", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOnCallTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstOnCallType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "On call type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status on call type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete On Call Type", Description = "Menghapus on call type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("OnCallType", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstOnCallType>().Include(x => x.Shifts)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "On call type tidak ditemukan."));
            if (entity.Shifts.Any(x => !x.IsDelete))
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "On call type tidak dapat dihapus karena masih digunakan oleh shift."));

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
                "OnCallType.Delete",
                "Menghapus data on call type.",
                new { entity.Id, entity.OnCallTypeCode, entity.OnCallTypeName, entity.DeleteDateTime, entity.DeleteBy });

            return Ok(ApiResponse<object>.Ok(null, "On call type berhasil dihapus."));
        }

        private IQueryable<MstOnCallType> BaseQuery() =>
            _dbContext.Set<MstOnCallType>().AsNoTracking().Include(x => x.Shifts).Where(x => !x.IsDelete);

        private static IQueryable<MstOnCallType> ApplyFilter(IQueryable<MstOnCallType> query, bool? isRemoteAllowed, bool? isAllowanceEligible, bool? isActive, string? search)
        {
            if (isRemoteAllowed.HasValue) query = query.Where(x => x.IsRemoteAllowed == isRemoteAllowed.Value);
            if (isAllowanceEligible.HasValue) query = query.Where(x => x.IsAllowanceEligible == isAllowanceEligible.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.OnCallTypeCode.ToLower().Contains(keyword) ||
                    x.OnCallTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstOnCallType> ApplySorting(IQueryable<MstOnCallType> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "onCallTypeName").Trim().ToLowerInvariant() switch
            {
                "oncalltypecode" => desc ? query.OrderByDescending(x => x.OnCallTypeCode) : query.OrderBy(x => x.OnCallTypeCode),
                "responsetimeminutes" => desc
                    ? query.OrderByDescending(x => x.ResponseTimeMinutes).ThenBy(x => x.OnCallTypeName)
                    : query.OrderBy(x => x.ResponseTimeMinutes).ThenBy(x => x.OnCallTypeName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.OnCallTypeName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.OnCallTypeName),
                _ => desc ? query.OrderByDescending(x => x.OnCallTypeName) : query.OrderBy(x => x.OnCallTypeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateOnCallTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.OnCallTypeName)) return (false, "Nama on call type wajib diisi.");
            if (request.ResponseTimeMinutes < 0) return (false, "Waktu respons tidak boleh kurang dari 0.");
            if (request.MinimumCallHours < 0) return (false, "Minimum jam panggilan tidak boleh kurang dari 0.");
            if (request.MaximumCallHours < request.MinimumCallHours)
                return (false, "Maksimum jam panggilan tidak boleh lebih kecil dari minimum jam panggilan.");
            if (request.IsRemoteAllowed && request.RequiresOnSitePresence)
                return (false, "On call type tidak boleh mengizinkan remote sekaligus mewajibkan kehadiran di tempat.");

            var normalizedName = request.OnCallTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstOnCallType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.OnCallTypeName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama on call type sudah digunakan.");
            return (true, null);
        }

        private static OnCallTypeResponse MapResponse(MstOnCallType x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            OnCallTypeCode = x.OnCallTypeCode,
            OnCallTypeName = x.OnCallTypeName,
            Description = x.Description,
            ResponseTimeMinutes = x.ResponseTimeMinutes,
            MinimumCallHours = x.MinimumCallHours,
            MaximumCallHours = x.MaximumCallHours,
            IsRemoteAllowed = x.IsRemoteAllowed,
            RequiresOnSitePresence = x.RequiresOnSitePresence,
            CountsAsWorkingTime = x.CountsAsWorkingTime,
            IsAllowanceEligible = x.IsAllowanceEligible,
            IsActive = x.IsActive,
            ShiftCount = x.Shifts.Count(y => !y.IsDelete),
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstOnCallType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.OnCallTypeCode.StartsWith(CodePrefix))
                .Select(x => x.OnCallTypeCode).ToListAsync();
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

        private static List<OnCallTypeCustomPeriodOptionResponse> BuildPeriodOptions() => new()
        {
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" },
            new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
    }
}
