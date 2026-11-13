namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants
{
    /// <summary>
    /// Kosakata status dokumen Pengadaan, persis seperti
    /// <c>perencanaan-permintaan/contracts/state-transition-matrix.md</c> (PBJ-PP-STATE 1.0).
    /// Disimpan sebagai teks huruf besar, mengikuti pola status Finance Purchasing.
    /// </summary>
    public static class ProcurementStatuses
    {
        /// <summary>Status permintaan pengadaan (PR) — matriks status bagian 1.</summary>
        public static class PurchaseRequest
        {
            public const string Draft = "DRAFT";
            public const string Submitted = "SUBMITTED";
            public const string Returned = "RETURNED";
            public const string PendingApproval = "PENDING_APPROVAL";
            public const string Approved = "APPROVED";
            public const string FulfilledFromStock = "FULFILLED_FROM_STOCK";
            public const string Rejected = "REJECTED";
            public const string Cancelled = "CANCELLED";
            public const string ConvertedToPackage = "CONVERTED_TO_PACKAGE";

            /// <summary>
            /// Status yang memakan sisa kuantitas rencana (PBJ-DEC-029). Contoh: PR 6 unit yang
            /// <c>APPROVED</c> mengurangi sisa rencana; PR yang <c>CANCELLED</c> mengembalikannya.
            /// </summary>
            public static readonly IReadOnlySet<string> ConsumesPlanQuota = new HashSet<string>(StringComparer.Ordinal)
            {
                Submitted,
                PendingApproval,
                Approved,
                FulfilledFromStock,
                ConvertedToPackage
            };

            /// <summary>Status final; dokumen tidak dihidupkan kembali.</summary>
            public static readonly IReadOnlySet<string> Final = new HashSet<string>(StringComparer.Ordinal)
            {
                Rejected,
                Cancelled,
                FulfilledFromStock,
                ConvertedToPackage
            };
        }

        /// <summary>Status rencana kebutuhan, rencana paket, dan pagu — matriks status bagian 2–4.</summary>
        public static class Plan
        {
            public const string Draft = "DRAFT";
            public const string PendingApproval = "PENDING_APPROVAL";
            public const string Returned = "RETURNED";
            public const string Approved = "APPROVED";
            public const string Superseded = "SUPERSEDED";
            public const string Rejected = "REJECTED";
            public const string Cancelled = "CANCELLED";
        }
    }
}
