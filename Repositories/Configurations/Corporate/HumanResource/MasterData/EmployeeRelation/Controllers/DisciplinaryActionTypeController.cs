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
    [Route("api/v1/corporate/human-resource/master-data/actiontypes")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Disciplinary Action Type",
        AreaName = "Corporate",
        ControllerName = "DisciplinaryActionType",
        Description = "Corporate human resource master data disciplinary action type",
        SortOrder = 50)]
    [Tags("Corporate / Human Resource / Master Data / Disciplinary Action Type")]
    public class DisciplinaryActionTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "DAT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DisciplinaryActionTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Disciplinary Action Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DisciplinaryActionType", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new DisciplinaryActionTypeFilterMetadataResponse
            {
                DefaultFilter = new DisciplinaryActionTypeDefaultFilterResponse(),
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

            return Ok(ApiResponse<DisciplinaryActionTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter disciplinary action type berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Disciplinary Action Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DisciplinaryActionType", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();
            var result = new DisciplinaryActionTypeSummaryResponse
            {
                TotalData = await query.CountAsync(cancellationToken),
                ActiveData = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveData = await query.CountAsync(x => !x.IsActive, cancellationToken),
                UsedData = await _dbContext.Set<WfpDisciplinaryAction>()
                    .CountAsync(x => !x.IsDelete && x.DisciplinaryActionTypeId != Guid.Empty, cancellationToken)
            };

            return Ok(ApiResponse<DisciplinaryActionTypeSummaryResponse>.Ok(
                result,
                "Ringkasan disciplinary action type berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Disciplinary Action Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DisciplinaryActionType", "Read")]
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
                .Select(x => new DisciplinaryActionTypeResponse
                {
                    Id = x.Id,
                    ActionTypeCode = x.ActionTypeCode,
                    ActionTypeName = x.ActionTypeName,
                    DefaultActionLevel = x.DefaultActionLevel,
                    DefaultEffectiveDays = x.DefaultEffectiveDays,
                    RequiresApproval = x.RequiresApproval,
                    AllowsAppeal = x.AllowsAppeal,
                    IsConfidential = x.IsConfidential,
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

            return Ok(ApiResponse<PagedResult<DisciplinaryActionTypeResponse>>.Ok(
                new PagedResult<DisciplinaryActionTypeResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data disciplinary action type berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Disciplinary Action Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DisciplinaryActionType", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] string? search,
            CancellationToken cancellationToken)
        {
            var query = ApplyFilter(BuildBaseQuery(), true, search);
            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.ActionTypeName)
                .Take(100)
                .Select(x => new DisciplinaryActionTypeOptionResponse
                {
                    Id = x.Id,
                    Code = x.ActionTypeCode,
                    Name = x.ActionTypeName
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<DisciplinaryActionTypeOptionResponse>>.Ok(
                data,
                "Pilihan disciplinary action type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Disciplinary Action Type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DisciplinaryActionType", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DisciplinaryActionTypeDetailResponse
                {
                    Id = x.Id,
                    ActionTypeCode = x.ActionTypeCode,
                    ActionTypeName = x.ActionTypeName,
                    DefaultActionLevel = x.DefaultActionLevel,
                    DefaultEffectiveDays = x.DefaultEffectiveDays,
                    RequiresApproval = x.RequiresApproval,
                    AllowsAppeal = x.AllowsAppeal,
                    IsConfidential = x.IsConfidential,
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
                return NotFound(ApiResponse<object>.Fail(404, "Disciplinary Action Type tidak ditemukan."));

            return Ok(ApiResponse<DisciplinaryActionTypeDetailResponse>.Ok(data, "Detail berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Disciplinary Action Type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DisciplinaryActionType", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateDisciplinaryActionTypeRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request.ActionTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            var entity = new MstDisciplinaryActionType
            {
                Id = Guid.NewGuid(),
                ActionTypeCode = await GenerateCodeAsync(cancellationToken),
                ActionTypeName = request.ActionTypeName.Trim(),
                DefaultActionLevel = request.DefaultActionLevel,
                DefaultEffectiveDays = request.DefaultEffectiveDays,
                RequiresApproval = request.RequiresApproval,
                AllowsAppeal = request.AllowsAppeal,
                IsConfidential = request.IsConfidential,
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDisciplinaryActionType>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(LogCategory, "DisciplinaryActionType.Create", "Membuat disciplinary action type.", new { entity.Id, entity.ActionTypeCode });
            return await GetById(entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Disciplinary Action Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DisciplinaryActionType", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateDisciplinaryActionTypeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstDisciplinaryActionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Disciplinary Action Type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request.ActionTypeName, cancellationToken);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage!));

            entity.ActionTypeName = request.ActionTypeName.Trim();
            entity.DefaultActionLevel = request.DefaultActionLevel;
            entity.DefaultEffectiveDays = request.DefaultEffectiveDays;
            entity.RequiresApproval = request.RequiresApproval;
            entity.AllowsAppeal = request.AllowsAppeal;
            entity.IsConfidential = request.IsConfidential;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);
            return await GetById(id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Disciplinary Action Type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DisciplinaryActionType", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateDisciplinaryActionTypeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstDisciplinaryActionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Disciplinary Action Type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Disciplinary Action Type", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DisciplinaryActionType", "Delete")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstDisciplinaryActionType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Disciplinary Action Type tidak ditemukan."));

            var used = await _dbContext.Set<WfpDisciplinaryAction>()
                .AnyAsync(x => !x.IsDelete && x.DisciplinaryActionTypeId == id, cancellationToken);

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

        private IQueryable<MstDisciplinaryActionType> BuildBaseQuery()
        {
            return _dbContext.Set<MstDisciplinaryActionType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDisciplinaryActionType> ApplyFilter(
            IQueryable<MstDisciplinaryActionType> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.ActionTypeCode.ToLower().Contains(keyword) ||
                    x.ActionTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstDisciplinaryActionType> ApplySorting(
            IQueryable<MstDisciplinaryActionType> query,
            string? sortBy,
            string? sortDirection)
        {
            var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "name").Trim().ToLowerInvariant() switch
            {
                "code" => descending ? query.OrderByDescending(x => x.ActionTypeCode) : query.OrderBy(x => x.ActionTypeCode),
                "sortorder" => descending ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder),
                "isactive" => descending ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                "createdatetime" => descending ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => descending ? query.OrderByDescending(x => x.ActionTypeName) : query.OrderBy(x => x.ActionTypeName)
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
            var query = _dbContext.Set<MstDisciplinaryActionType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ActionTypeName.ToLower() == normalized);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            if (await query.AnyAsync(cancellationToken))
                return (false, "Nama tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstDisciplinaryActionType>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ActionTypeCode.StartsWith(CodePrefix))
                .Select(x => x.ActionTypeCode)
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
