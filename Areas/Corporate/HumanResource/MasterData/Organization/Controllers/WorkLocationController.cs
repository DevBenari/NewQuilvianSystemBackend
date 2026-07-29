using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

using WorkLocationPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.WorkLocationResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/work-locations")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Work Location",
        AreaName = "Corporate",
        ControllerName = "WorkLocation",
        Description = "Corporate human resource master data work location",
        SortOrder = 22)]
    [Tags("Corporate / Human Resource / Master Data / Work Location")]
    public class WorkLocationController : ControllerBase
    {
        private static readonly HashSet<string> AllowedLocationTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "WorkArea",
                "Office",
                "Clinic",
                "Ward",
                "Laboratory",
                "Pharmacy",
                "Warehouse",
                "Remote"
            };

        private const string CodePrefix = "WLC-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;

        public WorkLocationController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Work Location", Description = "Melihat metadata filter work location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkLocation", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WorkLocationFilterMetadataResponse
            {
                DefaultFilter = new WorkLocationDefaultFilterResponse(),
                LocationTypeOptions = AllowedLocationTypes
                    .OrderBy(x => x)
                    .Select(x => new WorkLocationStringOptionResponse
                    {
                        Value = x,
                        Label = x
                    })
                    .ToList(),
                SortOptions = new List<WorkLocationSortOptionResponse>
                {
                    new() { Value = "locationCode", Label = "Kode lokasi" },
                    new() { Value = "locationName", Label = "Nama lokasi" },
                    new() { Value = "locationType", Label = "Tipe lokasi" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isPrimary", Label = "Lokasi utama" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WorkLocationFilterMetadataResponse>.Ok(
                result,
                "Metadata filter work location berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Work Location", Description = "Melihat ringkasan work location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkLocation", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();

            var result = new WorkLocationSummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                PrimaryData = await query.CountAsync(x => x.IsPrimary),
                RemoteData = await query.CountAsync(x => x.LocationType == "Remote"),
                ClinicalAreaData = await query.CountAsync(x =>
                    x.LocationType == "Clinic" ||
                    x.LocationType == "Ward" ||
                    x.LocationType == "Laboratory" ||
                    x.LocationType == "Pharmacy")
            };

            return Ok(ApiResponse<WorkLocationSummaryResponse>.Ok(
                result,
                "Ringkasan work location berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Work Location", Description = "Melihat data work location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkLocation", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] Guid? departmentId,
            [FromQuery] string? locationType,
            [FromQuery] bool? isPrimary,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "locationName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);

            var query = ApplyFilter(
                BaseQuery(),
                legalEntityId,
                hospitalSiteId,
                organizationUnitId,
                departmentId,
                locationType,
                isPrimary,
                isActive,
                search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkLocationResponse
                {
                    Id = x.Id,
                    LegalEntityId = x.LegalEntityId,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    DepartmentId = x.DepartmentId,
                    LocationCode = x.LocationCode,
                    LocationName = x.LocationName,
                    LocationType = x.LocationType,
                    BuildingName = x.BuildingName,
                    FloorName = x.FloorName,
                    RoomName = x.RoomName,
                    Address = x.Address,
                    IsPrimary = x.IsPrimary,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            var result = new WorkLocationPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkLocationPagedResult>.Ok(
                result,
                "Data work location berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Work Location", Description = "Melihat pilihan work location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkLocation", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] Guid? legalEntityId,
            [FromQuery] Guid? hospitalSiteId,
            [FromQuery] Guid? organizationUnitId,
            [FromQuery] string? locationType,
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
                organizationUnitId,
                null,
                locationType,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsPrimary)
                .ThenBy(x => x.LocationName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new WorkLocationOptionResponse
                {
                    Id = x.Id,
                    HospitalSiteId = x.HospitalSiteId,
                    OrganizationUnitId = x.OrganizationUnitId,
                    LocationCode = x.LocationCode,
                    LocationName = x.LocationName,
                    LocationType = x.LocationType,
                    IsPrimary = x.IsPrimary
                })
                .ToListAsync();

            var result = new WorkLocationOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<WorkLocationOptionPagedResponse>.Ok(
                result,
                "Pilihan work location berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Work Location", Description = "Melihat detail work location", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkLocation", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work location tidak ditemukan."));
            }

            var result = new WorkLocationDetailResponse
            {
                Id = entity.Id,
                LegalEntityId = entity.LegalEntityId,
                HospitalSiteId = entity.HospitalSiteId,
                OrganizationUnitId = entity.OrganizationUnitId,
                DepartmentId = entity.DepartmentId,
                LocationCode = entity.LocationCode,
                LocationName = entity.LocationName,
                LocationType = entity.LocationType,
                BuildingName = entity.BuildingName,
                FloorName = entity.FloorName,
                RoomName = entity.RoomName,
                Address = entity.Address,
                IsPrimary = entity.IsPrimary,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            };

            return Ok(ApiResponse<WorkLocationDetailResponse>.Ok(
                result,
                "Detail work location berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Work Location", Description = "Membuat work location", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkLocation", "Create")]
        public async Task<IActionResult> Create(
            [FromBody] CreateWorkLocationRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data work location tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            if (request.IsPrimary)
                await UnsetPrimaryAsync(null, request.HospitalSiteId, now, actor);

            var entity = new MstWorkLocation
            {
                Id = Guid.NewGuid(),
                LegalEntityId = request.LegalEntityId,
                HospitalSiteId = request.HospitalSiteId,
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                DepartmentId = NormalizeGuid(request.DepartmentId),
                LocationCode = await GenerateCodeAsync(),
                LocationName = request.LocationName.Trim(),
                LocationType = NormalizeLocationType(request.LocationType),
                BuildingName = NormalizeText(request.BuildingName),
                FloorName = NormalizeText(request.FloorName),
                RoomName = NormalizeText(request.RoomName),
                Address = NormalizeText(request.Address),
                IsPrimary = request.IsPrimary,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstWorkLocation>().Add(entity);
            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new
                {
                    entity.Id,
                    entity.LocationCode,
                    entity.LocationName
                },
                "Work location berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Work Location", Description = "Mengubah work location", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkLocation", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateWorkLocationRequest request)
        {
            var entity = await _dbContext.Set<MstWorkLocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work location tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data work location tidak valid."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            if (request.IsPrimary && request.IsActive)
                await UnsetPrimaryAsync(id, request.HospitalSiteId, now, actor);

            entity.LegalEntityId = request.LegalEntityId;
            entity.HospitalSiteId = request.HospitalSiteId;
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.DepartmentId = NormalizeGuid(request.DepartmentId);
            entity.LocationName = request.LocationName.Trim();
            entity.LocationType = NormalizeLocationType(request.LocationType);
            entity.BuildingName = NormalizeText(request.BuildingName);
            entity.FloorName = NormalizeText(request.FloorName);
            entity.RoomName = NormalizeText(request.RoomName);
            entity.Address = NormalizeText(request.Address);
            entity.IsPrimary = request.IsPrimary && request.IsActive;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Work location berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Work Location Status", Description = "Mengubah status work location", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("WorkLocation", "Update")]
        public async Task<IActionResult> UpdateStatus(
            Guid id,
            [FromBody] UpdateWorkLocationStatusRequest request)
        {
            var entity = await _dbContext.Set<MstWorkLocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work location tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            if (request.IsPrimary == true && request.IsActive)
                await UnsetPrimaryAsync(id, entity.HospitalSiteId, now, actor);

            entity.IsActive = request.IsActive;

            if (request.IsPrimary.HasValue)
                entity.IsPrimary = request.IsPrimary.Value && request.IsActive;
            else if (!request.IsActive)
                entity.IsPrimary = false;

            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status work location berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Work Location", Description = "Menghapus work location", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("WorkLocation", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstWorkLocation>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Work location tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPrimary = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Work location berhasil dihapus."));
        }

        private IQueryable<MstWorkLocation> BaseQuery()
        {
            return _dbContext.Set<MstWorkLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstWorkLocation> ApplyFilter(
            IQueryable<MstWorkLocation> query,
            Guid? legalEntityId,
            Guid? hospitalSiteId,
            Guid? organizationUnitId,
            Guid? departmentId,
            string? locationType,
            bool? isPrimary,
            bool? isActive,
            string? search)
        {
            if (legalEntityId.HasValue && legalEntityId.Value != Guid.Empty)
                query = query.Where(x => x.LegalEntityId == legalEntityId.Value);

            if (hospitalSiteId.HasValue && hospitalSiteId.Value != Guid.Empty)
                query = query.Where(x => x.HospitalSiteId == hospitalSiteId.Value);

            if (organizationUnitId.HasValue && organizationUnitId.Value != Guid.Empty)
                query = query.Where(x => x.OrganizationUnitId == organizationUnitId.Value);

            if (departmentId.HasValue && departmentId.Value != Guid.Empty)
                query = query.Where(x => x.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(locationType))
                query = query.Where(x => x.LocationType == locationType.Trim());

            if (isPrimary.HasValue)
                query = query.Where(x => x.IsPrimary == isPrimary.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.LocationCode.ToLower().Contains(keyword) ||
                    x.LocationName.ToLower().Contains(keyword) ||
                    (x.BuildingName != null && x.BuildingName.ToLower().Contains(keyword)) ||
                    (x.RoomName != null && x.RoomName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstWorkLocation> ApplySorting(
            IQueryable<MstWorkLocation> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "locationName").Trim().ToLowerInvariant() switch
            {
                "locationcode" => desc
                    ? query.OrderByDescending(x => x.LocationCode)
                    : query.OrderBy(x => x.LocationCode),

                "locationtype" => desc
                    ? query.OrderByDescending(x => x.LocationType)
                    : query.OrderBy(x => x.LocationType),

                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "isprimary" => desc
                    ? query.OrderByDescending(x => x.IsPrimary)
                    : query.OrderBy(x => x.IsPrimary),

                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => desc
                    ? query.OrderByDescending(x => x.LocationName)
                    : query.OrderBy(x => x.LocationName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateWorkLocationRequest request)
        {
            if (request.LegalEntityId == Guid.Empty)
                return (false, "Legal entity wajib dipilih.");

            if (request.HospitalSiteId == Guid.Empty)
                return (false, "Hospital site wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.LocationName))
                return (false, "Nama work location wajib diisi.");

            if (!AllowedLocationTypes.Contains(request.LocationType.Trim()))
                return (false, "Location type tidak valid.");

            var hospitalValid = await _dbContext.Set<MstHospitalSite>()
                .AnyAsync(x =>
                    x.Id == request.HospitalSiteId &&
                    x.LegalEntityId == request.LegalEntityId &&
                    x.IsActive &&
                    !x.IsDelete);

            if (!hospitalValid)
                return (false, "Hospital site tidak valid untuk legal entity tersebut.");

            if (request.OrganizationUnitId.HasValue &&
                request.OrganizationUnitId.Value != Guid.Empty)
            {
                var organizationUnitValid = await _dbContext.Set<MstOrganizationUnit>()
                    .AnyAsync(x =>
                        x.Id == request.OrganizationUnitId.Value &&
                        x.LegalEntityId == request.LegalEntityId &&
                        (x.HospitalSiteId == null ||
                         x.HospitalSiteId == request.HospitalSiteId) &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!organizationUnitValid)
                    return (false, "Organization unit tidak valid untuk scope tersebut.");
            }

            var normalizedName = request.LocationName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstWorkLocation>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.HospitalSiteId == request.HospitalSiteId &&
                    x.LocationName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Nama work location sudah digunakan pada hospital site tersebut.");

            return (true, null);
        }

        private async Task UnsetPrimaryAsync(
            Guid? excludeId,
            Guid hospitalSiteId,
            DateTime now,
            Guid actor)
        {
            var query = _dbContext.Set<MstWorkLocation>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsPrimary &&
                    x.HospitalSiteId == hospitalSiteId);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var rows = await query.ToListAsync();

            foreach (var row in rows)
            {
                row.IsPrimary = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actor;
            }
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstWorkLocation>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.LocationCode.StartsWith(CodePrefix))
                .Select(x => x.LocationCode)
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

        private static string NormalizeLocationType(string value)
        {
            return AllowedLocationTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
