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

using ContractTypePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs.ContractTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/contract-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Contract Type",
        AreaName = "Corporate",
        ControllerName = "ContractType",
        Description = "Corporate human resource master data contract type",
        SortOrder = 80)]
    [Tags("Corporate / Human Resource / Master Data / Contract Type")]
    public class ContractTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "CTT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ContractTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<ContractTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Contract Type", Description = "Melihat metadata filter contract type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ContractType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new ContractTypeFilterMetadataResponse
            {
                DefaultFilter = new ContractTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<ContractTypeSortOptionResponse>
                {
                    new() { Value = "contractTypeCode", Label = "Kode jenis kontrak" },
                    new() { Value = "contractTypeName", Label = "Nama jenis kontrak" },
                    new() { Value = "defaultDurationMonths", Label = "Durasi default (bulan)" },
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "ContractType.GetFilterMetadata", "Mengambil metadata filter contract type.", result);
            return Ok(ApiResponse<ContractTypeFilterMetadataResponse>.Ok(result, "Metadata filter contract type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<ContractTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Contract Type", Description = "Melihat ringkasan contract type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ContractType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new ContractTypeSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                RenewableData = await query.CountAsync(x => x.IsRenewable),
                ProbationApplicableData = await query.CountAsync(x => x.IsProbationApplicable)
            };
            return Ok(ApiResponse<ContractTypeSummaryResponse>.Ok(result, "Ringkasan contract type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ContractTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Contract Type", Description = "Melihat data contract type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ContractType", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isRenewable,
            [FromQuery] bool? isProbationApplicable,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "contractTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isRenewable, isProbationApplicable, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<ContractTypePagedResult>.Ok(new ContractTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data contract type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<ContractTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Contract Type", Description = "Melihat pilihan contract type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ContractType", "Read")]
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
                .OrderBy(x => x.SortOrder).ThenBy(x => x.ContractTypeName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new ContractTypeOptionResponse
                {
                    Id = x.Id,
                    ContractTypeCode = x.ContractTypeCode,
                    ContractTypeName = x.ContractTypeName,
                    DefaultDurationMonths = x.DefaultDurationMonths,
                    IsRenewable = x.IsRenewable
                })
                .ToListAsync();

            return Ok(ApiResponse<ContractTypeOptionPagedResponse>.Ok(new ContractTypeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan contract type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<ContractTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Contract Type", Description = "Melihat detail contract type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ContractType", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Contract type tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new ContractTypeDetailResponse
            {
                Id = response.Id,
                ContractTypeCode = response.ContractTypeCode,
                ContractTypeName = response.ContractTypeName,
                Description = response.Description,
                DefaultDurationMonths = response.DefaultDurationMonths,
                IsRenewable = response.IsRenewable,
                RequiresEndDate = response.RequiresEndDate,
                IsProbationApplicable = response.IsProbationApplicable,
                SortOrder = response.SortOrder,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<ContractTypeDetailResponse>.Ok(result, "Detail contract type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Contract Type", Description = "Membuat contract type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ContractType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateContractTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload contract type wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstContractType
            {
                Id = Guid.NewGuid(),
                ContractTypeCode = await GenerateCodeAsync(),
                ContractTypeName = request.ContractTypeName.Trim(),
                Description = NormalizeText(request.Description),
                DefaultDurationMonths = request.DefaultDurationMonths,
                IsRenewable = request.IsRenewable,
                RequiresEndDate = request.RequiresEndDate,
                IsProbationApplicable = request.IsProbationApplicable,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstContractType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "ContractType.Create",
                "Membuat data contract type.",
                new { entity.Id, entity.ContractTypeCode, entity.ContractTypeName, entity.IsActive, entity.CreateDateTime, entity.CreateBy });

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.ContractTypeCode, entity.ContractTypeName }, "Contract type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Contract Type", Description = "Mengubah contract type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ContractType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateContractTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload contract type wajib diisi."));

            var entity = await _dbContext.Set<MstContractType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Contract type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.ContractTypeName = request.ContractTypeName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.DefaultDurationMonths = request.DefaultDurationMonths;
            entity.IsRenewable = request.IsRenewable;
            entity.RequiresEndDate = request.RequiresEndDate;
            entity.IsProbationApplicable = request.IsProbationApplicable;
            entity.SortOrder = request.SortOrder ?? entity.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "ContractType.Update",
                "Mengubah data contract type.",
                new { entity.Id, entity.ContractTypeCode, entity.ContractTypeName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy });

            return Ok(ApiResponse<object>.Ok(null, "Contract type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Contract Type Status", Description = "Mengubah status contract type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("ContractType", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateContractTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstContractType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Contract type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status contract type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Contract Type", Description = "Menghapus contract type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("ContractType", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstContractType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Contract type tidak ditemukan."));

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
                "ContractType.Delete",
                "Menghapus data contract type.",
                new { entity.Id, entity.ContractTypeCode, entity.ContractTypeName, entity.DeleteDateTime, entity.DeleteBy });

            return Ok(ApiResponse<object>.Ok(null, "Contract type berhasil dihapus."));
        }

        private IQueryable<MstContractType> BaseQuery() =>
            _dbContext.Set<MstContractType>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstContractType> ApplyFilter(IQueryable<MstContractType> query, bool? isRenewable, bool? isProbationApplicable, bool? isActive, string? search)
        {
            if (isRenewable.HasValue) query = query.Where(x => x.IsRenewable == isRenewable.Value);
            if (isProbationApplicable.HasValue) query = query.Where(x => x.IsProbationApplicable == isProbationApplicable.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ContractTypeCode.ToLower().Contains(keyword) ||
                    x.ContractTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstContractType> ApplySorting(IQueryable<MstContractType> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "contractTypeName").Trim().ToLowerInvariant() switch
            {
                "contracttypecode" => desc ? query.OrderByDescending(x => x.ContractTypeCode) : query.OrderBy(x => x.ContractTypeCode),
                "defaultdurationmonths" => desc
                    ? query.OrderByDescending(x => x.DefaultDurationMonths).ThenBy(x => x.ContractTypeName)
                    : query.OrderBy(x => x.DefaultDurationMonths).ThenBy(x => x.ContractTypeName),
                "sortorder" => desc
                    ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.ContractTypeName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.ContractTypeName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.ContractTypeName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.ContractTypeName),
                _ => desc ? query.OrderByDescending(x => x.ContractTypeName) : query.OrderBy(x => x.ContractTypeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateContractTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ContractTypeName)) return (false, "Nama contract type wajib diisi.");
            if (request.SortOrder.HasValue && request.SortOrder.Value < 0) return (false, "Urutan tampil tidak boleh kurang dari 0.");
            if (request.DefaultDurationMonths.HasValue && request.DefaultDurationMonths.Value <= 0)
                return (false, "Durasi default kontrak harus lebih besar dari nol.");

            var normalizedName = request.ContractTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstContractType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.ContractTypeName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama contract type sudah digunakan.");
            return (true, null);
        }

        private static ContractTypeResponse MapResponse(MstContractType x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            ContractTypeCode = x.ContractTypeCode,
            ContractTypeName = x.ContractTypeName,
            Description = x.Description,
            DefaultDurationMonths = x.DefaultDurationMonths,
            IsRenewable = x.IsRenewable,
            RequiresEndDate = x.RequiresEndDate,
            IsProbationApplicable = x.IsProbationApplicable,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstContractType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.ContractTypeCode.StartsWith(CodePrefix))
                .Select(x => x.ContractTypeCode).ToListAsync();
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

        private static List<ContractTypeCustomPeriodOptionResponse> BuildPeriodOptions() => new()
        {
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" },
            new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
    }
}
