using Microsoft.EntityFrameworkCore;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Repositories;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Layanan terpusat satu-satunya penulis buku mutasi subledger Finance (FIN-DES-079, FIN-DEC-123).
/// Mencakup mutasi piutang (FinReceivableMovement), utang supplier (FinSupplierPayableMovement),
/// dan kas (FinCashMovement).
///
/// Service ini MUST NOT membuka, commit, atau rollback transaksi sendiri — pemanggil WAJIB
/// sudah berada di dalam transaksi database aktif dan memegang advisory lock agregatnya.
/// </summary>
public sealed class FinanceSubledgerMovementService
{
    private readonly ApplicationDbContext _dbContext;

    public FinanceSubledgerMovementService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Mencatat satu baris mutasi piutang (FinReceivableMovement).
    /// Invariant: BalanceAfter = BalanceBefore + deltaAmount (FIN-VAL-165, ditegakkan CK_FinReceivableMovement_Balance di DB).
    /// Invariant: BalanceAfter MUST sama dengan receivable.OutstandingAmount saat ini (FIN-VAL-167).
    /// Menegakkan FIN-VAL-166 (advisory lock/transaksi aktif) dan FIN-VAL-171 (metode pembayaran).
    /// </summary>
    public Task<FinReceivableMovement> RecordReceivableMovementAsync(
        FinReceivable receivable,
        string movementType,
        decimal deltaAmount,
        decimal balanceBefore,
        DateTimeOffset occurredAt,
        Guid actorUserId,
        Guid correlationId,
        Guid causationId,
        Guid? sourceAllocationId = null,
        string? paymentMethodCode = null,
        string? fundingSourceType = null,
        Guid? fundingSourceId = null,
        string? referenceNumber = null,
        Guid? proofId = null,
        Guid? openingItemBatchId = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(receivable);
        if (string.IsNullOrWhiteSpace(movementType))
            throw new ArgumentException("MovementType wajib diisi.", nameof(movementType));

        // FIN-VAL-166: Pemanggil wajib sudah berada di dalam transaksi aktif (dan memegang advisory lock)
        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new FinanceSubledgerException("Perubahan saldo sedang diproses. Coba lagi beberapa saat.", 409);
        }

        // FIN-VAL-171: Metode pembayaran hanya berlaku untuk mutasi pembayaran
        var isPayment = movementType == FinReceivableMovementTypes.PembayaranLangsung;
        if (!isPayment && (!string.IsNullOrWhiteSpace(paymentMethodCode) || !string.IsNullOrWhiteSpace(fundingSourceType) || proofId.HasValue))
        {
            throw new FinanceSubledgerException("Metode pembayaran hanya berlaku untuk mutasi pembayaran.", 400);
        }

        // Check constraint DB: CK_FinReceivableMovement_FundingSource ("PaymentMethodCode" IS NULL OR "FundingSourceType" IS NOT NULL)
        if (!string.IsNullOrWhiteSpace(paymentMethodCode) && string.IsNullOrWhiteSpace(fundingSourceType))
        {
            throw new FinanceSubledgerException("FundingSourceType wajib diisi bila PaymentMethodCode terisi.", 400);
        }

        var balanceAfter = balanceBefore + deltaAmount;

        // FIN-VAL-165: BalanceAfter tidak sama dengan BalanceBefore + deltaAmount (500)
        if (balanceAfter != balanceBefore + deltaAmount)
        {
            throw new InvalidOperationException("Perhitungan saldo mutasi tidak konsisten. Hubungi pengelola sistem.");
        }

        // FIN-VAL-167: BalanceAfter baris terakhir berbeda dari OutstandingAmount agregatnya (500)
        if (receivable.OutstandingAmount != balanceAfter)
        {
            throw new InvalidOperationException("Saldo buku mutasi tidak cocok dengan sisa tagihan. Hubungi pengelola sistem.");
        }

        // Tanggal bisnis WIB (BE-FIN-058, BE-FIN-059, FIN-DES-082)
        var businessDate = FinanceBusinessDate.ToDateOnly(occurredAt);

