using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.DTOs
{
    /// <summary>
    /// Permintaan menetapkan seorang penulis catatan sebagai berhalangan.
    ///
    /// Hanya untuk penetapan manual oleh kepala unit. Penetapan karena akun nonaktif tidak
    /// dibuat lewat jalur ini — sistem menyimpulkannya sendiri, karena keadaan yang dapat
    /// disimpulkan otomatis tidak boleh bergantung pada seseorang mengingat untuk mencatatnya.
    /// </summary>
    public class CreateAuthorDelegationRequest
    {
        [Required(ErrorMessage = "Penulis yang berhalangan wajib dipilih.")]
        public Guid OriginalAuthorUserId { get; set; }

        [Required(ErrorMessage = "Alasan penetapan wajib diisi.")]
        [MaxLength(500, ErrorMessage = "Alasan penetapan terlalu panjang. Batasnya 500 huruf.")]
        public string GrantReason { get; set; } = string.Empty;

        /// <summary>
        /// Wajib diisi. Penetapan tanpa batas waktu adalah pintu belakang permanen.
        /// </summary>
        [Required(ErrorMessage = "Batas waktu penetapan wajib diisi.")]
        public DateTime ValidUntil { get; set; }
    }

    public class RevokeAuthorDelegationRequest
    {
        [MaxLength(500)]
        public string? RevokeReason { get; set; }
    }

    public class AuthorDelegationResponse
    {
        public Guid Id { get; set; }
        public Guid OriginalAuthorUserId { get; set; }
        public string? OriginalAuthorName { get; set; }
        public AuthorDelegationTrigger Trigger { get; set; }
        public string TriggerName { get; set; } = string.Empty;
        public Guid? GrantedByUserId { get; set; }
        public string? GrantedByName { get; set; }
        public string? GrantReason { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime? ValidUntil { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool IsActive { get; set; }

        /// <summary>
        /// Apakah penetapan ini masih berlaku pada saat data diambil. Dihitung server supaya
        /// layar tidak perlu membandingkan tanggal sendiri.
        /// </summary>
        public bool IsCurrentlyValid { get; set; }
    }

    /// <summary>
    /// Jawaban atas pertanyaan "apakah saya boleh membuat addendum pada dokumen ini, dan atas
    /// dasar apa".
    ///
    /// Disediakan supaya layar dapat menampilkan tombol addendum hanya kepada yang berhak, dan
    /// menjelaskan alasannya bila tidak berhak. Menampilkan tombol yang selalu gagal saat
    /// ditekan adalah pengalaman yang buruk dan mendorong pengguna mencari jalan lain.
    /// </summary>
    public class AddendumAuthorityResponse
    {
        public bool IsAllowed { get; set; }

        /// <summary>Benar bila pengguna adalah penulis asli dokumen.</summary>
        public bool IsOriginalAuthor { get; set; }

        /// <summary>Benar bila kewenangan berasal dari penetapan berhalangan.</summary>
        public bool IsSubstituteAuthor { get; set; }

        public Guid? DelegationId { get; set; }

        public AuthorDelegationTrigger? DelegationTrigger { get; set; }

        /// <summary>Penjelasan yang dapat dibaca pengguna, baik saat berhak maupun tidak.</summary>
        public string Explanation { get; set; } = string.Empty;
    }
}
