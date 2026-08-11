using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.EmployeeRelationManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/sanctiontypes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Sanction Type",
        AreaName = "Corporate",
        ControllerName = "SanctionType",
        Description = "Corporate human resource master data sanction type",
        SortOrder = 50)]
    [Tags("Corporate / Human Resource / Master Data / Sanction Type")]
    public class SanctionTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "SAN-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public SanctionTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Sanction Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SanctionType", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new SanctionTypeFilterMetadataResponse
            {
                DefaultFilter = new SanctionTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<EmployeeRelationSortOptionResponse>
                {
                    new() { Value = "code", Label = "Kode" },
                    new() { Value = "name", Label = "Nama" },
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<SanctionTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter sanction type berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Sanction Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SanctionType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();
            var result = new SanctionTypeSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                UsedData = await _dbContext.Set<WfpDisciplinaryAction>()
                    .CountAsync(x => !x.IsDelete && x.SanctionTypeId.HasValue, cancellationToken)
            };

            return Ok(ApiResponse<SanctionTypeSummaryResponse>.Ok(
                result,
                "Ringkasan sanction type berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Sanction Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SanctionType", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "name",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(BuildBaseQuery(), isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SanctionTypeResponse
                {
                    Id = x.Id,
                    SanctionTypeCode = x.SanctionTypeCode,
                    SanctionTypeName = x.SanctionTypeName,
                    SanctionLevel = x.SanctionLevel,
                    DefaultDurationDays = x.DefaultDurationDays,
                    IsFinalSanction = x.IsFinalSanction,
                    AllowsAppeal = x.AllowsAppeal,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<PagedResult<SanctionTypeResponse>>.Ok(
                new PagedResult<SanctionTypeResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data sanction type berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Sanction Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SanctionType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var query = ApplyFilter(BuildBaseQuery(), true, search);
            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SanctionTypeName)
                .Take(100)
                .Select(x => new SanctionTypeOptionResponse
                {
                    Id = x.Id,
                    Code = x.SanctionTypeCode,
                    Name = x.SanctionTypeName
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<SanctionTypeOptionResponse>>.Ok(
                data,
                "Pilihan sanction type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Sanction Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SanctionType", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new SanctionTypeDetailResponse
                {
                    Id = x.Id,
                    SanctionTypeCode = x.SanctionTypeCode,
                    SanctionTypeName = x.SanctionTypeName,
                    SanctionLevel = x.SanctionLevel,
                    DefaultDurationDays = x.DefaultDurationDays,
                    IsFinalSanction = x.IsFinalSanction,
                    AllowsAppeal = x.AllowsAppeal,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sanction Type tidak ditemukan."));

            return Ok(ApiResponse<SanctionTypeDetailResponse>.Ok(data, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Sanction Type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SanctionType", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateSanctionTypeRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request.SanctionTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var entity = new MstSanctionType
            {
                Id = Guid.NewGuid(),
                SanctionTypeCode = await GenerateCodeAsync(cancellationToken),
                SanctionTypeName = request.SanctionTypeName.Trim(),
                SanctionLevel = request.SanctionLevel,
                DefaultDurationDays = request.DefaultDurationDays,
                IsFinalSanction = request.IsFinalSanction,
                AllowsAppeal = request.AllowsAppeal,
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstSanctionType>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "SanctionType.Create", "Membuat sanction type.", new { entity.Id, entity.SanctionTypeCode });
            return await GetById(entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Sanction Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SanctionType", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateSanctionTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSanctionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sanction Type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request.SanctionTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            entity.SanctionTypeName = request.SanctionTypeName.Trim();
            entity.SanctionLevel = request.SanctionLevel;
            entity.DefaultDurationDays = request.DefaultDurationDays;
            entity.IsFinalSanction = request.IsFinalSanction;
            entity.AllowsAppeal = request.AllowsAppeal;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetById(id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Sanction Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SanctionType", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateSanctionTypeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSanctionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sanction Type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Sanction Type", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("SanctionType", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSanctionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Sanction Type tidak ditemukan."));

            var used = await _dbContext.Set<WfpDisciplinaryAction>()
                .AnyAsync(x => !x.IsDelete && x.SanctionTypeId == id, cancellationToken);

            if (used)
                return BadRequest(ApiResponse<object>.Fail(400, "Data tidak dapat dihapus karena sudah digunakan."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Data berhasil dihapus."));
        }

        private IQueryable<MstSanctionType> BuildBaseQuery()
        {
            return _dbContext.Set<MstSanctionType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstSanctionType> ApplyFilter(
            IQueryable<MstSanctionType> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.SanctionTypeCode.ToLower().Contains(keyword) ||
                    x.SanctionTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstSanctionType> ApplySorting(
            IQueryable<MstSanctionType> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "name").Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.SanctionTypeCode) : query.OrderBy(x => x.SanctionTypeCode),
                "sortorder" => descending ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
                "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.SanctionTypeName) : query.OrderBy(x => x.SanctionTypeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string name,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                return (false, "Nama wajib diisi.");

            var normalized = name.Trim().ToLower();
            var query = _dbContext.Set<MstSanctionType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.SanctionTypeName.ToLower() == normalized);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (await query.AnyAsync(cancellationToken))
                return (false, "Nama tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstSanctionType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.SanctionTypeCode.StartsWith(CodePrefix))
                .Select(x => x.SanctionTypeCode)
                .ToListAsync(cancellationToken);

            var used = codes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue("user_id") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            return (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static List<EmployeeRelationCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<EmployeeRelationCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
