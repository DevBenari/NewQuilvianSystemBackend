using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workflow.Controllers;

using HospitalSitePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.HospitalSiteResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/hospital-sites")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Hospital Site",
        AreaName = "Corporate",
        ControllerName = "HospitalSite",
        Description = "Corporate human resource master data hospital site",
        SortOrder = 20)]
    [Tags("Corporate / Human Resource / Master Data / Hospital Site")]
    public class HospitalSiteController : ControllerBase
    {
        private static readonly HashSet<string> AllowedSiteTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Hospital",
                "Clinic",
                "Laboratory",
                "Office",
                "Warehouse",
                "TrainingCenter",
                "Other"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "HST-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public HospitalSiteController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Hospital Site", Description = "Melihat metadata filter hospital site", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HospitalSite", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new HospitalSiteFilterMetadataResponse
            {
                DefaultFilter = new HospitalSiteDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SiteTypeOptions = AllowedSiteTypes
                    .OrderBy(x => x)
                    .Select(x => new HospitalSiteStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                SortOptions = new List<HospitalSiteSortOptionResponse>
                {
                    new() { Value = "siteCode", Label = "Kode site" },
                    new() { Value = "siteName", Label = "Nama site" },
                    new() { Value = "siteType", Label = "Tipe site" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "HospitalSite.GetFilterMetadata",
                "Mengambil metadata filter hospital site.",
                result
            );

            return Ok(ApiResponse<HospitalSiteFilterMetadataResponse>.Ok(
                result,
                "Metadata filter hospital site berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Hospital Site", Description = "Melihat ringkasan hospital site", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HospitalSite", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();

            var result = new HospitalSiteSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                MainSiteData = await query.CountAsync(x => x.IsMainSite),
                HospitalData = await query.CountAsync(x => x.SiteType == "Hospital"),
                ClinicData = await query.CountAsync(x => x.SiteType == "Clinic")
            };

            return Ok(ApiResponse<HospitalSiteSummaryResponse>.Ok(
                result,
                "Ringkasan hospital site berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Hospital Site", Description = "Melihat data hospital site", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HospitalSite", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? legalEntityId,
            [FromQuery] string? siteType,
            [FromQuery] bool? isMainSite,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "siteName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);

            var query = ApplyFilter(
                BaseQuery(),
                legalEntityId,
                siteType,
                isMainSite,
                isActive,
                search);

            query = WorkflowMasterDataSupport.ApplyDateFilter(query, startDate, endDate, customPeriod);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HospitalSiteResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    SiteCode = x.SiteCode,
                    SiteName = x.SiteName,
                    SiteType = x.SiteType,
                    AccreditationNumber = x.AccreditationNumber,
                    TimeZoneId = x.TimeZoneId,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Address = x.Address,
                    CountryId = x.CountryId,
                    ProvinceId = x.ProvinceId,
                    CityId = x.CityId,
                    DistrictId = x.DistrictId,
                    PostalCodeId = x.PostalCodeId,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsMainSite = x.IsMainSite,
                    IsActive = x.IsActive,
                    OrganizationUnitCount = x.OrganizationUnits.Count(y => !y.IsDelete),
                    CostCenterCount = x.CostCenters.Count(y => !y.IsDelete),
                    WorkLocationCount = x.WorkLocations.Count(y => !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var result = new HospitalSitePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<HospitalSitePagedResult>.Ok(
                result,
                "Data hospital site berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Hospital Site", Description = "Melihat pilihan hospital site", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HospitalSite", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] string? siteType,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);

            var query = ApplyFilter(
                BaseQuery(),
                legalEntityId,
                siteType,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsMainSite)
                .ThenBy(x => x.SiteName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new HospitalSiteOptionResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    SiteCode = x.SiteCode,
                    SiteName = x.SiteName,
                    SiteType = x.SiteType,
                    IsMainSite = x.IsMainSite
                })
                .ToListAsync();

            var result = new HospitalSiteOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<HospitalSiteOptionPagedResponse>.Ok(
                result,
                "Pilihan hospital site berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Hospital Site", Description = "Melihat detail hospital site", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("HospitalSite", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Hospital site tidak ditemukan."));
            }

            var result = new HospitalSiteDetailResponse
            {
                Id = entity.Id,
                LegalEntityId = entity.LegalEntityId,
                SiteCode = entity.SiteCode,
                SiteName = entity.SiteName,
                SiteType = entity.SiteType,
                AccreditationNumber = entity.AccreditationNumber,
                TimeZoneId = entity.TimeZoneId,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                CountryId = entity.CountryId,
                ProvinceId = entity.ProvinceId,
                CityId = entity.CityId,
                DistrictId = entity.DistrictId,
                PostalCodeId = entity.PostalCodeId,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsMainSite = entity.IsMainSite,
                IsActive = entity.IsActive,
                OrganizationUnitCount = entity.OrganizationUnits.Count(x => !x.IsDelete),
                CostCenterCount = entity.CostCenters.Count(x => !x.IsDelete),
                WorkLocationCount = entity.WorkLocations.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            };

            return Ok(ApiResponse<HospitalSiteDetailResponse>.Ok(
                result,
                "Detail hospital site berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Hospital Site", Description = "Membuat hospital site", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("HospitalSite", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateHospitalSiteRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data hospital site tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            if (request.IsMainSite)
                await UnsetMainSiteAsync(null, request.LegalEntityId, now, actor);

            var entity = new MstHospitalSite
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                SiteCode = await GenerateCodeAsync(),
                SiteName = request.SiteName.Trim(),
                SiteType = NormalizeSiteType(request.SiteType),
                AccreditationNumber = NormalizeText(request.AccreditationNumber),
                TimeZoneId = NormalizeText(request.TimeZoneId) ?? "Asia/Jakarta",
                Email = NormalizeText(request.Email),
                PhoneNumber = NormalizeText(request.PhoneNumber),
                Address = NormalizeText(request.Address),
                CountryId = NormalizeGuid(request.CountryId),
                ProvinceId = NormalizeGuid(request.ProvinceId),
                CityId = NormalizeGuid(request.CityId),
                DistrictId = NormalizeGuid(request.DistrictId),
                PostalCodeId = NormalizeGuid(request.PostalCodeId),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                IsMainSite = request.IsMainSite,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstHospitalSite>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "HospitalSite.Create",
                "Membuat data hospital site.",
                new { entity.Id, entity.LegalEntityId, entity.SiteCode, entity.SiteName, entity.SiteType, entity.IsMainSite, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    entity.Id,
                    entity.SiteCode,
                    entity.SiteName
                },
                "Hospital site berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Hospital Site", Description = "Mengubah hospital site", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("HospitalSite", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateHospitalSiteRequest request)
        {
            var entity = await _dbContext.Set<MstHospitalSite>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Hospital site tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data hospital site tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            if (request.IsMainSite && request.IsActive)
                await UnsetMainSiteAsync(id, request.LegalEntityId, now, actor);

            entity.LegalEntityId = request.LegalEntityId;
            entity.SiteName = request.SiteName.Trim();
            entity.SiteType = NormalizeSiteType(request.SiteType);
            entity.AccreditationNumber = NormalizeText(request.AccreditationNumber);
            entity.TimeZoneId = NormalizeText(request.TimeZoneId) ?? "Asia/Jakarta";
            entity.Email = NormalizeText(request.Email);
            entity.PhoneNumber = NormalizeText(request.PhoneNumber);
            entity.Address = NormalizeText(request.Address);
            entity.CountryId = NormalizeGuid(request.CountryId);
            entity.ProvinceId = NormalizeGuid(request.ProvinceId);
            entity.CityId = NormalizeGuid(request.CityId);
            entity.DistrictId = NormalizeGuid(request.DistrictId);
            entity.PostalCodeId = NormalizeGuid(request.PostalCodeId);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.IsMainSite = request.IsMainSite && request.IsActive;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "HospitalSite.Update",
                "Mengubah data hospital site.",
                new { entity.Id, entity.LegalEntityId, entity.SiteCode, entity.SiteName, entity.SiteType, entity.IsMainSite, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Hospital site berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Hospital Site Status", Description = "Mengubah status hospital site", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("HospitalSite", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateHospitalSiteStatusRequest request)
        {
            var entity = await _dbContext.Set<MstHospitalSite>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Hospital site tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            if (request.IsMainSite == true && request.IsActive)
                await UnsetMainSiteAsync(id, entity.LegalEntityId, now, actor);

            entity.IsActive = request.IsActive;

            if (request.IsMainSite.HasValue)
                entity.IsMainSite = request.IsMainSite.Value && request.IsActive;
            else if (!request.IsActive)
                entity.IsMainSite = false;

            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status hospital site berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Hospital Site", Description = "Menghapus hospital site", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("HospitalSite", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstHospitalSite>()
                .Include(x => x.OrganizationUnits)
                .Include(x => x.CostCenters)
                .Include(x => x.WorkLocations)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Hospital site tidak ditemukan."));
            }

            var isUsed =
                entity.OrganizationUnits.Any(x => !x.IsDelete) ||
                entity.CostCenters.Any(x => !x.IsDelete) ||
                entity.WorkLocations.Any(x => !x.IsDelete);

            if (isUsed)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Hospital site tidak dapat dihapus karena masih digunakan."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsMainSite = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "HospitalSite.Delete",
                "Menghapus data hospital site.",
                new { entity.Id, entity.SiteCode, entity.SiteName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Hospital site berhasil dihapus."));
        }

        private IQueryable<MstHospitalSite> BaseQuery()
        {
            return _dbContext.Set<MstHospitalSite>()
                .AsNoTracking()
                .Include(x => x.OrganizationUnits)
                .Include(x => x.CostCenters)
                .Include(x => x.WorkLocations)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstHospitalSite> ApplyFilter(
            IQueryable<MstHospitalSite> query,
            Guid? legalEntityId,
            string? siteType,
            bool? isMainSite,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == legalEntityId.Value);

            if (!string.IsNullOrWhiteSpace(siteType))
                query = query.Where(x => x.SiteType == siteType.Trim());

            if (isMainSite.HasValue)
                query = query.Where(x => x.IsMainSite == isMainSite.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.SiteCode.ToLower().Contains(keyword) ||
                    x.SiteName.ToLower().Contains(keyword) ||
                    (x.Address != null && x.Address.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstHospitalSite> ApplySorting(
            IQueryable<MstHospitalSite> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "siteName").Trim().ToLowerInvariant() switch
            {
                "sitecode" => desc
                    ? query.OrderByDescending(x => x.SiteCode)
                    : query.OrderBy(x => x.SiteCode),

                "sitetype" => desc
                    ? query.OrderByDescending(x => x.SiteType)
                    : query.OrderBy(x => x.SiteType),

                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => desc
                    ? query.OrderByDescending(x => x.SiteName)
                    : query.OrderBy(x => x.SiteName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateHospitalSiteRequest request)
        {
            if (request.LegalEntityId == Guid.Empty)
                return (false, "Legal entity wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.SiteName))
                return (false, "Nama hospital site wajib diisi.");

            if (!AllowedSiteTypes.Contains(request.SiteType.Trim()))
                return (false, "Site type tidak valid.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");
            }

            if (!await _dbContext.Set<MstLegalEntity>()
                    .AnyAsync(x =>
                        x.Id == request.LegalEntityId &&
                        x.IsActive &&
                        !x.IsDelete))
            {
                return (false, "Legal entity tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.SiteName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstHospitalSite>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.LegalEntityId == request.LegalEntityId &&
                    x.SiteName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama hospital site sudah digunakan pada legal entity tersebut.");

            return (true, null);
        }

        private async Task UnsetMainSiteAsync(
            Guid? excludeId,
            Guid legalEntityId,
            DateTime now,
            Guid actor)
        {
            var query = _dbContext.Set<MstHospitalSite>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsMainSite &&
                    x.LegalEntityId == legalEntityId);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                row.IsMainSite = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actor;
            }
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstHospitalSite>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.SiteCode.StartsWith(CodePrefix))
                .Select(x => x.SiteCode)
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

        private static string NormalizeSiteType(string value)
        {
            return AllowedSiteTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static List<HospitalSiteCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<HospitalSiteCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
