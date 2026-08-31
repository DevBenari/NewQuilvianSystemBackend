using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgTriage", Schema = "public")]
    public class EmgTriage : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        public Guid TriageLevelId { get; set; }

        public Guid? PatientVitalSignId { get; set; }

        public int Sequence { get; set; } = 1;

        public bool IsRetriage { get; set; }

        public Guid? PreviousTriageId { get; set; }

        public EmergencyTriageSystem TriageSystem { get; set; }
            = EmergencyTriageSystem.ATS;

        public EmergencyTriageStatus TriageStatus { get; set; }
            = EmergencyTriageStatus.Draft;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Salinan target waktu master saat penilaian dibuat. Kosong berarti level triase
        /// yang dipakai memang belum punya target, sehingga tidak ada batas yang dilanggar.
        /// </summary>
        public int? MaxWaitingMinutesSnapshot { get; set; }

        public DateTime? ResponseDueAt { get; set; }

        public bool ImmediateCareAllowed { get; set; }

        [MaxLength(1000)]
        public string? TriageReason { get; set; }

        [MaxLength(1000)]
        public string? AirwaySummary { get; set; }

        [MaxLength(1000)]
        public string? BreathingSummary { get; set; }

        [MaxLength(1000)]
        public string? CirculationSummary { get; set; }

        [MaxLength(1000)]
        public string? DisabilitySummary { get; set; }

        [MaxLength(1000)]
        public string? ExposureSummary { get; set; }

        [MaxLength(1000)]
        public string? RedFlagSummary { get; set; }

        public Guid PerformedByUserId { get; set; }

        public Guid? ReviewedByUserId { get; set; }

        public DateTime? ReviewedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Menandai penilaian yang batas waktu responsnya sudah terlampaui. Diisi oleh
        /// proses pemantau, bukan oleh petugas, dan tidak pernah dihitung ulang untuk
        /// riwayat lama.
        /// </summary>
        public bool IsSlaBreached { get; set; } = false;

        /// <summary>
        /// Waktu pelampauan batas tercatat. Kosong selama batas belum terlampaui.
        /// Setelah terisi nilainya tetap, supaya pemindaian berulang tidak menggeser
        /// kapan keterlambatan sebenarnya terjadi.
        /// </summary>
        public DateTime? SlaBreachedAt { get; set; }

        public EmgVisit? EmergencyVisit { get; set; }

        public EmgTriageLevel? TriageLevel { get; set; }

        public TrxPatientVitalSign? PatientVitalSign { get; set; }

        public EmgTriage? PreviousTriage { get; set; }

        public ApplicationUser? PerformedByUser { get; set; }

        public ApplicationUser? ReviewedByUser { get; set; }

        public ICollection<EmgTriage> Retriages { get; set; }
            = new List<EmgTriage>();

        public ICollection<EmgTriageDetail> Details { get; set; }
            = new List<EmgTriageDetail>();
    }
}
