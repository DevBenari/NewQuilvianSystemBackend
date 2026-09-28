using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Jembatan satu efek folio ke invoice canonical (<c>RJ-E2E-DEC-003</c>, <c>RJ-E2E-DEC-016</c>).
///
/// Setiap panggilan berjalan pada scope DI dan <c>DbContext</c>-nya sendiri. Alasannya:
/// <see cref="BillingInvoiceService.UpsertChargeAsync"/> membuka transaksi serializable sendiri,
/// dan bila gagal dapat meninggalkan entity yang terlacak. Memakai context pemanggil (folio atau
/// producer klinis) akan membuat sisa itu ikut tersimpan oleh SaveChanges mereka berikutnya.
///
/// Jembatan tidak pernah melempar ke pemanggil. Kegagalannya hanya mengubah status sinkron efek,
/// sehingga transaksi klinis dan folio yang sudah sah tidak pernah ikut batal (<c>AC-RJ-013</c>).
/// </summary>
public sealed class BillingClinicalChargeBridgeService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BillingClinicalChargeBridgeService> _logger;

    public BillingClinicalChargeBridgeService(
        IServiceScopeFactory scopeFactory,
        ILogger<BillingClinicalChargeBridgeService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    /// <summary>Konteks folio yang menjadi calon penerusan ke invoice Rawat Jalan (V2.7.1).</summary>
    public static bool IsCandidateSourceContext(string? sourceContext) =>
        sourceContext is BillingSourceContract.ProcedureSourceContext
            or BillingSourceContract.LaboratorySourceContext
            or BillingSourceContract.RadiologySourceContext
            or BillingSourceContract.PrescriptionSourceContext
            or BillingSourceContract.ConsultationSourceContext;

    /// <summary>
    /// Meneruskan efek bila statusnya <c>Pending</c> atau <c>Failed</c>. Efek dengan status lain
    /// diabaikan, sehingga panggilan berulang (replay folio, pekerja latar) aman.
    /// </summary>
    public async Task<BillingInvoiceSyncStatus?> TrySyncEffectAsync(
        Guid processingEffectId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await SyncEffectAsync(processingEffectId, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Pengaman terakhir: status efek tetap Pending/Failed sehingga dapat dikirim ulang.
            _logger.LogError(exception, "Jembatan invoice gagal untuk efek {EffectId}.", processingEffectId);
            return null;
        }
    }

    private async Task<BillingInvoiceSyncStatus?> SyncEffectAsync(Guid effectId, CancellationToken cancellationToken)
    {
        using var readScope = _scopeFactory.CreateScope();
        var db = readScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var effect = await db.Set<BilProcessingEffect>().AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == effectId && !x.IsDelete, cancellationToken);
        if (effect is null
            || effect.InvoiceSyncStatus is not (BillingInvoiceSyncStatus.Pending or BillingInvoiceSyncStatus.Failed))
            return effect?.InvoiceSyncStatus;

        var plan = await BuildPlanAsync(db, readScope.ServiceProvider, effect, cancellationToken);
        if (plan.Outcome is not null)
            return await RecordAsync(effect, plan.Outcome, cancellationToken);

        var outcome = await UpsertAsync(plan.Request!, plan.IdempotencyKey, plan.ActorUserId, cancellationToken);
        return await RecordAsync(effect, outcome, cancellationToken);
    }

    private async Task<SyncPlan> BuildPlanAsync(
        ApplicationDbContext db,
        IServiceProvider services,
        BilProcessingEffect effect,
        CancellationToken cancellationToken)
    {
        if (!IsCandidateSourceContext(effect.SourceContext))
            return SyncPlan.Done(SyncOutcome.NotApplicable(BillingBridgeCodes.SourceOutOfScope, "Konteks sumber di luar cakupan penagihan Rawat Jalan."));

        var encounter = await (
                from folio in db.Set<BilFolio>().AsNoTracking()
                join enc in db.Set<RegPatientEncounter>().AsNoTracking() on folio.EncounterId equals enc.Id
                where folio.Id == effect.FolioId
                select new { enc.Id, enc.EncounterType, enc.ClinicId, enc.PatientClassId })
            .FirstOrDefaultAsync(cancellationToken);
        if (encounter is null)
            return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.EncounterNotFound, "Kunjungan untuk efek folio ini tidak ditemukan."));
        if (encounter.EncounterType != EncounterType.Outpatient)
            return SyncPlan.Done(SyncOutcome.NotApplicable(BillingBridgeCodes.NotOutpatient, "Kunjungan bukan Rawat Jalan."));

        var domain = MapDomain(effect.SourceContext);

        // Pembatalan, konsultasi, dan resep ditangani task berikutnya (BE-RJE-009, 007, 008).
        // Efeknya tetap Pending — tidak dibuang — supaya diproses begitu dukungannya tersedia.
        if (effect.IsClinicalCancellation)
            return SyncPlan.Done(SyncOutcome.StayPending(BillingBridgeCodes.CancellationPendingSupport, "Penerusan pembatalan klinis belum tersedia pada jembatan.", domain));
        if (domain is not (BillingBridgeSourceDomains.Procedure or BillingBridgeSourceDomains.Laboratory or BillingBridgeSourceDomains.Radiology))
            return SyncPlan.Done(SyncOutcome.StayPending(BillingBridgeCodes.SourcePendingSupport, $"Penerusan {domain} belum tersedia pada jembatan.", domain));

        var fact = await db.Set<CliClinicalMilestoneFact>().AsNoTracking()
            .Where(x => x.MilestoneFactId == effect.MilestoneFactId
                && x.MilestoneFactVersion == effect.MilestoneFactVersion
                && x.SourceContext == effect.SourceContext
                && x.EffectType == effect.EffectType
                && !x.IsDelete)
            .Select(x => new { x.SourceAggregateId, x.SourceItemId, x.Quantity, x.ActorUserId, x.CorrelationId, x.CausationId })
            .FirstOrDefaultAsync(cancellationToken);
        if (fact is null)
            return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.FactNotFound, "Fakta klinis untuk efek folio ini tidak ditemukan.", domain));

        var detailId = domain == BillingBridgeSourceDomains.Procedure ? fact.SourceAggregateId : fact.SourceItemId;
        if (detailId is null || detailId == Guid.Empty)
            return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected, "Identitas butir pelayanan tidak tersedia pada fakta klinis.", domain));

        // Invoice di luar OPEN ditangani BE-RJE-005 lewat adjustment. Sampai itu tersedia, efek
        // berhenti di antrean alih-alih memaksa UpsertChargeAsync menolak.
        var invoiceStatus = await db.Set<BilInvoice>().AsNoTracking()
            .Where(x => x.EncounterId == encounter.Id && !x.IsDelete)
            .Select(x => x.Status)
            .FirstOrDefaultAsync(cancellationToken);
        if (invoiceStatus is not null && invoiceStatus != BillingInvoiceStatuses.Open)
            return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.InvoiceNotOpen, $"Tagihan kunjungan sudah berstatus {invoiceStatus}; penyesuaian belum tersedia otomatis.", domain));

        var resolver = services.GetRequiredService<BillingSourceTariffResolver>();
        var tariff = await resolver.ResolveAsync(
            new BillingTariffResolutionRequest(
                domain, fact.SourceAggregateId, fact.SourceItemId, fact.Quantity,
                encounter.ClinicId, encounter.PatientClassId, DateTime.SpecifyKind(effect.OccurredAt, DateTimeKind.Utc)),
            cancellationToken);

        switch (tariff.Kind)
        {
            case BillingTariffResolutionKind.NotBillable:
                return SyncPlan.Done(SyncOutcome.NotApplicable(tariff.Code!, tariff.Message!));
            case BillingTariffResolutionKind.Rejected:
                return SyncPlan.Done(SyncOutcome.Reconcile(tariff.Code!, tariff.Message!, domain, detailId.Value.ToString("D")));
            case BillingTariffResolutionKind.Pending:
                return SyncPlan.Done(SyncOutcome.StayPending(tariff.Code!, tariff.Message!, domain));
        }

        var request = new UpsertChargeRequest
        {
            EncounterId = encounter.Id,
            SourceDomain = domain,
            SourceDetailId = detailId.Value.ToString("D"),
            SourceVersion = effect.MilestoneFactVersion,
            SourceStatus = MapBillableStatus(domain),
            OccurredAt = new DateTimeOffset(DateTime.SpecifyKind(effect.OccurredAt, DateTimeKind.Utc)),
            CategoryId = tariff.CategoryId,
            TariffId = tariff.TariffId,
            DescriptionSnapshot = tariff.Description,
            Quantity = tariff.Quantity,
            UnitPrice = tariff.UnitPrice,
            // Pembagian jasa medis milik modul medical-fee; sama dengan catalog-charges hari ini.
            DoctorShare = 0,
            ContractVersion = ContractBillingChargeSourceAdapter.IntegrationContractVersion13,
            CorrelationId = fact.CorrelationId == Guid.Empty ? effect.MilestoneFactId : fact.CorrelationId,
            CausationId = fact.CausationId is { } causation && causation != Guid.Empty ? causation : effect.MilestoneFactId
        };

        return SyncPlan.Upsert(request, DeterministicKey(effect.MilestoneFactId, effect.MilestoneFactVersion), fact.ActorUserId);
    }

    private async Task<SyncOutcome> UpsertAsync(
        UpsertChargeRequest request,
        Guid idempotencyKey,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var invoiceService = scope.ServiceProvider.GetRequiredService<BillingInvoiceService>();
        try
        {
            var invoice = await invoiceService.UpsertChargeAsync(request, idempotencyKey, actorUserId, cancellationToken);
            var item = invoice.Items.FirstOrDefault(x =>
                string.Equals(x.SourceDomain, request.SourceDomain, StringComparison.OrdinalIgnoreCase)
                && string.Equals(x.SourceDetailId, request.SourceDetailId, StringComparison.OrdinalIgnoreCase));
            return SyncOutcome.Synced(invoice.Id, item?.Id, request.SourceDomain, request.SourceDetailId);
        }
        catch (BillingInvoiceValidationException exception)
        {
            return SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected, exception.Message, request.SourceDomain, request.SourceDetailId);
        }
        catch (BillingInvoiceConflictException exception)
        {
            return SyncOutcome.Reconcile(BillingBridgeCodes.SourceConflict, exception.Message, request.SourceDomain, request.SourceDetailId);
        }
        catch (KeyNotFoundException exception)
        {
            return SyncOutcome.Reconcile(BillingBridgeCodes.EncounterNotFound, exception.Message, request.SourceDomain, request.SourceDetailId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Gangguan sementara (koneksi, lock, serialisasi): dicoba ulang dengan kunci yang sama.
            _logger.LogWarning(exception, "Penerusan efek ke invoice gagal sementara untuk {Domain} {DetailId}.",
                request.SourceDomain, request.SourceDetailId);
            return SyncOutcome.Transient(exception.GetType().Name, request.SourceDomain, request.SourceDetailId);
        }
    }

    /// <summary>
    /// Menulis hasil ke kolom sinkron efek pada context baru, dijaga <c>InvoiceSyncVersion</c>.
    /// Bila pemroses lain lebih dulu menulis, hasil ini dibuang — hasil yang sudah tersimpan
    /// lebih baru, dan upsert invoice sendiri idempoten.
    /// </summary>
    private async Task<BillingInvoiceSyncStatus?> RecordAsync(
        BilProcessingEffect snapshot,
        SyncOutcome outcome,
        CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<LoggerService>();

        var effect = await db.Set<BilProcessingEffect>()
            .FirstOrDefaultAsync(x => x.Id == snapshot.Id, cancellationToken);
        if (effect is null || effect.InvoiceSyncVersion != snapshot.InvoiceSyncVersion)
            return effect?.InvoiceSyncStatus;

        var now = DateTime.UtcNow;
        var status = outcome.Status;
        effect.InvoiceSourceDomain = outcome.SourceDomain ?? effect.InvoiceSourceDomain;
        effect.InvoiceSourceDetailId = outcome.SourceDetailId ?? effect.InvoiceSourceDetailId;
        effect.InvoiceSyncErrorCode = outcome.Code;
        effect.InvoiceSyncErrorMessage = Truncate(outcome.Message, 1000);

        if (status == BillingInvoiceSyncStatus.Synced)
        {
            effect.InvoiceId = outcome.InvoiceId;
            effect.InvoiceItemId = outcome.InvoiceItemId;
            effect.InvoiceSyncedAt = now;
            effect.InvoiceSyncNextAttemptAt = null;
        }
        else if (status == BillingInvoiceSyncStatus.Failed)
        {
            effect.InvoiceSyncAttemptCount += 1;
            var policy = await db.Set<MstBillingSyncPolicy>().AsNoTracking()
                .FirstOrDefaultAsync(x => x.PolicyCode == BillingSyncPolicyCodes.InvoiceSync && x.IsActive && !x.IsDelete, cancellationToken);
            if (policy is null)
            {
                status = BillingInvoiceSyncStatus.ReconciliationRequired;
                effect.InvoiceSyncErrorCode = BillingBridgeCodes.SyncPolicyInactive;
                effect.InvoiceSyncErrorMessage = "Pengiriman ulang otomatis sedang dimatikan. Item menunggu penanganan manual.";
                effect.InvoiceSyncNextAttemptAt = null;
            }
            else if (effect.InvoiceSyncAttemptCount >= policy.MaxAttemptCount)
            {
                status = BillingInvoiceSyncStatus.ReconciliationRequired;
                effect.InvoiceSyncErrorCode = BillingBridgeCodes.RetryExhausted;
                effect.InvoiceSyncErrorMessage = "Pengiriman otomatis sudah dicoba berulang kali dan belum berhasil.";
                effect.InvoiceSyncNextAttemptAt = null;
            }
            else
            {
                effect.InvoiceSyncNextAttemptAt = now.AddSeconds(BackoffSeconds(policy, effect.InvoiceSyncAttemptCount));
            }
        }
        else
        {
            effect.InvoiceSyncNextAttemptAt = null;
        }

        effect.InvoiceSyncStatus = status;
        effect.InvoiceSyncVersion += 1;
        effect.UpdateDateTime = now;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return null;
        }

        await logger.AuditAsync(
            "BillingManagement",
            status == BillingInvoiceSyncStatus.Synced ? "BillingBridge.Sync" : "BillingBridge.Outcome",
            "Hasil penerusan efek folio ke invoice canonical.",
            new
            {
                ProcessingEffectId = effect.Id,
                effect.FolioId,
                effect.SourceContext,
                SourceDomain = effect.InvoiceSourceDomain,
                SourceDetailId = effect.InvoiceSourceDetailId,
                SourceVersion = effect.MilestoneFactVersion,
                effect.InvoiceId,
                effect.CorrelationId,
                Outcome = status.ToString(),
                Code = effect.InvoiceSyncErrorCode,
                effect.InvoiceSyncAttemptCount
            });

        return status;
    }

    /// <summary>min(Base × 2^(n−1), Max) — V2.11.</summary>
    internal static int BackoffSeconds(MstBillingSyncPolicy policy, int attempt)
    {
        var exponent = Math.Clamp(attempt - 1, 0, 20);
        var delay = (long)policy.BaseDelaySeconds << exponent;
        return (int)Math.Min(delay, policy.MaxDelaySeconds);
    }

    /// <summary>
    /// Kunci idempotency yang selalu sama untuk satu (fakta, versi), sehingga kirim ulang oleh
    /// siapa pun tidak pernah menghasilkan item kedua (V2.7.2).
    /// </summary>
    internal static Guid DeterministicKey(Guid milestoneFactId, int milestoneFactVersion)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"RJ-E2E|{milestoneFactId:D}|{milestoneFactVersion}"));
        var guidBytes = bytes[..16];
        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x50); // penanda versi 5 (berbasis nama)
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80); // varian RFC 4122
        return new Guid(guidBytes);
    }

    private static string MapDomain(string sourceContext) => sourceContext switch
    {
        BillingSourceContract.ProcedureSourceContext => BillingBridgeSourceDomains.Procedure,
        BillingSourceContract.LaboratorySourceContext => BillingBridgeSourceDomains.Laboratory,
        BillingSourceContract.RadiologySourceContext => BillingBridgeSourceDomains.Radiology,
        BillingSourceContract.PrescriptionSourceContext => BillingBridgeSourceDomains.Pharmacy,
        BillingSourceContract.ConsultationSourceContext => BillingBridgeSourceDomains.Consultation,
        _ => sourceContext.ToUpperInvariant()
    };

    private static string MapBillableStatus(string domain) => domain switch
    {
        BillingBridgeSourceDomains.Laboratory => "ACCEPTED",
        _ => "PERFORMED"
    };

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];

    private sealed record SyncPlan(SyncOutcome? Outcome, UpsertChargeRequest? Request, Guid IdempotencyKey, Guid ActorUserId)
    {
        public static SyncPlan Done(SyncOutcome outcome) => new(outcome, null, Guid.Empty, Guid.Empty);
        public static SyncPlan Upsert(UpsertChargeRequest request, Guid key, Guid actor) => new(null, request, key, actor);
    }

    private sealed record SyncOutcome(
        BillingInvoiceSyncStatus Status,
        string? Code,
        string? Message,
        string? SourceDomain,
        string? SourceDetailId,
        Guid? InvoiceId,
        Guid? InvoiceItemId)
    {
        public static SyncOutcome NotApplicable(string code, string message) =>
            new(BillingInvoiceSyncStatus.NotApplicable, code, message, null, null, null, null);
        public static SyncOutcome Reconcile(string code, string message, string? domain = null, string? detailId = null) =>
            new(BillingInvoiceSyncStatus.ReconciliationRequired, code, message, domain, detailId, null, null);
        public static SyncOutcome StayPending(string code, string message, string? domain) =>
            new(BillingInvoiceSyncStatus.Pending, code, message, domain, null, null, null);
        public static SyncOutcome Transient(string errorType, string domain, string detailId) =>
            new(BillingInvoiceSyncStatus.Failed, BillingBridgeCodes.TransientFailure, $"Gangguan sementara ({errorType}); akan dicoba ulang.", domain, detailId, null, null);
        public static SyncOutcome Synced(Guid invoiceId, Guid? itemId, string domain, string detailId) =>
            new(BillingInvoiceSyncStatus.Synced, null, null, domain, detailId, invoiceId, itemId);
    }
}

