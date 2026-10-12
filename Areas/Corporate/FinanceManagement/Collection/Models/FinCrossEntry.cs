using QuilvianSystemBackend.Areas.Administrator.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Aggregate root Ayat Silang (Cross-Entry / Unidentified Payer Receipt).
/// Penerimaan uang dari pihak asuransi/penjamin yang sudah masuk ke rekening rumah sakit
/// tetapi rincian invoice/piutang yang dibayar belum diketahui.
///
/// Saldo tersedia TIDAK DISIMPAN TERPISAH (AvailableBalance tidak ada kolomnya),
/// melainkan berasal langsung dari FinReceipt.UnallocatedAmount (authoritative single source of truth).
/// </summary>
[Table("FinCrossEntry", Schema = "public")]
public sealed class FinCrossEntry : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>No. Ayat Silang resmi (misal: AS-100626-0004). Wajib diisi pengguna dan unik.</summary>
    [Required, MaxLength(50)]
    public string CrossEntryNumber { get; set; } = string.Empty;

    /// <summary>No. Referensi rekening koran bank.</summary>
    [Required, MaxLength(150)]
    public string ReferenceNumber { get; set; } = string.Empty;

    /// <summary>Rujukan penjamin asuransi pembayar.</summary>
    public Guid InsuranceProviderId { get; set; }
    public MstInsuranceProvider? InsuranceProvider { get; set; }

    /// <summary>Rujukan rekening bank rumah sakit penerima.</summary>
    public Guid BankAccountId { get; set; }
    public MstBankAccount? BankAccount { get; set; }

    /// <summary>Rujukan relasi 1:1 ke FinReceipt yang dibuat bersamaan.</summary>
    public Guid ReceiptId { get; set; }
    public FinReceipt? Receipt { get; set; }

    /// <summary>Nominal asli uang masuk (immutable snapshot, selalu sama dengan Receipt.Amount).</summary>
    public decimal OriginalAmount { get; set; }

    /// <summary>Tanggal transaksi mutasi bank.</summary>
    public DateOnly TransactionDate { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    /// <summary>Status Ayat Silang: OPEN, PARTIALLY_USED, FULLY_USED, CANCELLED.</summary>
    [Required, MaxLength(30)]
    public string Status { get; set; } = FinCrossEntryStatuses.Open;

    /// <summary>Optimistic concurrency token.</summary>
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinCrossEntryTransaction> Transactions { get; set; } = new List<FinCrossEntryTransaction>();
    public ICollection<FinCrossEntryDocument> Documents { get; set; } = new List<FinCrossEntryDocument>();
}

public static class FinCrossEntryStatuses
{
    public const string Open = "OPEN";
    public const string PartiallyUsed = "PARTIALLY_USED";
    public const string FullyUsed = "FULLY_USED";
    public const string Cancelled = "CANCELLED";
}
