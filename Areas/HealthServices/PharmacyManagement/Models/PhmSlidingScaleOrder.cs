using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Protokol sliding scale per pasien, menempel pada satu butir resep insulin berdosis skala —
    /// <c>BE-RWI-103</c>, migration <c>R6</c>, kamus data 0.5 bagian 13.7.
    /// </summary>
    /// <remarks>
    /// Rentangnya <b>tersalin</b> ke <see cref="PhmSlidingScaleOrderVersion"/>, bukan dirujuk ke versi
    /// template, sehingga template yang berganti versi tidak pernah menggeser dosis pasien yang
    /// sedang berjalan (<c>FR-DOK-097</c>). <see cref="CurrentVersionNumber"/> sekaligus penjaga
    /// benturan: permintaan penyesuaian membawa nomor versi yang dibacanya.
    /// </remarks>
    [Table("PhmSlidingScaleOrder", Schema = "public")]
    public class PhmSlidingScaleOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor bisnis dari provider number-series, bukan <c>Count+1</c> (<c>QBE-CODE-002</c>).</summary>
        [Required]
        [MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid PrescriptionId { get; set; }

        [Required]
        public Guid PrescriptionItemId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        public Guid InpEpisodeId { get; set; }

        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid TemplateId { get; set; }

        public SlidingScaleOrderStatus OrderStatus { get; set; } = SlidingScaleOrderStatus.Active;

        public int CurrentVersionNumber { get; set; } = 1;

        /// <summary>Usulan gate <c>G-22</c>: jadwal pemeriksaan GDS.</summary>
        [MaxLength(30)]
        public string? CheckFrequencyCode { get; set; }

        public DateTime? StoppedAt { get; set; }

        public Guid? StoppedByUserId { get; set; }

        /// <summary>Alasan penghentian. SENSITIF.</summary>
        [MaxLength(500)]
        public string? StopReason { get; set; }

        public bool IsActive { get; set; } = true;

        public PhmPrescription? Prescription { get; set; }

        public PhmPrescriptionItem? PrescriptionItem { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        public MstPatient? Patient { get; set; }

        public PhmSlidingScaleTemplate? Template { get; set; }

        public ApplicationUser? StoppedByUser { get; set; }

        public ICollection<PhmSlidingScaleOrderVersion> Versions { get; set; } =
            new List<PhmSlidingScaleOrderVersion>();
    }
}
