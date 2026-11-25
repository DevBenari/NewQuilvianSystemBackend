namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Butir hak akses modul Pengadaan — <c>contracts/permission-audit-matrix.md</c>
    /// (PBJ-PP-PERM 1.1). Konstanta mencegah selisih satu huruf antara <c>[AccessController]</c>,
    /// <c>[AccessAction]</c>, dan <c>[AccessPermission]</c> yang berujung 403 permanen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Hak akses tidak ditentukan kode.</b> Pemegangnya diputuskan admin di Pengaturan →
    /// Manajemen Role → Akses Role. Tidak ada nama peran, departemen, maupun jabatan di modul ini.
    /// </para>
    /// <para>
    /// <b>Lihat semua unit (PBJ-DEC-036).</b> Cakupan lintas unit adalah aksi
    /// <see cref="ReadAllUnitsAction"/> pada resource tiap jenis dokumen, misalnya
    /// <c>ProcurementRequest : ReadAllUnits</c>. Aksi itu didaftarkan lewat
    /// <c>[assembly: AccessExplicitPermission]</c> oleh task yang membuat controller dokumennya,
    /// karena penanda wajib menunjuk resource yang sudah punya endpoint.
    /// </para>
    /// </remarks>
    public static class ProcurementPermissions
    {
        public const string ModuleCode = "CORPORATE_PROCUREMENT_MANAGEMENT";
        public const string ModuleName = "Corporate Procurement Management";
        public const string AreaName = "Corporate";

        /// <summary>Kode modul layar Akses Role untuk master Pengadaan (BE-PBJ-003).</summary>
        public const string MasterDataModuleCode = "CORPORATE_PROCUREMENT_MANAGEMENT_MASTER_DATA";
        public const string MasterDataModuleName = "Corporate Procurement Management Master Data";

        /// <summary>Kode modul layar Akses Role untuk dokumen bersama (lampiran, BE-PBJ-004).</summary>
        public const string CommonModuleCode = "CORPORATE_PROCUREMENT_MANAGEMENT_COMMON";
        public const string CommonModuleName = "Corporate Procurement Management Common";

        /// <summary>Kode modul layar Akses Role untuk permintaan pengadaan (BE-PBJ-006 dst.).</summary>
        public const string RequestModuleCode = "CORPORATE_PROCUREMENT_MANAGEMENT_REQUEST";
        public const string RequestModuleName = "Corporate Procurement Management Request";

        /// <summary>Kode modul layar Akses Role untuk perencanaan: rencana kebutuhan, pagu, rencana paket (BE-PBJ-009 dst.).</summary>
        public const string PlanningModuleCode = "CORPORATE_PROCUREMENT_MANAGEMENT_PLANNING";
        public const string PlanningModuleName = "Corporate Procurement Management Planning";

        /// <summary>Kode modul layar Akses Role untuk manajemen vendor (BE-PBJ-VD-001, PBJ-VD-PERM 1.0).</summary>
        public const string VendorModuleCode = "CORPORATE_PROCUREMENT_MANAGEMENT_VENDOR";
        public const string VendorModuleName = "Corporate Procurement Management Vendor";

        public const string ReadAction = "Read";
        public const string CreateAction = "Create";
        public const string UpdateAction = "Update";
        public const string DeleteAction = "Delete";
        public const string UploadAction = "Upload";

        // Aksi perpindahan status dokumen (AccessType Update/Create), PBJ-PP-PERM bagian 2.
        public const string SubmitAction = "Submit";
        public const string CancelAction = "Cancel";
        public const string ReviseAction = "Revise";
        public const string VerifyAction = "Verify";
        public const string CancelApprovedAction = "CancelApproved";
        /// <summary>Membatasi dan mencabut Dibatasi vendor (PBJ-DEC-047).</summary>
        public const string RestrictAction = "Restrict";
        /// <summary>Memfinalkan penilaian vendor (PBJ-DEC-049).</summary>
        public const string FinalizeAction = "Finalize";

        public const string ReadAllUnitsAction = "ReadAllUnits";

        public const string PurchaseRequestResource = "ProcurementRequest";
        public const string PurchaseRequestVerificationResource = "ProcurementRequestVerification";
        public const string NeedPlanResource = "ProcurementNeedPlan";
        public const string PackagePlanResource = "ProcurementPackagePlan";
        public const string BudgetResource = "ProcurementBudget";
        // PBJ-DEC-037: satu controller = satu resource; ProcurementMasterData dihapus.
        public const string ItemResource = "ProcurementItem";
        public const string ItemCategoryResource = "ProcurementItemCategory";
        public const string FundSourceResource = "ProcurementFundSource";
        public const string ProcurementTypeResource = "ProcurementType";
        public const string AttachmentResource = "ProcurementAttachment";

        // Manajemen vendor (BE-PBJ-VD-001, PBJ-VD-PERM 1.0 bagian 2).
        public const string VendorResource = "ProcurementVendor";
        public const string VendorDocumentResource = "ProcurementVendorDocument";
        public const string VendorStatusRequestResource = "ProcurementVendorStatusRequest";
        public const string VendorEvaluationResource = "ProcurementVendorEvaluation";
        public const string VendorDocumentTypeResource = "ProcurementVendorDocumentType";
        public const string CategoryDocumentRequirementResource = "ProcurementCategoryDocumentRequirement";
        public const string EvaluationCriterionResource = "ProcurementEvaluationCriterion";
    }
}
