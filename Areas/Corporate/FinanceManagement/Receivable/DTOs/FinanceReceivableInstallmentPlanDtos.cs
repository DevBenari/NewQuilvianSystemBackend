using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class CreateInstallmentPlanRequest
{
    [Range(2, 60)] public int InstallmentCount { get; set; }
    [Range(0.01, double.MaxValue)] public decimal InstallmentAmount { get; set; }
    [Required, RegularExpression(@"^\d{4}-(0[1-9]|1[0-2])$")] public string FirstDeductionPeriod { get; set; } = string.Empty;
    [MaxLength(512)] public string? AgreementDocumentPath { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class ApproveInstallmentPlanRequest
{
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class RejectInstallmentPlanRequest
{
    [Required, MaxLength(500)] public string RejectionReason { get; set; } = string.Empty;
}

public sealed class CancelInstallmentPlanRequest
{
    [Required, MaxLength(500)] public string CancelReason { get; set; } = string.Empty;
}

public sealed class InstallmentPlanQuery
{
    public string? Status { get; set; }
    public Guid? BenefitOwnerId { get; set; }
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }
    public string? DeductionPeriod { get; set; }

    /// <summary>true = hanya perjanjian yang punya angsuran TERTUNGGAK; false = hanya yang tidak punya; kosong = semua.</summary>
    public bool? HasArrears { get; set; }

    /// <summary>Nomor perjanjian, nomor piutang, nama atau NIP / nomor dokter pemilik benefit.</summary>
    public string? Search { get; set; }
    public string SortBy { get; set; } = "requestedAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public class InstallmentPlanResponse
{
    public Guid Id { get; set; }
    public string PlanNumber { get; set; } = string.Empty;
    public Guid ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public int InstallmentCount { get; set; }
    public decimal InstallmentAmount { get; set; }
    public decimal TotalAgreedAmount { get; set; }
    public string FirstDeductionPeriod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? RequestedByName { get; set; }

    /// <summary>
    /// Pengaju. Dipakai layar untuk TIDAK menampilkan tombol Setujui/Tolak bagi pengajunya sendiri (FIN-DEC-166);
    /// penegakannya tetap di service dan CK_FinReceivableInstallmentPlan_MakerChecker, bukan di layar.
    /// </summary>
    public Guid RequestedByUserId { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancelReason { get; set; }
    public string? Notes { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class InstallmentPlanListResponse : InstallmentPlanResponse
{
    /// <summary>Sensitif (FIN-DEC-172) — tetap dikembalikan ke pengguna berhak, MUST NOT masuk custom logger.</summary>
    public Guid? BenefitOwnerId { get; set; }

    /// <summary>Sensitif. Identitas canonical pemilik (karyawan atau dokter tetap).</summary>
    public Guid? BenefitOwnerWorkforceProfileId { get; set; }
    /// <summary>Sensitif.</summary>
    public string? BenefitRelationship { get; set; }

    /// <summary>Nama pemilik manfaat dari identitas Workforce (karyawan atau dokter tetap); kosong bila tidak dapat diselesaikan.</summary>
    public string? BenefitOwnerName { get; set; }

    /// <summary>NIP (karyawan) atau nomor dokter.</summary>
    public string? BenefitOwnerEmployeeNumber { get; set; }

    /// <summary>EMPLOYEE atau DOCTOR.</summary>
    public string? BenefitOwnerType { get; set; }
    public decimal OutstandingAmount { get; set; }

    /// <summary>Jumlah potongan gaji yang BERHASIL (PaidAmount seluruh angsuran). Potongan gagal tidak dihitung.</summary>
    public decimal PaidAmount { get; set; }

    /// <summary>Jumlah angsuran berstatus TERTUNGGAK (potongan gagal, sisanya terbawa ke periode berikutnya).</summary>
    public int ArrearsInstallmentCount { get; set; }

    /// <summary>Sisa nominal seluruh angsuran TERTUNGGAK.</summary>
    public decimal ArrearsAmount { get; set; }
}

public sealed class InstallmentPlanDetailResponse : InstallmentPlanResponse
{
    /// <summary>Sensitif.</summary>
    public string? BenefitRelationship { get; set; }
    public string? BenefitOwnerName { get; set; }
    public string? BenefitOwnerEmployeeNumber { get; set; }
    public string? BenefitOwnerType { get; set; }

    /// <summary>Sisa kartu piutang saat ini (bisa berbeda dari total disepakati bila piutang berubah sejak diajukan).</summary>
    public decimal ReceivableOutstandingAmount { get; set; }
    public List<InstallmentResponse> Installments { get; set; } = new();
}

/// <summary>Ringkasan seluruh perjanjian angsuran (tidak mengikuti saringan daftar).</summary>
public sealed class InstallmentPlanSummaryResponse
{
    /// <summary>Status MENUNGGU.</summary>
    public int PendingCount { get; set; }

    /// <summary>Status DISETUJUI (jadwal berjalan).</summary>
    public int RunningCount { get; set; }

    /// <summary>Perjanjian DISETUJUI yang punya sedikitnya satu angsuran TERTUNGGAK.</summary>
    public int ArrearsCount { get; set; }

    /// <summary>Status SELESAI.</summary>
    public int CompletedCount { get; set; }
}

public sealed class InstallmentResponse
{
    public Guid Id { get; set; }
    public int InstallmentNumber { get; set; }
    public string DeductionPeriod { get; set; } = string.Empty;
    public decimal ScheduledAmount { get; set; }
    public decimal CarriedOverAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? LastResultAt { get; set; }
}
