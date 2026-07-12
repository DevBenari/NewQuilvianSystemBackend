using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

using ResponsePrescriptionCompoundPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs.PrescriptionCompoundResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-compounds")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Prescription Compound",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionCompound",
        Description = "Racikan pada resep dokter",
        SortOrder = 3
    )]
    [Tags("Health Services / Pharmacy Management / Prescription Compound")]
    public class PrescriptionCompoundController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Pharmacy";

        private readonly ApplicationDbContext _dbContext;
        private readonly PrescriptionAggregateService _prescriptionAggregateService;
        private readonly LoggerService _loggerService;

        public PrescriptionCompoundController(
            ApplicationDbContext dbContext,
            PrescriptionAggregateService prescriptionAggregateService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _prescriptionAggregateService = prescriptionAggregateService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Compound", Description = "Melihat metadata filter racikan resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionCompound", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PrescriptionCompoundFilterMetadataResponse
            {
                DefaultFilter = new PrescriptionCompoundDefaultFilterResponse(),
                SortOptions = new List<PrescriptionCompoundSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "compoundName", Label = "Nama racikan" },
                    new() { Value = "ingredientCount", Label = "Jumlah bahan" },
                    new() { Value = "totalPrice", Label = "Total harga" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionCompound.GetFilterMetadata",
                "Mengambil metadata filter racikan resep.",
                result);

            return Ok(ApiResponse<PrescriptionCompoundFilterMetadataResponse>.Ok(
                result,
                "Metadata filter racikan resep berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePrescriptionCompoundPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Compound", Description = "Melihat data racikan resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionCompound", "Read")]
        public async Task<IActionResult> GetCompounds(
            [FromQuery] string? search,
            [FromQuery] Guid? prescriptionId,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isApproved,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = BuildBaseQuery().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.CompoundName.ToLower().Contains(keyword) ||
                    (x.CompoundForm != null && x.CompoundForm.ToLower().Contains(keyword)) ||
                    (x.Signa != null && x.Signa.ToLower().Contains(keyword)));
            }

            if (prescriptionId.HasValue && prescriptionId.Value != Guid.Empty)
                query = query.Where(x => x.PrescriptionId == prescriptionId.Value);

            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);

            if (isApproved.HasValue)
                query = query.Where(x => x.IsApproved == isApproved.Value);

            var totalData = await query.CountAsync(cancellationToken);

            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new ResponsePrescriptionCompoundPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<ResponsePrescriptionCompoundPagedResult>.Ok(
                result,
                "Data racikan resep berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Prescription Compound", Description = "Melihat detail racikan resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionCompound", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .Include(x => x.Items.Where(i => !i.IsDelete && !i.IsCancel && i.IsActive))
                    .ThenInclude(x => x.ApprovedByUser)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Racikan resep tidak ditemukan."));
            }

            var result = new PrescriptionCompoundDetailResponse();
            CopyResponse(ToResponse(entity), result);
            result.Items = entity.Items
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DrugNameSnapshot)
                .Select(PrescriptionCompoundItemController.ToResponseStatic)
                .ToList();

            return Ok(ApiResponse<PrescriptionCompoundDetailResponse>.Ok(
                result,
                "Detail racikan resep berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Prescription Compound", Description = "Menambahkan racikan ke resep", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PrescriptionCompound", "Create")]
        public async Task<IActionResult> CreateCompound(
            [FromBody] CreatePrescriptionCompoundRequest request,
            CancellationToken cancellationToken = default)
        {
            if (request.PrescriptionId == Guid.Empty || string.IsNullOrWhiteSpace(request.CompoundName))
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "PrescriptionId dan nama racikan wajib diisi."));
            }

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    request.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }

            var packageUnit = await ResolveMeasurementAsync(request.PackageUnitMeasurementId, cancellationToken);
            var doseUnit = await ResolveMeasurementAsync(request.DoseUnitMeasurementId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entity = new TrxPrescriptionCompound
            {
                Id = Guid.NewGuid(),
                PrescriptionId = request.PrescriptionId,
                CompoundName = request.CompoundName.Trim(),
                CompoundForm = NormalizeNullableText(request.CompoundForm),
                TotalPackage = request.TotalPackage,
                PackageUnitMeasurementId = packageUnit?.Id,
                PackageUnitNameSnapshot = packageUnit?.MeasurementName,
                PackageUnitSymbolSnapshot = packageUnit?.MeasurementSymbol,
                DosePerUse = request.DosePerUse,
                DoseUnitMeasurementId = doseUnit?.Id,
                DoseUnitNameSnapshot = doseUnit?.MeasurementName,
                DoseUnitSymbolSnapshot = doseUnit?.MeasurementSymbol,
                FrequencyCode = NormalizeNullableText(request.FrequencyCode),
                FrequencyText = NormalizeNullableText(request.FrequencyText),
                FrequencyPerDay = request.FrequencyPerDay,
                DurationValue = request.DurationValue,
                DurationUnit = NormalizeNullableText(request.DurationUnit),
                IsAsNeeded = request.IsAsNeeded,
                AdministrationTime = NormalizeNullableText(request.AdministrationTime),
                Signa = NormalizeNullableText(request.Signa),
                CompoundingInstruction = NormalizeNullableText(request.CompoundingInstruction),
                AdministrationInstruction = NormalizeNullableText(request.AdministrationInstruction),
                DoctorNote = NormalizeNullableText(request.DoctorNote),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPrescriptionCompound>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                entity.PrescriptionId,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var response = ToMutationResponse(entity, aggregate);

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionCompound.CreateCompound",
                "Menambahkan racikan ke resep.",
                response);

            return Ok(ApiResponse<PrescriptionCompoundMutationResponse>.Ok(
                response,
                "Racikan resep berhasil ditambahkan."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Prescription Compound", Description = "Mengubah racikan resep", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PrescriptionCompound", "Update")]
        public async Task<IActionResult> UpdateCompound(
            Guid id,
            [FromBody] UpdatePrescriptionCompoundRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionCompound>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Racikan resep tidak ditemukan."));

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(entity.PrescriptionId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }

            var packageUnit = await ResolveMeasurementAsync(request.PackageUnitMeasurementId, cancellationToken);
            var doseUnit = await ResolveMeasurementAsync(request.DoseUnitMeasurementId, cancellationToken);
            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.CompoundName = request.CompoundName.Trim();
            entity.CompoundForm = NormalizeNullableText(request.CompoundForm);
            entity.TotalPackage = request.TotalPackage;
            entity.PackageUnitMeasurementId = packageUnit?.Id;
            entity.PackageUnitNameSnapshot = packageUnit?.MeasurementName;
            entity.PackageUnitSymbolSnapshot = packageUnit?.MeasurementSymbol;
            entity.DosePerUse = request.DosePerUse;
            entity.DoseUnitMeasurementId = doseUnit?.Id;
            entity.DoseUnitNameSnapshot = doseUnit?.MeasurementName;
            entity.DoseUnitSymbolSnapshot = doseUnit?.MeasurementSymbol;
            entity.FrequencyCode = NormalizeNullableText(request.FrequencyCode);
            entity.FrequencyText = NormalizeNullableText(request.FrequencyText);
            entity.FrequencyPerDay = request.FrequencyPerDay;
            entity.DurationValue = request.DurationValue;
            entity.DurationUnit = NormalizeNullableText(request.DurationUnit);
            entity.IsAsNeeded = request.IsAsNeeded;
            entity.AdministrationTime = NormalizeNullableText(request.AdministrationTime);
            entity.Signa = NormalizeNullableText(request.Signa);
            entity.CompoundingInstruction = NormalizeNullableText(request.CompoundingInstruction);
            entity.AdministrationInstruction = NormalizeNullableText(request.AdministrationInstruction);
            entity.DoctorNote = NormalizeNullableText(request.DoctorNote);
            entity.SortOrder = request.SortOrder;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                entity.PrescriptionId,
                actorUserId,
                now,
                cancellationToken);

            var response = ToMutationResponse(entity, aggregate);
            return Ok(ApiResponse<PrescriptionCompoundMutationResponse>.Ok(response, "Racikan resep berhasil diubah."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Prescription Compound", Description = "Menghapus racikan resep", AccessType = AccessTypes.Delete, SortOrder = 4)]
        [AccessPermission("PrescriptionCompound", "Delete")]
        public async Task<IActionResult> DeleteCompound(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionCompound>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Racikan resep tidak ditemukan."));

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(entity.PrescriptionId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            var items = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                .Where(x => x.PrescriptionCompoundId == entity.Id && !x.IsDelete)
                .ToListAsync(cancellationToken);

            foreach (var item in items)
            {
                item.IsDelete = true;
                item.IsActive = false;
                item.DeleteDateTime = now;
                item.DeleteBy = actorUserId;
                item.UpdateDateTime = now;
                item.UpdateBy = actorUserId;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _prescriptionAggregateService.RebuildAsync(entity.PrescriptionId, actorUserId, now, cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Racikan resep berhasil dihapus."));
        }

        private IQueryable<TrxPrescriptionCompound> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPrescriptionCompound>()
                .Include(x => x.Prescription)
                .Include(x => x.PackageUnitMeasurement)
                .Include(x => x.DoseUnitMeasurement)
                .Where(x => !x.IsDelete && !x.IsCancel);
        }

        private async Task<MstMeasurement?> ResolveMeasurementAsync(Guid? id, CancellationToken cancellationToken)
        {
            if (!id.HasValue || id.Value == Guid.Empty)
                return null;

            var measurement = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id.Value && x.IsActive && !x.IsDelete, cancellationToken);

            if (measurement == null)
                throw new InvalidOperationException("Satuan tidak ditemukan atau tidak aktif.");

            return measurement;
        }

        private static IQueryable<TrxPrescriptionCompound> ApplySorting(
            IQueryable<TrxPrescriptionCompound> query,
            string? sortBy,
            string? sortDirection)
        {
            var desc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);
            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "compoundname" => desc ? query.OrderByDescending(x => x.CompoundName) : query.OrderBy(x => x.CompoundName),
                "ingredientcount" => desc ? query.OrderByDescending(x => x.IngredientCount) : query.OrderBy(x => x.IngredientCount),
                "totalprice" => desc ? query.OrderByDescending(x => x.TotalPrice) : query.OrderBy(x => x.TotalPrice),
                "createdatetime" => desc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => desc ? query.OrderByDescending(x => x.SortOrder) : query.OrderBy(x => x.SortOrder)
            };
        }

        private static PrescriptionCompoundResponse ToResponse(TrxPrescriptionCompound x)
        {
            return new PrescriptionCompoundResponse
            {
                Id = x.Id,
                PrescriptionId = x.PrescriptionId,
                PrescriptionNumber = x.Prescription?.PrescriptionNumber ?? string.Empty,
                CompoundName = x.CompoundName,
                CompoundForm = x.CompoundForm,
                TotalPackage = x.TotalPackage,
                PackageUnitMeasurementId = x.PackageUnitMeasurementId,
                PackageUnitNameSnapshot = x.PackageUnitNameSnapshot,
                PackageUnitSymbolSnapshot = x.PackageUnitSymbolSnapshot,
                DosePerUse = x.DosePerUse,
                DoseUnitMeasurementId = x.DoseUnitMeasurementId,
                DoseUnitNameSnapshot = x.DoseUnitNameSnapshot,
                DoseUnitSymbolSnapshot = x.DoseUnitSymbolSnapshot,
                FrequencyCode = x.FrequencyCode,
                FrequencyText = x.FrequencyText,
                FrequencyPerDay = x.FrequencyPerDay,
                DurationValue = x.DurationValue,
                DurationUnit = x.DurationUnit,
                IsAsNeeded = x.IsAsNeeded,
                AdministrationTime = x.AdministrationTime,
                Signa = x.Signa,
                CompoundingInstruction = x.CompoundingInstruction,
                AdministrationInstruction = x.AdministrationInstruction,
                DoctorNote = x.DoctorNote,
                IngredientCount = x.IngredientCount,
                TotalPrice = x.TotalPrice,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                IsNeedApproval = x.IsNeedApproval,
                IsApproved = x.IsApproved,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static void CopyResponse(PrescriptionCompoundResponse source, PrescriptionCompoundResponse target)
        {
            foreach (var property in typeof(PrescriptionCompoundResponse).GetProperties())
                property.SetValue(target, property.GetValue(source));
        }

        private static PrescriptionCompoundMutationResponse ToMutationResponse(
            TrxPrescriptionCompound entity,
            PrescriptionAggregateResult aggregate)
        {
            var result = new PrescriptionCompoundMutationResponse
            {
                RegularItemCount = aggregate.RegularItemCount,
                CompoundCount = aggregate.CompoundCount,
                CompoundIngredientCount = aggregate.CompoundIngredientCount,
                TotalItemCount = aggregate.TotalItemCount,
                PrescriptionTotalPrice = aggregate.TotalPrice,
                PrescriptionCoveredAmount = aggregate.CoveredAmount,
                PrescriptionPatientPayAmount = aggregate.PatientPayAmount
            };
            CopyResponse(ToResponse(entity), result);
            return result;
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableText(string? value)
            => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
