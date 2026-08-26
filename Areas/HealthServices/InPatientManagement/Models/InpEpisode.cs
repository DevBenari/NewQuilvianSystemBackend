using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models
{
    [Table("InpEpisode", Schema = "public")]
    public class InpEpisode : IdentityModel
    {

        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string EpisodeNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        [Required]
        public Guid PatientClassId { get; set; }

        public InpEpisodeStatus EpisodeStatus { get; set; } = InpEpisodeStatus.Draft;

        public DateTime? AdmittedAt { get; set; }

        public DateTime? DischargeDecidedAt { get; set; }

        public DateTime? PhysicallyLeftAt { get; set; }

        public Guid? PhysicallyLeftByUserId { get; set; }

        public Guid? MotherEpisodeId { get; set; }

        public bool RequiresIsolation { get; set; } = false;

        public InpIsolationSource? IsolationSource { get; set; }

        public Guid? IsolationSetByUserId { get; set; }

        public Guid? IsolationSetByDoctorId { get; set; }

        public DateTime? IsolationSetAt { get; set; }

        [MaxLength(500)]
        public string? IsolationNote { get; set; }

        public DateTime? ClosedAt { get; set; }

        public InpDischargeType DischargeType { get; set; } = InpDischargeType.Unknown;

        public bool IsClosedWithoutFinancialClearance { get; set; } = false;

        [MaxLength(500)]
        public string? ClosedWithoutClearanceReason { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstPatientClass? PatientClass { get; set; }

        public ApplicationUser? PhysicallyLeftByUser { get; set; }

        public InpEpisode? MotherEpisode { get; set; }

        public ApplicationUser? IsolationSetByUser { get; set; }

        public MstDoctor? IsolationSetByDoctor { get; set; }

        public ICollection<InpDoctorAssignment> DoctorAssignments { get; set; } = new List<InpDoctorAssignment>();

        public ICollection<InpNurseAssignment> NurseAssignments { get; set; } = new List<InpNurseAssignment>();

        public ICollection<InpBedReservation> BedReservations { get; set; } = new List<InpBedReservation>();

        public ICollection<InpBedPlacement> BedPlacements { get; set; } = new List<InpBedPlacement>();

        public ICollection<InpClearanceMark> ClearanceMarks { get; set; } = new List<InpClearanceMark>();

        public ICollection<InpFinancialClearance> FinancialClearances { get; set; } = new List<InpFinancialClearance>();

        public ICollection<InpStatusHistory> StatusHistories { get; set; } = new List<InpStatusHistory>();

        public ICollection<InpCorrectionSession> CorrectionSessions { get; set; } = new List<InpCorrectionSession>();
    }
}
