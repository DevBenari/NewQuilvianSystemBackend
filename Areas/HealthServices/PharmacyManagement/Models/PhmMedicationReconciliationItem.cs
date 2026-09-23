using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Satu obat yang sedang dipakai pasien sebelum masuk rawat inap — <c>BE-RWI-101</c>,
    /// migration <c>R5</c>, kamus data 0.5 bagian 13.2.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Dicatat perawat saat admisi, diputuskan dokter lewat
    /// <see cref="PhmMedicationReconciliationDecision"/>. <see cref="CurrentDecision"/> hanya salinan
    /// keputusan terakhir supaya daftar dapat disaring tanpa membaca riwayat; sumber kebenarannya
    /// tetap baris keputusan terbaru.
    /// </para>
    /// <para>
    /// <b>Tidak ada kolom nama obat teks bebas</b> — <c>RWI-DEC-134</c>. Obat yang belum ada pada
    /// master obat didaftarkan dulu lewat jalur non-formularium.
    /// </para>
    /// </remarks>
    [Table("PhmMedicationReconciliationItem", Schema = "public")]
    public class PhmMedicationReconciliationItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid InpEpisodeId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid DrugId { get; set; }

        [Required]
        [MaxLength(250)]
        public string DrugNameSnapshot { get; set; } = string.Empty;

        public bool IsFormularySnapshot { get; set; }

        public decimal? Dose { get; set; }

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? DrugFormSnapshot { get; set; }

        [MaxLength(100)]
        public string? FrequencyText { get; set; }

        public HomeMedicationRoute Route { get; set; }

        /// <summary>Catatan perawat. SENSITIF — permission-audit-matrix 0.6.0 bagian 9.2.</summary>
        [MaxLength(500)]
        public string? Note { get; set; }

        public DateTime RecordedAt { get; set; }

        [Required]
        public Guid RecordedByEmployeeId { get; set; }

        [Required]
        public Guid RecordedByUserId { get; set; }

        public ReconciliationDecisionType CurrentDecision { get; set; } = ReconciliationDecisionType.Pending;

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }

        /// <summary>
        /// <c>false</c> bila baris salah catat dibatalkan perawat sebelum ada keputusan dokter.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public RegPatientEncounter? Encounter { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        public MstPatient? Patient { get; set; }

        public MstDrug? Drug { get; set; }

        public MstMeasurement? DoseUnitMeasurement { get; set; }

        public MstEmployee? RecordedByEmployee { get; set; }

        public ApplicationUser? RecordedByUser { get; set; }

        public ICollection<PhmMedicationReconciliationDecision> Decisions { get; set; } =
            new List<PhmMedicationReconciliationDecision>();
    }
}
