using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Models;

/// <summary>
/// Satu pengajuan uang kas kecil beserta seluruh perjalanannya (PC-DES-001).
/// Sengaja TIDAK memiliki kolom penghubung ke BilInvoice, BilCashierShift, maupun
/// BilSettlement — kas kecil dan kas fisik shift kasir adalah dua uang yang berbeda
/// (PC-DEC-001). Baris ber-Status REJECTED bersifat terminal dan immutable (PC-DES-013),
/// dan sejak revisi 15 September 2026 (PC-DEC-016) hanya baris warisan yang dapat
/// memilikinya — tidak ada voucher baru yang memasuki status ini.
/// Pembatalan memakai penandaan IsCancel warisan IdentityModel, bukan status keenam (PC-DES-007).
/// </summary>
[Table("BilPettyCashVoucher", Schema = "public")]
public sealed class BilPettyCashVoucher : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Contoh PTC-20260907-0001. Dialokasikan BillingNumberSeriesService; MUST NOT diisi frontend.</summary>
    [Required, MaxLength(40)] public string VoucherNumber { get; set; } = string.Empty;

    /// <summary>SENSITIF. Teks bebas; MUST NOT diberi foreign key (PC-DES-010, PC-DEC-011, ditegaskan ulang PC-DEC-019).</summary>
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

    /// <summary>SENSITIF. Wajib terisi ketika Status = REJECTED; MUST kosong pada status lain.
    /// Warisan pra-revisi 15 September 2026 — MUST NOT dihapus, memuat jejak keputusan
    /// yang pernah terjadi sebelum gerbang persetujuan dicabut (PC-DEC-016, PC-DES-015).</summary>
    [MaxLength(500)] public string? RejectionReason { get; set; }

    public Guid? DisbursedBy { get; set; }

    /// <summary>Saat saldo kolam berkurang (PC-DEC-009).</summary>
    public DateTimeOffset? DisbursedAt { get; set; }

    [MaxLength(60)] public string? ProofReferenceNumber { get; set; }

    public Guid? ProofSubmittedBy { get; set; }

    public DateTimeOffset? ProofSubmittedAt { get; set; }

    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>Akumulasi sisa uang yang sudah dikembalikan penerima (PC-DES-019, PC-DES-020).
    /// MUST NOT melampaui Amount. Boleh bertambah berkali-kali; TIDAK mengubah Status.</summary>
    public decimal ReturnedAmount { get; set; }

    /// <summary>Petugas yang membalik pencairan voucher ini (PC-DES-020). Kosong selama
    /// belum pernah dibalik.</summary>
    public Guid? ReversedBy { get; set; }

    /// <summary>Kapan pembalikan terjadi. Kosong selama belum pernah dibalik.</summary>
    public DateTimeOffset? ReversedAt { get; set; }

    /// <summary>SENSITIF. Wajib terisi ketika Status = REVERSED; MUST kosong pada status
    /// lain — pola yang sama dengan RejectionReason (PC-DES-020).</summary>
    [MaxLength(500)] public string? ReversalReason { get; set; }

    public Guid? IdempotencyKey { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public MstPettyCashCategory Category { get; set; } = null!;

    public ICollection<BilPettyCashVoucherCommand> Commands { get; set; } = new List<BilPettyCashVoucherCommand>();
}

public static class PettyCashVoucherStatuses
{
    /// <summary>Warisan pra-revisi 15 September 2026. Tetap ada karena
    /// PettyCashVoucherService (di luar scope task fondasi ini) masih menulis nilai ini
    /// sebagai status awal voucher baru pada CreateAsync; dipensiunkan penuh saat
    /// BE-BKC-055 menyentuh service tersebut. Baris warisan dipetakan migration menjadi
    /// Requested (PC-DES-024).</summary>
    public const string WaitingApproval = "WAITING_APPROVAL";

    /// <summary>Warisan pra-revisi 15 September 2026, lihat catatan pada <see cref="WaitingApproval"/>.
    /// Baris lama dipetakan migration menjadi <see cref="Requested"/> (PC-DES-024).</summary>
    public const string Approved = "APPROVED";

    public const string CashReceived = "CASH_RECEIVED";
    public const string Completed = "COMPLETED";

    /// <summary>Warisan — hanya baris sebelum 15 September 2026 yang memilikinya. Tidak ada
    /// voucher baru yang dapat memasuki status ini sejak PC-DEC-016 (PC-DES-015).</summary>
    public const string Rejected = "REJECTED";

    /// <summary>Status awal voucher baru setelah gerbang persetujuan dicabut (PC-DEC-016,
    /// PC-DES-015). Menggantikan makna WaitingApproval; belum dipakai sebagai default
    /// property Status sampai BE-BKC-055 menyentuh PettyCashVoucherService.</summary>
    public const string Requested = "REQUESTED";

    /// <summary>Pencairan dibalik karena seharusnya tidak terjadi; terminal (PC-DES-020).</summary>
    public const string Reversed = "REVERSED";
}
