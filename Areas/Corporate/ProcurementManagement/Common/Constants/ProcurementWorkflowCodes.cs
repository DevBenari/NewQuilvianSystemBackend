namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Kode definisi dan kode kondisi mesin workflow bersama (Wfl) yang dipakai Pengadaan —
    /// <c>02-backend-architecture.md</c> bagian 8 (PBJ-DEC-007, 016, 019, 030).
    /// </summary>
    /// <remarks>
    /// Contoh: PR mendesak memakai definisi <c>PBJ-PR-MENDESAK</c>; PR yang nilai belinya melebihi
    /// sisa pagu mengirim kode kondisi <c>PBJ_OVER_BUDGET</c> sehingga langkah Direktur Keuangan
    /// ikut dibuat.
    /// </remarks>
    public static class ProcurementWorkflowCodes
    {
        public const string WorkflowCategory = "Procurement";

        public const string PurchaseRequestDefinition = "PBJ-PR";
        public const string UrgentPurchaseRequestDefinition = "PBJ-PR-MENDESAK";
        public const string NeedPlanDefinition = "PBJ-RKB";
        public const string PackagePlanDefinition = "PBJ-RPK";
        public const string BudgetDefinition = "PBJ-PAGU";

        // Manajemen vendor (BE-PBJ-VD-001; PBJ-DEC-021, PBJ-ASM-003): usulan blokir dan buka blokir.
        public const string VendorBlockDefinition = "PBJ-VND-BLOKIR";
        public const string VendorUnblockDefinition = "PBJ-VND-BUKA";

        // Pemilihan penyedia (BE-PBJ-PS-001; PBJ-DEC-072): persetujuan persiapan paket dan penetapan penyedia.
        public const string PackagePreparationDefinition = "PBJ-PKT-PERSIAPAN";
        public const string PackageAwardDefinition = "PBJ-PKT-PENETAPAN";

        public const string OverBudgetCondition = "PBJ_OVER_BUDGET";
        public const string OutOfPlanCondition = "PBJ_OUT_OF_PLAN";

        /// <summary>
        /// Metode paket dipilih di luar rentang HPS-nya dengan alasan (PBJ-DEC-068). Contoh: HPS Rp 42.750.000 tetapi
        /// Tim Pengadaan memilih Pembandingan Penawaran; langkah persetujuan tambahan ikut dibuat.
        /// </summary>
        public const string MethodOutOfRangeCondition = "PBJ_METHOD_OUT_OF_RANGE";

        /// <summary>
        /// Nilai penetapan melebihi HPS yang disetujui, dengan alasan (PBJ-DEC-073). Contoh: penetapan Rp 44.000.000
        /// untuk HPS Rp 42.750.000.
        /// </summary>
        public const string OverHpsCondition = "PBJ_OVER_HPS";
    }
}
