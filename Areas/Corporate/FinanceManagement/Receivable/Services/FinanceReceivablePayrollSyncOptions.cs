namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;

/// <summary>
/// BE-FIN-094, FIN-DEC-190. Pola sama persis dengan <c>LeavePayrollIntegrationOptions</c> (HR,
/// LeaveManagement) — kode komponen payroll dikonfigurasi, bukan ditebak atau dibuat otomatis.
/// <see cref="PayrollComponentCode"/> MUST sudah ada di <c>MstPayrollComponent</c> sebelum sinkron
/// pertama berhasil; bila belum, sinkron dilewati per baris dan dicatat sebagai temuan, BUKAN
/// dibuat sendiri oleh Finance (konfigurasi komponen payroll adalah keputusan bisnis HR).
/// </summary>
public sealed class FinanceReceivablePayrollSyncOptions
{
    public bool Enabled { get; set; } = true;
    public string PayrollComponentCode { get; set; } = "FINANCE_INSTALLMENT_DEDUCTION";
    public string CurrencyCode { get; set; } = "IDR";
}
