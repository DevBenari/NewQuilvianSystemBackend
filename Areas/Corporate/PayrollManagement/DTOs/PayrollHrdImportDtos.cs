using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

public sealed class PayrollHrdReadinessQuery
{
    /// <summary>Bila true, hanya penerima yang memiliki masalah kelengkapan.</summary>
    public bool OnlyIssues { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Kelengkapan data satu penerima untuk dihitung: penetapan gaji, rekening, dan input HRD yang sudah masuk.</summary>
public sealed class PayrollHrdReadinessItemResponse
{
    public Guid PayrollRunEmployeeId { get; set; }
    public Guid WorkforceProfileId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public bool HasPayrollProfile { get; set; }
    public bool HasApprovedSalaryAssignment { get; set; }
    public bool HasVerifiedBankAccount { get; set; }
    public int AttendanceDayCount { get; set; }
    public int OvertimeItemCount { get; set; }
    public int VariableInputCount { get; set; }

    /// <summary>Masalah kelengkapan dalam bahasa Indonesia; kosong berarti siap dihitung.</summary>
    public List<string> Issues { get; set; } = new();
}

/// <summary>
/// Kesiapan sebuah run untuk dihitung: populasi penerima dan kelengkapan handoff data HRD pada cutoff periode. Hanya baca.
/// Kekurangan data sumber diperbaiki di HRD (bukan diubah Payroll); setelah itu impor diulang (idempoten).
/// </summary>
public sealed class PayrollRunHrdReadinessResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public bool IsLocked { get; set; }
    public string PeriodCode { get; set; } = string.Empty;
    public DateTime? AttendanceCutoffDate { get; set; }
    public DateTime? VariableInputCutoffDate { get; set; }

    public int TotalEmployees { get; set; }
    public int EmployeesWithAttendance { get; set; }
    public int EmployeesWithOvertime { get; set; }
    public int EmployeesWithVariableInput { get; set; }
    public int EmployeesWithoutApprovedSalary { get; set; }
    public int EmployeesWithoutVerifiedBank { get; set; }
    public int EmployeesWithIssues { get; set; }

    /// <summary>Siap dihitung bila ada penerima, tidak terkunci, dan statusnya masih menerima input.</summary>
    public bool CanImport { get; set; }
    public PagedResult<PayrollHrdReadinessItemResponse> Employees { get; set; } = new();
}

public sealed class PayrollHrdImportRequest
{
    public bool IncludeAttendance { get; set; } = true;
    public bool IncludeLeave { get; set; } = true;
    public bool IncludeOvertime { get; set; } = true;
    [MaxLength(1000)] public string? Notes { get; set; }
    [MaxLength(40)] public string? ExpectedRowVersion { get; set; }
}

public sealed class PayrollHrdImportItemResponse
{
    public string Reference { get; set; } = string.Empty;
    public string? WorkforceName { get; set; }
    public DateOnly? Date { get; set; }
    public bool Success { get; set; }

    /// <summary>Dibuat | Diperbarui | Idempoten | Gagal | ... (apa adanya dari HRD, diterjemahkan sederhana).</summary>
    public string Result { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public sealed class PayrollHrdSourceResultResponse
{
    /// <summary>Attendance | Leave | Overtime.</summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>Completed | CompletedWithIssues | Failed | Skipped.</summary>
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int TotalTarget { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int IdempotentCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Warnings { get; set; } = new();
    public List<PayrollHrdImportItemResponse> Items { get; set; } = new();
}

public sealed class PayrollHrdImportResponse
{
    public Guid PayrollRunId { get; set; }
    public string RunNumber { get; set; } = string.Empty;
    public string RunStatus { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime CompletedAt { get; set; }
    public int TotalFailed { get; set; }
    public bool HasIssues { get; set; }
    public List<PayrollHrdSourceResultResponse> Sources { get; set; } = new();
}
