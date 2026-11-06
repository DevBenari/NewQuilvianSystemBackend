using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Services
{
    /// <summary>
    /// Status bayar dan syarat uang muka pesanan Optik (<c>BE-OPT-032</c>, <c>INT-OPT-06</c>, <c>OPT-DEC-008</c>, <c>009</c>,
    /// <c>034</c>, <c>035</c>, <c>044</c>). Hanya membaca Billing; tidak pernah menulis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Bagian pasien per pesanan</b> dibaca dari rincian kalkulasi Billing versi terkini, dengan rumus yang sama dengan
    /// Billing sendiri: <c>(Net − Primary − Unresolved) + (Pajak − PajakPrimary − PajakUnresolved)</c> untuk setiap baris
    /// tagihan milik pesanan ini. Jadi tanggungan penjamin dan diskon sudah diperhitungkan Billing (<c>OPT-DEC-044</c>).
    /// </para>
    /// <para>
    /// <b>Terbayar dibaca per tagihan kunjungan</b> (aturan sementara <c>OPT-DEC-035</c>, sampai alokasi per baris
    /// <c>OPT-APR-003</c> tersedia): bagian pasien tagihan dikurangi sisa menurut <see cref="BillingInvoiceClosureService"/>.
    /// Supaya angka itu tidak tertukar antarpesanan, pesanan ini tertahan selama ada pesanan berpengerjaan lain di kunjungan
    /// yang sama dan sisa tagihan di luar baris pesanan ini belum nol (<c>OPT-VAL-047</c>).
    /// </para>
    /// <para>
    /// <b>Fail-closed.</b> Tagihan tidak ada, kalkulasi belum mengikuti baris pesanan, rincian per baris tidak tersedia pada
    /// pasien berpenjamin, atau basis data tidak terjangkau → <c>503</c>, tidak pernah dianggap lunas.
    /// </para>
    /// </remarks>
    public class OptOrderPaymentGate
    {
        public const string PaidBasisPerInvoice = "PER_INVOICE_TEMPORARY";
        public const string UnavailableMessage =
            "Status pembayaran pesanan belum dapat dibaca dari tagihan. Coba lagi beberapa saat atau minta kasir menghitung ulang tagihan.";

        private static readonly OptOrderStatus[] StatusBerpengerjaan =
            [OptOrderStatus.InFabrication, OptOrderStatus.ReadyForPickup, OptOrderStatus.NotCollected];

        private readonly ApplicationDbContext _dbContext;
        private readonly BillingInvoiceClosureService _closureService;

        public OptOrderPaymentGate(ApplicationDbContext dbContext, BillingInvoiceClosureService closureService)
        {
            _dbContext = dbContext;
            _closureService = closureService;
        }

        /// <summary>Minimum uang muka: persentase beku × bagian pasien, dibulatkan ke atas ke rupiah penuh.</summary>
        public static decimal MinimumDownPayment(decimal percent, decimal patientAmount) =>
            patientAmount <= 0 ? 0m : Math.Ceiling(percent / 100m * patientAmount);

        public async Task<OptResult<OptOrderPaymentStatusResponse>> GetStatusAsync(OptOrder order, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(order);

            // Remake garansi tidak ditagih dan dikirim tanpa uang muka (OPT-STATE-v1 bagian 3).
            if (order.IsRemake)
                return OptResult<OptOrderPaymentStatusResponse>.Ok(new OptOrderPaymentStatusResponse
                {
                    OrderId = order.Id, OrderNumber = order.OrderNumber, DownPaymentMinPercent = 0m,
                    IsDownPaymentMet = true, IsInvoiceSettled = true,
                });
            if (order.BilledAt is null)
                return OptResult<OptOrderPaymentStatusResponse>.Conflict(OptErrorCodes.Val057,
                    "Pesanan belum ditagihkan, sehingga belum ada status pembayaran.");

            try
            {
                var invoice = await _dbContext.Set<BilInvoice>().AsNoTracking()
                    .Where(x => x.EncounterId == order.EncounterId && !x.IsDelete)
                    .OrderByDescending(x => x.CreateDateTime)
                    .FirstOrDefaultAsync(cancellationToken);
                if (invoice is null)
                    return Unavailable();

                var calculation = await _dbContext.Set<BilCalculationVersion>().AsNoTracking()
                    .FirstOrDefaultAsync(x => x.InvoiceId == invoice.Id && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete, cancellationToken);
                if (calculation is null)
                    return Unavailable();

                var breakdown = BillingCalculationService.DeserializeBreakdown(calculation.BreakdownSnapshot);
                var berpenjamin = calculation.PrimaryAmount + calculation.ExcessAmount > 0;
                if (berpenjamin && !breakdown.Coverage.IsPerItemAllocationAvailable)
                    return Unavailable();

                var itemIds = order.Items.Select(x => x.Id.ToString("D")).ToList();
                var baris = await _dbContext.Set<BilInvoiceItem>().AsNoTracking()
                    .Where(x => x.InvoiceId == invoice.Id && x.SourceDomain == BillingBridgeSourceDomains.Optical
                                && x.Status == BillingInvoiceItemStatuses.Active && !x.IsDelete && itemIds.Contains(x.SourceDetailId))
                    .Select(x => x.Id)
                    .ToListAsync(cancellationToken);

                var rincian = breakdown.Items.ToDictionary(x => x.InvoiceItemId);
                if (baris.Any(id => !rincian.ContainsKey(id)))
                    return Unavailable(); // kalkulasi belum mengikuti baris pesanan

                var bagianPasien = baris.Sum(id => PatientPortion(rincian[id]));
                var sisa = await _closureService.CalculateOutstandingAsync(invoice, calculation, cancellationToken);
                var terbayar = Math.Max(0m, calculation.PatientAmount - sisa);
                var minimum = MinimumDownPayment(order.DownPaymentMinPercentSnapshot, bagianPasien);

                string? penahan = null;
                var sisaDiLuarPesanan = Math.Max(0m, calculation.PatientAmount - bagianPasien - terbayar);
                if (sisaDiLuarPesanan > 0)
                {
                    penahan = await _dbContext.OptOrders.AsNoTracking()
                        .Where(x => x.EncounterId == order.EncounterId && x.Id != order.Id && !x.IsDelete && !x.IsRemake
                                    && StatusBerpengerjaan.Contains(x.Status))
                        .OrderBy(x => x.CreateDateTime)
                        .Select(x => x.OrderNumber)
                        .FirstOrDefaultAsync(cancellationToken);
                }

                return OptResult<OptOrderPaymentStatusResponse>.Ok(new OptOrderPaymentStatusResponse
                {
                    OrderId = order.Id,
                    OrderNumber = order.OrderNumber,
                    InvoiceId = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    OrderPatientAmount = bagianPasien,
                    DownPaymentMinPercent = order.DownPaymentMinPercentSnapshot,
                    DownPaymentMinAmount = minimum,
                    InvoicePaidAmount = terbayar,
                    InvoiceOutstandingAmount = sisa,
                    IsDownPaymentMet = bagianPasien <= 0 || terbayar >= minimum,
                    IsInvoiceSettled = sisa <= 0,
                    BlockingOrderNumber = penahan,
                });
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                return Unavailable();
            }
        }

        /// <summary>Porsi pasien satu baris, sama dengan rumus Billing (<c>BillingPayerEditService</c>).</summary>
        public static decimal PatientPortion(BillingManagement.Billing.Dtos.CalculationItemResponse x) =>
            (x.NetAmount - x.ItemPrimaryAmount - x.ItemUnresolvedAmount) + (x.TaxAmount - x.TaxPrimaryAmount - x.TaxUnresolvedAmount);

        private static OptResult<OptOrderPaymentStatusResponse> Unavailable() =>
            OptResult<OptOrderPaymentStatusResponse>.Unavailable(OptErrorCodes.BillingUnavailable, UnavailableMessage);
    }
}
