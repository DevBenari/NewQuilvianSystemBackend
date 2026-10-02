using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Instruksi dokter tentang bagaimana cuci darah dijalankan (<c>HMD-DEC-009</c>).
    /// </summary>
    /// <remarks>
    /// Resep berstatus <c>Active</c> tidak boleh disunting. Perubahan instruksi membuat resep
    /// baru yang menggantikan resep lama pada satu transaksi. Satu episode hanya punya satu
    /// resep <c>Active</c>, dijaga unique index bersyarat <c>IX_HmdPrescription_EpisodeId_Active</c>.
    /// </remarks>
    [Table("HmdPrescription", Schema = "public")]
    public class HmdPrescription : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        /// <summary>Dokter pemberi instruksi; menjadi <c>InstructingDoctorId</c> pada tindakan pasien.</summary>
        [Required]
        public Guid PrescribingDoctorId { get; set; }

        public MstDoctor? PrescribingDoctor { get; set; }

        public DateOnly EffectiveDate { get; set; }

        public int FrequencyPerWeek { get; set; }

        public int TargetDurationMinutes { get; set; }

        public int? TargetUltrafiltrationMl { get; set; }

        /// <summary>Target kecepatan aliran darah (QB), ml/menit.</summary>
        public int? BloodFlowRate { get; set; }

        /// <summary>Target kecepatan aliran dialisat (QD), ml/menit.</summary>
        public int? DialysateFlowRate { get; set; }

        /// <summary>Jenis dializer, misalnya low-flux atau high-flux.</summary>
        [MaxLength(100)]
        public string? DialyzerType { get; set; }

        /// <summary>Komposisi dialisat yang diinstruksikan.</summary>
        [MaxLength(200)]
        public string? DialysateComposition { get; set; }

        /// <summary>Profil natrium dan bikarbonat.</summary>
        [MaxLength(200)]
        public string? SodiumBicarbonateProfile { get; set; }

        public decimal? DialysateTemperatureC { get; set; }

        /// <summary>Rencana antikoagulasi. SENSITIF.</summary>
        [MaxLength(500)]
        public string? AnticoagulantPlan { get; set; }

        public Guid? VascularAccessId { get; set; }

        public HmdVascularAccess? VascularAccess { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? ClinicalNote { get; set; }

        public HmdPrescriptionStatus PrescriptionStatus { get; set; } = HmdPrescriptionStatus.Draft;

        public Guid? SupersededByPrescriptionId { get; set; }

        public HmdPrescription? SupersededByPrescription { get; set; }

        public DateTime? ActivatedAt { get; set; }

        public Guid? ActivatedByUserId { get; set; }

        [MaxLength(500)]
        public string? CancelReason { get; set; }

        public int Version { get; set; }
    }
}