/// <summary>SourceDomain invoice untuk jembatan Rawat Jalan (V2.7.2).</summary>
public static class BillingBridgeSourceDomains
{
    public const string Procedure = "PROCEDURE";
    public const string Laboratory = "LABORATORY";
    public const string Radiology = "RADIOLOGY";
    public const string Pharmacy = "PHARMACY";
    public const string Consultation = "CONSULTATION";
}

/// <summary>Kode sebab pada antrean rekonsiliasi — validation matrix V2.</summary>
public static class BillingBridgeCodes
{
    public const string SourceOutOfScope = "SOURCE_OUT_OF_SCOPE";
    public const string NotOutpatient = "NOT_OUTPATIENT";
    public const string NotBillable = "NOT_BILLABLE";
    public const string RepeatInternalError = "REPEAT_INTERNAL_ERROR";
    public const string TariffNotFound = "TARIFF_NOT_FOUND";
    public const string SourceNotFound = "SOURCE_NOT_FOUND";
    public const string FactNotFound = "FACT_NOT_FOUND";
    public const string EncounterNotFound = "ENCOUNTER_NOT_FOUND";
    public const string SourceRejected = "SOURCE_REJECTED";
    public const string SourceConflict = "SOURCE_CONFLICT";
    public const string InvoiceNotOpen = "INVOICE_NOT_OPEN";
    public const string CancellationPendingSupport = "CANCELLATION_PENDING_SUPPORT";
    public const string SourcePendingSupport = "SOURCE_PENDING_SUPPORT";
    public const string TransientFailure = "TRANSIENT_FAILURE";
    public const string RetryExhausted = "RETRY_EXHAUSTED";
    public const string SyncPolicyInactive = "SYNC_POLICY_INACTIVE";
}
