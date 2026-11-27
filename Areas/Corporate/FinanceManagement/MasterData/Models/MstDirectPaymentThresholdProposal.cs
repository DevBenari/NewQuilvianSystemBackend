using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models;

/// <summary>
/// Pengajuan perubahan Batas Pembayaran Langsung (FIN-BE-DPT-001). Setiap penetapan atau perubahan nominal — termasuk yang
/// pertama kali — WAJIB lewat: Pengajuan → Menunggu → Disetujui / Ditolak → Aktifkan.
///
/// Tabel ini TIDAK PERNAH dibaca pembayaran AR/AP. Satu-satunya sumber nominal yang berlaku bagi transaksi tetap
/// <see cref="MstDirectPaymentThreshold"/> (baris aktif tunggal); baris ini baru menggantikannya saat <c>Activate</c>, dalam satu
/// transaksi. Akibatnya pengajuan yang Menunggu, Disetujui-belum-aktif, atau Ditolak tidak dapat memengaruhi pembayaran.
///
/// Dua dimensi status yang sengaja dipisah: <see cref="ApprovalStatus"/> (Menunggu/Disetujui/Ditolak) dan
/// <see cref="ActivationStatus"/> (Belum Aktif/Aktif/Digantikan). "Disetujui" BUKAN sinonim "Aktif".
///
/// Riwayat permanen: setiap tahap menulis kolom tahapnya sendiri sekali (tidak ditimpa tahap lain), dan setiap kejadian juga
/// ditulis append-only ke <see cref="MstDirectPaymentThresholdEvent"/>. Baris tidak pernah dihapus.
/// </summary>
[Table("MstDirectPaymentThresholdProposal", Schema = "public")]
public sealed class MstDirectPaymentThresholdProposal : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Identitas yang terbaca manusia, contoh <c>BPL-20261128-1A2B3C4D</c>. Dibentuk service dari Id (unik di database).</summary>
    [Required, MaxLength(40)]
    public string ProposalNumber { get; set; } = string.Empty;

    public decimal ProposedAmount { get; set; }

    /// <summary>Batas aktif saat pengajuan dibuat; <c>null</c> bila belum ada batas aktif (pengajuan pertama).</summary>
    public decimal? PreviousActiveAmount { get; set; }

    [Required, MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;

    /// <summary><see cref="DirectPaymentThresholdApprovalStatuses"/>.</summary>
    [Required, MaxLength(20)]
    public string ApprovalStatus { get; set; } = DirectPaymentThresholdApprovalStatuses.Waiting;

    /// <summary><see cref="DirectPaymentThresholdActivationStatuses"/>.</summary>
    [Required, MaxLength(20)]
    public string ActivationStatus { get; set; } = DirectPaymentThresholdActivationStatuses.NotActive;

    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    /// <summary>Wajib saat ditolak.</summary>
    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    public Guid? ActivatedBy { get; set; }
    public DateTime? ActivatedAt { get; set; }

    /// <summary>
    /// Tanggal Berlaku = waktu aktivasi berhasil disimpan (UTC). BUKAN tanggal pengajuan atau persetujuan. <c>null</c> sampai diaktifkan.
    /// </summary>
    public DateTime? EffectiveFrom { get; set; }

    /// <summary>Nominal yang sebenarnya digantikan saat aktivasi (bisa batas legacy tanpa pengajuan). <c>null</c> bila tidak ada batas sebelumnya.</summary>
    public decimal? ActivatedPreviousAmount { get; set; }

    public DateTime? SupersededAt { get; set; }
    public Guid? SupersededBy { get; set; }
    public Guid? SupersededByProposalId { get; set; }

    /// <summary>Penanda versi optimistic concurrency, diputar service pada setiap tahap (pola <see cref="MstDirectPaymentThreshold.RowVersion"/>).</summary>
    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();
}

/// <summary>Nilai <see cref="MstDirectPaymentThresholdProposal.ApprovalStatus"/>. Sengaja berbentuk teks tetap di database (bukan enum angka).</summary>
public static class DirectPaymentThresholdApprovalStatuses
{
    public const string Waiting = "MENUNGGU";
    public const string Approved = "DISETUJUI";
    public const string Rejected = "DITOLAK";

    public static readonly string[] All = [Waiting, Approved, Rejected];
}

/// <summary>Nilai <see cref="MstDirectPaymentThresholdProposal.ActivationStatus"/>.</summary>
public static class DirectPaymentThresholdActivationStatuses
{
    public const string NotActive = "BELUM_AKTIF";
    public const string Active = "AKTIF";
    public const string Superseded = "DIGANTIKAN";

    public static readonly string[] All = [NotActive, Active, Superseded];
}

/// <summary>Jenis kejadian pada <see cref="MstDirectPaymentThresholdEvent"/>.</summary>
public static class DirectPaymentThresholdEventTypes
{
    public const string ProposalCreated = "PROPOSAL_CREATED";
    public const string ProposalApproved = "PROPOSAL_APPROVED";
    public const string ProposalRejected = "PROPOSAL_REJECTED";
    public const string ThresholdActivated = "THRESHOLD_ACTIVATED";
    public const string ThresholdSuperseded = "THRESHOLD_SUPERSEDED";

    public static readonly string[] All =
        [ProposalCreated, ProposalApproved, ProposalRejected, ThresholdActivated, ThresholdSuperseded];
}
