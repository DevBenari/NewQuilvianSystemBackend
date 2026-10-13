using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Aggregate root Pengajuan Pemutihan Tagihan AR V2 (Header).
/// Menggabungkan pengajuan pemutihan untuk satu Invoice AR (PER_INVOICE) atau beberapa Invoice AR (BATCH).
/// Lifecycle: DRAFT -> SUBMITTED -> APPROVED -> POSTED (atau SUBMITTED -> REJECTED).
/// </summary>
[Table("FinReceivableWriteOffProposal", Schema = "public")]
public sealed class FinReceivableWriteOffProposal : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)]
    public string ProposalNumber { get; set; } = string.Empty;

    public DateOnly ProposalDate { get; set; }

    /// <summary>PER_INVOICE atau BATCH.</summary>
    [Required, MaxLength(30)]
    public string ProposalType { get; set; } = FinReceivableWriteOffProposalTypes.PerInvoice;

    /// <summary>Snapshot jenis layanan: RANAP, RAJAL, IGD, ODC, RAD, LAB.</summary>
    [MaxLength(30)]
    public string? ServiceType { get; set; }

    /// <summary>Penjamin terkait (MstCompanyGuarantor / MstInsuranceProvider). Wajib konsisten per batch.</summary>
    public Guid? DebtorReferenceId { get; set; }

    /// <summary>Kode alasan pemutihan: UNCOLLECTIBLE, CLAIM_REJECTED, SOCIAL_RELIEF, ADMIN_CORRECTION, POLICY_EXPIRED.</summary>
    [Required, MaxLength(50)]
    public string ReasonCode { get; set; } = FinReceivableWriteOffReasonCodes.Uncollectible;

    /// <summary>Uraian/keterangan lengkap alasan pemutihan.</summary>
    [Required, MaxLength(500)]
    public string ReasonDescription { get; set; } = string.Empty;

    /// <summary>DRAFT, SUBMITTED, APPROVED, POSTED, REJECTED.</summary>
    [Required, MaxLength(30)]
    public string Status { get; set; } = FinReceivableWriteOffProposalStatuses.Draft;

    /// <summary>Total nominal awal invoice yang dicakup proposal.</summary>
    public decimal TotalInvoiceAmount { get; set; }

    /// <summary>Total sisa piutang (outstanding) yang masih tertagih saat pengajuan.</summary>
    public decimal TotalOutstandingAmount { get; set; }

    /// <summary>Total nominal pemutihan yang diusulkan oleh pemohon.</summary>
    public decimal TotalProposedAmount { get; set; }

    /// <summary>Total nominal pemutihan yang disetujui oleh approver (maker-checker).</summary>
    public decimal TotalApprovedAmount { get; set; }

    /// <summary>Pengguna yang mengajukan (maker).</summary>
    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }

    /// <summary>Pengguna yang menyetujui (checker, MUST NOT equal RequestedBy).</summary>
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    /// <summary>Pengguna yang mengeksekusi posting ke saldo piutang dan outbox akuntansi.</summary>
    public Guid? PostedBy { get; set; }
    public DateTimeOffset? PostedAt { get; set; }

    /// <summary>Pengguna yang menolak pengajuan.</summary>
    public Guid? RejectedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinReceivableWriteOffProposalItem> Items { get; set; } = new List<FinReceivableWriteOffProposalItem>();
}

public static class FinReceivableWriteOffProposalTypes
{
    public const string PerInvoice = "PER_INVOICE";
    public const string Batch = "BATCH";
}

public static class FinReceivableWriteOffProposalStatuses
{
    public const string Draft = "DRAFT";
    public const string Submitted = "SUBMITTED";
    public const string Approved = "APPROVED";
    public const string Posted = "POSTED";
    public const string Rejected = "REJECTED";
}

public static class FinReceivableWriteOffReasonCodes
{
    public const string Uncollectible = "UNCOLLECTIBLE";
    public const string ClaimRejected = "CLAIM_REJECTED";
    public const string SocialRelief = "SOCIAL_RELIEF";
    public const string AdminCorrection = "ADMIN_CORRECTION";
    public const string PolicyExpired = "POLICY_EXPIRED";

    public static readonly IReadOnlyDictionary<string, string> DisplayLabels = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { Uncollectible, "Tidak Tertagih" },
        { ClaimRejected, "Klaim Ditolak" },
        { SocialRelief, "Keringanan / Dana Sosial" },
        { AdminCorrection, "Koreksi Administrasi" },
        { PolicyExpired, "Kedaluwarsa Sesuai Kebijakan" }
    };
}
