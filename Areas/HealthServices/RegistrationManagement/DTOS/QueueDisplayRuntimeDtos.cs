using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class QueueDisplayRuntimeCurrentResponse
    {
        public Guid DisplayDeviceId { get; set; }
        public string DisplayCode { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Guid NurseStationClusterId { get; set; }
        public string NurseStationClusterName { get; set; } = string.Empty;
        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public string? LocationName { get; set; }
        public string? FloorName { get; set; }
        public string? RoomName { get; set; }
        public bool EnableVoiceCalling { get; set; }
        public bool ShowPatientName { get; set; }
        public bool ShowDoctorName { get; set; }
        public bool ShowClinicName { get; set; }
        public int RefreshIntervalSeconds { get; set; }
        public DateTime ServerDateTime { get; set; }
    }

    public class QueueDisplayRuntimeItemResponse
    {
        public Guid QueueId { get; set; }
        public Guid EncounterId { get; set; }
        public string EncounterNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MaskedPatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;
        public Guid ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime QueueDate { get; set; }
        public int QueueNumber { get; set; }
        public string QueueCode { get; set; } = string.Empty;
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public bool IsPriorityQueue { get; set; }
        public DateTime? LastNurseCalledAt { get; set; }
        public DateTime? LastDoctorCalledAt { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public DateTime? DoctorCallExpiresAt { get; set; }
        public string DisplayText { get; set; } = string.Empty;
        public string? VoiceText { get; set; }
        public string? VoiceAudioUrl { get; set; }
        public string? VoiceAudioDownloadUrl { get; set; }
        public string? VoiceAudioFileName { get; set; }
        public string? VoiceDateKey { get; set; }
        public string? VoiceContentType { get; set; }
    }

    public class QueueDisplayRuntimeCalledResponse
    {
        public Guid? QueueId { get; set; }
        public string? QueueCode { get; set; }
        public QueueStatus? QueueStatus { get; set; }
        public string? QueueStatusName { get; set; }
        public string? ClinicName { get; set; }
        public string? DoctorName { get; set; }
        public DateTime? CalledAt { get; set; }
        public string? DisplayText { get; set; }
        public string? VoiceText { get; set; }
        public string? VoiceAudioUrl { get; set; }
        public string? VoiceAudioDownloadUrl { get; set; }
        public string? VoiceAudioFileName { get; set; }
        public string? VoiceDateKey { get; set; }
        public string? VoiceContentType { get; set; }
    }

    public class QueueDisplayRuntimeSummaryResponse
    {
        public int TotalQueue { get; set; }
        public int WaitingForNurseQueue { get; set; }
        public int CalledByNurseQueue { get; set; }
        public int InNurseScreeningQueue { get; set; }
        public int WaitingForDoctorQueue { get; set; }
        public int CalledByDoctorQueue { get; set; }
        public int InConsultationQueue { get; set; }
        public int CompletedQueue { get; set; }
    }
}
