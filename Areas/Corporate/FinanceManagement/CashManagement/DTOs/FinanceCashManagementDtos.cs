namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;

/// <summary>
/// Parameter penyaringan daftar setoran bank (FIN-API-1.0).
/// </summary>
public sealed class BankDepositQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public Guid? BankAccountId { get; set; }
    public string? Status { get; set; }
    public DateOnly? DepositDateFrom { get; set; }
    public DateOnly? DepositDateTo { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "depositdate";
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Model respons data setoran bank.
/// </summary>
public sealed class BankDepositResponse
{
    public Guid Id { get; set; }
    public string DepositNumber { get; set; } = string.Empty;
    public DateOnly DepositDate { get; set; }
    public Guid BankAccountId { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string? BankName { get; set; }
    public decimal Amount { get; set; }
    public Guid? CashierShiftId { get; set; }
    public string? ShiftNumber { get; set; }
    public string? DepositSlipNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? PostedBy { get; set; }
    public DateTimeOffset? PostedAt { get; set; }
    public string? Notes { get; set; }
    public Guid RowVersion { get; set; }
}

/// <summary>
/// Permintaan pembuatan draf setoran bank baru.
/// </summary>
public sealed class CreateBankDepositRequest
{
    public Guid BankAccountId { get; set; }
    public DateOnly DepositDate { get; set; }
    public decimal Amount { get; set; }
    public Guid? CashierShiftId { get; set; }
    public string? DepositSlipNumber { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Permintaan posting setoran bank dengan optimistik concurrency dan nomor slip setoran opsional.
/// </summary>
public sealed class PostBankDepositRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string? DepositSlipNumber { get; set; }
}

/// <summary>
/// Permintaan verifikasi setoran bank dengan rekening koran.
/// </summary>
public sealed class VerifyBankDepositRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string? DepositSlipNumber { get; set; }
}

/// <summary>
/// Permintaan pembatalan setoran bank.
/// </summary>
public sealed class CancelBankDepositRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Parameter kueri saldo kas tersedia.
/// </summary>
public sealed class AvailableBalanceQuery
{
    public DateOnly? Date { get; set; }
    public Guid? CashierShiftId { get; set; }
}

/// <summary>
/// Rincian saldo kas tersedia yang dihitung backend (FIN-DEC-017, FR-FIN-060).
/// </summary>
public sealed class AvailableCashBalanceResponse
{
    public DateOnly Date { get; set; }
    public Guid? CashierShiftId { get; set; }
    public decimal TotalCashierCash { get; set; }
    public decimal TotalDepositedCash { get; set; }
    public decimal AvailableBalance { get; set; }
}

/// <summary>
/// Parameter penyaringan riwayat kas harian.
/// </summary>
public sealed class DailyCashQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public DateOnly? DateFrom { get; set; }
    public DateOnly? DateTo { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "cashdate";
    public string SortDirection { get; set; } = "desc";
}

/// <summary>
/// Model respons posisi kas harian (FinDailyCashSnapshot).
/// </summary>
public sealed class DailyCashResponse
{
    public Guid Id { get; set; }
    public DateOnly CashDate { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal CashReceiptAmount { get; set; }
    public decimal OtherReceiptAmount { get; set; }
    public decimal DisbursementAmount { get; set; }
    public decimal BankDepositAmount { get; set; }
    public decimal ClosingBalance { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ClosedBy { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }
    public Guid RowVersion { get; set; }
}

/// <summary>
/// Rincian per shift kasir yang menyusun angka kas tunai harian.
/// </summary>
public sealed class DailyCashShiftBreakdownItem
{
    public Guid ShiftId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public Guid CashierId { get; set; }
    public Guid RegisterId { get; set; }
    public decimal OpeningCash { get; set; }
    public decimal SystemCash { get; set; }
    public decimal PhysicalCash { get; set; }
    public decimal Variance { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal EffectiveCash { get; set; }
}

/// <summary>
/// Rincian penelusuran angka kas harian ke shift kasir dan setoran bank.
/// </summary>
public sealed class DailyCashBreakdownResponse
{
    public DailyCashResponse Snapshot { get; set; } = new();
    public List<DailyCashShiftBreakdownItem> CashierShifts { get; set; } = [];
    public List<BankDepositResponse> BankDeposits { get; set; } = [];
}

/// <summary>
/// Permintaan penutupan kas harian.
/// </summary>
public sealed class CloseDailyCashRequest
{
    public Guid? ExpectedRowVersion { get; set; }
    public decimal? OpeningBalance { get; set; }
    public decimal? OtherReceiptAmount { get; set; }
    public decimal? DisbursementAmount { get; set; }
    public string? Notes { get; set; }
}
