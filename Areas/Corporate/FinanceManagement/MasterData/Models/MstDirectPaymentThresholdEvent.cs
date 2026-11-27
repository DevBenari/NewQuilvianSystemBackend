using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Kejadian APPEND-ONLY pada alur Batas Pembayaran Langsung (FIN-BE-DPT-001): pengajuan dibuat, disetujui, ditolak, batas
/// diaktifkan, dan batas lama digantikan. Baris ini tidak pernah diubah atau dihapus oleh kode mana pun, sehingga riwayat tetap
/// utuh ketika pengajuan baru dibuat atau batas diganti. Logger aplikasi BUKAN satu-satunya tempat riwayat — pengajuan dan
/// kejadiannya tersimpan di database dan dapat dibaca ulang setelah aplikasi dimulai ulang.
/// </summary>
[Table("MstDirectPaymentThresholdEvent", Schema = "public")]
public sealed class MstDirectPaymentThresholdEvent : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ProposalId { get; set; }
    public MstDirectPaymentThresholdProposal? Proposal { get; set; }

    /// <summary>
    /// Pengajuan lain yang terkait: pada <c>THRESHOLD_ACTIVATED</c> ia pengajuan yang digantikan, pada
    /// <c>THRESHOLD_SUPERSEDED</c> ia pengajuan yang menggantikan. <c>null</c> bila tidak ada.
    /// </summary>
    public Guid? RelatedProposalId { get; set; }

    /// <summary><see cref="DirectPaymentThresholdEventTypes"/>.</summary>
    [Required, MaxLength(30)]
    public string EventType { get; set; } = string.Empty;

    public decimal? PreviousAmount { get; set; }
    public decimal? NewAmount { get; set; }

    public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }

    /// <summary>Status sebelum tindakan, bentuk <c>{ApprovalStatus}/{ActivationStatus}</c>; <c>null</c> pada pembuatan.</summary>
    [MaxLength(40)]
    public string? StatusBefore { get; set; }

    /// <summary>Status sesudah tindakan, bentuk <c>{ApprovalStatus}/{ActivationStatus}</c>.</summary>
    [Required, MaxLength(40)]
    public string StatusAfter { get; set; } = string.Empty;

    /// <summary>Alasan pengajuan atau penolakan bila ada.</summary>
    [MaxLength(500)]
    public string? Reason { get; set; }

    /// <summary>Aktor teridentifikasi SuperAdmin oleh backend saat tindakan (bukan klaim dari layar).</summary>
    public bool ActorIsSuperAdmin { get; set; }

    /// <summary>Penyetuju sama dengan pengaju. Hanya mungkin bila aktornya SuperAdmin (pengecualian khusus fitur ini).</summary>
    public bool IsSelfApproval { get; set; }

    /// <summary>
    /// Penggantian atas batas yang sudah aktif SEBELUM alur ini ada (tanpa pengajuan, tanpa persetujuan terdokumentasi).
    /// Tidak dibuatkan persetujuan atau tanggal berlaku fiktif untuknya.
    /// </summary>
    public bool IsLegacyBaseline { get; set; }
}
