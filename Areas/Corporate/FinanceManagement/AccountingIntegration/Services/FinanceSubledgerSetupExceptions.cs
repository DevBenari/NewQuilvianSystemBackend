namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>
/// Pelanggaran aturan bisnis pemetaan atau saldo awal subledger (HTTP 422).
/// </summary>
public class FinanceSubledgerValidationException : Exception
{
    public FinanceSubledgerValidationException(string message) : base(message) { }
}

/// <summary>
/// Konflik data pemetaan akun control atau saldo awal ganda/terkunci (HTTP 409).
/// </summary>
public class FinanceSubledgerConflictException : Exception
{
    public FinanceSubledgerConflictException(string message) : base(message) { }
}

/// <summary>
/// Parameter atau format pemetaan akun control subledger tidak sah (HTTP 400).
/// </summary>
public class FinanceSubledgerBadRequestException : Exception
{
    public FinanceSubledgerBadRequestException(string message) : base(message) { }
}
