using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.RadiologyManagement.Models
{
    /// <summary>
    /// Gambar radiologi yang diunggah petugas — *Unggah Gambar* Form Hasil v1 (<c>BE-RAD-22</c>,
    /// <c>RAD-EVD-001</c> bagian 7.1).
    /// </summary>
    /// <remarks>
    /// Berkas fisik disimpan di penyimpanan **privat** (<c>FileStorage:PrivateRootPath</c>), hanya dapat
    /// dibaca lewat endpoint berizin — pola <c>ReferralDocumentStorageService</c>. Baris ini hanya
    /// metadata; isi gambar tidak pernah disimpan di basis data. Bukan pengganti PACS (<c>RAD-OPEN-013</c>).
    /// </remarks>
    public class RadOrderImage : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid RadOrderId { get; set; }

        /// <summary>Study yang digambarkan, bila petugas memilihnya. Kosong = gambar pesanan.</summary>
        public Guid? RadStudyId { get; set; }

        [Required, MaxLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ContentType { get; set; } = string.Empty;

        public long SizeBytes { get; set; }

        /// <summary>Jalur relatif terhadap root privat, mis. <c>radiology-images/{orderId}/{id}.jpg</c>.</summary>
        [Required, MaxLength(500)]
        public string StoragePath { get; set; } = string.Empty;

        public int SortOrder { get; set; }

        public RadOrder? RadOrder { get; set; }
    }
}
