using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;

/// <summary>
/// Dokumen batch hasil potongan gaji per periode (BE-FIN-117, FIN-DES-106, FIN-DEC-220, FIN-DEC-221).
/// Mengikat hasil potongan berkas ke angsuran piutang pegawai secara atomik.
/// </summary>
[Table("FinPayrollDeductionBatch", Schema = "public")]
public sealed class FinPayrollDeductionBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor unik batch resmi (QBE-CODE-002).</summary>
    [Required, MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Periode pemotongan gaji (format: YYYY-MM).</summary>
    [Required, MaxLength(7)]
    public string DeductionPeriod { get; set; } = string.Empty;

    /// <summary>Rujukan logis ke MstPayrollPeriod.Id (HR).</summary>
    public Guid PayrollPeriodId { get; set; }

    /// <summary>REGULER atau KOREKSI.</summary>
    [Required, MaxLength(10)]
    public string BatchType { get; set; } = FinPayrollDeductionBatchTypes.Reguler;

    /// <summary>DRAFT, DIAJUKAN, DISAHKAN, DITOLAK, DIBATALKAN.</summary>
    [Required, MaxLength(20)]
    public string Status { get; set; } = FinPayrollDeductionBatchStatuses.Draft;

    /// <summary>Total nominal menurut laporan penggajian.</summary>
    public decimal ControlTotalAmount { get; set; }

    /// <summary>Jumlah nominal terpotong pada baris-baris valid.</summary>
    public decimal TotalDeductedAmount { get; set; }

    public int TotalRows { get; set; }
    public int InvalidRows { get; set; }

    /// <summary>Nomor laporan penggajian (wajib saat diajukan).</summary>
    [MaxLength(100)]
    public string? PayrollReportNumber { get; set; }

    /// <summary>Rujukan bukti dokumen laporan penggajian.</summary>
    public Guid? ProofId { get; set; }
    public FinTransactionProof? Proof { get; set; }

    /// <summary>Alasan koreksi (wajib bila BatchType = KOREKSI).</summary>
    [MaxLength(500)]
    public string? CorrectionReason { get; set; }

    /// <summary>Nama berkas yang diunggah.</summary>
    [Required, MaxLength(255)]
    public string SourceFileName { get; set; } = string.Empty;

    public Guid? SubmittedBy { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTimeOffset? RejectedAt { get; set; }

    [MaxLength(500)]
    public string? RejectionReason { get; set; }

    [ConcurrencyCheck]
    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<FinPayrollDeductionBatchItem> Items { get; set; } = new List<FinPayrollDeductionBatchItem>();
}

public static class FinPayrollDeductionBatchTypes
{
    public const string Reguler = "REGULER";
    public const string Koreksi = "KOREKSI";
}

public static class FinPayrollDeductionBatchStatuses
{
    public const string Draft = "DRAFT";
    public const string Diajukan = "DIAJUKAN";
    public const string Disahkan = "DISAHKAN";
    public const string Ditolak = "DITOLAK";
    public const string Dibatalkan = "DIBATALKAN";
}