        var movement = new FinReceivableMovement
        {
            Id = Guid.NewGuid(),
            ReceivableId = receivable.Id,
            MovementType = movementType,
            Amount = deltaAmount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            BusinessDate = businessDate,
            OccurredAt = occurredAt,
            SourceAllocationId = sourceAllocationId,
            PaymentMethodCode = paymentMethodCode,
            FundingSourceType = fundingSourceType,
            FundingSourceId = fundingSourceId,
            ReferenceNumber = referenceNumber,
            ProofId = proofId,
            OpeningItemBatchId = openingItemBatchId,
            Notes = notes,
            CorrelationId = correlationId,
            CausationId = causationId,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.FinReceivableMovements.Add(movement);
        return Task.FromResult(movement);
    }

    /// <summary>
    /// Mencatat satu baris mutasi utang supplier (FinSupplierPayableMovement).
    /// Invariant: BalanceAfter = BalanceBefore + deltaAmount (FIN-VAL-165, ditegakkan CK_FinSupplierPayableMovement_Balance di DB).
    /// Invariant: BalanceAfter MUST sama dengan payable.OutstandingAmount saat ini (FIN-VAL-167).
    /// Menegakkan FIN-VAL-166 (advisory lock/transaksi aktif) dan FIN-VAL-171 (metode pembayaran).
    /// </summary>
    public Task<FinSupplierPayableMovement> RecordSupplierPayableMovementAsync(
        FinSupplierPayable payable,
        string movementType,
        decimal deltaAmount,
        decimal balanceBefore,
        DateTimeOffset occurredAt,
        Guid actorUserId,
        Guid correlationId,
        Guid causationId,
        DateOnly? businessDateOverride = null,
        Guid? paymentId = null,
        Guid? paymentAllocationId = null,
        string? paymentMethodCode = null,
        string? fundingSourceType = null,
        Guid? fundingSourceId = null,
        string? referenceNumber = null,
        Guid? proofId = null,
        Guid? openingItemBatchId = null,
        string? notes = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(payable);
        if (string.IsNullOrWhiteSpace(movementType))
            throw new ArgumentException("MovementType wajib diisi.", nameof(movementType));

        // FIN-VAL-166: Pemanggil wajib sudah berada di dalam transaksi aktif (dan memegang advisory lock)
        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new FinanceSubledgerException("Perubahan saldo sedang diproses. Coba lagi beberapa saat.", 409);
        }

        // FIN-VAL-171: Metode pembayaran hanya berlaku untuk mutasi pembayaran
        var isPayment = movementType is FinSupplierPayableMovementTypes.PembayaranDokumen or FinSupplierPayableMovementTypes.PembayaranLangsung;
        if (!isPayment && (!string.IsNullOrWhiteSpace(paymentMethodCode) || !string.IsNullOrWhiteSpace(fundingSourceType) || proofId.HasValue))
        {
            throw new FinanceSubledgerException("Metode pembayaran hanya berlaku untuk mutasi pembayaran.", 400);
        }

        var balanceAfter = balanceBefore + deltaAmount;

        // FIN-VAL-165: BalanceAfter tidak sama dengan BalanceBefore + deltaAmount (500)
        if (balanceAfter != balanceBefore + deltaAmount)
        {
            throw new InvalidOperationException("Perhitungan saldo mutasi tidak konsisten. Hubungi pengelola sistem.");
        }

        // FIN-VAL-167: BalanceAfter baris terakhir berbeda dari OutstandingAmount agregatnya (500)
        if (payable.OutstandingAmount != balanceAfter)
        {
            throw new InvalidOperationException("Saldo buku mutasi tidak cocok dengan sisa tagihan. Hubungi pengelola sistem.");
        }

        // Tanggal bisnis WIB: disalin dari override bila ada (misal ApprovedAt pembayaran), atau dari occurredAt
        var businessDate = businessDateOverride ?? FinanceBusinessDate.ToDateOnly(occurredAt);

        var movement = new FinSupplierPayableMovement
        {
            Id = Guid.NewGuid(),
            SupplierPayableId = payable.Id,
            MovementType = movementType,
            Amount = deltaAmount,
            BalanceBefore = balanceBefore,
            BalanceAfter = balanceAfter,
            BusinessDate = businessDate,
            OccurredAt = occurredAt,
            PaymentId = paymentId,
            PaymentAllocationId = paymentAllocationId,
            PaymentMethodCode = paymentMethodCode,
            FundingSourceType = fundingSourceType,
            FundingSourceId = fundingSourceId,
            ReferenceNumber = referenceNumber,
            ProofId = proofId,
            OpeningItemBatchId = openingItemBatchId,
            Notes = notes,
            CorrelationId = correlationId,
            CausationId = causationId,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.FinSupplierPayableMovements.Add(movement);
        return Task.FromResult(movement);
    }

