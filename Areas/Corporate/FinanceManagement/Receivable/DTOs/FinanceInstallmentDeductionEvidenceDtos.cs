using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// Unggah bukti potong gaji untuk satu angsuran (multipart/form-data). Wajib tepat SATU dari <see cref="File"/> atau
/// <see cref="ExistingProofId"/>: yang kedua memakai ulang berkas laporan penggajian (jenis LAPORAN-PENGGAJIAN) yang sudah
/// tersimpan, sehingga satu laporan untuk banyak pegawai tidak disimpan berulang. Tidak mengubah saldo piutang.
/// </summary>
public sealed class UploadInstallmentDeductionEvidenceRequest
{
    public IFormFile? File { get; set; }

    public Guid? ExistingProofId { get; set; }

    /// <summary>Nomor laporan penggajian sebagaimana tertera pada dokumen.</summary>
    [Required, MaxLength(100)]
    public string DocumentNumber { get; set; } = string.Empty;

    /// <summary>BERHASIL, SEBAGIAN, atau GAGAL menurut dokumen.</summary>
    [Required, MaxLength(10)]
    public string ResultStatus { get; set; } = string.Empty;

    /// <summary>Nominal terpotong menurut dokumen (0 bila GAGAL). Pembanding verifikator, bukan sumber saldo.</summary>
    [Range(0, 999999999999.99)]
    public decimal DeductedAmount { get; set; }

    public DateTimeOffset? DeductionDate { get; set; }

    public Guid? PayrollPeriodId { get; set; }

    public Guid? PayrollRunId { get; set; }

    /// <summary>Wajib untuk hasil GAGAL (alasan yang sah); opsional untuk lainnya.</summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class VerifyInstallmentDeductionEvidenceRequest
{
    public Guid? RowVersion { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public sealed class RejectInstallmentDeductionEvidenceRequest
{
    public Guid? RowVersion { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public static class InstallmentDeductionEvidenceSources
{
    /// <summary>Diunggah HRD lewat penghubung bukti angsuran.</summary>
    public const string Hrd = "HRD";

    /// <summary>Diturunkan dari batch hasil potongan Finance (FinPayrollDeductionBatch.ProofId); tidak disalin.</summary>
    public const string FinanceBatch = "BATCH_FINANCE";
}

/// <summary>Hasil payroll yang diturunkan dari status angsuran (bukan dari bukti).</summary>
public static class InstallmentPayrollResultStates
{
    public const string NoResult = "BELUM_ADA_HASIL";
    public const string Success = "BERHASIL";
    public const string Partial = "SEBAGIAN";
    public const string Failed = "GAGAL";
    public const string Cancelled = "DIBATALKAN";
}

public sealed class InstallmentDeductionEvidenceResponse
{
    /// <summary>Null untuk bukti turunan batch Finance (belum punya baris penghubung sendiri).</summary>
    public Guid? Id { get; set; }
    public string Source { get; set; } = InstallmentDeductionEvidenceSources.Hrd;
    public Guid ProofId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string ResultStatus { get; set; } = string.Empty;
    public decimal DeductedAmount { get; set; }
    public DateTimeOffset? DeductionDate { get; set; }
    public Guid? PayrollPeriodId { get; set; }
    public Guid? PayrollRunId { get; set; }
    public string? Notes { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? UploadedByName { get; set; }
    public DateTimeOffset? UploadedAt { get; set; }
    public string? VerifiedByName { get; set; }
    public DateTimeOffset? VerifiedAt { get; set; }
    public string? VerificationNotes { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class InstallmentDeductionEvidenceInstallmentResponse
{
    public Guid InstallmentId { get; set; }
    public int InstallmentNumber { get; set; }
    public string DeductionPeriod { get; set; } = string.Empty;
    public string InstallmentStatus { get; set; } = string.Empty;
    public decimal ScheduledAmount { get; set; }
    public decimal CarriedOverAmount { get; set; }

    /// <summary>Terpotong menurut buku Finance (hasil payroll yang sah), bukan menurut dokumen bukti.</summary>
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateTimeOffset? LastResultAt { get; set; }

    /// <summary>BELUM_ADA_HASIL, BERHASIL, SEBAGIAN, GAGAL, atau DIBATALKAN — diturunkan dari status angsuran.</summary>
    public string PayrollResultStatus { get; set; } = InstallmentPayrollResultStates.NoResult;

    /// <summary>BELUM_ADA_BUKTI bila tidak ada bukti non-ditolak; selain itu status bukti terkini yang paling kuat.</summary>
    public string EvidenceStatus { get; set; } = "BELUM_ADA_BUKTI";

    /// <summary>Periode sudah berjalan, angsuran belum dibatalkan, dan perjanjian masih berlaku.</summary>
    public bool CanUploadEvidence { get; set; }

    public List<InstallmentDeductionEvidenceResponse> Evidence { get; set; } = new();
}

public sealed class InstallmentDeductionEvidenceOverviewResponse
{
    public Guid PlanId { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public string PlanStatus { get; set; } = string.Empty;
    public Guid ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public List<InstallmentDeductionEvidenceInstallmentResponse> Installments { get; set; } = new();
}
