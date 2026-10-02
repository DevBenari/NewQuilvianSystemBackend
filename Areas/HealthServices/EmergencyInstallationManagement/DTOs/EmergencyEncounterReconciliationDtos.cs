using QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Responses;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.EmergencyInstallationManagement.DTOs
{
    public class EmergencyEncounterReconciliationPreviewResponse
    {
        public int CountK1 { get; set; }
        public int CountK1Outpatient { get; set; }
        public int CountK2 { get; set; }
        public int CountK3 { get; set; }
        public int CountK4 { get; set; }

        public int ExpectedCount { get; set; }

        public PagedResult<EmergencyEncounterReconciliationPreviewRowResponse> Rows { get; set; }
            = new PagedResult<EmergencyEncounterReconciliationPreviewRowResponse>();
    }

    public class EmergencyEncounterReconciliationPreviewRowResponse
    {
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public EncounterType EncounterType { get; set; }
        public EncounterStatus EncounterStatus { get; set; }
        public DateTime RegisteredAt { get; set; }
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string EmergencyVisitNumber { get; set; } = string.Empty;
        public EmergencyVisitStatus VisitStatus { get; set; }
        public DateTime? VisitCompletedAt { get; set; }
        public EmergencyReconciliationClass Class { get; set; }
        public EncounterStatus StatusAfter { get; set; }
        public DateTime? CompletedAtAfter { get; set; }
    }

    public class ExecuteEmergencyEncounterReconciliationRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }

        public int? ExpectedCount { get; set; }
    }

    public class ReverseEmergencyEncounterReconciliationRequest
    {
        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    public class EmergencyEncounterReconciliationRunResponse
    {
        public Guid Id { get; set; }
        public string RunNumber { get; set; } = string.Empty;
        public EmergencyReconciliationRunStatus Status { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int CountK1 { get; set; }
        public int CountK1Outpatient { get; set; }
        public int CountK2 { get; set; }
        public int CountK3 { get; set; }
        public int CountK4 { get; set; }
        public int CountWritten { get; set; }
        public Guid ExecutedByUserId { get; set; }
        public string? ExecutedByName { get; set; }
        public DateTime ExecutedAt { get; set; }
        public Guid? ReversedByUserId { get; set; }
        public string? ReversedByName { get; set; }
        public DateTime? ReversedAt { get; set; }
        public string? ReverseReason { get; set; }
        public int? CountReversed { get; set; }
        public int? CountSkipped { get; set; }

        public List<EmergencyEncounterReconciliationItemResponse>? Items { get; set; }
    }

    public class EmergencyEncounterReconciliationItemResponse
    {
        public Guid Id { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid EmergencyVisitId { get; set; }
        public string? EmergencyVisitNumber { get; set; }
        public EmergencyReconciliationClass Class { get; set; }
        public EncounterStatus StatusBefore { get; set; }
        public EncounterStatus StatusAfter { get; set; }
        public DateTime? CompletedAtBefore { get; set; }
        public DateTime? CompletedAtAfter { get; set; }
        public bool IsReversed { get; set; }
        public string? ReverseSkipReason { get; set; }
    }
}
