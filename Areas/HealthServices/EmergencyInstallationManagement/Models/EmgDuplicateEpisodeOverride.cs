using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Models
{
    [Table("EmgDuplicateEpisodeOverride", Schema = "public")]
    public class EmgDuplicateEpisodeOverride : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        public Guid? OverriddenEncounterId { get; set; }

        public Guid? OverriddenVisitId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        public Guid OverriddenByUserId { get; set; }

        public DateTime OverriddenAt { get; set; } = DateTime.UtcNow;

        public RegPatientEncounter? Encounter { get; set; }

        public MstPatient? Patient { get; set; }

        public RegPatientEncounter? OverriddenEncounter { get; set; }

        public EmgVisit? OverriddenVisit { get; set; }

        public ApplicationUser? OverriddenByUser { get; set; }
    }
}
