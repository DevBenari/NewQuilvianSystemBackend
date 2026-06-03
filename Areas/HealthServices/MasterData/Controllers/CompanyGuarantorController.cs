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

using ResponseCompanyGuarantorPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.CompanyGuarantorResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/company-guarantors")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Company Guarantor",
        AreaName = "HealthServices",
        ControllerName = "CompanyGuarantor",
        Description = "Health service master data company guarantor",
        SortOrder = 15
    )]
    [Tags("Health Services / Master Data / Company Guarantor")]
    public class CompanyGuarantorController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string CompanyGuarantorCodePrefix = "CG-RSMMC-";
        private const int CompanyGuarantorCodeDigitLength = 5;

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
                CustomPeriods = new List<CompanyGuarantorCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7Days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" }
                },
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
                GuarantorTypeOptions = AllowedGuarantorTypes
                    .OrderBy(x => x)
                    .Select(x => new CompanyGuarantorStringOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList(),
                BillingMethodOptions = AllowedBillingMethods
                    .OrderBy(x => x)
                    .Select(x => new CompanyGuarantorStringOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList()
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
                AllowExcessPaymentByPatientGuarantor = await query.CountAsync(x => x.IsAllowExcessPaymentByPatient),
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

            var query = _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

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

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
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
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? search = null)
        {
            var query = _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

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
                    (x.IntegrationCode != null && x.IntegrationCode.ToLower().Contains(keyword)));
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
        [AccessAction("Read", "Read Company Guarantor", Description = "Melihat detail company guarantor", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("CompanyGuarantor", "Read")]
        public async Task<IActionResult> GetCompanyGuarantorById(Guid id)
        {
            var data = await _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
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
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Company Guarantor", Description = "Membuat data company guarantor", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("CompanyGuarantor", "Create")]
        public async Task<IActionResult> CreateCompanyGuarantor([FromBody] CreateCompanyGuarantorRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                companyGuarantorName: request.CompanyGuarantorName,
                guarantorType: request.GuarantorType,
                billingMethod: request.BillingMethod,
                externalCompanyCode: request.ExternalCompanyCode,
                integrationCode: request.IntegrationCode,
                contractNumber: request.ContractNumber,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                creditLimitAmount: request.CreditLimitAmount,
                currentOutstandingAmount: request.CurrentOutstandingAmount,
                paymentDueDays: request.PaymentDueDays,
                picEmail: request.PicEmail
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
                CompanyGuarantorCode = await GenerateCompanyGuarantorCodeAsync(),
                CompanyGuarantorName = request.CompanyGuarantorName.Trim(),
                CompanyGroupName = NormalizeNullableString(request.CompanyGroupName),
                GuarantorType = NormalizeGuarantorType(request.GuarantorType),
                BillingMethod = NormalizeBillingMethod(request.BillingMethod),
                ExternalCompanyCode = NormalizeNullableString(request.ExternalCompanyCode),
                IntegrationCode = NormalizeNullableString(request.IntegrationCode),
                ContractNumber = NormalizeNullableString(request.ContractNumber),
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

            _dbContext.Set<MstCompanyGuarantor>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new CompanyGuarantorCreateResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                GuarantorType = entity.GuarantorType,
                BillingMethod = entity.BillingMethod,
                IsActive = entity.IsActive
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
                companyGuarantorName: request.CompanyGuarantorName,
                guarantorType: request.GuarantorType,
                billingMethod: request.BillingMethod,
                externalCompanyCode: request.ExternalCompanyCode,
                integrationCode: request.IntegrationCode,
                contractNumber: request.ContractNumber,
                contractStartDate: request.ContractStartDate,
                contractEndDate: request.ContractEndDate,
                creditLimitAmount: request.CreditLimitAmount,
                currentOutstandingAmount: request.CurrentOutstandingAmount,
                paymentDueDays: request.PaymentDueDays,
                picEmail: request.PicEmail
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
            entity.CompanyGroupName = NormalizeNullableString(request.CompanyGroupName);
            entity.GuarantorType = NormalizeGuarantorType(request.GuarantorType);
            entity.BillingMethod = NormalizeBillingMethod(request.BillingMethod);
            entity.ExternalCompanyCode = NormalizeNullableString(request.ExternalCompanyCode);
            entity.IntegrationCode = NormalizeNullableString(request.IntegrationCode);
            entity.ContractNumber = NormalizeNullableString(request.ContractNumber);
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

            var result = new CompanyGuarantorUpdateResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = new CompanyGuarantorDeleteResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                DeleteDateTime = entity.DeleteDateTime
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

        private static IQueryable<MstCompanyGuarantor> ApplyDateFilter(
            IQueryable<MstCompanyGuarantor> query,
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

        private async Task<string> GenerateCompanyGuarantorCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.CompanyGuarantorCode.StartsWith(CompanyGuarantorCodePrefix))
                .Select(x => x.CompanyGuarantorCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractCompanyGuarantorSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{CompanyGuarantorCodePrefix}{nextNumber.ToString().PadLeft(CompanyGuarantorCodeDigitLength, '0')}";
        }

        private static int? TryExtractCompanyGuarantorSequenceNumber(string companyGuarantorCode)
        {
            if (string.IsNullOrWhiteSpace(companyGuarantorCode))
                return null;

            if (!companyGuarantorCode.StartsWith(CompanyGuarantorCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = companyGuarantorCode[CompanyGuarantorCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstCompanyGuarantor> ApplySorting(
            IQueryable<MstCompanyGuarantor> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
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

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string companyGuarantorName,
            string guarantorType,
            string billingMethod,
            string? externalCompanyCode,
            string? integrationCode,
            string? contractNumber,
            DateTime? contractStartDate,
            DateTime? contractEndDate,
            decimal? creditLimitAmount,
            decimal? currentOutstandingAmount,
            int paymentDueDays,
            string? picEmail)
        {
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

            if (!string.IsNullOrWhiteSpace(picEmail) && !picEmail.Contains('@'))
                return (false, "Format email PIC tidak valid.");

            var normalizedName = companyGuarantorName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstCompanyGuarantor>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.CompanyGuarantorName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama company guarantor sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(externalCompanyCode))
            {
                var normalizedExternalCompanyCode = externalCompanyCode.Trim().ToLower();

                var duplicateExternalCompanyCode = await _dbContext.Set<MstCompanyGuarantor>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.ExternalCompanyCode != null &&
                        x.ExternalCompanyCode.ToLower() == normalizedExternalCompanyCode &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateExternalCompanyCode)
                    return (false, "External company code sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(integrationCode))
            {
                var normalizedIntegrationCode = integrationCode.Trim().ToLower();

                var duplicateIntegrationCode = await _dbContext.Set<MstCompanyGuarantor>()
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

                var duplicateContractNumber = await _dbContext.Set<MstCompanyGuarantor>()
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

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
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
