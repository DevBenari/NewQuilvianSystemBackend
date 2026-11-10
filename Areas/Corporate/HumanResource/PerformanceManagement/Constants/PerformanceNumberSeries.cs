using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Constants
{
    /// <summary>
    /// Definisi deret nomor penugasan FPPE yang dialokasikan provider bersama
    /// <c>NumberSeriesAllocator</c> (<c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>).
    /// Sebelumnya nomor dibentuk dari bulan ditambah angka acak 4 digit. Bentuk tetap
    /// <c>FPPE-202611-0001</c>, hanya bagian akhirnya yang kini berurutan.
    /// </summary>
    public static class PerformanceNumberSeries
    {
        public static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public const string FppeEngagementKey = "HRD_FPPE_ENGAGEMENT";
        public const string FppeEngagementPrefix = "FPPE";
        public const string FppeEngagementResetPolicy = NumberSeriesResetPolicies.Monthly;
        public const int FppeEngagementDigits = 4;
    }
}
