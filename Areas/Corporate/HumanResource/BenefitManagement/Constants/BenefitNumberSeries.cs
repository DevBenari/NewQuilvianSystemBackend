using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Constants
{
    /// <summary>
    /// Definisi deret nomor dokumen Benefit Management yang dialokasikan provider bersama
    /// <c>NumberSeriesAllocator</c> (<c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 nomor klaim COB dan referensi koreksi plafon dibentuk dari tanggal
    /// ditambah angka acak (<c>new Random().Next</c>), sehingga dua dokumen bisa bernomor sama.
    /// Nomor kepesertaan placeholder yang dibuat jalur pendaftaran tanggungan ESS sebelumnya memakai
    /// awalan <c>ENR-</c> acak; kini memakai deret yang sama dengan <c>EmployeeBenefitEnrollmentService</c>
    /// (<c>HRD_BENEFIT_ENROLLMENT</c>, awalan <c>BEN</c>) supaya satu tabel hanya punya satu format nomor.
    /// </remarks>
    public static class BenefitNumberSeries
    {
        public static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        /// <summary>Deret kepesertaan benefit — identik dengan yang dipakai <c>EmployeeBenefitEnrollmentService</c>.</summary>
        public const string EnrollmentKey = "HRD_BENEFIT_ENROLLMENT";
        public const string EnrollmentPrefix = "BEN";
        public const string EnrollmentResetPolicy = NumberSeriesResetPolicies.Never;
        public const int EnrollmentDigits = 6;

        /// <summary>Klaim koordinasi manfaat (COB) ESS. Bentuk: <c>CLM-COB-20261110-0001</c>.</summary>
        public const string CobClaimKey = "HRD_BENEFIT_COB_CLAIM";
        public const string CobClaimPrefix = "CLM-COB";
        public const string CobClaimResetPolicy = NumberSeriesResetPolicies.Daily;
        public const int CobClaimDigits = 4;

        /// <summary>Referensi koreksi manual plafon berobat. Bentuk: <c>ADJ-20261110-0001</c>.</summary>
        public const string LedgerAdjustmentKey = "HRD_BENEFIT_LEDGER_ADJUSTMENT";
        public const string LedgerAdjustmentPrefix = "ADJ";
        public const string LedgerAdjustmentResetPolicy = NumberSeriesResetPolicies.Daily;
        public const int LedgerAdjustmentDigits = 4;
    }
}
