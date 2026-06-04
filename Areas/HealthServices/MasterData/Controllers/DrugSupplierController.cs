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

using ResponseDrugSupplierPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DrugSupplierResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/drug-suppliers")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Drug Supplier",
        AreaName = "HealthServices",
        ControllerName = "DrugSupplier",
        Description = "Health service master data drug supplier mapping",
        SortOrder = 15
    )]
    [Tags("Health Services / Master Data / Drug Supplier")]
    public class DrugSupplierController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string CodePrefix = "DSUP-RSMMC-";
        private const int CodeNumberLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DrugSupplierController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Supplier", Description = "Melihat data drug supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugSupplier", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DrugSupplierFilterMetadataResponse
            {
                DefaultFilter = new DrugSupplierDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<DrugSupplierSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "drugSupplierCode", Label = "Kode drug supplier" },
                    new() { Value = "drugName", Label = "Nama obat" },
                    new() { Value = "supplierName", Label = "Nama supplier" },
                    new() { Value = "supplierDrugCode", Label = "Kode obat supplier" },
                    new() { Value = "defaultPurchasePrice", Label = "Harga beli default" },
                    new() { Value = "leadTimeDays", Label = "Lead time" },
                    new() { Value = "isPreferredSupplier", Label = "Preferred supplier" },
                    new() { Value = "isDefaultForPurchase", Label = "Default pembelian" },
                    new() { Value = "isAllowPurchase", Label = "Boleh pembelian" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugSupplier.GetFilterMetadata",
                "Mengambil metadata filter drug supplier.",
                result
            );

            return Ok(ApiResponse<DrugSupplierFilterMetadataResponse>.Ok(
                result,
                "Metadata filter drug supplier berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Supplier", Description = "Melihat data drug supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugSupplier", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<MstDrugSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DrugSupplierSummaryResponse
            {
                TotalDrugSupplier = await query.CountAsync(),
                ActiveDrugSupplier = await query.CountAsync(x => x.IsActive),
                InactiveDrugSupplier = await query.CountAsync(x => !x.IsActive),
                PreferredSupplier = await query.CountAsync(x => x.IsPreferredSupplier),
                ContractSupplier = await query.CountAsync(x => x.IsContractSupplier),
                DefaultForPurchase = await query.CountAsync(x => x.IsDefaultForPurchase),
                AllowPurchase = await query.CountAsync(x => x.IsAllowPurchase),
                RequireQuotation = await query.CountAsync(x => x.IsRequireQuotation),
                WithPurchaseUnit = await query.CountAsync(x => x.PurchaseUnitMeasurementId.HasValue),
                EffectiveDrugSupplier = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredDrugSupplier = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today)
            };

            return Ok(ApiResponse<DrugSupplierSummaryResponse>.Ok(
                result,
                "Ringkasan drug supplier berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugSupplierPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Supplier", Description = "Melihat data drug supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugSupplier", "Read")]
        public async Task<IActionResult> GetDrugSuppliers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? drugId,
            [FromQuery] Guid? supplierId,
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

            var dateRange = ResolveDateRange(startDate, endDate, customPeriod);

            if (!dateRange.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    dateRange.ErrorMessage ?? "Filter tanggal tidak valid."
                ));
            }

            var query = BuildBaseQuery();

            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
                query = query.Where(x => x.SupplierId == supplierId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugSupplierResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    GenericName = x.Drug != null ? x.Drug.GenericName : null,
                    BrandName = x.Drug != null ? x.Drug.BrandName : null,
                    SupplierId = x.SupplierId,
                    SupplierCode = x.Supplier != null ? x.Supplier.SupplierCode : string.Empty,
                    SupplierName = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    SupplierType = x.Supplier != null ? x.Supplier.SupplierType : string.Empty,
                    IsSupplierBlacklisted = x.Supplier != null && x.Supplier.IsBlacklisted,
                    DrugSupplierCode = x.DrugSupplierCode,
                    SupplierDrugCode = x.SupplierDrugCode,
                    SupplierDrugName = x.SupplierDrugName,
                    PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                    PurchaseUnitMeasurementCode = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementCode : null,
                    PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementName : null,
                    PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementSymbol : null,
                    MinimumOrderQuantity = x.MinimumOrderQuantity,
                    OrderMultipleQuantity = x.OrderMultipleQuantity,
                    MaximumOrderQuantity = x.MaximumOrderQuantity,
                    MinimumPurchaseAmount = x.MinimumPurchaseAmount,
                    DefaultPurchasePrice = x.DefaultPurchasePrice,
                    LastPurchasePrice = x.LastPurchasePrice,
                    ContractPurchasePrice = x.ContractPurchasePrice,
                    DiscountPercent = x.DiscountPercent,
                    TaxPercent = x.TaxPercent,
                    LeadTimeDays = x.LeadTimeDays,
                    IsPreferredSupplier = x.IsPreferredSupplier,
                    IsContractSupplier = x.IsContractSupplier,
                    IsDefaultForPurchase = x.IsDefaultForPurchase,
                    IsAllowPurchase = x.IsAllowPurchase,
                    IsRequireQuotation = x.IsRequireQuotation,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime
                })
                .ToListAsync();

            var result = new ResponseDrugSupplierPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDrugSupplierPagedResult>.Ok(
                result,
                "Data drug supplier berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Supplier", Description = "Melihat data pilihan drug supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugSupplier", "Read")]
        public async Task<IActionResult> GetDrugSupplierOptions(
    [FromQuery] Guid? drugId,
    [FromQuery] Guid? supplierId,
    [FromQuery] bool onlyActive = true,
    [FromQuery] string? search = null,
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery();

            if (onlyActive)
                query = query.Where(x => x.IsActive);

            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
                query = query.Where(x => x.SupplierId == supplierId.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                .ThenBy(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugSupplierOptionResponse
                {
                    Id = x.Id,

                    DrugId = x.DrugId,
                    DrugName = x.Drug != null
                        ? x.Drug.DrugName
                        : string.Empty,

                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier != null
                        ? x.Supplier.SupplierName
                        : string.Empty,

                    DrugSupplierCode = x.DrugSupplierCode,
                    SupplierDrugCode = x.SupplierDrugCode,
                    SupplierDrugName = x.SupplierDrugName,

                    PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                    PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement != null
                        ? x.PurchaseUnitMeasurement.MeasurementName
                        : null,
                    PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement != null
                        ? x.PurchaseUnitMeasurement.MeasurementSymbol
                        : null,

                    MinimumOrderQuantity = x.MinimumOrderQuantity,
                    OrderMultipleQuantity = x.OrderMultipleQuantity,
                    MaximumOrderQuantity = x.MaximumOrderQuantity,

                    DefaultPurchasePrice = x.DefaultPurchasePrice,
                    LastPurchasePrice = x.LastPurchasePrice,
                    ContractPurchasePrice = x.ContractPurchasePrice,
                    DiscountPercent = x.DiscountPercent,
                    TaxPercent = x.TaxPercent,
                    LeadTimeDays = x.LeadTimeDays,

                    IsPreferredSupplier = x.IsPreferredSupplier,
                    IsContractSupplier = x.IsContractSupplier,
                    IsDefaultForPurchase = x.IsDefaultForPurchase,
                    IsAllowPurchase = x.IsAllowPurchase,
                    IsRequireQuotation = x.IsRequireQuotation
                })
                .ToListAsync();

            var result = new DrugSupplierOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DrugSupplierOptionPagedResponse>.Ok(
                result,
                "Data pilihan drug supplier berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug Supplier", Description = "Melihat detail drug supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugSupplier", "Read")]
        public async Task<IActionResult> GetDrugSupplierById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugSupplierDetailResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    GenericName = x.Drug != null ? x.Drug.GenericName : null,
                    BrandName = x.Drug != null ? x.Drug.BrandName : null,
                    SupplierId = x.SupplierId,
                    SupplierCode = x.Supplier != null ? x.Supplier.SupplierCode : string.Empty,
                    SupplierName = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    SupplierType = x.Supplier != null ? x.Supplier.SupplierType : string.Empty,
                    IsSupplierBlacklisted = x.Supplier != null && x.Supplier.IsBlacklisted,
                    DrugSupplierCode = x.DrugSupplierCode,
                    SupplierDrugCode = x.SupplierDrugCode,
                    SupplierDrugName = x.SupplierDrugName,
                    PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                    PurchaseUnitMeasurementCode = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementCode : null,
                    PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementName : null,
                    PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement != null ? x.PurchaseUnitMeasurement.MeasurementSymbol : null,
                    MinimumOrderQuantity = x.MinimumOrderQuantity,
                    OrderMultipleQuantity = x.OrderMultipleQuantity,
                    MaximumOrderQuantity = x.MaximumOrderQuantity,
                    MinimumPurchaseAmount = x.MinimumPurchaseAmount,
                    DefaultPurchasePrice = x.DefaultPurchasePrice,
                    LastPurchasePrice = x.LastPurchasePrice,
                    ContractPurchasePrice = x.ContractPurchasePrice,
                    DiscountPercent = x.DiscountPercent,
                    TaxPercent = x.TaxPercent,
                    LeadTimeDays = x.LeadTimeDays,
                    IsPreferredSupplier = x.IsPreferredSupplier,
                    IsContractSupplier = x.IsContractSupplier,
                    IsDefaultForPurchase = x.IsDefaultForPurchase,
                    IsAllowPurchase = x.IsAllowPurchase,
                    IsRequireQuotation = x.IsRequireQuotation,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
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
                    "Drug supplier tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DrugSupplierDetailResponse>.Ok(
                data,
                "Detail drug supplier berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Drug Supplier", Description = "Membuat data drug supplier", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DrugSupplier", "Create")]
        public async Task<IActionResult> CreateDrugSupplier([FromBody] CreateDrugSupplierRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                drugId: request.DrugId,
                supplierId: request.SupplierId,
                supplierDrugCode: request.SupplierDrugCode,
                purchaseUnitMeasurementId: request.PurchaseUnitMeasurementId,
                minimumOrderQuantity: request.MinimumOrderQuantity,
                orderMultipleQuantity: request.OrderMultipleQuantity,
                maximumOrderQuantity: request.MaximumOrderQuantity,
                minimumPurchaseAmount: request.MinimumPurchaseAmount,
                defaultPurchasePrice: request.DefaultPurchasePrice,
                lastPurchasePrice: request.LastPurchasePrice,
                contractPurchasePrice: request.ContractPurchasePrice,
                discountPercent: request.DiscountPercent,
                taxPercent: request.TaxPercent,
                leadTimeDays: request.LeadTimeDays,
                isPreferredSupplier: request.IsPreferredSupplier,
                isDefaultForPurchase: request.IsDefaultForPurchase,
                isAllowPurchase: request.IsAllowPurchase,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug supplier tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPreferredSupplier)
            {
                await ResetPreferredSupplierAsync(
                    drugId: request.DrugId,
                    excludeId: null,
                    actorUserId: actorUserId,
                    now: now
                );
            }

            if (request.IsDefaultForPurchase)
            {
                await ResetDefaultForPurchaseAsync(
                    drugId: request.DrugId,
                    excludeId: null,
                    actorUserId: actorUserId,
                    now: now
                );
            }

            var entity = new MstDrugSupplier
            {
                Id = Guid.NewGuid(),
                DrugId = request.DrugId,
                SupplierId = request.SupplierId,
                DrugSupplierCode = await GenerateDrugSupplierCodeAsync(),
                SupplierDrugCode = NormalizeNullableText(request.SupplierDrugCode),
                SupplierDrugName = NormalizeNullableText(request.SupplierDrugName),
                PurchaseUnitMeasurementId = NormalizeNullableGuid(request.PurchaseUnitMeasurementId),
                MinimumOrderQuantity = request.MinimumOrderQuantity,
                OrderMultipleQuantity = request.OrderMultipleQuantity,
                MaximumOrderQuantity = request.MaximumOrderQuantity,
                MinimumPurchaseAmount = request.MinimumPurchaseAmount,
                DefaultPurchasePrice = request.DefaultPurchasePrice,
                LastPurchasePrice = request.LastPurchasePrice,
                ContractPurchasePrice = request.ContractPurchasePrice,
                DiscountPercent = request.DiscountPercent,
                TaxPercent = request.TaxPercent,
                LeadTimeDays = request.LeadTimeDays,
                IsPreferredSupplier = request.IsPreferredSupplier,
                IsContractSupplier = request.IsContractSupplier,
                IsDefaultForPurchase = request.IsDefaultForPurchase,
                IsAllowPurchase = request.IsAllowPurchase,
                IsRequireQuotation = request.IsRequireQuotation,
                EffectiveStartDate = request.EffectiveStartDate,
                EffectiveEndDate = request.EffectiveEndDate,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDrugSupplier>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildBaseQuery()
                .Where(x => x.Id == entity.Id)
                .Select(x => new DrugSupplierCreateResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    SupplierId = x.SupplierId,
                    SupplierName = x.Supplier != null ? x.Supplier.SupplierName : string.Empty,
                    DrugSupplierCode = x.DrugSupplierCode,
                    SupplierDrugCode = x.SupplierDrugCode,
                    IsPreferredSupplier = x.IsPreferredSupplier,
                    IsDefaultForPurchase = x.IsDefaultForPurchase,
                    IsAllowPurchase = x.IsAllowPurchase,
                    IsActive = x.IsActive
                })
                .FirstAsync();

            await transaction.CommitAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugSupplier.CreateDrugSupplier",
                "Membuat data drug supplier.",
                response
            );

            return Ok(ApiResponse<DrugSupplierCreateResponse>.Ok(
                response,
                "Drug supplier berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Drug Supplier", Description = "Mengubah data drug supplier", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugSupplier", "Update")]
        public async Task<IActionResult> UpdateDrugSupplier(Guid id, [FromBody] UpdateDrugSupplierRequest request)
        {
            var entity = await _dbContext.Set<MstDrugSupplier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug supplier tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                drugId: request.DrugId,
                supplierId: request.SupplierId,
                supplierDrugCode: request.SupplierDrugCode,
                purchaseUnitMeasurementId: request.PurchaseUnitMeasurementId,
                minimumOrderQuantity: request.MinimumOrderQuantity,
                orderMultipleQuantity: request.OrderMultipleQuantity,
                maximumOrderQuantity: request.MaximumOrderQuantity,
                minimumPurchaseAmount: request.MinimumPurchaseAmount,
                defaultPurchasePrice: request.DefaultPurchasePrice,
                lastPurchasePrice: request.LastPurchasePrice,
                contractPurchasePrice: request.ContractPurchasePrice,
                discountPercent: request.DiscountPercent,
                taxPercent: request.TaxPercent,
                leadTimeDays: request.LeadTimeDays,
                isPreferredSupplier: request.IsPreferredSupplier,
                isDefaultForPurchase: request.IsDefaultForPurchase,
                isAllowPurchase: request.IsAllowPurchase,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug supplier tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsPreferredSupplier)
            {
                await ResetPreferredSupplierAsync(
                    drugId: request.DrugId,
                    excludeId: id,
                    actorUserId: actorUserId,
                    now: now
                );
            }

            if (request.IsDefaultForPurchase)
            {
                await ResetDefaultForPurchaseAsync(
                    drugId: request.DrugId,
                    excludeId: id,
                    actorUserId: actorUserId,
                    now: now
                );
            }

            entity.DrugId = request.DrugId;
            entity.SupplierId = request.SupplierId;
            entity.SupplierDrugCode = NormalizeNullableText(request.SupplierDrugCode);
            entity.SupplierDrugName = NormalizeNullableText(request.SupplierDrugName);
            entity.PurchaseUnitMeasurementId = NormalizeNullableGuid(request.PurchaseUnitMeasurementId);
            entity.MinimumOrderQuantity = request.MinimumOrderQuantity;
            entity.OrderMultipleQuantity = request.OrderMultipleQuantity;
            entity.MaximumOrderQuantity = request.MaximumOrderQuantity;
            entity.MinimumPurchaseAmount = request.MinimumPurchaseAmount;
            entity.DefaultPurchasePrice = request.DefaultPurchasePrice;
            entity.LastPurchasePrice = request.LastPurchasePrice;
            entity.ContractPurchasePrice = request.ContractPurchasePrice;
            entity.DiscountPercent = request.DiscountPercent;
            entity.TaxPercent = request.TaxPercent;
            entity.LeadTimeDays = request.LeadTimeDays;
            entity.IsPreferredSupplier = request.IsPreferredSupplier;
            entity.IsContractSupplier = request.IsContractSupplier;
            entity.IsDefaultForPurchase = request.IsDefaultForPurchase;
            entity.IsAllowPurchase = request.IsAllowPurchase;
            entity.IsRequireQuotation = request.IsRequireQuotation;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug supplier berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug Supplier", Description = "Menghapus data drug supplier", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DrugSupplier", "Delete")]
        public async Task<IActionResult> DeleteDrugSupplier(Guid id)
        {
            var entity = await _dbContext.Set<MstDrugSupplier>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug supplier tidak ditemukan."
                ));
            }

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = DateTime.UtcNow;
            entity.DeleteBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug supplier berhasil dihapus."
            ));
        }

        private IQueryable<MstDrugSupplier> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrugSupplier>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrugSupplier> ApplySearch(
            IQueryable<MstDrugSupplier> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var keyword = search.Trim().ToLower();

            return query.Where(x =>
                x.DrugSupplierCode.ToLower().Contains(keyword) ||
                (x.SupplierDrugCode != null && x.SupplierDrugCode.ToLower().Contains(keyword)) ||
                (x.SupplierDrugName != null && x.SupplierDrugName.ToLower().Contains(keyword)) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.GenericName != null && x.Drug.GenericName.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.BrandName != null && x.Drug.BrandName.ToLower().Contains(keyword)) ||
                (x.Supplier != null && x.Supplier.SupplierCode.ToLower().Contains(keyword)) ||
                (x.Supplier != null && x.Supplier.SupplierName.ToLower().Contains(keyword)) ||
                (x.Supplier != null && x.Supplier.SupplierType.ToLower().Contains(keyword)) ||
                (x.PurchaseUnitMeasurement != null && x.PurchaseUnitMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                (x.PurchaseUnitMeasurement != null && x.PurchaseUnitMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                (x.PurchaseUnitMeasurement != null && x.PurchaseUnitMeasurement.MeasurementSymbol != null && x.PurchaseUnitMeasurement.MeasurementSymbol.ToLower().Contains(keyword)));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid drugId,
            Guid supplierId,
            string? supplierDrugCode,
            Guid? purchaseUnitMeasurementId,
            decimal minimumOrderQuantity,
            decimal orderMultipleQuantity,
            decimal? maximumOrderQuantity,
            decimal? minimumPurchaseAmount,
            decimal defaultPurchasePrice,
            decimal? lastPurchasePrice,
            decimal? contractPurchasePrice,
            decimal? discountPercent,
            decimal? taxPercent,
            int leadTimeDays,
            bool isPreferredSupplier,
            bool isDefaultForPurchase,
            bool isAllowPurchase,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (drugId == Guid.Empty)
                return (false, "Obat wajib dipilih.");

            if (supplierId == Guid.Empty)
                return (false, "Supplier wajib dipilih.");

            if (minimumOrderQuantity <= 0)
                return (false, "Minimum order quantity harus lebih dari 0.");

            if (orderMultipleQuantity <= 0)
                return (false, "Order multiple quantity harus lebih dari 0.");

            if (maximumOrderQuantity.HasValue && maximumOrderQuantity.Value <= 0)
                return (false, "Maximum order quantity harus lebih dari 0.");

            if (maximumOrderQuantity.HasValue && maximumOrderQuantity.Value < minimumOrderQuantity)
                return (false, "Maximum order quantity tidak boleh lebih kecil dari minimum order quantity.");

            if (minimumPurchaseAmount.HasValue && minimumPurchaseAmount.Value < 0)
                return (false, "Minimum purchase amount tidak boleh kurang dari 0.");

            if (defaultPurchasePrice < 0)
                return (false, "Default purchase price tidak boleh kurang dari 0.");

            if (lastPurchasePrice.HasValue && lastPurchasePrice.Value < 0)
                return (false, "Last purchase price tidak boleh kurang dari 0.");

            if (contractPurchasePrice.HasValue && contractPurchasePrice.Value < 0)
                return (false, "Contract purchase price tidak boleh kurang dari 0.");

            if (discountPercent.HasValue && (discountPercent.Value < 0 || discountPercent.Value > 100))
                return (false, "Discount percent harus berada di antara 0 sampai 100.");

            if (taxPercent.HasValue && (taxPercent.Value < 0 || taxPercent.Value > 100))
                return (false, "Tax percent harus berada di antara 0 sampai 100.");

            if (leadTimeDays < 0)
                return (false, "Lead time tidak boleh kurang dari 0 hari.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate.Value.Date < effectiveStartDate.Value.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            if ((isPreferredSupplier || isDefaultForPurchase) && !isAllowPurchase)
                return (false, "Preferred/default supplier harus boleh digunakan untuk pembelian.");

            var drugExists = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == drugId && x.IsActive && !x.IsDelete);

            if (!drugExists)
                return (false, "Obat tidak valid atau tidak aktif.");

            var supplier = await _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => x.Id == supplierId && x.IsActive && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.IsBlacklisted,
                    x.SupplierName
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return (false, "Supplier tidak valid atau tidak aktif.");

            if (supplier.IsBlacklisted && (isAllowPurchase || isPreferredSupplier || isDefaultForPurchase))
                return (false, "Supplier sedang blacklist sehingga tidak dapat digunakan untuk pembelian.");

            var normalizedPurchaseUnitMeasurementId = NormalizeNullableGuid(purchaseUnitMeasurementId);

            if (normalizedPurchaseUnitMeasurementId.HasValue)
            {
                var measurementExists = await _dbContext.Set<MstMeasurement>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedPurchaseUnitMeasurementId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!measurementExists)
                    return (false, "Satuan pembelian tidak valid atau tidak aktif.");
            }

            var duplicateMappingQuery = _dbContext.Set<MstDrugSupplier>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.SupplierId == supplierId);

            if (excludeId.HasValue)
                duplicateMappingQuery = duplicateMappingQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateMappingQuery.AnyAsync())
                return (false, "Supplier tersebut sudah terdaftar untuk obat ini.");

            if (!string.IsNullOrWhiteSpace(supplierDrugCode))
            {
                var normalizedSupplierDrugCode = supplierDrugCode.Trim().ToUpperInvariant();

                var duplicateSupplierDrugCodeQuery = _dbContext.Set<MstDrugSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.SupplierId == supplierId &&
                        x.SupplierDrugCode != null &&
                        x.SupplierDrugCode.ToUpper() == normalizedSupplierDrugCode);

                if (excludeId.HasValue)
                    duplicateSupplierDrugCodeQuery = duplicateSupplierDrugCodeQuery.Where(x => x.Id != excludeId.Value);

                if (await duplicateSupplierDrugCodeQuery.AnyAsync())
                    return (false, "Kode obat supplier sudah digunakan pada supplier tersebut.");
            }

            return (true, null);
        }

        private async Task ResetPreferredSupplierAsync(
            Guid drugId,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var items = await _dbContext.Set<MstDrugSupplier>()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.IsPreferredSupplier &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var item in items)
            {
                item.IsPreferredSupplier = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task ResetDefaultForPurchaseAsync(
            Guid drugId,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var items = await _dbContext.Set<MstDrugSupplier>()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.IsDefaultForPurchase &&
                    (!excludeId.HasValue || x.Id != excludeId.Value))
                .ToListAsync();

            foreach (var item in items)
            {
                item.IsDefaultForPurchase = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateDrugSupplierCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDrugSupplier>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugSupplierCode.StartsWith(CodePrefix))
                .Select(x => x.DrugSupplierCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractCodeNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return CodePrefix + nextNumber.ToString().PadLeft(CodeNumberLength, '0');
        }

        private static int? ExtractCodeNumber(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return null;

            if (!code.StartsWith(CodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = code[CodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstDrugSupplier> ApplySorting(
            IQueryable<MstDrugSupplier> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "drugsuppliercode" => isDesc
                    ? query.OrderByDescending(x => x.DrugSupplierCode)
                    : query.OrderBy(x => x.DrugSupplierCode),

                "drugname" => isDesc
                    ? query.OrderByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                    : query.OrderBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty),

                "suppliername" => isDesc
                    ? query.OrderByDescending(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
                    : query.OrderBy(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty),

                "supplierdrugcode" => isDesc
                    ? query.OrderByDescending(x => x.SupplierDrugCode)
                    : query.OrderBy(x => x.SupplierDrugCode),

                "defaultpurchaseprice" => isDesc
                    ? query.OrderByDescending(x => x.DefaultPurchasePrice)
                    : query.OrderBy(x => x.DefaultPurchasePrice),

                "leadtimedays" => isDesc
                    ? query.OrderByDescending(x => x.LeadTimeDays)
                    : query.OrderBy(x => x.LeadTimeDays),

                "ispreferredsupplier" => isDesc
                    ? query.OrderByDescending(x => x.IsPreferredSupplier)
                    : query.OrderBy(x => x.IsPreferredSupplier),

                "isdefaultforpurchase" => isDesc
                    ? query.OrderByDescending(x => x.IsDefaultForPurchase)
                    : query.OrderBy(x => x.IsDefaultForPurchase),

                "isallowpurchase" => isDesc
                    ? query.OrderByDescending(x => x.IsAllowPurchase)
                    : query.OrderBy(x => x.IsAllowPurchase),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                        .ThenByDescending(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                        .ThenBy(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<DrugSupplierCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DrugSupplierCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "last7Days", Label = "7 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "lastMonth", Label = "Bulan lalu" }
            };
        }

        private static (bool IsValid, DateTime? Start, DateTime? EndExclusive, string? ErrorMessage) ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = DateTime.UtcNow.Date;

            DateTime? start = startDate?.Date;
            DateTime? endExclusive = endDate?.Date.AddDays(1);

            if (!string.IsNullOrWhiteSpace(customPeriod))
            {
                switch (customPeriod.Trim().ToLowerInvariant())
                {
                    case "today":
                        start = today;
                        endExclusive = today.AddDays(1);
                        break;

                    case "last7days":
                        start = today.AddDays(-6);
                        endExclusive = today.AddDays(1);
                        break;

                    case "thismonth":
                        start = new DateTime(today.Year, today.Month, 1);
                        endExclusive = start.Value.AddMonths(1);
                        break;

                    case "lastmonth":
                        var firstDayThisMonth = new DateTime(today.Year, today.Month, 1);
                        start = firstDayThisMonth.AddMonths(-1);
                        endExclusive = firstDayThisMonth;
                        break;
                }
            }

            if (start.HasValue && endExclusive.HasValue && endExclusive.Value <= start.Value)
            {
                return (false, null, null, "EndDate tidak boleh lebih kecil atau sama dengan StartDate.");
            }

            return (true, start, endExclusive, null);
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            if (!value.HasValue || value.Value == Guid.Empty)
                return null;

            return value.Value;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }
    }
}
