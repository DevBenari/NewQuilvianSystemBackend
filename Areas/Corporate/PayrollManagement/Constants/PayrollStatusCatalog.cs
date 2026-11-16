namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

/// <summary>
/// PrlPayrollPayment.PaymentStatus (kolom string, tanpa perubahan skema). Scheduled = instruksi sudah dikirim ke Finance dan
/// menunggu hasil bank; Paid hanya boleh ditetapkan lewat aksi ACK oleh pemegang izin Acknowledge: hasil bank dari Finance, atau
/// (PAY-BE-11) konfirmasi manual petugas Payroll berwenang selama belum ada koneksi bank (lihat PayrollConfirmationSources).
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

/// <summary>PrlPayrollPayslip.PayslipStatus (kolom string, tanpa perubahan skema).</summary>
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
/// FINANCE_AP (utang dokter di Finance AP). Disimpan sebagai klaim pada PrlPayrollVariableInput (SourceType = MdfServiceFee).
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
    /// <summary>Satu baris per jenis tunjangan dari Struktur Gaji (PGJ-C1, PRL-DEC-02). SourceId = MstSalaryStructureAllowance.Id.</summary>
    public const string StructureAllowance = "PAY_STRUCTURE_ALLOWANCE";
    /// <summary>Tunjangan transport bulanan tetap (PGJ-C1, PRL-DEC-46). SourceId = PrlTransportAllowance.Id.</summary>
    public const string TransportAllowance = "PAY_TRANSPORT_ALLOWANCE";
    public const string Overtime = "PAY_OVERTIME";
    public const string AttendanceAllowance = "PAY_ATTENDANCE_ALLOWANCE";
    public const string VariableEarning = "PAY_VARIABLE_EARNING";
    public const string AttendanceDeduction = "PAY_ATTENDANCE_DEDUCTION";
    public const string UnpaidLeave = "PAY_UNPAID_LEAVE";
    public const string VariableDeduction = "PAY_VARIABLE_DEDUCTION";
    public const string Tax = "PAY_TAX";
    public const string InsuranceEmployee = "PAY_INSURANCE_EMPLOYEE";
}

/// <summary>Nilai PrlPayrollVariableInput.SourceType yang dikenal modul Payroll.</summary>
public static class PayrollVariableInputSources
{
    public const string FinanceReceivableInstallment = "FinanceReceivableInstallment";
    public const string MedicalServiceFee = "MdfServiceFee";
    /// <summary>Insentif, bonus, honor tugas tambahan yang diinput petugas; dihitung hanya bila Approved/Applied (PGJ-C1, PRL-DEC-07).</summary>
    public const string ManualInput = "PayrollManualInput";
}

/// <summary>Nilai PrlPayrollEmployeeComponent.SourceType baru untuk baris tunjangan dari mesin hitung (PGJ-C1).</summary>
public static class PayrollEngineComponentSources
{
    public const string SalaryStructureAllowance = "SalaryStructureAllowance";
    public const string TransportAllowance = "TransportAllowance";
}
