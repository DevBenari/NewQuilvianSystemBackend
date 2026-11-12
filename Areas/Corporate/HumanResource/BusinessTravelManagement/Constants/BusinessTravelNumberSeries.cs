using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BusinessTravelManagement.Constants
{
    /// <summary>
    /// Deret nomor perjalanan dinas yang dialokasikan <c>NumberSeriesAllocator</c> (<c>QBE-CODE-003</c>).
    /// </summary>
    public static class BusinessTravelNumberSeries
    {
        public static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public const int SequenceDigits = 4;

        public const string ExpenseClaimKey = "HRD_TRV_EXPENSE_CLAIM";
        public const string ExpenseClaimPrefix = "TRV-EXP";
        public const string ExpenseClaimResetPolicy = NumberSeriesResetPolicies.Monthly;

        public const string PayrollDeductionKey = "HRD_TRV_PAYROLL_DEDUCTION";
        public const string PayrollDeductionPrefix = "DED-TRV";
        public const string PayrollDeductionResetPolicy = NumberSeriesResetPolicies.Monthly;
    }
}
