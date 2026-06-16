using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Globalization;
using System.Security.Claims;

using ResponseInsuranceProviderPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.InsuranceProviderResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/insurance-providers")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Insurance Provider",
        AreaName = "Administrator",
        ControllerName = "InsuranceProvider",
        Description = "Administrator master data insurance provider",
        SortOrder = 13
    )]
    [Tags("Administrator / Master Data / Insurance Provider")]
    public class InsuranceProviderController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string KioskReadPolicy = "KioskRead";
        private const string InsuranceProviderCodePrefix = "IP-RSMMC-";
        private const int InsuranceProviderCodeDigitLength = 5;

        private const string ContractStatusActive = "active";
        private const string ContractStatusExpired = "expired";
        private const string ContractStatusUpcoming = "upcoming";
        private const string ContractStatusNoContract = "noContract";

        private static readonly List<string> AllowedProviderTypes = new()
        {
            "PrivateInsurance",
            "TPA",
            "GovernmentInsurance",
            "CorporateInsurance",
            "Other"
        };

        private static readonly List<string> AllowedClaimMethods = new()
        {
            "Cashless",
            "Reimbursement",
            "GuaranteeLetter",
            "Mixed"
        };

        private static readonly List<string> AllowedContractStatuses = new()
        {
            ContractStatusActive,
            ContractStatusExpired,
            ContractStatusUpcoming,
            ContractStatusNoContract
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public InsuranceProviderController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat metadata filter insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new InsuranceProviderFilterMetadataResponse
            {
                DefaultFilter = new InsuranceProviderDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<InsuranceProviderSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "insuranceProviderCode", Label = "Kode insurance provider" },
                    new() { Value = "insuranceProviderName", Label = "Nama insurance provider" },
                    new() { Value = "insuranceGroupName", Label = "Group insurance" },
                    new() { Value = "providerType", Label = "Tipe provider" },
                    new() { Value = "claimMethod", Label = "Metode klaim" },
                    new() { Value = "contractStartDate", Label = "Tanggal mulai kontrak" },
                    new() { Value = "contractEndDate", Label = "Tanggal akhir kontrak" },
                    new() { Value = "isUsingInsuranceTariffBook", Label = "Pakai tarif insurance" },
                    new() { Value = "isNeedEligibilityCheck", Label = "Butuh eligibility check" },
                    new() { Value = "isNeedGuaranteeLetter", Label = "Butuh guarantee letter" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProviderTypeOptions = BuildProviderTypeOptions(),
                ClaimMethodOptions = BuildClaimMethodOptions(),
                ContractStatusOptions = BuildContractStatusOptions(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceProvider.GetFilterMetadata",
                "Mengambil metadata filter insurance provider.",
                result
            );

            return Ok(ApiResponse<InsuranceProviderFilterMetadataResponse>.Ok(
                result,
                "Metadata filter insurance provider berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat ringkasan insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var query = BuildBaseQuery();

            var result = new InsuranceProviderSummaryResponse
            {
                TotalInsuranceProvider = await query.CountAsync(),
                ActiveInsuranceProvider = await query.CountAsync(x => x.IsActive),
                InactiveInsuranceProvider = await query.CountAsync(x => !x.IsActive),
                PrivateInsuranceProvider = await query.CountAsync(x => x.ProviderType == "PrivateInsurance"),
                TpaProvider = await query.CountAsync(x => x.ProviderType == "TPA"),
                GovernmentInsuranceProvider = await query.CountAsync(x => x.ProviderType == "GovernmentInsurance"),
                CorporateInsuranceProvider = await query.CountAsync(x => x.ProviderType == "CorporateInsurance"),
                OtherInsuranceProvider = await query.CountAsync(x => x.ProviderType == "Other"),
                CashlessProvider = await query.CountAsync(x => x.ClaimMethod == "Cashless"),
                ReimbursementProvider = await query.CountAsync(x => x.ClaimMethod == "Reimbursement"),
                GuaranteeLetterProvider = await query.CountAsync(x => x.ClaimMethod == "GuaranteeLetter"),
                MixedClaimProvider = await query.CountAsync(x => x.ClaimMethod == "Mixed"),
                NeedEligibilityCheckProvider = await query.CountAsync(x => x.IsNeedEligibilityCheck),
                NeedGuaranteeLetterProvider = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                NeedReferralLetterProvider = await query.CountAsync(x => x.IsNeedReferralLetter),
                NeedApprovalForProcedureProvider = await query.CountAsync(x => x.IsNeedApprovalForProcedure),
                NeedApprovalForDrugProvider = await query.CountAsync(x => x.IsNeedApprovalForDrug),
                UsingInsuranceTariffBookProvider = await query.CountAsync(x => x.IsUsingInsuranceTariffBook),
                UsingHospitalTariffProvider = await query.CountAsync(x => x.IsUsingHospitalTariff),
                CoverageLimitedByPlanProvider = await query.CountAsync(x => x.IsCoverageLimitedByPlan),
                AllowExcessPaymentByPatientProvider = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
                ActiveContractProvider = await query.CountAsync(x =>
                    (x.ContractStartDate.HasValue || x.ContractEndDate.HasValue) &&
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= today) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= today)),
                ExpiredContractProvider = await query.CountAsync(x =>
                    x.ContractEndDate.HasValue && x.ContractEndDate.Value.Date < today),
                UpcomingContractProvider = await query.CountAsync(x =>
                    x.ContractStartDate.HasValue && x.ContractStartDate.Value.Date > today),
                NoContractDateProvider = await query.CountAsync(x =>
                    !x.ContractStartDate.HasValue && !x.ContractEndDate.HasValue)
            };

            return Ok(ApiResponse<InsuranceProviderSummaryResponse>.Ok(
                result,
                "Ringkasan insurance provider berhasil diambil."
            ));
        }

        [HttpGet]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<ResponseInsuranceProviderPagedResult>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat data insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetInsuranceProviders(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? providerType,
            [FromQuery] string? claimMethod,
            [FromQuery] string? contractStatus,
            [FromQuery] bool? isUsingInsuranceTariffBook,
            [FromQuery] bool? isUsingHospitalTariff,
            [FromQuery] bool? isNeedEligibilityCheck,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedReferralLetter,
            [FromQuery] bool? isNeedApprovalForProcedure,
            [FromQuery] bool? isNeedApprovalForDrug,
            [FromQuery] bool? isCoverageLimitedByPlan,
            [FromQuery] bool? isAllowExcessPaymentByPatient,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                providerType,
                claimMethod,
                contractStatus,
                isUsingInsuranceTariffBook,
                isUsingHospitalTariff,
                isNeedEligibilityCheck,
                isNeedGuaranteeLetter,
                isNeedReferralLetter,
                isNeedApprovalForProcedure,
                isNeedApprovalForDrug,
                isCoverageLimitedByPlan,
                isAllowExcessPaymentByPatient
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorNames = await GetActorNameMapAsync(
                entities
                    .Select(x => x.CreateBy)
                    .Where(x => x != Guid.Empty)
            );

            var items = entities
                .Select(x => MapResponse(x, actorNames))
                .ToList();

            var result = new ResponseInsuranceProviderPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseInsuranceProviderPagedResult>.Ok(
                result,
                "Data insurance provider berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [Authorize(Policy = KioskReadPolicy)]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat data pilihan insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        public async Task<IActionResult> GetInsuranceProviderOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? providerType = null,
            [FromQuery] string? claimMethod = null,
            [FromQuery] string? contractStatus = null,
            [FromQuery] bool? isUsingInsuranceTariffBook = null,
            [FromQuery] bool? isUsingHospitalTariff = null,
            [FromQuery] bool? isNeedEligibilityCheck = null,
            [FromQuery] bool? isNeedGuaranteeLetter = null,
            [FromQuery] bool? isNeedReferralLetter = null,
            [FromQuery] bool? isNeedApprovalForProcedure = null,
            [FromQuery] bool? isNeedApprovalForDrug = null,
            [FromQuery] bool? isCoverageLimitedByPlan = null,
            [FromQuery] bool? isAllowExcessPaymentByPatient = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var useOnlyActive = activeOnly ?? onlyActive;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                search,
                useOnlyActive ? true : null,
                providerType,
                claimMethod,
                contractStatus,
                isUsingInsuranceTariffBook,
                isUsingHospitalTariff,
                isNeedEligibilityCheck,
                isNeedGuaranteeLetter,
                isNeedReferralLetter,
                isNeedApprovalForProcedure,
                isNeedApprovalForDrug,
                isCoverageLimitedByPlan,
                isAllowExcessPaymentByPatient
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.InsuranceProviderName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new InsuranceProviderOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<InsuranceProviderOptionPagedResponse>.Ok(
                result,
                "Data pilihan insurance provider berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat detail insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviderById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance provider tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<InsuranceProviderDetailResponse>.Ok(
                data,
                "Detail insurance provider berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Insurance Provider",
            Description = "Membuat data insurance provider",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("InsuranceProvider", "Create")]
        public async Task<IActionResult> CreateInsuranceProvider([FromBody] CreateInsuranceProviderRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance provider tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedInsuranceProviderCode = await GenerateInsuranceProviderCodeAsync();

            var entity = new MstInsuranceProvider
            {
                Id = Guid.NewGuid(),
                InsuranceProviderCode = generatedInsuranceProviderCode,
                InsuranceProviderName = request.InsuranceProviderName.Trim(),
                InsuranceGroupName = NormalizeNullableString(request.InsuranceGroupName),
                ProviderType = NormalizeProviderType(request.ProviderType),
                ClaimMethod = NormalizeClaimMethod(request.ClaimMethod),
                ExternalProviderCode = NormalizeUpperNullableString(request.ExternalProviderCode),
                IntegrationCode = NormalizeUpperNullableString(request.IntegrationCode),
                ContractNumber = NormalizeNullableString(request.ContractNumber),
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                IsUsingInsuranceTariffBook = request.IsUsingInsuranceTariffBook,
                IsUsingHospitalTariff = request.IsUsingHospitalTariff,
                IsNeedEligibilityCheck = request.IsNeedEligibilityCheck,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = request.IsNeedReferralLetter,
                IsNeedApprovalForProcedure = request.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = request.IsNeedApprovalForDrug,
                IsCoverageLimitedByPlan = request.IsCoverageLimitedByPlan,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                PicName = NormalizeNullableString(request.PicName),
                PicPhoneNumber = NormalizeNullableString(request.PicPhoneNumber),
                PicWhatsAppNumber = NormalizeNullableString(request.PicWhatsAppNumber),
                PicEmail = NormalizeLowerNullableString(request.PicEmail),
                OfficeAddress = NormalizeNullableString(request.OfficeAddress),
                LogoPath = NormalizeNullableString(request.LogoPath),
                BillingInstruction = NormalizeNullableString(request.BillingInstruction),
                ClaimInstruction = NormalizeNullableString(request.ClaimInstruction),
                Description = NormalizeNullableString(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstInsuranceProvider>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var result = new InsuranceProviderCreateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                ProviderType = entity.ProviderType,
                ProviderTypeName = BuildProviderTypeLabel(entity.ProviderType),
                ClaimMethod = entity.ClaimMethod,
                ClaimMethodName = BuildClaimMethodLabel(entity.ClaimMethod),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceProvider.CreateInsuranceProvider",
                "Membuat data insurance provider.",
                result
            );

            return Ok(ApiResponse<InsuranceProviderCreateResponse>.Ok(
                result,
                "Insurance provider berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Insurance Provider",
            Description = "Mengubah data insurance provider",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("InsuranceProvider", "Update")]
        public async Task<IActionResult> UpdateInsuranceProvider(
            Guid id,
            [FromBody] UpdateInsuranceProviderRequest request)
        {
            var entity = await _dbContext.Set<MstInsuranceProvider>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance provider tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance provider tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.InsuranceProviderName = request.InsuranceProviderName.Trim();
            entity.InsuranceGroupName = NormalizeNullableString(request.InsuranceGroupName);
            entity.ProviderType = NormalizeProviderType(request.ProviderType);
            entity.ClaimMethod = NormalizeClaimMethod(request.ClaimMethod);
            entity.ExternalProviderCode = NormalizeUpperNullableString(request.ExternalProviderCode);
            entity.IntegrationCode = NormalizeUpperNullableString(request.IntegrationCode);
            entity.ContractNumber = NormalizeNullableString(request.ContractNumber);
            entity.ContractStartDate = request.ContractStartDate;
            entity.ContractEndDate = request.ContractEndDate;
            entity.IsUsingInsuranceTariffBook = request.IsUsingInsuranceTariffBook;
            entity.IsUsingHospitalTariff = request.IsUsingHospitalTariff;
            entity.IsNeedEligibilityCheck = request.IsNeedEligibilityCheck;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsNeedReferralLetter = request.IsNeedReferralLetter;
            entity.IsNeedApprovalForProcedure = request.IsNeedApprovalForProcedure;
            entity.IsNeedApprovalForDrug = request.IsNeedApprovalForDrug;
            entity.IsCoverageLimitedByPlan = request.IsCoverageLimitedByPlan;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
            entity.PicName = NormalizeNullableString(request.PicName);
            entity.PicPhoneNumber = NormalizeNullableString(request.PicPhoneNumber);
            entity.PicWhatsAppNumber = NormalizeNullableString(request.PicWhatsAppNumber);
            entity.PicEmail = NormalizeLowerNullableString(request.PicEmail);
            entity.OfficeAddress = NormalizeNullableString(request.OfficeAddress);
            entity.LogoPath = NormalizeNullableString(request.LogoPath);
            entity.BillingInstruction = NormalizeNullableString(request.BillingInstruction);
            entity.ClaimInstruction = NormalizeNullableString(request.ClaimInstruction);
            entity.Description = NormalizeNullableString(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new InsuranceProviderUpdateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                ProviderType = entity.ProviderType,
                ProviderTypeName = BuildProviderTypeLabel(entity.ProviderType),
                ClaimMethod = entity.ClaimMethod,
                ClaimMethodName = BuildClaimMethodLabel(entity.ClaimMethod),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceProvider.UpdateInsuranceProvider",
                "Mengubah data insurance provider.",
                result
            );

            return Ok(ApiResponse<InsuranceProviderUpdateResponse>.Ok(
                result,
                "Insurance provider berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Insurance Provider Status",
            Description = "Mengubah status insurance provider",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("InsuranceProvider", "Update")]
        public async Task<IActionResult> UpdateInsuranceProviderStatus(
            Guid id,
            [FromBody] UpdateInsuranceProviderStatusRequest request)
        {
            var entity = await _dbContext.Set<MstInsuranceProvider>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance provider tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new InsuranceProviderUpdateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                ProviderType = entity.ProviderType,
                ProviderTypeName = BuildProviderTypeLabel(entity.ProviderType),
                ClaimMethod = entity.ClaimMethod,
                ClaimMethodName = BuildClaimMethodLabel(entity.ClaimMethod),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            return Ok(ApiResponse<InsuranceProviderUpdateResponse>.Ok(
                result,
                "Status insurance provider berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Insurance Provider",
            Description = "Menghapus data insurance provider",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("InsuranceProvider", "Delete")]
        public async Task<IActionResult> DeleteInsuranceProvider(
            Guid id,
            [FromBody] DeleteInsuranceProviderRequest? request = null)
        {
            var entity = await _dbContext.Set<MstInsuranceProvider>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance provider tidak ditemukan."
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
            {
                entity.Description = request.DeleteReason.Trim();
            }

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });

            var result = new InsuranceProviderDeleteResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "InsuranceProvider.DeleteInsuranceProvider",
                "Menghapus data insurance provider.",
                result
            );

            return Ok(ApiResponse<InsuranceProviderDeleteResponse>.Ok(
                result,
                "Insurance provider berhasil dihapus."
            ));
        }

        private IQueryable<MstInsuranceProvider> BuildBaseQuery()
        {
            return _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstInsuranceProvider> ApplyDateFilter(
            IQueryable<MstInsuranceProvider> query,
            DateRangeResolveResult dateRange)
        {
            if (dateRange.Start.HasValue)
            {
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);
            }

            if (dateRange.EndExclusive.HasValue)
            {
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);
            }

            return query;
        }

        private static IQueryable<MstInsuranceProvider> ApplyStandardFilter(
            IQueryable<MstInsuranceProvider> query,
            string? search,
            bool? isActive,
            string? providerType,
            string? claimMethod,
            string? contractStatus,
            bool? isUsingInsuranceTariffBook,
            bool? isUsingHospitalTariff,
            bool? isNeedEligibilityCheck,
            bool? isNeedGuaranteeLetter,
            bool? isNeedReferralLetter,
            bool? isNeedApprovalForProcedure,
            bool? isNeedApprovalForDrug,
            bool? isCoverageLimitedByPlan,
            bool? isAllowExcessPaymentByPatient)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(providerType))
            {
                var normalizedProviderType = NormalizeProviderType(providerType);
                query = query.Where(x => x.ProviderType == normalizedProviderType);
            }

            if (!string.IsNullOrWhiteSpace(claimMethod))
            {
                var normalizedClaimMethod = NormalizeClaimMethod(claimMethod);
                query = query.Where(x => x.ClaimMethod == normalizedClaimMethod);
            }

            query = ApplyContractStatusFilter(query, contractStatus);

            if (isUsingInsuranceTariffBook.HasValue)
            {
                query = query.Where(x => x.IsUsingInsuranceTariffBook == isUsingInsuranceTariffBook.Value);
            }

            if (isUsingHospitalTariff.HasValue)
            {
                query = query.Where(x => x.IsUsingHospitalTariff == isUsingHospitalTariff.Value);
            }

            if (isNeedEligibilityCheck.HasValue)
            {
                query = query.Where(x => x.IsNeedEligibilityCheck == isNeedEligibilityCheck.Value);
            }

            if (isNeedGuaranteeLetter.HasValue)
            {
                query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);
            }

            if (isNeedReferralLetter.HasValue)
            {
                query = query.Where(x => x.IsNeedReferralLetter == isNeedReferralLetter.Value);
            }

            if (isNeedApprovalForProcedure.HasValue)
            {
                query = query.Where(x => x.IsNeedApprovalForProcedure == isNeedApprovalForProcedure.Value);
            }

            if (isNeedApprovalForDrug.HasValue)
            {
                query = query.Where(x => x.IsNeedApprovalForDrug == isNeedApprovalForDrug.Value);
            }

            if (isCoverageLimitedByPlan.HasValue)
            {
                query = query.Where(x => x.IsCoverageLimitedByPlan == isCoverageLimitedByPlan.Value);
            }

            if (isAllowExcessPaymentByPatient.HasValue)
            {
                query = query.Where(x => x.IsAllowExcessPaymentByPatient == isAllowExcessPaymentByPatient.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedProviderTypes = AllowedProviderTypes
                    .Where(x =>
                        x.ToLower().Contains(keyword) ||
                        BuildProviderTypeLabel(x).ToLower().Contains(keyword))
                    .ToList();

                var matchedClaimMethods = AllowedClaimMethods
                    .Where(x =>
                        x.ToLower().Contains(keyword) ||
                        BuildClaimMethodLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.InsuranceProviderCode.ToLower().Contains(keyword) ||
                    x.InsuranceProviderName.ToLower().Contains(keyword) ||
                    x.ProviderType.ToLower().Contains(keyword) ||
                    x.ClaimMethod.ToLower().Contains(keyword) ||
                    (x.InsuranceGroupName != null && x.InsuranceGroupName.ToLower().Contains(keyword)) ||
                    (x.ExternalProviderCode != null && x.ExternalProviderCode.ToLower().Contains(keyword)) ||
                    (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)) ||
                    (x.ContractNumber != null && x.ContractNumber.ToLower().Contains(keyword)) ||
                    (x.PicName != null && x.PicName.ToLower().Contains(keyword)) ||
                    (x.PicPhoneNumber != null && x.PicPhoneNumber.ToLower().Contains(keyword)) ||
                    (x.PicWhatsAppNumber != null && x.PicWhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.PicEmail != null && x.PicEmail.ToLower().Contains(keyword)) ||
                    (x.OfficeAddress != null && x.OfficeAddress.ToLower().Contains(keyword)) ||
                    (x.BillingInstruction != null && x.BillingInstruction.ToLower().Contains(keyword)) ||
                    (x.ClaimInstruction != null && x.ClaimInstruction.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                    matchedProviderTypes.Contains(x.ProviderType) ||
                    matchedClaimMethods.Contains(x.ClaimMethod));
            }

            return query;
        }

        private static IQueryable<MstInsuranceProvider> ApplyContractStatusFilter(
            IQueryable<MstInsuranceProvider> query,
            string? contractStatus)
        {
            if (string.IsNullOrWhiteSpace(contractStatus))
            {
                return query;
            }

            var normalizedStatus = NormalizeContractStatus(contractStatus);
            var today = DateTime.UtcNow.Date;

            return normalizedStatus switch
            {
                ContractStatusActive => query.Where(x =>
                    (x.ContractStartDate.HasValue || x.ContractEndDate.HasValue) &&
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= today) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= today)),

                ContractStatusExpired => query.Where(x =>
                    x.ContractEndDate.HasValue &&
                    x.ContractEndDate.Value.Date < today),

                ContractStatusUpcoming => query.Where(x =>
                    x.ContractStartDate.HasValue &&
                    x.ContractStartDate.Value.Date > today),

                ContractStatusNoContract => query.Where(x =>
                    !x.ContractStartDate.HasValue &&
                    !x.ContractEndDate.HasValue),

                _ => query
            };
        }

        private static IOrderedQueryable<MstInsuranceProvider> ApplySorting(
            IQueryable<MstInsuranceProvider> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDescending = string.Equals(
                sortDirection,
                "desc",
                StringComparison.OrdinalIgnoreCase
            );

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDescending
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "insuranceprovidercode" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProviderCode)
                    : query.OrderBy(x => x.InsuranceProviderCode),

                "insuranceprovidername" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.InsuranceProviderName),

                "insurancegroupname" => isDescending
                    ? query.OrderByDescending(x => x.InsuranceGroupName).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.InsuranceGroupName).ThenBy(x => x.InsuranceProviderName),

                "providertype" => isDescending
                    ? query.OrderByDescending(x => x.ProviderType).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.ProviderType).ThenBy(x => x.InsuranceProviderName),

                "claimmethod" => isDescending
                    ? query.OrderByDescending(x => x.ClaimMethod).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.ClaimMethod).ThenBy(x => x.InsuranceProviderName),

                "contractstartdate" => isDescending
                    ? query.OrderByDescending(x => x.ContractStartDate).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.ContractStartDate).ThenBy(x => x.InsuranceProviderName),

                "contractenddate" => isDescending
                    ? query.OrderByDescending(x => x.ContractEndDate).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.ContractEndDate).ThenBy(x => x.InsuranceProviderName),

                "isusinginsurancetariffbook" => isDescending
                    ? query.OrderByDescending(x => x.IsUsingInsuranceTariffBook).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.IsUsingInsuranceTariffBook).ThenBy(x => x.InsuranceProviderName),

                "isneedeligibilitycheck" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedEligibilityCheck).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.IsNeedEligibilityCheck).ThenBy(x => x.InsuranceProviderName),

                "isneedguaranteeletter" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedGuaranteeLetter).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.IsNeedGuaranteeLetter).ThenBy(x => x.InsuranceProviderName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.InsuranceProviderName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.InsuranceProviderName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateInsuranceProviderRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.InsuranceProviderName))
            {
                return (false, "Nama insurance provider wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.ProviderType))
            {
                return (false, "Tipe provider wajib diisi.");
            }

            if (!AllowedProviderTypes.Any(x => string.Equals(x, request.ProviderType.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Tipe provider tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (string.IsNullOrWhiteSpace(request.ClaimMethod))
            {
                return (false, "Metode klaim wajib diisi.");
            }

            if (!AllowedClaimMethods.Any(x => string.Equals(x, request.ClaimMethod.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Metode klaim tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.ContractStartDate.HasValue &&
                request.ContractEndDate.HasValue &&
                request.ContractEndDate.Value.Date < request.ContractStartDate.Value.Date)
            {
                return (false, "Tanggal akhir kontrak tidak boleh lebih kecil dari tanggal mulai kontrak.");
            }

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
            }

            if (!string.IsNullOrWhiteSpace(request.PicEmail) &&
                !new EmailAddressAttribute().IsValid(request.PicEmail.Trim()))
            {
                return (false, "Format email PIC tidak valid.");
            }

            var normalizedName = request.InsuranceProviderName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama insurance provider sudah digunakan.");
            }

            var externalProviderCode = NormalizeUpperNullableString(request.ExternalProviderCode);

            if (!string.IsNullOrWhiteSpace(externalProviderCode))
            {
                var duplicateExternalProviderCodeQuery = _dbContext.Set<MstInsuranceProvider>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalProviderCode != null &&
                        x.ExternalProviderCode.ToUpper() == externalProviderCode);

                if (excludeId.HasValue)
                {
                    duplicateExternalProviderCodeQuery = duplicateExternalProviderCodeQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateExternalProviderCodeQuery.AnyAsync())
                {
                    return (false, "External provider code sudah digunakan.");
                }
            }

            var integrationCode = NormalizeUpperNullableString(request.IntegrationCode);

            if (!string.IsNullOrWhiteSpace(integrationCode))
            {
                var duplicateIntegrationCodeQuery = _dbContext.Set<MstInsuranceProvider>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IntegrationCode != null &&
                        x.IntegrationCode.ToUpper() == integrationCode);

                if (excludeId.HasValue)
                {
                    duplicateIntegrationCodeQuery = duplicateIntegrationCodeQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateIntegrationCodeQuery.AnyAsync())
                {
                    return (false, "Integration code sudah digunakan.");
                }
            }

            var contractNumber = NormalizeNullableString(request.ContractNumber);

            if (!string.IsNullOrWhiteSpace(contractNumber))
            {
                var normalizedContractNumber = contractNumber.ToLower();

                var duplicateContractNumberQuery = _dbContext.Set<MstInsuranceProvider>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ContractNumber != null &&
                        x.ContractNumber.ToLower() == normalizedContractNumber);

                if (excludeId.HasValue)
                {
                    duplicateContractNumberQuery = duplicateContractNumberQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateContractNumberQuery.AnyAsync())
                {
                    return (false, "Contract number sudah digunakan.");
                }
            }

            return (true, null);
        }

        private async Task<string> GenerateInsuranceProviderCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstInsuranceProvider>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.InsuranceProviderCode.StartsWith(InsuranceProviderCodePrefix))
                .Select(x => x.InsuranceProviderCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractInsuranceProviderSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return InsuranceProviderCodePrefix + nextNumber.ToString("D" + InsuranceProviderCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractInsuranceProviderSequenceNumber(string insuranceProviderCode)
        {
            if (string.IsNullOrWhiteSpace(insuranceProviderCode))
            {
                return null;
            }

            if (!insuranceProviderCode.StartsWith(InsuranceProviderCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = insuranceProviderCode[InsuranceProviderCodePrefix.Length..];

            return int.TryParse(numberText, NumberStyles.None, CultureInfo.InvariantCulture, out var number)
                ? number
                : null;
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(
            IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
            {
                return new Dictionary<Guid, string?>();
            }

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name =
                        x.DisplayName ??
                        x.UserName ??
                        x.Email ??
                        x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static InsuranceProviderResponse MapResponse(
            MstInsuranceProvider entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var contractStatus = ResolveContractStatus(
                entity.ContractStartDate,
                entity.ContractEndDate,
                DateTime.UtcNow.Date
            );

            return new InsuranceProviderResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                InsuranceGroupName = entity.InsuranceGroupName,
                ProviderType = entity.ProviderType,
                ProviderTypeName = BuildProviderTypeLabel(entity.ProviderType),
                ClaimMethod = entity.ClaimMethod,
                ClaimMethodName = BuildClaimMethodLabel(entity.ClaimMethod),
                ExternalProviderCode = entity.ExternalProviderCode,
                IntegrationCode = entity.IntegrationCode,
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractStatus = contractStatus,
                ContractStatusName = BuildContractStatusLabel(contractStatus),
                IsUsingInsuranceTariffBook = entity.IsUsingInsuranceTariffBook,
                IsUsingHospitalTariff = entity.IsUsingHospitalTariff,
                IsNeedEligibilityCheck = entity.IsNeedEligibilityCheck,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsNeedApprovalForProcedure = entity.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = entity.IsNeedApprovalForDrug,
                IsCoverageLimitedByPlan = entity.IsCoverageLimitedByPlan,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                PicName = entity.PicName,
                PicPhoneNumber = entity.PicPhoneNumber,
                PicWhatsAppNumber = entity.PicWhatsAppNumber,
                PicEmail = entity.PicEmail,
                OfficeAddress = entity.OfficeAddress,
                LogoPath = entity.LogoPath,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static InsuranceProviderDetailResponse MapDetailResponse(
            MstInsuranceProvider entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var contractStatus = ResolveContractStatus(
                entity.ContractStartDate,
                entity.ContractEndDate,
                DateTime.UtcNow.Date
            );

            return new InsuranceProviderDetailResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                InsuranceGroupName = entity.InsuranceGroupName,
                ProviderType = entity.ProviderType,
                ProviderTypeName = BuildProviderTypeLabel(entity.ProviderType),
                ClaimMethod = entity.ClaimMethod,
                ClaimMethodName = BuildClaimMethodLabel(entity.ClaimMethod),
                ExternalProviderCode = entity.ExternalProviderCode,
                IntegrationCode = entity.IntegrationCode,
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractStatus = contractStatus,
                ContractStatusName = BuildContractStatusLabel(contractStatus),
                IsUsingInsuranceTariffBook = entity.IsUsingInsuranceTariffBook,
                IsUsingHospitalTariff = entity.IsUsingHospitalTariff,
                IsNeedEligibilityCheck = entity.IsNeedEligibilityCheck,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsNeedApprovalForProcedure = entity.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = entity.IsNeedApprovalForDrug,
                IsCoverageLimitedByPlan = entity.IsCoverageLimitedByPlan,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                PicName = entity.PicName,
                PicPhoneNumber = entity.PicPhoneNumber,
                PicWhatsAppNumber = entity.PicWhatsAppNumber,
                PicEmail = entity.PicEmail,
                OfficeAddress = entity.OfficeAddress,
                LogoPath = entity.LogoPath,
                BillingInstruction = entity.BillingInstruction,
                ClaimInstruction = entity.ClaimInstruction,
                Description = entity.Description,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static InsuranceProviderOptionResponse MapOptionResponse(
            MstInsuranceProvider entity)
        {
            var contractStatus = ResolveContractStatus(
                entity.ContractStartDate,
                entity.ContractEndDate,
                DateTime.UtcNow.Date
            );

            return new InsuranceProviderOptionResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                InsuranceGroupName = entity.InsuranceGroupName,
                ProviderType = entity.ProviderType,
                ProviderTypeName = BuildProviderTypeLabel(entity.ProviderType),
                ClaimMethod = entity.ClaimMethod,
                ClaimMethodName = BuildClaimMethodLabel(entity.ClaimMethod),
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractStatus = contractStatus,
                ContractStatusName = BuildContractStatusLabel(contractStatus),
                IsUsingInsuranceTariffBook = entity.IsUsingInsuranceTariffBook,
                IsUsingHospitalTariff = entity.IsUsingHospitalTariff,
                IsNeedEligibilityCheck = entity.IsNeedEligibilityCheck,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedReferralLetter = entity.IsNeedReferralLetter,
                IsNeedApprovalForProcedure = entity.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = entity.IsNeedApprovalForDrug,
                IsCoverageLimitedByPlan = entity.IsCoverageLimitedByPlan,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                SortOrder = entity.SortOrder
            };
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
            {
                return null;
            }

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageSize < 1)
            {
                pageSize = 25;
            }

            if (pageSize > 100)
            {
                pageSize = 100;
            }

            return (pageNumber, pageSize);
        }

        private static DateRangeResolveResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var period = customPeriod?.Trim().ToLowerInvariant();
            var today = DateTime.UtcNow.Date;

            DateTime? start = null;
            DateTime? endExclusive = null;

            switch (period)
            {
                case null:
                case "":
                case "custom":
                    if (startDate.HasValue)
                    {
                        start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                    }

                    if (endDate.HasValue)
                    {
                        endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                    }

                    break;

                case "today":
                    start = today;
                    endExclusive = today.AddDays(1);
                    break;

                case "last7days":
                    start = today.AddDays(-6);
                    endExclusive = today.AddDays(1);
                    break;

                case "last30days":
                    start = today.AddDays(-29);
                    endExclusive = today.AddDays(1);
                    break;

                case "thismonth":
                    start = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endExclusive = start.Value.AddMonths(1);
                    break;

                case "lastmonth":
                    var currentMonthStart = new DateTime(today.Year, today.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                    start = currentMonthStart.AddMonths(-1);
                    endExclusive = currentMonthStart;
                    break;

                default:
                    return DateRangeResolveResult.Invalid($"customPeriod '{customPeriod}' tidak valid.");
            }

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
            {
                return DateRangeResolveResult.Invalid("startDate tidak boleh lebih besar atau sama dengan endDate.");
            }

            return DateRangeResolveResult.Valid(start, endExclusive);
        }

        private static List<InsuranceProviderCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<InsuranceProviderCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<InsuranceProviderStringOptionResponse> BuildProviderTypeOptions()
        {
            return AllowedProviderTypes
                .Select(x => new InsuranceProviderStringOptionResponse
                {
                    Value = x,
                    Label = BuildProviderTypeLabel(x)
                })
                .ToList();
        }

        private static List<InsuranceProviderStringOptionResponse> BuildClaimMethodOptions()
        {
            return AllowedClaimMethods
                .Select(x => new InsuranceProviderStringOptionResponse
                {
                    Value = x,
                    Label = BuildClaimMethodLabel(x)
                })
                .ToList();
        }

        private static List<InsuranceProviderStringOptionResponse> BuildContractStatusOptions()
        {
            return AllowedContractStatuses
                .Select(x => new InsuranceProviderStringOptionResponse
                {
                    Value = x,
                    Label = BuildContractStatusLabel(x),
                    Description = x switch
                    {
                        ContractStatusActive => "Kontrak sedang berjalan.",
                        ContractStatusExpired => "Kontrak sudah berakhir.",
                        ContractStatusUpcoming => "Kontrak belum mulai.",
                        ContractStatusNoContract => "Tanggal kontrak belum diisi.",
                        _ => null
                    }
                })
                .ToList();
        }

        private static string NormalizeProviderType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedProviderTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "PrivateInsurance";
        }

        private static string NormalizeClaimMethod(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedClaimMethods
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "Cashless";
        }

        private static string NormalizeContractStatus(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedContractStatuses
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? string.Empty;
        }

        private static string ResolveContractStatus(
            DateTime? contractStartDate,
            DateTime? contractEndDate,
            DateTime today)
        {
            if (!contractStartDate.HasValue && !contractEndDate.HasValue)
            {
                return ContractStatusNoContract;
            }

            if (contractStartDate.HasValue && contractStartDate.Value.Date > today)
            {
                return ContractStatusUpcoming;
            }

            if (contractEndDate.HasValue && contractEndDate.Value.Date < today)
            {
                return ContractStatusExpired;
            }

            return ContractStatusActive;
        }

        private static string BuildProviderTypeLabel(string value)
        {
            return value switch
            {
                "PrivateInsurance" => "Private Insurance",
                "TPA" => "TPA",
                "GovernmentInsurance" => "Government Insurance",
                "CorporateInsurance" => "Corporate Insurance",
                "Other" => "Other",
                _ => SplitPascalCase(value)
            };
        }

        private static string BuildClaimMethodLabel(string value)
        {
            return value switch
            {
                "Cashless" => "Cashless",
                "Reimbursement" => "Reimbursement",
                "GuaranteeLetter" => "Guarantee Letter",
                "Mixed" => "Mixed",
                _ => SplitPascalCase(value)
            };
        }

        private static string BuildContractStatusLabel(string value)
        {
            return value switch
            {
                ContractStatusActive => "Active",
                ContractStatusExpired => "Expired",
                ContractStatusUpcoming => "Upcoming",
                ContractStatusNoContract => "No Contract Date",
                _ => SplitPascalCase(value)
            };
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static List<InsuranceProviderQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<InsuranceProviderQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, group, tipe provider, metode klaim, kode eksternal, integration code, nomor kontrak, PIC, alamat, instruksi, atau deskripsi.", Example = "cashless" },
                new() { Name = "isActive", Type = "bool?", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "providerType", Type = "string", Description = "Filter tipe provider.", Example = "PrivateInsurance" },
                new() { Name = "claimMethod", Type = "string", Description = "Filter metode klaim.", Example = "Cashless" },
                new() { Name = "contractStatus", Type = "string", Description = "Filter status kontrak: active, expired, upcoming, noContract.", Example = "active" },
                new() { Name = "isUsingInsuranceTariffBook", Type = "bool?", Description = "Filter provider yang memakai tariff book asuransi.", Example = "true" },
                new() { Name = "isUsingHospitalTariff", Type = "bool?", Description = "Filter provider yang memakai tarif rumah sakit.", Example = "false" },
                new() { Name = "isNeedEligibilityCheck", Type = "bool?", Description = "Filter kebutuhan eligibility check.", Example = "true" },
                new() { Name = "isNeedGuaranteeLetter", Type = "bool?", Description = "Filter kebutuhan guarantee letter.", Example = "true" },
                new() { Name = "isNeedReferralLetter", Type = "bool?", Description = "Filter kebutuhan surat rujukan.", Example = "false" },
                new() { Name = "isNeedApprovalForProcedure", Type = "bool?", Description = "Filter kebutuhan approval tindakan.", Example = "true" },
                new() { Name = "isNeedApprovalForDrug", Type = "bool?", Description = "Filter kebutuhan approval obat.", Example = "false" },
                new() { Name = "isCoverageLimitedByPlan", Type = "bool?", Description = "Filter pembatasan coverage berdasarkan plan.", Example = "true" },
                new() { Name = "isAllowExcessPaymentByPatient", Type = "bool?", Description = "Filter apakah selisih biaya boleh dibayar pasien.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<InsuranceProviderFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<InsuranceProviderFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<InsuranceProviderFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<InsuranceProviderFormFieldMetadataResponse>
            {
                new() { Name = "insuranceProviderCode", Label = "Kode Insurance Provider", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format IP-RSMMC-00001.", Example = "IP-RSMMC-00001", SortOrder = 1 },
                new() { Name = "insuranceProviderName", Label = "Nama Insurance Provider", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "Asuransi Sehat Sentosa", SortOrder = 2 },
                new() { Name = "insuranceGroupName", Label = "Group Insurance", Section = "Basic", InputType = "text", MaxLength = 100, Example = "Sehat Group", SortOrder = 3 },
                new() { Name = "providerType", Label = "Tipe Provider", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "providerTypeOptions", Example = "PrivateInsurance", SortOrder = 4 },
                new() { Name = "claimMethod", Label = "Metode Klaim", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "claimMethodOptions", Example = "Cashless", SortOrder = 5 },
                new() { Name = "externalProviderCode", Label = "Kode Eksternal", Section = "Integration", InputType = "text", MaxLength = 50, Example = "EXT-INS-001", SortOrder = 6 },
                new() { Name = "integrationCode", Label = "Kode Integrasi", Section = "Integration", InputType = "text", MaxLength = 50, Example = "INS001", SortOrder = 7 },
                new() { Name = "contractNumber", Label = "Nomor Kontrak", Section = "Contract", InputType = "text", MaxLength = 100, Example = "PKS/INS/2026/001", SortOrder = 8 },
                new() { Name = "contractStartDate", Label = "Tanggal Mulai Kontrak", Section = "Contract", InputType = "date", SortOrder = 9 },
                new() { Name = "contractEndDate", Label = "Tanggal Akhir Kontrak", Section = "Contract", InputType = "date", SortOrder = 10 },
                new() { Name = "isUsingInsuranceTariffBook", Label = "Pakai Tariff Book Insurance", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isUsingHospitalTariff", Label = "Pakai Tarif Rumah Sakit", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isNeedEligibilityCheck", Label = "Butuh Eligibility Check", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isNeedGuaranteeLetter", Label = "Butuh Guarantee Letter", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "isNeedReferralLetter", Label = "Butuh Surat Rujukan", Section = "Rule", InputType = "switch", SortOrder = 15 },
                new() { Name = "isNeedApprovalForProcedure", Label = "Butuh Approval Tindakan", Section = "Rule", InputType = "switch", SortOrder = 16 },
                new() { Name = "isNeedApprovalForDrug", Label = "Butuh Approval Obat", Section = "Rule", InputType = "switch", SortOrder = 17 },
                new() { Name = "isCoverageLimitedByPlan", Label = "Coverage Dibatasi Plan", Section = "Rule", InputType = "switch", SortOrder = 18 },
                new() { Name = "isAllowExcessPaymentByPatient", Label = "Selisih Boleh Dibayar Pasien", Section = "Rule", InputType = "switch", SortOrder = 19 },
                new() { Name = "picName", Label = "Nama PIC", Section = "Contact", InputType = "text", MaxLength = 100, SortOrder = 20 },
                new() { Name = "picPhoneNumber", Label = "Telepon PIC", Section = "Contact", InputType = "text", MaxLength = 30, SortOrder = 21 },
                new() { Name = "picWhatsAppNumber", Label = "WhatsApp PIC", Section = "Contact", InputType = "text", MaxLength = 30, SortOrder = 22 },
                new() { Name = "picEmail", Label = "Email PIC", Section = "Contact", InputType = "email", MaxLength = 200, SortOrder = 23 },
                new() { Name = "officeAddress", Label = "Alamat Kantor", Section = "Contact", InputType = "textarea", MaxLength = 500, SortOrder = 24 },
                new() { Name = "logoPath", Label = "Logo Path", Section = "Additional", InputType = "text", MaxLength = 500, SortOrder = 25 },
                new() { Name = "billingInstruction", Label = "Instruksi Billing", Section = "Instruction", InputType = "textarea", MaxLength = 250, SortOrder = 26 },
                new() { Name = "claimInstruction", Label = "Instruksi Klaim", Section = "Instruction", InputType = "textarea", MaxLength = 250, SortOrder = 27 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 28 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 29 }
            };

            if (isUpdate)
            {
                fields.Add(new InsuranceProviderFormFieldMetadataResponse
                {
                    Name = "isActive",
                    Label = "Status Aktif",
                    Section = "Status",
                    InputType = "switch",
                    SortOrder = 99
                });
            }

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeUpperNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeLowerNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private sealed class DateRangeResolveResult
        {
            public bool IsValid { get; private set; }
            public string? ErrorMessage { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }

            public static DateRangeResolveResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResolveResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResolveResult Invalid(string errorMessage)
            {
                return new DateRangeResolveResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
