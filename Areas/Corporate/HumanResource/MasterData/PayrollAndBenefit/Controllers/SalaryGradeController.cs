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

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/salary-grades")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Salary Grade",
        AreaName = "Corporate",
        ControllerName = "SalaryGrade",
        Description = "Corporate human resource master data salary grade",
        SortOrder = 40)]
    [Tags("Corporate / Human Resource / Master Data / Salary Grade")]
    public class SalaryGradeController : ControllerBase
    {
        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "SGR-RSMMC-";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public SalaryGradeController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<SalaryGradeFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Salary Grade", Description = "Melihat metadata filter salary grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryGrade", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new SalaryGradeFilterMetadataResponse
            {
                DefaultFilter = new SalaryGradeDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                SortOptions = new List<SalaryGradeSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "salaryGradeCode", Label = "Kode salary grade" },
                    new() { Value = "salaryGradeName", Label = "Nama salary grade" },
                    new() { Value = "gradeLevel", Label = "Level grade" },
                    new() { Value = "minimumSalary", Label = "Gaji minimum" },
                    new() { Value = "maximumSalary", Label = "Gaji maksimum" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<SalaryGradeFilterMetadataResponse>.Ok(
                result,
                "Metadata filter salary grade berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SalaryGradeSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Salary Grade", Description = "Melihat ringkasan salary grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryGrade", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();

            var result = new SalaryGradeSummaryResponse
            {
                TotalSalaryGrade = await query.CountAsync(cancellationToken),
                ActiveSalaryGrade = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveSalaryGrade = await query.CountAsync(x => !x.IsActive, cancellationToken),
                LinkedEmployeeGrade = await query.CountAsync(x => x.EmployeeGradeId != null, cancellationToken),
                UsedBySalaryStructure = await query.CountAsync(
                    x => _dbContext.Set<MstSalaryStructure>().Any(
                        s => s.SalaryGradeId == x.Id && !s.IsDelete),
                    cancellationToken)
            };

            return Ok(ApiResponse<SalaryGradeSummaryResponse>.Ok(
                result,
                "Ringkasan salary grade berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<SalaryGradeResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Salary Grade", Description = "Melihat data salary grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryGrade", "Read")]
        public async Task<IActionResult> GetSalaryGrades(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? employeeGradeId,
            [FromQuery] string? currencyCode,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "gradeLevel",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyFilter(query, employeeGradeId, currencyCode, isActive, search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SalaryGradeResponse
                {
                    Id = x.Id,
                    EmployeeGradeId = x.EmployeeGradeId,
                    SalaryGradeCode = x.SalaryGradeCode,
                    SalaryGradeName = x.SalaryGradeName,
                    GradeLevel = x.GradeLevel,
                    CurrencyCode = x.CurrencyCode,
                    MinimumSalary = x.MinimumSalary,
                    MidpointSalary = x.MidpointSalary,
                    MaximumSalary = x.MaximumSalary,
                    AnnualIncrementPercentage = x.AnnualIncrementPercentage,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    SalaryStructureCount = _dbContext.Set<MstSalaryStructure>()
                        .Count(s => s.SalaryGradeId == x.Id && !s.IsDelete),
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

            var result = new PagedResult<SalaryGradeResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<PagedResult<SalaryGradeResponse>>.Ok(
                result,
                "Data salary grade berhasil diambil."));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<SalaryGradeOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Salary Grade", Description = "Melihat pilihan salary grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryGrade", "Read")]
        public async Task<IActionResult> GetSalaryGradeOptions(
            [FromQuery] Guid? employeeGradeId,
            [FromQuery] string? currencyCode,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(),
                employeeGradeId,
                currencyCode,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(x => x.GradeLevel)
                .ThenBy(x => x.SalaryGradeName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SalaryGradeOptionResponse
                {
                    Id = x.Id,
                    SalaryGradeCode = x.SalaryGradeCode,
                    SalaryGradeName = x.SalaryGradeName,
                    GradeLevel = x.GradeLevel,
                    CurrencyCode = x.CurrencyCode,
                    MinimumSalary = x.MinimumSalary,
                    MidpointSalary = x.MidpointSalary,
                    MaximumSalary = x.MaximumSalary
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<SalaryGradeOptionPagedResponse>.Ok(
                new SalaryGradeOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan salary grade berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SalaryGradeDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Salary Grade", Description = "Melihat detail salary grade", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryGrade", "Read")]
        public async Task<IActionResult> GetSalaryGradeById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new SalaryGradeDetailResponse
                {
                    Id = x.Id,
                    EmployeeGradeId = x.EmployeeGradeId,
                    SalaryGradeCode = x.SalaryGradeCode,
                    SalaryGradeName = x.SalaryGradeName,
                    GradeLevel = x.GradeLevel,
                    CurrencyCode = x.CurrencyCode,
                    MinimumSalary = x.MinimumSalary,
                    MidpointSalary = x.MidpointSalary,
                    MaximumSalary = x.MaximumSalary,
                    AnnualIncrementPercentage = x.AnnualIncrementPercentage,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    SalaryStructureCount = _dbContext.Set<MstSalaryStructure>()
                        .Count(s => s.SalaryGradeId == x.Id && !s.IsDelete),
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                    CreateByName = x.CreateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.CreateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault(),
                    UpdateDateTime = x.UpdateDateTime,
                    UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                    UpdateByName = x.UpdateBy == Guid.Empty
                        ? null
                        : _dbContext.Users
                            .Where(u => u.Id == x.UpdateBy)
                            .Select(u => u.DisplayName ?? u.UserName ?? u.Email ?? u.UserCode)
                            .FirstOrDefault()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Salary grade tidak ditemukan."));
            }

            return Ok(ApiResponse<SalaryGradeDetailResponse>.Ok(
                data,
                "Detail salary grade berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SalaryGradeCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Salary Grade", Description = "Membuat data salary grade", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SalaryGrade", "Create")]
        public async Task<IActionResult> CreateSalaryGrade(
            [FromBody] CreateSalaryGradeRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request, cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data salary grade tidak valid."));
            }

            var entity = new MstSalaryGrade
            {
                Id = Guid.NewGuid(),
                EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId),
                SalaryGradeCode = await GenerateCodeAsync(cancellationToken),
                SalaryGradeName = request.SalaryGradeName.Trim(),
                GradeLevel = request.GradeLevel,
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                MinimumSalary = request.MinimumSalary,
                MidpointSalary = request.MidpointSalary,
                MaximumSalary = request.MaximumSalary,
                AnnualIncrementPercentage = request.AnnualIncrementPercentage,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstSalaryGrade>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var result = new SalaryGradeCreateResponse
            {
                Id = entity.Id,
                SalaryGradeCode = entity.SalaryGradeCode,
                SalaryGradeName = entity.SalaryGradeName,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "SalaryGrade.CreateSalaryGrade",
                "Membuat data salary grade.",
                result);

            return Ok(ApiResponse<SalaryGradeCreateResponse>.Ok(
                result,
                "Salary grade berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Salary Grade", Description = "Mengubah data salary grade", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SalaryGrade", "Update")]
        public async Task<IActionResult> UpdateSalaryGrade(
            Guid id,
            [FromBody] UpdateSalaryGradeRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSalaryGrade>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary grade tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request, cancellationToken);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage ?? "Data salary grade tidak valid."));

            entity.EmployeeGradeId = NormalizeGuid(request.EmployeeGradeId);
            entity.SalaryGradeName = request.SalaryGradeName.Trim();
            entity.GradeLevel = request.GradeLevel;
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.MinimumSalary = request.MinimumSalary;
            entity.MidpointSalary = request.MidpointSalary;
            entity.MaximumSalary = request.MaximumSalary;
            entity.AnnualIncrementPercentage = request.AnnualIncrementPercentage;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Salary grade berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Salary Grade Status", Description = "Mengubah status salary grade", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("SalaryGrade", "Update")]
        public async Task<IActionResult> UpdateSalaryGradeStatus(
            Guid id,
            [FromBody] UpdateSalaryGradeStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSalaryGrade>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary grade tidak ditemukan."));

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status salary grade berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Salary Grade", Description = "Menghapus salary grade", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("SalaryGrade", "Delete")]
        public async Task<IActionResult> DeleteSalaryGrade(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSalaryGrade>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary grade tidak ditemukan."));

            var isUsed = await _dbContext.Set<MstSalaryStructure>()
                .AnyAsync(x => x.SalaryGradeId == id && !x.IsDelete, cancellationToken);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(400, "Salary grade tidak dapat dihapus karena sudah digunakan salary structure."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Salary grade berhasil dihapus."));
        }

        private IQueryable<MstSalaryGrade> BuildBaseQuery()
        {
            return _dbContext.Set<MstSalaryGrade>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstSalaryGrade> ApplyFilter(
            IQueryable<MstSalaryGrade> query,
            Guid? employeeGradeId,
            string? currencyCode,
            bool? isActive,
            string? search)
        {
            if (employeeGradeId.HasValue && employeeGradeId.Value != Guid.Empty)
                query = query.Where(x => x.EmployeeGradeId == employeeGradeId.Value);

            if (!string.IsNullOrWhiteSpace(currencyCode))
                query = query.Where(x => x.CurrencyCode == currencyCode.Trim().ToUpper());

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.SalaryGradeCode.ToLower().Contains(keyword) ||
                    x.SalaryGradeName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstSalaryGrade> ApplySorting(
            IQueryable<MstSalaryGrade> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "gradeLevel").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "salarygradecode" => desc ? query.OrderByDescending(x => x.SalaryGradeCode) : query.OrderBy(x => x.SalaryGradeCode),
                "salarygradename" => desc ? query.OrderByDescending(x => x.SalaryGradeName) : query.OrderBy(x => x.SalaryGradeName),
                "minimumsalary" => desc ? query.OrderByDescending(x => x.MinimumSalary) : query.OrderBy(x => x.MinimumSalary),
                "maximumsalary" => desc ? query.OrderByDescending(x => x.MaximumSalary) : query.OrderBy(x => x.MaximumSalary),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc
                    ? query.OrderByDescending(x => x.GradeLevel).ThenByDescending(x => x.SalaryGradeName)
                    : query.OrderBy(x => x.GradeLevel).ThenBy(x => x.SalaryGradeName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateSalaryGradeRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.SalaryGradeName))
                return (false, "Nama salary grade wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
                return (false, "Currency code harus terdiri dari tiga karakter.");

            if (request.MinimumSalary > request.MidpointSalary || request.MidpointSalary > request.MaximumSalary)
                return (false, "Rentang salary harus Minimum <= Midpoint <= Maximum.");

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var employeeGradeId = NormalizeGuid(request.EmployeeGradeId);

            if (employeeGradeId.HasValue)
            {
                var exists = await _dbContext.MstEmployeeGrades
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == employeeGradeId.Value &&
                        x.IsActive &&
                        !x.IsDelete,
                        cancellationToken);

                if (!exists)
                    return (false, "Employee grade tidak ditemukan atau tidak aktif.");
            }

            var normalizedName = request.SalaryGradeName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstSalaryGrade>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.SalaryGradeName.ToLower() == normalizedName &&
                    x.EmployeeGradeId == employeeGradeId);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Salary grade dengan nama dan employee grade tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstSalaryGrade>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.SalaryGradeCode.StartsWith(CodePrefix))
                .Select(x => x.SalaryGradeCode)
                .ToListAsync(cancellationToken);

            return GenerateNextCode(codes, CodePrefix, 5);
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }

        private static Guid? NormalizeGuid(Guid? value)
        {
            return !value.HasValue || value.Value == Guid.Empty ? null : value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            return (pageNumber < 1 ? 1 : pageNumber, pageSize < 1 ? 25 : Math.Min(pageSize, 100));
        }

        private static string GenerateNextCode(IEnumerable<string> codes, string prefix, int length)
        {
            var used = codes
                .Select(x => x.Replace(prefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .ToHashSet();

            var next = 1;
            while (used.Contains(next)) next++;
            return prefix + next.ToString().PadLeft(length, '0');
        }

        private static IQueryable<MstSalaryGrade> ApplyDateFilter(
            IQueryable<MstSalaryGrade> query,
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

        private static List<SalaryGradeCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<SalaryGradeCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
