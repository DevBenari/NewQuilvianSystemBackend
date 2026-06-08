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
        private const string InsuranceProviderCodePrefix = "IP-RSMMC-";
        private const int InsuranceProviderCodeDigitLength = 5;

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
                CustomPeriods = new List<InsuranceProviderCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7Days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" }
                },
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
                ProviderTypeOptions = AllowedProviderTypes
                    .OrderBy(x => x)
                    .Select(x => new InsuranceProviderStringOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList(),
                ClaimMethodOptions = AllowedClaimMethods
                    .OrderBy(x => x)
                    .Select(x => new InsuranceProviderStringOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList()
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
                UsingInsuranceTariffBookProvider = await query.CountAsync(x => x.IsUsingInsuranceTariffBook),
                UsingHospitalTariffProvider = await query.CountAsync(x => x.IsUsingHospitalTariff),
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
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
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

            var query = _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.InsuranceProviderCode.ToLower().Contains(keyword) ||
                    x.InsuranceProviderName.ToLower().Contains(keyword) ||
                    x.ProviderType.ToLower().Contains(keyword) ||
                    x.ClaimMethod.ToLower().Contains(keyword) ||
                    x.InsuranceGroupName != null && x.InsuranceGroupName.ToLower().Contains(keyword) ||
                    x.ExternalProviderCode != null && x.ExternalProviderCode.ToLower().Contains(keyword) ||
                    x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword) ||
                    x.ContractNumber != null && x.ContractNumber.ToLower().Contains(keyword) ||
                    x.PicName != null && x.PicName.ToLower().Contains(keyword) ||
                    x.PicPhoneNumber != null && x.PicPhoneNumber.ToLower().Contains(keyword) ||
                    x.PicWhatsAppNumber != null && x.PicWhatsAppNumber.ToLower().Contains(keyword) ||
                    x.PicEmail != null && x.PicEmail.ToLower().Contains(keyword) ||
                    x.OfficeAddress != null && x.OfficeAddress.ToLower().Contains(keyword) ||
                    x.BillingInstruction != null && x.BillingInstruction.ToLower().Contains(keyword) ||
                    x.ClaimInstruction != null && x.ClaimInstruction.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
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
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat data insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviderOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.InsuranceProviderCode.ToLower().Contains(keyword) ||
                    x.InsuranceProviderName.ToLower().Contains(keyword) ||
                    x.ProviderType.ToLower().Contains(keyword) ||
                    x.ClaimMethod.ToLower().Contains(keyword) ||
                    x.InsuranceGroupName != null && x.InsuranceGroupName.ToLower().Contains(keyword) ||
                    x.ExternalProviderCode != null && x.ExternalProviderCode.ToLower().Contains(keyword) ||
                    x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.InsuranceProviderName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InsuranceProviderOptionResponse
                {
                    Id = x.Id,
                    InsuranceProviderCode = x.InsuranceProviderCode,
                    InsuranceProviderName = x.InsuranceProviderName,
                    InsuranceGroupName = x.InsuranceGroupName,
                    ProviderType = x.ProviderType,
                    ClaimMethod = x.ClaimMethod,
                    IsUsingInsuranceTariffBook = x.IsUsingInsuranceTariffBook,
                    IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                    IsNeedEligibilityCheck = x.IsNeedEligibilityCheck,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedReferralLetter = x.IsNeedReferralLetter,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsCoverageLimitedByPlan = x.IsCoverageLimitedByPlan,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient
                })
                .ToListAsync();

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
        [AccessAction("Read", "Read Insurance Provider", Description = "Melihat detail insurance provider", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviderById(Guid id)
        {
            var data = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Insurance Provider", Description = "Membuat data insurance provider", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("InsuranceProvider", "Create")]
        public async Task<IActionResult> CreateInsuranceProvider([FromBody] CreateInsuranceProviderRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                insuranceProviderName: request.InsuranceProviderName,
                providerType: request.ProviderType,
                claimMethod: request.ClaimMethod,
                externalProviderCode: request.ExternalProviderCode,
                integrationCode: request.IntegrationCode,
                contractNumber: request.ContractNumber,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                picEmail: request.PicEmail
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
                InsuranceProviderCode = await GenerateInsuranceProviderCodeAsync(),
                InsuranceProviderName = request.InsuranceProviderName.Trim(),
                InsuranceGroupName = NormalizeNullableString(request.InsuranceGroupName),
                ProviderType = NormalizeProviderType(request.ProviderType),
                ClaimMethod = NormalizeClaimMethod(request.ClaimMethod),
                ExternalProviderCode = NormalizeNullableString(request.ExternalProviderCode),
                IntegrationCode = NormalizeNullableString(request.IntegrationCode),
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
                PicEmail = NormalizeNullableString(request.PicEmail),
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

            var result = new InsuranceProviderCreateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                ProviderType = entity.ProviderType,
                ClaimMethod = entity.ClaimMethod,
                IsActive = entity.IsActive
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
                insuranceProviderName: request.InsuranceProviderName,
                providerType: request.ProviderType,
                claimMethod: request.ClaimMethod,
                externalProviderCode: request.ExternalProviderCode,
                integrationCode: request.IntegrationCode,
                contractNumber: request.ContractNumber,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                picEmail: request.PicEmail
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data insurance provider tidak valid."
                ));
            }

            entity.InsuranceProviderName = request.InsuranceProviderName.Trim();
            entity.InsuranceGroupName = NormalizeNullableString(request.InsuranceGroupName);
            entity.ProviderType = NormalizeProviderType(request.ProviderType);
            entity.ClaimMethod = NormalizeClaimMethod(request.ClaimMethod);
            entity.ExternalProviderCode = NormalizeNullableString(request.ExternalProviderCode);
            entity.IntegrationCode = NormalizeNullableString(request.IntegrationCode);
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
            entity.PicEmail = NormalizeNullableString(request.PicEmail);
            entity.OfficeAddress = NormalizeNullableString(request.OfficeAddress);
            entity.LogoPath = NormalizeNullableString(request.LogoPath);
            entity.BillingInstruction = NormalizeNullableString(request.BillingInstruction);
            entity.ClaimInstruction = NormalizeNullableString(request.ClaimInstruction);
            entity.Description = NormalizeNullableString(request.Description);
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

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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

            return Ok(ApiResponse<object>.Ok(
                null,
                "Insurance provider berhasil dihapus."
            ));
        }

        private static InsuranceProviderResponse ToResponse(MstInsuranceProvider x)
        {
            return new InsuranceProviderResponse
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
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string insuranceProviderName,
            string providerType,
            string claimMethod,
            string? externalProviderCode,
            string? integrationCode,
            string? contractNumber,
            DateTime? contractStartDate,
            DateTime? contractEndDate,
            string? picEmail)
        {
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

            if (!string.IsNullOrWhiteSpace(picEmail) && !picEmail.Contains('@'))
                return (false, "Format email PIC tidak valid.");

            var normalizedName = insuranceProviderName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstInsuranceProvider>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.InsuranceProviderName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama insurance provider sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(externalProviderCode))
            {
                var normalizedExternalProviderCode = externalProviderCode.Trim().ToLower();

                var duplicateExternalProviderCode = await _dbContext.Set<MstInsuranceProvider>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.ExternalProviderCode != null &&
                        x.ExternalProviderCode.ToLower() == normalizedExternalProviderCode &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateExternalProviderCode)
                    return (false, "External provider code sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(integrationCode))
            {
                var normalizedIntegrationCode = integrationCode.Trim().ToLower();

                var duplicateIntegrationCode = await _dbContext.Set<MstInsuranceProvider>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.IntegrationCode != null &&
                        x.IntegrationCode.ToLower() == normalizedIntegrationCode &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateIntegrationCode)
                    return (false, "Integration code sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(contractNumber))
            {
                var normalizedContractNumber = contractNumber.Trim().ToLower();

                var duplicateContractNumber = await _dbContext.Set<MstInsuranceProvider>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.ContractNumber != null &&
                        x.ContractNumber.ToLower() == normalizedContractNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateContractNumber)
                    return (false, "Contract number sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateInsuranceProviderCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstInsuranceProvider>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.InsuranceProviderCode.StartsWith(InsuranceProviderCodePrefix))
                .Select(x => x.InsuranceProviderCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractInsuranceProviderSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{InsuranceProviderCodePrefix}{nextNumber.ToString().PadLeft(InsuranceProviderCodeDigitLength, '0')}";
        }

        private static int? TryExtractInsuranceProviderSequenceNumber(string insuranceProviderCode)
        {
            if (string.IsNullOrWhiteSpace(insuranceProviderCode))
                return null;

            if (!insuranceProviderCode.StartsWith(InsuranceProviderCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = insuranceProviderCode[InsuranceProviderCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstInsuranceProvider> ApplyDateFilter(
            IQueryable<MstInsuranceProvider> query,
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var now = DateTime.UtcNow.Date;

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

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }
    }
}
