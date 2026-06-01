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
    [Table("TrxPatientDiagnosis", Schema = "public")]
    public class TrxPatientDiagnosis : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid ConsultationId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public Guid? DiagnosisId { get; set; }
        // Relasi ke master ICD-10 MstDiagnosis.
        // Nullable agar data lama / diagnosis manual tetap aman.

        [Required]
        [MaxLength(50)]
        public string DiagnosisCode { get; set; } = string.Empty;
        // Snapshot kode diagnosis, contoh: I10, E11.9

        [Required]
        [MaxLength(500)]
        public string DiagnosisName { get; set; } = string.Empty;
        // Snapshot nama diagnosis dari MstDiagnosis saat transaksi dibuat.

        [MaxLength(50)]
        public string DiagnosisMasterType { get; set; } = "ICD10";
        // ICD10, Local, Custom, Manual

        [MaxLength(100)]
        public string? IcdVersion { get; set; }
        // Contoh: ICD-10 2019

        public PatientDiagnosisType DiagnosisType { get; set; } = PatientDiagnosisType.Secondary;

        public PatientDiagnosisStatus DiagnosisStatus { get; set; } = PatientDiagnosisStatus.Active;

        public bool IsPrimary { get; set; } = false;

        public bool IsChronic { get; set; } = false;

        public bool IsNewCase { get; set; } = true;

        public bool IsConfirmed { get; set; } = true;
        // True jika diagnosis sudah dikonfirmasi dokter.

        public bool IsFromMasterDiagnosis { get; set; } = false;
        // True jika dipilih dari MstDiagnosis, false jika manual/free text.

        public DateTime DiagnosisDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? OnsetDate { get; set; }

        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        [MaxLength(1000)]
        public string? AssessmentNote { get; set; }

        [MaxLength(1000)]
        public string? PlanNote { get; set; }

        [MaxLength(500)]
        public string? DifferentialDiagnosisNote { get; set; }

        [MaxLength(500)]
        public string? SupportingFindingNote { get; set; }

        public int SortOrder { get; set; } = 0;

        public DateTime? ResolvedAt { get; set; }

        public Guid? ResolvedByUserId { get; set; }

        [MaxLength(250)]
        public string? ResolvedReason { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public MstPatient? Patient { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public MstDiagnosis? Diagnosis { get; set; }

        public ApplicationUser? ResolvedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
