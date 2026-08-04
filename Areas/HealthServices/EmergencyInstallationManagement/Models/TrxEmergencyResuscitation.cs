using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("TrxEmergencyResuscitation", Schema = "public")]
    public class TrxEmergencyResuscitation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EmergencyVisitId { get; set; }

        [Required]
        [MaxLength(50)]
        public string ResuscitationNumber { get; set; } = string.Empty;

        public EmergencyResuscitationStatus ResuscitationStatus { get; set; }
            = EmergencyResuscitationStatus.Planned;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; set; }

        [MaxLength(250)]
        public string? Location { get; set; }

        [MaxLength(1000)]
        public string? TriggerCondition { get; set; }

        public Guid? TeamLeaderDoctorId { get; set; }

        public Guid? RecordedByUserId { get; set; }

        public bool WasCardiopulmonaryResuscitationPerformed { get; set; }

        public DateTime? CardiopulmonaryResuscitationStartedAt { get; set; }

        public DateTime? ReturnOfSpontaneousCirculationAt { get; set; }

        public int DefibrillationCount { get; set; }

        [MaxLength(1000)]
        public string? AirwayManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? BreathingManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? CirculationManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? NeurologicalManagementSummary { get; set; }

        [MaxLength(1000)]
        public string? OutcomeSummary { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxEmergencyVisit? EmergencyVisit { get; set; }

        public MstDoctor? TeamLeaderDoctor { get; set; }

        public ApplicationUser? RecordedByUser { get; set; }

        public ICollection<TrxEmergencyProcedureDetail> ProcedureDetails { get; set; }
            = new List<TrxEmergencyProcedureDetail>();
    }
}
