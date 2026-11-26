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

            /// <summary>Label tampilan berbahasa Indonesia (matriks status bagian 2–4).</summary>
            public static readonly IReadOnlyDictionary<string, string> Labels = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                [Draft] = "Draf",
                [PendingApproval] = "Menunggu Persetujuan",
                [Returned] = "Dikembalikan",
                [Approved] = "Disetujui",
                [Superseded] = "Digantikan revisi",
                [Rejected] = "Ditolak",
                [Cancelled] = "Dibatalkan"
            };

            public static string LabelOf(string? status) =>
                status != null && Labels.TryGetValue(status, out var label) ? label : status ?? string.Empty;

            /// <summary>Isi dokumen hanya boleh diubah pada status ini.</summary>
            public static bool IsEditable(string? status) =>
                string.Equals(status, Draft, StringComparison.Ordinal) ||
                string.Equals(status, Returned, StringComparison.Ordinal);

            /// <summary>
            /// Status yang masih "hidup" untuk aturan satu dokumen per kunci (PBJ-VAL-040, 060).
            /// Contoh: rencana IGD 2027 yang <c>REJECTED</c> tidak menghalangi rencana baru, tetapi yang
            /// <c>DRAFT</c> menghalanginya.
            /// </summary>
            public static bool IsAlive(string? status) =>
                !string.Equals(status, Rejected, StringComparison.Ordinal) &&
                !string.Equals(status, Cancelled, StringComparison.Ordinal) &&
                !string.Equals(status, Superseded, StringComparison.Ordinal);

            /// <summary>Revisi yang belum final (PBJ-VAL-041).</summary>
            public static readonly IReadOnlySet<string> Unfinished = new HashSet<string>(StringComparer.Ordinal)
            {
                Draft,
                PendingApproval,
                Returned
            };
        }

        /// <summary>
        /// Status verifikasi dokumen legalitas vendor — <c>manajemen-vendor/contracts/state-transition-matrix.md</c>
        /// bagian 2 (PBJ-VD-STATE 1.0). Dibaca daftar vendor sejak BE-PBJ-VD-003; ditulis BE-PBJ-VD-004.
        /// </summary>
        public static class VendorDocument
        {
            public const string Pending = "PENDING";
            public const string Verified = "VERIFIED";
            public const string Rejected = "REJECTED";
            public const string Cancelled = "CANCELLED";
        }

        /// <summary>
        /// Status penilaian vendor — <c>manajemen-vendor/contracts/state-transition-matrix.md</c> bagian 4
        /// (PBJ-VD-STATE 1.0). Dibaca detail vendor sejak BE-PBJ-VD-003; ditulis BE-PBJ-VD-008.
        /// </summary>
        public static class VendorEvaluation
        {
            public const string Draft = "DRAFT";
            public const string Final = "FINAL";
            public const string Superseded = "SUPERSEDED";
            public const string Cancelled = "CANCELLED";
        }

        /// <summary>
        /// Status usulan blokir/buka blokir vendor — <c>manajemen-vendor/contracts/state-transition-matrix.md</c> bagian 3
        /// (PBJ-VD-STATE 1.0; BE-PBJ-VD-007). Usulan terbuka = <c>DRAFT</c>, <c>PENDING_APPROVAL</c>, <c>RETURNED</c>.
        /// </summary>
        public static class VendorStatusRequest
        {
            public const string Draft = "DRAFT";
            public const string PendingApproval = "PENDING_APPROVAL";
            public const string Returned = "RETURNED";
            public const string Approved = "APPROVED";
            public const string Rejected = "REJECTED";
            public const string Cancelled = "CANCELLED";

            public static readonly IReadOnlySet<string> Open = new HashSet<string>(StringComparer.Ordinal)
            {
                Draft,
                PendingApproval,
                Returned
            };
        }
    }
}
