using QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.OpticalManagement.Models
{
    /// <summary>Atribut Optik untuk satu barang <c>MstDrug</c> (<c>OPT-DEC-031</c>). Merk memakai <c>MstDrug.BrandName</c>; nama, harga, dan stok tidak digandakan.</summary>
    /// <remarks>OPT-BP-001, BE-OPT-002 — kolom mengikuti <c>erd/data-dictionary.md</c> r1.</remarks>
    [Table("OptItemProfile", Schema = "public")]
    public class OptItemProfile : IdentityModel
    {
        /// <summary>Kunci utama</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Barang; satu profil per barang</summary>
        [Required]
        public Guid DrugId { get; set; }

        /// <summary>Frame/SpectacleLens/ContactLens/Accessory/ReadyMadeGlasses</summary>
        public OptItemCategory ItemCategory { get; set; }

        /// <summary>Frame</summary>
        [MaxLength(50)]
        public string? Color { get; set; }

        /// <summary>Frame, contoh 52-18-140</summary>
        [MaxLength(20)]
        public string? FrameSize { get; set; }

        /// <summary>Frame</summary>
        [MaxLength(50)]
        public string? FrameMaterial { get; set; }

        /// <summary>Lensa, contoh 1,56</summary>
        public decimal? LensIndex { get; set; }

        /// <summary>Lensa</summary>
        public OptLensDesign? LensDesign { get; set; }

        /// <summary>Lensa, contoh antiradiasi</summary>
        [MaxLength(100)]
        public string? LensCoating { get; set; }

        /// <summary>Lensa kontak</summary>
        public decimal? ContactLensBaseCurve { get; set; }

        /// <summary>Lensa kontak</summary>
        public decimal? ContactLensDiameter { get; set; }

        /// <summary>Lensa kontak, contoh bulanan</summary>
        [MaxLength(50)]
        public string? ContactLensReplacement { get; set; }

        /// <summary>Profil nonaktif tidak bisa dipilih di pesanan baru</summary>
        public bool IsActive { get; set; } = true;
    }
}
