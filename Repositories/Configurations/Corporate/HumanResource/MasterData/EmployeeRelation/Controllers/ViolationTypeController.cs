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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.EmployeeRelation.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/violationtypes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Violation Type",
        AreaName = "Corporate",
        ControllerName = "ViolationType",
        Description = "Corporate human resource master data violation type",
        SortOrder = 50)]
    [Tags("Corporate / Human Resource / Master Data / Violation Type")]
    public class ViolationTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "VIO-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public ViolationTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Violation Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ViolationType", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new ViolationTypeFilterMetadataResponse
            {
                DefaultFilter = new ViolationTypeDefaultFilterResponse(),
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

            return Ok(ApiResponse<ViolationTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter violation type berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Violation Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ViolationType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();
            var result = new ViolationTypeSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                UsedData = await _dbContext.Set<WfpDisciplinaryAction>()
                    .CountAsync(x => !x.IsDelete && x.ViolationTypeId.HasValue, cancellationToken)
            };

            return Ok(ApiResponse<ViolationTypeSummaryResponse>.Ok(
                result,
                "Ringkasan violation type berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Violation Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ViolationType", "Read")]
        public async Task<IActionResult> GetData(
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
            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new ViolationTypeResponse
                {
                    Id = x.Id,
                    ViolationTypeCode = x.ViolationTypeCode,
                    ViolationTypeName = x.ViolationTypeName,
                    ViolationCategory = x.ViolationCategory,
                    SeverityLevel = x.SeverityLevel,
                    RequiresInvestigation = x.RequiresInvestigation,
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

            return Ok(ApiResponse<PagedResult<ViolationTypeResponse>>.Ok(
                new PagedResult<ViolationTypeResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data violation type berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Violation Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ViolationType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var query = ApplyFilter(BuildBaseQuery(), true, search);
            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ViolationTypeName)
                .Take(100)
                .Select(x => new ViolationTypeOptionResponse
                {
                    Id = x.Id,
                    Code = x.ViolationTypeCode,
                    Name = x.ViolationTypeName
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<ViolationTypeOptionResponse>>.Ok(
                data,
                "Pilihan violation type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Violation Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("ViolationType", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new ViolationTypeDetailResponse
                {
                    Id = x.Id,
                    ViolationTypeCode = x.ViolationTypeCode,
                    ViolationTypeName = x.ViolationTypeName,
                    ViolationCategory = x.ViolationCategory,
                    SeverityLevel = x.SeverityLevel,
                    RequiresInvestigation = x.RequiresInvestigation,
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
                return NotFound(ApiResponse<object>.Fail(404, "Violation Type tidak ditemukan."));

            return Ok(ApiResponse<ViolationTypeDetailResponse>.Ok(data, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Violation Type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("ViolationType", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateViolationTypeRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request.ViolationTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var entity = new MstViolationType
            {
                Id = Guid.NewGuid(),
                ViolationTypeCode = await GenerateCodeAsync(cancellationToken),
                ViolationTypeName = request.ViolationTypeName.Trim(),
                ViolationCategory = request.ViolationCategory,
                SeverityLevel = request.SeverityLevel,
                RequiresInvestigation = request.RequiresInvestigation,
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstViolationType>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "ViolationType.Create", "Membuat violation type.", new { entity.Id, entity.ViolationTypeCode });
            return await GetById(entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Violation Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ViolationType", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateViolationTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstViolationType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Violation Type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request.ViolationTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            entity.ViolationTypeName = request.ViolationTypeName.Trim();
            entity.ViolationCategory = request.ViolationCategory;
            entity.SeverityLevel = request.SeverityLevel;
            entity.RequiresInvestigation = request.RequiresInvestigation;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetById(id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Violation Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("ViolationType", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateViolationTypeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstViolationType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Violation Type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Violation Type", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("ViolationType", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstViolationType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Violation Type tidak ditemukan."));

            var used = await _dbContext.Set<WfpDisciplinaryAction>()
                .AnyAsync(x => !x.IsDelete && x.ViolationTypeId == id, cancellationToken);

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

        private IQueryable<MstViolationType> BuildBaseQuery()
        {
            return _dbContext.Set<MstViolationType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstViolationType> ApplyFilter(
            IQueryable<MstViolationType> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ViolationTypeCode.ToLower().Contains(keyword) ||
                    x.ViolationTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstViolationType> ApplySorting(
            IQueryable<MstViolationType> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "name").Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.ViolationTypeCode) : query.OrderBy(x => x.ViolationTypeCode),
                "sortorder" => descending ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
                "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.ViolationTypeName) : query.OrderBy(x => x.ViolationTypeName)
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
            var query = _dbContext.Set<MstViolationType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ViolationTypeName.ToLower() == normalized);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (await query.AnyAsync(cancellationToken))
                return (false, "Nama tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstViolationType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ViolationTypeCode.StartsWith(CodePrefix))
                .Select(x => x.ViolationTypeCode)
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
    }
}
