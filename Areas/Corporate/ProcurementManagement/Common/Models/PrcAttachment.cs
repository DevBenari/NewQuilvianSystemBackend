using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.Corporate.ProcurementManagement.Common.Models
{
    /// <summary>
    /// Lampiran berkas seluruh dokumen Pengadaan (BE-PBJ-004, PBJ-DEC-023; kamus data bagian 5).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Rujukan ke dokumen polimorfik (<see cref="DocumentType"/> + <see cref="DocumentId"/>), tanpa
    /// FK. Berkas pengganti membuat <b>versi baru</b> dengan <see cref="AttachmentKey"/> yang sama;
    /// versi lama tetap tersimpan dengan <see cref="IsCurrent"/> = false. Berkas tidak pernah
    /// ditimpa.
    /// </para>
    /// <para>
    /// Contoh: foto kondisi kursi roda diunggah (versi 1), lalu diganti foto yang lebih jelas
    /// (versi 2). Keduanya tetap dapat dibuka dari riwayat lampiran.
    /// </para>
    /// <para>
    /// Berkas disimpan di luar <c>wwwroot</c>, sehingga hanya dapat diunduh lewat endpoint yang
    /// memeriksa hak atas dokumen induknya.
    /// </para>
    /// </remarks>
    [Table("PrcAttachment", Schema = "public")]
    public class PrcAttachment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(40)]
        public string DocumentType { get; set; } = string.Empty;

        [Required]
        public Guid DocumentId { get; set; }

        /// <summary>Contoh: "Foto kondisi", "KAK", "Penawaran".</summary>
        [MaxLength(50)]
        public string? AttachmentCategory { get; set; }

        /// <summary>Identitas lampiran lintas versi.</summary>
        public Guid AttachmentKey { get; set; } = Guid.NewGuid();

        public int VersionNo { get; set; } = 1;

        public bool IsCurrent { get; set; } = true;

        /// <summary>SENSITIF. Nama berkas asli; tidak boleh masuk logger.</summary>
        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        /// <summary>Nama acak di penyimpanan (Guid + ekstensi).</summary>
        [Required]
        [MaxLength(100)]
        public string StoredFileName { get; set; } = string.Empty;

        /// <summary>Jalur relatif terhadap akar penyimpanan lampiran Pengadaan.</summary>
        [Required]
        [MaxLength(500)]
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>Ditentukan server dari tanda tangan berkas, bukan dari klien.</summary>
        [Required]
        [MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        [Required]
        [MaxLength(64)]
        public string Sha256 { get; set; } = string.Empty;

        public Guid UploadedByUserId { get; set; }
    }
}
