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
using System.Data;
using System.Globalization;
using System.Security.Claims;

using ResponseCompanyGuarantorPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.CompanyGuarantorResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/company-guarantors")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Company Guarantor",
        AreaName = "Administrator",
        ControllerName = "CompanyGuarantor",
        Description = "Administrator master data company guarantor",
        SortOrder = 15
    )]
    [Tags("Administrator / Master Data / Company Guarantor")]
    public class CompanyGuarantorController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string CompanyGuarantorCodePrefix = "CG-RSMMC-";
        private const int CompanyGuarantorCodeDigitLength = 5;

        private const string ContractStatusActive = "active";
        private const string ContractStatusExpired = "expired";
        private const string ContractStatusUpcoming = "upcoming";
        private const string ContractStatusNoContract = "noContract";

        private static readonly List<string> AllowedGuarantorTypes = new()
        {
            "Corporate",
            "Government",
            "Foundation",
            "School",
            "Other"
        };

        private static readonly List<string> AllowedBillingMethods = new()
        {
            "Invoice",
            "Deposit",
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

        public CompanyGuarantorController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor",
            Description = "Melihat metadata filter company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new CompanyGuarantorFilterMetadataResponse
            {
                DefaultFilter = new CompanyGuarantorDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<CompanyGuarantorSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "companyGuarantorCode", Label = "Kode company guarantor" },
                    new() { Value = "companyGuarantorName", Label = "Nama company guarantor" },
                    new() { Value = "companyGroupName", Label = "Group company" },
                    new() { Value = "guarantorType", Label = "Tipe guarantor" },
                    new() { Value = "billingMethod", Label = "Metode billing" },
                    new() { Value = "creditLimitAmount", Label = "Credit limit" },
                    new() { Value = "currentOutstandingAmount", Label = "Outstanding" },
                    new() { Value = "paymentDueDays", Label = "Payment due days" },
                    new() { Value = "contractStartDate", Label = "Tanggal mulai kontrak" },
                    new() { Value = "contractEndDate", Label = "Tanggal akhir kontrak" },
                    new() { Value = "isUsingCompanyTariffBook", Label = "Pakai tarif company" },
                    new() { Value = "isNeedGuaranteeLetter", Label = "Butuh guarantee letter" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                GuarantorTypeOptions = BuildAllowedStringOptions(AllowedGuarantorTypes),
                BillingMethodOptions = BuildAllowedStringOptions(AllowedBillingMethods),
                ContractStatusOptions = BuildContractStatusOptions(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
                ResetButtonLabel = "Reset"
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantor.GetFilterMetadata",
                "Mengambil metadata filter company guarantor.",
                result
            );

            return Ok(ApiResponse<CompanyGuarantorFilterMetadataResponse>.Ok(
                result,
                "Metadata filter company guarantor berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction(
            "Read",
            "Read Company Guarantor",
            Description = "Melihat ringkasan company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;
            var query = BuildBaseQuery();

            var result = new CompanyGuarantorSummaryResponse
            {
                TotalCompanyGuarantor = await query.CountAsync(),
                ActiveCompanyGuarantor = await query.CountAsync(x => x.IsActive),
                InactiveCompanyGuarantor = await query.CountAsync(x => !x.IsActive),
                CorporateGuarantor = await query.CountAsync(x => x.GuarantorType == "Corporate"),
                GovernmentGuarantor = await query.CountAsync(x => x.GuarantorType == "Government"),
                FoundationGuarantor = await query.CountAsync(x => x.GuarantorType == "Foundation"),
                SchoolGuarantor = await query.CountAsync(x => x.GuarantorType == "School"),
                OtherGuarantor = await query.CountAsync(x => x.GuarantorType == "Other"),
                InvoiceBillingGuarantor = await query.CountAsync(x => x.BillingMethod == "Invoice"),
                DepositBillingGuarantor = await query.CountAsync(x => x.BillingMethod == "Deposit"),
                MixedBillingGuarantor = await query.CountAsync(x => x.BillingMethod == "Mixed"),
                UsingCompanyTariffBookGuarantor = await query.CountAsync(x => x.IsUsingCompanyTariffBook),
                UsingHospitalTariffGuarantor = await query.CountAsync(x => x.IsUsingHospitalTariff),
                NeedGuaranteeLetterGuarantor = await query.CountAsync(x => x.IsNeedGuaranteeLetter),
                NeedEmployeeVerificationGuarantor = await query.CountAsync(x => x.IsNeedEmployeeVerification),
                NeedApprovalForProcedureGuarantor = await query.CountAsync(x => x.IsNeedApprovalForProcedure),
                NeedApprovalForDrugGuarantor = await query.CountAsync(x => x.IsNeedApprovalForDrug),
                CoverageLimitedByEmployeeGradeGuarantor = await query.CountAsync(x => x.IsCoverageLimitedByEmployeeGrade),
                AllowExcessPaymentByPatientGuarantor = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
                ActiveContractGuarantor = await query.CountAsync(x =>
                    (x.ContractStartDate.HasValue || x.ContractEndDate.HasValue) &&
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= today) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= today)),
                ExpiredContractGuarantor = await query.CountAsync(x =>
                    x.ContractEndDate.HasValue && x.ContractEndDate.Value.Date < today),
                UpcomingContractGuarantor = await query.CountAsync(x =>
                    x.ContractStartDate.HasValue && x.ContractStartDate.Value.Date > today),
                NoContractDateGuarantor = await query.CountAsync(x =>
                    !x.ContractStartDate.HasValue && !x.ContractEndDate.HasValue)
            };

            return Ok(ApiResponse<CompanyGuarantorSummaryResponse>.Ok(
                result,
                "Ringkasan company guarantor berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseCompanyGuarantorPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Company Guarantor",
            Description = "Melihat data company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantors(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? guarantorType,
            [FromQuery] string? billingMethod,
            [FromQuery] string? contractStatus,
            [FromQuery] bool? isUsingCompanyTariffBook,
            [FromQuery] bool? isUsingHospitalTariff,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedEmployeeVerification,
            [FromQuery] bool? isNeedApprovalForProcedure,
            [FromQuery] bool? isNeedApprovalForDrug,
            [FromQuery] bool? isCoverageLimitedByEmployeeGrade,
            [FromQuery] bool? isAllowExcessPaymentByPatient,
            [FromQuery] bool? hasCreditLimit,
            [FromQuery] bool? hasOutstanding,
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

            var filterValidation = ValidateFilterValues(guarantorType, billingMethod, contractStatus);

            if (!filterValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    filterValidation.ErrorMessage ?? "Filter company guarantor tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                search,
                isActive,
                guarantorType,
                billingMethod,
                contractStatus,
                isUsingCompanyTariffBook,
                isUsingHospitalTariff,
                isNeedGuaranteeLetter,
                isNeedEmployeeVerification,
                isNeedApprovalForProcedure,
                isNeedApprovalForDrug,
                isCoverageLimitedByEmployeeGrade,
                isAllowExcessPaymentByPatient,
                hasCreditLimit,
                hasOutstanding
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

            var result = new ResponseCompanyGuarantorPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseCompanyGuarantorPagedResult>.Ok(
                result,
                "Data company guarantor berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorOptionPagedResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Company Guarantor",
            Description = "Melihat data pilihan company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantorOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? guarantorType = null,
            [FromQuery] string? billingMethod = null,
            [FromQuery] string? contractStatus = null,
            [FromQuery] bool? isUsingCompanyTariffBook = null,
            [FromQuery] bool? isUsingHospitalTariff = null,
            [FromQuery] bool? isNeedGuaranteeLetter = null,
            [FromQuery] bool? isNeedEmployeeVerification = null,
            [FromQuery] bool? isNeedApprovalForProcedure = null,
            [FromQuery] bool? isNeedApprovalForDrug = null,
            [FromQuery] bool? isCoverageLimitedByEmployeeGrade = null,
            [FromQuery] bool? isAllowExcessPaymentByPatient = null,
            [FromQuery] bool? hasCreditLimit = null,
            [FromQuery] bool? hasOutstanding = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var filterValidation = ValidateFilterValues(guarantorType, billingMethod, contractStatus);

            if (!filterValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    filterValidation.ErrorMessage ?? "Filter company guarantor tidak valid."
                ));
            }

            var useOnlyActive = activeOnly ?? onlyActive;

            var query = BuildBaseQuery();
            query = ApplyStandardFilter(
                query,
                search,
                useOnlyActive ? true : null,
                guarantorType,
                billingMethod,
                contractStatus,
                isUsingCompanyTariffBook,
                isUsingHospitalTariff,
                isNeedGuaranteeLetter,
                isNeedEmployeeVerification,
                isNeedApprovalForProcedure,
                isNeedApprovalForDrug,
                isCoverageLimitedByEmployeeGrade,
                isAllowExcessPaymentByPatient,
                hasCreditLimit,
                hasOutstanding
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CompanyGuarantorName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

            var result = new CompanyGuarantorOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<CompanyGuarantorOptionPagedResponse>.Ok(
                result,
                "Data pilihan company guarantor berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Read",
            "Read Company Guarantor",
            Description = "Melihat detail company guarantor",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantorById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Company guarantor tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

            return Ok(ApiResponse<CompanyGuarantorDetailResponse>.Ok(
                data,
                "Detail company guarantor berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Create",
            "Create Company Guarantor",
            Description = "Membuat data company guarantor",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
        [AccessPermission("CompanyGuarantor", "Create")]
        public async Task<IActionResult> CreateCompanyGuarantor([FromBody] CreateCompanyGuarantorRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                request: request
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data company guarantor tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedCompanyGuarantorCode = await GenerateCompanyGuarantorCodeAsync();

            var entity = new MstCompanyGuarantor
            {
                Id = Guid.NewGuid(),
                CompanyGuarantorCode = generatedCompanyGuarantorCode,
                CompanyGuarantorName = request.CompanyGuarantorName.Trim(),
                CompanyGroupName = NormalizeNullableText(request.CompanyGroupName),
                GuarantorType = NormalizeGuarantorType(request.GuarantorType),
                BillingMethod = NormalizeBillingMethod(request.BillingMethod),
                ExternalCompanyCode = NormalizeUpperNullableText(request.ExternalCompanyCode),
                IntegrationCode = NormalizeUpperNullableText(request.IntegrationCode),
                ContractNumber = NormalizeNullableText(request.ContractNumber),
                ContractStartDate = request.ContractStartDate,
                ContractEndDate = request.ContractEndDate,
                IsUsingCompanyTariffBook = request.IsUsingCompanyTariffBook,
                IsUsingHospitalTariff = request.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = request.IsNeedEmployeeVerification,
                IsNeedApprovalForProcedure = request.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = request.IsNeedApprovalForDrug,
                IsCoverageLimitedByEmployeeGrade = request.IsCoverageLimitedByEmployeeGrade,
                IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient,
                CreditLimitAmount = request.CreditLimitAmount,
                CurrentOutstandingAmount = request.CurrentOutstandingAmount,
                PaymentDueDays = request.PaymentDueDays,
                PicName = NormalizeNullableText(request.PicName),
                PicPhoneNumber = NormalizeNullableText(request.PicPhoneNumber),
                PicWhatsAppNumber = NormalizeNullableText(request.PicWhatsAppNumber),
                PicEmail = NormalizeLowerNullableText(request.PicEmail),
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

            _dbContext.Set<MstCompanyGuarantor>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });

            var result = new CompanyGuarantorCreateResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildStringLabel(entity.GuarantorType),
                BillingMethod = entity.BillingMethod,
                BillingMethodName = BuildStringLabel(entity.BillingMethod),
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantor.CreateCompanyGuarantor",
                "Membuat data company guarantor.",
                result
            );

            return Ok(ApiResponse<CompanyGuarantorCreateResponse>.Ok(
                result,
                "Company guarantor berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Company Guarantor",
            Description = "Mengubah data company guarantor",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("CompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateCompanyGuarantor(
            Guid id,
            [FromBody] UpdateCompanyGuarantorRequest request)
        {
            var entity = await _dbContext.Set<MstCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Company guarantor tidak ditemukan."
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
                    validation.ErrorMessage ?? "Data company guarantor tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CompanyGuarantorName = request.CompanyGuarantorName.Trim();
            entity.CompanyGroupName = NormalizeNullableText(request.CompanyGroupName);
            entity.GuarantorType = NormalizeGuarantorType(request.GuarantorType);
            entity.BillingMethod = NormalizeBillingMethod(request.BillingMethod);
            entity.ExternalCompanyCode = NormalizeUpperNullableText(request.ExternalCompanyCode);
            entity.IntegrationCode = NormalizeUpperNullableText(request.IntegrationCode);
            entity.ContractNumber = NormalizeNullableText(request.ContractNumber);
            entity.ContractStartDate = request.ContractStartDate;
            entity.ContractEndDate = request.ContractEndDate;
            entity.IsUsingCompanyTariffBook = request.IsUsingCompanyTariffBook;
            entity.IsUsingHospitalTariff = request.IsUsingHospitalTariff;
            entity.IsNeedGuaranteeLetter = request.IsNeedGuaranteeLetter;
            entity.IsNeedEmployeeVerification = request.IsNeedEmployeeVerification;
            entity.IsNeedApprovalForProcedure = request.IsNeedApprovalForProcedure;
            entity.IsNeedApprovalForDrug = request.IsNeedApprovalForDrug;
            entity.IsCoverageLimitedByEmployeeGrade = request.IsCoverageLimitedByEmployeeGrade;
            entity.IsAllowExcessPaymentByPatient = request.IsAllowExcessPaymentByPatient;
            entity.CreditLimitAmount = request.CreditLimitAmount;
            entity.CurrentOutstandingAmount = request.CurrentOutstandingAmount;
            entity.PaymentDueDays = request.PaymentDueDays;
            entity.PicName = NormalizeNullableText(request.PicName);
            entity.PicPhoneNumber = NormalizeNullableText(request.PicPhoneNumber);
            entity.PicWhatsAppNumber = NormalizeNullableText(request.PicWhatsAppNumber);
            entity.PicEmail = NormalizeLowerNullableText(request.PicEmail);
            entity.OfficeAddress = NormalizeNullableText(request.OfficeAddress);
            entity.LogoPath = NormalizeNullableText(request.LogoPath);
            entity.BillingInstruction = NormalizeNullableText(request.BillingInstruction);
            entity.ClaimInstruction = NormalizeNullableText(request.ClaimInstruction);
            entity.Description = NormalizeNullableText(request.Description);
            entity.SortOrder = request.SortOrder;
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new CompanyGuarantorUpdateResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildStringLabel(entity.GuarantorType),
                BillingMethod = entity.BillingMethod,
                BillingMethodName = BuildStringLabel(entity.BillingMethod),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantor.UpdateCompanyGuarantor",
                "Mengubah data company guarantor.",
                result
            );

            return Ok(ApiResponse<CompanyGuarantorUpdateResponse>.Ok(
                result,
                "Company guarantor berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Update",
            "Update Company Guarantor Status",
            Description = "Mengubah status company guarantor",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("CompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateCompanyGuarantorStatus(
            Guid id,
            [FromBody] UpdateCompanyGuarantorStatusRequest request)
        {
            var entity = await _dbContext.Set<MstCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Company guarantor tidak ditemukan."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });

            var result = new CompanyGuarantorUpdateResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildStringLabel(entity.GuarantorType),
                BillingMethod = entity.BillingMethod,
                BillingMethodName = BuildStringLabel(entity.BillingMethod),
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantor.UpdateCompanyGuarantorStatus",
                "Mengubah status company guarantor.",
                result
            );

            return Ok(ApiResponse<CompanyGuarantorUpdateResponse>.Ok(
                result,
                "Status company guarantor berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Company Guarantor",
            Description = "Menghapus data company guarantor",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("CompanyGuarantor", "Delete")]
        public async Task<IActionResult> DeleteCompanyGuarantor(
            Guid id,
            [FromBody] DeleteCompanyGuarantorRequest? request = null)
        {
            var entity = await _dbContext.Set<MstCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Company guarantor tidak ditemukan."
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

            var result = new CompanyGuarantorDeleteResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantor.DeleteCompanyGuarantor",
                "Menghapus data company guarantor.",
                result
            );

            return Ok(ApiResponse<CompanyGuarantorDeleteResponse>.Ok(
                result,
                "Company guarantor berhasil dihapus."
            ));
        }

        private IQueryable<MstCompanyGuarantor> BuildBaseQuery()
        {
            return _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstCompanyGuarantor> ApplyDateFilter(
            IQueryable<MstCompanyGuarantor> query,
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

        private static IQueryable<MstCompanyGuarantor> ApplyStandardFilter(
            IQueryable<MstCompanyGuarantor> query,
            string? search,
            bool? isActive,
            string? guarantorType,
            string? billingMethod,
            string? contractStatus,
            bool? isUsingCompanyTariffBook,
            bool? isUsingHospitalTariff,
            bool? isNeedGuaranteeLetter,
            bool? isNeedEmployeeVerification,
            bool? isNeedApprovalForProcedure,
            bool? isNeedApprovalForDrug,
            bool? isCoverageLimitedByEmployeeGrade,
            bool? isAllowExcessPaymentByPatient,
            bool? hasCreditLimit,
            bool? hasOutstanding)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CompanyGuarantorCode.ToLower().Contains(keyword) ||
                    x.CompanyGuarantorName.ToLower().Contains(keyword) ||
                    x.GuarantorType.ToLower().Contains(keyword) ||
                    x.BillingMethod.ToLower().Contains(keyword) ||
                    (x.CompanyGroupName != null && x.CompanyGroupName.ToLower().Contains(keyword)) ||
                    (x.ExternalCompanyCode != null && x.ExternalCompanyCode.ToLower().Contains(keyword)) ||
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

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(guarantorType))
            {
                var normalizedGuarantorType = NormalizeGuarantorType(guarantorType);
                query = query.Where(x => x.GuarantorType == normalizedGuarantorType);
            }

            if (!string.IsNullOrWhiteSpace(billingMethod))
            {
                var normalizedBillingMethod = NormalizeBillingMethod(billingMethod);
                query = query.Where(x => x.BillingMethod == normalizedBillingMethod);
            }

            if (!string.IsNullOrWhiteSpace(contractStatus))
            {
                query = ApplyContractStatusFilter(query, contractStatus);
            }

            if (isUsingCompanyTariffBook.HasValue)
            {
                query = query.Where(x => x.IsUsingCompanyTariffBook == isUsingCompanyTariffBook.Value);
            }

            if (isUsingHospitalTariff.HasValue)
            {
                query = query.Where(x => x.IsUsingHospitalTariff == isUsingHospitalTariff.Value);
            }

            if (isNeedGuaranteeLetter.HasValue)
            {
                query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);
            }

            if (isNeedEmployeeVerification.HasValue)
            {
                query = query.Where(x => x.IsNeedEmployeeVerification == isNeedEmployeeVerification.Value);
            }

            if (isNeedApprovalForProcedure.HasValue)
            {
                query = query.Where(x => x.IsNeedApprovalForProcedure == isNeedApprovalForProcedure.Value);
            }

            if (isNeedApprovalForDrug.HasValue)
            {
                query = query.Where(x => x.IsNeedApprovalForDrug == isNeedApprovalForDrug.Value);
            }

            if (isCoverageLimitedByEmployeeGrade.HasValue)
            {
                query = query.Where(x => x.IsCoverageLimitedByEmployeeGrade == isCoverageLimitedByEmployeeGrade.Value);
            }

            if (isAllowExcessPaymentByPatient.HasValue)
            {
                query = query.Where(x => x.IsAllowExcessPaymentByPatient == isAllowExcessPaymentByPatient.Value);
            }

            if (hasCreditLimit.HasValue)
            {
                query = hasCreditLimit.Value
                    ? query.Where(x => x.CreditLimitAmount.HasValue && x.CreditLimitAmount.Value > 0)
                    : query.Where(x => !x.CreditLimitAmount.HasValue || x.CreditLimitAmount.Value <= 0);
            }

            if (hasOutstanding.HasValue)
            {
                query = hasOutstanding.Value
                    ? query.Where(x => x.CurrentOutstandingAmount.HasValue && x.CurrentOutstandingAmount.Value > 0)
                    : query.Where(x => !x.CurrentOutstandingAmount.HasValue || x.CurrentOutstandingAmount.Value <= 0);
            }

            return query;
        }

        private static IQueryable<MstCompanyGuarantor> ApplyContractStatusFilter(
            IQueryable<MstCompanyGuarantor> query,
            string contractStatus)
        {
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

        private static IOrderedQueryable<MstCompanyGuarantor> ApplySorting(
            IQueryable<MstCompanyGuarantor> query,
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

                "companyguarantorcode" => isDescending
                    ? query.OrderByDescending(x => x.CompanyGuarantorCode)
                    : query.OrderBy(x => x.CompanyGuarantorCode),

                "companyguarantorname" => isDescending
                    ? query.OrderByDescending(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.CompanyGuarantorName),

                "companygroupname" => isDescending
                    ? query.OrderByDescending(x => x.CompanyGroupName).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.CompanyGroupName).ThenBy(x => x.CompanyGuarantorName),

                "guarantortype" => isDescending
                    ? query.OrderByDescending(x => x.GuarantorType).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.GuarantorType).ThenBy(x => x.CompanyGuarantorName),

                "billingmethod" => isDescending
                    ? query.OrderByDescending(x => x.BillingMethod).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.BillingMethod).ThenBy(x => x.CompanyGuarantorName),

                "creditlimitamount" => isDescending
                    ? query.OrderByDescending(x => x.CreditLimitAmount).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.CreditLimitAmount).ThenBy(x => x.CompanyGuarantorName),

                "currentoutstandingamount" => isDescending
                    ? query.OrderByDescending(x => x.CurrentOutstandingAmount).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.CurrentOutstandingAmount).ThenBy(x => x.CompanyGuarantorName),

                "paymentduedays" => isDescending
                    ? query.OrderByDescending(x => x.PaymentDueDays).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.PaymentDueDays).ThenBy(x => x.CompanyGuarantorName),

                "contractstartdate" => isDescending
                    ? query.OrderByDescending(x => x.ContractStartDate).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.ContractStartDate).ThenBy(x => x.CompanyGuarantorName),

                "contractenddate" => isDescending
                    ? query.OrderByDescending(x => x.ContractEndDate).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.ContractEndDate).ThenBy(x => x.CompanyGuarantorName),

                "isusingcompanytariffbook" => isDescending
                    ? query.OrderByDescending(x => x.IsUsingCompanyTariffBook).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.IsUsingCompanyTariffBook).ThenBy(x => x.CompanyGuarantorName),

                "isneedguaranteeletter" => isDescending
                    ? query.OrderByDescending(x => x.IsNeedGuaranteeLetter).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.IsNeedGuaranteeLetter).ThenBy(x => x.CompanyGuarantorName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.CompanyGuarantorName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.CompanyGuarantorName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateCompanyGuarantorRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyGuarantorName))
            {
                return (false, "Nama company guarantor wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.GuarantorType))
            {
                return (false, "Tipe guarantor wajib diisi.");
            }

            if (!IsAllowedValue(AllowedGuarantorTypes, request.GuarantorType))
            {
                return (false, "Tipe guarantor tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (string.IsNullOrWhiteSpace(request.BillingMethod))
            {
                return (false, "Metode billing wajib diisi.");
            }

            if (!IsAllowedValue(AllowedBillingMethods, request.BillingMethod))
            {
                return (false, "Metode billing tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.ContractStartDate.HasValue &&
                request.ContractEndDate.HasValue &&
                request.ContractEndDate.Value.Date < request.ContractStartDate.Value.Date)
            {
                return (false, "Tanggal akhir kontrak tidak boleh lebih kecil dari tanggal mulai kontrak.");
            }

            if (request.CreditLimitAmount.HasValue && request.CreditLimitAmount.Value < 0)
            {
                return (false, "Credit limit tidak boleh lebih kecil dari 0.");
            }

            if (request.CurrentOutstandingAmount.HasValue && request.CurrentOutstandingAmount.Value < 0)
            {
                return (false, "Outstanding tidak boleh lebih kecil dari 0.");
            }

            if (request.PaymentDueDays < 0)
            {
                return (false, "Payment due days tidak boleh lebih kecil dari 0.");
            }

            if (request.PaymentDueDays > 3650)
            {
                return (false, "Payment due days tidak boleh lebih dari 3650.");
            }

            if (request.SortOrder < 0)
            {
                return (false, "Urutan tidak boleh kurang dari 0.");
            }

            if (!string.IsNullOrWhiteSpace(request.PicEmail) && !request.PicEmail.Contains('@'))
            {
                return (false, "Format email PIC tidak valid.");
            }

            var normalizedName = request.CompanyGuarantorName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.CompanyGuarantorName.ToLower() == normalizedName);

            if (excludeId.HasValue)
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama company guarantor sudah digunakan.");
            }

            var externalCompanyCode = NormalizeUpperNullableText(request.ExternalCompanyCode);

            if (!string.IsNullOrWhiteSpace(externalCompanyCode))
            {
                var duplicateExternalCompanyCodeQuery = _dbContext.Set<MstCompanyGuarantor>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ExternalCompanyCode != null &&
                        x.ExternalCompanyCode.ToLower() == externalCompanyCode.ToLower());

                if (excludeId.HasValue)
                {
                    duplicateExternalCompanyCodeQuery = duplicateExternalCompanyCodeQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateExternalCompanyCodeQuery.AnyAsync())
                {
                    return (false, "External company code sudah digunakan.");
                }
            }

            var integrationCode = NormalizeUpperNullableText(request.IntegrationCode);

            if (!string.IsNullOrWhiteSpace(integrationCode))
            {
                var duplicateIntegrationCodeQuery = _dbContext.Set<MstCompanyGuarantor>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.IntegrationCode != null &&
                        x.IntegrationCode.ToLower() == integrationCode.ToLower());

                if (excludeId.HasValue)
                {
                    duplicateIntegrationCodeQuery = duplicateIntegrationCodeQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateIntegrationCodeQuery.AnyAsync())
                {
                    return (false, "Integration code sudah digunakan.");
                }
            }

            var contractNumber = NormalizeNullableText(request.ContractNumber);

            if (!string.IsNullOrWhiteSpace(contractNumber))
            {
                var duplicateContractNumberQuery = _dbContext.Set<MstCompanyGuarantor>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.ContractNumber != null &&
                        x.ContractNumber.ToLower() == contractNumber.ToLower());

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

        private static (bool IsValid, string? ErrorMessage) ValidateFilterValues(
            string? guarantorType,
            string? billingMethod,
            string? contractStatus)
        {
            if (!string.IsNullOrWhiteSpace(guarantorType) && !IsAllowedValue(AllowedGuarantorTypes, guarantorType))
            {
                return (false, "Filter tipe guarantor tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!string.IsNullOrWhiteSpace(billingMethod) && !IsAllowedValue(AllowedBillingMethods, billingMethod))
            {
                return (false, "Filter metode billing tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (!string.IsNullOrWhiteSpace(contractStatus) && !IsAllowedValue(AllowedContractStatuses, contractStatus))
            {
                return (false, "Filter status kontrak tidak valid. Gunakan active, expired, upcoming, atau noContract.");
            }

            return (true, null);
        }

        private async Task<string> GenerateCompanyGuarantorCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstCompanyGuarantor>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CompanyGuarantorCode.StartsWith(CompanyGuarantorCodePrefix))
                .Select(x => x.CompanyGuarantorCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractCompanyGuarantorSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CompanyGuarantorCodePrefix + nextNumber.ToString("D" + CompanyGuarantorCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractCompanyGuarantorSequenceNumber(string companyGuarantorCode)
        {
            if (string.IsNullOrWhiteSpace(companyGuarantorCode))
            {
                return null;
            }

            if (!companyGuarantorCode.StartsWith(CompanyGuarantorCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = companyGuarantorCode[CompanyGuarantorCodePrefix.Length..];

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

        private static CompanyGuarantorResponse MapResponse(
            MstCompanyGuarantor entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var contractStatus = BuildContractStatus(entity.ContractStartDate, entity.ContractEndDate);

            return new CompanyGuarantorResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CompanyGroupName = entity.CompanyGroupName,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildStringLabel(entity.GuarantorType),
                BillingMethod = entity.BillingMethod,
                BillingMethodName = BuildStringLabel(entity.BillingMethod),
                ExternalCompanyCode = entity.ExternalCompanyCode,
                IntegrationCode = entity.IntegrationCode,
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractStatus = contractStatus,
                ContractStatusName = BuildContractStatusLabel(contractStatus),
                IsUsingCompanyTariffBook = entity.IsUsingCompanyTariffBook,
                IsUsingHospitalTariff = entity.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = entity.IsNeedEmployeeVerification,
                IsNeedApprovalForProcedure = entity.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = entity.IsNeedApprovalForDrug,
                IsCoverageLimitedByEmployeeGrade = entity.IsCoverageLimitedByEmployeeGrade,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                CreditLimitAmount = entity.CreditLimitAmount,
                CurrentOutstandingAmount = entity.CurrentOutstandingAmount,
                PaymentDueDays = entity.PaymentDueDays,
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

        private static CompanyGuarantorDetailResponse MapDetailResponse(
            MstCompanyGuarantor entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            var contractStatus = BuildContractStatus(entity.ContractStartDate, entity.ContractEndDate);

            return new CompanyGuarantorDetailResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CompanyGroupName = entity.CompanyGroupName,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildStringLabel(entity.GuarantorType),
                BillingMethod = entity.BillingMethod,
                BillingMethodName = BuildStringLabel(entity.BillingMethod),
                ExternalCompanyCode = entity.ExternalCompanyCode,
                IntegrationCode = entity.IntegrationCode,
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractStatus = contractStatus,
                ContractStatusName = BuildContractStatusLabel(contractStatus),
                IsUsingCompanyTariffBook = entity.IsUsingCompanyTariffBook,
                IsUsingHospitalTariff = entity.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = entity.IsNeedEmployeeVerification,
                IsNeedApprovalForProcedure = entity.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = entity.IsNeedApprovalForDrug,
                IsCoverageLimitedByEmployeeGrade = entity.IsCoverageLimitedByEmployeeGrade,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                CreditLimitAmount = entity.CreditLimitAmount,
                CurrentOutstandingAmount = entity.CurrentOutstandingAmount,
                PaymentDueDays = entity.PaymentDueDays,
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

        private static CompanyGuarantorOptionResponse MapOptionResponse(
            MstCompanyGuarantor entity)
        {
            var contractStatus = BuildContractStatus(entity.ContractStartDate, entity.ContractEndDate);

            return new CompanyGuarantorOptionResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CompanyGroupName = entity.CompanyGroupName,
                GuarantorType = entity.GuarantorType,
                GuarantorTypeName = BuildStringLabel(entity.GuarantorType),
                BillingMethod = entity.BillingMethod,
                BillingMethodName = BuildStringLabel(entity.BillingMethod),
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
                ContractStatus = contractStatus,
                ContractStatusName = BuildContractStatusLabel(contractStatus),
                IsUsingCompanyTariffBook = entity.IsUsingCompanyTariffBook,
                IsUsingHospitalTariff = entity.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = entity.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = entity.IsNeedEmployeeVerification,
                IsNeedApprovalForProcedure = entity.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = entity.IsNeedApprovalForDrug,
                IsCoverageLimitedByEmployeeGrade = entity.IsCoverageLimitedByEmployeeGrade,
                IsAllowExcessPaymentByPatient = entity.IsAllowExcessPaymentByPatient,
                CreditLimitAmount = entity.CreditLimitAmount,
                CurrentOutstandingAmount = entity.CurrentOutstandingAmount,
                PaymentDueDays = entity.PaymentDueDays,
                SortOrder = entity.SortOrder
            };
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

        private static List<CompanyGuarantorCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<CompanyGuarantorCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<CompanyGuarantorStringOptionResponse> BuildAllowedStringOptions(
            IEnumerable<string> values)
        {
            return values
                .Select(x => new CompanyGuarantorStringOptionResponse
                {
                    Value = x,
                    Label = BuildStringLabel(x)
                })
                .ToList();
        }

        private static List<CompanyGuarantorStringOptionResponse> BuildContractStatusOptions()
        {
            return new List<CompanyGuarantorStringOptionResponse>
            {
                new() { Value = ContractStatusActive, Label = "Active", Description = "Kontrak sedang berjalan." },
                new() { Value = ContractStatusExpired, Label = "Expired", Description = "Kontrak sudah melewati tanggal akhir." },
                new() { Value = ContractStatusUpcoming, Label = "Upcoming", Description = "Kontrak belum mulai." },
                new() { Value = ContractStatusNoContract, Label = "No Contract", Description = "Tanggal kontrak belum diisi." }
            };
        }

        private static string NormalizeGuarantorType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedGuarantorTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "Corporate";
        }

        private static string NormalizeBillingMethod(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedBillingMethods
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "Invoice";
        }

        private static string NormalizeContractStatus(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedContractStatuses
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? string.Empty;
        }

        private static string BuildContractStatus(DateTime? contractStartDate, DateTime? contractEndDate)
        {
            var today = DateTime.UtcNow.Date;

            if (!contractStartDate.HasValue && !contractEndDate.HasValue)
            {
                return ContractStatusNoContract;
            }

            if (contractEndDate.HasValue && contractEndDate.Value.Date < today)
            {
                return ContractStatusExpired;
            }

            if (contractStartDate.HasValue && contractStartDate.Value.Date > today)
            {
                return ContractStatusUpcoming;
            }

            return ContractStatusActive;
        }

        private static string BuildContractStatusLabel(string value)
        {
            return value switch
            {
                ContractStatusActive => "Active",
                ContractStatusExpired => "Expired",
                ContractStatusUpcoming => "Upcoming",
                ContractStatusNoContract => "No Contract",
                _ => BuildStringLabel(value)
            };
        }

        private static string BuildStringLabel(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static bool IsAllowedValue(IEnumerable<string> allowedValues, string value)
        {
            return allowedValues.Any(x => string.Equals(
                x,
                value.Trim(),
                StringComparison.OrdinalIgnoreCase
            ));
        }

        private static List<CompanyGuarantorQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<CompanyGuarantorQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, group, tipe, metode billing, kode integrasi, nomor kontrak, PIC, alamat, instruksi, atau deskripsi.", Example = "PT Sehat" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "guarantorType", Type = "string", Description = "Filter tipe guarantor.", Example = "Corporate" },
                new() { Name = "billingMethod", Type = "string", Description = "Filter metode billing.", Example = "Invoice" },
                new() { Name = "contractStatus", Type = "string", Description = "Filter status kontrak: active, expired, upcoming, noContract.", Example = "active" },
                new() { Name = "isUsingCompanyTariffBook", Type = "bool", Description = "Filter penggunaan tarif company.", Example = "true" },
                new() { Name = "isUsingHospitalTariff", Type = "bool", Description = "Filter penggunaan tarif rumah sakit.", Example = "false" },
                new() { Name = "isNeedGuaranteeLetter", Type = "bool", Description = "Filter butuh guarantee letter.", Example = "true" },
                new() { Name = "isNeedEmployeeVerification", Type = "bool", Description = "Filter butuh verifikasi karyawan.", Example = "true" },
                new() { Name = "isNeedApprovalForProcedure", Type = "bool", Description = "Filter butuh approval tindakan.", Example = "true" },
                new() { Name = "isNeedApprovalForDrug", Type = "bool", Description = "Filter butuh approval obat.", Example = "false" },
                new() { Name = "isCoverageLimitedByEmployeeGrade", Type = "bool", Description = "Filter coverage berdasarkan grade karyawan.", Example = "true" },
                new() { Name = "isAllowExcessPaymentByPatient", Type = "bool", Description = "Filter selisih boleh dibayar pasien.", Example = "true" },
                new() { Name = "hasCreditLimit", Type = "bool", Description = "Filter penjamin yang memiliki credit limit lebih dari 0 atau tidak.", Example = "true" },
                new() { Name = "hasOutstanding", Type = "bool", Description = "Filter penjamin yang memiliki outstanding lebih dari 0 atau tidak.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<CompanyGuarantorFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<CompanyGuarantorFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<CompanyGuarantorFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<CompanyGuarantorFormFieldMetadataResponse>
            {
                new() { Name = "companyGuarantorCode", Label = "Kode Company Guarantor", Section = "Basic", InputType = "readonly", IsRequiredOnCreate = false, IsRequiredOnUpdate = false, RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format CG-RSMMC-00001.", Example = "CG-RSMMC-00001", SortOrder = 1 },
                new() { Name = "companyGuarantorName", Label = "Nama Company Guarantor", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 200, Example = "PT Sehat Bersama", SortOrder = 2 },
                new() { Name = "companyGroupName", Label = "Group Company", Section = "Basic", InputType = "text", MaxLength = 100, Example = "Sehat Group", SortOrder = 3 },
                new() { Name = "guarantorType", Label = "Tipe Guarantor", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "guarantorTypeOptions", SortOrder = 4 },
                new() { Name = "billingMethod", Label = "Metode Billing", Section = "Billing", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", OptionsSource = "billingMethodOptions", SortOrder = 5 },
                new() { Name = "externalCompanyCode", Label = "External Company Code", Section = "Integration", InputType = "text", MaxLength = 50, Example = "EXT-COMP-001", SortOrder = 6 },
                new() { Name = "integrationCode", Label = "Integration Code", Section = "Integration", InputType = "text", MaxLength = 50, Example = "INT-COMP-001", SortOrder = 7 },
                new() { Name = "contractNumber", Label = "Nomor Kontrak", Section = "Contract", InputType = "text", MaxLength = 100, Example = "PKS/001/2026", SortOrder = 8 },
                new() { Name = "contractStartDate", Label = "Tanggal Mulai Kontrak", Section = "Contract", InputType = "date", SortOrder = 9 },
                new() { Name = "contractEndDate", Label = "Tanggal Akhir Kontrak", Section = "Contract", InputType = "date", SortOrder = 10 },
                new() { Name = "isUsingCompanyTariffBook", Label = "Pakai Tarif Company", Section = "Rule", InputType = "switch", SortOrder = 11 },
                new() { Name = "isUsingHospitalTariff", Label = "Pakai Tarif Rumah Sakit", Section = "Rule", InputType = "switch", SortOrder = 12 },
                new() { Name = "isNeedGuaranteeLetter", Label = "Butuh Guarantee Letter", Section = "Rule", InputType = "switch", SortOrder = 13 },
                new() { Name = "isNeedEmployeeVerification", Label = "Butuh Verifikasi Karyawan", Section = "Rule", InputType = "switch", SortOrder = 14 },
                new() { Name = "isNeedApprovalForProcedure", Label = "Butuh Approval Tindakan", Section = "Rule", InputType = "switch", SortOrder = 15 },
                new() { Name = "isNeedApprovalForDrug", Label = "Butuh Approval Obat", Section = "Rule", InputType = "switch", SortOrder = 16 },
                new() { Name = "isCoverageLimitedByEmployeeGrade", Label = "Coverage Sesuai Grade", Section = "Rule", InputType = "switch", SortOrder = 17 },
                new() { Name = "isAllowExcessPaymentByPatient", Label = "Selisih Boleh Dibayar Pasien", Section = "Rule", InputType = "switch", SortOrder = 18 },
                new() { Name = "creditLimitAmount", Label = "Credit Limit", Section = "Finance", InputType = "number", Example = "100000000", SortOrder = 19 },
                new() { Name = "currentOutstandingAmount", Label = "Current Outstanding", Section = "Finance", InputType = "number", Example = "0", SortOrder = 20 },
                new() { Name = "paymentDueDays", Label = "Payment Due Days", Section = "Finance", InputType = "number", Example = "30", SortOrder = 21 },
                new() { Name = "picName", Label = "Nama PIC", Section = "Contact", InputType = "text", MaxLength = 100, Example = "Budi", SortOrder = 22 },
                new() { Name = "picPhoneNumber", Label = "No. Telepon PIC", Section = "Contact", InputType = "text", MaxLength = 30, Example = "021123456", SortOrder = 23 },
                new() { Name = "picWhatsAppNumber", Label = "No. WhatsApp PIC", Section = "Contact", InputType = "text", MaxLength = 30, Example = "08123456789", SortOrder = 24 },
                new() { Name = "picEmail", Label = "Email PIC", Section = "Contact", InputType = "email", MaxLength = 200, Example = "pic@company.com", SortOrder = 25 },
                new() { Name = "officeAddress", Label = "Alamat Kantor", Section = "Contact", InputType = "textarea", MaxLength = 500, SortOrder = 26 },
                new() { Name = "logoPath", Label = "Logo Path", Section = "Media", InputType = "text", MaxLength = 500, SortOrder = 27 },
                new() { Name = "billingInstruction", Label = "Instruksi Billing", Section = "Instruction", InputType = "textarea", MaxLength = 250, SortOrder = 28 },
                new() { Name = "claimInstruction", Label = "Instruksi Klaim", Section = "Instruction", InputType = "textarea", MaxLength = 250, SortOrder = 29 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 30 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 31 }
            };

            if (isUpdate)
            {
                fields.Add(new CompanyGuarantorFormFieldMetadataResponse
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

        private Guid GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
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

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeUpperNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToUpperInvariant();
        }

        private static string? NormalizeLowerNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
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
