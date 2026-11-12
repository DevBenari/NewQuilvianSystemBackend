namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Kode aksi pada riwayat dokumen Pengadaan — <c>permission-audit-matrix.md</c> bagian 5
    /// ("kejadian yang wajib meninggalkan jejak").
    /// </summary>
    public static class ProcurementDocumentHistoryActions
    {
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Submit = "SUBMIT";
        public const string StockCheck = "STOCK_CHECK";
        public const string VerifyComplete = "VERIFY_COMPLETE";
        public const string Return = "RETURN";
        public const string Reject = "REJECT";
        public const string Cancel = "CANCEL";
        public const string Withdraw = "WITHDRAW";
        public const string Revise = "REVISE";
        public const string WorkflowApproved = "WF_APPROVED";
        public const string WorkflowRejected = "WF_REJECTED";
        public const string WorkflowReturned = "WF_RETURNED";
        public const string Attach = "ATTACH";
        public const string Detach = "DETACH";
        public const string Activate = "ACTIVATE";
        public const string Deactivate = "DEACTIVATE";
        public const string ConvertToPackage = "CONVERT_TO_PACKAGE";
    }
}
