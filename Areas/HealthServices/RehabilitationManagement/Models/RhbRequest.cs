using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Satu permintaan layanan rehab untuk satu pasien — aggregate root
    /// (<c>data-dictionary.md</c> bagian 2.1, FEAT-001..003).
    /// </summary>
    /// <remarks>
    /// Permintaan bukan asesmen dan bukan bukti terapi. Penolakan klinis hanya lewat aksi
    /// disposisi (RHB-INV-02). Tidak pernah dihapus; diakhiri lewat <c>cancel</c>.
    /// </remarks>
    [Table("RhbRequest", Schema = "public")]
    public class RhbRequest : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor dari <c>NumberSeriesAllocator</c>, deret <c>RHB_REQUEST</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string RequestNumber { get; set; } = string.Empty;

        [Required]
        public Guid PatientId { get; set; }

        public RehabRequestSource SourceType { get; set; }

        /// <summary>Kunjungan asal. Wajib kecuali rujukan eksternal (<c>CK_RhbRequest_Source</c>).</summary>
        public Guid? SourceEncounterId { get; set; }

        public Guid? InpEpisodeId { get; set; }

        /// <summary>Surat rujukan internal (<c>CliDoctorCertificate</c>). Rujukan logis, bukan FK.</summary>
        public Guid? InternalReferralLetterId { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(200)]
        public string? ExternalInstitutionName { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(100)]
        public string? ExternalReferralNumber { get; set; }

        public Guid? RequestingDoctorId { get; set; }

        /// <summary>Nama perujuk luar. SENSITIF.</summary>
        [MaxLength(150)]
        public string? RequestingPractitionerName { get; set; }

        /// <summary>Indikasi atau alasan rujukan. SENSITIF — data klinis.</summary>
        [Required]
        [MaxLength(2000)]
        public string IndicationText { get; set; } = string.Empty;

        public DateTime RequestedAt { get; set; }

        public RehabRequestStatus RequestStatus { get; set; } = RehabRequestStatus.Draft;

        /// <summary>Alasan melanjutkan walau ada potensi duplikasi (RHB-DEC-010). SENSITIF.</summary>
        [MaxLength(500)]
        public string? DuplicateOverrideReason { get; set; }

        /// <summary>Alasan pengembalian administratif atau permintaan klarifikasi. SENSITIF.</summary>
        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        /// <summary>Alasan pengalihan atau penolakan klinis. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? DispositionReason { get; set; }

        [MaxLength(200)]
        public string? RedirectTargetText { get; set; }

        public Guid? DispositionByUserId { get; set; }

        public DateTime? DispositionAt { get; set; }

        public Guid? SubmittedByUserId { get; set; }

        public DateTime? SubmittedAt { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        public int Version { get; set; }

        public ICollection<RhbRequestDiscipline> Disciplines { get; set; } = new List<RhbRequestDiscipline>();

        public RhbEpisode? Episode { get; set; }
    }
}
