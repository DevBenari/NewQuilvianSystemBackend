using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientMedicalHistory", Schema = "public")]
    public class TrxPatientMedicalHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string MedicalHistoryRecordNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        // Optional relasi ke master ICD-10.
        // Nullable agar riwayat manual seperti "riwayat operasi usus buntu"
        // tetap bisa dicatat tanpa harus memilih ICD.
        public Guid? DiagnosisId { get; set; }

        // =========================
        // MEDICAL HISTORY INFORMATION
        // =========================

        public PatientMedicalHistoryType HistoryType { get; set; } = PatientMedicalHistoryType.Disease;

        public PatientMedicalHistoryStatus HistoryStatus { get; set; } = PatientMedicalHistoryStatus.Active;

        public PatientMedicalHistorySeverity Severity { get; set; } = PatientMedicalHistorySeverity.Unknown;

        public PatientMedicalHistoryCertainty Certainty { get; set; } = PatientMedicalHistoryCertainty.Unknown;

        [MaxLength(50)]
        public string? ConditionCode { get; set; }
        // Snapshot kode diagnosis jika berasal dari ICD-10. Contoh: I10, E11.9.

        [Required]
        [MaxLength(500)]
        public string ConditionName { get; set; } = string.Empty;
        // Contoh: Hipertensi, Diabetes Mellitus, Asma, Stroke.

        [MaxLength(250)]
        public string? ConditionGroupName { get; set; }
        // Contoh: Cardiovascular, Endocrine, Respiratory.

        [MaxLength(50)]
        public string ConditionMasterType { get; set; } = "Manual";
        // ICD10, Local, Custom, Manual.

        [MaxLength(100)]
        public string? IcdVersion { get; set; }
        // Contoh: ICD-10 2019.

        public bool IsFromMasterDiagnosis { get; set; } = false;

        // =========================
        // CLINICAL FLAGS
        // =========================

        public bool IsCurrentProblem { get; set; } = false;
        // True jika riwayat masih menjadi masalah aktif saat ini.

        public bool IsChronic { get; set; } = false;

        public bool IsComorbidity { get; set; } = false;
        // True jika kondisi ini menjadi komorbid penting.

        public bool IsUnderTreatment { get; set; } = false;

        public bool IsControlled { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsHereditaryRelated { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsPregnancyRelated { get; set; } = false;

        public bool IsSurgicalHistory { get; set; } = false;

        public bool IsHospitalizationHistory { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;
        // Dipakai untuk CDSS / clinical risk alert.

        public bool IsAlertEnabled { get; set; } = false;
        // Tidak semua riwayat perlu alert. Contoh CKD, DM, stroke bisa true.

        // =========================
        // TIMELINE
        // =========================

        public DateTime RecordedDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? OnsetDate { get; set; }

        public int? OnsetAgeYear { get; set; }

        public DateTime? DiagnosedDate { get; set; }

        public DateTime? LastTreatmentDate { get; set; }

        public DateTime? LastControlDate { get; set; }

        // =========================
        // DETAIL NOTE
        // =========================

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }
        // Patient, Family, OldMedicalRecord, Doctor, Nurse, ExternalDocument.

        [MaxLength(1000)]
        public string? TreatmentHistory { get; set; }

        [MaxLength(1000)]
        public string? MedicationHistory { get; set; }

        [MaxLength(1000)]
        public string? SurgeryHistory { get; set; }

        [MaxLength(1000)]
        public string? HospitalizationHistory { get; set; }

        [MaxLength(1000)]
        public string? ComplicationNote { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? RiskNote { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // =========================
        // VERIFICATION
        // =========================

        public bool IsVerified { get; set; } = false;

        public DateTime? VerifiedAt { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        // =========================
        // RESOLUTION / CANCELLATION
        // =========================

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        [MaxLength(250)]
        public string? ResolvedReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDiagnosis? Diagnosis { get; set; }

        public ApplicationUser? VerifiedByUser { get; set; }

        public ApplicationUser? ResolvedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
