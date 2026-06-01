using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class QueueResponse
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
        public Guid? DoctorScheduleId { get; set; }
        public DateTime QueueDate { get; set; }
        public int QueueNumber { get; set; }
        public string QueueCode { get; set; } = string.Empty;
        public QueueStatus QueueStatus { get; set; }
        public int NurseCallAttemptCount { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public int DoctorCallAttemptCount { get; set; }
        public DateTime? DoctorCallExpiresAt { get; set; }
        public int SkipCount { get; set; }
        public int RequeueCount { get; set; }
        public bool IsPriorityQueue { get; set; }
        public bool IsScreeningRequired { get; set; }
        public bool IsDoctorRequired { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class QueueDetailResponse : QueueResponse
    {
        public DateTime? LastNurseCalledAt { get; set; }
        public DateTime? LastDoctorCalledAt { get; set; }
        public DateTime? ScreeningStartedAt { get; set; }
        public DateTime? ScreeningCompletedAt { get; set; }
        public DateTime? ConsultationStartedAt { get; set; }
        public DateTime? ConsultationCompletedAt { get; set; }
        public DateTime? LastSkippedAt { get; set; }
        public string? SkipReason { get; set; }
        public DateTime? LastRequeuedAt { get; set; }
        public string? RequeueReason { get; set; }
        public DateTime? NoShowAt { get; set; }
        public string? NoShowReason { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string? CancelReason { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Notes { get; set; }
    }

    public class QueueActionRequest
    {
        [MaxLength(250)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class QueueActionResponse
    {
        public Guid QueueId { get; set; }
        public Guid EncounterId { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public EncounterStatus EncounterStatus { get; set; }
        public int NurseCallAttemptCount { get; set; }
        public DateTime? NurseCallExpiresAt { get; set; }
        public int DoctorCallAttemptCount { get; set; }
        public DateTime? DoctorCallExpiresAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}