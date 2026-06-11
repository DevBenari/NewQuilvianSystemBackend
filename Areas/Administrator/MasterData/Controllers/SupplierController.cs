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
using System.Globalization;
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

        private static readonly List<string> AllowedSupplierTypes = new()
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
        [AccessAction(
            "Read",
            "Read Supplier",
            Description = "Melihat metadata filter supplier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new SupplierFilterMetadataResponse
            {
                DefaultFilter = new SupplierDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
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
                    new() { Value = "isPrincipal", Label = "Principal" },
                    new() { Value = "isDistributor", Label = "Distributor" },
                    new() { Value = "isManufacturer", Label = "Manufacturer" },
                    new() { Value = "isPharmacySupplier", Label = "Supplier farmasi" },
                    new() { Value = "isMedicalDeviceSupplier", Label = "Supplier alat kesehatan" },
                    new() { Value = "isLaboratorySupplier", Label = "Supplier laboratorium" },
                    new() { Value = "isConsumableSupplier", Label = "Supplier consumable" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                SupplierTypeOptions = BuildSupplierTypeOptions(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata(),
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
        [AccessAction(
            "Read",
            "Read Supplier",
            Description = "Melihat ringkasan supplier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
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
                ConsumableSupplier = await query.CountAsync(x => x.IsConsumableSupplier),
                WithTaxNumberSupplier = await query.CountAsync(x => x.TaxNumber != null && x.TaxNumber != string.Empty),
                WithCreditLimitSupplier = await query.CountAsync(x => x.CreditLimitAmount.HasValue),
                WithBankAccountSupplier = await query.CountAsync(x =>
                    x.BankAccountNumber != null &&
                    x.BankAccountNumber != string.Empty)
            };

            return Ok(ApiResponse<SupplierSummaryResponse>.Ok(
                result,
                "Ringkasan supplier berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseSupplierPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction(
            "Read",
            "Read Supplier",
            Description = "Melihat data supplier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSuppliers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? search,
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
            [FromQuery] bool? hasTaxNumber,
            [FromQuery] bool? hasCreditLimit,
            [FromQuery] bool? hasBankAccount,
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
                hasTaxNumber,
                hasCreditLimit,
                hasBankAccount
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
        [AccessAction(
            "Read",
            "Read Supplier",
            Description = "Melihat data pilihan supplier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
        [AccessPermission("Supplier", "Read")]
        public async Task<IActionResult> GetSupplierOptions(
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? supplierType = null,
            [FromQuery] bool? isPreferredSupplier = null,
            [FromQuery] bool? isBlacklisted = null,
            [FromQuery] bool? isTaxable = null,
            [FromQuery] bool? isPrincipal = null,
            [FromQuery] bool? isDistributor = null,
            [FromQuery] bool? isManufacturer = null,
            [FromQuery] bool? isPharmacySupplier = null,
            [FromQuery] bool? isMedicalDeviceSupplier = null,
            [FromQuery] bool? isLaboratorySupplier = null,
            [FromQuery] bool? isConsumableSupplier = null,
            [FromQuery] bool? hasTaxNumber = null,
            [FromQuery] bool? hasCreditLimit = null,
            [FromQuery] bool? hasBankAccount = null,
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
                hasTaxNumber,
                hasCreditLimit,
                hasBankAccount
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.IsBlacklisted)
                .ThenByDescending(x => x.IsPreferredSupplier)
                .ThenBy(x => x.SortOrder)
                .ThenBy(x => x.SupplierName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities
                .Select(MapOptionResponse)
                .ToList();

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
        [AccessAction(
            "Read",
            "Read Supplier",
            Description = "Melihat detail supplier",
            AccessType = AccessTypes.Read,
            SortOrder = 1
        )]
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
        [AccessAction(
            "Create",
            "Create Supplier",
            Description = "Membuat data supplier",
            AccessType = AccessTypes.Create,
            SortOrder = 2
        )]
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.CreateBy });
            var result = MapCreateResponse(entity, actorNames);

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
        [AccessAction(
            "Update",
            "Update Supplier",
            Description = "Mengubah data supplier",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Supplier", "Update")]
        public async Task<IActionResult> UpdateSupplier(
            Guid id,
            [FromBody] UpdateSupplierRequest request)
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var result = MapUpdateResponse(entity, actorNames);

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
        [AccessAction(
            "Update",
            "Update Supplier Status",
            Description = "Mengubah status supplier",
            AccessType = AccessTypes.Update,
            SortOrder = 3
        )]
        [AccessPermission("Supplier", "Update")]
        public async Task<IActionResult> UpdateSupplierStatus(
            Guid id,
            [FromBody] UpdateSupplierStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var actorNames = await GetActorNameMapAsync(new[] { entity.UpdateBy });
            var result = MapUpdateResponse(entity, actorNames);

            return Ok(ApiResponse<SupplierUpdateResponse>.Ok(
                result,
                "Status supplier berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<SupplierDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(
            "Delete",
            "Delete Supplier",
            Description = "Menghapus data supplier",
            AccessType = AccessTypes.Delete,
            SortOrder = 4
        )]
        [AccessPermission("Supplier", "Delete")]
        public async Task<IActionResult> DeleteSupplier(
            Guid id,
            [FromBody] DeleteSupplierRequest? request = null)
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

            var actorNames = await GetActorNameMapAsync(new[] { entity.DeleteBy });
            var result = new SupplierDeleteResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                DeleteDateTime = entity.DeleteDateTime,
                DeleteBy = entity.DeleteBy == Guid.Empty ? null : (Guid?)entity.DeleteBy,
                DeleteByName = GetActorName(actorNames, entity.DeleteBy)
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

        private static IQueryable<MstSupplier> ApplyStandardFilter(
            IQueryable<MstSupplier> query,
            string? search,
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
            bool? hasTaxNumber,
            bool? hasCreditLimit,
            bool? hasBankAccount)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                var matchedSupplierTypes = AllowedSupplierTypes
                    .Where(x =>
                        x.ToLower().Contains(keyword) ||
                        BuildSupplierTypeLabel(x).ToLower().Contains(keyword))
                    .ToList();

                query = query.Where(x =>
                    x.SupplierCode.ToLower().Contains(keyword) ||
                    x.SupplierName.ToLower().Contains(keyword) ||
                    x.SupplierType.ToLower().Contains(keyword) ||
                    matchedSupplierTypes.Contains(x.SupplierType) ||
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

            if (isActive.HasValue)
            {
                query = query.Where(x => x.IsActive == isActive.Value);
            }

            if (!string.IsNullOrWhiteSpace(supplierType))
            {
                var normalizedSupplierType = NormalizeSupplierType(supplierType);
                query = query.Where(x => x.SupplierType == normalizedSupplierType);
            }

            if (isPreferredSupplier.HasValue)
            {
                query = query.Where(x => x.IsPreferredSupplier == isPreferredSupplier.Value);
            }

            if (isBlacklisted.HasValue)
            {
                query = query.Where(x => x.IsBlacklisted == isBlacklisted.Value);
            }

            if (isTaxable.HasValue)
            {
                query = query.Where(x => x.IsTaxable == isTaxable.Value);
            }

            if (isPrincipal.HasValue)
            {
                query = query.Where(x => x.IsPrincipal == isPrincipal.Value);
            }

            if (isDistributor.HasValue)
            {
                query = query.Where(x => x.IsDistributor == isDistributor.Value);
            }

            if (isManufacturer.HasValue)
            {
                query = query.Where(x => x.IsManufacturer == isManufacturer.Value);
            }

            if (isPharmacySupplier.HasValue)
            {
                query = query.Where(x => x.IsPharmacySupplier == isPharmacySupplier.Value);
            }

            if (isMedicalDeviceSupplier.HasValue)
            {
                query = query.Where(x => x.IsMedicalDeviceSupplier == isMedicalDeviceSupplier.Value);
            }

            if (isLaboratorySupplier.HasValue)
            {
                query = query.Where(x => x.IsLaboratorySupplier == isLaboratorySupplier.Value);
            }

            if (isConsumableSupplier.HasValue)
            {
                query = query.Where(x => x.IsConsumableSupplier == isConsumableSupplier.Value);
            }

            if (hasTaxNumber.HasValue)
            {
                query = hasTaxNumber.Value
                    ? query.Where(x => x.TaxNumber != null && x.TaxNumber != string.Empty)
                    : query.Where(x => x.TaxNumber == null || x.TaxNumber == string.Empty);
            }

            if (hasCreditLimit.HasValue)
            {
                query = hasCreditLimit.Value
                    ? query.Where(x => x.CreditLimitAmount.HasValue)
                    : query.Where(x => !x.CreditLimitAmount.HasValue);
            }

            if (hasBankAccount.HasValue)
            {
                query = hasBankAccount.Value
                    ? query.Where(x => x.BankAccountNumber != null && x.BankAccountNumber != string.Empty)
                    : query.Where(x => x.BankAccountNumber == null || x.BankAccountNumber == string.Empty);
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

                "isprincipal" => isDescending
                    ? query.OrderByDescending(x => x.IsPrincipal).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsPrincipal).ThenBy(x => x.SupplierName),

                "isdistributor" => isDescending
                    ? query.OrderByDescending(x => x.IsDistributor).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsDistributor).ThenBy(x => x.SupplierName),

                "ismanufacturer" => isDescending
                    ? query.OrderByDescending(x => x.IsManufacturer).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsManufacturer).ThenBy(x => x.SupplierName),

                "ispharmacysupplier" => isDescending
                    ? query.OrderByDescending(x => x.IsPharmacySupplier).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsPharmacySupplier).ThenBy(x => x.SupplierName),

                "ismedicaldevicesupplier" => isDescending
                    ? query.OrderByDescending(x => x.IsMedicalDeviceSupplier).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsMedicalDeviceSupplier).ThenBy(x => x.SupplierName),

                "islaboratorysupplier" => isDescending
                    ? query.OrderByDescending(x => x.IsLaboratorySupplier).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsLaboratorySupplier).ThenBy(x => x.SupplierName),

                "isconsumablesupplier" => isDescending
                    ? query.OrderByDescending(x => x.IsConsumableSupplier).ThenBy(x => x.SupplierName)
                    : query.OrderBy(x => x.IsConsumableSupplier).ThenBy(x => x.SupplierName),

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
            {
                return (false, "Nama supplier wajib diisi.");
            }

            if (string.IsNullOrWhiteSpace(request.SupplierType))
            {
                return (false, "Tipe supplier wajib diisi.");
            }

            if (!AllowedSupplierTypes.Any(x => string.Equals(x, request.SupplierType.Trim(), StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "Tipe supplier tidak valid. Gunakan nilai dari endpoint filters/metadata.");
            }

            if (request.PaymentTermDays < 0)
            {
                return (false, "Payment term tidak boleh kurang dari 0 hari.");
            }

            if (request.LeadTimeDays < 0)
            {
                return (false, "Lead time tidak boleh kurang dari 0 hari.");
            }

            if (request.MinimumPurchaseAmount < 0)
            {
                return (false, "Minimum purchase amount tidak boleh kurang dari 0.");
            }

            if (request.CreditLimitAmount.HasValue && request.CreditLimitAmount.Value < 0)
            {
                return (false, "Credit limit amount tidak boleh kurang dari 0.");
            }

            if (request.TaxPercent.HasValue && (request.TaxPercent.Value < 0 || request.TaxPercent.Value > 100))
            {
                return (false, "Tax percent harus berada di antara 0 sampai 100.");
            }

            if (request.IsTaxable && !request.TaxPercent.HasValue)
            {
                return (false, "Tax percent wajib diisi jika supplier taxable.");
            }

            if (!request.IsTaxable && request.TaxPercent.HasValue)
            {
                return (false, "Tax percent hanya boleh diisi jika supplier taxable.");
            }

            if (request.IsBlacklisted && string.IsNullOrWhiteSpace(request.BlacklistReason))
            {
                return (false, "Alasan blacklist wajib diisi jika supplier masuk blacklist.");
            }

            if (request.IsBlacklisted && request.IsPreferredSupplier)
            {
                return (false, "Supplier blacklist tidak boleh menjadi preferred supplier.");
            }

            if (!string.IsNullOrWhiteSpace(request.Email) &&
                !new EmailAddressAttribute().IsValid(request.Email.Trim()))
            {
                return (false, "Format email supplier tidak valid.");
            }

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
            {
                duplicateNameQuery = duplicateNameQuery.Where(x => x.Id != excludeId.Value);
            }

            if (await duplicateNameQuery.AnyAsync())
            {
                return (false, "Nama supplier sudah digunakan.");
            }

            var legalName = NormalizeNullableString(request.LegalName);

            if (!string.IsNullOrWhiteSpace(legalName))
            {
                var normalizedLegalName = legalName.ToLower();

                var duplicateLegalNameQuery = _dbContext.Set<MstSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.LegalName != null &&
                        x.LegalName.ToLower() == normalizedLegalName);

                if (excludeId.HasValue)
                {
                    duplicateLegalNameQuery = duplicateLegalNameQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateLegalNameQuery.AnyAsync())
                {
                    return (false, "Nama legal supplier sudah digunakan.");
                }
            }

            var taxNumber = NormalizeNullableString(request.TaxNumber);

            if (!string.IsNullOrWhiteSpace(taxNumber))
            {
                var normalizedTaxNumber = taxNumber.ToLower();

                var duplicateTaxNumberQuery = _dbContext.Set<MstSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.TaxNumber != null &&
                        x.TaxNumber.ToLower() == normalizedTaxNumber);

                if (excludeId.HasValue)
                {
                    duplicateTaxNumberQuery = duplicateTaxNumberQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateTaxNumberQuery.AnyAsync())
                {
                    return (false, "Tax number supplier sudah digunakan.");
                }
            }

            var businessLicenseNumber = NormalizeNullableString(request.BusinessLicenseNumber);

            if (!string.IsNullOrWhiteSpace(businessLicenseNumber))
            {
                var normalizedBusinessLicenseNumber = businessLicenseNumber.ToLower();

                var duplicateLicenseQuery = _dbContext.Set<MstSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.BusinessLicenseNumber != null &&
                        x.BusinessLicenseNumber.ToLower() == normalizedBusinessLicenseNumber);

                if (excludeId.HasValue)
                {
                    duplicateLicenseQuery = duplicateLicenseQuery.Where(x => x.Id != excludeId.Value);
                }

                if (await duplicateLicenseQuery.AnyAsync())
                {
                    return (false, "Business license number supplier sudah digunakan.");
                }
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
                .Where(x => x > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return SupplierCodePrefix + nextNumber.ToString("D" + SupplierCodeDigitLength, CultureInfo.InvariantCulture);
        }

        private static int? TryExtractSupplierSequenceNumber(string supplierCode)
        {
            if (string.IsNullOrWhiteSpace(supplierCode))
            {
                return null;
            }

            if (!supplierCode.StartsWith(SupplierCodePrefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var numberText = supplierCode[SupplierCodePrefix.Length..];

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

        private static SupplierOptionResponse MapOptionResponse(MstSupplier entity)
        {
            return new SupplierOptionResponse
            {
                Id = entity.Id,
                SupplierCode = entity.SupplierCode,
                SupplierName = entity.SupplierName,
                LegalName = entity.LegalName,
                SupplierType = entity.SupplierType,
                SupplierTypeName = BuildSupplierTypeLabel(entity.SupplierType),
                SupplierGroupName = entity.SupplierGroupName,
                ContactPersonName = entity.ContactPersonName,
                PhoneNumber = entity.PhoneNumber,
                WhatsAppNumber = entity.WhatsAppNumber,
                Email = entity.Email,
                PaymentTermDays = entity.PaymentTermDays,
                LeadTimeDays = entity.LeadTimeDays,
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
                SortOrder = entity.SortOrder
            };
        }

        private static SupplierCreateResponse MapCreateResponse(
            MstSupplier entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
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
                IsActive = entity.IsActive,
                CreateDateTime = entity.CreateDateTime,
                CreateBy = entity.CreateBy == Guid.Empty ? null : (Guid?)entity.CreateBy,
                CreateByName = GetActorName(actorNames, entity.CreateBy)
            };
        }

        private static SupplierUpdateResponse MapUpdateResponse(
            MstSupplier entity,
            IReadOnlyDictionary<Guid, string?> actorNames)
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
                UpdateDateTime = entity.UpdateDateTime,
                UpdateBy = entity.UpdateBy == Guid.Empty ? null : (Guid?)entity.UpdateBy,
                UpdateByName = GetActorName(actorNames, entity.UpdateBy)
            };
        }

        private static List<SupplierTypeOptionResponse> BuildSupplierTypeOptions()
        {
            return AllowedSupplierTypes
                .Select(x => new SupplierTypeOptionResponse
                {
                    Value = x,
                    Label = BuildSupplierTypeLabel(x),
                    Description = BuildSupplierTypeDescription(x)
                })
                .ToList();
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

        private static string BuildSupplierTypeDescription(string value)
        {
            return value switch
            {
                "General" => "Supplier umum non-spesifik.",
                "Pharmacy" => "Supplier obat dan produk farmasi.",
                "MedicalDevice" => "Supplier alat kesehatan.",
                "Laboratory" => "Supplier reagen, alat, atau kebutuhan laboratorium.",
                "Consumable" => "Supplier bahan habis pakai.",
                "Distributor" => "Distributor resmi produk tertentu.",
                "Principal" => "Pemilik atau principal produk.",
                "Manufacturer" => "Produsen atau pabrik produk.",
                "Other" => "Jenis supplier lainnya.",
                _ => string.Empty
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

        private static List<SupplierCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<SupplierCustomPeriodOptionResponse>
            {
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thismonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastmonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false }
            };
        }

        private static List<SupplierQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<SupplierQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "DateTime?", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "DateTime?", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Filter periode cepat: custom, today, last7days, last30days, thismonth, lastmonth.", Example = "thismonth" },
                new() { Name = "search", Type = "string", Description = "Cari berdasarkan kode, nama, legal name, tipe, group, NPWP, kontak, email, alamat, bank, blacklist reason, atau deskripsi.", Example = "farmasi" },
                new() { Name = "supplierType", Type = "string", Description = "Filter berdasarkan tipe supplier.", Example = "Pharmacy" },
                new() { Name = "isActive", Type = "bool", Description = "Filter status aktif.", Example = "true" },
                new() { Name = "isPreferredSupplier", Type = "bool", Description = "Filter preferred supplier.", Example = "true" },
                new() { Name = "isBlacklisted", Type = "bool", Description = "Filter blacklist supplier.", Example = "false" },
                new() { Name = "isTaxable", Type = "bool", Description = "Filter supplier taxable.", Example = "true" },
                new() { Name = "isPrincipal", Type = "bool", Description = "Filter principal.", Example = "true" },
                new() { Name = "isDistributor", Type = "bool", Description = "Filter distributor.", Example = "true" },
                new() { Name = "isManufacturer", Type = "bool", Description = "Filter manufacturer.", Example = "true" },
                new() { Name = "isPharmacySupplier", Type = "bool", Description = "Filter supplier farmasi.", Example = "true" },
                new() { Name = "isMedicalDeviceSupplier", Type = "bool", Description = "Filter supplier alat kesehatan.", Example = "true" },
                new() { Name = "isLaboratorySupplier", Type = "bool", Description = "Filter supplier laboratorium.", Example = "true" },
                new() { Name = "isConsumableSupplier", Type = "bool", Description = "Filter supplier bahan habis pakai.", Example = "true" },
                new() { Name = "hasTaxNumber", Type = "bool", Description = "Filter supplier yang memiliki atau tidak memiliki tax number.", Example = "true" },
                new() { Name = "hasCreditLimit", Type = "bool", Description = "Filter supplier yang memiliki atau tidak memiliki credit limit.", Example = "true" },
                new() { Name = "hasBankAccount", Type = "bool", Description = "Filter supplier yang memiliki atau tidak memiliki rekening bank.", Example = "true" },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting.", Example = "sortOrder" },
                new() { Name = "sortDirection", Type = "string", Description = "Arah sorting: asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "int", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "int", Description = "Jumlah data per halaman, maksimal 100.", Example = "25" }
            };
        }

        private static List<SupplierFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: false);
        }

        private static List<SupplierFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            return BuildFieldMetadata(isUpdate: true);
        }

        private static List<SupplierFormFieldMetadataResponse> BuildFieldMetadata(bool isUpdate)
        {
            var fields = new List<SupplierFormFieldMetadataResponse>
            {
                new() { Name = "supplierCode", Label = "Kode Supplier", Section = "Basic", InputType = "readonly", RequiredType = "AutoGenerated", MaxLength = 50, Description = "Digenerate otomatis oleh sistem dengan format SUP-RSMMC-00001.", Example = "SUP-RSMMC-00001", SortOrder = 1 },
                new() { Name = "supplierName", Label = "Nama Supplier", Section = "Basic", InputType = "text", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 150, Example = "PT Supplier Farmasi Sehat", SortOrder = 2 },
                new() { Name = "legalName", Label = "Nama Legal", Section = "Basic", InputType = "text", MaxLength = 150, Example = "PT Supplier Farmasi Sehat", SortOrder = 3 },
                new() { Name = "supplierType", Label = "Tipe Supplier", Section = "Basic", InputType = "select", IsRequiredOnCreate = true, IsRequiredOnUpdate = true, RequiredType = "Required", MaxLength = 50, OptionsSource = "supplierTypeOptions", Example = "Pharmacy", SortOrder = 4 },
                new() { Name = "supplierGroupName", Label = "Group Supplier", Section = "Basic", InputType = "text", MaxLength = 100, Example = "Group Farmasi", SortOrder = 5 },
                new() { Name = "taxNumber", Label = "Tax Number / NPWP", Section = "Legal", InputType = "text", MaxLength = 50, SortOrder = 6 },
                new() { Name = "businessLicenseNumber", Label = "Nomor Izin Usaha", Section = "Legal", InputType = "text", MaxLength = 100, SortOrder = 7 },
                new() { Name = "contactPersonName", Label = "Nama Kontak", Section = "Contact", InputType = "text", MaxLength = 100, SortOrder = 8 },
                new() { Name = "phoneNumber", Label = "Nomor Telepon", Section = "Contact", InputType = "text", MaxLength = 50, SortOrder = 9 },
                new() { Name = "whatsAppNumber", Label = "Nomor WhatsApp", Section = "Contact", InputType = "text", MaxLength = 50, SortOrder = 10 },
                new() { Name = "email", Label = "Email", Section = "Contact", InputType = "email", MaxLength = 150, Example = "supplier@example.com", SortOrder = 11 },
                new() { Name = "website", Label = "Website", Section = "Contact", InputType = "url", MaxLength = 150, Example = "https://supplier.com", SortOrder = 12 },
                new() { Name = "address", Label = "Alamat", Section = "Address", InputType = "textarea", MaxLength = 500, SortOrder = 13 },
                new() { Name = "cityName", Label = "Kota", Section = "Address", InputType = "text", MaxLength = 100, SortOrder = 14 },
                new() { Name = "provinceName", Label = "Provinsi", Section = "Address", InputType = "text", MaxLength = 100, SortOrder = 15 },
                new() { Name = "postalCode", Label = "Kode Pos", Section = "Address", InputType = "text", MaxLength = 20, SortOrder = 16 },
                new() { Name = "countryName", Label = "Negara", Section = "Address", InputType = "text", MaxLength = 100, SortOrder = 17 },
                new() { Name = "bankName", Label = "Nama Bank", Section = "Bank", InputType = "text", MaxLength = 100, SortOrder = 18 },
                new() { Name = "bankAccountNumber", Label = "Nomor Rekening", Section = "Bank", InputType = "text", MaxLength = 100, SortOrder = 19 },
                new() { Name = "bankAccountName", Label = "Nama Pemilik Rekening", Section = "Bank", InputType = "text", MaxLength = 150, SortOrder = 20 },
                new() { Name = "paymentTermDays", Label = "Payment Term Days", Section = "Commercial", InputType = "number", MinValue = 0, MaxValue = 3650, SortOrder = 21 },
                new() { Name = "leadTimeDays", Label = "Lead Time Days", Section = "Commercial", InputType = "number", MinValue = 0, MaxValue = 3650, SortOrder = 22 },
                new() { Name = "minimumPurchaseAmount", Label = "Minimum Purchase Amount", Section = "Commercial", InputType = "number", MinValue = 0, MaxValue = 999999999999, SortOrder = 23 },
                new() { Name = "creditLimitAmount", Label = "Credit Limit Amount", Section = "Commercial", InputType = "number", MinValue = 0, MaxValue = 999999999999, SortOrder = 24 },
                new() { Name = "isTaxable", Label = "Taxable", Section = "Tax", InputType = "switch", SortOrder = 25 },
                new() { Name = "taxPercent", Label = "Tax Percent", Section = "Tax", InputType = "number", MinValue = 0, MaxValue = 100, Description = "Wajib diisi jika taxable aktif.", SortOrder = 26 },
                new() { Name = "isPrincipal", Label = "Principal", Section = "Role", InputType = "switch", SortOrder = 27 },
                new() { Name = "isDistributor", Label = "Distributor", Section = "Role", InputType = "switch", SortOrder = 28 },
                new() { Name = "isManufacturer", Label = "Manufacturer", Section = "Role", InputType = "switch", SortOrder = 29 },
                new() { Name = "isPharmacySupplier", Label = "Supplier Farmasi", Section = "Scope", InputType = "switch", SortOrder = 30 },
                new() { Name = "isMedicalDeviceSupplier", Label = "Supplier Alat Kesehatan", Section = "Scope", InputType = "switch", SortOrder = 31 },
                new() { Name = "isLaboratorySupplier", Label = "Supplier Laboratorium", Section = "Scope", InputType = "switch", SortOrder = 32 },
                new() { Name = "isConsumableSupplier", Label = "Supplier Consumable", Section = "Scope", InputType = "switch", SortOrder = 33 },
                new() { Name = "isPreferredSupplier", Label = "Preferred Supplier", Section = "Status", InputType = "switch", Description = "Tidak boleh aktif jika supplier blacklist.", SortOrder = 34 },
                new() { Name = "isBlacklisted", Label = "Blacklist", Section = "Status", InputType = "switch", SortOrder = 35 },
                new() { Name = "blacklistReason", Label = "Alasan Blacklist", Section = "Status", InputType = "textarea", MaxLength = 250, Description = "Wajib diisi jika blacklist aktif.", SortOrder = 36 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", InputType = "number", SortOrder = 37 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", InputType = "textarea", MaxLength = 250, SortOrder = 38 }
            };

            if (isUpdate)
            {
                fields.Add(new SupplierFormFieldMetadataResponse
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
