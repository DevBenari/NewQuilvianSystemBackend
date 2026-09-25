using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;
using QuilvianSystemBackend.Areas.HealthServices.PatientManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Models
{
    /// <summary>
    /// Permintaan cuci darah yang datang dari luar unit HD — bangsal, poliklinik, atau IGD
    /// (<c>HMD-DEC-008</c>).
    /// </summary>
    /// <remarks>
    /// Permintaan <b>tidak pernah</b> berubah sendiri menjadi sesi. Bentuknya menyalin
    /// <c>LabOrder</c>/<c>RadOrder</c>: <c>EncounterId</c> wajib, <c>InpEpisodeId</c> boleh kosong,
    /// penahanan menyimpan status sebelumnya pada <see cref="StatusBeforeHold"/>.
    /// </remarks>
    [Table("HmdOrder", Schema = "public")]
    public class HmdOrder : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Nomor permintaan dari <c>NumberSeriesAllocator</c>, deret <c>HMD_ORDER</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string OrderNumber { get; set; } = string.Empty;

        [Required]
        public Guid PatientId { get; set; }

        public MstPatient? Patient { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public RegPatientEncounter? Encounter { get; set; }

        public Guid? InpEpisodeId { get; set; }

        public InpEpisode? InpEpisode { get; set; }

        /// <summary>Program HD pasien, bila pasien sudah punya.</summary>
        public Guid? EpisodeId { get; set; }

        public HmdEpisode? Episode { get; set; }

        [Required]
        public Guid RequestedByUserId { get; set; }

        /// <summary>Dokter yang meminta, bila pembuat permintaannya perawat.</summary>
        public Guid? RequestingDoctorId { get; set; }

        public MstDoctor? RequestingDoctor { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public HmdOrderPriority Priority { get; set; } = HmdOrderPriority.Routine;

        /// <summary>Indikasi klinis permintaan. SENSITIF.</summary>
        [Required]
        [MaxLength(1000)]
        public string ClinicalReason { get; set; } = string.Empty;

        public DateOnly? RequestedDate { get; set; }

        public HmdOrderStatus OrderStatus { get; set; } = HmdOrderStatus.Requested;

        public HmdOrderStatus? StatusBeforeHold { get; set; }

        public Guid? DecisionByUserId { get; set; }

        public DateTime? DecisionAt { get; set; }

        /// <summary>Alasan penahanan, penolakan, atau pembatalan. SENSITIF.</summary>
        [MaxLength(1000)]
        public string? DecisionReason { get; set; }

        /// <summary>Concurrency token; dua keputusan serentak atas permintaan yang sama tidak sama-sama tersimpan.</summary>
        public int Version { get; set; }
    }
}
