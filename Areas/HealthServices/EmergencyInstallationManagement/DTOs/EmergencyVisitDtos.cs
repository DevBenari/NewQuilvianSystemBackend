using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyVisitResponse
    {
        public Guid Id { get; set; }
        public string EmergencyVisitNumber { get; set; } = string.Empty;
        public Guid? EncounterId { get; set; }
        public Guid? PatientId { get; set; }
        public Guid ServiceUnitId { get; set; }
        public Guid? ArrivalModeId { get; set; }
        public Guid? CaseTypeId { get; set; }
        public DateTime ArrivalDateTime { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? ArrivalLocation { get; set; }
        public string? FoundLocation { get; set; }
        public string? TraumaLocation { get; set; }
        public DateTime? TraumaDateTime { get; set; }
        public bool IsUnknownPatient { get; set; }
        public string? TemporaryPatientAlias { get; set; }
        public bool IsImmediateCareAllowed { get; set; }
        public EmergencyRegistrationStatus RegistrationStatus { get; set; }
        public EmergencyVisitStatus VisitStatus { get; set; }
        public DateTime? RegistrationCompletedAt { get; set; }
        public Guid? RegistrationCompletedByUserId { get; set; }
        public DateTime? TreatmentStartedAt { get; set; }
        public DateTime? VisitCompletedAt { get; set; }
        public string? Notes { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class CreateEmergencyVisitRequest
    {
        [MaxLength(50)]
        public string? EmergencyVisitNumber { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ArrivalModeId { get; set; }

        public Guid? CaseTypeId { get; set; }

        public DateTime ArrivalDateTime { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(250)]
        public string? ArrivalLocation { get; set; }

        [MaxLength(250)]
        public string? FoundLocation { get; set; }

        [MaxLength(250)]
        public string? TraumaLocation { get; set; }

        public DateTime? TraumaDateTime { get; set; }

        public bool IsUnknownPatient { get; set; }

        [MaxLength(100)]
        public string? TemporaryPatientAlias { get; set; }

        public bool IsImmediateCareAllowed { get; set; }

        public EmergencyRegistrationStatus RegistrationStatus { get; set; } = EmergencyRegistrationStatus.Pending;

        public EmergencyVisitStatus VisitStatus { get; set; } = EmergencyVisitStatus.Arrived;

        public DateTime? RegistrationCompletedAt { get; set; }

        public Guid? RegistrationCompletedByUserId { get; set; }

        public DateTime? TreatmentStartedAt { get; set; }

        public DateTime? VisitCompletedAt { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public bool IsActive { get; set; } = true;

    }

    public class UpdateEmergencyVisitRequest : CreateEmergencyVisitRequest
    {
    }

    public class UpdateEmergencyVisitRegistrationStatusRequest
    {
        [Required]
        public EmergencyRegistrationStatus RegistrationStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public class UpdateEmergencyVisitVisitStatusRequest
    {
        [Required]
        public EmergencyVisitStatus VisitStatus { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