    /// <summary>
    /// Mencatat satu baris mutasi kas (FinCashMovement) — FIN-DES-081, FIN-DEC-124..127, 132..133.
    /// Invariant: Amount selalu positif (> 0, FIN-VAL-168, ditegakkan CK_FinCashMovement_Amount di DB).
    /// Satu-satunya pengecualian: mutasi SALDO-AWAL boleh bernilai nol (saldo awal kas kosong); nilai negatif tetap ditolak.
    /// Direction: IN / OUT (ditegakkan CK_FinCashMovement_Direction di DB).
    /// Menegakkan FIN-VAL-166 (transaksi aktif) dan FIN-VAL-169 (idempotensi via unique index IX_FinCashMovement_Source).
    /// </summary>
    public async Task<FinCashMovement?> RecordCashMovementAsync(
        string movementType,
        string direction,
        decimal amount,
        DateOnly businessDate,
        DateTimeOffset occurredAt,
        string sourceReferenceType,
        string sourceReferenceId,
        Guid actorUserId,
        Guid correlationId,
        Guid causationId,
        Guid? cashierShiftId = null,
        string? paymentMethodCode = null,
        string? notes = null,
        bool ignoreDuplicate = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(movementType))
            throw new ArgumentException("MovementType wajib diisi.", nameof(movementType));

        if (direction is not (FinCashMovementDirections.In or FinCashMovementDirections.Out))
            throw new ArgumentException("Direction mutasi kas harus IN atau OUT.", nameof(direction));

        // FIN-VAL-168: Nominal mutasi kas harus lebih besar dari nol (400); mutasi SALDO-AWAL boleh nol
        var allowsZeroAmount = movementType == FinCashMovementTypes.SaldoAwal;
        if (amount < 0m || (amount == 0m && !allowsZeroAmount))
        {
            throw new FinanceSubledgerException("Nominal mutasi kas harus lebih besar dari nol.", 400);
        }

        if (string.IsNullOrWhiteSpace(sourceReferenceType))
            throw new ArgumentException("SourceReferenceType wajib diisi.", nameof(sourceReferenceType));

        if (string.IsNullOrWhiteSpace(sourceReferenceId))
            throw new ArgumentException("SourceReferenceId wajib diisi.", nameof(sourceReferenceId));

        // FIN-VAL-166: Pemanggil wajib sudah berada di dalam transaksi aktif
        if (_dbContext.Database.CurrentTransaction is null)
        {
            throw new FinanceSubledgerException("Perubahan saldo sedang diproses. Coba lagi beberapa saat.", 409);
        }

        // FIN-VAL-169: Idempotensi pasangan (SourceReferenceType, SourceReferenceId, MovementType)
        var duplicate = await _dbContext.FinCashMovements.AsNoTracking()
            .AnyAsync(x => x.SourceReferenceType == sourceReferenceType
                        && x.SourceReferenceId == sourceReferenceId
                        && x.MovementType == movementType
                        && !x.IsDelete, cancellationToken);
        if (duplicate)
        {
            if (ignoreDuplicate) return null;
            throw new FinanceSubledgerException("Mutasi kas untuk sumber ini sudah tercatat.", 409);
        }

        var movement = new FinCashMovement
        {
            Id = Guid.NewGuid(),
            MovementType = movementType,
            Direction = direction,
            Amount = amount,
            BusinessDate = businessDate,
            OccurredAt = occurredAt,
            SourceReferenceType = sourceReferenceType,
            SourceReferenceId = sourceReferenceId,
            CashierShiftId = cashierShiftId,
            PaymentMethodCode = paymentMethodCode,
            Notes = notes,
            CorrelationId = correlationId,
            CausationId = causationId,
            CreateDateTime = DateTime.UtcNow,
            CreateBy = actorUserId
        };

        _dbContext.FinCashMovements.Add(movement);
        return movement;
    }
}

/// <summary>
/// Pengecualian khusus operasi subledger Finance yang membawa kode status HTTP.
/// </summary>
public sealed class FinanceSubledgerException : Exception
{
    public int StatusCode { get; }

    public FinanceSubledgerException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}
