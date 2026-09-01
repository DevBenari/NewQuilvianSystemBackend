using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Daftar keperluan akses yang dapat dipilih pengguna saat membuka rekam medis pasien di
    /// luar rawatannya.
    ///
    /// Menyediakan pilihan jauh lebih baik daripada kotak teks bebas saja: alasan menjadi dapat
    /// dihitung dan dibandingkan saat ditinjau. Kotak teks bebas menghasilkan jawaban yang
    /// tidak dapat dikelompokkan, sehingga tinjauan berubah menjadi membaca satu per satu.
    ///
    /// Letak berkas mengikuti aturan struktur: master tinggal di MasterData/Models, bukan di
    /// folder submodulnya.
    /// </summary>
    [Table("MstMedicalRecordAccessPurpose", Schema = "public")]
    public class MstMedicalRecordAccessPurpose : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(50)]
        public string PurposeCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PurposeName { get; set; } = string.Empty;

        /// <summary>
        /// Bila benar, pengguna wajib menuliskan alasannya sendiri. Dipakai pilihan "Lainnya",
        /// yang tanpa penjelasan tambahan tidak berguna saat ditinjau.
        /// </summary>
        public bool IsFreeTextRequired { get; set; } = false;

        /// <summary>
        /// Bila benar, akses dengan keperluan ini masuk antrean tinjauan unit rekam medis.
        /// </summary>
        public bool RequiresReview { get; set; } = true;

        public int SortOrder { get; set; } = 0;

        [MaxLength(250)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
