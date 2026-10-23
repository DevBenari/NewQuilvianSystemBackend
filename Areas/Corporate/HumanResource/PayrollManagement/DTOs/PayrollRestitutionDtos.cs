namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PayrollManagement.DTOs;

/// <summary>
/// DTO permintaan restitusi salah potong cicilan piutang RS (BE-FIN-111, FIN-DEC-195).
/// </summary>
public sealed class PayrollRestitutionRequest
{
    /// <summary>MstEmployee.Id (kontrak lama). Boleh kosong bila <see cref="WorkforceProfileId"/> diisi (dokter tetap).</summary>
    public Guid EmployeeId { get; set; }

    /// <summary>Pemilik slip gaji berdasarkan MstWorkforceProfile.Id; identitas utama untuk karyawan maupun dokter.</summary>
    public Guid? WorkforceProfileId { get; set; }
    public decimal Amount { get; set; }
    public Guid SourceId { get; set; }
    public string SourceType { get; set; } = "FinanceReceivableCorrection";
    public string Reason { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? TargetPeriodCode { get; set; }
    public Guid ActorUserId { get; set; }
}

/// <summary>
/// DTO hasil restitusi salah potong cicilan piutang RS (BE-FIN-111, AC 1, AC 2, AC 3).
/// </summary>
public sealed class PayrollRestitutionResponse
{
    public Guid RestitutionAdjustmentId { get; set; }
    public Guid RestitutionReversalId { get; set; }
    public Guid PayrollRunId { get; set; }
    public Guid PayrollRunEmployeeId { get; set; }

    /// <summary>Guid.Empty bila pemilik slip adalah dokter tanpa data karyawan (tidak ada EmployeeId palsu).</summary>
    public Guid EmployeeId { get; set; }
    public Guid? WorkforceProfileId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string EmployeeNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string TargetPeriodCode { get; set; } = string.Empty;
    public bool IsIdempotentReplay { get; set; }
    public string Status { get; set; } = "BERHASIL"; // BERHASIL, IDEMPOTENT, TIDAK_ADA_PAYROLL
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

