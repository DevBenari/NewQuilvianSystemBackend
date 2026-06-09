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
using System.ComponentModel.DataAnnotations;
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
        [AccessAction("Read", "Read Supplier", Description = "Melihat metadata filter supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new SupplierFilterMetadataResponse
            {
                DefaultFilter = new SupplierDefaultFilterResponse(),
                CustomPeriods = new List<SupplierCustomPeriodOptionResponse>
                {
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thismonth", Label = "Bulan ini" },
                    new() { Value = "lastmonth", Label = "Bulan lalu" }
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
                    new() { Value = "minimumPurchaseAmount", Label = "Minimum purchase" },
                    new() { Value = "creditLimitAmount", Label = "Credit limit" },
                    new() { Value = "isPreferredSupplier", Label = "Preferred supplier" },
                    new() { Value = "isBlacklisted", Label = "Blacklist" },
                    new() { Value = "isTaxable", Label = "Taxable" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SupplierTypeOptions = AllowedSupplierTypes
                    .OrderBy(x => x)
                    .Select(x => new SupplierTypeOptionResponse
                    {
                        Value = x,
                        Label = BuildSupplierTypeLabel(x)
                    })
                    .ToList(),
                ResetButtonLabel = "Reset"
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
        [AccessAction("Read", "Read Supplier", Description = "Melihat ringkasan supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

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
            [FromQuery] string? supplierType,
            [FromQuery] bool? isPreferredSupplier,
            [FromQuery] bool? isBlacklisted,
            [FromQuery] bool? isTaxable,
            [FromQuery] bool? isPrincipal,
            [FromQuery] bool? isDistributor,
            [FromQuery] bool? isManufacturer,
            [FromQuery] bool? isPharmacySupplier,
            [FromQuery] bool? isMedicalDeviceSupplier,
            [FromQuery] bool? isLaboratorySupplier,
            [FromQuery] bool? isConsumableSupplier,
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
            query = ApplyStandardFilter(
                query,
                isActive,
                supplierType,
                isPreferredSupplier,
                isBlacklisted,
                isTaxable,
                isPrincipal,
                isDistributor,
                isManufacturer,
                isPharmacySupplier,
                isMedicalDeviceSupplier,
                isLaboratorySupplier,
                isConsumableSupplier,
                search
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
        [AccessAction("Read", "Read Supplier", Description = "Melihat data pilihan supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSupplierOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] string? supplierType = null,
            [FromQuery] bool? isPreferredSupplier = null,
            [FromQuery] bool? isBlacklisted = null,
            [FromQuery] bool? isTaxable = null,
            [FromQuery] bool? isPharmacySupplier = null,
            [FromQuery] bool? isMedicalDeviceSupplier = null,
            [FromQuery] bool? isLaboratorySupplier = null,
            [FromQuery] bool? isConsumableSupplier = null,
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
                supplierType,
                isPreferredSupplier,
                isBlacklisted,
                isTaxable,
                null,
                null,
                null,
                isPharmacySupplier,
                isMedicalDeviceSupplier,
                isLaboratorySupplier,
                isConsumableSupplier,
                search
            );

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
                    SupplierTypeName = BuildSupplierTypeLabel(x.SupplierType),
                    SupplierGroupName = x.SupplierGroupName,
                    ContactPersonName = x.ContactPersonName,
                    PhoneNumber = x.PhoneNumber,
                    WhatsAppNumber = x.WhatsAppNumber,
                    Email = x.Email,
                    PaymentTermDays = x.PaymentTermDays,
                    LeadTimeDays = x.LeadTimeDays,
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
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Supplier tidak ditemukan."
                ));
            }

            var actorNames = await GetActorNameMapAsync(new[]
            {
                entity.CreateBy,
                entity.UpdateBy
            });

            var data = MapDetailResponse(entity, actorNames);

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
                request: request
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
                Email = NormalizeLowerNullableString(request.Email),
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
                TaxPercent = request.IsTaxable ? request.TaxPercent : null,
                IsPrincipal = request.IsPrincipal,
                IsDistributor = request.IsDistributor,
                IsManufacturer = request.IsManufacturer,
                IsPharmacySupplier = request.IsPharmacySupplier,
                IsMedicalDeviceSupplier = request.IsMedicalDeviceSupplier,
                IsLaboratorySupplier = request.IsLaboratorySupplier,
                IsConsumableSupplier = request.IsConsumableSupplier,
                IsPreferredSupplier = request.IsBlacklisted ? false : request.IsPreferredSupplier,
                IsBlacklisted = request.IsBlacklisted,
                BlacklistReason = request.IsBlacklisted ? NormalizeNullableString(request.BlacklistReason) : null,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableString(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstSupplier>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var result = MapCreateResponse(entity);

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
        [ProducesResponseType(typeof(ApiResponse<SupplierUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
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
                request: request
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

            entity.SupplierName = request.SupplierName.Trim();
            entity.LegalName = NormalizeNullableString(request.LegalName);
            entity.SupplierType = NormalizeSupplierType(request.SupplierType);
            entity.SupplierGroupName = NormalizeNullableString(request.SupplierGroupName);
            entity.TaxNumber = NormalizeNullableString(request.TaxNumber);
            entity.BusinessLicenseNumber = NormalizeNullableString(request.BusinessLicenseNumber);
            entity.ContactPersonName = NormalizeNullableString(request.ContactPersonName);
            entity.PhoneNumber = NormalizeNullableString(request.PhoneNumber);
            entity.WhatsAppNumber = NormalizeNullableString(request.WhatsAppNumber);
            entity.Email = NormalizeLowerNullableString(request.Email);
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
            entity.TaxPercent = request.IsTaxable ? request.TaxPercent : null;
            entity.IsPrincipal = request.IsPrincipal;
            entity.IsDistributor = request.IsDistributor;
            entity.IsManufacturer = request.IsManufacturer;
            entity.IsPharmacySupplier = request.IsPharmacySupplier;
            entity.IsMedicalDeviceSupplier = request.IsMedicalDeviceSupplier;
            entity.IsLaboratorySupplier = request.IsLaboratorySupplier;
            entity.IsConsumableSupplier = request.IsConsumableSupplier;
            entity.IsPreferredSupplier = request.IsBlacklisted ? false : request.IsPreferredSupplier;
            entity.IsBlacklisted = request.IsBlacklisted;
            entity.BlacklistReason = request.IsBlacklisted ? NormalizeNullableString(request.BlacklistReason) : null;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableString(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var result = MapUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Supplier.UpdateSupplier",
                "Mengubah data supplier.",
                result
            );

            return Ok(ApiResponse<SupplierUpdateResponse>.Ok(
                result,
                "Supplier berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<SupplierUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Supplier Status", Description = "Mengubah status supplier", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Supplier", "Update")]
        public async Task<IActionResult> UpdateSupplierStatus(Guid id, [FromBody] UpdateSupplierStatusRequest request)
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

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            var result = MapUpdateResponse(entity);

            return Ok(ApiResponse<SupplierUpdateResponse>.Ok(
                result,
                "Status supplier berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SupplierDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Supplier", Description = "Menghapus data supplier", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Supplier", "Delete")]
        public async Task<IActionResult> DeleteSupplier(Guid id, [FromBody] DeleteSupplierRequest? request = null)
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

            var result = new SupplierDeleteResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                DeleteDateTime = entity.DeleteDateTime
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Supplier.DeleteSupplier",
                "Menghapus data supplier.",
                result
            );

            return Ok(ApiResponse<SupplierDeleteResponse>.Ok(
                result,
                "Supplier berhasil dihapus."
            ));
        }

        private IQueryable<MstSupplier> BuildBaseQuery()
        {
            return _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstSupplier> ApplyDateFilter(
            IQueryable<MstSupplier> query,
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
                var endExclusive = DateTime.SpecifyKind(endDate.Value.Date.AddDays(1), DateTimeKind.Utc);
                query = query.Where(x => x.CreateDateTime < endExclusive);
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

        private static IQueryable<MstSupplier> ApplyStandardFilter(
            IQueryable<MstSupplier> query,
            bool? isActive,
            string? supplierType,
            bool? isPreferredSupplier,
            bool? isBlacklisted,
            bool? isTaxable,
            bool? isPrincipal,
            bool? isDistributor,
            bool? isManufacturer,
            bool? isPharmacySupplier,
            bool? isMedicalDeviceSupplier,
            bool? isLaboratorySupplier,
            bool? isConsumableSupplier,
            string? search)
        {
            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(supplierType))
            {
                var normalizedSupplierType = NormalizeSupplierType(supplierType);
                query = query.Where(x => x.SupplierType == normalizedSupplierType);
            }

            if (isPreferredSupplier.HasValue)
                query = query.Where(x => x.IsPreferredSupplier == isPreferredSupplier.Value);

            if (isBlacklisted.HasValue)
                query = query.Where(x => x.IsBlacklisted == isBlacklisted.Value);

            if (isTaxable.HasValue)
                query = query.Where(x => x.IsTaxable == isTaxable.Value);

            if (isPrincipal.HasValue)
                query = query.Where(x => x.IsPrincipal == isPrincipal.Value);

            if (isDistributor.HasValue)
                query = query.Where(x => x.IsDistributor == isDistributor.Value);

            if (isManufacturer.HasValue)
                query = query.Where(x => x.IsManufacturer == isManufacturer.Value);

            if (isPharmacySupplier.HasValue)
                query = query.Where(x => x.IsPharmacySupplier == isPharmacySupplier.Value);

            if (isMedicalDeviceSupplier.HasValue)
                query = query.Where(x => x.IsMedicalDeviceSupplier == isMedicalDeviceSupplier.Value);

            if (isLaboratorySupplier.HasValue)
                query = query.Where(x => x.IsLaboratorySupplier == isLaboratorySupplier.Value);

            if (isConsumableSupplier.HasValue)
                query = query.Where(x => x.IsConsumableSupplier == isConsumableSupplier.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.SupplierCode.ToLower().Contains(keyword) ||
                    x.SupplierName.ToLower().Contains(keyword) ||
                    x.SupplierType.ToLower().Contains(keyword) ||
                    (x.LegalName != null && x.LegalName.ToLower().Contains(keyword)) ||
                    (x.SupplierGroupName != null && x.SupplierGroupName.ToLower().Contains(keyword)) ||
                    (x.TaxNumber != null && x.TaxNumber.ToLower().Contains(keyword)) ||
                    (x.BusinessLicenseNumber != null && x.BusinessLicenseNumber.ToLower().Contains(keyword)) ||
                    (x.ContactPersonName != null && x.ContactPersonName.ToLower().Contains(keyword)) ||
                    (x.PhoneNumber != null && x.PhoneNumber.ToLower().Contains(keyword)) ||
                    (x.WhatsAppNumber != null && x.WhatsAppNumber.ToLower().Contains(keyword)) ||
                    (x.Email != null && x.Email.ToLower().Contains(keyword)) ||
                    (x.Website != null && x.Website.ToLower().Contains(keyword)) ||
                    (x.Address != null && x.Address.ToLower().Contains(keyword)) ||
                    (x.CityName != null && x.CityName.ToLower().Contains(keyword)) ||
                    (x.ProvinceName != null && x.ProvinceName.ToLower().Contains(keyword)) ||
                    (x.PostalCode != null && x.PostalCode.ToLower().Contains(keyword)) ||
                    (x.CountryName != null && x.CountryName.ToLower().Contains(keyword)) ||
                    (x.BankName != null && x.BankName.ToLower().Contains(keyword)) ||
                    (x.BankAccountNumber != null && x.BankAccountNumber.ToLower().Contains(keyword)) ||
                    (x.BankAccountName != null && x.BankAccountName.ToLower().Contains(keyword)) ||
                    (x.BlacklistReason != null && x.BlacklistReason.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private static IOrderedQueryable<MstSupplier> ApplySorting(
            IQueryable<MstSupplier> query,
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

                "suppliercode" => isDescending
                    ? query.OrderByDescending(x => x.SupplierCode)
                    : query.OrderBy(x => x.SupplierCode),

                "suppliername" => isDescending
                    ? query.OrderByDescending(x => x.SupplierName)
                    : query.OrderBy(x => x.SupplierName),

                "legalname" => isDescending
                    ? query.OrderByDescending(x => x.LegalName).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.LegalName).ThenBy(x => x.SupplierName),

                "suppliertype" => isDescending
                    ? query.OrderByDescending(x => x.SupplierType).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.SupplierType).ThenBy(x => x.SupplierName),

                "suppliergroupname" => isDescending
                    ? query.OrderByDescending(x => x.SupplierGroupName).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.SupplierGroupName).ThenBy(x => x.SupplierName),

                "paymenttermdays" => isDescending
                    ? query.OrderByDescending(x => x.PaymentTermDays).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.PaymentTermDays).ThenBy(x => x.SupplierName),

                "leadtimedays" => isDescending
                    ? query.OrderByDescending(x => x.LeadTimeDays).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.LeadTimeDays).ThenBy(x => x.SupplierName),

                "minimumpurchaseamount" => isDescending
                    ? query.OrderByDescending(x => x.MinimumPurchaseAmount).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.MinimumPurchaseAmount).ThenBy(x => x.SupplierName),

                "creditlimitamount" => isDescending
                    ? query.OrderByDescending(x => x.CreditLimitAmount).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.CreditLimitAmount).ThenBy(x => x.SupplierName),

                "ispreferredsupplier" => isDescending
                    ? query.OrderByDescending(x => x.IsPreferredSupplier).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsPreferredSupplier).ThenBy(x => x.SupplierName),

                "isblacklisted" => isDescending
                    ? query.OrderByDescending(x => x.IsBlacklisted).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsBlacklisted).ThenBy(x => x.SupplierName),

                "istaxable" => isDescending
                    ? query.OrderByDescending(x => x.IsTaxable).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsTaxable).ThenBy(x => x.SupplierName),

                "isactive" => isDescending
                    ? query.OrderByDescending(x => x.IsActive).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsActive).ThenBy(x => x.SupplierName),

                _ => isDescending
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.SupplierName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.SupplierName)
            };
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            CreateSupplierRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.SupplierName))
                return (false, "Nama supplier wajib diisi.");

            if (string.IsNullOrWhiteSpace(request.SupplierType))
                return (false, "Tipe supplier wajib diisi.");

            if (!AllowedSupplierTypes.Contains(request.SupplierType.Trim()))
                return (false, "Tipe supplier tidak valid. Gunakan General, Pharmacy, MedicalDevice, Laboratory, Consumable, Distributor, Principal, Manufacturer, atau Other.");

            if (request.PaymentTermDays < 0)
                return (false, "Payment term tidak boleh kurang dari 0 hari.");

            if (request.LeadTimeDays < 0)
                return (false, "Lead time tidak boleh kurang dari 0 hari.");

            if (request.MinimumPurchaseAmount < 0)
                return (false, "Minimum purchase amount tidak boleh kurang dari 0.");

            if (request.CreditLimitAmount.HasValue && request.CreditLimitAmount.Value < 0)
                return (false, "Credit limit amount tidak boleh kurang dari 0.");

            if (request.TaxPercent.HasValue && (request.TaxPercent.Value < 0 || request.TaxPercent.Value > 100))
                return (false, "Tax percent harus berada di antara 0 sampai 100.");

            if (request.IsTaxable && !request.TaxPercent.HasValue)
                return (false, "Tax percent wajib diisi jika supplier taxable.");

            if (!request.IsTaxable && request.TaxPercent.HasValue)
                return (false, "Tax percent hanya boleh diisi jika supplier taxable.");

            if (request.IsBlacklisted && string.IsNullOrWhiteSpace(request.BlacklistReason))
                return (false, "Alasan blacklist wajib diisi jika supplier masuk blacklist.");

            if (request.IsBlacklisted && request.IsPreferredSupplier)
                return (false, "Supplier blacklist tidak boleh menjadi preferred supplier.");

            if (!string.IsNullOrWhiteSpace(request.Email) && !new EmailAddressAttribute().IsValid(request.Email.Trim()))
                return (false, "Format email supplier tidak valid.");

            if (!string.IsNullOrWhiteSpace(request.Website) &&
                !Uri.TryCreate(request.Website.Trim(), UriKind.Absolute, out _))
            {
                return (false, "Format website supplier tidak valid. Gunakan URL lengkap seperti https://contoh.com.");
            }

            var normalizedName = request.SupplierName.Trim().ToLower();

            var duplicateNameQuery = _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.SupplierName.ToLower() == normalizedName);

            if (excludeId.HasValue)
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateNameQuery.AnyAsync())
                return (false, "Nama supplier sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(request.LegalName))
            {
                var normalizedLegalName = request.LegalName.Trim().ToLower();

                var duplicateLegalNameQuery = _dbContext.Set<MstSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.LegalName != null &&
                        x.LegalName.ToLower() == normalizedLegalName);

                if (excludeId.HasValue)
                    duplicateLegalNameQuery = duplicateLegalNameQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateLegalNameQuery.AnyAsync())
                    return (false, "Nama legal supplier sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(request.TaxNumber))
            {
                var normalizedTaxNumber = request.TaxNumber.Trim().ToLower();

                var duplicateTaxNumberQuery = _dbContext.Set<MstSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.TaxNumber != null &&
                        x.TaxNumber.ToLower() == normalizedTaxNumber);

                if (excludeId.HasValue)
                    duplicateTaxNumberQuery = duplicateTaxNumberQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateTaxNumberQuery.AnyAsync())
                    return (false, "Tax number supplier sudah digunakan.");
            }

            if (!string.IsNullOrWhiteSpace(request.BusinessLicenseNumber))
            {
                var normalizedLicenseNumber = request.BusinessLicenseNumber.Trim().ToLower();

                var duplicateLicenseNumberQuery = _dbContext.Set<MstSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.BusinessLicenseNumber != null &&
                        x.BusinessLicenseNumber.ToLower() == normalizedLicenseNumber);

                if (excludeId.HasValue)
                    duplicateLicenseNumberQuery = duplicateLicenseNumberQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateLicenseNumberQuery.AnyAsync())
                    return (false, "Business license number supplier sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateSupplierCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstSupplier>()
                .IgnoreQueryFilters()
                .AsNoTracking()
                .Where(x => x.SupplierCode.StartsWith(SupplierCodePrefix))
                .Select(x => x.SupplierCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractSupplierSequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return SupplierCodePrefix + nextNumber.ToString().PadLeft(SupplierCodeDigitLength, '0');
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

        private static SupplierResponse MapResponse(
            MstSupplier entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new SupplierResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                LegalName = entity.LegalName,
                SupplierType = entity.SupplierType,
                SupplierTypeName = BuildSupplierTypeLabel(entity.SupplierType),
                SupplierGroupName = entity.SupplierGroupName,
                TaxNumber = entity.TaxNumber,
                BusinessLicenseNumber = entity.BusinessLicenseNumber,
                ContactPersonName = entity.ContactPersonName,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                Website = entity.Website,
                CityName = entity.CityName,
                ProvinceName = entity.ProvinceName,
                PostalCode = entity.PostalCode,
                CountryName = entity.CountryName,
                PaymentTermDays = entity.PaymentTermDays,
                LeadTimeDays = entity.LeadTimeDays,
                MinimumPurchaseAmount = entity.MinimumPurchaseAmount,
                CreditLimitAmount = entity.CreditLimitAmount,
                IsTaxable = entity.IsTaxable,
                TaxPercent = entity.TaxPercent,
                IsPrincipal = entity.IsPrincipal,
                IsDistributor = entity.IsDistributor,
                IsManufacturer = entity.IsManufacturer,
                IsPharmacySupplier = entity.IsPharmacySupplier,
                IsMedicalDeviceSupplier = entity.IsMedicalDeviceSupplier,
                IsLaboratorySupplier = entity.IsLaboratorySupplier,
                IsConsumableSupplier = entity.IsConsumableSupplier,
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsBlacklisted = entity.IsBlacklisted,
                SortOrder = entity.SortOrder,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static SupplierDetailResponse MapDetailResponse(
            MstSupplier entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
        {
            return new SupplierDetailResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                LegalName = entity.LegalName,
                SupplierType = entity.SupplierType,
                SupplierTypeName = BuildSupplierTypeLabel(entity.SupplierType),
                SupplierGroupName = entity.SupplierGroupName,
                TaxNumber = entity.TaxNumber,
                BusinessLicenseNumber = entity.BusinessLicenseNumber,
                ContactPersonName = entity.ContactPersonName,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                Website = entity.Website,
                Address = entity.Address,
                CityName = entity.CityName,
                ProvinceName = entity.ProvinceName,
                PostalCode = entity.PostalCode,
                CountryName = entity.CountryName,
                BankName = entity.BankName,
                BankAccountNumber = entity.BankAccountNumber,
                BankAccountName = entity.BankAccountName,
                PaymentTermDays = entity.PaymentTermDays,
                LeadTimeDays = entity.LeadTimeDays,
                MinimumPurchaseAmount = entity.MinimumPurchaseAmount,
                CreditLimitAmount = entity.CreditLimitAmount,
                IsTaxable = entity.IsTaxable,
                TaxPercent = entity.TaxPercent,
                IsPrincipal = entity.IsPrincipal,
                IsDistributor = entity.IsDistributor,
                IsManufacturer = entity.IsManufacturer,
                IsPharmacySupplier = entity.IsPharmacySupplier,
                IsMedicalDeviceSupplier = entity.IsMedicalDeviceSupplier,
                IsLaboratorySupplier = entity.IsLaboratorySupplier,
                IsConsumableSupplier = entity.IsConsumableSupplier,
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsBlacklisted = entity.IsBlacklisted,
                BlacklistReason = entity.BlacklistReason,
                SortOrder = entity.SortOrder,
                Description = entity.Description,
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy),
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static SupplierCreateResponse MapCreateResponse(MstSupplier entity)
        {
            return new SupplierCreateResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                SupplierType = entity.SupplierType,
                SupplierTypeName = BuildSupplierTypeLabel(entity.SupplierType),
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsBlacklisted = entity.IsBlacklisted,
                IsActive = entity.IsActive
            };
        }

        private static SupplierUpdateResponse MapUpdateResponse(MstSupplier entity)
        {
            return new SupplierUpdateResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                SupplierType = entity.SupplierType,
                SupplierTypeName = BuildSupplierTypeLabel(entity.SupplierType),
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsBlacklisted = entity.IsBlacklisted,
                IsActive = entity.IsActive,
                UpdateDateTime = entity.UpdateDateTime
            };
        }

        private static string NormalizeSupplierType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedSupplierTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private static string BuildSupplierTypeLabel(string value)
        {
            return value switch
            {
                "MedicalDevice" => "Medical Device",
                _ => SplitPascalCase(value)
            };
        }

        private static string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
        }

        private static string? GetActorName(
            IReadOnlyDictionary<Guid, string?> actorNames,
            Guid actorId)
        {
            if (actorId == Guid.Empty)
                return null;

            return actorNames.TryGetValue(actorId, out var actorName)
                ? actorName
                : null;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(
            int pageNumber,
            int pageSize)
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
            var userIdValue =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue("user_id");

            return Guid.TryParse(userIdValue, out var userId)
                ? userId
                : Guid.Empty;
        }

        private static string? NormalizeNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private static string? NormalizeLowerNullableString(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim().ToLowerInvariant();
        }
    }
}
