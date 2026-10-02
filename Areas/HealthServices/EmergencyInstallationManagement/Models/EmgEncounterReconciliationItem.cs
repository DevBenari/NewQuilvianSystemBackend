using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgEncounterReconciliationItem", Schema = "public")]
    public class EmgEncounterReconciliationItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RunId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid EmergencyVisitId { get; set; }

        public EmergencyReconciliationClass Class { get; set; }

        public EncounterStatus StatusBefore { get; set; }

        public EncounterStatus StatusAfter { get; set; }

        public DateTime? CompletedAtBefore { get; set; }

        public DateTime? CompletedAtAfter { get; set; }

        public bool IsReversed { get; set; }

        [MaxLength(200)]
        public string? ReverseSkipReason { get; set; }

        public EmgEncounterReconciliationRun? Run { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public EmgVisit? EmergencyVisit { get; set; }
    }
}
