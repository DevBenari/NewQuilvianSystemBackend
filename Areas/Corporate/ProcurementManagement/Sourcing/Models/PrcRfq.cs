using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Constants;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Sourcing.Models
{
    /// <summary>
    /// Permintaan penawaran (BE-PBJ-PS-004; PBJ-DEC-075, 081): batas waktu mengikat, nomor <c>RFQ</c> saat dikirim.
    /// </summary>
    /// <remarks>
    /// Satu RFQ <c>DRAFT</c>/<c>SENT</c> per paket. RFQ gagal tetap tersimpan, dan RFQ ulang mendapat nomor baru.
    /// Contoh: RFQ-000007 untuk PKT-000012, batas 10-04-2027 16.00.
    /// </remarks>
    [Table("PrcRfq", Schema = "public")]
    public class PrcRfq : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid PackageId { get; set; }

        public PrcPackage? Package { get; set; }

        [MaxLength(20)]
        public string? RfqNumber { get; set; }

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = ProcurementStatuses.Rfq.Draft;

        public DateTimeOffset? SentAt { get; set; }

        public DateTimeOffset Deadline { get; set; }

        public int ExtensionCount { get; set; }

        public DateTimeOffset? ClosedAt { get; set; }

        /// <summary>Wajib saat <c>FAILED</c>.</summary>
        [MaxLength(500)]
        public string? FailureReason { get; set; }

        /// <summary>RFQ asal pada paket hasil pecah (PBJ-DEC-083).</summary>
        public Guid? InheritedFromRfqId { get; set; }

        public PrcRfq? InheritedFromRfq { get; set; }

        /// <summary>Token konkurensi; nilai lama ditolak 409 <c>PBJ-VAL-090</c>.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();

        public List<PrcRfqInvitation> Invitations { get; set; } = new();
    }
}
