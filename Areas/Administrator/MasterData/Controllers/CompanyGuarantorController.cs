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
        private const string CodePrefix = "CG-RSMMC-";
        private const int CodeNumberLength = 5;

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
                CustomPeriods = new List<CompanyGuarantorCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
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
                GuarantorTypeOptions = BuildAllowedStringOptions(AllowedGuarantorTypes),
                BillingMethodOptions = BuildAllowedStringOptions(AllowedBillingMethods),
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

            var query = BuildBaseQuery();
            query = ApplyDateFilter(query, startDate, endDate, customPeriod);
            query = ApplyStandardFilter(query, isActive, search);

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
                search
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

            try
            {
                var entity = new MstCompanyGuarantor
                {
                    Id = Guid.NewGuid(),
                    CompanyGuarantorCode = await GenerateCompanyGuarantorCodeAsync(),
                    CompanyGuarantorName = request.CompanyGuarantorName.Trim(),
                    CompanyGroupName = NormalizeNullableString(request.CompanyGroupName),
                    GuarantorType = NormalizeGuarantorType(request.GuarantorType),
                    BillingMethod = NormalizeBillingMethod(request.BillingMethod),
                    ExternalCompanyCode = NormalizeUpperNullableString(request.ExternalCompanyCode),
                    IntegrationCode = NormalizeUpperNullableString(request.IntegrationCode),
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
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "CompanyGuarantor.CreateCompanyGuarantor",
                    "Gagal membuat data company guarantor.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat membuat company guarantor."
                    )
                );
            }
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

            try
            {
                entity.CompanyGuarantorName = request.CompanyGuarantorName.Trim();
                entity.CompanyGroupName = NormalizeNullableString(request.CompanyGroupName);
                entity.GuarantorType = NormalizeGuarantorType(request.GuarantorType);
                entity.BillingMethod = NormalizeBillingMethod(request.BillingMethod);
                entity.ExternalCompanyCode = NormalizeUpperNullableString(request.ExternalCompanyCode);
                entity.IntegrationCode = NormalizeUpperNullableString(request.IntegrationCode);
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

                var actorNames = await GetActorNameMapAsync(new[] { actorUserId });

                var result = new CompanyGuarantorUpdateResponse
                {
                    Id = entity.Id,
                    CompanyGuarantorCode = entity.CompanyGuarantorCode,
                    CompanyGuarantorName = entity.CompanyGuarantorName,
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
            catch (Exception ex)
            {
                await _loggerService.ErrorAsync(
                    LogCategory,
                    "CompanyGuarantor.UpdateCompanyGuarantor",
                    "Gagal mengubah data company guarantor.",
                    ex
                );

                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    ApiResponse<object>.Fail(
                        StatusCodes.Status500InternalServerError,
                        "Terjadi kesalahan saat memperbarui company guarantor."
                    )
                );
            }
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
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

            await _loggerService.InfoAsync(
                LogCategory,
                "CompanyGuarantor.UpdateCompanyGuarantorStatus",
                "Mengubah status company guarantor.",
                new
                {
                    entity.Id,
                    entity.CompanyGuarantorCode,
                    entity.CompanyGuarantorName,
                    entity.IsActive
                }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
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

        private IQueryable<MstCompanyGuarantor> BuildBaseQuery()
        {
            return _dbContext.Set<MstCompanyGuarantor>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstCompanyGuarantor> ApplyDateFilter(
            IQueryable<MstCompanyGuarantor> query,
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

        private static IQueryable<MstCompanyGuarantor> ApplyStandardFilter(
            IQueryable<MstCompanyGuarantor> query,
            bool? isActive,
            string? search)
        {
            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

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

            return query;
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

                "contractstartdate" => isDescending
                    ? query.OrderByDescending(x => x.ContractStartDate).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.ContractStartDate).ThenBy(x => x.CompanyGuarantorName),

                "contractenddate" => isDescending
                    ? query.OrderByDescending(x => x.ContractEndDate).ThenBy(x => x.CompanyGuarantorName)
                    : query.OrderBy(x => x.ContractEndDate).ThenBy(x => x.CompanyGuarantorName),

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

            if (!AllowedGuarantorTypes.Contains(request.GuarantorType.Trim()))
            {
                return (false, "Tipe guarantor tidak valid. Gunakan Corporate, Government, Foundation, School, atau Other.");
            }

            if (string.IsNullOrWhiteSpace(request.BillingMethod))
            {
                return (false, "Metode billing wajib diisi.");
            }

            if (!AllowedBillingMethods.Contains(request.BillingMethod.Trim()))
            {
                return (false, "Metode billing tidak valid. Gunakan Invoice, Deposit, atau Mixed.");
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

            var externalCompanyCode = NormalizeUpperNullableString(request.ExternalCompanyCode);

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

            var integrationCode = NormalizeUpperNullableString(request.IntegrationCode);

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

            var contractNumber = NormalizeNullableString(request.ContractNumber);

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

        private async Task<string> GenerateCompanyGuarantorCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstCompanyGuarantor>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.CompanyGuarantorCode.StartsWith(CodePrefix))
                .Select(x => x.CompanyGuarantorCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(x => x.Replace(CodePrefix, string.Empty))
                .Where(x => int.TryParse(x, out _))
                .Select(int.Parse)
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
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
            return new CompanyGuarantorResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CompanyGroupName = entity.CompanyGroupName,
                GuarantorType = entity.GuarantorType,
                BillingMethod = entity.BillingMethod,
                ExternalCompanyCode = entity.ExternalCompanyCode,
                IntegrationCode = entity.IntegrationCode,
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
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
            return new CompanyGuarantorDetailResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CompanyGroupName = entity.CompanyGroupName,
                GuarantorType = entity.GuarantorType,
                BillingMethod = entity.BillingMethod,
                ExternalCompanyCode = entity.ExternalCompanyCode,
                IntegrationCode = entity.IntegrationCode,
                ContractNumber = entity.ContractNumber,
                ContractStartDate = entity.ContractStartDate,
                ContractEndDate = entity.ContractEndDate,
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
            return new CompanyGuarantorOptionResponse
            {
                Id = entity.Id,
                CompanyGuarantorCode = entity.CompanyGuarantorCode,
                CompanyGuarantorName = entity.CompanyGuarantorName,
                CompanyGroupName = entity.CompanyGroupName,
                GuarantorType = entity.GuarantorType,
                BillingMethod = entity.BillingMethod,
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
                PaymentDueDays = entity.PaymentDueDays
            };
        }

        private static List<CompanyGuarantorStringOptionResponse> BuildAllowedStringOptions(
            IEnumerable<string> values)
        {
            return values
                .OrderBy(x => x)
                .Select(x => new CompanyGuarantorStringOptionResponse
                {
                    Value = x,
                    Label = SplitPascalCase(x)
                })
                .ToList();
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

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
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
    }
}
