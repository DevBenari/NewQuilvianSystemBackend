using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models
{
    [Table("TrxDoctorConsultation", Schema = "public")]
    public class TrxDoctorConsultation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string ConsultationNumber { get; set; } = string.Empty;

        [Required]
        public Guid EncounterId { get; set; }

        /// <summary>
        /// Antrean asal konsultasi. <b>Boleh kosong</b> sejak <c>BE-IGD-028</c>, dengan alasan
        /// yang sama seperti <c>TrxPatientAssessment.QueueId</c>: pasien IGD tidak berantre.
        /// </summary>
        /// <remarks>
        /// <c>FR-IGD-062</c>. Tabel milik <c>ClinicalManagement</c>; perubahan dikerjakan IGD
        /// atas wewenang <c>IGD-DEC-107</c>.
        /// </remarks>
        public Guid? QueueId { get; set; }

        public Guid? AssessmentId { get; set; }

        /// <summary>
        /// Perawatan rawat inap yang menaungi catatan ini. <b>Boleh kosong</b>: catatan
        /// poliklinik, IGD, dan medical check-up tidak punya perawatan rawat inap.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-040</c>, <c>INV-DOK-01</c>. Episode sebenarnya dapat diturunkan dari
        /// <see cref="EncounterId"/> karena satu episode menempel pada tepat satu kunjungan,
        /// tetapi kolom ini tetap disimpan supaya pertanyaan "perawatan A atau perawatan B"
        /// terjawab tanpa join berlapis pada setiap pembacaan lini masa, dan supaya penjagaan
        /// <c>INV-DOK-01</c> menjadi pemeriksaan satu kolom.
        ///
        /// <para>
        /// <b>Keduanya wajib sepakat.</b> Bila kolom ini terisi tetapi tidak cocok dengan
        /// perawatan milik <see cref="EncounterId"/>, permintaan ditolak — <c>VAL-DOK-26</c>.
        /// </para>
        /// </remarks>
        public Guid? InpEpisodeId { get; set; }

        /// <summary>
        /// Waktu pemeriksaan yang sebenarnya, berbeda dari waktu penulisan pada
        /// <see cref="ConsultationDateTime"/>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-040</c>. Visite pukul 07.40 yang baru sempat diketik pukul 11.00 harus
        /// terbaca pada urutan pukul 07.40; memaksa keduanya sama membuat lini masa perkembangan
        /// pasien menyesatkan. Boleh kosong: catatan lama dan catatan rawat jalan tidak
        /// memilikinya.
        /// </remarks>
        public DateTime? ClinicalDateTime { get; set; }

        /// <summary>
        /// Tautan <b>opsional</b> ke kejadian visite dokter pada <c>CliPhysicianVisit</c>.
        /// </summary>
        /// <remarks>
        /// <c>BE-RWI-040</c>, <c>INV-DOK-07</c>. Satu kejadian visite tidak wajib punya catatan,
        /// dan satu catatan tidak wajib punya kejadian visite. Karena itu kolomnya nullable dan
        /// perilaku hapusnya <c>SetNull</c>: kejadian visite yang dibatalkan tidak boleh
        /// menyeret catatan SOAP-nya.
        ///
        /// <para>
        /// Namanya sengaja bukan <c>VisitId</c> supaya tidak tertukar dengan kunjungan IGD.
        /// </para>
        /// </remarks>
        public Guid? PhysicianVisitId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DoctorId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        public Guid? ClinicId { get; set; }

        public DateTime ConsultationDateTime { get; set; } = DateTime.UtcNow;

        public DoctorConsultationStatus ConsultationStatus { get; set; } = DoctorConsultationStatus.Draft;

        // =========================
        // VITAL SIGN SNAPSHOT
        // =========================
        public bool IsVitalSignCopiedFromAssessment { get; set; } = true;

        public int? BloodPressureSystolic { get; set; }

        public int? BloodPressureDiastolic { get; set; }

        public int? PulseRate { get; set; }

        public int? RespiratoryRate { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? OxygenSaturation { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        public decimal? BMI { get; set; }

        // =========================
        // CLINICAL SUMMARY
        // =========================
        [MaxLength(1000)]
        public string? ChiefComplaint { get; set; }

        [MaxLength(4000)]
        public string? HistoryOfPresentIllness { get; set; }

        [MaxLength(4000)]
        public string? PhysicalExamination { get; set; }

        // =========================
        // SOAP
        // =========================
        [MaxLength(4000)]
        public string? Subjective { get; set; }

        [MaxLength(4000)]
        public string? Objective { get; set; }

        [MaxLength(4000)]
        public string? Assessment { get; set; }

        [MaxLength(4000)]
        public string? Plan { get; set; }

        // =========================
        // DIAGNOSIS SUMMARY
        // Detail ICD-10 disimpan di TrxPatientDiagnosis
        // =========================
        [MaxLength(2000)]
        public string? DiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? PrimaryDiagnosisText { get; set; }

        [MaxLength(2000)]
        public string? SecondaryDiagnosisText { get; set; }

        public int DiagnosisCount { get; set; } = 0;

        public bool HasPrimaryDiagnosis { get; set; } = false;

        // =========================
        // PROCEDURE SUMMARY
        // Detail tindakan disimpan di TrxPatientProcedure
        // =========================
        [MaxLength(2000)]
        public string? ProcedureText { get; set; }

        public int ProcedureCount { get; set; } = 0;

        public bool HasProcedure { get; set; } = false;

        // =========================
        // PRESCRIPTION SUMMARY
        // Detail resep nanti disimpan di PharmacyManagement
        // =========================
        [MaxLength(2000)]
        public string? PrescriptionText { get; set; }

        public int PrescriptionCount { get; set; } = 0;

        public bool HasPrescription { get; set; } = false;

        // =========================
        // SUPPORTING ORDER SUMMARY
        // Detail order lab/radiology/penunjang nanti di OrderManagement
        // =========================
        [MaxLength(2000)]
        public string? SupportingOrderText { get; set; }

        public int SupportingOrderCount { get; set; } = 0;

        public bool HasSupportingOrder { get; set; } = false;

        // =========================
        // DOCUMENT / CONSENT SUMMARY
        // Detail dokumen, consent, surat medis disimpan di table terpisah
        // =========================
        public int MedicalCertificateCount { get; set; } = 0;

        public int ClinicalDocumentCount { get; set; } = 0;

        public int ConsentCount { get; set; } = 0;

        // =========================
        // PLAN SUMMARY
        // Detail tindakan/resep/surat/penunjang nanti bisa tabel terpisah
        // =========================
        [MaxLength(2000)]
        public string? ProcedurePlan { get; set; }

        [MaxLength(2000)]
        public string? PrescriptionPlan { get; set; }

        [MaxLength(2000)]
        public string? SupportingExamPlan { get; set; }

        [MaxLength(2000)]
        public string? ReferralPlan { get; set; }

        [MaxLength(2000)]
        public string? EducationPlan { get; set; }

        public DateTime? FollowUpDate { get; set; }

        [MaxLength(500)]
        public string? FollowUpNote { get; set; }

        // =========================
        // WORKFLOW
        // =========================
        public DateTime? StartedAt { get; set; }

        public Guid? StartedByUserId { get; set; }

        public DateTime? CompletedAt { get; set; }

        public Guid? CompletedByUserId { get; set; }

        public DateTime? CancelledAt { get; set; }

        public Guid? CancelledByUserId { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }

        [MaxLength(1000)]
        public string? DoctorNote { get; set; }

        public bool IsActive { get; set; } = true;

        public TrxPatientEncounter? Encounter { get; set; }

        public TrxQueue? Queue { get; set; }

        public TrxPatientAssessment? PatientAssessment { get; set; }

        public MstPatient? Patient { get; set; }

        public MstDoctor? Doctor { get; set; }

        public MstServiceUnit? ServiceUnit { get; set; }

        public MstClinic? Clinic { get; set; }

        public ApplicationUser? StartedByUser { get; set; }

        public ApplicationUser? CompletedByUser { get; set; }

        public ApplicationUser? CancelledByUser { get; set; }
    }
}