namespace QuilvianSystemBackend.Areas.Corporate.PayrollManagement.Constants;

/// <summary>
/// Nilai PrlPayrollVariableInput.InputType untuk input manual (SourceType = PayrollVariableInputSources.ManualInput).
/// Blueprint payroll-gaji PGJ-C1: insentif bulanan, bonus dalam run bulanan, dan honor tugas tambahan (PRL-DEC-07, 31).
/// </summary>
public static class PayrollManualInputTypes
{
    public const string Incentive = "Incentive";
    public const string Bonus = "Bonus";

    /// <summary>Honor tugas tambahan; wajib menyebut tugasnya di Notes (validasi VI-06).</summary>
    public const string AdditionalDutyHonorarium = "AdditionalDutyHonorarium";

    public static readonly IReadOnlyList<string> All = new[] { Incentive, Bonus, AdditionalDutyHonorarium };
}

/// <summary>
/// Nilai PrlPayrollVariableInput.InputStatus untuk input manual (state-transition-matrix.md bagian 1, PGJ-C1).
/// Applied dan Cancelled-oleh-sistem ditulis aksi run (PAY-BE-20); sisanya oleh petugas (PAY-BE-19).
/// </summary>
public static class PayrollManualInputStatuses
{
    public const string Draft = "Draft";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
    public const string Cancelled = "Cancelled";
    public const string Applied = "Applied";

    public static readonly IReadOnlyList<string> All = new[] { Draft, Submitted, Approved, Rejected, Cancelled, Applied };
}
