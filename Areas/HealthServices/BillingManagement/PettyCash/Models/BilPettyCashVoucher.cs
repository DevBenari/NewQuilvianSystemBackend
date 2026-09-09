using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

/// <summary>
/// Satu pengajuan uang kas kecil beserta seluruh perjalanannya (PC-DES-001).
/// Sengaja TIDAK memiliki kolom penghubung ke BilInvoice, BilCashierShift, maupun
/// BilSettlement — kas kecil dan kas fisik shift kasir adalah dua uang yang berbeda
/// (PC-DEC-001). Baris ber-Status REJECTED bersifat terminal dan immutable (PC-DES-013).
/// Pembatalan memakai penandaan IsCancel warisan IdentityModel, bukan status keenam (PC-DES-007).
/// </summary>
[Table("BilPettyCashVoucher", Schema = "public")]
public sealed class BilPettyCashVoucher : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Contoh PTC-20260907-0001. Dialokasikan BillingNumberSeriesService; MUST NOT diisi frontend.</summary>
    [Required, MaxLength(40)] public string VoucherNumber { get; set; } = string.Empty;

    /// <summary>SENSITIF. Teks bebas; MUST NOT diberi foreign key (PC-DES-010, PC-DEC-011).</summary>
    [Required, MaxLength(150)] public string RecipientName { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }

    public decimal Amount { get; set; }

    /// <summary>SENSITIF. Keperluan pengeluaran, kalimat bebas.</summary>
    [Required, MaxLength(500)] public string Purpose { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string Status { get; set; } = PettyCashVoucherStatuses.WaitingApproval;

    public Guid RequestedBy { get; set; }

    public DateTimeOffset SubmittedAt { get; set; }

    public Guid? DecidedBy { get; set; }

    public DateTimeOffset? DecidedAt { get; set; }

    /// <summary>SENSITIF. Wajib terisi ketika Status = REJECTED; MUST kosong pada status lain.</summary>
    [MaxLength(500)] public string? RejectionReason { get; set; }

    public Guid? DisbursedBy { get; set; }

    /// <summary>Saat saldo kolam berkurang (PC-DEC-009).</summary>
    public DateTimeOffset? DisbursedAt { get; set; }

    [MaxLength(60)] public string? ProofReferenceNumber { get; set; }

    public Guid? ProofSubmittedBy { get; set; }

    public DateTimeOffset? ProofSubmittedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    public Guid? IdempotencyKey { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public MstPettyCashCategory Category { get; set; } = null!;

    public ICollection<BilPettyCashVoucherCommand> Commands { get; set; } = new List<BilPettyCashVoucherCommand>();
}

public static class PettyCashVoucherStatuses
{
    public const string WaitingApproval = "WAITING_APPROVAL";
    public const string Approved = "APPROVED";
    public const string CashReceived = "CASH_RECEIVED";
    public const string Completed = "COMPLETED";
    public const string Rejected = "REJECTED";
}
