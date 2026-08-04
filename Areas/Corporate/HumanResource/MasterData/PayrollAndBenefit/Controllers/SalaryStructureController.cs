using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;
using System.Text.Json;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/master-data/salary-structures")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_MASTER_DATA",
        moduleName: "Human Resource Master Data",
        displayName: "Salary Structure",
        AreaName = "Corporate",
        ControllerName = "SalaryStructure",
        Description = "Corporate human resource master data salary structure",
        SortOrder = 41)]
    [Tags("Corporate / Human Resource / Master Data / Salary Structure")]
    public class SalaryStructureController : ControllerBase
    {
        private static readonly HashSet<string> AllowedPaymentFrequencies =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "Monthly",
                "BiWeekly",
                "Weekly",
                "Daily"
            };

        private const string LogCategory = "Corporate.HumanResource.MasterData";
        private const string CodePrefix = "SST-RSMMC-";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public SalaryStructureController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<SalaryStructureFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Salary Structure", Description = "Melihat metadata filter salary structure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryStructure", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new SalaryStructureFilterMetadataResponse
            {
                DefaultFilter = new SalaryStructureDefaultFilterResponse(),
                CustomPeriods = BuildPeriodOptions(),
                PaymentFrequencyOptions = AllowedPaymentFrequencies
                    .OrderBy(x => x)
                    .Select(x => new SalaryStructureStringOptionResponse
                    {
                        Value = x,
                        Label = BuildPaymentFrequencyLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<SalaryStructureSortOptionResponse>
                {
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "salaryStructureCode", Label = "Kode salary structure" },
                    new() { Value = "salaryStructureName", Label = "Nama salary structure" },
                    new() { Value = "salaryGradeName", Label = "Salary grade" },
                    new() { Value = "defaultBaseSalary", Label = "Gaji dasar default" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<SalaryStructureFilterMetadataResponse>.Ok(
                result,
                "Metadata filter salary structure berhasil diambil."));
        }

        [HttpGet("summary")]
        [AccessAction("Read", "Read Salary Structure", Description = "Melihat ringkasan salary structure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryStructure", "Read")]
        public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
        {
            var query = BuildBaseQuery();

            var result = new SalaryStructureSummaryResponse
            {
                TotalSalaryStructure = await query.CountAsync(cancellationToken),
                ActiveSalaryStructure = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactiveSalaryStructure = await query.CountAsync(x => !x.IsActive, cancellationToken),
                DefaultSalaryStructure = await query.CountAsync(x => x.IsDefault, cancellationToken),
                ProratedSalaryStructure = await query.CountAsync(x => x.IsProrated, cancellationToken),
                OvertimeIncludedStructure = await query.CountAsync(x => x.IncludeOvertime, cancellationToken)
            };

            return Ok(ApiResponse<SalaryStructureSummaryResponse>.Ok(
                result,
                "Ringkasan salary structure berhasil diambil."));
        }

        [HttpGet]
        [AccessAction("Read", "Read Salary Structure", Description = "Melihat data salary structure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryStructure", "Read")]
        public async Task<IActionResult> GetSalaryStructures(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? salaryGradeId,
            [FromQuery] string? paymentFrequency,
            [FromQuery] bool? isDefault,
            [FromQuery] bool? isProrated,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "salaryStructureName",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyDateFilter(BuildBaseQuery(), startDate, endDate, customPeriod);
            query = ApplyFilter(query, salaryGradeId, paymentFrequency, isDefault, isProrated, isActive, search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var items = rows.Select(MapResponse).ToList();

            return Ok(ApiResponse<PagedResult<SalaryStructureResponse>>.Ok(
                new PagedResult<SalaryStructureResponse>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data salary structure berhasil diambil."));
        }

        [HttpGet("options")]
        [AccessAction("Read", "Read Salary Structure", Description = "Melihat pilihan salary structure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryStructure", "Read")]
        public async Task<IActionResult> GetSalaryStructureOptions(
            [FromQuery] Guid? salaryGradeId,
            [FromQuery] string? paymentFrequency,
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
                salaryGradeId,
                paymentFrequency,
                null,
                null,
                onlyActive ? true : null,
                search);

            var totalData = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.IsDefault)
                .ThenBy(x => x.SalaryStructureName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SalaryStructureOptionResponse
                {
                    Id = x.Id,
                    SalaryGradeId = x.SalaryGradeId,
                    SalaryGradeName = x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeName : null,
                    SalaryStructureCode = x.SalaryStructureCode,
                    SalaryStructureName = x.SalaryStructureName,
                    CurrencyCode = x.CurrencyCode,
                    PaymentFrequency = x.PaymentFrequency,
                    DefaultBaseSalary = x.DefaultBaseSalary,
                    IsDefault = x.IsDefault
                })
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<SalaryStructureOptionPagedResponse>.Ok(
                new SalaryStructureOptionPagedResponse
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalData = totalData,
                    TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                    Items = items
                },
                "Data pilihan salary structure berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [AccessAction("Read", "Read Salary Structure", Description = "Melihat detail salary structure", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("SalaryStructure", "Read")]
        public async Task<IActionResult> GetSalaryStructureById(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary structure tidak ditemukan."));

            var result = new SalaryStructureDetailResponse
            {
                Id = entity.Id,
                SalaryGradeId = entity.SalaryGradeId,
                SalaryGradeCode = entity.SalaryGrade?.SalaryGradeCode,
                SalaryGradeName = entity.SalaryGrade?.SalaryGradeName,
                LegalEntityId = entity.LegalEntityId,
                HospitalSiteId = entity.HospitalSiteId,
                OrganizationUnitId = entity.OrganizationUnitId,
                EmployeeCategoryId = entity.EmployeeCategoryId,
                EmploymentTypeId = entity.EmploymentTypeId,
                SalaryStructureCode = entity.SalaryStructureCode,
                SalaryStructureName = entity.SalaryStructureName,
                CurrencyCode = entity.CurrencyCode,
                PaymentFrequency = entity.PaymentFrequency,
                DefaultBaseSalary = entity.DefaultBaseSalary,
                MinimumBaseSalary = entity.MinimumBaseSalary,
                MaximumBaseSalary = entity.MaximumBaseSalary,
                StandardWorkingDaysPerMonth = entity.StandardWorkingDaysPerMonth,
                StandardWorkingHoursPerMonth = entity.StandardWorkingHoursPerMonth,
                IsProrated = entity.IsProrated,
                IncludeOvertime = entity.IncludeOvertime,
                IncludeShiftAllowance = entity.IncludeShiftAllowance,
                IncludeOnCallAllowance = entity.IncludeOnCallAllowance,
                IncludeHazardAllowance = entity.IncludeHazardAllowance,
                IncludeBenefitDeduction = entity.IncludeBenefitDeduction,
                ComponentConfigurationJson = entity.ComponentConfigurationJson,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                Description = entity.Description,
                IsDefault = entity.IsDefault,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                WorkforcePayrollCount = await _dbContext.Set<WfpPayroll>()
                    .CountAsync(x => x.SalaryStructureId == id && !x.IsDelete, cancellationToken),
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserName(entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserName(entity.UpdateBy)
            };

            return Ok(ApiResponse<SalaryStructureDetailResponse>.Ok(
                result,
                "Detail salary structure berhasil diambil."));
        }

        [HttpPost]
        [AccessAction("Create", "Create Salary Structure", Description = "Membuat data salary structure", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("SalaryStructure", "Create")]
        public async Task<IActionResult> CreateSalaryStructure(
            [FromBody] CreateSalaryStructureRequest request,
            CancellationToken cancellationToken)
        {
            var validation = await ValidateRequestAsync(null, request, cancellationToken);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage ?? "Data salary structure tidak valid."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            if (request.IsDefault)
                await UnsetOtherDefaultAsync(null, request.SalaryGradeId, now, actor, cancellationToken);

            var entity = new MstSalaryStructure
            {
                Id = Guid.NewGuid(),
                SalaryGradeId = request.SalaryGradeId,
                LegalEntityId = NormalizeGuid(request.LegalEntityId),
                HospitalSiteId = NormalizeGuid(request.HospitalSiteId),
                OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId),
                EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId),
                EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId),
                SalaryStructureCode = await GenerateCodeAsync(cancellationToken),
                SalaryStructureName = request.SalaryStructureName.Trim(),
                CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
                PaymentFrequency = NormalizePaymentFrequency(request.PaymentFrequency),
                DefaultBaseSalary = request.DefaultBaseSalary,
                MinimumBaseSalary = request.MinimumBaseSalary,
                MaximumBaseSalary = request.MaximumBaseSalary,
                StandardWorkingDaysPerMonth = request.StandardWorkingDaysPerMonth,
                StandardWorkingHoursPerMonth = request.StandardWorkingHoursPerMonth,
                IsProrated = request.IsProrated,
                IncludeOvertime = request.IncludeOvertime,
                IncludeShiftAllowance = request.IncludeShiftAllowance,
                IncludeOnCallAllowance = request.IncludeOnCallAllowance,
                IncludeHazardAllowance = request.IncludeHazardAllowance,
                IncludeBenefitDeduction = request.IncludeBenefitDeduction,
                ComponentConfigurationJson = NormalizeNullableText(request.ComponentConfigurationJson),
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                Description = NormalizeNullableText(request.Description),
                IsDefault = request.IsDefault,
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actor,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstSalaryStructure>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<SalaryStructureCreateResponse>.Ok(
                new SalaryStructureCreateResponse
                {
                    Id = entity.Id,
                    SalaryStructureCode = entity.SalaryStructureCode,
                    SalaryStructureName = entity.SalaryStructureName,
                    IsActive = entity.IsActive
                },
                "Salary structure berhasil dibuat."));
        }

        [HttpPut("{id:guid}")]
        [AccessAction("Update", "Update Salary Structure", Description = "Mengubah data salary structure", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("SalaryStructure", "Update")]
        public async Task<IActionResult> UpdateSalaryStructure(
            Guid id,
            [FromBody] UpdateSalaryStructureRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSalaryStructure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary structure tidak ditemukan."));

            var validation = await ValidateRequestAsync(id, request, cancellationToken);

            if (!validation.IsValid)
                return BadRequest(ApiResponse<object>.Fail(400, validation.ErrorMessage ?? "Data salary structure tidak valid."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            if (request.IsDefault)
                await UnsetOtherDefaultAsync(id, request.SalaryGradeId, now, actor, cancellationToken);

            ApplyRequest(entity, request);
            entity.IsDefault = request.IsDefault && request.IsActive;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Salary structure berhasil diperbarui."));
        }

        [HttpPatch("{id:guid}/status")]
        [AccessAction("Update", "Update Salary Structure Status", Description = "Mengubah status salary structure", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("SalaryStructure", "Update")]
        public async Task<IActionResult> UpdateSalaryStructureStatus(
            Guid id,
            [FromBody] UpdateSalaryStructureStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSalaryStructure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary structure tidak ditemukan."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();

            if (request.IsDefault == true)
                await UnsetOtherDefaultAsync(id, entity.SalaryGradeId, now, actor, cancellationToken);

            entity.IsActive = request.IsActive;
            entity.IsDefault = request.IsDefault.HasValue
                ? request.IsDefault.Value && request.IsActive
                : entity.IsDefault && request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Status salary structure berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [AccessAction("Delete", "Delete Salary Structure", Description = "Menghapus salary structure", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("SalaryStructure", "Delete")]
        public async Task<IActionResult> DeleteSalaryStructure(
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<MstSalaryStructure>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(404, "Salary structure tidak ditemukan."));

            var isUsed = await _dbContext.Set<WfpPayroll>()
                .AnyAsync(x => x.SalaryStructureId == id && !x.IsDelete, cancellationToken);

            if (isUsed)
                return BadRequest(ApiResponse<object>.Fail(400, "Salary structure tidak dapat dihapus karena sudah digunakan profil payroll."));

            var now = DateTime.UtcNow;
            var actor = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsDefault = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actor;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actor;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Salary structure berhasil dihapus."));
        }

        private IQueryable<MstSalaryStructure> BuildBaseQuery()
        {
            return _dbContext.Set<MstSalaryStructure>()
                .AsNoTracking()
                .Include(x => x.SalaryGrade)
                .Where(x => !x.IsDelete);
        }

        private SalaryStructureResponse MapResponse(MstSalaryStructure x)
        {
            return new SalaryStructureResponse
            {
                Id = x.Id,
                SalaryGradeId = x.SalaryGradeId,
                SalaryGradeCode = x.SalaryGrade?.SalaryGradeCode,
                SalaryGradeName = x.SalaryGrade?.SalaryGradeName,
                LegalEntityId = x.LegalEntityId,
                HospitalSiteId = x.HospitalSiteId,
                OrganizationUnitId = x.OrganizationUnitId,
                EmployeeCategoryId = x.EmployeeCategoryId,
                EmploymentTypeId = x.EmploymentTypeId,
                SalaryStructureCode = x.SalaryStructureCode,
                SalaryStructureName = x.SalaryStructureName,
                CurrencyCode = x.CurrencyCode,
                PaymentFrequency = x.PaymentFrequency,
                DefaultBaseSalary = x.DefaultBaseSalary,
                MinimumBaseSalary = x.MinimumBaseSalary,
                MaximumBaseSalary = x.MaximumBaseSalary,
                StandardWorkingDaysPerMonth = x.StandardWorkingDaysPerMonth,
                StandardWorkingHoursPerMonth = x.StandardWorkingHoursPerMonth,
                IsProrated = x.IsProrated,
                IncludeOvertime = x.IncludeOvertime,
                IncludeShiftAllowance = x.IncludeShiftAllowance,
                IncludeOnCallAllowance = x.IncludeOnCallAllowance,
                IncludeHazardAllowance = x.IncludeHazardAllowance,
                IncludeBenefitDeduction = x.IncludeBenefitDeduction,
                ComponentConfigurationJson = x.ComponentConfigurationJson,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                Description = x.Description,
                IsDefault = x.IsDefault,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                WorkforcePayrollCount = _dbContext.Set<WfpPayroll>()
                    .Count(p => p.SalaryStructureId == x.Id && !p.IsDelete),
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = GetUserName(x.CreateBy)
            };
        }

        private void ApplyRequest(
            MstSalaryStructure entity,
            CreateSalaryStructureRequest request)
        {
            entity.SalaryGradeId = request.SalaryGradeId;
            entity.LegalEntityId = NormalizeGuid(request.LegalEntityId);
            entity.HospitalSiteId = NormalizeGuid(request.HospitalSiteId);
            entity.OrganizationUnitId = NormalizeGuid(request.OrganizationUnitId);
            entity.EmployeeCategoryId = NormalizeGuid(request.EmployeeCategoryId);
            entity.EmploymentTypeId = NormalizeGuid(request.EmploymentTypeId);
            entity.SalaryStructureName = request.SalaryStructureName.Trim();
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.PaymentFrequency = NormalizePaymentFrequency(request.PaymentFrequency);
            entity.DefaultBaseSalary = request.DefaultBaseSalary;
            entity.MinimumBaseSalary = request.MinimumBaseSalary;
            entity.MaximumBaseSalary = request.MaximumBaseSalary;
            entity.StandardWorkingDaysPerMonth = request.StandardWorkingDaysPerMonth;
            entity.StandardWorkingHoursPerMonth = request.StandardWorkingHoursPerMonth;
            entity.IsProrated = request.IsProrated;
            entity.IncludeOvertime = request.IncludeOvertime;
            entity.IncludeShiftAllowance = request.IncludeShiftAllowance;
            entity.IncludeOnCallAllowance = request.IncludeOnCallAllowance;
            entity.IncludeHazardAllowance = request.IncludeHazardAllowance;
            entity.IncludeBenefitDeduction = request.IncludeBenefitDeduction;
            entity.ComponentConfigurationJson = NormalizeNullableText(request.ComponentConfigurationJson);
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateSalaryStructureRequest request,
            CancellationToken cancellationToken)
        {
            if (request.SalaryGradeId == Guid.Empty)
                return (false, "Salary grade wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.SalaryStructureName))
                return (false, "Nama salary structure wajib diisi.");

            if (!AllowedPaymentFrequencies.Contains(request.PaymentFrequency.Trim()))
                return (false, "Payment frequency tidak valid.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) || request.CurrencyCode.Trim().Length != 3)
                return (false, "Currency code harus terdiri dari tiga karakter.");

            if (request.MinimumBaseSalary.HasValue && request.DefaultBaseSalary < request.MinimumBaseSalary.Value)
                return (false, "Default base salary tidak boleh lebih kecil dari minimum base salary.");

            if (request.MaximumBaseSalary.HasValue && request.DefaultBaseSalary > request.MaximumBaseSalary.Value)
                return (false, "Default base salary tidak boleh lebih besar dari maximum base salary.");

            if (request.MinimumBaseSalary.HasValue &&
                request.MaximumBaseSalary.HasValue &&
                request.MaximumBaseSalary.Value < request.MinimumBaseSalary.Value)
            {
                return (false, "Maximum base salary tidak boleh lebih kecil dari minimum base salary.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            if (!string.IsNullOrWhiteSpace(request.ComponentConfigurationJson))
            {
                try
                {
                    JsonDocument.Parse(request.ComponentConfigurationJson);
                }
                catch
                {
                    return (false, "ComponentConfigurationJson harus berupa JSON valid.");
                }
            }

            var salaryGradeExists = await _dbContext.Set<MstSalaryGrade>()
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == request.SalaryGradeId &&
                    x.IsActive &&
                    !x.IsDelete,
                    cancellationToken);

            if (!salaryGradeExists)
                return (false, "Salary grade tidak ditemukan atau tidak aktif.");

            var normalizedName = request.SalaryStructureName.Trim().ToLower();
            var duplicateQuery = _dbContext.Set<MstSalaryStructure>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.SalaryStructureName.ToLower() == normalizedName &&
                    x.SalaryGradeId == request.SalaryGradeId);

            if (excludeId.HasValue)
                duplicateQuery = duplicateQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateQuery.AnyAsync(cancellationToken))
                return (false, "Salary structure dengan nama dan salary grade tersebut sudah digunakan.");

            return (true, null);
        }

        private async Task UnsetOtherDefaultAsync(
            Guid? exceptId,
            Guid salaryGradeId,
            DateTime now,
            Guid actor,
            CancellationToken cancellationToken)
        {
            var query = _dbContext.Set<MstSalaryStructure>()
                .Where(x =>
                    !x.IsDelete &&
                    x.IsDefault &&
                    x.SalaryGradeId == salaryGradeId);

            if (exceptId.HasValue)
                query = query.Where(x => x.Id != exceptId.Value);

            var rows = await query.ToListAsync(cancellationToken);

            foreach (var row in rows)
            {
                row.IsDefault = false;
                row.UpdateDateTime = now;
                row.UpdateBy = actor;
            }
        }

        private static IQueryable<MstSalaryStructure> ApplyFilter(
            IQueryable<MstSalaryStructure> query,
            Guid? salaryGradeId,
            string? paymentFrequency,
            bool? isDefault,
            bool? isProrated,
            bool? isActive,
            string? search)
        {
            if (salaryGradeId.HasValue && salaryGradeId.Value != Guid.Empty)
                query = query.Where(x => x.SalaryGradeId == salaryGradeId.Value);

            if (!string.IsNullOrWhiteSpace(paymentFrequency))
                query = query.Where(x => x.PaymentFrequency == paymentFrequency.Trim());

            if (isDefault.HasValue)
                query = query.Where(x => x.IsDefault == isDefault.Value);

            if (isProrated.HasValue)
                query = query.Where(x => x.IsProrated == isProrated.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.SalaryStructureCode.ToLower().Contains(keyword) ||
                    x.SalaryStructureName.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.SalaryGrade != null && x.SalaryGrade.SalaryGradeName.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstSalaryStructure> ApplySorting(
            IQueryable<MstSalaryStructure> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "salaryStructureName").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "salarystructurecode" => desc ? query.OrderByDescending(x => x.SalaryStructureCode) : query.OrderBy(x => x.SalaryStructureCode),
                "salarygradename" => desc
                    ? query.OrderByDescending(x => x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeName : string.Empty)
                    : query.OrderBy(x => x.SalaryGrade != null ? x.SalaryGrade.SalaryGradeName : string.Empty),
                "defaultbasesalary" => desc ? query.OrderByDescending(x => x.DefaultBaseSalary) : query.OrderBy(x => x.DefaultBaseSalary),
                "isdefault" => desc ? query.OrderByDescending(x => x.IsDefault) : query.OrderBy(x => x.IsDefault),
                "isactive" => desc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => desc ? query.OrderByDescending(x => x.SalaryStructureName) : query.OrderBy(x => x.SalaryStructureName)
            };
        }

        private async Task<string> GenerateCodeAsync(CancellationToken cancellationToken)
        {
            var codes = await _dbContext.Set<MstSalaryStructure>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.SalaryStructureCode.StartsWith(CodePrefix))
                .Select(x => x.SalaryStructureCode)
                .ToListAsync(cancellationToken);

            return GenerateNextCode(codes, CodePrefix, 5);
        }

        private string? GetUserName(Guid userId)
        {
            if (userId == Guid.Empty) return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
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

        private static string NormalizePaymentFrequency(string value)
        {
            return AllowedPaymentFrequencies.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildPaymentFrequencyLabel(string value)
        {
            return value switch
            {
                "Monthly" => "Bulanan",
                "BiWeekly" => "Dua Mingguan",
                "Weekly" => "Mingguan",
                "Daily" => "Harian",
                _ => value
            };
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

        private static IQueryable<MstSalaryStructure> ApplyDateFilter(
            IQueryable<MstSalaryStructure> query,
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

        private static List<SalaryStructureCustomPeriodOptionResponse> BuildPeriodOptions()
        {
            return new List<SalaryStructureCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7days", Label = "7 hari terakhir" },
                new() { Value = "thismonth", Label = "Bulan ini" },
                new() { Value = "lastmonth", Label = "Bulan lalu" }
            };
        }
    }
}
