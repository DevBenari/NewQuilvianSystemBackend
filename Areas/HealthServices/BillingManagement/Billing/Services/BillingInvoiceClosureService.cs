using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// BKC-DES-028: satu tempat untuk perhitungan sisa tagihan pasien dan penyelarasan status
/// penutupan invoice (FINAL/CLOSED, BKC-DEC-100/BKC-DEC-104/BKC-DES-031). Tidak pernah membuka
/// transaction sendiri dan tidak pernah memanggil SaveChangesAsync sendiri (BKC-DES-030) -
/// pemanggil memegang batas transaksi dan SaveChanges, mengikuti pola
/// BillingArApHandoffService.StageHandoffsForFinalizationAsync. Perhitungan sisa tagihan
/// dipindahkan apa adanya dari salinan privat BillingFinalizationService dan
/// BillingFinancialExceptionService (§ 21 capability map mencatat keduanya identik).
/// </summary>
public sealed class BillingInvoiceClosureService
{
    private readonly ApplicationDbContext _dbContext;

    public BillingInvoiceClosureService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Perhitungan murni atas versi kalkulasi yang sudah dipegang pemanggil. TIDAK melempar
    /// exception untuk versi kalkulasi yang hilang - pemanggil yang bertanggung jawab
    /// memastikan versinya ada sebelum memanggil overload ini.
    /// </summary>
    public async Task<decimal> CalculateOutstandingAsync(
        BilInvoice invoice,
        BilCalculationVersion calculation,
        CancellationToken cancellationToken)
    {
        var paidAmount = await _dbContext.BilPaymentAllocations.AsNoTracking()
            .Where(x => x.TargetType == BillingAllocationTargetTypes.Invoice
                && x.TargetId == invoice.Id && !x.IsDelete)
            .SumAsync(
                x => (decimal?)(x.ReversesAllocationId.HasValue ? -x.Amount : x.Amount),
                cancellationToken) ?? 0;
        var allocationExcess = await _dbContext.BilRefundableCredits.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.SourceType == BillingRefundableCreditSourceTypes.AllocationExcess && !x.IsDelete)
            .SumAsync(x => (decimal?)x.AvailableAmount, cancellationToken) ?? 0;
        // BE-BKC-029/BKC-DES-024: writeOffTotal HANYA menyaring kategori PATIENT_AR - write-off
        // residual non-billable tidak pernah mengurangi piutang pasien, tidak berubah oleh
        // konsolidasi BKC-DES-028.
        var writeOffTotal = await _dbContext.BilWriteOffCases.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.Status == BillingWriteOffCaseStatuses.Posted
                && x.Category == BillingWriteOffCategories.PatientAr && !x.IsDelete)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        // BKC-DES-024: reversal atas write-off residual dikecualikan dari adjustmentNet - satu
        // paket dengan penyaringan writeOffTotal di atas.
        var residualCaseIds = await _dbContext.BilWriteOffCases.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id && x.Category == BillingWriteOffCategories.NonBillableResidual)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);
        var adjustmentNet = await _dbContext.BilAdjustments.AsNoTracking()
            .Where(x => x.InvoiceId == invoice.Id
                && x.Status == BillingAdjustmentStatuses.Posted && !x.IsDelete
                && (x.ReversesWriteOffCaseId == null
                    || !residualCaseIds.Contains(x.ReversesWriteOffCaseId.Value)))
            .SumAsync(
                x => (decimal?)(x.Direction == BillingAdjustmentDirections.Credit ? x.Amount : -x.Amount),
                cancellationToken) ?? 0;
        return Math.Max(
            calculation.PatientAmount - paidAmount + allocationExcess - writeOffTotal - adjustmentNet, 0);
    }

    /// <summary>
    /// Overload yang mencari versi kalkulasi current sendiri. Melempar
    /// <see cref="BillingInvoiceClosureValidationException"/> bila belum ada. Pemanggil yang
    /// butuh pesan bertipe domainnya sendiri MUST menangkap dan membungkus ulang pesannya,
    /// mengikuti pola BillingSettlementService.ReconcileTenderAsync terhadap
    /// BillingAllocationValidationException - supaya pesan yang sampai ke layar tetap identik.
    /// </summary>
    public async Task<decimal> CalculateOutstandingAsync(
        BilInvoice invoice,
        CancellationToken cancellationToken)
    {
        var calculation = await RequireCurrentCalculationAsync(invoice, cancellationToken);
        return await CalculateOutstandingAsync(invoice, calculation, cancellationToken);
    }

    /// <summary>
    /// BKC-DEC-100/BKC-DES-031: menyelaraskan FINAL/CLOSED berdasarkan sisa tagihan pasien.
    /// MUST dipanggil sesudah SaveChangesAsync milik peristiwa pemicu (BKC-DES-030) supaya
    /// perhitungan AsNoTracking di atas melihat baris yang baru saja ditulis pemanggil.
    /// Pemanggil bertanggung jawab memanggil SaveChangesAsync lagi sesudah method ini selesai.
    /// Idempotent: keadaan yang tidak berubah tidak menulis apa pun (BIL-VAL-108).
    /// </summary>
    public async Task<InvoiceClosureChange> SyncClosureAsync(
        Guid invoiceId,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        var invoice = await _dbContext.BilInvoices
            .SingleOrDefaultAsync(x => x.Id == invoiceId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

        // BIL-VAL-108: OPEN dan SETTLED_BY_WRITE_OFF bukan urusan penyelarasan ini.
        if (invoice.Status != BillingInvoiceStatuses.Final
            && invoice.Status != BillingInvoiceStatuses.Closed)
            return InvoiceClosureChange.None(invoiceId, invoice.Status);

        // BKC-DES-032: kunci event-specific (BIL_TENDER_*, BIL_ADJUSTMENT_*, dst.) MUST sudah
        // diambil pemanggil sebelum memanggil method ini; kunci ledger invoice menyusul di sini.
        await AcquireInvoiceLedgerLockAsync(invoiceId, cancellationToken);

        var calculation = await RequireCurrentCalculationAsync(invoice, cancellationToken);
        var outstanding = await CalculateOutstandingAsync(invoice, calculation, cancellationToken);

        var statusBefore = invoice.Status;
        if (invoice.Status == BillingInvoiceStatuses.Final && outstanding <= 0)
        {
            invoice.Status = BillingInvoiceStatuses.Closed;
            invoice.ClosedAt = occurredAt;
        }
        else if (invoice.Status == BillingInvoiceStatuses.Closed && outstanding > 0)
        {
            // BKC-DES-031: transisi balik. Tujuannya FINAL, bukan OPEN - yang dibatalkan adalah
            // pembayarannya, bukan finalisasinya; versi kalkulasi tetap terkunci.
            invoice.Status = BillingInvoiceStatuses.Final;
            invoice.ClosedAt = null;
        }
        else
        {
            return InvoiceClosureChange.None(invoiceId, invoice.Status);
        }

        invoice.RowVersion = Guid.NewGuid();
        invoice.UpdateDateTime = DateTime.UtcNow;
        invoice.UpdateBy = actorUserId;

        return new InvoiceClosureChange(invoiceId, statusBefore, invoice.Status, outstanding, true);
    }

    private async Task<BilCalculationVersion> RequireCurrentCalculationAsync(
        BilInvoice invoice, CancellationToken cancellationToken) =>
        await _dbContext.BilCalculationVersions.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.InvoiceId == invoice.Id
                    && x.VersionNo == invoice.CurrentCalculationVersion && !x.IsDelete,
                cancellationToken)
            ?? throw new BillingInvoiceClosureValidationException(
                "Invoice belum memiliki hasil perhitungan terkini.");

    private Task AcquireInvoiceLedgerLockAsync(Guid invoiceId, CancellationToken cancellationToken) =>
        _dbContext.Database.IsRelational()
            ? _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INVOICE_LEDGER_{invoiceId:N}"],
                cancellationToken)
            : Task.CompletedTask;
}

/// <summary>
/// Hasil kecil, tidak dipersist - memberi tahu pemanggil apakah status invoice berpindah,
/// supaya pemanggil dapat menulis audit sesudah commit (mengikuti pola
/// BillingAllocationService.SettlementAllocationResult).
/// </summary>
public readonly record struct InvoiceClosureChange(
    Guid InvoiceId,
    string StatusBefore,
    string StatusAfter,
    decimal Outstanding,
    bool Changed)
{
    public static InvoiceClosureChange None(Guid invoiceId, string status) =>
        new(invoiceId, status, status, 0, false);
}

public abstract class BillingInvoiceClosureException : Exception
{
    protected BillingInvoiceClosureException(string message) : base(message) { }
}

public sealed class BillingInvoiceClosureValidationException(string message)
    : BillingInvoiceClosureException(message);
