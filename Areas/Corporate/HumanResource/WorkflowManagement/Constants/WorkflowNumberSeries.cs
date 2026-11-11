using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.WorkflowManagement.Constants
{
    /// <summary>
    /// Definisi deret nomor Workflow V2 yang dialokasikan provider bersama
    /// <c>NumberSeriesAllocator</c> (<c>QBE-CODE-003</c>, <c>QBE-CODE-006</c>).
    /// </summary>
    /// <remarks>
    /// Nomor permohonan workflow tetap berbentuk <c>WF-{TIPE}-{yyyyMMdd}-{urut}</c>. Karena awalan
    /// provider dibatasi 15 karakter dan nama tipe bisa panjang, deret dialokasikan dengan awalan
    /// <c>WF</c> harian, lalu nama tipe disisipkan oleh service pemanggil. Nomor delegasi Plh tetap
    /// <c>SK-PLH-{yyyyMMdd}-{urut}</c>.
    /// </remarks>
    public static class WorkflowNumberSeries
    {
        public static readonly Guid SystemFallbackUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        public const string InstanceKey = "HRD_WF_INSTANCE";
        public const string InstancePrefix = "WF";
        public const string InstanceResetPolicy = NumberSeriesResetPolicies.Daily;
        public const int InstanceDigits = 4;

        public const string DelegationKey = "HRD_WF_DELEGATION";
        public const string DelegationPrefix = "SK-PLH";
        public const string DelegationResetPolicy = NumberSeriesResetPolicies.Daily;
        public const int DelegationDigits = 4;

        /// <summary>Mengambil nomor urut (bagian sesudah tanda hubung terakhir) dari nomor yang diterbitkan provider.</summary>
        public static string TrailingSequence(string allocatedNumber)
        {
            var index = allocatedNumber.LastIndexOf('-');
            return index >= 0 ? allocatedNumber[(index + 1)..] : allocatedNumber;
        }
    }
}
