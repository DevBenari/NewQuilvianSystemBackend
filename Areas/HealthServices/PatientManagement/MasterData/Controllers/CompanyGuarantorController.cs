using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseCompanyGuarantorPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.DTOs.CompanyGuarantorResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/patient-management/master-data/company-guarantors")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PATIENT_MANAGEMENT_MASTER_DATA",
        moduleName: "Health Service Patient Management Master Data",
        displayName: "Company Guarantor",
        AreaName = "HealthServices",
        ControllerName = "CompanyGuarantor",
        Description = "Health service patient management master data company guarantor",
        SortOrder = 15
    )]
    [Tags("Health Services / Patient Management / Master Data / Company Guarantor")]
    public class CompanyGuarantorController : ControllerBase
    {
        private const string LogCategory = "HealthServices.PatientManagement.MasterData";

        private static readonly HashSet<string> AllowedGuarantorTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "Corporate",
            "Government",
            "Foundation",
            "School",
            "Other"
        };

        private static readonly HashSet<string> AllowedBillingMethods = new(StringComparer.OrdinalIgnoreCase)
        {
            "Invoice",
            "Deposit",
            "Mixed"
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
        [AccessAction("Read", "Read Company Guarantor", Description = "Melihat data company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new CompanyGuarantorFilterMetadataResponse
            {
                DefaultFilter = new CompanyGuarantorDefaultFilterResponse(),
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
                    new() { Value = "contractStartDate", Label = "Tanggal mulai kontrak" },
                    new() { Value = "contractEndDate", Label = "Tanggal akhir kontrak" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                GuarantorTypes = AllowedGuarantorTypes.OrderBy(x => x).ToList(),
                BillingMethods = AllowedBillingMethods.OrderBy(x => x).ToList()
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
        [AccessAction("Read", "Read Company Guarantor", Description = "Melihat data company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new CompanyGuarantorSummaryResponse
            {
                TotalCompanyGuarantor = await query.CountAsync(),
                ActiveCompanyGuarantor = await query.CountAsync(x => x.IsActive),
                InactiveCompanyGuarantor = await query.CountAsync(x => !x.IsActive),
                CorporateGuarantor = await query.CountAsync(x => x.GuarantorType == "Corporate"),
                GovernmentGuarantor = await query.CountAsync(x => x.GuarantorType == "Government"),
                FoundationGuarantor = await query.CountAsync(x => x.GuarantorType == "Foundation"),
                SchoolGuarantor = await query.CountAsync(x => x.GuarantorType == "School"),
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
                ActiveContractGuarantor = await query.CountAsync(x =>
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= today) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= today)),
                ExpiredContractGuarantor = await query.CountAsync(x =>
                    x.ContractEndDate.HasValue && x.ContractEndDate.Value.Date < today)
            };

            return Ok(ApiResponse<CompanyGuarantorSummaryResponse>.Ok(
                result,
                "Ringkasan company guarantor berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseCompanyGuarantorPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Company Guarantor", Description = "Melihat data company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantors(
            [FromQuery] string? search,
            [FromQuery] bool? isActive,
            [FromQuery] string? guarantorType,
            [FromQuery] string? billingMethod,
            [FromQuery] string? companyGroupName,
            [FromQuery] bool? isUsingCompanyTariffBook,
            [FromQuery] bool? isUsingHospitalTariff,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedEmployeeVerification,
            [FromQuery] bool? isNeedApprovalForProcedure,
            [FromQuery] bool? isNeedApprovalForDrug,
            [FromQuery] bool? isCoverageLimitedByEmployeeGrade,
            [FromQuery] bool? isAllowExcessPaymentByPatient,
            [FromQuery] DateTime? contractDate,
            [FromQuery] decimal? minimumCreditLimitAmount,
            [FromQuery] decimal? maximumCreditLimitAmount,
            [FromQuery] decimal? minimumCurrentOutstandingAmount,
            [FromQuery] decimal? maximumCurrentOutstandingAmount,
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
                guarantorType,
                billingMethod,
                companyGroupName,
                isUsingCompanyTariffBook,
                isUsingHospitalTariff,
                isNeedGuaranteeLetter,
                isNeedEmployeeVerification,
                isNeedApprovalForProcedure,
                isNeedApprovalForDrug,
                isCoverageLimitedByEmployeeGrade,
                isAllowExcessPaymentByPatient,
                contractDate,
                minimumCreditLimitAmount,
                maximumCreditLimitAmount,
                minimumCurrentOutstandingAmount,
                maximumCurrentOutstandingAmount
            );

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new CompanyGuarantorResponse
                {
                    Id = x.Id,
                    CompanyGuarantorCode = x.CompanyGuarantorCode,
                    CompanyGuarantorName = x.CompanyGuarantorName,
                    CompanyGroupName = x.CompanyGroupName,
                    GuarantorType = x.GuarantorType,
                    BillingMethod = x.BillingMethod,
                    ExternalCompanyCode = x.ExternalCompanyCode,
                    IntegrationCode = x.IntegrationCode,
                    ContractNumber = x.ContractNumber,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    IsUsingCompanyTariffBook = x.IsUsingCompanyTariffBook,
                    IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsCoverageLimitedByEmployeeGrade = x.IsCoverageLimitedByEmployeeGrade,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    CreditLimitAmount = x.CreditLimitAmount,
                    CurrentOutstandingAmount = x.CurrentOutstandingAmount,
                    PaymentDueDays = x.PaymentDueDays,
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
        [ProducesResponseType(typeof(ApiResponse<List<CompanyGuarantorOptionResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Company Guarantor", Description = "Melihat data company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantorOptions(
            [FromQuery] string? guarantorType,
            [FromQuery] string? billingMethod,
            [FromQuery] bool? isNeedGuaranteeLetter,
            [FromQuery] bool? isNeedEmployeeVerification,
            [FromQuery] bool? isNeedApprovalForProcedure,
            [FromQuery] bool? isNeedApprovalForDrug,
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(guarantorType))
                query = query.Where(x => x.GuarantorType == guarantorType.Trim());

            if (!string.IsNullOrWhiteSpace(billingMethod))
                query = query.Where(x => x.BillingMethod == billingMethod.Trim());

            if (isNeedGuaranteeLetter.HasValue)
                query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);

            if (isNeedEmployeeVerification.HasValue)
                query = query.Where(x => x.IsNeedEmployeeVerification == isNeedEmployeeVerification.Value);

            if (isNeedApprovalForProcedure.HasValue)
                query = query.Where(x => x.IsNeedApprovalForProcedure == isNeedApprovalForProcedure.Value);

            if (isNeedApprovalForDrug.HasValue)
                query = query.Where(x => x.IsNeedApprovalForDrug == isNeedApprovalForDrug.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CompanyGuarantorCode.ToLower().Contains(keyword) ||
                    x.CompanyGuarantorName.ToLower().Contains(keyword) ||
                    (x.CompanyGroupName != null && x.CompanyGroupName.ToLower().Contains(keyword)));
            }

            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.CompanyGuarantorName)
                .Select(x => new CompanyGuarantorOptionResponse
                {
                    Id = x.Id,
                    CompanyGuarantorCode = x.CompanyGuarantorCode,
                    CompanyGuarantorName = x.CompanyGuarantorName,
                    CompanyGroupName = x.CompanyGroupName,
                    GuarantorType = x.GuarantorType,
                    BillingMethod = x.BillingMethod,
                    IsUsingCompanyTariffBook = x.IsUsingCompanyTariffBook,
                    IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsCoverageLimitedByEmployeeGrade = x.IsCoverageLimitedByEmployeeGrade,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    CreditLimitAmount = x.CreditLimitAmount,
                    CurrentOutstandingAmount = x.CurrentOutstandingAmount,
                    PaymentDueDays = x.PaymentDueDays
                })
                .ToListAsync();

            return Ok(ApiResponse<List<CompanyGuarantorOptionResponse>>.Ok(
                data,
                "Data pilihan company guarantor berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Company Guarantor", Description = "Melihat data company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantorById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new CompanyGuarantorDetailResponse
                {
                    Id = x.Id,
                    CompanyGuarantorCode = x.CompanyGuarantorCode,
                    CompanyGuarantorName = x.CompanyGuarantorName,
                    CompanyGroupName = x.CompanyGroupName,
                    GuarantorType = x.GuarantorType,
                    BillingMethod = x.BillingMethod,
                    ExternalCompanyCode = x.ExternalCompanyCode,
                    IntegrationCode = x.IntegrationCode,
                    ContractNumber = x.ContractNumber,
                    ContractStartDate = x.ContractStartDate,
                    ContractEndDate = x.ContractEndDate,
                    IsUsingCompanyTariffBook = x.IsUsingCompanyTariffBook,
                    IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                    IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                    IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                    IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                    IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                    IsCoverageLimitedByEmployeeGrade = x.IsCoverageLimitedByEmployeeGrade,
                    IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                    CreditLimitAmount = x.CreditLimitAmount,
                    CurrentOutstandingAmount = x.CurrentOutstandingAmount,
                    PaymentDueDays = x.PaymentDueDays,
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
                    "Company guarantor tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<CompanyGuarantorDetailResponse>.Ok(
                data,
                "Detail company guarantor berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorCreateResponse>), StatusCodes.Status200OK)]
        [AccessAction("Create", "Create Company Guarantor", Description = "Membuat data company guarantor", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("CompanyGuarantor", "Create")]
        public async Task<IActionResult> CreateCompanyGuarantor([FromBody] CreateCompanyGuarantorRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                companyGuarantorCode: request.CompanyGuarantorCode,
                companyGuarantorName: request.CompanyGuarantorName,
                guarantorType: request.GuarantorType,
                billingMethod: request.BillingMethod,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                creditLimitAmount: request.CreditLimitAmount,
                currentOutstandingAmount: request.CurrentOutstandingAmount,
                paymentDueDays: request.PaymentDueDays
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

            var entity = new MstCompanyGuarantor
            {
                Id = Guid.NewGuid(),
                CompanyGuarantorCode = request.CompanyGuarantorCode.Trim().ToUpperInvariant(),
                CompanyGuarantorName = request.CompanyGuarantorName.Trim(),
                CompanyGroupName = NormalizeNullableText(request.CompanyGroupName),
                GuarantorType = request.GuarantorType.Trim(),
                BillingMethod = request.BillingMethod.Trim(),
                ExternalCompanyCode = NormalizeNullableText(request.ExternalCompanyCode),
                IntegrationCode = NormalizeNullableText(request.IntegrationCode),
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
                PicEmail = NormalizeNullableText(request.PicEmail),
                OfficeAddress = NormalizeNullableText(request.OfficeAddress),
                LogoPath = NormalizeNullableText(request.LogoPath),
                BillingInstruction = NormalizeNullableText(request.BillingInstruction),
                ClaimInstruction = NormalizeNullableText(request.ClaimInstruction),
                Description = NormalizeNullableText(request.Description),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstCompanyGuarantor>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = new CompanyGuarantorCreateResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName
            };

            return Ok(ApiResponse<CompanyGuarantorCreateResponse>.Ok(
                response,
                "Company guarantor berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Company Guarantor", Description = "Mengubah data company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateCompanyGuarantor(Guid id, [FromBody] UpdateCompanyGuarantorRequest request)
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
                companyGuarantorCode: request.CompanyGuarantorCode,
                companyGuarantorName: request.CompanyGuarantorName,
                guarantorType: request.GuarantorType,
                billingMethod: request.BillingMethod,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                creditLimitAmount: request.CreditLimitAmount,
                currentOutstandingAmount: request.CurrentOutstandingAmount,
                paymentDueDays: request.PaymentDueDays
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data company guarantor tidak valid."
                ));
            }

            entity.CompanyGuarantorCode = request.CompanyGuarantorCode.Trim().ToUpperInvariant();
            entity.CompanyGuarantorName = request.CompanyGuarantorName.Trim();
            entity.CompanyGroupName = NormalizeNullableText(request.CompanyGroupName);
            entity.GuarantorType = request.GuarantorType.Trim();
            entity.BillingMethod = request.BillingMethod.Trim();
            entity.ExternalCompanyCode = NormalizeNullableText(request.ExternalCompanyCode);
            entity.IntegrationCode = NormalizeNullableText(request.IntegrationCode);
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
                "Company guarantor berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorStatusResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Company Guarantor Status", Description = "Mengubah status aktif company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateCompanyGuarantorStatus(Guid id, [FromQuery] bool isActive)
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

            entity.IsActive = isActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new CompanyGuarantorStatusResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                IsActive = entity.IsActive
            };

            return Ok(ApiResponse<CompanyGuarantorStatusResponse>.Ok(
                response,
                "Status company guarantor berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/outstanding")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorOutstandingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Company Guarantor Outstanding", Description = "Mengubah outstanding company guarantor", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("CompanyGuarantor", "Update")]
        public async Task<IActionResult> UpdateCompanyGuarantorOutstanding(Guid id, [FromQuery] decimal? currentOutstandingAmount)
        {
            if (currentOutstandingAmount.HasValue && currentOutstandingAmount.Value < 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Outstanding tidak boleh lebih kecil dari 0."
                ));
            }

            var entity = await _dbContext.Set<MstCompanyGuarantor>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Company guarantor tidak ditemukan."
                ));
            }

            entity.CurrentOutstandingAmount = currentOutstandingAmount;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new CompanyGuarantorOutstandingResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CreditLimitAmount = entity.CreditLimitAmount,
                CurrentOutstandingAmount = entity.CurrentOutstandingAmount
            };

            return Ok(ApiResponse<CompanyGuarantorOutstandingResponse>.Ok(
                response,
                "Outstanding company guarantor berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<CompanyGuarantorDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Company Guarantor", Description = "Menghapus data company guarantor", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("CompanyGuarantor", "Delete")]
        public async Task<IActionResult> DeleteCompanyGuarantor(Guid id)
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

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var response = new CompanyGuarantorDeleteResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                IsDelete = entity.IsDelete
            };

            return Ok(ApiResponse<CompanyGuarantorDeleteResponse>.Ok(
                response,
                "Company guarantor berhasil dihapus."
            ));
        }

        private IQueryable<MstCompanyGuarantor> BuildBaseQuery()
        {
            return _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstCompanyGuarantor> ApplyFilters(
            IQueryable<MstCompanyGuarantor> query,
            string? search,
            bool? isActive,
            string? guarantorType,
            string? billingMethod,
            string? companyGroupName,
            bool? isUsingCompanyTariffBook,
            bool? isUsingHospitalTariff,
            bool? isNeedGuaranteeLetter,
            bool? isNeedEmployeeVerification,
            bool? isNeedApprovalForProcedure,
            bool? isNeedApprovalForDrug,
            bool? isCoverageLimitedByEmployeeGrade,
            bool? isAllowExcessPaymentByPatient,
            DateTime? contractDate,
            decimal? minimumCreditLimitAmount,
            decimal? maximumCreditLimitAmount,
            decimal? minimumCurrentOutstandingAmount,
            decimal? maximumCurrentOutstandingAmount)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.CompanyGuarantorCode.ToLower().Contains(keyword) ||
                    x.CompanyGuarantorName.ToLower().Contains(keyword) ||
                    (x.CompanyGroupName != null && x.CompanyGroupName.ToLower().Contains(keyword)) ||
                    x.GuarantorType.ToLower().Contains(keyword) ||
                    x.BillingMethod.ToLower().Contains(keyword) ||
                    (x.ExternalCompanyCode != null && x.ExternalCompanyCode.ToLower().Contains(keyword)) ||
                    (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)) ||
                    (x.ContractNumber != null && x.ContractNumber.ToLower().Contains(keyword)) ||
                    (x.PicName != null && x.PicName.ToLower().Contains(keyword)) ||
                    (x.PicEmail != null && x.PicEmail.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(guarantorType))
                query = query.Where(x => x.GuarantorType == guarantorType.Trim());

            if (!string.IsNullOrWhiteSpace(billingMethod))
                query = query.Where(x => x.BillingMethod == billingMethod.Trim());

            if (!string.IsNullOrWhiteSpace(companyGroupName))
            {
                var groupKeyword = companyGroupName.Trim().ToLower();
                query = query.Where(x => x.CompanyGroupName != null && x.CompanyGroupName.ToLower().Contains(groupKeyword));
            }

            if (isUsingCompanyTariffBook.HasValue)
                query = query.Where(x => x.IsUsingCompanyTariffBook == isUsingCompanyTariffBook.Value);

            if (isUsingHospitalTariff.HasValue)
                query = query.Where(x => x.IsUsingHospitalTariff == isUsingHospitalTariff.Value);

            if (isNeedGuaranteeLetter.HasValue)
                query = query.Where(x => x.IsNeedGuaranteeLetter == isNeedGuaranteeLetter.Value);

            if (isNeedEmployeeVerification.HasValue)
                query = query.Where(x => x.IsNeedEmployeeVerification == isNeedEmployeeVerification.Value);

            if (isNeedApprovalForProcedure.HasValue)
                query = query.Where(x => x.IsNeedApprovalForProcedure == isNeedApprovalForProcedure.Value);

            if (isNeedApprovalForDrug.HasValue)
                query = query.Where(x => x.IsNeedApprovalForDrug == isNeedApprovalForDrug.Value);

            if (isCoverageLimitedByEmployeeGrade.HasValue)
                query = query.Where(x => x.IsCoverageLimitedByEmployeeGrade == isCoverageLimitedByEmployeeGrade.Value);

            if (isAllowExcessPaymentByPatient.HasValue)
                query = query.Where(x => x.IsAllowExcessPaymentByPatient == isAllowExcessPaymentByPatient.Value);

            if (contractDate.HasValue)
            {
                var selectedDate = contractDate.Value.Date;

                query = query.Where(x =>
                    (!x.ContractStartDate.HasValue || x.ContractStartDate.Value.Date <= selectedDate) &&
                    (!x.ContractEndDate.HasValue || x.ContractEndDate.Value.Date >= selectedDate));
            }

            if (minimumCreditLimitAmount.HasValue)
                query = query.Where(x => x.CreditLimitAmount.HasValue && x.CreditLimitAmount.Value >= minimumCreditLimitAmount.Value);

            if (maximumCreditLimitAmount.HasValue)
                query = query.Where(x => x.CreditLimitAmount.HasValue && x.CreditLimitAmount.Value <= maximumCreditLimitAmount.Value);

            if (minimumCurrentOutstandingAmount.HasValue)
                query = query.Where(x => x.CurrentOutstandingAmount.HasValue && x.CurrentOutstandingAmount.Value >= minimumCurrentOutstandingAmount.Value);

            if (maximumCurrentOutstandingAmount.HasValue)
                query = query.Where(x => x.CurrentOutstandingAmount.HasValue && x.CurrentOutstandingAmount.Value <= maximumCurrentOutstandingAmount.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string companyGuarantorCode,
            string companyGuarantorName,
            string guarantorType,
            string billingMethod,
            DateTime? contractStartDate,
            DateTime? contractEndDate,
            decimal? creditLimitAmount,
            decimal? currentOutstandingAmount,
            int paymentDueDays)
        {
            if (string.IsNullOrWhiteSpace(companyGuarantorCode))
                return (false, "Kode company guarantor wajib diisi.");

            if (string.IsNullOrWhiteSpace(companyGuarantorName))
                return (false, "Nama company guarantor wajib diisi.");

            if (string.IsNullOrWhiteSpace(guarantorType))
                return (false, "Tipe guarantor wajib diisi.");

            if (!AllowedGuarantorTypes.Contains(guarantorType.Trim()))
                return (false, "Tipe guarantor tidak valid. Gunakan Corporate, Government, Foundation, School, atau Other.");

            if (string.IsNullOrWhiteSpace(billingMethod))
                return (false, "Metode billing wajib diisi.");

            if (!AllowedBillingMethods.Contains(billingMethod.Trim()))
                return (false, "Metode billing tidak valid. Gunakan Invoice, Deposit, atau Mixed.");

            if (contractStartDate.HasValue && contractEndDate.HasValue && contractEndDate.Value.Date < contractStartDate.Value.Date)
                return (false, "Tanggal akhir kontrak tidak boleh lebih kecil dari tanggal mulai kontrak.");

            if (creditLimitAmount.HasValue && creditLimitAmount.Value < 0)
                return (false, "Credit limit tidak boleh lebih kecil dari 0.");

            if (currentOutstandingAmount.HasValue && currentOutstandingAmount.Value < 0)
                return (false, "Outstanding tidak boleh lebih kecil dari 0.");

            if (paymentDueDays < 0)
                return (false, "Payment due days tidak boleh lebih kecil dari 0.");

            var normalizedCode = companyGuarantorCode.Trim().ToUpperInvariant();
            var normalizedName = companyGuarantorName.Trim().ToLower();

            var duplicateCode = await _dbContext.Set<MstCompanyGuarantor>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.CompanyGuarantorCode.ToUpper() == normalizedCode &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateCode)
                return (false, "Kode company guarantor sudah digunakan.");

            var duplicateName = await _dbContext.Set<MstCompanyGuarantor>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.CompanyGuarantorName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama company guarantor sudah digunakan.");

            return (true, null);
        }

        private static IQueryable<MstCompanyGuarantor> ApplySorting(
            IQueryable<MstCompanyGuarantor> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "companyguarantorcode" => isDesc
                    ? query.OrderByDescending(x => x.CompanyGuarantorCode)
                    : query.OrderBy(x => x.CompanyGuarantorCode),

                "companyguarantorname" => isDesc
                    ? query.OrderByDescending(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.CompanyGuarantorName),

                "companygroupname" => isDesc
                    ? query.OrderByDescending(x => x.CompanyGroupName)
                    : query.OrderBy(x => x.CompanyGroupName),

                "guarantortype" => isDesc
                    ? query.OrderByDescending(x => x.GuarantorType)
                    : query.OrderBy(x => x.GuarantorType),

                "billingmethod" => isDesc
                    ? query.OrderByDescending(x => x.BillingMethod)
                    : query.OrderBy(x => x.BillingMethod),

                "creditlimitamount" => isDesc
                    ? query.OrderByDescending(x => x.CreditLimitAmount)
                    : query.OrderBy(x => x.CreditLimitAmount),

                "currentoutstandingamount" => isDesc
                    ? query.OrderByDescending(x => x.CurrentOutstandingAmount)
                    : query.OrderBy(x => x.CurrentOutstandingAmount),

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
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.CompanyGuarantorName)
            };
        }

        private static CompanyGuarantorResponse ToResponse(MstCompanyGuarantor x)
        {
            return new CompanyGuarantorResponse
            {
                Id = x.Id,
                CompanyGuarantorCode = x.CompanyGuarantorCode,
                CompanyGuarantorName = x.CompanyGuarantorName,
                CompanyGroupName = x.CompanyGroupName,
                GuarantorType = x.GuarantorType,
                BillingMethod = x.BillingMethod,
                ExternalCompanyCode = x.ExternalCompanyCode,
                IntegrationCode = x.IntegrationCode,
                ContractNumber = x.ContractNumber,
                ContractStartDate = x.ContractStartDate,
                ContractEndDate = x.ContractEndDate,
                IsUsingCompanyTariffBook = x.IsUsingCompanyTariffBook,
                IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                IsCoverageLimitedByEmployeeGrade = x.IsCoverageLimitedByEmployeeGrade,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                CreditLimitAmount = x.CreditLimitAmount,
                CurrentOutstandingAmount = x.CurrentOutstandingAmount,
                PaymentDueDays = x.PaymentDueDays,
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

        private static CompanyGuarantorDetailResponse ToDetailResponse(MstCompanyGuarantor x)
        {
            return new CompanyGuarantorDetailResponse
            {
                Id = x.Id,
                CompanyGuarantorCode = x.CompanyGuarantorCode,
                CompanyGuarantorName = x.CompanyGuarantorName,
                CompanyGroupName = x.CompanyGroupName,
                GuarantorType = x.GuarantorType,
                BillingMethod = x.BillingMethod,
                ExternalCompanyCode = x.ExternalCompanyCode,
                IntegrationCode = x.IntegrationCode,
                ContractNumber = x.ContractNumber,
                ContractStartDate = x.ContractStartDate,
                ContractEndDate = x.ContractEndDate,
                IsUsingCompanyTariffBook = x.IsUsingCompanyTariffBook,
                IsUsingHospitalTariff = x.IsUsingHospitalTariff,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsNeedEmployeeVerification = x.IsNeedEmployeeVerification,
                IsNeedApprovalForProcedure = x.IsNeedApprovalForProcedure,
                IsNeedApprovalForDrug = x.IsNeedApprovalForDrug,
                IsCoverageLimitedByEmployeeGrade = x.IsCoverageLimitedByEmployeeGrade,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                CreditLimitAmount = x.CreditLimitAmount,
                CurrentOutstandingAmount = x.CurrentOutstandingAmount,
                PaymentDueDays = x.PaymentDueDays,
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