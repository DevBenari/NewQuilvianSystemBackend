using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    /// <summary>
    /// Surat dokter rawat jalan — surat sakit, surat sehat, dan surat rujukan
    /// (RJ-DOC-REV-BE-004). Data pasien, dokter, dan tujuan rujukan disimpan sebagai snapshot
    /// saat surat terbit supaya cetak ulang tetap sama walau master berubah.
    /// </summary>
    public class CliDoctorCertificate : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string CertificateNumber { get; set; } = string.Empty;

        public DoctorCertificateType CertificateType { get; set; } = DoctorCertificateType.SickLeave;

        public DoctorCertificateStatus CertificateStatus { get; set; } = DoctorCertificateStatus.Issued;

        public Guid PatientId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid? QueueId { get; set; }

        public Guid? ConsultationId { get; set; }

        /// <summary>Dokter Penanggung Jawab kunjungan, diambil dari kunjungan — bukan dari klien.</summary>
        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public DateTime IssuedDate { get; set; }

        public string? Purpose { get; set; }

        public string? Diagnosis { get; set; }

        public string? ClinicalSummary { get; set; }

        public string? AdditionalNote { get; set; }

        // Snapshot identitas
        public string PatientNameSnapshot { get; set; } = string.Empty;
        public string? MedicalRecordNumberSnapshot { get; set; }
        public DateTime? BirthDateSnapshot { get; set; }
        public string? GenderSnapshot { get; set; }
        public string? AddressSnapshot { get; set; }
        public string? OccupationSnapshot { get; set; }
        public string? DoctorNameSnapshot { get; set; }
        public string? DoctorSipSnapshot { get; set; }
        public string? ClinicNameSnapshot { get; set; }
        public string? DoctorSignatureDataUrl { get; set; }

        // Surat sakit
        public DateTime? SickStartDate { get; set; }
        public DateTime? SickEndDate { get; set; }
        public int? SickDurationDays { get; set; }
        public string? ActivityRestriction { get; set; }

        // Surat sehat
        public DateTime? ExaminationDate { get; set; }
        public string? HealthConclusion { get; set; }
        public string? BloodPressure { get; set; }
        public string? Pulse { get; set; }
        public string? Temperature { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? ColorBlindResult { get; set; }
        public string? HealthRecommendation { get; set; }

        // Surat rujukan
        public DateTime? ReferralAdmissionDate { get; set; }
        public string? ReferralDiagnosis { get; set; }
        public string? ReferralReason { get; set; }
        public Guid? ReferralTargetServiceUnitId { get; set; }
        public Guid? ReferralTargetClinicId { get; set; }
        public string? ReferralTargetNameSnapshot { get; set; }
        public string? RequestedRoomClass { get; set; }
        public string? SpecialInstruction { get; set; }

        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
