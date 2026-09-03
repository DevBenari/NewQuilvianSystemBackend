using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Jawaban satu butir gerbang keselamatan untuk satu study.
    ///
    /// Baris ini membekukan kode dan nama butir pada saat penilaian dilakukan. Master data boleh
    /// berubah — butir boleh diganti namanya, dinonaktifkan, atau diperinci — tetapi apa yang
    /// ditanyakan kepada petugas hari itu tidak boleh ikut berubah di belakang layar. Tanpa
    /// pembekuan ini, jejak audit keselamatan akan menceritakan pertanyaan yang tidak pernah
    /// benar-benar diajukan.
    /// </summary>
    public class RadStudySafetyCheck : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RadStudyId { get; set; }

        [Required]
        public Guid SafetyRequirementId { get; set; }

        /// <summary>Kode butir, dibekukan saat baris ini dibuat.</summary>
        [Required]
        public string RequirementCodeSnapshot { get; set; } = string.Empty;

        /// <summary>Nama butir, dibekukan saat baris ini dibuat.</summary>
        [Required]
        public string RequirementNameSnapshot { get; set; } = string.Empty;

        /// <summary>
        /// Apakah butir ini wajib pada saat study dibuat. Dibekukan bersama nama dan kodenya,
        /// karena kewajiban adalah bagian dari pertanyaan yang diajukan hari itu.
        /// </summary>
        public bool IsMandatorySnapshot { get; set; }

        public int RuleVersionSnapshot { get; set; }

        public RadSafetyCheckState CheckState { get; set; } = RadSafetyCheckState.Pending;

        public DateTime? DecidedAt { get; set; }

        public Guid? DecidedByUserId { get; set; }

        public string? Note { get; set; }

        public int Version { get; set; }

        public RadStudy? RadStudy { get; set; }

        public MstRadSafetyRequirement? SafetyRequirement { get; set; }
    }
}
