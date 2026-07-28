using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.CredentialingManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseLicenseTypePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.LicenseTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/license-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "License Type",
        AreaName = "Corporate",
        ControllerName = "LicenseType",
        Description = "Corporate human resource master data license type",
        SortOrder = 16)]
    [Tags("Corporate / Human Resource / Master Data / License Type")]
    public class LicenseTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "LCT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public LicenseTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<LicenseTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read License Type", Description = "Melihat metadata filter license type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LicenseType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new LicenseTypeFilterMetadataResponse
            {
                DefaultFilter = new LicenseTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<LicenseTypeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "licenseTypeCode", Label = "Kode license type" },
                    new() { Value = "licenseTypeName", Label = "Nama license type" },
                    new() { Value = "professionName", Label = "Profession" },
                    new() { Value = "regulatoryBody", Label = "Regulatory body" },
                    new() { Value = "defaultValidityMonths", Label = "Masa berlaku default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "LicenseType.GetFilterMetadata", "Mengambil metadata filter license type.", result);
            return Ok(ApiResponse<LicenseTypeFilterMetadataResponse>.Ok(result, "Metadata filter license type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<LicenseTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read License Type", Description = "Melihat ringkasan license type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LicenseType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var result = new LicenseTypeSummaryResponse
            {
                TotalLicenseType = await query.CountAsync(),
                ActiveLicenseType = await query.CountAsync(x => x.IsActive),
                InactiveLicenseType = await query.CountAsync(x => !x.IsActive),
                ExpiryRequiredType = await query.CountAsync(x => x.RequiresExpiryDate),
                RenewableType = await query.CountAsync(x => x.IsRenewable),
                DocumentRequiredType = await query.CountAsync(x => x.RequiresDocument),
                VerificationRequiredType = await query.CountAsync(x => x.RequiresVerification)
            };

            return Ok(ApiResponse<LicenseTypeSummaryResponse>.Ok(result, "Ringkasan license type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseLicenseTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read License Type", Description = "Melihat data license type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LicenseType", "Read")]
        public async Task<IActionResult> GetLicenseTypes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? professionId,
            [FromQuery] bool? requiresExpiryDate,
            [FromQuery] bool? isRenewable,
            [FromQuery] bool? requiresDocument,
            [FromQuery] bool? requiresVerification,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "licenseTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, professionId, requiresExpiryDate, isRenewable, requiresDocument, requiresVerification, isActive, search);
            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LicenseTypeResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    LicenseTypeCode = x.LicenseTypeCode,
                    LicenseTypeName = x.LicenseTypeName,
                    IssuingAuthority = x.IssuingAuthority,
                    RegulatoryBody = x.RegulatoryBody,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsRenewable = x.IsRenewable,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceLicenseCount = _dbContext.Set<WfpCredentialLicense>().Count(c => c.LicenseTypeId == x.Id && !c.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.LicenseTypeId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseLicenseTypePagedResult>.Ok(
                new ResponseLicenseTypePagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data license type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<LicenseTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read License Type", Description = "Melihat pilihan license type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LicenseType", "Read")]
        public async Task<IActionResult> GetLicenseTypeOptions(
            [FromQuery] Guid? professionId,
            [FromQuery] bool? requiresExpiryDate,
            [FromQuery] bool? requiresDocument,
            [FromQuery] bool? requiresVerification,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyStandardFilter(BuildBaseQuery(), professionId, requiresExpiryDate, null, requiresDocument, requiresVerification, onlyActive ? true : null, search);
            var totalData = await query.CountAsync();
            var items = await query
                .OrderBy(x => x.LicenseTypeName)
                .ThenBy(x => x.LicenseTypeCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LicenseTypeOptionResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    LicenseTypeCode = x.LicenseTypeCode,
                    LicenseTypeName = x.LicenseTypeName,
                    IssuingAuthority = x.IssuingAuthority,
                    RegulatoryBody = x.RegulatoryBody,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification
                })
                .ToListAsync();

            return Ok(ApiResponse<LicenseTypeOptionPagedResponse>.Ok(
                new LicenseTypeOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan license type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<LicenseTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read License Type", Description = "Melihat detail license type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LicenseType", "Read")]
        public async Task<IActionResult> GetLicenseTypeById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new LicenseTypeDetailResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    LicenseTypeCode = x.LicenseTypeCode,
                    LicenseTypeName = x.LicenseTypeName,
                    IssuingAuthority = x.IssuingAuthority,
                    RegulatoryBody = x.RegulatoryBody,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsRenewable = x.IsRenewable,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceLicenseCount = _dbContext.Set<WfpCredentialLicense>().Count(c => c.LicenseTypeId == x.Id && !c.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.LicenseTypeId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "License type tidak ditemukan."));

            return Ok(ApiResponse<LicenseTypeDetailResponse>.Ok(data, "Detail license type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<LicenseTypeCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create License Type", Description = "Membuat data license type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("LicenseType", "Create")]
        public async Task<IActionResult> CreateLicenseType([FromBody] CreateLicenseTypeRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data license type tidak valid."));

            var entity = new MstLicenseType
            {
                Id = Guid.NewGuid(),
                ProfessionId = NormalizeGuid(request.ProfessionId),
                LicenseTypeCode = await GenerateCodeAsync(),
                LicenseTypeName = request.LicenseTypeName.Trim(),
                IssuingAuthority = NormalizeNullableString(request.IssuingAuthority),
                RegulatoryBody = NormalizeNullableString(request.RegulatoryBody),
                DefaultValidityMonths = request.DefaultValidityMonths,
                RequiresExpiryDate = request.RequiresExpiryDate,
                IsRenewable = request.IsRenewable,
                RequiresDocument = request.RequiresDocument,
                RequiresVerification = request.RequiresVerification,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.MstLicenseTypes.Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new LicenseTypeCreateResponse
            {
                Id = entity.Id,
                LicenseTypeCode = entity.LicenseTypeCode,
                LicenseTypeName = entity.LicenseTypeName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "LicenseType.CreateLicenseType", "Membuat data license type.", result);
            return Ok(ApiResponse<LicenseTypeCreateResponse>.Ok(result, "License type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update License Type", Description = "Mengubah data license type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("LicenseType", "Update")]
        public async Task<IActionResult> UpdateLicenseType(Guid id, [FromBody] UpdateLicenseTypeRequest request)
        {
            var entity = await _dbContext.MstLicenseTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "License type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data license type tidak valid."));

            entity.ProfessionId = NormalizeGuid(request.ProfessionId);
            entity.LicenseTypeName = request.LicenseTypeName.Trim();
            entity.IssuingAuthority = NormalizeNullableString(request.IssuingAuthority);
            entity.RegulatoryBody = NormalizeNullableString(request.RegulatoryBody);
            entity.DefaultValidityMonths = request.DefaultValidityMonths;
            entity.RequiresExpiryDate = request.RequiresExpiryDate;
            entity.IsRenewable = request.IsRenewable;
            entity.RequiresDocument = request.RequiresDocument;
            entity.RequiresVerification = request.RequiresVerification;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "LicenseType.UpdateLicenseType", "Mengubah data license type.", new { entity.Id, entity.LicenseTypeCode, entity.LicenseTypeName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "License type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update License Type Status", Description = "Mengubah status license type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("LicenseType", "Update")]
        public async Task<IActionResult> UpdateLicenseTypeStatus(Guid id, [FromBody] UpdateLicenseTypeStatusRequest request)
        {
            var entity = await _dbContext.MstLicenseTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "License type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status license type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete License Type", Description = "Menghapus license type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("LicenseType", "Delete")]
        public async Task<IActionResult> DeleteLicenseType(Guid id)
        {
            var entity = await _dbContext.MstLicenseTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "License type tidak ditemukan."));

            var isUsed =
                await _dbContext.Set<WfpCredentialLicense>().AsNoTracking().AnyAsync(x => x.LicenseTypeId == id && !x.IsDelete) ||
                await _dbContext.MstCredentialingRequirements.AsNoTracking().AnyAsync(x => x.LicenseTypeId == id && !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "License type tidak dapat dihapus karena sudah digunakan oleh workforce credential license atau credentialing requirement."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "LicenseType.DeleteLicenseType", "Menghapus data license type.", new { entity.Id, entity.LicenseTypeCode, entity.LicenseTypeName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "License type berhasil dihapus."));
        }

        private IQueryable<MstLicenseType> BuildBaseQuery() =>
            _dbContext.MstLicenseTypes.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstLicenseType> ApplyDateFilter(IQueryable<MstLicenseType> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstLicenseType> ApplyStandardFilter(
            IQueryable<MstLicenseType> query,
            Guid? professionId,
            bool? requiresExpiryDate,
            bool? isRenewable,
            bool? requiresDocument,
            bool? requiresVerification,
            bool? isActive,
            string? search)
        {
            if (professionId.HasValue && professionId.Value != Guid.Empty) query = query.Where(x => x.ProfessionId == professionId.Value);
            if (requiresExpiryDate.HasValue) query = query.Where(x => x.RequiresExpiryDate == requiresExpiryDate.Value);
            if (isRenewable.HasValue) query = query.Where(x => x.IsRenewable == isRenewable.Value);
            if (requiresDocument.HasValue) query = query.Where(x => x.RequiresDocument == requiresDocument.Value);
            if (requiresVerification.HasValue) query = query.Where(x => x.RequiresVerification == requiresVerification.Value);
            if (isActive.HasValue) query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.LicenseTypeCode.ToLower().Contains(keyword) ||
                    x.LicenseTypeName.ToLower().Contains(keyword) ||
                    x.IssuingAuthority != null && x.IssuingAuthority.ToLower().Contains(keyword) ||
                    x.RegulatoryBody != null && x.RegulatoryBody.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.Profession != null && x.Profession.ProfessionName.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstLicenseType> ApplySorting(IQueryable<MstLicenseType> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "licenseTypeName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "licensetypecode" => desc ? query.OrderByDescending(x => x.LicenseTypeCode) : query.OrderBy(x => x.LicenseTypeCode),
                "professionname" => desc ? query.OrderByDescending(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty) : query.OrderBy(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty),
                "regulatorybody" => desc ? query.OrderByDescending(x => x.RegulatoryBody) : query.OrderBy(x => x.RegulatoryBody),
                "defaultvaliditymonths" => desc ? query.OrderByDescending(x => x.DefaultValidityMonths) : query.OrderBy(x => x.DefaultValidityMonths),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.LicenseTypeName).ThenByDescending(x => x.LicenseTypeCode) : query.OrderBy(x => x.LicenseTypeName).ThenBy(x => x.LicenseTypeCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateLicenseTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.LicenseTypeName)) return (false, "Nama license type wajib diisi.");
            if (request.DefaultValidityMonths.HasValue && request.DefaultValidityMonths.Value <= 0) return (false, "Default validity months harus lebih besar dari 0.");
            if (request.IsRenewable && !request.RequiresExpiryDate) return (false, "License type renewable harus menggunakan tanggal kedaluwarsa.");

            var professionId = NormalizeGuid(request.ProfessionId);
            if (professionId.HasValue)
            {
                var exists = await _dbContext.MstProfessions.AsNoTracking().AnyAsync(x => x.Id == professionId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Profession tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.LicenseTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.MstLicenseTypes.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.LicenseTypeName.ToLower() == normalizedName &&
                x.ProfessionId == professionId);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "License type dengan nama dan profession tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.MstLicenseTypes.AsNoTracking().Where(x => !x.IsDelete && x.LicenseTypeCode.StartsWith(CodePrefix)).Select(x => x.LicenseTypeCode).ToListAsync();
            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value) => !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        private static string? NormalizeNullableString(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize) => (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));

        private static string GenerateNextCode(IEnumerable<string> codes, string prefix, int length)
        {
            var used = codes.Select(x => x.Replace(prefix, string.Empty)).Where(x => int.TryParse(x, out _)).Select(int.Parse).Where(x => x > 0).ToHashSet();
            var next = 1;
            while (used.Contains(next)) next++;
            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
                return (startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null, endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null);

            var today = DateTime.UtcNow.Date;
            return customPeriod?.Trim().ToLowerInvariant() switch
            {
                "today" => (today, today.AddDays(1)),
                "last7days" => (today.AddDays(-6), today.AddDays(1)),
                "thismonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                "lastmonth" => (new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-1), new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc)),
                _ => (null, null)
            };
        }

        private static List<LicenseTypeCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
    }
}
