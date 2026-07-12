using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
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

using ResponsePrescriptionItemPagedResult =
    QuilvianSystemBackend.Responses.PagedResult<
        QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs.PrescriptionItemResponse>;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-items")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Prescription Item",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionItem",
        Description = "Item obat umum pada resep dokter",
        SortOrder = 2
    )]
    [Tags("Health Services / Pharmacy Management / Prescription Item")]
    public class PrescriptionItemController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Pharmacy";

        private readonly ApplicationDbContext _dbContext;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly PrescriptionAggregateService _prescriptionAggregateService;
        private readonly LoggerService _loggerService;

        public PrescriptionItemController(
            ApplicationDbContext dbContext,
            InsuranceCoverageService insuranceCoverageService,
            PrescriptionAggregateService prescriptionAggregateService,
            LoggerService loggerService)
        {
            _dbContext = dbContext;
            _insuranceCoverageService = insuranceCoverageService;
            _prescriptionAggregateService = prescriptionAggregateService;
            _loggerService = loggerService;
        }

        [HttpGet("filters/metadata")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionItemFilterMetadataResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Item", Description = "Melihat metadata filter item resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionItem", "Read")]
        public async Task<IActionResult> GetFilterMetadata()
        {
            var result = new PrescriptionItemFilterMetadataResponse
            {
                DefaultFilter = new PrescriptionItemDefaultFilterResponse(),
                SortOptions = new List<PrescriptionItemSortOptionResponse>
                {
                    new() { Value = "sortOrder", Label = "Urutan" },
                    new() { Value = "drugCode", Label = "Kode obat" },
                    new() { Value = "drugName", Label = "Nama obat" },
                    new() { Value = "quantity", Label = "Jumlah" },
                    new() { Value = "totalPrice", Label = "Total harga" },
                    new() { Value = "patientPayAmount", Label = "Bayar pasien" },
                    new() { Value = "createDateTime", Label = "Tanggal dibuat" }
                },
                SortDirections = new List<string> { "asc", "desc" },
                PageSizeOptions = new List<int> { 10, 25, 50, 100 }
            };

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionItem.GetFilterMetadata",
                "Mengambil metadata filter item resep.",
                result);

            return Ok(ApiResponse<PrescriptionItemFilterMetadataResponse>.Ok(
                result,
                "Metadata filter item resep berhasil diambil."));
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<ResponsePrescriptionItemPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Item", Description = "Melihat item resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionItem", "Read")]
        public async Task<IActionResult> GetItems(
            [FromQuery] string? search,
            [FromQuery] Guid? prescriptionId,
            [FromQuery] Guid? drugId,
            [FromQuery] bool? isCoveredByInsurance,
            [FromQuery] bool? isNeedApproval,
            [FromQuery] bool? isApproved,
            [FromQuery] bool? isAntibiotic,
            [FromQuery] bool? isHighAlert,
            [FromQuery] string? sortBy = "sortOrder",
            [FromQuery] string? sortDirection = "asc",
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 25,
            CancellationToken cancellationToken = default)
        {
            var paging = NormalizePaging(pageNumber, pageSize);
            pageNumber = paging.PageNumber;
            pageSize = paging.PageSize;

            var query = ApplyFilters(
                BuildBaseQuery().AsNoTracking(),
                search,
                prescriptionId,
                drugId,
                isCoveredByInsurance,
                isNeedApproval,
                isApproved,
                isAntibiotic,
                isHighAlert);

            var totalData = await query.CountAsync(cancellationToken);
            var entities = await ApplySorting(query, sortBy, sortDirection)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new ResponsePrescriptionItemPagedResult
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalData = totalData,
                TotalPage = (int)Math.Ceiling(totalData / (double)pageSize),
                Items = entities.Select(ToResponse).ToList()
            };

            return Ok(ApiResponse<ResponsePrescriptionItemPagedResult>.Ok(
                result,
                "Data item resep berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionItemDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Prescription Item", Description = "Melihat detail item resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionItem", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Item resep tidak ditemukan."));
            }

            return Ok(ApiResponse<PrescriptionItemDetailResponse>.Ok(
                ToDetailResponse(entity),
                "Detail item resep berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionItemMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Prescription Item", Description = "Menambahkan obat umum ke resep", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PrescriptionItem", "Create")]
        public async Task<IActionResult> CreateItem(
            [FromBody] CreatePrescriptionItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var validation = await ValidateCreateRequestAsync(request, cancellationToken);
            if (!validation.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    validation.ErrorMessage ?? "Data item resep tidak valid."));
            }

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    request.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            var prescription = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == request.PrescriptionId && !x.IsDelete, cancellationToken);

            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .Include(x => x.DefaultDoseUnitMeasurement)
                .Include(x => x.DispenseUnitMeasurement)
                .FirstAsync(x => x.Id == request.DrugId && !x.IsDelete, cancellationToken);

            var doseUnit = await ResolveMeasurementAsync(
                request.DoseUnitMeasurementId ?? drug.DefaultDoseUnitMeasurementId,
                cancellationToken);

            var dispenseUnit = await ResolveMeasurementAsync(
                request.DispenseUnitMeasurementId ?? drug.DispenseUnitMeasurementId,
                cancellationToken);

            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                prescription.EncounterId,
                drug.Id,
                request.Quantity,
                prescription.PrescriptionDateTime,
                cancellationToken);

            if (!coverage.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    coverage.ErrorMessage ?? "Coverage obat tidak dapat dihitung."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var entity = new TrxPrescriptionItem
            {
                Id = Guid.NewGuid(),
                PrescriptionId = prescription.Id,
                DrugId = drug.Id,
                TariffId = coverage.TariffId,
                InsuranceTariffId = coverage.InsuranceTariffId,
                InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId,
                DrugCodeSnapshot = drug.DrugCode,
                DrugNameSnapshot = drug.DrugName,
                GenericNameSnapshot = drug.GenericName,
                DrugCategoryNameSnapshot = drug.DrugCategory?.DrugCategoryName,
                DrugFormSnapshot = drug.DrugForm,
                StrengthSnapshot = drug.Strength,
                RouteSnapshot = drug.Route,
                IsFormularySnapshot = drug.IsFormulary,
                IsGenericSnapshot = drug.IsGeneric,
                IsAntibioticSnapshot = drug.IsAntibiotic,
                IsNarcoticSnapshot = drug.IsNarcotic,
                IsPsychotropicSnapshot = drug.IsPsychotropic,
                IsHighAlertSnapshot = drug.IsHighAlert,
                Dose = request.Dose,
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
                AdministrationInstruction = NormalizeNullableText(request.AdministrationInstruction),
                DoctorNote = NormalizeNullableText(request.DoctorNote),
                Quantity = coverage.Quantity,
                DispenseUnitMeasurementId = dispenseUnit?.Id,
                DispenseUnitNameSnapshot = dispenseUnit?.MeasurementName,
                DispenseUnitSymbolSnapshot = dispenseUnit?.MeasurementSymbol,
                HospitalUnitPrice = coverage.HospitalUnitPrice,
                ContractUnitPrice = coverage.ContractUnitPrice,
                UnitPrice = coverage.UnitPrice,
                TotalPrice = coverage.TotalPrice,
                PricingSource = coverage.PricingSource,
                IsCoverageApplicable = coverage.IsCoverageApplicable,
                IsCoveredByInsurance = coverage.IsCovered,
                CoverageStatus = coverage.CoverageStatus,
                CoveragePercent = coverage.CoveragePercent,
                CoveredAmount = coverage.CoveredAmount,
                PatientPayAmount = coverage.PatientPayAmount,
                CoPaymentAmount = coverage.CoPaymentAmount,
                IsNeedApproval = coverage.IsNeedApproval || drug.IsNeedApproval,
                IsApproved = false,
                IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = coverage.IsAllowExcessPaymentByPatient,
                CoverageNote = BuildCoverageNote(coverage),
                SortOrder = request.SortOrder,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            _dbContext.Set<TrxPrescriptionItem>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                prescription.Id,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var response = ToMutationResponse(entity, aggregate);

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionItem.CreateItem",
                "Menambahkan obat umum ke resep.",
                response);

            return Ok(ApiResponse<PrescriptionItemMutationResponse>.Ok(
                response,
                "Item resep berhasil ditambahkan."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionItemMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Prescription Item", Description = "Mengubah item resep", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PrescriptionItem", "Update")]
        public async Task<IActionResult> UpdateItem(
            Guid id,
            [FromBody] UpdatePrescriptionItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Item resep tidak ditemukan."));
            }

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    entity.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            if (request.Dose <= 0 || request.Quantity <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Dose dan quantity harus lebih dari 0."));
            }

            var prescription = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == entity.PrescriptionId && !x.IsDelete, cancellationToken);

            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DefaultDoseUnitMeasurement)
                .Include(x => x.DispenseUnitMeasurement)
                .FirstAsync(x => x.Id == entity.DrugId && !x.IsDelete, cancellationToken);

            var doseUnit = await ResolveMeasurementAsync(
                request.DoseUnitMeasurementId ?? drug.DefaultDoseUnitMeasurementId,
                cancellationToken);

            var dispenseUnit = await ResolveMeasurementAsync(
                request.DispenseUnitMeasurementId ?? drug.DispenseUnitMeasurementId,
                cancellationToken);

            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                prescription.EncounterId,
                entity.DrugId,
                request.Quantity,
                prescription.PrescriptionDateTime,
                cancellationToken);

            if (!coverage.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    coverage.ErrorMessage ?? "Coverage obat tidak dapat dihitung."));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            entity.TariffId = coverage.TariffId;
            entity.InsuranceTariffId = coverage.InsuranceTariffId;
            entity.InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId;
            entity.Dose = request.Dose;
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
            entity.AdministrationInstruction = NormalizeNullableText(request.AdministrationInstruction);
            entity.DoctorNote = NormalizeNullableText(request.DoctorNote);
            entity.Quantity = coverage.Quantity;
            entity.DispenseUnitMeasurementId = dispenseUnit?.Id;
            entity.DispenseUnitNameSnapshot = dispenseUnit?.MeasurementName;
            entity.DispenseUnitSymbolSnapshot = dispenseUnit?.MeasurementSymbol;
            ApplyCoverage(entity, coverage, drug.IsNeedApproval);
            entity.SortOrder = request.SortOrder;
            entity.IsApproved = false;
            entity.ApprovedAt = null;
            entity.ApprovedByUserId = null;
            entity.ApprovalNote = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                prescription.Id,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var response = ToMutationResponse(entity, aggregate);

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionItem.UpdateItem",
                "Mengubah item resep.",
                response);

            return Ok(ApiResponse<PrescriptionItemMutationResponse>.Ok(
                response,
                "Item resep berhasil diubah."));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionItemMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Approve Prescription Item", Description = "Menyetujui item resep yang membutuhkan approval", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PrescriptionItem", "Update")]
        public async Task<IActionResult> ApproveItem(
            Guid id,
            [FromBody] ApprovePrescriptionItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Item resep tidak ditemukan."));
            }

            if (!entity.IsNeedApproval)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Item resep ini tidak membutuhkan approval."));
            }

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    entity.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            entity.IsApproved = true;
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                entity.PrescriptionId,
                actorUserId,
                now,
                cancellationToken);

            var response = ToMutationResponse(entity, aggregate);

            return Ok(ApiResponse<PrescriptionItemMutationResponse>.Ok(
                response,
                "Item resep berhasil disetujui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Delete", "Delete Prescription Item", Description = "Menghapus item resep", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PrescriptionItem", "Delete")]
        public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionItem>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Item resep tidak ditemukan."));
            }

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    entity.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            await _prescriptionAggregateService.RebuildAsync(
                entity.PrescriptionId,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionItem.DeleteItem",
                "Menghapus item resep.",
                new { entity.Id, entity.PrescriptionId, entity.DrugId });

            return Ok(ApiResponse<object>.Ok(
                null,
                "Item resep berhasil dihapus."));
        }

        private IQueryable<TrxPrescriptionItem> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPrescriptionItem>()
                .Include(x => x.Prescription)
                .Include(x => x.Drug)
                .Include(x => x.Tariff)
                .Include(x => x.InsuranceTariff)
                .Include(x => x.InsuranceCoverageRule)
                .Include(x => x.DoseUnitMeasurement)
                .Include(x => x.DispenseUnitMeasurement)
                .Include(x => x.ApprovedByUser)
                .Where(x => !x.IsDelete);
        }

        private static IQueryable<TrxPrescriptionItem> ApplyFilters(
            IQueryable<TrxPrescriptionItem> query,
            string? search,
            Guid? prescriptionId,
            Guid? drugId,
            bool? isCoveredByInsurance,
            bool? isNeedApproval,
            bool? isApproved,
            bool? isAntibiotic,
            bool? isHighAlert)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(x =>
                    x.DrugCodeSnapshot.ToLower().Contains(keyword) ||
                    x.DrugNameSnapshot.ToLower().Contains(keyword) ||
                    (x.GenericNameSnapshot != null && x.GenericNameSnapshot.ToLower().Contains(keyword)) ||
                    (x.Signa != null && x.Signa.ToLower().Contains(keyword)));
            }

            if (prescriptionId.HasValue && prescriptionId.Value != Guid.Empty)
                query = query.Where(x => x.PrescriptionId == prescriptionId.Value);
            if (drugId.HasValue && drugId.Value != Guid.Empty)
                query = query.Where(x => x.DrugId == drugId.Value);
            if (isCoveredByInsurance.HasValue)
                query = query.Where(x => x.IsCoveredByInsurance == isCoveredByInsurance.Value);
            if (isNeedApproval.HasValue)
                query = query.Where(x => x.IsNeedApproval == isNeedApproval.Value);
            if (isApproved.HasValue)
                query = query.Where(x => x.IsApproved == isApproved.Value);
            if (isAntibiotic.HasValue)
                query = query.Where(x => x.IsAntibioticSnapshot == isAntibiotic.Value);
            if (isHighAlert.HasValue)
                query = query.Where(x => x.IsHighAlertSnapshot == isHighAlert.Value);

            return query;
        }

        private async Task<(bool IsValid, string? ErrorMessage)> ValidateCreateRequestAsync(
            CreatePrescriptionItemRequest request,
            CancellationToken cancellationToken)
        {
            if (request.PrescriptionId == Guid.Empty)
                return (false, "PrescriptionId wajib diisi.");
            if (request.DrugId == Guid.Empty)
                return (false, "DrugId wajib diisi.");
            if (request.Dose <= 0)
                return (false, "Dose harus lebih dari 0.");
            if (request.Quantity <= 0)
                return (false, "Quantity harus lebih dari 0.");

            var prescriptionExists = await _dbContext.Set<TrxPrescription>()
                .AsNoTracking()
                .AnyAsync(x => x.Id == request.PrescriptionId && !x.IsDelete, cancellationToken);
            if (!prescriptionExists)
                return (false, "Resep tidak ditemukan.");

            var drug = await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == request.DrugId &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsPrescribable,
                    cancellationToken);
            if (drug == null)
                return (false, "Obat tidak ditemukan, tidak aktif, atau tidak dapat diresepkan.");

            return (true, null);
        }

        private async Task<MstMeasurement?> ResolveMeasurementAsync(
            Guid? measurementId,
            CancellationToken cancellationToken)
        {
            if (!measurementId.HasValue || measurementId.Value == Guid.Empty)
                return null;

            return await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == measurementId.Value &&
                    !x.IsDelete &&
                    x.IsActive &&
                    x.IsForDrug,
                    cancellationToken);
        }

        private static void ApplyCoverage(
            TrxPrescriptionItem entity,
            InsuranceCoverageResult coverage,
            bool drugNeedApproval)
        {
            entity.TariffId = coverage.TariffId;
            entity.InsuranceTariffId = coverage.InsuranceTariffId;
            entity.InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId;
            entity.Quantity = coverage.Quantity;
            entity.HospitalUnitPrice = coverage.HospitalUnitPrice;
            entity.ContractUnitPrice = coverage.ContractUnitPrice;
            entity.UnitPrice = coverage.UnitPrice;
            entity.TotalPrice = coverage.TotalPrice;
            entity.PricingSource = coverage.PricingSource;
            entity.IsCoverageApplicable = coverage.IsCoverageApplicable;
            entity.IsCoveredByInsurance = coverage.IsCovered;
            entity.CoverageStatus = coverage.CoverageStatus;
            entity.CoveragePercent = coverage.CoveragePercent;
            entity.CoveredAmount = coverage.CoveredAmount;
            entity.PatientPayAmount = coverage.PatientPayAmount;
            entity.CoPaymentAmount = coverage.CoPaymentAmount;
            entity.IsNeedApproval = coverage.IsNeedApproval || drugNeedApproval;
            entity.IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter;
            entity.IsAllowExcessPaymentByPatient = coverage.IsAllowExcessPaymentByPatient;
            entity.CoverageNote = BuildCoverageNote(coverage);
        }

        private static string? BuildCoverageNote(InsuranceCoverageResult coverage)
        {
            var values = new List<string>();
            if (!string.IsNullOrWhiteSpace(coverage.CoverageNote))
                values.Add(coverage.CoverageNote.Trim());
            values.AddRange(coverage.Warnings.Where(x => !string.IsNullOrWhiteSpace(x)));
            return values.Count == 0
                ? null
                : string.Join(" ", values.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private static IQueryable<TrxPrescriptionItem> ApplySorting(
            IQueryable<TrxPrescriptionItem> query,
            string? sortBy,
            string? sortDirection)
        {
            var isDesc = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

            return (sortBy ?? "sortOrder").ToLowerInvariant() switch
            {
                "drugcode" => isDesc ? query.OrderByDescending(x => x.DrugCodeSnapshot) : query.OrderBy(x => x.DrugCodeSnapshot),
                "drugname" => isDesc ? query.OrderByDescending(x => x.DrugNameSnapshot) : query.OrderBy(x => x.DrugNameSnapshot),
                "quantity" => isDesc ? query.OrderByDescending(x => x.Quantity) : query.OrderBy(x => x.Quantity),
                "totalprice" => isDesc ? query.OrderByDescending(x => x.TotalPrice) : query.OrderBy(x => x.TotalPrice),
                "patientpayamount" => isDesc ? query.OrderByDescending(x => x.PatientPayAmount) : query.OrderBy(x => x.PatientPayAmount),
                "createdatetime" => isDesc ? query.OrderByDescending(x => x.CreateDateTime) : query.OrderBy(x => x.CreateDateTime),
                _ => isDesc
                    ? query.OrderByDescending(x => x.SortOrder).ThenByDescending(x => x.CreateDateTime)
                    : query.OrderBy(x => x.SortOrder).ThenBy(x => x.CreateDateTime)
            };
        }

        private static PrescriptionItemResponse ToResponse(TrxPrescriptionItem x)
        {
            return new PrescriptionItemResponse
            {
                Id = x.Id,
                PrescriptionId = x.PrescriptionId,
                PrescriptionNumber = x.Prescription?.PrescriptionNumber ?? string.Empty,
                DrugId = x.DrugId,
                TariffId = x.TariffId,
                InsuranceTariffId = x.InsuranceTariffId,
                InsuranceCoverageRuleId = x.InsuranceCoverageRuleId,
                DrugCodeSnapshot = x.DrugCodeSnapshot,
                DrugNameSnapshot = x.DrugNameSnapshot,
                GenericNameSnapshot = x.GenericNameSnapshot,
                DrugCategoryNameSnapshot = x.DrugCategoryNameSnapshot,
                DrugFormSnapshot = x.DrugFormSnapshot,
                StrengthSnapshot = x.StrengthSnapshot,
                RouteSnapshot = x.RouteSnapshot,
                IsFormularySnapshot = x.IsFormularySnapshot,
                IsGenericSnapshot = x.IsGenericSnapshot,
                IsAntibioticSnapshot = x.IsAntibioticSnapshot,
                IsNarcoticSnapshot = x.IsNarcoticSnapshot,
                IsPsychotropicSnapshot = x.IsPsychotropicSnapshot,
                IsHighAlertSnapshot = x.IsHighAlertSnapshot,
                Dose = x.Dose,
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
                AdministrationInstruction = x.AdministrationInstruction,
                DoctorNote = x.DoctorNote,
                Quantity = x.Quantity,
                DispenseUnitMeasurementId = x.DispenseUnitMeasurementId,
                DispenseUnitNameSnapshot = x.DispenseUnitNameSnapshot,
                DispenseUnitSymbolSnapshot = x.DispenseUnitSymbolSnapshot,
                HospitalUnitPrice = x.HospitalUnitPrice,
                ContractUnitPrice = x.ContractUnitPrice,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice,
                PricingSource = x.PricingSource,
                IsCoverageApplicable = x.IsCoverageApplicable,
                IsCoveredByInsurance = x.IsCoveredByInsurance,
                CoverageStatus = x.CoverageStatus,
                CoveragePercent = x.CoveragePercent,
                CoveredAmount = x.CoveredAmount,
                PatientPayAmount = x.PatientPayAmount,
                CoPaymentAmount = x.CoPaymentAmount,
                IsNeedApproval = x.IsNeedApproval,
                IsApproved = x.IsApproved,
                IsNeedGuaranteeLetter = x.IsNeedGuaranteeLetter,
                IsAllowExcessPaymentByPatient = x.IsAllowExcessPaymentByPatient,
                CoverageNote = x.CoverageNote,
                SortOrder = x.SortOrder,
                IsActive = x.IsActive,
                CreateDateTime = x.CreateDateTime
            };
        }

        private static PrescriptionItemDetailResponse ToDetailResponse(TrxPrescriptionItem x)
        {
            var response = new PrescriptionItemDetailResponse
            {
                ApprovedAt = x.ApprovedAt,
                ApprovedByUserId = x.ApprovedByUserId,
                ApprovedByUserName = x.ApprovedByUser?.DisplayName,
                ApprovalNote = x.ApprovalNote
            };

            CopyBase(x, response);
            return response;
        }

        private static void CopyBase(TrxPrescriptionItem x, PrescriptionItemResponse response)
        {
            var baseResponse = ToResponse(x);
            foreach (var property in typeof(PrescriptionItemResponse).GetProperties())
            {
                property.SetValue(response, property.GetValue(baseResponse));
            }
        }

        private static PrescriptionItemMutationResponse ToMutationResponse(
            TrxPrescriptionItem entity,
            PrescriptionAggregateResult aggregate)
        {
            return new PrescriptionItemMutationResponse
            {
                Id = entity.Id,
                PrescriptionId = entity.PrescriptionId,
                DrugId = entity.DrugId,
                DrugNameSnapshot = entity.DrugNameSnapshot,
                Quantity = entity.Quantity,
                TotalPrice = entity.TotalPrice,
                CoveredAmount = entity.CoveredAmount,
                PatientPayAmount = entity.PatientPayAmount,
                CoverageStatus = entity.CoverageStatus,
                IsNeedApproval = entity.IsNeedApproval,
                IsApproved = entity.IsApproved,
                RegularItemCount = aggregate.RegularItemCount,
                TotalItemCount = aggregate.TotalItemCount,
                PrescriptionTotalPrice = aggregate.TotalPrice,
                PrescriptionCoveredAmount = aggregate.CoveredAmount,
                PrescriptionPatientPayAmount = aggregate.PatientPayAmount,
                PrescriptionNeedApproval = aggregate.IsNeedApproval
            };
        }

        private static (int PageNumber, int PageSize) NormalizePaging(int pageNumber, int pageSize)
        {
            if (pageNumber <= 0) pageNumber = 1;
            if (pageSize <= 0) pageSize = 25;
            if (pageSize > 100) pageSize = 100;
            return (pageNumber, pageSize);
        }

        private static string? NormalizeNullableText(string? value)
        {
            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
