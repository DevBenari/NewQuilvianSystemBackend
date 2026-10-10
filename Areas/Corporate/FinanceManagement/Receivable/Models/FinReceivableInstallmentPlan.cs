using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Satu perjanjian pembayaran bertahap atas satu kartu piutang (FIN-DES-099, FIN-DEC-165/166).
/// Aggregate tersendiri — bukan kolom tambahan pada FinReceivable — karena satu piutang boleh
/// berganti perjanjian sepanjang hidupnya (diajukan, ditolak, diajukan ulang), dan setiap
/// pengajuan punya pengaju, penyetuju, dokumen, serta jadwalnya sendiri.
/// </summary>
[Table("FinReceivableInstallmentPlan", Schema = "public")]
public sealed class FinReceivableInstallmentPlan : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(50)] public string PlanNumber { get; set; } = string.Empty;

    /// <summary>Kartu piutang yang diangsur. FinReceivable.cs tidak diberi collection balik — pola
    /// yang sama dengan FinReceivableMovement.Receivable.</summary>
    public Guid ReceivableId { get; set; }
    public FinReceivable? Receivable { get; set; }

    public int InstallmentCount { get; set; }
    public decimal InstallmentAmount { get; set; }

    /// <summary>MUST sama dengan InstallmentCount x InstallmentAmount (CK_FinReceivableInstallmentPlan_Total),
    /// dan MUST sama dengan sisa piutang saat disetujui (ditegakkan service, bukan database).</summary>
    public decimal TotalAgreedAmount { get; set; }

    [Required, MaxLength(7)] public string FirstDeductionPeriod { get; set; } = string.Empty;

    [Required, MaxLength(30)] public string Status { get; set; } = FinReceivableInstallmentPlanStatuses.Menunggu;

    /// <summary>Jalur berkas perjanjian yang ditandatangani. Memuat identitas pegawai.</summary>
    [MaxLength(512)] public string? AgreementDocumentPath { get; set; }

    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }

    /// <summary>MUST NOT sama dengan RequestedBy (CK_FinReceivableInstallmentPlan_MakerChecker).</summary>
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }

    [MaxLength(500)] public string? RejectionReason { get; set; }
    [MaxLength(500)] public string? CancelReason { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinReceivableInstallment> Installments { get; set; } = new List<FinReceivableInstallment>();
}

public static class FinReceivableInstallmentPlanStatuses
{
    public const string Menunggu = "MENUNGGU";
    public const string Disetujui = "DISETUJUI";
    public const string Ditolak = "DITOLAK";
    public const string Selesai = "SELESAI";
    public const string Dibatalkan = "DIBATALKAN";
}
