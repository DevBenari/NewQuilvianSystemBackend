namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

/// <summary>
/// TrxPayrollPayment.PaymentStatus (kolom string, tanpa perubahan skema). Scheduled = instruksi sudah dikirim ke Finance dan
/// menunggu hasil bank; Paid hanya boleh ditetapkan lewat ACK Finance yang berwenang, tidak pernah dari layar Payroll.
/// </summary>
public static class PayrollPaymentStatuses
{
    public const string Draft = "Draft";
    public const string Scheduled = "Scheduled";
    public const string Processing = "Processing";
    public const string Paid = "Paid";
    public const string Failed = "Failed";
    public const string Cancelled = "Cancelled";
    public const string PartiallyReversed = "PartiallyReversed";
    public const string Reversed = "Reversed";

    /// <summary>Status instruksi yang masih "hidup": tidak boleh ada instruksi kedua untuk penerima yang sama.</summary>
    public static bool IsLive(string? status) =>
        string.Equals(status, Scheduled, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Processing, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Paid, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, PartiallyReversed, StringComparison.OrdinalIgnoreCase);
}

/// <summary>TrxPayrollPayslip.PayslipStatus (kolom string, tanpa perubahan skema).</summary>
public static class PayrollPayslipStatuses
{
    public const string Draft = "Draft";
    public const string Generated = "Generated";
    public const string Published = "Published";
    public const string Downloaded = "Downloaded";
    public const string Superseded = "Superseded";
    public const string Cancelled = "Cancelled";
}

/// <summary>
/// Jalur pembayaran satu hak jasa medis. Tepat satu jalur aktif per sumber: PAYROLL (masuk slip gaji penerima payroll) atau
/// FINANCE_AP (utang dokter di Finance AP). Disimpan sebagai klaim pada TrxPayrollVariableInput (SourceType = MdfServiceFee).
/// </summary>
public static class PayrollMedicalFeePaymentPaths
{
    public const string Payroll = "PAYROLL";
    public const string FinanceAp = "FINANCE_AP";
}

/// <summary>Kode komponen yang dibuat mesin kalkulasi. Awalan PAY_ menandai baris milik mesin (dibangun ulang tiap hitung).</summary>
public static class PayrollEngineComponentCodes
{
    public const string Prefix = "PAY_";
    public const string BaseSalary = "PAY_BASE_SALARY";
    public const string FixedAllowance = "PAY_FIXED_ALLOWANCE";
    public const string Overtime = "PAY_OVERTIME";
    public const string AttendanceAllowance = "PAY_ATTENDANCE_ALLOWANCE";
    public const string VariableEarning = "PAY_VARIABLE_EARNING";
    public const string AttendanceDeduction = "PAY_ATTENDANCE_DEDUCTION";
    public const string UnpaidLeave = "PAY_UNPAID_LEAVE";
    public const string VariableDeduction = "PAY_VARIABLE_DEDUCTION";
    public const string Tax = "PAY_TAX";
    public const string InsuranceEmployee = "PAY_INSURANCE_EMPLOYEE";
}

/// <summary>Nilai TrxPayrollVariableInput.SourceType yang dikenal modul Payroll.</summary>
public static class PayrollVariableInputSources
{
    public const string FinanceReceivableInstallment = "FinanceReceivableInstallment";
    public const string MedicalServiceFee = "MdfServiceFee";
}
