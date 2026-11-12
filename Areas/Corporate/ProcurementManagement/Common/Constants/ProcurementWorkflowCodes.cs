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

        public const string OverBudgetCondition = "PBJ_OVER_BUDGET";
        public const string OutOfPlanCondition = "PBJ_OUT_OF_PLAN";
    }
}
