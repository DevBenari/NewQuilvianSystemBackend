using QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Riwayat perpindahan status pesanan dan study radiologi.
    ///
    /// Baris pada tabel ini hanya ditambahkan, tidak pernah diubah dan tidak pernah dihapus oleh
    /// alur operasional. Service tidak menyediakan satu pun jalur update terhadapnya.
    ///
    /// Untuk radiologi, sifat append-only itu punya arti tambahan: ia adalah satu-satunya tempat
    /// yang dapat menjawab berapa kali seorang pasien benar-benar disinari, termasuk acquisition
    /// yang gagal dan diulang. Menimpa baris lama akan menghapus paparan yang sudah terjadi dari
    /// catatan, sedangkan paparannya sendiri tidak ikut terhapus dari tubuh pasien.
    /// </summary>
    public class RadTransitionHistory : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RadOrderId { get; set; }

        public Guid? RadStudyId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        public RadTransitionScope Scope { get; set; }

        /// <summary>Nama tindakan operasional, misalnya <c>Study.StartAcquisition</c>.</summary>
        [Required]
        public string Action { get; set; } = string.Empty;

        /// <summary>Status sebelum perpindahan. Kosong pada pembuatan baris pertama.</summary>
        public string? FromStatus { get; set; }

        [Required]
        public string ToStatus { get; set; } = string.Empty;

        /// <summary>
        /// Salinan kode alasan pada saat kejadian, bukan hanya foreign key. Disimpan sebagai
        /// teks agar penonaktifan alasan di kemudian hari tidak mengubah makna riwayat lama.
        /// </summary>
        public string? ReasonCode { get; set; }

        public string? ReasonNote { get; set; }

        [Required]
        public Guid ActorUserId { get; set; }

        public DateTime OccurredAt { get; set; }

        public Guid? CorrelationId { get; set; }

        public RadOrder? RadOrder { get; set; }

        public RadStudy? RadStudy { get; set; }
    }
}
