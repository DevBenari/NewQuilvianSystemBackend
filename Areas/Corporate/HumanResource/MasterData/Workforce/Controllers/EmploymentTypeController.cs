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

using EmploymentTypePagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.DTOs.EmploymentTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/employment-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Employment Type",
        AreaName = "Corporate",
        ControllerName = "EmploymentType",
        Description = "Corporate human resource master data employment type",
        SortOrder = 78)]
    [Tags("Corporate / Human Resource / Master Data / Employment Type")]
    public class EmploymentTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "EMT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmploymentTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Type", Description = "Melihat metadata filter employment type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new EmploymentTypeFilterMetadataResponse
            {
                DefaultFilter = new EmploymentTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<EmploymentTypeSortOptionResponse>
                {
                    new() { Value = "employmentTypeCode", Label = "Kode tipe kepegawaian" },
                    new() { Value = "employmentTypeName", Label = "Nama tipe kepegawaian" },
                    new() { Value = "sortOrder", Label = "Urutan tampil" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "EmploymentType.GetFilterMetadata", "Mengambil metadata filter employment type.", result);
            return Ok(ApiResponse<EmploymentTypeFilterMetadataResponse>.Ok(result, "Metadata filter employment type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Type", Description = "Melihat ringkasan employment type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new EmploymentTypeSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                PermanentData = await query.CountAsync(x => x.IsPermanent),
                ContractBasedData = await query.CountAsync(x => x.IsContractBased)
            };
            return Ok(ApiResponse<EmploymentTypeSummaryResponse>.Ok(result, "Ringkasan employment type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<EmploymentTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Type", Description = "Melihat data employment type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentType", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isPermanent,
            [FromQuery] bool? isContractBased,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "employmentTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isPermanent, isContractBased, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            var actorNames = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => MapResponse(x, actorNames)).ToList();

            return Ok(ApiResponse<EmploymentTypePagedResult>.Ok(new EmploymentTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data employment type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Employment Type", Description = "Melihat pilihan employment type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentType", "Read")]
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
                .OrderBy(x => x.SortOrder).ThenBy(x => x.EmploymentTypeName)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .Select(x => new EmploymentTypeOptionResponse
                {
                    Id = x.Id,
                    EmploymentTypeCode = x.EmploymentTypeCode,
                    EmploymentTypeName = x.EmploymentTypeName,
                    IsPermanent = x.IsPermanent,
                    IsContractBased = x.IsContractBased
                })
                .ToListAsync();

            return Ok(ApiResponse<EmploymentTypeOptionPagedResponse>.Ok(new EmploymentTypeOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan employment type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<EmploymentTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Employment Type", Description = "Melihat detail employment type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmploymentType", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment type tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = MapResponse(entity, actorNames);
            var result = new EmploymentTypeDetailResponse
            {
                Id = response.Id,
                EmploymentTypeCode = response.EmploymentTypeCode,
                EmploymentTypeName = response.EmploymentTypeName,
                Description = response.Description,
                IsPermanent = response.IsPermanent,
                IsContractBased = response.IsContractBased,
                RequiresContractEndDate = response.RequiresContractEndDate,
                IsPayrollEligible = response.IsPayrollEligible,
                IsBenefitEligible = response.IsBenefitEligible,
                SortOrder = response.SortOrder,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
            return Ok(ApiResponse<EmploymentTypeDetailResponse>.Ok(result, "Detail employment type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Employment Type", Description = "Membuat employment type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmploymentType", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateEmploymentTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload employment type wajib diisi."));

            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstEmploymentType
            {
                Id = Guid.NewGuid(),
                EmploymentTypeCode = await GenerateCodeAsync(),
                EmploymentTypeName = request.EmploymentTypeName.Trim(),
                Description = NormalizeText(request.Description),
                IsPermanent = request.IsPermanent,
                IsContractBased = request.IsContractBased,
                RequiresContractEndDate = request.RequiresContractEndDate,
                IsPayrollEligible = request.IsPayrollEligible,
                IsBenefitEligible = request.IsBenefitEligible,
                SortOrder = request.SortOrder ?? 0,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmploymentType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmploymentType.Create",
                "Membuat data employment type.",
                new { entity.Id, entity.EmploymentTypeCode, entity.EmploymentTypeName, entity.IsActive, entity.CreateDateTime, entity.CreateBy });

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.EmploymentTypeCode, entity.EmploymentTypeName }, "Employment type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Employment Type", Description = "Mengubah employment type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmploymentType", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmploymentTypeRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Payload employment type wajib diisi."));

            var entity = await _dbContext.Set<MstEmploymentType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.EmploymentTypeName = request.EmploymentTypeName.Trim();
            entity.Description = NormalizeText(request.Description);
            entity.IsPermanent = request.IsPermanent;
            entity.IsContractBased = request.IsContractBased;
            entity.RequiresContractEndDate = request.RequiresContractEndDate;
            entity.IsPayrollEligible = request.IsPayrollEligible;
            entity.IsBenefitEligible = request.IsBenefitEligible;
            entity.SortOrder = request.SortOrder ?? entity.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "EmploymentType.Update",
                "Mengubah data employment type.",
                new { entity.Id, entity.EmploymentTypeCode, entity.EmploymentTypeName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy });

            return Ok(ApiResponse<object>.Ok(null, "Employment type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Employment Type Status", Description = "Mengubah status employment type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("EmploymentType", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmploymentTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstEmploymentType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status employment type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Employment Type", Description = "Menghapus employment type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("EmploymentType", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstEmploymentType>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Employment type tidak ditemukan."));

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
                "EmploymentType.Delete",
                "Menghapus data employment type.",
                new { entity.Id, entity.EmploymentTypeCode, entity.EmploymentTypeName, entity.DeleteDateTime, entity.DeleteBy });

            return Ok(ApiResponse<object>.Ok(null, "Employment type berhasil dihapus."));
        }

        private IQueryable<MstEmploymentType> BaseQuery() =>
            _dbContext.Set<MstEmploymentType>().AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstEmploymentType> ApplyFilter(IQueryable<MstEmploymentType> query, bool? isPermanent, bool? isContractBased, bool? isActive, string? search)
        {
            if (isPermanent.HasValue) query = query.Where(x => x.IsPermanent == isPermanent.Value);
            if (isContractBased.HasValue) query = query.Where(x => x.IsContractBased == isContractBased.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.EmploymentTypeCode.ToLower().Contains(keyword) ||
                    x.EmploymentTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstEmploymentType> ApplySorting(IQueryable<MstEmploymentType> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "employmentTypeName").Trim().ToLowerInvariant() switch
            {
                "employmenttypecode" => desc ? query.OrderByDescending(x => x.EmploymentTypeCode) : query.OrderBy(x => x.EmploymentTypeCode),
                "sortorder" => desc
                    ? query.OrderByDescending(x => x.SortOrder).ThenBy(x => x.EmploymentTypeName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.EmploymentTypeName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.EmploymentTypeName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.EmploymentTypeName),
                _ => desc ? query.OrderByDescending(x => x.EmploymentTypeName) : query.OrderBy(x => x.EmploymentTypeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateEmploymentTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmploymentTypeName)) return (false, "Nama employment type wajib diisi.");
            if (request.SortOrder.HasValue && request.SortOrder.Value < 0) return (false, "Urutan tampil tidak boleh kurang dari 0.");
            if (request.IsPermanent && request.IsContractBased) return (false, "Employment type tidak boleh permanen sekaligus berbasis kontrak.");
            if (request.RequiresContractEndDate && !request.IsContractBased) return (false, "Tanggal akhir kontrak hanya berlaku untuk employment type berbasis kontrak.");

            var normalizedName = request.EmploymentTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstEmploymentType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.EmploymentTypeName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Nama employment type sudah digunakan.");
            return (true, null);
        }

        private static EmploymentTypeResponse MapResponse(MstEmploymentType x, IReadOnlyDictionary<Guid, string?> actors) => new()
        {
            Id = x.Id,
            EmploymentTypeCode = x.EmploymentTypeCode,
            EmploymentTypeName = x.EmploymentTypeName,
            Description = x.Description,
            IsPermanent = x.IsPermanent,
            IsContractBased = x.IsContractBased,
            RequiresContractEndDate = x.RequiresContractEndDate,
            IsPayrollEligible = x.IsPayrollEligible,
            IsBenefitEligible = x.IsBenefitEligible,
            SortOrder = x.SortOrder,
            IsActive = x.IsActive,
            CreateDateTime = x.CreateDateTime,
            CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
            CreateByName = GetActorName(actors, x.CreateBy)
        };

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstEmploymentType>().AsNoTracking()
                .Where(x => !x.IsDelete && x.EmploymentTypeCode.StartsWith(CodePrefix))
                .Select(x => x.EmploymentTypeCode).ToListAsync();
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

        private static List<EmploymentTypeCustomPeriodOptionResponse> BuildPeriodOptions() => new()
        {
            new() { Value = "today", Label = "Hari ini" },
            new() { Value = "last7days", Label = "7 hari terakhir" },
            new() { Value = "thismonth", Label = "Bulan ini" },
            new() { Value = "lastmonth", Label = "Bulan lalu" }
        };
    }
}
