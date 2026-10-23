using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Catatan setiap impor susunan denah yang benar-benar dijalankan (BE-RWI-237, kamus data 22.2.3):
    /// siapa, kapan, manifest mana, lingkungan, apakah bed ditetapkan siap, dan ringkasan hasilnya.
    /// Dry run tidak dicatat. Tidak berisi data pasien dan tidak diubah setelah dibuat.
    /// </summary>
    [Table("MstFloorPlanImportRun", Schema = "public")]
    public class MstFloorPlanImportRun : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Versi data denah frontend yang diimpor, mis. <c>denah-rsmmc-2026-10-15</c>.</summary>
        [Required]
        [MaxLength(50)]
        public string ManifestVersion { get; set; } = string.Empty;

        /// <summary>SHA-256 manifest kanonis; run kedua dengan hash sama mudah dikenali.</summary>
        [Required]
        [MaxLength(64)]
        public string ManifestHash { get; set; } = string.Empty;

        /// <summary><c>Development</c>, <c>Staging</c>, atau <c>Production</c>.</summary>
        [Required]
        [MaxLength(32)]
        public string EnvironmentName { get; set; } = string.Empty;

        /// <summary>Benar hanya bila opsi dev/UAT penetapan siap aktif (RWI-DEC-321).</summary>
        public bool MarkedBedsReady { get; set; }

        [MaxLength(200)]
        public string? ReadyEvidenceReference { get; set; }

        /// <summary>Ringkasan hasil per kunci denah (skema kamus data 22.3), tanpa data pasien.</summary>
        [Required]
        public string SummaryJson { get; set; } = "{}";

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime StartedAtUtc { get; set; }

        public DateTime CompletedAtUtc { get; set; }

        public ApplicationUser? ActorUser { get; set; }
    }
}
