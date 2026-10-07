using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Models
{
    /// <summary>
    /// Butir checklist persiapan bedah untuk Catatan Pra-Operasi bangsal —
    /// <c>episode-rawat-inap</c> kontrak <c>0.10.0</c> kamus data 19.7 (<c>BE-RWI-172</c>, migration <c>E4</c>).
    /// </summary>
    /// <remarks>
    /// Empat kelompok <c>RWI-DEC-173</c> butir 3: Verifikasi pasien, Persiapan fisik, Hasil
    /// pemeriksaan, Persiapan lain. Contoh butir: "Gelang identitas terpasang" (Verifikasi pasien,
    /// wajib). Butir nonaktif tidak muncul di versi pra-operasi baru; versi lama tetap utuh karena
    /// nama dan sifat wajibnya disalin ke <c>OprWardPreOpItem</c> saat versi dibuat.
    /// </remarks>
    [Table("MstSurgicalPreparationItem", Schema = "public")]
    public class MstSurgicalPreparationItem : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Kode unik butir, misalnya <c>SPI-ID-01</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string Code { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string GroupName { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string ItemName { get; set; } = string.Empty;

        public bool IsMandatory { get; set; } = true;

        /// <summary>Urutan tampil butir di dalam kelompoknya pada form pra-operasi.</summary>
        public int SortOrder { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Konkurensi optimistis; berganti setiap kali baris diubah.</summary>
        public Guid RowVersion { get; set; } = Guid.NewGuid();
    }
}
