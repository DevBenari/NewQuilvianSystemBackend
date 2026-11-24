using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    /// <summary>
    /// Surat Tugas multi-pegawai (BE-HRD-382, keputusan pemilik <c>ADM-Q-03</c> = A: satu surat bertabel peserta).
    /// Aggregate administratif: diajukan Kepala Unit, diverifikasi HRD, disahkan pejabat, diterbitkan HRD bernomor
    /// resmi, lalu dilaksanakan. Tidak mengubah jabatan definitif, gaji, kontrak, atau kewenangan klinis; saat terbit,
    /// tiap peserta mendapat satu <c>HrdOfficialDutyRequest</c> berstatus disetujui agar presensi mengakui dinas luar.
    /// </summary>
    [Table("HrdAssignmentLetter", Schema = "public")]
    public class HrdAssignmentLetter : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor resmi; kosong sampai diterbitkan.</summary>
        [MaxLength(100)]
        public string? LetterNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string ActivityName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string DutyCategory { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? BasisDescription { get; set; }

        [MaxLength(500)]
        public string? BasisDocumentReference { get; set; }

        [Required]
        [MaxLength(2000)]
        public string TaskDescription { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Destination { get; set; } = string.Empty;

        public DateTime StartAt { get; set; }

        public DateTime EndAt { get; set; }

        public Guid? RequestingOrganizationUnitId { get; set; }

        [Required]
        [MaxLength(30)]
        public string LetterStatus { get; set; } = "Draft";

        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }
        public DateTime? VerifiedAt { get; set; }

        [MaxLength(1000)]
        public string? VerificationNote { get; set; }

        [MaxLength(1000)]
        public string? RevisionNote { get; set; }

        public Guid? EndorsedByUserId { get; set; }
        public DateTime? EndorsedAt { get; set; }

        [MaxLength(1000)]
        public string? EndorsementNote { get; set; }

        [MaxLength(200)]
        public string? SignerName { get; set; }

        [MaxLength(200)]
        public string? SignerTitle { get; set; }

        public Guid? IssuedByUserId { get; set; }
        public DateTime? IssuedAt { get; set; }

        public Guid? RejectedByUserId { get; set; }
        public DateTime? RejectedAt { get; set; }

        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public Guid? CancelledByUserId { get; set; }
        public DateTime? CancelledAt { get; set; }

        [MaxLength(1000)]
        public string? CancellationReason { get; set; }

        public Guid? ExecutionStartedByUserId { get; set; }
        public DateTime? ExecutionStartedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }
        public DateTime? CompletedAt { get; set; }

        [MaxLength(2000)]
        public string? ResultReport { get; set; }

        [MaxLength(500)]
        public string? ResultEvidenceReference { get; set; }

        [MaxLength(128)]
        public string? VerificationHash { get; set; }

        /// <summary>Naskah HTML yang diterbitkan (kop, nomor, daftar peserta, penandatangan, QR); diunduh lewat endpoint berizin.</summary>
        public string? DocumentSnapshotHtml { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<HrdAssignmentLetterParticipant> Participants { get; set; } = new List<HrdAssignmentLetterParticipant>();
    }
}
