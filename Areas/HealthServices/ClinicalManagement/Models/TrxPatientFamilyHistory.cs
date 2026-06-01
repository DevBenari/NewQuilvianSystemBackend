using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientFamilyHistory", Schema = "public")]
    public class TrxPatientFamilyHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string FamilyHistoryRecordNumber { get; set; } = string.Empty;

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

        // Jika anggota keluarga juga terdaftar sebagai pasien.
        public Guid? FamilyMemberPatientId { get; set; }

        // Optional relasi ke master ICD-10.
        // Nullable agar riwayat keluarga manual tetap bisa dicatat.
        public Guid? DiagnosisId { get; set; }

        // =========================
        // FAMILY RELATIONSHIP
        // =========================

        public PatientFamilyRelationshipType RelationshipType { get; set; } =
            PatientFamilyRelationshipType.Unknown;

        public PatientFamilyRelationshipSide RelationshipSide { get; set; } =
            PatientFamilyRelationshipSide.Unknown;

        [MaxLength(200)]
        public string? FamilyMemberNameSnapshot { get; set; }
        // Optional. Bisa dikosongkan jika tidak ingin menyimpan nama anggota keluarga.

        [MaxLength(100)]
        public string? RelationshipDescription { get; set; }
        // Contoh: "Ayah kandung", "Ibu dari pihak ibu", "Kakak perempuan".

        public bool IsFirstDegreeRelative { get; set; } = false;
        // Father, Mother, Brother, Sister, Son, Daughter.

        public bool IsSecondDegreeRelative { get; set; } = false;
        // Grandfather, Grandmother, Uncle, Aunt.

        public bool IsSameHousehold { get; set; } = false;

        // =========================
        // CONDITION INFORMATION
        // =========================

        [MaxLength(50)]
        public string? ConditionCode { get; set; }
        // Snapshot kode ICD jika berasal dari MstDiagnosis.

        [Required]
        [MaxLength(500)]
        public string ConditionName { get; set; } = string.Empty;
        // Contoh: Diabetes Mellitus, Hipertensi, Kanker Payudara, Stroke.

        [MaxLength(250)]
        public string? ConditionGroupName { get; set; }
        // Contoh: Cardiovascular, Endocrine, Oncology, Respiratory.

        [MaxLength(50)]
        public string ConditionMasterType { get; set; } = "Manual";
        // ICD10, Local, Custom, Manual.

        [MaxLength(100)]
        public string? IcdVersion { get; set; }
        // Contoh: ICD-10 2019.

        public bool IsFromMasterDiagnosis { get; set; } = false;

        public PatientFamilyHistoryStatus FamilyHistoryStatus { get; set; } =
            PatientFamilyHistoryStatus.Active;

        public PatientFamilyHistoryCertainty Certainty { get; set; } =
            PatientFamilyHistoryCertainty.Unknown;

        public PatientFamilyRiskLevel RiskLevel { get; set; } =
            PatientFamilyRiskLevel.Unknown;

        // =========================
        // CLINICAL RISK FLAGS
        // =========================

        public bool IsHereditaryDisease { get; set; } = false;

        public bool IsGeneticRisk { get; set; } = false;

        public bool IsChronicDisease { get; set; } = false;

        public bool IsCancerRelated { get; set; } = false;

        public bool IsCardiovascularRisk { get; set; } = false;

        public bool IsMetabolicRisk { get; set; } = false;

        public bool IsMentalHealthRelated { get; set; } = false;

        public bool IsInfectiousDisease { get; set; } = false;

        public bool IsHighRisk { get; set; } = false;

        public bool IsScreeningRecommended { get; set; } = false;

        public bool IsAlertEnabled { get; set; } = false;
        // Dipakai nanti untuk CDSS / preventive screening alert.

        // =========================
        // TIMELINE
        // =========================

        public DateTime RecordedDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? DiagnosedDate { get; set; }

        public int? AgeAtDiagnosisYear { get; set; }

        public bool IsFamilyMemberDeceased { get; set; } = false;

        public DateTime? DeathDate { get; set; }

        public int? AgeAtDeathYear { get; set; }

        [MaxLength(250)]
        public string? CauseOfDeath { get; set; }

        // =========================
        // DETAIL NOTE
        // =========================

        [MaxLength(100)]
        public string? SourceOfInformation { get; set; }
        // Patient, Family, OldMedicalRecord, Doctor, Nurse, ExternalDocument.

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? RiskNote { get; set; }

        [MaxLength(1000)]
        public string? ScreeningRecommendation { get; set; }

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

        public MstPatient? FamilyMemberPatient { get; set; }

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