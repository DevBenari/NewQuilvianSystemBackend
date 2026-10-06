using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    // RJ-DOC-REV-BE-004 — kontrak mengikuti payload yang sudah dikirim layar Surat Dokter
    // (`buildDoctorCertificatePayload`). `certificateType` berupa teks: sickLeave, health,
    // inpatientReferral.

    public class DoctorCertificateSickLeaveRequest
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        [Range(1, 365)]
        public int? DurationDays { get; set; }
        [MaxLength(1000)]
        public string? ActivityRestriction { get; set; }
    }

    public class DoctorCertificateHealthRequest
    {
        public DateTime? ExaminationDate { get; set; }
        [MaxLength(1000)] public string? Conclusion { get; set; }
        [MaxLength(50)] public string? BloodPressure { get; set; }
        [MaxLength(50)] public string? Pulse { get; set; }
        [MaxLength(50)] public string? Temperature { get; set; }
        [MaxLength(50)] public string? Weight { get; set; }
        [MaxLength(50)] public string? Height { get; set; }
        [MaxLength(200)] public string? ColorBlindResult { get; set; }
        [MaxLength(2000)] public string? Recommendation { get; set; }
    }

    public class DoctorCertificateReferralRequest
    {
        public DateTime? AdmissionDate { get; set; }
        [MaxLength(2000)] public string? Diagnosis { get; set; }
        [MaxLength(2000)] public string? Reason { get; set; }
        /// <summary>Unit layanan tujuan (master). Salah satu dari unit atau klinik wajib diisi.</summary>
        public Guid? TargetServiceUnitId { get; set; }
        /// <summary>Klinik/poli tujuan (master).</summary>
        public Guid? TargetClinicId { get; set; }
        /// <summary>Nama lama berupa teks bebas; hanya dipakai bila tidak ada pilihan master.</summary>
        [MaxLength(250)] public string? TargetServiceUnit { get; set; }
        [MaxLength(100)] public string? RequestedRoomClass { get; set; }
        [MaxLength(2000)] public string? SpecialInstruction { get; set; }
    }

    public class SaveDoctorCertificateRequest
    {
        public Guid? QueueId { get; set; }

        [Required]
        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        [Required, MaxLength(30)]
        public string CertificateType { get; set; } = string.Empty;

        public DateTime? IssuedDate { get; set; }

        [MaxLength(500)] public string? Purpose { get; set; }
        [MaxLength(250)] public string? PatientName { get; set; }
        public DateTime? BirthDate { get; set; }
        [MaxLength(50)] public string? Gender { get; set; }
        [MaxLength(1000)] public string? Address { get; set; }
        [MaxLength(200)] public string? Occupation { get; set; }
        [MaxLength(100)] public string? DoctorSip { get; set; }
        /// <summary>Data URL gambar tanda tangan, maksimal ±2 MB.</summary>
        [MaxLength(2_800_000)] public string? DoctorSignatureDataUrl { get; set; }
        [MaxLength(2000)] public string? Diagnosis { get; set; }
        [MaxLength(4000)] public string? ClinicalSummary { get; set; }
        [MaxLength(2000)] public string? AdditionalNote { get; set; }

        public DoctorCertificateSickLeaveRequest? SickLeave { get; set; }
        public DoctorCertificateHealthRequest? Health { get; set; }
        public DoctorCertificateReferralRequest? InpatientReferral { get; set; }
    }

    public class CancelDoctorCertificateRequest
    {
        [Required, MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }

    public class DoctorCertificateResponse
    {
        public Guid Id { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
        public string CertificateType { get; set; } = string.Empty;
        public string CertificateTypeName { get; set; } = string.Empty;
        public int CertificateStatus { get; set; }
        public string CertificateStatusName { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ClinicId { get; set; }
        public DateTime IssuedDate { get; set; }
        public string? Purpose { get; set; }
        public string? Diagnosis { get; set; }
        public string? ClinicalSummary { get; set; }
        public string? AdditionalNote { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? MedicalRecordNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Address { get; set; }
        public string? Occupation { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSip { get; set; }
        public string? ClinicName { get; set; }
        public string? DoctorSignatureDataUrl { get; set; }
        public DoctorCertificateSickLeaveRequest? SickLeave { get; set; }
        public DoctorCertificateHealthRequest? Health { get; set; }
        public DoctorCertificateReferralResponse? InpatientReferral { get; set; }
        public string? CancelReason { get; set; }
        public DateTime CreateDateTime { get; set; }
        public DateTime? UpdateDateTime { get; set; }
    }

    public class DoctorCertificateReferralResponse
    {
        public DateTime? AdmissionDate { get; set; }
        public string? Diagnosis { get; set; }
        public string? Reason { get; set; }
        public Guid? TargetServiceUnitId { get; set; }
        public Guid? TargetClinicId { get; set; }
        public string? TargetServiceUnit { get; set; }
        public string? RequestedRoomClass { get; set; }
        public string? SpecialInstruction { get; set; }
    }

    /// <summary>Pilihan tujuan rujukan yang dapat dicari: unit layanan dan klinik aktif.</summary>
    public class DoctorCertificateReferralTargetOption
    {
        public string Kind { get; set; } = string.Empty; // serviceUnit | clinic
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ParentName { get; set; }
    }
}
