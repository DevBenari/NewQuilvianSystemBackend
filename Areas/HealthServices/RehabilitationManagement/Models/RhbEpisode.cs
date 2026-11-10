using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Models
{
    /// <summary>
    /// Satu rangkaian layanan rehab sejak permintaan diterima sampai ditutup — aggregate root
    /// (<c>data-dictionary.md</c> bagian 2.3).
    /// </summary>
    /// <remarks>
    /// Dibuka dalam transaksi yang sama dengan aksi <c>accept</c> permintaan. Episode
    /// <c>Closed</c> tidak dibuka lagi; penutupan hanya lewat tanda tangan ringkasan akhir.
    /// </remarks>
    [Table("RhbEpisode", Schema = "public")]
    public class RhbEpisode : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor dari <c>NumberSeriesAllocator</c>, deret <c>RHB_EPISODE</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string EpisodeNumber { get; set; } = string.Empty;

        [Required]
        public Guid RequestId { get; set; }

        public RhbRequest? Request { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        /// <summary>Dokter Sp.KFR penanggung jawab episode (<c>MstDoctor</c>).</summary>
        [Required]
        public Guid ResponsibleDoctorId { get; set; }

        public RehabEpisodeStatus EpisodeStatus { get; set; } = RehabEpisodeStatus.AwaitingMedicalAssessment;

        public DateTime OpenedAt { get; set; }

        public RehabEpisodeClosureReason? ClosureReason { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public int Version { get; set; }

        public ICollection<RhbEpisodeDiscipline> Disciplines { get; set; } = new List<RhbEpisodeDiscipline>();

        public ICollection<RhbAssessment> Assessments { get; set; } = new List<RhbAssessment>();

        public ICollection<RhbGoal> Goals { get; set; } = new List<RhbGoal>();

        public ICollection<RhbProgram> Programs { get; set; } = new List<RhbProgram>();

        public ICollection<RhbEducationRecord> EducationRecords { get; set; } = new List<RhbEducationRecord>();

        public ICollection<RhbSession> Sessions { get; set; } = new List<RhbSession>();

        public ICollection<RhbProgressReview> ProgressReviews { get; set; } = new List<RhbProgressReview>();

        public RhbDischargeSummary? DischargeSummary { get; set; }
    }
}
