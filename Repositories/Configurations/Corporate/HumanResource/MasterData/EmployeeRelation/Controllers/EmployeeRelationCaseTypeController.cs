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
    [Route("api/v1/corporate/human-resource/master-data/casetypes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Employee Relation Case Type",
        AreaName = "Corporate",
        ControllerName = "EmployeeRelationCaseType",
        Description = "Corporate human resource master data employee relation case type",
        SortOrder = 50)]
    [Tags("Corporate / Human Resource / Master Data / Employee Relation Case Type")]
    public class EmployeeRelationCaseTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "ECT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public EmployeeRelationCaseTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Employee Relation Case Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeRelationCaseType", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new EmployeeRelationCaseTypeFilterMetadataResponse
            {
                DefaultFilter = new EmployeeRelationCaseTypeDefaultFilterResponse(),
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

            return Ok(ApiResponse<EmployeeRelationCaseTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter employee relation case type berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Employee Relation Case Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeRelationCaseType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();
            var result = new EmployeeRelationCaseTypeSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                UsedData = await _dbContext.Set<WfpDisciplinaryAction>()
                    .CountAsync(x => !x.IsDelete && x.EmployeeRelationCaseTypeId.HasValue, cancellationToken)
            };

            return Ok(ApiResponse<EmployeeRelationCaseTypeSummaryResponse>.Ok(
                result,
                "Ringkasan employee relation case type berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Employee Relation Case Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeRelationCaseType", "Read")]
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
                .Select(x => new EmployeeRelationCaseTypeResponse
                {
                    Id = x.Id,
                    CaseTypeCode = x.CaseTypeCode,
                    CaseTypeName = x.CaseTypeName,
                    CaseCategory = x.CaseCategory,
                    RequiresInvestigation = x.RequiresInvestigation,
                    RequiresHearing = x.RequiresHearing,
                    DefaultConfidential = x.DefaultConfidential,
                    TargetResolutionDays = x.TargetResolutionDays,
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

            return Ok(ApiResponse<PagedResult<EmployeeRelationCaseTypeResponse>>.Ok(
                new PagedResult<EmployeeRelationCaseTypeResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data employee relation case type berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Employee Relation Case Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeRelationCaseType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var query = ApplyFilter(BuildBaseQuery(), true, search);
            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CaseTypeName)
                .Take(100)
                .Select(x => new EmployeeRelationCaseTypeOptionResponse
                {
                    Id = x.Id,
                    Code = x.CaseTypeCode,
                    Name = x.CaseTypeName
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<EmployeeRelationCaseTypeOptionResponse>>.Ok(
                data,
                "Pilihan employee relation case type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Employee Relation Case Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("EmployeeRelationCaseType", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new EmployeeRelationCaseTypeDetailResponse
                {
                    Id = x.Id,
                    CaseTypeCode = x.CaseTypeCode,
                    CaseTypeName = x.CaseTypeName,
                    CaseCategory = x.CaseCategory,
                    RequiresInvestigation = x.RequiresInvestigation,
                    RequiresHearing = x.RequiresHearing,
                    DefaultConfidential = x.DefaultConfidential,
                    TargetResolutionDays = x.TargetResolutionDays,
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
                return NotFound(ApiResponse<object>.Fail(404, "Employee Relation Case Type tidak ditemukan."));

            return Ok(ApiResponse<EmployeeRelationCaseTypeDetailResponse>.Ok(data, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Employee Relation Case Type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("EmployeeRelationCaseType", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateEmployeeRelationCaseTypeRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request.CaseTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var entity = new MstEmployeeRelationCaseType
            {
                Id = Guid.NewGuid(),
                CaseTypeCode = await GenerateCodeAsync(cancellationToken),
                CaseTypeName = request.CaseTypeName.Trim(),
                CaseCategory = request.CaseCategory,
                RequiresInvestigation = request.RequiresInvestigation,
                RequiresHearing = request.RequiresHearing,
                DefaultConfidential = request.DefaultConfidential,
                TargetResolutionDays = request.TargetResolutionDays,
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstEmployeeRelationCaseType>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "EmployeeRelationCaseType.Create", "Membuat employee relation case type.", new { entity.Id, entity.CaseTypeCode });
            return await GetById(entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Employee Relation Case Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeRelationCaseType", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateEmployeeRelationCaseTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstEmployeeRelationCaseType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Employee Relation Case Type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request.CaseTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            entity.CaseTypeName = request.CaseTypeName.Trim();
            entity.CaseCategory = request.CaseCategory;
            entity.RequiresInvestigation = request.RequiresInvestigation;
            entity.RequiresHearing = request.RequiresHearing;
            entity.DefaultConfidential = request.DefaultConfidential;
            entity.TargetResolutionDays = request.TargetResolutionDays;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetById(id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Employee Relation Case Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("EmployeeRelationCaseType", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateEmployeeRelationCaseTypeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstEmployeeRelationCaseType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Employee Relation Case Type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Employee Relation Case Type", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("EmployeeRelationCaseType", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstEmployeeRelationCaseType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Employee Relation Case Type tidak ditemukan."));

            var used = await _dbContext.Set<WfpDisciplinaryAction>()
                .AnyAsync(x => !x.IsDelete && x.EmployeeRelationCaseTypeId == id, cancellationToken);

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

        private IQueryable<MstEmployeeRelationCaseType> BuildBaseQuery()
        {
            return _dbContext.Set<MstEmployeeRelationCaseType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstEmployeeRelationCaseType> ApplyFilter(
            IQueryable<MstEmployeeRelationCaseType> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CaseTypeCode.ToLower().Contains(keyword) ||
                    x.CaseTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstEmployeeRelationCaseType> ApplySorting(
            IQueryable<MstEmployeeRelationCaseType> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "name").Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.CaseTypeCode) : query.OrderBy(x => x.CaseTypeCode),
                "sortorder" => descending ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
                "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.CaseTypeName) : query.OrderBy(x => x.CaseTypeName)
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
            var query = _dbContext.Set<MstEmployeeRelationCaseType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.CaseTypeName.ToLower() == normalized);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (await query.AnyAsync(cancellationToken))
                return (false, "Nama tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstEmployeeRelationCaseType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.CaseTypeCode.StartsWith(CodePrefix))
                .Select(x => x.CaseTypeCode)
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
