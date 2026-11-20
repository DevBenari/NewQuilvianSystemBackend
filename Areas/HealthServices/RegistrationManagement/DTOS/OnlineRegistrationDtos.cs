using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Filter daftar Pendaftaran Online untuk petugas (<c>BE-KSK-PO-009</c>, <c>KSK-DEC-050</c>).
    /// Rentang bawaan hari ini (WIB), paling panjang 62 hari (<c>KSK-VAL-PO-069</c>).
    /// </summary>
    public class OnlineRegistrationListQuery
    {
        public DateTime? VisitDateFrom { get; set; }

        public DateTime? VisitDateTo { get; set; }

        public OnlineRegistrationStatus? Status { get; set; }

        /// <summary>
        /// Minimal 3 karakter (<c>KSK-VAL-PO-068</c>). Kode dicocokkan persis lewat hash; nomor
        /// pendaftaran, nama, No. RM, dan NIK dicocokkan sebagian.
        /// </summary>
        public string? Search { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public class OnlineRegistrationListItemResponse
    {
        public Guid Id { get; set; }

        public string RegistrationNumber { get; set; } = string.Empty;

        public OnlineRegistrationStatus Status { get; set; }

        public string StatusName { get; set; } = string.Empty;

        /// <summary>Kalender WIB, <c>yyyy-MM-dd</c>.</summary>
        public string VisitDate { get; set; } = string.Empty;

        public string ClinicName { get; set; } = string.Empty;

        public string DoctorName { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string StartTime { get; set; } = string.Empty;

        /// <summary>Format <c>HH:mm</c>.</summary>
        public string EndTime { get; set; } = string.Empty;

        public OnlineRegistrationPatientMode PatientMode { get; set; }

        public string PatientModeName { get; set; } = string.Empty;

        public string PatientName { get; set; } = string.Empty;

        /// <summary>Kosong untuk pasien baru sebelum check-in.</summary>
        public string? MedicalRecordNumber { get; set; }

        public string? IdentityNumber { get; set; }

        public OnlineRegistrationPatientType PatientType { get; set; }

        public string PatientTypeName { get; set; } = string.Empty;

        public EncounterPaymentType PaymentType { get; set; }

        public string PaymentTypeName { get; set; } = string.Empty;

        /// <summary>No. WhatsApp atau alamat email yang diisi online.</summary>
        public string? Contact { get; set; }

        public OnlineRegistrationEmailStatus EmailStatus { get; set; }

        public string EmailStatusName { get; set; } = string.Empty;

        public DateTime SubmittedAt { get; set; }

        /// <summary>Nomor kunjungan sesudah check-in.</summary>
        public string? EncounterNumber { get; set; }

        public bool CanChangeSchedule { get; set; }

        public bool CanChangeContact { get; set; }

        public bool CanCancel { get; set; }

        public Guid RowVersion { get; set; }
    }

    public class OnlineRegistrationDetailResponse : OnlineRegistrationListItemResponse
    {
        /// <summary>Kalender, <c>yyyy-MM-dd</c>.</summary>
        public string? BirthDate { get; set; }

        public Gender? Gender { get; set; }

        public string? GenderName { get; set; }

        public Guid ClinicId { get; set; }

        public Guid DoctorScheduleId { get; set; }

        public string? SessionName { get; set; }

        /// <summary>Penyedia/perusahaan dari kartu yang sudah ada.</summary>
        public string? PayerName { get; set; }

        /// <summary>Nomor kartu yang sudah ada, tersamar <c>****1234</c>.</summary>
        public string? MaskedCardNumber { get; set; }

        public OnlineRegistrationNewInsuranceResponse? NewInsurance { get; set; }

        public bool IsReferral { get; set; }

        public string? ReferralInstitutionName { get; set; }

        public string? ReferralNumber { get; set; }

        /// <summary>Kalender, <c>yyyy-MM-dd</c>.</summary>
        public string? ReferralDate { get; set; }

        public string? ReferralDoctorName { get; set; }

        public string? DiagnosisNote { get; set; }

        public string? Reason { get; set; }

        public List<OnlineRegistrationDocumentResponse> Documents { get; set; } = new();

        public bool HasConsent { get; set; }

        public DateTime? ConsentSignedAt { get; set; }

        public DateTime? CheckedInAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        public string? CancelReason { get; set; }

        public List<OnlineRegistrationHistoryResponse> Histories { get; set; } = new();
    }

    public class OnlineRegistrationNewInsuranceResponse
    {
        public Guid InsuranceProviderId { get; set; }

        public string ProviderName { get; set; } = string.Empty;

        public string PolicyNumber { get; set; } = string.Empty;

        public string? HolderName { get; set; }

        public string? HolderRelationship { get; set; }
    }

    public class OnlineRegistrationDocumentResponse
    {
        public Guid Id { get; set; }

        public string OriginalFileName { get; set; } = string.Empty;

        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        public int PageOrder { get; set; }
    }

    public class OnlineRegistrationHistoryResponse
    {
        public string EventCode { get; set; } = string.Empty;

        public string EventName { get; set; } = string.Empty;

        public OnlineRegistrationStatus? FromStatus { get; set; }

        public OnlineRegistrationStatus ToStatus { get; set; }

        public string SourceName { get; set; } = string.Empty;

        /// <summary>Nama petugas, atau nama sumber (Halaman Online, Kiosk, Sistem) bila tanpa petugas.</summary>
        public string ActorName { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public string? FromVisitDate { get; set; }

        public string? ToVisitDate { get; set; }

        public string? FromScheduleLabel { get; set; }

        public string? ToScheduleLabel { get; set; }

        public string? FromContact { get; set; }

        public string? ToContact { get; set; }

        public DateTime OccurredAt { get; set; }
    }

    /// <summary>Jumlah pendaftaran per status pada rentang tanggal kunjungan.</summary>
    public class OnlineRegistrationSummaryResponse
    {
        public string VisitDateFrom { get; set; } = string.Empty;

        public string VisitDateTo { get; set; } = string.Empty;

        public int AwaitingCheckIn { get; set; }

        public int CheckedIn { get; set; }

        public int RegisteredElsewhere { get; set; }

        public int NoShow { get; set; }

        public int Cancelled { get; set; }

        public int Total { get; set; }
    }
}
