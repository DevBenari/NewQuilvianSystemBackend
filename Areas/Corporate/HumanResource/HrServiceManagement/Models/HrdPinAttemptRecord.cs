using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.HrServiceManagement.Models
{
    /// <summary>
    /// Penghitung salah-PIN pembukaan nominal surat finansial di portal verifikasi publik, per dokumen dan
    /// alamat IP (VR-HRS-12, BE-HRD-204): maksimal 3 kali, lalu terkunci 30 menit.
    /// </summary>
    /// <remarks>
    /// Sebelum 10 November 2026 penghitung ini hanya ada di memori proses sehingga penguncian anti brute-force
    /// hilang saat aplikasi di-restart. Satu baris per <see cref="AttemptKey"/>.
    /// </remarks>
    [Table("HrdPinAttemptRecord", Schema = "public")]
    public class HrdPinAttemptRecord : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Gabungan hash verifikasi dokumen (huruf kecil) dan alamat IP klien.</summary>
        [Required]
        [MaxLength(200)]
        public string AttemptKey { get; set; } = string.Empty;

        [Required]
        [MaxLength(128)]
        public string VerificationHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(64)]
        public string ClientIp { get; set; } = string.Empty;

        public int FailedCount { get; set; }

        public DateTime? LockedUntilUtc { get; set; }

        public DateTime LastAttemptUtc { get; set; } = DateTime.UtcNow;
    }
}
