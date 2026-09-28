using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Penilaian Pra-HD atau Pasca-HD. Tanda vital klinisnya tinggal di <c>TrxPatientVitalSign</c>
    /// milik Clinical Management dan dirujuk lewat <see cref="PatientVitalSignId"/>.
    /// </summary>
    /// <remarks>
    /// Nilai pasca-HD tidak pernah disalin otomatis dari pra-HD: berat badan setelah tindakan
    /// adalah angka yang menentukan apakah target penarikan cairan tercapai.
    /// </remarks>
    [Table("HmdSessionAssessment", Schema = "public")]
    public class HmdSessionAssessment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid SessionId { get; set; }

        public HmdSession? Session { get; set; }

        public HmdAssessmentPhase Phase { get; set; }

        public DateTime AssessedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public Guid AssessedByUserId { get; set; }

        public decimal? BodyWeightKg { get; set; }

        /// <summary>Rujukan ke <c>TrxPatientVitalSign</c>; tanpa FK karena milik modul lain.</summary>
        public Guid? PatientVitalSignId { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? Complaint { get; set; }

        /// <summary>Kondisi akses vaskular, termasuk hemostasis pada fase pasca. SENSITIF.</summary>
        [MaxLength(500)]
        public string? AccessConditionNote { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? PatientCondition { get; set; }

        /// <summary>Hanya pada fase pasca.</summary>
        public bool? TargetAchieved { get; set; }
    }
}
