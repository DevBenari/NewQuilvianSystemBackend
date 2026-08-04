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

using LegalEntityPagedResult = QuilvianSystemBackend.Responses.PagedResult<QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.DTOs.LegalEntityResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/legal-entities")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Legal Entity",
        AreaName = "Corporate",
        ControllerName = "LegalEntity",
        Description = "Corporate human resource master data legal entity",
        SortOrder = 19)]
    [Tags("Corporate / Human Resource / Master Data / Legal Entity")]
    public class LegalEntityController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "LE-MMC-";
        private const int CodeNumberLength = 3;
        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LegalEntityController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [AccessAction("Read", "Read Legal Entity", Description = "Melihat metadata filter legal entity", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LegalEntity", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new LegalEntityFilterMetadataResponse
            {
                DefaultFilter = new LegalEntityDefaultFilterResponse(),
                SortOptions = new List<LegalEntitySortOptionResponse>
                {
                    new() { Value = "legalEntityCode", Label = "Kode entitas legal" },
                    new() { Value = "legalEntityName", Label = "Nama entitas legal" },
                    new() { Value = "shortName", Label = "Nama singkat" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "isDefault", Label = "Entitas default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "LegalEntity.GetFilterMetadata",
                "Mengambil metadata filter legal entity.",
                result
            );

            return Ok(ApiResponse<LegalEntityFilterMetadataResponse>.Ok(result, "Metadata filter legal entity berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Legal Entity", Description = "Melihat ringkasan legal entity", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LegalEntity", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BaseQuery();
            var result = new LegalEntitySummaryResponse
            {
                TotalData = await query.CountAsync(),
                ActiveData = await query.CountAsync(x => x.IsActive),
                InactiveData = await query.CountAsync(x => !x.IsActive),
                DefaultData = await query.CountAsync(x => x.IsDefault && x.IsActive)
            };

            return Ok(ApiResponse<LegalEntitySummaryResponse>.Ok(result, "Ringkasan legal entity berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Legal Entity", Description = "Melihat data legal entity", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LegalEntity", "Read")]
        public async Task<IActionResult> GetData(
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "legalEntityName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), isDefault, isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LegalEntityResponse
                {
                    Id = x.Id,
                    LegalEntityCode = x.LegalEntityCode,
                    LegalEntityName = x.LegalEntityName,
                    ShortName = x.ShortName,
                    TaxIdentificationNumber = x.TaxIdentificationNumber,
                    BusinessRegistrationNumber = x.BusinessRegistrationNumber,
                    Email = x.Email,
                    PhoneNumber = x.PhoneNumber,
                    Address = x.Address,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive,
                    HospitalSiteCount = x.HospitalSites.Count(y => !y.IsDelete),
                    OrganizationUnitCount = x.OrganizationUnits.Count(y => !y.IsDelete),
                    CostCenterCount = x.CostCenters.Count(y => !y.IsDelete),
                    WorkLocationCount = x.WorkLocations.Count(y => !y.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy
                })
                .ToListAsync();

            return Ok(ApiResponse<LegalEntityPagedResult>.Ok(new LegalEntityPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Data legal entity berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Legal Entity", Description = "Melihat pilihan legal entity", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LegalEntity", "Read")]
        public async Task<IActionResult> GetOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            NormalizePaging(ref pageNumber, ref pageSize);
            var query = ApplyFilter(BaseQuery(), null, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();

            var items = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.LegalEntityName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LegalEntityOptionResponse
                {
                    Id = x.Id,
                    LegalEntityCode = x.LegalEntityCode,
                    LegalEntityName = x.LegalEntityName,
                    ShortName = x.ShortName,
                    IsDefault = x.IsDefault
                })
                .ToListAsync();

            return Ok(ApiResponse<LegalEntityOptionPagedResponse>.Ok(new LegalEntityOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            }, "Pilihan legal entity berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Legal Entity", Description = "Melihat detail legal entity", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LegalEntity", "Read")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var entity = await BaseQuery().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Legal entity tidak ditemukan."));

            return Ok(ApiResponse<LegalEntityDetailResponse>.Ok(new LegalEntityDetailResponse
            {
                Id = entity.Id,
                LegalEntityCode = entity.LegalEntityCode,
                LegalEntityName = entity.LegalEntityName,
                ShortName = entity.ShortName,
                TaxIdentificationNumber = entity.TaxIdentificationNumber,
                BusinessRegistrationNumber = entity.BusinessRegistrationNumber,
                Email = entity.Email,
                PhoneNumber = entity.PhoneNumber,
                Address = entity.Address,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                HospitalSiteCount = entity.HospitalSites.Count(x => !x.IsDelete),
                OrganizationUnitCount = entity.OrganizationUnits.Count(x => !x.IsDelete),
                CostCenterCount = entity.CostCenters.Count(x => !x.IsDelete),
                WorkLocationCount = entity.WorkLocations.Count(x => !x.IsDelete),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy
            }, "Detail legal entity berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Legal Entity", Description = "Membuat legal entity", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LegalEntity", "Create")]
        public async Task<IActionResult> Create([FromBody] CreateLegalEntityRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();
            if (request.IsDefault) await UnsetDefaultAsync(null, now, actor);

            var entity = new MstLegalEntity
            {
                Id = Guid.NewGuid(),
                LegalEntityCode = await GenerateCodeAsync(),
                LegalEntityName = request.LegalEntityName.Trim(),
                ShortName = NormalizeText(request.ShortName),
                TaxIdentificationNumber = NormalizeText(request.TaxIdentificationNumber),
                BusinessRegistrationNumber = NormalizeText(request.BusinessRegistrationNumber),
                Email = NormalizeText(request.Email),
                PhoneNumber = NormalizeText(request.PhoneNumber),
                Address = NormalizeText(request.Address),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                IsDefault = request.IsDefault,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstLegalEntity>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "LegalEntity.Create",
                "Membuat data legal entity.",
                new { entity.Id, entity.LegalEntityCode, entity.LegalEntityName, entity.ShortName, entity.IsDefault, entity.IsActive, entity.CreateDateTime, entity.CreateBy }
            );

            return Ok(ApiResponse<object>.Ok(new { entity.Id, entity.LegalEntityCode, entity.LegalEntityName }, "Legal entity berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Legal Entity", Description = "Mengubah legal entity", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LegalEntity", "Update")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLegalEntityRequest request)
        {
            var entity = await _dbContext.Set<MstLegalEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Legal entity tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage!));

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();
            if (request.IsDefault && request.IsActive) await UnsetDefaultAsync(id, now, actor);

            entity.LegalEntityName = request.LegalEntityName.Trim();
            entity.ShortName = NormalizeText(request.ShortName);
            entity.TaxIdentificationNumber = NormalizeText(request.TaxIdentificationNumber);
            entity.BusinessRegistrationNumber = NormalizeText(request.BusinessRegistrationNumber);
            entity.Email = NormalizeText(request.Email);
            entity.PhoneNumber = NormalizeText(request.PhoneNumber);
            entity.Address = NormalizeText(request.Address);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.IsDefault = request.IsDefault && request.IsActive;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "LegalEntity.Update",
                "Mengubah data legal entity.",
                new { entity.Id, entity.LegalEntityCode, entity.LegalEntityName, entity.ShortName, entity.IsDefault, entity.IsActive, entity.UpdateDateTime, entity.UpdateBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Legal entity berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Legal Entity Status", Description = "Mengubah status legal entity", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LegalEntity", "Update")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateLegalEntityStatusRequest request)
        {
            var entity = await _dbContext.Set<MstLegalEntity>().FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Legal entity tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();
            if (request.IsDefault == true && request.IsActive) await UnsetDefaultAsync(id, now, actor);

            entity.IsActive = request.IsActive;
            if (request.IsDefault.HasValue) entity.IsDefault = request.IsDefault.Value && request.IsActive;
            else if (!request.IsActive) entity.IsDefault = false;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status legal entity berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Legal Entity", Description = "Menghapus legal entity", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LegalEntity", "Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var entity = await _dbContext.Set<MstLegalEntity>()
                .Include(x => x.HospitalSites)
                .Include(x => x.OrganizationUnits)
                .Include(x => x.CostCenters)
                .Include(x => x.WorkLocations)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Legal entity tidak ditemukan."));

            var isUsed = entity.HospitalSites.Any(x => !x.IsDelete) ||
                         entity.OrganizationUnits.Any(x => !x.IsDelete) ||
                         entity.CostCenters.Any(x => !x.IsDelete) ||
                         entity.WorkLocations.Any(x => !x.IsDelete);
            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Legal entity tidak dapat dihapus karena masih digunakan."));

            var now = DateTime.UtcNow;
            var actor = CurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();
            await _loggerService.InfoAsync(
                LogCategory,
                "LegalEntity.Delete",
                "Menghapus data legal entity.",
                new { entity.Id, entity.LegalEntityCode, entity.LegalEntityName, entity.DeleteDateTime, entity.DeleteBy }
            );

            return Ok(ApiResponse<object>.Ok(null, "Legal entity berhasil dihapus."));
        }

        private IQueryable<MstLegalEntity> BaseQuery() =>
            _dbContext.Set<MstLegalEntity>()
                .AsNoTracking()
                .Include(x => x.HospitalSites)
                .Include(x => x.OrganizationUnits)
                .Include(x => x.CostCenters)
                .Include(x => x.WorkLocations)
                .Where(x => !x.IsDelete);

        private static IQueryable<MstLegalEntity> ApplyFilter(
            IQueryable<MstLegalEntity> query,
            bool? isDefault,
            bool? isActive,
            string? search)
        {
            if (isDefault.HasValue) query = query.Where(x => x.IsDefault == isDefault.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.LegalEntityCode.ToLower().Contains(keyword) ||
                    x.LegalEntityName.ToLower().Contains(keyword) ||
                    (x.ShortName != null && x.ShortName.ToLower().Contains(keyword)) ||
                    (x.TaxIdentificationNumber != null && x.TaxIdentificationNumber.ToLower().Contains(keyword)) ||
                    (x.BusinessRegistrationNumber != null && x.BusinessRegistrationNumber.ToLower().Contains(keyword)));
            }
            return query;
        }

        private static IOrderedQueryable<MstLegalEntity> ApplySorting(IQueryable<MstLegalEntity> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "legalEntityName").Trim().ToLowerInvariant() switch
            {
                "legalentitycode" => desc ? query.OrderByDescending(x => x.LegalEntityCode) : query.OrderBy(x => x.LegalEntityCode),
                "shortname" => desc ? query.OrderByDescending(x => x.ShortName) : query.OrderBy(x => x.ShortName),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "isdefault" => desc ? query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.LegalEntityName) : query.OrderBy(x => x.IsDefault).ThenBy(x => x.LegalEntityName),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.LegalEntityName) : query.OrderBy(x => x.IsActive).ThenBy(x => x.LegalEntityName),
                _ => desc ? query.OrderByDescending(x => x.LegalEntityName) : query.OrderBy(x => x.LegalEntityName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateLegalEntityRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LegalEntityName)) return (false, "Nama legal entity wajib diisi.");
            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
                return (false, "EffectiveEndDate tidak boleh sebelum EffectiveStartDate.");

            var normalizedName = request.LegalEntityName.Trim().ToLower();
            var duplicateName = _dbContext.Set<MstLegalEntity>().AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityName.ToLower() == normalizedName);
            if (excludeId.HasValue) duplicateName = duplicateName.Where(x => x.Id != excludeId.Value);
            if (await duplicateName.AnyAsync()) return (false, "Nama legal entity sudah digunakan.");

            var taxNumber = NormalizeText(request.TaxIdentificationNumber);
            if (taxNumber != null)
            {
                var duplicateTax = _dbContext.Set<MstLegalEntity>().AsNoTracking()
                    .Where(x => !x.IsDelete && x.TaxIdentificationNumber == taxNumber);
                if (excludeId.HasValue) duplicateTax = duplicateTax.Where(x => x.Id != excludeId.Value);
                if (await duplicateTax.AnyAsync()) return (false, "Nomor identifikasi pajak sudah digunakan.");
            }

            return (true, null);
        }

        private async Task UnsetDefaultAsync(Guid? excludeId, DateTime now, Guid actor)
        {
            var query = _dbContext.Set<MstLegalEntity>().Where(x => !x.IsDelete && x.IsActive && x.IsDefault);
            if (excludeId.HasValue) query = query.Where(x => x.Id != excludeId.Value);
            var rows = await query.ToListAsync();
            foreach (var row in rows)
            {
                row.IsDefault = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actor;
            }
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstLegalEntity>().AsNoTracking()
                .Where(x => !x.IsDelete && x.LegalEntityCode.StartsWith(CodePrefix))
                .Select(x => x.LegalEntityCode)
                .ToListAsync();
            var used = codes.Select(x => x.Replace(CodePrefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return CodePrefix + next.ToString().PadLeft(CodeNumberLength, '0');
        }

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

        private static string? NormalizeText(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
