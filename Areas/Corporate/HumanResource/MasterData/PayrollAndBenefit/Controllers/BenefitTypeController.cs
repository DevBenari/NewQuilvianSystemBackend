using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using BenefitTypePagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs.BenefitTypeResponse>;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/benefit-types")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Benefit Type",
        AreaName = "Corporate",
        ControllerName = "BenefitType",
        Description = "Corporate human resource master data benefit type",
        SortOrder = 44)]
    [Tags("Corporate / Human Resource / Master Data / Benefit Type")]
    public class BenefitTypeController : ControllerBase
    {

        private static readonly HashSet<string> AllowedBenefitCategories =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "HealthInsurance", "LifeInsurance", "Retirement", "Medical",
                "Wellness", "Meal", "Transport", "Communication", "Education", "Other"
            };

        private static readonly HashSet<string> AllowedFundingTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Employer", "Employee", "Shared"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "BFT-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public BenefitTypeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<BenefitTypeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Type", Description = "Melihat metadata filter benefit type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitType", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new BenefitTypeFilterMetadataResponse
            {
                DefaultFilter = new BenefitTypeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<BenefitTypeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "benefitTypeCode", Label = "Kode" },
                    new() { Value = "benefitTypeName", Label = "Nama" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "BenefitType.GetFilterMetadata",
                "Mengambil metadata filter benefit type.",
                result);

            return Ok(ApiResponse<BenefitTypeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter benefit type berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<BenefitTypeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Type", Description = "Melihat ringkasan benefit type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitType", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new BenefitTypeSummaryResponse
            {
                TotalBenefitType = await query.CountAsync(),
                ActiveBenefitType = await query.CountAsync(x => x.IsActive),
                InactiveBenefitType = await query.CountAsync(x => !x.IsActive),
                EnrollmentRequiredType = await query.CountAsync(x => x.RequiresEnrollment),
                DependentAllowedType = await query.CountAsync(x => x.AllowsDependents),
                ClaimBasedType = await query.CountAsync(x => x.IsClaimBased),
                EvidenceRequiredType = await query.CountAsync(x => x.RequiresEvidence)
            };

            return Ok(ApiResponse<BenefitTypeSummaryResponse>.Ok(
                result,
                "Ringkasan benefit type berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<BenefitTypePagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Type", Description = "Melihat data benefit type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitType", "Read")]
        public async Task<IActionResult> GetBenefitTypes(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "benefitTypeName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.BenefitTypeCode.ToLower().Contains(keyword) ||
                    x.BenefitTypeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            var totalData = await query.CountAsync();

            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "isactive" => desc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.BenefitTypeName)
                    : query.OrderBy(x => x.BenefitTypeName)
            };

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new BenefitTypeResponse
                {
                    Id = x.Id,
                    BenefitTypeCode = x.BenefitTypeCode,
                    BenefitTypeName = x.BenefitTypeName,
                    BenefitCategory = x.BenefitCategory,
                    FundingType = x.FundingType,
                    IsTaxable = x.IsTaxable,
                    RequiresEnrollment = x.RequiresEnrollment,
                    AllowsDependents = x.AllowsDependents,
                    MaximumDependents = x.MaximumDependents,
                    IsClaimBased = x.IsClaimBased,
                    RequiresEvidence = x.RequiresEvidence,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    BenefitPlanCount = x.BenefitPlans.Count(p => !p.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault()
                })
                .ToListAsync();

            var result = new BenefitTypePagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<BenefitTypePagedResult>.Ok(
                result,
                "Data benefit type berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<BenefitTypeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Type", Description = "Melihat pilihan benefit type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitType", "Read")]
        public async Task<IActionResult> GetBenefitTypeOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.BenefitTypeCode.ToLower().Contains(keyword) ||
                    x.BenefitTypeName.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var rows = await query
                .OrderBy(x => x.BenefitTypeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = rows
                .Select(x =>
                {
                    var response = new BenefitTypeOptionResponse();
                    response.Id = x.Id;
                    return response;
                })
                .ToList();

            return Ok(ApiResponse<BenefitTypeOptionPagedResponse>.Ok(
                new BenefitTypeOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan benefit type berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<BenefitTypeDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Benefit Type", Description = "Melihat detail benefit type", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("BenefitType", "Read")]
        public async Task<IActionResult> GetBenefitTypeById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new BenefitTypeDetailResponse
                {
                    Id = x.Id,
                    BenefitTypeCode = x.BenefitTypeCode,
                    BenefitTypeName = x.BenefitTypeName,
                    BenefitCategory = x.BenefitCategory,
                    FundingType = x.FundingType,
                    IsTaxable = x.IsTaxable,
                    RequiresEnrollment = x.RequiresEnrollment,
                    AllowsDependents = x.AllowsDependents,
                    MaximumDependents = x.MaximumDependents,
                    IsClaimBased = x.IsClaimBased,
                    RequiresEvidence = x.RequiresEvidence,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    BenefitPlanCount = x.BenefitPlans.Count(p => !p.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty ? null : _dbContext.Users.Where(u => u.Id == x.CreateBy).Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode).FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Type tidak ditemukan."));
            }

            return Ok(ApiResponse<BenefitTypeDetailResponse>.Ok(
                data,
                "Detail benefit type berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<BenefitTypeCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Benefit Type", Description = "Membuat data benefit type", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("BenefitType", "Create")]
        public async Task<IActionResult> CreateBenefitType(
            [FromBody] CreateBenefitTypeRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data benefit type tidak valid."));
            }

            var entity = new MstBenefitType
            {
                Id = Guid.NewGuid(),
                BenefitTypeCode = await GenerateCodeAsync(),
                BenefitTypeName = request.BenefitTypeName.Trim(),
                BenefitCategory = NormalizeBenefitCategory(request.BenefitCategory),
                FundingType = NormalizeFundingType(request.FundingType),
                IsTaxable = request.IsTaxable,
                RequiresEnrollment = request.RequiresEnrollment,
                AllowsDependents = request.AllowsDependents,
                MaximumDependents = request.AllowsDependents ? request.MaximumDependents : 0,
                IsClaimBased = request.IsClaimBased,
                RequiresEvidence = request.RequiresEvidence,
                Description = NormalizeNullableString(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstBenefitType>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new BenefitTypeCreateResponse
            {
                Id = entity.Id,
                BenefitTypeCode = entity.BenefitTypeCode,
                BenefitTypeName = entity.BenefitTypeName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<BenefitTypeCreateResponse>.Ok(
                result,
                "Benefit Type berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Benefit Type", Description = "Mengubah data benefit type", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("BenefitType", "Update")]
        public async Task<IActionResult> UpdateBenefitType(
            Guid id,
            [FromBody] UpdateBenefitTypeRequest request)
        {
            var entity = await _dbContext.Set<MstBenefitType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Type tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data benefit type tidak valid."));
            }

            entity.BenefitTypeName = request.BenefitTypeName.Trim();
            entity.BenefitCategory = NormalizeBenefitCategory(request.BenefitCategory);
            entity.FundingType = NormalizeFundingType(request.FundingType);
            entity.IsTaxable = request.IsTaxable;
            entity.RequiresEnrollment = request.RequiresEnrollment;
            entity.AllowsDependents = request.AllowsDependents;
            entity.MaximumDependents = request.AllowsDependents ? request.MaximumDependents : 0;
            entity.IsClaimBased = request.IsClaimBased;
            entity.RequiresEvidence = request.RequiresEvidence;
            entity.Description = NormalizeNullableString(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Benefit Type berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Benefit Type Status", Description = "Mengubah status benefit type", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("BenefitType", "Update")]
        public async Task<IActionResult> UpdateBenefitTypeStatus(
            Guid id,
            [FromBody] UpdateBenefitTypeStatusRequest request)
        {
            var entity = await _dbContext.Set<MstBenefitType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Type tidak ditemukan."));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status benefit type berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Benefit Type", Description = "Menghapus benefit type", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("BenefitType", "Delete")]
        public async Task<IActionResult> DeleteBenefitType(Guid id)
        {
            var entity = await _dbContext.Set<MstBenefitType>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Benefit Type tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Benefit Type berhasil dihapus."));
        }

        private IQueryable<MstBenefitType> BuildBaseQuery()
        {
            return _dbContext.Set<MstBenefitType>()
                .AsNoTracking()
                .Include(x => x.BenefitPlans)
                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateBenefitTypeRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.BenefitTypeName))
                return (false, "Nama benefit type wajib diisi.");

            if (!AllowedBenefitCategories.Contains(request.BenefitCategory.Trim()))
                return (false, "Benefit category tidak valid.");

            if (!AllowedFundingTypes.Contains(request.FundingType.Trim()))
                return (false, "Funding type tidak valid.");

            if (!request.AllowsDependents && request.MaximumDependents > 0)
                return (false, "Maximum dependents hanya boleh diisi jika AllowsDependents aktif.");

            if (request.RequiresEvidence && !request.IsClaimBased)
                return (false, "Benefit yang membutuhkan evidence harus bersifat claim based.");

            var normalizedName = request.BenefitTypeName.Trim().ToLower();

            var duplicateQuery = _dbContext.Set<MstBenefitType>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BenefitTypeName.ToLower() == normalizedName &&
                    x.BenefitCategory == request.BenefitCategory.Trim());

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync())
                return (false, "Benefit type dengan nama dan kategori tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync()
        {
            var codes = await _dbContext.Set<MstBenefitType>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.BenefitTypeCode.StartsWith(CodePrefix))
                .Select(x => x.BenefitTypeCode)
                .ToListAsync();

            return GenerateNextCode(codes, CodePrefix, CodeNumberLength);
        }

        private Guid GetCurrentUserId()
        {
            var value =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(value, out var id)
                ? id
                : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty
                ? null
                : value.Value;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }


        private static string NormalizeBenefitCategory(string value)
        {
            return AllowedBenefitCategories.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string NormalizeFundingType(string value)
        {
            return AllowedFundingTypes.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            return (
                pageNumber < 1 ? 1 : pageNumber,
                pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static string GenerateNextCode(
            IEnumerable<string> codes,
            string prefix,
            int length)
        {
            var used = codes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var next = 1;

            while (used.Contains(next))
                next++;

            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static IQueryable<MstBenefitType> ApplyDateFilter(
            IQueryable<MstBenefitType> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var range = ResolveDateRange(startDate, endDate, customPeriod);
            if (range.Start.HasValue) query = query.Where(x => x.CreateDateTime >= range.Start.Value);
            if (range.EndExclusive.HasValue) query = query.Where(x => x.CreateDateTime < range.EndExclusive.Value);
            return query;
        }

        private static (DateTime? Start, DateTime? EndExclusive) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue || endDate.HasValue)
            {
                return (
                    startDate.HasValue ? DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc) : null,
                    endDate.HasValue ? DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc) : null);
            }

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

        private static List<BenefitTypeCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<BenefitTypeCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
