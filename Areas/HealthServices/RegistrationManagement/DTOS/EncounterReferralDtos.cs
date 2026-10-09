using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.DTOs
{
    /// <summary>
    /// Blok rincian rujukan pada <c>POST /patient-encounters/admin</c> dan <c>/kiosk</c>
    /// (<c>RJ-DOC-REFERRAL-001@1.0.0</c>). Unit tujuan poli adalah klinik kunjungan itu sendiri,
    /// sehingga tidak dikirim dua kali.
    /// </summary>
    public class EncounterReferralCreateRequest
    {
        /// <summary>Tanggal dan jam rujukan; kosong berarti waktu saat ini.</summary>
        public DateTime? ReferralDateTime { get; set; }

        /// <summary>Hanya <c>Clinic</c> (1) pada endpoint create kunjungan.</summary>
        public ReferralTargetUnitType TargetUnitType { get; set; } = ReferralTargetUnitType.Clinic;

        public Guid? DiagnosisId { get; set; }

        [MaxLength(500)]
        public string? DiagnosisNote { get; set; }

        [MaxLength(1000)]
        public string? ReferralReason { get; set; }
    }

    /// <summary>
    /// <c>PUT /patient-encounters/{id}/referral</c> — membuat rincian (jalur Laboratorium),
    /// melengkapi, atau mengoreksi.
    /// </summary>
    public class UpsertEncounterReferralRequest
    {
        /// <summary>Wajib bila rincian sudah ada (<c>RJ-VAL-PM-11</c>).</summary>
        public Guid? ExpectedRowVersion { get; set; }

        [MaxLength(250)]
        public string? ReferralNumber { get; set; }

        public DateTime? ReferralDateTime { get; set; }

        public Guid? ReferralInstitutionId { get; set; }

        public Guid? ReferralDoctorId { get; set; }

        /// <summary>Hanya dipakai saat rincian belum ada; sesudahnya wajib sama (<c>RJ-VAL-PM-10</c>).</summary>
        public ReferralTargetUnitType? TargetUnitType { get; set; }

        public Guid? TargetServiceUnitId { get; set; }

        public Guid? DiagnosisId { get; set; }

        [MaxLength(500)]
        public string? DiagnosisNote { get; set; }

        [MaxLength(1000)]
        public string? ReferralReason { get; set; }
    }

    public class EncounterReferralCreatedResponse
    {
        public Guid Id { get; set; }
        public bool IsComplete { get; set; }
        public Guid RowVersion { get; set; }
    }

    public class EncounterReferralReferenceResponse
    {
        public Guid Id { get; set; }
        public string? Code { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsPartner { get; set; }
    }

    public class EncounterReferralDocumentResponse
    {
        public Guid Id { get; set; }
        public string OriginalFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public int PageOrder { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    /// <summary>
    /// Rincian rujukan satu kunjungan. <see cref="MissingFields"/> memakai nilai tetap:
    /// <c>referralNumber</c>, <c>referralDateTime</c>, <c>referralInstitution</c>,
    /// <c>targetUnit</c>, <c>diagnosis</c>, <c>referralReason</c>, <c>documents</c>.
    /// </summary>
    public class EncounterReferralResponse
    {
        public Guid? Id { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public bool IsReferral { get; set; }
        public string? ReferralNumber { get; set; }
        public DateTime? ReferralDateTime { get; set; }
        public EncounterReferralReferenceResponse? ReferralInstitution { get; set; }
        public EncounterReferralReferenceResponse? ReferralDoctor { get; set; }
        public bool InstitutionIsPartnerSnapshot { get; set; }

        /// <summary>Kode/nama fasilitas dan nomor PKS saat rujukan dicatat (DEC-FRJ-001).</summary>
        public string? InstitutionCodeSnapshot { get; set; }

        public string? InstitutionNameSnapshot { get; set; }

        public string? AgreementNumberSnapshot { get; set; }
        public ReferralTargetUnitType? TargetUnitType { get; set; }
        public string? TargetUnitTypeName { get; set; }
        public Guid? TargetServiceUnitId { get; set; }
        public Guid? TargetClinicId { get; set; }
        public string? TargetUnitName { get; set; }
        public EncounterReferralReferenceResponse? Diagnosis { get; set; }
        public string? DiagnosisNote { get; set; }
        public string? ReferralReason { get; set; }
        public ReferralCaptureSource? CaptureSource { get; set; }
        public bool IsComplete { get; set; }
        public List<string> MissingFields { get; set; } = new();
        public bool IsLocked { get; set; }
        public Guid? RowVersion { get; set; }
        public List<EncounterReferralDocumentResponse> Documents { get; set; } = new();
    }
}
