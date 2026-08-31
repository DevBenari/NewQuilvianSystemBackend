using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models
{
    /// <summary>
    /// Koreksi atau tambahan terhadap dokumen klinis yang sudah terkunci.
    ///
    /// Addendum TIDAK PERNAH menimpa isi lama; ia menempel di bawahnya. Pembaca melihat
    /// keduanya dan tahu urutan kejadiannya (RM-DEC-004).
    ///
    /// Addendum juga tidak dapat diubah maupun dihapus setelah dibuat. Koreksi atas addendum
    /// dibuat sebagai addendum berikutnya dengan <see cref="Sequence"/> lebih tinggi. Karena itu
    /// tabel ini tidak memiliki kolom status — addendum hanya punya satu keadaan, yaitu ada.
    /// </summary>
    [Table("TrxClinicalNoteAddendum", Schema = "public")]
    public class TrxClinicalNoteAddendum : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid IntegrityId { get; set; }

        /// <summary>Urutan koreksi, dimulai dari 1. Unik bersama <see cref="IntegrityId"/>.</summary>
        public int Sequence { get; set; }

        /// <summary>
        /// Pembuat addendum. Bila dibuat penulis pengganti, kolom ini berisi nama penggantinya,
        /// BUKAN penulis asli.
        /// </summary>
        [Required]
        public Guid AuthorUserId { get; set; }

        /// <summary>
        /// Bernilai benar hanya bila <see cref="DelegationId"/> terisi dan penetapannya masih
        /// berlaku.
        /// </summary>
        public bool IsSubstituteAuthor { get; set; } = false;

        /// <summary>Dasar kewenangan penulis pengganti.</summary>
        public Guid? DelegationId { get; set; }

        /// <summary>Isi koreksi. SENSITIF — data klinis.</summary>
        [Required]
        [MaxLength(4000)]
        public string AddendumText { get; set; } = string.Empty;

        /// <summary>Alasan koreksi. SENSITIF. Wajib diisi tanpa kecuali.</summary>
        [Required]
        [MaxLength(500)]
        public string CorrectionReason { get; set; } = string.Empty;

        /// <summary>Addendum selalu final saat dibuat; tidak ada tahap draf.</summary>
        public DateTime SignedAt { get; set; } = DateTime.UtcNow;

        [MaxLength(250)]
        public string? SignatureDeviceInfo { get; set; }

        [MaxLength(64)]
        public string? SignatureIpAddress { get; set; }

        public TrxClinicalDocumentIntegrity? Integrity { get; set; }

        public TrxClinicalNoteAuthorDelegation? Delegation { get; set; }
    }
}
