using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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
            or BillingSourceContract.ConsultationSourceContext
            // BE-RWI-179: komponen biaya Kamar Operasi (anestesi, sewa kamar operasi, bahan).
            or BillingSourceContract.OperatingRoomSourceContext;

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

        var outcome = plan.Void is not null
            ? await VoidAsync(plan, cancellationToken)
            : plan.Adjustment is not null
                ? await AdjustAsync(plan, cancellationToken)
                : await UpsertAsync(plan.Request!, plan.IdempotencyKey, plan.ActorUserId, cancellationToken);
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
        // BE-RWI-155 / RWI-DEC-192 / INT-RWF-04: kunjungan rawat inap kini dijembatani dengan titik
        // tagih yang sama seperti rawat jalan (tindakan saat Completed, Lab saat spesimen diterima,
        // Radiologi saat kualitas citra diputuskan, obat saat diserahkan — RWI-DEC-195). Invoice
        // tujuannya invoice kunjungan itu sendiri, yang untuk rawat inap berlabel RANAP.
        if (encounter.EncounterType is not (EncounterType.Outpatient or EncounterType.Inpatient))
            return SyncPlan.Done(SyncOutcome.NotApplicable(BillingBridgeCodes.NotOutpatient, "Kunjungan bukan Rawat Jalan maupun Rawat Inap."));

        var domain = MapDomain(effect.SourceContext);

        if (domain is not (BillingBridgeSourceDomains.Procedure or BillingBridgeSourceDomains.Laboratory
            or BillingBridgeSourceDomains.Radiology or BillingBridgeSourceDomains.Consultation
            or BillingBridgeSourceDomains.Pharmacy or BillingBridgeSourceDomains.OperatingRoom))
            return SyncPlan.Done(SyncOutcome.StayPending(BillingBridgeCodes.SourcePendingSupport, $"Penerusan {domain} belum tersedia pada jembatan.", domain));

        var fact = await db.Set<CliClinicalMilestoneFact>().AsNoTracking()
            .Where(x => x.MilestoneFactId == effect.MilestoneFactId
                && x.MilestoneFactVersion == effect.MilestoneFactVersion
                && x.SourceContext == effect.SourceContext
                && x.EffectType == effect.EffectType
                && !x.IsDelete)
            .Select(x => new { x.SourceAggregateId, x.SourceItemId, x.Quantity, x.ActorUserId, x.CorrelationId, x.CausationId, x.RuleSnapshot })
            .FirstOrDefaultAsync(cancellationToken);
        if (fact is null)
            return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.FactNotFound, "Fakta klinis untuk efek folio ini tidak ditemukan.", domain));

        // Tindakan, konsultasi, dan resep diidentifikasi oleh agregatnya; Lab dan Radiologi oleh
        // butirnya. Resep wajib memakai id resep karena clearance farmasi membacanya begitu.
        var detailId = domain is BillingBridgeSourceDomains.Procedure or BillingBridgeSourceDomains.Consultation
            or BillingBridgeSourceDomains.Pharmacy
            ? fact.SourceAggregateId
            : fact.SourceItemId;
        if (detailId is null || detailId == Guid.Empty)
            return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected, "Identitas butir pelayanan tidak tersedia pada fakta klinis.", domain));

        var key = DeterministicKey(effect.MilestoneFactId, effect.MilestoneFactVersion);
        var correlationId = fact.CorrelationId == Guid.Empty ? effect.MilestoneFactId : fact.CorrelationId;
        var causationId = fact.CausationId is { } causation && causation != Guid.Empty ? causation : effect.MilestoneFactId;

        if (effect.IsClinicalCancellation)
            return await BuildCancellationPlanAsync(
                db, services, effect, encounter.Id, domain, detailId.Value.ToString("D"),
                key, fact.ActorUserId, correlationId, causationId, cancellationToken);

        // Tagihan versi lama yang baru sampai setelah pembatalannya tidak boleh menghidupkan item
        // lagi: item yang sudah di-void tidak lagi dianggap ada oleh upsert, sehingga upsert akan
        // membuat item baru.
        var cancelledLater = await db.Set<BilProcessingEffect>().AsNoTracking()
            .AnyAsync(x => x.MilestoneFactId == effect.MilestoneFactId
                && x.SourceContext == effect.SourceContext
                && x.MilestoneFactVersion > effect.MilestoneFactVersion
                && x.IsClinicalCancellation
                && !x.IsDelete, cancellationToken);
        if (cancelledLater)
            return SyncPlan.Done(SyncOutcome.NoChange(
                "Pelayanan ini sudah dibatalkan pada versi yang lebih baru; versi ini tidak ditagihkan.",
                domain, detailId.Value.ToString("D"), null));

        // Resep dua tahap (RJ-E2E-DEC-005): tahap 1 PRESCRIBED saat resep difinalkan, tahap 2
        // DISPENSED dengan jumlah yang benar-benar diserahkan per item obat.
        IReadOnlyDictionary<Guid, decimal>? dispensedQuantities = null;
        var sourceStatus = MapBillableStatus(domain);
        if (domain == BillingBridgeSourceDomains.Pharmacy)
        {
            if (!TryReadPrescriptionStage(fact.RuleSnapshot, out var isDispensed, out dispensedQuantities))
                return SyncPlan.Done(SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected,
                    "Rincian tahap resep pada fakta klinis tidak dapat dibaca.", domain, detailId.Value.ToString("D")));
            sourceStatus = isDispensed ? PharmacyDispensedStatus : PharmacyPrescribedStatus;
        }

        var invoice = await db.Set<BilInvoice>().AsNoTracking()
            .Where(x => x.EncounterId == encounter.Id && !x.IsDelete)
            .Select(x => new { x.Id, x.Status, x.RowVersion })
            .FirstOrDefaultAsync(cancellationToken);

        var resolver = services.GetRequiredService<BillingSourceTariffResolver>();
        var tariff = await resolver.ResolveAsync(
            new BillingTariffResolutionRequest(
                domain, fact.SourceAggregateId, fact.SourceItemId, fact.Quantity,
                encounter.ClinicId, encounter.PatientClassId, DateTime.SpecifyKind(effect.OccurredAt, DateTimeKind.Utc),
                dispensedQuantities),
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
            SourceStatus = sourceStatus,
            OccurredAt = new DateTimeOffset(DateTime.SpecifyKind(effect.OccurredAt, DateTimeKind.Utc)),
            CategoryId = tariff.CategoryId,
            TariffId = tariff.TariffId,
            DescriptionSnapshot = tariff.Description,
            Quantity = tariff.Quantity,
            UnitPrice = tariff.UnitPrice,
            // Pembagian jasa medis milik modul medical-fee; sama dengan catalog-charges hari ini.
            DoctorShare = 0,
            ContractVersion = ContractBillingChargeSourceAdapter.IntegrationContractVersion13,
            CorrelationId = correlationId,
            CausationId = causationId
        };

        // BE-RJE-005 / V2.7.5: tagihan yang sudah tidak OPEN tidak boleh disunting lagi, sehingga
        // perubahan setelah final menjadi adjustment yang menunggu persetujuan (RJ-BIL-DEC-004).
        if (invoice is not null && invoice.Status != BillingInvoiceStatuses.Open)
            return await BuildAdjustmentPlanAsync(
                db, effect, request.SourceDomain, request.SourceDetailId,
                decimal.Round(request.Quantity * request.UnitPrice, 2, MidpointRounding.AwayFromZero),
                $"tagihan sudah {invoice.Status}", key, fact.ActorUserId, invoice.Id, invoice.RowVersion, cancellationToken);

        return SyncPlan.Upsert(request, key, fact.ActorUserId);
    }

    /// <summary>
    /// Selisih dihitung terhadap nilai efektif saat ini: total item yang tercatat ditambah
    /// adjustment yang sudah diajukan revisi lain untuk fakta yang sama. Dengan begitu versi 3
    /// tidak mengulang selisih yang sudah diajukan versi 2.
    /// </summary>
    private static async Task<SyncPlan> BuildAdjustmentPlanAsync(
        ApplicationDbContext db,
        BilProcessingEffect effect,
        string sourceDomain,
        string sourceDetailId,
        decimal target,
        string context,
        Guid key,
        Guid actorUserId,
        Guid invoiceId,
        Guid invoiceRowVersion,
        CancellationToken cancellationToken)
    {
        // Revisi ini sudah pernah menghasilkan adjustment (status sinkronnya saja yang hilang):
        // pakai yang ada, jangan mengajukan ulang.
        var existing = await db.Set<BilAdjustment>().AsNoTracking()
            .Where(x => x.IdempotencyKey == key && !x.IsDelete)
            .Select(x => new { x.Id, x.InvoiceId })
            .FirstOrDefaultAsync(cancellationToken);
        if (existing is not null)
            return SyncPlan.Done(SyncOutcome.SyncedByAdjustment(
                existing.InvoiceId, existing.Id, sourceDomain, sourceDetailId));

        var itemTotal = await db.Set<BilInvoiceItem>().AsNoTracking()
            .Where(x => x.InvoiceId == invoiceId
                && x.SourceDomain == sourceDomain
                && x.SourceDetailId == sourceDetailId
                && x.Status == BillingInvoiceItemStatuses.Active
                && !x.IsDelete)
            .Select(x => (decimal?)(x.Quantity * x.UnitPrice))
            .SumAsync(cancellationToken) ?? 0m;

        var priorAdjustments = await (
                from other in db.Set<BilProcessingEffect>().AsNoTracking()
                join adjustment in db.Set<BilAdjustment>().AsNoTracking() on other.InvoiceAdjustmentId equals adjustment.Id
                where other.MilestoneFactId == effect.MilestoneFactId
                    && other.SourceContext == effect.SourceContext
                    // Hanya revisi yang lebih dulu. Revisi yang datang belakangan tidak boleh
                    // mengubah hasil hitung ulang revisi ini, supaya kirim ulang selalu identik.
                    && other.MilestoneFactVersion < effect.MilestoneFactVersion
                    && !adjustment.IsDelete
                select new { adjustment.Direction, adjustment.Amount })
            .ToListAsync(cancellationToken);
        var effective = itemTotal + priorAdjustments.Sum(x =>
            x.Direction == BillingAdjustmentDirections.Debit ? x.Amount : -x.Amount);

        var delta = target - effective;
        if (delta == 0)
            return SyncPlan.Done(SyncOutcome.SyncedWithoutChange(
                invoiceId, sourceDomain, sourceDetailId));

        var adjustmentRequest = new CreateAdjustmentRequest
        {
            InvoiceId = invoiceId,
            Direction = delta > 0 ? BillingAdjustmentDirections.Debit : BillingAdjustmentDirections.Credit,
            Amount = Math.Abs(delta),
            ExpectedInvoiceRowVersion = invoiceRowVersion,
            Reason = Truncate(
                $"Penyesuaian otomatis {sourceDomain} {sourceDetailId} versi {effect.MilestoneFactVersion}: " +
                $"{context}, nilai {effective:0.##} menjadi {target:0.##}.", 500)!,
            // CorrelationId wajib unik per adjustment; kunci deterministik (fakta, versi) memenuhinya
            // sekaligus menjaga kirim ulang tetap terbaca sebagai replay yang sama.
            CorrelationId = key,
            CausationId = effect.MilestoneFactId
        };

        return SyncPlan.Adjust(adjustmentRequest, key, actorUserId, sourceDomain, sourceDetailId);
    }

    /// <summary>
    /// Pembatalan klinis atas pelayanan yang sudah ditagih (V2.7.5, <c>RJ-E2E-DEC-010</c>):
    /// item yang belum dikerjakan (mis. <c>ACCEPTED</c>, <c>PRESCRIBED</c>) pada invoice <c>OPEN</c>
    /// di-void; selain itu nilai efektifnya dikreditkan lewat adjustment. Riwayat tagihan tidak
    /// pernah dihapus.
    /// </summary>
    private static async Task<SyncPlan> BuildCancellationPlanAsync(
        ApplicationDbContext db,
        IServiceProvider services,
        BilProcessingEffect effect,
        Guid encounterId,
        string domain,
        string detailId,
        Guid key,
        Guid actorUserId,
        Guid correlationId,
        Guid causationId,
        CancellationToken cancellationToken)
    {
        // Tagihan versi sebelumnya belum selesai diteruskan: pembatalan menunggu, supaya tidak
        // mendahului tagihan yang hendak dibatalkannya.
        var priorUnsynced = await db.Set<BilProcessingEffect>().AsNoTracking()
            .AnyAsync(x => x.MilestoneFactId == effect.MilestoneFactId
                && x.SourceContext == effect.SourceContext
                && x.MilestoneFactVersion < effect.MilestoneFactVersion
                && (x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Pending || x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Failed)
                && !x.IsDelete, cancellationToken);
        if (priorUnsynced)
            return SyncPlan.Done(SyncOutcome.WaitForPriorVersion(domain, detailId));

        var invoice = await db.Set<BilInvoice>().AsNoTracking()
            .Where(x => x.EncounterId == encounterId && !x.IsDelete)
            .Select(x => new { x.Id, x.Status, x.RowVersion })
            .FirstOrDefaultAsync(cancellationToken);
        var item = invoice is null
            ? null
            : await db.Set<BilInvoiceItem>().AsNoTracking()
                .Where(x => x.InvoiceId == invoice.Id
                    && x.SourceDomain == domain
                    && x.SourceDetailId == detailId
                    && x.Status == BillingInvoiceItemStatuses.Active
                    && !x.IsDelete)
                .Select(x => new { x.Id, x.SourceStatus })
                .FirstOrDefaultAsync(cancellationToken);
        if (invoice is null || item is null)
            return SyncPlan.Done(SyncOutcome.NoChange(
                "Tidak ada tagihan aktif untuk pelayanan ini; pembatalan tidak mengubah invoice.",
                domain, detailId, invoice?.Id));

        var adapter = services.GetRequiredService<IBillingChargeSourceAdapter>();
        if (invoice.Status == BillingInvoiceStatuses.Open && adapter.IsNormallyVoidable(domain, item.SourceStatus))
        {
            var voidRequest = new VoidInvoiceItemRequest
            {
                ExpectedRowVersion = invoice.RowVersion,
                SourceVersion = effect.MilestoneFactVersion,
                SourceStatus = CancelledStatus,
                ContractVersion = ContractBillingChargeSourceAdapter.IntegrationContractVersion13,
                Reason = Truncate($"Pembatalan klinis {domain} {detailId} versi {effect.MilestoneFactVersion}.", 500)!,
                CorrelationId = correlationId,
                CausationId = causationId
            };
            return SyncPlan.VoidItem(voidRequest, invoice.Id, item.Id, key, actorUserId, domain, detailId);
        }

        return await BuildAdjustmentPlanAsync(
            db, effect, domain, detailId, 0m,
            invoice.Status == BillingInvoiceStatuses.Open
                ? $"pembatalan klinis atas pelayanan berstatus {item.SourceStatus}"
                : $"pembatalan klinis, tagihan sudah {invoice.Status}",
            key, actorUserId, invoice.Id, invoice.RowVersion, cancellationToken);
    }

    private async Task<SyncOutcome> VoidAsync(SyncPlan plan, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var invoiceService = scope.ServiceProvider.GetRequiredService<BillingInvoiceService>();
        try
        {
            var invoice = await invoiceService.VoidItemAsync(
                plan.InvoiceId, plan.ItemId, plan.Void!, plan.IdempotencyKey, plan.ActorUserId, cancellationToken);
            return SyncOutcome.Voided(invoice.Id, plan.ItemId, plan.SourceDomain!, plan.SourceDetailId!);
        }
        catch (BillingInvoiceValidationException exception)
        {
            // Mis. perhitungan sudah terkunci karena pembayaran sedang diproses.
            return SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected, exception.Message, plan.SourceDomain, plan.SourceDetailId);
        }
        catch (KeyNotFoundException exception)
        {
            return SyncOutcome.Reconcile(BillingBridgeCodes.SourceNotFound, exception.Message, plan.SourceDomain, plan.SourceDetailId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Termasuk konflik RowVersion (invoice berubah bersamaan): dicoba ulang dengan kunci sama.
            _logger.LogWarning(exception, "Void otomatis gagal sementara untuk {Domain} {DetailId}.",
                plan.SourceDomain, plan.SourceDetailId);
            return SyncOutcome.Transient(exception.GetType().Name, plan.SourceDomain!, plan.SourceDetailId!);
        }
    }

    private async Task<SyncOutcome> AdjustAsync(SyncPlan plan, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var exceptionService = scope.ServiceProvider.GetRequiredService<BillingFinancialExceptionService>();
        var request = plan.Adjustment!;
        try
        {
            var adjustment = await exceptionService.CreateAdjustmentAsync(request, plan.IdempotencyKey, plan.ActorUserId, cancellationToken);
            return SyncOutcome.SyncedByAdjustment(adjustment.InvoiceId, adjustment.Id, plan.SourceDomain!, plan.SourceDetailId!);
        }
        catch (BillingFinancialExceptionValidationException exception)
        {
            // Mis. invoice CLOSED / SETTLED_BY_WRITE_OFF tidak menerima adjustment baru.
            return SyncOutcome.Reconcile(BillingBridgeCodes.AdjustmentRejected, exception.Message, plan.SourceDomain, plan.SourceDetailId);
        }
        catch (BillingFinancialExceptionForbiddenException exception)
        {
            return SyncOutcome.Reconcile(BillingBridgeCodes.AdjustmentRejected, exception.Message, plan.SourceDomain, plan.SourceDetailId);
        }
        catch (KeyNotFoundException exception)
        {
            return SyncOutcome.Reconcile(BillingBridgeCodes.AdjustmentRejected, exception.Message, plan.SourceDomain, plan.SourceDetailId);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // Termasuk konflik RowVersion (invoice berubah bersamaan): dicoba ulang dengan kunci sama.
            _logger.LogWarning(exception, "Adjustment otomatis gagal sementara untuk {Domain} {DetailId}.",
                plan.SourceDomain, plan.SourceDetailId);
            return SyncOutcome.Transient(exception.GetType().Name, plan.SourceDomain!, plan.SourceDetailId!);
        }
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
            effect.InvoiceItemId = outcome.InvoiceItemId ?? effect.InvoiceItemId;
            effect.InvoiceAdjustmentId = outcome.InvoiceAdjustmentId ?? effect.InvoiceAdjustmentId;
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
        BillingSourceContract.OperatingRoomSourceContext => BillingBridgeSourceDomains.OperatingRoom,
        _ => sourceContext.ToUpperInvariant()
    };

    private const string CancelledStatus = "CANCELLED";
    private const string PharmacyPrescribedStatus = "PRESCRIBED";
    private const string PharmacyDispensedStatus = "DISPENSED";

    /// <summary>
    /// Membaca tahap resep dari <c>RuleSnapshot</c>: <c>milestone = "Dispensed"</c> berarti tahap 2
    /// beserta <c>items[].prescriptionItemId/quantity</c> kumulatif; selain itu (termasuk kosong,
    /// untuk fakta lama) tahap 1.
    /// </summary>
    internal static bool TryReadPrescriptionStage(
        string? ruleSnapshot,
        out bool isDispensed,
        out IReadOnlyDictionary<Guid, decimal>? dispensedQuantities)
    {
        isDispensed = false;
        dispensedQuantities = null;
        if (string.IsNullOrWhiteSpace(ruleSnapshot)) return true;
        try
        {
            using var document = JsonDocument.Parse(ruleSnapshot);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("milestone", out var milestone)
                || milestone.GetString() != BillingSourceContract.PrescriptionMilestoneDispensed)
                return true;

            if (!root.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
                return false;
            var quantities = new Dictionary<Guid, decimal>();
            foreach (var item in items.EnumerateArray())
            {
                if (!item.TryGetProperty("prescriptionItemId", out var idElement)
                    || !Guid.TryParse(idElement.GetString(), out var itemId)
                    || !item.TryGetProperty("quantity", out var quantityElement)
                    || !quantityElement.TryGetDecimal(out var quantity)
                    || quantity < 0)
                    return false;
                quantities[itemId] = quantities.GetValueOrDefault(itemId) + quantity;
            }

            isDispensed = true;
            dispensedQuantities = quantities;
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private static string MapBillableStatus(string domain) => domain switch
    {
        BillingBridgeSourceDomains.Laboratory => "ACCEPTED",
        BillingBridgeSourceDomains.Consultation => "COMPLETED",
        // BE-RWI-179: komponen OK hanya ditagih saat kasus Completed (INV-RWF-30).
        BillingBridgeSourceDomains.OperatingRoom => "COMPLETED",
        _ => "PERFORMED"
    };

    private static string? Truncate(string? value, int max) =>
        value is null || value.Length <= max ? value : value[..max];

    private sealed record SyncPlan(
        SyncOutcome? Outcome,
        UpsertChargeRequest? Request,
        CreateAdjustmentRequest? Adjustment,
        Guid IdempotencyKey,
        Guid ActorUserId,
        string? SourceDomain,
        string? SourceDetailId,
        VoidInvoiceItemRequest? Void = null,
        Guid InvoiceId = default,
        Guid ItemId = default)
    {
        public static SyncPlan Done(SyncOutcome outcome) => new(outcome, null, null, Guid.Empty, Guid.Empty, null, null);
        public static SyncPlan VoidItem(VoidInvoiceItemRequest request, Guid invoiceId, Guid itemId, Guid key, Guid actor, string domain, string detailId) =>
            new(null, null, null, key, actor, domain, detailId, request, invoiceId, itemId);
        public static SyncPlan Upsert(UpsertChargeRequest request, Guid key, Guid actor) =>
            new(null, request, null, key, actor, request.SourceDomain, request.SourceDetailId);
        public static SyncPlan Adjust(CreateAdjustmentRequest adjustment, Guid key, Guid actor, string domain, string detailId) =>
            new(null, null, adjustment, key, actor, domain, detailId);
    }

    private sealed record SyncOutcome(
        BillingInvoiceSyncStatus Status,
        string? Code,
        string? Message,
        string? SourceDomain,
        string? SourceDetailId,
        Guid? InvoiceId,
        Guid? InvoiceItemId,
        Guid? InvoiceAdjustmentId = null)
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
        public static SyncOutcome SyncedByAdjustment(Guid invoiceId, Guid adjustmentId, string domain, string detailId) =>
            new(BillingInvoiceSyncStatus.Synced, BillingBridgeCodes.AdjustmentSubmitted,
                "Penyesuaian diajukan dan menunggu persetujuan Billing.", domain, detailId, invoiceId, null, adjustmentId);
        public static SyncOutcome Voided(Guid invoiceId, Guid itemId, string domain, string detailId) =>
            new(BillingInvoiceSyncStatus.Synced, BillingBridgeCodes.ItemVoided,
                "Pelayanan dibatalkan sebelum dikerjakan; item invoice di-void.", domain, detailId, invoiceId, itemId);
        public static SyncOutcome NoChange(string message, string domain, string detailId, Guid? invoiceId) =>
            new(BillingInvoiceSyncStatus.Synced, BillingBridgeCodes.NoFinancialChange, message, domain, detailId, invoiceId, null);
        public static SyncOutcome WaitForPriorVersion(string domain, string detailId) =>
            new(BillingInvoiceSyncStatus.Failed, BillingBridgeCodes.TransientFailure,
                "Menunggu tagihan versi sebelumnya selesai diteruskan; akan dicoba ulang.", domain, detailId, null, null);
        public static SyncOutcome SyncedWithoutChange(Guid invoiceId, string domain, string detailId) =>
            new(BillingInvoiceSyncStatus.Synced, BillingBridgeCodes.NoFinancialChange,
                "Tagihan sudah final dan nilainya tidak berubah; tidak ada penyesuaian.", domain, detailId, invoiceId, null);
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

    /// <summary>Komponen biaya Kamar Operasi (<c>BE-RWI-179</c>); butirnya per komponen.</summary>
    public const string OperatingRoom = "OPERATING_ROOM";
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
    public const string AdjustmentSubmitted = "ADJUSTMENT_SUBMITTED";
    public const string AdjustmentRejected = "ADJUSTMENT_REJECTED";
    public const string NoFinancialChange = "NO_FINANCIAL_CHANGE";
    public const string ItemVoided = "ITEM_VOIDED";
    public const string CancellationPendingSupport = "CANCELLATION_PENDING_SUPPORT";
    public const string SourcePendingSupport = "SOURCE_PENDING_SUPPORT";
    public const string TransientFailure = "TRANSIENT_FAILURE";
    public const string RetryExhausted = "RETRY_EXHAUSTED";
    public const string SyncPolicyInactive = "SYNC_POLICY_INACTIVE";
}
