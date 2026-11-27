using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Models
{
    /// <summary>
    /// Satu terbitan beku dokumen hasil untuk pasien (<c>LAB-DC-072</c>): nomor dokumen, status,
    /// token verifikasi, dan berkas arsip (<c>LAB-DEC-140</c>, <c>LAB-DEC-284</c>).
    /// </summary>
    /// <remarks>
    /// Tidak pernah dihapus. Hanya <see cref="Status"/>, <see cref="SupersededAt"/>, dan
    /// <see cref="TokenRevokedAt"/> yang berubah, dan hanya ketika versi berikutnya terbit
    /// (<c>INV-72</c>). Hari ini tidak ada pemicu versi kedua (<c>LAB-DEC-289</c>).
    /// </remarks>
    [Table("LabPatientResultVersion", Schema = "public")]
    public class LabPatientResultVersion : IdentityModel
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid DocumentId { get; set; }

        /// <summary>Nomor versi, berurutan mulai 1 per dokumen.</summary>
        public int VersionNumber { get; set; }

        /// <summary>Nomor tercetak, mis. <c>PK-26-000123-V1</c> (34.10 no. 7).</summary>
        [MaxLength(60)]
        public string DocumentNumber { get; set; } = string.Empty;

        public LabPatientResultVersionStatus Status { get; set; } = LabPatientResultVersionStatus.Current;

        /// <summary>Waktu terbit sebenarnya, UTC.</summary>
        public DateTime IssuedAt { get; set; }

        public DateTime? SupersededAt { get; set; }

        /// <summary>
        /// Isi kode QR: 32 byte acak, base64url (<c>INV-79</c>). <b>Rahasia akses</b> — tidak pernah
        /// dikirim ke layar dan tidak pernah dicatat ke log.
        /// </summary>
        [MaxLength(64)]
        public string VerificationToken { get; set; } = string.Empty;

        public DateTime? TokenRevokedAt { get; set; }

        /// <summary>Jalur relatif terhadap <c>FileStorage:PrivateRootPath</c>; tanpa nama pasien.</summary>
        [MaxLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        /// <summary>SHA-256 berkas, heksadesimal huruf kecil.</summary>
        [MaxLength(64)]
        public string FileSha256 { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public LabPatientResultDocument? Document { get; set; }

        public ICollection<LabPatientResultApproval> Approvals { get; set; } = new List<LabPatientResultApproval>();
    }
}
