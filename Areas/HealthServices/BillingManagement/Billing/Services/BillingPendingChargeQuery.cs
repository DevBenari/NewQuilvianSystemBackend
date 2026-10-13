using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// Pelayanan yang sudah layak ditagih tetapi belum menjadi item invoice — <c>BE-BKC-FIX-020</c>
/// (daftar kasir) dan <c>BE-BKC-FIX-021</c> (penjaga finalisasi), <c>ISSUE-BKC-001</c> T1.
/// </summary>
/// <remarks>
/// <para>
/// Dibaca dari dua tempat, bukan dari item invoice — item yang belum pernah terbentuk tidak dapat
/// diperiksa lewat item:
/// </para>
/// <list type="number">
/// <item><description>efek folio sumber klinis yang masih <c>Pending</c>, <c>Failed</c>, atau
/// <c>ReconciliationRequired</c> (termasuk sumber yang belum didukung jembatan dan racikan yang
/// ditolak), dan</description></item>
/// <item><description>fakta klinis tagih yang belum pernah diterima folio (<c>Pending</c> atau
/// <c>OutcomeUnknown</c>) dan belum diselesaikan lewat rekonsiliasi.</description></item>
/// </list>
/// <para>
/// Yang <b>tidak</b> dihitung: efek <c>NotApplicable</c> (gratis, tidak billable, resep rawat inap
/// yang belum diserahkan, sumber di luar cakupan), <c>Synced</c>, dan <c>Resolved</c>. Dengan begitu
/// tahap yang memang belum eligible tidak pernah mengunci invoice, sedangkan penyelesaian
/// rekonsiliasi atau kirim ulang yang berhasil langsung melepas penahannya.
/// </para>
/// </remarks>
public static class BillingPendingChargeQuery
{
    /// <summary>Jumlah pelayanan tertunda per kunjungan; kunjungan tanpa yang tertunda tidak dimuat.</summary>
    public static async Task<Dictionary<Guid, int>> CountByEncounterAsync(
        ApplicationDbContext db,
        IReadOnlyCollection<Guid> encounterIds,
        CancellationToken cancellationToken,
        bool excludeTariffNotFound = false)
    {
        var result = new Dictionary<Guid, int>();
        if (encounterIds.Count == 0) return result;

        var effectQuery =
            from effect in db.BilProcessingEffects.AsNoTracking()
            join folio in db.BilFolios.AsNoTracking() on effect.FolioId equals folio.Id
            where encounterIds.Contains(folio.EncounterId)
                && !effect.IsDelete
                && (effect.InvoiceSyncStatus == BillingInvoiceSyncStatus.Pending
                    || effect.InvoiceSyncStatus == BillingInvoiceSyncStatus.Failed
                    || effect.InvoiceSyncStatus == BillingInvoiceSyncStatus.ReconciliationRequired)
            select new { folio.EncounterId, effect.InvoiceSyncStatus, effect.InvoiceSyncErrorCode };
        if (excludeTariffNotFound)
            effectQuery = effectQuery.Where(x => !(x.InvoiceSyncStatus == BillingInvoiceSyncStatus.ReconciliationRequired
                && x.InvoiceSyncErrorCode == BillingBridgeCodes.TariffNotFound));

        var effectCounts = await effectQuery
            .GroupBy(x => x.EncounterId)
            .Select(g => new { EncounterId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var factCounts = await db.Set<CliClinicalMilestoneFact>().AsNoTracking()
            .Where(x => encounterIds.Contains(x.EncounterId)
                && !x.IsDelete
                && x.MilestoneKind == ClinicalMilestoneKind.ChargeEligibility
                && (x.DispatchStatus == ClinicalFactDispatchStatus.Pending
                    || x.DispatchStatus == ClinicalFactDispatchStatus.OutcomeUnknown)
                && x.ReconciliationResolvedAt == null)
            .GroupBy(x => x.EncounterId)
            .Select(g => new { EncounterId = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        foreach (var row in effectCounts.Concat(factCounts))
            result[row.EncounterId] = result.GetValueOrDefault(row.EncounterId) + row.Count;
        return result;
    }
}
