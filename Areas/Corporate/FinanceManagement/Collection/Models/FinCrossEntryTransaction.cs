using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Mutasi append-only riwayat transaksi Ayat Silang.
/// Mencatat initial receipt (kredit) dan alokasi pelunasan AR (debet).
/// Baris historis tidak pernah diedit; koreksi membuat baris baru.
/// </summary>
[Table("FinCrossEntryTransaction", Schema = "public")]
public sealed class FinCrossEntryTransaction : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CrossEntryId { get; set; }
    public FinCrossEntry? CrossEntry { get; set; }

    /// <summary>Jenis transaksi: INITIAL_RECEIPT, AR_ALLOCATION, AR_ALLOCATION_REVERSAL.</summary>
    [Required, MaxLength(40)]
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>Arah mutasi: CREDIT (penambahan saldo) atau DEBIT (pengurangan saldo/alokasi).</summary>
    [Required, MaxLength(10)]
    public string Direction { get; set; } = string.Empty;

    /// <summary>Nominal mutasi (selalu positif > 0).</summary>
    public decimal Amount { get; set; }

    /// <summary>Saldo tersedia setelah transaksi ini selesai (>= 0).</summary>
    public decimal BalanceAfterTransaction { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    /// <summary>Rujukan ke FinReceiptAllocation saat mutasi berasal dari alokasi AR.</summary>
    public Guid? ReceiptAllocationId { get; set; }
    public FinReceiptAllocation? ReceiptAllocation { get; set; }

    /// <summary>Rujukan self-reference ke transaksi asal bila ini merupakan reversal.</summary>
    public Guid? ReversalOfTransactionId { get; set; }
    public FinCrossEntryTransaction? ReversalOfTransaction { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}

public static class FinCrossEntryTransactionTypes
{
    public const string InitialReceipt = "INITIAL_RECEIPT";
    public const string ArAllocation = "AR_ALLOCATION";
    public const string ArAllocationReversal = "AR_ALLOCATION_REVERSAL";
}

public static class FinCrossEntryDirections
{
    public const string Credit = "CREDIT";
    public const string Debit = "DEBIT";
}
