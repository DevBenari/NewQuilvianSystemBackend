using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.PayrollAndBenefit.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkforceCore.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/corporate/human-resource/workforce-profiles/{workforceProfileId:guid}/payroll-profile")]
    [AccessController(
        moduleCode: "HUMAN_RESOURCE_PAYROLL_MANAGEMENT",
        moduleName: "Human Resource Payroll Management",
        displayName: "Workforce Payroll Profile",
        AreaName = "Corporate",
        ControllerName = "WorkforcePayrollProfile",
        Description = "Corporate human resource workforce payroll profile",
        SortOrder = 1
    )]
    [Tags("Corporate / Human Resource / Payroll Management / Payroll Profile")]
    public class WfpPayrollController : ControllerBase
    {
        private static readonly HashSet<string> AllowedPayrollStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Active",
            "Suspended",
            "OnHold",
            "Terminated"
        };

        private static readonly HashSet<string> AllowedPaymentFrequencies = new(StringComparer.OrdinalIgnoreCase)
        {
            "Monthly",
            "BiWeekly",
            "Weekly",
            "Daily"
        };

        private static readonly HashSet<string> AllowedPaymentMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "BankTransfer",
            "Cash",
            "Cheque"
        };

        private const string LogCategory = "Corporate.HumanResource.PayrollManagement";

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public WfpPayrollController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<WfpPayrollFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Profile", Description = "Melihat metadata filter profil payroll workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollProfile", "Read")]
        public IActionResult GetFilterMetadata()
        {
            var result = new WfpPayrollFilterMetadataResponse
            {
                DefaultFilter = new WfpPayrollDefaultFilterResponse(),
                PayrollStatusOptions = AllowedPayrollStatuses
                    .OrderBy(x => x)
                    .Select(x => new WfpPayrollStringOptionResponse
                    {
                        Value = x,
                        Label = BuildPayrollStatusLabel(x)
                    })
                    .ToList(),
                PaymentFrequencyOptions = AllowedPaymentFrequencies
                    .OrderBy(x => x)
                    .Select(x => new WfpPayrollStringOptionResponse
                    {
                        Value = x,
                        Label = BuildPaymentFrequencyLabel(x)
                    })
                    .ToList(),
                PaymentMethodOptions = AllowedPaymentMethods
                    .OrderBy(x => x)
                    .Select(x => new WfpPayrollStringOptionResponse
                    {
                        Value = x,
                        Label = BuildPaymentMethodLabel(x)
                    })
                    .ToList(),
                SortOptions = new List<WfpPayrollSortOptionResponse>
                {
                    new() { Value = "payrollStatus", Label = "Status payroll" },
                    new() { Value = "payrollNumber", Label = "Nomor payroll" },
                    new() { Value = "baseSalary", Label = "Gaji pokok" },
                    new() { Value = "netSalary", Label = "Gaji bersih" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            return Ok(ApiResponse<WfpPayrollFilterMetadataResponse>.Ok(
                result,
                "Metadata filter profil payroll workforce berhasil diambil."));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<WfpPayrollSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Profile", Description = "Melihat ringkasan profil payroll workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollProfile", "Read")]
        public async Task<IActionResult> GetSummary(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var query = _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .Where(x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete);

            var result = new WfpPayrollSummaryResponse
            {
                TotalPayrollProfile = await query.CountAsync(cancellationToken),
                ActivePayrollProfile = await query.CountAsync(x => x.IsActive, cancellationToken),
                InactivePayrollProfile = await query.CountAsync(x => !x.IsActive, cancellationToken),
                EligiblePayrollProfile = await query.CountAsync(x => x.IsPayrollEligible, cancellationToken),
                ConfidentialPayrollProfile = await query.CountAsync(x => x.IsConfidential, cancellationToken),
                TotalBaseSalary = await query.SumAsync(x => x.BaseSalary, cancellationToken),
                TotalGrossSalary = await query.SumAsync(x => x.GrossSalary, cancellationToken),
                TotalNetSalary = await query.SumAsync(x => x.NetSalary, cancellationToken)
            };

            return Ok(ApiResponse<WfpPayrollSummaryResponse>.Ok(
                result,
                "Ringkasan profil payroll workforce berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<WfpPayrollResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Profile", Description = "Melihat data profil payroll workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollProfile", "Read")]
        public async Task<IActionResult> GetPayrollProfiles(
            Guid workforceProfileId,
            [FromQuery] string? payrollStatus,
            [FromQuery] string? paymentFrequency,
            [FromQuery] string? paymentMethod,
            [FromQuery] bool? isPayrollEligible,
            [FromQuery] bool? isConfidential,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "createDateTime",
            [FromQuery] string? sortDirection = "desc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilter(
                BuildBaseQuery(workforceProfileId),
                payrollStatus,
                paymentFrequency,
                paymentMethod,
                isPayrollEligible,
                isConfidential,
                isActive,
                search);

            var totalData = await query.CountAsync(cancellationToken);
            var rows = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new PagedResult<WfpPayrollResponse>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = rows.Select(MapResponse).ToList()
            };

            return Ok(ApiResponse<PagedResult<WfpPayrollResponse>>.Ok(
                result,
                "Data profil payroll workforce berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpPayrollDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Workforce Payroll Profile", Description = "Melihat detail profil payroll workforce", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("WorkforcePayrollProfile", "Read")]
        public async Task<IActionResult> GetPayrollProfileById(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await BuildBaseQuery(workforceProfileId)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil payroll workforce tidak ditemukan."));
            }

            return Ok(ApiResponse<WfpPayrollDetailResponse>.Ok(
                MapDetailResponse(entity),
                "Detail profil payroll workforce berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<WfpPayrollDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Workforce Payroll Profile", Description = "Membuat profil payroll workforce", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("WorkforcePayrollProfile", "Create")]
        public async Task<IActionResult> CreatePayrollProfile(
            Guid workforceProfileId,
            [FromBody] CreateWfpPayrollRequest request,
            CancellationToken cancellationToken)
        {
            if (!await WorkforceProfileExistsAsync(workforceProfileId, cancellationToken))
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil tenaga kerja tidak ditemukan."));
            }

            if (await _dbContext.Set<WfpPayroll>().AnyAsync(
                    x => x.WorkforceProfileId == workforceProfileId && !x.IsDelete,
                    cancellationToken))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Profil payroll untuk workforce ini sudah tersedia."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request,
                null,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil payroll tidak valid."));
            }

            var entity = new WfpPayroll
            {
                Id = Guid.NewGuid(),
                WorkforceProfileId = workforceProfileId,
                CreateDateTime = DateTime.UtcNow,
                CreateBy = GetCurrentUserId(),
                IsDelete = false,
                IsCancel = false
            };

            ApplyRequest(entity, request);

            _dbContext.Set<WfpPayroll>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "WorkforcePayrollProfile.Create",
                "Membuat profil payroll workforce.",
                new { entity.Id, entity.WorkforceProfileId, entity.PayrollStatus });

            return await GetPayrollProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<WfpPayrollDetailResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Payroll Profile", Description = "Mengubah profil payroll workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforcePayrollProfile", "Update")]
        public async Task<IActionResult> UpdatePayrollProfile(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpPayrollRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpPayroll>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil payroll workforce tidak ditemukan."));
            }

            var validation = await ValidateRequestAsync(
                workforceProfileId,
                request,
                id,
                cancellationToken);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data profil payroll tidak valid."));
            }

            ApplyRequest(entity, request);
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await GetPayrollProfileById(workforceProfileId, entity.Id, cancellationToken);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Update Workforce Payroll Profile", Description = "Mengubah status profil payroll workforce", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("WorkforcePayrollProfile", "Update")]
        public async Task<IActionResult> UpdatePayrollProfileStatus(
            Guid workforceProfileId,
            Guid id,
            [FromBody] UpdateWfpPayrollStatusRequest request,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpPayroll>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil payroll workforce tidak ditemukan."));
            }

            if (!AllowedPayrollStatuses.Contains(request.PayrollStatus.Trim()))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PayrollStatus tidak valid."));
            }

            if (request.EffectiveEndDate.HasValue &&
                entity.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.Value.Date < entity.EffectiveStartDate.Value.Date)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate."));
            }

            entity.PayrollStatus = NormalizeAllowedValue(AllowedPayrollStatuses, request.PayrollStatus);
            entity.IsPayrollEligible = request.IsPayrollEligible;
            entity.IsActive = request.IsActive;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.Description = NormalizeNullableText(request.Description) ?? entity.Description;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status profil payroll workforce berhasil diperbarui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Workforce Payroll Profile", Description = "Menghapus profil payroll workforce", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("WorkforcePayrollProfile", "Delete")]
        public async Task<IActionResult> DeletePayrollProfile(
            Guid workforceProfileId,
            Guid id,
            CancellationToken cancellationToken)
        {
            var entity = await _dbContext.Set<WfpPayroll>()
                .FirstOrDefaultAsync(
                    x => x.Id == id &&
                         x.WorkforceProfileId == workforceProfileId &&
                         !x.IsDelete,
                    cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Profil payroll workforce tidak ditemukan."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPayrollEligible = false;
            entity.PayrollStatus = "Terminated";
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(
                null,
                "Profil payroll workforce berhasil dihapus."));
        }

        private IQueryable<WfpPayroll> BuildBaseQuery(Guid workforceProfileId)
        {
            return _dbContext.Set<WfpPayroll>()
                .AsNoTracking()
                .Include(x => x.WorkforceProfile)
                .Include(x => x.Employee)
                .Include(x => x.SalaryStructure)
                .Include(x => x.SalaryGrade)
                .Include(x => x.LastPayrollPeriod)
                .Where(x =>
                    x.WorkforceProfileId == workforceProfileId &&
                    !x.IsDelete);
        }

        private static IQueryable<WfpPayroll> ApplyFilter(
            IQueryable<WfpPayroll> query,
            string? payrollStatus,
            string? paymentFrequency,
            string? paymentMethod,
            bool? isPayrollEligible,
            bool? isConfidential,
            bool? isActive,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(payrollStatus))
                query = query.Where(x => x.PayrollStatus == payrollStatus.Trim());

            if (!string.IsNullOrWhiteSpace(paymentFrequency))
                query = query.Where(x => x.PaymentFrequency == paymentFrequency.Trim());

            if (!string.IsNullOrWhiteSpace(paymentMethod))
                query = query.Where(x => x.PaymentMethod == paymentMethod.Trim());

            if (isPayrollEligible.HasValue)
                query = query.Where(x => x.IsPayrollEligible == isPayrollEligible.Value);

            if (isConfidential.HasValue)
                query = query.Where(x => x.IsConfidential == isConfidential.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    (x.PayrollNumber != null && x.PayrollNumber.ToLower().Contains(keyword)) ||
                    (x.PayrollGroupCode != null && x.PayrollGroupCode.ToLower().Contains(keyword)) ||
                    x.PayrollStatus.ToLower().Contains(keyword) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<WfpPayroll> ApplySorting(
            IQueryable<WfpPayroll> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = !string.Equals(
                sortDirection?.Trim(),
                "asc",
                StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "createDateTime").Trim().ToLowerInvariant() switch
            {
                "payrollstatus" => isDescending
                    ? query.OrderByDescending(x => x.PayrollStatus)
                    : query.OrderBy(x => x.PayrollStatus),
                "payrollnumber" => isDescending
                    ? query.OrderByDescending(x => x.PayrollNumber)
                    : query.OrderBy(x => x.PayrollNumber),
                "basesalary" => isDescending
                    ? query.OrderByDescending(x => x.BaseSalary)
                    : query.OrderBy(x => x.BaseSalary),
                "netsalary" => isDescending
                    ? query.OrderByDescending(x => x.NetSalary)
                    : query.OrderBy(x => x.NetSalary),
                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),
                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime)
            };
        }

        private WfpPayrollResponse MapResponse(WfpPayroll entity)
        {
            return new WfpPayrollResponse
            {
                Id = entity.Id,
                WorkforceProfileId = entity.WorkforceProfileId,
                WorkforceProfileCode = entity.WorkforceProfile?.ProfileCode ?? string.Empty,
                WorkforceDisplayName = entity.WorkforceProfile?.DisplayName ?? string.Empty,
                EmployeeId = entity.EmployeeId,
                EmployeeCode = entity.Employee?.EmployeeCode,
                EmployeeName = entity.Employee?.FullName,
                OrganizationAssignmentId = entity.OrganizationAssignmentId,
                SalaryAssignmentId = entity.SalaryAssignmentId,
                BankAccountId = entity.BankAccountId,
                CostCenterId = entity.CostCenterId,
                SalaryStructureId = entity.SalaryStructureId,
                SalaryStructureCode = entity.SalaryStructure?.SalaryStructureCode,
                SalaryStructureName = entity.SalaryStructure?.SalaryStructureName,
                SalaryGradeId = entity.SalaryGradeId,
                SalaryGradeCode = entity.SalaryGrade?.SalaryGradeCode,
                SalaryGradeName = entity.SalaryGrade?.SalaryGradeName,
                LastPayrollPeriodId = entity.LastPayrollPeriodId,
                LastPayrollPeriodCode = entity.LastPayrollPeriod?.PayrollPeriodCode,
                LastPayrollPeriodName = entity.LastPayrollPeriod?.PayrollPeriodName,
                PayrollNumber = entity.PayrollNumber,
                PayrollGroupCode = entity.PayrollGroupCode,
                PayrollStatus = entity.PayrollStatus,
                CurrencyCode = entity.CurrencyCode,
                PaymentFrequency = entity.PaymentFrequency,
                PaymentMethod = entity.PaymentMethod,
                IsPayrollEligible = entity.IsPayrollEligible,
                IsConfidential = entity.IsConfidential,
                EffectiveStartDate = entity.EffectiveStartDate,
                EffectiveEndDate = entity.EffectiveEndDate,
                BaseSalary = entity.BaseSalary,
                TotalAllowance = entity.TotalAllowance,
                TotalDeduction = entity.TotalDeduction,
                GrossSalary = entity.GrossSalary,
                TaxAmount = entity.TaxAmount,
                InsuranceAmount = entity.InsuranceAmount,
                NetSalary = entity.NetSalary,
                LastCalculatedAt = entity.LastCalculatedAt,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : entity.CreateBy,
                CreateByName = GetUserDisplayName(entity.CreateBy)
            };
        }

        private WfpPayrollDetailResponse MapDetailResponse(WfpPayroll entity)
        {
            var response = MapResponse(entity);

            return new WfpPayrollDetailResponse
            {
                Id = response.Id,
                WorkforceProfileId = response.WorkforceProfileId,
                WorkforceProfileCode = response.WorkforceProfileCode,
                WorkforceDisplayName = response.WorkforceDisplayName,
                EmployeeId = response.EmployeeId,
                EmployeeCode = response.EmployeeCode,
                EmployeeName = response.EmployeeName,
                OrganizationAssignmentId = response.OrganizationAssignmentId,
                SalaryAssignmentId = response.SalaryAssignmentId,
                BankAccountId = response.BankAccountId,
                CostCenterId = response.CostCenterId,
                SalaryStructureId = response.SalaryStructureId,
                SalaryStructureCode = response.SalaryStructureCode,
                SalaryStructureName = response.SalaryStructureName,
                SalaryGradeId = response.SalaryGradeId,
                SalaryGradeCode = response.SalaryGradeCode,
                SalaryGradeName = response.SalaryGradeName,
                LastPayrollPeriodId = response.LastPayrollPeriodId,
                LastPayrollPeriodCode = response.LastPayrollPeriodCode,
                LastPayrollPeriodName = response.LastPayrollPeriodName,
                PayrollNumber = response.PayrollNumber,
                PayrollGroupCode = response.PayrollGroupCode,
                PayrollStatus = response.PayrollStatus,
                CurrencyCode = response.CurrencyCode,
                PaymentFrequency = response.PaymentFrequency,
                PaymentMethod = response.PaymentMethod,
                IsPayrollEligible = response.IsPayrollEligible,
                IsConfidential = response.IsConfidential,
                EffectiveStartDate = response.EffectiveStartDate,
                EffectiveEndDate = response.EffectiveEndDate,
                BaseSalary = response.BaseSalary,
                TotalAllowance = response.TotalAllowance,
                TotalDeduction = response.TotalDeduction,
                GrossSalary = response.GrossSalary,
                TaxAmount = response.TaxAmount,
                InsuranceAmount = response.InsuranceAmount,
                NetSalary = response.NetSalary,
                LastCalculatedAt = response.LastCalculatedAt,
                Description = response.Description,
                IsActive = response.IsActive,
                CreateDateTime = response.CreateDateTime,
                CreateBy = response.CreateBy,
                CreateByName = response.CreateByName,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : entity.UpdateBy,
                UpdateByName = GetUserDisplayName(entity.UpdateBy)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid workforceProfileId,
            CreateWfpPayrollRequest request,
            Guid? currentId,
            CancellationToken cancellationToken)
        {
            if (!AllowedPayrollStatuses.Contains(request.PayrollStatus.Trim()))
                return (false, "PayrollStatus tidak valid.");

            if (!AllowedPaymentFrequencies.Contains(request.PaymentFrequency.Trim()))
                return (false, "PaymentFrequency tidak valid.");

            if (!AllowedPaymentMethods.Contains(request.PaymentMethod.Trim()))
                return (false, "PaymentMethod tidak valid.");

            if (string.IsNullOrWhiteSpace(request.CurrencyCode) ||
                request.CurrencyCode.Trim().Length != 3)
            {
                return (false, "CurrencyCode harus terdiri dari tiga karakter.");
            }

            if (new[]
                {
                    request.BaseSalary,
                    request.TotalAllowance,
                    request.TotalDeduction,
                    request.GrossSalary,
                    request.TaxAmount,
                    request.InsuranceAmount,
                    request.NetSalary
                }.Any(x => x < 0))
            {
                return (false, "Nilai payroll tidak boleh negatif.");
            }

            if (request.EffectiveStartDate.HasValue &&
                request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "EffectiveEndDate tidak boleh lebih kecil dari EffectiveStartDate.");
            }

            var payrollNumber = NormalizeNullableText(request.PayrollNumber);
            if (payrollNumber != null &&
                await _dbContext.Set<WfpPayroll>().AnyAsync(
                    x => !x.IsDelete &&
                         x.PayrollNumber == payrollNumber &&
                         (!currentId.HasValue || x.Id != currentId.Value),
                    cancellationToken))
            {
                return (false, "PayrollNumber sudah digunakan.");
            }

            if (!await ValidateWorkforceRelationsAsync(
                    workforceProfileId,
                    request,
                    cancellationToken))
            {
                return (false, "Salah satu relasi workforce payroll tidak valid atau tidak aktif.");
            }

            return (true, null);
        }

        private async Task<bool> ValidateWorkforceRelationsAsync(
            Guid workforceProfileId,
            CreateWfpPayrollRequest request,
            CancellationToken cancellationToken)
        {
            if (HasValue(request.EmployeeId) &&
                !await _dbContext.Set<MstEmployee>().AnyAsync(
                    x => x.Id == request.EmployeeId!.Value &&
                         x.WorkforceProfileId == workforceProfileId &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.OrganizationAssignmentId) &&
                !await _dbContext.Set<WfpOrganizationAssignment>().AnyAsync(
                    x => x.Id == request.OrganizationAssignmentId!.Value &&
                         x.WorkforceProfileId == workforceProfileId &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.SalaryAssignmentId) &&
                !await _dbContext.Set<WfpSalaryAssignment>().AnyAsync(
                    x => x.Id == request.SalaryAssignmentId!.Value &&
                         x.WorkforceProfileId == workforceProfileId &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.BankAccountId) &&
                !await _dbContext.Set<WfpBankAccount>().AnyAsync(
                    x => x.Id == request.BankAccountId!.Value &&
                         x.WorkforceProfileId == workforceProfileId &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.CostCenterId) &&
                !await _dbContext.Set<MstCostCenter>().AnyAsync(
                    x => x.Id == request.CostCenterId!.Value &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.SalaryStructureId) &&
                !await _dbContext.Set<MstSalaryStructure>().AnyAsync(
                    x => x.Id == request.SalaryStructureId!.Value &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.SalaryGradeId) &&
                !await _dbContext.Set<MstSalaryGrade>().AnyAsync(
                    x => x.Id == request.SalaryGradeId!.Value &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            if (HasValue(request.LastPayrollPeriodId) &&
                !await _dbContext.Set<MstPayrollPeriod>().AnyAsync(
                    x => x.Id == request.LastPayrollPeriodId!.Value &&
                         x.IsActive &&
                         !x.IsDelete,
                    cancellationToken))
            {
                return false;
            }

            return true;
        }

        private static void ApplyRequest(
            WfpPayroll entity,
            CreateWfpPayrollRequest request)
        {
            entity.EmployeeId = NormalizeNullableGuid(request.EmployeeId);
            entity.OrganizationAssignmentId = NormalizeNullableGuid(request.OrganizationAssignmentId);
            entity.SalaryAssignmentId = NormalizeNullableGuid(request.SalaryAssignmentId);
            entity.BankAccountId = NormalizeNullableGuid(request.BankAccountId);
            entity.CostCenterId = NormalizeNullableGuid(request.CostCenterId);
            entity.SalaryStructureId = NormalizeNullableGuid(request.SalaryStructureId);
            entity.SalaryGradeId = NormalizeNullableGuid(request.SalaryGradeId);
            entity.LastPayrollPeriodId = NormalizeNullableGuid(request.LastPayrollPeriodId);
            entity.PayrollNumber = NormalizeNullableText(request.PayrollNumber);
            entity.PayrollGroupCode = NormalizeNullableText(request.PayrollGroupCode);
            entity.PayrollStatus = NormalizeAllowedValue(AllowedPayrollStatuses, request.PayrollStatus);
            entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
            entity.PaymentFrequency = NormalizeAllowedValue(AllowedPaymentFrequencies, request.PaymentFrequency);
            entity.PaymentMethod = NormalizeAllowedValue(AllowedPaymentMethods, request.PaymentMethod);
            entity.IsPayrollEligible = request.IsPayrollEligible;
            entity.IsConfidential = request.IsConfidential;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.BaseSalary = request.BaseSalary;
            entity.TotalAllowance = request.TotalAllowance;
            entity.TotalDeduction = request.TotalDeduction;
            entity.GrossSalary = request.GrossSalary;
            entity.TaxAmount = request.TaxAmount;
            entity.InsuranceAmount = request.InsuranceAmount;
            entity.NetSalary = request.NetSalary;
            entity.LastCalculatedAt = request.LastCalculatedAt;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
        }

        private async Task<bool> WorkforceProfileExistsAsync(
            Guid workforceProfileId,
            CancellationToken cancellationToken)
        {
            return workforceProfileId != Guid.Empty &&
                   await _dbContext.MstWorkforceProfiles
                       .AsNoTracking()
                       .AnyAsync(
                           x => x.Id == workforceProfileId &&
                                x.IsActive &&
                                !x.IsDelete,
                           cancellationToken);
        }

        private string? GetUserDisplayName(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            return _dbContext.Users
                .Where(x => x.Id == userId)
                .Select(x => x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode)
                .FirstOrDefault();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText =
                User.FindFirstValue("user_id") ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            pageSize = pageSize <= 0 ? 25 : pageSize;
            pageSize = pageSize > 100 ? 100 : pageSize;
            return (pageNumber, pageSize);
        }

        private static bool HasValue(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return !HasValue(value) ? null : value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string NormalizeAllowedValue(
            IEnumerable<string> allowedValues,
            string value)
        {
            return allowedValues.First(x =>
                x.Equals(value.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildPayrollStatusLabel(string value)
        {
            return value switch
            {
                "Active" => "Aktif",
                "Suspended" => "Ditangguhkan",
                "OnHold" => "Ditahan",
                "Terminated" => "Dihentikan",
                _ => value
            };
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

        private static string BuildPaymentMethodLabel(string value)
        {
            return value switch
            {
                "BankTransfer" => "Transfer Bank",
                "Cash" => "Tunai",
                "Cheque" => "Cek",
                _ => value
            };
        }
    }
}
