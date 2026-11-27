using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Vendor.Models
{
    /// <summary>
    /// Usulan blokir atau buka blokir vendor lewat approval <c>Wfl</c> (BE-PBJ-VD-001; PBJ-DEC-021, PBJ-ASM-003).
    /// </summary>
    /// <remarks>
    /// Hanya usulan yang disetujui yang menulis <c>MstSupplier.IsBlacklisted</c> (PBJ-DEC-060). Maksimal satu usulan
    /// terbuka (<c>DRAFT</c>, <c>PENDING_APPROVAL</c>, <c>RETURNED</c>) per vendor, dijaga index unik parsial.
    /// Contoh: UBV-000003 "blokir PT Alkes Maju — 3 NCR kritis" dengan bukti penilaian Semester I 2027.
    /// </remarks>
    [Table("PrcVendorStatusRequest", Schema = "public")]
    public class PrcVendorStatusRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary><c>UBV-…</c>, dialokasikan saat diajukan.</summary>
        [MaxLength(30)]
        public string? RequestNumber { get; set; }

        public Guid VendorProfileId { get; set; }

        /// <summary><c>BLOCK</c> atau <c>UNBLOCK</c>.</summary>
        [Required]
        [MaxLength(10)]
        public string RequestType { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>Penilaian yang ditautkan sebagai bukti (PBJ-DEC-048).</summary>
        public Guid? EvidenceEvaluationId { get; set; }

        /// <summary><c>DRAFT</c>, <c>PENDING_APPROVAL</c>, <c>RETURNED</c>, <c>APPROVED</c>, <c>REJECTED</c>, <c>CANCELLED</c>.</summary>
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "DRAFT";

        /// <summary>Instance <c>Wfl</c>; tanpa FK fisik, pola PR.</summary>
        public Guid? WorkflowInstanceId { get; set; }

        /// <summary>Bagian kunci idempotensi <c>Wfl</c>: <c>PBJ_VENDOR_STATUS:{Id}:{SubmissionCount}</c>.</summary>
        public int SubmissionCount { get; set; }

        public DateTimeOffset? SubmittedAt { get; set; }

        public DateTimeOffset? DecidedAt { get; set; }

        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public PrcVendorProfile? VendorProfile { get; set; }

        public PrcVendorEvaluation? EvidenceEvaluation { get; set; }
    }
}
