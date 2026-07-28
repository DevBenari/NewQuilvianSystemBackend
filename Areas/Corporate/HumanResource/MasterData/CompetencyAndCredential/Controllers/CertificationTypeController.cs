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

using ResponseCertificationTypePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.DTOs.CertificationTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.CompetencyAndCredential.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/certification-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Certification Type",
        AreaName = "Corporate",
        ControllerName = "CertificationType",
        Description = "Corporate human resource master data certification type",
        SortOrder = 15)]
    [Tags("Corporate / Human Resource / Master Data / Certification Type")]
    public class CertificationTypeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "CRT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public CertificationTypeController(ApplicationDbContext dbContext, LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<CertificationTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Certification Type", Description = "Melihat metadata filter certification type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CertificationType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new CertificationTypeFilterMetadataResponse
            {
                DefaultFilter = new CertificationTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<CertificationTypeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "certificationTypeCode", Label = "Kode certification type" },
                    new() { Value = "certificationTypeName", Label = "Nama certification type" },
                    new() { Value = "professionName", Label = "Profession" },
                    new() { Value = "defaultValidityMonths", Label = "Masa berlaku default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(LogCategory, "CertificationType.GetFilterMetadata", "Mengambil metadata filter certification type.", result);
            return Ok(ApiResponse<CertificationTypeFilterMetadataResponse>.Ok(result, "Metadata filter certification type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CertificationTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Certification Type", Description = "Melihat ringkasan certification type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CertificationType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();
            var result = new CertificationTypeSummaryResponse
            {
                TotalCertificationType = await query.CountAsync(),
                ActiveCertificationType = await query.CountAsync(x => x.IsActive),
                InactiveCertificationType = await query.CountAsync(x => !x.IsActive),
                ExpiryRequiredType = await query.CountAsync(x => x.RequiresExpiryDate),
                RenewableType = await query.CountAsync(x => x.IsRenewable),
                DocumentRequiredType = await query.CountAsync(x => x.RequiresDocument),
                VerificationRequiredType = await query.CountAsync(x => x.RequiresVerification)
            };

            return Ok(ApiResponse<CertificationTypeSummaryResponse>.Ok(result, "Ringkasan certification type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseCertificationTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Certification Type", Description = "Melihat data certification type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CertificationType", "Read")]
        public async Task<IActionResult> GetCertificationTypes(
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
            [FromQuery] string? sortBy = "certificationTypeName",
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
                .Select(x => new CertificationTypeResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    CertificationTypeCode = x.CertificationTypeCode,
                    CertificationTypeName = x.CertificationTypeName,
                    IssuingAuthority = x.IssuingAuthority,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsRenewable = x.IsRenewable,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceCertificationCount = _dbContext.Set<WfpCertification>().Count(c => c.CertificationTypeId == x.Id && !c.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.CertificationTypeId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            return Ok(ApiResponse<ResponseCertificationTypePagedResult>.Ok(
                new ResponseCertificationTypePagedResult
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data certification type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<CertificationTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Certification Type", Description = "Melihat pilihan certification type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CertificationType", "Read")]
        public async Task<IActionResult> GetCertificationTypeOptions(
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
                .OrderBy(x => x.CertificationTypeName)
                .ThenBy(x => x.CertificationTypeCode)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CertificationTypeOptionResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    CertificationTypeCode = x.CertificationTypeCode,
                    CertificationTypeName = x.CertificationTypeName,
                    IssuingAuthority = x.IssuingAuthority,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification
                })
                .ToListAsync();

            return Ok(ApiResponse<CertificationTypeOptionPagedResponse>.Ok(
                new CertificationTypeOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan certification type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CertificationTypeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Certification Type", Description = "Melihat detail certification type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CertificationType", "Read")]
        public async Task<IActionResult> GetCertificationTypeById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new CertificationTypeDetailResponse
                {
                    Id = x.Id,
                    ProfessionId = x.ProfessionId,
                    ProfessionCode = x.Profession != null ? x.Profession.ProfessionCode : null,
                    ProfessionName = x.Profession != null ? x.Profession.ProfessionName : null,
                    CertificationTypeCode = x.CertificationTypeCode,
                    CertificationTypeName = x.CertificationTypeName,
                    IssuingAuthority = x.IssuingAuthority,
                    DefaultValidityMonths = x.DefaultValidityMonths,
                    RequiresExpiryDate = x.RequiresExpiryDate,
                    IsRenewable = x.IsRenewable,
                    RequiresDocument = x.RequiresDocument,
                    RequiresVerification = x.RequiresVerification,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    WorkforceCertificationCount = _dbContext.Set<WfpCertification>().Count(c => c.CertificationTypeId == x.Id && !c.IsDelete),
                    CredentialingRequirementCount = _dbContext.MstCredentialingRequirements.Count(r => r.CertificationTypeId == x.Id && !r.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.UpdateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Certification type tidak ditemukan."));

            return Ok(ApiResponse<CertificationTypeDetailResponse>.Ok(data, "Detail certification type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CertificationTypeCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Certification Type", Description = "Membuat data certification type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("CertificationType", "Create")]
        public async Task<IActionResult> CreateCertificationType([FromBody] CreateCertificationTypeRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data certification type tidak valid."));

            var entity = new MstCertificationType
            {
                Id = Guid.NewGuid(),
                ProfessionId = NormalizeGuid(request.ProfessionId),
                CertificationTypeCode = await GenerateCodeAsync(),
                CertificationTypeName = request.CertificationTypeName.Trim(),
                IssuingAuthority = NormalizeNullableString(request.IssuingAuthority),
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

            _dbContext.MstCertificationTypes.Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new CertificationTypeCreateResponse
            {
                Id = entity.Id,
                CertificationTypeCode = entity.CertificationTypeCode,
                CertificationTypeName = entity.CertificationTypeName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(LogCategory, "CertificationType.CreateCertificationType", "Membuat data certification type.", result);
            return Ok(ApiResponse<CertificationTypeCreateResponse>.Ok(result, "Certification type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Certification Type", Description = "Mengubah data certification type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CertificationType", "Update")]
        public async Task<IActionResult> UpdateCertificationType(Guid id, [FromBody] UpdateCertificationTypeRequest request)
        {
            var entity = await _dbContext.MstCertificationTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Certification type tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request);
            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, validation.ErrorMessage ?? "Data certification type tidak valid."));

            entity.ProfessionId = NormalizeGuid(request.ProfessionId);
            entity.CertificationTypeName = request.CertificationTypeName.Trim();
            entity.IssuingAuthority = NormalizeNullableString(request.IssuingAuthority);
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

            await _loggerService.InfoAsync(LogCategory, "CertificationType.UpdateCertificationType", "Mengubah data certification type.", new { entity.Id, entity.CertificationTypeCode, entity.CertificationTypeName, entity.IsActive });
            return Ok(ApiResponse<object>.Ok(null, "Certification type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Certification Type Status", Description = "Mengubah status certification type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("CertificationType", "Update")]
        public async Task<IActionResult> UpdateCertificationTypeStatus(Guid id, [FromBody] UpdateCertificationTypeStatusRequest request)
        {
            var entity = await _dbContext.MstCertificationTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Certification type tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();
            await _dbContext.SaveChangesAsync();
            return Ok(ApiResponse<object>.Ok(null, "Status certification type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Delete Certification Type", Description = "Menghapus certification type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("CertificationType", "Delete")]
        public async Task<IActionResult> DeleteCertificationType(Guid id)
        {
            var entity = await _dbContext.MstCertificationTypes.FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);
            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Certification type tidak ditemukan."));

            var isUsed =
                await _dbContext.Set<WfpCertification>().AsNoTracking().AnyAsync(x => x.CertificationTypeId == id && !x.IsDelete) ||
                await _dbContext.MstCredentialingRequirements.AsNoTracking().AnyAsync(x => x.CertificationTypeId == id && !x.IsDelete);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Certification type tidak dapat dihapus karena sudah digunakan oleh workforce certification atau credentialing requirement."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(LogCategory, "CertificationType.DeleteCertificationType", "Menghapus data certification type.", new { entity.Id, entity.CertificationTypeCode, entity.CertificationTypeName, entity.DeleteDateTime });
            return Ok(ApiResponse<object>.Ok(null, "Certification type berhasil dihapus."));
        }

        private IQueryable<MstCertificationType> BuildBaseQuery() =>
            _dbContext.MstCertificationTypes.AsNoTracking().Where(x => !x.IsDelete);

        private static IQueryable<MstCertificationType> ApplyDateFilter(IQueryable<MstCertificationType> query, DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static IQueryable<MstCertificationType> ApplyStandardFilter(
            IQueryable<MstCertificationType> query,
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
                    x.CertificationTypeCode.ToLower().Contains(keyword) ||
                    x.CertificationTypeName.ToLower().Contains(keyword) ||
                    x.IssuingAuthority != null && x.IssuingAuthority.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword) ||
                    x.Profession != null && x.Profession.ProfessionName.ToLower().Contains(keyword));
            }

            return query;
        }

        private static IOrderedQueryable<MstCertificationType> ApplySorting(IQueryable<MstCertificationType> query, string? sortBy, string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "certificationTypeName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "certificationtypecode" => desc ? query.OrderByDescending(x => x.CertificationTypeCode) : query.OrderBy(x => x.CertificationTypeCode),
                "professionname" => desc ? query.OrderByDescending(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty) : query.OrderBy(x => x.Profession != null ? x.Profession.ProfessionName : string.Empty),
                "defaultvaliditymonths" => desc ? query.OrderByDescending(x => x.DefaultValidityMonths) : query.OrderBy(x => x.DefaultValidityMonths),
                "workforcecertificationcount" => desc ? query.OrderByDescending(x => x.Id) : query.OrderBy(x => x.Id),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.CertificationTypeName).ThenByDescending(x => x.CertificationTypeCode) : query.OrderBy(x => x.CertificationTypeName).ThenBy(x => x.CertificationTypeCode)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(Guid? excludeId, CreateCertificationTypeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CertificationTypeName)) return (false, "Nama certification type wajib diisi.");
            if (request.DefaultValidityMonths.HasValue && request.DefaultValidityMonths.Value <= 0) return (false, "Default validity months harus lebih besar dari 0.");
            if (request.IsRenewable && !request.RequiresExpiryDate) return (false, "Certification type renewable harus menggunakan tanggal kedaluwarsa.");

            var professionId = NormalizeGuid(request.ProfessionId);
            if (professionId.HasValue)
            {
                var exists = await _dbContext.MstProfessions.AsNoTracking().AnyAsync(x => x.Id == professionId.Value && x.IsActive && !x.IsDelete);
                if (!exists) return (false, "Profession tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.CertificationTypeName.Trim().ToLower();
            var duplicateQuery = _dbContext.MstCertificationTypes.AsNoTracking().Where(x =>
                !x.IsDelete &&
                x.CertificationTypeName.ToLower() == normalizedName &&
                x.ProfessionId == professionId);
            if (excludeId.HasValue) duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);
            if (await duplicateQuery.AnyAsync()) return (false, "Certification type dengan nama dan profession tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.MstCertificationTypes.AsNoTracking().Where(x => !x.IsDelete && x.CertificationTypeCode.StartsWith(CodePrefix)).Select(x => x.CertificationTypeCode).ToListAsync();
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

        private static List<CertificationTypeCustomPeriodOptionResponse> BuildPeriodOptions() =>
            new()
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
    }
}
