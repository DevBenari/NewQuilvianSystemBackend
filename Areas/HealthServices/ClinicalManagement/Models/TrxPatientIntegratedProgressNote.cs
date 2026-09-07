using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxPatientIntegratedProgressNote", Schema = "public")]
    public class TrxPatientIntegratedProgressNote : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ProgressNoteNumber { get; set; } = string.Empty;

        // =========================
        // CLINICAL CONTEXT
        // =========================

        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }

        public Guid? QueueId { get; set; }

        public Guid? ConsultationId { get; set; }

        public Guid? AssessmentId { get; set; }

        public Guid? VitalSignId { get; set; }

        public Guid? DoctorId { get; set; }

        public Guid? ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi catatan ini. Boleh kosong — <c>INV-DOK-01</c>.
        /// </summary>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Keadaan verifikasi DPJP atas catatan ini.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-040</c>. Bawaannya <c>NotRequired</c>: verifikasi hanya berlaku bila rumah
        /// sakit memang mewajibkannya, dan catatan yang tidak diwajibkan tidak boleh memenuhi
        /// daftar pantau.
        /// </remarks>
        public CpptVerificationStatus VerificationStatus { get; set; } = CpptVerificationStatus.NotRequired;

        /// <summary>Waktu verifikasi DPJP.</summary>
        public DateTime? VerifiedAt { get; set; }

        /// <summary>
        /// Pengguna yang memverifikasi. <b>Sengaja terpisah dari penulis asli</b> pada
        /// <see cref="ProviderUserId"/>.
        /// </summary>
        /// <remarks>
        /// <c>INV-DOK-11</c>, <c>AC-CAP021-03</c>. Verifikasi tidak pernah menulis ulang penulis
        /// catatan; keduanya adalah dua orang dengan dua tanggung jawab yang berbeda.
        /// </remarks>
        public Guid? VerifiedByUserId { get; set; }

        /// <summary>
        /// Batas waktu verifikasi. Kosong berarti tidak dipantau.
        /// </summary>
        /// <remarks>
        /// Nilai batasnya dihitung dari kebijakan, bukan ditanam di kode. <c>RWI-RULE-021</c>
        /// masih menunggu pengesahan pemilik klinis, sehingga kebijakan kosong berarti kolom ini
        /// tetap kosong dan daftar pantau tetap kosong.
        /// </remarks>
        public DateTime? VerificationDueAt { get; set; }

        // =========================
        // TIMELINE INFORMATION
        // =========================

        public DateTime NoteDateTime { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(50)]
        public string ProfessionType { get; set; } = string.Empty;
        // Doctor, Nurse, Pharmacist, Nutritionist, Midwife, Physiotherapist, Laboratory, Radiology, Other.

        [MaxLength(100)]
        public string? ProfessionName { get; set; }
        // Display label: Dokter, Perawat, Farmasi, Gizi, etc.

        public Guid? ProviderUserId { get; set; }

        [MaxLength(150)]
        public string? ProviderDisplayNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? ProviderRoleSnapshot { get; set; }

        [MaxLength(150)]
        public string? ServiceUnitNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? LocationSnapshot { get; set; }

        // =========================
        // SOURCE MODULE LINK
        // =========================

        [MaxLength(80)]
        public string? SourceModule { get; set; }
        // DoctorConsultation, PatientAssessment, PatientVitalSign, Prescription, Procedure, Pharmacy, Nutrition, ManualEntry.

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(80)]
        public string? SourceReferenceNumber { get; set; }

        // =========================
        // NOTE CONTENT
        // =========================

        public string? SubjectiveSummary { get; set; }

        public string? ObjectiveSummary { get; set; }

        public string? AssessmentSummary { get; set; }

        public string? PlanSummary { get; set; }

        public string? Instruction { get; set; }

        public string? Evaluation { get; set; }

        public string? NoteText { get; set; }

        public string? PrivateNote { get; set; }

        // =========================
        // WORKFLOW
        // =========================

        public bool IsGeneratedFromSource { get; set; } = false;

        public bool IsReadOnlyGenerated { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        // =========================
        // NAVIGATION
        // =========================

        public MstPatient? Patient { get; set; }

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxQueue? Queue { get; set; }

        public TrxDoctorConsultation? Consultation { get; set; }

        public TrxPatientAssessment? Assessment { get; set; }

        public TrxPatientVitalSign? VitalSign { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? ProviderUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}
