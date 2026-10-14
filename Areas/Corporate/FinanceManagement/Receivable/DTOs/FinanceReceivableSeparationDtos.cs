using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// Permintaan notifikasi pemisahan pegawai dari HR ke Finance (INT-HR-FIN-003 §P.5, FIN-DEC-169, FIN-DEC-194).
/// </summary>
public class SeparationNoticeRequest
{
    [Required] public Guid EmployeeId { get; set; }
    [Required] public DateOnly SeparationDate { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    [Required] public Guid SeparationId { get; set; }
}

/// <summary>
/// Respons notifikasi pemisahan pegawai dari Finance ke HR (FIN-API-1.9 §P.1.1).
/// </summary>
public class SeparationNoticeResponse
{
    public Guid EmployeeId { get; set; }
    public Guid SeparationId { get; set; }
    public int ActiveReceivableCount { get; set; }
    public int FlaggedReceivableCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
    public bool IsIdempotentReplay { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Alias kelas request sesuai penamaan kontrak FIN-API-1.9 §P.1.1.
/// </summary>
public sealed class EmployeeSeparationNoticeRequest : SeparationNoticeRequest
{
}

/// <summary>
/// Alias kelas response sesuai penamaan kontrak FIN-API-1.9 §P.1.1.
/// </summary>
public sealed class EmployeeSeparationNoticeResponse : SeparationNoticeResponse
{
}
