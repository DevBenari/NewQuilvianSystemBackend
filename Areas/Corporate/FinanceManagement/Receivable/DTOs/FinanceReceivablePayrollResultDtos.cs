using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>BE-FIN-095, INT-HR-FIN-001 §P.3. Payload persis seperti kontrak integrasi.</summary>
public sealed class PayrollResultRequest
{
    [Required] public Guid InstallmentId { get; set; }
    [Required] public Guid PayrollPeriodId { get; set; }
    [Required] public string Status { get; set; } = string.Empty;
    [Range(0, double.MaxValue)] public decimal DeductedAmount { get; set; }
    [Required] public DateTimeOffset ExecutionTimestamp { get; set; }
    public Guid? PayrollRunId { get; set; }
    [MaxLength(500)] public string? Notes { get; set; }
}

public sealed class PayrollResultResponse
{
    public Guid InstallmentId { get; set; }
    public string InstallmentStatus { get; set; } = string.Empty;
    public decimal OutstandingAmount { get; set; }
    public bool IsIdempotentReplay { get; set; }
}

public static class PayrollResultStatuses
{
    public const string Berhasil = "BERHASIL";
    public const string Sebagian = "SEBAGIAN";
    public const string Gagal = "GAGAL";
}
