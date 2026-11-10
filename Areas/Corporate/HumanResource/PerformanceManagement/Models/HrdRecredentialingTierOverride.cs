using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.PerformanceManagement.Models
{
    /// <summary>
    /// Jejak audit diskresi penyesuaian tier rekredensialing oleh Ketua Komite Medik (VR-OPP-19, BE-HRD-234).
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 catatan ini hanya hidup di memori proses (<c>ConcurrentDictionary</c> statis)
    /// dan hilang setiap kali aplikasi di-restart, padahal ini bukti akreditasi. Satu baris aktif per dokter;
    /// penyesuaian berikutnya menonaktifkan baris sebelumnya tanpa menghapusnya.
    /// </remarks>
    [Table("HrdRecredentialingTierOverride", Schema = "public")]
    public class HrdRecredentialingTierOverride : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid WorkforceProfileId { get; set; }

        /// <summary>Tier hasil kalkulasi sistem pada saat penyesuaian dilakukan.</summary>
        [Required]
        [MaxLength(40)]
        public string CalculatedTier { get; set; } = string.Empty;

        /// <summary>GreenFastTrack, YellowClarificationReview, atau RedPlenaryInvestigation.</summary>
        [Required]
        [MaxLength(40)]
        public string NewTier { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string JustificationReason { get; set; } = string.Empty;

        [Required]
        public Guid ApprovedByCommitteeChairmanId { get; set; }

        [MaxLength(200)]
        public string? ChairmanNameSnapshot { get; set; }

        public DateTime OverriddenAt { get; set; } = DateTime.UtcNow;

        /// <summary>Hanya satu penyesuaian yang aktif per dokter; yang lama dinonaktifkan, bukan dihapus.</summary>
        public bool IsActive { get; set; } = true;
    }
}
