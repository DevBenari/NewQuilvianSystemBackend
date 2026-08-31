using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Models
{
    /// <summary>
    /// Mencatat bahwa seorang penulis catatan dinyatakan berhalangan, sehingga kepala unit atau
    /// DPJP boleh membuat addendum menggantikannya. Menjawab RM-DEC-020.
    ///
    /// Untuk <see cref="AuthorDelegationTrigger.InactiveAccount"/>, baris ini tidak perlu dibuat
    /// manusia — sistem menyimpulkannya dari keadaan akun. Baris manual hanya dibuat untuk
    /// <see cref="AuthorDelegationTrigger.UnitHeadGrant"/>.
    /// </summary>
    [Table("MrcClinicalNoteAuthorDelegation", Schema = "public")]
    public class MrcClinicalNoteAuthorDelegation : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid OriginalAuthorUserId { get; set; }

        public AuthorDelegationTrigger Trigger { get; set; }

        /// <summary>Pemberi penetapan. Kosong bila sebabnya akun nonaktif.</summary>
        public Guid? GrantedByUserId { get; set; }

        /// <summary>
        /// Wajib diisi bila <see cref="Trigger"/> bernilai
        /// <see cref="AuthorDelegationTrigger.UnitHeadGrant"/>.
        /// </summary>
        [MaxLength(500)]
        public string? GrantReason { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Wajib diisi untuk penetapan manual.
        ///
        /// Penetapan tanpa batas waktu adalah pintu belakang permanen: bila kepala unit membuka
        /// jalur pengganti sekali lalu lupa menutupnya, catatan penulis itu selamanya dapat
        /// dikoreksi orang lain — dan itu menghapus makna RM-DEC-004.
        /// </summary>
        public DateTime? ValidUntil { get; set; }

        public DateTime? RevokedAt { get; set; }

        public Guid? RevokedByUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
