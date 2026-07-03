using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    public class DoctorQueueSummaryResponse
    {
        public int TotalQueue { get; set; }
        public int WaitingForDoctorQueue { get; set; }
        public int CalledByDoctorQueue { get; set; }
        public int InConsultationQueue { get; set; }
        public int CompletedQueue { get; set; }
        public int SkippedQueue { get; set; }
        public int NoShowQueue { get; set; }
        public int PriorityQueue { get; set; }
    }

    public class DoctorQueueResponse
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
        public string? DoctorCode { get; set; }
        public string? DoctorNumber { get; set; }
        public Guid? DoctorWorkforceProfileId { get; set; }
        public string? DoctorSpecialistName { get; set; }
        public string? DoctorSubSpecialistName { get; set; }

        public Guid? DoctorStrCredentialLicenseId { get; set; }
        public string? DoctorStrNumber { get; set; }
        public DateTime? DoctorStrIssueDate { get; set; }
        public DateTime? DoctorStrExpiredDate { get; set; }
        public bool DoctorStrIsVerified { get; set; }
        public bool DoctorStrIsCurrentlyValid { get; set; }

        public Guid? DoctorSipCredentialLicenseId { get; set; }
        public string? DoctorSipNumber { get; set; }
        public DateTime? DoctorSipIssueDate { get; set; }
        public DateTime? DoctorSipExpiredDate { get; set; }
        public string? DoctorSipPracticeLocation { get; set; }
        public bool DoctorSipIsVerified { get; set; }
        public bool DoctorSipIsCurrentlyValid { get; set; }

        public string? DoctorRegistrationNumber { get; set; }
        public string? DoctorLicenseNumber { get; set; }
        public string? DoctorCredentialLicenseType { get; set; }
        public string? DoctorCredentialLicenseNumber { get; set; }
        public DateTime? DoctorCredentialLicenseExpiredDate { get; set; }
        public bool DoctorCredentialLicenseIsVerified { get; set; }
        public bool DoctorCredentialLicenseIsCurrentlyValid { get; set; }

        public string? DoctorProfilePhotoPath { get; set; }
        public string? DoctorProfilePhotoUrl { get; set; }
        public string? DoctorPhotoPath { get; set; }
        public string? DoctorPhotoUrl { get; set; }
        public DateTime QueueDate { get; set; }
        public int QueueNumber { get; set; }
        public string QueueCode { get; set; } = string.Empty;
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public int DoctorCallAttemptCount { get; set; }
        public DateTime? LastDoctorCalledAt { get; set; }
        public DateTime? DoctorCallExpiresAt { get; set; }
        public DateTime ServerNowUtc { get; set; }
        public int DoctorCallRemainingSeconds { get; set; }
        public DateTime? ScreeningCompletedAt { get; set; }
        public DateTime? ConsultationStartedAt { get; set; }
        public DateTime? ConsultationCompletedAt { get; set; }
        public int SkipCount { get; set; }
        public int RequeueCount { get; set; }
        public bool IsPriorityQueue { get; set; }
        public bool IsDoctorRequired { get; set; }

        public EncounterPaymentType PaymentType { get; set; }
        public string PaymentTypeName { get; set; } = string.Empty;
        public Guid? PaymentMethodId { get; set; }
        public string? PaymentMethodName { get; set; }
        public string? PrimaryGuarantorNameSnapshot { get; set; }
        public string? PrimaryGuarantorTypeSnapshot { get; set; }

        public int PatientTotalVisitCount { get; set; }
        public int PatientVisitNumber { get; set; }

        public string? ChiefComplaint { get; set; }
        public string? AgeTextAtEncounter { get; set; }
        public string? AgeCategoryCodeSnapshot { get; set; }
        public string? AgeCategoryNameSnapshot { get; set; }
        public string? Notes { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class DoctorQueueActionRequest
    {
        [MaxLength(250)]
        public string? Reason { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public class DoctorQueueActionResponse
    {
        public Guid QueueId { get; set; }
        public Guid EncounterId { get; set; }
        public QueueStatus QueueStatus { get; set; }
        public string QueueStatusName { get; set; } = string.Empty;
        public EncounterStatus EncounterStatus { get; set; }
        public string EncounterStatusName { get; set; } = string.Empty;
        public int DoctorCallAttemptCount { get; set; }
        public DateTime? DoctorCallExpiresAt { get; set; }
        public DateTime ServerNowUtc { get; set; }
        public int DoctorCallRemainingSeconds { get; set; }
        public DateTime? ConsultationStartedAt { get; set; }
        public DateTime? ConsultationCompletedAt { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool VoiceEnabled { get; set; }
        public bool VoiceGenerated { get; set; }
        public bool VoiceFromCache { get; set; }
        public string? VoiceText { get; set; }
        public string? VoiceAudioUrl { get; set; }
        public string? VoiceAudioDownloadUrl { get; set; }
        public string? VoiceAudioFileName { get; set; }
        public string? VoiceDateKey { get; set; }
        public string? VoiceContentType { get; set; }
        public string? VoiceErrorMessage { get; set; }
    }

    public class DoctorQueueFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public DoctorQueueDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<DoctorQueueSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<DoctorQueueStatusOptionResponse> QueueStatusOptions { get; set; } = new();
        public string ResetButtonLabel { get; set; } = "Reset";
    }

    public class DoctorQueueDefaultFilterResponse
    {
        public DateTime? QueueDate { get; set; }
        public QueueStatus? QueueStatus { get; set; }
        public string? Search { get; set; }
        public string SortBy { get; set; } = "queueNumber";
        public string SortDirection { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class DoctorQueueSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class DoctorQueueStatusOptionResponse
    {
        public int Value { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }
}
