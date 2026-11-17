namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Jenis dokumen modul Pengadaan (BE-PBJ-002). Nilai yang sama dipakai sebagai
    /// <c>DocumentType</c> pada riwayat dan lampiran, serta sebagai <c>ReferenceType</c> pada
    /// instance workflow bersama (Wfl), sehingga satu dokumen dikenali dengan nama yang sama di
    /// seluruh sistem. Contoh: permintaan pengadaan PR-000031 tercatat sebagai <c>PBJ_PR</c>.
    /// </summary>
    public static class ProcurementDocumentTypes
    {
        /// <summary>Awalan seluruh jenis dokumen Pengadaan; dipakai penangan workflow untuk mengklaim dokumen.</summary>
        public const string Prefix = "PBJ_";

        public const string PurchaseRequest = "PBJ_PR";
        public const string NeedPlan = "PBJ_NEED_PLAN";
        public const string PackagePlan = "PBJ_PACKAGE_PLAN";
        public const string Budget = "PBJ_BUDGET";
        public const string Item = "PBJ_ITEM";

        private static readonly HashSet<string> Known = new(StringComparer.Ordinal)
        {
            PurchaseRequest,
            NeedPlan,
            PackagePlan,
            Budget,
            Item
        };

        /// <summary>Jenis dokumen yang dikenal sub-modul ini. Jenis lain ditolak (fail-closed).</summary>
        public static bool IsKnown(string? documentType) =>
            !string.IsNullOrWhiteSpace(documentType) && Known.Contains(documentType.Trim());
    }
}
