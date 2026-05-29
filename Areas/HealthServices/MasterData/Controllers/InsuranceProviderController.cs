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

using ResponseInsuranceProviderPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.InsuranceProviderResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/insurance-providers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Insurance Provider",
        AreaName = "HealthServices",
        ControllerName = "InsuranceProvider",
        Description = "Health service patient management master data insurance provider",
        SortOrder = 13
    )]
    [Tags("Health Services / Patient Management / Master Data / Insurance Provider")]
    public class InsuranceProviderController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private static readonly HashSet<string> AllowedProviderTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "PrivateInsurance",
            "TPA",
            "GovernmentInsurance",
            "CorporateInsurance",
            "Other"
        };

        private static readonly HashSet<string> AllowedClaimMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Cashless",
            "Reimbursement",
            "GuaranteeLetter",
            "Mixed"
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
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat data insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new InsuranceProviderFilterMetadataResponse
            {
                DefaultFilter = new InsuranceProviderDefaultFilterResponse(),
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
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ProviderTypes = AllowedProviderTypes.OrderBy(x => x).ToList(),
                ClaimMethods = AllowedClaimMethods.OrderBy(x => x).ToList()
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
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat data insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new InsuranceProviderSummaryResponse
            {
                TotalInsuranceProvider = await query.CountAsync(),
                ActiveInsuranceProvider = await query.CountAsync(x => x.IsActive),
                InactiveInsuranceProvider = await query.CountAsync(x => !x.IsActive),
                PrivateInsuranceProvider = await query.CountAsync(x => x.ProviderType == "PrivateInsurance"),
                TpaProvider = await query.CountAsync(x => x.ProviderType == "TPA"),
                GovernmentInsuranceProvider = await query.CountAsync(x => x.ProviderType == "GovernmentInsurance"),
                CorporateInsuranceProvider = await query.CountAsync(x => x.ProviderType == "CorporateInsurance"),
                CashlessProvider = await query.CountAsync(x => x.ClaimMethod == "Cashless"),
                ReimbursementProvider = await query.CountAsync(x => x.ClaimMethod == "Reimbursement"),
                GuaranteeLetterProvider = await query.CountAsync(x => x.ClaimMethod == "GuaranteeLetter"),
                MixedClaimProvider = await query.CountAsync(x => x.ClaimMethod == "Mixed"),
                NeedEligibilityCheckProvider = await query.CountAsync(x => x.IsNeedEligibilityCheck),
                NeedGuaranteeLetterProvider = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                NeedReferralLetterProvider = await query.CountAsync(x => x.IsNeedReferralLetter),
                NeedApprovalForProcedureProvider = await query.CountAsync(x => x.IsNeedApprovalForProcedure),
                NeedApprovalForDrugProvider = await query.CountAsync(x => x.IsNeedApprovalForDrug),
                ActiveContractProvider = await query.CountAsync(x =>
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= today) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= today)),
                ExpiredContractProvider = await query.CountAsync(x =>
                    x.ContractEndDate.HasValue && x.ContractEndDate.Value.Date < today)
            };

            return Ok(ApiResponse<InsuranceProviderSummaryResponse>.Ok(
                result,
                "Ringkasan insurance provider berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseInsuranceProviderPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat data insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviders(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? providerType,
            [FromQuery] string? claimMethod,
            [FromQuery] string? insuranceGroupName,
            [FromQuery] bool? isUsingInsuranceTariffBook,
            [FromQuery] bool? isUsingHospitalTariff,
            [FromQuery] bool? isNeedEligibilityCheck,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedReferralLetter,
            [FromQuery] bool? isNeedApprovalForProcedure,
            [FromQuery] bool? isNeedApprovalForDrug,
            [FromQuery] bool? isCoverageLimitedByPlan,
            [FromQuery] bool? isAllowExcessPaymentByPatient,
            [FromQuery] DateTime? contractDate,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.InsuranceProviderCode.ToLower().Contains(keyword) ||
                    x.InsuranceProviderName.ToLower().Contains(keyword) ||
                    x.InsuranceGroupName != null && x.InsuranceGroupName.ToLower().Contains(keyword) ||
                    x.ProviderType.ToLower().Contains(keyword) ||
                    x.ClaimMethod.ToLower().Contains(keyword) ||
                    x.ExternalProviderCode != null && x.ExternalProviderCode.ToLower().Contains(keyword) ||
                    x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword) ||
                    x.ContractNumber != null && x.ContractNumber.ToLower().Contains(keyword) ||
                    x.PicName != null && x.PicName.ToLower().Contains(keyword) ||
                    x.PicEmail != null && x.PicEmail.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(providerType))
            {
                var providerTypeKeyword = providerType.Trim();
                query = query.Where(x => x.ProviderType == providerTypeKeyword);
            }

            if (!string.IsNullOrWhiteSpace(claimMethod))
            {
                var claimMethodKeyword = claimMethod.Trim();
                query = query.Where(x => x.ClaimMethod == claimMethodKeyword);
            }

            if (!string.IsNullOrWhiteSpace(insuranceGroupName))
            {
                var groupKeyword = insuranceGroupName.Trim().ToLower();
                query = query.Where(x => x.InsuranceGroupName != null && x.InsuranceGroupName.ToLower().Contains(groupKeyword));
            }

            if (isUsingInsuranceTariffBook.HasValue)
                query = query.Where(x => x.IsUsingInsuranceTariffBook == isUsingInsuranceTariffBook.Value);

            if (isUsingHospitalTariff.HasValue)
                query = query.Where(x => x.IsUsingHospitalTariff == isUsingHospitalTariff.Value);

            if (isNeedEligibilityCheck.HasValue)
                query = query.Where(x => x.IsNeedEligibilityCheck == isNeedEligibilityCheck.Value);

            if (isNeedGuaranteeLetter.HasValue)
                query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);

            if (isNeedReferralLetter.HasValue)
                query = query.Where(x => x.IsNeedReferralLetter == isNeedReferralLetter.Value);

            if (isNeedApprovalForProcedure.HasValue)
                query = query.Where(x => x.IsNeedApprovalForProcedure == isNeedApprovalForProcedure.Value);

            if (isNeedApprovalForDrug.HasValue)
                query = query.Where(x => x.IsNeedApprovalForDrug == isNeedApprovalForDrug.Value);

            if (isCoverageLimitedByPlan.HasValue)
                query = query.Where(x => x.IsCoverageLimitedByPlan == isCoverageLimitedByPlan.Value);

            if (isAllowExcessPaymentByPatient.HasValue)
                query = query.Where(x => x.IsAllowExcessPaymentByPatient == isAllowExcessPaymentByPatient.Value);

            if (contractDate.HasValue)
            {
                var selectedDate = contractDate.Value.Date;

                query = query.Where(x =>
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= selectedDate) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= selectedDate));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InsuranceProviderResponse
                {
                    Id = x.Id,
                    InsuranceProviderCode = x.InsuranceProviderCode,
                    InsuranceProviderName = x.InsuranceProviderName,
                    InsuranceGroupName = x.InsuranceGroupName,
                    ProviderType = x.ProviderType,
                    ClaimMethod = x.ClaimMethod,
                    ExternalProviderCode = x.ExternalProviderCode,
                    IntegrationCode = x.IntegrationCode,
                    ContractNumber = x.ContractNumber,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    IsUsingInsuranceTariffBook = x.IsUsingInsuranceTariffBook,
                    IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                    IsNeedEligibilityCheck = x.IsNeedEligibilityCheck,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = x.IsNeedReferralLetter,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsCoverageLimitedByPlan = x.IsCoverageLimitedByPlan,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    PicName = x.PicName,
                    PicPhoneNumber = x.PicPhoneNumber,
                    PicWhatsAppNumber = x.PicWhatsAppNumber,
                    PicEmail = x.PicEmail,
                    OfficeAddress = x.OfficeAddress,
                    LogoPath = x.LogoPath,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

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
        [ProducesResponseType(typeof(ApiResponse<List<InsuranceProviderOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat data insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviderOptions(
            [FromQuery] string? providerType,
            [FromQuery] string? claimMethod,
            [FromQuery] bool? isNeedEligibilityCheck,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedReferralLetter,
            [FromQuery] bool? isNeedApprovalForProcedure,
            [FromQuery] bool? isNeedApprovalForDrug,
            [FromQuery] DateTime? contractDate,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(providerType))
                query = query.Where(x => x.ProviderType == providerType.Trim());

            if (!string.IsNullOrWhiteSpace(claimMethod))
                query = query.Where(x => x.ClaimMethod == claimMethod.Trim());

            if (isNeedEligibilityCheck.HasValue)
                query = query.Where(x => x.IsNeedEligibilityCheck == isNeedEligibilityCheck.Value);

            if (isNeedGuaranteeLetter.HasValue)
                query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);

            if (isNeedReferralLetter.HasValue)
                query = query.Where(x => x.IsNeedReferralLetter == isNeedReferralLetter.Value);

            if (isNeedApprovalForProcedure.HasValue)
                query = query.Where(x => x.IsNeedApprovalForProcedure == isNeedApprovalForProcedure.Value);

            if (isNeedApprovalForDrug.HasValue)
                query = query.Where(x => x.IsNeedApprovalForDrug == isNeedApprovalForDrug.Value);

            if (contractDate.HasValue)
            {
                var selectedDate = contractDate.Value.Date;

                query = query.Where(x =>
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= selectedDate) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= selectedDate));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.InsuranceProviderCode.ToLower().Contains(keyword) ||
                    x.InsuranceProviderName.ToLower().Contains(keyword) ||
                    x.InsuranceGroupName != null && x.InsuranceGroupName.ToLower().Contains(keyword));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.InsuranceProviderName)
                .Select(x => new InsuranceProviderOptionResponse
                {
                    Id = x.Id,
                    InsuranceProviderCode = x.InsuranceProviderCode,
                    InsuranceProviderName = x.InsuranceProviderName,
                    InsuranceGroupName = x.InsuranceGroupName,
                    ProviderType = x.ProviderType,
                    ClaimMethod = x.ClaimMethod,
                    IsNeedEligibilityCheck = x.IsNeedEligibilityCheck,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = x.IsNeedReferralLetter,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient
                })
                .ToListAsync();

            return Ok(ApiResponse<List<InsuranceProviderOptionResponse>>.Ok(
                data,
                "Data pilihan insurance provider berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat data insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviderById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new InsuranceProviderDetailResponse
                {
                    Id = x.Id,
                    InsuranceProviderCode = x.InsuranceProviderCode,
                    InsuranceProviderName = x.InsuranceProviderName,
                    InsuranceGroupName = x.InsuranceGroupName,
                    ProviderType = x.ProviderType,
                    ClaimMethod = x.ClaimMethod,
                    ExternalProviderCode = x.ExternalProviderCode,
                    IntegrationCode = x.IntegrationCode,
                    ContractNumber = x.ContractNumber,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    IsUsingInsuranceTariffBook = x.IsUsingInsuranceTariffBook,
                    IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                    IsNeedEligibilityCheck = x.IsNeedEligibilityCheck,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = x.IsNeedReferralLetter,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsCoverageLimitedByPlan = x.IsCoverageLimitedByPlan,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    PicName = x.PicName,
                    PicPhoneNumber = x.PicPhoneNumber,
                    PicWhatsAppNumber = x.PicWhatsAppNumber,
                    PicEmail = x.PicEmail,
                    OfficeAddress = x.OfficeAddress,
                    LogoPath = x.LogoPath,
                    BillingInstruction = x.BillingInstruction,
                    ClaimInstruction = x.ClaimInstruction,
                    Description = x.Description,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Insurance provider tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<InsuranceProviderDetailResponse>.Ok(
                data,
                "Detail insurance provider berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Insurance Provider", Description = "Membuat data insurance provider", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InsuranceProvider", "Create")]
        public async Task<IActionResult> CreateInsuranceProvider([FromBody] CreateInsuranceProviderRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                insuranceProviderCode: request.InsuranceProviderCode,
                insuranceProviderName: request.InsuranceProviderName,
                providerType: request.ProviderType,
                claimMethod: request.ClaimMethod,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate
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

            var entity = new MstInsuranceProvider
            {
                Id = Guid.NewGuid(),
                InsuranceProviderCode = request.InsuranceProviderCode.Trim().ToUpperInvariant(),
                InsuranceProviderName = request.InsuranceProviderName.Trim(),
                InsuranceGroupName = NormalizeNullableText(request.InsuranceGroupName),
                ProviderType = request.ProviderType.Trim(),
                ClaimMethod = request.ClaimMethod.Trim(),
                ExternalProviderCode = NormalizeNullableText(request.ExternalProviderCode),
                IntegrationCode = NormalizeNullableText(request.IntegrationCode),
                ContractNumber = NormalizeNullableText(request.ContractNumber),
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
                PicName = NormalizeNullableText(request.PicName),
                PicPhoneNumber = NormalizeNullableText(request.PicPhoneNumber),
                PicWhatsAppNumber = NormalizeNullableText(request.PicWhatsAppNumber),
                PicEmail = NormalizeNullableText(request.PicEmail),
                OfficeAddress = NormalizeNullableText(request.OfficeAddress),
                LogoPath = NormalizeNullableText(request.LogoPath),
                BillingInstruction = NormalizeNullableText(request.BillingInstruction),
                ClaimInstruction = NormalizeNullableText(request.ClaimInstruction),
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstInsuranceProvider>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new InsuranceProviderCreateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<InsuranceProviderCreateResponse>.Ok(
                response,
                "Insurance provider berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Provider", Description = "Mengubah data insurance provider", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceProvider", "Update")]
        public async Task<IActionResult> UpdateInsuranceProvider(Guid id, [FromBody] UpdateInsuranceProviderRequest request)
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
                insuranceProviderCode: request.InsuranceProviderCode,
                insuranceProviderName: request.InsuranceProviderName,
                providerType: request.ProviderType,
                claimMethod: request.ClaimMethod,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance provider tidak valid."
                ));
            }

            entity.InsuranceProviderCode = request.InsuranceProviderCode.Trim().ToUpperInvariant();
            entity.InsuranceProviderName = request.InsuranceProviderName.Trim();
            entity.InsuranceGroupName = NormalizeNullableText(request.InsuranceGroupName);
            entity.ProviderType = request.ProviderType.Trim();
            entity.ClaimMethod = request.ClaimMethod.Trim();
            entity.ExternalProviderCode = NormalizeNullableText(request.ExternalProviderCode);
            entity.IntegrationCode = NormalizeNullableText(request.IntegrationCode);
            entity.ContractNumber = NormalizeNullableText(request.ContractNumber);
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
            entity.PicName = NormalizeNullableText(request.PicName);
            entity.PicPhoneNumber = NormalizeNullableText(request.PicPhoneNumber);
            entity.PicWhatsAppNumber = NormalizeNullableText(request.PicWhatsAppNumber);
            entity.PicEmail = NormalizeNullableText(request.PicEmail);
            entity.OfficeAddress = NormalizeNullableText(request.OfficeAddress);
            entity.LogoPath = NormalizeNullableText(request.LogoPath);
            entity.BillingInstruction = NormalizeNullableText(request.BillingInstruction);
            entity.ClaimInstruction = NormalizeNullableText(request.ClaimInstruction);
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Insurance provider berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Insurance Provider Status", Description = "Mengubah status aktif insurance provider", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InsuranceProvider", "Update")]
        public async Task<IActionResult> UpdateInsuranceProviderStatus(Guid id, [FromQuery] bool isActive)
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new InsuranceProviderStatusResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<InsuranceProviderStatusResponse>.Ok(
                response,
                "Status insurance provider berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Insurance Provider", Description = "Menghapus data insurance provider", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("InsuranceProvider", "Delete")]
        public async Task<IActionResult> DeleteInsuranceProvider(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new InsuranceProviderDeleteResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                IsDelete = entity.IsDelete
            };

            return Ok(ApiResponse<InsuranceProviderDeleteResponse>.Ok(
                response,
                "Insurance provider berhasil dihapus."
            ));
        }

        private IQueryable<MstInsuranceProvider> BuildBaseQuery()
        {
            return _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string insuranceProviderCode,
            string insuranceProviderName,
            string providerType,
            string claimMethod,
            DateTime? contractStartDate,
            DateTime? contractEndDate)
        {
            if (string.IsNullOrWhiteSpace(insuranceProviderCode))
                return (false, "Kode insurance provider wajib diisi.");

            if (string.IsNullOrWhiteSpace(insuranceProviderName))
                return (false, "Nama insurance provider wajib diisi.");

            if (string.IsNullOrWhiteSpace(providerType))
                return (false, "Tipe provider wajib diisi.");

            if (!AllowedProviderTypes.Contains(providerType.Trim()))
                return (false, "Tipe provider tidak valid. Gunakan PrivateInsurance, TPA, GovernmentInsurance, CorporateInsurance, atau Other.");

            if (string.IsNullOrWhiteSpace(claimMethod))
                return (false, "Metode klaim wajib diisi.");

            if (!AllowedClaimMethods.Contains(claimMethod.Trim()))
                return (false, "Metode klaim tidak valid. Gunakan Cashless, Reimbursement, GuaranteeLetter, atau Mixed.");

            if (contractStartDate.HasValue && contractEndDate.HasValue && contractEndDate.Value.Date < contractStartDate.Value.Date)
                return (false, "Tanggal akhir kontrak tidak boleh lebih kecil dari tanggal mulai kontrak.");

            var normalizedCode = insuranceProviderCode.Trim().ToUpperInvariant();
            var normalizedName = insuranceProviderName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstInsuranceProvider>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode insurance provider sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstInsuranceProvider>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama insurance provider sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstInsuranceProvider> ApplySorting(
            IQueryable<MstInsuranceProvider> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "insuranceprovidercode" => isDesc
                    ? query.OrderByDescending(x => x.InsuranceProviderCode)
                    : query.OrderBy(x => x.InsuranceProviderCode),

                "insuranceprovidername" => isDesc
                    ? query.OrderByDescending(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.InsuranceProviderName),

                "insurancegroupname" => isDesc
                    ? query.OrderByDescending(x => x.InsuranceGroupName)
                    : query.OrderBy(x => x.InsuranceGroupName),

                "providertype" => isDesc
                    ? query.OrderByDescending(x => x.ProviderType)
                    : query.OrderBy(x => x.ProviderType),

                "claimmethod" => isDesc
                    ? query.OrderByDescending(x => x.ClaimMethod)
                    : query.OrderBy(x => x.ClaimMethod),

                "contractstartdate" => isDesc
                    ? query.OrderByDescending(x => x.ContractStartDate)
                    : query.OrderBy(x => x.ContractStartDate),

                "contractenddate" => isDesc
                    ? query.OrderByDescending(x => x.ContractEndDate)
                    : query.OrderBy(x => x.ContractEndDate),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.InsuranceProviderName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.InsuranceProviderName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
