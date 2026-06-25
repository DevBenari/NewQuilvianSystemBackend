using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class NurseStationQueueSummaryResponse
    {
        public int TotalQueue { get; set; }
        public int WaitingForNurseQueue { get; set; }
        public int CalledByNurseQueue { get; set; }
        public int InNurseScreeningQueue { get; set; }
        public int WaitingForDoctorQueue { get; set; }
        public int CompletedQueue { get; set; }
        public int SkippedQueue { get; set; }
        public int NoShowQueue { get; set; }
        public int PriorityQueue { get; set; }
    }

    public class NurseStationQueueResponse
    {
        public Guid Id { get; set; }
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public Guid? NurseStationClusterId { get; set; }
        public string? NurseStationClusterName { get; set; }
        public DateTime QueueDate { get; set; }
        public int QueueNumber { get; set; }
        public string QueueCode { get; set; } = string.Empty;
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public int NurseCallAttemptCount { get; set; }
        public DateTime? LastNurseCalledAt { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public DateTime? ScreeningStartedAt { get; set; }
        public DateTime? ScreeningCompletedAt { get; set; }
        public int SkipCount { get; set; }
        public int RequeueCount { get; set; }
        public DateTime? NoShowAt { get; set; }
        public bool IsPriorityQueue { get; set; }
        public bool IsScreeningRequired { get; set; }
        public bool IsDoctorRequired { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class NurseStationQueueActionRequest
    {
        [MaxLength(250)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class NurseStationQueueActionResponse
    {
        public Guid QueueId { get; set; }
        public Guid EncounterId { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public EncounterStatus EncounterStatus { get; set; }
        public string EncounterStatusName { get; set; } = string.Empty;
        public int NurseCallAttemptCount { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public DateTime? ScreeningStartedAt { get; set; }
        public DateTime? ScreeningCompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class NurseStationQueueFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public NurseStationQueueDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<NurseStationQueueSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<NurseStationQueueStatusOptionResponse> QueueStatusOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class NurseStationQueueDefaultFilterResponse
    {
        public DateTime? QueueDate { get; set; }
        public Guid? NurseStationClusterId { get; set; }
        public QueueStatus? QueueStatus { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "queueNumber";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class NurseStationQueueSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class NurseStationQueueStatusOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
