using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models
{
    /// <summary>
    /// Metadata satu berkas surat rujukan (<c>RJ-DOC-DEC-081</c>).
    /// </summary>
    /// <remarks>
    /// Berkasnya disimpan di folder <b>privat</b> (<c>FileStorage:PrivateRootPath</c>), di luar
    /// folder yang dilayani static files, dan hanya dapat dibaca lewat endpoint berizin. Surat
    /// rujukan memuat diagnosa, sehingga tidak boleh dapat dibuka lewat URL publik seperti foto
    /// kartu penjamin. Penghapusan bersifat lunak.
    /// </remarks>
    [Table("RegEncounterReferralDocument", Schema = "public")]
    public class RegEncounterReferralDocument : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid EncounterReferralId { get; set; }

        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        /// <summary>Path relatif terhadap <c>FileStorage:PrivateRootPath</c>.</summary>
        [Required]
        [MaxLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        /// <summary>Urutan halaman atau berkas, dimulai dari 1.</summary>
        public int PageOrder { get; set; }

        public RegEncounterReferral? EncounterReferral { get; set; }
    }
}
