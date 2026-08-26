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

using CostCenterPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.CostCenterResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/cost-centers")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Cost Center",
        AreaName = "Corporate",
        ControllerName = "CostCenter",
        Description = "Corporate human resource master data cost center",
        SortOrder = 26)]
    [Tags("Corporate / Human Resource / Master Data / Cost Center")]
    public class CostCenterController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "CC-MMC-";
        private const int CodeNumberLength = 5;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CostCenterController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Cost Center", Description = "Melihat metadata filter cost center", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CostCenter", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new CostCenterFilterMetadataResponse
            {
                DefaultFilter = new CostCenterDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<CostCenterSortOptionResponse>
                {
                    new() { Value = "costCenterCode", Label = "Kode pusat biaya" },
                    new() { Value = "costCenterName", Label = "Nama pusat biaya" },
                    new() { Value = "legalEntityName", Label = "Entitas legal" },
                    new() { Value = "hospitalSiteName", Label = "Lokasi rumah sakit" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CostCenter.GetFilterMetadata",
                "Mengambil metadata filter cost center.",
                result
            );

            return Ok(ApiResponse<CostCenterFilterMetadataResponse>.Ok(result, "Metadata filter cost center berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Cost Center", Description = "Melihat ringkasan cost center", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CostCenter", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new CostCenterSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                WithoutHospitalSiteData = await query.CountAsync(x => !x.HospitalSiteId.HasValue)
            };

            return Ok(ApiResponse<CostCenterSummaryResponse>.Ok(result, "Ringkasan cost center berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Cost Center", Description = "Melihat data cost center", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CostCenter", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] Guid? departmentId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "costCenterName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), legalEntityId, hospitalSiteId, organizationUnitId, departmentId, isActive, search);
            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CostCenterResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    LegalEntityCode = x.LegalEntity != null ? x.LegalEntity.LegalEntityCode : string.Empty,
                    LegalEntityName = x.LegalEntity != null ? x.LegalEntity.LegalEntityName : string.Empty,
                    HospitalSiteId = x.HospitalSiteId,
                    HospitalSiteCode = x.HospitalSite != null ? x.HospitalSite.SiteCode : null,
                    HospitalSiteName = x.HospitalSite != null ? x.HospitalSite.SiteName : null,
                    OrganizationUnitId = x.OrganizationUnitId,
                    OrganizationUnitCode = x.OrganizationUnit != null ? x.OrganizationUnit.UnitCode : null,
                    OrganizationUnitName = x.OrganizationUnit != null ? x.OrganizationUnit.UnitName : null,
                    DepartmentId = x.DepartmentId,
                    DepartmentCode = x.Department != null ? x.Department.DepartmentCode : null,
                    DepartmentName = x.Department != null ? x.Department.DepartmentName : null,
                    CostCenterCode = x.CostCenterCode,
                    CostCenterName = x.CostCenterName,
                    AccountingCode = x.AccountingCode,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(items.Select(x => x.CreateBy));
            foreach (var item in items)
                item.CreateByName = GetActorName(actorNames, item.CreateBy);

            return Ok(ApiResponse<CostCenterPagedResult>.Ok(new CostCenterPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data cost center berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Cost Center", Description = "Melihat pilihan cost center", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CostCenter", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] Guid? departmentId,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), legalEntityId, hospitalSiteId, organizationUnitId, departmentId, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.CostCenterName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CostCenterOptionResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    CostCenterCode = x.CostCenterCode,
                    CostCenterName = x.CostCenterName,
                    AccountingCode = x.AccountingCode
                })
                .ToListAsync();

            return Ok(ApiResponse<CostCenterOptionPagedResponse>.Ok(new CostCenterOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan cost center berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Cost Center", Description = "Melihat detail cost center", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CostCenter", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Cost center tidak ditemukan."));

            var actorNames = await GetActorNameMapAsync(new Guid?[] { entity.CreateBy, entity.UpdateBy });

            return Ok(ApiResponse<CostCenterDetailResponse>.Ok(new CostCenterDetailResponse
            {
                Id = entity.Id,
                LegalEntityId = entity.LegalEntityId,
                LegalEntityCode = entity.LegalEntity?.LegalEntityCode ?? string.Empty,
                LegalEntityName = entity.LegalEntity?.LegalEntityName ?? string.Empty,
                HospitalSiteId = entity.HospitalSiteId,
                HospitalSiteCode = entity.HospitalSite?.SiteCode,
                HospitalSiteName = entity.HospitalSite?.SiteName,
                OrganizationUnitId = entity.OrganizationUnitId,
                OrganizationUnitCode = entity.OrganizationUnit?.UnitCode,
                OrganizationUnitName = entity.OrganizationUnit?.UnitName,
                DepartmentId = entity.DepartmentId,
                DepartmentCode = entity.Department?.DepartmentCode,
                DepartmentName = entity.Department?.DepartmentName,
                CostCenterCode = entity.CostCenterCode,
                CostCenterName = entity.CostCenterName,
                AccountingCode = entity.AccountingCode,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            }, "Detail cost center berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Cost Center", Description = "Membuat cost center", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("CostCenter", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateCostCenterRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var entity = new MstCostCenter
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                CostCenterCode = await GenerateCodeAsync(),
                CostCenterName = request.CostCenterName.Trim(),
                AccountingCode = NormalizeText(request.AccountingCode),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeText(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = CurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstCostCenter>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "CostCenter.Create",
                "Membuat data cost center.",
                new { entity.Id, entity.LegalEntityId, entity.HospitalSiteId, entity.OrganizationUnitId, entity.DepartmentId, entity.CostCenterCode, entity.CostCenterName, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.CostCenterCode, entity.CostCenterName }, "Cost center berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Cost Center", Description = "Mengubah cost center", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CostCenter", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCostCenterRequest request)
        {
            var entity = await _dbContext.Set<MstCostCenter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Cost center tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            entity.LegalEntityId = request.LegalEntityId;
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId);
            entity.CostCenterName = request.CostCenterName.Trim();
            entity.AccountingCode = NormalizeText(request.AccountingCode);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "CostCenter.Update",
                "Mengubah data cost center.",
                new { entity.Id, entity.LegalEntityId, entity.HospitalSiteId, entity.OrganizationUnitId, entity.DepartmentId, entity.CostCenterCode, entity.CostCenterName, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Cost center berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Cost Center Status", Description = "Mengubah status cost center", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("CostCenter", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCostCenterStatusRequest request)
        {
            var entity = await _dbContext.Set<MstCostCenter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Cost center tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = CurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status cost center berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Cost Center", Description = "Menghapus cost center", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("CostCenter", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstCostCenter>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Cost center tidak ditemukan."));

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
                "CostCenter.Delete",
                "Menghapus data cost center.",
                new { entity.Id, entity.CostCenterCode, entity.CostCenterName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Cost center berhasil dihapus."));
        }

        private IQueryable<MstCostCenter> BaseQuery() =>
            _dbContext.Set<MstCostCenter>()
                .AsNoTracking()
                .Include(x => x.LegalEntity)
                .Include(x => x.HospitalSite)
                .Include(x => x.OrganizationUnit)
                .Include(x => x.Department)
                .Where(x => !x.IsDelete);

        private static IQueryable<MstCostCenter> ApplyFilter(
            IQueryable<MstCostCenter> query,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty) query = query.Where(x => x.LegalEntityId == legalEntityId.Value);
            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty) query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);
            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty) query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);
            if (departmentId.HasValue && departmentId.Value != Guid.Empty) query = query.Where(x => x.DepartmentId == departmentId.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CostCenterCode.ToLower().Contains(keyword) ||
                    x.CostCenterName.ToLower().Contains(keyword) ||
                    (x.AccountingCode != null && x.AccountingCode.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.LegalEntity != null && x.LegalEntity.LegalEntityName.ToLower().Contains(keyword)) ||
                    (x.HospitalSite != null && x.HospitalSite.SiteName.ToLower().Contains(keyword)) ||
                    (x.OrganizationUnit != null && x.OrganizationUnit.UnitName.ToLower().Contains(keyword)) ||
                    (x.Department != null && x.Department.DepartmentName.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstCostCenter> ApplySorting(IQueryable<MstCostCenter> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "costCenterName").Trim().ToLowerInvariant() switch
            {
                "costcentercode" => desc ? query.OrderByDescending(x => x.CostCenterCode) : query.OrderBy(x => x.CostCenterCode),
                "legalentityname" => desc
                    ? query.OrderByDescending(x => x.LegalEntity != null ? x.LegalEntity.LegalEntityName : string.Empty).ThenByDescending(x => x.CostCenterName)
                    : query.OrderBy(x => x.LegalEntity != null ? x.LegalEntity.LegalEntityName : string.Empty).ThenBy(x => x.CostCenterName),
                "hospitalsitename" => desc
                    ? query.OrderByDescending(x => x.HospitalSite != null ? x.HospitalSite.SiteName : string.Empty).ThenByDescending(x => x.CostCenterName)
                    : query.OrderBy(x => x.HospitalSite != null ? x.HospitalSite.SiteName : string.Empty).ThenBy(x => x.CostCenterName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CostCenterName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.CostCenterName),
                _ => desc ? query.OrderByDescending(x => x.CostCenterName) : query.OrderBy(x => x.CostCenterName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateCostCenterRequest request)
        {
            if (request.LegalEntityId == Guid.Empty) return (false, "Legal entity wajib dipilih.");
            if (string.IsNullOrWhiteSpace(request.CostCenterName)) return (false, "Nama cost center wajib diisi.");
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");

            var legalEntityExists = await _dbContext.Set<MstLegalEntity>().AsNoTracking()
                .AnyAsync(x => x.Id == request.LegalEntityId && x.IsActive && !x.IsDelete);
            if (!legalEntityExists) return (false, "Legal entity tidak ditemukan atau tidak aktif.");

            var hospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            if (hospitalSiteId.HasValue)
            {
                var siteExists = await _dbContext.Set<MstHospitalSite>().AsNoTracking()
                    .AnyAsync(x => x.Id == hospitalSiteId.Value && x.LegalEntityId == request.LegalEntityId && x.IsActive && !x.IsDelete);
                if (!siteExists) return (false, "Hospital site tidak ditemukan, tidak aktif, atau tidak sesuai legal entity.");
            }

            var organizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            if (organizationUnitId.HasValue)
            {
                var unit = await _dbContext.Set<MstOrganizationUnit>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.Id == organizationUnitId.Value && x.IsActive && !x.IsDelete);
                if (unit == null) return (false, "Organization unit tidak ditemukan atau tidak aktif.");
                if (unit.LegalEntityId != request.LegalEntityId) return (false, "Organization unit tidak sesuai legal entity.");
                if (hospitalSiteId.HasValue && unit.HospitalSiteId.HasValue && unit.HospitalSiteId.Value != hospitalSiteId.Value)
                    return (false, "Organization unit tidak sesuai hospital site.");
            }

            var departmentId = NormalizeGuid(request.DepartmentId);
            if (departmentId.HasValue)
            {
                var departmentExists = await _dbContext.Set<MstDepartment>().AsNoTracking()
                    .AnyAsync(x => x.Id == departmentId.Value && x.IsActive && !x.IsDelete);
                if (!departmentExists) return (false, "Department tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.CostCenterName.Trim().ToLower();
            var duplicateName = _dbContext.Set<MstCostCenter>().AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityId == request.LegalEntityId && x.CostCenterName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateName = duplicateName.Where(x => x.Id != excludeId.Value);
            if (await duplicateName.AnyAsync()) return (false, "Nama cost center sudah digunakan pada legal entity tersebut.");

            var accountingCode = NormalizeText(request.AccountingCode);
            if (accountingCode != null)
            {
                var duplicateAccounting = _dbContext.Set<MstCostCenter>().AsNoTracking()
                    .Where(x => !x.IsDelete && x.AccountingCode == accountingCode);
                if (excludeId.HasValue) duplicateAccounting = duplicateAccounting.Where(x => x.Id != excludeId.Value);
                if (await duplicateAccounting.AnyAsync()) return (false, "Accounting code sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstCostCenter>().AsNoTracking()
                .Where(x => !x.IsDelete && x.CostCenterCode.StartsWith(CodePrefix))
                .Select(x => x.CostCenterCode)
                .ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

        private async Task<Dictionary<Guid, string>> GetActorNameMapAsync(IEnumerable<Guid?> actorIds)
        {
            var ids = actorIds
                .Where(x => x.HasValue && x.Value != Guid.Empty)
                .Select(x => x!.Value)
                .Distinct()
                .ToList();
            if (ids.Count == 0) return new Dictionary<Guid, string>();

            return await _dbContext.Users.AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode);
        }

        private static string? GetActorName(IReadOnlyDictionary<Guid, string> actorNames, Guid? actorId) =>
            !actorId.HasValue || actorId.Value == Guid.Empty ? null : actorNames.GetValueOrDefault(actorId.Value);

        private Guid CurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static void NormalizePaging(ref int pageNumber, ref int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : Math.Min(pageSize, 100);
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static List<CostCenterCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<CostCenterCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
