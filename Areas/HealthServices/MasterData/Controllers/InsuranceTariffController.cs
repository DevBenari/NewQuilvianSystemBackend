using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseInsuranceTariffPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.InsuranceTariffResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/insurance-tariffs")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Insurance Tariff",
        AreaName = "HealthServices",
        ControllerName = "InsuranceTariff",
        Description = "Health service master data insurance tariff",
        SortOrder = 14
    )]
    [Tags("Health Services / Master Data / Insurance Tariff")]
    public class InsuranceTariffController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string InsuranceTariffCodePrefix = "ITR-RSMMC-";
        private const int InsuranceTariffCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public InsuranceTariffController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Tariff", Description = "Melihat data insurance tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceTariff", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new InsuranceTariffFilterMetadataResponse
            {
                DefaultFilter = new InsuranceTariffDefaultFilterResponse(),
                CustomPeriods = new List<InsuranceTariffCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7Days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<InsuranceTariffSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "insuranceTariffCode", Label = "Kode tariff asuransi" },
                    new() { Value = "insuranceTariffName", Label = "Nama tariff asuransi" },
                    new() { Value = "tariffCategoryName", Label = "Kategori tariff" },
                    new() { Value = "patientClassName", Label = "Kelas pasien" },
                    new() { Value = "benefitPlanCode", Label = "Kode benefit plan" },
                    new() { Value = "contractPrice", Label = "Harga kontrak" },
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceTariff.GetFilterMetadata",
                "Mengambil metadata filter insurance tariff.",
                result
            );

            return Ok(ApiResponse<InsuranceTariffFilterMetadataResponse>.Ok(
                result,
                "Metadata filter insurance tariff berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Tariff", Description = "Melihat data insurance tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceTariff", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var operationalDate = AppDateTimeHelper.OperationalDate().Date;
            var nextOperationalDate = operationalDate.AddDays(1);

            var query = _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new InsuranceTariffSummaryResponse
            {
                TotalInsuranceTariff = await query.CountAsync(),
                ActiveInsuranceTariff = await query.CountAsync(x => x.IsActive),
                InactiveInsuranceTariff = await query.CountAsync(x => !x.IsActive),
                NeedApprovalTariff = await query.CountAsync(x =>
                    x.IsActive && x.IsNeedApproval),
                UsingContractPriceTariff = await query.CountAsync(x =>
                    x.IsActive && x.IsUsingContractPrice),
                EffectiveTariff = await query.CountAsync(x =>
                    x.IsActive &&
                    (!x.EffectiveStartDate.HasValue ||
                     x.EffectiveStartDate.Value < nextOperationalDate) &&
                    (!x.EffectiveEndDate.HasValue ||
                     x.EffectiveEndDate.Value >= operationalDate)),
                ExpiredTariff = await query.CountAsync(x =>
                    x.IsActive &&
                    x.EffectiveEndDate.HasValue &&
                    x.EffectiveEndDate.Value < operationalDate),
                FutureTariff = await query.CountAsync(x =>
                    x.IsActive &&
                    x.EffectiveStartDate.HasValue &&
                    x.EffectiveStartDate.Value >= nextOperationalDate)
            };

            return Ok(ApiResponse<InsuranceTariffSummaryResponse>.Ok(
                result,
                "Ringkasan insurance tariff berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseInsuranceTariffPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Tariff", Description = "Melihat data insurance tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceTariff", "Read")]
        public async Task<IActionResult> GetInsuranceTariffs(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] bool? isActive,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var operationalDate = AppDateTimeHelper.OperationalDate().Date;
            var nextOperationalDate = operationalDate.AddDays(1);

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty)
                query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.Tariff != null && x.Tariff.TariffCategoryId == tariffCategoryId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InsuranceTariffResponse
                {
                    Id = x.Id,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderCode = x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderCode
                        : string.Empty,
                    InsuranceProviderName = x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderName
                        : string.Empty,

                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null
                        ? x.Tariff.TariffCode
                        : string.Empty,
                    TariffName = x.Tariff != null
                        ? x.Tariff.TariffName
                        : string.Empty,
                    TariffCategoryId = x.Tariff != null
                        ? x.Tariff.TariffCategoryId
                        : Guid.Empty,
                    TariffCategoryName = x.Tariff != null && x.Tariff.TariffCategory != null
                        ? x.Tariff.TariffCategory.TariffCategoryName
                        : string.Empty,
                    DrugId = x.Tariff != null ? x.Tariff.DrugId : null,
                    DrugName = x.Tariff != null && x.Tariff.Drug != null
                        ? x.Tariff.Drug.DrugName
                        : null,
                    ProcedureId = x.Tariff != null ? x.Tariff.ProcedureId : null,
                    ProcedureName = x.Tariff != null && x.Tariff.Procedure != null
                        ? x.Tariff.Procedure.ProcedureName
                        : null,

                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null
                        ? x.PatientClass.PatientClassCode
                        : null,
                    PatientClassName = x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : null,

                    InsuranceTariffCode = x.InsuranceTariffCode,
                    InsuranceTariffName = x.InsuranceTariffName,
                    ExternalServiceCode = x.ExternalServiceCode,
                    ExternalClassCode = x.ExternalClassCode,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,

                    HospitalPrice = x.Tariff != null ? x.Tariff.NormalPrice : 0,
                    ContractPrice = x.ContractPrice,
                    HospitalPriceSnapshot = x.HospitalPriceSnapshot,
                    DiscountAmount = x.DiscountAmount,
                    DiscountPercent = x.DiscountPercent,
                    IsUsingContractPrice = x.IsUsingContractPrice,
                    IsNeedApproval = x.IsNeedApproval,
                    Priority = x.Priority,

                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsCurrentlyEffective =
                        x.IsActive &&
                        (!x.EffectiveStartDate.HasValue ||
                         x.EffectiveStartDate.Value < nextOperationalDate) &&
                        (!x.EffectiveEndDate.HasValue ||
                         x.EffectiveEndDate.Value >= operationalDate),

                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseInsuranceTariffPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseInsuranceTariffPagedResult>.Ok(
                result,
                "Data insurance tariff berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Tariff", Description = "Melihat data insurance tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceTariff", "Read")]
        public async Task<IActionResult> GetInsuranceTariffOptions(
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] Guid? tariffCategoryId,
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

            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty)
                query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.Tariff != null && x.Tariff.TariffCategoryId == tariffCategoryId.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.InsuranceTariffName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InsuranceTariffOptionResponse
                {
                    Id = x.Id,

                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderName
                        : string.Empty,

                    InsuranceTariffCode = x.InsuranceTariffCode,
                    InsuranceTariffName = x.InsuranceTariffName,

                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null
                        ? x.Tariff.TariffCode
                        : string.Empty,
                    TariffName = x.Tariff != null
                        ? x.Tariff.TariffName
                        : string.Empty,

                    DrugId = x.Tariff != null ? x.Tariff.DrugId : null,
                    DrugName = x.Tariff != null && x.Tariff.Drug != null
                        ? x.Tariff.Drug.DrugName
                        : null,

                    ProcedureId = x.Tariff != null ? x.Tariff.ProcedureId : null,
                    ProcedureName = x.Tariff != null && x.Tariff.Procedure != null
                        ? x.Tariff.Procedure.ProcedureName
                        : null,

                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : null,

                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,

                    ContractPrice = x.ContractPrice,
                    IsNeedApproval = x.IsNeedApproval,
                    Priority = x.Priority
                })
                .ToListAsync();

            var result = new InsuranceTariffOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<InsuranceTariffOptionPagedResponse>.Ok(
                result,
                "Data pilihan insurance tariff berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Insurance Tariff", Description = "Melihat data insurance tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceTariff", "Read")]
        public async Task<IActionResult> GetInsuranceTariffById(Guid id)
        {
            var operationalDate = AppDateTimeHelper.OperationalDate().Date;
            var nextOperationalDate = operationalDate.AddDays(1);

            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new InsuranceTariffDetailResponse
                {
                    Id = x.Id,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderCode = x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderCode
                        : string.Empty,
                    InsuranceProviderName = x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderName
                        : string.Empty,

                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null
                        ? x.Tariff.TariffCode
                        : string.Empty,
                    TariffName = x.Tariff != null
                        ? x.Tariff.TariffName
                        : string.Empty,
                    TariffCategoryId = x.Tariff != null
                        ? x.Tariff.TariffCategoryId
                        : Guid.Empty,
                    TariffCategoryName = x.Tariff != null && x.Tariff.TariffCategory != null
                        ? x.Tariff.TariffCategory.TariffCategoryName
                        : string.Empty,
                    DrugId = x.Tariff != null ? x.Tariff.DrugId : null,
                    DrugName = x.Tariff != null && x.Tariff.Drug != null
                        ? x.Tariff.Drug.DrugName
                        : null,
                    ProcedureId = x.Tariff != null ? x.Tariff.ProcedureId : null,
                    ProcedureName = x.Tariff != null && x.Tariff.Procedure != null
                        ? x.Tariff.Procedure.ProcedureName
                        : null,

                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null
                        ? x.PatientClass.PatientClassCode
                        : null,
                    PatientClassName = x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : null,

                    InsuranceTariffCode = x.InsuranceTariffCode,
                    InsuranceTariffName = x.InsuranceTariffName,
                    ExternalServiceCode = x.ExternalServiceCode,
                    ExternalClassCode = x.ExternalClassCode,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,

                    HospitalPrice = x.Tariff != null ? x.Tariff.NormalPrice : 0,
                    ContractPrice = x.ContractPrice,
                    HospitalPriceSnapshot = x.HospitalPriceSnapshot,
                    DiscountAmount = x.DiscountAmount,
                    DiscountPercent = x.DiscountPercent,
                    IsUsingContractPrice = x.IsUsingContractPrice,
                    IsNeedApproval = x.IsNeedApproval,
                    Priority = x.Priority,

                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    IsCurrentlyEffective =
                        x.IsActive &&
                        (!x.EffectiveStartDate.HasValue ||
                         x.EffectiveStartDate.Value < nextOperationalDate) &&
                        (!x.EffectiveEndDate.HasValue ||
                         x.EffectiveEndDate.Value >= operationalDate),

                    SortOrder = x.SortOrder,
                    BillingInstruction = x.BillingInstruction,
                    ClaimInstruction = x.ClaimInstruction,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    UpdateDateTime = x.UpdateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance tariff tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<InsuranceTariffDetailResponse>.Ok(
                data,
                "Detail insurance tariff berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Insurance Tariff", Description = "Membuat data insurance tariff", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InsuranceTariff", "Create")]
        public async Task<IActionResult> CreateInsuranceTariff([FromBody] CreateInsuranceTariffRequest request)
        {
            var validation = await ValidateRequestAsync(null, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance tariff tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var generatedCode = await GenerateInsuranceTariffCodeAsync();

            var entity = new MstInsuranceTariff
            {
                Id = Guid.NewGuid(),
                InsuranceProviderId = request.InsuranceProviderId,
                TariffId = request.TariffId,
                PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                InsuranceTariffCode = generatedCode,
                InsuranceTariffName = request.InsuranceTariffName.Trim(),
                ExternalServiceCode = NormalizeNullableString(request.ExternalServiceCode),
                ExternalClassCode = NormalizeNullableString(request.ExternalClassCode),
                BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant(),
                BenefitPlanName = NormalizeNullableString(request.BenefitPlanName),
                ContractPrice = request.ContractPrice,
                HospitalPriceSnapshot = request.HospitalPriceSnapshot,
                DiscountAmount = request.DiscountAmount,
                DiscountPercent = request.DiscountPercent,
                IsUsingContractPrice = request.IsUsingContractPrice,
                IsNeedApproval = request.IsNeedApproval,
                Priority = request.Priority,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                SortOrder = request.SortOrder,
                BillingInstruction = NormalizeNullableString(request.BillingInstruction),
                ClaimInstruction = NormalizeNullableString(request.ClaimInstruction),
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstInsuranceTariff>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new InsuranceTariffCreateResponse
            {
                Id = entity.Id,
                InsuranceProviderId = entity.InsuranceProviderId,
                TariffId = entity.TariffId,
                PatientClassId = entity.PatientClassId,
                InsuranceTariffCode = entity.InsuranceTariffCode,
                InsuranceTariffName = entity.InsuranceTariffName,
                ContractPrice = entity.ContractPrice,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceTariff.CreateInsuranceTariff",
                "Membuat data insurance tariff.",
                response
            );

            return Ok(ApiResponse<InsuranceTariffCreateResponse>.Ok(
                response,
                "Insurance tariff berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Tariff", Description = "Mengubah data insurance tariff", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceTariff", "Update")]
        public async Task<IActionResult> UpdateInsuranceTariff(Guid id, [FromBody] UpdateInsuranceTariffRequest request)
        {
            var entity = await _dbContext.Set<MstInsuranceTariff>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance tariff tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(id, request);

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance tariff tidak valid."
                ));
            }

            entity.InsuranceProviderId = request.InsuranceProviderId;
            entity.TariffId = request.TariffId;
            entity.PatientClassId = NormalizeNullableGuid(request.PatientClassId);
            entity.InsuranceTariffName = request.InsuranceTariffName.Trim();
            entity.ExternalServiceCode = NormalizeNullableString(request.ExternalServiceCode);
            entity.ExternalClassCode = NormalizeNullableString(request.ExternalClassCode);
            entity.BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant();
            entity.BenefitPlanName = NormalizeNullableString(request.BenefitPlanName);
            entity.ContractPrice = request.ContractPrice;
            entity.HospitalPriceSnapshot = request.HospitalPriceSnapshot;
            entity.DiscountAmount = request.DiscountAmount;
            entity.DiscountPercent = request.DiscountPercent;
            entity.IsUsingContractPrice = request.IsUsingContractPrice;
            entity.IsNeedApproval = request.IsNeedApproval;
            entity.Priority = request.Priority;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.SortOrder = request.SortOrder;
            entity.BillingInstruction = NormalizeNullableString(request.BillingInstruction);
            entity.ClaimInstruction = NormalizeNullableString(request.ClaimInstruction);
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Insurance tariff berhasil diperbarui."
            ));
        }


        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Tariff", Description = "Mengubah status insurance tariff", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceTariff", "Update")]
        public async Task<IActionResult> UpdateInsuranceTariffStatus(Guid id, [FromBody] UpdateInsuranceTariffStatusRequest request)
        {
            var entity = await _dbContext.Set<MstInsuranceTariff>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance tariff tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new InsuranceTariffStatusResponse
            {
                Id = entity.Id,
                InsuranceTariffCode = entity.InsuranceTariffCode,
                InsuranceTariffName = entity.InsuranceTariffName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceTariff.UpdateInsuranceTariffStatus",
                request.IsActive ? "Mengaktifkan insurance tariff." : "Menonaktifkan insurance tariff.",
                result
            );

            return Ok(ApiResponse<InsuranceTariffStatusResponse>.Ok(
                result,
                request.IsActive ? "Insurance tariff berhasil diaktifkan." : "Insurance tariff berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Insurance Tariff", Description = "Menghapus data insurance tariff", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("InsuranceTariff", "Delete")]
        public async Task<IActionResult> DeleteInsuranceTariff(Guid id, [FromBody] DeleteInsuranceTariffRequest? request = null)
        {
            var entity = await _dbContext.Set<MstInsuranceTariff>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance tariff tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            if (!string.IsNullOrWhiteSpace(request?.DeleteReason))
                entity.Description = NormalizeNullableString(request.DeleteReason);

            await _dbContext.SaveChangesAsync();

            var result = new InsuranceTariffDeleteResponse
            {
                Id = entity.Id,
                InsuranceTariffCode = entity.InsuranceTariffCode,
                InsuranceTariffName = entity.InsuranceTariffName,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<InsuranceTariffDeleteResponse>.Ok(
                result,
                "Insurance tariff berhasil dihapus."
            ));
        }

        private IQueryable<MstInsuranceTariff> BuildBaseQuery()
        {
            return _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Include(x => x.InsuranceProvider)
                .Include(x => x.Tariff)
                .Include(x => x.PatientClass)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstInsuranceTariff> ApplySearch(
            IQueryable<MstInsuranceTariff> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var keyword = search.Trim().ToLower();

            return query.Where(x =>
                x.InsuranceTariffCode.ToLower().Contains(keyword) ||
                x.InsuranceTariffName.ToLower().Contains(keyword) ||
                (x.ExternalServiceCode != null && x.ExternalServiceCode.ToLower().Contains(keyword)) ||
                (x.ExternalClassCode != null && x.ExternalClassCode.ToLower().Contains(keyword)) ||
                (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                (x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(keyword)) ||
                (x.BillingInstruction != null && x.BillingInstruction.ToLower().Contains(keyword)) ||
                (x.ClaimInstruction != null && x.ClaimInstruction.ToLower().Contains(keyword)) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderCode.ToLower().Contains(keyword)) ||
                (x.Tariff != null && x.Tariff.TariffCode.ToLower().Contains(keyword)) ||
                (x.Tariff != null && x.Tariff.TariffName.ToLower().Contains(keyword)) ||
                (x.PatientClass != null && x.PatientClass.PatientClassCode.ToLower().Contains(keyword)) ||
                (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateInsuranceTariffRequest request)
        {
            if (request.InsuranceProviderId == Guid.Empty)
                return (false, "Insurance provider wajib dipilih.");

            if (request.TariffId == Guid.Empty)
                return (false, "Tariff rumah sakit wajib dipilih.");

            if (string.IsNullOrWhiteSpace(request.InsuranceTariffName))
                return (false, "Nama insurance tariff wajib diisi.");

            if (request.ContractPrice < 0)
                return (false, "Harga kontrak tidak boleh kurang dari 0.");

            if (request.HospitalPriceSnapshot.HasValue && request.HospitalPriceSnapshot.Value < 0)
                return (false, "Snapshot harga rumah sakit tidak boleh kurang dari 0.");

            if (request.DiscountAmount.HasValue && request.DiscountAmount.Value < 0)
                return (false, "Discount amount tidak boleh kurang dari 0.");

            if (request.DiscountPercent.HasValue &&
                (request.DiscountPercent.Value < 0 || request.DiscountPercent.Value > 100))
            {
                return (false, "Discount percent harus berada di antara 0 sampai 100.");
            }

            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");
            }

            var providerExists = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.InsuranceProviderId && x.IsActive && !x.IsDelete);

            if (!providerExists)
                return (false, "Insurance provider tidak valid atau tidak aktif.");

            var tariff = await _dbContext.Set<MstTariff>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == request.TariffId && x.IsActive && !x.IsDelete);

            if (tariff == null)
                return (false, "Tariff rumah sakit tidak valid atau tidak aktif.");

            var patientClassId = NormalizeNullableGuid(request.PatientClassId);
            if (patientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AsNoTracking()
                    .AnyAsync(x => x.Id == patientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                    return (false, "Patient class tidak valid atau tidak aktif.");
            }

            var benefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant();
            var startDate = request.EffectiveStartDate?.Date;
            var endDate = request.EffectiveEndDate?.Date;

            var overlapQuery = _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderId == request.InsuranceProviderId &&
                    x.TariffId == request.TariffId &&
                    x.PatientClassId == patientClassId &&
                    x.BenefitPlanCode == benefitPlanCode &&
                    x.Priority == request.Priority);

            if (excludeId.HasValue)
                overlapQuery = overlapQuery.Where(x => x.Id != excludeId.Value);

            var hasOverlap = await overlapQuery.AnyAsync(x =>
                (!endDate.HasValue || !x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= endDate.Value) &&
                (!x.EffectiveEndDate.HasValue || !startDate.HasValue || x.EffectiveEndDate.Value.Date >= startDate.Value));

            if (hasOverlap)
            {
                return (false,
                    "Periode insurance tariff overlap dengan data lain pada provider, tariff, benefit plan, kelas, dan priority yang sama.");
            }

            return (true, null);
        }

        private async Task<string> GenerateInsuranceTariffCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.InsuranceTariffCode.StartsWith(InsuranceTariffCodePrefix))
                .Select(x => x.InsuranceTariffCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractInsuranceTariffSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{InsuranceTariffCodePrefix}{nextNumber.ToString().PadLeft(InsuranceTariffCodeDigitLength, '0')}";
        }

        private static int? TryExtractInsuranceTariffSequenceNumber(string insuranceTariffCode)
        {
            if (string.IsNullOrWhiteSpace(insuranceTariffCode))
                return null;

            if (!insuranceTariffCode.StartsWith(InsuranceTariffCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = insuranceTariffCode[InsuranceTariffCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstInsuranceTariff> ApplyDateFilter(
            IQueryable<MstInsuranceTariff> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var now = AppDateTimeHelper.OperationalDate();

            if (!string.IsNullOrWhiteSpace(customPeriod))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        startDate = now;
                        endDate = now;
                        break;

                    case "last7days":
                        startDate = now.AddDays(-6);
                        endDate = now;
                        break;

                    case "thismonth":
                        startDate = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        endDate = startDate.Value.AddMonths(1).AddDays(-1);
                        break;

                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                        startDate = firstDayThisMonth.AddMonths(-1);
                        endDate = firstDayThisMonth.AddDays(-1);
                        break;
                }
            }

            if (startDate.HasValue)
            {
                var start = startDate.Value.Date;
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = endDate.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(x => x.CreateDateTime <= end);
            }

            return query;
        }

        private static IQueryable<MstInsuranceTariff> ApplySorting(
            IQueryable<MstInsuranceTariff> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var normalizedSortBy = (sortBy ?? "sortOrder").Trim().ToLowerInvariant();

            return normalizedSortBy switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "insuranceprovidername" => isDesc
                    ? query.OrderByDescending(x => x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderName
                        : string.Empty)
                    : query.OrderBy(x => x.InsuranceProvider != null
                        ? x.InsuranceProvider.InsuranceProviderName
                        : string.Empty),

                "insurancetariffcode" => isDesc
                    ? query.OrderByDescending(x => x.InsuranceTariffCode)
                    : query.OrderBy(x => x.InsuranceTariffCode),

                "insurancetariffname" => isDesc
                    ? query.OrderByDescending(x => x.InsuranceTariffName)
                    : query.OrderBy(x => x.InsuranceTariffName),

                "tariffcategoryname" => isDesc
                    ? query.OrderByDescending(x => x.Tariff != null && x.Tariff.TariffCategory != null
                        ? x.Tariff.TariffCategory.TariffCategoryName
                        : string.Empty)
                    : query.OrderBy(x => x.Tariff != null && x.Tariff.TariffCategory != null
                        ? x.Tariff.TariffCategory.TariffCategoryName
                        : string.Empty),

                "patientclassname" => isDesc
                    ? query.OrderByDescending(x => x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : string.Empty)
                    : query.OrderBy(x => x.PatientClass != null
                        ? x.PatientClass.PatientClassName
                        : string.Empty),

                "benefitplancode" => isDesc
                    ? query.OrderByDescending(x => x.BenefitPlanCode)
                    : query.OrderBy(x => x.BenefitPlanCode),

                "contractprice" => isDesc
                    ? query.OrderByDescending(x => x.ContractPrice)
                    : query.OrderBy(x => x.ContractPrice),

                "effectivestartdate" => isDesc
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),

                "effectiveenddate" => isDesc
                    ? query.OrderByDescending(x => x.EffectiveEndDate)
                    : query.OrderBy(x => x.EffectiveEndDate),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.InsuranceTariffName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.InsuranceTariffName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdClaim, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
