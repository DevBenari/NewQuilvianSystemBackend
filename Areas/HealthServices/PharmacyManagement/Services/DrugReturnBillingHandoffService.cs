using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Constants;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;

// Fakta retur memakai stream penyerahan asli; jumlah dan nomor retur tetap dapat ditelusuri.
public sealed class DrugReturnBillingHandoffService(ApplicationDbContext db, ClinicalMilestoneFactProducer producer,
    ILogger<DrugReturnBillingHandoffService> logger)
{
    public async Task ValidateAcceptedSourceAsync(PhmDrugReturn returned, VerifyDrugReturnRequest request, CancellationToken ct)
    {
        var decisions = request.Items.ToDictionary(x => x.DrugReturnItemId);
        var accepted = returned.Items.Where(x => !x.IsDelete && decisions[x.Id].AcceptedStatus == DrugStockStatus.Available
            && decisions[x.Id].AcceptedQuantity > 0).ToList();
        if (accepted.Count == 0) return;
        if (returned.SourceDrugUsageId.HasValue)
        {
            var usage = await db.PhmDrugUsages.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Allocations)
                .SingleOrDefaultAsync(x => x.Id == returned.SourceDrugUsageId && !x.IsDelete, ct);
            if (usage == null || usage.EncounterId != returned.EncounterId)
                throw new DrugReturnUnprocessableException("PHM074", "Dokumen penyerahan retur tidak sesuai kunjungan pasien.");
            var earlier = await db.PhmDrugReturns.AsNoTracking().Include(x => x.Items)
                .Where(x => x.SourceDrugUsageId == usage.Id && x.Id != returned.Id && x.Status == DrugReturnStatus.Verified && !x.IsDelete).ToListAsync(ct);
            foreach (var group in accepted.GroupBy(x => (x.DrugId, x.DrugBatchId, x.MeasurementId)))
            {
                var delivered = usage.Items.Where(x => !x.IsDelete && x.DrugId == group.Key.DrugId && x.MeasurementId == group.Key.MeasurementId)
                    .SelectMany(x => x.Allocations.Where(a => !a.IsDelete && a.DrugBatchId == group.Key.DrugBatchId)).Sum(x => x.Quantity);
                var previous = earlier.SelectMany(x => x.Items).Where(x => !x.IsDelete && x.AcceptedStatus == DrugStockStatus.Available
                    && x.DrugId == group.Key.DrugId && x.DrugBatchId == group.Key.DrugBatchId && x.MeasurementId == group.Key.MeasurementId)
                    .Sum(x => x.AcceptedQuantity ?? 0);
                if (delivered <= 0 || previous + group.Sum(x => decisions[x.Id].AcceptedQuantity) > delivered)
                    throw new DrugReturnUnprocessableException("PHM074", "Jumlah layak kembali melebihi penyerahan obat dan batch sumber.");
            }
        }
        else if (returned.SourceOprMaterialUsageId.HasValue)
        {
            var usage = await db.Set<OprMaterialUsage>().AsNoTracking().Include(x => x.OprCase)
                .SingleOrDefaultAsync(x => x.Id == returned.SourceOprMaterialUsageId && !x.IsDelete, ct);
            if (usage?.OprCase?.EncounterId != returned.EncounterId
                || accepted.Any(x => x.DrugId != usage.ExternalItemId || x.MeasurementId != usage.UnitMeasurementId))
                throw new DrugReturnUnprocessableException("PHM074", "Bahan retur tidak sesuai pemakaian kamar operasi sumber.");
            var previous = await db.PhmDrugReturns.AsNoTracking().Where(x => x.SourceOprMaterialUsageId == usage.Id
                && x.Id != returned.Id && x.Status == DrugReturnStatus.Verified && !x.IsDelete)
                .SelectMany(x => x.Items.Where(i => !i.IsDelete && i.AcceptedStatus == DrugStockStatus.Available))
                .SumAsync(x => x.AcceptedQuantity ?? 0, ct);
            if (previous + accepted.Sum(x => decisions[x.Id].AcceptedQuantity) > usage.Quantity)
                throw new DrugReturnUnprocessableException("PHM074", "Jumlah layak kembali melebihi pemakaian bahan operasi sumber.");
        }
    }

    public async Task EmitAsync(Guid returnId, Guid actorId, CancellationToken ct)
    {
        try
        {
            var returned = await db.PhmDrugReturns.AsNoTracking().Include(x => x.Items)
                .SingleAsync(x => x.Id == returnId && !x.IsDelete, ct);
            var accepted = returned.Items.Where(x => !x.IsDelete && x.AcceptedStatus == DrugStockStatus.Available
                && x.AcceptedQuantity > 0).ToList();
            if (returned.Status != DrugReturnStatus.Verified || accepted.Count == 0) return;

            // BE-BKC-FIX-017: retur atas pemakaian TANPA resep mengurangi tagihan butir pemakaian
            // yang bersangkutan (satu fakta per butir), bukan tagihan resep.
            if (returned.SourceDrugUsageId.HasValue)
            {
                var sourceUsage = await db.PhmDrugUsages.AsNoTracking().Include(x => x.Items)
                    .SingleOrDefaultAsync(x => x.Id == returned.SourceDrugUsageId && !x.IsDelete, ct);
                if (sourceUsage is { PrescriptionId: null })
                {
                    if (sourceUsage.EncounterId == returned.EncounterId)
                        await EmitNonPrescriptionReturnAsync(returned, sourceUsage, actorId, ct);
                    return;
                }
            }

            // Retry Verify memakai fakta retur yang sama; tidak menerbitkan revisi tambahan.
            var marker = $"\"returnId\":\"{returnId:D}\"";
            var prior = await db.Set<CliClinicalMilestoneFact>().AsNoTracking()
                .Where(x => !x.IsDelete && x.CausationId == returnId && x.RuleSnapshot != null && x.RuleSnapshot.Contains(marker))
                .OrderByDescending(x => x.MilestoneFactVersion).FirstOrDefaultAsync(ct);
            if (prior != null)
            {
                await producer.EmitClinicalCancellationAsync(new ClinicalMilestoneFactRequest
                {
                    SourceContext = prior.SourceContext, SourceAggregateId = prior.SourceAggregateId, SourceItemId = prior.SourceItemId,
                    EffectType = prior.EffectType, EncounterId = prior.EncounterId, OccurredAt = prior.OccurredAt,
                    Quantity = prior.Quantity, Unit = prior.Unit, RuleSnapshot = prior.RuleSnapshot,
                    TariffSnapshot = prior.TariffSnapshot, CorrelationId = prior.CorrelationId, CausationId = returnId
                }, actorId, ct);
                return;
            }
            ClinicalMilestoneFactRequest? request = null;
            if (returned.SourceDrugUsageId.HasValue)
            {
                var usage = await db.PhmDrugUsages.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Allocations)
                    .SingleOrDefaultAsync(x => x.Id == returned.SourceDrugUsageId && !x.IsDelete, ct);
                if (usage?.PrescriptionId == null || usage.EncounterId != returned.EncounterId) return;
                var netItems = await GetNetDispensedItemsAsync(usage.PrescriptionId.Value, ct);
                request = new ClinicalMilestoneFactRequest
                {
                    SourceContext = BillingSourceContract.PrescriptionSourceContext, SourceAggregateId = usage.PrescriptionId.Value,
                    EffectType = BillingSourceContract.PrescriptionChargeEffectType, EncounterId = returned.EncounterId,
                    RuleSnapshot = JsonSerializer.Serialize(new
                    {
                        milestone = "DrugReturn", returnId, returnNumber = returned.ReturnNumber,
                        sourceDrugUsageId = usage.Id, remainingItems = netItems,
                        returnedItems = accepted.Select(x => new { drugId = x.DrugId, batchId = x.DrugBatchId, quantity = x.AcceptedQuantity })
                    })
                };
            }
            else if (returned.SourceOprMaterialUsageId.HasValue)
            {
                var usage = await db.Set<OprMaterialUsage>().AsNoTracking().Include(x => x.OprCase)
                    .SingleOrDefaultAsync(x => x.Id == returned.SourceOprMaterialUsageId && !x.IsDelete, ct);
                if (usage?.OprCase == null || usage.OprCase.EncounterId != returned.EncounterId) return;
                var priorReturns = await db.PhmDrugReturns.AsNoTracking().Where(x => x.SourceOprMaterialUsageId == usage.Id
                    && x.Status == DrugReturnStatus.Verified && !x.IsDelete)
                    .SelectMany(x => x.Items.Where(i => !i.IsDelete && i.AcceptedStatus == DrugStockStatus.Available))
                    .SumAsync(x => x.AcceptedQuantity ?? 0, ct);
                request = new ClinicalMilestoneFactRequest
                {
                    SourceContext = BillingSourceContract.OperatingRoomSourceContext, SourceAggregateId = usage.OprCaseId,
                    SourceItemId = usage.Id, EffectType = BillingSourceContract.OperatingRoomChargeEffectType, EncounterId = returned.EncounterId,
                    RuleSnapshot = JsonSerializer.Serialize(new
                    {
                        milestone = "DrugReturn", returnId, returnNumber = returned.ReturnNumber,
                        sourceOprMaterialUsageId = usage.Id, remainingQuantity = Math.Max(0, usage.Quantity - priorReturns)
                    })
                };
            }
            if (request == null) return;
            request.OccurredAt = returned.VerifiedAt ?? DateTime.UtcNow;
            request.Quantity = accepted.Sum(x => x.AcceptedQuantity ?? 0); request.Unit = "UNIT";
            request.CorrelationId = request.SourceAggregateId; request.CausationId = returnId;
            var result = await producer.EmitClinicalCancellationAsync(request, actorId, ct);
            if (!result.IsClinicallySafe)
                logger.LogWarning("Fakta retur {ReturnId} perlu rekonsiliasi: {Code}.", returnId, result.Code);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Fakta Billing retur {ReturnId} belum terkirim; Verify dapat diulang dengan kunci yang sama.", returnId);
        }
    }

    /// <summary>
    /// Retur pemakaian obat/BMHP tanpa resep — <c>BE-BKC-FIX-017</c>. Untuk setiap butir pemakaian
    /// yang jumlah kembaliannya berubah karena retur ini, terbit satu fakta pembatalan bermarka
    /// <c>DrugReturn</c> berisi sisa jumlah yang tetap ditagih; Billing mengurangi baris butir itu
    /// (atau mengajukan adjustment bila invoice sudah final).
    /// </summary>
    /// <remarks>
    /// Retur dicatat per obat dan batch, sedangkan tagihan per butir. Jumlah kembali per obat
    /// dibagikan ke butir-butir obat itu menurut urutan baris, dihitung ulang dari seluruh retur
    /// terverifikasi, sehingga hasilnya selalu sama walau dijalankan ulang. Verify yang diulang
    /// menerbitkan ulang fakta yang sama (replay), tidak pernah revisi tambahan.
    /// </remarks>
    private async Task EmitNonPrescriptionReturnAsync(PhmDrugReturn returned, PhmDrugUsage usage, Guid actorId, CancellationToken ct)
    {
        var verified = await db.PhmDrugReturns.AsNoTracking().Include(x => x.Items)
            .Where(x => x.SourceDrugUsageId == usage.Id && x.Status == DrugReturnStatus.Verified && !x.IsDelete)
            .ToListAsync(ct);
        static decimal Accepted(IEnumerable<PhmDrugReturn> returns, Guid drugId, Guid measurementId) =>
            returns.SelectMany(x => x.Items).Where(x => !x.IsDelete && x.AcceptedStatus == DrugStockStatus.Available
                && x.DrugId == drugId && x.MeasurementId == measurementId).Sum(x => x.AcceptedQuantity ?? 0);

        var marker = $"\"returnId\":\"{returned.Id:D}\"";
        var occurredAt = returned.VerifiedAt ?? DateTime.UtcNow;
        foreach (var group in usage.Items.Where(x => !x.IsDelete).GroupBy(x => (x.DrugId, x.MeasurementId)))
        {
            var lines = group.OrderBy(x => x.LineNumber).ToList();
            var totalAfter = Accepted(verified, group.Key.DrugId, group.Key.MeasurementId);
            var totalBefore = Accepted(verified.Where(x => x.Id != returned.Id), group.Key.DrugId, group.Key.MeasurementId);
            decimal consumedAfter = 0, consumedBefore = 0;
            foreach (var line in lines)
            {
                var returnedAfter = Math.Min(line.Quantity, Math.Max(0, totalAfter - consumedAfter));
                var returnedBefore = Math.Min(line.Quantity, Math.Max(0, totalBefore - consumedBefore));
                consumedAfter += line.Quantity;
                consumedBefore += line.Quantity;
                if (returnedAfter == returnedBefore) continue;

                var prior = await db.Set<CliClinicalMilestoneFact>().AsNoTracking()
                    .Where(x => !x.IsDelete && x.SourceContext == BillingSourceContract.DrugUsageSourceContext
                        && x.SourceItemId == line.Id && x.CausationId == returned.Id
                        && x.RuleSnapshot != null && x.RuleSnapshot.Contains(marker))
                    .OrderByDescending(x => x.MilestoneFactVersion).FirstOrDefaultAsync(ct);
                var request = prior != null
                    ? new ClinicalMilestoneFactRequest
                    {
                        SourceContext = prior.SourceContext, SourceAggregateId = prior.SourceAggregateId, SourceItemId = prior.SourceItemId,
                        EffectType = prior.EffectType, EncounterId = prior.EncounterId, OccurredAt = prior.OccurredAt,
                        Quantity = prior.Quantity, Unit = prior.Unit, RuleSnapshot = prior.RuleSnapshot,
                        TariffSnapshot = prior.TariffSnapshot, CorrelationId = prior.CorrelationId, CausationId = returned.Id
                    }
                    : new ClinicalMilestoneFactRequest
                    {
                        SourceContext = BillingSourceContract.DrugUsageSourceContext, SourceAggregateId = usage.Id, SourceItemId = line.Id,
                        EffectType = BillingSourceContract.DrugUsageChargeEffectType, EncounterId = returned.EncounterId,
                        OccurredAt = occurredAt, Quantity = returnedAfter - returnedBefore, Unit = "UNIT",
                        RuleSnapshot = JsonSerializer.Serialize(new
                        {
                            milestone = "DrugReturn", returnId = returned.Id, returnNumber = returned.ReturnNumber,
                            sourceDrugUsageId = usage.Id, remainingQuantity = line.Quantity - returnedAfter
                        }),
                        CorrelationId = usage.Id, CausationId = returned.Id
                    };
                var result = await producer.EmitClinicalCancellationAsync(request, actorId, ct);
                if (!result.IsClinicallySafe)
                    logger.LogWarning("Fakta retur pemakaian {ReturnId} butir {ItemId} perlu rekonsiliasi: {Code}.",
                        returned.Id, line.Id, result.Code);
            }
        }
    }

    public async Task<List<NetDispensedItem>> GetNetDispensedItemsAsync(Guid prescriptionId, CancellationToken ct)
    {
        var usages = await db.PhmDrugUsages.AsNoTracking().Include(x => x.Items).ThenInclude(x => x.Allocations)
            .Where(x => x.PrescriptionId == prescriptionId && !x.IsDelete
                && (x.Status == DrugUsageStatus.NotBilled || x.Status == DrugUsageStatus.Billed)).ToListAsync(ct);
        var usageIds = usages.Select(x => x.Id).ToList();
        var returns = await db.PhmDrugReturns.AsNoTracking().Include(x => x.Items)
            .Where(x => x.SourceDrugUsageId.HasValue && usageIds.Contains(x.SourceDrugUsageId.Value)
                && x.Status == DrugReturnStatus.Verified && !x.IsDelete).ToListAsync(ct);
        var quantities = new Dictionary<Guid, decimal>();
        foreach (var usage in usages)
        {
            var usageReturns = returns.Where(x => x.SourceDrugUsageId == usage.Id).SelectMany(x => x.Items)
                .Where(x => !x.IsDelete && x.AcceptedStatus == DrugStockStatus.Available && x.AcceptedQuantity > 0)
                .GroupBy(x => (x.DrugId, x.DrugBatchId, x.MeasurementId))
                .ToDictionary(x => x.Key, x => x.Sum(i => i.AcceptedQuantity ?? 0));
            foreach (var item in usage.Items.Where(x => !x.IsDelete && x.PrescriptionItemId.HasValue).OrderBy(x => x.LineNumber))
            {
                decimal returnedQuantity = 0;
                foreach (var allocation in item.Allocations.Where(x => !x.IsDelete).OrderBy(x => x.SequenceNumber))
                {
                    var key = (item.DrugId, allocation.DrugBatchId, item.MeasurementId);
                    var applied = Math.Min(allocation.Quantity, usageReturns.GetValueOrDefault(key));
                    returnedQuantity += applied; usageReturns[key] = usageReturns.GetValueOrDefault(key) - applied;
                }
                var id = item.PrescriptionItemId!.Value;
                quantities[id] = quantities.GetValueOrDefault(id) + Math.Max(0, item.Quantity - returnedQuantity);
            }
        }
        return quantities.OrderBy(x => x.Key).Select(x => new NetDispensedItem(x.Key.ToString("D"), x.Value)).ToList();
    }
}

public sealed record NetDispensedItem(string prescriptionItemId, decimal quantity);
