using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;

/// <summary>
/// Mengajukan penetapan atau perubahan Batas Pembayaran Langsung (FIN-BE-DPT-001). Pengajuan TIDAK mengubah batas yang berlaku;
/// statusnya selalu awal <c>MENUNGGU</c> + <c>BELUM_AKTIF</c>, termasuk untuk penetapan pertama.
/// </summary>
public sealed class CreateDirectPaymentThresholdProposalRequest
{
    public decimal Amount { get; set; }

    /// <summary>
    /// Wajib (dijawab 422). SENGAJA tanpa <c>[Required]</c> agar pipeline validasi otomatis tidak memotong dengan 400 generik
    /// (alasan yang sama dengan <see cref="UpdateDirectPaymentThresholdRequest"/>).
    /// </summary>
    [MaxLength(500)]
    public string ChangeReason { get; set; } = string.Empty;
}

/// <summary>Menyetujui pengajuan yang berstatus MENUNGGU. Tidak mengaktifkan nominal.</summary>
public sealed class ApproveDirectPaymentThresholdProposalRequest
{
    /// <summary>Penanda versi yang dibaca klien dari pengajuan; basi atau tidak dikirim dijawab 409.</summary>
    public Guid? ExpectedRowVersion { get; set; }
}

/// <summary>Menolak pengajuan yang berstatus MENUNGGU. Alasan penolakan wajib.</summary>
public sealed class RejectDirectPaymentThresholdProposalRequest
{
    public Guid? ExpectedRowVersion { get; set; }

    /// <summary>Wajib (dijawab 422). Tanpa <c>[Required]</c> dengan alasan yang sama seperti <c>ChangeReason</c>.</summary>
    [MaxLength(500)]
    public string RejectionReason { get; set; } = string.Empty;
}

/// <summary>Mengaktifkan pengajuan yang sudah DISETUJUI dan masih BELUM_AKTIF; menggantikan batas aktif secara atomik.</summary>
public sealed class ActivateDirectPaymentThresholdProposalRequest
{
    public Guid? ExpectedRowVersion { get; set; }
}

public sealed class DirectPaymentThresholdProposalQuery
{
    /// <summary>Pencarian nomor pengajuan (sebagian).</summary>
    [MaxLength(100)] public string? Search { get; set; }

    /// <summary><c>MENUNGGU</c>, <c>DISETUJUI</c>, atau <c>DITOLAK</c>.</summary>
    public string? ApprovalStatus { get; set; }

    /// <summary><c>BELUM_AKTIF</c>, <c>AKTIF</c>, atau <c>DIGANTIKAN</c>.</summary>
    public string? ActivationStatus { get; set; }

