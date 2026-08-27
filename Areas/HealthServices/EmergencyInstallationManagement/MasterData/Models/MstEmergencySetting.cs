using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.Models
{
    [Table("MstEmergencySetting", Schema = "public")]
    public class MstEmergencySetting : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = "DEFAULT";

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = "Default Emergency Setting";

        [Required]
        public Guid DefaultEmergencyServiceUnitId { get; set; }

        public EmergencyTriageSystem TriageSystem { get; set; }
            = EmergencyTriageSystem.ATS;

        public bool AllowProvisionalRegistration { get; set; } = true;

        public bool AllowUnknownPatient { get; set; } = true;

        public bool AutoCreateProvisionalEncounter { get; set; } = true;

        public int ImmediateCareLevelThreshold { get; set; } = 2;

        public int RequireRegistrationBeforeTreatmentFromLevel { get; set; } = 3;

        public bool RequireTriageBeforeStandardRegistration { get; set; } = true;

        public bool RequireRegistrationCompletionBeforeDisposition { get; set; } = true;

        [Required]
        [MaxLength(20)]
        public string TemporaryPatientNumberPrefix { get; set; } = "TMP";

        [Required]
        [MaxLength(20)]
        public string EmergencyVisitNumberPrefix { get; set; } = "IGD";

        public bool IsDefault { get; set; } = true;

        public bool IsActive { get; set; } = true;

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public MstServiceUnit? DefaultEmergencyServiceUnit { get; set; }
    }
}
