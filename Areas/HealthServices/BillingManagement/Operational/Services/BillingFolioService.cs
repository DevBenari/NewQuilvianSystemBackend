using System.Data;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Services
{
    public enum BillingServiceResultKind
    {
        Success,
        Validation,
        NotFound,
        Conflict
    }

    public sealed class BillingServiceResult<T>
    {
        private BillingServiceResult(
            BillingServiceResultKind kind,
            T? value,
            string? errorCode,
            string? errorMessage,
            int? appliedVersion)
        {
            Kind = kind;
            Value = value;
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            AppliedVersion = appliedVersion;
        }

        public BillingServiceResultKind Kind { get; }

        public T? Value { get; }

        public string? ErrorCode { get; }

        public string? ErrorMessage { get; }

        public int? AppliedVersion { get; }

        public static BillingServiceResult<T> Success(T value) =>
            new(BillingServiceResultKind.Success, value, null, null, null);

        public static BillingServiceResult<T> Validation(string code, string message) =>
            new(BillingServiceResultKind.Validation, default, code, message, null);

        public static BillingServiceResult<T> NotFound(string code, string message) =>
            new(BillingServiceResultKind.NotFound, default, code, message, null);

        public static BillingServiceResult<T> Conflict(
            string code,
            string message,
            int? appliedVersion = null) =>
            new(BillingServiceResultKind.Conflict, default, code, message, appliedVersion);
    }

    public class BillingFolioService
    {
        public const string InternalConsumer = "BillingInternalApi";
        public const string RecognizeMilestoneOperation = "RecognizeMilestone";
        public const string InternalTestSourceContext = BillingSourceContract.InternalTestSourceContext;
        public const string InternalTestEffectType = BillingSourceContract.InternalTestEffectType;

        /// <summary>
        /// Consumer yang dipakai producer clinical fact internal (<c>RJ-BIL-BE-002</c>).
        /// Dibedakan dari <see cref="InternalConsumer"/> agar idempotency scope endpoint HTTP
        /// dan producer in-process tidak saling menimpa.
        /// </summary>
        public const string ClinicalFactConsumer = "ClinicalFactProducer";

        private const int MaxPersistenceAttempts = 3;
        private const string ReviewRequiredCode = "BIL_CALCULATION_REVIEW_REQUIRED";

        private readonly ApplicationDbContext _dbContext;

        public BillingFolioService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<BillingFolioDetailResponse?> GetByIdAsync(
            Guid folioId,
            CancellationToken cancellationToken = default)
        {
            if (folioId == Guid.Empty)
                return null;

            var folio = await BuildFolioQuery()
                .FirstOrDefaultAsync(x => x.Id == folioId, cancellationToken);

            return folio == null ? null : ToDetailResponse(folio);
        }

        public async Task<BillingFolioDetailResponse?> GetByEncounterAsync(
            Guid encounterId,
            CancellationToken cancellationToken = default)
        {
            if (encounterId == Guid.Empty)
                return null;

            var folio = await BuildFolioQuery()
                .FirstOrDefaultAsync(x => x.EncounterId == encounterId, cancellationToken);

            return folio == null ? null : ToDetailResponse(folio);
        }

        public async Task<BillingServiceResult<RecognizeBillingMilestoneResponse>> RecognizeMilestoneAsync(
            RecognizeBillingMilestoneRequest request,
            Guid actorUserId,
            string consumer = InternalConsumer,
            string operationType = RecognizeMilestoneOperation,
            CancellationToken cancellationToken = default)
        {
            var validation = ValidateRequest(request, actorUserId, consumer, operationType);
            if (validation != null)
                return BillingServiceResult<RecognizeBillingMilestoneResponse>.Validation(
                    validation.Value.Code,
                    validation.Value.Message);

            var normalizedRequest = NormalizeRequest(request);
            var fingerprint = BuildFingerprint(normalizedRequest, consumer, operationType);

            var existing = await FindProcessingEffectAsync(
                consumer,
                operationType,
                normalizedRequest.IdempotencyKey,
                cancellationToken);

            if (existing != null)
                return BuildExistingEffectResult(existing, fingerprint);

            for (var attempt = 1; attempt <= MaxPersistenceAttempts; attempt++)
            {
                await using var transaction = await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);

                try
                {
                    existing = await _dbContext.Set<BilProcessingEffect>()
                        .FirstOrDefaultAsync(
                            x => x.Consumer == consumer &&
                                 x.OperationType == operationType &&
                                 x.IdempotencyKey == normalizedRequest.IdempotencyKey,
                            cancellationToken);

                    if (existing != null)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return BuildExistingEffectResult(existing, fingerprint);
                    }

                    var encounterExists = await _dbContext.Set<TrxPatientEncounter>()
                        .AsNoTracking()
                        .AnyAsync(
                            x => x.Id == normalizedRequest.EncounterId && !x.IsDelete,
                            cancellationToken);

                    if (!encounterExists)
                    {
                        await transaction.RollbackAsync(cancellationToken);
                        return BillingServiceResult<RecognizeBillingMilestoneResponse>.NotFound(
                            "BIL_ENCOUNTER_NOT_FOUND",
                            "Encounter tidak ditemukan.");
                    }

                    var appliedEffect = await FindLatestAppliedEffectAsync(
                        normalizedRequest.SourceContext,
                        normalizedRequest.MilestoneFactId,
                        normalizedRequest.EffectType,
                        cancellationToken);

                    if (appliedEffect != null)
                    {
                        if (normalizedRequest.MilestoneFactVersion < appliedEffect.MilestoneFactVersion)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return BuildVersionConflictResult(appliedEffect.MilestoneFactVersion);
                        }

                        if (normalizedRequest.MilestoneFactVersion == appliedEffect.MilestoneFactVersion)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            if (!string.Equals(
                                    appliedEffect.RequestFingerprint,
                                    fingerprint,
                                    StringComparison.Ordinal))
                            {
                                return BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                                    "BIL_VERSION_CONFLICT",
                                    "Revisi milestone yang sama telah diproses dengan input material yang berbeda.",
                                    appliedEffect.MilestoneFactVersion);
                            }

                            return BillingServiceResult<RecognizeBillingMilestoneResponse>.Success(
                                ToMilestoneResponse(appliedEffect, isReplay: true));
                        }

                        var priorCharge = appliedEffect.ChargeLineId.HasValue
                            ? await _dbContext.Set<BilChargeLine>()
                                .AsNoTracking()
                                .FirstOrDefaultAsync(
                                    x => x.Id == appliedEffect.ChargeLineId.Value && !x.IsDelete,
                                    cancellationToken)
                            : null;

                        if (priorCharge == null || !appliedEffect.FolioId.HasValue)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                                "BIL_OUTCOME_UNKNOWN",
                                "Referensi finansial revisi sebelumnya tidak lengkap dan memerlukan rekonsiliasi.",
                                appliedEffect.MilestoneFactVersion);
                        }

                        var revisionFolio = await _dbContext.Set<BilFolio>()
                            .FirstOrDefaultAsync(
                                x => x.Id == appliedEffect.FolioId.Value && !x.IsDelete,
                                cancellationToken);

                        if (revisionFolio == null)
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            return BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                                "BIL_OUTCOME_UNKNOWN",
                                "Folio revisi sebelumnya tidak tersedia dan memerlukan rekonsiliasi.",
                                appliedEffect.MilestoneFactVersion);
                        }

                        var detectedAt = DateTime.UtcNow;
                        if (revisionFolio.Status != BillingFolioStatus.ReviewRequired)
                        {
                            revisionFolio.Status = BillingFolioStatus.ReviewRequired;
                            revisionFolio.Version += 1;
                            revisionFolio.UpdateDateTime = detectedAt;
                            revisionFolio.UpdateBy = actorUserId;
                        }

                        var newerRevisionEffect = CreateProcessingEffect(
                            normalizedRequest,
                            actorUserId,
                            consumer,
                            operationType,
                            fingerprint,
                            revisionFolio.Id,
                            priorCharge.Id,
                            BillingChargeCalculationStatus.PendingFinancialReview,
                            ReviewRequiredCode,
                            "Revisi fakta klinis yang lebih baru memerlukan evaluasi koreksi finansial.");

                        _dbContext.Set<BilProcessingEffect>().Add(newerRevisionEffect);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        return BillingServiceResult<RecognizeBillingMilestoneResponse>.Success(
                            ToMilestoneResponse(newerRevisionEffect, isReplay: false));
                    }

                    var duplicateCharge = await _dbContext.Set<BilChargeLine>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(
                            x => x.SourceContext == normalizedRequest.SourceContext &&
                                 x.SourceAggregateId == normalizedRequest.SourceAggregateId &&
                                 x.SourceItemId == normalizedRequest.SourceItemId &&
                                 x.MilestoneFactId == normalizedRequest.MilestoneFactId &&
                                 x.EffectType == normalizedRequest.EffectType,
                            cancellationToken);

                    if (duplicateCharge != null)
                    {
                        var duplicateEffect = CreateProcessingEffect(
                            normalizedRequest,
                            actorUserId,
                            consumer,
                            operationType,
                            fingerprint,
                            duplicateCharge.FolioId,
                            duplicateCharge.Id,
                            duplicateCharge.CalculationStatus);

                        _dbContext.Set<BilProcessingEffect>().Add(duplicateEffect);
                        await _dbContext.SaveChangesAsync(cancellationToken);
                        await transaction.CommitAsync(cancellationToken);

                        return BillingServiceResult<RecognizeBillingMilestoneResponse>.Success(
                            ToMilestoneResponse(duplicateEffect, isReplay: false));
                    }

                    var folio = await _dbContext.Set<BilFolio>()
                        .FirstOrDefaultAsync(
                            x => x.EncounterId == normalizedRequest.EncounterId && !x.IsDelete,
                            cancellationToken);

                    var now = DateTime.UtcNow;
                    if (folio == null)
                    {
                        folio = new BilFolio
                        {
                            Id = Guid.NewGuid(),
                            EncounterId = normalizedRequest.EncounterId,
                            Status = BillingFolioStatus.ReviewRequired,
                            Version = 1,
                            IsActive = true,
                            CreateDateTime = now,
                            CreateBy = actorUserId,
                            IsDelete = false,
                            IsCancel = false
                        };
                        _dbContext.Set<BilFolio>().Add(folio);
                    }
                    else if (folio.Status == BillingFolioStatus.Open)
                    {
                        folio.Status = BillingFolioStatus.ReviewRequired;
                        folio.Version += 1;
                        folio.UpdateDateTime = now;
                        folio.UpdateBy = actorUserId;
                    }

                    var chargeLine = new BilChargeLine
                    {
                        Id = Guid.NewGuid(),
                        FolioId = folio.Id,
                        SourceContext = normalizedRequest.SourceContext,
                        SourceAggregateId = normalizedRequest.SourceAggregateId,
                        SourceItemId = normalizedRequest.SourceItemId,
                        MilestoneFactId = normalizedRequest.MilestoneFactId,
                        MilestoneFactVersion = normalizedRequest.MilestoneFactVersion,
                        EffectType = normalizedRequest.EffectType,
                        OccurredAt = normalizedRequest.OccurredAt,
                        CalculationStatus = BillingChargeCalculationStatus.PendingFinancialReview,
                        ReviewReasonCode = ReviewRequiredCode,
                        Version = 1,
                        IsActive = true,
                        CreateDateTime = now,
                        CreateBy = actorUserId,
                        IsDelete = false,
                        IsCancel = false
                    };

                    if (normalizedRequest.Quantity.HasValue)
                    {
                        chargeLine.Components.Add(new BilChargeComponent
                        {
                            Id = Guid.NewGuid(),
                            ComponentKey = "Primary",
                            Quantity = normalizedRequest.Quantity,
                            Unit = normalizedRequest.Unit,
                            TariffSnapshot = normalizedRequest.TariffSnapshot,
                            RuleSnapshot = normalizedRequest.RuleSnapshot,
                            RoundingSnapshot = normalizedRequest.RoundingSnapshot,
                            CalculatedAmount = null,
                            CalculationVersion = 1,
                            CreateDateTime = now,
                            CreateBy = actorUserId,
                            IsDelete = false,
                            IsCancel = false
                        });
                    }

                    var processingEffect = CreateProcessingEffect(
                        normalizedRequest,
                        actorUserId,
                        consumer,
                        operationType,
                        fingerprint,
                        folio.Id,
                        chargeLine.Id,
                        chargeLine.CalculationStatus);

                    _dbContext.Set<BilChargeLine>().Add(chargeLine);
                    _dbContext.Set<BilProcessingEffect>().Add(processingEffect);

                    await _dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return BillingServiceResult<RecognizeBillingMilestoneResponse>.Success(
                        ToMilestoneResponse(processingEffect, isReplay: false));
                }
                catch (Exception exception) when (IsRetryableConcurrencyFailure(exception))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _dbContext.ChangeTracker.Clear();

                    existing = await FindProcessingEffectAsync(
                        consumer,
                        operationType,
                        normalizedRequest.IdempotencyKey,
                        cancellationToken);

                    if (existing != null)
                        return BuildExistingEffectResult(existing, fingerprint);

                    if (attempt == MaxPersistenceAttempts)
                    {
                        return BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                            "BIL_OUTCOME_UNKNOWN",
                            "Hasil pemrosesan belum dapat dipastikan setelah konflik concurrency. Lakukan rekonsiliasi sebelum mencoba kembali.");
                    }
                }
            }

            return BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                "BIL_OUTCOME_UNKNOWN",
                "Hasil pemrosesan belum dapat dipastikan.");
        }

        private IQueryable<BilFolio> BuildFolioQuery()
        {
            return _dbContext.Set<BilFolio>()
                .AsNoTracking()
                .Where(x => !x.IsDelete)
                .Include(x => x.ChargeLines.Where(line => !line.IsDelete))
                .ThenInclude(x => x.Components.Where(component => !component.IsDelete))
                .AsSplitQuery();
        }

        private async Task<BilProcessingEffect?> FindProcessingEffectAsync(
            string consumer,
            string operationType,
            string idempotencyKey,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<BilProcessingEffect>()
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Consumer == consumer &&
                         x.OperationType == operationType &&
                         x.IdempotencyKey == idempotencyKey,
                    cancellationToken);
        }

        private async Task<BilProcessingEffect?> FindLatestAppliedEffectAsync(
            string sourceContext,
            Guid milestoneFactId,
            string effectType,
            CancellationToken cancellationToken)
        {
            return await _dbContext.Set<BilProcessingEffect>()
                .AsNoTracking()
                .Where(x => x.SourceContext == sourceContext &&
                            x.MilestoneFactId == milestoneFactId &&
                            x.EffectType == effectType &&
                            x.Outcome == BillingProcessingOutcome.Succeeded)
                .OrderByDescending(x => x.MilestoneFactVersion)
                .ThenByDescending(x => x.CompletedAt)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private static BillingServiceResult<RecognizeBillingMilestoneResponse> BuildVersionConflictResult(
            int appliedVersion) =>
            BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                "BIL_VERSION_CONFLICT",
                "Versi milestone lebih lama daripada revisi yang telah diproses.",
                appliedVersion);

        private static BillingServiceResult<RecognizeBillingMilestoneResponse> BuildExistingEffectResult(
            BilProcessingEffect existing,
            string fingerprint)
        {
            if (!string.Equals(existing.RequestFingerprint, fingerprint, StringComparison.Ordinal))
            {
                return BillingServiceResult<RecognizeBillingMilestoneResponse>.Conflict(
                    "BIL_IDEMPOTENCY_CONFLICT",
                    "IdempotencyKey telah digunakan dengan input material yang berbeda.",
                    existing.MilestoneFactVersion);
            }

            return BillingServiceResult<RecognizeBillingMilestoneResponse>.Success(
                ToMilestoneResponse(existing, isReplay: true));
        }

        private static BilProcessingEffect CreateProcessingEffect(
            RecognizeBillingMilestoneRequest request,
            Guid actorUserId,
            string consumer,
            string operationType,
            string fingerprint,
            Guid folioId,
            Guid chargeLineId,
            BillingChargeCalculationStatus calculationStatus,
            string? errorCode = null,
            string? errorMessage = null)
        {
            var now = DateTime.UtcNow;
            return new BilProcessingEffect
            {
                Id = Guid.NewGuid(),
                Consumer = consumer,
                OperationType = operationType,
                IdempotencyKey = request.IdempotencyKey,
                RequestFingerprint = fingerprint,
                SourceContext = request.SourceContext,
                MilestoneFactId = request.MilestoneFactId,
                MilestoneFactVersion = request.MilestoneFactVersion,
                EffectType = request.EffectType,
                OccurredAt = request.OccurredAt,
                Outcome = BillingProcessingOutcome.Succeeded,
                FolioId = folioId,
                ChargeLineId = chargeLineId,
                CalculationStatus = calculationStatus,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                CorrelationId = request.CorrelationId,
                CausationId = request.CausationId,
                CompletedAt = now,
                CreateDateTime = now,
                CreateBy = actorUserId,
                IsDelete = false,
                IsCancel = false
            };
        }

        private static RecognizeBillingMilestoneResponse ToMilestoneResponse(
            BilProcessingEffect effect,
            bool isReplay)
        {
            var response = new RecognizeBillingMilestoneResponse
            {
                ProcessingEffectId = effect.Id,
                IsReplay = isReplay,
                FolioId = effect.FolioId,
                ChargeLineId = effect.ChargeLineId,
                Outcome = effect.Outcome,
                CalculationStatus = effect.CalculationStatus,
                Version = 1
            };

            if (effect.CalculationStatus == BillingChargeCalculationStatus.PendingFinancialReview)
            {
                response.Errors.Add(new BillingContractErrorResponse
                {
                    Code = ReviewRequiredCode,
                    Message = effect.ErrorMessage ??
                              "Komponen finansial memerlukan review karena formula perhitungan yang disetujui belum tersedia."
                });
            }

            return response;
        }

        private static BillingFolioDetailResponse ToDetailResponse(BilFolio folio)
        {
            return new BillingFolioDetailResponse
            {
                Id = folio.Id,
                EncounterId = folio.EncounterId,
                Status = folio.Status,
                Version = folio.Version,
                CreateDateTime = folio.CreateDateTime,
                UpdateDateTime = folio.UpdateDateTime,
                ChargeLines = folio.ChargeLines
                    .OrderBy(x => x.OccurredAt)
                    .ThenBy(x => x.CreateDateTime)
                    .Select(x => new BillingChargeLineResponse
                    {
                        Id = x.Id,
                        SourceContext = x.SourceContext,
                        SourceAggregateId = x.SourceAggregateId,
                        SourceItemId = x.SourceItemId,
                        MilestoneFactId = x.MilestoneFactId,
                        MilestoneFactVersion = x.MilestoneFactVersion,
                        EffectType = x.EffectType,
                        OccurredAt = x.OccurredAt,
                        CalculationStatus = x.CalculationStatus,
                        Currency = x.Currency,
                        GrossAmount = x.GrossAmount,
                        EligibleAmount = x.EligibleAmount,
                        ReviewReasonCode = x.ReviewReasonCode,
                        Version = x.Version,
                        Components = x.Components
                            .OrderBy(component => component.ComponentKey)
                            .Select(component => new BillingChargeComponentResponse
                            {
                                Id = component.Id,
                                ComponentKey = component.ComponentKey,
                                Quantity = component.Quantity,
                                Unit = component.Unit,
                                TariffSnapshot = component.TariffSnapshot,
                                RuleSnapshot = component.RuleSnapshot,
                                RoundingSnapshot = component.RoundingSnapshot,
                                CalculatedAmount = component.CalculatedAmount,
                                CalculationVersion = component.CalculationVersion
                            })
                            .ToList()
                    })
                    .ToList()
            };
        }

        private static (string Code, string Message)? ValidateRequest(
            RecognizeBillingMilestoneRequest request,
            Guid actorUserId,
            string consumer,
            string operationType)
        {
            if (actorUserId == Guid.Empty)
                return ("BIL_FORBIDDEN", "Actor pemrosesan Billing tidak valid.");

            if (string.IsNullOrWhiteSpace(consumer) || string.IsNullOrWhiteSpace(operationType))
                return ("BIL_SOURCE_INVALID", "Consumer dan OperationType wajib tersedia.");

            if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Trim().Length > 128)
                return ("BIL_SOURCE_INVALID", "IdempotencyKey wajib diisi dan maksimal 128 karakter.");

            if (request.MilestoneFactId == Guid.Empty || request.MilestoneFactVersion < 1)
                return ("BIL_SOURCE_INVALID", "Identitas dan versi milestone fact tidak valid.");

            if (request.EncounterId == Guid.Empty || request.SourceAggregateId == Guid.Empty)
                return ("BIL_SOURCE_INVALID", "EncounterId dan SourceAggregateId wajib diisi.");

            if (request.SourceItemId == Guid.Empty)
                return ("BIL_SOURCE_INVALID", "SourceItemId tidak boleh Guid.Empty.");

            var sourceContext = request.SourceContext?.Trim();
            if (string.IsNullOrWhiteSpace(sourceContext) || sourceContext.Length > 50)
                return ("BIL_SOURCE_INVALID", "SourceContext wajib diisi dan maksimal 50 karakter.");

            if (!BillingSourceContract.IsKnownSourceContext(sourceContext))
                return ("BIL_SOURCE_INVALID", "SourceContext belum diizinkan oleh kontrak Billing.");

            if (string.IsNullOrWhiteSpace(request.EffectType) || request.EffectType.Trim().Length > 100)
                return ("BIL_SOURCE_INVALID", "EffectType wajib diisi dan maksimal 100 karakter.");

            if (!BillingSourceContract.IsAllowedEffectType(sourceContext, request.EffectType.Trim()))
                return ("BIL_SOURCE_INVALID", "EffectType belum diizinkan untuk SourceContext tersebut.");

            if (request.OccurredAt == default)
                return ("BIL_SOURCE_INVALID", "OccurredAt wajib diisi.");

            if (request.OccurredAt.Kind == DateTimeKind.Unspecified)
                return ("BIL_SOURCE_INVALID", "OccurredAt wajib menyertakan timezone atau UTC.");

            if (request.Quantity.HasValue && request.Quantity.Value <= 0)
                return ("BIL_SOURCE_INVALID", "Quantity harus lebih besar dari nol.");

            if (request.Quantity.HasValue != !string.IsNullOrWhiteSpace(request.Unit))
                return ("BIL_SOURCE_INVALID", "Quantity dan Unit wajib tersedia bersama-sama.");

            var invalidSnapshot = ValidateSnapshot("TariffSnapshot", request.TariffSnapshot) ??
                                  ValidateSnapshot("RuleSnapshot", request.RuleSnapshot) ??
                                  ValidateSnapshot("RoundingSnapshot", request.RoundingSnapshot);
            return invalidSnapshot;
        }

        private static (string Code, string Message)? ValidateSnapshot(string name, string? value)
        {
            if (value == null)
                return null;

            if (string.IsNullOrWhiteSpace(value) || value.Length > 20_000)
                return ("BIL_SOURCE_INVALID", $"{name} harus berupa JSON non-kosong maksimal 20000 karakter.");

            try
            {
                using var document = JsonDocument.Parse(value);
                if (document.RootElement.ValueKind is not JsonValueKind.Object)
                    return ("BIL_SOURCE_INVALID", $"{name} harus berupa JSON object.");
            }
            catch (JsonException)
            {
                return ("BIL_SOURCE_INVALID", $"{name} bukan JSON yang valid.");
            }

            return null;
        }

        private static RecognizeBillingMilestoneRequest NormalizeRequest(
            RecognizeBillingMilestoneRequest request)
        {
            return new RecognizeBillingMilestoneRequest
            {
                IdempotencyKey = request.IdempotencyKey.Trim(),
                MilestoneFactId = request.MilestoneFactId,
                MilestoneFactVersion = request.MilestoneFactVersion,
                EncounterId = request.EncounterId,
                SourceContext = request.SourceContext.Trim(),
                SourceAggregateId = request.SourceAggregateId,
                SourceItemId = request.SourceItemId,
                EffectType = request.EffectType.Trim(),
                OccurredAt = request.OccurredAt.ToUniversalTime(),
                Quantity = request.Quantity,
                Unit = NormalizeNullableText(request.Unit),
                TariffSnapshot = NormalizeJson(request.TariffSnapshot),
                RuleSnapshot = NormalizeJson(request.RuleSnapshot),
                RoundingSnapshot = NormalizeJson(request.RoundingSnapshot),
                CorrelationId = request.CorrelationId == Guid.Empty ? null : request.CorrelationId,
                CausationId = request.CausationId == Guid.Empty ? null : request.CausationId
            };
        }

        private static string BuildFingerprint(
            RecognizeBillingMilestoneRequest request,
            string consumer,
            string operationType)
        {
            var material = string.Join(
                "|",
                consumer,
                operationType,
                request.MilestoneFactId.ToString("D"),
                request.MilestoneFactVersion.ToString(CultureInfo.InvariantCulture),
                request.EncounterId.ToString("D"),
                request.SourceContext,
                request.SourceAggregateId.ToString("D"),
                request.SourceItemId?.ToString("D") ?? string.Empty,
                request.EffectType,
                request.OccurredAt.ToString("O", CultureInfo.InvariantCulture),
                request.Quantity?.ToString(CultureInfo.InvariantCulture) ?? string.Empty,
                request.Unit ?? string.Empty,
                request.TariffSnapshot ?? string.Empty,
                request.RuleSnapshot ?? string.Empty,
                request.RoundingSnapshot ?? string.Empty);

            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(material)))
                .ToLowerInvariant();
        }

        private static string? NormalizeNullableText(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static string? NormalizeJson(string? value)
        {
            if (value == null)
                return null;

            using var document = JsonDocument.Parse(value);
            return JsonSerializer.Serialize(document.RootElement);
        }

        private static bool IsRetryableConcurrencyFailure(Exception exception)
        {
            var postgresException = exception as PostgresException ??
                                    exception.InnerException as PostgresException;

            return exception is DbUpdateConcurrencyException ||
                   postgresException?.SqlState is PostgresErrorCodes.UniqueViolation or
                       PostgresErrorCodes.SerializationFailure;
        }
    }
}
