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
        public const string Supersede = "SUPERSEDE";
        public const string WorkflowApproved = "WF_APPROVED";
        public const string WorkflowRejected = "WF_REJECTED";
        public const string WorkflowReturned = "WF_RETURNED";
        public const string Attach = "ATTACH";
        public const string Detach = "DETACH";
        public const string Activate = "ACTIVATE";
        public const string Deactivate = "DEACTIVATE";
        public const string ConvertToPackage = "CONVERT_TO_PACKAGE";

        // Manajemen vendor (BE-PBJ-VD-003; PBJ-VD-PERM 1.0 bagian 5): kategori vendor ditambah atau dilepas.
        public const string CategoryAdd = "CATEGORY_ADD";
        public const string CategoryRemove = "CATEGORY_REMOVE";

        // Dokumen legalitas vendor (BE-PBJ-VD-004): diverifikasi petugas selain pengunggah (PBJ-DEC-046).
        public const string Verify = "VERIFY";

        // Jejak dokumen legalitas pada riwayat vendor (PBJ_VENDOR), di samping riwayat dokumennya sendiri.
        public const string DocumentAdd = "DOCUMENT_ADD";
        public const string DocumentVerify = "DOCUMENT_VERIFY";
        public const string DocumentReject = "DOCUMENT_REJECT";
        public const string DocumentCancel = "DOCUMENT_CANCEL";

        // Status Dibatasi vendor (BE-PBJ-VD-006; PBJ-DEC-047).
        public const string Restrict = "RESTRICT";
        public const string RestrictionLift = "RESTRICTION_LIFT";

        // Penilaian vendor (BE-PBJ-VD-008; PBJ-DEC-049): finalisasi oleh orang lain, dan jejaknya di riwayat vendor.
        public const string Finalize = "FINALIZE";
        public const string EvaluationFinal = "EVALUATION_FINAL";
        public const string EvaluationSupersede = "EVALUATION_SUPERSEDE";

        // Usulan blokir/buka blokir vendor pada riwayat vendor (BE-PBJ-VD-007; PBJ-VD-PERM 1.0 bagian 5).
        public const string StatusRequestSubmit = "STATUS_REQUEST_SUBMIT";
        public const string StatusRequestReject = "STATUS_REQUEST_REJECT";
        public const string VendorBlock = "VENDOR_BLOCK";
        public const string VendorUnblock = "VENDOR_UNBLOCK";

        // Pemilihan penyedia (BE-PBJ-PS-001; PBJ-PS-PERM 1.0 bagian 5). Paket (PBJ_PACKAGE): baris PR masuk/keluar,
        // buka ulang persiapan, pecah, catat nomor PO. RFQ (PBJ_RFQ): kirim, perpanjang batas waktu, tutup, gagal, catatan
        // penawaran, negosiasi. Penetapan (PBJ_PACKAGE_AWARD): tidak dapat diterapkan dan cabut.
        public const string LineAdd = "LINE_ADD";
        public const string LineRemove = "LINE_REMOVE";
        public const string Reopen = "REOPEN";
        public const string Split = "SPLIT";
        public const string RecordOrder = "RECORD_ORDER";
        public const string Send = "SEND";
        public const string DeadlineExtend = "DEADLINE_EXTEND";
        public const string Close = "CLOSE";
        public const string Fail = "FAIL";
        public const string QuotationRecord = "QUOTATION_RECORD";
        public const string QuotationUpdate = "QUOTATION_UPDATE";
        public const string QuotationVoid = "QUOTATION_VOID";
        public const string Negotiate = "NEGOTIATE";
        public const string NotApplied = "NOT_APPLIED";
        public const string Revoke = "REVOKE";
    }
}
