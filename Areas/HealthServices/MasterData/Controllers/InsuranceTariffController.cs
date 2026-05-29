using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
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
        SortOrder = 27
    )]
    [Tags("Health Services / Master Data / Insurance Tariff")]
    public class InsuranceTariffController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";

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
                SortOptions = new List<InsuranceTariffSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "insuranceProviderName", Label = "Provider asuransi" },
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
            var now = DateTime.UtcNow.Date;

            var query = _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new InsuranceTariffSummaryResponse
            {
                TotalInsuranceTariff = await query.CountAsync(),
                ActiveInsuranceTariff = await query.CountAsync(x => x.IsActive),
                InactiveInsuranceTariff = await query.CountAsync(x => !x.IsActive),
                DrugTariff = await query.CountAsync(x => x.IsDrug),
                ConsumableTariff = await query.CountAsync(x => x.IsConsumable),
                ProcedureTariff = await query.CountAsync(x => x.IsProcedure),
                LaboratoryTariff = await query.CountAsync(x => x.IsLaboratory),
                RadiologyTariff = await query.CountAsync(x => x.IsRadiology),
                RoomChargeTariff = await query.CountAsync(x => x.IsRoomCharge),
                NeedApprovalTariff = await query.CountAsync(x => x.IsNeedApproval),
                EffectiveTariff = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= now) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= now)),
                ExpiredTariff = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < now)
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
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] Guid? tariffId,
            [FromQuery] Guid? drugId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] string? benefitPlanCode,
            [FromQuery] string? providerName,
            [FromQuery] bool? isUsingContractPrice,
            [FromQuery] bool? isSurgeryRelated,
            [FromQuery] bool? isRoomCharge,
            [FromQuery] bool? isDrug,
            [FromQuery] bool? isConsumable,
            [FromQuery] bool? isProcedure,
            [FromQuery] bool? isLaboratory,
            [FromQuery] bool? isRadiology,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] decimal? minimumPrice,
            [FromQuery] decimal? maximumPrice,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyFilters(
                query,
                search,
                isActive,
                insuranceProviderId,
                tariffId,
                drugId,
                procedureId,
                tariffCategoryId,
                patientClassId,
                benefitPlanCode,
                providerName,
                isUsingContractPrice,
                isSurgeryRelated,
                isRoomCharge,
                isDrug,
                isConsumable,
                isProcedure,
                isLaboratory,
                isRadiology,
                isNeedApproval,
                effectiveDate,
                minimumPrice,
                maximumPrice
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InsuranceTariffResponse
                {
                    Id = x.Id,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderCode = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderCode : string.Empty,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null ? x.Tariff.TariffCode : null,
                    TariffName = x.Tariff != null ? x.Tariff.TariffName : null,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : null,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : null,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassNameFromMaster = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    InsuranceTariffCode = x.InsuranceTariffCode,
                    InsuranceTariffName = x.InsuranceTariffName,
                    ExternalServiceCode = x.ExternalServiceCode,
                    ExternalClassCode = x.ExternalClassCode,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    PatientClassName = x.PatientClassName,
                    ProviderName = x.ProviderName,
                    ContractPrice = x.ContractPrice,
                    HospitalPriceSnapshot = x.HospitalPriceSnapshot,
                    DiscountAmount = x.DiscountAmount,
                    DiscountPercent = x.DiscountPercent,
                    IsUsingContractPrice = x.IsUsingContractPrice,
                    IsSurgeryRelated = x.IsSurgeryRelated,
                    IsRoomCharge = x.IsRoomCharge,
                    IsDrug = x.IsDrug,
                    IsConsumable = x.IsConsumable,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsNeedApproval = x.IsNeedApproval,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
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
        [ProducesResponseType(typeof(ApiResponse<List<InsuranceTariffOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Tariff", Description = "Melihat data insurance tariff", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceTariff", "Read")]
        public async Task<IActionResult> GetInsuranceTariffOptions(
            [FromQuery] Guid? insuranceProviderId,
            [FromQuery] Guid? tariffId,
            [FromQuery] Guid? drugId,
            [FromQuery] Guid? procedureId,
            [FromQuery] Guid? tariffCategoryId,
            [FromQuery] Guid? patientClassId,
            [FromQuery] string? benefitPlanCode,
            [FromQuery] DateTime? effectiveDate,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            query = ApplyFilters(
                query,
                search,
                null,
                insuranceProviderId,
                tariffId,
                drugId,
                procedureId,
                tariffCategoryId,
                patientClassId,
                benefitPlanCode,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                effectiveDate,
                null,
                null
            );

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.InsuranceTariffName)
                .Select(x => new InsuranceTariffOptionResponse
                {
                    Id = x.Id,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    InsuranceTariffCode = x.InsuranceTariffCode,
                    InsuranceTariffName = x.InsuranceTariffName,
                    TariffId = x.TariffId,
                    TariffName = x.Tariff != null ? x.Tariff.TariffName : null,
                    DrugId = x.DrugId,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassName = x.PatientClass != null ? x.PatientClass.PatientClassName : x.PatientClassName,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    ContractPrice = x.ContractPrice,
                    IsNeedApproval = x.IsNeedApproval
                })
                .ToListAsync();

            return Ok(ApiResponse<List<InsuranceTariffOptionResponse>>.Ok(
                data,
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
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new InsuranceTariffDetailResponse
                {
                    Id = x.Id,
                    InsuranceProviderId = x.InsuranceProviderId,
                    InsuranceProviderCode = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderCode : string.Empty,
                    InsuranceProviderName = x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty,
                    TariffId = x.TariffId,
                    TariffCode = x.Tariff != null ? x.Tariff.TariffCode : null,
                    TariffName = x.Tariff != null ? x.Tariff.TariffName : null,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : null,
                    DrugName = x.Drug != null ? x.Drug.DrugName : null,
                    ProcedureId = x.ProcedureId,
                    ProcedureCode = x.Procedure != null ? x.Procedure.ProcedureCode : null,
                    ProcedureName = x.Procedure != null ? x.Procedure.ProcedureName : null,
                    TariffCategoryId = x.TariffCategoryId,
                    TariffCategoryCode = x.TariffCategory != null ? x.TariffCategory.TariffCategoryCode : null,
                    TariffCategoryName = x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : null,
                    PatientClassId = x.PatientClassId,
                    PatientClassCode = x.PatientClass != null ? x.PatientClass.PatientClassCode : null,
                    PatientClassNameFromMaster = x.PatientClass != null ? x.PatientClass.PatientClassName : null,
                    InsuranceTariffCode = x.InsuranceTariffCode,
                    InsuranceTariffName = x.InsuranceTariffName,
                    ExternalServiceCode = x.ExternalServiceCode,
                    ExternalClassCode = x.ExternalClassCode,
                    BenefitPlanCode = x.BenefitPlanCode,
                    BenefitPlanName = x.BenefitPlanName,
                    PatientClassName = x.PatientClassName,
                    ProviderName = x.ProviderName,
                    ContractPrice = x.ContractPrice,
                    HospitalPriceSnapshot = x.HospitalPriceSnapshot,
                    DiscountAmount = x.DiscountAmount,
                    DiscountPercent = x.DiscountPercent,
                    IsUsingContractPrice = x.IsUsingContractPrice,
                    IsSurgeryRelated = x.IsSurgeryRelated,
                    IsRoomCharge = x.IsRoomCharge,
                    IsDrug = x.IsDrug,
                    IsConsumable = x.IsConsumable,
                    IsProcedure = x.IsProcedure,
                    IsLaboratory = x.IsLaboratory,
                    IsRadiology = x.IsRadiology,
                    IsNeedApproval = x.IsNeedApproval,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    SortOrder = x.SortOrder,
                    BillingInstruction = x.BillingInstruction,
                    ClaimInstruction = x.ClaimInstruction,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
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

            var entity = new MstInsuranceTariff
            {
                Id = Guid.NewGuid(),
                InsuranceProviderId = request.InsuranceProviderId,
                TariffId = NormalizeNullableGuid(request.TariffId),
                DrugId = NormalizeNullableGuid(request.DrugId),
                ProcedureId = NormalizeNullableGuid(request.ProcedureId),
                TariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId),
                PatientClassId = NormalizeNullableGuid(request.PatientClassId),
                InsuranceTariffCode = request.InsuranceTariffCode.Trim().ToUpperInvariant(),
                InsuranceTariffName = request.InsuranceTariffName.Trim(),
                ExternalServiceCode = NormalizeNullableString(request.ExternalServiceCode),
                ExternalClassCode = NormalizeNullableString(request.ExternalClassCode),
                BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant(),
                BenefitPlanName = NormalizeNullableString(request.BenefitPlanName),
                PatientClassName = NormalizeNullableString(request.PatientClassName),
                ProviderName = NormalizeNullableString(request.ProviderName),
                ContractPrice = request.ContractPrice,
                HospitalPriceSnapshot = request.HospitalPriceSnapshot,
                DiscountAmount = request.DiscountAmount,
                DiscountPercent = request.DiscountPercent,
                IsUsingContractPrice = request.IsUsingContractPrice,
                IsSurgeryRelated = request.IsSurgeryRelated,
                IsRoomCharge = request.IsRoomCharge,
                IsDrug = request.IsDrug,
                IsConsumable = request.IsConsumable,
                IsProcedure = request.IsProcedure,
                IsLaboratory = request.IsLaboratory,
                IsRadiology = request.IsRadiology,
                IsNeedApproval = request.IsNeedApproval,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                SortOrder = request.SortOrder,
                BillingInstruction = NormalizeNullableString(request.BillingInstruction),
                ClaimInstruction = NormalizeNullableString(request.ClaimInstruction),
                Description = NormalizeNullableString(request.Description),
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                UpdateBy = Guid.Empty,
                DeleteBy = Guid.Empty,
                CancelBy = Guid.Empty,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstInsuranceTariff>().Add(entity);
            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceTariff.Create",
                "Membuat data insurance tariff.",
                entity
            );

            var result = new InsuranceTariffCreateResponse
            {
                Id = entity.Id,
                InsuranceProviderId = entity.InsuranceProviderId,
                TariffId = entity.TariffId,
                DrugId = entity.DrugId,
                ProcedureId = entity.ProcedureId,
                TariffCategoryId = entity.TariffCategoryId,
                PatientClassId = entity.PatientClassId,
                InsuranceTariffCode = entity.InsuranceTariffCode,
                InsuranceTariffName = entity.InsuranceTariffName,
                ContractPrice = entity.ContractPrice,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<InsuranceTariffCreateResponse>.Ok(
                result,
                "Insurance tariff berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceTariffDetailResponse>), StatusCodes.Status200OK)]
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
            entity.TariffId = NormalizeNullableGuid(request.TariffId);
            entity.DrugId = NormalizeNullableGuid(request.DrugId);
            entity.ProcedureId = NormalizeNullableGuid(request.ProcedureId);
            entity.TariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId);
            entity.PatientClassId = NormalizeNullableGuid(request.PatientClassId);
            entity.InsuranceTariffCode = request.InsuranceTariffCode.Trim().ToUpperInvariant();
            entity.InsuranceTariffName = request.InsuranceTariffName.Trim();
            entity.ExternalServiceCode = NormalizeNullableString(request.ExternalServiceCode);
            entity.ExternalClassCode = NormalizeNullableString(request.ExternalClassCode);
            entity.BenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant();
            entity.BenefitPlanName = NormalizeNullableString(request.BenefitPlanName);
            entity.PatientClassName = NormalizeNullableString(request.PatientClassName);
            entity.ProviderName = NormalizeNullableString(request.ProviderName);
            entity.ContractPrice = request.ContractPrice;
            entity.HospitalPriceSnapshot = request.HospitalPriceSnapshot;
            entity.DiscountAmount = request.DiscountAmount;
            entity.DiscountPercent = request.DiscountPercent;
            entity.IsUsingContractPrice = request.IsUsingContractPrice;
            entity.IsSurgeryRelated = request.IsSurgeryRelated;
            entity.IsRoomCharge = request.IsRoomCharge;
            entity.IsDrug = request.IsDrug;
            entity.IsConsumable = request.IsConsumable;
            entity.IsProcedure = request.IsProcedure;
            entity.IsLaboratory = request.IsLaboratory;
            entity.IsRadiology = request.IsRadiology;
            entity.IsNeedApproval = request.IsNeedApproval;
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

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceTariff.Update",
                "Mengubah data insurance tariff.",
                entity
            );

            return await GetInsuranceTariffById(entity.Id);
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Tariff Status", Description = "Mengubah status insurance tariff", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("InsuranceTariff", "Update")]
        public async Task<IActionResult> UpdateInsuranceTariffStatus(Guid id, [FromBody] InsuranceTariffStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id, entity.IsActive },
                request.IsActive
                    ? "Insurance tariff berhasil diaktifkan."
                    : "Insurance tariff berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Insurance Tariff", Description = "Menghapus data insurance tariff", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("InsuranceTariff", "Delete")]
        public async Task<IActionResult> DeleteInsuranceTariff(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { entity.Id },
                "Insurance tariff berhasil dihapus."
            ));
        }

        [HttpPost("bulk-delete")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Delete", "Bulk Delete Insurance Tariff", Description = "Menghapus banyak data insurance tariff", AccessType = AccessTypes.Delete, SortOrder = 6)]
        [AccessPermission("InsuranceTariff", "Delete")]
        public async Task<IActionResult> BulkDeleteInsuranceTariff([FromBody] BulkDeleteInsuranceTariffRequest request)
        {
            if (request.Ids == null || request.Ids.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Minimal pilih satu insurance tariff."
                ));
            }

            var ids = request.Ids.Distinct().ToList();

            var entities = await _dbContext.Set<MstInsuranceTariff>()
                .Where(x => ids.Contains(x.Id) && !x.IsDelete)
                .ToListAsync();

            if (entities.Count == 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Data insurance tariff tidak ditemukan atau sudah dihapus."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            foreach (var entity in entities)
            {
                entity.IsDelete = true;
                entity.IsActive = false;
                entity.DeleteDateTime = now;
                entity.DeleteBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                new { DeletedCount = entities.Count },
                $"{entities.Count} insurance tariff berhasil dihapus."
            ));
        }

        private IQueryable<MstInsuranceTariff> BuildBaseQuery()
        {
            return _dbContext.Set<MstInsuranceTariff>()
                .AsNoTracking()
                .Include(x => x.InsuranceProvider)
                .Include(x => x.Tariff)
                .Include(x => x.Drug)
                .Include(x => x.Procedure)
                .Include(x => x.TariffCategory)
                .Include(x => x.PatientClass)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstInsuranceTariff> ApplyFilters(
            IQueryable<MstInsuranceTariff> query,
            string? search,
            bool? isActive,
            Guid? insuranceProviderId,
            Guid? tariffId,
            Guid? drugId,
            Guid? procedureId,
            Guid? tariffCategoryId,
            Guid? patientClassId,
            string? benefitPlanCode,
            string? providerName,
            bool? isUsingContractPrice,
            bool? isSurgeryRelated,
            bool? isRoomCharge,
            bool? isDrug,
            bool? isConsumable,
            bool? isProcedure,
            bool? isLaboratory,
            bool? isRadiology,
            bool? isNeedApproval,
            DateTime? effectiveDate,
            decimal? minimumPrice,
            decimal? maximumPrice)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.InsuranceTariffCode.ToLower().Contains(keyword) ||
                    x.InsuranceTariffName.ToLower().Contains(keyword) ||
                    (x.ExternalServiceCode != null && x.ExternalServiceCode.ToLower().Contains(keyword)) ||
                    (x.ExternalClassCode != null && x.ExternalClassCode.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(keyword)) ||
                    (x.BenefitPlanName != null && x.BenefitPlanName.ToLower().Contains(keyword)) ||
                    (x.PatientClassName != null && x.PatientClassName.ToLower().Contains(keyword)) ||
                    (x.ProviderName != null && x.ProviderName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderCode.ToLower().Contains(keyword)) ||
                    (x.InsuranceProvider != null && x.InsuranceProvider.InsuranceProviderName.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffCode.ToLower().Contains(keyword)) ||
                    (x.Tariff != null && x.Tariff.TariffName.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                    (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureCode.ToLower().Contains(keyword)) ||
                    (x.Procedure != null && x.Procedure.ProcedureName.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryCode.ToLower().Contains(keyword)) ||
                    (x.TariffCategory != null && x.TariffCategory.TariffCategoryName.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassCode.ToLower().Contains(keyword)) ||
                    (x.PatientClass != null && x.PatientClass.PatientClassName.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (insuranceProviderId.HasValue && insuranceProviderId.Value != Guid.Empty)
                query = query.Where(x => x.InsuranceProviderId == insuranceProviderId.Value);

            if (tariffId.HasValue && tariffId.Value != Guid.Empty)
                query = query.Where(x => x.TariffId == tariffId.Value);

            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (procedureId.HasValue && procedureId.Value != Guid.Empty)
                query = query.Where(x => x.ProcedureId == procedureId.Value);

            if (tariffCategoryId.HasValue && tariffCategoryId.Value != Guid.Empty)
                query = query.Where(x => x.TariffCategoryId == tariffCategoryId.Value);

            if (patientClassId.HasValue && patientClassId.Value != Guid.Empty)
                query = query.Where(x => x.PatientClassId == patientClassId.Value);

            if (!string.IsNullOrWhiteSpace(benefitPlanCode))
            {
                var normalizedBenefitPlanCode = benefitPlanCode.Trim().ToLower();
                query = query.Where(x => x.BenefitPlanCode != null && x.BenefitPlanCode.ToLower().Contains(normalizedBenefitPlanCode));
            }

            if (!string.IsNullOrWhiteSpace(providerName))
            {
                var normalizedProviderName = providerName.Trim().ToLower();
                query = query.Where(x => x.ProviderName != null && x.ProviderName.ToLower().Contains(normalizedProviderName));
            }

            if (isUsingContractPrice.HasValue)
                query = query.Where(x => x.IsUsingContractPrice == isUsingContractPrice.Value);

            if (isSurgeryRelated.HasValue)
                query = query.Where(x => x.IsSurgeryRelated == isSurgeryRelated.Value);

            if (isRoomCharge.HasValue)
                query = query.Where(x => x.IsRoomCharge == isRoomCharge.Value);

            if (isDrug.HasValue)
                query = query.Where(x => x.IsDrug == isDrug.Value);

            if (isConsumable.HasValue)
                query = query.Where(x => x.IsConsumable == isConsumable.Value);

            if (isProcedure.HasValue)
                query = query.Where(x => x.IsProcedure == isProcedure.Value);

            if (isLaboratory.HasValue)
                query = query.Where(x => x.IsLaboratory == isLaboratory.Value);

            if (isRadiology.HasValue)
                query = query.Where(x => x.IsRadiology == isRadiology.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (effectiveDate.HasValue)
            {
                var selectedDate = effectiveDate.Value.Date;

                query = query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= selectedDate) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= selectedDate));
            }

            if (minimumPrice.HasValue)
                query = query.Where(x => x.ContractPrice >= minimumPrice.Value);

            if (maximumPrice.HasValue)
                query = query.Where(x => x.ContractPrice <= maximumPrice.Value);

            return query;
        }

        private static IQueryable<MstInsuranceTariff> ApplySorting(
            IQueryable<MstInsuranceTariff> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            var normalizedSortBy = (sortBy ?? "sortOrder").Trim().ToLowerInvariant();

            return normalizedSortBy switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),
                "insuranceprovidername" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty)
                    : query.OrderBy(x => x.InsuranceProvider != null ? x.InsuranceProvider.InsuranceProviderName : string.Empty),
                "insurancetariffcode" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceTariffCode)
                    : query.OrderBy(x => x.InsuranceTariffCode),
                "insurancetariffname" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceTariffName)
                    : query.OrderBy(x => x.InsuranceTariffName),
                "tariffcategoryname" => isDescending
                    ? query.OrderByDescending(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : string.Empty)
                    : query.OrderBy(x => x.TariffCategory != null ? x.TariffCategory.TariffCategoryName : string.Empty),
                "patientclassname" => isDescending
                    ? query.OrderByDescending(x => x.PatientClass != null ? x.PatientClass.PatientClassName : x.PatientClassName)
                    : query.OrderBy(x => x.PatientClass != null ? x.PatientClass.PatientClassName : x.PatientClassName),
                "benefitplancode" => isDescending
                    ? query.OrderByDescending(x => x.BenefitPlanCode)
                    : query.OrderBy(x => x.BenefitPlanCode),
                "contractprice" => isDescending
                    ? query.OrderByDescending(x => x.ContractPrice)
                    : query.OrderBy(x => x.ContractPrice),
                "effectivestartdate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveStartDate)
                    : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDescending
                    ? query.OrderByDescending(x => x.EffectiveEndDate)
                    : query.OrderBy(x => x.EffectiveEndDate),
                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),
                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.InsuranceTariffName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.InsuranceTariffName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateInsuranceTariffRequest request)
        {
            if (request.InsuranceProviderId == Guid.Empty)
                return (false, "Insurance provider wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.InsuranceTariffCode))
                return (false, "Kode insurance tariff wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.InsuranceTariffName))
                return (false, "Nama insurance tariff wajib diisi.");

            if (request.ContractPrice < 0)
                return (false, "Harga kontrak tidak boleh kurang dari 0.");

            if (request.HospitalPriceSnapshot.HasValue && request.HospitalPriceSnapshot.Value < 0)
                return (false, "Snapshot harga rumah sakit tidak boleh kurang dari 0.");

            if (request.DiscountAmount.HasValue && request.DiscountAmount.Value < 0)
                return (false, "Discount amount tidak boleh kurang dari 0.");

            if (request.DiscountPercent.HasValue && (request.DiscountPercent.Value < 0 || request.DiscountPercent.Value > 100))
                return (false, "Discount percent harus berada di antara 0 sampai 100.");

            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue &&
                request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
            {
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");
            }

            var normalizedTariffId = NormalizeNullableGuid(request.TariffId);
            var normalizedDrugId = NormalizeNullableGuid(request.DrugId);
            var normalizedProcedureId = NormalizeNullableGuid(request.ProcedureId);
            var normalizedTariffCategoryId = NormalizeNullableGuid(request.TariffCategoryId);
            var normalizedPatientClassId = NormalizeNullableGuid(request.PatientClassId);

            var selectedItemCount = new[]
            {
                normalizedTariffId,
                normalizedDrugId,
                normalizedProcedureId,
                normalizedTariffCategoryId
            }.Count(x => x.HasValue);

            if (selectedItemCount == 0)
                return (false, "Minimal hubungkan insurance tariff ke tariff, drug, procedure, atau tariff category.");

            if (selectedItemCount > 1)
                return (false, "Insurance tariff hanya boleh terhubung ke salah satu: tariff, drug, procedure, atau tariff category.");

            var providerExists = await _dbContext.Set<MstInsuranceProvider>()
                .AnyAsync(x => x.Id == request.InsuranceProviderId && x.IsActive && !x.IsDelete);

            if (!providerExists)
                return (false, "Insurance provider tidak valid atau tidak aktif.");

            if (normalizedTariffId.HasValue)
            {
                var tariffExists = await _dbContext.Set<MstTariff>()
                    .AnyAsync(x => x.Id == normalizedTariffId.Value && x.IsActive && !x.IsDelete);

                if (!tariffExists)
                    return (false, "Tariff tidak valid atau tidak aktif.");
            }

            if (normalizedDrugId.HasValue)
            {
                var drugExists = await _dbContext.Set<MstDrug>()
                    .AnyAsync(x => x.Id == normalizedDrugId.Value && x.IsActive && !x.IsDelete);

                if (!drugExists)
                    return (false, "Drug tidak valid atau tidak aktif.");
            }

            if (normalizedProcedureId.HasValue)
            {
                var procedureExists = await _dbContext.Set<MstProcedure>()
                    .AnyAsync(x => x.Id == normalizedProcedureId.Value && x.IsActive && !x.IsDelete);

                if (!procedureExists)
                    return (false, "Procedure tidak valid atau tidak aktif.");
            }

            if (normalizedTariffCategoryId.HasValue)
            {
                var tariffCategoryExists = await _dbContext.Set<MstTariffCategory>()
                    .AnyAsync(x => x.Id == normalizedTariffCategoryId.Value && x.IsActive && !x.IsDelete);

                if (!tariffCategoryExists)
                    return (false, "Tariff category tidak valid atau tidak aktif.");
            }

            if (normalizedPatientClassId.HasValue)
            {
                var patientClassExists = await _dbContext.Set<MstPatientClass>()
                    .AnyAsync(x => x.Id == normalizedPatientClassId.Value && x.IsActive && !x.IsDelete);

                if (!patientClassExists)
                    return (false, "Patient class tidak valid atau tidak aktif.");
            }

            var normalizedCode = request.InsuranceTariffCode.Trim().ToUpperInvariant();
            var normalizedExternalClassCode = NormalizeNullableString(request.ExternalClassCode);
            var normalizedBenefitPlanCode = NormalizeNullableString(request.BenefitPlanCode)?.ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstInsuranceTariff>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderId == request.InsuranceProviderId &&
                    x.InsuranceTariffCode == normalizedCode &&
                    x.ExternalClassCode == normalizedExternalClassCode &&
                    x.BenefitPlanCode == normalizedBenefitPlanCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode insurance tariff dengan provider, kelas eksternal, dan benefit plan tersebut sudah digunakan.");

            var normalizedName = request.InsuranceTariffName.Trim().ToLowerInvariant();

            var duplicateNameInScope = await _dbContext.Set<MstInsuranceTariff>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderId == request.InsuranceProviderId &&
                    x.TariffId == normalizedTariffId &&
                    x.DrugId == normalizedDrugId &&
                    x.ProcedureId == normalizedProcedureId &&
                    x.TariffCategoryId == normalizedTariffCategoryId &&
                    x.PatientClassId == normalizedPatientClassId &&
                    x.BenefitPlanCode == normalizedBenefitPlanCode &&
                    x.InsuranceTariffName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateNameInScope)
                return (false, "Nama insurance tariff pada scope provider, item, kelas pasien, dan benefit plan tersebut sudah digunakan.");

            return (true, null);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private static string? NormalizeNullableString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return value.Trim();
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1)
                pageNumber = 1;

            if (pageSize < 1)
                pageSize = 25;

            if (pageSize > 100)
                pageSize = 100;

            return (pageNumber, pageSize);
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
