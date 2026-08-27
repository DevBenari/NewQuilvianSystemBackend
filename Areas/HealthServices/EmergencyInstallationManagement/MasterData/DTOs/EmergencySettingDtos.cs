using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.MasterData.DTOs
{
    public class EmergencySettingResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public Guid DefaultEmergencyServiceUnitId { get; set; }
        public EmergencyTriageSystem TriageSystem { get; set; }
        public bool AllowProvisionalRegistration { get; set; }
        public bool AllowUnknownPatient { get; set; }
        public bool AutoCreateProvisionalEncounter { get; set; }
        public int ImmediateCareLevelThreshold { get; set; }
        public int RequireRegistrationBeforeTreatmentFromLevel { get; set; }
        public bool RequireTriageBeforeStandardRegistration { get; set; }
        public bool RequireRegistrationCompletionBeforeDisposition { get; set; }
        public string TemporaryPatientNumberPrefix { get; set; } = string.Empty;
        public string EmergencyVisitNumberPrefix { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencySettingRequest
    {
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = "DEFAULT";

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = "Default Emergency Setting";

        [Required]
        public Guid DefaultEmergencyServiceUnitId { get; set; }

        public EmergencyTriageSystem TriageSystem { get; set; } = EmergencyTriageSystem.ATS;

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

    }

    public class UpdateEmergencySettingRequest : CreateEmergencySettingRequest
    {
    }

    public class EmergencySettingOptionResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public EmergencyTriageSystem TriageSystem { get; set; }
    }
}