    /// <summary>Tanggal pengajuan (zona waktu aplikasi), batas awal dan akhir inklusif.</summary>
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class DirectPaymentThresholdHistoryQuery
{
    /// <summary>Pencarian nomor pengajuan (sebagian).</summary>
    [MaxLength(100)] public string? Search { get; set; }

    /// <summary><see cref="QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models.DirectPaymentThresholdEventTypes"/>.</summary>
    public string? EventType { get; set; }

    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>Satu pengajuan beserta seluruh tahap, pelaku, dan waktunya. Nama pelaku dibaca saat menyusun respons (tidak disalin).</summary>
public sealed class DirectPaymentThresholdProposalResponse
{
    public Guid Id { get; set; }
    public string ProposalNumber { get; set; } = string.Empty;

    public decimal ProposedAmount { get; set; }
    public decimal? PreviousActiveAmount { get; set; }

    /// <summary><c>ProposedAmount - PreviousActiveAmount</c>; <c>null</c> bila belum ada batas sebelumnya.</summary>
    public decimal? AmountDifference { get; set; }

    public string ChangeReason { get; set; } = string.Empty;

    /// <summary>STATUS PENGAJUAN: <c>MENUNGGU</c>, <c>DISETUJUI</c>, <c>DITOLAK</c>.</summary>
    public string ApprovalStatus { get; set; } = string.Empty;

    /// <summary>STATUS AKTIVASI: <c>BELUM_AKTIF</c>, <c>AKTIF</c>, <c>DIGANTIKAN</c>. Terpisah dari status pengajuan.</summary>
    public string ActivationStatus { get; set; } = string.Empty;

    public string? RequestedByName { get; set; }
    public DateTime RequestedAt { get; set; }

    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public string? RejectedByName { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectionReason { get; set; }

    public string? ActivatedByName { get; set; }
    public DateTime? ActivatedAt { get; set; }

    /// <summary>Tanggal Berlaku = waktu aktivasi. <c>null</c> sampai diaktifkan; tidak pernah diisi dari tanggal pengajuan/persetujuan.</summary>
    public DateTime? EffectiveFrom { get; set; }

    public decimal? ActivatedPreviousAmount { get; set; }

    public DateTime? SupersededAt { get; set; }
    public string? SupersededByName { get; set; }
    public string? SupersededByProposalNumber { get; set; }

    /// <summary>Aktivitas terbaru pada pengajuan ini: waktu, pelaku (dari audit tahap, bukan pengguna login), dan jenisnya.</summary>
    public DateTime LastUpdatedAt { get; set; }
    public string? LastUpdatedByName { get; set; }

    /// <summary><see cref="QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Models.DirectPaymentThresholdEventTypes"/>.</summary>
    public string LastUpdatedAction { get; set; } = string.Empty;

    /// <summary>Penanda versi; dikirim kembali klien pada Setujui/Tolak/Aktifkan. Layar tidak menampilkannya.</summary>
    public Guid RowVersion { get; set; }

    /// <summary>
    /// Tindakan yang SECARA STATUS masih mungkin bagi pemirsa ini: <c>Approve</c>, <c>Reject</c>, <c>Activate</c>. Hanya bantuan tampilan —
    /// izin Role Management tetap diperiksa layar dan backend, dan setiap endpoint memeriksa ulang status dan kewenangan.
    /// <c>Approve</c> tidak muncul bagi pengaju non-SuperAdmin pada pengajuannya sendiri.
    /// </summary>
    public List<string> AvailableActions { get; set; } = new();

    /// <summary>Pengajuan ini dibuat oleh pemirsa yang sedang login.</summary>
    public bool IsOwnProposal { get; set; }
}

public sealed class DirectPaymentThresholdActiveVersionResponse
{
    /// <summary><c>PROPOSAL</c> (dibuktikan pengajuan), <c>LEGACY</c> (aktif sebelum alur ini ada: data historis belum tersedia), <c>NONE</c> (belum ada batas aktif).</summary>
    public string Source { get; set; } = DirectPaymentThresholdActiveSources.None;

    public string? ProposalNumber { get; set; }
    public decimal? Amount { get; set; }
    public DateTime? EffectiveFrom { get; set; }
    public string? ActivatedByName { get; set; }
    public DateTime? ActivatedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
}

public static class DirectPaymentThresholdActiveSources
{
    public const string Proposal = "PROPOSAL";
    public const string Legacy = "LEGACY";
    public const string None = "NONE";
}

public sealed class DirectPaymentThresholdProposalSummaryResponse
{
    public int TotalProposals { get; set; }
    public int WaitingProposals { get; set; }
    public int ApprovedNotActiveProposals { get; set; }
    public int RejectedProposals { get; set; }
    public DirectPaymentThresholdActiveVersionResponse ActiveVersion { get; set; } = new();
}

public sealed class DirectPaymentThresholdHistoryResponse
{
    public Guid Id { get; set; }
    public Guid ProposalId { get; set; }
    public string ProposalNumber { get; set; } = string.Empty;
    public string? RelatedProposalNumber { get; set; }
    public string EventType { get; set; } = string.Empty;
    public decimal? PreviousAmount { get; set; }
    public decimal? NewAmount { get; set; }
    public string? ActorName { get; set; }
    public DateTime OccurredAt { get; set; }
    public string? StatusBefore { get; set; }
    public string StatusAfter { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool ActorIsSuperAdmin { get; set; }
    public bool IsSelfApproval { get; set; }
    public bool IsLegacyBaseline { get; set; }
}
