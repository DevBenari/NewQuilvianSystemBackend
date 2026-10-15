namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>Parameter, berkas, atau format batch hasil potongan tidak sah (HTTP 400).</summary>
public sealed class FinancePayrollDeductionBatchBadRequestException : Exception
{
    public FinancePayrollDeductionBatchBadRequestException(string message) : base(message) { }
}

/// <summary>Pelanggaran aturan bisnis batch hasil potongan gaji (HTTP 422).</summary>
public sealed class FinancePayrollDeductionBatchValidationException : Exception
{
    public FinancePayrollDeductionBatchValidationException(string message) : base(message) { }
}

/// <summary>Konflik status batch atau konkurensi (RowVersion) (HTTP 409).</summary>
public sealed class FinancePayrollDeductionBatchConflictException : Exception
{
    public FinancePayrollDeductionBatchConflictException(string message) : base(message) { }
}

/// <summary>Pelanggaran hak akses atau maker-checker (HTTP 403).</summary>
public sealed class FinancePayrollDeductionBatchForbiddenException : Exception
{
    public FinancePayrollDeductionBatchForbiddenException(string message) : base(message) { }
}

/// <summary>Batch hasil potongan tidak ditemukan (HTTP 404).</summary>
public sealed class FinancePayrollDeductionBatchNotFoundException : Exception
{
    public FinancePayrollDeductionBatchNotFoundException(string message) : base(message) { }
}

