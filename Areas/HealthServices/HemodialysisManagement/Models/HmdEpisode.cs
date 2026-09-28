using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Satu program cuci darah seorang pasien — aggregate root. Bukan <c>InpEpisode</c>.
    /// </summary>
    /// <remarks>
    /// Satu pasien hanya boleh punya satu episode <c>Active</c>. Aturan itu ditegakkan service
    /// dan dijaga unique index bersyarat <c>IX_HmdEpisode_PatientId_Active</c> di database.
    /// </remarks>
    [Table("HmdEpisode", Schema = "public")]
    public class HmdEpisode : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor episode dari <c>NumberSeriesAllocator</c>, deret <c>HMD_EPISODE</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string EpisodeNumber { get; set; } = string.Empty;

        [Required]
        public Guid PatientId { get; set; }

        public MstPatient? Patient { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        /// <summary>Dokter penanggung jawab program.</summary>
        [Required]
        public Guid DpjpDoctorId { get; set; }

        public MstDoctor? DpjpDoctor { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public HmdEpisodeStatus EpisodeStatus { get; set; } = HmdEpisodeStatus.Draft;

        [MaxLength(500)]
        public string? SuspendReason { get; set; }

        public HmdEpisodeClosureReason? ClosureReason { get; set; }

        /// <summary>Keterangan penutupan. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ClosureNote { get; set; }

        public DateTime? ActivatedAt { get; set; }

        public Guid? ActivatedByUserId { get; set; }

        public DateTime? ClosedAt { get; set; }

        public Guid? ClosedByUserId { get; set; }

        public int Version { get; set; }

        public ICollection<HmdEligibilityAssessment> EligibilityAssessments { get; set; } = new List<HmdEligibilityAssessment>();

        public ICollection<HmdVascularAccess> VascularAccesses { get; set; } = new List<HmdVascularAccess>();

        public ICollection<HmdSerologyReview> SerologyReviews { get; set; } = new List<HmdSerologyReview>();

        public ICollection<HmdIsolationDecision> IsolationDecisions { get; set; } = new List<HmdIsolationDecision>();

        public ICollection<HmdPrescription> Prescriptions { get; set; } = new List<HmdPrescription>();

        public ICollection<HmdSession> Sessions { get; set; } = new List<HmdSession>();
    }
}
