using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using OrganizationUnitPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.OrganizationUnitResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/organization-units")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Organization Unit",
        AreaName = "Corporate",
        ControllerName = "OrganizationUnit",
        Description = "Corporate human resource master data organization unit",
        SortOrder = 21)]
    [Tags("Corporate / Human Resource / Master Data / Organization Unit")]
    public class OrganizationUnitController : ControllerBase
    {
        private static readonly HashSet<string> AllowedUnitTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Directorate",
                "Division",
                "Installation",
                "Department",
                "Section",
                "Team",
                "Unit"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "ORG-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public OrganizationUnitController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Organization Unit", Description = "Melihat metadata filter organization unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OrganizationUnit", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new OrganizationUnitFilterMetadataResponse
            {
                DefaultFilter = new OrganizationUnitDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                UnitTypeOptions = AllowedUnitTypes
                    .OrderBy(x => x)
                    .Select(x => new OrganizationUnitStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                SortOptions = new List<OrganizationUnitSortOptionResponse>
                {
                    new() { Value = "unitCode", Label = "Kode unit" },
                    new() { Value = "unitName", Label = "Nama unit" },
                    new() { Value = "unitType", Label = "Tipe unit" },
                    new() { Value = "levelNumber", Label = "Level" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "OrganizationUnit.GetFilterMetadata",
                "Mengambil metadata filter organization unit.",
                result
            );

            return Ok(ApiResponse<OrganizationUnitFilterMetadataResponse>.Ok(
                result,
                "Metadata filter organization unit berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Organization Unit", Description = "Melihat ringkasan organization unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OrganizationUnit", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();

            var result = new OrganizationUnitSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                OperationalData = await query.CountAsync(x => x.IsOperationalUnit),
                RootUnitData = await query.CountAsync(x => x.ParentOrganizationUnitId == null),
                ChildUnitData = await query.CountAsync(x => x.ParentOrganizationUnitId != null)
            };

            return Ok(ApiResponse<OrganizationUnitSummaryResponse>.Ok(
                result,
                "Ringkasan organization unit berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Organization Unit", Description = "Melihat data organization unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OrganizationUnit", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? parentOrganizationUnitId,
            [FromQuery] string? unitType,
            [FromQuery] bool? isOperationalUnit,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "unitName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);

            var query = ApplyFilter(
                BaseQuery(),
                legalEntityId,
                hospitalSiteId,
                parentOrganizationUnitId,
                unitType,
                isOperationalUnit,
                isActive,
                search);

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrganizationUnitResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    ParentOrganizationUnitId = x.ParentOrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    UnitType = x.UnitType,
                    LevelNumber = x.LevelNumber,
                    IsOperationalUnit = x.IsOperationalUnit,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    ChildOrganizationUnitCount = x.ChildOrganizationUnits.Count(y => !y.IsDelete),
                    CostCenterCount = x.CostCenters.Count(y => !y.IsDelete),
                    WorkLocationCount = x.WorkLocations.Count(y => !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var result = new OrganizationUnitPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<OrganizationUnitPagedResult>.Ok(
                result,
                "Data organization unit berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Organization Unit", Description = "Melihat pilihan organization unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OrganizationUnit", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? parentOrganizationUnitId,
            [FromQuery] string? unitType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);

            var query = ApplyFilter(
                BaseQuery(),
                legalEntityId,
                hospitalSiteId,
                parentOrganizationUnitId,
                unitType,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.LevelNumber)
                .ThenBy(x => x.UnitName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new OrganizationUnitOptionResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    ParentOrganizationUnitId = x.ParentOrganizationUnitId,
                    UnitCode = x.UnitCode,
                    UnitName = x.UnitName,
                    UnitType = x.UnitType,
                    LevelNumber = x.LevelNumber
                })
                .ToListAsync();

            var result = new OrganizationUnitOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<OrganizationUnitOptionPagedResponse>.Ok(
                result,
                "Pilihan organization unit berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Organization Unit", Description = "Melihat detail organization unit", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("OrganizationUnit", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization unit tidak ditemukan."));
            }

            var result = new OrganizationUnitDetailResponse
            {
                Id = entity.Id,
                LegalEntityId = entity.LegalEntityId,
                HospitalSiteId = entity.HospitalSiteId,
                ParentOrganizationUnitId = entity.ParentOrganizationUnitId,
                DepartmentId = entity.DepartmentId,
                UnitCode = entity.UnitCode,
                UnitName = entity.UnitName,
                UnitType = entity.UnitType,
                LevelNumber = entity.LevelNumber,
                IsOperationalUnit = entity.IsOperationalUnit,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                ChildOrganizationUnitCount = entity.ChildOrganizationUnits.Count(x => !x.IsDelete),
                CostCenterCount = entity.CostCenters.Count(x => !x.IsDelete),
                WorkLocationCount = entity.WorkLocations.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            };

            return Ok(ApiResponse<OrganizationUnitDetailResponse>.Ok(
                result,
                "Detail organization unit berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Organization Unit", Description = "Membuat organization unit", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("OrganizationUnit", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateOrganizationUnitRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data organization unit tidak valid."));
            }

            var entity = new MstOrganizationUnit
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                ParentOrganizationUnitId = NormalizeGuid(request.ParentOrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                UnitCode = await GenerateCodeAsync(),
                UnitName = request.UnitName.Trim(),
                UnitType = NormalizeUnitType(request.UnitType),
                LevelNumber = request.LevelNumber,
                IsOperationalUnit = request.IsOperationalUnit,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = CurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstOrganizationUnit>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "OrganizationUnit.Create",
                "Membuat data organization unit.",
                new { entity.Id, entity.LegalEntityId, entity.HospitalSiteId, entity.ParentOrganizationUnitId, entity.DepartmentId, entity.UnitCode, entity.UnitName, entity.UnitType, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    entity.Id,
                    entity.UnitCode,
                    entity.UnitName
                },
                "Organization unit berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Organization Unit", Description = "Mengubah organization unit", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("OrganizationUnit", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateOrganizationUnitRequest request)
        {
            var entity = await _dbContext.Set<MstOrganizationUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization unit tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data organization unit tidak valid."));
            }

            entity.LegalEntityId = request.LegalEntityId;
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.ParentOrganizationUnitId = NormalizeGuid(request.ParentOrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId);
            entity.UnitName = request.UnitName.Trim();
            entity.UnitType = NormalizeUnitType(request.UnitType);
            entity.LevelNumber = request.LevelNumber;
            entity.IsOperationalUnit = request.IsOperationalUnit;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "OrganizationUnit.Update",
                "Mengubah data organization unit.",
                new { entity.Id, entity.LegalEntityId, entity.HospitalSiteId, entity.ParentOrganizationUnitId, entity.DepartmentId, entity.UnitCode, entity.UnitName, entity.UnitType, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Organization unit berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Organization Unit Status", Description = "Mengubah status organization unit", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("OrganizationUnit", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateOrganizationUnitStatusRequest request)
        {
            var entity = await _dbContext.Set<MstOrganizationUnit>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization unit tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status organization unit berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Organization Unit", Description = "Menghapus organization unit", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("OrganizationUnit", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstOrganizationUnit>()
                .Include(x => x.ChildOrganizationUnits)
                .Include(x => x.CostCenters)
                .Include(x => x.WorkLocations)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Organization unit tidak ditemukan."));
            }

            var isUsed =
                entity.ChildOrganizationUnits.Any(x => !x.IsDelete) ||
                entity.CostCenters.Any(x => !x.IsDelete) ||
                entity.WorkLocations.Any(x => !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Organization unit tidak dapat dihapus karena masih digunakan."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "OrganizationUnit.Delete",
                "Menghapus data organization unit.",
                new { entity.Id, entity.UnitCode, entity.UnitName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Organization unit berhasil dihapus."));
        }

        private IQueryable<MstOrganizationUnit> BaseQuery()
        {
            return _dbContext.Set<MstOrganizationUnit>()
                .AsNoTracking()
                .Include(x => x.ChildOrganizationUnits)
                .Include(x => x.CostCenters)
                .Include(x => x.WorkLocations)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstOrganizationUnit> ApplyFilter(
            IQueryable<MstOrganizationUnit> query,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? parentOrganizationUnitId,
            string? unitType,
            bool? isOperationalUnit,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == legalEntityId.Value);

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);

            if (parentOrganizationUnitId.HasValue &&
                parentOrganizationUnitId.Value != Guid.Empty)
            {
                query = query.Where(x =>
                    x.ParentOrganizationUnitId == parentOrganizationUnitId.Value);
            }

            if (!string.IsNullOrWhiteSpace(unitType))
                query = query.Where(x => x.UnitType == unitType.Trim());

            if (isOperationalUnit.HasValue)
                query = query.Where(x => x.IsOperationalUnit == isOperationalUnit.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.UnitCode.ToLower().Contains(keyword) ||
                    x.UnitName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstOrganizationUnit> ApplySorting(
            IQueryable<MstOrganizationUnit> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "unitName").Trim().ToLowerInvariant() switch
            {
                "unitcode" => desc
                    ? query.OrderByDescending(x => x.UnitCode)
                    : query.OrderBy(x => x.UnitCode),

                "unittype" => desc
                    ? query.OrderByDescending(x => x.UnitType)
                    : query.OrderBy(x => x.UnitType),

                "levelnumber" => desc
                    ? query.OrderByDescending(x => x.LevelNumber)
                    : query.OrderBy(x => x.LevelNumber),

                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => desc
                    ? query.OrderByDescending(x => x.UnitName)
                    : query.OrderBy(x => x.UnitName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateOrganizationUnitRequest request)
        {
            if (request.LegalEntityId == Guid.Empty)
                return (false, "Legal entity wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.UnitName))
                return (false, "Nama organization unit wajib diisi.");

            if (!AllowedUnitTypes.Contains(request.UnitType.Trim()))
                return (false, "Unit type tidak valid.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (excludeId.HasValue &&
                request.ParentOrganizationUnitId == excludeId.Value)
            {
                return (false, "Organization unit tidak dapat menjadi parent untuk dirinya sendiri.");
            }

            if (!await _dbContext.Set<MstLegalEntity>()
                    .AnyAsync(x =>
                        x.Id == request.LegalEntityId &&
                        x.IsActive &&
                        !x.IsDelete))
            {
                return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            }

            if (request.HospitalSiteId.HasValue &&
                request.HospitalSiteId.Value != Guid.Empty)
            {
                var hospitalValid = await _dbContext.Set<MstHospitalSite>()
                    .AnyAsync(x =>
                        x.Id == request.HospitalSiteId.Value &&
                        x.LegalEntityId == request.LegalEntityId &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!hospitalValid)
                    return (false, "Hospital site tidak valid untuk legal entity tersebut.");
            }

            if (request.ParentOrganizationUnitId.HasValue &&
                request.ParentOrganizationUnitId.Value != Guid.Empty)
            {
                var parentValid = await _dbContext.Set<MstOrganizationUnit>()
                    .AnyAsync(x =>
                        x.Id == request.ParentOrganizationUnitId.Value &&
                        x.LegalEntityId == request.LegalEntityId &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!parentValid)
                    return (false, "Parent organization unit tidak valid.");
            }

            var normalizedName = request.UnitName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstOrganizationUnit>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.LegalEntityId == request.LegalEntityId &&
                    x.UnitName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama organization unit sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstOrganizationUnit>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.UnitCode.StartsWith(CodePrefix))
                .Select(x => x.UnitCode)
                .ToListAsync();

            var used = codes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            var next = 1;

            while (used.Contains(next))
                next++;

            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private Guid CurrentUserId()
        {
            var value =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        private static void NormalizePaging(
            ref int pageNumber,
            ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeUnitType(string value)
        {
            return AllowedUnitTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static List<OrganizationUnitCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<OrganizationUnitCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
