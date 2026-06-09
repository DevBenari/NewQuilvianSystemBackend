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
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat metadata filter insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new InsuranceProviderFilterMetadataResponse
            {
                DefaultFilter = new InsuranceProviderDefaultFilterResponse(),
                CustomPeriods = new List<InsuranceProviderCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
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
                        Label = BuildProviderTypeLabel(x)
                    })
                    .ToList(),
                ClaimMethodOptions = AllowedClaimMethods
                    .OrderBy(x => x)
                    .Select(x => new InsuranceProviderStringOptionResponse
                    {
                        Value = x,
                        Label = BuildClaimMethodLabel(x)
                    })
                    .ToList(),
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
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat data insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviders(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] bool? isActive,
            [FromQuery] string? providerType,
            [FromQuery] string? claimMethod,
            [FromQuery] string? search,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, isActive, providerType, claimMethod, search);

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
        [ProducesResponseType(typeof(ApiResponse<InsuranceProviderOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Insurance Provider",
            Description = "Melihat data pilihan insurance provider",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("InsuranceProvider", "Read")]
        public async Task<IActionResult> GetInsuranceProviderOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? providerType = null,
            [FromQuery] string? claimMethod = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            query = ApplyStandardFilter(
                query,
                onlyActive ? true : null,
                providerType,
                claimMethod,
                search
            );

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
                    ProviderTypeName = BuildProviderTypeLabel(x.ProviderType),
                    ClaimMethod = x.ClaimMethod,
                    ClaimMethodName = BuildClaimMethodLabel(x.ClaimMethod),
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
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime
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
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new InsuranceProviderUpdateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
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

            var result = new InsuranceProviderUpdateResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
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

            var result = new InsuranceProviderDeleteResponse
            {
                Id = entity.Id,
                InsuranceProviderCode = entity.InsuranceProviderCode,
                InsuranceProviderName = entity.InsuranceProviderName,
                DeleteDateTime = entity.DeleteDateTime
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
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            if (startDate.HasValue)
            {
                var start = DateTime.SpecifyKind(startDate.Value.Date, DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime >= start);
            }

            if (endDate.HasValue)
            {
                var end = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < end);
            }

            if (!startDate.HasValue &&
                !endDate.HasValue &&
                !string.IsNullOrWhiteSpace(customPeriod))
            {
                var today = DateTime.UtcNow.Date;

                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        query = query.Where(x =>
                            x.CreateDateTime >= today &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "last7days":
                        query = query.Where(x =>
                            x.CreateDateTime >= today.AddDays(-6) &&
                            x.CreateDateTime < today.AddDays(1));
                        break;

                    case "thismonth":
                        var thisMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        query = query.Where(x =>
                            x.CreateDateTime >= thisMonthStart &&
                            x.CreateDateTime < thisMonthStart.AddMonths(1));
                        break;

                    case "lastmonth":
                        var currentMonthStart = new DateTime(
                            today.Year,
                            today.Month,
                            1,
                            0,
                            0,
                            0,
                            DateTimeKind.Utc
                        );

                        var lastMonthStart = currentMonthStart.AddMonths(-1);

                        query = query.Where(x =>
                            x.CreateDateTime >= lastMonthStart &&
                            x.CreateDateTime < currentMonthStart);
                        break;
                }
            }

            return query;
        }

        private static IQueryable<MstInsuranceProvider> ApplyStandardFilter(
            IQueryable<MstInsuranceProvider> query,
            bool? isActive,
            string? providerType,
            string? claimMethod,
            string? search)
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

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

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
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
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
                    ? query.OrderByDescending(x => x.InsuranceGroupName)
                    : query.OrderBy(x => x.InsuranceGroupName),

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

            if (!AllowedProviderTypes.Contains(request.ProviderType.Trim()))
            {
                return (false, "Tipe provider tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (string.IsNullOrWhiteSpace(request.ClaimMethod))
            {
                return (false, "Metode klaim wajib diisi.");
            }

            if (!AllowedClaimMethods.Contains(request.ClaimMethod.Trim()))
            {
                return (false, "Metode klaim tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.ContractStartDate.HasValue &&
                request.ContractEndDate.HasValue &&
                request.ContractEndDate.Value.Date < request.ContractStartDate.Value.Date)
            {
                return (false, "Tanggal akhir kontrak tidak boleh lebih kecil dari tanggal mulai kontrak.");
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

            var externalProviderCode = NormalizeNullableString(request.ExternalProviderCode);

            if (!string.IsNullOrWhiteSpace(externalProviderCode))
            {
                var normalizedExternalProviderCode = externalProviderCode.ToLower();

                var duplicateExternalProviderCodeQuery = _dbContext.Set<MstInsuranceProvider>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalProviderCode != null &&
                        x.ExternalProviderCode.ToLower() == normalizedExternalProviderCode);

                if (excludeId.HasValue)
                {
                    duplicateExternalProviderCodeQuery = duplicateExternalProviderCodeQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateExternalProviderCodeQuery.AnyAsync())
                {
                    return (false, "External provider code sudah digunakan.");
                }
            }

            var integrationCode = NormalizeNullableString(request.IntegrationCode);

            if (!string.IsNullOrWhiteSpace(integrationCode))
            {
                var normalizedIntegrationCode = integrationCode.ToLower();

                var duplicateIntegrationCodeQuery = _dbContext.Set<MstInsuranceProvider>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IntegrationCode != null &&
                        x.IntegrationCode.ToLower() == normalizedIntegrationCode);

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

            return InsuranceProviderCodePrefix + nextNumber.ToString().PadLeft(InsuranceProviderCodeDigitLength, '0');
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

            return int.TryParse(numberText, out var number)
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
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                BillingInstruction = entity.BillingInstruction,
                ClaimInstruction = entity.ClaimInstruction,
                Description = entity.Description,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
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

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
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

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
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
    }
}
