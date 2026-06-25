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
using System.Data;
using System.Security.Claims;

using ResponseMeasurementPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs.MeasurementResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/measurements")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Measurement",
        AreaName = "HealthServices",
        ControllerName = "Measurement",
        Description = "Health service master data measurement / satuan",
        SortOrder = 10
    )]
    [Tags("Health Services / Master Data / Measurement")]
    public class MeasurementController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData";
        private const string MeasurementCodePrefix = "MS-RSMMC-";
        private const int MeasurementCodeDigitLength = 5;

        private static readonly HashSet<string> AllowedMeasurementTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "General",
            "Weight",
            "Volume",
            "Length",
            "Count",
            "Time",
            "Dose",
            "Pharmacy"
        };

        private readonly ApplicationDbContext _dbContext;
        private readonly LoggerService _loggerService;

        public MeasurementController(
            ApplicationDbContext dbContext,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement", Description = "Melihat data measurement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Measurement", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new MeasurementFilterMetadataResponse
            {
                DefaultFilter = new MeasurementDefaultFilterResponse(),
                CustomPeriods = BuildCustomPeriodOptions(),
                SortOptions = new List<MeasurementSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" },
                    new() { Value = "measurementCode", Label = "Kode satuan" },
                    new() { Value = "measurementName", Label = "Nama satuan" },
                    new() { Value = "measurementSymbol", Label = "Simbol satuan" },
                    new() { Value = "measurementType", Label = "Tipe satuan" },
                    new() { Value = "measurementGroupName", Label = "Group satuan" },
                    new() { Value = "isBaseUnit", Label = "Base unit" },
                    new() { Value = "isDecimalAllowed", Label = "Boleh decimal" },
                    new() { Value = "decimalPrecision", Label = "Precision decimal" },
                    new() { Value = "isActive", Label = "Status aktif" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 },
                MeasurementTypeOptions = BuildMeasurementTypeOptions(),
                QueryParameters = BuildQueryParameterInfo(),
                CreateFields = BuildCreateFieldMetadata(),
                UpdateFields = BuildUpdateFieldMetadata()
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "Measurement.GetFilterMetadata",
                "Mengambil metadata filter measurement.",
                result
            );

            return Ok(ApiResponse<MeasurementFilterMetadataResponse>.Ok(
                result,
                "Metadata filter measurement berhasil diambil."
            ));
        }

        [HttpGet("summary")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementSummaryResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement", Description = "Melihat data measurement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Measurement", "Read")]
        public async Task<IActionResult> GetSummary()
        {
            var query = BuildBaseQuery();

            var result = new MeasurementSummaryResponse
            {
                TotalMeasurement = await query.CountAsync(),
                ActiveMeasurement = await query.CountAsync(x => x.IsActive),
                InactiveMeasurement = await query.CountAsync(x => !x.IsActive),
                BaseUnitMeasurement = await query.CountAsync(x => x.IsBaseUnit),
                DecimalAllowedMeasurement = await query.CountAsync(x => x.IsDecimalAllowed),
                DrugMeasurement = await query.CountAsync(x => x.IsForDrug),
                LaboratoryMeasurement = await query.CountAsync(x => x.IsForLaboratory),
                VitalSignMeasurement = await query.CountAsync(x => x.IsForVitalSign),
                GeneralUseMeasurement = await query.CountAsync(x => x.IsForGeneralUse)
            };

            return Ok(ApiResponse<MeasurementSummaryResponse>.Ok(
                result,
                "Ringkasan measurement berhasil diambil."
            ));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponseMeasurementPagedResult>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Read", "Read Measurement", Description = "Melihat data measurement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Measurement", "Read")]
        public async Task<IActionResult> GetMeasurements(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? customPeriod,
            [FromQuery] string? measurementType,
            [FromQuery] bool? isActive,
            [FromQuery] bool? isBaseUnit,
            [FromQuery] bool? isDecimalAllowed,
            [FromQuery] bool? isForDrug,
            [FromQuery] bool? isForLaboratory,
            [FromQuery] bool? isForVitalSign,
            [FromQuery] bool? isForGeneralUse,
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
                measurementType,
                isActive,
                isBaseUnit,
                isDecimalAllowed,
                isForDrug,
                isForLaboratory,
                isForVitalSign,
                isForGeneralUse,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var actorMap = await GetActorNameMapAsync(entities.Select(x => x.CreateBy));
            var items = entities.Select(x => ToResponse(x, actorMap)).ToList();

            var result = new ResponseMeasurementPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<ResponseMeasurementPagedResult>.Ok(
                result,
                "Data measurement berhasil diambil."
            ));
        }

        [HttpGet("options")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementOptionPagedResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Measurement", Description = "Melihat data pilihan measurement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Measurement", "Read")]
        public async Task<IActionResult> GetMeasurementOptions(
            [FromQuery] string? measurementType,
            [FromQuery] bool? isBaseUnit,
            [FromQuery] bool? isForDrug,
            [FromQuery] bool? isForLaboratory,
            [FromQuery] bool? isForVitalSign,
            [FromQuery] bool? isForGeneralUse,
            [FromQuery] bool onlyActive = true,
            [FromQuery] bool? activeOnly = null,
            [FromQuery] string? search = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var isActive = activeOnly ?? (onlyActive ? true : null);

            var query = ApplyStandardFilter(
                BuildBaseQuery(),
                measurementType,
                isActive,
                isBaseUnit,
                null,
                isForDrug,
                isForLaboratory,
                isForVitalSign,
                isForGeneralUse,
                search
            );

            var totalData = await query.CountAsync();

            var entities = await query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.MeasurementName)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var items = entities.Select(ToOptionResponse).ToList();

            var result = new MeasurementOptionPagedResponse
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = items
            };

            return Ok(ApiResponse<MeasurementOptionPagedResponse>.Ok(
                result,
                "Data pilihan measurement berhasil diambil."
            ));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Measurement", Description = "Melihat detail measurement", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("Measurement", "Read")]
        public async Task<IActionResult> GetMeasurementById(Guid id)
        {
            var entity = await BuildBaseQuery()
                .FirstOrDefaultAsync(x => x.Id == id);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement tidak ditemukan."
                ));
            }

            var actorMap = await GetActorNameMapAsync(new[] { entity.CreateBy, entity.UpdateBy });
            var data = ToDetailResponse(entity, actorMap);

            return Ok(ApiResponse<MeasurementDetailResponse>.Ok(
                data,
                "Detail measurement berhasil diambil."
            ));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<MeasurementCreateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Measurement", Description = "Membuat data measurement", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("Measurement", "Create")]
        public async Task<IActionResult> CreateMeasurement([FromBody] CreateMeasurementRequest request)
        {
            var validation = await ValidateRequestAsync(
                excludeId: null,
                measurementName: request.MeasurementName,
                measurementSymbol: request.MeasurementSymbol,
                measurementType: request.MeasurementType,
                measurementGroupName: request.MeasurementGroupName,
                decimalPrecision: request.DecimalPrecision
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data measurement tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            var generatedCode = await GenerateMeasurementCodeAsync();
            var codeValidation = await ValidateGeneratedMeasurementCodeAsync(generatedCode);

            if (!codeValidation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    codeValidation.ErrorMessage ?? "Kode measurement otomatis tidak valid."
                ));
            }

            var entity = new MstMeasurement
            {
                Id = Guid.NewGuid(),
                MeasurementCode = generatedCode,
                MeasurementName = request.MeasurementName.Trim(),
                MeasurementSymbol = NormalizeNullableText(request.MeasurementSymbol),
                MeasurementType = NormalizeMeasurementType(request.MeasurementType),
                MeasurementGroupName = NormalizeNullableText(request.MeasurementGroupName),
                IsBaseUnit = request.IsBaseUnit,
                IsDecimalAllowed = request.IsDecimalAllowed,
                DecimalPrecision = request.IsDecimalAllowed ? request.DecimalPrecision : 0,
                IsForDrug = request.IsForDrug,
                IsForLaboratory = request.IsForLaboratory,
                IsForVitalSign = request.IsForVitalSign,
                IsForGeneralUse = request.IsForGeneralUse,
                SortOrder = request.SortOrder,
                Description = NormalizeNullableText(request.Description),
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<MstMeasurement>().Add(entity);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            var response = ToCreateUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Measurement.CreateMeasurement",
                "Membuat data measurement.",
                response
            );

            return Ok(ApiResponse<MeasurementCreateResponse>.Ok(
                response,
                "Measurement berhasil dibuat."
            ));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Measurement", Description = "Mengubah data measurement", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Measurement", "Update")]
        public async Task<IActionResult> UpdateMeasurement(Guid id, [FromBody] UpdateMeasurementRequest request)
        {
            var entity = await _dbContext.Set<MstMeasurement>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement tidak ditemukan."
                ));
            }

            var validation = await ValidateRequestAsync(
                excludeId: id,
                measurementName: request.MeasurementName,
                measurementSymbol: request.MeasurementSymbol,
                measurementType: request.MeasurementType,
                measurementGroupName: request.MeasurementGroupName,
                decimalPrecision: request.DecimalPrecision
            );

            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data measurement tidak valid."
                ));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.MeasurementName = request.MeasurementName.Trim();
            entity.MeasurementSymbol = NormalizeNullableText(request.MeasurementSymbol);
            entity.MeasurementType = NormalizeMeasurementType(request.MeasurementType);
            entity.MeasurementGroupName = NormalizeNullableText(request.MeasurementGroupName);
            entity.IsBaseUnit = request.IsBaseUnit;
            entity.IsDecimalAllowed = request.IsDecimalAllowed;
            entity.DecimalPrecision = request.IsDecimalAllowed ? request.DecimalPrecision : 0;
            entity.IsForDrug = request.IsForDrug;
            entity.IsForLaboratory = request.IsForLaboratory;
            entity.IsForVitalSign = request.IsForVitalSign;
            entity.IsForGeneralUse = request.IsForGeneralUse;
            entity.SortOrder = request.SortOrder;
            entity.Description = NormalizeNullableText(request.Description);
            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            await _loggerService.InfoAsync(
                LogCategory,
                "Measurement.UpdateMeasurement",
                "Mengubah data measurement.",
                response
            );

            return Ok(ApiResponse<MeasurementUpdateResponse>.Ok(
                response,
                "Measurement berhasil diperbarui."
            ));
        }

        [HttpPatch("{id:guid}/status")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementUpdateResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Measurement Status", Description = "Mengubah status aktif measurement", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("Measurement", "Update")]
        public async Task<IActionResult> UpdateMeasurementStatus(Guid id, [FromBody] UpdateMeasurementStatusRequest request)
        {
            var entity = await _dbContext.Set<MstMeasurement>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement tidak ditemukan."
                ));
            }

            entity.IsActive = request.IsActive;
            entity.UpdateDateTime = DateTime.UtcNow;
            entity.UpdateBy = GetCurrentUserId();

            if (!string.IsNullOrWhiteSpace(request.Reason))
                entity.Description = request.Reason.Trim();

            await _dbContext.SaveChangesAsync();

            var response = ToUpdateResponse(entity);

            return Ok(ApiResponse<MeasurementUpdateResponse>.Ok(
                response,
                request.IsActive ? "Measurement berhasil diaktifkan." : "Measurement berhasil dinonaktifkan."
            ));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<MeasurementDeleteResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Measurement", Description = "Menghapus data measurement", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("Measurement", "Delete")]
        public async Task<IActionResult> DeleteMeasurement(Guid id, [FromBody] DeleteMeasurementRequest? request = null)
        {
            var entity = await _dbContext.Set<MstMeasurement>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Measurement tidak ditemukan."
                ));
            }

            var usedByConversion = await _dbContext.Set<MstMeasurementConversion>()
                .AsNoTracking()
                .AnyAsync(x => !x.IsDelete && (x.FromMeasurementId == id || x.ToMeasurementId == id));

            var usedByDrug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    (x.StrengthMeasurementId == id ||
                     x.BaseUnitMeasurementId == id ||
                     x.DispenseUnitMeasurementId == id ||
                     x.PurchaseUnitMeasurementId == id ||
                     x.StockUnitMeasurementId == id ||
                     x.DefaultDoseUnitMeasurementId == id));

            if (usedByConversion || usedByDrug)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Measurement tidak dapat dihapus karena sudah digunakan oleh conversion atau drug. Nonaktifkan saja jika masih dibutuhkan untuk histori."
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
                entity.Description = request.DeleteReason.Trim();

            await _dbContext.SaveChangesAsync();

            var response = new MeasurementDeleteResponse
            {
                Id = entity.Id,
                MeasurementCode = entity.MeasurementCode,
                MeasurementName = entity.MeasurementName,
                IsDelete = entity.IsDelete,
                IsActive = entity.IsActive,
                DeleteDateTime = entity.DeleteDateTime
            };

            return Ok(ApiResponse<MeasurementDeleteResponse>.Ok(
                response,
                "Measurement berhasil dihapus."
            ));
        }

        private IQueryable<MstMeasurement> BuildBaseQuery()
        {
            return _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<MstMeasurement> ApplyDateFilter(IQueryable<MstMeasurement> query, DateRangeResult dateRange)
        {
            if (dateRange.Start.HasValue)
                query = query.Where(x => x.CreateDateTime >= dateRange.Start.Value);

            if (dateRange.EndExclusive.HasValue)
                query = query.Where(x => x.CreateDateTime < dateRange.EndExclusive.Value);

            return query;
        }

        private static IQueryable<MstMeasurement> ApplyStandardFilter(
            IQueryable<MstMeasurement> query,
            string? measurementType,
            bool? isActive,
            bool? isBaseUnit,
            bool? isDecimalAllowed,
            bool? isForDrug,
            bool? isForLaboratory,
            bool? isForVitalSign,
            bool? isForGeneralUse,
            string? search)
        {
            if (!string.IsNullOrWhiteSpace(measurementType))
            {
                var normalizedType = NormalizeMeasurementType(measurementType);
                query = query.Where(x => x.MeasurementType.ToLower() == normalizedType.ToLower());
            }

            if (isActive.HasValue)
                query = query.Where(x => x.IsActive == isActive.Value);

            if (isBaseUnit.HasValue)
                query = query.Where(x => x.IsBaseUnit == isBaseUnit.Value);

            if (isDecimalAllowed.HasValue)
                query = query.Where(x => x.IsDecimalAllowed == isDecimalAllowed.Value);

            if (isForDrug.HasValue)
                query = query.Where(x => x.IsForDrug == isForDrug.Value);

            if (isForLaboratory.HasValue)
                query = query.Where(x => x.IsForLaboratory == isForLaboratory.Value);

            if (isForVitalSign.HasValue)
                query = query.Where(x => x.IsForVitalSign == isForVitalSign.Value);

            if (isForGeneralUse.HasValue)
                query = query.Where(x => x.IsForGeneralUse == isForGeneralUse.Value);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();

                query = query.Where(x =>
                    x.MeasurementCode.ToLower().Contains(keyword) ||
                    x.MeasurementName.ToLower().Contains(keyword) ||
                    x.MeasurementType.ToLower().Contains(keyword) ||
                    (x.MeasurementSymbol != null && x.MeasurementSymbol.ToLower().Contains(keyword)) ||
                    (x.MeasurementGroupName != null && x.MeasurementGroupName.ToLower().Contains(keyword)) ||
                    (x.Description != null && x.Description.ToLower().Contains(keyword)));
            }

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateRequestAsync(
            Guid? excludeId,
            string measurementName,
            string? measurementSymbol,
            string measurementType,
            string? measurementGroupName,
            int decimalPrecision)
        {
            if (string.IsNullOrWhiteSpace(measurementName))
                return (false, "Nama measurement wajib diisi.");

            if (measurementName.Trim().Length > 150)
                return (false, "Nama measurement maksimal 150 karakter.");

            if (string.IsNullOrWhiteSpace(measurementType))
                return (false, "Tipe measurement wajib diisi.");

            if (!AllowedMeasurementTypes.Contains(measurementType.Trim()))
            {
                return (false, "Tipe measurement tidak valid. Gunakan salah satu: General, Weight, Volume, Length, Count, Time, Dose, Pharmacy.");
            }

            if (decimalPrecision < 0 || decimalPrecision > 8)
                return (false, "Decimal precision harus berada di antara 0 sampai 8.");

            var normalizedName = measurementName.Trim().ToLower();
            var normalizedType = NormalizeMeasurementType(measurementType);
            var normalizedSymbol = NormalizeNullableText(measurementSymbol)?.ToLower();
            var normalizedGroupName = NormalizeNullableText(measurementGroupName)?.ToLower();

            var duplicateName = await _dbContext.Set<MstMeasurement>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.MeasurementName.ToLower() == normalizedName &&
                    (!excludeId.HasValue || x.Id != excludeId.Value));

            if (duplicateName)
                return (false, "Nama measurement sudah digunakan.");

            if (!string.IsNullOrWhiteSpace(normalizedSymbol))
            {
                var duplicateSymbolInTypeAndGroup = await _dbContext.Set<MstMeasurement>()
                    .AnyAsync(x =>
                        !x.IsDelete &&
                        x.MeasurementSymbol != null &&
                        x.MeasurementSymbol.ToLower() == normalizedSymbol &&
                        x.MeasurementType.ToLower() == normalizedType.ToLower() &&
                        ((x.MeasurementGroupName == null && normalizedGroupName == null) ||
                         (x.MeasurementGroupName != null && x.MeasurementGroupName.ToLower() == normalizedGroupName)) &&
                        (!excludeId.HasValue || x.Id != excludeId.Value));

                if (duplicateSymbolInTypeAndGroup)
                    return (false, "Simbol measurement pada tipe dan group tersebut sudah digunakan.");
            }

            return (true, null);
        }

        private async Task<string> GenerateMeasurementCodeAsync()
        {
            var existingCodes = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .Where(x =>
                    !x.IsDelete &&
                    x.MeasurementCode.StartsWith(MeasurementCodePrefix))
                .Select(x => x.MeasurementCode)
                .ToListAsync();

            var usedNumbers = existingCodes
                .Select(ExtractMeasurementCodeNumber)
                .Where(x => x.HasValue && x.Value > 0)
                .Select(x => x!.Value)
                .ToHashSet();

            var nextNumber = 1;

            while (usedNumbers.Contains(nextNumber))
                nextNumber++;

            return MeasurementCodePrefix + nextNumber.ToString($"D{MeasurementCodeDigitLength}");
        }

        private static int? ExtractMeasurementCodeNumber(string measurementCode)
        {
            if (string.IsNullOrWhiteSpace(measurementCode))
                return null;

            if (!measurementCode.StartsWith(MeasurementCodePrefix, StringComparison.OrdinalIgnoreCase))
                return null;

            var numberText = measurementCode[MeasurementCodePrefix.Length..];

            return int.TryParse(numberText, out var number)
                ? number
                : null;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateGeneratedMeasurementCodeAsync(string measurementCode)
        {
            if (string.IsNullOrWhiteSpace(measurementCode))
                return (false, "Kode measurement otomatis gagal dibuat.");

            var normalizedCode = measurementCode.Trim().ToUpperInvariant();

            var duplicateCode = await _dbContext.Set<MstMeasurement>()
                .AnyAsync(x =>
                    !x.IsDelete &&
                    x.MeasurementCode.ToUpper() == normalizedCode);

            if (duplicateCode)
                return (false, "Kode measurement otomatis sudah digunakan. Silakan ulangi proses create.");

            return (true, null);
        }

        private static IQueryable<MstMeasurement> ApplySorting(
            IQueryable<MstMeasurement> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                "measurementcode" => isDesc ? query.OrderByDescending(x => x.MeasurementCode) : query.OrderBy(x => x.MeasurementCode),
                "measurementname" => isDesc ? query.OrderByDescending(x => x.MeasurementName) : query.OrderBy(x => x.MeasurementName),
                "measurementsymbol" => isDesc ? query.OrderByDescending(x => x.MeasurementSymbol) : query.OrderBy(x => x.MeasurementSymbol),
                "measurementtype" => isDesc ? query.OrderByDescending(x => x.MeasurementType) : query.OrderBy(x => x.MeasurementType),
                "measurementgroupname" => isDesc ? query.OrderByDescending(x => x.MeasurementGroupName) : query.OrderBy(x => x.MeasurementGroupName),
                "isbaseunit" => isDesc ? query.OrderByDescending(x => x.IsBaseUnit) : query.OrderBy(x => x.IsBaseUnit),
                "isdecimalallowed" => isDesc ? query.OrderByDescending(x => x.IsDecimalAllowed) : query.OrderBy(x => x.IsDecimalAllowed),
                "decimalprecision" => isDesc ? query.OrderByDescending(x => x.DecimalPrecision) : query.OrderBy(x => x.DecimalPrecision),
                "isactive" => isDesc ? query.OrderByDescending(x => x.IsActive) : query.OrderBy(x => x.IsActive),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.MeasurementName)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.MeasurementName)
            };
        }

        private static MeasurementResponse ToResponse(MstMeasurement x, Dictionary<Guid, string?> actorMap)
        {
            return new MeasurementResponse
            {
                Id = x.Id,
                MeasurementCode = x.MeasurementCode,
                MeasurementName = x.MeasurementName,
                MeasurementSymbol = x.MeasurementSymbol,
                MeasurementType = x.MeasurementType,
                MeasurementTypeName = SplitPascalCase(x.MeasurementType),
                MeasurementGroupName = x.MeasurementGroupName,
                IsBaseUnit = x.IsBaseUnit,
                IsDecimalAllowed = x.IsDecimalAllowed,
                DecimalPrecision = x.DecimalPrecision,
                IsForDrug = x.IsForDrug,
                IsForLaboratory = x.IsForLaboratory,
                IsForVitalSign = x.IsForVitalSign,
                IsForGeneralUse = x.IsForGeneralUse,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime,
                CreateBy = x.CreateBy == Guid.Empty ? null : x.CreateBy,
                CreateByName = x.CreateBy == Guid.Empty ? null : actorMap.GetValueOrDefault(x.CreateBy)
            };
        }

        private static MeasurementDetailResponse ToDetailResponse(MstMeasurement x, Dictionary<Guid, string?> actorMap)
        {
            return new MeasurementDetailResponse
            {
                Id = x.Id,
                MeasurementCode = x.MeasurementCode,
                MeasurementName = x.MeasurementName,
                MeasurementSymbol = x.MeasurementSymbol,
                MeasurementType = x.MeasurementType,
                MeasurementTypeName = SplitPascalCase(x.MeasurementType),
                MeasurementGroupName = x.MeasurementGroupName,
                IsBaseUnit = x.IsBaseUnit,
                IsDecimalAllowed = x.IsDecimalAllowed,
                DecimalPrecision = x.DecimalPrecision,
                IsForDrug = x.IsForDrug,
                IsForLaboratory = x.IsForLaboratory,
                IsForVitalSign = x.IsForVitalSign,
                IsForGeneralUse = x.IsForGeneralUse,
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
        }

        private static MeasurementOptionResponse ToOptionResponse(MstMeasurement x)
        {
            return new MeasurementOptionResponse
            {
                Id = x.Id,
                MeasurementCode = x.MeasurementCode,
                MeasurementName = x.MeasurementName,
                MeasurementSymbol = x.MeasurementSymbol,
                MeasurementType = x.MeasurementType,
                MeasurementTypeName = SplitPascalCase(x.MeasurementType),
                MeasurementGroupName = x.MeasurementGroupName,
                IsBaseUnit = x.IsBaseUnit,
                IsDecimalAllowed = x.IsDecimalAllowed,
                DecimalPrecision = x.DecimalPrecision,
                IsForDrug = x.IsForDrug,
                IsForLaboratory = x.IsForLaboratory,
                IsForVitalSign = x.IsForVitalSign,
                IsForGeneralUse = x.IsForGeneralUse,
                IsActive = x.IsActive
            };
        }

        private static MeasurementCreateResponse ToCreateUpdateResponse(MstMeasurement entity)
        {
            return new MeasurementCreateResponse
            {
                Id = entity.Id,
                MeasurementCode = entity.MeasurementCode,
                MeasurementName = entity.MeasurementName,
                MeasurementSymbol = entity.MeasurementSymbol,
                MeasurementType = entity.MeasurementType,
                MeasurementTypeName = SplitPascalCase(entity.MeasurementType),
                IsActive = entity.IsActive
            };
        }

        private static MeasurementUpdateResponse ToUpdateResponse(MstMeasurement entity)
        {
            return new MeasurementUpdateResponse
            {
                Id = entity.Id,
                MeasurementCode = entity.MeasurementCode,
                MeasurementName = entity.MeasurementName,
                MeasurementSymbol = entity.MeasurementSymbol,
                MeasurementType = entity.MeasurementType,
                MeasurementTypeName = SplitPascalCase(entity.MeasurementType),
                IsActive = entity.IsActive
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

        private static List<MeasurementCustomPeriodOptionResponse> BuildCustomPeriodOptions()
        {
            return new List<MeasurementCustomPeriodOptionResponse>
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

        private static List<MeasurementStringOptionResponse> BuildMeasurementTypeOptions()
        {
            return AllowedMeasurementTypes
                .OrderBy(x => x)
                .Select(x => new MeasurementStringOptionResponse
                {
                    Value = x,
                    Label = SplitPascalCase(x)
                })
                .ToList();
        }

        private static List<MeasurementQueryParameterInfoResponse> BuildQueryParameterInfo()
        {
            return new List<MeasurementQueryParameterInfoResponse>
            {
                new() { Name = "startDate", Type = "date", Description = "Tanggal awal filter berdasarkan CreateDateTime.", Example = "2026-06-01" },
                new() { Name = "endDate", Type = "date", Description = "Tanggal akhir filter berdasarkan CreateDateTime.", Example = "2026-06-30" },
                new() { Name = "customPeriod", Type = "string", Description = "Preset periode: all, today, yesterday, last7days, last30days, thismonth, lastmonth, thisyear, custom.", Example = "thisMonth" },
                new() { Name = "measurementType", Type = "string", Description = "Filter tipe measurement.", Example = "Weight" },
                new() { Name = "isActive", Type = "boolean", Description = "Filter status aktif." },
                new() { Name = "isBaseUnit", Type = "boolean", Description = "Filter base unit." },
                new() { Name = "isDecimalAllowed", Type = "boolean", Description = "Filter satuan yang mengizinkan decimal." },
                new() { Name = "isForDrug", Type = "boolean", Description = "Filter untuk penggunaan obat." },
                new() { Name = "isForLaboratory", Type = "boolean", Description = "Filter untuk laboratorium." },
                new() { Name = "isForVitalSign", Type = "boolean", Description = "Filter untuk vital sign." },
                new() { Name = "isForGeneralUse", Type = "boolean", Description = "Filter penggunaan umum." },
                new() { Name = "search", Type = "string", Description = "Pencarian kode, nama, simbol, tipe, group, atau deskripsi." },
                new() { Name = "sortBy", Type = "string", Description = "Kolom sorting." },
                new() { Name = "sortDirection", Type = "string", Description = "asc atau desc.", Example = "asc" },
                new() { Name = "pageNumber", Type = "integer", Description = "Nomor halaman.", Example = "1" },
                new() { Name = "pageSize", Type = "integer", Description = "Jumlah data per halaman.", Example = "25" }
            };
        }

        private static List<MeasurementFormFieldMetadataResponse> BuildCreateFieldMetadata()
        {
            return new List<MeasurementFormFieldMetadataResponse>
            {
                new() { Name = "measurementCode", Label = "Kode Measurement", Section = "Basic", DataType = "string", InputType = "readonly", Required = false, IsReadonly = true, RequiredType = "AutoGenerated", MaxLength = 50, Placeholder = "Auto generated", Description = "Dibuat otomatis oleh sistem dengan format MS-RSMMC-00001.", SortOrder = 1 },
                new() { Name = "measurementName", Label = "Nama Measurement", Section = "Basic", DataType = "string", InputType = "text", Required = true, RequiredType = "Required", MaxLength = 150, SortOrder = 2 },
                new() { Name = "measurementSymbol", Label = "Simbol", Section = "Basic", DataType = "string", InputType = "text", MaxLength = 50, Placeholder = "mg, ml, tablet", SortOrder = 3 },
                new() { Name = "measurementType", Label = "Tipe Measurement", Section = "Basic", DataType = "string", InputType = "select", Required = true, RequiredType = "Required", OptionsSource = "measurementTypeOptions", Description = "General, Weight, Volume, Length, Count, Time, Dose, Pharmacy.", SortOrder = 4 },
                new() { Name = "measurementGroupName", Label = "Group Measurement", Section = "Basic", DataType = "string", InputType = "text", MaxLength = 100, Placeholder = "Weight, Volume, Tablet, Injection, Syrup", SortOrder = 5 },
                new() { Name = "isBaseUnit", Label = "Base Unit", Section = "Rule", DataType = "boolean", InputType = "switch", SortOrder = 6 },
                new() { Name = "isDecimalAllowed", Label = "Boleh Decimal", Section = "Rule", DataType = "boolean", InputType = "switch", SortOrder = 7 },
                new() { Name = "decimalPrecision", Label = "Decimal Precision", Section = "Rule", DataType = "integer", InputType = "number", Description = "Nilai 0 sampai 8. Jika decimal tidak diizinkan, sistem menyimpan 0.", SortOrder = 8 },
                new() { Name = "isForDrug", Label = "Untuk Obat", Section = "Usage", DataType = "boolean", InputType = "switch", SortOrder = 9 },
                new() { Name = "isForLaboratory", Label = "Untuk Laboratorium", Section = "Usage", DataType = "boolean", InputType = "switch", SortOrder = 10 },
                new() { Name = "isForVitalSign", Label = "Untuk Vital Sign", Section = "Usage", DataType = "boolean", InputType = "switch", SortOrder = 11 },
                new() { Name = "isForGeneralUse", Label = "Untuk Umum", Section = "Usage", DataType = "boolean", InputType = "switch", SortOrder = 12 },
                new() { Name = "sortOrder", Label = "Urutan", Section = "Display", DataType = "integer", InputType = "number", SortOrder = 13 },
                new() { Name = "description", Label = "Deskripsi", Section = "Additional", DataType = "string", InputType = "textarea", MaxLength = 250, SortOrder = 14 }
            };
        }

        private static List<MeasurementFormFieldMetadataResponse> BuildUpdateFieldMetadata()
        {
            var fields = BuildCreateFieldMetadata();

            fields.Add(new MeasurementFormFieldMetadataResponse
            {
                Name = "isActive",
                Label = "Status Aktif",
                Section = "Status",
                DataType = "boolean",
                InputType = "switch",
                SortOrder = 99
            });

            return fields.OrderBy(x => x.SortOrder).ToList();
        }

        private async Task<Dictionary<Guid, string?>> GetActorNameMapAsync(IEnumerable<Guid> actorIds)
        {
            var ids = actorIds.Where(x => x != Guid.Empty).Distinct().ToList();

            if (!ids.Any())
                return new Dictionary<Guid, string?>();

            return await _dbContext.Users
                .AsNoTracking()
                .Where(x => ids.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    Name = x.DisplayName ?? x.UserName ?? x.Email ?? x.UserCode
                })
                .ToDictionaryAsync(x => x.Id, x => x.Name);
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            return (pageNumber, pageSize);
        }

        private static string NormalizeMeasurementType(string value)
        {
            var trimmed = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();

            var matched = AllowedMeasurementTypes
                .FirstOrDefault(x => string.Equals(x, trimmed, StringComparison.OrdinalIgnoreCase));

            return matched ?? "General";
        }

        private Guid GetCurrentUserId()
        {
            var userIdText = User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                             User.FindFirstValue("user_id");

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

        private static string SplitPascalCase(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return string.Empty;

            return string.Concat(value.Select((x, i) =>
                i > 0 && char.IsUpper(x) ? " " + x : x.ToString()));
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
