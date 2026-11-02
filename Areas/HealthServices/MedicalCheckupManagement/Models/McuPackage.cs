using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Models
{
    /// <summary>
    /// Paket MCU yang dijual — <c>data-dictionary.md</c> bagian 1. Contoh <i>Paket Basic</i>
    /// Rp 350.000: Darah Lengkap, Gula Darah Puasa, Thorax, Pemeriksaan Fisik.
    /// </summary>
    /// <remarks>
    /// Harga tidak disimpan di sini: harga umum adalah tarif paket <see cref="GeneralTariffId"/>,
    /// harga perusahaan ada di <see cref="McuPackageCompanyPrice"/>.
    /// </remarks>
    [Table("McuPackage", Schema = "public")]
    public class McuPackage : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Huruf besar tanpa spasi, contoh <c>MCU-BASIC</c>.</summary>
        [Required]
        [MaxLength(30)]
        public string PackageCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string PackageName { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>Harga umum; tarifnya wajib ber-<c>IsPackageTariff</c>.</summary>
        [Required]
        public Guid GeneralTariffId { get; set; }

        public MstTariff? GeneralTariff { get; set; }

        /// <summary>Nonaktif = tidak dapat dipilih untuk order baru.</summary>
        public bool IsActive { get; set; } = true;

        public ICollection<McuPackageComponent> Components { get; set; } = new List<McuPackageComponent>();

        public ICollection<McuPackageCompanyPrice> CompanyPrices { get; set; } = new List<McuPackageCompanyPrice>();
    }
}
