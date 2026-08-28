using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgObservation", Schema = "public")]
    public class EmgObservation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ObservationNumber { get; set; } = string.Empty;

        public EmergencyObservationStatus ObservationStatus { get; set; }
            = EmergencyObservationStatus.Active;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? EndedAt { get; set; }

        [MaxLength(250)]
        public string? ObservationLocation { get; set; }

        [MaxLength(1000)]
        public string? Indication { get; set; }

        [MaxLength(2000)]
        public string? ObservationPlan { get; set; }

        public Guid? ResponsibleDoctorId { get; set; }

        public Guid? ResponsibleNurseUserId { get; set; }

        [MaxLength(1000)]
        public string? CompletionSummary { get; set; }

        [MaxLength(1000)]
        public string? EscalationReason { get; set; }

        public bool IsActive { get; set; } = true;

        public EmgVisit? EmergencyVisit { get; set; }

        public MstDoctor? ResponsibleDoctor { get; set; }

        public ApplicationUser? ResponsibleNurseUser { get; set; }

        public ICollection<EmgObservationDetail> Details { get; set; }
            = new List<EmgObservationDetail>();

        public ICollection<EmgProcedureDetail> ProcedureDetails { get; set; }
            = new List<EmgProcedureDetail>();
    }
}
