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

using ResponseDrugUnitConversionPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.DrugUnitConversionResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/drug-unit-conversions")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Drug Unit Conversion",
        AreaName = "HealthServices",
        ControllerName = "DrugUnitConversion",
        Description = "Health service master data drug unit conversion",
        SortOrder = 12
    )]
    [Tags("Health Services / Master Data / Drug Unit Conversion")]
    public class DrugUnitConversionController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string CodePrefix = "DUC-RSMMC-";
        private const int CodeNumberLength = 5;

        private static readonly HashSet<string> AllowedConversionTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "PurchaseToStock",
            "StockToDispense",
            "DispenseToBase",
            "StrengthToBase",
            "DoseToBase",
            "Compound"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public DrugUnitConversionController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<DrugUnitConversionFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Unit Conversion", Description = "Melihat data drug unit conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugUnitConversion", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new DrugUnitConversionFilterMetadataResponse
            {
                DefaultFilter = new DrugUnitConversionDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<DrugUnitConversionSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "conversionCode", Label = "Kode konversi" },
                    new() { Value = "conversionName", Label = "Nama konversi" },
                    new() { Value = "drugName", Label = "Nama obat" },
                    new() { Value = "fromMeasurementName", Label = "Dari satuan" },
                    new() { Value = "toMeasurementName", Label = "Ke satuan" },
                    new() { Value = "conversionFactor", Label = "Faktor konversi" },
                    new() { Value = "conversionType", Label = "Tipe konversi" },
                    new() { Value = "isDefault", Label = "Default" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                ConversionTypeOptions = AllowedConversionTypes.OrderBy(x => x).ToList()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugUnitConversion.GetFilterMetadata",
                "Mengambil metadata filter drug unit conversion.",
                result
            );

            return Ok(ApiResponse<DrugUnitConversionFilterMetadataResponse>.Ok(
                result,
                "Metadata filter drug unit conversion berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<DrugUnitConversionSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Unit Conversion", Description = "Melihat data drug unit conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugUnitConversion", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var today = DateTime.UtcNow.Date;

            var query = _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);

            var result = new DrugUnitConversionSummaryResponse
            {
                TotalDrugUnitConversion = await query.CountAsync(),
                ActiveDrugUnitConversion = await query.CountAsync(x => x.IsActive),
                InactiveDrugUnitConversion = await query.CountAsync(x => !x.IsActive),
                DefaultConversion = await query.CountAsync(x => x.IsDefault),
                BidirectionalConversion = await query.CountAsync(x => x.IsBidirectional),
                PurchaseConversion = await query.CountAsync(x => x.IsForPurchase),
                StockConversion = await query.CountAsync(x => x.IsForStock),
                DispensingConversion = await query.CountAsync(x => x.IsForDispensing),
                PrescriptionConversion = await query.CountAsync(x => x.IsForPrescription),
                CompoundConversion = await query.CountAsync(x => x.IsForCompound),
                EffectiveConversion = await query.CountAsync(x =>
                    (!x.EffectiveStartDate.HasValue || x.EffectiveStartDate.Value.Date <= today) &&
                    (!x.EffectiveEndDate.HasValue || x.EffectiveEndDate.Value.Date >= today)),
                ExpiredConversion = await query.CountAsync(x =>
                    x.EffectiveEndDate.HasValue && x.EffectiveEndDate.Value.Date < today)
            };

            return Ok(ApiResponse<DrugUnitConversionSummaryResponse>.Ok(
                result,
                "Ringkasan drug unit conversion berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseDrugUnitConversionPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Unit Conversion", Description = "Melihat data drug unit conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugUnitConversion", "Read")]
        public async Task<IActionResult> GetDrugUnitConversions(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] Guid? drugId,
            [FromQuery] Guid? fromMeasurementId,
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

            if (fromMeasurementId.HasValue && fromMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.FromMeasurementId == fromMeasurementId.Value);

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => ToResponse(x))
                .ToListAsync();

            var result = new ResponseDrugUnitConversionPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseDrugUnitConversionPagedResult>.Ok(
                result,
                "Data drug unit conversion berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<DrugUnitConversionOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Drug Unit Conversion", Description = "Melihat data pilihan drug unit conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugUnitConversion", "Read")]
        public async Task<IActionResult> GetDrugUnitConversionOptions(
    [FromQuery] Guid? drugId,
    [FromQuery] Guid? fromMeasurementId,
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

            if (fromMeasurementId.HasValue && fromMeasurementId.Value != Guid.Empty)
                query = query.Where(x => x.FromMeasurementId == fromMeasurementId.Value);

            query = ApplySearch(query, search);

            var totalData = await query.CountAsync();

            var items = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                .ThenBy(x => x.ConversionName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new DrugUnitConversionOptionResponse
                {
                    Id = x.Id,

                    DrugId = x.DrugId,
                    DrugName = x.Drug != null
                        ? x.Drug.DrugName
                        : string.Empty,

                    ConversionCode = x.ConversionCode,
                    ConversionName = x.ConversionName,

                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementName = x.FromMeasurement != null
                        ? x.FromMeasurement.MeasurementName
                        : string.Empty,
                    FromMeasurementSymbol = x.FromMeasurement != null
                        ? x.FromMeasurement.MeasurementSymbol
                        : null,

                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementName = x.ToMeasurement != null
                        ? x.ToMeasurement.MeasurementName
                        : string.Empty,
                    ToMeasurementSymbol = x.ToMeasurement != null
                        ? x.ToMeasurement.MeasurementSymbol
                        : null,

                    FromQuantity = x.FromQuantity,
                    ToQuantity = x.ToQuantity,
                    ConversionFactor = x.ConversionFactor,
                    ConversionType = x.ConversionType,

                    IsDefault = x.IsDefault,
                    IsBidirectional = x.IsBidirectional,
                    IsForPurchase = x.IsForPurchase,
                    IsForStock = x.IsForStock,
                    IsForDispensing = x.IsForDispensing,
                    IsForPrescription = x.IsForPrescription,
                    IsForCompound = x.IsForCompound
                })
                .ToListAsync();

            var result = new DrugUnitConversionOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<DrugUnitConversionOptionPagedResponse>.Ok(
                result,
                "Data pilihan drug unit conversion berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugUnitConversionDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Drug Unit Conversion", Description = "Melihat detail drug unit conversion", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("DrugUnitConversion", "Read")]
        public async Task<IActionResult> GetDrugUnitConversionById(Guid id)
        {
            var data = await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugUnitConversionDetailResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    ConversionCode = x.ConversionCode,
                    ConversionName = x.ConversionName,
                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementCode = x.FromMeasurement != null ? x.FromMeasurement.MeasurementCode : string.Empty,
                    FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                    FromMeasurementSymbol = x.FromMeasurement != null ? x.FromMeasurement.MeasurementSymbol : null,
                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementCode = x.ToMeasurement != null ? x.ToMeasurement.MeasurementCode : string.Empty,
                    ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                    ToMeasurementSymbol = x.ToMeasurement != null ? x.ToMeasurement.MeasurementSymbol : null,
                    FromQuantity = x.FromQuantity,
                    ToQuantity = x.ToQuantity,
                    ConversionFactor = x.ConversionFactor,
                    ConversionType = x.ConversionType,
                    IsDefault = x.IsDefault,
                    IsBidirectional = x.IsBidirectional,
                    IsForPurchase = x.IsForPurchase,
                    IsForStock = x.IsForStock,
                    IsForDispensing = x.IsForDispensing,
                    IsForPrescription = x.IsForPrescription,
                    IsForCompound = x.IsForCompound,
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
                    "Drug unit conversion tidak ditemukan."
                ));
            }

            return Ok(ApiResponse<DrugUnitConversionDetailResponse>.Ok(
                data,
                "Detail drug unit conversion berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<DrugUnitConversionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Drug Unit Conversion", Description = "Membuat data drug unit conversion", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("DrugUnitConversion", "Create")]
        public async Task<IActionResult> CreateDrugUnitConversion([FromBody] CreateDrugUnitConversionRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                drugId: request.DrugId,
                conversionName: request.ConversionName,
                fromMeasurementId: request.FromMeasurementId,
                toMeasurementId: request.ToMeasurementId,
                fromQuantity: request.FromQuantity,
                toQuantity: request.ToQuantity,
                conversionFactor: request.ConversionFactor,
                conversionType: request.ConversionType,
                isBidirectional: request.IsBidirectional,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug unit conversion tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedConversionType = NormalizeConversionType(request.ConversionType);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsDefault)
            {
                await ResetDefaultConversionAsync(
                    drugId: request.DrugId,
                    conversionType: normalizedConversionType,
                    excludeId: null,
                    actorUserId: actorUserId,
                    now: now
                );
            }

            var entity = new MstDrugUnitConversion
            {
                Id = Guid.NewGuid(),
                DrugId = request.DrugId,
                ConversionCode = await GenerateConversionCodeAsync(),
                ConversionName = request.ConversionName.Trim(),
                FromMeasurementId = request.FromMeasurementId,
                ToMeasurementId = request.ToMeasurementId,
                FromQuantity = request.FromQuantity,
                ToQuantity = request.ToQuantity,
                ConversionFactor = request.ConversionFactor,
                ConversionType = normalizedConversionType,
                IsDefault = request.IsDefault,
                IsBidirectional = request.IsBidirectional,
                IsForPurchase = request.IsForPurchase,
                IsForStock = request.IsForStock,
                IsForDispensing = request.IsForDispensing,
                IsForPrescription = request.IsForPrescription,
                IsForCompound = request.IsForCompound,
                EffectiveStartDate = request.EffectiveStartDate?.Date,
                EffectiveEndDate = request.EffectiveEndDate?.Date,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstDrugUnitConversion>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = await BuildCreateResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugUnitConversion.CreateDrugUnitConversion",
                "Membuat data drug unit conversion.",
                response
            );

            return Ok(ApiResponse<DrugUnitConversionCreateResponse>.Ok(
                response,
                "Drug unit conversion berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<DrugUnitConversionCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Drug Unit Conversion", Description = "Mengubah data drug unit conversion", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("DrugUnitConversion", "Update")]
        public async Task<IActionResult> UpdateDrugUnitConversion(Guid id, [FromBody] UpdateDrugUnitConversionRequest request)
        {
            var entity = await _dbContext.Set<MstDrugUnitConversion>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug unit conversion tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                drugId: request.DrugId,
                conversionName: request.ConversionName,
                fromMeasurementId: request.FromMeasurementId,
                toMeasurementId: request.ToMeasurementId,
                fromQuantity: request.FromQuantity,
                toQuantity: request.ToQuantity,
                conversionFactor: request.ConversionFactor,
                conversionType: request.ConversionType,
                isBidirectional: request.IsBidirectional,
                effectiveStartDate: request.EffectiveStartDate,
                effectiveEndDate: request.EffectiveEndDate
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data drug unit conversion tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            var normalizedConversionType = NormalizeConversionType(request.ConversionType);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            if (request.IsDefault)
            {
                await ResetDefaultConversionAsync(
                    drugId: request.DrugId,
                    conversionType: normalizedConversionType,
                    excludeId: id,
                    actorUserId: actorUserId,
                    now: now
                );
            }

            entity.DrugId = request.DrugId;
            entity.ConversionName = request.ConversionName.Trim();
            entity.FromMeasurementId = request.FromMeasurementId;
            entity.ToMeasurementId = request.ToMeasurementId;
            entity.FromQuantity = request.FromQuantity;
            entity.ToQuantity = request.ToQuantity;
            entity.ConversionFactor = request.ConversionFactor;
            entity.ConversionType = normalizedConversionType;
            entity.IsDefault = request.IsDefault;
            entity.IsBidirectional = request.IsBidirectional;
            entity.IsForPurchase = request.IsForPurchase;
            entity.IsForStock = request.IsForStock;
            entity.IsForDispensing = request.IsForDispensing;
            entity.IsForPrescription = request.IsForPrescription;
            entity.IsForCompound = request.IsForCompound;
            entity.EffectiveStartDate = request.EffectiveStartDate?.Date;
            entity.EffectiveEndDate = request.EffectiveEndDate?.Date;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = await BuildCreateResponseAsync(entity.Id);

            await _loggerService.InfoAsync(
                LogCategory,
                "DrugUnitConversion.UpdateDrugUnitConversion",
                "Mengubah data drug unit conversion.",
                response
            );

            return Ok(ApiResponse<DrugUnitConversionCreateResponse>.Ok(
                response,
                "Drug unit conversion berhasil diperbarui."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Drug Unit Conversion", Description = "Menghapus data drug unit conversion", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("DrugUnitConversion", "Delete")]
        public async Task<IActionResult> DeleteDrugUnitConversion(Guid id)
        {
            var entity = await _dbContext.Set<MstDrugUnitConversion>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Drug unit conversion tidak ditemukan."
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

            return Ok(ApiResponse<object>.Ok(
                null,
                "Drug unit conversion berhasil dihapus."
            ));
        }

        private IQueryable<MstDrugUnitConversion> BuildBaseQuery()
        {
            return _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstDrugUnitConversion> ApplySearch(
            IQueryable<MstDrugUnitConversion> query,
            string? search)
        {
            if (string.IsNullOrWhiteSpace(search))
                return query;

            var keyword = search.Trim().ToLower();

            return query.Where(x =>
                x.ConversionCode.ToLower().Contains(keyword) ||
                x.ConversionName.ToLower().Contains(keyword) ||
                x.ConversionType.ToLower().Contains(keyword) ||
                (x.Description != null && x.Description.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.DrugCode.ToLower().Contains(keyword)) ||
                (x.Drug != null && x.Drug.DrugName.ToLower().Contains(keyword)) ||
                (x.FromMeasurement != null && x.FromMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                (x.FromMeasurement != null && x.FromMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                (x.FromMeasurement != null && x.FromMeasurement.MeasurementSymbol != null && x.FromMeasurement.MeasurementSymbol.ToLower().Contains(keyword)) ||
                (x.ToMeasurement != null && x.ToMeasurement.MeasurementCode.ToLower().Contains(keyword)) ||
                (x.ToMeasurement != null && x.ToMeasurement.MeasurementName.ToLower().Contains(keyword)) ||
                (x.ToMeasurement != null && x.ToMeasurement.MeasurementSymbol != null && x.ToMeasurement.MeasurementSymbol.ToLower().Contains(keyword)));
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            Guid drugId,
            string conversionName,
            Guid fromMeasurementId,
            Guid toMeasurementId,
            decimal fromQuantity,
            decimal toQuantity,
            decimal conversionFactor,
            string conversionType,
            bool isBidirectional,
            DateTime? effectiveStartDate,
            DateTime? effectiveEndDate)
        {
            if (drugId == Guid.Empty)
                return (false, "Drug wajib dipilih.");

            if (string.IsNullOrWhiteSpace(conversionName))
                return (false, "Nama konversi wajib diisi.");

            if (fromMeasurementId == Guid.Empty)
                return (false, "From measurement wajib dipilih.");

            if (toMeasurementId == Guid.Empty)
                return (false, "To measurement wajib dipilih.");

            if (fromMeasurementId == toMeasurementId)
                return (false, "From measurement dan to measurement tidak boleh sama.");

            if (fromQuantity <= 0)
                return (false, "From quantity harus lebih dari 0.");

            if (toQuantity <= 0)
                return (false, "To quantity harus lebih dari 0.");

            if (conversionFactor <= 0)
                return (false, "Conversion factor harus lebih dari 0.");

            if (string.IsNullOrWhiteSpace(conversionType))
                return (false, "Tipe konversi wajib diisi.");

            if (!AllowedConversionTypes.Contains(conversionType.Trim()))
            {
                return (false, "Tipe konversi tidak valid. Gunakan salah satu: General, PurchaseToStock, StockToDispense, DispenseToBase, StrengthToBase, DoseToBase, Compound.");
            }

            if (effectiveStartDate.HasValue && effectiveEndDate.HasValue && effectiveEndDate.Value.Date < effectiveStartDate.Value.Date)
                return (false, "Tanggal akhir berlaku tidak boleh lebih kecil dari tanggal mulai berlaku.");

            var drugExists = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == drugId && x.IsActive && !x.IsDelete);

            if (!drugExists)
                return (false, "Drug tidak valid atau tidak aktif.");

            var fromMeasurementExists = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == fromMeasurementId && x.IsActive && !x.IsDelete);

            if (!fromMeasurementExists)
                return (false, "From measurement tidak valid atau tidak aktif.");

            var toMeasurementExists = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == toMeasurementId && x.IsActive && !x.IsDelete);

            if (!toMeasurementExists)
                return (false, "To measurement tidak valid atau tidak aktif.");

            var normalizedName = conversionName.Trim().ToLower();
            var normalizedConversionType = NormalizeConversionType(conversionType);

            var duplicateName = await _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.ConversionName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama konversi untuk drug tersebut sudah digunakan.");

            var duplicateExact = await _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.FromMeasurementId == fromMeasurementId &&
                    x.ToMeasurementId == toMeasurementId &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateExact)
                return (false, "Konversi unit untuk drug tersebut sudah ada.");

            var duplicateReverse = await _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.FromMeasurementId == toMeasurementId &&
                    x.ToMeasurementId == fromMeasurementId &&
                    (x.IsBidirectional || isBidirectional) &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateReverse)
                return (false, "Konversi unit kebalikan untuk drug tersebut sudah ada dan bersifat bidirectional.");

            return (true, null);
        }

        private async Task ResetDefaultConversionAsync(
            Guid drugId,
            string conversionType,
            Guid? excludeId,
            Guid actorUserId,
            DateTime now)
        {
            var query = _dbContext.Set<MstDrugUnitConversion>()
                .Where(x =>
                    !x.IsDelete &&
                    x.DrugId == drugId &&
                    x.ConversionType == conversionType &&
                    x.IsDefault);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            var defaultConversions = await query.ToListAsync();

            foreach (var item in defaultConversions)
            {
                item.IsDefault = false;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }
        }

        private async Task<string> GenerateConversionCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstDrugUnitConversion>()
                .AsNoTracking()
                .Where(x => !x.IsDelete && x.ConversionCode.StartsWith(CodePrefix))
                .Select(x => x.ConversionCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(code =>
                {
                    var numberText = code.Length > CodePrefix.Length
                        ? code[CodePrefix.Length..]
                        : string.Empty;

                    return int.TryParse(numberText, out var number)
                        ? number
                        : 0;
                })
                .Where(number => number > 0)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
            {
                nextNumber++;
            }

            return $"{CodePrefix}{nextNumber.ToString().PadLeft(CodeNumberLength, '0')}";
        }

        private async Task<DrugUnitConversionCreateResponse> BuildCreateResponseAsync(Guid id)
        {
            return await BuildBaseQuery()
                .Where(x => x.Id == id)
                .Select(x => new DrugUnitConversionCreateResponse
                {
                    Id = x.Id,
                    DrugId = x.DrugId,
                    DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                    ConversionCode = x.ConversionCode,
                    ConversionName = x.ConversionName,
                    FromMeasurementId = x.FromMeasurementId,
                    FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                    ToMeasurementId = x.ToMeasurementId,
                    ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                    ConversionFactor = x.ConversionFactor,
                    ConversionType = x.ConversionType,
                    IsDefault = x.IsDefault,
                    IsActive = x.IsActive
                })
                .FirstAsync();
        }

        private static DrugUnitConversionResponse ToResponse(MstDrugUnitConversion x)
        {
            return new DrugUnitConversionResponse
            {
                Id = x.Id,
                DrugId = x.DrugId,
                DrugCode = x.Drug != null ? x.Drug.DrugCode : string.Empty,
                DrugName = x.Drug != null ? x.Drug.DrugName : string.Empty,
                ConversionCode = x.ConversionCode,
                ConversionName = x.ConversionName,
                FromMeasurementId = x.FromMeasurementId,
                FromMeasurementCode = x.FromMeasurement != null ? x.FromMeasurement.MeasurementCode : string.Empty,
                FromMeasurementName = x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty,
                FromMeasurementSymbol = x.FromMeasurement != null ? x.FromMeasurement.MeasurementSymbol : null,
                ToMeasurementId = x.ToMeasurementId,
                ToMeasurementCode = x.ToMeasurement != null ? x.ToMeasurement.MeasurementCode : string.Empty,
                ToMeasurementName = x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty,
                ToMeasurementSymbol = x.ToMeasurement != null ? x.ToMeasurement.MeasurementSymbol : null,
                FromQuantity = x.FromQuantity,
                ToQuantity = x.ToQuantity,
                ConversionFactor = x.ConversionFactor,
                ConversionType = x.ConversionType,
                IsDefault = x.IsDefault,
                IsBidirectional = x.IsBidirectional,
                IsForPurchase = x.IsForPurchase,
                IsForStock = x.IsForStock,
                IsForDispensing = x.IsForDispensing,
                IsForPrescription = x.IsForPrescription,
                IsForCompound = x.IsForCompound,
                EffectiveStartDate = x.EffectiveStartDate,
                EffectiveEndDate = x.EffectiveEndDate,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static IQueryable<MstDrugUnitConversion> ApplySorting(
            IQueryable<MstDrugUnitConversion> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc
                    ? query.OrderByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.CreateDateTime),

                "conversioncode" => isDesc
                    ? query.OrderByDescending(x => x.ConversionCode)
                    : query.OrderBy(x => x.ConversionCode),

                "conversionname" => isDesc
                    ? query.OrderByDescending(x => x.ConversionName)
                    : query.OrderBy(x => x.ConversionName),

                "drugname" => isDesc
                    ? query.OrderByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                    : query.OrderBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty),

                "frommeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.FromMeasurement != null ? x.FromMeasurement.MeasurementName : string.Empty),

                "tomeasurementname" => isDesc
                    ? query.OrderByDescending(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty)
                    : query.OrderBy(x => x.ToMeasurement != null ? x.ToMeasurement.MeasurementName : string.Empty),

                "conversionfactor" => isDesc
                    ? query.OrderByDescending(x => x.ConversionFactor)
                    : query.OrderBy(x => x.ConversionFactor),

                "conversiontype" => isDesc
                    ? query.OrderByDescending(x => x.ConversionType)
                    : query.OrderBy(x => x.ConversionType),

                "isdefault" => isDesc
                    ? query.OrderByDescending(x => x.IsDefault)
                    : query.OrderBy(x => x.IsDefault),

                "isactive" => isDesc
                    ? query.OrderByDescending(x => x.IsActive)
                    : query.OrderBy(x => x.IsActive),

                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder)
                        .ThenByDescending(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                        .ThenByDescending(x => x.ConversionName)
                    : query.OrderBy(x => x.SortOrder)
                        .ThenBy(x => x.Drug != null ? x.Drug.DrugName : string.Empty)
                        .ThenBy(x => x.ConversionName)
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static List<DrugUnitConversionCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<DrugUnitConversionCustomPeriodOptionResponse>
            {
                new() { Value = "today", Label = "Hari ini" },
                new() { Value = "yesterday", Label = "Kemarin" },
                new() { Value = "last7Days", Label = "7 hari terakhir" },
                new() { Value = "last30Days", Label = "30 hari terakhir" },
                new() { Value = "thisMonth", Label = "Bulan ini" },
                new() { Value = "lastMonth", Label = "Bulan lalu" },
                new() { Value = "thisYear", Label = "Tahun ini" }
            };
        }

        private static DateRangeResult ResolveDateRange(DateTime? startDate, DateTime? endDate, string? customPeriod)
        {
            if (string.IsNullOrWhiteSpace(customPeriod))
            {
                return BuildDateRange(startDate, endDate);
            }

            var today = DateTime.UtcNow.Date;

            return customPeriod.Trim().ToLowerInvariant() switch
            {
                "today" => BuildDateRange(today, today),
                "yesterday" => BuildDateRange(today.AddDays(-1), today.AddDays(-1)),
                "last7days" => BuildDateRange(today.AddDays(-6), today),
                "last30days" => BuildDateRange(today.AddDays(-29), today),
                "thismonth" => BuildDateRange(new DateTime(today.Year, today.Month, 1), today),
                "lastmonth" => BuildDateRange(
                    new DateTime(today.Year, today.Month, 1).AddMonths(-1),
                    new DateTime(today.Year, today.Month, 1).AddDays(-1)),
                "thisyear" => BuildDateRange(new DateTime(today.Year, 1, 1), today),
                _ => new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = "Custom period tidak valid."
                }
            };
        }

        private static DateRangeResult BuildDateRange(DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue && endDate.HasValue && endDate.Value.Date < startDate.Value.Date)
            {
                return new DateRangeResult
                {
                    IsValid = false,
                    ErrorMessage = "End date tidak boleh lebih kecil dari start date."
                };
            }

            return new DateRangeResult
            {
                IsValid = true,
                Start = startDate?.Date,
                EndExclusive = endDate?.Date.AddDays(1)
            };
        }

        private static string NormalizeConversionType(string value)
        {
            var trimmed = value.Trim();

            var matched = AllowedConversionTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
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

        private class DateRangeResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public DateTime? Start { get; set; }
            public DateTime? EndExclusive { get; set; }
        }
    }
}
