using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseSupplierPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.Administrator.MasterData.DTOs.SupplierResponse>;

namespace QuilvianSystemBackend.Areas.Administrator.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/administrator/master-data/suppliers")]
    [AccessController(
        moduleCode: "ADMINISTRATOR_MASTER_DATA",
        moduleName: "Administrator Master Data",
        displayName: "Supplier",
        AreaName = "Administrator",
        ControllerName = "Supplier",
        Description = "Administrator master data supplier",
        SortOrder = 14
    )]
    [Tags("Administrator / Master Data / Supplier")]
    public class SupplierController : ControllerBase
    {
        private const string LogCategory = "Administrator.MasterData";
        private const string SupplierCodePrefix = "SUP-RSMMC-";
        private const int SupplierCodeDigitLength = 5;

        private static readonly HashSet<string> AllowedSupplierTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "Pharmacy",
            "MedicalDevice",
            "Laboratory",
            "Consumable",
            "Distributor",
            "Principal",
            "Manufacturer",
            "Other"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public SupplierController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<SupplierFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Supplier", Description = "Melihat data supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new SupplierFilterMetadataResponse
            {
                DefaultFilter = new SupplierDefaultFilterResponse(),
                CustomPeriods = new List<SupplierCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7Days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" }
                },
                SortOptions = new List<SupplierSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "supplierCode", Label = "Kode supplier" },
                    new() { Value = "supplierName", Label = "Nama supplier" },
                    new() { Value = "legalName", Label = "Nama legal" },
                    new() { Value = "supplierType", Label = "Tipe supplier" },
                    new() { Value = "supplierGroupName", Label = "Group supplier" },
                    new() { Value = "paymentTermDays", Label = "Termin pembayaran" },
                    new() { Value = "leadTimeDays", Label = "Lead time" },
                    new() { Value = "isPreferredSupplier", Label = "Preferred supplier" },
                    new() { Value = "isBlacklisted", Label = "Blacklist" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SupplierTypeOptions = AllowedSupplierTypes
                    .OrderBy(x => x)
                    .Select(x => new SupplierTypeOptionResponse
                    {
                        Value = x,
                        Label = SplitPascalCase(x)
                    })
                    .ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Supplier.GetFilterMetadata",
                "Mengambil metadata filter supplier.",
                result
            );

            return Ok(ApiResponse<SupplierFilterMetadataResponse>.Ok(
                result,
                "Metadata filter supplier berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<SupplierSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Supplier", Description = "Melihat data supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new SupplierSummaryResponse
            {
                TotalSupplier = await query.CountAsync(),
                ActiveSupplier = await query.CountAsync(x => x.IsActive),
                InactiveSupplier = await query.CountAsync(x => !x.IsActive),
                PreferredSupplier = await query.CountAsync(x => x.IsPreferredSupplier),
                BlacklistedSupplier = await query.CountAsync(x => x.IsBlacklisted),
                TaxableSupplier = await query.CountAsync(x => x.IsTaxable),
                PrincipalSupplier = await query.CountAsync(x => x.IsPrincipal),
                DistributorSupplier = await query.CountAsync(x => x.IsDistributor),
                ManufacturerSupplier = await query.CountAsync(x => x.IsManufacturer),
                PharmacySupplier = await query.CountAsync(x => x.IsPharmacySupplier),
                MedicalDeviceSupplier = await query.CountAsync(x => x.IsMedicalDeviceSupplier),
                LaboratorySupplier = await query.CountAsync(x => x.IsLaboratorySupplier),
                ConsumableSupplier = await query.CountAsync(x => x.IsConsumableSupplier)
            };

            return Ok(ApiResponse<SupplierSummaryResponse>.Ok(
                result,
                "Ringkasan supplier berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseSupplierPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Supplier", Description = "Melihat data supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSuppliers(
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

            var query = _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            query = ApplyDateFilter(query, startDate, endDate, customPeriod);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.SupplierCode.ToLower().Contains(keyword) ||
                    x.SupplierName.ToLower().Contains(keyword) ||
                    x.SupplierType.ToLower().Contains(keyword) ||
                    x.LegalName != null && x.LegalName.ToLower().Contains(keyword) ||
                    x.SupplierGroupName != null && x.SupplierGroupName.ToLower().Contains(keyword) ||
                    x.TaxNumber != null && x.TaxNumber.ToLower().Contains(keyword) ||
                    x.BusinessLicenseNumber != null && x.BusinessLicenseNumber.ToLower().Contains(keyword) ||
                    x.ContactPersonName != null && x.ContactPersonName.ToLower().Contains(keyword) ||
                    x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword) ||
                    x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword) ||
                    x.Email != null && x.Email.ToLower().Contains(keyword) ||
                    x.Website != null && x.Website.ToLower().Contains(keyword) ||
                    x.CityName != null && x.CityName.ToLower().Contains(keyword) ||
                    x.ProvinceName != null && x.ProvinceName.ToLower().Contains(keyword) ||
                    x.CountryName != null && x.CountryName.ToLower().Contains(keyword) ||
                    x.Description != null && x.Description.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseSupplierPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseSupplierPagedResult>.Ok(
                result,
                "Data supplier berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<SupplierOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Supplier", Description = "Melihat data supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSupplierOptions(
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.SupplierCode.ToLower().Contains(keyword) ||
                    x.SupplierName.ToLower().Contains(keyword) ||
                    x.SupplierType.ToLower().Contains(keyword) ||
                    x.LegalName != null && x.LegalName.ToLower().Contains(keyword) ||
                    x.SupplierGroupName != null && x.SupplierGroupName.ToLower().Contains(keyword) ||
                    x.ContactPersonName != null && x.ContactPersonName.ToLower().Contains(keyword) ||
                    x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword) ||
                    x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword) ||
                    x.Email != null && x.Email.ToLower().Contains(keyword));
            }

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.SupplierName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SupplierOptionResponse
                {
                    Id = x.Id,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    LegalName = x.LegalName,
                    SupplierType = x.SupplierType,
                    SupplierGroupName = x.SupplierGroupName,
                    ContactPersonName = x.ContactPersonName,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    PaymentTermDays = x.PaymentTermDays,
                    LeadTimeDays = x.LeadTimeDays,
                    IsTaxable = x.IsTaxable,
                    TaxPercent = x.TaxPercent,
                    IsPreferredSupplier = x.IsPreferredSupplier,
                    IsBlacklisted = x.IsBlacklisted
                })
                .ToListAsync();

            var result = new SupplierOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<SupplierOptionPagedResponse>.Ok(
                result,
                "Data pilihan supplier berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SupplierDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Supplier", Description = "Melihat detail supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSupplierById(Guid id)
        {
            var data = await _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDelete)
                .Select(x => new SupplierDetailResponse
                {
                    Id = x.Id,
                    SupplierCode = x.SupplierCode,
                    SupplierName = x.SupplierName,
                    LegalName = x.LegalName,
                    SupplierType = x.SupplierType,
                    SupplierGroupName = x.SupplierGroupName,
                    TaxNumber = x.TaxNumber,
                    BusinessLicenseNumber = x.BusinessLicenseNumber,
                    ContactPersonName = x.ContactPersonName,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    Website = x.Website,
                    Address = x.Address,
                    CityName = x.CityName,
                    ProvinceName = x.ProvinceName,
                    PostalCode = x.PostalCode,
                    CountryName = x.CountryName,
                    BankName = x.BankName,
                    BankAccountNumber = x.BankAccountNumber,
                    BankAccountName = x.BankAccountName,
                    PaymentTermDays = x.PaymentTermDays,
                    LeadTimeDays = x.LeadTimeDays,
                    MinimumPurchaseAmount = x.MinimumPurchaseAmount,
                    CreditLimitAmount = x.CreditLimitAmount,
                    IsTaxable = x.IsTaxable,
                    TaxPercent = x.TaxPercent,
                    IsPrincipal = x.IsPrincipal,
                    IsDistributor = x.IsDistributor,
                    IsManufacturer = x.IsManufacturer,
                    IsPharmacySupplier = x.IsPharmacySupplier,
                    IsMedicalDeviceSupplier = x.IsMedicalDeviceSupplier,
                    IsLaboratorySupplier = x.IsLaboratorySupplier,
                    IsConsumableSupplier = x.IsConsumableSupplier,
                    IsPreferredSupplier = x.IsPreferredSupplier,
                    IsBlacklisted = x.IsBlacklisted,
                    BlacklistReason = x.BlacklistReason,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Supplier tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<SupplierDetailResponse>.Ok(
                data,
                "Detail supplier berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<SupplierCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Supplier", Description = "Membuat data supplier", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Supplier", "Create")]
        public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                supplierName: request.SupplierName,
                supplierType: request.SupplierType,
                taxNumber: request.TaxNumber,
                businessLicenseNumber: request.BusinessLicenseNumber,
                email: request.Email,
                paymentTermDays: request.PaymentTermDays,
                leadTimeDays: request.LeadTimeDays,
                minimumPurchaseAmount: request.MinimumPurchaseAmount,
                creditLimitAmount: request.CreditLimitAmount,
                taxPercent: request.TaxPercent,
                isTaxable: request.IsTaxable,
                isBlacklisted: request.IsBlacklisted,
                blacklistReason: request.BlacklistReason
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data supplier tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstSupplier
            {
                Id = Guid.NewGuid(),
                SupplierCode = await GenerateSupplierCodeAsync(),
                SupplierName = request.SupplierName.Trim(),
                LegalName = NormalizeNullableString(request.LegalName),
                SupplierType = NormalizeSupplierType(request.SupplierType),
                SupplierGroupName = NormalizeNullableString(request.SupplierGroupName),
                TaxNumber = NormalizeNullableString(request.TaxNumber),
                BusinessLicenseNumber = NormalizeNullableString(request.BusinessLicenseNumber),
                ContactPersonName = NormalizeNullableString(request.ContactPersonName),
                PhoneNumber = NormalizeNullableString(request.PhoneNumber),
                WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber),
                Email = NormalizeNullableString(request.Email),
                Website = NormalizeNullableString(request.Website),
                Address = NormalizeNullableString(request.Address),
                CityName = NormalizeNullableString(request.CityName),
                ProvinceName = NormalizeNullableString(request.ProvinceName),
                PostalCode = NormalizeNullableString(request.PostalCode),
                CountryName = NormalizeNullableString(request.CountryName),
                BankName = NormalizeNullableString(request.BankName),
                BankAccountNumber = NormalizeNullableString(request.BankAccountNumber),
                BankAccountName = NormalizeNullableString(request.BankAccountName),
                PaymentTermDays = request.PaymentTermDays,
                LeadTimeDays = request.LeadTimeDays,
                MinimumPurchaseAmount = request.MinimumPurchaseAmount,
                CreditLimitAmount = request.CreditLimitAmount,
                IsTaxable = request.IsTaxable,
                TaxPercent = request.TaxPercent,
                IsPrincipal = request.IsPrincipal,
                IsDistributor = request.IsDistributor,
                IsManufacturer = request.IsManufacturer,
                IsPharmacySupplier = request.IsPharmacySupplier,
                IsMedicalDeviceSupplier = request.IsMedicalDeviceSupplier,
                IsLaboratorySupplier = request.IsLaboratorySupplier,
                IsConsumableSupplier = request.IsConsumableSupplier,
                IsPreferredSupplier = request.IsPreferredSupplier,
                IsBlacklisted = request.IsBlacklisted,
                BlacklistReason = NormalizeNullableString(request.BlacklistReason),
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            if (entity.IsBlacklisted)
                entity.IsPreferredSupplier = false;

            _dbContext.Set<MstSupplier>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = new SupplierCreateResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                SupplierType = entity.SupplierType,
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsBlacklisted = entity.IsBlacklisted,
                IsActive = entity.IsActive
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Supplier.CreateSupplier",
                "Membuat data supplier.",
                result
            );

            return Ok(ApiResponse<SupplierCreateResponse>.Ok(
                result,
                "Supplier berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Supplier", Description = "Mengubah data supplier", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Supplier", "Update")]
        public async Task<IActionResult> UpdateSupplier(Guid id, [FromBody] UpdateSupplierRequest request)
        {
            var entity = await _dbContext.Set<MstSupplier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Supplier tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                supplierName: request.SupplierName,
                supplierType: request.SupplierType,
                taxNumber: request.TaxNumber,
                businessLicenseNumber: request.BusinessLicenseNumber,
                email: request.Email,
                paymentTermDays: request.PaymentTermDays,
                leadTimeDays: request.LeadTimeDays,
                minimumPurchaseAmount: request.MinimumPurchaseAmount,
                creditLimitAmount: request.CreditLimitAmount,
                taxPercent: request.TaxPercent,
                isTaxable: request.IsTaxable,
                isBlacklisted: request.IsBlacklisted,
                blacklistReason: request.BlacklistReason
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data supplier tidak valid."
                ));
            }

            entity.SupplierName = request.SupplierName.Trim();
            entity.LegalName = NormalizeNullableString(request.LegalName);
            entity.SupplierType = NormalizeSupplierType(request.SupplierType);
            entity.SupplierGroupName = NormalizeNullableString(request.SupplierGroupName);
            entity.TaxNumber = NormalizeNullableString(request.TaxNumber);
            entity.BusinessLicenseNumber = NormalizeNullableString(request.BusinessLicenseNumber);
            entity.ContactPersonName = NormalizeNullableString(request.ContactPersonName);
            entity.PhoneNumber = NormalizeNullableString(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber);
            entity.Email = NormalizeNullableString(request.Email);
            entity.Website = NormalizeNullableString(request.Website);
            entity.Address = NormalizeNullableString(request.Address);
            entity.CityName = NormalizeNullableString(request.CityName);
            entity.ProvinceName = NormalizeNullableString(request.ProvinceName);
            entity.PostalCode = NormalizeNullableString(request.PostalCode);
            entity.CountryName = NormalizeNullableString(request.CountryName);
            entity.BankName = NormalizeNullableString(request.BankName);
            entity.BankAccountNumber = NormalizeNullableString(request.BankAccountNumber);
            entity.BankAccountName = NormalizeNullableString(request.BankAccountName);
            entity.PaymentTermDays = request.PaymentTermDays;
            entity.LeadTimeDays = request.LeadTimeDays;
            entity.MinimumPurchaseAmount = request.MinimumPurchaseAmount;
            entity.CreditLimitAmount = request.CreditLimitAmount;
            entity.IsTaxable = request.IsTaxable;
            entity.TaxPercent = request.TaxPercent;
            entity.IsPrincipal = request.IsPrincipal;
            entity.IsDistributor = request.IsDistributor;
            entity.IsManufacturer = request.IsManufacturer;
            entity.IsPharmacySupplier = request.IsPharmacySupplier;
            entity.IsMedicalDeviceSupplier = request.IsMedicalDeviceSupplier;
            entity.IsLaboratorySupplier = request.IsLaboratorySupplier;
            entity.IsConsumableSupplier = request.IsConsumableSupplier;
            entity.IsPreferredSupplier = request.IsBlacklisted ? false : request.IsPreferredSupplier;
            entity.IsBlacklisted = request.IsBlacklisted;
            entity.BlacklistReason = NormalizeNullableString(request.BlacklistReason);
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Supplier berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Supplier", Description = "Menghapus data supplier", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Supplier", "Delete")]
        public async Task<IActionResult> DeleteSupplier(Guid id)
        {
            var entity = await _dbContext.Set<MstSupplier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Supplier tidak ditemukan."
                ));
            }

            var isUsedByDrugSupplier = await _dbContext.Set<MstDrugSupplier>()
                .AnyAsync(x => x.SupplierId == id && !x.IsDelete);

            if (isUsedByDrugSupplier)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Supplier tidak dapat dihapus karena sudah digunakan pada mapping drug supplier."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Supplier berhasil dihapus."
            ));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string supplierName,
            string supplierType,
            string? taxNumber,
            string? businessLicenseNumber,
            string? email,
            int paymentTermDays,
            int leadTimeDays,
            decimal minimumPurchaseAmount,
            decimal? creditLimitAmount,
            decimal? taxPercent,
            bool isTaxable,
            bool isBlacklisted,
            string? blacklistReason)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                return (false, "Nama supplier wajib diisi.");

            if (string.IsNullOrWhiteSpace(supplierType))
                return (false, "Tipe supplier wajib diisi.");

            if (!AllowedSupplierTypes.Contains(supplierType.Trim()))
                return (false, "Tipe supplier tidak valid. Gunakan salah satu: General, Pharmacy, MedicalDevice, Laboratory, Consumable, Distributor, Principal, Manufacturer, Other.");

            if (paymentTermDays < 0)
                return (false, "Payment term tidak boleh kurang dari 0 hari.");

            if (leadTimeDays < 0)
                return (false, "Lead time tidak boleh kurang dari 0 hari.");

            if (minimumPurchaseAmount < 0)
                return (false, "Minimum purchase amount tidak boleh kurang dari 0.");

            if (creditLimitAmount.HasValue && creditLimitAmount.Value < 0)
                return (false, "Credit limit amount tidak boleh kurang dari 0.");

            if (taxPercent.HasValue && (taxPercent.Value < 0 || taxPercent.Value > 100))
                return (false, "Tax percent harus berada di antara 0 sampai 100.");

            if (isTaxable && !taxPercent.HasValue)
                return (false, "Tax percent wajib diisi jika supplier taxable.");

            if (isBlacklisted && string.IsNullOrWhiteSpace(blacklistReason))
                return (false, "Alasan blacklist wajib diisi jika supplier masuk blacklist.");

            if (!string.IsNullOrWhiteSpace(email) && !email.Contains('@'))
                return (false, "Format email supplier tidak valid.");

            var normalizedName = supplierName.Trim().ToLower();

            var duplicateName = await _dbContext.Set<MstSupplier>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.SupplierName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama supplier sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(taxNumber))
            {
                var normalizedTaxNumber = taxNumber.Trim().ToLower();

                var duplicateTaxNumber = await _dbContext.Set<MstSupplier>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.TaxNumber != null &&
                        x.TaxNumber.ToLower() == normalizedTaxNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateTaxNumber)
                    return (false, "Tax number supplier sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(businessLicenseNumber))
            {
                var normalizedLicenseNumber = businessLicenseNumber.Trim().ToLower();

                var duplicateLicenseNumber = await _dbContext.Set<MstSupplier>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.BusinessLicenseNumber != null &&
                        x.BusinessLicenseNumber.ToLower() == normalizedLicenseNumber &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateLicenseNumber)
                    return (false, "Business license number supplier sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateSupplierCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.SupplierCode.StartsWith(SupplierCodePrefix))
                .Select(x => x.SupplierCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractSupplierSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{SupplierCodePrefix}{nextNumber.ToString().PadLeft(SupplierCodeDigitLength, '0')}";
        }

        private static int? TryExtractSupplierSequenceNumber(string supplierCode)
        {
            if (string.IsNullOrWhiteSpace(supplierCode))
                return null;

            if (!supplierCode.StartsWith(SupplierCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = supplierCode[SupplierCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstSupplier> ApplyDateFilter(
            IQueryable<MstSupplier> query,
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
                query = query.Where(x => x.CreateDateTime >= startDate.Value.Date);

            if (endDate.HasValue)
                query = query.Where(x => x.CreateDateTime < endDate.Value.Date.AddDays(1));

            return query;
        }

        private static IQueryable<MstSupplier> ApplySorting(
            IQueryable<MstSupplier> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "suppliercode" => isDesc
                    ? query.OrderByDescending(x => x.SupplierCode)
                    : query.OrderBy(x => x.SupplierCode),

                "suppliername" => isDesc
                    ? query.OrderByDescending(x => x.SupplierName)
                    : query.OrderBy(x => x.SupplierName),

                "legalname" => isDesc
                    ? query.OrderByDescending(x => x.LegalName)
                    : query.OrderBy(x => x.LegalName),

                "suppliertype" => isDesc
                    ? query.OrderByDescending(x => x.SupplierType)
                    : query.OrderBy(x => x.SupplierType),

                "suppliergroupname" => isDesc
                    ? query.OrderByDescending(x => x.SupplierGroupName)
                    : query.OrderBy(x => x.SupplierGroupName),

                "paymenttermdays" => isDesc
                    ? query.OrderByDescending(x => x.PaymentTermDays)
                    : query.OrderBy(x => x.PaymentTermDays),

                "leadtimedays" => isDesc
                    ? query.OrderByDescending(x => x.LeadTimeDays)
                    : query.OrderBy(x => x.LeadTimeDays),

                "ispreferredsupplier" => isDesc
                    ? query.OrderByDescending(x => x.IsPreferredSupplier)
                    : query.OrderBy(x => x.IsPreferredSupplier),

                "isblacklisted" => isDesc
                    ? query.OrderByDescending(x => x.IsBlacklisted)
                    : query.OrderBy(x => x.IsBlacklisted),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.SupplierName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.SupplierName)
            };
        }

        private static SupplierResponse ToResponse(MstSupplier x)
        {
            return new SupplierResponse
            {
                Id = x.Id,
                SupplierCode = x.SupplierCode,
                SupplierName = x.SupplierName,
                LegalName = x.LegalName,
                SupplierType = x.SupplierType,
                SupplierGroupName = x.SupplierGroupName,
                TaxNumber = x.TaxNumber,
                BusinessLicenseNumber = x.BusinessLicenseNumber,
                ContactPersonName = x.ContactPersonName,
                PhoneNumber = x.PhoneNumber,
                WhatsAppNumber = x.WhatsAppNumber,
                Email = x.Email,
                Website = x.Website,
                CityName = x.CityName,
                ProvinceName = x.ProvinceName,
                CountryName = x.CountryName,
                PaymentTermDays = x.PaymentTermDays,
                LeadTimeDays = x.LeadTimeDays,
                MinimumPurchaseAmount = x.MinimumPurchaseAmount,
                CreditLimitAmount = x.CreditLimitAmount,
                IsTaxable = x.IsTaxable,
                TaxPercent = x.TaxPercent,
                IsPrincipal = x.IsPrincipal,
                IsDistributor = x.IsDistributor,
                IsManufacturer = x.IsManufacturer,
                IsPharmacySupplier = x.IsPharmacySupplier,
                IsMedicalDeviceSupplier = x.IsMedicalDeviceSupplier,
                IsLaboratorySupplier = x.IsLaboratorySupplier,
                IsConsumableSupplier = x.IsConsumableSupplier,
                IsPreferredSupplier = x.IsPreferredSupplier,
                IsBlacklisted = x.IsBlacklisted,
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

        private static string NormalizeSupplierType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedSupplierTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private static string SplitPascalCase(string value)
        {
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
