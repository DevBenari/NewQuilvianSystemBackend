using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
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
                    new() { Value = "effectiveStartDate", Label = "Tanggal mulai berlaku" },
                    new() { Value = "effectiveEndDate", Label = "Tanggal akhir berlaku" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                EffectiveStatusOptions = new List<string> { "all", "effective", "expired", "future" },
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
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
            var today = AppDateTimeHelper.OperationalDate();
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
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today),
                FutureDrugSupplier = await query.CountAsync(x =>
                    x.EffectiveStartDate.HasValue && x.EffectiveStartDate.Value.Date > today)
            };

            return Ok(ApiResponse<DrugSupplierSummaryResponse>.Ok(
                result,
                "Ringkasan drug supplier berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugSupplierPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Drug Supplier", Description = "Melihat data drug supplier", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugSupplier", "Read")]
        public async Task<IActionResult> GetDrugSuppliers(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? drugId,
            [FromQuery] Guid? supplierId,
            [FromQuery] Guid? purchaseUnitMeasurementId,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isPreferredSupplier,
            [FromQuery] bool? isContractSupplier,
            [FromQuery] bool? isDefaultForPurchase,
            [FromQuery] bool? isAllowPurchase,
            [FromQuery] bool? isRequireQuotation,
            [FromQuery] string? effectiveStatus,
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
            query = ApplyDateFilter(query, dateRange);
            query = ApplyStandardFilter(
                query,
                drugId,
                supplierId,
                purchaseUnitMeasurementId,
                isActive,
                isPreferredSupplier,
                isContractSupplier,
                isDefaultForPurchase,
                isAllowPurchase,
                isRequireQuotation,
                effectiveStatus,
                search
            );

            var totalData = await query.CountAsync();
            var data = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorMap = await GetActorNameMapAsync(data.Select(x => x.CreateBy));
            var items = data.Select(x => ToResponse(x, actorMap)).ToList();

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
            [FromQuery] bool onlyAllowPurchase = false,
            [FromQuery] string? effectiveStatus = null,
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

            if (onlyAllowPurchase)
                query = query.Where(x => x.IsAllowPurchase);

            query = ApplyStandardFilter(
                query,
                drugId,
                supplierId,
                purchaseUnitMeasurementId: null,
                isActive: null,
                isPreferredSupplier: null,
                isContractSupplier: null,
                isDefaultForPurchase: null,
                isAllowPurchase: null,
                isRequireQuotation: null,
                effectiveStatus,
                search
            );

            var totalData = await query.CountAsync();
            var data = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                .ThenBy(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = data.Select(ToOptionResponse).ToList();

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
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug supplier tidak ditemukan."
                ));
            }

            var actorMap = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var response = ToDetailResponse(entity, actorMap);

            return Ok(ApiResponse<DrugSupplierDetailResponse>.Ok(
                response,
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
                request: request
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
                SupplierDrugCode = NormalizeNullableText(request.SupplierDrugCode)?.ToUpperInvariant(),
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
            await transaction.CommitAsync();

            var response = await BuildCreateUpdateResponseAsync(entity.Id);

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
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierUpdateResponse>), StatusCodes.Status200OK)]
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
                request: request
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
            entity.SupplierDrugCode = NormalizeNullableText(request.SupplierDrugCode)?.ToUpperInvariant();
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

            var response = await BuildUpdateResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugSupplier.UpdateDrugSupplier",
                "Mengubah data drug supplier.",
                response
            );

            return Ok(ApiResponse<DrugSupplierUpdateResponse>.Ok(
                response,
                "Drug supplier berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Supplier Status", Description = "Mengubah status aktif drug supplier", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugSupplier", "Update")]
        public async Task<IActionResult> UpdateDrugSupplierStatus(Guid id, [FromBody] UpdateDrugSupplierStatusRequest request)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsActive = request.IsActive;
            if (!request.IsActive)
            {
                entity.IsPreferredSupplier = false;
                entity.IsDefaultForPurchase = false;
            }

            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = await BuildUpdateResponseAsync(entity.Id);

            return Ok(ApiResponse<DrugSupplierUpdateResponse>.Ok(
                response,
                request.IsActive
                    ? "Drug supplier berhasil diaktifkan."
                    : "Drug supplier berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugSupplierDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug Supplier", Description = "Menghapus data drug supplier", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DrugSupplier", "Delete")]
        public async Task<IActionResult> DeleteDrugSupplier(Guid id, [FromBody] DeleteDrugSupplierRequest? request = null)
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

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.IsPreferredSupplier = false;
            entity.IsDefaultForPurchase = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            var deleteReason = NormalizeNullableText(request?.DeleteReason);
            if (!string.IsNullOrWhiteSpace(deleteReason))
                entity.Description = deleteReason;

            await _dbContext.SaveChangesAsync();

            var response = new DrugSupplierDeleteResponse
            {
                Id = entity.Id,
                DrugSupplierCode = entity.DrugSupplierCode,
                SupplierDrugCode = entity.SupplierDrugCode,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<DrugSupplierDeleteResponse>.Ok(
                response,
                "Drug supplier berhasil dihapus."
            ));
        }

        private IQueryable<MstDrugSupplier> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrugSupplier>()
                .AsNoTracking()
                .Include(x => x.Drug)
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseUnitMeasurement)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrugSupplier> ApplyDateFilter(
            IQueryable<MstDrugSupplier> query,
            DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            return query;
        }

        private static IQueryable<MstDrugSupplier> ApplyStandardFilter(
            IQueryable<MstDrugSupplier> query,
            Guid? drugId,
            Guid? supplierId,
            Guid? purchaseUnitMeasurementId,
            bool? isActive,
            bool? isPreferredSupplier,
            bool? isContractSupplier,
            bool? isDefaultForPurchase,
            bool? isAllowPurchase,
            bool? isRequireQuotation,
            string? effectiveStatus,
            string? search)
        {
            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);

            if (supplierId.HasValue && supplierId.Value != Guid.Empty)
                query = query.Where(x => x.SupplierId == supplierId.Value);

            if (purchaseUnitMeasurementId.HasValue && purchaseUnitMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.PurchaseUnitMeasurementId == purchaseUnitMeasurementId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isPreferredSupplier.HasValue)
                query = query.Where(x => x.IsPreferredSupplier == isPreferredSupplier.Value);

            if (isContractSupplier.HasValue)
                query = query.Where(x => x.IsContractSupplier == isContractSupplier.Value);

            if (isDefaultForPurchase.HasValue)
                query = query.Where(x => x.IsDefaultForPurchase == isDefaultForPurchase.Value);

            if (isAllowPurchase.HasValue)
                query = query.Where(x => x.IsAllowPurchase == isAllowPurchase.Value);

            if (isRequireQuotation.HasValue)
                query = query.Where(x => x.IsRequireQuotation == isRequireQuotation.Value);

            query = ApplyEffectiveStatusFilter(query, effectiveStatus);
            query = ApplySearch(query, search);

            return query;
        }

        private static IQueryable<MstDrugSupplier> ApplyEffectiveStatusFilter(
            IQueryable<MstDrugSupplier> query,
            string? effectiveStatus)
        {
            if (string.IsNullOrWhiteSpace(effectiveStatus) ||
                string.Equals(effectiveStatus, "all", StringComparison.OrdinalIgnoreCase))
            {
                return query;
            }

            var today = AppDateTimeHelper.OperationalDate();
            var status = effectiveStatus.Trim().ToLowerInvariant();

            return status switch
            {
                "effective" => query.Where(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                "expired" => query.Where(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today),
                "future" => query.Where(x =>
                    x.EffectiveStartDate.HasValue && x.EffectiveStartDate.Value.Date > today),
                _ => query
            };
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
            CreateDrugSupplierRequest request)
        {
            if (request.DrugId == Guid.Empty)
                return (false, "Obat wajib dipilih.");

            if (request.SupplierId == Guid.Empty)
                return (false, "Supplier wajib dipilih.");

            if (request.MinimumOrderQuantity <= 0)
                return (false, "Minimum order quantity harus lebih dari 0.");

            if (request.OrderMultipleQuantity <= 0)
                return (false, "Order multiple quantity harus lebih dari 0.");

            if (request.MaximumOrderQuantity.HasValue && request.MaximumOrderQuantity.Value <= 0)
                return (false, "Maximum order quantity harus lebih dari 0.");

            if (request.MaximumOrderQuantity.HasValue && request.MaximumOrderQuantity.Value < request.MinimumOrderQuantity)
                return (false, "Maximum order quantity tidak boleh lebih kecil dari minimum order quantity.");

            if (request.MinimumPurchaseAmount.HasValue && request.MinimumPurchaseAmount.Value < 0)
                return (false, "Minimum purchase amount tidak boleh kurang dari 0.");

            if (request.DefaultPurchasePrice < 0)
                return (false, "Default purchase price tidak boleh kurang dari 0.");

            if (request.LastPurchasePrice.HasValue && request.LastPurchasePrice.Value < 0)
                return (false, "Last purchase price tidak boleh kurang dari 0.");

            if (request.ContractPurchasePrice.HasValue && request.ContractPurchasePrice.Value < 0)
                return (false, "Contract purchase price tidak boleh kurang dari 0.");

            if (request.DiscountPercent.HasValue && (request.DiscountPercent.Value < 0 || request.DiscountPercent.Value > 100))
                return (false, "Discount percent harus berada di antara 0 sampai 100.");

            if (request.TaxPercent.HasValue && (request.TaxPercent.Value < 0 || request.TaxPercent.Value > 100))
                return (false, "Tax percent harus berada di antara 0 sampai 100.");

            if (request.LeadTimeDays < 0)
                return (false, "Lead time tidak boleh kurang dari 0 hari.");

            if (request.LeadTimeDays > 3650)
                return (false, "Lead time tidak boleh lebih dari 3650 hari.");

            if (request.EffectiveStartDate.HasValue && request.EffectiveEndDate.HasValue && request.EffectiveEndDate.Value.Date < request.EffectiveStartDate.Value.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            if ((request.IsPreferredSupplier || request.IsDefaultForPurchase) && !request.IsAllowPurchase)
                return (false, "Preferred/default supplier harus boleh digunakan untuk pembelian.");

            var drugExists = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.DrugId && x.IsActive && !x.IsDelete);

            if (!drugExists)
                return (false, "Obat tidak valid atau tidak aktif.");

            var supplier = await _dbContext.Set<MstSupplier>()
                .AsNoTracking()
                .Where(x => x.Id == request.SupplierId && x.IsActive && !x.IsDelete)
                .Select(x => new
                {
                    x.Id,
                    x.IsBlacklisted,
                    x.SupplierName
                })
                .FirstOrDefaultAsync();

            if (supplier == null)
                return (false, "Supplier tidak valid atau tidak aktif.");

            if (supplier.IsBlacklisted && (request.IsAllowPurchase || request.IsPreferredSupplier || request.IsDefaultForPurchase))
                return (false, "Supplier sedang blacklist sehingga tidak dapat digunakan untuk pembelian.");

            var normalizedPurchaseUnitMeasurementId = NormalizeNullableGuid(request.PurchaseUnitMeasurementId);

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
                    x.DrugId == request.DrugId &&
                    x.SupplierId == request.SupplierId);

            if (excludeId.HasValue)
                duplicateMappingQuery = duplicateMappingQuery.Where(x => x.Id != excludeId.Value);

            if (await duplicateMappingQuery.AnyAsync())
                return (false, "Supplier tersebut sudah terdaftar untuk obat ini.");

            if (!string.IsNullOrWhiteSpace(request.SupplierDrugCode))
            {
                var normalizedSupplierDrugCode = request.SupplierDrugCode.Trim().ToUpperInvariant();

                var duplicateSupplierDrugCodeQuery = _dbContext.Set<MstDrugSupplier>()
                    .AsNoTracking()
                    .Where(x =>
                        !x.IsDelete &&
                        x.SupplierId == request.SupplierId &&
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
                .Where(x => x.DrugSupplierCode.StartsWith(CodePrefix))
                .Select(x => x.DrugSupplierCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractCodeNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

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

        private async Task<DrugSupplierCreateResponse> BuildCreateUpdateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return ToCreateUpdateResponse(entity);
        }

        private async Task<DrugSupplierUpdateResponse> BuildUpdateResponseAsync(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstAsync(x => x.Id == id);

            return new DrugSupplierUpdateResponse
            {
                Id = entity.Id,
                DrugId = entity.DrugId,
                DrugName = entity.Drug?.DrugName ?? string.Empty,
                SupplierId = entity.SupplierId,
                SupplierName = entity.Supplier?.SupplierName ?? string.Empty,
                DrugSupplierCode = entity.DrugSupplierCode,
                SupplierDrugCode = entity.SupplierDrugCode,
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsDefaultForPurchase = entity.IsDefaultForPurchase,
                IsAllowPurchase = entity.IsAllowPurchase,
                IsActive = entity.IsActive
            };
        }

        private static DrugSupplierCreateResponse ToCreateUpdateResponse(MstDrugSupplier entity)
        {
            return new DrugSupplierCreateResponse
            {
                Id = entity.Id,
                DrugId = entity.DrugId,
                DrugName = entity.Drug?.DrugName ?? string.Empty,
                SupplierId = entity.SupplierId,
                SupplierName = entity.Supplier?.SupplierName ?? string.Empty,
                DrugSupplierCode = entity.DrugSupplierCode,
                SupplierDrugCode = entity.SupplierDrugCode,
                IsPreferredSupplier = entity.IsPreferredSupplier,
                IsDefaultForPurchase = entity.IsDefaultForPurchase,
                IsAllowPurchase = entity.IsAllowPurchase,
                IsActive = entity.IsActive
            };
        }

        private static IQueryable<MstDrugSupplier> ApplySorting(
            IQueryable<MstDrugSupplier> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "drugsuppliercode" => isDesc ? query.OrderByDescending(x => x.DrugSupplierCode) : query.OrderBy(x => x.DrugSupplierCode),
                "drugname" => isDesc
                    ? query.OrderByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                    : query.OrderBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty),
                "suppliername" => isDesc
                    ? query.OrderByDescending(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
                    : query.OrderBy(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty),
                "supplierdrugcode" => isDesc ? query.OrderByDescending(x => x.SupplierDrugCode) : query.OrderBy(x => x.SupplierDrugCode),
                "defaultpurchaseprice" => isDesc ? query.OrderByDescending(x => x.DefaultPurchasePrice) : query.OrderBy(x => x.DefaultPurchasePrice),
                "leadtimedays" => isDesc ? query.OrderByDescending(x => x.LeadTimeDays) : query.OrderBy(x => x.LeadTimeDays),
                "ispreferredsupplier" => isDesc ? query.OrderByDescending(x => x.IsPreferredSupplier) : query.OrderBy(x => x.IsPreferredSupplier),
                "isdefaultforpurchase" => isDesc ? query.OrderByDescending(x => x.IsDefaultForPurchase) : query.OrderBy(x => x.IsDefaultForPurchase),
                "isallowpurchase" => isDesc ? query.OrderByDescending(x => x.IsAllowPurchase) : query.OrderBy(x => x.IsAllowPurchase),
                "effectivestartdate" => isDesc ? query.OrderByDescending(x => x.EffectiveStartDate) : query.OrderBy(x => x.EffectiveStartDate),
                "effectiveenddate" => isDesc ? query.OrderByDescending(x => x.EffectiveEndDate) : query.OrderBy(x => x.EffectiveEndDate),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                        .ThenByDescending(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                        .ThenBy(x => x.Supplier != null ? x.Supplier.SupplierName : string.Empty)
            };
        }

        private static DrugSupplierResponse ToResponse(MstDrugSupplier x, Dictionary<Guid, string?> actorMap)
        {
            return new DrugSupplierResponse
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.Drug?.DrugCode ?? string.Empty,
                DrugName = x.Drug?.DrugName ?? string.Empty,
                GenericName = x.Drug?.GenericName,
                BrandName = x.Drug?.BrandName,
                SupplierId = x.SupplierId,
                SupplierCode = x.Supplier?.SupplierCode ?? string.Empty,
                SupplierName = x.Supplier?.SupplierName ?? string.Empty,
                SupplierType = x.Supplier?.SupplierType ?? string.Empty,
                SupplierTypeName = ToSupplierTypeName(x.Supplier?.SupplierType),
                IsSupplierBlacklisted = x.Supplier?.IsBlacklisted ?? false,
                DrugSupplierCode = x.DrugSupplierCode,
                SupplierDrugCode = x.SupplierDrugCode,
                SupplierDrugName = x.SupplierDrugName,
                PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                PurchaseUnitMeasurementCode = x.PurchaseUnitMeasurement?.MeasurementCode,
                PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement?.MeasurementName,
                PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement?.MeasurementSymbol,
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
                IsCurrentlyEffective = IsCurrentlyEffective(x),
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = x.CreateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.CreateBy)
            };
        }

        private static DrugSupplierDetailResponse ToDetailResponse(MstDrugSupplier x, Dictionary<Guid, string?> actorMap)
        {
            var response = new DrugSupplierDetailResponse
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.Drug?.DrugCode ?? string.Empty,
                DrugName = x.Drug?.DrugName ?? string.Empty,
                GenericName = x.Drug?.GenericName,
                BrandName = x.Drug?.BrandName,
                SupplierId = x.SupplierId,
                SupplierCode = x.Supplier?.SupplierCode ?? string.Empty,
                SupplierName = x.Supplier?.SupplierName ?? string.Empty,
                SupplierType = x.Supplier?.SupplierType ?? string.Empty,
                SupplierTypeName = ToSupplierTypeName(x.Supplier?.SupplierType),
                IsSupplierBlacklisted = x.Supplier?.IsBlacklisted ?? false,
                DrugSupplierCode = x.DrugSupplierCode,
                SupplierDrugCode = x.SupplierDrugCode,
                SupplierDrugName = x.SupplierDrugName,
                PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                PurchaseUnitMeasurementCode = x.PurchaseUnitMeasurement?.MeasurementCode,
                PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement?.MeasurementName,
                PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement?.MeasurementSymbol,
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
                IsCurrentlyEffective = IsCurrentlyEffective(x),
                SortOrder = x.SortOrder,
                Description = x.Description,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = x.CreateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.CreateBy),
                UpdateDateTime = x.UpdateDateTime,
                UpdateBy = x.UpdateBy == Guid.Empty ? null : x.UpdateBy,
                UpdateByName = x.UpdateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.UpdateBy)
            };

            return response;
        }

        private static DrugSupplierOptionResponse ToOptionResponse(MstDrugSupplier x)
        {
            return new DrugSupplierOptionResponse
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.Drug?.DrugCode ?? string.Empty,
                DrugName = x.Drug?.DrugName ?? string.Empty,
                SupplierId = x.SupplierId,
                SupplierCode = x.Supplier?.SupplierCode ?? string.Empty,
                SupplierName = x.Supplier?.SupplierName ?? string.Empty,
                DrugSupplierCode = x.DrugSupplierCode,
                SupplierDrugCode = x.SupplierDrugCode,
                SupplierDrugName = x.SupplierDrugName,
                PurchaseUnitMeasurementId = x.PurchaseUnitMeasurementId,
                PurchaseUnitMeasurementName = x.PurchaseUnitMeasurement?.MeasurementName,
                PurchaseUnitMeasurementSymbol = x.PurchaseUnitMeasurement?.MeasurementSymbol,
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
                IsCurrentlyEffective = IsCurrentlyEffective(x)
            };
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (!ids.Any())
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.UserName ?? x.Email
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static bool IsCurrentlyEffective(MstDrugSupplier x)
        {
            var today = AppDateTimeHelper.OperationalDate();
            return (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                   (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today);
        }

        private static string ToSupplierTypeName(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return value.Trim() switch
            {
                "General" => "Umum",
                "Pharmacy" => "Farmasi",
                "MedicalDevice" => "Alat kesehatan",
                "Laboratory" => "Laboratorium",
                "Consumable" => "Consumable",
                "Distributor" => "Distributor",
                "Principal" => "Principal",
                "Manufacturer" => "Manufacturer",
                "Other" => "Lainnya",
                _ => value.Trim()
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
                new() { Value = "all", Label = "Semua", Description = "Tampilkan semua data tanpa filter tanggal.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "today", Label = "Hari ini", Description = "Data yang dibuat hari ini.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "yesterday", Label = "Kemarin", Description = "Data yang dibuat kemarin.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last7days", Label = "7 hari terakhir", Description = "Data yang dibuat dalam 7 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "last30days", Label = "30 hari terakhir", Description = "Data yang dibuat dalam 30 hari terakhir.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisMonth", Label = "Bulan ini", Description = "Data yang dibuat pada bulan berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "lastMonth", Label = "Bulan lalu", Description = "Data yang dibuat pada bulan sebelumnya.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "thisYear", Label = "Tahun ini", Description = "Data yang dibuat pada tahun berjalan.", UsesStartDate = false, UsesEndDate = false },
                new() { Value = "custom", Label = "Custom", Description = "Gunakan startDate dan endDate.", UsesStartDate = true, UsesEndDate = true }
            };
        }

        private static DateRangeResult ResolveDateRange(
            DateTime? startDate,
            DateTime? endDate,
            string? customPeriod)
        {
            var today = AppDateTimeHelper.OperationalDate();
            var period = customPeriod?.Trim().ToLowerInvariant();

            if (!string.IsNullOrWhiteSpace(period) && period != "custom")
            {
                return period switch
                {
                    "all" => DateRangeResult.Valid(null, null),
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "yesterday" => DateRangeResult.Valid(today.AddDays(-1), today),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "last30days" => DateRangeResult.Valid(today.AddDays(-29), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    "thisyear" => DateRangeResult.Valid(new DateTime(today.Year, 1, 1), new DateTime(today.Year + 1, 1, 1)),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
        }

        private static List<DrugSupplierQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<DrugSupplierQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode. Jika bukan custom, startDate dan endDate diabaikan.", Example = "thisMonth" },
                new() { Name = "drugId", Type = "guid", Description = "Filter obat." },
                new() { Name = "supplierId", Type = "guid", Description = "Filter supplier." },
                new() { Name = "purchaseUnitMeasurementId", Type = "guid", Description = "Filter satuan pembelian." },
                new() { Name = "isPreferredSupplier", Type = "boolean", Description = "Filter supplier preferred." },
                new() { Name = "isDefaultForPurchase", Type = "boolean", Description = "Filter default pembelian." },
                new() { Name = "effectiveStatus", Type = "string", Description = "all, effective, expired, atau future.", Example = "effective" },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "search", Type = "string", Description = "Pencarian kode mapping, obat, supplier, atau satuan pembelian." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<DrugSupplierFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<DrugSupplierFormFieldMetadataResponse>
            {
                new() { Name = "drugSupplierCode", Label = "Kode drug supplier", DataType = "string", InputType = "text", Required = false, IsReadonly = true, Placeholder = "Auto generated", Description = "Dibuat otomatis oleh sistem dengan format DSUP-RSMMC-00001." },
                new() { Name = "drugId", Label = "Obat", DataType = "guid", InputType = "select", Required = true, IsReadonly = false },
                new() { Name = "supplierId", Label = "Supplier", DataType = "guid", InputType = "select", Required = true, IsReadonly = false },
                new() { Name = "supplierDrugCode", Label = "Kode obat supplier", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "supplierDrugName", Label = "Nama obat supplier", DataType = "string", InputType = "text", Required = false, IsReadonly = false },
                new() { Name = "purchaseUnitMeasurementId", Label = "Satuan pembelian", DataType = "guid", InputType = "select", Required = false, IsReadonly = false },
                new() { Name = "minimumOrderQuantity", Label = "Minimum order", DataType = "decimal", InputType = "number", Required = true, IsReadonly = false },
                new() { Name = "orderMultipleQuantity", Label = "Kelipatan order", DataType = "decimal", InputType = "number", Required = true, IsReadonly = false },
                new() { Name = "maximumOrderQuantity", Label = "Maksimum order", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "minimumPurchaseAmount", Label = "Minimum nominal pembelian", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "defaultPurchasePrice", Label = "Harga beli default", DataType = "decimal", InputType = "number", Required = true, IsReadonly = false },
                new() { Name = "lastPurchasePrice", Label = "Harga beli terakhir", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "contractPurchasePrice", Label = "Harga kontrak", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "discountPercent", Label = "Diskon (%)", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "taxPercent", Label = "Pajak (%)", DataType = "decimal?", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "leadTimeDays", Label = "Lead time hari", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "isPreferredSupplier", Label = "Preferred supplier", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isContractSupplier", Label = "Supplier kontrak", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isDefaultForPurchase", Label = "Default pembelian", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isAllowPurchase", Label = "Boleh pembelian", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "isRequireQuotation", Label = "Wajib quotation", DataType = "boolean", InputType = "switch", Required = false, IsReadonly = false },
                new() { Name = "effectiveStartDate", Label = "Mulai berlaku", DataType = "date", InputType = "date", Required = false, IsReadonly = false },
                new() { Name = "effectiveEndDate", Label = "Akhir berlaku", DataType = "date", InputType = "date", Required = false, IsReadonly = false },
                new() { Name = "sortOrder", Label = "Urutan", DataType = "integer", InputType = "number", Required = false, IsReadonly = false },
                new() { Name = "description", Label = "Deskripsi", DataType = "string", InputType = "textarea", Required = false, IsReadonly = false }
            };
        }

        private static List<DrugSupplierFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();
            fields.Add(new DrugSupplierFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status aktif",
                DataType = "boolean",
                InputType = "switch",
                Required = false,
                IsReadonly = false
            });

            return fields;
        }

        private static Guid? NormalizeNullableGuid(Guid? value)
        {
            return value.HasValue && value.Value != Guid.Empty
                ? value.Value
                : null;
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? null
                : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("user_id");

            return Guid.TryParse(userIdText, out var userId)
                ? userId
                : Guid.Empty;
        }

        private sealed class DateRangeResult
        {
            public bool IsValid { get; private init; }
            public DateTime? Start { get; private init; }
            public DateTime? EndExclusive { get; private init; }
            public string? ErrorMessage { get; private init; }

            public static DateRangeResult Valid(DateTime? start, DateTime? endExclusive)
            {
                return new DateRangeResult
                {
                    IsValid = true,
                    Start = start,
                    EndExclusive = endExclusive
                };
            }

            public static DateRangeResult Invalid(string errorMessage)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
        }
    }
}
