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

            /// <summary>
            /// Status final; dokumen tidak dihidupkan kembali. <c>CONVERTED_TO_PACKAGE</c> <b>bukan</b> final sejak
            /// BE-PBJ-PS-005: PR kembali <c>APPROVED</c> bila barisnya dilepas dari paket yang batal (PBJ-DEC-077).
            /// </summary>
            public static readonly IReadOnlySet<string> Final = new HashSet<string>(StringComparer.Ordinal)
            {
                Rejected,
                Cancelled,
                FulfilledFromStock
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

        /// <summary>
        /// Status paket pengadaan — <c>pemilihan-penyedia/contracts/state-transition-matrix.md</c> bagian 1 (PBJ-PS-STATE 1.0;
        /// BE-PBJ-PS-001). Jalur normal: <c>DRAFT</c> → <c>PREPARATION_PENDING_APPROVAL</c> → <c>READY_FOR_RFQ</c> →
        /// <c>RFQ_OPEN</c> → <c>EVALUATION</c> → <c>AWARD_PENDING_APPROVAL</c> → <c>AWARDED</c> → <c>ORDERED</c>.
        /// </summary>
        public static class Package
        {
            public const string Draft = "DRAFT";
            public const string PreparationPendingApproval = "PREPARATION_PENDING_APPROVAL";
            public const string PreparationReturned = "PREPARATION_RETURNED";
            public const string ReadyForRfq = "READY_FOR_RFQ";
            public const string RfqOpen = "RFQ_OPEN";
            public const string Evaluation = "EVALUATION";
            public const string AwardPendingApproval = "AWARD_PENDING_APPROVAL";
            public const string Awarded = "AWARDED";
            public const string Ordered = "ORDERED";
            public const string Rejected = "REJECTED";
            public const string Cancelled = "CANCELLED";

            /// <summary>
            /// Persiapan paket (isi, baris PR, lampiran persiapan) hanya dapat diubah pada <c>DRAFT</c> dan
            /// <c>PREPARATION_RETURNED</c> (PBJ-PS-STATE 1.0 bagian 1; BE-PBJ-PS-005, 006).
            /// </summary>
            public static bool IsPreparationEditable(string? status) =>
                string.Equals(status, Draft, StringComparison.Ordinal) ||
                string.Equals(status, PreparationReturned, StringComparison.Ordinal);
        }

        /// <summary>Status RFQ — matriks status pemilihan-penyedia bagian 2 (PBJ-PS-STATE 1.0).</summary>
        public static class Rfq
        {
            public const string Draft = "DRAFT";
            public const string Sent = "SENT";
            public const string Closed = "CLOSED";
            public const string Failed = "FAILED";
            public const string Cancelled = "CANCELLED";
        }

        /// <summary>Status catatan penawaran (PBJ-PS-STATE 1.0). Catatan yang salah dibatalkan menjadi <c>VOID</c>.</summary>
        public static class Quotation
        {
            public const string Recorded = "RECORDED";
            public const string Void = "VOID";
        }

        /// <summary>
        /// Status penetapan penyedia — matriks status pemilihan-penyedia bagian 3 (PBJ-PS-STATE 1.0). <c>NOT_APPLIED</c> =
        /// disetujui tetapi vendor sudah tidak layak saat diterapkan (PBJ-DEC-092); <c>REVOKED</c> = dicabut sebelum
        /// nomor PO (PBJ-DEC-093).
        /// </summary>
        public static class Award
        {
            public const string PendingApproval = "PENDING_APPROVAL";
            public const string Returned = "RETURNED";
            public const string Approved = "APPROVED";
            public const string Rejected = "REJECTED";
            public const string Withdrawn = "WITHDRAWN";
            public const string Revoked = "REVOKED";
            public const string NotApplied = "NOT_APPLIED";
        }

        /// <summary>Hasil evaluasi teknis per baris penawaran × kriteria × evaluator (PBJ-DEC-074).</summary>
        public static class TechnicalEvaluationResult
        {
            public const string Pass = "PASS";
            public const string Fail = "FAIL";
        }

        /// <summary>
        /// Tahap paket tempat jenis dokumen paket diunggah (<c>MstProcurementPackageDocumentType.Stage</c>). Contoh: KAK/TOR
        /// pada <c>PREPARATION</c>, Surat Penawaran Harga pada <c>QUOTATION</c>.
        /// </summary>
        public static class PackageDocumentStage
        {
            public const string Preparation = "PREPARATION";
            public const string Quotation = "QUOTATION";
            public const string Negotiation = "NEGOTIATION";

            public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
            {
                Preparation,
                Quotation,
                Negotiation
            };
        }
    }
}
