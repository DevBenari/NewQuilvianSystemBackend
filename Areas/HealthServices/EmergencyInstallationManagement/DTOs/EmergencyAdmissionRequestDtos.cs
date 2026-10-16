using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class CreateEmergencyAdmissionRequestRequest
    {
        public Guid DispositionId { get; set; }
    }

    public class UpdateEmergencyAdmissionRequestRequest
    {
        public Guid? RequestedServiceUnitId { get; set; }
        public Guid? RequestedPatientClassId { get; set; }
        public Guid? ProposedInpatientDoctorId { get; set; }
        public int EstimatedLengthOfStayDays { get; set; } = 3;
        public bool IsCito { get; set; }
        public bool NeedsIntensiveCare { get; set; }
        public bool NeedsIsolation { get; set; }
        public bool NeedsOxygenSupport { get; set; }
        public bool NeedsSpecialMonitoring { get; set; }
        public string? ClinicalIndication { get; set; }
        public string? CarePlanInstruction { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class EmergencyAdmissionRequestRowVersionRequest
    {
        public Guid RowVersion { get; set; }
    }

    public class ReturnEmergencyAdmissionRequestRequest
    {
        public string? Note { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class CancelEmergencyAdmissionRequestRequest
    {
        public string? Reason { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class EmergencyAdmissionRequestReference
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
    }

    public class EmergencyAdmissionRequestPatientInfo
    {
        public Guid PatientId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string? MedicalRecordNumber { get; set; }
        public string? Gender { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? AgeText { get; set; }
    }

    public class EmergencyAdmissionRequestAllergyItem
    {
        public string AllergenName { get; set; } = string.Empty;
        public PatientAllergyCategory Category { get; set; }
        public PatientAllergySeverity Severity { get; set; }
        public string? Reaction { get; set; }
    }

    public class EmergencyAdmissionRequestAllergyInfo
    {
        public EmergencyAdmissionAllergyState State { get; set; } = EmergencyAdmissionAllergyState.NotRecorded;
        public List<EmergencyAdmissionRequestAllergyItem> Items { get; set; } = [];
    }

    public class EmergencyAdmissionRequestPayerInfo
    {
        public string? PaymentTypeName { get; set; }
        public string? PayerName { get; set; }
        public string? ClassEntitlementText { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class EmergencyAdmissionRequestDiagnosisItem
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public PatientDiagnosisType Type { get; set; }
        public bool IsPrimary { get; set; }
    }

    public class EmergencyAdmissionRequestDoctorInfo
    {
        public Guid DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public bool HasAccount { get; set; }
    }

    public class EmergencyAdmissionRequestSignerInfo
    {
        public string? DoctorName { get; set; }
        public string? PositionName { get; set; }
        public DateTime? SignedAt { get; set; }
    }

    public class EmergencyAdmissionRequestEventItem
    {
        public EmergencyAdmissionRequestEventType EventType { get; set; }
        public EmergencyAdmissionRequestStatus? FromStatus { get; set; }
        public EmergencyAdmissionRequestStatus ToStatus { get; set; }
        public Guid ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public DateTime OccurredAt { get; set; }
        public string? Note { get; set; }
    }

    public class EmergencyAdmissionRequestSummary
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string? EmergencyVisitNumber { get; set; }
        public Guid DispositionId { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public EmergencyAdmissionRequestStatus Status { get; set; }
        public EmergencyAdmissionRequestDisplayStatus DisplayStatus { get; set; }
        public string? RequestedServiceUnitName { get; set; }
        public string? RequestedPatientClassName { get; set; }
        public bool IsCito { get; set; }
        public DateTime? SignatureRequestedAt { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime CreateDateTime { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class EmergencyAdmissionRequestResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string? EmergencyVisitNumber { get; set; }
        public Guid DispositionId { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public EmergencyAdmissionRequestStatus Status { get; set; }
        public EmergencyAdmissionRequestDisplayStatus DisplayStatus { get; set; }
        public string Origin { get; set; } = "IGD";
        public EmergencyAdmissionRequestPatientInfo Patient { get; set; } = new();
        public EmergencyAdmissionRequestAllergyInfo Allergies { get; set; } = new();
        public EmergencyAdmissionRequestPayerInfo Payer { get; set; } = new();
        public List<EmergencyAdmissionRequestDiagnosisItem> Diagnoses { get; set; } = [];
        public EmergencyAdmissionRequestReference? RequestedServiceUnit { get; set; }
        public EmergencyAdmissionRequestReference? RequestedPatientClass { get; set; }
        public EmergencyAdmissionRequestReference? ProposedInpatientDoctor { get; set; }
        public int EstimatedLengthOfStayDays { get; set; }
        public bool IsCito { get; set; }
        public bool NeedsIntensiveCare { get; set; }
        public bool NeedsIsolation { get; set; }
        public bool NeedsOxygenSupport { get; set; }
        public bool NeedsSpecialMonitoring { get; set; }
        public List<string> SpecialNeeds { get; set; } = [];
        public string? ClinicalIndication { get; set; }
        public string? CarePlanInstruction { get; set; }
        public EmergencyAdmissionRequestDoctorInfo? ActiveIgdDoctor { get; set; }
        public bool CanSign { get; set; }
        public DateTime? SignatureRequestedAt { get; set; }
        public EmergencyAdmissionRequestSignerInfo? Signer { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancellationReason { get; set; }
        public List<EmergencyAdmissionRequestEventItem> Events { get; set; } = [];
        public DateTime CreateDateTime { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class EmergencyInpatientDoctorOption
    {
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public string? SpecialistName { get; set; }
        public bool IsScheduledInUnit { get; set; }
    }

    public class EmergencyAdmissionRequestSnapshot
    {
        public DateTime CapturedAt { get; set; }
        public string Origin { get; set; } = "IGD";
        public string? EmergencyVisitNumber { get; set; }
        public string? EncounterNumber { get; set; }
        public EmergencyAdmissionRequestPatientInfo Patient { get; set; } = new();
        public EmergencyAdmissionRequestAllergyInfo Allergies { get; set; } = new();
        public EmergencyAdmissionRequestPayerInfo Payer { get; set; } = new();
        public List<EmergencyAdmissionRequestDiagnosisItem> Diagnoses { get; set; } = [];
        public string? RequestedServiceUnitName { get; set; }
        public string? RequestedPatientClassName { get; set; }
        public string? ProposedInpatientDoctorName { get; set; }
        public int EstimatedLengthOfStayDays { get; set; }
        public bool IsCito { get; set; }
        public bool NeedsIntensiveCare { get; set; }
        public bool NeedsIsolation { get; set; }
        public bool NeedsOxygenSupport { get; set; }
        public bool NeedsSpecialMonitoring { get; set; }
        public string? ClinicalIndication { get; set; }
        public string? CarePlanInstruction { get; set; }
        public EmergencyAdmissionRequestSignerInfo Signer { get; set; } = new();
    }

    public class EmergencyAdmissionRequestPrintResponse
    {
        public Guid Id { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string Origin { get; set; } = "IGD";
        public string? EmergencyVisitNumber { get; set; }
        public string? EncounterNumber { get; set; }
        public DateTime CapturedAt { get; set; }
        public EmergencyAdmissionRequestPatientInfo Patient { get; set; } = new();
        public EmergencyAdmissionRequestAllergyInfo Allergies { get; set; } = new();
        public EmergencyAdmissionRequestPayerInfo Payer { get; set; } = new();
        public List<EmergencyAdmissionRequestDiagnosisItem> Diagnoses { get; set; } = [];
        public string? RequestedServiceUnitName { get; set; }
        public string? RequestedPatientClassName { get; set; }
        public string? ProposedInpatientDoctorName { get; set; }
        public int EstimatedLengthOfStayDays { get; set; }
        public bool IsCito { get; set; }
        public List<string> SpecialNeeds { get; set; } = [];
        public string? ClinicalIndication { get; set; }
        public string? CarePlanInstruction { get; set; }
        public EmergencyAdmissionRequestSignerInfo Signer { get; set; } = new();
        public string SignatureStatement { get; set; } = string.Empty;
        public DateTime PrintedAt { get; set; }
    }
}
