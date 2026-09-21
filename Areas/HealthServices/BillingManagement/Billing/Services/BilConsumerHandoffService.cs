using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Cashier.Models;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Repositories;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Cryptography;
using System.Text;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;

/// <summary>
/// BKC-DES-038: Satu service penerbit fakta ke konsumen hilir (Finance dan Farmasi).
/// Tidak pernah membuka transaksi sendiri dan selalu berjalan di dalam batas transaksi pemanggil,
/// sehingga uang dan suratnya tidak pernah terpisah nasib (BKC-DEC-106).
/// Memakai ulang kunci penasihat tagihan yang sudah ada (BKC-DES-032).
/// </summary>
public sealed class BilConsumerHandoffService
{
    private const string LogCategory = "HealthServices.BillingManagement.Billing";
    private readonly ApplicationDbContext _dbContext;
    private readonly LoggerService _loggerService;

    public BilConsumerHandoffService(
        ApplicationDbContext dbContext,
        LoggerService loggerService)
    {
        _dbContext = dbContext;
        _loggerService = loggerService;
    }

    /// <summary>
    /// Menerbitkan surat penerimaan uang (BilCollectionHandoff) untuk Finance saat tender mencapai
    /// terminal status (SUCCEEDED atau REVERSED).
    /// Mematuhi BIL-INT-013, BIL-VAL-110 (idempotensi), dan BIL-VAL-111 (shift kasir wajib untuk tunai).
    /// </summary>
    public async Task<BilCollectionHandoff?> PublishForTenderAsync(
        BilTender tender,
        Guid actorUserId,
        DateTimeOffset occurredAt,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(tender);

        if (tender.Status != BillingTenderStatuses.Succeeded
            && tender.Status != BillingTenderStatuses.Reversed)
        {
            return null;
        }

        var settlement = tender.Settlement
            ?? await _dbContext.BilSettlements.SingleOrDefaultAsync(
                x => x.Id == tender.SettlementId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Settlement tidak ditemukan.");

        if (!settlement.InvoiceId.HasValue)
        {
            // Settlement tanpa InvoiceId (misalnya DepositTopUp) tidak memiliki invoice penagihan
            // untuk diterbitkan surat penerimaan tagihannya.
            return null;
        }

        var invoice = await _dbContext.BilInvoices.SingleOrDefaultAsync(
            x => x.Id == settlement.InvoiceId.Value && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Invoice tidak ditemukan.");

        var paymentMethod = await _dbContext.MstPaymentMethods.SingleOrDefaultAsync(
            x => x.Id == tender.PaymentMethodId && !x.IsDelete, cancellationToken)
            ?? throw new KeyNotFoundException("Metode pembayaran tidak ditemukan.");

        // BIL-VAL-111: Tender tunai wajib membawa identitas shift kasir.
        // Bila tidak membawa shift, penerbitan ditolak beserta transaksinya.
        var isCash = paymentMethod.IsCash
            || string.Equals(paymentMethod.PaymentMethodType, "Cash", StringComparison.OrdinalIgnoreCase);

        if (isCash && !tender.CashierShiftId.HasValue)
        {
            throw new BillingConsumerHandoffValidationException(
                "Pembayaran tunai tidak dapat diteruskan ke pembukuan karena shift kasirnya tidak diketahui. Tutup dan buka kembali shift, lalu ulangi");
        }

        // BIL-VAL-110: Satu tender satu surat per keadaan (idempotensi).
        var existingByTenderAndStatus = await _dbContext.BilCollectionHandoffs
            .FirstOrDefaultAsync(
                x => x.TenderId == tender.Id && x.TenderStatus == tender.Status && !x.IsDelete,
                cancellationToken);

        if (existingByTenderAndStatus != null)
        {
            return existingByTenderAndStatus;
        }

        var handoffKey = ComputeHandoffKey(tender.Id, tender.Status);
        var existingByKey = await _dbContext.BilCollectionHandoffs
            .FirstOrDefaultAsync(
                x => x.HandoffKey == handoffKey && !x.IsDelete,
                cancellationToken);

        if (existingByKey != null)
        {
            return existingByKey;
        }

        // BKC-DES-032: Mengambil kunci penasihat tagihan di dalam transaksi berjalan
        if (_dbContext.Database.IsRelational())
        {
            await _dbContext.Database.ExecuteSqlRawAsync(
                "SELECT pg_advisory_xact_lock(hashtext({0}));",
                [$"BIL_INVOICE_LEDGER_{invoice.Id:N}"],
                cancellationToken);
        }

        var allocationIds = await _dbContext.BilPaymentAllocations
            .Where(x => x.SettlementId == tender.SettlementId && !x.IsDelete)
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var paymentAllocationIds = allocationIds.Count > 0
            ? string.Join(",", allocationIds)
            : null;

        var handoff = new BilCollectionHandoff
        {
            Id = Guid.NewGuid(),
            TenderId = tender.Id,
            SettlementId = tender.SettlementId,
            InvoiceId = invoice.Id,
            PaymentAllocationIds = paymentAllocationIds,
            PaymentMethodId = tender.PaymentMethodId,
            PaymentMethodAccountId = tender.PaymentMethodAccountId,
            Amount = tender.Amount,
            KwitansiNumber = tender.KwitansiNumber,
            CashierShiftId = tender.CashierShiftId,
            ProviderReference = tender.ProviderReference,
            ProviderEventId = tender.LastProviderEventId,
            OccurredAt = occurredAt,
            SourceInvoiceStatus = invoice.Status,
            TenderStatus = tender.Status,
            HandoffKey = handoffKey,
            CorrelationId = tender.CorrelationId,
            CausationId = tender.CausationId,
            Status = BillingHandoffStatuses.Created,
            AcknowledgedAt = null,
            RowVersion = Guid.NewGuid(),
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.BilCollectionHandoffs.Add(handoff);

        await _loggerService.AuditAsync(
            LogCategory,
            "BillingCollectionHandoff.Created",
            "Surat penerimaan uang diterbitkan untuk Finance.",
            new
            {
                HandoffId = handoff.Id,
                handoff.TenderId,
                handoff.SettlementId,
                handoff.InvoiceId,
                handoff.TenderStatus,
                handoff.Amount,
                handoff.HandoffKey,
                ActorUserId = actorUserId
            });

        return handoff;
    }

    /// <summary>
    /// Kunci handoff diturunkan secara deterministik dari pasangan TenderId dan TenderStatus.
    /// </summary>
    public static Guid ComputeHandoffKey(Guid tenderId, string tenderStatus)
    {
        var raw = Encoding.UTF8.GetBytes($"BIL_COLLECTION_{tenderId:N}_{tenderStatus}");
        var hash = MD5.HashData(raw);
        return new Guid(hash);
    }
}

public abstract class BillingConsumerHandoffException : Exception
{
    protected BillingConsumerHandoffException(string message) : base(message) { }
}

public sealed class BillingConsumerHandoffValidationException(string message)
    : BillingConsumerHandoffException(message);
