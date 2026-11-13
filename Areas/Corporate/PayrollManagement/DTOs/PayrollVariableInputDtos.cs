namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.DTOs;

// PAY-BE-19 (blueprint payroll-gaji PGJ-C1, api-contract.md grup Payroll Variable Input).
// Penghasilan variabel manual: insentif, bonus, honor tugas tambahan (SourceType = PayrollManualInput).
// Seluruh nominal di bawah ini sensitif dan tidak boleh masuk log.

public sealed class PayrollVariableInputQuery
{
    public string? InputStatus { get; set; }
    public string? InputType { get; set; }

    /// <summary>Nama atau nomor pegawai, atau nomor input.</summary>
    public string? Search { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class CreatePayrollVariableInputRequest
{
    public Guid PayrollRunEmployeeId { get; set; }
    public Guid PayrollComponentId { get; set; }

    /// <summary>Incentive, Bonus, atau AdditionalDutyHonorarium.</summary>
    public string InputType { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    /// <summary>Wajib untuk AdditionalDutyHonorarium: menyebut tugasnya (VI-06).</summary>
    public string? Notes { get; set; }

    public string? AttachmentPath { get; set; }
}

public sealed class UpdatePayrollVariableInputContentRequest
{
    public Guid PayrollComponentId { get; set; }
    public string InputType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public string? ExpectedRowVersion { get; set; }
}

public sealed class ImportPayrollVariableInputRequest
{
    public List<ImportPayrollVariableInputRow> Rows { get; set; } = new();
}

public sealed class ImportPayrollVariableInputRow
{
    public string? EmployeeNumber { get; set; }
    public string? PayrollComponentCode { get; set; }
    public string? InputType { get; set; }
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
}

public sealed class PayrollVariableInputImportResult
{
    public int CreatedCount { get; set; }
    public List<PayrollVariableInputImportFailedRow> FailedRows { get; set; } = new();
}

public sealed class PayrollVariableInputImportFailedRow
{
    /// <summary>Nomor baris pada kiriman, mulai dari 1.</summary>
    public int RowNumber { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class PayrollVariableInputActionRequest
{
    public string? ExpectedRowVersion { get; set; }
}

public sealed class PayrollVariableInputDecisionRequest
{
    public string? Reason { get; set; }
    public string? ExpectedRowVersion { get; set; }
}

public sealed class PayrollVariableInputBulkRequest
{
    public List<Guid> InputIds { get; set; } = new();
}

public sealed class PayrollVariableInputBulkResult
{
    public List<Guid> Succeeded { get; set; } = new();
    public List<PayrollVariableInputBulkFailure> Failed { get; set; } = new();
}

public sealed class PayrollVariableInputBulkFailure
{
    public Guid InputId { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public sealed class PayrollVariableInputResponse
{
    public Guid Id { get; set; }
    public string InputNumber { get; set; } = string.Empty;
    public Guid PayrollRunEmployeeId { get; set; }
    public string EmployeeNumber { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public Guid PayrollComponentId { get; set; }
    public string PayrollComponentCode { get; set; } = string.Empty;
    public string PayrollComponentName { get; set; } = string.Empty;
    public string InputType { get; set; } = string.Empty;
    public string InputStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Notes { get; set; }
    public string? AttachmentPath { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string? SubmittedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedByName { get; set; }
    public string? DecisionReason { get; set; }

    /// <summary>Aksi yang sah untuk status input dan status run saat ini: update, submit, approve, reject, cancel.</summary>
    public List<string> AvailableActions { get; set; } = new();

    public string RowVersion { get; set; } = string.Empty;
}

/// <summary>Impor ditolak seluruhnya (VI-08). Dibawa ke controller agar daftar baris gagal ikut di <c>Errors</c> respons 400.</summary>
public sealed class PayrollVariableInputImportException(string message, PayrollVariableInputImportResult result) : Exception(message)
{
    public PayrollVariableInputImportResult Result { get; } = result;
}
