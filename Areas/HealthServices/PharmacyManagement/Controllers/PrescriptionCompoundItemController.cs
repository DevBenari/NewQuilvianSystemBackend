using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/prescription-compound-items")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Prescription Compound Item",
        AreaName = "HealthServices",
        ControllerName = "PrescriptionCompoundItem",
        Description = "Bahan obat pada racikan resep dokter",
        SortOrder = 4
    )]
    [Tags("Health Services / Pharmacy Management / Prescription Compound Item")]
    public class PrescriptionCompoundItemController : ControllerBase
    {
        private const string LogCategory = "HealthServices.Pharmacy";

        private readonly ApplicationDbContext _dbContext;
        private readonly InsuranceCoverageService _insuranceCoverageService;
        private readonly PrescriptionAggregateService _prescriptionAggregateService;
        private readonly LoggerService _loggerService;
        private readonly CompoundCalculationService _compoundCalculationService = new();

        public PrescriptionCompoundItemController(
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

        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<List<PrescriptionCompoundItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Prescription Compound Item", Description = "Melihat bahan racikan resep", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionCompoundItem", "Read")]
        public async Task<IActionResult> GetItems(
            [FromQuery] Guid prescriptionCompoundId,
            CancellationToken cancellationToken = default)
        {
            if (prescriptionCompoundId == Guid.Empty)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "PrescriptionCompoundId wajib diisi."));

            var items = await BuildBaseQuery()
                .AsNoTracking()
                .Where(x => x.PrescriptionCompoundId == prescriptionCompoundId)
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.DrugNameSnapshot)
                .ToListAsync(cancellationToken);

            return Ok(ApiResponse<List<PrescriptionCompoundItemResponse>>.Ok(
                items.Select(ToResponseStatic).ToList(),
                "Bahan racikan resep berhasil diambil."));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundItemDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Prescription Compound Item", Description = "Melihat detail bahan racikan", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("PrescriptionCompoundItem", "Read")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await BuildBaseQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (entity == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Bahan racikan tidak ditemukan."));

            var result = new PrescriptionCompoundItemDetailResponse();
            CopyResponse(ToResponseStatic(entity), result);
            result.ApprovedAt = entity.ApprovedAt;
            result.ApprovedByUserId = entity.ApprovedByUserId;
            result.ApprovedByUserName = entity.ApprovedByUser?.DisplayName;
            result.ApprovalNote = entity.ApprovalNote;

            return Ok(ApiResponse<PrescriptionCompoundItemDetailResponse>.Ok(result, "Detail bahan racikan berhasil diambil."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundItemMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Create", "Create Prescription Compound Item", Description = "Menambahkan bahan ke racikan", AccessType = AccessTypes.Create, SortOrder = 2)]
        [AccessPermission("PrescriptionCompoundItem", "Create")]
        public async Task<IActionResult> CreateItem(
            [FromBody] CreatePrescriptionCompoundItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var compound = await _dbContext.Set<TrxPrescriptionCompound>()
                .Include(x => x.Prescription)
                .FirstOrDefaultAsync(
                    x => x.Id == request.PrescriptionCompoundId && !x.IsDelete,
                    cancellationToken);

            if (compound?.Prescription == null)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Racikan atau resep tidak ditemukan."));

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    compound.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            var duplicate = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                .AnyAsync(x =>
                    x.PrescriptionCompoundId == compound.Id &&
                    x.DrugId == request.DrugId &&
                    !x.IsDelete && !x.IsCancel,
                    cancellationToken);

            if (duplicate)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Obat tersebut sudah ada pada racikan."));

            var drug = await LoadDrugAsync(request.DrugId, cancellationToken);
            if (drug == null || !drug.IsCompoundIngredientAllowed)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Obat tidak ditemukan, tidak aktif, atau tidak diizinkan sebagai bahan racikan."));

            CompoundItemCalculationContext context;
            try
            {
                context = await BuildCalculationContextAsync(
                    compound,
                    drug,
                    request,
                    existingVerifiedSourceQuantity: null,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                compound.Prescription.EncounterId,
                drug.Id,
                context.Calculation.PricingQuantity,
                compound.Prescription.PrescriptionDateTime,
                cancellationToken);

            if (!coverage.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    coverage.ErrorMessage ?? "Coverage bahan racikan tidak dapat dihitung."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            await using var transaction = await _dbContext.Database
                .BeginTransactionAsync(cancellationToken);

            var entity = BuildEntity(
                compound.Id,
                drug,
                request,
                context,
                coverage,
                now,
                actorUserId);

            _dbContext.Set<TrxPrescriptionCompoundItem>().Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                compound.PrescriptionId,
                actorUserId,
                now,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var response = await ToMutationResponseAsync(
                entity,
                aggregate,
                cancellationToken);

            await _loggerService.InfoAsync(
                LogCategory,
                "PrescriptionCompoundItem.CreateItem",
                "Menambahkan bahan racikan dengan kalkulasi sumber dan pricing quantity.",
                response);

            return Ok(ApiResponse<PrescriptionCompoundItemMutationResponse>.Ok(
                response,
                "Bahan racikan berhasil ditambahkan."));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundItemMutationResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Prescription Compound Item", Description = "Mengubah bahan racikan", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("PrescriptionCompoundItem", "Update")]
        public async Task<IActionResult> UpdateItem(
            Guid id,
            [FromBody] UpdatePrescriptionCompoundItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                .Include(x => x.PrescriptionCompound)
                    .ThenInclude(x => x!.Prescription)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity?.PrescriptionCompound?.Prescription == null)
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Bahan racikan tidak ditemukan."));

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(
                    entity.PrescriptionCompound.PrescriptionId,
                    cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            var drug = await LoadDrugAsync(entity.DrugId, cancellationToken);
            if (drug == null)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    "Master obat tidak ditemukan."));

            CompoundItemCalculationContext preliminaryContext;
            CompoundItemCalculationContext context;
            decimal? verifiedSourceQuantity;
            var verificationWasInvalidated = false;

            try
            {
                preliminaryContext = await BuildCalculationContextAsync(
                    entity.PrescriptionCompound,
                    drug,
                    request,
                    existingVerifiedSourceQuantity: null,
                    cancellationToken);

                var canPreserveExistingVerification =
                    request.VerifiedSourceQuantity is null &&
                    entity.VerifiedSourceQuantity is > 0 &&
                    IsExistingVerificationCompatible(
                        entity,
                        request,
                        preliminaryContext);

                verifiedSourceQuantity = request.VerifiedSourceQuantity
                    ?? (canPreserveExistingVerification
                        ? entity.VerifiedSourceQuantity
                        : null);

                verificationWasInvalidated =
                    request.VerifiedSourceQuantity is null &&
                    entity.VerifiedSourceQuantity is > 0 &&
                    !canPreserveExistingVerification;

                context = verifiedSourceQuantity is > 0
                    ? await BuildCalculationContextAsync(
                        entity.PrescriptionCompound,
                        drug,
                        request,
                        verifiedSourceQuantity,
                        cancellationToken)
                    : preliminaryContext;
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    ex.Message));
            }

            var coverage = await _insuranceCoverageService.ResolveDrugAsync(
                entity.PrescriptionCompound.Prescription.EncounterId,
                entity.DrugId,
                context.Calculation.PricingQuantity,
                entity.PrescriptionCompound.Prescription.PrescriptionDateTime,
                cancellationToken);

            if (!coverage.IsValid)
                return BadRequest(ApiResponse<object>.Fail(
                    StatusCodes.Status400BadRequest,
                    coverage.ErrorMessage ?? "Coverage bahan racikan tidak dapat dihitung."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();

            ApplyCalculation(
                entity,
                request,
                context,
                verifiedSourceQuantity);

            if (verificationWasInvalidated)
            {
                entity.CalculationStatus = "PharmacyReverificationRequired";
                entity.CalculationNote = AppendNote(
                    entity.CalculationNote,
                    "Verifikasi quantity farmasi dibatalkan karena formula klinis berubah.");
            }

            ApplyCoverage(entity, coverage);
            entity.IngredientInstruction = NormalizeNullableText(
                request.IngredientInstruction);
            entity.SortOrder = request.SortOrder;
            entity.IsApproved = false;
            entity.ApprovedAt = null;
            entity.ApprovedByUserId = null;
            entity.ApprovalNote = null;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var aggregate = await _prescriptionAggregateService.RebuildAsync(
                entity.PrescriptionCompound.PrescriptionId,
                actorUserId,
                now,
                cancellationToken);

            var response = await ToMutationResponseAsync(
                entity,
                aggregate,
                cancellationToken);

            return Ok(ApiResponse<PrescriptionCompoundItemMutationResponse>.Ok(
                response,
                "Bahan racikan berhasil diubah."));
        }

        [HttpPatch("{id:guid}/approve")]
        [ProducesResponseType(typeof(ApiResponse<PrescriptionCompoundItemMutationResponse>), StatusCodes.Status200OK)]
        [AccessAction("Update", "Approve Prescription Compound Item", Description = "Menyetujui bahan racikan", AccessType = AccessTypes.Update, SortOrder = 4)]
        [AccessPermission("PrescriptionCompoundItem", "Update")]
        public async Task<IActionResult> ApproveItem(
            Guid id,
            [FromBody] ApprovePrescriptionCompoundItemRequest request,
            CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                .Include(x => x.PrescriptionCompound)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity?.PrescriptionCompound == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Bahan racikan tidak ditemukan."));

            if (!entity.IsNeedApproval)
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Bahan racikan ini tidak membutuhkan approval."));

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsApproved = true;
            entity.ApprovedAt = now;
            entity.ApprovedByUserId = actorUserId;
            entity.ApprovalNote = NormalizeNullableText(request.ApprovalNote);
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            var aggregate = await _prescriptionAggregateService.RebuildAsync(entity.PrescriptionCompound.PrescriptionId, actorUserId, now, cancellationToken);
            var response = await ToMutationResponseAsync(entity, aggregate, cancellationToken);

            return Ok(ApiResponse<PrescriptionCompoundItemMutationResponse>.Ok(response, "Bahan racikan berhasil disetujui."));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
        [AccessAction("Delete", "Delete Prescription Compound Item", Description = "Menghapus bahan racikan", AccessType = AccessTypes.Delete, SortOrder = 5)]
        [AccessPermission("PrescriptionCompoundItem", "Delete")]
        public async Task<IActionResult> DeleteItem(Guid id, CancellationToken cancellationToken = default)
        {
            var entity = await _dbContext.Set<TrxPrescriptionCompoundItem>()
                .Include(x => x.PrescriptionCompound)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDelete, cancellationToken);

            if (entity?.PrescriptionCompound == null)
                return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Bahan racikan tidak ditemukan."));

            try
            {
                await _prescriptionAggregateService.EnsureEditableAsync(entity.PrescriptionCompound.PrescriptionId, cancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
            }

            var now = DateTime.UtcNow;
            var actorUserId = GetCurrentUserId();
            entity.IsDelete = true;
            entity.IsActive = false;
            entity.DeleteDateTime = now;
            entity.DeleteBy = actorUserId;
            entity.UpdateDateTime = now;
            entity.UpdateBy = actorUserId;

            await _dbContext.SaveChangesAsync(cancellationToken);
            await _prescriptionAggregateService.RebuildAsync(entity.PrescriptionCompound.PrescriptionId, actorUserId, now, cancellationToken);

            return Ok(ApiResponse<object>.Ok(null, "Bahan racikan berhasil dihapus."));
        }

        private IQueryable<TrxPrescriptionCompoundItem> BuildBaseQuery()
        {
            return _dbContext.Set<TrxPrescriptionCompoundItem>()
                .Include(x => x.PrescriptionCompound)
                .Include(x => x.ApprovedByUser)
                .Where(x => !x.IsDelete && !x.IsCancel);
        }

        private async Task<MstDrug?> LoadDrugAsync(
            Guid drugId,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<MstDrug>()
                .AsNoTracking()
                .Include(x => x.DrugCategory)
                .Include(x => x.StrengthMeasurement)
                .Include(x => x.BaseUnitMeasurement)
                .Include(x => x.DispenseUnitMeasurement)
                .Include(x => x.DefaultDoseUnitMeasurement)
                .FirstOrDefaultAsync(
                    x => x.Id == drugId &&
                         x.IsActive &&
                         !x.IsDelete &&
                         x.IsPrescribable,
                    cancellationToken);
        }

        private async Task<MstMeasurement?> ResolveMeasurementAsync(
            Guid? id,
            CancellationToken cancellationToken)
        {
            if (!id.HasValue || id.Value == Guid.Empty)
                return null;

            return await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == id.Value && x.IsActive && !x.IsDelete,
                    cancellationToken)
                ?? throw new InvalidOperationException(
                    "Satuan tidak ditemukan atau tidak aktif.");
        }

        private async Task<MstMeasurement?> ResolveMeasurementAsync(
            Guid? id,
            string? nameOrSymbol,
            CancellationToken cancellationToken)
        {
            var byId = await ResolveMeasurementAsync(id, cancellationToken);
            if (byId != null) return byId;

            var token = CompoundCalculationService.NormalizeUnitToken(nameOrSymbol);
            if (string.IsNullOrWhiteSpace(token)) return null;

            var candidates = await _dbContext.Set<MstMeasurement>()
                .AsNoTracking()
                .Where(x => x.IsActive && !x.IsDelete && x.IsForDrug)
                .ToListAsync(cancellationToken);

            return candidates.FirstOrDefault(x =>
                string.Equals(
                    CompoundCalculationService.NormalizeUnitToken(x.MeasurementSymbol),
                    token,
                    StringComparison.Ordinal) ||
                string.Equals(
                    CompoundCalculationService.NormalizeUnitToken(x.MeasurementName),
                    token,
                    StringComparison.Ordinal));
        }

        private async Task<CompoundItemCalculationContext> BuildCalculationContextAsync(
            TrxPrescriptionCompound compound,
            MstDrug drug,
            PrescriptionCompoundItemMutationRequestBase request,
            decimal? existingVerifiedSourceQuantity,
            CancellationToken cancellationToken)
        {
            var parsedStrength = _compoundCalculationService.ParseStrength(drug.Strength);

            var quantityUnitId = request.QuantityUnitMeasurementId
                ?? drug.BaseUnitMeasurementId
                ?? drug.DispenseUnitMeasurementId
                ?? drug.DefaultDoseUnitMeasurementId;
            var quantityUnit = await ResolveMeasurementAsync(
                quantityUnitId,
                cancellationToken);

            if (quantityUnit == null)
                throw new InvalidOperationException(
                    $"Satuan bahan racikan untuk obat {drug.DrugName} belum dikonfigurasi.");

            var targetUnit = await ResolveMeasurementAsync(
                request.TargetUnitMeasurementId ?? drug.StrengthMeasurementId,
                request.TargetUnitName ?? parsedStrength.StrengthUnitToken,
                cancellationToken);

            var sourceStrengthUnit = await ResolveMeasurementAsync(
                request.SourceStrengthMeasurementId ?? drug.StrengthMeasurementId,
                request.SourceStrengthUnitName ?? parsedStrength.StrengthUnitToken,
                cancellationToken);

            var sourceContentMeasurementId = request.SourceContentUnitMeasurementId;
            if (!sourceContentMeasurementId.HasValue &&
                string.IsNullOrWhiteSpace(parsedStrength.SourceContentUnitToken) &&
                string.IsNullOrWhiteSpace(request.SourceContentUnitName))
            {
                sourceContentMeasurementId = quantityUnit.Id;
            }

            var sourceContentUnit = await ResolveMeasurementAsync(
                sourceContentMeasurementId,
                request.SourceContentUnitName ?? parsedStrength.SourceContentUnitToken,
                cancellationToken) ?? quantityUnit;

            var sourceStrengthValue = request.SourceStrengthValue
                ?? drug.StrengthValue
                ?? parsedStrength.StrengthValue;
            var sourceContentQuantity = request.SourceContentQuantity
                ?? parsedStrength.SourceContentQuantity;

            var calculation = _compoundCalculationService.Calculate(
                new CompoundCalculationRequest
                {
                    CompoundCalculationMode = compound.CalculationMode,
                    IngredientCalculationMode = request.CalculationMode,
                    IngredientRole = request.IngredientRole,
                    TotalPackage = compound.TotalPackage,
                    FinalQuantity = compound.FinalQuantity,
                    FinalQuantityUnit = CompoundUnitDescriptor.Create(
                        compound.FinalQuantityMeasurementId,
                        compound.FinalQuantityUnitNameSnapshot,
                        compound.FinalQuantityUnitSymbolSnapshot),
                    LegacyAmountPerPackage = request.AmountPerPackage,
                    LegacyTotalQuantity = request.TotalQuantity,
                    TargetValue = request.TargetValue,
                    TargetUnit = ToDescriptor(targetUnit),
                    TargetConcentrationUnit = request.TargetConcentrationUnit,
                    SourceStrengthValue = sourceStrengthValue,
                    SourceStrengthUnit = ToDescriptor(sourceStrengthUnit),
                    SourceContentQuantity = sourceContentQuantity,
                    SourceContentUnit = ToDescriptor(sourceContentUnit),
                    VerifiedSourceQuantity = request.VerifiedSourceQuantity
                        ?? existingVerifiedSourceQuantity,
                    AllowFractionalSource = drug.IsAllowFractionalDispense,
                    IsQuantitySufficientToFinal =
                        request.IsQuantitySufficientToFinal,
                    RequiresManualStrengthVerification =
                        parsedStrength.RequiresManualVerification
                });

            if (!calculation.IsValid)
                throw new InvalidOperationException(
                    calculation.ErrorMessage ??
                    "Kalkulasi bahan racikan tidak valid.");

            return new CompoundItemCalculationContext
            {
                QuantityUnit = quantityUnit,
                TargetUnit = targetUnit,
                SourceStrengthUnit = sourceStrengthUnit,
                SourceContentUnit = sourceContentUnit,
                SourceStrengthValue = sourceStrengthValue,
                SourceContentQuantity = sourceContentQuantity,
                ParsedStrengthWarning = parsedStrength.Warning,
                Calculation = calculation
            };
        }

        private static CompoundUnitDescriptor? ToDescriptor(
            MstMeasurement? measurement)
            => measurement == null
                ? null
                : CompoundUnitDescriptor.Create(
                    measurement.Id,
                    measurement.MeasurementName,
                    measurement.MeasurementSymbol);

        private static TrxPrescriptionCompoundItem BuildEntity(
            Guid compoundId,
            MstDrug drug,
            PrescriptionCompoundItemMutationRequestBase request,
            CompoundItemCalculationContext context,
            InsuranceCoverageResult coverage,
            DateTime now,
            Guid actorUserId)
        {
            var entity = new TrxPrescriptionCompoundItem
            {
                Id = Guid.NewGuid(),
                PrescriptionCompoundId = compoundId,
                DrugId = drug.Id,
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
                IsAllowFractionalSourceSnapshot = drug.IsAllowFractionalDispense,
                IngredientInstruction = NormalizeNullableText(
                    request.IngredientInstruction),
                SortOrder = request.SortOrder,
                IsApproved = false,
                IsActive = true,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };

            ApplyCalculation(
                entity,
                request,
                context,
                request.VerifiedSourceQuantity);
            ApplyCoverage(entity, coverage);
            return entity;
        }

        private static void ApplyCalculation(
            TrxPrescriptionCompoundItem entity,
            PrescriptionCompoundItemMutationRequestBase request,
            CompoundItemCalculationContext context,
            decimal? verifiedSourceQuantity)
        {
            var result = context.Calculation;
            entity.CalculationMode = result.IngredientCalculationMode;
            entity.IngredientRole = request.IngredientRole;
            entity.TargetValue = request.TargetValue;
            entity.TargetUnitMeasurementId = context.TargetUnit?.Id;
            entity.TargetUnitNameSnapshot = context.TargetUnit?.MeasurementName
                ?? NormalizeNullableText(request.TargetUnitName);
            entity.TargetUnitSymbolSnapshot = context.TargetUnit?.MeasurementSymbol;
            entity.TargetConcentrationUnit = NormalizeNullableText(
                request.TargetConcentrationUnit);
            entity.CalculatedActiveAmount = result.CalculatedActiveAmount;
            entity.CalculatedActiveUnitMeasurementId =
                result.CalculatedActiveUnit?.Id;
            entity.CalculatedActiveUnitNameSnapshot =
                result.CalculatedActiveUnit?.Name;
            entity.CalculatedActiveUnitSymbolSnapshot =
                result.CalculatedActiveUnit?.Symbol;
            entity.SourceStrengthValue = context.SourceStrengthValue;
            entity.SourceStrengthMeasurementId = context.SourceStrengthUnit?.Id;
            entity.SourceStrengthUnitNameSnapshot =
                context.SourceStrengthUnit?.MeasurementName
                ?? NormalizeNullableText(request.SourceStrengthUnitName);
            entity.SourceStrengthUnitSymbolSnapshot =
                context.SourceStrengthUnit?.MeasurementSymbol;
            entity.SourceContentQuantity = context.SourceContentQuantity;
            entity.SourceContentUnitMeasurementId = context.SourceContentUnit?.Id;
            entity.SourceContentUnitNameSnapshot =
                context.SourceContentUnit?.MeasurementName
                ?? NormalizeNullableText(request.SourceContentUnitName);
            entity.SourceContentUnitSymbolSnapshot =
                context.SourceContentUnit?.MeasurementSymbol;
            entity.TheoreticalSourceQuantity = result.TheoreticalSourceQuantity;
            entity.VerifiedSourceQuantity = verifiedSourceQuantity;
            entity.PricingQuantity = result.PricingQuantity;
            entity.IsQuantitySufficientToFinal =
                request.IsQuantitySufficientToFinal;
            entity.CalculationStatus = result.CalculationStatus;
            entity.CalculationNote = BuildCalculationNote(
                result.CalculationNote,
                context.ParsedStrengthWarning,
                result.Warnings);
            entity.AmountPerPackage = result.AmountPerPackage;
            entity.TotalQuantity = result.TotalQuantity;
            entity.QuantityUnitMeasurementId = context.QuantityUnit.Id;
            entity.QuantityUnitNameSnapshot = context.QuantityUnit.MeasurementName;
            entity.QuantityUnitSymbolSnapshot = context.QuantityUnit.MeasurementSymbol;
        }

        private static string? BuildCalculationNote(
            string? calculationNote,
            string? parserWarning,
            IEnumerable<string>? warnings)
        {
            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(calculationNote))
                notes.Add(calculationNote.Trim());
            if (!string.IsNullOrWhiteSpace(parserWarning))
                notes.Add(parserWarning.Trim());
            if (warnings != null)
                notes.AddRange(warnings.Where(x => !string.IsNullOrWhiteSpace(x)));

            return notes.Count == 0
                ? null
                : string.Join("; ", notes.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        private sealed class CompoundItemCalculationContext
        {
            public MstMeasurement QuantityUnit { get; set; } = null!;
            public MstMeasurement? TargetUnit { get; set; }
            public MstMeasurement? SourceStrengthUnit { get; set; }
            public MstMeasurement? SourceContentUnit { get; set; }
            public decimal? SourceStrengthValue { get; set; }
            public decimal SourceContentQuantity { get; set; } = 1m;
            public string? ParsedStrengthWarning { get; set; }
            public CompoundCalculationResult Calculation { get; set; } = null!;
        }

        private static bool IsExistingVerificationCompatible(
            TrxPrescriptionCompoundItem existing,
            PrescriptionCompoundItemMutationRequestBase request,
            CompoundItemCalculationContext preliminaryContext)
        {
            var calculation = preliminaryContext.Calculation;

            return
                existing.CalculationMode == calculation.IngredientCalculationMode &&
                existing.IngredientRole == request.IngredientRole &&
                DecimalEquals(existing.TargetValue, request.TargetValue) &&
                MeasurementEquals(
                    existing.TargetUnitMeasurementId,
                    existing.TargetUnitNameSnapshot,
                    existing.TargetUnitSymbolSnapshot,
                    preliminaryContext.TargetUnit) &&
                DecimalEquals(
                    existing.SourceStrengthValue,
                    preliminaryContext.SourceStrengthValue) &&
                MeasurementEquals(
                    existing.SourceStrengthMeasurementId,
                    existing.SourceStrengthUnitNameSnapshot,
                    existing.SourceStrengthUnitSymbolSnapshot,
                    preliminaryContext.SourceStrengthUnit) &&
                DecimalEquals(
                    existing.SourceContentQuantity,
                    preliminaryContext.SourceContentQuantity) &&
                MeasurementEquals(
                    existing.SourceContentUnitMeasurementId,
                    existing.SourceContentUnitNameSnapshot,
                    existing.SourceContentUnitSymbolSnapshot,
                    preliminaryContext.SourceContentUnit) &&
                existing.QuantityUnitMeasurementId ==
                    preliminaryContext.QuantityUnit.Id &&
                DecimalEquals(
                    existing.TheoreticalSourceQuantity,
                    calculation.TheoreticalSourceQuantity) &&
                DecimalEquals(
                    existing.AmountPerPackage,
                    calculation.AmountPerPackage) &&
                existing.IsQuantitySufficientToFinal ==
                    request.IsQuantitySufficientToFinal;
        }

        private static bool MeasurementEquals(
            Guid? existingId,
            string? existingName,
            string? existingSymbol,
            MstMeasurement? current)
        {
            if (current == null)
            {
                return !existingId.HasValue &&
                    string.IsNullOrWhiteSpace(existingName) &&
                    string.IsNullOrWhiteSpace(existingSymbol);
            }

            if (existingId.HasValue && existingId.Value == current.Id)
                return true;

            var existingToken = CompoundCalculationService.NormalizeUnitToken(
                existingSymbol ?? existingName);
            var currentToken = CompoundCalculationService.NormalizeUnitToken(
                current.MeasurementSymbol ?? current.MeasurementName);

            return !string.IsNullOrWhiteSpace(existingToken) &&
                string.Equals(existingToken, currentToken, StringComparison.Ordinal);
        }

        private static bool DecimalEquals(decimal? left, decimal? right)
        {
            if (!left.HasValue && !right.HasValue) return true;
            if (!left.HasValue || !right.HasValue) return false;
            return Math.Abs(left.Value - right.Value) <= 0.0001m;
        }

        private static string AppendNote(string? current, string note)
        {
            if (string.IsNullOrWhiteSpace(current)) return note;
            return current.Contains(note, StringComparison.OrdinalIgnoreCase)
                ? current
                : $"{current}; {note}";
        }

        private static void ApplyCoverage(TrxPrescriptionCompoundItem entity, InsuranceCoverageResult coverage)
        {
            entity.TariffId = coverage.TariffId;
            entity.InsuranceTariffId = coverage.InsuranceTariffId;
            entity.InsuranceCoverageRuleId = coverage.InsuranceCoverageRuleId;
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
            entity.IsNeedApproval = coverage.IsNeedApproval;
            entity.IsNeedGuaranteeLetter = coverage.IsNeedGuaranteeLetter;
            entity.IsAllowExcessPaymentByPatient = coverage.IsAllowExcessPaymentByPatient;
            entity.CoverageNote = BuildCoverageNote(coverage);
        }

        private static string? BuildCoverageNote(InsuranceCoverageResult coverage)
        {
            var notes = new List<string>();
            if (!string.IsNullOrWhiteSpace(coverage.CoverageNote)) notes.Add(coverage.CoverageNote);
            notes.AddRange(coverage.Warnings.Where(x => !string.IsNullOrWhiteSpace(x)));
            return notes.Count == 0 ? null : string.Join("; ", notes.Distinct(StringComparer.OrdinalIgnoreCase));
        }

        public static PrescriptionCompoundItemResponse ToResponseStatic(TrxPrescriptionCompoundItem x)
        {
            return new PrescriptionCompoundItemResponse
            {
                Id = x.Id,
                PrescriptionCompoundId = x.PrescriptionCompoundId,
                CompoundName = x.PrescriptionCompound?.CompoundName ?? string.Empty,
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
                IsAllowFractionalSource = x.IsAllowFractionalSourceSnapshot,
                CalculationMode = x.CalculationMode,
                CalculationModeName = x.CalculationMode.ToString(),
                IngredientRole = x.IngredientRole,
                IngredientRoleName = x.IngredientRole.ToString(),
                TargetValue = x.TargetValue,
                TargetUnitMeasurementId = x.TargetUnitMeasurementId,
                TargetUnitNameSnapshot = x.TargetUnitNameSnapshot,
                TargetUnitSymbolSnapshot = x.TargetUnitSymbolSnapshot,
                TargetConcentrationUnit = x.TargetConcentrationUnit,
                CalculatedActiveAmount = x.CalculatedActiveAmount,
                CalculatedActiveUnitMeasurementId = x.CalculatedActiveUnitMeasurementId,
                CalculatedActiveUnitNameSnapshot = x.CalculatedActiveUnitNameSnapshot,
                CalculatedActiveUnitSymbolSnapshot = x.CalculatedActiveUnitSymbolSnapshot,
                SourceStrengthValue = x.SourceStrengthValue,
                SourceStrengthMeasurementId = x.SourceStrengthMeasurementId,
                SourceStrengthUnitNameSnapshot = x.SourceStrengthUnitNameSnapshot,
                SourceStrengthUnitSymbolSnapshot = x.SourceStrengthUnitSymbolSnapshot,
                SourceContentQuantity = x.SourceContentQuantity,
                SourceContentUnitMeasurementId = x.SourceContentUnitMeasurementId,
                SourceContentUnitNameSnapshot = x.SourceContentUnitNameSnapshot,
                SourceContentUnitSymbolSnapshot = x.SourceContentUnitSymbolSnapshot,
                TheoreticalSourceQuantity = x.TheoreticalSourceQuantity,
                VerifiedSourceQuantity = x.VerifiedSourceQuantity,
                PricingQuantity = x.PricingQuantity,
                IsQuantitySufficientToFinal = x.IsQuantitySufficientToFinal,
                CalculationStatus = x.CalculationStatus,
                CalculationNote = x.CalculationNote,
                AmountPerPackage = x.AmountPerPackage,
                TotalQuantity = x.TotalQuantity,
                QuantityUnitMeasurementId = x.QuantityUnitMeasurementId,
                QuantityUnitNameSnapshot = x.QuantityUnitNameSnapshot,
                QuantityUnitSymbolSnapshot = x.QuantityUnitSymbolSnapshot,
                IngredientInstruction = x.IngredientInstruction,
                HospitalUnitPrice = x.HospitalUnitPrice,
                ContractUnitPrice = x.ContractUnitPrice,
                UnitPrice = x.UnitPrice,
                TotalPrice = x.TotalPrice,
                HospitalTotalPrice = Math.Round(
                    x.HospitalUnitPrice * x.PricingQuantity,
                    2,
                    MidpointRounding.AwayFromZero),
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

        private async Task<PrescriptionCompoundItemMutationResponse> ToMutationResponseAsync(
            TrxPrescriptionCompoundItem entity,
            PrescriptionAggregateResult aggregate,
            CancellationToken cancellationToken)
        {
            var compound = await _dbContext.Set<TrxPrescriptionCompound>()
                .AsNoTracking()
                .FirstAsync(x => x.Id == entity.PrescriptionCompoundId, cancellationToken);

            var result = new PrescriptionCompoundItemMutationResponse
            {
                IngredientCount = compound.IngredientCount,
                CompoundTotalPrice = compound.TotalPrice,
                CompoundCoveredAmount = compound.CoveredAmount,
                CompoundPatientPayAmount = compound.PatientPayAmount,
                RegularItemCount = aggregate.RegularItemCount,
                CompoundCount = aggregate.CompoundCount,
                CompoundIngredientCount = aggregate.CompoundIngredientCount,
                TotalItemCount = aggregate.TotalItemCount,
                PrescriptionTotalPrice = aggregate.TotalPrice,
                PrescriptionCoveredAmount = aggregate.CoveredAmount,
                PrescriptionPatientPayAmount = aggregate.PatientPayAmount
            };
            CopyResponse(ToResponseStatic(entity), result);
            return result;
        }

        private static void CopyResponse(PrescriptionCompoundItemResponse source, PrescriptionCompoundItemResponse target)
        {
            foreach (var property in typeof(PrescriptionCompoundItemResponse).GetProperties())
                property.SetValue(target, property.GetValue(source));
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
