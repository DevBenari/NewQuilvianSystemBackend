using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Resume MCU satu order, berversi. Koreksi sesudah verifikasi membuat versi baru; versi lama
    /// tetap tersimpan dan menandai alasannya pada <see cref="ReopenReason"/>.
    /// </summary>
    /// <remarks>
    /// Satu order paling banyak satu versi yang berlaku (<see cref="IsCurrent"/>) — dijaga unique index.
    /// Pemverifikasi tidak boleh akun yang sama dengan dokter pemberi approve (dijaga service).
    /// Seluruh teks klinis SENSITIF; hanya <see cref="WorkRecommendationText"/> yang boleh sampai ke
    /// perusahaan tanpa persetujuan pelepasan.
    /// </remarks>
    [Table("McuResume", Schema = "public")]
    public class McuResume : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid McuOrderId { get; set; }

        public McuOrder? McuOrder { get; set; }

        public int VersionNumber { get; set; } = 1;

        public McuResumeStatus ResumeStatus { get; set; } = McuResumeStatus.Draft;

        public bool IsCurrent { get; set; } = true;

        /// <summary>SENSITIF. Kondisi kesehatan keseluruhan; wajib saat approve.</summary>
        [MaxLength(4000)]
        public string? GeneralConditionText { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(4000)]
        public string? ImportantFindingsText { get; set; }

        /// <summary>SENSITIF. Rekomendasi medis berikutnya; wajib saat approve.</summary>
        [MaxLength(4000)]
        public string? RecommendationText { get; set; }

        /// <summary>Wajib saat approve.</summary>
        public Guid? FitnessCategoryId { get; set; }

        public McuFitnessCategory? FitnessCategory { get; set; }

        /// <summary>SENSITIF. Satu-satunya teks yang boleh ke perusahaan tanpa persetujuan.</summary>
        [MaxLength(1000)]
        public string? WorkRecommendationText { get; set; }

        public Guid? AuthorDoctorId { get; set; }

        public MstDoctor? AuthorDoctor { get; set; }

        public Guid? ApprovedByDoctorId { get; set; }

        public MstDoctor? ApprovedByDoctor { get; set; }

        public DateTime? ApprovedAt { get; set; }

        [MaxLength(500)]
        public string? ReturnReason { get; set; }

        public Guid? VerifiedByUserId { get; set; }

        public DateTime? VerifiedAt { get; set; }

        [MaxLength(500)]
        public string? ReopenReason { get; set; }

        public Guid? ReopenedByDoctorId { get; set; }

        public MstDoctor? ReopenedByDoctor { get; set; }

        public DateTime? ReopenedAt { get; set; }
    }
}
