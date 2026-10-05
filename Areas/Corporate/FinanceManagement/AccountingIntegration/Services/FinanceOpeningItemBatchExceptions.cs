namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;

/// <summary>Parameter, berkas, atau format batch migrasi tagihan lama tidak sah (HTTP 400).</summary>
public sealed class OpeningItemBatchBadRequestException : Exception
{
    public OpeningItemBatchBadRequestException(string message) : base(message) { }
}

/// <summary>Pelanggaran aturan bisnis batch migrasi tagihan lama (HTTP 422).</summary>
public sealed class OpeningItemBatchValidationException : Exception
{
    public OpeningItemBatchValidationException(string message) : base(message) { }
}

/// <summary>Tindakan tidak berlaku untuk status batch saat ini (HTTP 409).</summary>
public sealed class OpeningItemBatchConflictException : Exception
{
    public OpeningItemBatchConflictException(string message) : base(message) { }
}

/// <summary>
/// Nilai sah menurut kontrak tetapi sistem belum siap memenuhinya — pola sama dengan
/// FinanceTransactionProofNotConfiguredException/FIN-VAL-220 (HTTP 503).
/// </summary>
public sealed class OpeningItemBatchNotReadyException : Exception
{
    public OpeningItemBatchNotReadyException(string message) : base(message) { }
}
