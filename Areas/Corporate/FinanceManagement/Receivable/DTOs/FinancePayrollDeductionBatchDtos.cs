using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// Jadwal potongan cicilan satu periode beserta total (BE-FIN-116, FIN-DEC-219, FIN-API-1.11 §S.1.1).
/// </summary>
public sealed class DeductionScheduleResponse
{
    public string Period { get; set; } = string.Empty;

    /// <summary>
    /// ID master periode payroll HR yang bulannya bersesuaian (bila sudah terdaftar di MstPayrollPeriod).
    /// </summary>
    public Guid? PayrollPeriodId { get; set; }

    public decimal TotalDueAmount { get; set; }

    public int RowCount { get; set; }

    public List<DeductionScheduleRowResponse> Rows { get; set; } = [];
}

/// <summary>
/// Satu baris jadwal angsuran potongan gaji yang jatuh tempo pada suatu periode (FIN-API-1.11 §S.1.1).
/// </summary>
public sealed class DeductionScheduleRowResponse
{
    public Guid InstallmentId { get; set; }

    /// <summary>
    /// Kode angsuran gabungan ({PlanNumber}-{InstallmentNumber}).
    /// </summary>
    public string InstallmentCode { get; set; } = string.Empty;

    public string EmployeeNumber { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string OrganizationUnitName { get; set; } = string.Empty;

    public string ReceivableNumber { get; set; } = string.Empty;

    public int InstallmentNumber { get; set; }

    public decimal ScheduledAmount { get; set; }

    /// <summary>
    /// Tunggakan dari periode sebelumnya yang ikut dilimpahkan ke periode ini (FIN-DEC-168).
    /// </summary>
    public decimal CarriedOverAmount { get; set; }

    /// <summary>
    /// Total nominal terutang yang harus dipotong (ScheduledAmount + CarriedOverAmount - PaidAmount).
    /// </summary>
    public decimal DueAmount { get; set; }
}

/// <summary>
/// Request upload berkas batch hasil potongan (FIN-API-1.11 §S.1.1, BE-FIN-117).
/// </summary>
public sealed class UploadDeductionBatchRequest
{
    public IFormFile? File { get; set; }

    [MaxLength(7)]
    public string Period { get; set; } = string.Empty;

    public Guid PayrollPeriodId { get; set; }

    [MaxLength(20)]
    public string BatchType { get; set; } = "REGULER"; // REGULER / KOREKSI

    public decimal ControlTotalAmount { get; set; }

    [MaxLength(500)]
    public string? CorrectionReason { get; set; }
}

/// <summary>
/// Request pengajuan batch hasil potongan dengan bukti (FIN-API-1.11 §S.1.1, BE-FIN-117).
/// </summary>
public sealed class SubmitDeductionBatchRequest
{
    public Guid RowVersion { get; set; }

    [MaxLength(100)]
    public string PayrollReportNumber { get; set; } = string.Empty;

    public Guid ProofId { get; set; }
}

/// <summary>
/// Request transisi status batch (Approve, ApproveCorrection) (FIN-API-1.11 §S.1.1, BE-FIN-117).
/// </summary>
public sealed class DeductionBatchTransitionRequest
{
    public Guid RowVersion { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

/// <summary>
/// Request penolakan atau pembatalan batch dengan alasan wajib (FIN-API-1.11 §S.1.1, BE-FIN-117).
/// </summary>
public sealed class RejectDeductionBatchRequest
{
    public Guid RowVersion { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class CancelDeductionBatchRequest
{
    public Guid RowVersion { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Ringkasan batch pada daftar berpaging (FIN-API-1.11 §S.1.1).
/// </summary>
public sealed class DeductionBatchListResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string BatchType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ControlTotalAmount { get; set; }
    public decimal TotalDeductedAmount { get; set; }
    public int TotalRows { get; set; }
    public int InvalidRows { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}

/// <summary>
/// Rincian lengkap batch beserta baris item (FIN-API-1.11 §S.1.1).
/// </summary>
public sealed class DeductionBatchResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string Period { get; set; } = string.Empty;
    public string BatchType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ControlTotalAmount { get; set; }
    public decimal TotalDeductedAmount { get; set; }
    public int TotalRows { get; set; }
    public int InvalidRows { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public Guid PayrollPeriodId { get; set; }
    public string? PayrollReportNumber { get; set; }
    public Guid? ProofId { get; set; }
    public string? CorrectionReason { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
    public List<DeductionBatchItemResponse> Items { get; set; } = [];
    public List<DeductionScheduleRowResponse> MissingScheduledInstallments { get; set; } = [];
}

/// <summary>
/// Rincian satu baris item dalam batch hasil potongan (FIN-API-1.11 §S.1.1).
/// </summary>
public sealed class DeductionBatchItemResponse
{
    public int RowNumber { get; set; }
    public string InstallmentCode { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string ResultStatus { get; set; } = string.Empty;
    public decimal DeductedAmount { get; set; }
    public decimal DueAmount { get; set; }
    public decimal PreviousDeductedAmount { get; set; }
    public decimal DeltaAmount { get; set; }
    public string ValidationStatus { get; set; } = string.Empty;
    public string? ValidationMessage { get; set; }
    public DateTimeOffset? AppliedAt { get; set; }
}

/// <summary>
/// Filter kueri berpaging daftar batch (FIN-API-1.11 §S.1.1).
/// </summary>
public sealed class DeductionBatchPagedQuery
{
    public string? Period { get; set; }
    public string? BatchType { get; set; }
    public string? Status { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

