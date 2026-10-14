using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// BE-FIN-095, BE-FIN-110, BE-FIN-114, INT-HR-FIN-001 §P.3, FIN-API-1.11 §S.3.
/// Payload hasil potongan gaji dengan PayrollRunId opsional (wajib untuk panggilan HTTP langsung per FIN-VAL-287).
/// </summary>
public class PayrollResultRequest
{
    [Required] public Guid InstallmentId { get; set; }
    [Required] public Guid PayrollPeriodId { get; set; }
    [Required] public string Status { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal DeductedAmount { get; set; }
    [Required] public DateTimeOffset ExecutionTimestamp { get; set; }
    public Guid? PayrollRunId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public class PayrollResultResponse
{
    public Guid InstallmentId { get; set; }
    public string InstallmentStatus { get; set; } = string.Empty;
    public decimal OutstandingAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal CarriedOverToNextPeriod { get; set; }
    public bool IsIdempotentReplay { get; set; }
    public DateTimeOffset ProcessedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>Alias kelas request sesuai penamaan kontrak FIN-API-1.9 §P.1.1 dan FIN-API-1.11 §S.3.</summary>
public sealed class SyncPayrollResultRequest : PayrollResultRequest { }

/// <summary>Alias kelas response sesuai penamaan kontrak FIN-API-1.9 §P.1.1 dan FIN-API-1.11 §S.3.</summary>
public sealed class SyncPayrollResultResponse : PayrollResultResponse { }

public static class PayrollResultStatuses
{
    public const string Berhasil = "BERHASIL";
    public const string Sebagian = "SEBAGIAN";
    public const string Gagal = "GAGAL";
}
