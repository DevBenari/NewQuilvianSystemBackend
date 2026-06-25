using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponseDrugStockPolicyPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DrugStockPolicyResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/drug-stock-policies")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Drug Stock Policy",
        AreaName = "HealthServices",
        ControllerName = "DrugStockPolicy",
        Description = "Health service master data drug stock policy",
        SortOrder = 16
    )]
    [Tags("Health Services / Master Data / Drug Stock Policy")]
    public class DrugStockPolicyController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string StockPolicyCodePrefix = "DSP-RSMMC-";
        private const int StockPolicyCodeDigitLength = 5;

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DrugStockPolicyController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DrugStockPolicyFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Stock Policy", Description = "Melihat data drug stock policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStockPolicy", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DrugStockPolicyFilterMetadataResponse
            {
                DefaultFilter = new DrugStockPolicyDefaultFilterResponse(),
                CustomPeriods = new List<DrugStockPolicyCustomPeriodOptionResponse>
                {
                    new() { Value = "all", Label = "Semua" },
                    new() { Value = "today", Label = "Hari ini" },
                    new() { Value = "last7days", Label = "7 hari terakhir" },
                    new() { Value = "thisMonth", Label = "Bulan ini" },
                    new() { Value = "lastMonth", Label = "Bulan lalu" },
                    new() { Value = "custom", Label = "Custom" }
                },
                SortOptions = new List<DrugStockPolicySortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "stockPolicyCode", Label = "Kode stock policy" },
                    new() { Value = "stockPolicyName", Label = "Nama stock policy" },
                    new() { Value = "drugName", Label = "Nama obat" },
                    new() { Value = "storageLocationName", Label = "Lokasi penyimpanan" },
                    new() { Value = "serviceUnitName", Label = "Service unit" },
                    new() { Value = "clinicName", Label = "Clinic" },
                    new() { Value = "stockUnitMeasurementName", Label = "Satuan stok" },
                    new() { Value = "minimumStockQuantity", Label = "Minimum stock" },
                    new() { Value = "maximumStockQuantity", Label = "Maximum stock" },
                    new() { Value = "reorderPointQuantity", Label = "Reorder point" },
                    new() { Value = "isAutoReorderEnabled", Label = "Auto reorder" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugStockPolicy.GetFilterMetadata",
                "Mengambil metadata filter drug stock policy.",
                result
            );

            return Ok(ApiResponse<DrugStockPolicyFilterMetadataResponse>.Ok(
                result,
                "Metadata filter drug stock policy berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DrugStockPolicySummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Stock Policy", Description = "Melihat data drug stock policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStockPolicy", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = AppDateTimeHelper.OperationalDate();

            var query = _dbContext.Set<MstDrugStockPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DrugStockPolicySummaryResponse
            {
                TotalDrugStockPolicy = await query.CountAsync(),
                ActiveDrugStockPolicy = await query.CountAsync(x => x.IsActive),
                InactiveDrugStockPolicy = await query.CountAsync(x => !x.IsActive),
                AutoReorderEnabledPolicy = await query.CountAsync(x => x.IsAutoReorderEnabled),
                AllowNegativeStockPolicy = await query.CountAsync(x => x.IsAllowNegativeStock),
                BatchRequiredPolicy = await query.CountAsync(x => x.IsBatchRequired),
                ExpiryDateRequiredPolicy = await query.CountAsync(x => x.IsExpiryDateRequired),
                StockOpnameRequiredPolicy = await query.CountAsync(x => x.IsStockOpnameRequired),
                WithStorageLocationPolicy = await query.CountAsync(x => x.StorageLocationId.HasValue),
                WithServiceUnitPolicy = await query.CountAsync(x => x.ServiceUnitId.HasValue),
                WithClinicPolicy = await query.CountAsync(x => x.ClinicId.HasValue),
                EffectiveDrugStockPolicy = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredDrugStockPolicy = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today)
            };

            return Ok(ApiResponse<DrugStockPolicySummaryResponse>.Ok(
                result,
                "Ringkasan drug stock policy berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugStockPolicyPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Drug Stock Policy", Description = "Melihat data drug stock policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStockPolicy", "Read")]
        public async Task<IActionResult> GetDrugStockPolicies(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? drugId,
            [FromQuery] Guid? storageLocationId,
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

            if (storageLocationId.HasValue && storageLocationId.Value != Guid.Empty)
                query = query.Where(x => x.StorageLocationId == storageLocationId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugStockPolicyResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    GenericName = x.Drug != null ? x.Drug.GenericName : null,
                    BrandName = x.Drug != null ? x.Drug.BrandName : null,
                    StorageLocationId = x.StorageLocationId,
                    StorageLocationCode = x.StorageLocation != null ? x.StorageLocation.StorageLocationCode : null,
                    StorageLocationName = x.StorageLocation != null ? x.StorageLocation.StorageLocationName : null,
                    StorageLocationType = x.StorageLocation != null ? x.StorageLocation.StorageLocationType : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    StockUnitMeasurementId = x.StockUnitMeasurementId,
                    StockUnitMeasurementCode = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementCode : string.Empty,
                    StockUnitMeasurementName = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementName : string.Empty,
                    StockUnitMeasurementSymbol = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementSymbol : null,
                    StockPolicyCode = x.StockPolicyCode,
                    StockPolicyName = x.StockPolicyName,
                    MinimumStockQuantity = x.MinimumStockQuantity,
                    MaximumStockQuantity = x.MaximumStockQuantity,
                    ReorderPointQuantity = x.ReorderPointQuantity,
                    ReorderQuantity = x.ReorderQuantity,
                    SafetyStockQuantity = x.SafetyStockQuantity,
                    CriticalStockQuantity = x.CriticalStockQuantity,
                    LeadTimeDays = x.LeadTimeDays,
                    ExpiryWarningDays = x.ExpiryWarningDays,
                    NearExpiryWarningDays = x.NearExpiryWarningDays,
                    IsAutoReorderEnabled = x.IsAutoReorderEnabled,
                    IsAllowNegativeStock = x.IsAllowNegativeStock,
                    IsBatchRequired = x.IsBatchRequired,
                    IsExpiryDateRequired = x.IsExpiryDateRequired,
                    IsStockOpnameRequired = x.IsStockOpnameRequired,
                    StockOpnameIntervalDays = x.StockOpnameIntervalDays,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    SortOrder = x.SortOrder,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy
                })
                .ToListAsync();

            var result = new ResponseDrugStockPolicyPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDrugStockPolicyPagedResult>.Ok(
                result,
                "Data drug stock policy berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DrugStockPolicyOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Stock Policy", Description = "Melihat data pilihan drug stock policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStockPolicy", "Read")]
        public async Task<IActionResult> GetDrugStockPolicyOptions(
    [FromQuery] Guid? drugId,
    [FromQuery] Guid? storageLocationId,
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

            if (storageLocationId.HasValue && storageLocationId.Value != Guid.Empty)
                query = query.Where(x => x.StorageLocationId == storageLocationId.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.StockPolicyName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugStockPolicyOptionResponse
                {
                    Id = x.Id,

                    DrugId = x.DrugId,
                    DrugName = x.Drug != null
                        ? x.Drug.DrugName
                        : string.Empty,

                    StorageLocationId = x.StorageLocationId,
                    StorageLocationName = x.StorageLocation != null
                        ? x.StorageLocation.StorageLocationName
                        : null,

                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitName = x.ServiceUnit != null
                        ? x.ServiceUnit.ServiceUnitName
                        : null,

                    ClinicId = x.ClinicId,
                    ClinicName = x.Clinic != null
                        ? x.Clinic.ClinicName
                        : null,

                    StockUnitMeasurementId = x.StockUnitMeasurementId,
                    StockUnitMeasurementName = x.StockUnitMeasurement != null
                        ? x.StockUnitMeasurement.MeasurementName
                        : string.Empty,
                    StockUnitMeasurementSymbol = x.StockUnitMeasurement != null
                        ? x.StockUnitMeasurement.MeasurementSymbol
                        : null,

                    StockPolicyCode = x.StockPolicyCode,
                    StockPolicyName = x.StockPolicyName,

                    MinimumStockQuantity = x.MinimumStockQuantity,
                    MaximumStockQuantity = x.MaximumStockQuantity,
                    ReorderPointQuantity = x.ReorderPointQuantity,
                    ReorderQuantity = x.ReorderQuantity,
                    SafetyStockQuantity = x.SafetyStockQuantity,
                    CriticalStockQuantity = x.CriticalStockQuantity,

                    IsAutoReorderEnabled = x.IsAutoReorderEnabled,
                    IsAllowNegativeStock = x.IsAllowNegativeStock,
                    IsBatchRequired = x.IsBatchRequired,
                    IsExpiryDateRequired = x.IsExpiryDateRequired,
                    IsStockOpnameRequired = x.IsStockOpnameRequired
                })
                .ToListAsync();

            var result = new DrugStockPolicyOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DrugStockPolicyOptionPagedResponse>.Ok(
                result,
                "Data pilihan drug stock policy berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugStockPolicyDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug Stock Policy", Description = "Melihat detail drug stock policy", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugStockPolicy", "Read")]
        public async Task<IActionResult> GetDrugStockPolicyById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugStockPolicyDetailResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    GenericName = x.Drug != null ? x.Drug.GenericName : null,
                    BrandName = x.Drug != null ? x.Drug.BrandName : null,
                    StorageLocationId = x.StorageLocationId,
                    StorageLocationCode = x.StorageLocation != null ? x.StorageLocation.StorageLocationCode : null,
                    StorageLocationName = x.StorageLocation != null ? x.StorageLocation.StorageLocationName : null,
                    StorageLocationType = x.StorageLocation != null ? x.StorageLocation.StorageLocationType : null,
                    ServiceUnitId = x.ServiceUnitId,
                    ServiceUnitCode = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitCode : null,
                    ServiceUnitName = x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : null,
                    ClinicId = x.ClinicId,
                    ClinicCode = x.Clinic != null ? x.Clinic.ClinicCode : null,
                    ClinicName = x.Clinic != null ? x.Clinic.ClinicName : null,
                    StockUnitMeasurementId = x.StockUnitMeasurementId,
                    StockUnitMeasurementCode = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementCode : string.Empty,
                    StockUnitMeasurementName = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementName : string.Empty,
                    StockUnitMeasurementSymbol = x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementSymbol : null,
                    StockPolicyCode = x.StockPolicyCode,
                    StockPolicyName = x.StockPolicyName,
                    MinimumStockQuantity = x.MinimumStockQuantity,
                    MaximumStockQuantity = x.MaximumStockQuantity,
                    ReorderPointQuantity = x.ReorderPointQuantity,
                    ReorderQuantity = x.ReorderQuantity,
                    SafetyStockQuantity = x.SafetyStockQuantity,
                    CriticalStockQuantity = x.CriticalStockQuantity,
                    LeadTimeDays = x.LeadTimeDays,
                    ExpiryWarningDays = x.ExpiryWarningDays,
                    NearExpiryWarningDays = x.NearExpiryWarningDays,
                    IsAutoReorderEnabled = x.IsAutoReorderEnabled,
                    IsAllowNegativeStock = x.IsAllowNegativeStock,
                    IsBatchRequired = x.IsBatchRequired,
                    IsExpiryDateRequired = x.IsExpiryDateRequired,
                    IsStockOpnameRequired = x.IsStockOpnameRequired,
                    StockOpnameIntervalDays = x.StockOpnameIntervalDays,
                    EffectiveStartDate = x.EffectiveStartDate,
                    EffectiveEndDate = x.EffectiveEndDate,
                    SortOrder = x.SortOrder,
                    Description = x.Description,
                    IsActive = x.IsActive,
                    CreateDateTime = x.CreateDateTime,
                    CreateBy = x.CreateBy == Guid.Empty ? null : (Guid?)x.CreateBy
                })
                .FirstOrDefaultAsync();

            if (data == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug stock policy tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DrugStockPolicyDetailResponse>.Ok(
                data,
                "Detail drug stock policy berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DrugStockPolicyCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Drug Stock Policy", Description = "Membuat data drug stock policy", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DrugStockPolicy", "Create")]
        public async Task<IActionResult> CreateDrugStockPolicy([FromBody] CreateDrugStockPolicyRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                drugId: request.DrugId,
                storageLocationId: request.StorageLocationId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                stockUnitMeasurementId: request.StockUnitMeasurementId,
                stockPolicyName: request.StockPolicyName,
                minimumStockQuantity: request.MinimumStockQuantity,
                maximumStockQuantity: request.MaximumStockQuantity,
                reorderPointQuantity: request.ReorderPointQuantity,
                reorderQuantity: request.ReorderQuantity,
                safetyStockQuantity: request.SafetyStockQuantity,
                criticalStockQuantity: request.CriticalStockQuantity,
                leadTimeDays: request.LeadTimeDays,
                expiryWarningDays: request.ExpiryWarningDays,
                nearExpiryWarningDays: request.NearExpiryWarningDays,
                isAutoReorderEnabled: request.IsAutoReorderEnabled,
                isBatchRequired: request.IsBatchRequired,
                isExpiryDateRequired: request.IsExpiryDateRequired,
                isStockOpnameRequired: request.IsStockOpnameRequired,
                stockOpnameIntervalDays: request.StockOpnameIntervalDays,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug stock policy tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            var entity = new MstDrugStockPolicy
            {
                Id = Guid.NewGuid(),
                DrugId = request.DrugId,
                StorageLocationId = NormalizeNullableGuid(request.StorageLocationId),
                ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId),
                ClinicId = NormalizeNullableGuid(request.ClinicId),
                StockUnitMeasurementId = request.StockUnitMeasurementId,
                StockPolicyCode = await GenerateStockPolicyCodeAsync(),
                StockPolicyName = request.StockPolicyName.Trim(),
                MinimumStockQuantity = request.MinimumStockQuantity,
                MaximumStockQuantity = request.MaximumStockQuantity,
                ReorderPointQuantity = request.ReorderPointQuantity,
                ReorderQuantity = request.ReorderQuantity,
                SafetyStockQuantity = request.SafetyStockQuantity,
                CriticalStockQuantity = request.CriticalStockQuantity,
                LeadTimeDays = request.LeadTimeDays,
                ExpiryWarningDays = request.ExpiryWarningDays,
                NearExpiryWarningDays = request.NearExpiryWarningDays,
                IsAutoReorderEnabled = request.IsAutoReorderEnabled,
                IsAllowNegativeStock = request.IsAllowNegativeStock,
                IsBatchRequired = request.IsBatchRequired,
                IsExpiryDateRequired = request.IsExpiryDateRequired,
                IsStockOpnameRequired = request.IsStockOpnameRequired,
                StockOpnameIntervalDays = request.StockOpnameIntervalDays,
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

            _dbContext.Set<MstDrugStockPolicy>().Add(entity);
            await _dbContext.SaveChangesAsync();

            var response = await BuildCreateResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugStockPolicy.CreateDrugStockPolicy",
                "Membuat data drug stock policy.",
                response
            );

            return Ok(ApiResponse<DrugStockPolicyCreateResponse>.Ok(
                response,
                "Drug stock policy berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Stock Policy", Description = "Mengubah data drug stock policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugStockPolicy", "Update")]
        public async Task<IActionResult> UpdateDrugStockPolicy(Guid id, [FromBody] UpdateDrugStockPolicyRequest request)
        {
            var entity = await _dbContext.Set<MstDrugStockPolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug stock policy tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                drugId: request.DrugId,
                storageLocationId: request.StorageLocationId,
                serviceUnitId: request.ServiceUnitId,
                clinicId: request.ClinicId,
                stockUnitMeasurementId: request.StockUnitMeasurementId,
                stockPolicyName: request.StockPolicyName,
                minimumStockQuantity: request.MinimumStockQuantity,
                maximumStockQuantity: request.MaximumStockQuantity,
                reorderPointQuantity: request.ReorderPointQuantity,
                reorderQuantity: request.ReorderQuantity,
                safetyStockQuantity: request.SafetyStockQuantity,
                criticalStockQuantity: request.CriticalStockQuantity,
                leadTimeDays: request.LeadTimeDays,
                expiryWarningDays: request.ExpiryWarningDays,
                nearExpiryWarningDays: request.NearExpiryWarningDays,
                isAutoReorderEnabled: request.IsAutoReorderEnabled,
                isBatchRequired: request.IsBatchRequired,
                isExpiryDateRequired: request.IsExpiryDateRequired,
                isStockOpnameRequired: request.IsStockOpnameRequired,
                stockOpnameIntervalDays: request.StockOpnameIntervalDays,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug stock policy tidak valid."
                ));
            }

            entity.DrugId = request.DrugId;
            entity.StorageLocationId = NormalizeNullableGuid(request.StorageLocationId);
            entity.ServiceUnitId = NormalizeNullableGuid(request.ServiceUnitId);
            entity.ClinicId = NormalizeNullableGuid(request.ClinicId);
            entity.StockUnitMeasurementId = request.StockUnitMeasurementId;
            entity.StockPolicyName = request.StockPolicyName.Trim();
            entity.MinimumStockQuantity = request.MinimumStockQuantity;
            entity.MaximumStockQuantity = request.MaximumStockQuantity;
            entity.ReorderPointQuantity = request.ReorderPointQuantity;
            entity.ReorderQuantity = request.ReorderQuantity;
            entity.SafetyStockQuantity = request.SafetyStockQuantity;
            entity.CriticalStockQuantity = request.CriticalStockQuantity;
            entity.LeadTimeDays = request.LeadTimeDays;
            entity.ExpiryWarningDays = request.ExpiryWarningDays;
            entity.NearExpiryWarningDays = request.NearExpiryWarningDays;
            entity.IsAutoReorderEnabled = request.IsAutoReorderEnabled;
            entity.IsAllowNegativeStock = request.IsAllowNegativeStock;
            entity.IsBatchRequired = request.IsBatchRequired;
            entity.IsExpiryDateRequired = request.IsExpiryDateRequired;
            entity.IsStockOpnameRequired = request.IsStockOpnameRequired;
            entity.StockOpnameIntervalDays = request.StockOpnameIntervalDays;
            entity.EffectiveStartDate = request.EffectiveStartDate;
            entity.EffectiveEndDate = request.EffectiveEndDate;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugStockPolicy.UpdateDrugStockPolicy",
                "Mengubah data drug stock policy.",
                new { Id = entity.Id, entity.StockPolicyCode, entity.StockPolicyName, entity.IsActive }
            );

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug stock policy berhasil diperbarui."
            ));
        }


        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Stock Policy Status", Description = "Mengubah status drug stock policy", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugStockPolicy", "Update")]
        public async Task<IActionResult> UpdateDrugStockPolicyStatus(
            Guid id,
            [FromBody] UpdateDrugStockPolicyStatusRequest request)
        {
            var entity = await _dbContext.Set<MstDrugStockPolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug stock policy tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            await _dbContext.SaveChangesAsync();

            return Ok(ApiResponse<object>.Ok(
                null,
                "Status drug stock policy berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug Stock Policy", Description = "Menghapus data drug stock policy", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DrugStockPolicy", "Delete")]
        public async Task<IActionResult> DeleteDrugStockPolicy(Guid id, [FromBody] DeleteDrugStockPolicyRequest? request = null)
        {
            var entity = await _dbContext.Set<MstDrugStockPolicy>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug stock policy tidak ditemukan."
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

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug stock policy berhasil dihapus."
            ));
        }

        private IQueryable<MstDrugStockPolicy> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrugStockPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrugStockPolicy> ApplySearch(
            IQueryable<MstDrugStockPolicy> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var keyword = search.Trim().ToLower();

            return query.Where(x =>
                x.StockPolicyCode.ToLower().Contains(keyword) ||
                x.StockPolicyName.ToLower().Contains(keyword) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.GenericName != null && x.Drug.GenericName.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.BrandName != null && x.Drug.BrandName.ToLower().Contains(keyword)) ||
                (x.StorageLocation != null && x.StorageLocation.StorageLocationCode.ToLower().Contains(keyword)) ||
                (x.StorageLocation != null && x.StorageLocation.StorageLocationName.ToLower().Contains(keyword)) ||
                (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitCode.ToLower().Contains(keyword)) ||
                (x.ServiceUnit != null && x.ServiceUnit.ServiceUnitName.ToLower().Contains(keyword)) ||
                (x.Clinic != null && x.Clinic.ClinicCode.ToLower().Contains(keyword)) ||
                (x.Clinic != null && x.Clinic.ClinicName.ToLower().Contains(keyword)) ||
                (x.StockUnitMeasurement != null && x.StockUnitMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                (x.StockUnitMeasurement != null && x.StockUnitMeasurement.MeasurementName.ToLower().Contains(keyword)));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid drugId,
            Guid? storageLocationId,
            Guid? serviceUnitId,
            Guid? clinicId,
            Guid stockUnitMeasurementId,
            string stockPolicyName,
            decimal minimumStockQuantity,
            decimal maximumStockQuantity,
            decimal reorderPointQuantity,
            decimal reorderQuantity,
            decimal safetyStockQuantity,
            decimal criticalStockQuantity,
            int leadTimeDays,
            int expiryWarningDays,
            int nearExpiryWarningDays,
            bool isAutoReorderEnabled,
            bool isBatchRequired,
            bool isExpiryDateRequired,
            bool isStockOpnameRequired,
            int stockOpnameIntervalDays,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (drugId == Guid.Empty)
                return (false, "Drug wajib dipilih.");

            if (stockUnitMeasurementId == Guid.Empty)
                return (false, "Satuan stok wajib dipilih.");

            if (string.IsNullOrWhiteSpace(stockPolicyName))
                return (false, "Nama stock policy wajib diisi.");

            if (minimumStockQuantity < 0)
                return (false, "Minimum stock quantity tidak boleh kurang dari 0.");

            if (maximumStockQuantity < 0)
                return (false, "Maximum stock quantity tidak boleh kurang dari 0.");

            if (reorderPointQuantity < 0)
                return (false, "Reorder point quantity tidak boleh kurang dari 0.");

            if (reorderQuantity < 0)
                return (false, "Reorder quantity tidak boleh kurang dari 0.");

            if (safetyStockQuantity < 0)
                return (false, "Safety stock quantity tidak boleh kurang dari 0.");

            if (criticalStockQuantity < 0)
                return (false, "Critical stock quantity tidak boleh kurang dari 0.");

            if (maximumStockQuantity > 0 && maximumStockQuantity < minimumStockQuantity)
                return (false, "Maximum stock quantity tidak boleh lebih kecil dari minimum stock quantity.");

            if (maximumStockQuantity > 0 && reorderPointQuantity > maximumStockQuantity)
                return (false, "Reorder point quantity tidak boleh lebih besar dari maximum stock quantity.");

            if (minimumStockQuantity > 0 && criticalStockQuantity > minimumStockQuantity)
                return (false, "Critical stock quantity tidak boleh lebih besar dari minimum stock quantity.");

            if (maximumStockQuantity > 0 && safetyStockQuantity > maximumStockQuantity)
                return (false, "Safety stock quantity tidak boleh lebih besar dari maximum stock quantity.");

            if (isAutoReorderEnabled && reorderPointQuantity <= 0)
                return (false, "Reorder point quantity wajib lebih dari 0 jika auto reorder aktif.");

            if (isAutoReorderEnabled && reorderQuantity <= 0)
                return (false, "Reorder quantity wajib lebih dari 0 jika auto reorder aktif.");

            if (leadTimeDays < 0)
                return (false, "Lead time days tidak boleh kurang dari 0.");

            if (expiryWarningDays < 0)
                return (false, "Expiry warning days tidak boleh kurang dari 0.");

            if (nearExpiryWarningDays < 0)
                return (false, "Near expiry warning days tidak boleh kurang dari 0.");

            if (expiryWarningDays < nearExpiryWarningDays)
                return (false, "Expiry warning days tidak boleh lebih kecil dari near expiry warning days.");

            if (isStockOpnameRequired && stockOpnameIntervalDays <= 0)
                return (false, "Stock opname interval days wajib lebih dari 0 jika stock opname required.");

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate.Value < effectiveStartDate.Value)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == drugId && x.IsActive && !x.IsDelete);

            if (drug == null)
                return (false, "Drug tidak valid atau tidak aktif.");

            if (drug.IsBatchTracked && !isBatchRequired)
                return (false, "Drug ini batch tracked, sehingga batch required tidak boleh dinonaktifkan.");

            if (drug.IsExpiryDateTracked && !isExpiryDateRequired)
                return (false, "Drug ini expiry date tracked, sehingga expiry date required tidak boleh dinonaktifkan.");

            var stockUnitExists = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == stockUnitMeasurementId && x.IsActive && !x.IsDelete);

            if (!stockUnitExists)
                return (false, "Satuan stok tidak valid atau tidak aktif.");

            var normalizedStorageLocationId = NormalizeNullableGuid(storageLocationId);
            var normalizedServiceUnitId = NormalizeNullableGuid(serviceUnitId);
            var normalizedClinicId = NormalizeNullableGuid(clinicId);

            MstDrugStorageLocation? storageLocation = null;

            if (normalizedStorageLocationId.HasValue)
            {
                storageLocation = await _dbContext.Set<MstDrugStorageLocation>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == normalizedStorageLocationId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (storageLocation == null)
                    return (false, "Storage location tidak valid atau tidak aktif.");
            }

            if (normalizedServiceUnitId.HasValue)
            {
                var serviceUnitExists = await _dbContext.Set<MstServiceUnit>()
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.Id == normalizedServiceUnitId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (!serviceUnitExists)
                    return (false, "Service unit tidak valid atau tidak aktif.");
            }

            if (normalizedClinicId.HasValue)
            {
                var clinic = await _dbContext.Set<MstClinic>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.Id == normalizedClinicId.Value &&
                        x.IsActive &&
                        !x.IsDelete);

                if (clinic == null)
                    return (false, "Clinic tidak valid atau tidak aktif.");

                if (normalizedServiceUnitId.HasValue && clinic.ServiceUnitId != normalizedServiceUnitId.Value)
                    return (false, "Clinic tidak sesuai dengan service unit yang dipilih.");
            }

            if (storageLocation != null)
            {
                if (normalizedServiceUnitId.HasValue &&
                    storageLocation.ServiceUnitId.HasValue &&
                    storageLocation.ServiceUnitId.Value != normalizedServiceUnitId.Value)
                {
                    return (false, "Storage location tidak sesuai dengan service unit yang dipilih.");
                }

                if (normalizedClinicId.HasValue &&
                    storageLocation.ClinicId.HasValue &&
                    storageLocation.ClinicId.Value != normalizedClinicId.Value)
                {
                    return (false, "Storage location tidak sesuai dengan clinic yang dipilih.");
                }
            }

            var normalizedName = stockPolicyName.Trim().ToLower();

            var duplicateScope = await _dbContext.Set<MstDrugStockPolicy>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.StorageLocationId == normalizedStorageLocationId &&
                    x.ServiceUnitId == normalizedServiceUnitId &&
                    x.ClinicId == normalizedClinicId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateScope)
                return (false, "Stock policy untuk drug, storage location, service unit, dan clinic tersebut sudah ada.");

            var duplicateName = await _dbContext.Set<MstDrugStockPolicy>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.StockPolicyName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama stock policy sudah digunakan.");

            return (true, null);
        }

        private async Task<DrugStockPolicyCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugStockPolicyCreateResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    StorageLocationId = x.StorageLocationId,
                    StorageLocationName = x.StorageLocation != null ? x.StorageLocation.StorageLocationName : null,
                    StockPolicyCode = x.StockPolicyCode,
                    StockPolicyName = x.StockPolicyName,
                    IsAutoReorderEnabled = x.IsAutoReorderEnabled,
                    IsActive = x.IsActive
                })
                .FirstOrDefaultAsync();

            return data ?? new DrugStockPolicyCreateResponse();
        }

        private async Task<string> GenerateStockPolicyCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDrugStockPolicy>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.StockPolicyCode.StartsWith(StockPolicyCodePrefix))
                .Select(x => x.StockPolicyCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(TryExtractStockPolicySequenceNumber)
                .Where(x => x.HasValue)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return $"{StockPolicyCodePrefix}{nextNumber.ToString().PadLeft(StockPolicyCodeDigitLength, '0')}";
        }

        private static int? TryExtractStockPolicySequenceNumber(string stockPolicyCode)
        {
            if (string.IsNullOrWhiteSpace(stockPolicyCode))
                return null;

            if (!stockPolicyCode.StartsWith(StockPolicyCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = stockPolicyCode[StockPolicyCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private static IQueryable<MstDrugStockPolicy> ApplySorting(
            IQueryable<MstDrugStockPolicy> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").Trim().ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "stockpolicycode" => isDesc
                    ? query.OrderByDescending(x => x.StockPolicyCode)
                    : query.OrderBy(x => x.StockPolicyCode),

                "stockpolicyname" => isDesc
                    ? query.OrderByDescending(x => x.StockPolicyName)
                    : query.OrderBy(x => x.StockPolicyName),

                "drugname" => isDesc
                    ? query.OrderByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                    : query.OrderBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty),

                "storagelocationname" => isDesc
                    ? query.OrderByDescending(x => x.StorageLocation != null ? x.StorageLocation.StorageLocationName : string.Empty)
                    : query.OrderBy(x => x.StorageLocation != null ? x.StorageLocation.StorageLocationName : string.Empty),

                "serviceunitname" => isDesc
                    ? query.OrderByDescending(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty)
                    : query.OrderBy(x => x.ServiceUnit != null ? x.ServiceUnit.ServiceUnitName : string.Empty),

                "clinicname" => isDesc
                    ? query.OrderByDescending(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty)
                    : query.OrderBy(x => x.Clinic != null ? x.Clinic.ClinicName : string.Empty),

                "stockunitmeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.StockUnitMeasurement != null ? x.StockUnitMeasurement.MeasurementName : string.Empty),

                "minimumstockquantity" => isDesc
                    ? query.OrderByDescending(x => x.MinimumStockQuantity)
                    : query.OrderBy(x => x.MinimumStockQuantity),

                "maximumstockquantity" => isDesc
                    ? query.OrderByDescending(x => x.MaximumStockQuantity)
                    : query.OrderBy(x => x.MaximumStockQuantity),

                "reorderpointquantity" => isDesc
                    ? query.OrderByDescending(x => x.ReorderPointQuantity)
                    : query.OrderBy(x => x.ReorderPointQuantity),

                "isautoreorderenabled" => isDesc
                    ? query.OrderByDescending(x => x.IsAutoReorderEnabled)
                    : query.OrderBy(x => x.IsAutoReorderEnabled),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.StockPolicyName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.StockPolicyName)
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
                    "today" => DateRangeResult.Valid(today, today.AddDays(1)),
                    "last7days" => DateRangeResult.Valid(today.AddDays(-6), today.AddDays(1)),
                    "thismonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1), new DateTime(today.Year, today.Month, 1).AddMonths(1)),
                    "lastmonth" => DateRangeResult.Valid(new DateTime(today.Year, today.Month, 1).AddMonths(-1), new DateTime(today.Year, today.Month, 1)),
                    "all" => DateRangeResult.Valid(null, null),
                    _ => DateRangeResult.Invalid("Custom period tidak dikenali.")
                };
            }

            var start = startDate?.Date;
            var endExclusive = endDate?.Date.AddDays(1);

            if (start.HasValue && endExclusive.HasValue && start.Value >= endExclusive.Value)
                return DateRangeResult.Invalid("StartDate tidak boleh lebih besar dari EndDate.");

            return DateRangeResult.Valid(start, endExclusive);
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

        private class DateRangeResult
        {
            public bool IsValid { get; private set; }
            public DateTime? Start { get; private set; }
            public DateTime? EndExclusive { get; private set; }
            public string? ErrorMessage { get; private set; }

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
