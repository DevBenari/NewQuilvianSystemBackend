using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.LifecycleManagement.DTOs;

/// <summary>
/// Metadata filter untuk halaman Exit Clearance HR.
/// </summary>
public sealed class ExitClearanceFilterMetadataResponse
{
    public List<string> ClearanceStatusOptions { get; set; } = new() { "Draft", "InProgress", "Completed", "Cancelled" };
    public List<string> SortDirections { get; set; } = new() { "asc", "desc" };
    public List<int> PageSizeOptions { get; set; } = new() { 10, 25, 50, 100 };
}

/// <summary>
/// Ringkasan jumlah exit clearance berdasarkan status dan status tanggungan Finance.
/// </summary>
public sealed class ExitClearanceSummaryResponse
{
    public int TotalData { get; set; }
    public int Draft { get; set; }
    public int InProgress { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int PendingFinance { get; set; }
}

/// <summary>
/// Item daftar exit clearance untuk tabel / list view.
/// </summary>
public class ExitClearanceListResponse
{
    public Guid Id { get; set; }
    public string ClearanceNumber { get; set; } = string.Empty;
    public Guid EmployeeSeparationId { get; set; }
    public string SeparationNumber { get; set; } = string.Empty;
    public Guid WorkforceProfileId { get; set; }
    public Guid? EmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string PositionName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string ClearanceStatus { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public bool IsDepartmentCleared { get; set; }
    public bool IsAssetCleared { get; set; }
    public bool IsFinanceCleared { get; set; } // Dihitung authoritative dari Finance (AC 3)
    public bool IsPayrollCleared { get; set; }
    public bool IsItAccessCleared { get; set; }
    public bool IsHrCleared { get; set; }
    public decimal OutstandingReceivableAmount { get; set; }
    public bool CanComplete { get; set; } // true jika IsFinanceCleared == true dan belum selesai
    public DateTime CreateDateTime { get; set; }
}

/// <summary>
/// Detail lengkap exit clearance termasuk informasi rincian piutang Finance.
/// </summary>
public sealed class ExitClearanceDetailResponse : ExitClearanceListResponse
{
    public int OpenReceivableCount { get; set; }
    public DateOnly? OldestDueDate { get; set; }
    public string? Notes { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public string? CompletedByName { get; set; }
    public int TotalItem { get; set; }
    public int ClearedItem { get; set; }
    public Guid? OffboardingChecklistId { get; set; }
}

/// <summary>
/// Permintaan inisiasi exit clearance baru.
/// </summary>
public sealed class CreateExitClearanceRequest
{
    [Required] public Guid EmployeeSeparationId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? DueDate { get; set; }
    [MaxLength(1500)] public string? Notes { get; set; }
}

/// <summary>
/// Permintaan pembaruan checklist departemen HR.
/// CATATAN: IsFinanceCleared sengaja TIDAK disertakan di sini (AC 3, FIN-DEC-193).
/// Status Finance bebas tanggungan MUST dihitung authoritative dari Finance, bukan diedit manual.
/// </summary>
public sealed class UpdateExitClearanceChecklistRequest
{
    public bool? IsDepartmentCleared { get; set; }
    public bool? IsAssetCleared { get; set; }
    public bool? IsPayrollCleared { get; set; }
    public bool? IsItAccessCleared { get; set; }
    public bool? IsHrCleared { get; set; }
    [MaxLength(1500)] public string? Notes { get; set; }
}

/// <summary>
/// Permintaan penyelesaian exit clearance.
/// </summary>
public sealed class CompleteExitClearanceRequest
{
    [MaxLength(1500)] public string? Notes { get; set; }
}

/// <summary>
/// Permintaan pembatalan exit clearance.
/// </summary>
public sealed class CancelExitClearanceRequest
{
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Respons pemeriksaan langsung ke Finance Clearance API (INT-HR-FIN-002 §P.4).
/// </summary>
public sealed class ExitClearanceFinanceCheckResponse
{
    public Guid ClearanceId { get; set; }
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public bool IsFinanceCleared { get; set; }
    public decimal OutstandingAmount { get; set; }
    public int OpenReceivableCount { get; set; }
    public DateOnly? OldestDueDate { get; set; }
    public DateTimeOffset CheckedAt { get; set; }
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Parameter query pencarian exit clearance.
/// </summary>
public sealed class ExitClearanceQuery
{
    public string? Search { get; set; }
    public string? ClearanceStatus { get; set; }
    public Guid? EmployeeId { get; set; }
    public bool? IsFinanceCleared { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreateDateTime";
    public string SortDirection { get; set; } = "desc";
}

