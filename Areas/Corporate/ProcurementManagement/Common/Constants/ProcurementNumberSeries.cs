using QuilvianSystemBackend.Areas.Platform.NumberSeriesManagement.Constants;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Kunci deret nomor bisnis Pengadaan untuk <c>NumberSeriesAllocator</c> (PBJ-DEC-025 bagian
    /// yang tetap berlaku, PBJ-DEC-031). Nomor hanya dialokasikan Module Service; Count+1 atau
    /// Max+1 tidak dipakai (QBE-CODE-002, QBE-CODE-003).
    /// </summary>
    /// <remarks>
    /// Mengikuti invariant Platform <c>INV-PLT-004</c>: deret tidak pernah diulang, tanpa tahun
    /// di nomor. Contoh hasil: <c>PR-000031</c>, <c>RKB-000004</c>, <c>RPK-000002</c>,
    /// <c>KTL-000045</c>. Nomor dialokasikan saat dokumen diajukan, bukan saat draft; nomor dari
    /// transaksi yang gagal hangus dan tidak dipakai ulang (<c>DEC-PLT-008</c>).
    /// </remarks>
    public static class ProcurementNumberSeries
    {
        public const string ResetPolicy = NumberSeriesResetPolicies.Never;
        public const int SequenceDigits = 6;

        public const string PurchaseRequestSequenceKey = "PBJ_PR";
        public const string NeedPlanSequenceKey = "PBJ_NEED_PLAN";
        public const string PackagePlanSequenceKey = "PBJ_PACKAGE_PLAN";
        public const string ItemSequenceKey = "PBJ_ITEM";

        public const string PurchaseRequestPrefix = "PR";
        public const string NeedPlanPrefix = "RKB";
        public const string PackagePlanPrefix = "RPK";
        public const string ItemPrefix = "KTL";
    }
}
