using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

public sealed partial class BillingClinicalChargeBridgeService
{
    private static bool IsDrugReturnSnapshot(string? snapshot)
    {
        try
        {
            using var json = JsonDocument.Parse(snapshot ?? "{}");
            return json.RootElement.TryGetProperty("milestone", out var milestone) && milestone.GetString() == "DrugReturn";
        }
        catch (JsonException) { return false; }
    }

    private async Task<SyncOutcome> SyncDrugReturnAsync(ApplicationDbContext db, IServiceProvider services,
        BilProcessingEffect effect, CliClinicalMilestoneFact fact, CancellationToken ct)
    {
        var domain = MapDomain(effect.SourceContext);
        var detail = domain == BillingBridgeSourceDomains.Pharmacy ? fact.SourceAggregateId : fact.SourceItemId ?? Guid.Empty;
        var detailId = detail.ToString("D");
        if (await db.Set<BilProcessingEffect>().AsNoTracking().AnyAsync(x => x.MilestoneFactId == effect.MilestoneFactId
            && x.SourceContext == effect.SourceContext && x.MilestoneFactVersion < effect.MilestoneFactVersion && !x.IsDelete
            && (x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Pending || x.InvoiceSyncStatus == BillingInvoiceSyncStatus.Failed), ct))
            return SyncOutcome.WaitForPriorVersion(domain, detailId);
        var invoice = await (from folio in db.Set<BilFolio>().AsNoTracking()
            join inv in db.BilInvoices.AsNoTracking() on folio.EncounterId equals inv.EncounterId
            join enc in db.RegPatientEncounters.AsNoTracking() on inv.EncounterId equals enc.Id
            where folio.Id == effect.FolioId && !inv.IsDelete
            select new { inv.Id, inv.Status, inv.RowVersion, enc.ClinicId, enc.PatientClassId }).SingleOrDefaultAsync(ct);
        if (invoice == null) return SyncOutcome.NoChange("Retur belum memiliki invoice sumber.", domain, detailId, null);
        var key = DeterministicKey(effect.MilestoneFactId, effect.MilestoneFactVersion);
        var applied = await db.BilInvoiceItems.AsNoTracking().AnyAsync(x => x.InvoiceId == invoice.Id
            && x.LastIdempotencyKey == key && !x.IsDelete, ct);
        if (applied) return SyncOutcome.NoChange("Retur sudah diterapkan sebelumnya.", domain, detailId, invoice.Id);
        var item = await db.BilInvoiceItems.AsNoTracking().SingleOrDefaultAsync(x => x.InvoiceId == invoice.Id
            && x.SourceDomain == domain && x.SourceDetailId == detailId && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete, ct);
        if (item == null || item.SourceVersion >= effect.MilestoneFactVersion)
            return SyncOutcome.NoChange("Tidak ada versi tagihan aktif yang perlu dikurangi.", domain, detailId, invoice.Id);
        using var json = JsonDocument.Parse(fact.RuleSnapshot!);
        var root = json.RootElement;
        var returnNumber = root.GetProperty("returnNumber").GetString() ?? "Retur";
        IReadOnlyDictionary<Guid, decimal>? quantities = null;
        decimal remaining;
        if (domain == BillingBridgeSourceDomains.Pharmacy)
        {
            quantities = root.GetProperty("remainingItems").EnumerateArray().ToDictionary(
                x => Guid.Parse(x.GetProperty("prescriptionItemId").GetString()!), x => x.GetProperty("quantity").GetDecimal());
            remaining = quantities.Values.Sum();
        }
        else remaining = root.GetProperty("remainingQuantity").GetDecimal();
        decimal targetQuantity = 0, targetUnitPrice = 0;
        if (remaining > 0)
        {
            var tariff = await services.GetRequiredService<BillingSourceTariffResolver>().ResolveAsync(
                new BillingTariffResolutionRequest(domain, fact.SourceAggregateId, fact.SourceItemId, remaining,
                    invoice.ClinicId, invoice.PatientClassId, item.SourceOccurredAt.UtcDateTime, quantities), ct);
            if (tariff.Kind != BillingTariffResolutionKind.Resolved)
                return SyncOutcome.Reconcile(tariff.Code ?? BillingBridgeCodes.SourceRejected, tariff.Message ?? "Tarif retur tidak dapat dibaca.", domain, detailId);
            targetQuantity = tariff.Quantity; targetUnitPrice = tariff.UnitPrice;
        }
        var target = decimal.Round(targetQuantity * targetUnitPrice, 2, MidpointRounding.AwayFromZero);
        if (target > item.Quantity * item.UnitPrice)
            return SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected, "Retur tidak boleh menambah tagihan. Periksa tarif sumber.", domain, detailId);
        if (invoice.Status != BillingInvoiceStatuses.Open)
        {
            var plan = await BuildAdjustmentPlanAsync(db, effect, domain, detailId, target, $"retur {returnNumber}", key,
                fact.ActorUserId, invoice.Id, invoice.RowVersion, ct);
            return plan.Outcome ?? await AdjustAsync(plan, ct);
        }
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var newItemId = await scope.ServiceProvider.GetRequiredService<BillingDrugReturnService>().ApplyAsync(
                new BillingDrugReturnRequest(invoice.Id, item.Id, invoice.RowVersion, effect.MilestoneFactVersion,
                    targetQuantity, targetUnitPrice, returnNumber, fact.OccurredAt, fact.CorrelationId,
                    fact.CausationId ?? effect.MilestoneFactId), key, fact.ActorUserId, ct);
            return SyncOutcome.Synced(invoice.Id, newItemId, domain, detailId);
        }
        catch (BillingInvoiceValidationException ex) { return SyncOutcome.Reconcile(BillingBridgeCodes.SourceRejected, ex.Message, domain, detailId); }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Penerapan retur {ReturnId} menunggu kirim ulang.", fact.CausationId);
            return SyncOutcome.Transient(ex.GetType().Name, domain, detailId);
        }
    }
}
