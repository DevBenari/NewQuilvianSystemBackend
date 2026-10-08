using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services
{
    /// <summary>
    /// Status pembayaran kunjungan yang menaungi pesanan Lab — kolom <i>Pembayaran</i> dan syarat
    /// <i>Proses Pemeriksaan</i> (<c>LAB-EVD-013</c> butir 5, membuka <c>LAB-DEC-189</c> untuk
    /// Proses saja).
    ///
    /// <para>
    /// <b>Aturan v1 yang diikuti.</b> Pasien asuransi atau penjamin perusahaan selalu lolos —
    /// labelnya <i>Asuransi / Penjamin</i>. Pasien tunai lolos hanya bila tagihannya lunas.
    /// </para>
    ///
    /// <para>
    /// <b>Satuannya kunjungan, bukan pemeriksaan.</b> Billing V2 mencatat pembayaran pada
    /// invoice (<c>BilPaymentAllocation.TargetType = Invoice</c>), dan satu kunjungan memiliki
    /// invoicenya sendiri. Rumus lunasnya disalin dari
    /// <c>BillingInvoiceService.GetPaymentHistoryAsync</c>: tanggungan pasien dari versi
    /// kalkulasi terakhir, dibandingkan dengan seluruh tender <c>SUCCEEDED</c> pada settlement
    /// <c>INVOICE_PAYMENT</c>. Bila invoice belum pernah dikalkulasi, tanggungan diperkirakan
    /// dari baris tagihan aktif supaya kasir tetap melihat nominal.
    /// </para>
    ///
    /// <para>
    /// Hanya membaca. Tagihan Lab baru terbit saat specimen diterima, sehingga sebelum itu
    /// statusnya <c>NotBilled</c> — Proses Pemeriksaan memang belum sah pada tahap itu.
    /// </para>
    /// </summary>
    internal static class LabPaymentClearanceRules
    {
        public const string Paid = "Paid";
        public const string Unpaid = "Unpaid";
        public const string Guaranteed = "Guaranteed";
        public const string NotBilled = "NotBilled";

        internal sealed record Clearance(string Status, bool IsCleared, decimal OutstandingAmount);

        public static async Task<Dictionary<Guid, Clearance>> ReadAsync(
            ApplicationDbContext dbContext,
            IReadOnlyCollection<(Guid EncounterId, EncounterPaymentType? PaymentType)> encounters,
            CancellationToken cancellationToken)
        {
            var hasil = new Dictionary<Guid, Clearance>();
            if (encounters.Count == 0) return hasil;

            foreach (var (encounterId, paymentType) in encounters)
            {
                if (paymentType is EncounterPaymentType.Insurance or EncounterPaymentType.CompanyGuarantor)
                {
                    hasil[encounterId] = new Clearance(Guaranteed, true, 0m);
                }
            }

            var tunaiIds = encounters
                .Select(x => x.EncounterId)
                .Where(id => !hasil.ContainsKey(id))
                .Distinct()
                .ToList();

            if (tunaiIds.Count == 0) return hasil;

            var invoices = await dbContext.BilInvoices
                .AsNoTracking()
                .Where(x => tunaiIds.Contains(x.EncounterId))
                .Select(x => new { x.Id, x.EncounterId, x.Status })
                .ToListAsync(cancellationToken);

            var invoiceIds = invoices.Select(x => x.Id).ToList();

            var kalkulasi = invoiceIds.Count == 0
                ? []
                : await dbContext.BilCalculationVersions
                    .AsNoTracking()
                    .Where(x => invoiceIds.Contains(x.InvoiceId))
                    .Select(x => new { x.InvoiceId, x.VersionNo, x.PatientAmount })
                    .ToListAsync(cancellationToken);

            var tanggunganPerInvoice = kalkulasi
                .GroupBy(x => x.InvoiceId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.VersionNo).First().PatientAmount);

            var perkiraanPerInvoice = invoiceIds.Count == 0
                ? new Dictionary<Guid, decimal>()
                : await dbContext.BilInvoiceItems
                    .AsNoTracking()
                    .Where(x => invoiceIds.Contains(x.InvoiceId) && x.Status == BillingInvoiceItemStatuses.Active)
                    .GroupBy(x => x.InvoiceId)
                    .Select(g => new { InvoiceId = g.Key, Total = g.Sum(x => x.Quantity * x.UnitPrice) })
                    .ToDictionaryAsync(x => x.InvoiceId, x => x.Total, cancellationToken);

            var settlement = invoiceIds.Count == 0
                ? []
                : await dbContext.BilSettlements
                    .AsNoTracking()
                    .Where(s => s.InvoiceId != null && invoiceIds.Contains(s.InvoiceId!.Value) &&
                                s.Purpose == BillingSettlementPurposes.InvoicePayment)
                    .Select(s => new { s.Id, InvoiceId = s.InvoiceId!.Value })
                    .ToListAsync(cancellationToken);

            var settlementKeInvoice = settlement.ToDictionary(s => s.Id, s => s.InvoiceId);
            var settlementIds = settlement.Select(s => s.Id).ToList();

            var dibayarPerInvoice = settlementIds.Count == 0
                ? new Dictionary<Guid, decimal>()
                : (await dbContext.BilTenders
                    .AsNoTracking()
                    .Where(t => settlementIds.Contains(t.SettlementId) && t.Status == BillingTenderStatuses.Succeeded)
                    .Select(t => new { t.SettlementId, t.Amount })
                    .ToListAsync(cancellationToken))
                    .GroupBy(t => settlementKeInvoice[t.SettlementId])
                    .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));

            foreach (var encounterId in tunaiIds)
            {
                var milik = invoices.Where(x => x.EncounterId == encounterId).ToList();
                var sisa = 0m;
                var adaTagihan = false;

                foreach (var invoice in milik)
                {
                    if (invoice.Status is BillingInvoiceStatuses.Closed or BillingInvoiceStatuses.SettledByWriteOff)
                    {
                        adaTagihan = true;
                        continue;
                    }

                    var sudahDikalkulasi = tanggunganPerInvoice.TryGetValue(invoice.Id, out var tanggungan);
                    if (!sudahDikalkulasi)
                    {
                        tanggungan = perkiraanPerInvoice.GetValueOrDefault(invoice.Id);
                    }

                    if (!sudahDikalkulasi && tanggungan <= 0m) continue;

                    adaTagihan = true;
                    var dibayar = dibayarPerInvoice.GetValueOrDefault(invoice.Id);
                    sisa += Math.Max(tanggungan - dibayar, 0m);
                }

                hasil[encounterId] = !adaTagihan
                    ? new Clearance(NotBilled, false, 0m)
                    : sisa > 0m
                        ? new Clearance(Unpaid, false, sisa)
                        : new Clearance(Paid, true, 0m);
            }

            return hasil;
        }
    }
}
